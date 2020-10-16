using Gamefreak130.JobOverhaulSpace;
using Gamefreak130.JobOverhaulSpace.Helpers;
using Gamefreak130.JobOverhaulSpace.Helpers.Situations;
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
using Sims3.Gameplay.ChildAndTeenUpdates;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.MapTags;
using Sims3.Gameplay.Objects.Electronics;
using Sims3.Gameplay.Objects.Miscellaneous;
using Sims3.Gameplay.Objects.RabbitHoles;
using Sims3.Gameplay.Objects.Seating;
using Sims3.Gameplay.Opportunities;
using Sims3.Gameplay.Roles;
using Sims3.Gameplay.Skills;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using System;
using System.Collections.Generic;
using System.Reflection;
using static Gamefreak130.JobOverhaulSpace.Helpers.Listeners;
using static Gamefreak130.JobOverhaulSpace.Helpers.Methods;
using static Gamefreak130.JobOverhaulSpace.Interactions.Interviews;
using static Sims3.Gameplay.ActiveCareer.ActiveCareers.DaycareTransportSituation;
using static Sims3.Gameplay.Queries;
using Methods = Gamefreak130.Common.Methods;

namespace Gamefreak130
{
    public class JobOverhaul
    {
        [Tunable]
        private static readonly bool kCJackB;

        static JobOverhaul()
        {
            LoadSaveManager.ObjectGroupsPreLoad += new ObjectGroupsPreLoadHandler(OnPreLoad);
            LoadSaveManager.ObjectGroupsPostLoad += new ObjectGroupsPostLoadHandler(OnPostLoad);
            World.OnWorldQuitEventHandler += new EventHandler(OnWorldQuit);
            World.OnWorldLoadFinishedEventHandler += new EventHandler(OnWorldLoadFinished);
            RandomNewspaperSeed = RandomUtil.GetInt(32767);
            RandomComputerSeed = RandomUtil.GetInt(32767);
        }

        private static void OnPreLoad()
        {
            // DO NOT FORGET
            // UPDATE EVERY FINAL BUILD COMPILATION IN IL
            // TO ACCOUNT FOR DAYCARE VISITLOT WORKAROUND
            // SERIOUSLY DO NOT FORGET THIS YOU FUCKING MORON

            //TODO Add not enough celebrity level string for PT Film
            //TODO Fix newspaper posture cleanup (and test holo computer)
            //TODO Fix Combo rabbitholes for base game
            //CONSIDER Custom degrees for jobs?
            //CONSIDER Audition string?
            //CONSIDER Random amount of jobs per day from specified min to max?
            //CONSIDER Fix Rabbit hole proxy jobs w/out replacing rabbit hole?
            //CONSIDER Multiple interviews, job offers at one time?
            new Common.BuffBooter("Gamefreak130_InterviewBuffs").LoadBuffData();
            Methods.InjectInteraction<Computer>(ref Computer.FindJob.Singleton, new FindJobComputer.Definition(), true);
            Methods.InjectInteraction<Computer>(ref Computer.UploadResume.Singleton, new UploadResumeComputer.Definition(), true);
            Methods.InjectInteraction<Computer>(ref Computer.RegisterAsSelfEmployedComputer.Singleton, new SelfEmployed.RegisterAsSelfEmployedComputer.Definition(), true);
            Methods.InjectInteraction<Newspaper>(ref FindJobNewspaper.Singleton, new FindJobNewspaperEx.Definition(), true);
            Methods.InjectInteraction<Newspaper>(ref RegisterAsSelfEmployedNewspaper.Singleton, new SelfEmployed.RegisterAsSelfEmployedNewspaper.Definition(), true);
            Methods.InjectInteraction<PhoneSmart>(ref Phone.UploadResume.Singleton, new UploadResumePhone.Definition(), true);
            Methods.InjectInteraction<Phone>(ref Phone.CallRegisterAsSelfEmployed.Singleton, new SelfEmployed.RegisterAsSelfEmployedPhone.Definition(), true);
            Methods.InjectInteraction<Phone>(ref Phone.CallToCancelSteadyGig.Singleton, new CallToCancelSteadyGigEx.Definition(), true);
            Methods.InjectInteraction<CityHall>(ref CityHall.RegisterAsSelfEmployed.Singleton, new SelfEmployed.RegisterAsSelfEmployedCityHall.Definition(), true);
            Methods.InjectInteraction<CityHall>(ref CityHall.JoinActiveCareerInteriorDesigner.Singleton, new JoinActiveCareerInteriorDesignerEx.Definition(), true);
            Methods.InjectInteraction<CityHall>(ref CityHall.JoinActiveCareerLifeguard.Singleton, new JoinActiveCareerLifeguardEx.Definition(), true);
            Methods.InjectInteraction<ScienceLab>(ref ScienceLab.JoinActiveCareerGhostHunter.Singleton, new JoinActiveCareerGhostHunterEx.Definition(), true);
            Methods.InjectInteraction<PoliceStation>(ref PoliceStation.JoinActiveCareerPrivateEye.Singleton, new JoinActiveCareerPrivateEyeEx.Definition(), true);
            Methods.InjectInteraction<Lot>(ref Sims3.Gameplay.Services.DeliverNewspaper.Singleton, new DeliverNewspaperEx.Definition(), false);
            Methods.InjectInteraction<Lot>(ref JoinFirefighterActiveCareer.Singleton, new JoinFirefighterActiveCareerEx.Definition(), true);
            Methods.InjectInteraction<Lot>(ref JoinStylistActiveCareer.Singleton, new JoinStylistActiveCareerEx.Definition(), true);
            Methods.InjectInteraction<Lot>(ref GhostHunter.GoToJob.Singleton, new GhostHunterEx.GoToJobEx.Definition(), true);
            Methods.InjectInteraction<Terrain>(ref GhostHunter.ScanForGhosts.Singleton, new GhostHunterEx.ScanForGhostsEx.Definition(), true);
            Methods.InjectInteraction<Sim>(ref InteriorDesigner.EvaluateRenovation.Singleton, new EvaluateRenovationEx.Definition(), false);
            Methods.InjectInteraction<Sim>(ref Styling.StylistRole.JoinActiveCareerStylistSocial.Singleton, new JoinActiveCareerStylistSocialEx.Definition("Join Stylist Active Career"), true);
            Methods.InjectInteraction<Sim>(ref Stylist.FinishGiveFashionAdvice.Singleton, new FinishGiveFashionAdviceEx.Definition(), true);
            Methods.InjectInteraction<Sim>(ref Proprietor.AskToJoinPerformanceCareer.Singleton, new AskToJoinPerformanceCareerEx.Definition("Ask To Join Performance Career", ""), true);
            GoToOccupationJobLocation.Singleton = new GoToOccupationJobLocationEx.Definition();
            VisitLotAndWaitForDaycareGreeting.VisitLotAndWaitForDaycareGreetingDefinition singleton2 = new VisitLotAndWaitForDaycareGreetingEx.VisitLotAndWaitForDaycareGreetingDefinitionEx();
            Common.Tunings.Inject(VisitLotAndWaitForDaycareGreeting.VisitLotAndWaitForDaycareGreetingSingleton.GetType(), typeof(Lot), singleton2.GetType(), typeof(Lot), true);
            VisitLotAndWaitForDaycareGreeting.VisitLotAndWaitForDaycareGreetingSingleton = singleton2;
            foreach (string key in SocialRuleRHS.sDictionary.Keys)
            {
                if (key == "Vaccinate")
                {
                    SocialRuleRHS.sDictionary[key][0].mProceduralEffectBeforeUpdate = typeof(VaccinationSessionSituationEx).GetMethod("BeforeVaccinate");
                }
                if (key == "Diagnose")
                {
                    ActionData.sData[key].ProceduralTest = typeof(FreeClinicSessionSituationEx).GetMethod("TestDiagnosis");
                    SocialRuleRHS.sDictionary[key][0].mProceduralEffectBeforeUpdate = typeof(FreeClinicSessionSituationEx).GetMethod("BeforeDiagnose");
                }
                if (key == "Ask To Join PerformanceArtist Career" || key == "Ask To Join Magician Career" || key == "Ask To Join Singer Career")
                {
                    SocialRuleRHS.sDictionary[key][0].mProceduralEffectAfterUpdate = typeof(AskToJoinPerformanceCareerEx).GetMethod("OnRequestFinish");
                }
                if (key == "Join Stylist Active Career")
                {
                    SocialRuleRHS.sDictionary[key][0].mProceduralEffectAfterUpdate = typeof(JoinActiveCareerStylistSocialEx).GetMethod("OnRequestFinish");
                }
            }
            foreach (Opportunity opportunity in GenericManager<OpportunityNames, Opportunity, Opportunity>.sDictionary.Values)
            {
                if (opportunity.ProductVersion == ProductVersion.BaseGame || opportunity.ProductVersion == ProductVersion.EP2 || opportunity.ProductVersion == ProductVersion.EP3 || opportunity.ProductVersion == ProductVersion.EP4 || (opportunity.ProductVersion == ProductVersion.EP11 && opportunity.IsCareer))
                {
                    if (!(opportunity.Guid.ToString().Contains("HandinessSkill_FixRestaurant") || opportunity.Guid.ToString().Contains("HandinessSkill_Upgrade") || opportunity.Guid.ToString().Contains("HandinessSkill_Repair")))
                    {
                        opportunity.mSharedData.mTargetWorldRequired = WorldName.Undefined;
                    }
                }
                if (opportunity.ProductVersion == ProductVersion.EP7 && opportunity.Guid.ToString().Contains("FortuneTellerCareer"))
                {
                    Opportunity.OpportunitySharedData.RequirementInfo requirement = new Opportunity.OpportunitySharedData.RequirementInfo
                    {
                        mType = RequirementType.Career,
                        mGuid = (ulong)OccupationNames.FortuneTeller,
                        mMinLevel = 1,
                        mMaxLevel = 10
                    };
                    opportunity.mSharedData.mRequirementList.Add(requirement);
                }
                if (opportunity.ProductVersion == ProductVersion.EP9 && opportunity.Guid.ToString().Contains("Academics"))
                {
                    Opportunity.OpportunitySharedData.RequirementInfo requirement = new Opportunity.OpportunitySharedData.RequirementInfo
                    {
                        mType = RequirementType.Career,
                        mGuid = (ulong)OccupationNames.AcademicCareer,
                        mMinLevel = 1,
                        mMaxLevel = 10
                    };
                    opportunity.mSharedData.mRequirementList.Add(requirement);
                }
            }
            if (GameUtils.IsInstalled(ProductVersion.EP3))
            {
                Occupation.sOccupationStaticDataMap.TryGetValue((ulong)OccupationNames.Film, out OccupationStaticData data);
                data.JobStaticDataMap.TryGetValue((uint)JobId.PromoteNewMovieBars, out JobStaticData data2);
                if (Array.IndexOf(data2.CommercialLotSubTypes, CommercialLotSubType.kEP11_FutureBar) == -1)
                {
                    Array.Resize(ref data2.CommercialLotSubTypes, data2.CommercialLotSubTypes.Length + 1);
                    data2.CommercialLotSubTypes[data2.CommercialLotSubTypes.Length - 1] = CommercialLotSubType.kEP11_FutureBar;
                }
            }
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                if (assembly.FullName.Contains("icarusallsorts.PoolLifeguard"))
                {
                    IsPoolLifeguardModInstalled = true;
                }
                if (assembly.FullName.Contains("NRaasOnceRead"))
                {
                    IsOnceReadInstalled = true;
                }
            }
        }

        private static void OnPostLoad()
        {
            Methods.InjectInteraction<RabbitHole>(ref GetJobInRabbitHole.Singleton, new GetJobInRabbitHoleEx.Definition(), true);
            Methods.InjectInteraction<Newspaper>(ref GetNewspaperChooser.Singleton, new GetNewspaperChooserEx.Definition(), true);
            ReadSomethingInInventoryEx.Definition singleton = new ReadSomethingInInventoryEx.Definition();
            Common.Tunings.Inject(Sim.ReadSomethingInInventory.Singleton.GetType(), typeof(Sim), singleton.GetType(), typeof(Sim), true);
            Sim.ReadSomethingInInventory.Singleton = singleton;
            Methods.InjectInteraction<RabbitHole>(ref CollegeOfBusiness.AttendResumeWritingAndInterviewTechniquesClass.Singleton, new AttendResumeAndInterviewClass.Definition(), true);
            BootSettings();
        }

        private static void OnWorldQuit(object sender, EventArgs e)
        {
            SavedOccupationsForTravel.Clear();
            InterviewList.Clear();
            RandomNewspaperSeeds.Clear();
            RandomNewspaperSeed = RandomUtil.GetInt(32767);
            RandomComputerSeed = RandomUtil.GetInt(32767);
        }

        private static void OnWorldLoadFinished(object sender, EventArgs e)
        {
            if (Household.ActiveHousehold != null)
            {
                InitInjection();
                return;
            }
            EventTracker.AddListener(EventTypeId.kEventSimSelected, new ProcessEventDelegate(OnSimSelected));
        }

        private static ListenerAction OnSimSelected(Event e)
        {
            if (Household.ActiveHousehold != null)
            {
                InitInjection();
                return ListenerAction.Remove;
            }
            return ListenerAction.Keep;
        }

        private static void InitInjection()
        {
            foreach (Newspaper newspaper in GetObjects<Newspaper>())
            {
                Methods.AddInteraction(newspaper, ChangeSettings.Singleton);
                newspaper.RemoveInteractionByType(FindActiveCareerNewspaper.Singleton);
                if (!RandomNewspaperSeeds.ContainsKey(newspaper.ObjectId))
                {
                    RandomNewspaperSeeds.Add(newspaper.ObjectId, RandomNewspaperSeed);
                }
            }
            foreach (Computer computer in GetObjects<Computer>())
            {
                Methods.AddInteraction(computer, ChangeSettings.Singleton);
                Type definitionType = Computer.FindActiveCareer.Singleton.GetType();
                computer.RemoveInteractionByType(definitionType);
                InteractionObjectPair iop = null;
                if (computer.ItemComp != null && computer.ItemComp.InteractionsInventory != null)
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
                if (iop != null)
                {
                    computer.ItemComp.InteractionsInventory.Remove(iop);
                }
            }
            foreach (Phone phone in GetObjects<Phone>())
            {
                Methods.AddInteraction(phone, ChangeSettings.Singleton);
            }
            foreach (SchoolRabbitHole school in GetObjects<SchoolRabbitHole>())
            {
                Methods.AddInteraction(school, CollegeOfBusiness.AttendResumeWritingAndInterviewTechniquesClass.Singleton);
            }
            foreach (CityHall cityHall in GetObjects<CityHall>())
            {
                Methods.AddInteraction(cityHall, JoinActiveCareerDaycare.Singleton);
            }
            foreach (LifeguardChair chair in GetObjects<LifeguardChair>())
            {
                chair.RemoveInteractionByType(LifeguardChair.JoinLifeGuardCareer.Singleton);
            }
            AlarmManager.Global.AddAlarm(1f, TimeUnit.Seconds, FixupInterviews, "Gamefreak130 wuz here -- Delayed Fixup", AlarmType.NeverPersisted, null);
            AlarmManager.Global.AddAlarm(1f, TimeUnit.Seconds, FixupCareerOpportunities, "Gamefreak130 wuz here -- Delayed Fixup", AlarmType.NeverPersisted, null);
            AlarmManager.Global.AddAlarm(1f, TimeUnit.Seconds, FixupNewspaperSeeds, "Gamefreak130 wuz here -- Delayed Fixup", AlarmType.NeverPersisted, null);
            AlarmManager.Global.AddAlarm(2f, TimeUnit.Seconds, FixupOccupations, "Gamefreak130 wuz here -- Delayed Fixup", AlarmType.NeverPersisted, null);
            World.OnObjectPlacedInLotEventHandler += new EventHandler(OnObjectPlacedInLot);
            EventTracker.AddListener(EventTypeId.kInventoryObjectAdded, new ProcessEventDelegate(OnObjectChanged));
            EventTracker.AddListener(EventTypeId.kObjectStateChanged, new ProcessEventDelegate(OnObjectChanged));
            EventTracker.AddListener(EventTypeId.kSimDied, new ProcessEventDelegate(OnSimDestroyed));
            EventTracker.AddListener(EventTypeId.kSimDescriptionDisposed, new ProcessEventDelegate(OnSimDestroyed));
            EventTracker.AddListener(EventTypeId.kSentSimToBoardingSchool, new ProcessEventDelegate(OnSimInBoardingSchool));
            EventTracker.AddListener(EventTypeId.kSimEnteredVacationWorld, new ProcessEventDelegate(OnTravelComplete));
            EventTracker.AddListener(EventTypeId.kSimReturnedFromVacationWorld, new ProcessEventDelegate(OnTravelComplete));
            EventTracker.AddListener(EventTypeId.kSimCompletedOccupationTask, new ProcessEventDelegate(OnTaskCompleted));
            EventTracker.AddListener(EventTypeId.kCareerPerformanceChanged, new ProcessEventDelegate(OnPerformanceChange));
            EventTracker.AddListener(EventTypeId.kFinishedWork, new ProcessEventDelegate(OnFinishedWork));
            EventTracker.AddListener(EventTypeId.kTravelToFuture, new ProcessEventDelegate(SaveOccupationForTravel));
            EventTracker.AddListener(EventTypeId.kTravelToPresent, new ProcessEventDelegate(SaveOccupationForTravel));
        }

        // This is absolute garbage
        // But I'm afraid to touch it 'cause it works
        private static void BootSettings()
        {
            List<string> currentInterviews = new List<string>();
            List<string> currentCareers = new List<string>();
            List<string> currentSelfEmployedJobs = new List<string>();
            List<string> newCareers = new List<string>();
            foreach (Career career in CareerManager.CareerList)
            {
                if (GameUtils.IsInstalled(career.SharedData.ProductVersion) && !(career is School))
                {
                    career.mAvailableInFuture = true;
                    string name = career.SharedData.Name.Substring(34);
                    currentInterviews.Add(name);
                    currentCareers.Add(name);
                    newCareers.Add(name);
                }
            }
            List<string> newActiveCareers = new List<string>();
            foreach (ActiveCareer career in CareerManager.GetActiveCareers())
            {
                if (career.GetOccupationStaticDataForActiveCareer().CanJoinCareerFromComputerOrNewspaper && !career.IsAcademicCareer)
                {
                    career.mAvailableInFuture = true;
                    string name = career.Guid.ToString();
                    currentCareers.Add(name);
                    if (!Settings.CareerAvailabilitySettings.ContainsKey(name))
                    {
                        newActiveCareers.Add(career.Guid.ToString());
                    }
                }
            }
            List<string> newSelfEmployedJobs = new List<string>();
            foreach (Occupation occupation in CareerManager.OccupationList)
            {
                if (occupation is SkillBasedCareer)
                {
                    string name = occupation.Guid.ToString();
                    currentSelfEmployedJobs.Add(name);
                    if (!Settings.SelfEmployedAvailabilitySettings.ContainsKey(name))
                    {
                        newSelfEmployedJobs.Add(name);
                    }
                }
            }

            XmlDbData data = XmlDbData.ReadData("Gamefreak130.JobOverhaul.CareerInterviewTuning");
            if (data != null)
            {
                data.Tables.TryGetValue("Career", out XmlDbTable xmlDbTable);
                if (xmlDbTable != null)
                {
                    foreach (XmlDbRow row in xmlDbTable.Rows)
                    {
                        string name = row.GetString("CareerName");
                        if (!string.IsNullOrEmpty(name) && !Settings.InterviewSettings.ContainsKey(name) && newCareers.Contains(name))
                        {
                            bool requiresInterview = row.GetBool("RequiresInterview");
                            string toList = row.GetString("PositiveTraits").Replace(" ", string.Empty);
                            List<string> posTraitList = new List<string>(toList.Split(new char[] { ',' }));
                            List<TraitNames> posTraits = new List<TraitNames>(posTraitList.Count);
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
                                            if (ResourceUtils.HashString64(str) == id)
                                            {
                                                posTraits.Add((TraitNames)ResourceUtils.HashString64(str));
                                            }
                                        }
                                    }
                                }
                            }
                            toList = row.GetString("NegativeTraits").Replace(" ", string.Empty);
                            List<string> negTraitList = new List<string>(toList.Split(new char[] { ',' }));
                            List<TraitNames> negTraits = new List<TraitNames>(negTraitList.Count);
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
                                            if (ResourceUtils.HashString64(str) == id)
                                            {
                                                negTraits.Add((TraitNames)ResourceUtils.HashString64(str));
                                            }
                                        }
                                    }
                                }
                            }
                            toList = row.GetString("RequiredSkills").Replace(" ", string.Empty);
                            List<string> skillList = new List<string>(toList.Split(new char[] { ',' }));
                            List<SkillNames> skills = new List<SkillNames>(skillList.Count);
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
                                            if (ResourceUtils.HashString64(str) == id)
                                            {
                                                skills.Add((SkillNames)ResourceUtils.HashString64(str));
                                            }
                                        }
                                    }
                                }
                            }
                            InterviewSettings interviewData = new InterviewSettings(requiresInterview, posTraits, negTraits, skills);
                            Settings.InterviewSettings.Add(name, interviewData);
                        }
                    }
                }
            }
            data = XmlDbData.ReadData("Gamefreak130.JobOverhaul.AvailableOccupationTuning");
            if (data != null)
            {
                data.Tables.TryGetValue("Occupation", out XmlDbTable xmlDbTable);
                if (xmlDbTable != null)
                {
                    foreach (XmlDbRow row in xmlDbTable.Rows)
                    {
                        string name = row.GetString("OccupationName");
                        if (!string.IsNullOrEmpty(name))
                        {
                            bool isAvailable = row.GetBool("Enabled");
                            string toList = row.GetString("RequiredDegrees").Replace(" ", string.Empty);
                            List<string> degreeList = new List<string>(toList.Split(new char[] { ',' }));
                            List<AcademicDegreeNames> degrees = new List<AcademicDegreeNames>(degreeList.Count);
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
                                            if (ResourceUtils.HashString64(str) == id)
                                            {
                                                degrees.Add((AcademicDegreeNames)ResourceUtils.HashString64(str));
                                            }
                                        }
                                    }
                                }
                            }
                            if (newCareers.Contains(name))
                            {
                                newCareers.Remove(name);
                                if (!Settings.CareerAvailabilitySettings.ContainsKey(name))
                                {
                                    CareerAvailabilitySettings settings = new CareerAvailabilitySettings(isAvailable, false, degrees);
                                    Settings.CareerAvailabilitySettings.Add(name, settings);
                                }
                                continue;
                            }
                            if (newActiveCareers.Contains(name))
                            {
                                newActiveCareers.Remove(name);
                                CareerAvailabilitySettings settings = new CareerAvailabilitySettings(isAvailable, true, degrees);
                                Settings.CareerAvailabilitySettings.Add(name, settings);
                            }
                        }
                    }
                }
            }
            data = XmlDbData.ReadData("Gamefreak130.JobOverhaul.AvailableSelfEmployedTuning");
            if (data != null)
            {
                data.Tables.TryGetValue("SelfEmployedCareer", out XmlDbTable xmlDbTable);
                if (xmlDbTable != null)
                {
                    foreach (XmlDbRow row in xmlDbTable.Rows)
                    {
                        string name = row.TryGetEnum("CareerName", out _, OccupationNames.Undefined) ? row.GetString("CareerName") : ResourceUtils.HashString64(row.GetString("CareerName")).ToString();
                        if (!string.IsNullOrEmpty(name))
                        {
                            bool isAvailable = row.GetBool("Enabled");
                            if (newSelfEmployedJobs.Contains(name))
                            {
                                newSelfEmployedJobs.Remove(name);
                                Settings.SelfEmployedAvailabilitySettings.Add(name, isAvailable);
                            }
                        }
                    }
                }
            }
            foreach (Career career in CareerManager.CareerList)
            {
                string name = career.SharedData.Name.Substring(34);
                if (newCareers.Contains(name))
                {
                    newCareers.Remove(name);
                    if (!Settings.InterviewSettings.ContainsKey(name))
                    {
                        Settings.InterviewSettings.Add(name, new InterviewSettings(true, new List<TraitNames>() { TraitNames.Ambitious }, new List<TraitNames>() { TraitNames.Loser }, new List<SkillNames>()));
                    }
                    if (!Settings.CareerAvailabilitySettings.ContainsKey(name))
                    {
                        Settings.CareerAvailabilitySettings.Add(name, new CareerAvailabilitySettings(true, false, new List<AcademicDegreeNames>()));
                    }
                }
            }
            foreach (ActiveCareer career in CareerManager.GetActiveCareers())
            {
                string name = career.Guid.ToString();
                if (newActiveCareers.Contains(name))
                {
                    newActiveCareers.Remove(name);
                    Settings.CareerAvailabilitySettings.Add(name, new CareerAvailabilitySettings(true, true, new List<AcademicDegreeNames>()));
                }
            }
            foreach (Occupation occupation in CareerManager.OccupationList)
            {
                string name = occupation.Guid.ToString();
                if (newSelfEmployedJobs.Contains(name))
                {
                    newSelfEmployedJobs.Remove(name);
                    Settings.SelfEmployedAvailabilitySettings.Add(name, true);
                }
            }

            string[] interviewKeys = new string[Settings.InterviewSettings.Count];
            string[] careerKeys = new string[Settings.CareerAvailabilitySettings.Count];
            string[] selfEmployedKeys = new string[Settings.SelfEmployedAvailabilitySettings.Count];
            Settings.InterviewSettings.Keys.CopyTo(interviewKeys, 0);
            Settings.CareerAvailabilitySettings.Keys.CopyTo(careerKeys, 0);
            Settings.SelfEmployedAvailabilitySettings.Keys.CopyTo(selfEmployedKeys, 0);
            foreach (string key in interviewKeys)
            {
                if (!currentInterviews.Contains(key))
                {
                    Settings.InterviewSettings.Remove(key);
                }
            }
            foreach (string key in careerKeys)
            {
                if (!currentCareers.Contains(key))
                {
                    Settings.CareerAvailabilitySettings.Remove(key);
                }
            }
            foreach (string key in selfEmployedKeys)
            {
                if (!currentSelfEmployedJobs.Contains(key))
                {
                    Settings.SelfEmployedAvailabilitySettings.Remove(key);
                }
            }
        }

        private static void FixupInterviews()
        {
            foreach (InterviewData data in InterviewList)
            {
                if (ScavengerManager.GetSimFromDescriptionId(data.ActorId) is Sim sim)
                {
                    data.RabbitHole.AddInteraction(new DoInterview.Definition(data));
                    PhoneCell phone = sim.Inventory.Find<PhoneCell>();
                    if (phone != null)
                    {
                        phone.AddInventoryInteraction(new PostponeInterview.Definition(data));
                        phone.AddInventoryInteraction(new CancelInterview.Definition(data));
                    }
                    foreach (PhoneHome phoneHome in GetObjects<PhoneHome>())
                    {
                        AddPhoneInteractions(phoneHome, data);
                    }
                    data.RabbitHoleDisposedListener = EventTracker.AddListener(EventTypeId.kEventObjectDisposed, new ProcessEventDelegate(data.OnRabbitHoleDisposed), null, data.RabbitHole);
                }
            }
        }

        private static void FixupCareerOpportunities()
        {
            MapTagsModel.Singleton.MapTagRefreshAll += new EventHandler(OnMapTagsUpdated);
            foreach (Sim sim in Household.ActiveHousehold.Sims)
            {
                if (sim.OpportunityManager is OpportunityManager manager)
                {
                    List<Opportunity> toRemove = new List<Opportunity>();
                    foreach (Opportunity opportunity in manager.List)
                    {
                        if ((opportunity.IsCareer || opportunity.IsSkill || opportunity.IsLocationBased || opportunity.IsDare || opportunity.IsSocialGroup || opportunity.IsDayJob) && opportunity.WorldStartedIn != GameUtils.GetCurrentWorld())
                        {
                            foreach (Opportunity.OpportunitySharedData.EventInfo info in opportunity.EventList)
                            {
                                if (info.mEventId == EventTypeId.kSimEnteredVacationWorld || info.mEventId == EventTypeId.kSimReturnedFromVacationWorld)
                                {
                                    goto CJACK_01;
                                }
                            }
                            toRemove.Add(opportunity);
                        }
                        CJACK_01:
                        continue;
                    }
                    foreach (Opportunity opportunity in toRemove)
                    {
                        if (opportunity.IsDare || opportunity.IsSocialGroup || opportunity.IsDayJob || opportunity.Guid.ToString().Contains("Academics"))
                        {
                            manager.CancelOpportunity(opportunity.Guid, false);
                            continue;
                        }
                        manager.RemoveElement((ulong)opportunity.OpportunityCategory);
                        opportunity.Cleanup(true);
                        OpportunityTrackerModel.FireOpportunitiesChanged();
                    }
                }
            }
        }

        private static void FixupOccupations()
        {
            foreach (Sim sim in GetObjects<Sim>())
            {
                if (sim.CareerManager is CareerManager manager)
                {
                    ulong id = sim.SimDescription.SimDescriptionId;
                    if (SavedOccupationsForTravel.ContainsKey(id))
                    {
                        if (SavedOccupationsForTravel[id] != null)
                        {
                            SavedTravelOccupation savedOccupation = SavedOccupationsForTravel[id];
                            Occupation occupation = CareerManager.GetStaticOccupation(savedOccupation.Guid);
                            OccupationNames previousOccupation = manager.Occupation.Guid;
                            CareerLocation location = null;
                            if (savedOccupation.RabbitHole != null)
                            {
                                savedOccupation.RabbitHole.CareerLocations.TryGetValue((ulong)savedOccupation.Guid, out location);
                            }
                            else if (occupation is Career)
                            {
                                continue;
                            }
                            if (manager.Occupation != null)
                            {
                                manager.Occupation.LeaveJob(false, Career.LeaveJobReason.kDebug);
                            }
                            GreyedOutTooltipCallback callback = null;
                            if (occupation is SkillBasedCareer || (occupation is Career career && career.CareerAgeTest(sim.SimDescription) && career.CanAcceptCareer(sim.ObjectId, ref callback)) || (occupation is ActiveCareer activeCareer && activeCareer.IsActiveCareerAvailable() && (activeCareer.GetOccupationStaticDataForActiveCareer().ValidAges & sim.SimDescription.Age) != CASAgeGenderFlags.None))
                            {
                                manager.QuitCareers.Remove(previousOccupation);
                                AcquireOccupationParameters parameters = new AcquireOccupationParameters(savedOccupation.Guid, location, false, false);
                                if (occupation is Career)
                                {
                                    parameters.JumpStartJob = true;
                                    (occupation as Career).CareerLevels.TryGetValue(savedOccupation.Branch, out Dictionary<int, CareerLevel> dictionary);
                                    dictionary.TryGetValue(savedOccupation.Level, out CareerLevel level);
                                    parameters.JumpStartLevel = level;
                                }
                                manager.AcquireOccupation(parameters);
                                if (manager.OccupationAsCareer is Career newCareer)
                                {
                                    newCareer.WorkAnniversary = savedOccupation.Anniversary;
                                    newCareer.mPerformance = savedOccupation.Xp;
                                    newCareer.mPayPerHourExtra = savedOccupation.ExtraPay;
                                    SimDescription description = SimDescription.Find(savedOccupation.BossId);
                                    if (description != null && newCareer.Coworkers.Contains(description))
                                    {
                                        newCareer.SetBoss(description);
                                    }
                                }
                                else
                                {
                                    XpBasedCareer xpBasedCareer = manager.Occupation as XpBasedCareer;
                                    if ((xpBasedCareer as SkillBasedCareer) != null)
                                    {
                                        (xpBasedCareer as SkillBasedCareer).mMoneyEarned = savedOccupation.MoneyEarned;
                                    }
                                    xpBasedCareer.mLevel = savedOccupation.Level;
                                    xpBasedCareer.mOvermaxLevel = savedOccupation.OverMaxLevel;
                                    xpBasedCareer.mXp = savedOccupation.Xp;
                                }
                            }
                        }
                        else if (manager.Occupation != null)
                        {
                            manager.Occupation.Cleanup();
                            manager.mJob = null;
                        }
                        SavedOccupationsForTravel.Remove(id);
                    }
                    if ((GameUtils.IsFutureWorld() && Sims3.Gameplay.GameStates.IsNewGame) || (manager.OccupationAsActiveCareer is Lifeguard lifeguardCareer && !IsActiveCareerAvailable(lifeguardCareer)))
                    {
                        manager.Occupation.Cleanup();
                        manager.mJob = null;
                    }
                }
            }
        }

        private static void FixupNewspaperSeeds()
        {
            ObjectGuid[] keys = new ObjectGuid[Settings.InterviewSettings.Count];
            RandomNewspaperSeeds.Keys.CopyTo(keys, 0);
            foreach (ObjectGuid id in keys)
            {
                if (GameObject.GetObject(id) == null || (GameObject.GetObject(id) is Newspaper newspaper && newspaper.IsOld))
                {
                    RandomNewspaperSeeds.Remove(id);
                }
            }
        }

        public const ulong kReadyForInterviewGuid = 0xCA57D12A3647413D;

        public const ulong kGotTheJobGuid = 0x1770B99317A5D98A;

        public const ulong kBadInterviewGuid = 0x552A7AD84AF2FA7E;

        public static bool IsPoolLifeguardModInstalled;

        public static bool IsOnceReadInstalled;

        [PersistableStatic]
        public static Dictionary<ulong, SavedTravelOccupation> SavedOccupationsForTravel = new Dictionary<ulong, SavedTravelOccupation>();

        [PersistableStatic]
        public static int RandomNewspaperSeed;

        [PersistableStatic]
        public static Dictionary<ObjectGuid, int> RandomNewspaperSeeds = new Dictionary<ObjectGuid, int>();

        [PersistableStatic]
        public static int RandomComputerSeed;

        [PersistableStatic]
        private static PersistedSettings sSettings;

        public static PersistedSettings Settings
        {
            get
            {
                if (sSettings == null)
                {
                    sSettings = new PersistedSettings();
                }
                return sSettings;
            }
        }

        internal static void ResetSettings()
        {
            sSettings = null;
            BootSettings();
        }
    }
}