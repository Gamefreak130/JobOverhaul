using Gamefreak130.JobOverhaulSpace;
using Gamefreak130.JobOverhaulSpace.Helpers.OccupationStates;
using Gamefreak130.JobOverhaulSpace.Interactions;
using Gamefreak130.JobOverhaulSpace.Situations;
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
using Sims3.Gameplay.MapTags;
using Sims3.Gameplay.Objects.Electronics;
using Sims3.Gameplay.Objects.Miscellaneous;
using Sims3.Gameplay.Objects.RabbitHoles;
using Sims3.Gameplay.Objects.Seating;
using Sims3.Gameplay.Opportunities;
using Sims3.Gameplay.Roles;
using Sims3.Gameplay.Services;
using Sims3.Gameplay.Skills;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using System;
using System.Collections.Generic;
using static Gamefreak130.JobOverhaulSpace.Helpers.Listeners;
using static Gamefreak130.JobOverhaulSpace.Helpers.Methods;
using static Gamefreak130.JobOverhaulSpace.Interactions.Interviews;
using static Sims3.Gameplay.Queries;

namespace Gamefreak130
{
    public static class JobOverhaul
    {
        [Tunable]
        private static readonly bool kCJackB;

        static JobOverhaul()
        {
            LoadSaveManager.ObjectGroupsPreLoad += OnPreLoad;
            LoadSaveManager.ObjectGroupsPostLoad += OnPostLoad;
            World.OnWorldQuitEventHandler += OnWorldQuit;
            World.OnWorldLoadFinishedEventHandler += OnWorldLoadFinished;
            RandomNewspaperSeed = RandomUtil.GetInt(32767);
            RandomComputerSeed = RandomUtil.GetInt(32767);
        }

        private static void OnPreLoad()
        {
            // DO NOT FORGET
            // UPDATE EVERY FINAL BUILD COMPILATION IN IL
            // TO ACCOUNT FOR DAYCARE VISITLOT WORKAROUND
            // SERIOUSLY DO NOT FORGET THIS YOU FUCKING MORON

            //TEST In-place upgrade
            //CONSIDER Move QuitWork?
            //CONSIDER Random amount of jobs per day from specified min to max?
            //CONSIDER Fix Rabbit hole proxy jobs w/out replacing rabbit hole?
            //CONSIDER Move FixupInterviews to worldload?
            //TODO Fixup the rest of the string tables

            IsPoolLifeguardModInstalled = Common.Helpers.FindAssembly("icarusallsorts.PoolLifeguard");
            IsOnceReadInstalled = Common.Helpers.FindAssembly("NRaasOnceRead");
            IsCareersInstalled = Common.Helpers.FindAssembly("NRaasCareer");
            new Common.BuffBooter("Gamefreak130_InterviewBuffs").LoadBuffData();
            Common.Helpers.InjectInteraction<Computer>(ref Computer.FindJob.Singleton, new FindJobComputer.Definition(), true);
            Common.Helpers.InjectInteraction<Computer>(ref Computer.UploadResume.Singleton, new UploadResumeComputer.Definition(), true);
            Common.Helpers.InjectInteraction<Computer>(ref Computer.RegisterAsSelfEmployedComputer.Singleton, new SelfEmployed.RegisterAsSelfEmployedComputer.Definition(), true);
            Common.Helpers.InjectInteraction<Newspaper>(ref FindJobNewspaper.Singleton, new FindJobNewspaperEx.Definition(), true);
            Common.Helpers.InjectInteraction<Newspaper>(ref RegisterAsSelfEmployedNewspaper.Singleton, new SelfEmployed.RegisterAsSelfEmployedNewspaperEx.Definition(), true);
            Common.Helpers.InjectInteraction<PhoneSmart>(ref Phone.UploadResume.Singleton, new UploadResumePhone.Definition(), true);
            Common.Helpers.InjectInteraction<Phone>(ref Phone.CallRegisterAsSelfEmployed.Singleton, new SelfEmployed.RegisterAsSelfEmployedPhone.Definition(), true);
            Common.Helpers.InjectInteraction<Phone>(ref Phone.CallToCancelSteadyGig.Singleton, new CallToCancelSteadyGigEx.Definition(), true);
            Common.Helpers.InjectInteraction<CityHall>(ref CityHall.JoinActiveCareerInteriorDesigner.Singleton, new JoinActiveCareerInteriorDesignerEx.Definition(), true);
            Common.Helpers.InjectInteraction<CityHall>(ref CityHall.JoinActiveCareerLifeguard.Singleton, new JoinActiveCareerLifeguardEx.Definition(), true);
            Common.Helpers.InjectInteraction<ScienceLab>(ref ScienceLab.JoinActiveCareerGhostHunter.Singleton, new JoinActiveCareerGhostHunterEx.Definition(), true);
            Common.Helpers.InjectInteraction<PoliceStation>(ref PoliceStation.JoinActiveCareerPrivateEye.Singleton, new JoinActiveCareerPrivateEyeEx.Definition(), true);
            Common.Helpers.InjectInteraction<Lot>(ref DeliverNewspaper.Singleton, new DeliverNewspaperEx.Definition(), false);
            Common.Helpers.InjectInteraction<Lot>(ref JoinFirefighterActiveCareer.Singleton, new JoinFirefighterActiveCareerEx.Definition(), true);
            Common.Helpers.InjectInteraction<Lot>(ref JoinStylistActiveCareer.Singleton, new JoinStylistActiveCareerEx.Definition(), true);
            Common.Helpers.InjectInteraction<Lot>(ref GhostHunter.GoToJob.Singleton, new GhostHunterEx.GoToJobEx.Definition(), true);
            Common.Helpers.InjectInteraction<Terrain>(ref GhostHunter.ScanForGhosts.Singleton, new GhostHunterEx.ScanForGhostsEx.Definition(), true);
            Common.Helpers.InjectInteraction<Sim>(ref InteriorDesigner.EvaluateRenovation.Singleton, new EvaluateRenovationEx.Definition(), false);
            Common.Helpers.InjectInteraction<Sim>(ref Styling.StylistRole.JoinActiveCareerStylistSocial.Singleton, new JoinActiveCareerStylistSocialEx.Definition("Join Stylist Active Career"), true);
            Common.Helpers.InjectInteraction<Sim>(ref Stylist.FinishGiveFashionAdvice.Singleton, new FinishGiveFashionAdviceEx.Definition(), true);
            Common.Helpers.InjectInteraction<Sim>(ref Proprietor.AskToJoinPerformanceCareer.Singleton, new AskToJoinPerformanceCareerEx.Definition("Ask To Join Performance Career", ""), true);
            if (!IsCareersInstalled)
            {
                Common.Helpers.InjectInteraction<CityHall>(ref CityHall.RegisterAsSelfEmployed.Singleton, new SelfEmployed.RegisterAsSelfEmployedCityHall.Definition(), true);
            }
            Common.Helpers.InjectInteraction<GameObject, GoToOccupationJobLocation.Definition>(ref GoToOccupationJobLocation.Singleton, new GoToOccupationJobLocationEx.Definition(), false);
            Common.Helpers.InjectInteraction<Lot, DaycareTransportSituation.VisitLotAndWaitForDaycareGreeting.VisitLotAndWaitForDaycareGreetingDefinition>(ref DaycareTransportSituation.VisitLotAndWaitForDaycareGreeting.VisitLotAndWaitForDaycareGreetingSingleton,
                new VisitLotAndWaitForDaycareGreetingEx.VisitLotAndWaitForDaycareGreetingDefinitionEx(),
                true);
            foreach (string key in SocialRuleRHS.sDictionary.Keys)
            {
                if (key is "Vaccinate")
                {
                    SocialRuleRHS.sDictionary[key][0].mProceduralEffectBeforeUpdate = typeof(VaccinationSessionSituationEx).GetMethod("BeforeVaccinate");
                }
                if (key is "Diagnose")
                {
                    ActionData.sData[key].ProceduralTest = typeof(FreeClinicSessionSituationEx).GetMethod("TestDiagnosis");
                    SocialRuleRHS.sDictionary[key][0].mProceduralEffectBeforeUpdate = typeof(FreeClinicSessionSituationEx).GetMethod("BeforeDiagnose");
                }
                if (key is "Ask To Join PerformanceArtist Career" or "Ask To Join Magician Career" or "Ask To Join Singer Career")
                {
                    SocialRuleRHS.sDictionary[key][0].mProceduralEffectAfterUpdate = typeof(AskToJoinPerformanceCareerEx).GetMethod("OnRequestFinish");
                }
                if (key is "Join Stylist Active Career")
                {
                    SocialRuleRHS.sDictionary[key][0].mProceduralEffectAfterUpdate = typeof(JoinActiveCareerStylistSocialEx).GetMethod("OnRequestFinish");
                }
            }
            foreach (Opportunity opportunity in GenericManager<OpportunityNames, Opportunity, Opportunity>.sDictionary.Values)
            {
                if (opportunity is { ProductVersion: ProductVersion.BaseGame or ProductVersion.EP2 or ProductVersion.EP3 or ProductVersion.EP4 } or { ProductVersion: ProductVersion.EP11, IsCareer: true })
                {
                    if (!(opportunity.Guid.ToString().Contains("HandinessSkill_FixRestaurant") || opportunity.Guid.ToString().Contains("HandinessSkill_Upgrade") || opportunity.Guid.ToString().Contains("HandinessSkill_Repair")))
                    {
                        opportunity.mSharedData.mTargetWorldRequired = WorldName.Undefined;
                    }
                }
                if (opportunity.ProductVersion is ProductVersion.EP7 && opportunity.Guid.ToString().Contains("FortuneTellerCareer"))
                {
                    Opportunity.OpportunitySharedData.RequirementInfo requirement = new()
                    {
                        mType = RequirementType.Career,
                        mGuid = (ulong)OccupationNames.FortuneTeller,
                        mMinLevel = 1,
                        mMaxLevel = 10
                    };
                    opportunity.mSharedData.mRequirementList.Add(requirement);
                }
                if (opportunity.ProductVersion is ProductVersion.EP9 && opportunity.Guid.ToString().Contains("Academics"))
                {
                    Opportunity.OpportunitySharedData.RequirementInfo requirement = new()
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
        }

        private static void OnPostLoad()
        {
            Common.Helpers.InjectInteraction<RabbitHole>(ref GetJobInRabbitHole.Singleton, new GetJobInRabbitHoleEx.Definition(), true);
            Common.Helpers.InjectInteraction<Newspaper>(ref GetNewspaperChooser.Singleton, new GetNewspaperChooserEx.Definition(), true);
            if (IsCareersInstalled)
            {
                Common.Helpers.InjectInteraction<CityHall>(ref CityHall.RegisterAsSelfEmployed.Singleton, new SelfEmployed.RegisterAsSelfEmployedCityHall.Definition(), true);
            }
            Common.Helpers.InjectInteraction<Sim>(ref Sim.ReadSomethingInInventory.Singleton, new ReadSomethingInInventoryEx.Definition(), true);
            Common.Helpers.InjectInteraction<RabbitHole>(ref CollegeOfBusiness.AttendResumeWritingAndInterviewTechniquesClass.Singleton, new AttendResumeAndInterviewClass.Definition(), true);
            BootSettings();
        }

        private static void OnWorldQuit(object sender, EventArgs e)
        {
            sSavedOccupationsForTravel = null;
            InterviewLists.Clear();
            RandomNewspaperSeeds.Clear();
            RandomNewspaperSeed = RandomUtil.GetInt(32767);
            RandomComputerSeed = RandomUtil.GetInt(32767);
        }

        private static void OnWorldLoadFinished(object sender, EventArgs e)
        {
            if (Household.ActiveHousehold is not null)
            {
                InitInjection();
                return;
            }
            EventTracker.AddListener(EventTypeId.kEventSimSelected, OnSimSelected);
        }

        private static ListenerAction OnSimSelected(Event e)
        {
            if (Household.ActiveHousehold is not null)
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
                Common.Helpers.AddInteraction(newspaper, ChangeSettings.Singleton);
                newspaper.RemoveInteractionByType(FindActiveCareerNewspaper.Singleton);
                if (!RandomNewspaperSeeds.ContainsKey(newspaper.ObjectId))
                {
                    RandomNewspaperSeeds.Add(newspaper.ObjectId, RandomNewspaperSeed);
                }
            }
            foreach (Computer computer in GetObjects<Computer>())
            {
                Common.Helpers.AddInteraction(computer, ChangeSettings.Singleton);
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
            foreach (Phone phone in GetObjects<Phone>())
            {
                Common.Helpers.AddInteraction(phone, ChangeSettings.Singleton);
                Common.Helpers.AddInteraction(phone, PostponeInterview.Singleton);
                Common.Helpers.AddInteraction(phone, CancelInterview.Singleton);
            }
            foreach (SchoolRabbitHole school in GetObjects<SchoolRabbitHole>())
            {
                Common.Helpers.AddInteraction(school, CollegeOfBusiness.AttendResumeWritingAndInterviewTechniquesClass.Singleton);
            }
            foreach (CityHall cityHall in GetObjects<CityHall>())
            {
                Common.Helpers.AddInteraction(cityHall, JoinActiveCareerDaycare.Singleton);
            }
            foreach (LifeguardChair chair in GetObjects<LifeguardChair>())
            {
                chair.RemoveInteractionByType(LifeguardChair.JoinLifeGuardCareer.Singleton);
            }
            AlarmManager.Global.AddAlarm(1f, TimeUnit.Seconds, FixupInterviews, "Gamefreak130 wuz here -- Delayed Fixup", AlarmType.NeverPersisted, null);
            AlarmManager.Global.AddAlarm(1f, TimeUnit.Seconds, FixupCareerOpportunities, "Gamefreak130 wuz here -- Delayed Fixup", AlarmType.NeverPersisted, null);
            AlarmManager.Global.AddAlarm(1f, TimeUnit.Seconds, FixupNewspaperSeeds, "Gamefreak130 wuz here -- Delayed Fixup", AlarmType.NeverPersisted, null);
            AlarmManager.Global.AddAlarm(2f, TimeUnit.Seconds, FixupLifeguard, "Gamefreak130 wuz here -- Delayed Fixup", AlarmType.NeverPersisted, null);
            World.OnObjectPlacedInLotEventHandler += OnObjectPlacedInLot;
            EventTracker.AddListener(EventTypeId.kInventoryObjectAdded, OnObjectChanged);
            EventTracker.AddListener(EventTypeId.kObjectStateChanged, OnObjectChanged);
            EventTracker.AddListener(EventTypeId.kSimDied, OnSimDestroyed);
            EventTracker.AddListener(EventTypeId.kSimDescriptionDisposed, OnSimDestroyed);
            EventTracker.AddListener(EventTypeId.kSentSimToBoardingSchool, OnSimInBoardingSchool);
            EventTracker.AddListener(EventTypeId.kSimEnteredVacationWorld, OnTravelComplete);
            EventTracker.AddListener(EventTypeId.kSimReturnedFromVacationWorld, OnTravelComplete);
            EventTracker.AddListener(EventTypeId.kSimCompletedOccupationTask, OnTaskCompleted);
            EventTracker.AddListener(EventTypeId.kCareerPerformanceChanged, OnPerformanceChange);
            EventTracker.AddListener(EventTypeId.kFinishedWork, OnFinishedWork);
            EventTracker.AddListener(EventTypeId.kTravelToFuture, SaveOccupationForTravel);
            EventTracker.AddListener(EventTypeId.kTravelToPresent, SaveOccupationForTravel);
        }

        // This is absolute garbage
        // But I'm afraid to touch it 'cause it works
        private static void BootSettings()
        {
            List<string> currentInterviews = new();
            List<string> currentCareers = new();
            List<string> currentSelfEmployedJobs = new();
            List<string> newCareers = new();
            Type unemployedType = IsCareersInstalled ? Type.GetType("NRaas.Gameplay.Careers.Unemployed, NRaasCareer") : null;
            foreach (Career career in CareerManager.CareerList)
            {
                if (GameUtils.IsInstalled(career.SharedData.ProductVersion) && career is not School && career.GetType() != unemployedType)
                {
                    career.mAvailableInFuture = true;
                    string name = career.SharedData.Name.Substring(34);
                    currentInterviews.Add(name);
                    currentCareers.Add(name);
                    newCareers.Add(name);
                }
            }
            List<string> newActiveCareers = new();
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
            List<string> newSelfEmployedJobs = new();
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
            if (data is not null)
            {
                data.Tables.TryGetValue("Career", out XmlDbTable xmlDbTable);
                if (xmlDbTable is not null)
                {
                    foreach (XmlDbRow row in xmlDbTable.Rows)
                    {
                        string name = row.GetString("CareerName");
                        if (!string.IsNullOrEmpty(name) && !Settings.InterviewSettings.ContainsKey(name) && newCareers.Contains(name))
                        {
                            bool requiresInterview = row.GetBool("RequiresInterview");
                            string toList = row.GetString("PositiveTraits").Replace(" ", string.Empty);
                            List<string> posTraitList = new(toList.Split(new[] { ',' }));
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
                                            if (ResourceUtils.HashString64(str) == id)
                                            {
                                                posTraits.Add((TraitNames)ResourceUtils.HashString64(str));
                                            }
                                        }
                                    }
                                }
                            }
                            toList = row.GetString("NegativeTraits").Replace(" ", string.Empty);
                            List<string> negTraitList = new(toList.Split(new[] { ',' }));
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
                                            if (ResourceUtils.HashString64(str) == id)
                                            {
                                                negTraits.Add((TraitNames)ResourceUtils.HashString64(str));
                                            }
                                        }
                                    }
                                }
                            }
                            toList = row.GetString("RequiredSkills").Replace(" ", string.Empty);
                            List<string> skillList = new(toList.Split(new[] { ',' }));
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
                                            if (ResourceUtils.HashString64(str) == id)
                                            {
                                                skills.Add((SkillNames)ResourceUtils.HashString64(str));
                                            }
                                        }
                                    }
                                }
                            }
                            InterviewSettings interviewData = new(requiresInterview, posTraits, negTraits, skills);
                            Settings.InterviewSettings.Add(name, interviewData);
                        }
                    }
                }
            }
            data = XmlDbData.ReadData("Gamefreak130.JobOverhaul.AvailableOccupationTuning");
            if (data is not null)
            {
                data.Tables.TryGetValue("Occupation", out XmlDbTable xmlDbTable);
                if (xmlDbTable is not null)
                {
                    foreach (XmlDbRow row in xmlDbTable.Rows)
                    {
                        string name = row.GetString("OccupationName");
                        if (!string.IsNullOrEmpty(name))
                        {
                            bool isAvailable = row.GetBool("Enabled");
                            string toList = row.GetString("RequiredDegrees").Replace(" ", string.Empty);
                            List<string> degreeList = new(toList.Split(new[] { ',' }));
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
                                    CareerAvailabilitySettings settings = new(isAvailable, false, degrees);
                                    Settings.CareerAvailabilitySettings.Add(name, settings);
                                }
                                continue;
                            }
                            if (newActiveCareers.Contains(name))
                            {
                                newActiveCareers.Remove(name);
                                CareerAvailabilitySettings settings = new(isAvailable, true, degrees);
                                Settings.CareerAvailabilitySettings.Add(name, settings);
                            }
                        }
                    }
                }
            }
            data = XmlDbData.ReadData("Gamefreak130.JobOverhaul.AvailableSelfEmployedTuning");
            if (data is not null)
            {
                data.Tables.TryGetValue("SelfEmployedCareer", out XmlDbTable xmlDbTable);
                if (xmlDbTable is not null)
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
                        Settings.InterviewSettings.Add(name, new());
                    }
                    if (!Settings.CareerAvailabilitySettings.ContainsKey(name))
                    {
                        Settings.CareerAvailabilitySettings.Add(name, new(false));
                    }
                }
            }
            foreach (ActiveCareer career in CareerManager.GetActiveCareers())
            {
                string name = career.Guid.ToString();
                if (newActiveCareers.Contains(name))
                {
                    newActiveCareers.Remove(name);
                    Settings.CareerAvailabilitySettings.Add(name, new(true));
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
            foreach (ulong key in new List<ulong>(InterviewLists.Keys))
            {
                if (SimDescription.Find(key)?.CreatedSim is not Sim sim)
                {
                    InterviewData.DisposeActorData(key);
                }
                else
                {
                    List<InterviewData> list = InterviewLists[key];
                    for (int i = list.Count - 1; i >= 0; i--)
                    {
                        InterviewData data = list[i];
                        if (data.RabbitHole is RabbitHole rabbitHole && !rabbitHole.HasBeenDestroyed)
                        {
                            rabbitHole.AddInteraction(new DoInterview.Definition(data));
                            data.RabbitHoleDisposedListener = EventTracker.AddListener(EventTypeId.kEventObjectDisposed, data.OnRabbitHoleDisposed, null, rabbitHole);
                        }
                        else
                        {
                            data.Dispose(false);
                        }
                    }
                }
            }
        }

        private static void FixupCareerOpportunities()
        {
            MapTagsModel.Singleton.MapTagRefreshAll += OnMapTagsUpdated;
            foreach (Sim sim in Household.ActiveHousehold.Sims)
            {
                if (sim.OpportunityManager is OpportunityManager manager)
                {
                    List<Opportunity> toRemove = new();
                    foreach (Opportunity opportunity in manager.List)
                    {
                        if ((opportunity.IsCareer || opportunity.IsSkill || opportunity.IsLocationBased || opportunity.IsDare || opportunity.IsSocialGroup || opportunity.IsDayJob) && opportunity.WorldStartedIn != GameUtils.GetCurrentWorld())
                        {
                            foreach (Opportunity.OpportunitySharedData.EventInfo info in opportunity.EventList)
                            {
                                if (info.mEventId is EventTypeId.kSimEnteredVacationWorld || info.mEventId is EventTypeId.kSimReturnedFromVacationWorld)
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

        private static void FixupLifeguard()
        {
            foreach (Sim sim in GetObjects<Sim>())
            {
                if (sim.CareerManager is CareerManager { OccupationAsActiveCareer: Lifeguard lifeguard } manager && !IsActiveCareerAvailable(lifeguard))
                {
                    DropOccupation(manager);
                }
            }
        }

        private static void FixupNewspaperSeeds()
        {
            ObjectGuid[] keys = new ObjectGuid[Settings.InterviewSettings.Count];
            RandomNewspaperSeeds.Keys.CopyTo(keys, 0);
            foreach (ObjectGuid id in keys)
            {
                if (GameObject.GetObject(id) is Newspaper { IsOld: true } or null)
                {
                    RandomNewspaperSeeds.Remove(id);
                }
            }
        }

        public const ulong kReadyForInterviewGuid = 0xCA57D12A3647413D;

        public const ulong kGotTheJobGuid = 0x1770B99317A5D98A;

        public const ulong kBadInterviewGuid = 0x552A7AD84AF2FA7E;

        public static bool IsPoolLifeguardModInstalled { get; private set; }

        public static bool IsOnceReadInstalled { get; private set; }

        public static bool IsCareersInstalled { get; private set; }

        [PersistableStatic]
        public static int RandomNewspaperSeed;

        [PersistableStatic]
        public static Dictionary<ObjectGuid, int> RandomNewspaperSeeds = new();

        [PersistableStatic]
        public static int RandomComputerSeed;

        [PersistableStatic]
        private static Dictionary<ulong, OccupationState> sSavedOccupationsForTravel;

        [PersistableStatic]
        private static PersistedSettings sSettings;

        public static Dictionary<ulong, OccupationState> SavedOccupationsForTravel => sSavedOccupationsForTravel ??= new();

        public static PersistedSettings Settings => sSettings ??= new();

        internal static void ResetSettings()
        {
            sSettings = null;
            BootSettings();
        }
    }
}