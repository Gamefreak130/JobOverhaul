using Gamefreak130.JobOverhaulSpace.Helpers.OccupationStates;
using Gamefreak130.JobOverhaulSpace.Interactions;
using Sims3.Gameplay;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Academics;
using Sims3.Gameplay.ActiveCareer;
using Sims3.Gameplay.ActiveCareer.ActiveCareers;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Careers;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.MapTags;
using Sims3.Gameplay.Objects.Electronics;
using Sims3.Gameplay.Objects.Miscellaneous;
using Sims3.Gameplay.Objects.RabbitHoles;
using Sims3.Gameplay.Objects.Seating;
using Sims3.Gameplay.Skills;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.UI;
using System;
using System.Collections.Generic;
using static Gamefreak130.JobOverhaul;
using static Gamefreak130.JobOverhaulSpace.Helpers.Methods;
using static Gamefreak130.JobOverhaulSpace.Interactions.Interviews;
using static Sims3.Gameplay.Queries;

namespace Gamefreak130.JobOverhaulSpace.Helpers
{
    [Persistable]
    public class OccupationEntryTuple
    {
        public Occupation OccupationEntry { get; set; }

        public CareerLocation CareerLocation { get; }

        private OccupationEntryTuple()
        {
        }

        public OccupationEntryTuple(Occupation entry, CareerLocation careerLocation)
        {
            OccupationEntry = entry;
            CareerLocation = careerLocation;
        }
    }

    internal static class Listeners
    {
        internal static void OnMapTagsUpdated(object sender, EventArgs e)
        {
            if (MapTagManager.ActiveMapTagManager is not null && Sim.ActiveActor.OccupationAsActiveCareer?.Jobs is IEnumerable<Job> jobs)
            {
                foreach (Job job in jobs)
                {
                    if (job.MapTagEnabled && job.RabbitHole is not null)
                    {
                        job.Specification.mRabbitHole = job.RabbitHole.RabbitHoleProxy;
                    }
                }
                MapTagManager.ActiveMapTagManager.RefreshActiveCareerMapTags();
            }
        }

        internal static void OnObjectPlacedInLot(object sender, EventArgs e)
        {
            if (e is World.OnObjectPlacedInLotEventArgs onObjectPlacedInLotEventArgs)
            {
                AddObjectInteractions(GameObject.GetObject(onObjectPlacedInLotEventArgs.ObjectId));
            }
        }

        internal static ListenerAction OnObjectChanged(Event e)
        {
            AddObjectInteractions(e.TargetObject as GameObject);
            return ListenerAction.Keep;
        }

        private static void AddObjectInteractions(GameObject @object)
        {
            if (@object is Computer or Phone or Newspaper)
            {
                Common.Helpers.AddInteraction(@object, ChangeSettings.Singleton);
            }
            if (@object is Computer computer)
            {
                Type definitionType = Computer.FindActiveCareer.Singleton.GetType();
                computer.RemoveInteractionByType(definitionType);
                InteractionObjectPair iop = null;
                if (computer.ItemComp?.InteractionsInventory is not null)
                {
                    foreach (InteractionObjectPair current in computer.ItemComp.InteractionsInventory)
                    {
                        if (current.InteractionDefinition.GetType() == definitionType)
                        {
                            iop = current;
                            break;
                        }
                    }
                }
                if (iop is not null)
                {
                    computer.ItemComp.InteractionsInventory.Remove(iop);
                }
            }
            if (@object is Newspaper newspaper)
            {
                newspaper.RemoveInteractionByType(FindActiveCareerNewspaper.Singleton);
                if (!RandomNewspaperSeeds.ContainsKey(newspaper.ObjectId))
                {
                    RandomNewspaperSeeds.Add(newspaper.ObjectId, RandomNewspaperSeed + SimClock.ElapsedCalendarDays());
                }
            }
            if (@object is Phone phone)
            {
                Common.Helpers.AddInteraction(phone, PostponeInterview.Singleton);
                Common.Helpers.AddInteraction(phone, CancelInterview.Singleton);
            }
            if (@object is SchoolRabbitHole school)
            {
                Common.Helpers.AddInteraction(school, CollegeOfBusiness.AttendResumeWritingAndInterviewTechniquesClass.Singleton);
            }
            if (@object is CityHall cityHall)
            {
                Common.Helpers.AddInteraction(cityHall, JoinActiveCareerDaycare.Singleton);
            }
            if (@object is LifeguardChair chair)
            {
                chair.RemoveInteractionByType(LifeguardChair.JoinLifeGuardCareer.Singleton);
            }
        }

        internal static ListenerAction OnSimInBoardingSchool(Event e)
        {
            if (e.TargetObject is Sim targetSim)
            {
                InterviewData.DisposeActorData(targetSim.SimDescription.SimDescriptionId);
            }
            return ListenerAction.Keep;
        }

        internal static ListenerAction OnSimDestroyed(Event e)
        {
            if (e.Actor is Sim sim)
            {
                InterviewData.DisposeActorData(sim.SimDescription.SimDescriptionId);
            }
            return ListenerAction.Keep;
        }

        internal static ListenerAction OnTravelComplete(Event e)
        {
            if (e.Actor is Sim sim)
            {
                foreach (InventoryStack stack in sim.Inventory.InventoryItems.Values)
                {
                    foreach (InventoryItem item in stack.List)
                    {
                        if (item.Object is Newspaper newspaper)
                        {
                            newspaper.MakeOld();
                            RandomNewspaperSeeds.Remove(newspaper.ObjectId);
                        }
                    }
                }

                if (sim.CareerManager is CareerManager manager && (GameUtils.IsFutureWorld() || !GameUtils.IsAnyTravelBasedWorld()))
                {
                    ulong id = sim.SimDescription.SimDescriptionId;
                    if (SavedOccupationsForTravel.ContainsKey(id))
                    {
                        OccupationState state = SavedOccupationsForTravel[id];
                        state.AcquireOccupation(manager);
                        SavedOccupationsForTravel.Remove(id);
                    }
                    else if (GameUtils.IsFutureWorld())
                    {
                        DropOccupation(manager);
                    }
                }
            }
            return ListenerAction.Keep;
        }

        internal static ListenerAction SaveOccupationForTravel(Event e)
        {
            if (e.Actor?.CareerManager is not null)
            {
                ulong id = e.Actor.SimDescription.SimDescriptionId;
                OccupationState state = e.Actor.CareerManager.Occupation switch
                {
                    ArtAppraiserCareer c   => new ArtAppraiserState(c),
                    Business c             => new BusinessState(c),
                    Education c            => new EducationState(c),
                    Film c                 => new FilmState(c),
                    FortuneTellerCareer c  => new FortuneTellerState(c),
                    Journalism c           => new JournalismState(c),
                    LawEnforcement c       => new LawEnforcementState(c),
                    Medical c              => new MedicalState(c),
                    Music c                => new MusicState(c),
                    Political c            => new PoliticalState(c),
                    ProSports c            => new ProSportsState(c),
                    School c               => new SchoolState(c),
                    Science c              => new ScienceState(c),
                    SportsAgentCareer c    => new SportsAgentState(c),
                    Career c               => new CareerState(c),
                    SingerCareer p         => new SingerState(p),
                    PerformanceCareer p    => new PerformanceCareerState(p),
                    ActiveFireFighter a    => new FireFighterState(a),
                    Daycare a              => new DaycareState(a),
                    Lifeguard a            => new LifeguardState(a),
                    PrivateEye a           => new PrivateEyeState(a),
                    ActiveCareer a         => new ActiveCareerState(a),
                    SkillBasedCareer s     => new SkillBasedCareerState(s),
                    _                      => new NullState()
                };

                SavedOccupationsForTravel[id] = state;
            }
            return ListenerAction.Keep;
        }

        internal static ListenerAction OnTaskCompleted(Event e)
        {
            if (e is OccupationTaskEvent { TaskId: TaskId.PickUpFood } occupationTaskEvent && (occupationTaskEvent.Actor as Sim)?.Occupation?.CurrentJob is Job job)
            {
                if (occupationTaskEvent.Undone)
                {
                    job.OnTaskUncomplete(occupationTaskEvent.TaskId, occupationTaskEvent.TargetObject as GameObject);
                }
                else
                {
                    job.OnTaskComplete(occupationTaskEvent.TaskId, occupationTaskEvent.TargetObject as GameObject);
                }
            }
            return ListenerAction.Keep;
        }

        internal static ListenerAction OnPerformanceChange(Event e)
        {
            if (e.Actor is Sim { IsSelectable: true } sim && sim.OccupationAsCareer is Career { Performance: > 99f } career and not School)
            {
                career.mPerformance = 99f;
                sim.CareerManager.UpdatePerformanceUI(career);
            }
            return ListenerAction.Keep;
        }

        internal static ListenerAction OnFinishedWork(Event e)
        {
            AlarmManager.Global.AddAlarm(1f, TimeUnit.Seconds,
                delegate {
                    if (e.Actor is Sim { IsSelectable: true } sim && sim.OccupationAsCareer is Career { Performance: >= 99f } career and not School && RandomUtil.RandomChance(Settings.PromotionChance)
                        && TwoButtonDialog.Show(LocalizeString(sim.IsFemale, "PromotionOfferDialog", sim), LocalizationHelper.Yes, LocalizationHelper.No))
                    {
                        career.ShouldPromote = true;
                        career.PromoteIfShould();
                    }
                }, "Gamefreak130 wuz here", AlarmType.NeverPersisted, null);
            return ListenerAction.Keep;
        }
    }

    public static class Methods
    {
        public static bool TestApplyForProfession(Sim sim, OccupationNames profession, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
        {
            if (!Settings.CareerAvailabilityMap.TryGetValue(profession.ToString(), out PersistedSettings.CareerAvailabilitySettings settings) || !settings.IsAvailable)
            {
                return false;
            }
            if (GameUtils.IsInstalled(ProductVersion.EP9) && settings.RequiredDegrees.Count > 0)
            {
                foreach (AcademicDegreeNames degree in settings.RequiredDegrees)
                {
                    if (sim.DegreeManager is null || !sim.DegreeManager.HasCompletedDegree(degree))
                    {
                        greyedOutTooltipCallback = InteractionInstance.CreateTooltipCallback(LocalizeString(sim.IsFemale, "DoesNotHaveRequiredDegrees"));
                        return false;
                    }
                }
            }
            return Settings.EnableJoinProfessionInRabbitHoleOrLot && CareerManager.GetStaticOccupation(profession) is ActiveCareer activeCareer && IsActiveCareerAvailable(activeCareer) && ActiveCareer.CanAddActiveCareer(sim.SimDescription, profession);
        }

        public static bool FindJobTest(Sim actor, bool restrictToEarnedDegrees)
        {
            bool flag = false;
            return FindJobTest(actor, restrictToEarnedDegrees, ref flag);
        }

        public static bool FindJobTest(Sim actor, bool restrictToEarnedDegrees, ref bool hasDegree)
        {
            hasDegree = false;
            SimDescription simDescription = actor.SimDescription;
            if (simDescription.IsEnrolledInBoardingSchool())
            {
                return false;
            }
            if (restrictToEarnedDegrees)
            {
                bool flag = false;
                if (actor.SkillManager.GetElement(SkillNames.SocialNetworking) is SocialNetworkingSkill socialNetworkingSkill)
                {
                    flag = socialNetworkingSkill.HasEnoughFollowersForOccupationBonus();
                }
                if (flag)
                {
                    restrictToEarnedDegrees = false;
                }
            }
            AcademicDegreeManager degreeManager = actor.CareerManager.DegreeManager;
            if (restrictToEarnedDegrees && degreeManager is not null && degreeManager.HasCompletedAnyDegree())
            {
                hasDegree = true;
            }
            else if (restrictToEarnedDegrees)
            {
                return false;
            }
            foreach (Occupation current in CareerManager.OccupationList)
            {
                string name = (current as Career)?.SharedData.Name.Substring(34) ?? current.Guid.ToString();
                if (Settings.CareerAvailabilityMap.TryGetValue(name, out PersistedSettings.CareerAvailabilitySettings settings) && settings.IsAvailable)
                {
                    if (current is Career career and not School && career.Locations.Count > 0 && career.CareerAgeTest(actor.SimDescription))
                    {
                        return true;
                    }
                    if (current is ActiveCareer { IsAcademicCareer: false } activeCareer && !actor.SimDescription.TeenOrBelow && ActiveCareer.GetActiveCareerStaticData(current.Guid).CanJoinCareerFromComputerOrNewspaper && IsActiveCareerAvailable(activeCareer))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static List<OccupationEntryTuple> GetRandomJobs(Sim actor, int maxJobOpps, bool isResume, int randomSeed)
        {
            bool flag = false;
            if (actor.SkillManager.GetElement(SkillNames.SocialNetworking) is SocialNetworkingSkill socialNetworkingSkill && Settings.NumBonusResumeJobs > 0)
            {
                flag = socialNetworkingSkill.HasEnoughFollowersForOccupationBonus();
            }
            AcademicDegreeManager degreeManager = actor.CareerManager.DegreeManager;
            if (isResume && degreeManager is null && !flag)
            {
                return new();
            }
            Random randomizer = new(randomSeed);
            int numJobOpps = RandomUtil.GetInt(Settings.MinJobOffers, maxJobOpps, randomizer);
            List<OccupationEntryTuple> list = new();
            List<OccupationEntryTuple> list2 = new();
            GreyedOutTooltipCallback tooltipCallback = null;
            if (flag && Career.GetDreamsAndPromisesJobWithLocation(actor) is Career career && career.Locations.Count > 0 && career.CanAcceptCareer(actor.ObjectId, ref tooltipCallback) && career.CareerAgeTest(actor.SimDescription))
            {
                list.Add(new(career, RandomUtil.GetRandomObjectFromList(career.Locations)));
            }
            foreach (RabbitHole rabbitHole in GetObjects<RabbitHole>())
            {
                foreach (CareerLocation location in rabbitHole.CareerLocations.Values)
                {
                    if (location.Career is Career locCareer and not School && !string.IsNullOrEmpty(location.Owner.GetLocalizedName()))
                    {
                        OccupationEntryTuple tuple = new(locCareer, location);
                        if (isResume && degreeManager is not null && degreeManager.HasCompletedDegreeForOccupation(locCareer.Guid) && !list.Contains(tuple) && locCareer.CanAcceptCareer(actor.ObjectId, ref tooltipCallback) && locCareer.CareerAgeTest(actor.SimDescription))
                        {
                            list.Add(tuple);
                            continue;
                        }
                        list2.Add(tuple);
                    }
                }
            }
            foreach (ActiveCareer current in CareerManager.GetActiveCareers())
            {
                if (!current.IsAcademicCareer && ActiveCareer.GetActiveCareerStaticData(current.Guid).CanJoinCareerFromComputerOrNewspaper && IsActiveCareerAvailable(current))
                {
                    OccupationEntryTuple tuple = new(current, null);
                    if (isResume && degreeManager is not null && degreeManager.HasCompletedDegreeForOccupation(current.Guid) && !actor.SimDescription.TeenOrBelow && current.CanAcceptCareer(actor.ObjectId, ref tooltipCallback))
                    {
                        list.Add(tuple);
                        continue;
                    }
                    list2.Add(tuple);
                }
            }
            List<OccupationEntryTuple> list3 = new(numJobOpps);
            int num = 0;
            int randNum = list.Count != 0 ? RandomUtil.GetInt(1, list.Count, randomizer) : 0;
            while ((num < numJobOpps || flag) && list.Count != 0 && num != randNum)
            {
                OccupationEntryTuple rand = RandomUtil.GetRandomObjectFromList(list, randomizer);
                if (rand.OccupationEntry is Occupation occupation)
                {
                    string name = (occupation as Career)?.SharedData.Name.Substring(34) ?? occupation.Guid.ToString();
                    if (Settings.CareerAvailabilityMap.TryGetValue(name, out PersistedSettings.CareerAvailabilitySettings settings) && settings.IsAvailable)
                    {
                        list3.Add(rand);
                        num++;
                    }
                    if (num >= numJobOpps + Settings.NumBonusResumeJobs)
                    {
                        flag = false;
                    }
                    list.Remove(rand);
                }
            }
            while ((num < numJobOpps || flag) && list2.Count != 0)
            {
                OccupationEntryTuple rand = RandomUtil.GetRandomObjectFromList(list2, randomizer);
                if (rand.OccupationEntry is Occupation occupation)
                {
                    string name = (occupation as Career)?.SharedData.Name.Substring(34) ?? occupation.Guid.ToString();
                    if (Settings.CareerAvailabilityMap.TryGetValue(name, out PersistedSettings.CareerAvailabilitySettings settings) && settings.IsAvailable)
                    {
                        list3.Add(rand);
                        num++;
                    }
                    if (num >= numJobOpps + Settings.NumBonusResumeJobs)
                    {
                        flag = false;
                    }
                    list2.Remove(rand);
                }
            }
            return list3;
        }

        public static void OfferJob(Sim sim, OccupationEntryTuple occupation)
        {
            List<OccupationEntryTuple> list = new(1) { occupation };
            UI.CareerSelectionModelEx.Singleton.ShowCareerSelection(sim, sim.ObjectId, list);
        }

        public static void DropOccupation(CareerManager manager)
        {
            OccupationNames previousOccupation = manager.Occupation?.Guid ?? default;
            manager.Occupation?.LeaveJob(false, Career.LeaveJobReason.kNone);
            manager.QuitCareers.Remove(previousOccupation);
        }

        public static bool IsActiveCareerAvailable(ActiveCareer career) => career is Lifeguard && IsPoolLifeguardModInstalled
                ? LotManager.GetCommercialLots(CommercialLotSubType.kBeach) is not null || LotManager.GetCommercialLots(CommercialLotSubType.kPool) is not null
                : career.IsActiveCareerAvailable();

        public static string GetTextDayOfWeek(DateAndTime time)
        {
            int num = (int)SimClock.ConvertFromTicks(time.Ticks, TimeUnit.Days);
            return SimClockUtils.LocalizeString(SimClockUtils.DaysOfWeek[num % 7]);
        }

        internal static string LocalizeString(string name, params object[] parameters) => Localization.LocalizeString("Gamefreak130/LocalizedMod/JobOverhaul:" + name, parameters);

        internal static string LocalizeString(bool isFemale, string name, params object[] parameters) => Localization.LocalizeString(isFemale, "Gamefreak130/LocalizedMod/JobOverhaul:" + name, parameters);
    }
}