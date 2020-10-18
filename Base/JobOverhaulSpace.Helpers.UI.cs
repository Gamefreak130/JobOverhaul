﻿using Gamefreak130.JobOverhaulSpace.Interactions;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Academics;
using Sims3.Gameplay.ActiveCareer;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Careers;
using Sims3.Gameplay.ChildAndTeenUpdates;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.MapTags;
using Sims3.Gameplay.Objects.Miscellaneous;
using Sims3.Gameplay.Objects.RabbitHoles;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.UI;
using Sims3.UI.Hud;
using System.Collections.Generic;
using static Gamefreak130.JobOverhaul;
using static Gamefreak130.JobOverhaulSpace.Helpers.Methods;
using static Gamefreak130.JobOverhaulSpace.Interactions.Interviews;
using static Sims3.Gameplay.Queries;
using static Sims3.UI.ObjectPicker;
using static Sims3.UI.StyledNotification;

namespace Gamefreak130.JobOverhaulSpace.Helpers.UI
{
    public class CareerSelectionModelEx : CareerSelectionModel
    {
        new public static CareerSelectionModelEx Singleton = new CareerSelectionModelEx();

        public List<OccupationEntryTuple> mAvailableCareersLocationsEx;

        public void ShowCareerSelection(Sim sim, ObjectGuid interactingObject, List<OccupationEntryTuple> careers)
        {
            while (mSim != null)
            {
                Simulator.Sleep(0u);
            }
            mSim = sim;
            try
            {
                mHomeLotMapTag = new HomeLotMapTag(mSim.LotHome, mSim);
                CareerSelectionModel.Singleton.mHomeLotMapTag = mHomeLotMapTag;
                mCurrentObject = interactingObject;
                mCurrentState = CareerSelectionStates.kSeeSim;
                CareerSelectionModel.Singleton.mCurrentState = CareerSelectionStates.kSeeSim;
                if (CameraController.GetControllerType() == CameraControllerType.Video)
                {
                    CameraController.SetControllerType(CameraControllerType.Main);
                }
                mPrevPitch = CameraController.GetPitchDegrees();
                mPrevYaw = CameraController.GetYawDegrees();
                mPrevPosition = CameraController.GetTarget();
                mPrevZoom = CameraController.GetZoom();
                mAvailableCareersLocationsEx.Clear();
                mCareerEntries.Clear();
                foreach (OccupationEntryTuple tuple in careers)
                {
                    if (tuple.OccupationEntry is Career && tuple.CareerLocation == null)
                    {
                        continue;
                    }
                    if (tuple.OccupationEntry is XpBasedCareer && mSim.CareerManager.QuitCareers.TryGetValue(tuple.OccupationEntry.Guid, out Occupation occupation))
                    {
                        tuple.OccupationEntry = occupation.CloneCareerAtNewStartingLevel();
                    }
                    mAvailableCareersLocationsEx.Add(tuple);
                    mCareerEntries.Add(tuple.OccupationEntry);
                }
                IOccupationEntry occupationEntry = mCareerEntries.Count > 0 ? CareerSelectionDialogEx.Show(sim.IsFemale) : null;
                if (occupationEntry != null && mCurrentState != CareerSelectionStates.kSelectingCareer)
                {
                    Occupation occupation = occupationEntry as Occupation;
                    CareerSelectionStates careerSelectionStates = mCurrentState;
                    mCurrentState = CareerSelectionStates.kSelectingCareer;
                    CareerSelectionModel.Singleton.mCurrentState = mCurrentState;
                    if (!occupation.IsActive)
                    {
                        if (mCurrentCareerLocation != null)
                        {
                            Career career = occupation as Career;
                            AcquireOccupationParameters occupationParameters = new AcquireOccupationParameters(mCurrentCareerLocation, false, true);
                            if (BoardingSchool.DidSimGraduate(mSim.SimDescription, BoardingSchool.BoardingSchoolTypes.None, false))
                            {
                                BoardingSchool.UpdateAcquireOccupationParameters(mSim.SimDescription.BoardingSchool, career.CareerGuid, ref occupationParameters);
                            }
                            if (mSim.AcquireOccupation(occupationParameters))
                            {
                                string newOccupationTnsText = mSim.CareerManager.Occupation.GetNewOccupationTnsText();
                                if (!string.IsNullOrEmpty(newOccupationTnsText))
                                {
                                    mSim.CareerManager.Occupation.ShowOccupationTNS(newOccupationTnsText);
                                }
                                Audio.StartSound("sting_career_positive");
                            }
                        }
                    }
                    else
                    {
                        AcquireOccupationParameters occupationParameters = new AcquireOccupationParameters(occupation.Guid, false, true);
                        if (mSim.AcquireOccupation(occupationParameters))
                        {
                            string newOccupationTnsText = mSim.CareerManager.Occupation.GetNewOccupationTnsText();
                            if (!string.IsNullOrEmpty(newOccupationTnsText))
                            {
                                mSim.CareerManager.Occupation.ShowOccupationTNS(newOccupationTnsText);
                            }
                        }
                    }
                    mCurrentState = careerSelectionStates;
                    CareerSelectionModel.Singleton.mCurrentState = careerSelectionStates;
                }
                if (mCurrentState == CareerSelectionStates.kSeeLocation)
                {
                    int currentIndex = mCareerEntries.IndexOf(occupationEntry);
                    SetLocation(false, currentIndex);
                }
            }
            finally
            {
                mSim = null;
            }
        }

        public bool ShowCareerSelection(Sim sim, ObjectGuid interactingObject, List<OccupationEntryTuple> careers, bool isResume)
        {
            while (mSim != null)
            {
                Simulator.Sleep(0u);
            }
            mSim = sim;
            try
            {
                mHomeLotMapTag = new HomeLotMapTag(mSim.LotHome, mSim);
                CareerSelectionModel.Singleton.mHomeLotMapTag = mHomeLotMapTag;
                mCurrentObject = interactingObject;
                mCurrentState = CareerSelectionStates.kSeeSim;
                CareerSelectionModel.Singleton.mCurrentState = CareerSelectionStates.kSeeSim;
                if (CameraController.GetControllerType() == CameraControllerType.Video)
                {
                    CameraController.SetControllerType(CameraControllerType.Main);
                }
                mPrevPitch = CameraController.GetPitchDegrees();
                mPrevYaw = CameraController.GetYawDegrees();
                mPrevPosition = CameraController.GetTarget();
                mPrevZoom = CameraController.GetZoom();
                mAvailableCareersLocationsEx.Clear();
                mCareerEntries.Clear();
                foreach (OccupationEntryTuple tuple in careers)
                {
                    if (tuple.OccupationEntry is Career && tuple.CareerLocation == null)
                    {
                        continue;
                    }
                    mAvailableCareersLocationsEx.Add(tuple);
                    mCareerEntries.Add(tuple.OccupationEntry);
                }
                IOccupationEntry occupationEntry = mCareerEntries.Count > 0 ? CareerSelectionDialogEx.Show(sim.IsFemale) : null;
                if (occupationEntry != null && mCurrentState != CareerSelectionStates.kSelectingCareer)
                {
                    Occupation occupation = occupationEntry as Occupation;
                    CareerSelectionStates careerSelectionStates = mCurrentState;
                    mCurrentState = CareerSelectionStates.kSelectingCareer;
                    CareerSelectionModel.Singleton.mCurrentState = mCurrentState;
                    if (interactingObject.ObjectFromId<Newspaper>() is Newspaper newspaper)
                    {
                        FindJobNewspaperEx interaction = sim.CurrentInteraction as FindJobNewspaperEx;
                        newspaper.StopUsingNewspaper(sim, interaction.mCurrentStateMachine, interaction.mFromInventory);
                    }
                    if (!occupation.IsActive)
                    {
                        if (mCurrentCareerLocation != null)
                        {
                            if (isResume)
                            {
                                Career career = occupation as Career;
                                string name = career.SharedData.Name.Substring(34);
                                if (Settings.InterviewSettings.TryGetValue(name, out InterviewSettings interviewSettings) && interviewSettings.RequiresInterview)
                                {
                                    new InterviewData(mCurrentCareerLocation, sim);
                                }
                                else
                                {
                                    AcquireOccupationParameters occupationParameters = new AcquireOccupationParameters(mCurrentCareerLocation, false, true);
                                    if (BoardingSchool.DidSimGraduate(mSim.SimDescription, BoardingSchool.BoardingSchoolTypes.None, false))
                                    {
                                        BoardingSchool.UpdateAcquireOccupationParameters(mSim.SimDescription.BoardingSchool, career.CareerGuid, ref occupationParameters);
                                    }
                                    if (mSim.AcquireOccupation(occupationParameters))
                                    {
                                        string newOccupationTnsText = mSim.CareerManager.Occupation.GetNewOccupationTnsText();
                                        if (!string.IsNullOrEmpty(newOccupationTnsText))
                                        {
                                            mSim.CareerManager.Occupation.ShowOccupationTNS(newOccupationTnsText);
                                        }
                                        Audio.StartSound("sting_career_positive");
                                    }
                                }
                            }
                            else
                            {
                                SubmitJobApplication joinContinuation = SubmitJobApplication.Singleton.CreateInstance(mCurrentCareerLocation.Owner, mSim, new InteractionPriority(InteractionPriorityLevel.UserDirected), false, true) as SubmitJobApplication;
                                joinContinuation.Occupation = mCurrentCareerLocation.Career.Guid;
                                mSim.InteractionQueue.PushAsContinuation(joinContinuation, true);
                                mSim.ShowTNSIfSelectable(LocalizeString("SubmitJobApplicationGotoTNS", new object[] { mCurrentCareerLocation.Owner.GetLocalizedName() }), NotificationStyle.kGameMessagePositive, ObjectGuid.InvalidObjectGuid, ObjectGuid.InvalidObjectGuid);
                            }
                        }
                    }
                    else
                    {
                        CityHall[] objects = GetObjects<CityHall>();
                        if (objects.Length > 0)
                        {
                            JoinActiveCareerContinuation joinContinuation = JoinActiveCareerContinuation.Singleton.CreateInstance(objects[0], mSim, new InteractionPriority(InteractionPriorityLevel.UserDirected), false, true) as JoinActiveCareerContinuation;
                            joinContinuation.CareerToSet = occupation.Guid;
                            mSim.InteractionQueue.PushAsContinuation(joinContinuation, true);
                            mSim.ShowTNSIfSelectable(Localization.LocalizeString("Gameplay/Objects/RabbitHoles/CityHall:RegisterAsSelfEmployedGotoTNS"), NotificationStyle.kGameMessagePositive, ObjectGuid.InvalidObjectGuid, ObjectGuid.InvalidObjectGuid);
                        }
                        else
                        {
                            AcquireOccupationParameters occupationParameters = new AcquireOccupationParameters(occupation.Guid, false, true);
                            if (mSim.AcquireOccupation(occupationParameters))
                            {
                                string newOccupationTnsText = mSim.CareerManager.Occupation.GetNewOccupationTnsText();
                                if (!string.IsNullOrEmpty(newOccupationTnsText))
                                {
                                    mSim.CareerManager.Occupation.ShowOccupationTNS(newOccupationTnsText);
                                }
                            }
                        }
                    }
                    mCurrentState = careerSelectionStates;
                    CareerSelectionModel.Singleton.mCurrentState = careerSelectionStates;
                    return true;
                }
                if (mCurrentState == CareerSelectionStates.kSeeLocation)
                {
                    int currentIndex = mCareerEntries.IndexOf(occupationEntry);
                    SetLocation(false, currentIndex);
                }
                return false;
            }
            finally
            {
                mSim = null;
            }
        }

        public void CareerSelected(IOccupationEntry entry, int index)
        {
            if ((!entry.IsActive && mCurrentState != CareerSelectionStates.kSelectingCareer && mAvailableCareersLocationsEx[index].CareerLocation != null) || entry.IsActive)
            {
                mCurrentCareerLocation = mAvailableCareersLocationsEx[index].CareerLocation;
                CareerSelectionModel.Singleton.mCurrentCareerLocation = mCurrentCareerLocation;
                bool flag = entry.IsActive && entry.ActiveCareerLotID != 0uL;
                if (flag)
                {
                    mActiveCareerLot = LotManager.GetLot(entry.ActiveCareerLotID);
                    CareerSelectionModel.Singleton.mActiveCareerLot = mActiveCareerLot;
                }
                if (mCurrentState == CareerSelectionStates.kSeeLocation)
                {
                    if (flag)
                    {
                        UpdateCameraForActiveCareerLocation(entry.ActiveCareerLotID);
                        return;
                    }
                    UpdateCameraForCareerLocation();
                }
            }
        }

        new public void SetLocation(bool seeLocation, int currentIndex)
        {
            CareerSelectionStates careerSelectionStates = seeLocation ? CareerSelectionStates.kSeeLocation : CareerSelectionStates.kSeeSim;
            if (careerSelectionStates != mCurrentState)
            {
                mCurrentState = careerSelectionStates;
                CareerSelectionModel.Singleton.mCurrentState = careerSelectionStates;
                RefreshMaptags();
                if (mCurrentState == CareerSelectionStates.kSeeLocation)
                {
                    if (mCareerEntries[currentIndex].IsActive)
                    {
                        UpdateCameraForActiveCareerLocation(mCareerEntries[currentIndex].ActiveCareerLotID);
                        return;
                    }
                    UpdateCameraForCareerLocation();
                    return;
                }
                else
                {
                    CameraController.Unlock();
                    CameraController.RequestLerpToTarget(mPrevPosition, 1f, true);
                    CameraController.RequestLerpToZoomAndRotation(mPrevZoom, mPrevPitch, mPrevYaw, 1f);
                }
            }
        }

        public CareerSelectionModelEx()
        {
            mAvailableCareersLocationsEx = new List<OccupationEntryTuple>();
            mCareerEntries = new List<IOccupationEntry>();
        }
    }

    public class CareerSelectionDialogEx : ModalDialog
    {
        private enum ControlIDs : uint
        {
            ButtonOK = 65333248u,
            WindowObjectImage = 107605763u,
            WindowSimImage,
            TextCareerName,
            TextCareerTitle,
            TextPayPerHour,
            TextHours,
            ScrollDescription,
            ScrollWindow,
            TextDaySunday = 107605792u,
            TextDayMonday,
            TextDayTuesday,
            TextDayWednesday,
            TextDayThursday,
            TextDayFriday,
            TextDaySaturday,
            WindowCurrentDayArrowStart = 107605808u,
            CareerCycleLeftButton = 114633984u,
            CareerCycleRightButton,
            JobIconWin,
            SeeLocationButton,
            CancelButton
        }

        private readonly IHudModel mHudModel;

        private readonly CareerSelectionModelEx mCareerSelectionModel;

        private IOccupationEntry mSelectedCareer;

        private readonly List<IOccupationEntry> mCareerEntries;

        private bool mbResult;

        private readonly bool mIsFemale;

        private Color kDayTextNotWorkingColor = new Color(2155905152u);

        private Color kDayTextWorkingColor = new Color(4278198336u);

        private readonly bool mWasMapview;

        private static readonly string kLayoutName = "CareerSelectDialog";

        private static readonly int kExportID = 1;

        private int mCurrentIndex;

        public static IOccupationEntry Show(bool isFemale)
        {
            if (ScreenGrabController.InProgress)
            {
                return null;
            }
            Responder.Instance.HudModel.RestoreUIVisibility();
            IOccupationEntry result;
            using (CareerSelectionDialogEx careerSelectionDialog = new CareerSelectionDialogEx(isFemale))
            {
                careerSelectionDialog.StartModal();
                result = careerSelectionDialog.mbResult ? careerSelectionDialog.mSelectedCareer : null;
            }
            return result;
        }

        public override bool OnEnd(uint endID)
        {
            mbResult = endID == OkayID && mSelectedCareer != null;
            Responder.Instance.OptionsModel.UIDisableSave = false;
            mCareerSelectionModel.SetLocation(false, mCurrentIndex);
            UIManager.DarkenBackground(false);
            SetGameUIVisibility(true);
            BorderTreatmentsController.SetButtonEnabled(true);
            UIManager.GetSceneWindow().MapViewModeEnabled = mWasMapview;
            WindowBase modalWindow = UIManager.GetModalWindow();
            if (modalWindow == mModalDialogWindow)
            {
                UIManager.EndModal(mModalDialogWindow);
            }
            PieMenu.Hide();
            UIManager.SetOverrideCursor(0u);
            return base.OnEnd(endID);
        }

        private void SetGameUIVisibility(bool visible)
        {
            UIManager.GetUITopWindow().GetChildByID(57857282u, true).Visible = visible;
            UIManager.GetUITopWindow().GetChildByID(57857283u, true).Visible = visible;
            UIManager.GetUITopWindow().GetChildByID(57857291u, true).Visible = visible;
            UIManager.GetUITopWindow().GetChildByID(57857293u, true).Visible = visible;
            UIManager.GetUITopWindow().GetChildByID(57857296u, true).Visible = visible;
            UIManager.GetUITopWindow().GetChildByID(57857295u, true).Visible = visible;
        }

        public CareerSelectionDialogEx(bool isFemale) : base(kLayoutName, kExportID, false, PauseMode.PauseSimulator, null)
        {
            mEnableBackgroundDarkening = false;
            mHudModel = HudController.Instance.Model;
            mCareerSelectionModel = CareerSelectionModelEx.Singleton;
            mCareerEntries = mCareerSelectionModel.mCareerEntries;
            mIsFemale = isFemale;
            Window window = mModalDialogWindow.GetChildByID(107605763u, true) as Window;
            ImageDrawable imageDrawable = window.Drawable as ImageDrawable;
            imageDrawable.Image = UIManager.GetThumbnailImage(Responder.Instance.HudModel.GetThumbnailForGameObject(mCareerSelectionModel.InteractionObjectGuid));
            window.Invalidate();
            window = mModalDialogWindow.GetChildByID(107605764u, true) as Window;
            imageDrawable = window.Drawable as ImageDrawable;
            imageDrawable.Image = UIManager.GetThumbnailImage(Responder.Instance.HudModel.GetThumbnailForGameObject(mCareerSelectionModel.SimGuid));
            window.Invalidate();
            Button button = mModalDialogWindow.GetChildByID(65333248u, true) as Button;
            button.Click += new UIEventHandler<UIButtonClickEventArgs>(OnAcceptCareer);
            button = mModalDialogWindow.GetChildByID(114633988u, true) as Button;
            button.Click += new UIEventHandler<UIButtonClickEventArgs>(OnCancelButtonClick);
            if (mCareerEntries.Count > 2)
            {
                button = mModalDialogWindow.GetChildByID(114633984u, true) as Button;
                button.Click += new UIEventHandler<UIButtonClickEventArgs>(OnCareerSelectionChanged);
                button = mModalDialogWindow.GetChildByID(114633985u, true) as Button;
                button.Click += new UIEventHandler<UIButtonClickEventArgs>(OnCareerSelectionChanged);
            }
            else
            {
                button = mModalDialogWindow.GetChildByID(114633984u, true) as Button;
                button.Visible = false;
                button = mModalDialogWindow.GetChildByID(114633985u, true) as Button;
                button.Visible = false;
            }
            button = mModalDialogWindow.GetChildByID(114633987u, true) as Button;
            button.Click += new UIEventHandler<UIButtonClickEventArgs>(OnSeeLocationButtonClick);
            FillCareerInfo(mCareerEntries[0]);
            EnableDisableAcceptCareerButton(mCareerSelectionModel, mCareerEntries[0]);
            window = mModalDialogWindow.GetChildByID((uint)(107605808 + mHudModel.CurrentDay), true) as Window;
            window.Visible = true;
            mSelectedCareer = mCareerEntries[0];
            mCareerSelectionModel.CareerSelected(mSelectedCareer, mCurrentIndex);
            OkayID = 65333248u;
            CancelID = 114633988u;
            mWasMapview = UIManager.GetSceneWindow().MapViewModeEnabled;
            UIManager.DarkenBackground(true);
            UIManager.BeginModal(mModalDialogWindow);
            UIManager.SetOverrideCursor(95342848u);
            Responder.Instance.OptionsModel.UIDisableSave = true;
        }

        private void OnCareerSelectionChanged(WindowBase sender, UIButtonClickEventArgs eventArgs)
        {
            int count = mCareerEntries.Count;
            mCurrentIndex = ((sender.ID == 114633984u) ? (mCurrentIndex + 1) : (mCurrentIndex + (count - 1))) % count;
            mSelectedCareer = mCareerEntries[mCurrentIndex];
            FillCareerInfo(mSelectedCareer);
            EnableDisableAcceptCareerButton(mCareerSelectionModel, mSelectedCareer);
            if (mSelectedCareer != null)
            {
                mCareerSelectionModel.CareerSelected(mSelectedCareer, mCurrentIndex);
                UIManager.GetSceneWindow().MapViewModeEnabled = false;
            }
            eventArgs.Handled = true;
        }

        private void OnAcceptCareer(WindowBase sender, UIButtonClickEventArgs eventArgs)
        {
            EndDialog(OkayID);
            eventArgs.Handled = true;
        }

        private void OnCancelButtonClick(WindowBase sender, UIButtonClickEventArgs eventArgs)
        {
            EndDialog(CancelID);
            eventArgs.Handled = true;
        }

        private void OnSeeLocationButtonClick(WindowBase sender, UIButtonClickEventArgs eventArgs)
        {
            bool selected = (sender as Button).Selected;
            mCareerSelectionModel.SetLocation(selected, mCurrentIndex);
            if (selected)
            {
                UIManager.GetSceneWindow().MapViewModeEnabled = false;
            }
            UIManager.DarkenBackground(!selected);
            SetGameUIVisibility(!selected);
            BorderTreatmentsController.SetButtonEnabled(!selected);
            WindowBase modalWindow = UIManager.GetModalWindow();
            if (selected && modalWindow == mModalDialogWindow)
            {
                UIManager.EndModal(mModalDialogWindow);
                return;
            }
            if (modalWindow != mModalDialogWindow)
            {
                UIManager.BeginModal(mModalDialogWindow);
            }
        }

        private void FillCareerInfo(IOccupationEntry entry)
        {
            if (entry != null)
            {
                Text text;
                Window window = mModalDialogWindow.GetChildByID(114633986u, true) as Window;
                (window.Drawable as ImageDrawable).Image = UIManager.LoadUIImage(ResourceKey.CreatePNGKey(entry.CareerIconColored, 0u));
                window.Position = new Vector2(80f, 130f);
                text = mModalDialogWindow.GetChildByID(107605765u, true) as Text;
                text.Caption = LocalizeString("CareerOfferHeader");
                text = mModalDialogWindow.GetChildByID(107605766u, true) as Text;
                text.Caption = entry.GetLocalizedCareerName(mIsFemale);
                text = mModalDialogWindow.GetChildByID(107605767u, true) as Text;
                float oldHeight = text.Area.Height;
                if (entry.IsActive)
                {
                    text.Caption = entry.PayPerHourOrStipend > 0 ? Responder.Instance.LocalizationModel.LocalizeString("UI/Caption/CareerSelection:WeeklyStipend", new object[] { entry.PayPerHourOrStipend }) : string.Empty;
                    text.AutoSize(true);
                    text = mModalDialogWindow.GetChildByID(107605768u, true) as Text;
                    if (entry.HasOpenHours)
                    {
                        text.Caption = Responder.Instance.LocalizationModel.LocalizeString("UI/Caption/CareerSelection:OpenHours");
                        HideWorkDays();
                    }
                    else
                    {
                        text.Caption = entry.CareerOfferWorkHours;
                        UpdateDaysofWeek(entry.CareerOfferWorkDays);
                    }
                }
                else
                {
                    text.Caption = mCareerSelectionModel.mAvailableCareersLocationsEx[mCurrentIndex].CareerLocation.Owner.GetLocalizedName();
                    text.AutoSize(true);
                    text = mModalDialogWindow.GetChildByID(107605768u, true) as Text;
                    text.Caption = (entry as Career).IsPartTime ? LocalizeString("PartTime") : LocalizeString("FullTime");
                    HideWorkDays();
                }
                text = mModalDialogWindow.GetChildByID(107605767u, true) as Text;
                float offset = text.Area.Height - oldHeight;
                text = mModalDialogWindow.GetChildByID(107605768u, true) as Text;
                text.Position = new Vector2(text.Area.TopLeft.x, text.Area.TopLeft.y + offset);
                text = mModalDialogWindow.GetChildByID(107605769u, true) as Text;
                text.Caption = mCareerSelectionModel.LocalizeCareerDetails(entry.CareerOfferInfo);
                text.AutoSize(true);
                text.Position = new Vector2(0f, 0f);
                ScrollWindow scrollWindow = mModalDialogWindow.GetChildByID(107605770u, true) as ScrollWindow;
                scrollWindow.Update();
                Button button = mModalDialogWindow.GetChildByID(114633987u, true) as Button;
                button.Visible = !(entry.IsActive && entry.ActiveCareerLotID == 0uL);
                window.Invalidate();
            }
        }

        private void EnableDisableAcceptCareerButton(ICareerSelectionModel model, IOccupationEntry entry)
        {
            if (entry is Occupation occupation)
            {
                GreyedOutTooltipCallback greyedOutTooltipCallback = null;
                Sim sim = GameObject.GetObject<Sim>(model.SimGuid);
                Button button = mModalDialogWindow.GetChildByID(65333248u, true) as Button;
                button.Enabled = false;
                string name = occupation is Career ? (occupation as Career).SharedData.Name.Substring(34) : occupation.Guid.ToString();
                if (!occupation.CanAcceptCareer(model.SimGuid, ref greyedOutTooltipCallback))
                {
                    button.TooltipText = greyedOutTooltipCallback != null ? greyedOutTooltipCallback() : LocalizeString("NotCorrectAgeForOccupation");
                    if (button.TooltipText == Localization.LocalizeString(sim.IsFemale, "Gameplay/Occupation:GreyedOutUiTooltipAlreadyHasOccupation", new object[] { sim.SimDescription }))
                    {
                        if ((!occupation.IsActive && mCareerSelectionModel.mAvailableCareersLocationsEx[mCurrentIndex].CareerLocation is CareerLocation location && sim.CareerLocation == location.Owner) || occupation.IsActive)
                        {
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                if (entry is Career career && !career.CareerAgeTest(sim.SimDescription))
                {
                    button.TooltipText = LocalizeString(sim.IsFemale, "NotCorrectAgeForOccupation");
                    return;
                }
                if (entry is ActiveCareer && sim.SimDescription.TeenOrBelow)
                {
                    button.TooltipText = LocalizeString(sim.IsFemale, "TooYoungForProfession");
                    return;
                }
                if (GameUtils.IsInstalled(ProductVersion.EP9) && Settings.CareerAvailabilitySettings.TryGetValue(name, out CareerAvailabilitySettings settings) && settings.RequiredDegrees.Count > 0)
                {
                    foreach (AcademicDegreeNames degree in settings.RequiredDegrees)
                    {
                        if (sim.DegreeManager == null || !sim.DegreeManager.HasCompletedDegree(degree))
                        {
                            button.TooltipText = LocalizeString(sim.IsFemale, "DoesNotHaveRequiredDegrees");
                            return;
                        }
                    }
                }
                button.Enabled = true;
                button.TooltipText = string.Empty;
            }
        }

        private void UpdateDaysofWeek(string workDays)
        {
            for (uint num = 107605792u; num <= 107605798u; num += 1u)
            {
                Text text = mModalDialogWindow.GetChildByID(num, true) as Text;
                text.Visible = true;
                text.TextColor = kDayTextNotWorkingColor;
                text.AutoSize(false);
            }
            char[] array = workDays.ToCharArray();
            for (int i = 0; i < array.Length; i++)
            {
                char c = array[i];
                uint num2 = 0u;
                char c2 = c;
                if (c2 != 'F')
                {
                    switch (c2)
                    {
                        case 'M':
                            num2 = 107605793u;
                            break;
                        case 'R':
                            num2 = 107605796u;
                            break;
                        case 'S':
                            num2 = 107605798u;
                            break;
                        case 'T':
                            num2 = 107605794u;
                            break;
                        case 'U':
                            num2 = 107605792u;
                            break;
                        case 'W':
                            num2 = 107605795u;
                            break;
                    }
                }
                else
                {
                    num2 = 107605797u;
                }
                if (num2 > 0u)
                {
                    Text text2 = mModalDialogWindow.GetChildByID(num2, true) as Text;
                    if (text2 != null)
                    {
                        text2.TextColor = kDayTextWorkingColor;
                    }
                }
            }
        }

        private void HideWorkDays()
        {
            for (uint num = 107605792u; num <= 107605798u; num += 1u)
            {
                Text text = mModalDialogWindow.GetChildByID(num, true) as Text;
                text.Visible = false;
            }
        }
    }

    // Allows for okay button to be clicked with no items selected
    // In which case an empty RowInfo list is returned
    public class ObjectPickerDialogEx : ObjectPickerDialog
    {
        public ObjectPickerDialogEx(bool modal, PauseMode pauseMode, string title, string buttonTrue, string buttonFalse, List<TabInfo> listObjs, List<HeaderInfo> headers, int numSelectableRows, Vector2 position, bool viewTypeToggle, List<RowInfo> preSelectedRows, bool showHeadersAndToggle, bool disableCloseButton) : base(modal, pauseMode, title, buttonTrue, buttonFalse, listObjs, headers, numSelectableRows, position, viewTypeToggle, preSelectedRows, showHeadersAndToggle, disableCloseButton)
        {
            mOkayButton.Enabled = true;
            mTable.ObjectTable.TableChanged -= OnTableChanged;
            mTable.SelectionChanged -= OnSelectionChanged;
            mTable.SelectionChanged += OnSelectionChangedEx;
            mTable.RowSelected -= OnSelectionChanged;
            mTable.RowSelected += OnSelectionChangedEx;
            mTable.Selected = preSelectedRows;
        }

        new public static List<RowInfo> Show(bool modal, PauseMode pauseType, string title, string buttonTrue, string buttonFalse, List<TabInfo> listObjs, List<HeaderInfo> headers, int numSelectableRows, Vector2 position, bool viewTypeToggle, List<RowInfo> preSelectedRows, bool showHeadersAndToggle, bool disableCloseButton)
        {
            using (ObjectPickerDialogEx objectPickerDialog = new ObjectPickerDialogEx(modal, pauseType, title, buttonTrue, buttonFalse, listObjs, headers, numSelectableRows, position, viewTypeToggle, preSelectedRows, showHeadersAndToggle, disableCloseButton))
            {
                objectPickerDialog.StartModal();
                return objectPickerDialog.Result;
            }
        }

        public override bool OnEnd(uint endID)
        {
            if (endID == OkayID)
            {
                if (!mOkayButton.Enabled)
                {
                    return false;
                }
                mResult = mTable.Selected ?? new List<RowInfo>();
            }
            else
            {
                mResult = null;
            }
            mTable.Populate(null, null, 0);
            return true;
        }

        private void OnSelectionChangedEx(List<RowInfo> selectedRows) => Audio.StartSound("ui_tertiary_button");
    }
}
