using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Careers;
using Sims3.Gameplay.ChildAndTeenUpdates;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Objects.Electronics;
using Sims3.Gameplay.Seasons;
using Sims3.Gameplay.Skills;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using Sims3.UI;
using System;
using System.Collections.Generic;
using static Gamefreak130.JobOverhaul;
using static Gamefreak130.JobOverhaulSpace.Helpers.Methods;
using static Sims3.Gameplay.Queries;
using static Sims3.UI.StyledNotification;

namespace Gamefreak130.JobOverhaulSpace.Interactions
{
    public class Interviews
    {
        [Persistable]
        public class InterviewData
        {
            public ulong ActorId { get; }

            public DateAndTime InterviewDate { get; set; }

            public int TimesPostponed { get; set; }

            public AlarmHandle RemindAlarm { get; set; } = AlarmHandle.kInvalidHandle;

            public AlarmHandle TimeoutAlarm { get; set; } = AlarmHandle.kInvalidHandle;

            public EventListener RabbitHoleDisposedListener { get; set; }

            public RabbitHole RabbitHole { get; }

            public OccupationNames CareerName { get; }

            public InterviewData()
            {
            }

            public InterviewData(CareerLocation careerLocation, Sim actor)
            {
                RabbitHole = careerLocation.Owner;
                ActorId = actor.SimDescription.SimDescriptionId;
                CareerName = careerLocation.Career.Guid;
                TimesPostponed = 0;
                float interviewDelta = careerLocation.Career.SharedData.Category == Career.CareerCategory.FullTime ? SimClock.HoursUntil(Settings.FullTimeInterviewHour) + 24 : SimClock.HoursUntil(Settings.PartTimeInterviewHour) + 24;
                HolidayManager manager = HolidayManager.Instance;
                while (SimClock.Add(SimClock.CurrentTime(), TimeUnit.Hours, interviewDelta).DayOfWeek == DaysOfTheWeek.Saturday || SimClock.Add(SimClock.CurrentTime(), TimeUnit.Hours, interviewDelta).DayOfWeek == DaysOfTheWeek.Sunday
                    || (manager != null && SimClock.IsSameDay(SimClock.Add(SimClock.CurrentTime(), TimeUnit.Hours, interviewDelta), manager.mStartDateTimeOfHoliday)))
                {
                    interviewDelta += 24;
                }
                InterviewDate = SimClock.Add(SimClock.CurrentTime(), TimeUnit.Hours, interviewDelta);
                string text = LocalizeString(actor.IsFemale, "InterviewOffer", new object[]
                    {
                        actor,
                        careerLocation.Owner.GetLocalizedName(),
                        careerLocation.Career.GetLocalizedCareerName(actor.IsFemale),
                        GetTextDayOfWeek(InterviewDate),
                        SimClockUtils.GetText(Convert.ToInt32(InterviewDate.Hour), 0)
                    });
                Show(new Format(text, NotificationStyle.kGameMessagePositive));
                RemindAlarm = AlarmManager.Global.AddAlarm(SimClock.HoursUntil(InterviewDate) - 1, TimeUnit.Hours, new AlarmTimerCallback(OnReminderCallback), "Gamefreak130 wuz here -- Interview Reminder", AlarmType.AlwaysPersisted, actor);
                TimeoutAlarm = AlarmManager.Global.AddAlarm(SimClock.HoursUntil(InterviewDate) + 1, TimeUnit.Hours, new AlarmTimerCallback(OnInterviewTimeout), "Gamefreak130 wuz here -- Interview Timeout", AlarmType.AlwaysPersisted, actor);
                careerLocation.Owner.AddInteraction(new DoInterview.Definition(this));
                RabbitHoleDisposedListener = EventTracker.AddListener(EventTypeId.kEventObjectDisposed, new ProcessEventDelegate(OnRabbitHoleDisposed), null, RabbitHole);
                PhoneCell phone = actor.Inventory.Find<PhoneCell>();
                if (phone != null)
                {
                    phone.AddInventoryInteraction(new PostponeInterview.Definition(this));
                    phone.AddInventoryInteraction(new CancelInterview.Definition(this));
                }
                foreach (PhoneHome phoneHome in GetObjects<PhoneHome>())
                {
                    AddPhoneInteractions(phoneHome, this);
                }
                InterviewList.Add(this);
                Audio.StartSound("sting_opp_success");
            }

            protected internal void OnReminderCallback()
            {
                DoInterview.Definition definition = new DoInterview.Definition(this);
                if (ScavengerManager.GetSimFromDescriptionId(ActorId) is Sim sim)
                {
                    sim.InteractionQueue.Add(definition.CreateInstance(RabbitHole, sim, new InteractionPriority(InteractionPriorityLevel.UserDirected), true, true) as DoInterview);
                    sim.ShowTNSIfSelectable(LocalizeString(sim.IsFemale, "InterviewReminder", new object[]
                        {
                            sim,
                            RabbitHole.GetLocalizedName()
                        }), NotificationStyle.kGameMessagePositive, ObjectGuid.InvalidObjectGuid, ObjectGuid.InvalidObjectGuid);
                }
            }

            protected internal void OnInterviewTimeout() => Dispose(true);

            protected internal ListenerAction OnRabbitHoleDisposed(Event e)
            {
                Dispose(false);
                return ListenerAction.Remove;
            }

            protected internal void Dispose(bool isTimeout)
            {
                InteractionObjectPair iop = null;
                if (ScavengerManager.GetSimFromDescriptionId(ActorId) is Sim sim)
                {
                    PhoneCell phone = sim.Inventory.Find<PhoneCell>();
                    if (phone != null)
                    {
                        RemovePhoneInteractions(sim, phone, isTimeout);
                    }
                    foreach (PhoneHome phoneHome in GetObjects<PhoneHome>())
                    {
                        RemovePhoneInteractions(sim, phoneHome, isTimeout);
                    }
                    if (RabbitHole != null)
                    {
                        foreach (InteractionObjectPair current in RabbitHole.Interactions)
                        {
                            if (current.InteractionDefinition is DoInterview.Definition definition && definition.mData.ActorId == ActorId)
                            {
                                iop = current;
                                break;
                            }
                        }
                    }
                    if (iop != null)
                    {
                        InteractionInstance firstInstance = sim.InteractionQueue.GetCurrentInteraction();
                        if (firstInstance != null && firstInstance.InteractionDefinition.GetType() == iop.InteractionDefinition.GetType() && isTimeout)
                        {
                            firstInstance.CancellableByPlayer = false;
                        }
                        else if (isTimeout)
                        {
                            if (sim.IsSelectable)
                            {
                                Audio.StartSound("sting_opp_fail");
                                sim.ShowTNSIfSelectable(LocalizeString(sim.IsFemale, "InterviewTimeout", new object[]
                                {
                                    sim,
                                    CareerManager.GetStaticOccupation(CareerName).GetLocalizedCareerName(sim.IsFemale)
                                }), NotificationStyle.kGameMessageNegative, ObjectGuid.InvalidObjectGuid, sim.ObjectId);
                            }
                        }
                        for (int i = sim.InteractionQueue.Count - 1; i >= 1; i--)
                        {
                            InteractionInstance current = sim.InteractionQueue.mInteractionList[i];
                            if (current.InteractionDefinition.GetType() == iop.InteractionDefinition.GetType())
                            {
                                sim.InteractionQueue.RemoveInteractionByRef(current);
                            }
                        }
                        RabbitHole.RemoveInteraction(iop);
                    }
                }
                InterviewList.Remove(this);
                AlarmManager.Global.RemoveAlarm(RemindAlarm);
                AlarmManager.Global.RemoveAlarm(TimeoutAlarm);
                EventTracker.RemoveListener(RabbitHoleDisposedListener);
                RabbitHoleDisposedListener = null;
            }
        }

        [PersistableStatic]
        public static List<InterviewData> InterviewList = new List<InterviewData>();

        public class DoInterview : RabbitHole.RabbitHoleInteraction<Sim, RabbitHole>
        {
            public class Definition : InteractionDefinition<Sim, RabbitHole, DoInterview>
            {
                internal InterviewData mData;

                public Definition()
                {
                }

                public Definition(InterviewData data) => mData = data;

                public override string GetInteractionName(Sim actor, RabbitHole target, InteractionObjectPair iop) => LocalizeString("DoInterview");

                public override InteractionInstance CreateInstance(ref InteractionInstanceParameters parameters)
                {
                    DoInterview doInterview = base.CreateInstance(ref parameters) as DoInterview;
                    doInterview.mData = mData;
                    return doInterview;
                }

                public override bool Test(Sim actor, RabbitHole target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    if (!(mData.ActorId == actor.SimDescription.SimDescriptionId))
                    {
                        return false;
                    }
                    if (SimClock.CurrentTime() < SimClock.Subtract(mData.InterviewDate, TimeUnit.Hours, 1))
                    {
                        greyedOutTooltipCallback = CreateTooltipCallback(LocalizeString("NotYetTimeForInterview", new object[]
                        {
                            actor,
                            GetTextDayOfWeek(mData.InterviewDate),
                            SimClockUtils.GetText(Convert.ToInt32(mData.InterviewDate.Hour), 0)
                        }));
                        return false;
                    }
                    return true;
                }
            }

            private InterviewData mData;

            private AlarmHandle mAlarm1 = AlarmHandle.kInvalidHandle;

            private AlarmHandle mAlarm2 = AlarmHandle.kInvalidHandle;

            private bool mSuccess;

            public override bool Run()
            {
                Actor.SwitchToOutfitWithSpin(Sim.ClothesChangeReason.GoingToRabbitHole, OutfitCategories.Formalwear);
                return base.Run();
            }

            public override bool InRabbitHole()
            {
                CancellableByPlayer = false;
                mData.Dispose(false);
                if (Actor.IsSelectable && CareerManager.GetStaticCareer(mData.CareerName) is Career career && Settings.InterviewSettings.TryGetValue(career.SharedData.Name.Substring(34), out InterviewSettings settings))
                {
                    float chance = career.Category == Career.CareerCategory.FullTime ? Settings.BaseFullTimeJobChance : Settings.BasePartTimeJobChance;
                    chance -= Settings.PostponeInterviewChanceChange * mData.TimesPostponed;
                    foreach (TraitNames trait in settings.PositiveTraits)
                    {
                        if (Actor.TraitManager.HasElement(trait))
                        {
                            chance += Settings.PositiveTraitInterviewChanceChange;
                        }
                    }
                    foreach (TraitNames trait in settings.NegativeTraits)
                    {
                        if (Actor.TraitManager.HasElement(trait))
                        {
                            chance -= Settings.NegativeTraitInterviewChanceChange;
                        }
                    }
                    foreach (SkillNames skill in settings.RequiredSkills)
                    {
                        if (Actor.SkillManager.HasElement(skill))
                        {
                            chance += Settings.RequiredSkillInterviewChanceChange * Actor.SkillManager.GetElement(skill).SkillLevel;
                        }
                    }
                    if (Actor.BuffManager.HasElement(kReadyForInterviewGuid))
                    {
                        chance += Settings.ReadyForInterviewChanceChange;
                    }
                    chance *= MathHelpers.LinearInterpolate(-100f, 100f, 0.65f, 1.15f, Actor.MoodManager.MoodValue);
                    MathUtils.Clamp(chance, 0, 100);
                    mSuccess = RandomUtil.RandomChance(chance);
                    float time = Settings.InterviewTime / 3;
                    mAlarm1 = AlarmManager.Global.AddAlarm(time, TimeUnit.Minutes, new AlarmTimerCallback(ShowFirstMessage), "Gamefreak130 wuz here -- Interview Message Callback", AlarmType.AlwaysPersisted, Actor);
                    mAlarm2 = AlarmManager.Global.AddAlarm(time * 2, TimeUnit.Minutes, new AlarmTimerCallback(ShowSecondMessage), "Gamefreak130 wuz here -- Interview Message Callback", AlarmType.AlwaysPersisted, Actor);
                    bool flag = DoTimedLoop(Settings.InterviewTime);
                    if (!flag)
                    {
                        return false;
                    }
                    if (mSuccess)
                    {
                        if (mData.RabbitHole.CareerLocations.TryGetValue((ulong)mData.CareerName, out CareerLocation location))
                        {
                            bool showUi = Actor.SimDescription.CareerManager.IsLocationTransfer(new AcquireOccupationParameters(location, false, false));
                            AcquireOccupationParameters occupationParameters = new AcquireOccupationParameters(location, showUi, true);
                            if (BoardingSchool.DidSimGraduate(Actor.SimDescription, BoardingSchool.BoardingSchoolTypes.None, false))
                            {
                                BoardingSchool.UpdateAcquireOccupationParameters(Actor.SimDescription.BoardingSchool, (ulong)mData.CareerName, ref occupationParameters);
                            }
                            if (Actor.AcquireOccupation(occupationParameters))
                            {
                                if (!showUi)
                                {
                                    Occupation occupation = Actor.CareerManager.Occupation;
                                    string newOccupationTnsText = occupation.GetNewOccupationTnsText();
                                    if (!string.IsNullOrEmpty(newOccupationTnsText))
                                    {
                                        occupation.ShowOccupationTNS(newOccupationTnsText);
                                    }
                                    Audio.StartSound("sting_career_positive");
                                }
                                Actor.BuffManager.AddElement(0x1770B99317A5D98A, (Origin)ResourceUtils.HashString64("FromGoodInterview"));
                            }
                            return true;
                        }
                    }
                    else
                    {
                        Actor.ShowTNSIfSelectable(LocalizeString(Actor.IsFemale, "InterviewFailure", new object[]
                        {
                            Actor
                        }), NotificationStyle.kGameMessageNegative, Target.RabbitHoleProxy.ObjectId, ObjectGuid.InvalidObjectGuid);
                        Audio.StartSound("sting_opp_fail");
                        Actor.BuffManager.AddElement(0x552A7AD84AF2FA7E, (Origin)ResourceUtils.HashString64("FromBadInterview"));
                        return true;
                    }
                }
                return false;
            }

            public void ShowFirstMessage() => Actor.ShowTNSIfSelectable(LocalizeString(Actor.IsFemale, mSuccess ? "FirstInterviewTNSGood" : "FirstInterviewTNSBad"), NotificationStyle.kSimTalking);

            public void ShowSecondMessage() => Actor.ShowTNSIfSelectable(LocalizeString(Actor.IsFemale, mSuccess ? "SecondInterviewTNSGood" : "SecondInterviewTNSBad"), NotificationStyle.kSimTalking);

            public override void Cleanup()
            {
                base.Cleanup();
                AlarmManager.Global.RemoveAlarm(mAlarm1);
                AlarmManager.Global.RemoveAlarm(mAlarm2);
            }
        }

        public class PostponeInterview : Phone.Call
        {
            public class Definition : CallDefinition<PostponeInterview>
            {
                internal InterviewData mData;

                public Definition()
                {
                }

                public Definition(InterviewData data) => mData = data;

                public override string[] GetPath(bool isFemale) => new string[] { Phone.LocalizeString("JobsAndOffers") + Localization.Ellipsis };

                public override string GetInteractionName(Sim actor, Phone target, InteractionObjectPair iop) => LocalizeString("PostponeInterview");

                public override bool Test(Sim a, Phone target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    if (SimClock.CurrentTime() > SimClock.Subtract(mData.InterviewDate, TimeUnit.Hours, 1))
                    {
                        greyedOutTooltipCallback = CreateTooltipCallback(LocalizeString(a.IsFemale, "PostponePointOfNoReturn", new object[] { a }));
                        return false;
                    }
                    if (mData.TimesPostponed >= Settings.MaxInterviewPostpones)
                    {
                        greyedOutTooltipCallback = CreateTooltipCallback(LocalizeString(a.IsFemale, "OutOfPostpones", new object[] { a }));
                        return false;
                    }
                    return base.Test(a, target, isAutonomous, ref greyedOutTooltipCallback) && mData.ActorId == a.SimDescription.SimDescriptionId;
                }
            }

            public override ConversationBehavior OnCallConnected() => ConversationBehavior.TalkBriefly;

            public override void OnCallFinished()
            {
                InterviewData data = (InteractionDefinition as Definition).mData;
                HolidayManager manager = HolidayManager.Instance;
                data.InterviewDate = SimClock.Add(data.InterviewDate, TimeUnit.Days, 1);
                while (data.InterviewDate.DayOfWeek == DaysOfTheWeek.Saturday || data.InterviewDate.DayOfWeek == DaysOfTheWeek.Sunday || (manager != null && SimClock.IsSameDay(data.InterviewDate, manager.mStartDateTimeOfHoliday)))
                {
                    data.InterviewDate = SimClock.Add(data.InterviewDate, TimeUnit.Days, 1);
                }
                AlarmManager.Global.RemoveAlarm(data.RemindAlarm);
                data.RemindAlarm = AlarmManager.Global.AddAlarm(SimClock.HoursUntil(data.InterviewDate) - 1, TimeUnit.Hours, new AlarmTimerCallback(data.OnReminderCallback), "Gamefreak130 wuz here -- Interview Reminder", AlarmType.AlwaysPersisted, Actor);
                AlarmManager.Global.RemoveAlarm(data.TimeoutAlarm);
                data.TimeoutAlarm = AlarmManager.Global.AddAlarm(SimClock.HoursUntil(data.InterviewDate) + 1, TimeUnit.Hours, new AlarmTimerCallback(data.OnInterviewTimeout), "Gamefreak130 wuz here -- Interview Timeout", AlarmType.AlwaysPersisted, Actor);
                data.TimesPostponed += 1;
                Actor.ShowTNSIfSelectable(LocalizeString(Actor.IsFemale, data.TimesPostponed >= Settings.MaxInterviewPostpones ? "InterviewPostponedFinal" : "InterviewPostponed", new object[]
                    {
                        Actor,
                        data.RabbitHole.GetLocalizedName(),
                        GetTextDayOfWeek(data.InterviewDate),
                        SimClockUtils.GetText(Convert.ToInt32(data.InterviewDate.Hour), 0)
                    }), data.TimesPostponed >= Settings.MaxInterviewPostpones ? NotificationStyle.kGameMessageNegative : NotificationStyle.kSystemMessage);
            }
        }

        public class CancelInterview : Phone.Call
        {
            public class Definition : CallDefinition<CancelInterview>
            {
                internal InterviewData mData;

                public Definition()
                {
                }

                public Definition(InterviewData data) => mData = data;

                public override string[] GetPath(bool isFemale) => new string[] { Phone.LocalizeString("JobsAndOffers") + Localization.Ellipsis };

                public override string GetInteractionName(Sim actor, Phone target, InteractionObjectPair iop) => LocalizeString("CancelInterview");

                public override bool Test(Sim a, Phone target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback) => base.Test(a, target, isAutonomous, ref greyedOutTooltipCallback) && mData.ActorId == a.SimDescription.SimDescriptionId;
            }

            public override ConversationBehavior OnCallConnected() => ConversationBehavior.TalkBriefly;

            public override void OnCallFinished()
            {
                if (TwoButtonDialog.Show(LocalizeString(Actor.IsFemale, "CancelInterviewDialog", new object[] { Actor }), LocalizationHelper.Yes, LocalizationHelper.No))
                {
                    InterviewData data = (InteractionDefinition as Definition).mData;
                    data.Dispose(false);
                    Actor.ShowTNSIfSelectable(LocalizeString(Actor.IsFemale, "InterviewCanceled", new object[]
                    {
                        Actor,
                        CareerManager.GetStaticOccupation(data.CareerName).GetLocalizedCareerName(Actor.IsFemale)
                    }), NotificationStyle.kSystemMessage);
                }
            }
        }
    }
}
