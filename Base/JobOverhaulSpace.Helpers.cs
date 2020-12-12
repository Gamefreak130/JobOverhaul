﻿using Gamefreak130.JobOverhaulSpace.Helpers.OccupationStates;
using Gamefreak130.JobOverhaulSpace.Interactions;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Academics;
using Sims3.Gameplay.ActiveCareer;
using Sims3.Gameplay.ActiveCareer.ActiveCareers;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
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
using System.Reflection;
using System.Xml;
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

        public CareerLocation CareerLocation { get; set; }

        public OccupationEntryTuple()
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
                GameObject @object = GameObject.GetObject(onObjectPlacedInLotEventArgs.ObjectId);
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
                if (@object is PhoneHome phone)
                {
                    foreach (InterviewData data in InterviewList)
                    {
                        AddPhoneInteractions(phone, data);
                    }
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
        }

        internal static ListenerAction OnObjectChanged(Event e)
        {
            if (e.TargetObject is Computer or Phone or Newspaper)
            {
                Common.Helpers.AddInteraction((GameObject)e.TargetObject, ChangeSettings.Singleton);
            }
            if (e.TargetObject is Computer computer)
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
            if (e.TargetObject is Newspaper newspaper)
            {
                newspaper.RemoveInteractionByType(FindActiveCareerNewspaper.Singleton);
                if (!RandomNewspaperSeeds.ContainsKey(newspaper.ObjectId))
                {
                    RandomNewspaperSeeds.Add(newspaper.ObjectId, RandomNewspaperSeed + SimClock.ElapsedCalendarDays());
                }
            }
            if (e.TargetObject is PhoneHome phoneHome)
            {
                foreach (InterviewData data in InterviewList)
                {
                    AddPhoneInteractions(phoneHome, data);
                }
            }
            if (e.TargetObject is SchoolRabbitHole school)
            {
                Common.Helpers.AddInteraction(school, CollegeOfBusiness.AttendResumeWritingAndInterviewTechniquesClass.Singleton);
            }
            if (e.TargetObject is CityHall cityHall)
            {
                Common.Helpers.AddInteraction(cityHall, JoinActiveCareerDaycare.Singleton);
            }
            if (e.TargetObject is LifeguardChair chair)
            {
                chair.RemoveInteractionByType(LifeguardChair.JoinLifeGuardCareer.Singleton);
            }
            return ListenerAction.Keep;
        }

        internal static ListenerAction OnSimInBoardingSchool(Event e)
        {
            if (e.TargetObject is Sim targetSim)
            {
                foreach (InterviewData data in InterviewList)
                {
                    if (data.ActorId == targetSim.SimDescription.SimDescriptionId)
                    {
                        InteractionObjectPair iop = null;
                        foreach (InteractionObjectPair current in data.RabbitHole.Interactions)
                        {
                            if ((current.InteractionDefinition as DoInterview.Definition)?.mData.ActorId == data.ActorId)
                            {
                                iop = current;
                                break;
                            }
                        }
                        if (iop is not null)
                        {
                            data.RabbitHole.RemoveInteraction(iop);
                        }
                        PhoneCell phone = targetSim.Inventory.Find<PhoneCell>();
                        if (phone is not null)
                        {
                            RemovePhoneInteractions(targetSim, phone, false);
                        }
                        foreach (PhoneHome phoneHome in GetObjects<PhoneHome>())
                        {
                            RemovePhoneInteractions(targetSim, phoneHome, false);
                        }
                        AlarmManager.Global.RemoveAlarm(data.RemindAlarm);
                        AlarmManager.Global.RemoveAlarm(data.TimeoutAlarm);
                        InterviewList.Remove(data);
                    }
                }
            }
            return ListenerAction.Keep;
        }

        internal static ListenerAction OnSimDestroyed(Event e)
        {
            if (e.Actor is not null)
            {
                foreach (InterviewData data in InterviewList)
                {
                    if (data.ActorId == e.Actor.SimDescription.SimDescriptionId)
                    {
                        InteractionObjectPair iop = null;
                        foreach (InteractionObjectPair current in data.RabbitHole.Interactions)
                        {
                            if ((current.InteractionDefinition as DoInterview.Definition)?.mData.ActorId == data.ActorId)
                            {
                                iop = current;
                                break;
                            }
                        }
                        if (iop is not null)
                        {
                            data.RabbitHole.RemoveInteraction(iop);
                        }
                        foreach (PhoneHome phoneHome in GetObjects<PhoneHome>())
                        {
                            RemovePhoneInteractions(e.Actor as Sim, phoneHome, false);
                        }
                        AlarmManager.Global.RemoveAlarm(data.RemindAlarm);
                        AlarmManager.Global.RemoveAlarm(data.TimeoutAlarm);
                        InterviewList.Remove(data);
                    }
                }
            }
            return ListenerAction.Keep;
        }

        internal static ListenerAction OnTravelComplete(Event e)
        {
            if (e.Actor is Sim sim)
            {
                foreach (Sims3.Gameplay.InventoryStack stack in sim.Inventory.InventoryItems.Values)
                {
                    foreach (Sims3.Gameplay.InventoryItem item in stack.List)
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

                if (SavedOccupationsForTravel.ContainsKey(id))
                {
                    SavedOccupationsForTravel[id] = state;
                }
                else
                {
                    SavedOccupationsForTravel.Add(id, state);
                }
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
                delegate () {
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
            if (!Settings.CareerAvailabilitySettings.TryGetValue(profession.ToString(), out CareerAvailabilitySettings settings) || !settings.IsAvailable)
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

        public static void AddPhoneInteractions(PhoneHome phoneHome, InterviewData data)
        {
            bool hasPostpone = false;
            bool hasCancel = false;
            foreach (InteractionObjectPair iop in phoneHome.Interactions)
            {
                if ((iop.InteractionDefinition as PostponeInterview.Definition)?.mData == data)
                {
                    hasPostpone = true;
                }
                if ((iop.InteractionDefinition as CancelInterview.Definition)?.mData == data)
                {
                    hasCancel = true;
                }
            }
            if (!hasPostpone)
            {
                phoneHome.AddInteraction(new PostponeInterview.Definition(data));
            }
            if (!hasCancel)
            {
                phoneHome.AddInteraction(new CancelInterview.Definition(data));
            }
        }

        public static void RemovePhoneInteractions(Sim actor, Phone phone, bool stopCurrentInteraction)
        {
            InteractionObjectPair postponeIop = null;
            InteractionObjectPair cancelIop = null;
            ulong id = actor.SimDescription.SimDescriptionId;
            List<InteractionObjectPair> pairs = phone is PhoneCell ? phone.ItemComp.InteractionsInventory : phone.Interactions;
            foreach (InteractionObjectPair current in pairs)
            {
                if ((current.InteractionDefinition as PostponeInterview.Definition)?.mData.ActorId == id)
                {
                    postponeIop = current;
                }
                if ((current.InteractionDefinition as CancelInterview.Definition)?.mData.ActorId == id)
                {
                    cancelIop = current;
                }
            }
            if (postponeIop is not null)
            {
                pairs.Remove(postponeIop);
                for (int i = actor.InteractionQueue.Count - 1; i >= 0; i--)
                {
                    InteractionInstance current = actor.InteractionQueue.mInteractionList[i];
                    if (current.InteractionDefinition == postponeIop.InteractionDefinition)
                    {
                        actor.InteractionQueue.RemoveInteractionByRef(current);
                    }
                }
            }
            if (cancelIop is not null)
            {
                pairs.Remove(cancelIop);
                for (int i = actor.InteractionQueue.Count - 1; i >= 0; i--)
                {
                    InteractionInstance current = actor.InteractionQueue.mInteractionList[i];
                    if (current.InteractionDefinition == cancelIop.InteractionDefinition)
                    {
                        actor.InteractionQueue.RemoveInteractionByRef(current);
                    }
                }
            }
            if (actor.InteractionQueue.GetCurrentInteraction() is InteractionInstance instance)
            {
                if (stopCurrentInteraction && (instance.InteractionDefinition.GetType() == postponeIop.InteractionDefinition.GetType() || instance.InteractionDefinition.GetType() == cancelIop.InteractionDefinition.GetType()))
                {
                    actor.InteractionQueue.CancelNthInteraction(0, false, ExitReason.CanceledByScript);
                }
            }
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
                if (Settings.CareerAvailabilitySettings.TryGetValue(name, out CareerAvailabilitySettings settings) && settings.IsAvailable)
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

        public static List<OccupationEntryTuple> GetRandomJobs(Sim actor, int numbJobOpps, bool isResume, int randomSeed)
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
            Random randomizer = new(randomSeed);
            List<OccupationEntryTuple> list3 = new(numbJobOpps);
            int num = 0;
            int randNum = list.Count != 0 ? RandomUtil.GetInt(1, list.Count, randomizer) : 0;
            while ((num < numbJobOpps || flag) && list.Count != 0 && num != randNum)
            {
                OccupationEntryTuple rand = RandomUtil.GetRandomObjectFromList(list, randomizer);
                if (rand.OccupationEntry is Occupation occupation)
                {
                    string name = (occupation as Career)?.SharedData.Name.Substring(34) ?? occupation.Guid.ToString();
                    if (Settings.CareerAvailabilitySettings.TryGetValue(name, out CareerAvailabilitySettings settings) && settings.IsAvailable)
                    {
                        list3.Add(rand);
                        num++;
                    }
                    if (num >= numbJobOpps + Settings.NumBonusResumeJobs)
                    {
                        flag = false;
                    }
                    list.Remove(rand);
                }
            }
            while ((num < numbJobOpps || flag) && list2.Count != 0)
            {
                OccupationEntryTuple rand = RandomUtil.GetRandomObjectFromList(list2, randomizer);
                if (rand.OccupationEntry is Occupation occupation)
                {
                    string name = (occupation as Career)?.SharedData.Name.Substring(34) ?? occupation.Guid.ToString();
                    if (Settings.CareerAvailabilitySettings.TryGetValue(name, out CareerAvailabilitySettings settings) && settings.IsAvailable)
                    {
                        list3.Add(rand);
                        num++;
                    }
                    if (num >= numbJobOpps + Settings.NumBonusResumeJobs)
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

        internal static void ParseXml(XmlNode startElement)
        {
            for (XmlNode node = startElement; node is not null; node = node.NextSibling)
            {
                if (node.Attributes["value"] is not null)
                {
                    SetImplicitValue(node.Name, node.Attributes["value"].Value);
                }
                else
                {
                    if (node.Name is "mInterviewSettings")
                    {
                        Dictionary<string, InterviewSettings> dict = new();
                        for (XmlNode node2 = node.FirstChild; node2 is not null; node2 = node2.NextSibling)
                        {
                            bool.TryParse(node2.ChildNodes.Item(0).Attributes["value"].Value, out bool val);
                            List<string> posTraitList = new(node2.ChildNodes.Item(1).Attributes["value"].Value.Split(new[] { ',' }));
                            List<TraitNames> posTraits = new(posTraitList.Count);
                            foreach (string str in posTraitList)
                            {
                                if (!string.IsNullOrEmpty(str))
                                {
                                    if (ParserFunctions.TryParseEnum(str, out TraitNames trait, TraitNames.Unknown))
                                    {
                                        posTraits.Add(trait);
                                    }
                                    else
                                    {
                                        foreach (ulong id in GenericManager<TraitNames, Trait, Trait>.sDictionary.Keys)
                                        {
                                            if (str == id.ToString())
                                            {
                                                posTraits.Add((TraitNames)id);
                                            }
                                        }
                                    }
                                }
                            }
                            List<string> negTraitList = new(node2.ChildNodes.Item(2).Attributes["value"].Value.Split(new[] { ',' }));
                            List<TraitNames> negTraits = new(negTraitList.Count);
                            foreach (string str in negTraitList)
                            {
                                if (!string.IsNullOrEmpty(str))
                                {
                                    if (ParserFunctions.TryParseEnum(str, out TraitNames trait, TraitNames.Unknown))
                                    {
                                        negTraits.Add(trait);
                                    }
                                    else
                                    {
                                        foreach (ulong id in GenericManager<TraitNames, Trait, Trait>.sDictionary.Keys)
                                        {
                                            if (str == id.ToString())
                                            {
                                                negTraits.Add((TraitNames)id);
                                            }
                                        }
                                    }
                                }
                            }
                            List<string> skillList = new(node2.ChildNodes.Item(3).Attributes["value"].Value.Split(new[] { ',' }));
                            List<SkillNames> skills = new(skillList.Count);
                            foreach (string str in skillList)
                            {
                                if (!string.IsNullOrEmpty(str))
                                {
                                    if (ParserFunctions.TryParseEnum(str, out SkillNames skill, SkillNames.None))
                                    {
                                        skills.Add(skill);
                                    }
                                    else
                                    {
                                        foreach (ulong id in GenericManager<SkillNames, Skill, Skill>.sDictionary.Keys)
                                        {
                                            if (str == id.ToString())
                                            {
                                                skills.Add((SkillNames)id);
                                            }
                                        }
                                    }
                                }
                            }
                            dict.Add(node2.Name, new(val, posTraits, negTraits, skills));
                        }
                        foreach (string key in dict.Keys)
                        {
                            if (Settings.InterviewSettings.ContainsKey(key))
                            {
                                Settings.InterviewSettings[key] = dict[key];
                            }
                        }
                    }
                    else if (node.Name is "mCareerAvailabilitySettings")
                    {
                        Dictionary<string, CareerAvailabilitySettings> dict = new();
                        for (XmlNode node2 = node.FirstChild; node2 is not null; node2 = node2.NextSibling)
                        {
                            bool.TryParse(node2.ChildNodes.Item(0).Attributes["value"].Value, out bool val);
                            List<string> degreeList = new(node2.ChildNodes.Item(1).Attributes["value"].Value.Split(new[] { ',' }));
                            List<AcademicDegreeNames> degrees = new(degreeList.Count);
                            foreach (string str in degreeList)
                            {
                                if (!string.IsNullOrEmpty(str))
                                {
                                    if (ParserFunctions.TryParseEnum(str, out AcademicDegreeNames degree, AcademicDegreeNames.Undefined))
                                    {
                                        degrees.Add(degree);
                                    }
                                    else
                                    {
                                        foreach (ulong id in GenericManager<AcademicDegreeNames, AcademicDegreeStaticData, AcademicDegree>.sDictionary.Keys)
                                        {
                                            if (str == id.ToString())
                                            {
                                                degrees.Add((AcademicDegreeNames)id);
                                            }
                                        }
                                    }
                                }
                            }
                            dict.Add(node2.Name.Substring(1), new(val, false, degrees));
                        }
                        foreach (string key in dict.Keys)
                        {
                            if (Settings.CareerAvailabilitySettings.ContainsKey(key))
                            {
                                Settings.CareerAvailabilitySettings[key].IsAvailable = dict[key].IsAvailable;
                                Settings.CareerAvailabilitySettings[key].RequiredDegrees = dict[key].RequiredDegrees;
                            }
                        }
                    }
                    else if (node.Name is "mSelfEmployedAvailabilitySettings")
                    {
                        Dictionary<string, bool> dict = new();
                        for (XmlNode node2 = node.FirstChild; node2 is not null; node2 = node2.NextSibling)
                        {
                            bool.TryParse(node2.Attributes["value"].Value, out bool val);
                            dict.Add(node2.Name.Substring(1), val);
                        }
                        foreach (string key in dict.Keys)
                        {
                            if (Settings.SelfEmployedAvailabilitySettings.ContainsKey(key))
                            {
                                Settings.SelfEmployedAvailabilitySettings[key] = dict[key];
                            }
                        }
                    }
                }
            }
        }

        private static void SetImplicitValue(string name, string value)
        {
            FieldInfo info = typeof(PersistedSettings).GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (info.FieldType == typeof(bool))
            {
                bool.TryParse(value, out bool val);
                info.SetValue(Settings, val);
            }
            if (info.FieldType == typeof(int))
            {
                int.TryParse(value, out int val2);
                info.SetValue(Settings, val2);
            }
            if (info.FieldType == typeof(float))
            {
                float.TryParse(value, out float val3);
                info.SetValue(Settings, val3);
            }
        }

        public static string GetTextDayOfWeek(DateAndTime time)
        {
            int num = (int)SimClock.ConvertFromTicks(time.Ticks, TimeUnit.Days);
            return SimClockUtils.LocalizeString(SimClockUtils.DaysOfWeek[num % 7]);
        }

        internal static string LocalizeString(string name, params object[] parameters) => Localization.LocalizeString("Gamefreak130/LocalizedMod/JobOverhaul:" + name, parameters);

        internal static string LocalizeString(bool isFemale, string name, params object[] parameters) => Localization.LocalizeString(isFemale, "Gamefreak130/LocalizedMod/JobOverhaul:" + name, parameters);
    }
}