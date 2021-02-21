using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Careers;
using Sims3.Gameplay.CAS;
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
using static Sims3.UI.ObjectPicker;

namespace Gamefreak130.JobOverhaulSpace.Interactions
{
    public static class Interviews
    {
        [Persistable]
        public class InterviewData
        {
            public ulong ActorId { get; }

            public DateAndTime InterviewDate { get; set; }

            public int TimesPostponed { get; set; }

            public AlarmHandle RemindAlarm { get; set; }

            public AlarmHandle TimeoutAlarm { get; set; }

            public EventListener RabbitHoleDisposedListener { get; set; }

            public RabbitHole RabbitHole { get; }

            public OccupationNames CareerName { get; }

            private InterviewData()
            {
            }

            public InterviewData(CareerLocation careerLocation, Sim actor)
            {
                RabbitHole = careerLocation.Owner;
                ActorId = actor.SimDescription.SimDescriptionId;
                CareerName = careerLocation.Career.Guid;
                TimesPostponed = 0;
                float interviewDelta = careerLocation.Career.SharedData.Category is Career.CareerCategory.FullTime ? SimClock.HoursUntil(Settings.FullTimeInterviewHour) + 24 : SimClock.HoursUntil(Settings.PartTimeInterviewHour) + 24;
                HolidayManager manager = HolidayManager.Instance;
                while (SimClock.Add(SimClock.CurrentTime(), TimeUnit.Hours, interviewDelta).DayOfWeek is DaysOfTheWeek.Saturday or DaysOfTheWeek.Sunday
                    || (manager is not null && SimClock.IsSameDay(SimClock.Add(SimClock.CurrentTime(), TimeUnit.Hours, interviewDelta), manager.mStartDateTimeOfHoliday)))
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
                        SimClockUtils.GetText(Convert.ToInt32(InterviewDate.Hour))
                    });
                StyledNotification.Show(new(text, StyledNotification.NotificationStyle.kGameMessagePositive));
                RemindAlarm = AlarmManager.Global.AddAlarm(SimClock.HoursUntil(InterviewDate) - 1, TimeUnit.Hours, OnReminderCallback, "Gamefreak130 wuz here -- Interview Reminder", AlarmType.AlwaysPersisted, actor);
                TimeoutAlarm = AlarmManager.Global.AddAlarm(SimClock.HoursUntil(InterviewDate) + 0.5f, TimeUnit.Hours, OnInterviewTimeout, "Gamefreak130 wuz here -- Interview Timeout", AlarmType.AlwaysPersisted, actor);
                careerLocation.Owner.AddInteraction(new DoInterview.Definition(this));
                RabbitHoleDisposedListener = EventTracker.AddListener(EventTypeId.kEventObjectDisposed, OnRabbitHoleDisposed, null, RabbitHole);
                if (!InterviewLists.ContainsKey(ActorId))
                {
                    InterviewLists[ActorId] = new();
                }
                InterviewLists[ActorId].Add(this);
                Audio.StartSound("sting_opp_success");
            }

            protected internal void OnReminderCallback()
            {
                DoInterview.Definition definition = new(this);
                if (SimDescription.Find(ActorId)?.CreatedSim is Sim sim)
                {
                    sim.InteractionQueue.Add(definition.CreateInstance(RabbitHole, sim, new(InteractionPriorityLevel.UserDirected), true, true) as DoInterview);
                    sim.ShowTNSIfSelectable(LocalizeString(sim.IsFemale, "InterviewReminder", sim, RabbitHole.GetLocalizedName()), StyledNotification.NotificationStyle.kGameMessagePositive, ObjectGuid.InvalidObjectGuid, ObjectGuid.InvalidObjectGuid);
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
                if (SimDescription.Find(ActorId)?.CreatedSim is Sim sim)
                {
                    foreach (Phone phone in GetObjects<Phone>())
                    {
                        CancelPhoneInteractions(phone, isTimeout);
                    } 
                    if (RabbitHole is not null)
                    {
                        foreach (InteractionObjectPair current in RabbitHole.Interactions)
                        {
                            if ((current.InteractionDefinition as DoInterview.Definition)?.mData.ActorId == ActorId)
                            {
                                iop = current;
                                break;
                            }
                        }
                    }
                    if (iop is not null)
                    {
                        InteractionInstance firstInstance = sim.InteractionQueue.GetCurrentInteraction();
                        if (isTimeout)
                        {
                            if (firstInstance?.InteractionDefinition is DoInterview.Definition definition && definition.mData == this)
                            {
                                firstInstance.CancellableByPlayer = false;
                            }
                            else if (sim.IsSelectable)
                            {
                                Audio.StartSound("sting_opp_fail");
                                sim.ShowTNSIfSelectable(LocalizeString(sim.IsFemale, "InterviewTimeout", new object[]
                                {
                                    sim,
                                    CareerManager.GetStaticOccupation(CareerName).GetLocalizedCareerName(sim.IsFemale)
                                }), StyledNotification.NotificationStyle.kGameMessageNegative, ObjectGuid.InvalidObjectGuid, sim.ObjectId);
                            }
                        }
                        for (int i = sim.InteractionQueue.Count - 1; i >= 1; i--)
                        {
                            InteractionInstance current = sim.InteractionQueue.mInteractionList[i];
                            if (current.InteractionDefinition is DoInterview.Definition definition && definition.mData == this)
                            {
                                sim.InteractionQueue.RemoveInteractionByRef(current);
                            }
                        }
                        RabbitHole.RemoveInteraction(iop);
                    }
                }
                if (InterviewLists.TryGetValue(ActorId, out List<InterviewData> actorList))
                {
                    actorList.Remove(this);
                    if (actorList.Count == 0)
                    {
                        InterviewLists.Remove(ActorId);
                    }
                }
                AlarmManager.Global.RemoveAlarm(RemindAlarm);
                AlarmManager.Global.RemoveAlarm(TimeoutAlarm);
                EventTracker.RemoveListener(RabbitHoleDisposedListener);
                RabbitHoleDisposedListener = null;
            }

            private void CancelPhoneInteractions(Phone phone, bool stopCurrentInteraction)
            {
                if (SimDescription.Find(ActorId)?.CreatedSim is Sim actor)
                {
                    InteractionObjectPair postponeIop = null;
                    InteractionObjectPair cancelIop = null;
                    List<InteractionObjectPair> pairs = phone is PhoneCell ? phone.ItemComp.InteractionsInventory : phone.Interactions;
                    foreach (InteractionObjectPair current in pairs)
                    {
                        if (current.InteractionDefinition as PostponeInterview.Definition is not null)
                        {
                            postponeIop = current;
                        }
                        if (current.InteractionDefinition as CancelInterview.Definition is not null)
                        {
                            cancelIop = current;
                        }
                    }
                    if (postponeIop is not null)
                    {
                        for (int i = actor.InteractionQueue.Count - 1; i >= 0; i--)
                        {
                            InteractionInstance current = actor.InteractionQueue.mInteractionList[i];
                            if (current.InteractionDefinition == postponeIop.InteractionDefinition && (current.GetSelectedObject() as InterviewData) == this)
                            {
                                actor.InteractionQueue.RemoveInteractionByRef(current);
                            }
                        }
                    }
                    if (cancelIop is not null)
                    {
                        for (int i = actor.InteractionQueue.Count - 1; i >= 0; i--)
                        {
                            InteractionInstance current = actor.InteractionQueue.mInteractionList[i];
                            if (current.InteractionDefinition == cancelIop.InteractionDefinition && (current.GetSelectedObject() as InterviewData) == this)
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
            }

            internal static void DisposeActorData(ulong actorId)
            {
                if (InterviewLists.TryGetValue(actorId, out List<InterviewData> list))
                {
                    for (int i = list.Count - 1; i >= 0; i--)
                    {
                        list[i].Dispose(false);
                    }
                }
            }
        }

        [PersistableStatic]
        public static Dictionary<ulong, List<InterviewData>> InterviewLists = new();

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
                    if (mData.ActorId != actor.SimDescription.SimDescriptionId)
                    {
                        return false;
                    }
                    if (SimClock.CurrentTime() < SimClock.Subtract(mData.InterviewDate, TimeUnit.Hours, 1))
                    {
                        greyedOutTooltipCallback = CreateTooltipCallback(LocalizeString("NotYetTimeForInterview", new object[]
                        {
                            actor,
                            GetTextDayOfWeek(mData.InterviewDate),
                            SimClockUtils.GetText(Convert.ToInt32(mData.InterviewDate.Hour))
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
                if (Actor.IsSelectable && CareerManager.GetStaticCareer(mData.CareerName) is Career career && Settings.InterviewMap.TryGetValue(career.SharedData.Name.Substring(34), out PersistedSettings.InterviewSettings settings))
                {
                    float chance = career.Category is Career.CareerCategory.FullTime ? Settings.BaseFullTimeJobChance : Settings.BasePartTimeJobChance;
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
                    mAlarm1 = AlarmManager.Global.AddAlarm(time, TimeUnit.Minutes, ShowFirstMessage, "Gamefreak130 wuz here -- Interview Message Callback", AlarmType.AlwaysPersisted, Actor);
                    mAlarm2 = AlarmManager.Global.AddAlarm(time * 2, TimeUnit.Minutes, ShowSecondMessage, "Gamefreak130 wuz here -- Interview Message Callback", AlarmType.AlwaysPersisted, Actor);
                    bool flag = DoTimedLoop(Settings.InterviewTime);
                    if (!flag)
                    {
                        return false;
                    }
                    if (mSuccess)
                    {
                        if (mData.RabbitHole.CareerLocations.TryGetValue((ulong)mData.CareerName, out CareerLocation location))
                        {
                            bool showUi = Actor.SimDescription.CareerManager.IsLocationTransfer(new(location, false, false));
                            AcquireOccupationParameters occupationParameters = new(location, showUi, true);
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
                        Actor.ShowTNSIfSelectable(LocalizeString(Actor.IsFemale, "InterviewFailure", Actor), StyledNotification.NotificationStyle.kGameMessageNegative, Target.RabbitHoleProxy.ObjectId, ObjectGuid.InvalidObjectGuid);
                        Audio.StartSound("sting_opp_fail");
                        Actor.BuffManager.AddElement(0x552A7AD84AF2FA7E, (Origin)ResourceUtils.HashString64("FromBadInterview"));
                        return true;
                    }
                }
                return false;
            }

            public void ShowFirstMessage() => Actor.ShowTNSIfSelectable(LocalizeString(Actor.IsFemale, mSuccess ? "FirstInterviewTNSGood" : "FirstInterviewTNSBad"), StyledNotification.NotificationStyle.kSimTalking);

            public void ShowSecondMessage() => Actor.ShowTNSIfSelectable(LocalizeString(Actor.IsFemale, mSuccess ? "SecondInterviewTNSGood" : "SecondInterviewTNSBad"), StyledNotification.NotificationStyle.kSimTalking);

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
                public override string[] GetPath(bool isFemale) => new[] { Phone.LocalizeString("JobsAndOffers") + Localization.Ellipsis };

                public override string GetInteractionName(Sim actor, Phone target, InteractionObjectPair iop) => LocalizeString("PostponeInterview") + Localization.Ellipsis;

                public override bool Test(Sim a, Phone target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    if (InterviewLists.TryGetValue(a.SimDescription.SimDescriptionId, out List<InterviewData> list))
                    {
                        foreach (InterviewData data in list)
                        {
                            if (SimClock.CurrentTime() < SimClock.Subtract(data.InterviewDate, TimeUnit.Hours, 1f) && data.TimesPostponed < Settings.MaxInterviewPostpones)
                            {
                                return base.Test(a, target, isAutonomous, ref greyedOutTooltipCallback);
                            }
                        }
                    }
                    return false;
                }

                public override void PopulatePieMenuPicker(ref InteractionInstanceParameters parameters, out List<TabInfo> listObjs, out List<HeaderInfo> headers, out int NumSelectableRows)
                {
                    headers = new()
                    {
                        new("Ui/Caption/ObjectPicker:Name", "", 350),
                        new("Gamefreak130/LocalizedMod/JobOverhaul:TimeHeader", "", 150)
                    };
                    List<RowInfo> rows = new();
                    foreach (InterviewData data in InterviewLists[parameters.Actor.SimDescription.SimDescriptionId])
                    {
                        if (SimClock.CurrentTime() < SimClock.Subtract(data.InterviewDate, TimeUnit.Hours, 1f) && data.TimesPostponed < Settings.MaxInterviewPostpones)
                        {
                            Occupation occupation = CareerManager.GetStaticOccupation(data.CareerName);
                            rows.Add(new(data, new()
                            {
                                new ImageAndTextColumn(occupation.CareerIconColored, $"{occupation.CareerName} ({data.RabbitHole.GetLocalizedName()})"),
                                new TextColumn($"{SimClockUtils.GetDayAsText(data.InterviewDate.DayOfWeek)} {SimClockUtils.GetText(Convert.ToInt32(data.InterviewDate.Hour))}")
                            }));
                        }
                    }
                    listObjs = new() { new("", "", rows) };
                    NumSelectableRows = 1;
                }
            }

            public static readonly InteractionDefinition Singleton = new Definition();

            public override bool Run() => GetSelectedObject() is InterviewData data && SimClock.CurrentTime() < SimClock.Subtract(data.InterviewDate, TimeUnit.Hours, 1f) && data.TimesPostponed < Settings.MaxInterviewPostpones && base.Run();

            public override ConversationBehavior OnCallConnected() 
            {
                InterviewData data = GetSelectedObject() as InterviewData;
                if (InterviewLists.TryGetValue(data.ActorId, out List<InterviewData> list) && list.Contains(data))
                {
                    HolidayManager manager = HolidayManager.Instance;
                    do
                    {
                        data.InterviewDate = SimClock.Add(data.InterviewDate, TimeUnit.Days, 1);
                    } while (data.InterviewDate.DayOfWeek is DaysOfTheWeek.Saturday or DaysOfTheWeek.Sunday || (manager is not null && SimClock.IsSameDay(data.InterviewDate, manager.mStartDateTimeOfHoliday)));
                    AlarmManager.Global.RemoveAlarm(data.RemindAlarm);
                    data.RemindAlarm = AlarmManager.Global.AddAlarm(SimClock.HoursUntil(data.InterviewDate) - 1, TimeUnit.Hours, data.OnReminderCallback, "Gamefreak130 wuz here -- Interview Reminder", AlarmType.AlwaysPersisted, Actor);
                    AlarmManager.Global.RemoveAlarm(data.TimeoutAlarm);
                    data.TimeoutAlarm = AlarmManager.Global.AddAlarm(SimClock.HoursUntil(data.InterviewDate) + 0.5f, TimeUnit.Hours, data.OnInterviewTimeout, "Gamefreak130 wuz here -- Interview Timeout", AlarmType.AlwaysPersisted, Actor);
                    data.TimesPostponed += 1;
                    Actor.ShowTNSIfSelectable(LocalizeString(Actor.IsFemale, data.TimesPostponed >= Settings.MaxInterviewPostpones ? "InterviewPostponedFinal" : "InterviewPostponed", new object[]
                        {
                        Actor,
                        data.RabbitHole.GetLocalizedName(),
                        GetTextDayOfWeek(data.InterviewDate),
                        SimClockUtils.GetText(Convert.ToInt32(data.InterviewDate.Hour))
                        }), data.TimesPostponed >= Settings.MaxInterviewPostpones ? StyledNotification.NotificationStyle.kGameMessageNegative : StyledNotification.NotificationStyle.kSystemMessage);
                    return ConversationBehavior.TalkBriefly;
                }
                return ConversationBehavior.JustHangUp;
            }
        }

        public class CancelInterview : Phone.Call
        {
            public class Definition : CallDefinition<CancelInterview>
            {
                public override string[] GetPath(bool isFemale) => new[] { Phone.LocalizeString("JobsAndOffers") + Localization.Ellipsis };

                public override string GetInteractionName(Sim actor, Phone target, InteractionObjectPair iop) => LocalizeString("CancelInterview") + Localization.Ellipsis;

                public override bool Test(Sim a, Phone target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback) => base.Test(a, target, isAutonomous, ref greyedOutTooltipCallback) && InterviewLists.ContainsKey(a.SimDescription.SimDescriptionId);

                public override void PopulatePieMenuPicker(ref InteractionInstanceParameters parameters, out List<TabInfo> listObjs, out List<HeaderInfo> headers, out int NumSelectableRows)
                {
                    headers = new()
                    {
                        new("Ui/Caption/ObjectPicker:Name", "", 350),
                        new("Gamefreak130/LocalizedMod/JobOverhaul:TimeHeader", "", 150)
                    };
                    List<RowInfo> rows = new();
                    foreach (InterviewData data in InterviewLists[parameters.Actor.SimDescription.SimDescriptionId])
                    {
                        Occupation occupation = CareerManager.GetStaticOccupation(data.CareerName);
                        rows.Add(new(data, new()
                        {
                            new ImageAndTextColumn(occupation.CareerIconColored, $"{occupation.CareerName} ({data.RabbitHole.GetLocalizedName()})"),
                            new TextColumn($"{SimClockUtils.GetDayAsText(data.InterviewDate.DayOfWeek)} {SimClockUtils.GetText(Convert.ToInt32(data.InterviewDate.Hour))}")
                        }));
                    }
                    listObjs = new() { new("", "", rows) };
                    NumSelectableRows = 1;
                }
            }

            public static readonly InteractionDefinition Singleton = new Definition();

            public override ConversationBehavior OnCallConnected()
            {
                InterviewData data = GetSelectedObject() as InterviewData;
                if (InterviewLists.TryGetValue(data.ActorId, out List<InterviewData> list) && list.Contains(data))
                {
                    if (TwoButtonDialog.Show(LocalizeString(Actor.IsFemale, "CancelInterviewDialog", Actor), LocalizationHelper.Yes, LocalizationHelper.No))
                    {
                        data.Dispose(false);
                        Actor.ShowTNSIfSelectable(LocalizeString(Actor.IsFemale, "InterviewCanceled", new object[]
                        {
                        Actor,
                        CareerManager.GetStaticOccupation(data.CareerName).GetLocalizedCareerName(Actor.IsFemale)
                        }), StyledNotification.NotificationStyle.kSystemMessage);
                        return ConversationBehavior.Nod;
                    }
                    return ConversationBehavior.ShakeHead;
                }
                return ConversationBehavior.JustHangUp;
            }
        }
    }
}
