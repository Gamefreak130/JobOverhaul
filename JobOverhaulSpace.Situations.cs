using Sims3.Gameplay.ActiveCareer.ActiveCareers;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Careers;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Objects.Miscellaneous;
using Sims3.Gameplay.Situations;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.UI;
using System.Collections.Generic;

namespace Gamefreak130.JobOverhaulSpace.Situations
{
    public class ChildDaycareChildMonitorEx : ChildDaycareChildMonitor
    {
        public ChildDaycareChildMonitorEx()
        {
        }

        public ChildDaycareChildMonitorEx(Sim sim, Daycare daycare, DaycareWorkdaySituation daycareSituation)
        {
            mChildSimDescId = sim.SimDescription.SimDescriptionId;
            mDaycare = daycare;
            mDaycareSituation = daycareSituation;
            mAlarmHandle = sim.AddAlarmRepeating(3f, TimeUnit.Minutes, Update, 3f, TimeUnit.Minutes, "Daycare Child Monitor - " + sim.Name, AlarmType.AlwaysPersisted);
            SetupStartingConditions(sim);
            mMetricRecord = new(sim, GetMotiveDataDictionary());
        }

        public override void SetupStartingConditions(Sim daycareChild)
        {
            Dictionary<CommodityKind, Daycare.DaycareMotiveStaticData> motiveDataDictionary = GetMotiveDataDictionary();
            foreach (Daycare.DaycareMotiveStaticData current in motiveDataDictionary.Values)
            {
                float @float = RandomUtil.GetFloat(current.InitialValueMin, current.InitialValueMax);
                daycareChild.Motives.SetValue(current.CommodityKind, @float);
            }
            float badMotiveChance = GetBadMotiveChance();
            if (RandomUtil.RandomChance(badMotiveChance))
            {
                Daycare.DaycareMotiveStaticData randomObjectFromDictionary = RandomUtil.GetRandomObjectFromDictionary(motiveDataDictionary);
                float float2 = RandomUtil.GetFloat(randomObjectFromDictionary.BadValueMin, randomObjectFromDictionary.BadValueMax);
                daycareChild.Motives.SetValue(randomObjectFromDictionary.CommodityKind, float2);
            }
            AddInteractions();
            if (daycareChild.School is School school)
            {
                school.AddHomeworkToStudent(false);
                if (school.OwnersHomework is Homework homework)
                {
                    homework.PercentComplete = GetHomeworkStartPercent();
                }
            }
        }
    }

    public class VaccinationSessionSituationEx : VaccinationSessionSituation
    {
        new public class RouteEveryoneToLot : ChildSituation<VaccinationSessionSituationEx>
        {
            public const string sLocalizationKey = "/RouteEveryoneToLot";

            public string LocalizeString(string name, params object[] parameters) => Localization.LocalizeString(Parent.GetLocalizeKey() + "/RouteEveryoneToLot:" + name, parameters);

            public string LocalizeString(bool isFemale, string name, params object[] parameters) => Localization.LocalizeString(isFemale, Parent.GetLocalizeKey() + "/RouteEveryoneToLot:" + name, parameters);

            public RouteEveryoneToLot()
            {
            }

            public RouteEveryoneToLot(VaccinationSessionSituationEx parent) : base(parent)
            {
            }

            public override void Init(VaccinationSessionSituationEx parent)
            {
                if (Lot is null)
                {
                    Exit();
                }
                parent.VaccinationSessionBroadcaster = new(parent.Lot, parent.AskForVaccinationBroadcasterParams, OnEnterBroadcaster);
                parent.BringRandomSimsToSession(parent.NumSimsToInitiallyBring);
                if (parent.Vaccinator.LotCurrent != parent.Lot)
                {
                    parent.Vaccinator.InteractionQueue.CancelNthInteraction(1);
                    InteractionInstance interactionInstance = ForceSituationSpecificInteraction(parent.Lot, parent.Vaccinator, VisitCommunityLot.Singleton, null, null, OnVaccinatorRouteFail, new InteractionPriority(InteractionPriorityLevel.UserDirected));
                    (interactionInstance as ITakeSimToWorkLocation)?.SetTakingSimToWork();
                }
                string message = LocalizeString("Tns1GetToLocation", parent.Vaccinator, parent.Lot.Name);
                parent.Vaccinator.ShowTNSIfSelectable(message, StyledNotification.NotificationStyle.kGameMessagePositive, ObjectGuid.InvalidObjectGuid, parent.Vaccinator.ObjectId);
            }

            public bool ValidTest(Sim sim) => sim.IsNPC && !sim.SimDescription.IsGhost && !sim.SimDescription.IsMummy && !sim.SimDescription.IsFrankenstein && !sim.SimDescription.IsEP11Bot && sim != Parent.Vaccinator 
                && !Parent.LivingWithVaccinator(sim) && !sim.IsPerformingAService && !sim.SimDescription.ChildOrBelow && !Parent.IsInIgnoreList(sim) && !Sims3.Gameplay.Objects.Vehicles.CarNpcManager.Singleton.NpcDriversManager.IsNpcDriver(sim) && !Parent.IsAskingAlready(sim);

            public void OnEnterBroadcaster(Sim s, ReactionBroadcaster broadcaster)
            {
                if (Parent.GetNumSimsCurrentlyInSession() < Parent.MaxSimsInSessionAtAnyOneTime && ValidTest(s))
                {
                    Parent.PushAskInteraction(s);
                    return;
                }
                if (!Parent.VaccinatorArrived && s == Parent.Vaccinator)
                {
                    Parent.SetVaccinatorArrived();
                    string message = LocalizeString(Parent.Vaccinator.IsFemale, "Tns2VaccinationSessionStarting", Parent.Vaccinator, Parent.Lot.Name, Parent.VaccinationSessionMaxDurationInHours);
                    Parent.Vaccinator.ShowTNSIfSelectable(message, StyledNotification.NotificationStyle.kGameMessagePositive, ObjectGuid.InvalidObjectGuid, Parent.Vaccinator.ObjectId);
                    string name = string.Format("Vaccination Session 1 hour remaining - Vaccinator: {0}", Parent.Vaccinator);
                    Parent.VaccinationSessionTimer = Parent.Vaccinator.AddAlarm(Parent.VaccinationSessionMaxDurationInHours - 1, TimeUnit.Hours, Parent.OnVaccinationSessionOneHourRemaining, name, AlarmType.DeleteOnReset);
                }
            }

            public void OnVaccinatorRouteFail(Sim actor, float x)
            {
                if (!Parent.VaccinatorArrived)
                {
                    Exit();
                }
            }
        }

        public VaccinationSessionSituationEx()
        {
        }

        public VaccinationSessionSituationEx(Sim vaccinator, Lot lot)
        {
            Vaccinator = vaccinator;
            Vaccinator.AssignRole(this);
            mLeaveConversationListener = EventTracker.AddListener(EventTypeId.kLeftConversation, OnConversationLeft, Vaccinator);
            mLot = lot;
            mLotId = lot?.LotId ?? 0uL;
            SetState(new RouteEveryoneToLot(this));
            sAllSituations.Add(this);
        }

        new public void OnVaccinateeRouteFail(Sim actor, float x)
        {
            if (!IsInSeekersList(actor))
            {
                BringRandomSimsToSession(1);
            }
        }

        new public static VaccinationSessionSituation Create(Sim vaccinator, Lot lot) => new VaccinationSessionSituationEx(vaccinator, lot);

        new public static VaccinationSessionSituationEx GetVaccinationSessionSituation(Sim sim)
        {
            List<Situation> situations = sim.Autonomy.SituationComponent.Situations;
            foreach (Situation current in situations)
            {
                if (current is VaccinationSessionSituationEx)
                {
                    return current as VaccinationSessionSituationEx;
                }
            }
            return null;
        }

        new public bool BringRandomSimTest(Sim sim, Lot lot) => sim.IsNPC && !sim.SimDescription.IsGhost && !sim.SimDescription.IsMummy && !sim.SimDescription.IsFrankenstein && !sim.SimDescription.IsEP11Bot && sim.LotCurrent != lot 
            && !LivingWithVaccinator(sim) && (CameraController.IsMapViewModeEnabled() || !World.IsObjectOnScreen(sim.ObjectId)) && !IsInSeekersList(sim) && !IsInInterruptedList(sim) && !IsInIgnoreList(sim) && !sim.IsAtWork;

        new public bool BringRandomSickSimTest(Sim sim, Lot lot) => sim.BuffManager.HasElement(BuffNames.Germy) && BringRandomSimTest(sim, lot);

        new public void BringRandomSimsToSession(int numSimsRequested)
        {
            int numSimsCurrentlyInSession = GetNumSimsCurrentlyInSession();
            if (numSimsCurrentlyInSession >= MaxSimsInSessionAtAnyOneTime)
            {
                return;
            }
            int num = numSimsRequested;
            if (numSimsRequested + numSimsCurrentlyInSession > MaxSimsInSessionAtAnyOneTime)
            {
                num = MaxSimsInSessionAtAnyOneTime - numSimsCurrentlyInSession;
            }
            int num2 = 0;
            if (GameUtils.IsInstalled(ProductVersion.EP8))
            {
                num2 = Lot.RouteRandomNPCSimsHere(num, BringRandomSickSimTest, OnVaccinateeRouteFail);
            }
            if (num2 != num)
            {
                num2 = Lot.RouteRandomNPCSimsHere(num, BringRandomSimTest, OnVaccinateeRouteFail);
            }
        }

        public static void BeforeVaccinate(Sim actor, Sim target, string _, ActiveTopic __, InteractionInstance ___)
        {
            if (GetVaccinationSessionSituation(actor) is VaccinationSessionSituationEx vaccinationSessionSituation)
            {
                vaccinationSessionSituation.NumVaccinations++;
                vaccinationSessionSituation.AddToIgnoreList(target);
                vaccinationSessionSituation.BringRandomSimsToSession(1);
                target.SimDescription.HealthManager?.Vaccinate();
            }
        }
    }

    public class FreeClinicSessionSituationEx : VaccinationSessionSituationEx
    {
        public class AskForDiagnosisEx : Sim.AskForDiagnosis
        {
            new public class Definition : InteractionDefinition<Sim, Sim, AskForDiagnosisEx>
            {
                public override bool Test(Sim actor, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback) => GetFreeClinicSessionSituation(target) is not null;
            }

            new public static InteractionDefinition Singleton = new Definition();
        }

        public new const string sLocalizationKey = "Gameplay/Situations/FreeClinicSessionSituation";

        public override int[] SuccessRatingThreshold => FreeClinicSessionSituation.kFreeClinicSuccessRatingThreshold;

        public override int NumSimsToInitiallyBring => FreeClinicSessionSituation.kFreeClinicNumSimsToInitiallyBring;

        public override int MaxSimsInSessionAtAnyOneTime => FreeClinicSessionSituation.kFreeClinicMaxSimsInSessionAtAnyOneTime;

        public override float MaxDistanceForAutonomousSign => FreeClinicSessionSituation.kFreeClinicMaxDistanceForAutonomousSign;

        public override int VaccinationSessionMaxDurationInHours => FreeClinicSessionSituation.kFreeClinicSessionMaxDurationInHours;

        public override float[] PerformanceChanges => FreeClinicSessionSituation.kFreeClinicPerformanceChanges;

        public override float HandledSimBonus => FreeClinicSessionSituation.kFreeClinicHandledSimBonus;

        public override float IgnoredSimPenalty => FreeClinicSessionSituation.kFreeClinicIgnoredSimPenalty;

        public override TaskId Task => TaskId.FreeHealthClinic;

        public override string LocalizeString(string name, params object[] parameters) => Localization.LocalizeString("Gameplay/Situations/FreeClinicSessionSituation:" + name, parameters);

        public override string LocalizeString(bool isFemale, string name, params object[] parameters) => Localization.LocalizeString(isFemale, "Gameplay/Situations/FreeClinicSessionSituation:" + name, parameters);

        public override string GetLocalizeKey() => "Gameplay/Situations/FreeClinicSessionSituation";

        public static FreeClinicSessionSituationEx GetFreeClinicSessionSituation(Sim sim)
        {
            List<Situation> situations = sim.Autonomy.SituationComponent.Situations;
            foreach (Situation current in situations)
            {
                if (current is FreeClinicSessionSituationEx)
                {
                    return current as FreeClinicSessionSituationEx;
                }
            }
            return null;
        }

        public override void AddInteraction(Sim sim)
        {
            SituationSocial.Definition i = new("Diagnose", new string[0], null, false);
            AddSituationSpecificInteraction(sim, Vaccinator, i, null, null, null, "Sims3.Gameplay.Autonomy.Diagnose+Definition");
        }

        public override void PushAskInteraction(Sim s)
        {
            Sim.AskForDiagnosis askForDiagnosis = RetrieveAskForVaccinationState(s, out Sim.AskForVaccinationState askState, out float timeRemainingInSpecifiedAskStateOverride)
                ? PushAskForDiagnosis(s, Vaccinator, new(InteractionPriorityLevel.NonCriticalNPCBehavior, 1f), askState, timeRemainingInSpecifiedAskStateOverride)
                : PushAskForDiagnosis(s, Vaccinator, new(InteractionPriorityLevel.NonCriticalNPCBehavior, 1f), Sim.AskForVaccinationState.WaitingForVaccinatorToArrive);
            askForDiagnosis.VaccinationSessionSituation = this;
            AddVaccinationSeeker(s);
        }

        public override bool IsAskingAlready(Sim sim)
        {
            foreach (InteractionInstance current in sim.InteractionQueue.InteractionList)
            {
                if (current is Sim.AskForDiagnosis)
                {
                    return true;
                }
                if (current is SocialInteractionB socialInteractionB)
                {
                    if ((socialInteractionB.InteractionDefinition as SocialInteractionB.Definition)?.Key is "Diagnose")
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override void SendEventSuccessfullyHeld() => EventTracker.SendEvent(EventTypeId.kHeldSuccessfulFreeClinic, Vaccinator);

        public FreeClinicSessionSituationEx()
        {
        }

        public FreeClinicSessionSituationEx(Sim diagnoser, Lot lot) : base(diagnoser, lot)
        {
        }

        new public static FreeClinicSessionSituationEx Create(Sim diagnoser, Lot lot) => new(diagnoser, lot);

        public static AskForDiagnosisEx PushAskForDiagnosis(Sim sim, Sim diagnoser, InteractionPriority priority, Sim.AskForVaccinationState askState)
        {
            AskForDiagnosisEx askForDiagnosis = PushAskForDiagnosis(sim, diagnoser, priority);
            askForDiagnosis?.SetAskState(askState);
            return askForDiagnosis;
        }

        public static AskForDiagnosisEx PushAskForDiagnosis(Sim sim, Sim diagnoser, InteractionPriority priority, Sim.AskForVaccinationState askState, float timeRemainingInSpecifiedAskStateOverride)
        {
            AskForDiagnosisEx askForDiagnosis = PushAskForDiagnosis(sim, diagnoser, priority);
            askForDiagnosis?.SetAskState(askState, timeRemainingInSpecifiedAskStateOverride);
            return askForDiagnosis;
        }

        public static AskForDiagnosisEx PushAskForDiagnosis(Sim sim, Sim diagnoser, InteractionPriority priority)
        {
            InteractionDefinition singleton = AskForDiagnosisEx.Singleton;
            AskForDiagnosisEx askForDiagnosis = singleton.CreateInstance(diagnoser, sim, priority, true, true) as AskForDiagnosisEx;
            sim.InteractionQueue.Add(askForDiagnosis);
            return askForDiagnosis;
        }

        public static bool TestDiagnose(Sim actor, Sim target, ActiveTopic _, bool isAutonomous, ref GreyedOutTooltipCallback __)
        {
            FreeClinicSessionSituationEx freeClinicSessionSituation = GetFreeClinicSessionSituation(actor);
            return freeClinicSessionSituation is not null && !freeClinicSessionSituation.IsInIgnoreList(target) && (freeClinicSessionSituation.IsInSeekersList(target) || freeClinicSessionSituation.IsInInterruptedList(target)) 
                && (!isAutonomous || actor.GetDistanceToObject(target) <= AutographSessionSituation.MaxDistanceForAutonomousSign);
        }

        public static void BeforeDiagnose(Sim actor, Sim target, string _, ActiveTopic __, InteractionInstance ___)
        {
            if (GetFreeClinicSessionSituation(actor) is FreeClinicSessionSituationEx freeClinicSessionSituation)
            {
                freeClinicSessionSituation.NumVaccinations++;
                freeClinicSessionSituation.AddToIgnoreList(target);
                freeClinicSessionSituation.BringRandomSimsToSession(1);
                target.SimDescription.HealthManager?.Vaccinate();
            }
        }
    }
}