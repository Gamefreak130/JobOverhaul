﻿using Gamefreak130.JobOverhaulSpace.Helpers;
using Gamefreak130.JobOverhaulSpace.Interactions;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Academics;
using Sims3.Gameplay.ActiveCareer;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Careers;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.CelebritySystem;
using Sims3.Gameplay.ChildAndTeenUpdates;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
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

namespace Gamefreak130.JobOverhaulSpace.UI
{
    public class CareerSelectionModelEx : CareerSelectionModel
    {
        new public static CareerSelectionModelEx Singleton = new();

        public List<OccupationEntryTuple> mAvailableCareersLocationsEx;

        public void ShowCareerSelection(Sim sim, ObjectGuid interactingObject, List<OccupationEntryTuple> careers)
        {
            while (mSim is not null)
            {
                Simulator.Sleep(0u);
            }
            mSim = sim;
            try
            {
                if (!mSim.IsSelectable)
                {
                    return;
                }
                mHomeLotMapTag = new(mSim.LotHome, mSim);
                CareerSelectionModel.Singleton.mHomeLotMapTag = mHomeLotMapTag;
                mCurrentObject = interactingObject;
                mCurrentState = CareerSelectionStates.kSeeSim;
                CareerSelectionModel.Singleton.mCurrentState = CareerSelectionStates.kSeeSim;
                if (CameraController.GetControllerType() is CameraControllerType.Video)
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
                    if (tuple is { OccupationEntry: Career, CareerLocation: null })
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
                if (occupationEntry is not null && mCurrentState is not CareerSelectionStates.kSelectingCareer)
                {
                    Occupation occupation = occupationEntry as Occupation;
                    CareerSelectionStates careerSelectionStates = mCurrentState;
                    mCurrentState = CareerSelectionStates.kSelectingCareer;
                    CareerSelectionModel.Singleton.mCurrentState = mCurrentState;
                    if (!occupation.IsActive)
                    {
                        if (mCurrentCareerLocation is not null)
                        {
                            Career career = occupation as Career;
                            AcquireOccupationParameters occupationParameters = new(mCurrentCareerLocation, false, true);
                            if (BoardingSchool.DidSimGraduate(mSim.SimDescription, BoardingSchool.BoardingSchoolTypes.None, false))
                            {
                                BoardingSchool.UpdateAcquireOccupationParameters(mSim.SimDescription.BoardingSchool, career.CareerGuid, ref occupationParameters);
                            }
                            if (mSim.AcquireOccupation(occupationParameters))
                            {
                                Career clonedCareer = career is School ? mSim.CareerManager.School : mSim.CareerManager.OccupationAsCareer;
                                string newOccupationTnsText = clonedCareer.GetNewOccupationTnsText();
                                if (!string.IsNullOrEmpty(newOccupationTnsText))
                                {
                                    clonedCareer.ShowOccupationTNS(newOccupationTnsText);
                                }
                                Audio.StartSound("sting_career_positive");
                            }
                        }
                    }
                    else
                    {
                        AcquireOccupationParameters occupationParameters = new(occupation.Guid, false, true);
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
                if (mCurrentState is CareerSelectionStates.kSeeLocation)
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
            while (mSim is not null)
            {
                Simulator.Sleep(0u);
            }
            mSim = sim;
            try
            {
                if (!mSim.IsSelectable)
                {
                    return false;
                }
                mHomeLotMapTag = new(mSim.LotHome, mSim);
                CareerSelectionModel.Singleton.mHomeLotMapTag = mHomeLotMapTag;
                mCurrentObject = interactingObject;
                mCurrentState = CareerSelectionStates.kSeeSim;
                CareerSelectionModel.Singleton.mCurrentState = CareerSelectionStates.kSeeSim;
                if (CameraController.GetControllerType() is CameraControllerType.Video)
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
                    if (tuple is { OccupationEntry: Career, CareerLocation: null })
                    {
                        continue;
                    }
                    mAvailableCareersLocationsEx.Add(tuple);
                    mCareerEntries.Add(tuple.OccupationEntry);
                }
                IOccupationEntry occupationEntry = mCareerEntries.Count > 0 ? CareerSelectionDialogEx.Show(sim.IsFemale) : null;
                if (occupationEntry is not null && mCurrentState is not CareerSelectionStates.kSelectingCareer)
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
                        if (mCurrentCareerLocation is not null)
                        {
                            if (isResume)
                            {
                                Career career = occupation as Career;
                                string name = career.SharedData.Name.Substring(34);
                                if (Settings.InterviewMap.TryGetValue(name, out PersistedSettings.InterviewSettings interviewSettings) && interviewSettings.RequiresInterview)
                                {
                                    new InterviewData(mCurrentCareerLocation, sim);
                                }
                                else
                                {
                                    AcquireOccupationParameters occupationParameters = new(mCurrentCareerLocation, false, true);
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
                                SubmitJobApplication joinContinuation = SubmitJobApplication.Singleton.CreateInstance(mCurrentCareerLocation.Owner, mSim, new(InteractionPriorityLevel.UserDirected), false, true) as SubmitJobApplication;
                                joinContinuation.Occupation = mCurrentCareerLocation.Career.Guid;
                                mSim.InteractionQueue.PushAsContinuation(joinContinuation, true);
                                mSim.ShowTNSIfSelectable(LocalizeString("SubmitJobApplicationGotoTNS", mCurrentCareerLocation.Owner.GetLocalizedName()), StyledNotification.NotificationStyle.kGameMessagePositive, ObjectGuid.InvalidObjectGuid, ObjectGuid.InvalidObjectGuid);
                            }
                        }
                    }
                    else
                    {
                        CityHall[] objects = GetObjects<CityHall>();
                        if (objects.Length > 0)
                        {
                            JoinActiveCareerContinuation joinContinuation = JoinActiveCareerContinuation.Singleton.CreateInstance(objects[0], mSim, new(InteractionPriorityLevel.UserDirected), false, true) as JoinActiveCareerContinuation;
                            joinContinuation.CareerToSet = occupation.Guid;
                            mSim.InteractionQueue.PushAsContinuation(joinContinuation, true);
                            mSim.ShowTNSIfSelectable(Localization.LocalizeString("Gameplay/Objects/RabbitHoles/CityHall:RegisterAsSelfEmployedGotoTNS"), StyledNotification.NotificationStyle.kGameMessagePositive, ObjectGuid.InvalidObjectGuid, ObjectGuid.InvalidObjectGuid);
                        }
                        else
                        {
                            AcquireOccupationParameters occupationParameters = new(occupation.Guid, false, true);
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
                if (mCurrentState is CareerSelectionStates.kSeeLocation)
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
            if ((!entry.IsActive && mCurrentState is not CareerSelectionStates.kSelectingCareer && mAvailableCareersLocationsEx[index].CareerLocation is not null) || entry.IsActive)
            {
                mCurrentCareerLocation = mAvailableCareersLocationsEx[index].CareerLocation;
                CareerSelectionModel.Singleton.mCurrentCareerLocation = mCurrentCareerLocation;
                bool flag = entry.IsActive && entry.ActiveCareerLotID != 0uL;
                if (flag)
                {
                    mActiveCareerLot = LotManager.GetLot(entry.ActiveCareerLotID);
                    CareerSelectionModel.Singleton.mActiveCareerLot = mActiveCareerLot;
                }
                if (mCurrentState is CareerSelectionStates.kSeeLocation)
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
                if (mCurrentState is CareerSelectionStates.kSeeLocation)
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
            mAvailableCareersLocationsEx = new();
            mCareerEntries = new();
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

        private static readonly Color kDayTextNotWorkingColor = new(2155905152u);

        private static readonly Color kDayTextWorkingColor = new(4278198336u);

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
            using (CareerSelectionDialogEx careerSelectionDialog = new(isFemale))
            {
                careerSelectionDialog.StartModal();
                result = careerSelectionDialog.mbResult ? careerSelectionDialog.mSelectedCareer : null;
            }
            return result;
        }

        public override bool OnEnd(uint endID)
        {
            mbResult = endID == OkayID && mSelectedCareer is not null;
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
            Window window = mModalDialogWindow.GetChildByID((uint)ControlIDs.WindowObjectImage, true) as Window;
            ImageDrawable imageDrawable = window.Drawable as ImageDrawable;
            imageDrawable.Image = UIManager.GetThumbnailImage(Responder.Instance.HudModel.GetThumbnailForGameObject(mCareerSelectionModel.InteractionObjectGuid));
            window.Invalidate();
            window = mModalDialogWindow.GetChildByID((uint)ControlIDs.WindowSimImage, true) as Window;
            imageDrawable = window.Drawable as ImageDrawable;
            imageDrawable.Image = UIManager.GetThumbnailImage(Responder.Instance.HudModel.GetThumbnailForGameObject(mCareerSelectionModel.SimGuid));
            window.Invalidate();
            Button button = mModalDialogWindow.GetChildByID((uint)ControlIDs.ButtonOK, true) as Button;
            button.Click += OnAcceptCareer;
            button = mModalDialogWindow.GetChildByID((uint)ControlIDs.CancelButton, true) as Button;
            button.Click += OnCancelButtonClick;
            if (mCareerEntries.Count > 2)
            {
                button = mModalDialogWindow.GetChildByID((uint)ControlIDs.CareerCycleLeftButton, true) as Button;
                button.Click += OnCareerSelectionChanged;
                button = mModalDialogWindow.GetChildByID((uint)ControlIDs.CareerCycleRightButton, true) as Button;
                button.Click += OnCareerSelectionChanged;
            }
            else
            {
                button = mModalDialogWindow.GetChildByID((uint)ControlIDs.CareerCycleLeftButton, true) as Button;
                button.Visible = false;
                button = mModalDialogWindow.GetChildByID((uint)ControlIDs.CareerCycleRightButton, true) as Button;
                button.Visible = false;
            }
            button = mModalDialogWindow.GetChildByID((uint)ControlIDs.SeeLocationButton, true) as Button;
            button.Click += OnSeeLocationButtonClick;
            FillCareerInfo(mCareerEntries[0]);
            EnableDisableAcceptCareerButton(mCareerSelectionModel, mCareerEntries[0]);
            window = mModalDialogWindow.GetChildByID((uint)((uint)ControlIDs.WindowCurrentDayArrowStart + mHudModel.CurrentDay), true) as Window;
            window.Visible = true;
            mSelectedCareer = mCareerEntries[0];
            mCareerSelectionModel.CareerSelected(mSelectedCareer, mCurrentIndex);
            OkayID = (uint)ControlIDs.ButtonOK;
            CancelID = (uint)ControlIDs.CancelButton;
            mWasMapview = UIManager.GetSceneWindow().MapViewModeEnabled;
            UIManager.DarkenBackground(true);
            UIManager.BeginModal(mModalDialogWindow);
            UIManager.SetOverrideCursor(95342848u);
            Responder.Instance.OptionsModel.UIDisableSave = true;
        }

        private void OnCareerSelectionChanged(WindowBase sender, UIButtonClickEventArgs eventArgs)
        {
            int count = mCareerEntries.Count;
            mCurrentIndex = ((sender.ID == (uint)ControlIDs.CareerCycleLeftButton) ? (mCurrentIndex + 1) : (mCurrentIndex + (count - 1))) % count;
            mSelectedCareer = mCareerEntries[mCurrentIndex];
            FillCareerInfo(mSelectedCareer);
            EnableDisableAcceptCareerButton(mCareerSelectionModel, mSelectedCareer);
            if (mSelectedCareer is not null)
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
            if (entry is not null)
            {
                Text text;
                Window window = mModalDialogWindow.GetChildByID((uint)ControlIDs.JobIconWin, true) as Window;
                (window.Drawable as ImageDrawable).Image = UIManager.LoadUIImage(ResourceKey.CreatePNGKey(entry.CareerIconColored, 0u));
                window.Position = new(80f, 130f);
                text = mModalDialogWindow.GetChildByID((uint)ControlIDs.TextCareerName, true) as Text;
                text.Caption = LocalizeString("CareerOfferHeader");
                text = mModalDialogWindow.GetChildByID((uint)ControlIDs.TextCareerTitle, true) as Text;
                text.Caption = entry.GetLocalizedCareerName(mIsFemale);
                text = mModalDialogWindow.GetChildByID((uint)ControlIDs.TextPayPerHour, true) as Text;
                float oldHeight = text.Area.Height;
                if (entry.IsActive)
                {
                    text.Caption = entry.PayPerHourOrStipend > 0 ? Responder.Instance.LocalizationModel.LocalizeString("UI/Caption/CareerSelection:WeeklyStipend", entry.PayPerHourOrStipend) : string.Empty;
                    text.AutoSize(true);
                    text = mModalDialogWindow.GetChildByID((uint)ControlIDs.TextHours, true) as Text;
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
                    text = mModalDialogWindow.GetChildByID((uint)ControlIDs.TextHours, true) as Text;
                    text.Caption = (entry as Career).IsPartTime ? LocalizeString("PartTime") : LocalizeString("FullTime");
                    HideWorkDays();
                }
                text = mModalDialogWindow.GetChildByID((uint)ControlIDs.TextPayPerHour, true) as Text;
                float offset = text.Area.Height - oldHeight;
                text = mModalDialogWindow.GetChildByID((uint)ControlIDs.TextHours, true) as Text;
                text.Position = new(text.Area.TopLeft.x, text.Area.TopLeft.y + offset);
                text = mModalDialogWindow.GetChildByID((uint)ControlIDs.ScrollDescription, true) as Text;
                text.Caption = mCareerSelectionModel.LocalizeCareerDetails(entry.CareerOfferInfo);
                text.AutoSize(true);
                text.Position = new(0f, 0f);
                ScrollWindow scrollWindow = mModalDialogWindow.GetChildByID((uint)ControlIDs.ScrollWindow, true) as ScrollWindow;
                scrollWindow.Update();
                Button button = mModalDialogWindow.GetChildByID((uint)ControlIDs.SeeLocationButton, true) as Button;
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
                SimDescription description = sim.SimDescription;
                Button button = mModalDialogWindow.GetChildByID((uint)ControlIDs.ButtonOK, true) as Button;
                button.Enabled = false;
                string name = (occupation as Career)?.SharedData.Name.Substring(34) ?? occupation.Guid.ToString();
                if (!occupation.CanAcceptCareer(model.SimGuid, ref greyedOutTooltipCallback))
                {
                    button.TooltipText = greyedOutTooltipCallback is not null ? greyedOutTooltipCallback() : LocalizeString("NotCorrectAgeForOccupation");
                    if (button.TooltipText == Localization.LocalizeString(sim.IsFemale, "Gameplay/Occupation:GreyedOutUiTooltipAlreadyHasOccupation", description))
                    {
                        if ((!occupation.IsActive && mCareerSelectionModel.mAvailableCareersLocationsEx[mCurrentIndex].CareerLocation?.Owner == sim.CareerLocation) || occupation.IsActive)
                        {
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                if (InterviewLists.TryGetValue(description.SimDescriptionId, out List<InterviewData> list))
                {
                    foreach (InterviewData data in list)
                    {
                        if (data.ActorId == description.SimDescriptionId && data.CareerName == occupation.Guid && data.RabbitHole == mCareerSelectionModel.mAvailableCareersLocationsEx[mCurrentIndex].CareerLocation.Owner)
                        {
                            button.TooltipText = LocalizeString(sim.IsFemale, "AlreadyHaveInterview");
                            return;
                        }
                    }
                }
                if (entry is Career career && !career.CareerAgeTest(description))
                {
                    button.TooltipText = entry is PTFilm && description.CelebrityLevel < CelebrityManager.LowestVisibleLevel
                        ? LocalizeString(sim.IsFemale, "NeedsCelebrityLevel")
                        : LocalizeString(sim.IsFemale, "NotCorrectAgeForOccupation");
                    return;
                }
                if (entry is ActiveCareer && description.TeenOrBelow)
                {
                    button.TooltipText = LocalizeString(sim.IsFemale, "TooYoungForProfession");
                    return;
                }
                if (GameUtils.IsInstalled(ProductVersion.EP9) && Settings.CareerAvailabilityMap.TryGetValue(name, out PersistedSettings.CareerAvailabilitySettings settings) && settings.RequiredDegrees.Count > 0)
                {
                    foreach (AcademicDegreeNames degree in settings.RequiredDegrees)
                    {
                        if (sim.DegreeManager is null || !sim.DegreeManager.HasCompletedDegree(degree))
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
            for (ControlIDs id = ControlIDs.TextDaySunday; id <= ControlIDs.TextDaySaturday; id += 1u)
            {
                Text text = mModalDialogWindow.GetChildByID((uint)id, true) as Text;
                text.Visible = true;
                text.TextColor = kDayTextNotWorkingColor;
                text.AutoSize(false);
            }
            char[] array = workDays.ToCharArray();
            for (int i = 0; i < array.Length; i++)
            {
                ControlIDs id2 = array[i] switch
                {
                    'U' => ControlIDs.TextDaySunday,
                    'M' => ControlIDs.TextDayMonday,
                    'T' => ControlIDs.TextDayTuesday,
                    'W' => ControlIDs.TextDayWednesday,
                    'R' => ControlIDs.TextDayThursday,
                    'F' => ControlIDs.TextDayFriday,
                    'S' => ControlIDs.TextDaySaturday,
                    _   => default
                };
                if (id2 > 0u)
                {
                    Text text2 = mModalDialogWindow.GetChildByID((uint)id2, true) as Text;
                    if (text2 is not null)
                    {
                        text2.TextColor = kDayTextWorkingColor;
                    }
                }
            }
        }

        private void HideWorkDays()
        {
            for (ControlIDs id = ControlIDs.TextDaySunday; id <= ControlIDs.TextDaySaturday; id += 1u)
            {
                Text text = mModalDialogWindow.GetChildByID((uint)id, true) as Text;
                text.Visible = false;
            }
        }
    }
}
