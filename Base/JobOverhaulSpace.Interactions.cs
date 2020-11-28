using Gamefreak130.JobOverhaulSpace.Helpers;
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
using Sims3.Gameplay.ChildAndTeenUpdates;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.EventSystem;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Objects;
using Sims3.Gameplay.Objects.Electronics;
using Sims3.Gameplay.Objects.Miscellaneous;
using Sims3.Gameplay.Objects.RabbitHoles;
using Sims3.Gameplay.Roles;
using Sims3.Gameplay.Seasons;
using Sims3.Gameplay.Situations;
using Sims3.Gameplay.Skills;
using Sims3.Gameplay.Socializing;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.SimIFace.CAS;
using Sims3.SimIFace.Enums;
using Sims3.UI;
using Sims3.UI.GameEntry;
using Sims3.UI.Hud;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using static Gamefreak130.JobOverhaul;
using static Gamefreak130.JobOverhaulSpace.Helpers.Methods;
using static Gamefreak130.JobOverhaulSpace.Interactions.Interviews;
using static Sims3.Gameplay.ActiveCareer.ActiveCareers.DaycareTransportSituation;
using static Sims3.Gameplay.GlobalFunctions;
using static Sims3.Gameplay.Queries;
using static Sims3.UI.ObjectPicker;
using static Sims3.UI.StyledNotification;
using Methods = Gamefreak130.Common.Methods;

namespace Gamefreak130.JobOverhaulSpace.Interactions
{
    public class ChangeSettings : ImmediateInteraction<Sim, GameObject>
    {
        [DoesntRequireTuning]
        public class Definition : ImmediateInteractionDefinition<Sim, GameObject, ChangeSettings>
        {
            private bool mIsPhone;

            public override string GetInteractionName(Sim actor, GameObject target, InteractionObjectPair iop)
            {
                return LocalizeString("ChangeSettingsName");
            }

            public override bool Test(Sim actor, GameObject target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                mIsPhone = target is PhoneCell;
                return (target is Computer or PhoneHome || (target is PhoneCell && (target as PhoneCell).IsUsableBy(actor)) || (target is Newspaper && (target as Newspaper).IsReadable)) && actor.SimDescription.TeenOrAbove && !GameUtils.IsOnVacation() && !GameUtils.IsUniversityWorld();
            }

            public override string[] GetPath(bool isFemale) => mIsPhone
                    ? (new string[] { Phone.LocalizeString("JobsAndOffers") + Localization.Ellipsis })
                    : (new string[] { Computer.LocalizeString("JobsAndProfessions") });
        }

        public static readonly InteractionDefinition Singleton = new Definition();

        public override bool RunFromInventory() => Run();

        public override bool Run()
        {
            Common.UI.MenuContainer container = new Common.UI.MenuContainer(LocalizeString("MenuTitle"), LocalizeString("Settings"));
            container.AddMenuObject(new Common.UI.SetValuePromptObject<int>(LocalizeString("NumBonusResumeJobsMenuName"), LocalizeString("NumBonusResumeJobsPrompt"), "NumBonusResumeJobs", () => GameUtils.IsInstalled(ProductVersion.EP9), Settings));
            container.AddMenuObject(new Common.UI.SetValuePromptObject<int>(LocalizeString("FullTimeInterviewHourMenuName"), LocalizeString("FullTimeInterviewHourPrompt"), "FullTimeInterviewHour", null, Settings));
            container.AddMenuObject(new Common.UI.SetValuePromptObject<int>(LocalizeString("PartTimeInterviewHourMenuName"), LocalizeString("PartTimeInterviewHourPrompt"), "PartTimeInterviewHour", null, Settings));
            container.AddMenuObject(new Common.UI.SetValuePromptObject<int>(LocalizeString("MaxInterviewPostponesMenuName"), LocalizeString("MaxInterviewPostponesPrompt"), "MaxInterviewPostpones", null, Settings));
            container.AddMenuObject(new Common.UI.SetValuePromptObject<float>(LocalizeString("ReadyForInterviewChanceChangeMenuName"), LocalizeString("ReadyForInterviewChanceChangePrompt"), "ReadyForInterviewChanceChange", 
                () => GameUtils.IsInstalled(ProductVersion.EP9), Settings));
            container.AddMenuObject(new Common.UI.SetValuePromptObject<float>(LocalizeString("BaseFullTimeJobChanceMenuName"), LocalizeString("BaseFullTimeJobChancePrompt"), "BaseFullTimeJobChance", null, Settings));
            container.AddMenuObject(new Common.UI.SetValuePromptObject<float>(LocalizeString("BasePartTimeJobChanceMenuName"), LocalizeString("BasePartTimeJobChancePrompt"), "BasePartTimeJobChance", null, Settings));
            container.AddMenuObject(new Common.UI.SetValuePromptObject<float>(LocalizeString("PostponeInterviewChanceChangeMenuName"), LocalizeString("PostponeInterviewChanceChangePrompt"), "PostponeInterviewChanceChange", null, Settings));
            container.AddMenuObject(new Common.UI.SetValuePromptObject<float>(LocalizeString("PositiveTraitInterviewChanceChangeMenuName"), LocalizeString("PositiveTraitInterviewChanceChangePrompt"), "PositiveTraitInterviewChanceChange", null, Settings));
            container.AddMenuObject(new Common.UI.SetValuePromptObject<float>(LocalizeString("NegativeTraitInterviewChanceChangeMenuName"), LocalizeString("NegativeTraitInterviewChanceChangePrompt"), "NegativeTraitInterviewChanceChange", null, Settings));
            container.AddMenuObject(new Common.UI.SetValuePromptObject<float>(LocalizeString("RequiredSkillInterviewChanceChangeMenuName"), LocalizeString("RequiredSkillInterviewChanceChangePrompt"), "RequiredSkillInterviewChanceChange", null, Settings));
            container.AddMenuObject(new Common.UI.SetValuePromptObject<float>(LocalizeString("PromotionChanceMenuName"), LocalizeString("PromotionChancePrompt"), "PromotionChance", null, Settings));
            container.AddMenuObject(new Common.UI.SetValuePromptObject<float>(LocalizeString("InterviewTimeMenuName"), LocalizeString("InterviewTimePrompt"), "InterviewTime", null, Settings));
            container.AddMenuObject(new Common.UI.SetValuePromptObject<float>(LocalizeString("ApplicationTimeMenuName"), LocalizeString("ApplicationTimePrompt"), "ApplicationTime", null, Settings));
            container.AddMenuObject(new Common.UI.GenericActionObject(LocalizeString("NewspaperSelfEmployedMenuName"), () => Settings.NewspaperSelfEmployed.ToString(), 
                () => GameUtils.IsInstalled(ProductVersion.EP2) || GameUtils.IsInstalled(ProductVersion.EP3) || GameUtils.IsInstalled(ProductVersion.EP5) || GameUtils.IsInstalled(ProductVersion.EP7) || GameUtils.IsInstalled(ProductVersion.EP10) || GameUtils.IsInstalled(ProductVersion.EP11), 
                () => Settings.NewspaperSelfEmployed = !Settings.NewspaperSelfEmployed));
            container.AddMenuObject(new Common.UI.GenericActionObject(LocalizeString("EnableGetJobInRabbitholeMenuName"), () => Settings.EnableGetJobInRabbitHole.ToString(), null, () => Settings.EnableGetJobInRabbitHole = !Settings.EnableGetJobInRabbitHole));
            container.AddMenuObject(new Common.UI.GenericActionObject(LocalizeString("EnableJoinProfessionMenuName"), () => Settings.EnableJoinProfessionInRabbitHoleOrLot.ToString(), () => CareerManager.GetActiveCareers().Count > 0, 
                () => Settings.EnableJoinProfessionInRabbitHoleOrLot = !Settings.EnableJoinProfessionInRabbitHoleOrLot));
            container.AddMenuObject(new Common.UI.GenericActionObject(LocalizeString("InstantGratificationMenuName"), () => Settings.InstantGratification.ToString(), null, () => Settings.InstantGratification = !Settings.InstantGratification));
            container.AddMenuObject(new Common.UI.GenericActionObject(LocalizeString("HoloComputerInstantGratificationMenuName"), () => Settings.HoloComputerInstantGratification.ToString(), () => !Settings.InstantGratification && GameUtils.IsInstalled(ProductVersion.EP11), 
                () => Settings.HoloComputerInstantGratification = !Settings.HoloComputerInstantGratification));
            container.AddMenuObject(new Common.UI.GenericActionObject(LocalizeString("HoloPhoneInstantGratificationMenuName"), () => Settings.HoloPhoneInstantGratification.ToString(), 
                () => !Settings.InstantGratification && GameUtils.IsInstalled(ProductVersion.EP11) && GameUtils.IsInstalled(ProductVersion.EP9), () => Settings.HoloPhoneInstantGratification = !Settings.HoloPhoneInstantGratification));
            container.AddMenuObject(new Common.UI.GenericActionObject(LocalizeString("ClearAllInterviewsMenuName"), () => string.Empty, null,
                delegate () {
                    if (AcceptCancelDialog.Show(LocalizeString("ClearAllInterviewsPrompt")))
                    {
                        for (int i = InterviewList.Count - 1; i >= 0; i--)
                        {
                            InterviewList[i].Dispose(false);
                        }
                        SimpleMessageDialog.Show(LocalizeString("ClearAllInterviewsMenuName"), LocalizeString("ClearAllInterviewsComplete"));
                    }
                }));
            container.AddMenuObject(new Common.UI.OneShotActionObject(LocalizeString("ResetMenuName"), () => string.Empty, null,
                delegate () {
                    if (AcceptCancelDialog.Show(LocalizeString("ResetPrompt")))
                    {
                        ResetSettings();
                        Show(new Format(LocalizeString("ResetComplete"), NotificationStyle.kSystemMessage));
                        return true;
                    }
                    return false;
                }));
            container.AddMenuObject(new Common.UI.GenericActionObject(LocalizeString("ExportMenuName"), () => string.Empty, null,
                delegate () {
                CJACK_01:
                    string name = StringInputDialog.Show(LocalizeString("ExportMenuName"), LocalizeString("ExportPrompt"), "Settings");
                    if (!string.IsNullOrEmpty(name))
                    {
                        name = "Gamefreak130.JobOverhaul." + name;
                        Sims3.Gameplay.BinModel.Singleton.PopulateExportBin();
                        foreach (IExportBinContents current in Sims3.Gameplay.BinModel.Singleton.ExportBinContents)
                        {
                            Sims3.Gameplay.ExportBinContents contents = current as Sims3.Gameplay.ExportBinContents;
                            if (contents.HouseholdName != null && contents.HouseholdName == name)
                            {
                                if (AcceptCancelDialog.Show(LocalizeString("ExportFileOverwritePrompt")))
                                {
                                    Sims3.Gameplay.BinModel.Singleton.DeleteFromExportBin(contents.BinInfo);
                                }
                                else
                                {
                                    goto CJACK_01;
                                }
                            }
                        }
                        Household household = Household.Create();
                        household.SetName(name);
                        household.BioText = Settings.Export();
                        Sims3.Gameplay.BinModel.Singleton.AddToExportBin(household);
                        household.Destroy();
                        SimpleMessageDialog.Show(LocalizeString("ExportMenuName"), LocalizeString("ExportComplete"));
                    }
                }));
            container.AddMenuObject(new Common.UI.OneShotActionObject(LocalizeString("ImportMenuName"), () => string.Empty, null,
                delegate () {
                    Sims3.Gameplay.BinModel.Singleton.PopulateExportBin();
                    List<TabInfo> list = new List<TabInfo>() { new TabInfo("shop_all_r2", string.Empty, new List<RowInfo>()) };
                    foreach (IExportBinContents current in Sims3.Gameplay.BinModel.Singleton.ExportBinContents)
                    {
                        if (current.HouseholdName != null && current.HouseholdName.Contains("Gamefreak130.JobOverhaul."))
                        {
                            RowInfo info = new RowInfo(current, new List<ColumnInfo>() { new TextColumn(current.HouseholdName.Substring(25)) });
                            list[0].RowInfo.Add(info);
                        }
                    }
                    if (list[0].RowInfo.Count == 0)
                    {
                        SimpleMessageDialog.Show(LocalizeString("ImportMenuName"), LocalizeString("ImportFileNotFound"));
                        return false;
                    }
                    List<RowInfo> list2 = ObjectPickerDialog.Show(true, ModalDialog.PauseMode.PauseSimulator, LocalizeString("ImportMenuName"), Localization.LocalizeString("Ui/Caption/ObjectPicker:OK"), Localization.LocalizeString("Ui/Caption/ObjectPicker:Cancel"), 
                        list, new List<HeaderInfo>() { new HeaderInfo("Ui/Caption/ObjectPicker:Name", "", 250) }, 1, new Vector2(-1f, -1f), false, null, false, false);
                    if (list2 != null)
                    {
                        XmlDocument xml = new XmlDocument();
                        xml.LoadXml(((IExportBinContents)list2[0].Item).HouseholdBio);
                        ParseXml(xml.DocumentElement.FirstChild);
                        SimpleMessageDialog.Show(LocalizeString("ImportMenuName"), LocalizeString("ImportComplete"));
                        return true;
                    }
                    return false;
                }));

            Common.UI.MenuContainer container2 = new Common.UI.MenuContainer(LocalizeString("MenuTitle"), LocalizeString("CareerAvailabilitySettingsMenuName"));
            foreach (string key in Settings.CareerAvailabilitySettings.Keys)
            {
                CareerAvailabilitySettings settings = Settings.CareerAvailabilitySettings[key];
                string name = settings.IsActive ? XpBasedCareer.LocalizeString(Actor.IsFemale, key) : Localization.LocalizeString(Actor.IsFemale, "Gameplay/Excel/Careers/CareerList:" + key);
                Common.UI.MenuContainer jobContainer = new Common.UI.MenuContainer(LocalizeString("MenuTitle"), name);
                jobContainer.AddMenuObject(new Common.UI.GenericActionObject(LocalizeString("IsAvailableMenuName"), () => settings.IsAvailable.ToString(), null, () => settings.IsAvailable = !settings.IsAvailable));
                jobContainer.AddMenuObject(new Common.UI.GenericActionObject(LocalizeString("RequiredDegreesMenuName"), () => string.Empty, () => GameUtils.IsInstalled(ProductVersion.EP9),
                    delegate () {
                        List<TabInfo> list = new List<TabInfo>() { new TabInfo("shop_all_r2", string.Empty, new List<RowInfo>()) };
                        List<RowInfo> list2 = new List<RowInfo>();
                        foreach (AcademicDegreeNames degree in GenericManager<AcademicDegreeNames, AcademicDegreeStaticData, AcademicDegree>.sDictionary.Keys)
                        {
                            RowInfo info = new RowInfo(degree, new List<ColumnInfo>() { new TextColumn(AcademicDegreeManager.GetStaticElement(degree).DegreeName) });
                            list[0].RowInfo.Add(info);
                            if (settings.RequiredDegrees.Contains(degree))
                            {
                                list2.Add(info);
                            }
                        }
                        List<RowInfo> list3 = UI.ObjectPickerDialogEx.Show(true, ModalDialog.PauseMode.PauseSimulator, LocalizeString("RequiredDegreesMenuName"), Localization.LocalizeString("Ui/Caption/ObjectPicker:OK"), Localization.LocalizeString("Ui/Caption/ObjectPicker:Cancel"), 
                            list, new List<HeaderInfo>() { new HeaderInfo("Ui/Caption/ObjectPicker:Degree", "", 250) }, list[0].RowInfo.Count, new Vector2(-1f, -1f), false, list2, false, false);
                        if (list3 != null)
                        {
                            settings.RequiredDegrees.Clear();
                            if (list3.Count > 0)
                            {
                                foreach (RowInfo info in list3)
                                {
                                    settings.RequiredDegrees.Add((AcademicDegreeNames)info.Item);
                                }
                            }
                        }
                    }));
                container2.AddMenuObject(new List<HeaderInfo>() { new HeaderInfo("Ui/Caption/ObjectPicker:Name", "", 250) }, new Common.UI.GenerateMenuObject(new List<Common.UI.ColumnDelegateStruct>() { new Common.UI.ColumnDelegateStruct(ColumnType.kText, () => new TextColumn(name)) }, jobContainer));
            }
            container.AddMenuObject(new Common.UI.GenerateMenuObject(LocalizeString("CareerAvailabilitySettingsMenuName"), container2));

            Common.UI.MenuContainer container3 = new Common.UI.MenuContainer(LocalizeString("MenuTitle"), LocalizeString("InterviewSettingsMenuName"));
            foreach (string key in Settings.InterviewSettings.Keys)
            {
                InterviewSettings settings = Settings.InterviewSettings[key];
                string name = Localization.LocalizeString(Actor.IsFemale, "Gameplay/Excel/Careers/CareerList:" + key);
                Common.UI.MenuContainer interviewContainer = new Common.UI.MenuContainer(LocalizeString("MenuTitle"), name);
                interviewContainer.AddMenuObject(new Common.UI.GenericActionObject(LocalizeString("RequiresInterviewMenuName"), () => settings.RequiresInterview.ToString(), null, () => settings.RequiresInterview = !settings.RequiresInterview));
                interviewContainer.AddMenuObject(new Common.UI.GenericActionObject(LocalizeString("RequiredSkillsMenuName"), () => string.Empty, null,
                    delegate () {
                        List<HeaderInfo> list = new List<HeaderInfo>()
                        {
                            new HeaderInfo("Ui/Caption/ObjectPicker:Name", "", 250),
                            new HeaderInfo("Product Version", "")
                        };
                        List<TabInfo> list2 = new List<TabInfo>() { new TabInfo("shop_all_r2", string.Empty, new List<RowInfo>()) };
                        List<RowInfo> list3 = new List<RowInfo>();
                        foreach (SkillNames skill in GenericManager<SkillNames, Skill, Skill>.sDictionary.Keys)
                        {
                            Skill staticSkill = SkillManager.GetStaticSkill(skill);
                            if (((staticSkill.AvailableAgeSpecies & CASAGSAvailabilityFlags.HumanAdult) != CASAGSAvailabilityFlags.None) && (!staticSkill.IsHiddenSkill(CASAGSAvailabilityFlags.HumanTeen | CASAGSAvailabilityFlags.HumanAdult) || staticSkill.IsHiddenWithSkillProgress))
                            {
                                RowInfo info = new RowInfo(skill, new List<ColumnInfo>() { new TextColumn(Skill.GetLocalizedSkillName(skill, Actor.SimDescription)), new TextColumn(SkillManager.GetStaticSkill(skill).NonPersistableData.SkillProductVersion.ToString()) });
                                list2[0].RowInfo.Add(info);
                                if (settings.RequiredSkills.Contains(skill))
                                {
                                    list3.Add(info);
                                }
                            }
                        }
                        List<RowInfo> skillList = UI.ObjectPickerDialogEx.Show(true, ModalDialog.PauseMode.PauseSimulator, LocalizeString("RequiredSkillsMenuName"), Localization.LocalizeString("Ui/Caption/ObjectPicker:OK"), Localization.LocalizeString("Ui/Caption/ObjectPicker:Cancel"), 
                            list2, list, list2[0].RowInfo.Count, new Vector2(-1f, -1f), false, list3, false, false);
                        if (skillList != null)
                        {
                            settings.RequiredSkills.Clear();
                            if (skillList.Count > 0)
                            {
                                foreach (RowInfo info in skillList)
                                {
                                    settings.RequiredSkills.Add((SkillNames)info.Item);
                                }
                            }
                        }
                    }));
                interviewContainer.AddMenuObject(new Common.UI.GenericActionObject(LocalizeString("PositiveTraitsMenuName"), () => string.Empty, null,
                    delegate () {
                        List<TabInfo> list = new List<TabInfo>() { new TabInfo("shop_all_r2", string.Empty, new List<RowInfo>()) };
                        List<RowInfo> list2 = new List<RowInfo>();
                        foreach (TraitNames trait in GenericManager<TraitNames, Trait, Trait>.sDictionary.Keys)
                        {
                            Trait staticTrait = TraitManager.GetTraitFromDictionary(trait);
                            if (staticTrait.IsVisible && (staticTrait.AgeSpeciesAvailabiltiyFlag & CASAGSAvailabilityFlags.HumanAdult) != CASAGSAvailabilityFlags.None)
                            {
                                RowInfo info = new RowInfo(trait, new List<ColumnInfo>() { new ThumbAndTextColumn(new ThumbnailKey(staticTrait.IconKey, ThumbnailSize.ExtraLarge), staticTrait.ToString()) });
                                list[0].RowInfo.Add(info);
                                if (settings.PositiveTraits.Contains(trait))
                                {
                                    list2.Add(info);
                                }
                            }
                        }
                        List<RowInfo> traitList = UI.ObjectPickerDialogEx.Show(true, ModalDialog.PauseMode.PauseSimulator, LocalizeString("PositiveTraitsMenuName"), Localization.LocalizeString("Ui/Caption/ObjectPicker:OK"), Localization.LocalizeString("Ui/Caption/ObjectPicker:Cancel"), 
                            list, new List<HeaderInfo>() { new HeaderInfo("Ui/Caption/ObjectPicker:Name", "", 400) }, list[0].RowInfo.Count, new Vector2(-1f, -1f), false, list2, false, false);
                        if (traitList != null)
                        {
                            settings.PositiveTraits.Clear();
                            if (traitList.Count > 0)
                            {
                                foreach (RowInfo info in traitList)
                                {
                                    settings.PositiveTraits.Add((TraitNames)info.Item);
                                }
                            }
                        }
                    }));
                interviewContainer.AddMenuObject(new Common.UI.GenericActionObject(LocalizeString("NegativeTraitsMenuName"), () => string.Empty, null,
                    delegate () {
                        List<TabInfo> list = new List<TabInfo>() { new TabInfo("shop_all_r2", string.Empty, new List<RowInfo>()) };
                        List<RowInfo> list2 = new List<RowInfo>();
                        foreach (TraitNames trait in GenericManager<TraitNames, Trait, Trait>.sDictionary.Keys)
                        {
                            Trait staticTrait = TraitManager.GetTraitFromDictionary(trait);
                            if (staticTrait.IsVisible && (staticTrait.AgeSpeciesAvailabiltiyFlag & CASAGSAvailabilityFlags.HumanAdult) != CASAGSAvailabilityFlags.None)
                            {
                                RowInfo info = new RowInfo(trait, new List<ColumnInfo>() { new ThumbAndTextColumn(new ThumbnailKey(staticTrait.IconKey, ThumbnailSize.ExtraLarge), staticTrait.ToString()) });
                                list[0].RowInfo.Add(info);
                                if (settings.NegativeTraits.Contains(trait))
                                {
                                    list2.Add(info);
                                }
                            }
                        }
                        List<RowInfo> traitList = UI.ObjectPickerDialogEx.Show(true, ModalDialog.PauseMode.PauseSimulator, LocalizeString("NegativeTraitsMenuName"), Localization.LocalizeString("Ui/Caption/ObjectPicker:OK"), Localization.LocalizeString("Ui/Caption/ObjectPicker:Cancel"), 
                            list, new List<HeaderInfo>() { new HeaderInfo("Ui/Caption/ObjectPicker:Name", "", 400) }, list[0].RowInfo.Count, new Vector2(-1f, -1f), false, list2, false, false);
                        if (traitList != null)
                        {
                            settings.NegativeTraits.Clear();
                            if (traitList.Count > 0)
                            {
                                foreach (RowInfo info in traitList)
                                {
                                    settings.NegativeTraits.Add((TraitNames)info.Item);
                                }
                            }
                        }
                    }));
                container3.AddMenuObject(new List<HeaderInfo>() { new HeaderInfo("Ui/Caption/ObjectPicker:Name", "", 250) }, new Common.UI.GenerateMenuObject(new List<Common.UI.ColumnDelegateStruct>() { new Common.UI.ColumnDelegateStruct(ColumnType.kText, () => new TextColumn(name)) }, interviewContainer));
            }
            container.AddMenuObject(new Common.UI.GenerateMenuObject(LocalizeString("InterviewSettingsMenuName"), container3));

            if (Settings.SelfEmployedAvailabilitySettings.Count > 0)
            {
                Common.UI.MenuContainer container4 = new Common.UI.MenuContainer(LocalizeString("MenuTitle"), LocalizeString("SelfEmployedAvailabilitySettingsMenuName"));
                foreach (string key in Settings.SelfEmployedAvailabilitySettings.Keys)
                {
                    container4.AddMenuObject(new Common.UI.GenericActionObject(XpBasedCareer.LocalizeString(Actor.IsFemale, key), () => Settings.SelfEmployedAvailabilitySettings[key].ToString(), null, 
                        () => Settings.SelfEmployedAvailabilitySettings[key] = !Settings.SelfEmployedAvailabilitySettings[key]));
                }
                container.AddMenuObject(new Common.UI.GenerateMenuObject(LocalizeString("SelfEmployedAvailabilitySettingsMenuName"), container4));
            }
            Common.UI.MenuController.ShowMenu(container);
            return true;
        }
    }

    public class DeliverNewspaperEx : Sims3.Gameplay.Services.DeliverNewspaper
    {
        [DoesntRequireTuning]
        new public class Definition : InteractionDefinition<Sim, Lot, DeliverNewspaperEx>
        {
            public override string GetInteractionName(Sim a, Lot target, InteractionObjectPair interaction) => "Deliver Newspaper";

            public override bool Test(Sim a, Lot target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback) => true;
        }

        public override bool Run()
        {
            Sim actor = Actor;
            if (!RouteToDeliverPaper(actor))
            {
                return false;
            }
            if (Actor.LotCurrent.LatestNewspaper != null)
            {
                Actor.LotCurrent.LatestNewspaper.MakeOld();
                RandomNewspaperSeeds.Remove(Actor.LotCurrent.LatestNewspaper.ObjectId);
            }
            int num = Newspaper.MakeAllNewspapersOld(Actor.LotCurrent);
            if (num > (long)(ulong)kMaxNumPapers)
            {
                string titleText = Localization.LocalizeString("Gameplay/Services/NewspaperDelivery:TooManyPapers");
                Show(new Format(titleText, actor.ObjectId, NotificationStyle.kSimTalking));
                return true;
            }
            Newspaper newspaper = (Newspaper)CreateObject("Newspaper", Vector3.OutOfWorld, 0, Vector3.UnitZ);
            RandomNewspaperSeeds.Add(newspaper.ObjectId, RandomNewspaperSeed + SimClock.ElapsedCalendarDays());
            Methods.AddInteraction(newspaper, ChangeSettings.Singleton);
            actor.ParentToRightHand(newspaper);
            CarrySystem.EnterWhileHolding(actor, newspaper);
            CarrySystem.PutDownOnFloor(actor);
            Actor.LotCurrent.LatestNewspaper = newspaper;
            Household household = Target.LotCurrent.Household;
            if (household != null)
            {
                foreach (Sim current in household.Sims)
                {
                    EventTracker.SendEvent(EventTypeId.kNewspaperDelivered, current);
                }
                if (household.Sims.Count > 0)
                {
                    Sims3.Gameplay.Tutorial.Tutorialette.TriggerLesson(Sims3.Gameplay.Tutorial.Lessons.Jobs, household.Sims[0]);
                }
            }
            return true;
        }
    }

    public class GetNewspaperChooserEx : GetNewspaperChooser
    {
        new public class Definition : InteractionDefinition<Sim, Newspaper, GetNewspaperChooserEx>, IOverridesPostureIcon, IHasTraitIcon
        {
            public InteractionDefinition IntDef;

            public Definition()
            {
            }

            public Definition(InteractionDefinition intDef) => IntDef = intDef;

            public override void AddInteractions(InteractionObjectPair iop, Sim actor, Newspaper target, List<InteractionObjectPair> results)
            {
                Add(FindJobNewspaper.Singleton, actor, target, results);
                if (GameUtils.IsInstalled(ProductVersion.EP2))
                {
                    Add(CheckTheChocolateReport.Singleton, actor, target, results);
                }
                Add(RegisterAsSelfEmployedNewspaper.Singleton, actor, target, results);
                Add(CheckCurrentEvents.Singleton, actor, target, results);
                Add(CheckWeeklyEvents.Singleton, actor, target, results);
                Add(ClipCoupon.Singleton, actor, target, results);
                Add(LookForClasses.Singleton, actor, target, results);
                if (GameUtils.IsInstalled(ProductVersion.EP5))
                {
                    Add(CheckLitterNewspaper.Singleton, actor, target, results);
                    Add(AdoptPetNewspaper.Singleton, actor, target, results);
                }
                Add(CheckWeatherNewspaper.Singleton, actor, target, results);
            }

            public void Add(InteractionDefinition interactionDefinition, Sim actor, Newspaper target, List<InteractionObjectPair> results)
            {
                List<InteractionObjectPair> list = new List<InteractionObjectPair>();
                InteractionObjectPair interactionObjectPair = new InteractionObjectPair(interactionDefinition, target);
                interactionDefinition.AddInteractions(interactionObjectPair, actor, list);
                foreach (InteractionObjectPair current in list)
                {
                    GetNewspaperChooser.Definition interaction = new GetNewspaperChooser.Definition(current.InteractionDefinition);
                    interactionObjectPair = new InteractionObjectPair(interaction, target);
                    results.Add(interactionObjectPair);
                }
            }

            public override string[] GetPath(bool isFemale) => IntDef.GetPath(isFemale);

            public override string GetInteractionName(Sim a, Newspaper target, InteractionObjectPair interaction) => IntDef.GetInteractionName(a, target, interaction);

            public ResourceKey GetTraitIcon(Sim actor, GameObject target) => IntDef is IHasTraitIcon hasTraitIcon ? hasTraitIcon.GetTraitIcon(actor, target) : ResourceKey.kInvalidResourceKey;

            public override bool Test(Sim a, Newspaper target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                if (target.InUse)
                {
                    return false;
                }
                if (IntDef != null)
                {
                    InteractionInstanceParameters interactionInstanceParameters = new InteractionInstanceParameters(new InteractionObjectPair(IntDef, target), a, new InteractionPriority(InteractionPriorityLevel.UserDirected), isAutonomous, true);
                    return InteractionDefinitionUtilities.IsPass(IntDef.Test(ref interactionInstanceParameters, ref greyedOutTooltipCallback));
                }
                return true;
            }

            public bool AllowPostureIcon(Sim actor, IGameObject target) => ShouldStayInPosture(actor, target);
        }

        public override bool Run()
        {
            if (ShouldStayInPosture(Actor, Target))
            {
                InteractionDefinition intDef = (InteractionDefinition as Definition).IntDef;
                InteractionInstance instance = intDef.CreateInstance(Target, Actor, Actor.InheritedPriority(), Autonomous, true);
                Actor.InteractionQueue.PushAsContinuation(instance, false);
            }
            else
            {
                InteractionDefinition interactionDefinition = new GetNewspaper.Definition((InteractionDefinition as Definition).IntDef);
                InteractionInstance instance2 = interactionDefinition.CreateInstance(Target, Actor, GetPriority(), Autonomous, true);
                Actor.InteractionQueue.PushAsContinuation(instance2, false);
            }
            return true;
        }
    }

    public class ReadSomethingInInventoryEx : Sim.ReadSomethingInInventory
    {
        new public class Definition : SoloSimInteractionDefinition<ReadSomethingInInventoryEx>
        {
            public override bool Test(Sim actor, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback) => isAutonomous && !actor.LotCurrent.IsWorldLot 
                && (!SeasonsManager.Enabled || (SeasonsManager.CurrentWeather != Weather.Rain && SeasonsManager.CurrentWeather != Weather.Hail) || SeasonsManager.IsShelteredFromPrecipitation(actor)) && base.Test(actor, target, isAutonomous, ref greyedOutTooltipCallback);
        }

        public override bool Run()
        {
            if (Actor.Posture.Satisfies(CommodityKind.Relaxing, null) || (!Actor.Motives.HasMotive(CommodityKind.BeSuspicious) && RandomUtil.RandomChance(Sim.ChanceOfReadingBookRatherThanNewsaperWhenReadingOutdoors)))
            {
                if (IsOnceReadInstalled)
                {
                    List<Book> books = new List<Book>();
                    MethodInfo method = Type.GetType("NRaas.OnceReadSpace.Interactions.ReadSomethingInInventoryEx, NRaasOnceRead").GetMethod("ChooseBook", BindingFlags.Static | BindingFlags.Public);
                    if (Actor.Inventory != null)
                    {
                        foreach (Sims3.Gameplay.InventoryStack current in Actor.Inventory.mItems.Values)
                        {
                            if (current.List.Count > 0 && current.List[0].Object is Book)
                            {
                                foreach (Sims3.Gameplay.InventoryItem current2 in current.List)
                                {
                                    if (current2.Object is Book inventoryBook)
                                    {
                                        books.Add(inventoryBook);
                                    }
                                }
                            }
                        }
                    }
                    if (method.Invoke(null, new object[] { Actor, books }) is Book book)
                    {
                        InteractionInstance instance = ReadBookChooser.Singleton.CreateInstance(book, Actor, mPriority, Autonomous, CancellableByPlayer);
                        BeginCommodityUpdates();
                        bool flag = false;
                        try
                        {
                            flag = instance.RunInteraction();
                        }
                        finally
                        {
                            EndCommodityUpdates(flag);
                        }
                        return flag;
                    }
                    Target.AddExitReason(ExitReason.FailedToStart);
                    return false;
                }
                return DoReadBook();
            }
            return DoReadNewspaperEx();
        }

        public bool DoReadNewspaperEx()
        {
            INewspaper newspaper = Actor.Inventory.Find<INewspaper>();
            if (newspaper == null)
            {
                newspaper = CreateObjectOutOfWorld("Newspaper") as INewspaper;
                Actor.Inventory.TryToAdd(newspaper);
            }
            if (newspaper != null)
            {
                newspaper.SetFromReadSomethingInInventory();
                InteractionInstance readInteraction = GetNewspaperChooser.Singleton.CreateInstance(newspaper, Actor, new InteractionPriority(InteractionPriorityLevel.Autonomous), true, true);
                (readInteraction.InteractionDefinition as GetNewspaperChooserEx.Definition).IntDef = CheckCurrentEvents.Singleton;
                BeginCommodityUpdates();
                bool flag = readInteraction.RunInteraction();
                EndCommodityUpdates(flag);
                return flag;
            }
            Target.AddExitReason(ExitReason.FailedToStart);
            return false;
        }
    }

    public class JoinActiveCareerContinuation : RabbitHole.RabbitHoleInteraction<Sim, CityHall>
    {
        public class Definition : InteractionDefinition<Sim, CityHall, JoinActiveCareerContinuation>
        {
            public override string GetInteractionName(Sim actor, CityHall target, InteractionObjectPair iop) => LocalizeString("JoinActiveCareerContinuation");

            public override bool Test(Sim actor, CityHall target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback) => true;
        }

        public OccupationNames CareerToSet;

        public static InteractionDefinition Singleton = new Definition();

        public override bool InRabbitHole()
        {
            TimedStage timedStage = new TimedStage(GetInteractionName(), Settings.ApplicationTime, false, false, true);
            Stages = new List<Stage>(new Stage[] { timedStage });
            StartStages();
            bool flag = DoLoop(ExitReason.Default);
            if (flag && Actor.IsSelectable && CareerManager.GetStaticOccupation(CareerToSet) is ActiveCareer activeCareer && !Actor.SimDescription.TeenOrBelow && ActiveCareer.CanAddActiveCareer(Actor.SimDescription, CareerToSet) 
                && ActiveCareer.GetActiveCareerStaticData(activeCareer.Guid).CanJoinCareerFromComputerOrNewspaper && IsActiveCareerAvailable(activeCareer))
            {
                AcquireOccupationParameters occupationParameters = new AcquireOccupationParameters(CareerToSet, false, true);
                if (Actor.AcquireOccupation(occupationParameters))
                {
                    string newOccupationTnsText = Actor.CareerManager.Occupation.GetNewOccupationTnsText();
                    if (!string.IsNullOrEmpty(newOccupationTnsText))
                    {
                        Actor.CareerManager.Occupation.ShowOccupationTNS(newOccupationTnsText);
                    }
                }
                return true;
            }
            return false;
        }
    }

    public class SubmitJobApplication : RabbitHole.RabbitHoleInteraction<Sim, RabbitHole>
    {
        public class Definition : InteractionDefinition<Sim, RabbitHole, SubmitJobApplication>
        {
            public override string GetInteractionName(Sim actor, RabbitHole target, InteractionObjectPair iop) => LocalizeString("SubmitJobApplication");

            public override bool Test(Sim actor, RabbitHole target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback) => true;
        }

        public OccupationNames Occupation;

        public static InteractionDefinition Singleton = new Definition();

        public override bool InRabbitHole()
        {
            TimedStage timedStage = new TimedStage(GetInteractionName(), Settings.ApplicationTime, false, false, true);
            Stages = new List<Stage>(new Stage[] { timedStage });
            StartStages();
            bool flag = DoLoop(ExitReason.Default);
            GreyedOutTooltipCallback tooltipCallback = null;
            if (flag && Actor.IsSelectable && Target.CareerLocations.TryGetValue((ulong)Occupation, out CareerLocation careerLocation) && careerLocation.Career.CanAcceptCareer(Actor.ObjectId, ref tooltipCallback) && careerLocation.Career.CareerAgeTest(Actor.SimDescription))
            {
                string name = careerLocation.Career.SharedData.Name.Substring(34);
                if (Settings.CareerAvailabilitySettings.TryGetValue(name, out CareerAvailabilitySettings availabilitySettings) && availabilitySettings.IsAvailable)
                {
                    if (GameUtils.IsInstalled(ProductVersion.EP9) && availabilitySettings.RequiredDegrees.Count > 0)
                    {
                        foreach (AcademicDegreeNames degree in availabilitySettings.RequiredDegrees)
                        {
                            if (Actor.DegreeManager == null || !Actor.DegreeManager.HasCompletedDegree(degree))
                            {
                                return false;
                            }
                        }
                    }
                    if (Settings.InterviewSettings.TryGetValue(name, out InterviewSettings interviewSettings) && interviewSettings.RequiresInterview)
                    {
                        new InterviewData(careerLocation, Actor);
                    }
                    else
                    {
                        AcquireOccupationParameters occupationParameters = new AcquireOccupationParameters(careerLocation, false, true);
                        if (BoardingSchool.DidSimGraduate(Actor.SimDescription, BoardingSchool.BoardingSchoolTypes.None, false))
                        {
                            BoardingSchool.UpdateAcquireOccupationParameters(Actor.SimDescription.BoardingSchool, careerLocation.Career.CareerGuid, ref occupationParameters);
                        }
                        if (Actor.AcquireOccupation(occupationParameters))
                        {
                            string newOccupationTnsText = Actor.CareerManager.Occupation.GetNewOccupationTnsText();
                            if (!string.IsNullOrEmpty(newOccupationTnsText))
                            {
                                Actor.CareerManager.Occupation.ShowOccupationTNS(newOccupationTnsText);
                            }
                            Audio.StartSound("sting_career_positive");
                        }
                    }
                    return true;
                }
            }
            return false;
        }
    }

    public class GoToOccupationJobLocationEx : GoToOccupationJobLocation
    {
        [DoesntRequireTuning]
        new public class Definition : GoToOccupationJobLocation.Definition
        {
            public override InteractionInstance CreateInstance(ref InteractionInstanceParameters parameters)
            {
                GoToOccupationJobLocationEx interaction = Activator.CreateInstance<GoToOccupationJobLocationEx>();
                interaction.Init(ref parameters);
                return interaction;
            }
        }

        public static bool SetupTasks(Job job)
        {
            if (job is MedicalJob medicalJob)
            {
                TaskCreationStaticData[] staticTasks = medicalJob.StaticTasks;
                if (staticTasks != null)
                {
                    TaskCreationStaticData[] array = staticTasks;
                    for (int i = 0; i < array.Length; i++)
                    {
                        TaskCreationStaticData taskCreationStaticData = array[i];
                        int @int = RandomUtil.GetInt(taskCreationStaticData.LowerBound, taskCreationStaticData.UpperBound);
                        for (int j = 0; j < @int; j++)
                        {
                            if (medicalJob.SetupTask(taskCreationStaticData.Id))
                            {
                                medicalJob.AddTask(taskCreationStaticData.Id);
                            }
                        }
                    }
                    if (medicalJob.mTasks != null && medicalJob.mPendingTasks == null)
                    {
                        medicalJob.mPendingTasks = new List<TaskId>(medicalJob.mTasks);
                    }
                }
                if (medicalJob.mTasks == null || medicalJob.mTasks.Count == 0)
                {
                    return false;
                }
                CameraController.OnCameraMapViewEnabledCallback += new CameraController.CameraMapViewEnabledHandler(medicalJob.OnCameraMapViewEnabledCallback);
                medicalJob.StartMusic();
                SetupRabbitholeJobTns(medicalJob);
                if (medicalJob.Id == JobId.Innoculation)
                {
                    medicalJob.mSituation = VaccinationSessionSituationEx.Create(medicalJob.Occupation.OwnerDescription.CreatedSim, medicalJob.Specification.mLot);
                }
                if (medicalJob.Id == JobId.FreeClinic)
                {
                    medicalJob.mSituation = FreeClinicSessionSituationEx.Create(medicalJob.Occupation.OwnerDescription.CreatedSim, medicalJob.Specification.mLot);
                }
                return true;
            }
            return job.SetupTasks();
        }

        public static void SetupRabbitholeJobTns(MedicalJob job)
        {
            RabbitholeTnsInfo rabbitholeTnsInfo = null;
            if (job.Id != JobId.GenericMedicalRabbitHoleJob)
            {
                if (job.RabbitHole is CityHall)
                {
                    rabbitholeTnsInfo = Medical.sRabbitholeJobTnsInfo["CityHall"];
                }
                if (job.RabbitHole is Hideout)
                {
                    rabbitholeTnsInfo = Medical.sRabbitholeJobTnsInfo["Hideout"];
                }
                if (job.RabbitHole is ScienceLab)
                {
                    rabbitholeTnsInfo = Medical.sRabbitholeJobTnsInfo["ScienceLab"];
                }
            }
            if (rabbitholeTnsInfo == null)
            {
                rabbitholeTnsInfo = Medical.sRabbitholeJobTnsInfo["Generic"];
            }
            if (rabbitholeTnsInfo != null)
            {
                job.mStartTnsKey = rabbitholeTnsInfo.StartTns;
                job.mUpdateTnsKey = rabbitholeTnsInfo.UpdateTnsKey + RandomUtil.GetInt(rabbitholeTnsInfo.NumUpdateTns - 1);
            }
        }

        public override bool Run()
        {
            Job job = (InteractionDefinition as GoToOccupationJobLocation.Definition).GetJobForLot(Actor, GetInteractionParameters());
            if (job == null)
            {
                return false;
            }
            job.GetInteractionDefinitionAndTargetToGoToWork(out InteractionDefinition interactionDefinition, out GameObject gameObject);
            if (interactionDefinition == null || gameObject == null)
            {
                return false;
            }
            InteractionInstance interactionInstance = interactionDefinition.CreateInstance(gameObject, Actor, GetPriority(), Autonomous, CancellableByPlayer);
            if (!(interactionInstance is ITakeSimToWorkLocation takeSimToWorkLocation))
            {
                return false;
            }
            if (interactionInstance is TerrainInteraction terrainInteraction)
            {
                terrainInteraction.SetTargetPosition(mJob.Lot.GetEntraceTargetPoint());
                job.CloseNewJobNotification();
                job.OnSimActivatedJob(Actor);
                job.SetCurrentJobLocation();
            }
            if (!Actor.InteractionQueue.PushAsContinuation(interactionInstance, true) && Actor.LotCurrent != gameObject)
            {
                return false;
            }
            if (!(interactionInstance is TerrainInteraction))
            {
                job.CloseNewJobNotification();
                job.OnSimActivatedJob(Actor);
                job.SetCurrentJobLocation();
            }
            if (job.HasBeenSetup)
            {
                return true;
            }
            if (!SetupTasks(job))
            {
                Actor.InteractionQueue.RemoveInteractionByRef(interactionInstance);
                job.Cleanup();
                return false;
            }
            job.OnStartDisplayIntroductoryTns();
            Actor.Occupation.MoneyEarnedFromLastJob = 0;
            takeSimToWorkLocation.SetTakingSimToWork();
            return true;
        }
    }

    public class GhostHunterEx
    {
        private static readonly SimDescription.DeathType[] sValidDeathTypes = new SimDescription.DeathType[]
        {
            SimDescription.DeathType.OldAge,
            SimDescription.DeathType.Drown,
            SimDescription.DeathType.Starve,
            SimDescription.DeathType.Electrocution,
            SimDescription.DeathType.Burn,
            SimDescription.DeathType.MummyCurse,
            SimDescription.DeathType.Meteor,
            SimDescription.DeathType.Thirst,
            SimDescription.DeathType.WateryGrave,
            SimDescription.DeathType.HumanStatue,
            SimDescription.DeathType.Transmuted,
            SimDescription.DeathType.HauntingCurse,
            SimDescription.DeathType.JellyBeanDeath,
            SimDescription.DeathType.Freeze,
            SimDescription.DeathType.BluntForceTrauma,
            SimDescription.DeathType.Ranting,
            SimDescription.DeathType.Shark,
            SimDescription.DeathType.ScubaDrown,
            SimDescription.DeathType.MermaidDehydrated,
            SimDescription.DeathType.Causality,
            SimDescription.DeathType.Jetpack
        };

        private static bool CreateAngryGhost(GhostHunter.GhostHunterJob job, Vector3 position, out SimDescription ghost)
        {
            ghost = null;
            if (position == Vector3.Invalid)
            {
                IGameObject gameObject = job.CreateGhostJig();
                if (gameObject == null)
                {
                    return false;
                }
                position = gameObject.Position;
                gameObject.Destroy();
            }
            WorldName randomObjectFromList = RandomUtil.GetRandomObjectFromList(GhostHunter.GhostHunterJob.sValidHomeWorlds);
            IUrnstone urnstone = CreateObjectOutOfWorld("UrnstoneHuman") as IUrnstone;
            if (GameUtils.IsFutureWorld() && RandomUtil.CoinFlip())
            {
                ghost = OccultRobot.MakeRobot(CASAgeGenderFlags.Adult, RandomUtil.CoinFlip() ? CASAgeGenderFlags.Male : CASAgeGenderFlags.Female, RandomUtil.CoinFlip() ? RobotForms.Hovering : RobotForms.Humanoid);
                ghost.SetDeathStyle(SimDescription.DeathType.Robot, false);
            }
            else
            {
                ghost = Genetics.MakeSim(RandomUtil.CoinFlip() ? CASAgeGenderFlags.Adult : CASAgeGenderFlags.Elder, RandomUtil.CoinFlip() ? CASAgeGenderFlags.Male : CASAgeGenderFlags.Female, randomObjectFromList, 4294967295u);
                ghost.FirstName = SimUtils.GetRandomGivenName(ghost.IsMale, randomObjectFromList);
                ghost.LastName = SimUtils.GetRandomFamilyName(randomObjectFromList);
                ghost.SetDeathStyle(RandomUtil.GetRandomObjectFromList(sValidDeathTypes), false);
                TraitNames trait = ghost.DeathStyle switch
                {
                    SimDescription.DeathType.Drown          => TraitNames.Hydrophobic,
                    SimDescription.DeathType.Electrocution  => TraitNames.AntiTV,
                    SimDescription.DeathType.Burn           => TraitNames.Pyromaniac,
                    _                                       => TraitNames.Unknown
                };
                ghost.TraitManager.AddHiddenElement(trait);
            }
            List<TraitNames> list = new List<TraitNames>(GhostHunter.kAngryGhostTraits);
            while (!ghost.TraitManager.TraitsMaxed() && list.Count > 0)
            {
                TraitNames randomObjectFromList2 = RandomUtil.GetRandomObjectFromList(list);
                list.Remove(randomObjectFromList2);
                if (ghost.TraitManager.CanAddTrait((ulong)randomObjectFromList2))
                {
                    ghost.TraitManager.AddElement(randomObjectFromList2);
                }
            }
            urnstone.SetDeadSimDescription(ghost);
            if (!urnstone.GhostSpawn(false, position, job.Lot, true))
            {
                urnstone.Destroy();
                return false;
            }
            ghost.CreatedSim.Autonomy.AllowedToRunMetaAutonomy = false;
            ghost.CreatedSim.Autonomy.Motives.CreateMotive(CommodityKind.BeAngryGhost);
            ghost.CreatedSim.AddInteraction(GhostHunter.BanishWithGhostGun.Singleton);
            ghost.CreatedSim.AddSoloInteraction(GhostHunter.AngryHaunt.Singleton);
            ActiveTopic.AddToSim(ghost.CreatedSim, "Angry Ghost");
            Relationship relationship = ghost.GetRelationship(job.Occupation.OwnerDescription, true);
            relationship.LTR.UpdateLiking(GhostHunter.kAngryGhostRelationshipLevelWithGhostHunter);
            if (job.Lot.Household != null)
            {
                foreach (SimDescription current in job.Lot.Household.AllSimDescriptions)
                {
                    relationship = ghost.GetRelationship(current, true);
                    relationship.LTR.UpdateLiking(GhostHunter.kAngryGhostRelationshipLevelWithHousehold);
                }
            }
            if (ghost.IsHuman && RandomUtil.RandomChance01(GhostHunter.kAngryGhostAncientOutfitChance))
            {
                string name = string.Format("{0}{1}{2}", OutfitUtils.GetAgePrefix(ghost.Age), OutfitUtils.GetGenderPrefix(ghost.Gender), RandomUtil.GetRandomStringFromList(GhostHunter.GhostHunterJob.sAncientCasOutfits));
                SimOutfit uniform = new SimOutfit(ResourceKey.CreateOutfitKeyFromProductVersion(name, ProductVersion.EP2));
                if (OutfitUtils.TryApplyUniformToOutfit(ghost.GetOutfit(OutfitCategories.Everyday, 0), uniform, ghost, "GhostHunter.CreateAngryGhost", out SimOutfit outfit))
                {
                    ghost.AddOutfit(outfit, OutfitCategories.Everyday, true);
                    ghost.CreatedSim.SwitchToOutfitWithoutSpin(OutfitCategories.Everyday);
                }
            }
            job.mSims = job.mSims ?? new List<Sim>();
            job.mSims.Add(ghost.CreatedSim);
            job.AddToManagedObjectList((GameObject)urnstone);
            return true;
        }

        public class ScanForGhostsEx : GhostHunter.ScanForGhosts
        {
            new public class Definition : InteractionDefinition<Sim, Terrain, ScanForGhostsEx>
            {
                public override InteractionTestResult Test(ref InteractionInstanceParameters parameters, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    Lot lotAtPoint = LotManager.GetLotAtPoint(parameters.Hit.mPoint);
                    Sim sim = (Sim)parameters.Actor;
                    if (!(sim.Occupation is GhostHunter ghostHunter))
                    {
                        return InteractionTestResult.Def_TestFailed;
                    }
                    Job job = ghostHunter.CurrentJob as GhostHunter.GhostHunterJob;
                    if (job == null || job.Lot != lotAtPoint || job.Id != JobId.GhostlyPresence)
                    {
                        job = ghostHunter.FindJobForLot(lotAtPoint, true);
                        if (job == null || !job.HasBeenSetup || job.Id != JobId.GhostlyPresence)
                        {
                            return InteractionTestResult.Def_TestFailed;
                        }
                    }
                    Route route = sim.CreateRoute();
                    return !route.IsPointRoutable(parameters.Hit.mPoint)
                        ? InteractionTestResult.GenericFail
                        : !Terrain.GoHere.SharedGoHereTests(ref parameters)
                        ? InteractionTestResult.Def_TestFailed
                        : base.Test(ref parameters, ref greyedOutTooltipCallback);
                }

                public override bool Test(Sim actor, Terrain target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback) => true;
            }

            private bool UnhideGhostsInRoom(int roomId)
            {
                GhostHunter.GhostHunterJob job = Actor.SimDescription.OccupationAsActiveCareer.CurrentJob as GhostHunter.GhostHunterJob;
                if (job.mJigs.TryGetValue(roomId, out List<IGameObject> list))
                {
                    foreach (IGameObject current in list)
                    {
                        current.SetHiddenFlags(HiddenFlags.Footprint);
                        CreateAngryGhost(job, current.Position, out SimDescription simDescription);
                        simDescription.CreatedSim.SetOpacity(0f, 0f);
                        simDescription.CreatedSim.FadeIn(true, 2f);
                        job.RemoveFromManagedObjectList((GameObject)current);
                        current.Destroy();
                        Occupation.SimCompletedTask(job.Occupation.OwnerDescription.CreatedSim, TaskId.FindGhost, simDescription.CreatedSim);
                    }
                    job.mJigs.Remove(roomId);
                    return true;
                }
                return false;
            }

            public override bool Run()
            {
                if (!Actor.RouteToPoint(Destination))
                {
                    return false;
                }
                BeginCommodityUpdates();
                EnterStateMachine("GhostHunter", "Enter Scan", "x");
                AnimateSim("Scan");
                bool flag = DoTimedLoop(kTimeToScan);
                if (flag)
                {
                    if (Actor.OccupationAsActiveCareer.CurrentJob is GhostHunter.GhostHunterJob ghostHunterJob)
                    {
                        if (UnhideGhostsInRoom(Actor.RoomId))
                        {
                            Actor.ShowTNSIfSelectable(GhostHunter.LocalizeString("GhostsUnhidden"), NotificationStyle.kGameMessagePositive, ObjectGuid.InvalidObjectGuid);
                        }
                        else
                        {
                            string name = "HintDistanceMax";
                            float closestDistanceToHiddenGhost = ghostHunterJob.GetClosestDistanceToHiddenGhost(Actor);
                            for (int i = 0; i < kHintDistances.Length; i++)
                            {
                                if (closestDistanceToHiddenGhost <= kHintDistances[i])
                                {
                                    name = "HintDistance" + i;
                                    break;
                                }
                            }
                            Actor.ShowTNSIfSelectable(GhostHunter.LocalizeString(name), NotificationStyle.kGameMessagePositive, ObjectGuid.InvalidObjectGuid);
                        }
                    }
                }
                AnimateSim("Exit Scan");
                EndCommodityUpdates(flag);
                return flag;
            }
        }

        public class GoToJobEx : GhostHunter.GoToJob
        {
            [DoesntRequireTuning]
            new public class Definition : InteractionDefinition<Sim, Lot, GoToJobEx>
            {
                public override string GetInteractionName(Sim actor, Lot target, InteractionObjectPair iop) => string.Empty;

                public override bool Test(Sim actor, Lot target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback) => actor.OccupationAsActiveCareer is GhostHunter;
            }

            public bool SetupTask(GhostHunter.GhostHunterJob job, TaskId id) => id == TaskId.EvictGhost
                    ? CreateAngryGhost(job, Vector3.Invalid, out SimDescription _)
                    : job.SetupTask(id);

            public override void Init(ref InteractionInstanceParameters parameters)
            {
                base.Init(ref parameters);
                if (!(Actor.Occupation.FindJobForLot(Target, true) is GhostHunter.GhostHunterJob ghostHunterJob) || ghostHunterJob.HasBeenSetup)
                {
                    return;
                }
                TaskCreationStaticData[] staticTasks = ghostHunterJob.StaticTasks;
                if (staticTasks != null)
                {
                    TaskCreationStaticData[] array = staticTasks;
                    for (int i = 0; i < array.Length; i++)
                    {
                        TaskCreationStaticData taskCreationStaticData = array[i];
                        int @int = RandomUtil.GetInt(taskCreationStaticData.LowerBound, taskCreationStaticData.UpperBound);
                        for (int j = 0; j < @int; j++)
                        {
                            if (SetupTask(ghostHunterJob, taskCreationStaticData.Id))
                            {
                                ghostHunterJob.AddTask(taskCreationStaticData.Id);
                            }
                        }
                    }
                    if (ghostHunterJob.mTasks != null && ghostHunterJob.mPendingTasks == null)
                    {
                        ghostHunterJob.mPendingTasks = new List<TaskId>(ghostHunterJob.mTasks);
                    }
                }
                if (ghostHunterJob.mTasks == null || ghostHunterJob.mTasks.Count == 0)
                {
                    return;
                }
                CameraController.OnCameraMapViewEnabledCallback += new CameraController.CameraMapViewEnabledHandler(ghostHunterJob.OnCameraMapViewEnabledCallback);
                ghostHunterJob.StartMusic();
                ghostHunterJob.CreateFogEmitters();
                ghostHunterJob.EnableSpiritLighting();
                ghostHunterJob.Occupation.OwnerDescription.CreatedSim.Motives.CreateMotive(CommodityKind.BeAGhostHunter);
                AlarmHandle handle = AlarmManager.Global.AddAlarmRepeating(10f, TimeUnit.Minutes, new AlarmTimerCallback(ghostHunterJob.VerifyConsistency), 10f, TimeUnit.Minutes, "Ghost Hunter - Verify Consistency", AlarmType.AlwaysPersisted, ghostHunterJob);
                AlarmManager.Global.AlarmWillYield(handle);
                if (ghostHunterJob.Id == JobId.GhostlyPresence)
                {
                    GhostHunter.ScanForGhosts.NumberOfInstances++;
                    Terrain.Singleton.RemoveInteractionByType(GhostHunter.ScanForGhosts.Singleton);
                    Terrain.Singleton.AddInteraction(GhostHunter.ScanForGhosts.Singleton);
                }
                if (ghostHunterJob.Lot != null)
                {
                    ghostHunterJob.Lot.RefreshObjectCacheUsers();
                }
            }
        }
    }

    public class EvaluateRenovationEx : InteriorDesigner.EvaluateRenovation
    {
        [DoesntRequireTuning]
        new public class Definition : InteractionDefinition<Sim, Sim, EvaluateRenovationEx>
        {
            public override string GetInteractionName(ref InteractionInstanceParameters parameters) => InteriorDesigner.LocalizeString(parameters.Actor, "EvaluateRenovation");

            public override bool Test(Sim a, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback) => true;
        }

        public override bool ShouldDeliverSucceed()
        {
            if (IsLotInValidState())
            {
                InteriorDesigner.Renovation.ReportTone reportTone = Renovation.GenerateReport();
                return reportTone != InteriorDesigner.Renovation.ReportTone.Bad;
            }
            return false;
        }

        public override void PreExit()
        {
            if (VisitSituation.FindVisitSituationInvolvingGuest(Target) == null)
            {
                VisitSituation.Create(Target, Actor.LotHome);
            }
            if (Actor.HasExitReason())
            {
                return;
            }
            if (IsLotInValidState())
            {
                Renovation.ShowReportTns();
                Renovation.Complete();
                return;
            }
            Renovation.ShowInvalidLotTns();
        }

        private bool IsLotInValidState()
        {
            if (CountObjects<IProducesFood>(Renovation.Lot) < 1u)
            {
                return false;
            }
            if (CountObjects<IToilet>(Renovation.Lot) < 1u)
            {
                return false;
            }
            if (CountObjects<IShower>(Renovation.Lot) < 1u && CountObjects<IBathtub>(Renovation.Lot) < 1u && CountObjects<Sims3.Gameplay.Objects.Plumbing.SonicShower>(Renovation.Lot) < 1u)
            {
                return false;
            }
            int num = 0;
            bool requiresBotBed = false;
            bool hasBotBed = false;
            foreach (SimDescription current in Renovation.Lot.Household.SimDescriptions)
            {
                if (current.ChildOrAbove)
                {
                    num++;
                }
                if (current.IsEP11Bot)
                {
                    requiresBotBed = true;
                }
            }
            int num2 = 0;
            IBed[] objects = GetObjects<IBed>(Renovation.Lot);
            for (int i = 0; i < objects.Length; i++)
            {
                IBed bed = objects[i];
                if (bed is IBotBed)
                {
                    hasBotBed = true;
                }
                num2 += bed.NumberOfSpots();
            }
            return num2 >= num && (!requiresBotBed || hasBotBed);
        }
    }

    public class VisitLotAndWaitForDaycareGreetingEx : VisitLotAndWaitForDaycareGreeting
    {
        public class VisitLotAndWaitForDaycareGreetingDefinitionEx : VisitLotAndWaitForDaycareGreetingDefinition
        {
            public override InteractionInstance CreateInstance(ref InteractionInstanceParameters parameters)
            {
                InteractionInstance instance = new VisitLotAndWaitForDaycareGreetingEx();
                instance.Init(ref parameters);
                return instance;
            }
        }

        public override bool Run()
        {
            IgnoreOutdoorHosts = true;
            IgnoreCallbox = true;
            mMoveAwayfromDoor = false;
            if (!base.Run() && Actor.LotCurrent != Target)
            {
                return false;
            }
            if (!(DaycareSituation.GetDaycareSituationForSim(Actor) is DaycareTransportSituation daycareSituation))
            {
                return false;
            }
            if (!(daycareSituation.Daycare.OwnerSim is Sim ownerSim))
            {
                return false;
            }
            Sims3.Gameplay.ActorSystems.Children.CaregiverRoutingMonitor.CountBabiesToddlersAndCaregivers(ownerSim.Household, ownerSim.LotHome, null, out int num, out int num2, out int num3, out Sim sim, true);
            if (num2 > 0 || Actor.SimDescription.Child)
            {
                OnGreeted(daycareSituation);
            }
            else
            {
                Target.MoveAwayFromDoor(Actor, Target);
                ownerSim.ShowTNSIfSelectable(TNSNames.WhereIsTheDaycareCaregiverTNS, Actor, null, null, Actor.IsFemale, false);
                if (!Actor.InteractionQueue.TryPushAsContinuation(this, WaitForCaregiver.Singleton))
                {
                    return false;
                }
            }
            return true;
        }

        private static void OnGreeted(DaycareTransportSituation arriveSituation)
        {
            if (arriveSituation is DaycareChildTransportSituation)
            {
                Sim childSim = arriveSituation.ChildSim;
                VisitSituation.OnInvitedIn(childSim);
                childSim.SocialComponent.SetInvitedOver(arriveSituation.Daycare.OwnerDescription.LotHome);
                DaycareWorkdaySituation daycareWorkdaySituationForLot = DaycareWorkdaySituation.GetDaycareWorkdaySituationForLot(arriveSituation.Daycare.OwnerDescription.LotHome);
                if (daycareWorkdaySituationForLot != null)
                {
                    arriveSituation.RemovePerson(childSim);
                    AddChild(childSim, daycareWorkdaySituationForLot);
                    arriveSituation.AddDebugEventLogEntry("child arrive situation on greeted (added child to workday situation)");
                }
                else
                {
                    arriveSituation.AddDebugEventLogEntry("child arrive situation on greeted (workday situation was null, child not added)");
                }
                InteractionInstance interactionInstance = childSim.Autonomy.FindBestAction();
                if (interactionInstance != null)
                {
                    childSim.InteractionQueue.PushAsContinuation(interactionInstance, true);
                }
                arriveSituation.Exit();
            }
            else
            {
                arriveSituation.OnGreeted();
            }
        }

        private static void AddChild(Sim sim, DaycareWorkdaySituation situation)
        {
            sim.AssignRole(situation);
            situation.mSimDescIds.Add(sim.SimDescription.SimDescriptionId);
            situation.AddAgeUpListenerIfChild(sim.SimDescription);
            DaycareChildMonitor value = sim.SimDescription.Age == CASAgeGenderFlags.Toddler
                ? new ToddlerDaycareChildMonitor(sim, situation.Daycare, situation)
                : (DaycareChildMonitor)new ChildDaycareChildMonitorEx(sim, situation.Daycare, situation);
            situation.mChildMonitors.Add(sim.SimDescription.SimDescriptionId, value);
            if (Daycare.IsProblemChildStatic(sim.SimDescription.SimDescriptionId))
            {
                if (situation.Daycare.GetChildManager(sim.SimDescription.SimDescriptionId) is DaycareChildManagerProblemChild daycareChildManagerProblemChild)
                {
                    Sim ownerSim = situation.Daycare.OwnerSim;
                    SimDescription parentSimDesc = daycareChildManagerProblemChild.GetParentSimDesc();
                    if (daycareChildManagerProblemChild.FirstDaycareDay)
                    {
                        ownerSim.ShowTNSAndPlayStingIfSelectable(Daycare.LocalizeDaycareString("TnsMessage_FirstDayProblemChild", parentSimDesc, sim.SimDescription, ownerSim.SimDescription), NotificationStyle.kSystemMessage, parentSimDesc, ownerSim.SimDescription, string.Empty);
                        daycareChildManagerProblemChild.FirstDaycareDay = false;
                    }
                }
            }
        }
    }

    public class FinishGiveFashionAdviceEx : Stylist.FinishGiveFashionAdvice
    {
        new public class Definition : InteractionDefinition<Sim, Sim, FinishGiveFashionAdviceEx>
        {
            public override string GetInteractionName(Sim actor, Sim target, InteractionObjectPair iop) => Localization.LocalizeString(actor.IsFemale, "Gameplay/Excel/Socializing/Action:GiveFashionAdvice");

            public override bool Test(Sim a, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback) => true;
        }

        public override bool Run()
        {
            Actor.LoopIdle();
            BeginCommodityUpdates();
            EnterStateMachine("StylistActiveCareer", "Enter", "y");
            DoLoop(ExitReason.Default, new InsideLoopFunction(StylistLoop), mCurrentStateMachine);
            SkillLevel stylerReactionType = Styling.GetStylerReactionType(StyleeReactionType);
            SetParameter("stylistReactionType", stylerReactionType);
            if (Actor.OnlyHasExitReason(ExitReason.Finished))
            {
                OnFashionInteractionFinishing();
                Animate("y", "Stylist Reaction");
                Animate("y", "Exit");
            }
            EndCommodityUpdates(true);
            return true;
        }

        public override void OnFashionInteractionFinishing()
        {
            base.OnFashionInteractionFinishing();
            EventTracker.SendEvent(EventTypeId.kGaveFashionAdvice, Actor, Target);
        }
    }

    public class AskToJoinPerformanceCareerEx : Proprietor.AskToJoinPerformanceCareer
    {
        new public class Definition : SocialInteractionA.Definition
        {
            public const string kProprietorJoinCareerInteractionsPathKey = "Gameplay/Roles/RoleProprietor:JoinCareer";

            public Definition(string text, string path) : base(text, new string[] { path }, null, false)
            {
            }

            public override void AddInteractions(InteractionObjectPair iop, Sim actor, Sim target, List<InteractionObjectPair> results)
            {
                string path = Localization.LocalizeString("Gameplay/Roles/RoleProprietor:JoinCareer") + Localization.Ellipsis;
                results.Add(new InteractionObjectPair(new Definition("Ask To Join Singer Career", path), iop.Target));
                results.Add(new InteractionObjectPair(new Definition("Ask To Join PerformanceArtist Career", path), iop.Target));
                results.Add(new InteractionObjectPair(new Definition("Ask To Join Magician Career", path), iop.Target));
            }

            public override bool Test(Sim a, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                OccupationNames occupation = ActionKey switch
                {
                    "Ask To Join Singer Career"             => OccupationNames.SingerCareer,
                    "Ask To Join PerformanceArtist Career"  => OccupationNames.PerformanceArtistCareer,
                    "Ask To Join Magician Career"           => OccupationNames.MagicianCareer,
                    _                                       => OccupationNames.Undefined
                };
                return a.Posture.AllowsNormalSocials() && target.Posture.AllowsNormalSocials() && !a.NeedsToBeGreeted(target) && target.SimDescription.AssignedRole is Proprietor && TestApplyForProfession(a, occupation, ref greyedOutTooltipCallback) && base.Test(a, target, isAutonomous, ref greyedOutTooltipCallback);
            }
        }

        public static void OnRequestFinish(Sim actor, Sim _, string interaction, ActiveTopic __, InteractionInstance ___)
        {
            OccupationNames occupationNames = OccupationNames.Undefined;
            if (interaction.Equals("Ask To Join Magician Career"))
            {
                occupationNames = OccupationNames.MagicianCareer;
            }
            else if (interaction.Equals("Ask To Join PerformanceArtist Career"))
            {
                occupationNames = OccupationNames.PerformanceArtistCareer;
            }
            else if (interaction.Equals("Ask To Join Singer Career"))
            {
                occupationNames = OccupationNames.SingerCareer;
            }
            if (ActiveCareer.CanAddActiveCareer(actor.SimDescription, occupationNames))
            {
                Occupation occupation = CareerManager.GetStaticOccupation(occupationNames);
                OfferJob(actor, new OccupationEntryTuple(occupation, null));
            }
        }
    }

    public class JoinActiveCareerStylistSocialEx : Styling.StylistRole.JoinActiveCareerStylistSocial
    {
        new public class Definition : SocialInteractionA.Definition
        {
            public Definition(string text) : base(text, new string[0], null, false)
            {
            }

            public override bool Test(Sim a, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback) 
                => a.Posture.AllowsNormalSocials() && target.Posture.AllowsNormalSocials() && target.SimDescription.HasActiveRole && target.SimDescription.AssignedRole is Styling.StylistRole
                    && TestApplyForProfession(a, OccupationNames.Stylist, ref greyedOutTooltipCallback) && base.Test(a, target, isAutonomous, ref greyedOutTooltipCallback);
        }

        public static void OnRequestFinish(Sim actor, Sim _, string __, ActiveTopic ___, InteractionInstance ____)
        {
            Occupation occupation = CareerManager.GetStaticOccupation(OccupationNames.Stylist);
            OfferJob(actor, new OccupationEntryTuple(occupation, null));
        }
    }

    public class JoinStylistActiveCareerEx : JoinStylistActiveCareer
    {
        new public class Definition : InteractionDefinition<Sim, Lot, JoinStylistActiveCareerEx>
        {
            public override string GetInteractionName(Sim actor, Lot target, InteractionObjectPair iop) => Localization.LocalizeString("Gameplay/Excel/Socializing/Action:JoinStylistActiveCareer");

            public override bool Test(Sim a, Lot target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback) => target.CommercialLotSubType == CommercialLotSubType.kEP2_Salon && TestApplyForProfession(a, OccupationNames.Stylist, ref greyedOutTooltipCallback);
        }

        public override bool Run()
        {
            if (Actor.LotCurrent != Target)
            {
                InteractionInstance interactionInstance = VisitCommunityLot.Singleton.CreateInstance(Target, Actor, GetPriority(), Autonomous, CancellableByPlayer);
                if (!interactionInstance.RunInteraction())
                {
                    return false;
                }
            }
            if (Actor.HasExitReason())
            {
                return false;
            }
            if (Actor.LotCurrent == Target)
            {
                Sim npc = GetNpc();
                if (npc != null)
                {
                    InteractionInstance instance = Styling.StylistRole.JoinActiveCareerStylistSocial.Singleton.CreateInstance(npc, Actor, GetPriority(), Autonomous, CancellableByPlayer);
                    if (Actor.InteractionQueue.PushAsContinuation(instance, true))
                    {
                        return true;
                    }
                }
            }
            if (!(Settings.EnableJoinProfessionInRabbitHoleOrLot && Stylist.IsStylistActiveCareerAvailable()))
            {
                return false;
            }
            Occupation occupation = CareerManager.GetStaticOccupation(OccupationName);
            OfferJob(Actor, new OccupationEntryTuple(occupation, null));
            return true;
        }
    }

    public class JoinFirefighterActiveCareerEx : JoinFirefighterActiveCareer
    {
        new public class Definition : InteractionDefinition<Sim, Lot, JoinFirefighterActiveCareerEx>
        {
            public override string GetInteractionName(Sim actor, Lot target, InteractionObjectPair iop) => Localization.LocalizeString(actor.IsFemale, "Gameplay/Excel/Socializing/Action:JoinFirefighterActiveCareer");

            public override bool Test(Sim a, Lot target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback) => target.CommercialLotSubType == CommercialLotSubType.kEP2_FireStation && TestApplyForProfession(a, OccupationNames.Firefighter, ref greyedOutTooltipCallback);
        }

        public override bool Run()
        {
            if (Actor.LotCurrent != Target)
            {
                InteractionInstance interactionInstance = VisitCommunityLot.Singleton.CreateInstance(Target, Actor, GetPriority(), Autonomous, CancellableByPlayer);
                if (!interactionInstance.RunInteraction())
                {
                    return false;
                }
            }
            if (Actor.HasExitReason())
            {
                return false;
            }
            if (!(Settings.EnableJoinProfessionInRabbitHoleOrLot && ActiveFireFighter.IsFireFighterActiveCareerAvailable()))
            {
                return false;
            }
            Occupation occupation = CareerManager.GetStaticOccupation(OccupationName);
            OfferJob(Actor, new OccupationEntryTuple(occupation, null));
            return true;
        }
    }

    public class JoinActiveCareerDaycare : RabbitHole.ModalDialogRabbitHoleInteraction<CityHall>
    {
        public class Definition : InteractionDefinition<Sim, CityHall, JoinActiveCareerDaycare>
        {
            public override bool Test(Sim a, CityHall target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback) => TestApplyForProfession(a, OccupationNames.Daycare, ref greyedOutTooltipCallback);

            public override string GetInteractionName(Sim actor, CityHall target, InteractionObjectPair iop) => LocalizeString("JoinActiveCareerDaycareName");
        }

        public static InteractionDefinition Singleton = new Definition();

        public override bool InRabbitHole()
        {
            TryDisablingCameraFollow(Actor);
            Occupation occupation = CareerManager.GetStaticOccupation(OccupationNames.Daycare);
            OfferJob(Actor, new OccupationEntryTuple(occupation, null));
            return true;
        }

        public override ThumbnailKey GetIconKey() => new ThumbnailKey(new ResourceKey(ResourceUtils.HashString64("w_daycare_career_large"), 796721156u, ResourceUtils.ProductVersionToGroupId(ProductVersion.BaseGame)), ThumbnailSize.Medium);
    }

    public class JoinActiveCareerPrivateEyeEx : PoliceStation.JoinActiveCareerPrivateEye
    {
        new public class Definition : InteractionDefinition<Sim, PoliceStation, JoinActiveCareerPrivateEyeEx>
        {
            public override bool Test(Sim a, PoliceStation target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback) => TestApplyForProfession(a, OccupationNames.PrivateEye, ref greyedOutTooltipCallback);
        }

        public override bool InRabbitHole()
        {
            TryDisablingCameraFollow(Actor);
            Occupation occupation = CareerManager.GetStaticOccupation(OccupationNames.PrivateEye);
            OfferJob(Actor, new OccupationEntryTuple(occupation, null));
            return true;
        }
    }

    public class JoinActiveCareerGhostHunterEx : ScienceLab.JoinActiveCareerGhostHunter
    {
        new public class Definition : InteractionDefinition<Sim, ScienceLab, JoinActiveCareerGhostHunterEx>
        {
            public override bool Test(Sim a, ScienceLab target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback) => TestApplyForProfession(a, OccupationNames.GhostHunter, ref greyedOutTooltipCallback);
        }

        public override bool InRabbitHole()
        {
            TryDisablingCameraFollow(Actor);
            Occupation occupation = CareerManager.GetStaticOccupation(OccupationNames.GhostHunter);
            OfferJob(Actor, new OccupationEntryTuple(occupation, null));
            return true;
        }
    }

    public class JoinActiveCareerInteriorDesignerEx : CityHall.JoinActiveCareerInteriorDesigner
    {
        new public class Definition : InteractionDefinition<Sim, CityHall, JoinActiveCareerInteriorDesignerEx>
        {
            public override bool Test(Sim a, CityHall target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback) => TestApplyForProfession(a, OccupationNames.InteriorDesigner, ref greyedOutTooltipCallback);
        }

        public override bool InRabbitHole()
        {
            TryDisablingCameraFollow(Actor);
            Occupation occupation = CareerManager.GetStaticOccupation(OccupationNames.InteriorDesigner);
            OfferJob(Actor, new OccupationEntryTuple(occupation, null));
            return true;
        }
    }

    public class JoinActiveCareerLifeguardEx : CityHall.JoinActiveCareerLifeguard
    {
        new public class Definition : InteractionDefinition<Sim, CityHall, JoinActiveCareerLifeguardEx>
        {
            public override bool Test(Sim a, CityHall target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback) => TestApplyForProfession(a, OccupationNames.Lifeguard, ref greyedOutTooltipCallback);
        }

        public override bool InRabbitHole()
        {
            TryDisablingCameraFollow(Actor);
            Occupation occupation = CareerManager.GetStaticOccupation(OccupationNames.Lifeguard);
            OfferJob(Actor, new OccupationEntryTuple(occupation, null));
            return true;
        }
    }

    public class GetJobInRabbitHoleEx : GetJobInRabbitHole
    {
        new public class Definition : InteractionDefinition<Sim, RabbitHole, GetJobInRabbitHoleEx>
        {
            public string mName = string.Empty;

            public OccupationNames CareerGuid;

            public Definition()
            {
            }

            public Definition(string s, CareerLocation careerLoc)
            {
                mName = s;
                CareerGuid = careerLoc.Career.Guid;
            }

            public override void AddInteractions(InteractionObjectPair iop, Sim actor, RabbitHole target, List<InteractionObjectPair> results)
            {
                if (actor == null)
                {
                    return;
                }
                foreach (CareerLocation current in target.CareerLocations.Values)
                {
                    if (actor.Occupation == null || (actor.Occupation.CareerLoc != current && actor.Occupation.Guid != current.Career.Guid))
                    {
                        results.Add(new InteractionObjectPair(new Definition(LocalizeString(actor.IsFemale, "JoinCareer" + current.Career.Guid.ToString(), new object[]
                        {
                            current.Career.Name
                        }), current), target));
                    }
                    else if (actor.Occupation.CareerLoc != current && actor.Occupation.Guid == current.Career.Guid)
                    {
                        results.Add(new InteractionObjectPair(new Definition(LocalizeString(actor.IsFemale, "TransferJob"), current), target));
                    }
                }
            }

            public override string GetInteractionName(Sim a, RabbitHole target, InteractionObjectPair interaction) => mName;

            public override bool Test(Sim a, RabbitHole target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                SimDescription simDescription = a.SimDescription;
                if (simDescription.IsEnrolledInBoardingSchool())
                {
                    return false;
                }
                CareerLocation careerLocation = GetCareerLocation(target, CareerGuid);
                Career career = careerLocation.Career;
                if (career is School)
                {
                    return career is SchoolElementary
                        ? simDescription.Child && a.School == null
                        : career is SchoolHigh && simDescription.Teen && a.School == null;
                }
                else
                {
                    GreyedOutTooltipCallback greyedOutTooltipCallback2 = CreateTooltipCallback("NOT USED");
                    Settings.InterviewSettings.TryGetValue(career.SharedData.Name.Substring(34), out InterviewSettings interviewSettings);
                    Settings.CareerAvailabilitySettings.TryGetValue(career.SharedData.Name.Substring(34), out CareerAvailabilitySettings availabilitySettings);
                    if (interviewSettings == null || availabilitySettings == null || !Settings.EnableGetJobInRabbitHole || !availabilitySettings.IsAvailable || interviewSettings.RequiresInterview || !career.CanAcceptCareer(a.ObjectId, ref greyedOutTooltipCallback2))
                    {
                        return false;
                    }
                    if (careerLocation == null || !career.CareerAgeTest(simDescription))
                    {
                        return false;
                    }
                    if (simDescription.Teen && AfterschoolActivity.DoesJobConflictWithActivities(a, career))
                    {
                        return false;
                    }
                    if (GameUtils.IsInstalled(ProductVersion.EP9) && availabilitySettings.RequiredDegrees.Count > 0)
                    {
                        foreach (AcademicDegreeNames degree in availabilitySettings.RequiredDegrees)
                        {
                            if (a.DegreeManager == null || !a.DegreeManager.HasCompletedDegree(degree))
                            {
                                greyedOutTooltipCallback = CreateTooltipCallback(Helpers.Methods.LocalizeString(a.IsFemale, "DoesNotHaveRequiredDegrees"));
                                return false;
                            }
                        }
                    }
                    Career occupationAsCareer = a.OccupationAsCareer;
                    return occupationAsCareer == null || occupationAsCareer.CareerLoc != careerLocation;
                }
            }
        }

        public override bool InRabbitHole()
        {
            CareerLocation careerLocation = GetCareerLocation(Target, (InteractionDefinition as Definition).CareerGuid);
            if (careerLocation == null)
            {
                return false;
            }
            TryDisablingCameraFollow(Actor);
            OfferJob(Actor, new OccupationEntryTuple(careerLocation.Career, careerLocation));
            return true;
        }
    }

    public class AttendResumeAndInterviewClass : CollegeOfBusiness.AttendResumeWritingAndInterviewTechniquesClass
    {
        new public class Definition : InteractionDefinition<Sim, RabbitHole, AttendResumeAndInterviewClass>
        {
            public override bool Test(Sim a, RabbitHole target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback) => a.FamilyFunds >= CollegeOfBusiness.kCostOfResumeInterviewClass && SimClock.IsTimeBetweenTimes(kStartAvailibilityTime, kEndAvailibilityTime);

            public override string GetInteractionName(ref InteractionInstanceParameters parameters) 
                => Localization.LocalizeString("Gameplay/Abstracts/ScriptObject/AttendResumeWritingAndInterviewTechniquesClass:InteractionName", new object[] { CollegeOfBusiness.kCostOfResumeInterviewClass });
        }

        public override bool InRabbitHole()
        {
            Actor.ModifyFunds(-CollegeOfBusiness.kCostOfResumeInterviewClass);
            StartStages();
            Actor.SkillManager.AddElement(SkillNames.Logic);
            Actor.SkillManager.AddElement(SkillNames.Charisma);
            BeginCommodityUpdates();
            bool flag = DoLoop(ExitReason.Default);
            EndCommodityUpdates(flag);
            if (flag)
            {
                Actor.BuffManager.AddElement(kReadyForInterviewGuid, Target is CollegeOfBusiness ? Origin.FromCollegeOfBusinessRabbitHole : Origin.FromSchool);
                Actor.ShowTNSIfSelectable(CollegeOfBusiness.LocalizeString(Actor.IsFemale, "InterviewResume", new object[]
                {
                    Actor
                }), NotificationStyle.kGameMessagePositive);
            }
            return flag;
        }
    }

    public class CallToCancelSteadyGigEx : Phone.CallToCancelSteadyGig
    {
        new public class Definition : CallDefinition<CallToCancelSteadyGigEx>
        {
            public override string[] GetPath(bool isFemale) => new string[] { Phone.LocalizeString("JobsAndOffers") + Localization.Ellipsis };

            public override bool Test(Sim a, Phone target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback) => target.IsUsableBy(a) && a.OccupationAsPerformanceCareer != null && a.OccupationAsPerformanceCareer.GetSteadyGigProprietors().Count != 0 
                && base.Test(a, target, isAutonomous, ref greyedOutTooltipCallback);

            public override void PopulatePieMenuPicker(ref InteractionInstanceParameters parameters, out List<TabInfo> listObjs, out List<HeaderInfo> headers, out int NumSelectableRows)
            {
                NumSelectableRows = 1;
                Sim sim = parameters.Actor as Sim;
                List<Sim> steadyGigProprietors = sim.OccupationAsPerformanceCareer.GetSteadyGigProprietors();
                PopulateSimPicker(ref parameters, out listObjs, out headers, steadyGigProprietors, false);
            }
        }
    }

    public class FindJobComputer : Computer.ComputerInteraction
    {
        public class Definition : InteractionDefinition<Sim, Computer, FindJobComputer>
        {
            public override bool Test(Sim actor, Computer target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                if (target.IsComputerUsable(actor, true, false, isAutonomous))
                {
                    if (FindJobTest(actor, true))
                    {
                        return false;
                    }
                    foreach (InterviewData data in InterviewList)
                    {
                        if (data.ActorId == actor.SimDescription.SimDescriptionId)
                        {
                            greyedOutTooltipCallback = CreateTooltipCallback(LocalizeString(actor.IsFemale, "AlreadyHaveInterview"));
                            return false;
                        }
                    }
                    if (actor.SimDescription.IsEP11Bot && actor.TraitManager != null && !actor.TraitManager.HasElement(TraitNames.ProfessionalChip))
                    {
                        greyedOutTooltipCallback = CreateTooltipCallback(Localization.LocalizeString("Ui/Tooltip/HUD/Navigation:RobotCareerDisallowed"));
                    }
                    else if (FindJobTest(actor, false))
                    {
                        return true;
                    }
                }
                return false;
            }

            public override string[] GetPath(bool isFemale) => new string[] { Computer.LocalizeString("JobsAndProfessions") };

            public override string GetInteractionName(Sim actor, Computer target, InteractionObjectPair iop) => LocalizeString("FindJobComputerName");
        }

        private AlarmHandle EventAlarm = AlarmHandle.kInvalidHandle;

        private List<OccupationEntryTuple> OccupationEntries = new List<OccupationEntryTuple>();

        public override bool Run()
        {
            StandardEntry();
            if (!Target.StartComputing(this, SurfaceHeight.Table, true))
            {
                StandardExit();
                return false;
            }
            Target.StartVideo(Computer.VideoType.Browse);
            AnimateSim("GenericTyping");
            OccupationEntries = GetRandomJobs(Actor, Target.ComputerTuning.FindJobNumJobOpportunies, false, RandomComputerSeed + SimClock.ElapsedCalendarDays());
            EventAlarm = AlarmManager.Global.AddAlarm(Target.ComputerTuning.FindJobNumMinutesBetweenOffer, TimeUnit.Minutes, new AlarmTimerCallback(FindJobAlarmCallback), "Gamefreak130 wuz here", AlarmType.AlwaysPersisted, Actor);
            DoLoop(ExitReason.Default);
            Target.StopComputing(this, Computer.StopComputingAction.TurnOff, false);
            StandardExit();
            return true;
        }

        private void FindJobAlarmCallback()
        {
            if (OccupationEntries.Count == 0)
            {
                Show(new Format(LocalizeString("JobsExhausted"), NotificationStyle.kGameMessagePositive));
                Actor.AddExitReason(ExitReason.Finished);
                return;
            }
            if (Settings.InstantGratification || (Settings.HoloComputerInstantGratification && Target is HoloComputer))
            {
                UI.CareerSelectionModelEx.Singleton.ShowCareerSelection(Actor, Target.ObjectId, OccupationEntries, false);
                Actor.AddExitReason(ExitReason.Finished);
                return;
            }
            List<OccupationEntryTuple> list = new List<OccupationEntryTuple>(1);
            OccupationEntryTuple entry = OccupationEntries[0];
            list.Add(entry);
            OccupationEntries.Remove(entry);
            if (UI.CareerSelectionModelEx.Singleton.ShowCareerSelection(Actor, Target.ObjectId, list, false))
            {
                Actor.AddExitReason(ExitReason.Finished);
                return;
            }
            AlarmManager.Global.RemoveAlarm(EventAlarm);
            EventAlarm = AlarmManager.Global.AddAlarm(Target.ComputerTuning.FindJobNumMinutesBetweenOffer, TimeUnit.Minutes, new AlarmTimerCallback(FindJobAlarmCallback), "Gamefreak130 wuz here", AlarmType.AlwaysPersisted, Actor);
        }

        public override void Cleanup()
        {
            base.Cleanup();
            AlarmManager.Global.RemoveAlarm(EventAlarm);
        }
    }

    public class FindJobNewspaperEx : HeldNewspaperInteraction
    {
        public class Definition : InteractionDefinition<Sim, Newspaper, FindJobNewspaperEx>
        {
            public override bool Test(Sim actor, Newspaper target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                if (target.IsReadable)
                {
                    foreach (InterviewData data in InterviewList)
                    {
                        if (data.ActorId == actor.SimDescription.SimDescriptionId)
                        {
                            greyedOutTooltipCallback = CreateTooltipCallback(LocalizeString(actor.IsFemale, "AlreadyHaveInterview", new object[]
                            {
                                actor
                            }));
                            return false;
                        }
                    }
                    if (actor.SimDescription.IsEP11Bot && actor.TraitManager != null && !actor.TraitManager.HasElement(TraitNames.ProfessionalChip))
                    {
                        greyedOutTooltipCallback = CreateTooltipCallback(Localization.LocalizeString("Ui/Tooltip/HUD/Navigation:RobotCareerDisallowed"));
                    }
                    else if (FindJobTest(actor, false))
                    {
                        return true;
                    }
                }
                return false;
            }

            public override string[] GetPath(bool isFemale) => new string[] { Computer.LocalizeString("JobsAndProfessions") };

            public override string GetInteractionName(Sim a, Newspaper target, InteractionObjectPair interaction) => LocalizeString("FindJobNewspaperName");
        }

        private AlarmHandle EventAlarm = AlarmHandle.kInvalidHandle;

        private List<OccupationEntryTuple> OccupationEntries = new List<OccupationEntryTuple>();

        internal bool mFromInventory;

        public override bool Run()
        {
            mFromInventory = false;
            return DoFindJob();
        }

        public override bool RunFromInventory()
        {
            mFromInventory = true;
            return DoFindJob();
        }

        private bool DoFindJob()
        {
            AcquireStateMachine("ReadNewspaper");
            if (!Target.StartUsingNewspaper(mFromInventory, false, Actor, mCurrentStateMachine))
            {
                CarrySystem.PutDownOnFloor(Actor);
                return false;
            }
            SetActor("newspaper", Target);
            SetActor("x", Actor);
            AnimateSim("ReadNewspaper");
            OccupationEntries = GetRandomJobs(Actor, Newspaper.FindJobNumJobsOpportunitiesPerDay, false, RandomNewspaperSeeds[Target.ObjectId]);
            EventAlarm = AlarmManager.Global.AddAlarm(Newspaper.FindJobNumMinutesBetweenOffer, TimeUnit.Minutes, new AlarmTimerCallback(FindJobAlarmCallback), "Gamefreak130 wuz here", AlarmType.AlwaysPersisted, Actor);
            DoLoop(ExitReason.Default);
            return Target.StopUsingNewspaper(Actor, mCurrentStateMachine, mFromInventory);
        }

        private void FindJobAlarmCallback()
        {
            if (OccupationEntries.Count == 0)
            {
                Show(new Format(LocalizeString("JobsExhausted"), NotificationStyle.kGameMessagePositive));
                Target.StopUsingNewspaper(Actor, mCurrentStateMachine, mFromInventory);
                Actor.AddExitReason(ExitReason.Finished);
                return;
            }
            if (Settings.InstantGratification)
            {
                if (!UI.CareerSelectionModelEx.Singleton.ShowCareerSelection(Actor, Target.ObjectId, OccupationEntries, false))
                {
                    Target.StopUsingNewspaper(Actor, mCurrentStateMachine, mFromInventory);
                }
                Actor.AddExitReason(ExitReason.Finished);
                return;
            }
            List<OccupationEntryTuple> list = new List<OccupationEntryTuple>(1);
            OccupationEntryTuple entry = OccupationEntries[0];
            list.Add(entry);
            OccupationEntries.Remove(entry);
            if (UI.CareerSelectionModelEx.Singleton.ShowCareerSelection(Actor, Target.ObjectId, list, false))
            {
                Actor.AddExitReason(ExitReason.Finished);
                return;
            }
            AlarmManager.Global.RemoveAlarm(EventAlarm);
            EventAlarm = AlarmManager.Global.AddAlarm(Newspaper.FindJobNumMinutesBetweenOffer, TimeUnit.Minutes, new AlarmTimerCallback(FindJobAlarmCallback), "Gamefreak130 wuz here", AlarmType.AlwaysPersisted, Actor);
        }

        public override void Cleanup()
        {
            base.Cleanup();
            AlarmManager.Global.RemoveAlarm(EventAlarm);
        }

        public override void PostureTransitionFailed(bool transitionExitResult) => Target.PutNewspaperAway(Actor, true);
    }

    public class UploadResumeComputer : Computer.ComputerInteraction
    {
        public class Definition : InteractionDefinition<Sim, Computer, UploadResumeComputer>
        {
            public override bool Test(Sim actor, Computer target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                if (target.IsComputerUsable(actor, true, false, isAutonomous))
                {
                    foreach (InterviewData data in InterviewList)
                    {
                        if (data.ActorId == actor.SimDescription.SimDescriptionId)
                        {
                            greyedOutTooltipCallback = CreateTooltipCallback(LocalizeString(actor.IsFemale, "AlreadyHaveInterview", new object[]
                            {
                                actor
                            }));
                            return false;
                        }
                    }
                    bool flag = false;
                    if (actor.SimDescription.IsEP11Bot && actor.TraitManager != null && !actor.TraitManager.HasElement(TraitNames.ProfessionalChip))
                    {
                        greyedOutTooltipCallback = CreateTooltipCallback(Localization.LocalizeString("Ui/Tooltip/HUD/Navigation:RobotCareerDisallowed"));
                        return false;
                    }
                    if (FindJobTest(actor, true, ref flag))
                    {
                        return true;
                    }
                    if (!flag)
                    {
                        greyedOutTooltipCallback = CreateTooltipCallback(Phone.UploadResume.LocalizeString("NoDegreeOrEnoughFollower"));
                    }
                }
                return false;
            }

            public override string[] GetPath(bool isFemale) => new string[] { Computer.LocalizeString("JobsAndProfessions") };
        }

        private AlarmHandle EventAlarm = AlarmHandle.kInvalidHandle;

        private List<OccupationEntryTuple> OccupationEntries = new List<OccupationEntryTuple>();

        public override bool Run()
        {
            StandardEntry();
            if (!Target.StartComputing(this, SurfaceHeight.Table, true))
            {
                StandardExit();
                return false;
            }
            Target.StartVideo(Computer.VideoType.Browse);
            AnimateSim("GenericTyping");
            OccupationEntries = GetRandomJobs(Actor, Target.ComputerTuning.FindJobNumJobOpportunies, true, RandomComputerSeed + SimClock.ElapsedCalendarDays());
            EventAlarm = AlarmManager.Global.AddAlarm(Target.ComputerTuning.FindJobNumMinutesBetweenOffer, TimeUnit.Minutes, new AlarmTimerCallback(FindJobAlarmCallback), "Gamefreak130 wuz here", AlarmType.AlwaysPersisted, Actor);
            DoLoop(ExitReason.Default);
            Target.StopComputing(this, Computer.StopComputingAction.TurnOff, false);
            StandardExit();
            return true;
        }

        private void FindJobAlarmCallback()
        {
            if (OccupationEntries.Count == 0)
            {
                Show(new Format(LocalizeString("JobsExhausted"), NotificationStyle.kGameMessagePositive));
                Actor.AddExitReason(ExitReason.Finished);
                return;
            }
            if (Settings.InstantGratification || (Settings.HoloComputerInstantGratification && Target is HoloComputer))
            {
                UI.CareerSelectionModelEx.Singleton.ShowCareerSelection(Actor, Target.ObjectId, OccupationEntries, true);
                Actor.AddExitReason(ExitReason.Finished);
                return;
            }
            List<OccupationEntryTuple> list = new List<OccupationEntryTuple>(1);
            OccupationEntryTuple entry = OccupationEntries[0];
            list.Add(entry);
            OccupationEntries.Remove(entry);
            if (UI.CareerSelectionModelEx.Singleton.ShowCareerSelection(Actor, Target.ObjectId, list, false))
            {
                Actor.AddExitReason(ExitReason.Finished);
                return;
            }
            AlarmManager.Global.RemoveAlarm(EventAlarm);
            EventAlarm = AlarmManager.Global.AddAlarm(Target.ComputerTuning.FindJobNumMinutesBetweenOffer, TimeUnit.Minutes, new AlarmTimerCallback(FindJobAlarmCallback), "Gamefreak130 wuz here", AlarmType.AlwaysPersisted, Actor);
        }

        public override void Cleanup()
        {
            base.Cleanup();
            AlarmManager.Global.RemoveAlarm(EventAlarm);
        }
    }

    public class UploadResumePhone : Interaction<Sim, PhoneSmart>
    {
        public class Definition : InteractionDefinition<Sim, PhoneSmart, UploadResumePhone>
        {
            public override bool Test(Sim actor, PhoneSmart target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                if (target.IsUsableBy(actor) && actor.Inventory.Find<PhoneSmart>() != null)
                {
                    foreach (InterviewData data in InterviewList)
                    {
                        if (data.ActorId == actor.SimDescription.SimDescriptionId)
                        {
                            greyedOutTooltipCallback = CreateTooltipCallback(LocalizeString(actor.IsFemale, "AlreadyHaveInterview", new object[]
                            {
                                actor
                            }));
                            return false;
                        }
                    }
                    bool flag = false;
                    if (actor.SimDescription.IsEP11Bot && actor.TraitManager != null && !actor.TraitManager.HasElement(TraitNames.ProfessionalChip))
                    {
                        greyedOutTooltipCallback = CreateTooltipCallback(Localization.LocalizeString("Ui/Tooltip/HUD/Navigation:RobotCareerDisallowed"));
                        return false;
                    }
                    if (FindJobTest(actor, true, ref flag))
                    {
                        return true;
                    }
                    if (!flag)
                    {
                        greyedOutTooltipCallback = CreateTooltipCallback(Phone.UploadResume.LocalizeString("NoDegreeOrEnoughFollower"));
                    }
                }
                return false;
            }

            public override string[] GetPath(bool isFemale) => new string[] { Phone.LocalizeString("JobsAndOffers") + Localization.Ellipsis };
        }

        private AlarmHandle EventAlarm = AlarmHandle.kInvalidHandle;

        private List<OccupationEntryTuple> OccupationEntries = new List<OccupationEntryTuple>();

        public override bool RunFromInventory() => Run();

        public override bool Run()
        {
            IGameObject gameObject = null;
            StandardEntry();
            Target.StartUsingSmartPhone(Actor, ref gameObject, this);
            AnimateSim("BrowseTheWeb");
            BeginCommodityUpdates();
            OccupationEntries = GetRandomJobs(Actor, Phone.UploadResume.FindJobNumJobOpportunies, true, RandomComputerSeed + SimClock.ElapsedCalendarDays());
            EventAlarm = AlarmManager.Global.AddAlarm(Phone.UploadResume.FindJobNumMinutesBetweenOffer, TimeUnit.Minutes, new AlarmTimerCallback(FindJobAlarmCallback), "Gamefreak130 wuz here", AlarmType.AlwaysPersisted, Actor);
            DoLoop(ExitReason.Default);
            EndCommodityUpdates(true);
            AnimateSim("NormalExit");
            Target.RemoveHandObject();
            StandardExit();
            return true;
        }

        private void FindJobAlarmCallback()
        {
            if (OccupationEntries.Count == 0)
            {
                Show(new Format(LocalizeString("JobsExhausted"), NotificationStyle.kGameMessagePositive));
                Actor.AddExitReason(ExitReason.Finished);
                return;
            }
            if (Settings.InstantGratification || (Settings.HoloPhoneInstantGratification && Target is PhoneFuture))
            {
                UI.CareerSelectionModelEx.Singleton.ShowCareerSelection(Actor, Target.ObjectId, OccupationEntries, true);
                Actor.AddExitReason(ExitReason.Finished);
                return;
            }
            List<OccupationEntryTuple> list = new List<OccupationEntryTuple>(1);
            OccupationEntryTuple entry = OccupationEntries[0];
            list.Add(entry);
            OccupationEntries.Remove(entry);
            if (UI.CareerSelectionModelEx.Singleton.ShowCareerSelection(Actor, Target.ObjectId, list, false))
            {
                Actor.AddExitReason(ExitReason.Finished);
                return;
            }
            AlarmManager.Global.RemoveAlarm(EventAlarm);
            EventAlarm = AlarmManager.Global.AddAlarm(Phone.UploadResume.FindJobNumMinutesBetweenOffer, TimeUnit.Minutes, new AlarmTimerCallback(FindJobAlarmCallback), "Gamefreak130 wuz here", AlarmType.AlwaysPersisted, Actor);
        }

        public override void Cleanup()
        {
            base.Cleanup();
            AlarmManager.Global.RemoveAlarm(EventAlarm);
        }
    }

    public class SelfEmployed
    {
        private static List<SkillBasedCareer.ValidSkillBasedCareerEntry> GetSkillBasedCareerList(Sim sim)
        {
            List<SkillBasedCareer.ValidSkillBasedCareerEntry> list = new List<SkillBasedCareer.ValidSkillBasedCareerEntry>();
            if (sim.SkillManager != null)
            {
                foreach (Occupation current in CareerManager.OccupationList)
                {
                    GreyedOutTooltipCallback greyedOutTooltipCallback = null;
                    if (current.CanAcceptCareer(sim.ObjectId, ref greyedOutTooltipCallback))
                    {
                        if (Settings.SelfEmployedAvailabilitySettings.TryGetValue(current.Guid.ToString(), out bool value) && value && Occupation.GetOccupationStaticData(current.Guid) is SkillBasedCareerStaticData staticData)
                        {
                            int skillLevel = sim.SkillManager.GetSkillLevel(staticData.CorrespondingSkillName);
                            SkillBasedCareer occupationAsSkillBasedCareer = sim.OccupationAsSkillBasedCareer;
                            if (occupationAsSkillBasedCareer == null || occupationAsSkillBasedCareer.Guid != current.Guid)
                            {
                                list.Add(new SkillBasedCareer.ValidSkillBasedCareerEntry
                                {
                                    Occupation = current.Guid,
                                    SkillLevelMet = skillLevel >= staticData.MinimumSkillLevel,
                                    MinimumSkillLevel = staticData.MinimumSkillLevel
                                });
                            }
                        }
                    }
                }
            }
            return list;
        }

        public class RegisterAsSelfEmployedCityHall : CityHall.RegisterAsSelfEmployed
        {
            new public class Definition : InteractionDefinition<Sim, CityHall, RegisterAsSelfEmployedCityHall>
            {
                internal SkillBasedCareer.ValidSkillBasedCareerEntry OccupationEntry;

                public override InteractionInstance CreateInstance(ref InteractionInstanceParameters parameters)
                {
                    OccupationEntry.SkillLevelMet = true;
                    OccupationEntry.MinimumSkillLevel = -1;
                    return base.CreateInstance(ref parameters);
                }

                public override string[] GetPath(bool isFemale) => new string[] { Localization.LocalizeString(isFemale, "Gameplay/Objects/RabbitHoles/CityHall:RegisterAsSelfEmployedPathName") };

                public override string GetInteractionName(Sim actor, CityHall target, InteractionObjectPair iop) => Occupation.GetLocalizedCareerName(OccupationEntry.Occupation, actor.SimDescription);

                public override bool Test(Sim a, CityHall target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    if (a.SimDescription.IsEnrolledInBoardingSchool())
                    {
                        return false;
                    }
                    if (!OccupationEntry.SkillLevelMet)
                    {
                        greyedOutTooltipCallback = CreateTooltipCallback(Localization.LocalizeString(a.IsFemale, "Gameplay/Objects/RabbitHoles/CityHall:RegisterAsSelfEmployedMinimumSkillNotMet", new object[]
                        {
                            a,
                            OccupationEntry.MinimumSkillLevel
                        }));
                        return false;
                    }
                    return true;
                }

                public override void AddInteractions(InteractionObjectPair iop, Sim actor, CityHall target, List<InteractionObjectPair> results)
                {
                    foreach (SkillBasedCareer.ValidSkillBasedCareerEntry current in GetSkillBasedCareerList(actor))
                    {
                        Definition definition = new Definition();
                        definition.OccupationEntry.Occupation = current.Occupation;
                        definition.OccupationEntry.MinimumSkillLevel = current.MinimumSkillLevel;
                        definition.OccupationEntry.SkillLevelMet = current.SkillLevelMet;
                        results.Add(new InteractionObjectPair(definition, target));
                    }
                }
            }

            public override void ConfigureInteraction()
            {
                CareerToSet = (InteractionDefinition as Definition).OccupationEntry.Occupation;
                mDisplayGotoCityHallTNS = CareerToSet == OccupationNames.Undefined;
            }

            public override bool InRabbitHole()
            {
                if (!UIUtils.IsOkayToStartModalDialog() || !ActiveCareer.CanAddActiveCareer(Actor.SimDescription, CareerToSet))
                {
                    return false;
                }
                TimedStage timedStage = new TimedStage(GetInteractionName(), Settings.ApplicationTime, false, false, true);
                Stages = new List<Stage>(new Stage[] { timedStage });
                StartStages();
                bool flag = DoLoop(ExitReason.Default);
                if (!flag)
                {
                    return false;
                }
                SkillBasedCareerStaticData skillBasedCareerStaticData = Occupation.GetOccupationStaticData(CareerToSet) as SkillBasedCareerStaticData;
                Occupation staticOccupation = CareerManager.GetStaticOccupation(CareerToSet);
                string localizedCareerName = Occupation.GetLocalizedCareerName(CareerToSet, Actor.SimDescription);
                string description = Localization.LocalizeString(Actor.IsFemale, skillBasedCareerStaticData.CareerDescriptionLocalizationKey);
                string text = string.Empty;
                foreach (string current in staticOccupation.ResponsibilitiesLocalizationKeys)
                {
                    text = text + Localization.LocalizeString(Actor.IsFemale, current) + "\n";
                }
                OpportunityDialog.MaptagObjectInfo mapTagObject;
                mapTagObject.mLotId = 0uL;
                mapTagObject.mMapTag = null;
                mapTagObject.mObjectGuid = ObjectGuid.InvalidObjectGuid;
                mapTagObject.mHouseholdLotId = 18446744073709551615uL;
                bool flag2 = OpportunityDialog.Show(ThumbnailKey.kInvalidThumbnailKey, Actor.ObjectId, ObjectGuid.InvalidObjectGuid, Actor.Name, OpportunityDialog.OpportunityType.SkillBasedCareer, localizedCareerName, description, string.Empty, text, mapTagObject, true, 
                    OpportunityDialog.DescriptionBackgroundType.StaticBackground, Actor.IsFemale, false);
                if (flag2)
                {
                    AcquireOccupationParameters occupationParameters = new AcquireOccupationParameters(CareerToSet, true, true);
                    return Actor.AcquireOccupation(occupationParameters);
                }
                return flag2;
            }
        }

        public class RegisterAsSelfEmployedNewspaper : Sims3.Gameplay.Objects.Miscellaneous.RegisterAsSelfEmployedNewspaper
        {
            new public class Definition : InteractionDefinition<Sim, Newspaper, RegisterAsSelfEmployedNewspaper>
            {
                internal SkillBasedCareer.ValidSkillBasedCareerEntry OccupationEntry;

                public override string[] GetPath(bool isFemale) 
                    => new string[]
                    {
                        Localization.LocalizeString("Gameplay/Objects/Miscellaneous/Newspaper:JobsAndProfessions"),
                        Localization.LocalizeString("Gameplay/Objects/Miscellaneous/Newspaper/RegisterAsSelfEmployed:PathName")
                    };

                public override string GetInteractionName(Sim a, Newspaper target, InteractionObjectPair interaction) => Occupation.GetLocalizedCareerName(OccupationEntry.Occupation, a.SimDescription);

                public override bool Test(Sim a, Newspaper target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    if (!Settings.NewspaperSelfEmployed || a.SimDescription.IsEnrolledInBoardingSchool() || !target.IsReadable || a.SimDescription.ChildOrBelow || GetObjects<CityHall>().Length == 0)
                    {
                        return false;
                    }
                    if (!OccupationEntry.SkillLevelMet)
                    {
                        greyedOutTooltipCallback = CreateTooltipCallback(Localization.LocalizeString(a.IsFemale, "Gameplay/Objects/Miscellaneous/Newspaper/RegisterAsSelfEmployed:MinimumSkillNotMet", new object[]
                        {
                            a,
                            OccupationEntry.MinimumSkillLevel
                        }));
                        return false;
                    }
                    return true;
                }

                public override void AddInteractions(InteractionObjectPair iop, Sim actor, Newspaper target, List<InteractionObjectPair> results)
                {
                    foreach (SkillBasedCareer.ValidSkillBasedCareerEntry current in GetSkillBasedCareerList(actor))
                    {
                        Definition definition = new Definition();
                        definition.OccupationEntry.Occupation = current.Occupation;
                        definition.OccupationEntry.MinimumSkillLevel = current.MinimumSkillLevel;
                        definition.OccupationEntry.SkillLevelMet = current.SkillLevelMet;
                        results.Add(new InteractionObjectPair(definition, target));
                    }
                }
            }

            public override bool Run() => DoFindJob(false);

            public override bool RunFromInventory() => DoFindJob(true);

            new private bool DoFindJob(bool bFromInventory)
            {
                StateMachineClient stateMachineClient = StateMachineClient.Acquire(Actor.Proxy.ObjectId, "ReadNewspaper");
                if (!Target.StartUsingNewspaper(bFromInventory, false, Actor, stateMachineClient))
                {
                    CarrySystem.PutDownOnFloor(Actor);
                    return false;
                }
                stateMachineClient.RequestState("x", "ReadNewspaper");
                DoTimedLoop(RandomUtil.GetFloat(kBrowseBeforeLeavingRange[0], kBrowseBeforeLeavingRange[1]));
                bool result = Target.StopUsingNewspaper(Actor, stateMachineClient, bFromInventory);
                CityHall[] objects = GetObjects<CityHall>();
                Definition definition = InteractionDefinition as Definition;
                if (objects.Length > 0)
                {
                    CityHall.RegisterAsSelfEmployed registerAsSelfEmployed = CityHall.RegisterAsSelfEmployed.Singleton.CreateInstance(objects[0], Actor, mPriority, Autonomous, CancellableByPlayer) as CityHall.RegisterAsSelfEmployed;
                    registerAsSelfEmployed.CareerToSet = definition.OccupationEntry.Occupation;
                    Actor.InteractionQueue.PushAsContinuation(registerAsSelfEmployed, true);
                }
                return result;
            }
        }

        public class RegisterAsSelfEmployedPhone : Phone.CallRegisterAsSelfEmployed
        {
            new public class Definition : CallDefinition<RegisterAsSelfEmployedPhone>
            {
                internal SkillBasedCareer.ValidSkillBasedCareerEntry OccupationEntry;

                public override string[] GetPath(bool isFemale) 
                    => new string[]
                    {
                        Phone.LocalizeString(isFemale, "JobsAndOffers")  + Localization.Ellipsis,
                        Phone.LocalizeString(isFemale, "RegisterAsSelfEmployedPathName")
                    };

                public override string GetInteractionName(Sim actor, Phone target, InteractionObjectPair iop) => Occupation.GetLocalizedCareerName(OccupationEntry.Occupation, actor.SimDescription);

                public override bool Test(Sim a, Phone target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    if (a.SimDescription.IsEnrolledInBoardingSchool() || !base.Test(a, target, isAutonomous, ref greyedOutTooltipCallback) || GetObjects<CityHall>().Length == 0)
                    {
                        return false;
                    }
                    if (!OccupationEntry.SkillLevelMet)
                    {
                        greyedOutTooltipCallback = CreateTooltipCallback(Phone.LocalizeString("RegisterAsSelfEmployedMinimumSkillNotMet", new object[]
                        {
                            a,
                            OccupationEntry.MinimumSkillLevel
                        }));
                        return false;
                    }
                    return true;
                }

                public override void AddInteractions(InteractionObjectPair iop, Sim actor, Phone target, List<InteractionObjectPair> results)
                {
                    List<SkillBasedCareer.ValidSkillBasedCareerEntry> skillBasedCareerList = GetSkillBasedCareerList(actor);
                    foreach (SkillBasedCareer.ValidSkillBasedCareerEntry current in skillBasedCareerList)
                    {
                        Definition definition = new Definition();
                        definition.OccupationEntry.Occupation = current.Occupation;
                        definition.OccupationEntry.MinimumSkillLevel = current.MinimumSkillLevel;
                        definition.OccupationEntry.SkillLevelMet = current.SkillLevelMet;
                        results.Add(new InteractionObjectPair(definition, target));
                    }
                }
            }

            public override void OnPhoneFinished()
            {
                Definition definition = InteractionDefinition as Definition;
                CityHall[] objects = GetObjects<CityHall>();
                if (objects.Length > 0)
                {
                    CityHall.RegisterAsSelfEmployed registerAsSelfEmployed = CityHall.RegisterAsSelfEmployed.Singleton.CreateInstance(objects[0], Actor, mPriority, Autonomous, CancellableByPlayer) as CityHall.RegisterAsSelfEmployed;
                    registerAsSelfEmployed.CareerToSet = definition.OccupationEntry.Occupation;
                    Actor.InteractionQueue.PushAsContinuation(registerAsSelfEmployed, true);
                }
            }
        }

        public class RegisterAsSelfEmployedComputer : Computer.RegisterAsSelfEmployedComputer
        {
            new public class Definition : InteractionDefinition<Sim, Computer, RegisterAsSelfEmployedComputer>
            {
                internal SkillBasedCareer.ValidSkillBasedCareerEntry OccupationEntry;

                public override string[] GetPath(bool isFemale) 
                    => new string[]
                    {
                        Computer.LocalizeString(isFemale, "JobsAndProfessions"),
                        Computer.LocalizeString(isFemale, "RegisterAsSelfEmployedPathName")
                    };

                public override string GetInteractionName(Sim actor, Computer target, InteractionObjectPair iop) => Occupation.GetLocalizedCareerName(OccupationEntry.Occupation, actor.SimDescription);

                public override bool Test(Sim a, Computer target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    if (a.SimDescription.IsEnrolledInBoardingSchool() || !target.IsComputerUsable(a, true, false, isAutonomous) || GetObjects<CityHall>().Length == 0)
                    {
                        return false;
                    }
                    if (!OccupationEntry.SkillLevelMet)
                    {
                        greyedOutTooltipCallback = CreateTooltipCallback(Computer.LocalizeString(a.IsFemale, "RegisterAsSelfEmployedMinimumSkillNotMet", new object[]
                        {
                            a,
                            OccupationEntry.MinimumSkillLevel
                        }));
                        return false;
                    }
                    return true;
                }

                public override void AddInteractions(InteractionObjectPair iop, Sim actor, Computer target, List<InteractionObjectPair> results)
                {
                    List<SkillBasedCareer.ValidSkillBasedCareerEntry> skillBasedCareerList = GetSkillBasedCareerList(actor);
                    foreach (SkillBasedCareer.ValidSkillBasedCareerEntry current in skillBasedCareerList)
                    {
                        Definition definition = new Definition();
                        definition.OccupationEntry.Occupation = current.Occupation;
                        definition.OccupationEntry.MinimumSkillLevel = current.MinimumSkillLevel;
                        definition.OccupationEntry.SkillLevelMet = current.SkillLevelMet;
                        results.Add(new InteractionObjectPair(definition, target));
                    }
                }
            }

            public override bool Run()
            {
                StandardEntry();
                if (!Target.StartComputing(this, SurfaceHeight.Table, true))
                {
                    StandardExit();
                    return false;
                }
                Target.StartVideo(Computer.VideoType.Browse);
                AnimateSim("GenericTyping");
                DoTimedLoop(RandomUtil.GetFloat(kBrowseBeforeLeavingRange[0], kBrowseBeforeLeavingRange[1]));
                Target.StopComputing(this, Computer.StopComputingAction.TurnOff, false);
                CityHall[] objects = GetObjects<CityHall>();
                Definition definition = InteractionDefinition as Definition;
                if (objects.Length > 0)
                {
                    CityHall.RegisterAsSelfEmployed registerAsSelfEmployed = CityHall.RegisterAsSelfEmployed.Singleton.CreateInstance(objects[0], Actor, mPriority, Autonomous, CancellableByPlayer) as CityHall.RegisterAsSelfEmployed;
                    registerAsSelfEmployed.CareerToSet = definition.OccupationEntry.Occupation;
                    Actor.InteractionQueue.PushAsContinuation(registerAsSelfEmployed, true);
                }
                StandardExit();
                return true;
            }
        }
    }
}