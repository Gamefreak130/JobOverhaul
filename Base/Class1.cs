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
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.MapTags;
using Sims3.Gameplay.Objects;
using Sims3.Gameplay.Objects.Electronics;
using Sims3.Gameplay.Objects.Miscellaneous;
using Sims3.Gameplay.Objects.RabbitHoles;
using Sims3.Gameplay.Objects.Seating;
using Sims3.Gameplay.Opportunities;
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
using static Gamefreak130.JobOverhaulSpace.Helpers.Listeners;
using static Gamefreak130.JobOverhaulSpace.Helpers.Methods;
using static Gamefreak130.JobOverhaulSpace.Interactions.Interviews;
using static Sims3.Gameplay.ActiveCareer.ActiveCareers.DaycareTransportSituation;
using static Sims3.Gameplay.GlobalFunctions;
using static Sims3.Gameplay.Queries;
using static Sims3.UI.ObjectPicker;
using static Sims3.UI.StyledNotification;
using Methods = Gamefreak130.Common.Methods;
using UI = Gamefreak130.Common.UI;

namespace Gamefreak130.Common
{
    public static class Tunings
    {
        internal static InteractionTuning Inject(Type oldType, Type oldTarget, Type newType, Type newTarget, bool clone)
        {
            InteractionTuning interactionTuning = null;
            InteractionTuning result;
            try
            {
                interactionTuning = AutonomyTuning.GetTuning(newType.FullName, newTarget.FullName);
                bool flag = interactionTuning == null;
                if (flag)
                {
                    interactionTuning = AutonomyTuning.GetTuning(oldType, oldType.FullName, oldTarget);
                    bool flag2 = interactionTuning == null;
                    if (flag2)
                    {
                        result = null;
                        return result;
                    }
                    if (clone)
                    {
                        interactionTuning = CloneTuning(interactionTuning);
                    }
                    AutonomyTuning.AddTuning(newType.FullName, newTarget.FullName, interactionTuning);
                }
                InteractionObjectPair.sTuningCache.Remove(new Pair<Type, Type>(newType, newTarget));
            }
            catch (Exception)
            {
            }
            result = interactionTuning;
            return result;
        }

        private static InteractionTuning CloneTuning(InteractionTuning oldTuning)
        {
            return new InteractionTuning
            {
                mFlags = oldTuning.mFlags,
                ActionTopic = oldTuning.ActionTopic,
                AlwaysChooseBest = oldTuning.AlwaysChooseBest,
                Availability = CloneAvailability(oldTuning.Availability),
                CodeVersion = oldTuning.CodeVersion,
                FullInteractionName = oldTuning.FullInteractionName,
                FullObjectName = oldTuning.FullObjectName,
                mChecks = Methods.CloneList(oldTuning.mChecks),
                mTradeoff = CloneTradeoff(oldTuning.mTradeoff),
                PosturePreconditions = oldTuning.PosturePreconditions,
                ScoringFunction = oldTuning.ScoringFunction,
                ScoringFunctionOnlyAppliesToSpecificCommodity = oldTuning.ScoringFunctionOnlyAppliesToSpecificCommodity,
                ScoringFunctionString = oldTuning.ScoringFunctionString,
                ShortInteractionName = oldTuning.ShortInteractionName,
                ShortObjectName = oldTuning.ShortObjectName
            };
        }

        private static Tradeoff CloneTradeoff(Tradeoff old)
        {
            return new Tradeoff
            {
                mFlags = old.mFlags,
                mInputs = Methods.CloneList(old.mInputs),
                mName = old.mName,
                mNumParameters = old.mNumParameters,
                mOutputs = Methods.CloneList(old.mOutputs),
                mVariableRestrictions = old.mVariableRestrictions,
                TimeEstimate = old.TimeEstimate
            };
        }

        private static Availability CloneAvailability(Availability old)
        {
            return new Availability
            {
                mFlags = old.mFlags,
                AgeSpeciesAvailabilityFlags = old.AgeSpeciesAvailabilityFlags,
                CareerThresholdType = old.CareerThresholdType,
                CareerThresholdValue = old.CareerThresholdValue,
                ExcludingBuffs = Methods.CloneList(old.ExcludingBuffs),
                ExcludingTraits = Methods.CloneList(old.ExcludingTraits),
                MoodThresholdType = old.MoodThresholdType,
                MoodThresholdValue = old.MoodThresholdValue,
                MotiveThresholdType = old.MotiveThresholdType,
                MotiveThresholdValue = old.MotiveThresholdValue,
                RequiredBuffs = Methods.CloneList(old.RequiredBuffs),
                RequiredTraits = Methods.CloneList(old.RequiredTraits),
                SkillThresholdType = old.SkillThresholdType,
                SkillThresholdValue = old.SkillThresholdValue,
                WorldRestrictionType = old.WorldRestrictionType,
                OccultRestrictions = old.OccultRestrictions,
                OccultRestrictionType = old.OccultRestrictionType,
                SnowLevelValue = old.SnowLevelValue,
                WorldRestrictionWorldNames = Methods.CloneList(old.WorldRestrictionWorldNames),
                WorldRestrictionWorldTypes = Methods.CloneList(old.WorldRestrictionWorldTypes)
            };
        }
    }

    public class BuffBooter
    {
        public string mXmlResource;

        public BuffBooter(string xmlResource)
        {
            mXmlResource = xmlResource;
        }

        public void LoadBuffData()
        {
            AddBuffs(null);
            UIManager.NewHotInstallStoreBuffData += new UIManager.NewHotInstallStoreBuffCallback(AddBuffs);
        }

        public void AddBuffs(ResourceKey[] resourceKeys)
        {
            XmlDbData xmlDbData = XmlDbData.ReadData(mXmlResource);
            if (xmlDbData != null)
            {
                BuffManager.ParseBuffData(xmlDbData, true);
            }
        }
    }

    public static class Methods
    {
        public static void InjectInteraction<Target>(ref InteractionDefinition singleton, InteractionDefinition newSingleton, bool requiresTuning) where Target : IGameObject
        {
            if (requiresTuning)
            {
                Tunings.Inject(singleton.GetType(), typeof(Target), newSingleton.GetType(), typeof(Target), true);
            }
            singleton = newSingleton;
        }

        public static void AddInteraction(GameObject gameObject, InteractionDefinition singleton)
        {
            foreach (InteractionObjectPair iop in gameObject.Interactions)
            {
                if (iop.InteractionDefinition.GetType() == singleton.GetType())
                {
                    return;
                }
            }
            if (gameObject.ItemComp != null && gameObject.ItemComp.InteractionsInventory != null)
            {
                foreach (InteractionObjectPair iop in gameObject.ItemComp.InteractionsInventory)
                {
                    if (iop.InteractionDefinition.GetType() == singleton.GetType())
                    {
                        return;
                    }
                }
            }
            gameObject.AddInteraction(singleton);
            gameObject.AddInventoryInteraction(singleton);
        }

        public static List<T> CloneList<T>(IEnumerable<T> old)
        {
            bool flag = old != null;
            List<T> result = flag ? new List<T>(old) : null;
            return result;
        }
    }

    public delegate T GenericDelegate<T>();
}

namespace Gamefreak130.Common.UI
{
    public struct PreviousMenuInfo
    {
        private readonly object mMenuID;

        private readonly int mMenuTab;

        public object MenuID
        {
            get
            {
                return mMenuID;
            }
        }

        public int MenuTab
        {
            get
            {
                return mMenuTab;
            }
        }

        public PreviousMenuInfo(object menuID, int tabNumber)
        {
            mMenuID = menuID;
            mMenuTab = tabNumber;
        }
    }

    public struct ColumnDelegateStruct
    {
        private readonly ColumnType mColumnType;

        private readonly GenericDelegate<ColumnInfo> mInfo;

        public ColumnType ColumnType
        {
            get
            {
                return mColumnType;
            }
        }

        public GenericDelegate<ColumnInfo> Info
        {
            get
            {
                return mInfo;
            }
        }

        public ColumnDelegateStruct(ColumnType ColType, GenericDelegate<ColumnInfo> infoDelegate)
        {
            mColumnType = ColType;
            mInfo = infoDelegate;
        }
    }

    public class RowTextFormat
    {
        private Color mTextColor;

        private bool mBoldTextStyle;

        private string mTooltip;

        public Color TextColor
        {
            get
            {
                return mTextColor;
            }

            set
            {
                mTextColor = value;
            }
        }

        public bool BoldTextStyle
        {
            get
            {
                return mBoldTextStyle;
            }

            set
            {
                mBoldTextStyle = value;
            }
        }

        public string Tooltip
        {
            get
            {
                return mTooltip;
            }

            set
            {
                mTooltip = value;
            }
        }

        public RowTextFormat(Color TextColor, bool BoldText, string TooltipText)
        {
            mTextColor = TextColor;
            mBoldTextStyle = BoldText;
            mTooltip = TooltipText;
        }
    }

    public class MenuContainer
    {
        private List<HeaderInfo> mHeaders;

        private List<RowInfo> mRowInformation;

        private List<TabInfo> mTabInformation;

        public string[] mTabImage;

        public string[] mTabText;

        private readonly string mMenuDisplayName;

        private readonly int mSelectable = 1;

        private readonly Action<List<RowInfo>> mOnEnd;

        private readonly GenericDelegate<List<RowInfo>> mRowPopulationDelegate;

        private readonly List<RowInfo> mHiddenRows;

        public string MenuDisplayName
        {
            get
            {
                return mMenuDisplayName;
            }
        }

        public int Selectable
        {
            get
            {
                return mSelectable;
            }
        }

        public List<HeaderInfo> Headers
        {
            get
            {
                return mHeaders;
            }
        }

        public List<TabInfo> TabInformation
        {
            get
            {
                return mTabInformation;
            }
        }

        public Action<List<RowInfo>> OnEnd
        {
            get
            {
                return mOnEnd;
            }
        }

        public MenuContainer()
        {
        }

        public MenuContainer(string Title)
        {
            mHiddenRows = new List<RowInfo>();
            mMenuDisplayName = Title;
            mTabImage = new string[]
                {
                    ""
                };
            mTabText = new string[]
                {
                    ""
                };
            mOnEnd = null;
            mHeaders = new List<HeaderInfo>();
            mRowInformation = new List<RowInfo>();
            mTabInformation = new List<TabInfo>();
        }

        public MenuContainer(string Title, string Subtitle)
        {
            mHiddenRows = new List<RowInfo>();
            mMenuDisplayName = Title;
            mTabImage = new string[]
                {
                    ""
                };
            mTabText = new string[]
                {
                    Subtitle
                };
            mOnEnd = null;
            mHeaders = new List<HeaderInfo>();
            mRowInformation = new List<RowInfo>();
            mTabInformation = new List<TabInfo>();
        }

        public MenuContainer(string Title, string[] TabImage, string[] TabName, int numSelectable, Action<List<RowInfo>> OnEndDelegate)
        {
            mHiddenRows = new List<RowInfo>();
            mMenuDisplayName = Title;
            mTabImage = TabImage;
            mTabText = TabName;
            mSelectable = numSelectable;
            mOnEnd = OnEndDelegate;
            mHeaders = new List<HeaderInfo>();
            mRowInformation = new List<RowInfo>();
            mTabInformation = new List<TabInfo>();
        }

        public MenuContainer(string Title, string[] TabImage, string[] TabName, int numSelectable, Action<List<RowInfo>> OnEndDelegate, GenericDelegate<List<RowInfo>> RowPopulationDelegate)
        {
            mHiddenRows = new List<RowInfo>();
            mMenuDisplayName = Title;
            mTabImage = TabImage;
            mTabText = TabName;
            mSelectable = numSelectable;
            mOnEnd = OnEndDelegate;
            mRowPopulationDelegate = RowPopulationDelegate;
            mHeaders = new List<HeaderInfo>();
            mRowInformation = new List<RowInfo>();
            mTabInformation = new List<TabInfo>();
            RefreshMenuObjects(0);
            if (mRowInformation.Count > 0)
            {
                for (int i = 0; i < mRowInformation[0].ColumnInfo.Count; i++)
                {
                    mHeaders.Add(new HeaderInfo("Ui/Caption/ObjectPicker:Sim", "", 200));
                }
            }
        }

        public void RefreshMenuObjects(int Tabnumber)
        {
            mRowInformation = mRowPopulationDelegate();
            mTabInformation = new List<TabInfo>
            {
                new TabInfo("", mTabText[Tabnumber], mRowInformation)
            };
        }

        public void SetHeaders(List<HeaderInfo> headers)
        {
            mHeaders = headers;
        }

        public void SetHeader(int HeaderNumber, HeaderInfo HeaderInfos)
        {
            mHeaders[HeaderNumber] = HeaderInfos;
        }

        public void ClearMenuObjects()
        {
            mTabInformation.Clear();
        }

        public void AddMenuObject(MenuObject MenuItem)
        {
            if (mTabInformation.Count < 1)
            {
                mRowInformation = new List<RowInfo>
                {
                    MenuItem.RowInformation
                };
                mTabInformation.Add(new TabInfo(mTabImage[0], mTabText[0], mRowInformation));
                mHeaders.Add(new HeaderInfo("Ui/Caption/ObjectPicker:Name", "", 300));
                mHeaders.Add(new HeaderInfo("Ui/Caption/ObjectPicker:Value", "", 100));
                return;
            }
            mTabInformation[0].RowInfo.Add(MenuItem.RowInformation);
        }

        public void AddMenuObject(List<HeaderInfo> headers, MenuObject MenuItem)
        {

            if (mTabInformation.Count < 1)
            {
                mRowInformation = new List<RowInfo>
                {
                    MenuItem.RowInformation
                };
                mTabInformation.Add(new TabInfo(mTabImage[0], mTabText[0], mRowInformation));
                mHeaders = headers;
                return;
            }
            mTabInformation[0].RowInfo.Add(MenuItem.RowInformation);
            mHeaders = headers;
        }

        public void AddMenuObject(List<HeaderInfo> headers, RowInfo item)
        {
            if (mTabInformation.Count < 1)
            {
                mRowInformation = new List<RowInfo>
                {
                    item
                };
                mTabInformation.Add(new TabInfo(mTabImage[0], mTabText[0], mRowInformation));
                mHeaders = headers;
                return;
            }
            mTabInformation[0].RowInfo.Add(item);
            mHeaders = headers;
        }

        public void UpdateRows()
        {
            for (int i = mHiddenRows.Count - 1; i >= 0; i--)
            {
                MenuObject item = mHiddenRows[i].Item as MenuObject;
                if (item.Test())
                {
                    mHiddenRows.RemoveAt(i);
                    AddMenuObject(item);
                }
            }
            for (int i = mTabInformation[0].RowInfo.Count - 1; i >= 0; i--)
            {
                MenuObject item = mTabInformation[0].RowInfo[i].Item as MenuObject;
                if (item.Test != null && !item.Test())
                {
                    mHiddenRows.Add(mTabInformation[0].RowInfo[i]);
                    mTabInformation[0].RowInfo.RemoveAt(i);
                }
            }
        }
    }

    public class MenuController : ModalDialog
    {
        private const int ITEM_TABLE = 99576784;

        private const int OKAY_BUTTON = 99576785;

        private const int CANCEL_BUTTON = 99576786;

        private const int TITLE_TEXT = 99576787;

        private const int TABLE_BACKGROUND = 99576788;

        private const int TABLE_BEZEL = 99576789;

        private const string kLayoutName = "UiObjectPicker";

        private const int kWinExportID = 1;

        private Vector2 mTableOffset;

        private ObjectPicker mTable;

        private Button mOkayButton;

        private Button mCloseButton;

        private TabContainer mTabsContainer;

        private bool mOkay;

        private List<RowInfo> mResult;

        private Action<List<RowInfo>> mEndDelegates;

        public bool Okay
        {
            get
            {
                return mOkay;
            }
        }

        public List<RowInfo> Result
        {
            get
            {
                return mResult;
            }
        }

        public Action<List<RowInfo>> EndDelegates
        {
            get
            {
                return mEndDelegates;
            }
        }

        public void ShowModal()
        {
            mModalDialogWindow.Moveable = true;
            StartModal();
        }

        public void Stop()
        {
            StopModal();
        }

        public MenuController(string title, string buttonTrue, string buttonFalse, List<TabInfo> listObjs, List<HeaderInfo> headers, int numSelectableRows, bool showHeadersAndToggle, Action<List<RowInfo>> EndResultDelegate) : this(true, PauseMode.PauseSimulator, title, buttonTrue, buttonFalse, listObjs, headers, numSelectableRows, showHeadersAndToggle)
        {
            mEndDelegates = EndResultDelegate;
        }

        public MenuController(bool modal, PauseMode pauseType, string title, string buttonTrue, string buttonFalse, List<TabInfo> listObjs, List<HeaderInfo> headers, int numSelectableRows, bool showHeadersAndToggle) : base("UiObjectPicker", 1, true, PauseMode.PauseSimulator, null)
        {
            if (mModalDialogWindow == null)
            {
                return;
            }
            Text text = mModalDialogWindow.GetChildByID(99576787u, false) as Text;
            text.Caption = title;
            mTable = mModalDialogWindow.GetChildByID(99576784u, false) as ObjectPicker;
            mTable.SelectionChanged += new ObjectPickerSelectionChanged(OnRowClicked);
            mTabsContainer = mTable.mTabs;
            mOkayButton = mModalDialogWindow.GetChildByID(99576785u, false) as Button;
            mOkayButton.TooltipText = buttonTrue;
            mOkayButton.Enabled = true;
            mOkayButton.Click += new UIEventHandler<UIButtonClickEventArgs>(OnOkayButtonClick);
            OkayID = mOkayButton.ID;
            SelectedID = mOkayButton.ID;
            mCloseButton = mModalDialogWindow.GetChildByID(99576786u, false) as Button;
            mCloseButton.TooltipText = buttonFalse;
            mCloseButton.Click += new UIEventHandler<UIButtonClickEventArgs>(OnCloseButtonClick);
            CancelID = mCloseButton.ID;
            mTableOffset = mModalDialogWindow.Area.BottomRight - mModalDialogWindow.Area.TopLeft - (mTable.Area.BottomRight - mTable.Area.TopLeft);
            mTable.ShowHeaders = true;
            mTable.ViewTypeToggle = false;
            mTable.ShowToggle = false;
            mTable.Populate(listObjs, headers, numSelectableRows);
            UpdateItems();
            ResizeWindow();
        }

        private void PopulateMenu(List<TabInfo> tabinfo, List<HeaderInfo> headers, int numSelectableRows)
        {
            mTable.Populate(tabinfo, headers, numSelectableRows);
        }

        public override void Dispose()
        {
            Dispose(true);
        }

        public void AddRow(int Tabnumber, RowInfo rinfo)
        {
            mTable.mItems[Tabnumber].RowInfo.Clear();
            mTable.mItems[Tabnumber].RowInfo.Add(rinfo);
            Repopulate();
        }

        public void UpdateItems()
        {
            if (mTable == null)
            {
                return;
            }
            foreach (TabInfo current in mTable.mItems)
            {
                foreach (RowInfo current2 in current.RowInfo)
                {
                    if (current2.Item is MenuObject)
                    {
                        (current2.Item as MenuObject).UpdateMenuObject();
                    }
                }
            }
            Repopulate();
            mTable.mTable.ScrollRowToTop(15);
        }

        public void SetTableColor(Color color)
        {
            mModalDialogWindow.GetChildByID(99576789u, false).ShadeColor = color;
        }

        public void SetTitleText(string Text)
        {
            (mModalDialogWindow.GetChildByID(99576787u, false) as Text).Caption = Text;
        }

        public void SetTitleText(string Text, Color TextColor)
        {
            (mModalDialogWindow.GetChildByID(99576787u, false) as Text).Caption = Text;
            (mModalDialogWindow.GetChildByID(99576787u, false) as Text).TextColor = TextColor;
        }

        public void SetTitleText(string Text, Color TextColor, uint TextStyle)
        {
            (mModalDialogWindow.GetChildByID(99576787u, false) as Text).Caption = Text;
            (mModalDialogWindow.GetChildByID(99576787u, false) as Text).TextColor = TextColor;
            (mModalDialogWindow.GetChildByID(99576787u, false) as Text).TextStyle = TextStyle;
        }

        public void SetTitleText(string Text, Color TextColor, bool TextStyleBold)
        {
            (mModalDialogWindow.GetChildByID(99576787u, false) as Text).Caption = Text;
            (mModalDialogWindow.GetChildByID(99576787u, false) as Text).TextColor = TextColor;
            (mModalDialogWindow.GetChildByID(99576787u, false) as Text).TextStyle = 2u;
        }

        public void SetTitleTextColor(Color TextColor)
        {
            (mModalDialogWindow.GetChildByID(99576787u, false) as Text).TextColor = TextColor;
        }

        public void SetTitleTextStyle(uint TextStyle)
        {
            (mModalDialogWindow.GetChildByID(99576787u, false) as Text).TextStyle = TextStyle;
        }

        private void Repopulate()
        {
            if (mTable.RepopulateTable())
            {
                ResizeWindow();
            }
        }

        private void ResizeWindow()
        {
            Rect area = mModalDialogWindow.Parent.Area;
            float width = area.Width;
            float height = area.Height;
            int num = (int)height - (int)(mTableOffset.y * 2f);
            num /= (int)mTable.mTable.RowHeight;
            if (num > mTable.mTable.NumberRows)
            {
                num = mTable.mTable.NumberRows;
            }
            mTable.mTable.VisibleRows = (uint)num;
            mTable.mTable.GridSizeDirty = true;
            mTable.OnPopulationComplete();
            mModalDialogWindow.Area = new Rect(mModalDialogWindow.Area.TopLeft, mModalDialogWindow.Area.TopLeft + mTable.TableArea.BottomRight + mTableOffset);
            Rect area2 = mModalDialogWindow.Area;
            float width2 = area2.Width;
            float height2 = area2.Height;
            float num2 = (float)Math.Round((width - width2) / 2f);
            float num3 = (float)Math.Round((height - height2) / 2f);
            area2.Set(num2, num3, num2 + width2, num3 + height2);
            mModalDialogWindow.Area = area2;
            Text text = mModalDialogWindow.GetChildByID(99576787u, false) as Text;
            Rect area3 = text.Area;
            area3.Set(area3.TopLeft.x, 20f, area3.BottomRight.x, 50f - area2.Height);
            text.Area = area3;
            mModalDialogWindow.Visible = true;
        }

        private void OnRowClicked(List<RowInfo> selectedRows)
        {
            Audio.StartSound("ui_tertiary_button");
            EndDialog(OkayID);
        }

        private void OnCloseButtonClick(WindowBase sender, UIButtonClickEventArgs eventArgs)
        {
            eventArgs.Handled = true;
            EndDialog(CancelID);
        }

        private void OnOkayButtonClick(WindowBase sender, UIButtonClickEventArgs eventArgs)
        {
            eventArgs.Handled = true;
            EndDialog(OkayID);
        }

        public override void EndDialog(uint endID)
        {
            if (OnEnd(endID))
            {
                StopModal();
                Dispose();
            }
            mTable = null;
            mModalDialogWindow = null;
        }

        public override bool OnEnd(uint endID)
        {
            if (endID == OkayID)
            {
                mEndDelegates?.Invoke(mTable.Selected);
                mResult = mTable.Selected;
                mOkay = true;
            }
            else
            {
                mResult = null;
                mOkay = false;
            }
            mTable.Populate(null, null, 0);
            mEndDelegates = null;
            return true;
        }

        public static bool ShowMenu(MenuContainer container)
        {
            while (true)
            {
                container.UpdateRows();
                MenuController controller = Show(container);
                if (controller.Okay)
                {
                    if (controller.Result != null)
                    {
                        foreach (RowInfo current in controller.Result)
                        {
                            if (((MenuObject)current.Item).OnActivation())
                            {
                                return true;
                            }
                        }
                        continue;
                    }
                    return true;
                }
                return false;
            }
        }

        public static bool ShowMenu(MenuContainer container, int tab)
        {
            while (true)
            {
                container.UpdateRows();
                MenuController controller = Show(container);
                if (controller.Okay)
                {
                    if (controller.Result != null)
                    {
                        foreach (RowInfo current in controller.Result)
                        {
                            if (((MenuObject)current.Item).OnActivation())
                            {
                                return true;
                            }
                        }
                        continue;
                    }
                    return true;
                }
                return false;
            }
        }

        private static MenuController Show(MenuContainer container)
        {
            MenuController menuController = new MenuController(container.MenuDisplayName, Localization.LocalizeString("Ui/Caption/Global:Ok", new object[0]), Localization.LocalizeString("Ui/Caption/Global:Cancel", new object[0]), container.TabInformation, container.Headers, container.Selectable, true, container.OnEnd);
            menuController.SetTitleTextStyle(2u);
            menuController.ShowModal();
            return menuController;
        }

        private static MenuController Show(MenuContainer container, int tab)
        {
            Sims3.Gameplay.Gameflow.SetGameSpeed(Gameflow.GameSpeed.Pause, Sims3.Gameplay.Gameflow.SetGameSpeedContext.GameStates);
            MenuController menuController = new MenuController(container.MenuDisplayName, Localization.LocalizeString("Ui/Caption/Global:Ok", new object[0]), Localization.LocalizeString("Ui/Caption/Global:Cancel", new object[0]), container.TabInformation, container.Headers, container.Selectable, true, container.OnEnd);
            menuController.SetTitleTextStyle(2u);
            if (tab >= 0)
            {
                if (tab < menuController.mTabsContainer.mTabs.Count)
                {
                    menuController.mTabsContainer.SelectTab(menuController.mTabsContainer.mTabs[tab]);
                }
                else
                {
                    menuController.mTabsContainer.SelectTab(menuController.mTabsContainer.mTabs[menuController.mTabsContainer.mTabs.Count - 1]);
                }
            }
            menuController.ShowModal();
            return menuController;
        }
    }

    public abstract class MenuObject : IDisposable
    {
        private RowInfo mRowInformation;

        private List<ColumnInfo> mColumnInfoList;

        protected List<ColumnDelegateStruct> mColumnActions;

        public readonly RowTextFormat mTextFormat;

        private GenericDelegate<bool> mTest;

        public RowInfo RowInformation
        {
            get
            {
                return mRowInformation;
            }
        }

        public GenericDelegate<bool> Test
        {
            get
            {
                return mTest;
            }

            set
            {
                mTest = value;
            }
        }

        public MenuObject()
        {
            mColumnInfoList = new List<ColumnInfo>();
            mColumnActions = new List<ColumnDelegateStruct>();
        }

        public void Fillin()
        {
            mRowInformation = new RowInfo(this, mColumnInfoList);
        }

        public void Fillin(Color TextColor)
        {
            mTextFormat.TextColor = TextColor;
            Fillin();
        }

        public void Fillin(Color TextColor, bool BoldTextStyle)
        {
            mTextFormat.TextColor = TextColor;
            mTextFormat.BoldTextStyle = BoldTextStyle;
            Fillin();
        }

        public void Fillin(bool BoldTextStyle)
        {
            mTextFormat.BoldTextStyle = BoldTextStyle;
            Fillin();
        }

        public void Fillin(string TooltipText)
        {
            mTextFormat.Tooltip = TooltipText;
            Fillin();
        }

        public void Dispose()
        {
            mRowInformation = null;
            mColumnInfoList.Clear();
            mColumnInfoList = null;
        }

        public virtual void PopulateColumnInfo()
        {
            foreach (ColumnDelegateStruct column in mColumnActions)
            {
                mColumnInfoList.Add(column.Info());
            }
        }

        public virtual void AdaptToMenu(TabInfo tabinfo)
        {
            List<ColumnInfo> columnInfo = tabinfo.RowInfo[0].ColumnInfo;
            foreach (ColumnInfo info in columnInfo)
            {
            }
        }

        public virtual bool OnActivation()
        {
            return true;
        }

        public void UpdateMenuObject()
        {
            for (int i = 0; i < mColumnInfoList.Count; i++)
            {
                mColumnInfoList[i] = mColumnActions[i].Info();
            }
            Fillin();
        }
    }

    public class GenericActionObject : MenuObject
    {
        private readonly Function mCallback;

        public Function Callback
        {
            get
            {
                return mCallback;
            }
        }

        public GenericActionObject(string name, GenericDelegate<string> getValue, GenericDelegate<bool> test, Function action)
        {
            mCallback = action;
            Test = test;
            mColumnActions = new List<ColumnDelegateStruct>
            {
                new ColumnDelegateStruct(ColumnType.kText, delegate () { return new TextColumn(name); }),
                new ColumnDelegateStruct(ColumnType.kText, delegate () { return new TextColumn(getValue()); })
            };
            PopulateColumnInfo();
            Fillin();
        }

        public GenericActionObject(List<ColumnDelegateStruct> columns, GenericDelegate<bool> test, Function action)
        {
            mCallback = action;
            Test = test;
            mColumnActions = columns;
            PopulateColumnInfo();
            Fillin();
        }

        public override bool OnActivation()
        {
            try
            {
                mCallback();
            }
            catch
            {
            }
            return false;
        }
    }

    public sealed class GenerateMenuObject : MenuObject
    {
        private readonly MenuContainer mToOpen;

        public GenerateMenuObject(string name, MenuContainer toOpen)
        {
            mToOpen = toOpen;
            mColumnActions = new List<ColumnDelegateStruct>
            {
                new ColumnDelegateStruct(ColumnType.kText, delegate () { return new TextColumn(name); }),
                new ColumnDelegateStruct(ColumnType.kText, delegate () { return new TextColumn(""); })
            };
            PopulateColumnInfo();
            Fillin();
        }

        public GenerateMenuObject(List<ColumnDelegateStruct> columns, MenuContainer toOpen)
        {
            mToOpen = toOpen;
            mColumnActions = columns;
            PopulateColumnInfo();
            Fillin();
        }

        public override bool OnActivation()
        {
            try
            {
                if (MenuController.ShowMenu(mToOpen))
                {
                    return true;
                }
            }
            catch
            {
            }
            return false;
        }
    }

    public sealed class OneShotActionObject : GenericActionObject
    {
        public OneShotActionObject(string name, GenericDelegate<string> getValue, GenericDelegate<bool> test, Function action) : base(name, getValue, test, action)
        {
        }

        public OneShotActionObject(List<ColumnDelegateStruct> columns, GenericDelegate<bool> test, Function action) : base(columns, test, action)
        {
        }

        public override bool OnActivation()
        {
            try
            {
                Callback();
            }
            catch
            {
            }
            return true;
        }
    }

    public sealed class ResetActionObject : GenericActionObject
    {
        private readonly GenericDelegate<bool> mCallback;

        public ResetActionObject(string name, GenericDelegate<string> getValue, GenericDelegate<bool> test, GenericDelegate<bool> action) : base(name, getValue, test, null)
        {
            mCallback = action;
        }

        public ResetActionObject(List<ColumnDelegateStruct> columns, GenericDelegate<bool> test, GenericDelegate<bool> action) : base(columns, test, null)
        {
            mCallback = action;
        }

        public override bool OnActivation()
        {
            try
            {
                if (mCallback())
                {
                    return true;
                }
            }
            catch
            {
            }
            return false;
        }
    }
}

namespace Gamefreak130
{
    public class JobOverhaul
    {
        [Tunable]
        private static bool kCJackB;

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
            //TODO Add Italian translation
            //CONSIDER Audition string?
            //CONSIDER Random amount of jobs per day from specified min to max?
            //CONSIDER Possible to reject careers after canceling an interview? Would require a static dictionary, and you know how fun those are to manage...
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
            EventTracker.AddListener(EventTypeId.kSimDied, new ProcessEventDelegate(OnSimDied));
            EventTracker.AddListener(EventTypeId.kSentSimToBoardingSchool, new ProcessEventDelegate(OnSimInBoardingSchool));
            EventTracker.AddListener(EventTypeId.kSimEnteredVacationWorld, new ProcessEventDelegate(OnTravelComplete));
            EventTracker.AddListener(EventTypeId.kSimReturnedFromVacationWorld, new ProcessEventDelegate(OnTravelComplete));
            EventTracker.AddListener(EventTypeId.kSimCompletedOccupationTask, new ProcessEventDelegate(OnTaskCompleted));
            EventTracker.AddListener(EventTypeId.kCareerPerformanceChanged, new ProcessEventDelegate(OnPerformanceChange));
            EventTracker.AddListener(EventTypeId.kFinishedWork, new ProcessEventDelegate(OnFinishedWork));
            EventTracker.AddListener(EventTypeId.kTravelToFuture, new ProcessEventDelegate(SaveOccupationForTravel));
            EventTracker.AddListener(EventTypeId.kTravelToPresent, new ProcessEventDelegate(SaveOccupationForTravel));
        }

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
                    if (!Settings.mCareerAvailabilitySettings.ContainsKey(name))
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
                    if (!Settings.mSelfEmployedAvailabilitySettings.ContainsKey(name))
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
                        if (!string.IsNullOrEmpty(name) && !Settings.mInterviewSettings.ContainsKey(name) && newCareers.Contains(name))
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
                            PersistedSettings.InterviewSettings interviewData = new PersistedSettings.InterviewSettings(requiresInterview, posTraits, negTraits, skills);
                            Settings.mInterviewSettings.Add(name, interviewData);
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
                                if (!Settings.mCareerAvailabilitySettings.ContainsKey(name))
                                {
                                    PersistedSettings.CareerAvailabilitySettings settings = new PersistedSettings.CareerAvailabilitySettings(isAvailable, false, degrees);
                                    Settings.mCareerAvailabilitySettings.Add(name, settings);
                                }
                                continue;
                            }
                            if (newActiveCareers.Contains(name))
                            {
                                newActiveCareers.Remove(name);
                                PersistedSettings.CareerAvailabilitySettings settings = new PersistedSettings.CareerAvailabilitySettings(isAvailable, true, degrees);
                                Settings.mCareerAvailabilitySettings.Add(name, settings);
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
                        string name = row.TryGetEnum("CareerName", out OccupationNames occupationNames, OccupationNames.Undefined) ? row.GetString("CareerName") : ResourceUtils.HashString64(row.GetString("CareerName")).ToString();
                        if (!string.IsNullOrEmpty(name))
                        {
                            bool isAvailable = row.GetBool("Enabled");
                            if (newSelfEmployedJobs.Contains(name))
                            {
                                newSelfEmployedJobs.Remove(name);
                                Settings.mSelfEmployedAvailabilitySettings.Add(name, isAvailable);
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
                    if (!Settings.mInterviewSettings.ContainsKey(name))
                    {
                        Settings.mInterviewSettings.Add(name, new PersistedSettings.InterviewSettings(true, new List<TraitNames>() { TraitNames.Ambitious }, new List<TraitNames>() { TraitNames.Loser }, new List<SkillNames>()));
                    }
                    if (!Settings.mCareerAvailabilitySettings.ContainsKey(name))
                    {
                        Settings.mCareerAvailabilitySettings.Add(name, new PersistedSettings.CareerAvailabilitySettings(true, false, new List<AcademicDegreeNames>()));
                    }
                }
            }
            foreach (ActiveCareer career in CareerManager.GetActiveCareers())
            {
                string name = career.Guid.ToString();
                if (newActiveCareers.Contains(name))
                {
                    newActiveCareers.Remove(name);
                    Settings.mCareerAvailabilitySettings.Add(name, new PersistedSettings.CareerAvailabilitySettings(true, true, new List<AcademicDegreeNames>()));
                }
            }
            foreach (Occupation occupation in CareerManager.OccupationList)
            {
                string name = occupation.Guid.ToString();
                if (newSelfEmployedJobs.Contains(name))
                {
                    newSelfEmployedJobs.Remove(name);
                    Settings.mSelfEmployedAvailabilitySettings.Add(name, true);
                }
            }

            string[] interviewKeys = new string[Settings.mInterviewSettings.Count];
            string[] careerKeys = new string[Settings.mCareerAvailabilitySettings.Count];
            string[] selfEmployedKeys = new string[Settings.mSelfEmployedAvailabilitySettings.Count];
            Settings.mInterviewSettings.Keys.CopyTo(interviewKeys, 0);
            Settings.mCareerAvailabilitySettings.Keys.CopyTo(careerKeys, 0);
            Settings.mSelfEmployedAvailabilitySettings.Keys.CopyTo(selfEmployedKeys, 0);
            foreach (string key in interviewKeys)
            {
                if (!currentInterviews.Contains(key))
                {
                    Settings.mInterviewSettings.Remove(key);
                }
            }
            foreach (string key in careerKeys)
            {
                if (!currentCareers.Contains(key))
                {
                    Settings.mCareerAvailabilitySettings.Remove(key);
                }
            }
            foreach (string key in selfEmployedKeys)
            {
                if (!currentSelfEmployedJobs.Contains(key))
                {
                    Settings.mSelfEmployedAvailabilitySettings.Remove(key);
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
            ObjectGuid[] keys = new ObjectGuid[Settings.mInterviewSettings.Count];
            RandomNewspaperSeeds.Keys.CopyTo(keys, 0);
            foreach (ObjectGuid id in keys)
            {
                if (GameObject.GetObject(id) == null || (GameObject.GetObject(id) is Newspaper newspaper && newspaper.IsOld))
                {
                    RandomNewspaperSeeds.Remove(id);
                }
            }
        }

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

namespace Gamefreak130.JobOverhaulSpace
{
    [Persistable]
    public class PersistedSettings
    {
        [Persistable]
        public class InterviewSettings
        {
            private bool mRequiresInterview;

            private List<TraitNames> mPositiveTraits;

            private List<TraitNames> mNegativeTraits;

            private List<SkillNames> mRequiredSkills;

            public bool RequiresInterview
            {
                get
                {
                    return mRequiresInterview;
                }

                set
                {
                    mRequiresInterview = value;
                }
            }

            public List<TraitNames> PositiveTraits
            {
                get
                {
                    return mPositiveTraits;
                }

                set
                {
                    mPositiveTraits = value;
                }
            }

            public List<TraitNames> NegativeTraits
            {
                get
                {
                    return mNegativeTraits;
                }

                set
                {
                    mNegativeTraits = value;
                }
            }

            public List<SkillNames> RequiredSkills
            {
                get
                {
                    return mRequiredSkills;
                }

                set
                {
                    mRequiredSkills = value;
                }
            }

            public InterviewSettings()
            {
            }

            public InterviewSettings(bool requiresInterview, List<TraitNames> posTraits, List<TraitNames> negTraits, List<SkillNames> skills)
            {
                mRequiresInterview = requiresInterview;
                mPositiveTraits = posTraits;
                mNegativeTraits = negTraits;
                mRequiredSkills = skills;
            }
        }

        [Persistable]
        public class CareerAvailabilitySettings
        {
            private bool mIsAvailable;

            private bool mIsActive;

            private List<AcademicDegreeNames> mRequiredDegrees;

            public bool IsAvailable
            {
                get
                {
                    return mIsAvailable;
                }
                set
                {
                    mIsAvailable = value;
                }
            }

            public List<AcademicDegreeNames> RequiredDegrees
            {
                get
                {
                    return mRequiredDegrees;
                }
                set
                {
                    mRequiredDegrees = value;
                }
            }

            public bool IsActive
            {
                get
                {
                    return mIsActive;
                }

                set
                {
                    mIsActive = value;
                }
            }

            public CareerAvailabilitySettings()
            {
            }

            public CareerAvailabilitySettings(bool isAvailable, bool isActive, List<AcademicDegreeNames> requiredDegrees)
            {
                mIsAvailable = isAvailable;
                mRequiredDegrees = requiredDegrees;
                mIsActive = isActive;
            }
        }

        [Tunable, TunableComment("True/False: Whether or not to enable the EA default 'Join Career' rabbithole interactions for occupations that do not require an interview")]
        private static bool kEnableGetJobInRabbitHole = true;

        [Tunable, TunableComment("True/False: Whether or not to enable the EA default 'Join Profession' interactions")]
        private static bool kEnableJoinProfessionInRabbitHoleOrLot = false;

        [Tunable, TunableComment("True/False: Whether or not job offers from computers, newspapers, or smartphones will be presented in one menu; if set to true, kHoloComputerInstantGratification and kHoloPhoneInstantGratification are also assumed to be true")]
        private static bool kInstantGratification = false;

        [Tunable, TunableComment("True/False: Whether or not to have the Holocomputer present all job offers in one menu like the crazy future tech it is, rather than through multiple dialogues over time")]
        private static bool kHoloComputerInstantGratification = true;

        [Tunable, TunableComment("True/False: Whether or not to have the Holophone present all job offers in one menu like the crazy future tech it is, rather than through multiple dialogues over time")]
        private static bool kHoloPhoneInstantGratification = true;

        [Tunable, TunableComment("True/False: Whether or not to allow the 'Register as Self-Employed' interaction on newspapers")]
        private static bool kNewspaperSelfEmployed = false;

        [Tunable, TunableComment("How many bonus job offers to award sims with enough blog followers through the 'Upload Resume' interaction")]
        private static int kNumBonusResumeJobs = 1;

        [Tunable, TunableComment("Range 0-24: The hour at which an interview for a full-time job will be scheduled")]
        private static int kFullTimeInterviewHour = 10;

        [Tunable, TunableComment("Range 0-24: The hour at which an interview for a part-time job will be scheduled")]
        private static int kPartTimeInterviewHour = 17;

        [Tunable, TunableComment("Maximum times a sim can postpone a job interview")]
        private static int kMaxInterviewPostpones = 3;

        [Tunable, TunableComment("Range 0-100: Base chance of a sim getting a full-time job after a job interview")]
        private static float kBaseFullTimeJobChance = 30;

        [Tunable, TunableComment("Range 0-100: Base chance of a sim getting a part-time job after a job interview")]
        private static float kBasePartTimeJobChance = 65;

        [Tunable, TunableComment("Range 0-100: How much each postponement of a job interview decreases the chances of getting a job after that interview")]
        private static float kPostponeInterviewChanceChange = 10;

        [Tunable, TunableComment("Range 0-100: How much the 'Ready For Interview' moodlet affects the chance of getting a job after a job interview")]
        private static float kReadyForInterviewChanceChange = 15;

        [Tunable, TunableComment("Range 0-100: How much traits beneficial to a job affect the chance of getting that job after an interview")]
        private static float kPositiveTraitInterviewChanceChange = 10;

        [Tunable, TunableComment("Range 0-100: How much traits detrimental to a job affect the chance of getting that job after an interview")]
        private static float kNegativeTraitInterviewChanceChange = 15;

        [Tunable, TunableComment("Range 0-100: Change in chance of a sim getting a job after an interview per level in that job's required skills")]
        private static float kRequiredSkillInterviewChanceChange = 3;

        [Tunable, TunableComment("Range 0-100: Chance that a sim gets offered a promotion upon leaving work when the performance bar is full")]
        private static float kPromotionChance = 10;

        [Tunable, TunableComment("The amount of time, in sim minutes, that a sim spends inside a rabbithole for a job interview")]
        private static float kInterviewTime = 60;

        [Tunable, TunableComment("The amount of time, in sim minutes, that a sim spends inside a rabbithole to fill out a job application or sign self-employment paperwork")]
        private static float kApplicationTime = 30;

        public int mNumBonusResumeJobs = kNumBonusResumeJobs;

        public int mFullTimeInterviewHour = kFullTimeInterviewHour;

        public int mPartTimeInterviewHour = kPartTimeInterviewHour;

        public int mMaxInterviewPostpones = kMaxInterviewPostpones;

        public bool mEnableGetJobInRabbitHole = kEnableGetJobInRabbitHole;

        public bool mEnableJoinProfessionInRabbitHoleOrLot = kEnableJoinProfessionInRabbitHoleOrLot;

        public bool mInstantGratification = kInstantGratification;

        public bool mHoloComputerInstantGratification = kHoloComputerInstantGratification;

        public bool mHoloPhoneInstantGratification = kHoloPhoneInstantGratification;

        public bool mNewspaperSelfEmployed = kNewspaperSelfEmployed;

        public float mBaseFullTimeJobChance = kBaseFullTimeJobChance;

        public float mBasePartTimeJobChance = kBasePartTimeJobChance;

        public float mPostponeInterviewChanceChange = kPostponeInterviewChanceChange;

        public float mReadyForInterviewChanceChange = kReadyForInterviewChanceChange;

        public float mPositiveTraitInterviewChanceChange = kPositiveTraitInterviewChanceChange;

        public float mNegativeTraitInterviewChanceChange = kNegativeTraitInterviewChanceChange;

        public float mRequiredSkillInterviewChanceChange = kRequiredSkillInterviewChanceChange;

        public float mPromotionChance = kPromotionChance;

        public float mInterviewTime = kInterviewTime;

        public float mApplicationTime = kApplicationTime;

        public Dictionary<string, InterviewSettings> mInterviewSettings = new Dictionary<string, InterviewSettings>();

        public Dictionary<string, CareerAvailabilitySettings> mCareerAvailabilitySettings = new Dictionary<string, CareerAvailabilitySettings>();

        public Dictionary<string, bool> mSelfEmployedAvailabilitySettings = new Dictionary<string, bool>();

        public PersistedSettings()
        {
        }

        public string Export()
        {
            string text = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<JobOverhaulSettings>";
            foreach (var field in typeof(PersistedSettings).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (field.GetValue(this) is Dictionary<string, InterviewSettings> dict)
                {
                    text += "\n  <mInterviewSettings>";
                    foreach (KeyValuePair<string, InterviewSettings> pair in dict)
                    {
                        InterviewSettings settings = pair.Value;
                        List<string> posTraits = new List<string>();
                        List<string> negTraits = new List<string>();
                        List<string> skills = new List<string>();
                        foreach (TraitNames trait in settings.PositiveTraits)
                        {
                            posTraits.Add(trait.ToString());
                        }
                        foreach (TraitNames trait in settings.NegativeTraits)
                        {
                            negTraits.Add(trait.ToString());
                        }
                        foreach (SkillNames skill in settings.RequiredSkills)
                        {
                            skills.Add(skill.ToString());
                        }
                        text += "\n    <" + pair.Key + ">";
                        text += "\n      <mRequiresInterview value=\"" + settings.RequiresInterview.ToString() + "\"/>";
                        text += "\n      <mPositiveTraits value=\"" + string.Join(",", posTraits.ToArray()) + "\"/>";
                        text += "\n      <mNegativeTraits value=\"" + string.Join(",", negTraits.ToArray()) + "\"/>";
                        text += "\n      <mRequiredSkills value=\"" + string.Join(",", skills.ToArray()) + "\"/>";
                        text += "\n    </" + pair.Key + ">";
                    }
                    text += "\n  </mInterviewSettings>";
                }
                else if (field.GetValue(this) is Dictionary<string, CareerAvailabilitySettings> dict2)
                {
                    text += "\n  <mCareerAvailabilitySettings>";
                    foreach (KeyValuePair<string, CareerAvailabilitySettings> pair in dict2)
                    {
                        CareerAvailabilitySettings settings = pair.Value;
                        List<string> list = new List<string>();
                        foreach (AcademicDegreeNames degree in settings.RequiredDegrees)
                        {
                            list.Add(degree.ToString());
                        }
                        text += "\n    <m" + pair.Key + ">";
                        text += "\n      <mIsAvailable value=\"" + settings.IsAvailable.ToString() + "\"/>";
                        text += "\n      <mRequiredDegrees value=\"" + string.Join(",", list.ToArray()) + "\"/>";
                        text += "\n    </m" + pair.Key + ">"; 
                    }
                    text += "\n  </mCareerAvailabilitySettings>";
                }
                else if (field.GetValue(this) is Dictionary<string, bool> dict3)
                {
                    text += "\n  <mSelfEmployedAvailabilitySettings>";
                    foreach (KeyValuePair<string, bool> pair in dict3)
                    {
                        text += "\n    <m" + pair.Key + " value=\"" + pair.Value.ToString() + "\"/>";
                    }
                    text += "\n  </mSelfEmployedAvailabilitySettings>";
                }
                else
                {
                    text += "\n  <" + field.Name + " value=\"" + field.GetValue(this).ToString() + "\"/>";
                }
            }
            return text + "\n</JobOverhaulSettings>";
        }
    }
}

namespace Gamefreak130.JobOverhaulSpace.Buffs
{
    internal class BuffReadyForInterview : Buff
    {
        private const ulong kReadyForInterviewGuid = 0xCA57D12A3647413D;

        public static ulong StaticGuid
        {
            get
            {
                return 0xCA57D12A3647413D;
            }
        }

        public BuffReadyForInterview(BuffData data) : base(data)
        {
        }
    }

    internal class BuffGotTheJob : Buff
    {
        private const ulong kGotTheJobGuid = 0x1770B99317A5D98A;

        public static ulong StaticGuid
        {
            get
            {
                return 0x1770B99317A5D98A;
            }
        }

        public BuffGotTheJob(BuffData data) : base(data)
        {
        }
    }

    internal class BuffBadInterview : Buff
    {
        private const ulong kGotTheJobGuid = 0x552A7AD84AF2FA7E;

        public static ulong StaticGuid
        {
            get
            {
                return 0x552A7AD84AF2FA7E;
            }
        }

        public BuffBadInterview(BuffData data) : base(data)
        {
        }
    }
}

namespace Gamefreak130.JobOverhaulSpace.Helpers
{
    [Persistable]
    public class OccupationEntryTuple
    {
        private Occupation mOccupationEntry;

        private readonly CareerLocation mCareerLocation;

        public Occupation OccupationEntry
        {
            get
            {
                return mOccupationEntry;
            }
            set
            {
                mOccupationEntry = value;
            }
        }

        public CareerLocation CareerLocation
        {
            get
            {
                return mCareerLocation;
            }
        }

        public OccupationEntryTuple()
        {
        }

        public OccupationEntryTuple(Occupation entry, CareerLocation careerLocation)
        {
            mOccupationEntry = entry;
            mCareerLocation = careerLocation;
        }
    }

    [Persistable]
    public class SavedTravelOccupation
    {
        private readonly OccupationNames mGuid;

        private readonly RabbitHole mRabbitHole;

        private readonly int mLevel;

        private readonly int mOverMaxLevel;

        private readonly string mBranch;

        private readonly float mXp;

        private readonly int mMoneyEarned;

        private readonly float mExtraPay;

        private readonly Anniversary mAnniversary;

        private readonly ulong mBossId;

        public OccupationNames Guid
        {
            get
            {
                return mGuid;
            }
        }

        public RabbitHole RabbitHole
        {
            get
            {
                return mRabbitHole;
            }
        }

        public int Level
        {
            get
            {
                return mLevel;
            }
        }

        public string Branch
        {
            get
            {
                return mBranch;
            }
        }

        public int OverMaxLevel
        {
            get
            {
                return mOverMaxLevel;
            }
        }

        public float Xp
        {
            get
            {
                return mXp;
            }
        }

        public float ExtraPay
        {
            get
            {
                return mExtraPay;
            }
        }

        public int MoneyEarned
        {
            get
            {
                return mMoneyEarned;
            }
        }

        public Anniversary Anniversary
        {
            get
            {
                return mAnniversary;
            }
        }

        public ulong BossId
        {
            get
            {
                return mBossId;
            }
        }

        public SavedTravelOccupation()
        {
        }

        public SavedTravelOccupation(Occupation occupation)
        {
            mGuid = occupation.Guid;
            mRabbitHole = occupation.OfficeLocation as RabbitHole;
            mLevel = occupation.Level;
            mBranch = occupation.CurLevelBranchName;
            if (occupation is XpBasedCareer xpBasedCareer)
            {
                mMoneyEarned = xpBasedCareer.TotalCareerMoneyEarned();
                mOverMaxLevel = xpBasedCareer.OvermaxLevel;
                mXp = xpBasedCareer.mXp;
            }
            else
            {
                Career career = occupation as Career;
                mBossId = career.Boss.SimDescriptionId;
                mAnniversary = career.WorkAnniversary;
                mExtraPay = career.mPayPerHourExtra;
                mXp = career.Performance;
            }
        }
    }

    internal static class Listeners
    {
        internal static void OnMapTagsUpdated(object sender, EventArgs e)
        {
            if (MapTagManager.ActiveMapTagManager != null && Sim.ActiveActor.OccupationAsActiveCareer is ActiveCareer career && career.Jobs != null)
            {
                foreach (Job job in career.Jobs)
                {
                    if (job.MapTagEnabled && job.RabbitHole != null)
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
                if (@object is Computer || @object is Phone || @object is Newspaper)
                {
                    Common.Methods.AddInteraction(@object, ChangeSettings.Singleton);
                }
                if (@object is Computer computer)
                {
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
                    Common.Methods.AddInteraction(school, CollegeOfBusiness.AttendResumeWritingAndInterviewTechniquesClass.Singleton);
                }
                if (@object is CityHall cityHall)
                {
                    Common.Methods.AddInteraction(cityHall, JoinActiveCareerDaycare.Singleton);
                }
                if (@object is LifeguardChair chair)
                {
                    chair.RemoveInteractionByType(LifeguardChair.JoinLifeGuardCareer.Singleton);
                }
            }
        }

        internal static ListenerAction OnObjectChanged(Event e)
        {
            if (e.TargetObject is Computer || e.TargetObject is Phone || e.TargetObject is Newspaper)
            {
                Common.Methods.AddInteraction((GameObject)e.TargetObject, ChangeSettings.Singleton);
            }
            if (e.TargetObject is Computer computer)
            {
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
                Common.Methods.AddInteraction(school, CollegeOfBusiness.AttendResumeWritingAndInterviewTechniquesClass.Singleton);
            }
            if (e.TargetObject is CityHall cityHall)
            {
                Common.Methods.AddInteraction(cityHall, JoinActiveCareerDaycare.Singleton);
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
                            if (current.InteractionDefinition is DoInterview.Definition definition && definition.mData.ActorId == data.ActorId)
                            {
                                iop = current;
                                break;
                            }
                        }
                        if (iop != null)
                        {
                            data.RabbitHole.RemoveInteraction(iop);
                        }
                        PhoneCell phone = targetSim.Inventory.Find<PhoneCell>();
                        if (phone != null)
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

        internal static ListenerAction OnSimDied(Event e)
        {
            if (e.Actor != null)
            {
                foreach (InterviewData data in InterviewList)
                {
                    if (data.ActorId == e.Actor.SimDescription.SimDescriptionId)
                    {
                        InteractionObjectPair iop = null;
                        foreach (InteractionObjectPair current in data.RabbitHole.Interactions)
                        {
                            if (current.InteractionDefinition is DoInterview.Definition definition && definition.mData.ActorId == data.ActorId)
                            {
                                iop = current;
                                break;
                            }
                        }
                        if (iop != null)
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
            if (e.Actor != null)
            {
                foreach (Sims3.Gameplay.InventoryStack stack in e.Actor.Inventory.InventoryItems.Values)
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
            }
            return ListenerAction.Keep;
        }

        internal static ListenerAction SaveOccupationForTravel(Event e)
        {
            if (e.Actor != null && e.Actor.CareerManager != null)
            {
                ulong id = e.Actor.SimDescription.SimDescriptionId;
                SavedTravelOccupation occupation = null;
                if (e.Actor.CareerManager.Occupation != null)
                {
                    occupation = new SavedTravelOccupation(e.Actor.CareerManager.Occupation);
                }
                if (SavedOccupationsForTravel.ContainsKey(id))
                {
                    SavedOccupationsForTravel[id] = occupation;
                }
                else
                {
                    SavedOccupationsForTravel.Add(id, occupation);
                }
            }
            return ListenerAction.Keep;
        }

        internal static ListenerAction OnTaskCompleted(Event e)
        {
            if (e is OccupationTaskEvent occupationTaskEvent && occupationTaskEvent.Actor is Sim sim && sim.Occupation != null && sim.Occupation.CurrentJob != null && occupationTaskEvent.TaskId == TaskId.PickUpFood)
            {
                if (occupationTaskEvent.Undone)
                {
                    sim.Occupation.CurrentJob.OnTaskUncomplete(occupationTaskEvent.TaskId, occupationTaskEvent.TargetObject as GameObject);
                }
                else
                {
                    sim.Occupation.CurrentJob.OnTaskComplete(occupationTaskEvent.TaskId, occupationTaskEvent.TargetObject as GameObject);
                }
            }
            return ListenerAction.Keep;
        }

        internal static ListenerAction OnPerformanceChange(Event e)
        {
            if (e.Actor is Sim sim && sim.IsSelectable && sim.OccupationAsCareer is Career career && !(career is School) && career.mPerformance > 99f)
            {
                career.mPerformance = 99f;
                sim.CareerManager.UpdatePerformanceUI(sim.Occupation);
            }
            return ListenerAction.Keep;
        }

        internal static ListenerAction OnFinishedWork(Event e)
        {
            AlarmManager.Global.AddAlarm(1f, TimeUnit.Seconds, 
                delegate() 
                {
                    if (e.Actor is Sim sim && sim.IsSelectable && sim.OccupationAsCareer is Career career && !(career is School) && career.Performance >= 99f && RandomUtil.RandomChance(Settings.mPromotionChance)
                        && TwoButtonDialog.Show(LocalizeString(sim.IsFemale, "PromotionOfferDialog", new object[] { sim }), LocalizationHelper.Yes, LocalizationHelper.No))
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
        public static void AddPhoneInteractions(PhoneHome phoneHome, InterviewData data)
        {
            bool hasPostpone = false;
            bool hasCancel = false;
            foreach (InteractionObjectPair iop in phoneHome.Interactions)
            {
                if (iop.InteractionDefinition is PostponeInterview.Definition definition && definition.mData == data)
                {
                    hasPostpone = true;
                }
                if (iop.InteractionDefinition is CancelInterview.Definition definition2 && definition2.mData == data)
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
                if (current.InteractionDefinition is PostponeInterview.Definition definition && definition.mData.ActorId == id)
                {
                    postponeIop = current;
                }
                if (current.InteractionDefinition is CancelInterview.Definition definition2 && definition2.mData.ActorId == id)
                {
                    cancelIop = current;
                }
            }
            if (postponeIop != null)
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
            if (cancelIop != null)
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
            if (restrictToEarnedDegrees && degreeManager != null && degreeManager.HasCompletedAnyDegree())
            {
                hasDegree = true;
            }
            else if (restrictToEarnedDegrees)
            {
                return false;
            }
            foreach (Occupation current in CareerManager.OccupationList)
            {
                string name = current is Career ? (current as Career).SharedData.Name.Substring(34) : current.Guid.ToString();
                if (Settings.mCareerAvailabilitySettings.TryGetValue(name, out PersistedSettings.CareerAvailabilitySettings settings) && settings.IsAvailable)
                {
                    if (current is Career career && !(career is School) && career.Locations.Count > 0 && career.CareerAgeTest(actor.SimDescription))
                    {
                        return true;
                    }
                    if (current is ActiveCareer activeCareer && !activeCareer.IsAcademicCareer && !actor.SimDescription.TeenOrBelow && ActiveCareer.GetActiveCareerStaticData(current.Guid).CanJoinCareerFromComputerOrNewspaper && IsActiveCareerAvailable(activeCareer))
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
            if (actor.SkillManager.GetElement(SkillNames.SocialNetworking) is SocialNetworkingSkill socialNetworkingSkill && JobOverhaul.Settings.mNumBonusResumeJobs > 0)
            {
                flag = socialNetworkingSkill.HasEnoughFollowersForOccupationBonus();
            }
            AcademicDegreeManager degreeManager = actor.CareerManager.DegreeManager;
            if (isResume && degreeManager == null && !flag)
            {
                return new List<OccupationEntryTuple>();
            }
            List<OccupationEntryTuple> list = new List<OccupationEntryTuple>();
            List<OccupationEntryTuple> list2 = new List<OccupationEntryTuple>();
            GreyedOutTooltipCallback tooltipCallback = null;
            if (flag)
            {
                Occupation dreamsAndPromisesOccupation = Career.GetDreamsAndPromisesJobWithLocation(actor);
                if (dreamsAndPromisesOccupation is Career career && career.Locations.Count > 0 && career.CanAcceptCareer(actor.ObjectId, ref tooltipCallback) && career.CareerAgeTest(actor.SimDescription))
                {
                    list.Add(new OccupationEntryTuple(career, RandomUtil.GetRandomObjectFromList(career.Locations)));
                }
            }
            foreach (RabbitHole rabbitHole in GetObjects<RabbitHole>())
            {
                foreach (CareerLocation location in rabbitHole.CareerLocations.Values)
                {
                    if (location.Career is Career career && !(career is School))
                    {
                        OccupationEntryTuple tuple = new OccupationEntryTuple(career, location);
                        if (isResume && degreeManager != null && degreeManager.HasCompletedDegreeForOccupation(career.Guid) && !list.Contains(tuple) && career.CanAcceptCareer(actor.ObjectId, ref tooltipCallback) && career.CareerAgeTest(actor.SimDescription))
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
                    OccupationEntryTuple tuple = new OccupationEntryTuple(current, null);
                    if (isResume && degreeManager != null && degreeManager.HasCompletedDegreeForOccupation(current.Guid) && !actor.SimDescription.TeenOrBelow && current.CanAcceptCareer(actor.ObjectId, ref tooltipCallback))
                    {
                        list.Add(tuple);
                        continue;
                    }
                    list2.Add(tuple);
                }
            }
            Random randomizer = new Random(randomSeed);
            List<OccupationEntryTuple> list3 = new List<OccupationEntryTuple>(numbJobOpps);
            int num = 0;
            int randNum = list.Count != 0 ? RandomUtil.GetInt(1, list.Count, randomizer) : 0;
            while ((num < numbJobOpps || flag) && list.Count != 0 && num != randNum)
            {
                OccupationEntryTuple rand = RandomUtil.GetRandomObjectFromList(list, randomizer);
                if (!(rand.OccupationEntry is Occupation occupation))
                {
                    continue;
                }
                string name = occupation is Career ? (occupation as Career).SharedData.Name.Substring(34) : occupation.Guid.ToString();
                if (Settings.mCareerAvailabilitySettings.TryGetValue(name, out PersistedSettings.CareerAvailabilitySettings settings) && settings.IsAvailable)
                {
                    list3.Add(rand);
                    num++;
                }
                if (num >= numbJobOpps + Settings.mNumBonusResumeJobs)
                {
                    flag = false;
                }
                list.Remove(rand);
            }
            while ((num < numbJobOpps || flag) && list2.Count != 0)
            {
                OccupationEntryTuple rand = RandomUtil.GetRandomObjectFromList(list2, randomizer);
                if (!(rand.OccupationEntry is Occupation occupation))
                {
                    continue;
                }
                string name = occupation is Career ? (occupation as Career).SharedData.Name.Substring(34) : occupation.Guid.ToString();
                if (Settings.mCareerAvailabilitySettings.TryGetValue(name, out PersistedSettings.CareerAvailabilitySettings settings) && settings.IsAvailable)
                {
                    list3.Add(rand);
                    num++;
                }
                if (num >= numbJobOpps + Settings.mNumBonusResumeJobs)
                {
                    flag = false;
                }
                list2.Remove(rand);
            }
            return list3;
        }

        public static void OfferJob(Sim sim, OccupationEntryTuple occupation)
        {
            List<OccupationEntryTuple> list = new List<OccupationEntryTuple>(1)
            {
                occupation
            };
            UI.CareerSelectionModelEx.Singleton.ShowCareerSelection(sim, sim.ObjectId, list);
        }

        public static bool IsActiveCareerAvailable(ActiveCareer career)
        {
            return career is Lifeguard && IsPoolLifeguardModInstalled
                ? LotManager.GetCommercialLots(CommercialLotSubType.kBeach) != null || LotManager.GetCommercialLots(CommercialLotSubType.kPool) != null
                : career.IsActiveCareerAvailable();
        }

        internal static void ParseXml(XmlNode startElement)
        {
            for (XmlNode node = startElement; node != null; node = node.NextSibling)
            {
                if (node.Attributes["value"] != null)
                {
                    SetImplicitValue(node.Name, node.Attributes["value"].Value);
                }
                else
                {
                    if (node.Name == "mInterviewSettings")
                    {
                        Dictionary<string, PersistedSettings.InterviewSettings> dict = new Dictionary<string, PersistedSettings.InterviewSettings>();
                        for (XmlNode node2 = node.FirstChild; node2 != null; node2 = node2.NextSibling)
                        {
                            bool.TryParse(node2.ChildNodes.Item(0).Attributes["value"].Value, out bool val);
                            List<string> posTraitList = new List<string>(node2.ChildNodes.Item(1).Attributes["value"].Value.Split(new char[] { ',' }));
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
                                            if (str == id.ToString())
                                            {
                                                posTraits.Add((TraitNames)id);
                                            }
                                        }
                                    }
                                }
                            }
                            List<string> negTraitList = new List<string>(node2.ChildNodes.Item(2).Attributes["value"].Value.Split(new char[] { ',' }));
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
                                            if (str == id.ToString())
                                            {
                                                negTraits.Add((TraitNames)id);
                                            }
                                        }
                                    }
                                }
                            }
                            List<string> skillList = new List<string>(node2.ChildNodes.Item(3).Attributes["value"].Value.Split(new char[] { ',' }));
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
                                            if (str == id.ToString())
                                            {
                                                skills.Add((SkillNames)id);
                                            }
                                        }
                                    }
                                }
                            }
                            dict.Add(node2.Name, new PersistedSettings.InterviewSettings(val, posTraits, negTraits, skills));
                        }
                        foreach (string key in dict.Keys)
                        {
                            if (Settings.mInterviewSettings.ContainsKey(key))
                            {
                                Settings.mInterviewSettings[key] = dict[key];
                            }
                        }
                    }
                    else if (node.Name == "mCareerAvailabilitySettings")
                    {
                        Dictionary<string, PersistedSettings.CareerAvailabilitySettings> dict = new Dictionary<string, PersistedSettings.CareerAvailabilitySettings>();
                        for (XmlNode node2 = node.FirstChild; node2 != null; node2 = node2.NextSibling)
                        {
                            bool.TryParse(node2.ChildNodes.Item(0).Attributes["value"].Value, out bool val);
                            List<string> degreeList = new List<string>(node2.ChildNodes.Item(1).Attributes["value"].Value.Split(new char[] { ',' }));
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
                                            if (str == id.ToString())
                                            {
                                                degrees.Add((AcademicDegreeNames)id);
                                            }
                                        }
                                    }
                                }
                            }
                            dict.Add(node2.Name.Substring(1), new PersistedSettings.CareerAvailabilitySettings(val, false, degrees));
                        }
                        foreach (string key in dict.Keys)
                        {
                            if (Settings.mCareerAvailabilitySettings.ContainsKey(key))
                            {
                                Settings.mCareerAvailabilitySettings[key].IsAvailable = dict[key].IsAvailable;
                                Settings.mCareerAvailabilitySettings[key].RequiredDegrees = dict[key].RequiredDegrees;
                            }
                        }
                    }
                    else
                    {
                        Dictionary<string, bool> dict = new Dictionary<string, bool>();
                        for (XmlNode node2 = node.FirstChild; node2 != null; node2 = node2.NextSibling)
                        {
                            bool.TryParse(node2.Attributes["value"].Value, out bool val);
                            dict.Add(node2.Name.Substring(1), val);
                        }
                        foreach (string key in dict.Keys)
                        {
                            if (Settings.mSelfEmployedAvailabilitySettings.ContainsKey(key))
                            {
                                Settings.mSelfEmployedAvailabilitySettings[key] = dict[key];
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
            return SimClockUtils.LocalizeString(SimClockUtils.DaysOfWeek[num % 7], new object[0]);
        }

        internal static string LocalizeString(string name, object[] parameters)
        {
            return Localization.LocalizeString("Gamefreak130/LocalizedMod/JobOverhaul:" + name, parameters);
        }

        internal static string LocalizeString(bool isFemale, string name, object[] parameters)
        {
            return Localization.LocalizeString(isFemale, "Gamefreak130/LocalizedMod/JobOverhaul:" + name, parameters);
        }
    }
}

namespace Gamefreak130.JobOverhaulSpace.Helpers.Situations
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
            mAlarmHandle = sim.AddAlarmRepeating(3f, TimeUnit.Minutes, new AlarmTimerCallback(Update), 3f, TimeUnit.Minutes, "Daycare Child Monitor - " + sim.Name, AlarmType.AlwaysPersisted);
            SetupStartingConditions(sim);
            mMetricRecord = new MetricRecord(sim, GetMotiveDataDictionary());
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

            public string LocalizeString(string name, params object[] parameters)
            {
                return Localization.LocalizeString(Parent.GetLocalizeKey() + "/RouteEveryoneToLot:" + name, parameters);
            }

            public string LocalizeString(bool isFemale, string name, params object[] parameters)
            {
                return Localization.LocalizeString(isFemale, Parent.GetLocalizeKey() + "/RouteEveryoneToLot:" + name, parameters);
            }

            public RouteEveryoneToLot()
            {
            }

            public RouteEveryoneToLot(VaccinationSessionSituationEx parent) : base(parent)
            {
            }

            public override void Init(VaccinationSessionSituationEx parent)
            {
                if (Lot == null)
                {
                    Exit();
                }
                parent.VaccinationSessionBroadcaster = new ReactionBroadcaster(parent.Lot, parent.AskForVaccinationBroadcasterParams, new ReactionBroadcaster.BroadcastCallback(OnEnterBroadcaster));
                parent.BringRandomSimsToSession(parent.NumSimsToInitiallyBring);
                if (parent.Vaccinator.LotCurrent != parent.Lot)
                {
                    parent.Vaccinator.InteractionQueue.CancelNthInteraction(1);
                    InteractionInstance interactionInstance = ForceSituationSpecificInteraction(parent.Lot, parent.Vaccinator, VisitCommunityLot.Singleton, null, null, new Callback(OnVaccinatorRouteFail), new InteractionPriority(InteractionPriorityLevel.UserDirected));
                    if (interactionInstance is ITakeSimToWorkLocation takeSimToWorkLocation)
                    {
                        takeSimToWorkLocation.SetTakingSimToWork();
                    }
                }
                string message = LocalizeString("Tns1GetToLocation", new object[]
                {
                    parent.Vaccinator,
                    parent.Lot.Name
                });
                parent.Vaccinator.ShowTNSIfSelectable(message, NotificationStyle.kGameMessagePositive, ObjectGuid.InvalidObjectGuid, parent.Vaccinator.ObjectId);
            }

            public bool ValidTest(Sim sim)
            {
                return sim.IsNPC && !sim.SimDescription.IsGhost && !sim.SimDescription.IsMummy && !sim.SimDescription.IsFrankenstein && !sim.SimDescription.IsEP11Bot && sim != Parent.Vaccinator && !Parent.LivingWithVaccinator(sim) && !sim.IsPerformingAService && !sim.SimDescription.ChildOrBelow && !Parent.IsInIgnoreList(sim) && !Sims3.Gameplay.Objects.Vehicles.CarNpcManager.Singleton.NpcDriversManager.IsNpcDriver(sim) && !Parent.IsAskingAlready(sim);
            }

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
                    string message = LocalizeString(Parent.Vaccinator.IsFemale, "Tns2VaccinationSessionStarting", new object[]
                    {
                        Parent.Vaccinator,
                        Parent.Lot.Name,
                        Parent.VaccinationSessionMaxDurationInHours
                    });
                    Parent.Vaccinator.ShowTNSIfSelectable(message, NotificationStyle.kGameMessagePositive, ObjectGuid.InvalidObjectGuid, Parent.Vaccinator.ObjectId);
                    string name = string.Format("Vaccination Session 1 hour remaining - Vaccinator: {0}", Parent.Vaccinator);
                    Parent.VaccinationSessionTimer = Parent.Vaccinator.AddAlarm(Parent.VaccinationSessionMaxDurationInHours - 1, TimeUnit.Hours, new AlarmTimerCallback(Parent.OnVaccinationSessionOneHourRemaining), name, AlarmType.DeleteOnReset);
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
            mLeaveConversationListener = EventTracker.AddListener(EventTypeId.kLeftConversation, new ProcessEventDelegate(OnConversationLeft), Vaccinator);
            mLot = lot;
            mLotId = (lot != null) ? lot.LotId : 0uL;
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

        new public static VaccinationSessionSituation Create(Sim vaccinator, Lot lot)
        {
            return new VaccinationSessionSituationEx(vaccinator, lot);
        }

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

        new public bool BringRandomSimTest(Sim sim, Lot lot)
        {
            return sim.IsNPC && !sim.SimDescription.IsGhost && !sim.SimDescription.IsMummy && !sim.SimDescription.IsFrankenstein && !sim.SimDescription.IsEP11Bot && sim.LotCurrent != lot && !LivingWithVaccinator(sim) && (CameraController.IsMapViewModeEnabled() || !World.IsObjectOnScreen(sim.ObjectId)) && !IsInSeekersList(sim) && !IsInInterruptedList(sim) && !IsInIgnoreList(sim) && !sim.IsAtWork;
        }

        new public bool BringRandomSickSimTest(Sim sim, Lot lot)
        {
            return sim.BuffManager.HasElement(BuffNames.Germy) && BringRandomSimTest(sim, lot);
        }

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
                num2 = Lot.RouteRandomNPCSimsHere(num, new Lot.RouteRandomNPCSimsToLotTest(BringRandomSickSimTest), new Callback(OnVaccinateeRouteFail));
            }
            if (num2 != num)
            {
                num2 = Lot.RouteRandomNPCSimsHere(num, new Lot.RouteRandomNPCSimsToLotTest(BringRandomSimTest), new Callback(OnVaccinateeRouteFail));
            }
        }

        public static void BeforeVaccinate(Sim actor, Sim target, string interaction, ActiveTopic topic, InteractionInstance i)
        {
            if (GetVaccinationSessionSituation(actor) is VaccinationSessionSituationEx vaccinationSessionSituation)
            {
                vaccinationSessionSituation.NumVaccinations++;
                vaccinationSessionSituation.AddToIgnoreList(target);
                vaccinationSessionSituation.BringRandomSimsToSession(1);
                HealthManager healthManager = target.SimDescription.HealthManager;
                if (healthManager != null)
                {
                    healthManager.Vaccinate();
                }
            }
        }
    }

    public class FreeClinicSessionSituationEx : VaccinationSessionSituationEx
    {
        public class AskForDiagnosisEx : Sim.AskForDiagnosis
        {
            new public class Definition : InteractionDefinition<Sim, Sim, AskForDiagnosisEx>
            {
                public override bool Test(Sim actor, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    return GetFreeClinicSessionSituation(target) != null;
                }
            }

            new public static InteractionDefinition Singleton = new Definition();
        }

        public new const string sLocalizationKey = "Gameplay/Situations/FreeClinicSessionSituation";

        public override int[] SuccessRatingThreshold
        {
            get
            {
                return FreeClinicSessionSituation.kFreeClinicSuccessRatingThreshold;
            }
        }

        public override int NumSimsToInitiallyBring
        {
            get
            {
                return FreeClinicSessionSituation.kFreeClinicNumSimsToInitiallyBring;
            }
        }

        public override int MaxSimsInSessionAtAnyOneTime
        {
            get
            {
                return FreeClinicSessionSituation.kFreeClinicMaxSimsInSessionAtAnyOneTime;
            }
        }

        public override float MaxDistanceForAutonomousSign
        {
            get
            {
                return FreeClinicSessionSituation.kFreeClinicMaxDistanceForAutonomousSign;
            }
        }

        public override int VaccinationSessionMaxDurationInHours
        {
            get
            {
                return FreeClinicSessionSituation.kFreeClinicSessionMaxDurationInHours;
            }
        }

        public override float[] PerformanceChanges
        {
            get
            {
                return FreeClinicSessionSituation.kFreeClinicPerformanceChanges;
            }
        }

        public override float HandledSimBonus
        {
            get
            {
                return FreeClinicSessionSituation.kFreeClinicHandledSimBonus;
            }
        }

        public override float IgnoredSimPenalty
        {
            get
            {
                return FreeClinicSessionSituation.kFreeClinicIgnoredSimPenalty;
            }
        }

        public override TaskId Task
        {
            get
            {
                return TaskId.FreeHealthClinic;
            }
        }

        public override string LocalizeString(string name, params object[] parameters)
        {
            return Localization.LocalizeString("Gameplay/Situations/FreeClinicSessionSituation:" + name, parameters);
        }

        public override string LocalizeString(bool isFemale, string name, params object[] parameters)
        {
            return Localization.LocalizeString(isFemale, "Gameplay/Situations/FreeClinicSessionSituation:" + name, parameters);
        }

        public override string GetLocalizeKey()
        {
            return "Gameplay/Situations/FreeClinicSessionSituation";
        }

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
            SituationSocial.Definition i = new SituationSocial.Definition("Diagnose", new string[0], null, false);
            AddSituationSpecificInteraction(sim, Vaccinator, i, null, null, null, "Sims3.Gameplay.Autonomy.Diagnose+Definition");
        }

        public override void PushAskInteraction(Sim s)
        {
            Sim.AskForDiagnosis askForDiagnosis = RetrieveAskForVaccinationState(s, out Sim.AskForVaccinationState askState, out float timeRemainingInSpecifiedAskStateOverride)
                ? PushAskForDiagnosis(s, Vaccinator, new InteractionPriority(InteractionPriorityLevel.NonCriticalNPCBehavior, 1f), askState, timeRemainingInSpecifiedAskStateOverride)
                : PushAskForDiagnosis(s, Vaccinator, new InteractionPriority(InteractionPriorityLevel.NonCriticalNPCBehavior, 1f), Sim.AskForVaccinationState.WaitingForVaccinatorToArrive);
            askForDiagnosis.VaccinationSessionSituation = this;
            AddVaccinationSeeker(s);
        }

        public override bool IsAskingAlready(Sim sim)
        {
            foreach (InteractionInstance current in sim.InteractionQueue.InteractionList)
            {
                if (current is Sim.AskForDiagnosis)
                {
                    bool result = true;
                    return result;
                }
                if (current is SocialInteractionB socialInteractionB)
                {
                    if (socialInteractionB.InteractionDefinition is SocialInteractionB.Definition definition && definition.Key == "Diagnose")
                    {
                        bool result = true;
                        return result;
                    }
                }
            }
            return false;
        }

        public override void SendEventSuccessfullyHeld()
        {
            EventTracker.SendEvent(EventTypeId.kHeldSuccessfulFreeClinic, Vaccinator);
        }

        public FreeClinicSessionSituationEx()
        {
        }

        public FreeClinicSessionSituationEx(Sim diagnoser, Lot lot) : base(diagnoser, lot)
        {
        }

        new public static FreeClinicSessionSituationEx Create(Sim diagnoser, Lot lot)
        {
            return new FreeClinicSessionSituationEx(diagnoser, lot);
        }

        public static AskForDiagnosisEx PushAskForDiagnosis(Sim sim, Sim diagnoser, InteractionPriority priority, Sim.AskForVaccinationState askState)
        {
            AskForDiagnosisEx askForDiagnosis = PushAskForDiagnosis(sim, diagnoser, priority);
            if (askForDiagnosis != null)
            {
                askForDiagnosis.SetAskState(askState);
            }
            return askForDiagnosis;
        }

        public static AskForDiagnosisEx PushAskForDiagnosis(Sim sim, Sim diagnoser, InteractionPriority priority, Sim.AskForVaccinationState askState, float timeRemainingInSpecifiedAskStateOverride)
        {
            AskForDiagnosisEx askForDiagnosis = PushAskForDiagnosis(sim, diagnoser, priority);
            if (askForDiagnosis != null)
            {
                askForDiagnosis.SetAskState(askState, timeRemainingInSpecifiedAskStateOverride);
            }
            return askForDiagnosis;
        }

        public static AskForDiagnosisEx PushAskForDiagnosis(Sim sim, Sim diagnoser, InteractionPriority priority)
        {
            InteractionDefinition singleton = AskForDiagnosisEx.Singleton;
            AskForDiagnosisEx askForDiagnosis = singleton.CreateInstance(diagnoser, sim, priority, true, true) as AskForDiagnosisEx;
            sim.InteractionQueue.Add(askForDiagnosis);
            return askForDiagnosis;
        }

        public static bool TestDiagnose(Sim actor, Sim target, ActiveTopic topic, bool isAutonomous, ref GreyedOutTooltipCallback callback)
        {
            FreeClinicSessionSituationEx freeClinicSessionSituation = GetFreeClinicSessionSituation(actor);
            return freeClinicSessionSituation != null && !freeClinicSessionSituation.IsInIgnoreList(target) && (freeClinicSessionSituation.IsInSeekersList(target) || freeClinicSessionSituation.IsInInterruptedList(target)) && (!isAutonomous || actor.GetDistanceToObject(target) <= AutographSessionSituation.MaxDistanceForAutonomousSign);
        }

        public static void BeforeDiagnose(Sim actor, Sim target, string interaction, ActiveTopic topic, InteractionInstance i)
        {
            if (GetFreeClinicSessionSituation(actor) is FreeClinicSessionSituationEx freeClinicSessionSituation)
            {
                freeClinicSessionSituation.NumVaccinations++;
                freeClinicSessionSituation.AddToIgnoreList(target);
                freeClinicSessionSituation.BringRandomSimsToSession(1);
                HealthManager healthManager = target.SimDescription.HealthManager;
                if (healthManager != null)
                {
                    healthManager.Vaccinate();
                }
            }
        }
    }
}

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
                    mCareerEntries.Add(tuple.OccupationEntry as Occupation);
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
                    mCareerEntries.Add(tuple.OccupationEntry as Occupation);
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
                                if (Settings.mInterviewSettings.TryGetValue(name, out PersistedSettings.InterviewSettings interviewSettings) && interviewSettings.RequiresInterview)
                                {
                                    InterviewData data = new InterviewData(mCurrentCareerLocation, sim);
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
                            mSim.ShowTNSIfSelectable(Localization.LocalizeString("Gameplay/Objects/RabbitHoles/CityHall:RegisterAsSelfEmployedGotoTNS", new object[0]), NotificationStyle.kGameMessagePositive, ObjectGuid.InvalidObjectGuid, ObjectGuid.InvalidObjectGuid);
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
                mCurrentCareerLocation = mAvailableCareersLocationsEx[index].CareerLocation as CareerLocation;
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

        private CareerSelectionModelEx mCareerSelectionModel;

        private IOccupationEntry mSelectedCareer;

        private List<IOccupationEntry> mCareerEntries;

        private bool mbResult;

        private bool mIsFemale;

        private Color kDayTextNotWorkingColor = new Color(2155905152u);

        private Color kDayTextWorkingColor = new Color(4278198336u);

        private bool mWasMapview;

        private static string kLayoutName = "CareerSelectDialog";

        private static int kExportID = 1;

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

        private bool OnPreSaveGame()
        {
            return false;
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
                text.Caption = LocalizeString("CareerOfferHeader", new object[0]);
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
                        text.Caption = Responder.Instance.LocalizationModel.LocalizeString("UI/Caption/CareerSelection:OpenHours", new object[0]);
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
                    text.Caption = (mCareerSelectionModel.mAvailableCareersLocationsEx[mCurrentIndex].CareerLocation as CareerLocation).Owner.GetLocalizedName();
                    text.AutoSize(true);
                    text = mModalDialogWindow.GetChildByID(107605768u, true) as Text;
                    text.Caption = (entry as Career).IsPartTime ? LocalizeString("PartTime", new object[0]) : LocalizeString("FullTime", new object[0]);
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
                button.Visible = entry.IsActive && entry.ActiveCareerLotID == 0uL ? false : true;
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
                    button.TooltipText = greyedOutTooltipCallback != null ? greyedOutTooltipCallback() : LocalizeString("NotCorrectAgeForOccupation", new object[0]);
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
                    button.TooltipText = LocalizeString(sim.IsFemale, "NotCorrectAgeForOccupation", new object[0]);
                    return;
                }
                if (entry is ActiveCareer activeCareer && sim.SimDescription.TeenOrBelow)
                {
                    button.TooltipText = LocalizeString(sim.IsFemale, "TooYoungForProfession", new object[0]);
                    return;
                }
                if (GameUtils.IsInstalled(ProductVersion.EP9) && Settings.mCareerAvailabilitySettings.TryGetValue(name, out PersistedSettings.CareerAvailabilitySettings settings) && settings.RequiredDegrees.Count > 0)
                {
                    foreach (AcademicDegreeNames degree in settings.RequiredDegrees)
                    {
                        if (sim.DegreeManager == null || !sim.DegreeManager.HasCompletedDegree(degree))
                        {
                            button.TooltipText = LocalizeString(sim.IsFemale, "DoesNotHaveRequiredDegrees", new object[0]);
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

        private void OnSelectionChangedEx(List<RowInfo> selectedRows)
        {
            Audio.StartSound("ui_tertiary_button");
        }
    }
}

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
                return LocalizeString("ChangeSettingsName", new object[0]);
            }

            public override bool Test(Sim actor, GameObject target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                mIsPhone = target is PhoneCell;
                return (target is Computer || target is PhoneHome || (target is PhoneCell && (target as PhoneCell).IsUsableBy(actor)) || (target is Newspaper && (target as Newspaper).IsReadable)) && actor.SimDescription.TeenOrAbove && !GameUtils.IsOnVacation() && !GameUtils.IsUniversityWorld();
            }

            public override string[] GetPath(bool isFemale)
            {
                return mIsPhone
                    ? (new string[]
                    {
                        Phone.LocalizeString("JobsAndOffers", new object[0]) + Localization.Ellipsis
                    })
                    : (new string[]
                    {
                        Computer.LocalizeString("JobsAndProfessions", new object[0])
                    });
            }
        }

        public static readonly InteractionDefinition Singleton = new Definition();

        public override bool RunFromInventory()
        {
            return Run();
        }

        public override bool Run()
        {
            UI.MenuContainer container = new UI.MenuContainer(LocalizeString("MenuTitle", new object[0]), LocalizeString("Settings", new object[0]));
            container.AddMenuObject(new UI.GenericActionObject(LocalizeString("NumBonusResumeJobsMenuName", new object[0]), delegate () { return Settings.mNumBonusResumeJobs.ToString(); }, delegate () { return GameUtils.IsInstalled(ProductVersion.EP9); },
                delegate ()
                {
                    string str = StringInputDialog.Show(LocalizeString("NumBonusResumeJobsMenuName", new object[0]), LocalizeString("NumBonusResumeJobsPrompt", new object[0]), Settings.mNumBonusResumeJobs.ToString(), true);
                    if (int.TryParse(str, out int num))
                    {
                        Settings.mNumBonusResumeJobs = num;
                    }
                }));
            container.AddMenuObject(new UI.GenericActionObject(LocalizeString("FullTimeInterviewHourMenuName", new object[0]), delegate () { return Settings.mFullTimeInterviewHour.ToString(); }, null,
                delegate ()
                {
                    string str = StringInputDialog.Show(LocalizeString("FullTimeInterviewHourMenuName", new object[0]), LocalizeString("FullTimeInterviewHourPrompt", new object[0]), Settings.mFullTimeInterviewHour.ToString(), true);
                    if (int.TryParse(str, out int num))
                    {
                        Settings.mFullTimeInterviewHour = Math.Min(num, 23);
                    }
                }));
            container.AddMenuObject(new UI.GenericActionObject(LocalizeString("PartTimeInterviewHourMenuName", new object[0]), delegate () { return Settings.mPartTimeInterviewHour.ToString(); }, null,
                delegate ()
                {
                    string str = StringInputDialog.Show(LocalizeString("PartTimeInterviewHourMenuName", new object[0]), LocalizeString("PartTimeInterviewHourPrompt", new object[0]), Settings.mPartTimeInterviewHour.ToString(), true);
                    if (int.TryParse(str, out int num))
                    {
                        Settings.mPartTimeInterviewHour = Math.Min(num, 23);
                    }
                }));
            container.AddMenuObject(new UI.GenericActionObject(LocalizeString("MaxInterviewPostponesMenuName", new object[0]), delegate () { return Settings.mMaxInterviewPostpones.ToString(); }, null,
                delegate ()
                {
                    string str = StringInputDialog.Show(LocalizeString("MaxInterviewPostponesMenuName", new object[0]), LocalizeString("MaxInterviewPostponesPrompt", new object[0]), Settings.mMaxInterviewPostpones.ToString(), true);
                    if (int.TryParse(str, out int num))
                    {
                        Settings.mMaxInterviewPostpones = num;
                    }
                }));
            container.AddMenuObject(new UI.GenericActionObject(LocalizeString("ReadyForInterviewChanceChangeMenuName", new object[0]), delegate () { return Settings.mReadyForInterviewChanceChange.ToString(); }, delegate () { return GameUtils.IsInstalled(ProductVersion.EP9); },
                delegate ()
                {
                    string str = StringInputDialog.Show(LocalizeString("ReadyForInterviewChanceChangeMenuName", new object[0]), LocalizeString("ReadyForInterviewChanceChangePrompt", new object[0]), Settings.mReadyForInterviewChanceChange.ToString(), true);
                    if (float.TryParse(str, out float num))
                    {
                        Settings.mReadyForInterviewChanceChange = Math.Min(num, 100);
                    }
                }));
            container.AddMenuObject(new UI.GenericActionObject(LocalizeString("BaseFullTimeJobChanceMenuName", new object[0]), delegate () { return Settings.mBaseFullTimeJobChance.ToString(); }, null,
                delegate () 
                {
                    string str = StringInputDialog.Show(LocalizeString("BaseFullTimeJobChanceMenuName", new object[0]), LocalizeString("BaseFullTimeJobChancePrompt", new object[0]), Settings.mBaseFullTimeJobChance.ToString(), true);
                    if (float.TryParse(str, out float num))
                    {
                        Settings.mBaseFullTimeJobChance = Math.Min(num, 100);
                    }
                }));
            container.AddMenuObject(new UI.GenericActionObject(LocalizeString("BasePartTimeJobChanceMenuName", new object[0]), delegate () { return Settings.mBasePartTimeJobChance.ToString(); }, null,
                delegate () 
                {
                    string str = StringInputDialog.Show(LocalizeString("BasePartTimeJobChanceMenuName", new object[0]), LocalizeString("BasePartTimeJobChancePrompt", new object[0]), Settings.mBasePartTimeJobChance.ToString(), true);
                    if (float.TryParse(str, out float num))
                    {
                        Settings.mBasePartTimeJobChance = Math.Min(num, 100);
                    }
                }));
            container.AddMenuObject(new UI.GenericActionObject(LocalizeString("PostponeInterviewChanceChangeMenuName", new object[0]), delegate () { return Settings.mPostponeInterviewChanceChange.ToString(); }, null,
                delegate () 
                {
                    string str = StringInputDialog.Show(LocalizeString("PostponeInterviewChanceChangeMenuName", new object[0]), LocalizeString("PostponeInterviewChanceChangePrompt", new object[0]), Settings.mPostponeInterviewChanceChange.ToString(), true);
                    if (float.TryParse(str, out float num))
                    {
                        Settings.mPostponeInterviewChanceChange = Math.Min(num, 100);
                    }
                }));
            container.AddMenuObject(new UI.GenericActionObject(LocalizeString("PositiveTraitInterviewChanceChangeMenuName", new object[0]), delegate () { return Settings.mPositiveTraitInterviewChanceChange.ToString(); }, null,
                delegate () 
                {
                    string str = StringInputDialog.Show(LocalizeString("PositiveTraitInterviewChanceChangeMenuName", new object[0]), LocalizeString("PositiveTraitInterviewChanceChangePrompt", new object[0]), Settings.mPositiveTraitInterviewChanceChange.ToString(), true);
                    if (float.TryParse(str, out float num))
                    {
                        Settings.mPositiveTraitInterviewChanceChange = Math.Min(num, 100);
                    }
                }));
            container.AddMenuObject(new UI.GenericActionObject(LocalizeString("NegativeTraitInterviewChanceChangeMenuName", new object[0]), delegate () { return Settings.mNegativeTraitInterviewChanceChange.ToString(); }, null,
                delegate () 
                {
                    string str = StringInputDialog.Show(LocalizeString("NegativeTraitInterviewChanceChangeMenuName", new object[0]), LocalizeString("NegativeTraitInterviewChanceChangePrompt", new object[0]), Settings.mNegativeTraitInterviewChanceChange.ToString(), true);
                    if (float.TryParse(str, out float num))
                    {
                        Settings.mNegativeTraitInterviewChanceChange = Math.Min(num, 100);
                    }
                }));
            container.AddMenuObject(new UI.GenericActionObject(LocalizeString("RequiredSkillInterviewChanceChangeMenuName", new object[0]), delegate () { return Settings.mRequiredSkillInterviewChanceChange.ToString(); }, null,
                delegate () 
                {
                    string str = StringInputDialog.Show(LocalizeString("RequiredSkillInterviewChanceChangeMenuName", new object[0]), LocalizeString("RequiredSkillInterviewChanceChangePrompt", new object[0]), Settings.mRequiredSkillInterviewChanceChange.ToString(), true);
                    if (float.TryParse(str, out float num))
                    {
                        Settings.mRequiredSkillInterviewChanceChange = Math.Min(num, 100);
                    }
                }));
            container.AddMenuObject(new UI.GenericActionObject(LocalizeString("PromotionChanceMenuName", new object[0]), delegate () { return Settings.mPromotionChance.ToString(); }, null,
                delegate ()
                {
                    string str = StringInputDialog.Show(LocalizeString("PromotionChanceMenuName", new object[0]), LocalizeString("PromotionChancePrompt", new object[0]), Settings.mPromotionChance.ToString(), true);
                    if (float.TryParse(str, out float num))
                    {
                        Settings.mPromotionChance = Math.Min(num, 100);
                    }
                }));
            container.AddMenuObject(new UI.GenericActionObject(LocalizeString("InterviewTimeMenuName", new object[0]), delegate () { return Settings.mInterviewTime.ToString(); }, null,
                delegate () 
                {
                    string str = StringInputDialog.Show(LocalizeString("InterviewTimeMenuName", new object[0]), LocalizeString("InterviewTimePrompt", new object[0]), Settings.mInterviewTime.ToString(), true);
                    if (float.TryParse(str, out float num))
                    {
                        Settings.mInterviewTime = num;
                    }
                }));
            container.AddMenuObject(new UI.GenericActionObject(LocalizeString("ApplicationTimeMenuName", new object[0]), delegate () { return Settings.mApplicationTime.ToString(); }, null,
                delegate () 
                {
                    string str = StringInputDialog.Show(LocalizeString("ApplicationTimeMenuName", new object[0]), LocalizeString("ApplicationTimePrompt", new object[0]), Settings.mApplicationTime.ToString(), true);
                    if (float.TryParse(str, out float num))
                    {
                        Settings.mApplicationTime = num;
                    }
                }));
            container.AddMenuObject(new UI.GenericActionObject(LocalizeString("NewspaperSelfEmployedMenuName", new object[0]), delegate () { return Settings.mNewspaperSelfEmployed.ToString(); }, delegate () { return GameUtils.IsInstalled(ProductVersion.EP2) || GameUtils.IsInstalled(ProductVersion.EP3) || GameUtils.IsInstalled(ProductVersion.EP5) || GameUtils.IsInstalled(ProductVersion.EP7) || GameUtils.IsInstalled(ProductVersion.EP10) || GameUtils.IsInstalled(ProductVersion.EP11); }, delegate () { Settings.mNewspaperSelfEmployed = !Settings.mNewspaperSelfEmployed; }));
            container.AddMenuObject(new UI.GenericActionObject(LocalizeString("EnableGetJobInRabbitholeMenuName", new object[0]), delegate () { return Settings.mEnableGetJobInRabbitHole.ToString(); }, null, delegate () { Settings.mEnableGetJobInRabbitHole = !Settings.mEnableGetJobInRabbitHole; }));
            container.AddMenuObject(new UI.GenericActionObject(LocalizeString("EnableJoinProfessionMenuName", new object[0]), delegate () { return Settings.mEnableJoinProfessionInRabbitHoleOrLot.ToString(); }, delegate () { return CareerManager.GetActiveCareers().Count > 0; }, delegate () { Settings.mEnableJoinProfessionInRabbitHoleOrLot = !Settings.mEnableJoinProfessionInRabbitHoleOrLot; }));
            container.AddMenuObject(new UI.GenericActionObject(LocalizeString("InstantGratificationMenuName", new object[0]), delegate () { return Settings.mInstantGratification.ToString(); }, null, delegate () { Settings.mInstantGratification = !Settings.mInstantGratification; }));
            container.AddMenuObject(new UI.GenericActionObject(LocalizeString("HoloComputerInstantGratificationMenuName", new object[0]), delegate () { return Settings.mHoloComputerInstantGratification.ToString(); }, delegate () { return !Settings.mInstantGratification && GameUtils.IsInstalled(ProductVersion.EP11); }, delegate () { Settings.mHoloComputerInstantGratification = !Settings.mHoloComputerInstantGratification; }));
            container.AddMenuObject(new UI.GenericActionObject(LocalizeString("HoloPhoneInstantGratificationMenuName", new object[0]), delegate () { return Settings.mHoloPhoneInstantGratification.ToString(); }, delegate () { return !Settings.mInstantGratification && GameUtils.IsInstalled(ProductVersion.EP11) && GameUtils.IsInstalled(ProductVersion.EP9); }, delegate () { Settings.mHoloPhoneInstantGratification = !Settings.mHoloPhoneInstantGratification; }));
            container.AddMenuObject(new UI.GenericActionObject(LocalizeString("ClearAllInterviewsMenuName", new object[0]), delegate () { return string.Empty; }, null,
                delegate ()
                {
                    if (AcceptCancelDialog.Show(LocalizeString("ClearAllInterviewsPrompt", new object[0])))
                    {
                        for (int i = InterviewList.Count - 1; i >= 0; i--)
                        {
                            InterviewList[i].Dispose(false);
                        }
                        SimpleMessageDialog.Show(LocalizeString("ClearAllInterviewsMenuName", new object[0]), LocalizeString("ClearAllInterviewsComplete", new object[0]));
                    }
                }));
            container.AddMenuObject(new UI.ResetActionObject(LocalizeString("ResetMenuName", new object[0]), delegate () { return string.Empty; }, null,
                delegate ()
                {
                    if (AcceptCancelDialog.Show(LocalizeString("ResetPrompt", new object[0])))
                    {
                        ResetSettings();
                        Show(new Format(LocalizeString("ResetComplete", new object[0]), NotificationStyle.kSystemMessage));
                        return true;
                    }
                    return false;
                }));
            container.AddMenuObject(new UI.GenericActionObject(LocalizeString("ExportMenuName", new object[0]), delegate () { return string.Empty; }, null,
                delegate ()
                {
                    CJACK_01:
                    string name = StringInputDialog.Show(LocalizeString("ExportMenuName", new object[0]), LocalizeString("ExportPrompt", new object[0]), "Settings");
                    if (!string.IsNullOrEmpty(name))
                    {
                        name = "Gamefreak130.JobOverhaul." + name;
                        Sims3.Gameplay.BinModel.Singleton.PopulateExportBin();
                        foreach (IExportBinContents current in Sims3.Gameplay.BinModel.Singleton.ExportBinContents)
                        {
                            Sims3.Gameplay.ExportBinContents contents = current as Sims3.Gameplay.ExportBinContents;
                            if (contents.HouseholdName != null && contents.HouseholdName == name)
                            {
                                if (AcceptCancelDialog.Show(LocalizeString("ExportFileOverwritePrompt", new object[0])))
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
                        SimpleMessageDialog.Show(LocalizeString("ExportMenuName", new object[0]), LocalizeString("ExportComplete", new object[0]));
                    }
                }));
            container.AddMenuObject(new UI.ResetActionObject(LocalizeString("ImportMenuName", new object[0]), delegate () { return string.Empty; }, null,
                delegate ()
                {
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
                        SimpleMessageDialog.Show(LocalizeString("ImportMenuName", new object[0]), LocalizeString("ImportFileNotFound", new object[0]));
                        return false;
                    }
                    List<RowInfo> list2 = ObjectPickerDialog.Show(true, ModalDialog.PauseMode.PauseSimulator, LocalizeString("ImportMenuName", new object[0]), Localization.LocalizeString("Ui/Caption/ObjectPicker:OK", new object[0]), Localization.LocalizeString("Ui/Caption/ObjectPicker:Cancel", new object[0]), list, new List<HeaderInfo>() { new HeaderInfo("Ui/Caption/ObjectPicker:Name", "", 250) }, 1, new Vector2(-1f, -1f), false, null, false, false);
                    if (list2 != null)
                    {
                        XmlDocument xml = new XmlDocument();
                        xml.LoadXml(((IExportBinContents)list2[0].Item).HouseholdBio);
                        ParseXml(xml.DocumentElement.FirstChild);
                        SimpleMessageDialog.Show(LocalizeString("ImportMenuName", new object[0]), LocalizeString("ImportComplete", new object[0]));
                        return true;
                    }
                    return false;
                }));

            UI.MenuContainer container2 = new UI.MenuContainer(LocalizeString("MenuTitle", new object[0]), LocalizeString("CareerAvailabilitySettingsMenuName", new object[0]));
            foreach (string key in Settings.mCareerAvailabilitySettings.Keys)
            {
                PersistedSettings.CareerAvailabilitySettings settings = Settings.mCareerAvailabilitySettings[key];
                string name = settings.IsActive ? XpBasedCareer.LocalizeString(Actor.IsFemale, key, new object[0]) : Localization.LocalizeString(Actor.IsFemale, "Gameplay/Excel/Careers/CareerList:" + key, new object[0]);
                UI.MenuContainer jobContainer = new UI.MenuContainer(LocalizeString("MenuTitle", new object[0]), name);
                jobContainer.AddMenuObject(new UI.GenericActionObject(LocalizeString("IsAvailableMenuName", new object[0]), delegate () { return settings.IsAvailable.ToString(); }, null, delegate () { settings.IsAvailable = !settings.IsAvailable; }));
                jobContainer.AddMenuObject(new UI.GenericActionObject(LocalizeString("RequiredDegreesMenuName", new object[0]), delegate () { return string.Empty; }, delegate () { return GameUtils.IsInstalled(ProductVersion.EP9); },
                    delegate ()
                    {
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
                        List<RowInfo> list3 = Helpers.UI.ObjectPickerDialogEx.Show(true, ModalDialog.PauseMode.PauseSimulator, LocalizeString("RequiredDegreesMenuName", new object[0]), Localization.LocalizeString("Ui/Caption/ObjectPicker:OK", new object[0]), Localization.LocalizeString("Ui/Caption/ObjectPicker:Cancel", new object[0]), list, new List<HeaderInfo>() { new HeaderInfo("Ui/Caption/ObjectPicker:Degree", "", 250) }, list[0].RowInfo.Count, new Vector2(-1f, -1f), false, list2, false, false);
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
                container2.AddMenuObject(new List<HeaderInfo>() { new HeaderInfo("Ui/Caption/ObjectPicker:Name", "", 250) }, new UI.GenerateMenuObject(new List<UI.ColumnDelegateStruct>() { new UI.ColumnDelegateStruct(ColumnType.kText, delegate () { return new TextColumn(name); }) }, jobContainer));
            }
            container.AddMenuObject(new UI.GenerateMenuObject(LocalizeString("CareerAvailabilitySettingsMenuName", new object[0]), container2));

            UI.MenuContainer container3 = new UI.MenuContainer(LocalizeString("MenuTitle", new object[0]), LocalizeString("InterviewSettingsMenuName", new object[0]));
            foreach (string key in Settings.mInterviewSettings.Keys)
            {
                PersistedSettings.InterviewSettings settings = Settings.mInterviewSettings[key];
                string name = Localization.LocalizeString(Actor.IsFemale, "Gameplay/Excel/Careers/CareerList:" + key, new object[0]);
                UI.MenuContainer interviewContainer = new UI.MenuContainer(LocalizeString("MenuTitle", new object[0]), name);
                interviewContainer.AddMenuObject(new UI.GenericActionObject(LocalizeString("RequiresInterviewMenuName", new object[0]), delegate () { return settings.RequiresInterview.ToString(); }, null, delegate () { settings.RequiresInterview = !settings.RequiresInterview; }));
                interviewContainer.AddMenuObject(new UI.GenericActionObject(LocalizeString("RequiredSkillsMenuName", new object[0]), delegate () { return string.Empty; }, null,
                    delegate ()
                    {
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
                        List<RowInfo> skillList = Helpers.UI.ObjectPickerDialogEx.Show(true, ModalDialog.PauseMode.PauseSimulator, LocalizeString("RequiredSkillsMenuName", new object[0]), Localization.LocalizeString("Ui/Caption/ObjectPicker:OK", new object[0]), Localization.LocalizeString("Ui/Caption/ObjectPicker:Cancel", new object[0]), list2, list, list2[0].RowInfo.Count, new Vector2(-1f, -1f), false, list3, false, false);
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
                interviewContainer.AddMenuObject(new UI.GenericActionObject(LocalizeString("PositiveTraitsMenuName", new object[0]), delegate () { return string.Empty; }, null,
                    delegate ()
                    {
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
                        List<RowInfo> traitList = Helpers.UI.ObjectPickerDialogEx.Show(true, ModalDialog.PauseMode.PauseSimulator, LocalizeString("PositiveTraitsMenuName", new object[0]), Localization.LocalizeString("Ui/Caption/ObjectPicker:OK", new object[0]), Localization.LocalizeString("Ui/Caption/ObjectPicker:Cancel", new object[0]), list, new List<HeaderInfo>() { new HeaderInfo("Ui/Caption/ObjectPicker:Name", "", 400) }, list[0].RowInfo.Count, new Vector2(-1f, -1f), false, list2, false, false);
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
                interviewContainer.AddMenuObject(new UI.GenericActionObject(LocalizeString("NegativeTraitsMenuName", new object[0]), delegate () { return string.Empty; }, null,
                    delegate ()
                    {
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
                        List<RowInfo> traitList = Helpers.UI.ObjectPickerDialogEx.Show(true, ModalDialog.PauseMode.PauseSimulator, LocalizeString("NegativeTraitsMenuName", new object[0]), Localization.LocalizeString("Ui/Caption/ObjectPicker:OK", new object[0]), Localization.LocalizeString("Ui/Caption/ObjectPicker:Cancel", new object[0]), list, new List<HeaderInfo>() { new HeaderInfo("Ui/Caption/ObjectPicker:Name", "", 400) }, list[0].RowInfo.Count, new Vector2(-1f, -1f), false, list2, false, false);
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
                container3.AddMenuObject(new List<HeaderInfo>() { new HeaderInfo("Ui/Caption/ObjectPicker:Name", "", 250) }, new UI.GenerateMenuObject(new List<UI.ColumnDelegateStruct>() { new UI.ColumnDelegateStruct(ColumnType.kText, delegate () { return new TextColumn(name); }) }, interviewContainer));
            }
            container.AddMenuObject(new UI.GenerateMenuObject(LocalizeString("InterviewSettingsMenuName", new object[0]), container3));

            if (Settings.mSelfEmployedAvailabilitySettings.Count > 0)
            {
                UI.MenuContainer container4 = new UI.MenuContainer(LocalizeString("MenuTitle", new object[0]), LocalizeString("SelfEmployedAvailabilitySettingsMenuName", new object[0]));
                foreach (string key in Settings.mSelfEmployedAvailabilitySettings.Keys)
                {
                    container4.AddMenuObject(new UI.GenericActionObject(XpBasedCareer.LocalizeString(Actor.IsFemale, key, new object[0]), delegate () { return Settings.mSelfEmployedAvailabilitySettings[key].ToString(); }, null, delegate () { Settings.mSelfEmployedAvailabilitySettings[key] = !Settings.mSelfEmployedAvailabilitySettings[key]; }));
                }
                container.AddMenuObject(new UI.GenerateMenuObject(LocalizeString("SelfEmployedAvailabilitySettingsMenuName", new object[0]), container4));
            }
            UI.MenuController.ShowMenu(container);
            return true;
        }
    }

    public class DeliverNewspaperEx : Sims3.Gameplay.Services.DeliverNewspaper
    {
        [DoesntRequireTuning]
        new public class Definition : InteractionDefinition<Sim, Lot, DeliverNewspaperEx>
        {
            public override string GetInteractionName(Sim a, Lot target, InteractionObjectPair interaction)
            {
                return "Deliver Newspaper";
            }

            public override bool Test(Sim a, Lot target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                return true;
            }
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
                string titleText = Localization.LocalizeString("Gameplay/Services/NewspaperDelivery:TooManyPapers", new object[0]);
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

            public Definition(InteractionDefinition intDef)
            {
                IntDef = intDef;
            }

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

            public override string[] GetPath(bool isFemale)
            {
                return IntDef.GetPath(isFemale);
            }

            public override string GetInteractionName(Sim a, Newspaper target, InteractionObjectPair interaction)
            {
                return IntDef.GetInteractionName(a, target, interaction);
            }

            public ResourceKey GetTraitIcon(Sim actor, GameObject target)
            {
                return IntDef is IHasTraitIcon hasTraitIcon ? hasTraitIcon.GetTraitIcon(actor, target) : ResourceKey.kInvalidResourceKey;
            }

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

            public bool AllowPostureIcon(Sim actor, IGameObject target)
            {
                return ShouldStayInPosture(actor, target);
            }
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
            public override bool Test(Sim actor, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                return isAutonomous && !actor.LotCurrent.IsWorldLot && (!SeasonsManager.Enabled || (SeasonsManager.CurrentWeather != Weather.Rain && SeasonsManager.CurrentWeather != Weather.Hail) || SeasonsManager.IsShelteredFromPrecipitation(actor)) && base.Test(actor, target, isAutonomous, ref greyedOutTooltipCallback);
            }
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
            public override string GetInteractionName(Sim actor, CityHall target, InteractionObjectPair iop)
            {
                return LocalizeString("JoinActiveCareerContinuation", new object[0]);
            }

            public override bool Test(Sim actor, CityHall target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                return true;
            }
        }

        public OccupationNames CareerToSet;

        public static InteractionDefinition Singleton = new Definition();

        public override bool InRabbitHole()
        {
            TimedStage timedStage = new TimedStage(GetInteractionName(), Settings.mApplicationTime, false, false, true);
            Stages = new List<Stage>(new Stage[] { timedStage });
            StartStages();
            bool flag = DoLoop(ExitReason.Default);
            if (flag && Actor.IsSelectable && CareerManager.GetStaticOccupation(CareerToSet) is ActiveCareer activeCareer && !Actor.SimDescription.TeenOrBelow && ActiveCareer.CanAddActiveCareer(Actor.SimDescription, CareerToSet) && ActiveCareer.GetActiveCareerStaticData(activeCareer.Guid).CanJoinCareerFromComputerOrNewspaper && IsActiveCareerAvailable(activeCareer))
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
            public override string GetInteractionName(Sim actor, RabbitHole target, InteractionObjectPair iop)
            {
                return LocalizeString("SubmitJobApplication", new object[0]);
            }

            public override bool Test(Sim actor, RabbitHole target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                return true;
            }
        }

        public OccupationNames Occupation;

        public static InteractionDefinition Singleton = new Definition();

        public override bool InRabbitHole()
        {
            TimedStage timedStage = new TimedStage(GetInteractionName(), Settings.mApplicationTime, false, false, true);
            Stages = new List<Stage>(new Stage[] { timedStage });
            StartStages();
            bool flag = DoLoop(ExitReason.Default);
            GreyedOutTooltipCallback tooltipCallback = null;
            if (flag && Actor.IsSelectable && Target.CareerLocations.TryGetValue((ulong)Occupation, out CareerLocation careerLocation) && careerLocation.Career.CanAcceptCareer(Actor.ObjectId, ref tooltipCallback) && careerLocation.Career.CareerAgeTest(Actor.SimDescription))
            {
                string name = careerLocation.Career.SharedData.Name.Substring(34);
                if (Settings.mCareerAvailabilitySettings.TryGetValue(name, out PersistedSettings.CareerAvailabilitySettings availabilitySettings) && availabilitySettings.IsAvailable)
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
                    if (Settings.mInterviewSettings.TryGetValue(name, out PersistedSettings.InterviewSettings interviewSettings) && interviewSettings.RequiresInterview)
                    {
                        InterviewData data = new InterviewData(careerLocation, Actor);
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
                switch (ghost.DeathStyle)
                {
                    case SimDescription.DeathType.Drown:
                        ghost.TraitManager.AddHiddenElement(TraitNames.Hydrophobic);
                        break;
                    case SimDescription.DeathType.Electrocution:
                        ghost.TraitManager.AddHiddenElement(TraitNames.AntiTV);
                        break;
                    case SimDescription.DeathType.Burn:
                        ghost.TraitManager.AddHiddenElement(TraitNames.Pyromaniac);
                        break;
                }
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

                public override bool Test(Sim actor, Terrain target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    return true;
                }
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
                            Actor.ShowTNSIfSelectable(GhostHunter.LocalizeString("GhostsUnhidden", new object[0]), NotificationStyle.kGameMessagePositive, ObjectGuid.InvalidObjectGuid);
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
                            Actor.ShowTNSIfSelectable(GhostHunter.LocalizeString(name, new object[0]), NotificationStyle.kGameMessagePositive, ObjectGuid.InvalidObjectGuid);
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
                public override string GetInteractionName(Sim actor, Lot target, InteractionObjectPair iop)
                {
                    return string.Empty;
                }

                public override bool Test(Sim actor, Lot target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    return actor.OccupationAsActiveCareer is GhostHunter;
                }
            }

            public bool SetupTask(GhostHunter.GhostHunterJob job, TaskId id)
            {
                return id == TaskId.EvictGhost
                    ? CreateAngryGhost(job, Vector3.Invalid, out SimDescription simDescription)
                    : job.SetupTask(id);
            }

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
            public override string GetInteractionName(ref InteractionInstanceParameters parameters)
            {
                return InteriorDesigner.LocalizeString(parameters.Actor, "EvaluateRenovation", new object[0]);
            }

            public override bool Test(Sim a, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                return true;
            }
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
                ownerSim.ShowTNSIfSelectable(TNSNames.WhereIsTheDaycareCaregiverTNS, Actor, null, null, Actor.IsFemale, false, new object[0]);
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
                        ownerSim.ShowTNSAndPlayStingIfSelectable(Daycare.LocalizeDaycareString("TnsMessage_FirstDayProblemChild", parentSimDesc, sim.SimDescription, ownerSim.SimDescription, new object[0]), NotificationStyle.kSystemMessage, parentSimDesc, ownerSim.SimDescription, string.Empty);
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
            public override string GetInteractionName(Sim actor, Sim target, InteractionObjectPair iop)
            {
                return Localization.LocalizeString(actor.IsFemale, "Gameplay/Excel/Socializing/Action:GiveFashionAdvice", new object[0]);
            }

            public override bool Test(Sim a, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                return true;
            }
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

            public Definition(string text, string path) : base(text, new string[]{ path }, null, false)
            {
            }

            public override void AddInteractions(InteractionObjectPair iop, Sim actor, Sim target, List<InteractionObjectPair> results)
            {
                string path = Localization.LocalizeString("Gameplay/Roles/RoleProprietor:JoinCareer", new object[0]) + Localization.Ellipsis;
                results.Add(new InteractionObjectPair(new Definition("Ask To Join Singer Career", path), iop.Target));
                results.Add(new InteractionObjectPair(new Definition("Ask To Join PerformanceArtist Career", path), iop.Target));
                results.Add(new InteractionObjectPair(new Definition("Ask To Join Magician Career", path), iop.Target));
            }

            public override bool Test(Sim a, Sim target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                OccupationNames occupation = OccupationNames.Undefined;
                switch (ActionKey)
                {
                    case "Ask To Join Singer Career":
                        occupation = OccupationNames.SingerCareer;
                        break;
                    case "Ask To Join PerformanceArtist Career":
                        occupation = OccupationNames.PerformanceArtistCareer;
                        break;
                    case "Ask To Join Magician Career":
                        occupation = OccupationNames.MagicianCareer;
                        break;
                }
                if (!Settings.mCareerAvailabilitySettings.TryGetValue(occupation.ToString(), out PersistedSettings.CareerAvailabilitySettings settings) || !settings.IsAvailable)
                {
                    return false;
                }
                if (GameUtils.IsInstalled(ProductVersion.EP9) && settings.RequiredDegrees.Count > 0)
                {
                    foreach (AcademicDegreeNames degree in settings.RequiredDegrees)
                    {
                        if (a.DegreeManager == null || !a.DegreeManager.HasCompletedDegree(degree))
                        {
                            greyedOutTooltipCallback = CreateTooltipCallback(Helpers.Methods.LocalizeString(a.IsFemale, "DoesNotHaveRequiredDegrees", new object[0]));
                            return false;
                        }
                    }
                }
                return Settings.mEnableJoinProfessionInRabbitHoleOrLot && a.Posture.AllowsNormalSocials() && target.Posture.AllowsNormalSocials() && !a.NeedsToBeGreeted(target) && target.SimDescription.AssignedRole is Proprietor && base.Test(a, target, isAutonomous, ref greyedOutTooltipCallback);
            }
        }

        public static void OnRequestFinish(Sim actor, Sim target, string interaction, ActiveTopic topic, InteractionInstance i)
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
            {
                Settings.mCareerAvailabilitySettings.TryGetValue(OccupationNames.Stylist.ToString(), out PersistedSettings.CareerAvailabilitySettings settings);
                if (!settings.IsAvailable)
                {
                    return false;
                }
                if (GameUtils.IsInstalled(ProductVersion.EP9) && settings.RequiredDegrees.Count > 0)
                {
                    foreach (AcademicDegreeNames degree in settings.RequiredDegrees)
                    {
                        if (a.DegreeManager == null || !a.DegreeManager.HasCompletedDegree(degree))
                        {
                            greyedOutTooltipCallback = CreateTooltipCallback(Helpers.Methods.LocalizeString(a.IsFemale, "DoesNotHaveRequiredDegrees", new object[0]));
                            return false;
                        }
                    }
                }
                return Settings.mEnableJoinProfessionInRabbitHoleOrLot && a.Posture.AllowsNormalSocials() && target.Posture.AllowsNormalSocials() && target.SimDescription.HasActiveRole && target.SimDescription.AssignedRole is Styling.StylistRole && base.Test(a, target, isAutonomous, ref greyedOutTooltipCallback);
            }
        }

        public static void OnRequestFinish(Sim actor, Sim target, string interaction, ActiveTopic topic, InteractionInstance i)
        {
            Occupation occupation = CareerManager.GetStaticOccupation(OccupationNames.Stylist);
            OfferJob(actor, new OccupationEntryTuple(occupation, null));
        }
    }

    public class JoinStylistActiveCareerEx : JoinStylistActiveCareer
    {
        new public class Definition : InteractionDefinition<Sim, Lot, JoinStylistActiveCareerEx>
        {
            public override string GetInteractionName(Sim actor, Lot target, InteractionObjectPair iop)
            {
                return Localization.LocalizeString("Gameplay/Excel/Socializing/Action:JoinStylistActiveCareer", new object[0]);
            }

            public override bool Test(Sim a, Lot target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                Settings.mCareerAvailabilitySettings.TryGetValue(OccupationNames.Stylist.ToString(), out PersistedSettings.CareerAvailabilitySettings settings);
                if (!settings.IsAvailable)
                {
                    return false;
                }
                if (GameUtils.IsInstalled(ProductVersion.EP9) && settings.RequiredDegrees.Count > 0)
                {
                    foreach (AcademicDegreeNames degree in settings.RequiredDegrees)
                    {
                        if (a.DegreeManager == null || !a.DegreeManager.HasCompletedDegree(degree))
                        {
                            greyedOutTooltipCallback = CreateTooltipCallback(LocalizeString(a.IsFemale, "DoesNotHaveRequiredDegrees", new object[0]));
                            return false;
                        }
                    }
                }
                return Settings.mEnableJoinProfessionInRabbitHoleOrLot && target.CommercialLotSubType == CommercialLotSubType.kEP2_Salon && !(a.Occupation is Stylist) && Stylist.IsStylistActiveCareerAvailable();
            }
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
            if (!(Settings.mEnableJoinProfessionInRabbitHoleOrLot && Stylist.IsStylistActiveCareerAvailable()))
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
            public override string GetInteractionName(Sim actor, Lot target, InteractionObjectPair iop)
            {
                return Localization.LocalizeString(actor.IsFemale, "Gameplay/Excel/Socializing/Action:JoinFirefighterActiveCareer", new object[0]);
            }

            public override bool Test(Sim a, Lot target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                Settings.mCareerAvailabilitySettings.TryGetValue(OccupationNames.Firefighter.ToString(), out PersistedSettings.CareerAvailabilitySettings settings);
                if (!settings.IsAvailable)
                {
                    return false;
                }
                if (GameUtils.IsInstalled(ProductVersion.EP9) && settings.RequiredDegrees.Count > 0)
                {
                    foreach (AcademicDegreeNames degree in settings.RequiredDegrees)
                    {
                        if (a.DegreeManager == null || !a.DegreeManager.HasCompletedDegree(degree))
                        {
                            greyedOutTooltipCallback = CreateTooltipCallback(LocalizeString(a.IsFemale, "DoesNotHaveRequiredDegrees", new object[0]));
                            return false;
                        }
                    }
                }
                return Settings.mEnableJoinProfessionInRabbitHoleOrLot && target.CommercialLotSubType == CommercialLotSubType.kEP2_FireStation && !a.IsActiveFirefighter && ActiveFireFighter.IsFireFighterActiveCareerAvailable();
            }
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
            if (!(Settings.mEnableJoinProfessionInRabbitHoleOrLot && ActiveFireFighter.IsFireFighterActiveCareerAvailable()))
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
            public override bool Test(Sim a, CityHall target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                Settings.mCareerAvailabilitySettings.TryGetValue(OccupationNames.Daycare.ToString(), out PersistedSettings.CareerAvailabilitySettings settings);
                if (!settings.IsAvailable)
                {
                    return false;
                }
                if (GameUtils.IsInstalled(ProductVersion.EP9) && settings.RequiredDegrees.Count > 0)
                {
                    foreach (AcademicDegreeNames degree in settings.RequiredDegrees)
                    {
                        if (a.DegreeManager == null || !a.DegreeManager.HasCompletedDegree(degree))
                        {
                            greyedOutTooltipCallback = CreateTooltipCallback(LocalizeString(a.IsFemale, "DoesNotHaveRequiredDegrees", new object[0]));
                            return false;
                        }
                    }
                }
                return Settings.mEnableJoinProfessionInRabbitHoleOrLot && ActiveCareer.CanAddActiveCareer(a.SimDescription, OccupationNames.Daycare);
            }

            public override string GetInteractionName(Sim actor, CityHall target, InteractionObjectPair iop)
            {
                return LocalizeString("JoinActiveCareerDaycareName", new object[0]);
            }
        }

        public static InteractionDefinition Singleton = new Definition();

        public override bool InRabbitHole()
        {
            TryDisablingCameraFollow(Actor);
            Occupation occupation = CareerManager.GetStaticOccupation(OccupationNames.Daycare);
            OfferJob(Actor, new OccupationEntryTuple(occupation, null));
            return true;
        }

        public override ThumbnailKey GetIconKey()
        {
            return new ThumbnailKey(new ResourceKey(ResourceUtils.HashString64("w_daycare_career_large"), 796721156u, ResourceUtils.ProductVersionToGroupId(ProductVersion.BaseGame)), ThumbnailSize.Medium);
        }
    }

    public class JoinActiveCareerPrivateEyeEx : PoliceStation.JoinActiveCareerPrivateEye
    {
        new public class Definition : InteractionDefinition<Sim, PoliceStation, JoinActiveCareerPrivateEyeEx>
        {
            public override bool Test(Sim a, PoliceStation target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                Settings.mCareerAvailabilitySettings.TryGetValue(OccupationNames.PrivateEye.ToString(), out PersistedSettings.CareerAvailabilitySettings settings);
                if (!settings.IsAvailable)
                {
                    return false;
                }
                if (GameUtils.IsInstalled(ProductVersion.EP9) && settings.RequiredDegrees.Count > 0)
                {
                    foreach (AcademicDegreeNames degree in settings.RequiredDegrees)
                    {
                        if (a.DegreeManager == null || !a.DegreeManager.HasCompletedDegree(degree))
                        {
                            greyedOutTooltipCallback = CreateTooltipCallback(LocalizeString(a.IsFemale, "DoesNotHaveRequiredDegrees", new object[0]));
                            return false;
                        }
                    }
                }
                return Settings.mEnableJoinProfessionInRabbitHoleOrLot && ActiveCareer.CanAddActiveCareer(a.SimDescription, OccupationNames.PrivateEye);
            }
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
            public override bool Test(Sim a, ScienceLab target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                Settings.mCareerAvailabilitySettings.TryGetValue(OccupationNames.GhostHunter.ToString(), out PersistedSettings.CareerAvailabilitySettings settings);
                if (!settings.IsAvailable)
                {
                    return false;
                }
                if (GameUtils.IsInstalled(ProductVersion.EP9) && settings.RequiredDegrees.Count > 0)
                {
                    foreach (AcademicDegreeNames degree in settings.RequiredDegrees)
                    {
                        if (a.DegreeManager == null || !a.DegreeManager.HasCompletedDegree(degree))
                        {
                            greyedOutTooltipCallback = CreateTooltipCallback(LocalizeString(a.IsFemale, "DoesNotHaveRequiredDegrees", new object[0]));
                            return false;
                        }
                    }
                }
                return Settings.mEnableJoinProfessionInRabbitHoleOrLot && ActiveCareer.CanAddActiveCareer(a.SimDescription, OccupationNames.GhostHunter);
            }
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
            public override bool Test(Sim a, CityHall target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                Settings.mCareerAvailabilitySettings.TryGetValue(OccupationNames.InteriorDesigner.ToString(), out PersistedSettings.CareerAvailabilitySettings settings);
                if (!settings.IsAvailable)
                {
                    return false;
                }
                if (GameUtils.IsInstalled(ProductVersion.EP9) && settings.RequiredDegrees.Count > 0)
                {
                    foreach (AcademicDegreeNames degree in settings.RequiredDegrees)
                    {
                        if (a.DegreeManager == null || !a.DegreeManager.HasCompletedDegree(degree))
                        {
                            greyedOutTooltipCallback = CreateTooltipCallback(LocalizeString(a.IsFemale, "DoesNotHaveRequiredDegrees", new object[0]));
                            return false;
                        }
                    }
                }
                return Settings.mEnableJoinProfessionInRabbitHoleOrLot && ActiveCareer.CanAddActiveCareer(a.SimDescription, OccupationNames.InteriorDesigner);
            }
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
            public override bool Test(Sim a, CityHall target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                Settings.mCareerAvailabilitySettings.TryGetValue(OccupationNames.Lifeguard.ToString(), out PersistedSettings.CareerAvailabilitySettings settings);
                if (!settings.IsAvailable)
                {
                    return false;
                }
                if (GameUtils.IsInstalled(ProductVersion.EP9) && settings.RequiredDegrees.Count > 0)
                {
                    foreach (AcademicDegreeNames degree in settings.RequiredDegrees)
                    {
                        if (a.DegreeManager == null || !a.DegreeManager.HasCompletedDegree(degree))
                        {
                            greyedOutTooltipCallback = CreateTooltipCallback(LocalizeString(a.IsFemale, "DoesNotHaveRequiredDegrees", new object[0]));
                            return false;
                        }
                    }
                }
                return Settings.mEnableJoinProfessionInRabbitHoleOrLot && CareerManager.GetStaticOccupation(OccupationNames.Lifeguard) is ActiveCareer activeCareer && IsActiveCareerAvailable(activeCareer) && ActiveCareer.CanAddActiveCareer(a.SimDescription, OccupationNames.Lifeguard);
            }
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
                        results.Add(new InteractionObjectPair(new Definition(LocalizeString(actor.IsFemale, "TransferJob", new object[0]), current), target));
                    }
                }
            }

            public override string GetInteractionName(Sim a, RabbitHole target, InteractionObjectPair interaction)
            {
                return mName;
            }

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
                    Settings.mInterviewSettings.TryGetValue(career.SharedData.Name.Substring(34), out PersistedSettings.InterviewSettings interviewSettings);
                    Settings.mCareerAvailabilitySettings.TryGetValue(career.SharedData.Name.Substring(34), out PersistedSettings.CareerAvailabilitySettings availabilitySettings);
                    if (interviewSettings == null || availabilitySettings == null || !Settings.mEnableGetJobInRabbitHole || !availabilitySettings.IsAvailable || interviewSettings.RequiresInterview || !career.CanAcceptCareer(a.ObjectId, ref greyedOutTooltipCallback2))
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
                                greyedOutTooltipCallback = CreateTooltipCallback(Helpers.Methods.LocalizeString(a.IsFemale, "DoesNotHaveRequiredDegrees", new object[0]));
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
            public override bool Test(Sim a, RabbitHole target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                return a.FamilyFunds >= CollegeOfBusiness.kCostOfResumeInterviewClass && SimClock.IsTimeBetweenTimes(kStartAvailibilityTime, kEndAvailibilityTime);
            }

            public override string GetInteractionName(ref InteractionInstanceParameters parameters)
            {
                return Localization.LocalizeString("Gameplay/Abstracts/ScriptObject/AttendResumeWritingAndInterviewTechniquesClass:InteractionName", new object[]
                {
                    CollegeOfBusiness.kCostOfResumeInterviewClass
                });
            }
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
                Actor.BuffManager.AddElement(0xCA57D12A3647413D, Target is CollegeOfBusiness ? Origin.FromCollegeOfBusinessRabbitHole : Origin.FromSchool);
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
            public override string[] GetPath(bool isFemale)
            {
                return new string[]
                {
                    Phone.LocalizeString("JobsAndOffers", new object[0]) + Localization.Ellipsis
                };
            }

            public override bool Test(Sim a, Phone target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
            {
                return target.IsUsableBy(a) && a.OccupationAsPerformanceCareer != null && a.OccupationAsPerformanceCareer.GetSteadyGigProprietors().Count != 0 && base.Test(a, target, isAutonomous, ref greyedOutTooltipCallback);
            }

            public override void PopulatePieMenuPicker(ref InteractionInstanceParameters parameters, out List<TabInfo> listObjs, out List<HeaderInfo> headers, out int NumSelectableRows)
            {
                NumSelectableRows = 1;
                Sim sim = parameters.Actor as Sim;
                List<Sim> steadyGigProprietors = sim.OccupationAsPerformanceCareer.GetSteadyGigProprietors();
                PopulateSimPicker(ref parameters, out listObjs, out headers, steadyGigProprietors, false);
            }
        }
    }

    public class Interviews
    {
        [Persistable]
        public class InterviewData
        {
            private readonly ulong mActorId;

            private DateAndTime mInterviewDate;

            private int mTimesPostponed;

            private readonly OccupationNames mCareerName;

            private readonly RabbitHole mRabbitHole;

            private AlarmHandle mRemindAlarm = AlarmHandle.kInvalidHandle;

            private AlarmHandle mTimeoutAlarm = AlarmHandle.kInvalidHandle;

            private EventListener mRabbitHoleDisposedListener;

            public ulong ActorId
            {
                get
                {
                    return mActorId;
                }
            }

            public DateAndTime InterviewDate
            {
                get
                {
                    return mInterviewDate;
                }

                set
                {
                    mInterviewDate = value;
                }
            }

            public int TimesPostponed
            {
                get
                {
                    return mTimesPostponed;
                }

                set
                {
                    mTimesPostponed = value;
                }
            }

            public AlarmHandle RemindAlarm
            {
                get
                {
                    return mRemindAlarm;
                }

                set
                {
                    mRemindAlarm = value;
                }
            }

            public AlarmHandle TimeoutAlarm
            {
                get
                {
                    return mTimeoutAlarm;
                }

                set
                {
                    mTimeoutAlarm = value;
                }
            }

            public EventListener RabbitHoleDisposedListener
            {
                get
                {
                    return mRabbitHoleDisposedListener;
                }

                set
                {
                    mRabbitHoleDisposedListener = value;
                }
            }

            public RabbitHole RabbitHole
            {
                get
                {
                    return mRabbitHole;
                }
            }

            public OccupationNames CareerName
            {
                get
                {
                    return mCareerName;
                }
            }

            public InterviewData()
            {
            }

            public InterviewData(CareerLocation careerLocation, Sim actor)
            {
                mRabbitHole = careerLocation.Owner;
                mActorId = actor.SimDescription.SimDescriptionId;
                mCareerName = careerLocation.Career.Guid;
                mTimesPostponed = 0;
                float interviewDelta = careerLocation.Career.SharedData.Category == Career.CareerCategory.FullTime ? SimClock.HoursUntil(Settings.mFullTimeInterviewHour) + 24 : SimClock.HoursUntil(Settings.mPartTimeInterviewHour) + 24;
                HolidayManager manager = HolidayManager.Instance;
                while (SimClock.Add(SimClock.CurrentTime(), TimeUnit.Hours, interviewDelta).DayOfWeek == DaysOfTheWeek.Saturday || SimClock.Add(SimClock.CurrentTime(), TimeUnit.Hours, interviewDelta).DayOfWeek == DaysOfTheWeek.Sunday || (manager != null && SimClock.IsSameDay(SimClock.Add(SimClock.CurrentTime(), TimeUnit.Hours, interviewDelta), manager.mStartDateTimeOfHoliday)))
                {
                    interviewDelta += 24;
                }
                mInterviewDate = SimClock.Add(SimClock.CurrentTime(), TimeUnit.Hours, interviewDelta);
                string text = LocalizeString(actor.IsFemale, "InterviewOffer", new object[]
                    {
                        actor,
                        careerLocation.Owner.GetLocalizedName(),
                        careerLocation.Career.GetLocalizedCareerName(actor.IsFemale),
                        GetTextDayOfWeek(InterviewDate),
                        SimClockUtils.GetText(Convert.ToInt32(mInterviewDate.Hour), 0)
                    });
                Show(new Format(text, NotificationStyle.kGameMessagePositive));
                mRemindAlarm = AlarmManager.Global.AddAlarm(SimClock.HoursUntil(InterviewDate) - 1, TimeUnit.Hours, new AlarmTimerCallback(OnReminderCallback), "Gamefreak130 wuz here -- Interview Reminder", AlarmType.AlwaysPersisted, actor);
                mTimeoutAlarm = AlarmManager.Global.AddAlarm(SimClock.HoursUntil(InterviewDate) + 1, TimeUnit.Hours, new AlarmTimerCallback(OnInterviewTimeout), "Gamefreak130 wuz here -- Interview Timeout", AlarmType.AlwaysPersisted, actor);
                careerLocation.Owner.AddInteraction(new DoInterview.Definition(this));
                mRabbitHoleDisposedListener = EventTracker.AddListener(EventTypeId.kEventObjectDisposed, new ProcessEventDelegate(OnRabbitHoleDisposed), null, mRabbitHole);
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
                    sim.InteractionQueue.Add(definition.CreateInstance(mRabbitHole, sim, new InteractionPriority(InteractionPriorityLevel.UserDirected), true, true) as DoInterview);
                    sim.ShowTNSIfSelectable(LocalizeString(sim.IsFemale, "InterviewReminder", new object[] 
                        {
                            sim,
                            mRabbitHole.GetLocalizedName()
                        }), NotificationStyle.kGameMessagePositive, ObjectGuid.InvalidObjectGuid, ObjectGuid.InvalidObjectGuid);
                }
            }

            protected internal void OnInterviewTimeout()
            {
                Dispose(true);
            }

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
                    if (mRabbitHole != null)
                    {
                        foreach (InteractionObjectPair current in mRabbitHole.Interactions)
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
                                    CareerManager.GetStaticOccupation(mCareerName).GetLocalizedCareerName(sim.IsFemale)
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
                        mRabbitHole.RemoveInteraction(iop);
                    }
                }
                InterviewList.Remove(this);
                AlarmManager.Global.RemoveAlarm(mRemindAlarm);
                AlarmManager.Global.RemoveAlarm(mTimeoutAlarm);
                EventTracker.RemoveListener(mRabbitHoleDisposedListener);
                mRabbitHoleDisposedListener = null;
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

                public Definition(InterviewData data)
                {
                    mData = data;
                }

                public override string GetInteractionName(Sim actor, RabbitHole target, InteractionObjectPair iop)
                {
                    return LocalizeString("DoInterview", new object[0]);
                }

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
                if (Actor.IsSelectable && CareerManager.GetStaticCareer(mData.CareerName) is Career career && Settings.mInterviewSettings.TryGetValue(career.SharedData.Name.Substring(34), out PersistedSettings.InterviewSettings settings))
                {
                    float chance = career.Category == Career.CareerCategory.FullTime ? Settings.mBaseFullTimeJobChance : Settings.mBasePartTimeJobChance;
                    chance -= Settings.mPostponeInterviewChanceChange * mData.TimesPostponed;
                    foreach (TraitNames trait in settings.PositiveTraits)
                    {
                        if (Actor.TraitManager.HasElement(trait))
                        {
                            chance += Settings.mPositiveTraitInterviewChanceChange;
                        }
                    }
                    foreach (TraitNames trait in settings.NegativeTraits)
                    {
                        if (Actor.TraitManager.HasElement(trait))
                        {
                            chance -= Settings.mNegativeTraitInterviewChanceChange;
                        }
                    }
                    foreach (SkillNames skill in settings.RequiredSkills)
                    {
                        if (Actor.SkillManager.HasElement(skill))
                        {
                            chance += Settings.mRequiredSkillInterviewChanceChange * Actor.SkillManager.GetElement(skill).SkillLevel;
                        }
                    }
                    if (Actor.BuffManager.HasElement(0xCA57D12A3647413D))
                    {
                        chance += Settings.mReadyForInterviewChanceChange;
                    }
                    chance *= MathHelpers.LinearInterpolate(-100f, 100f, 0.65f, 1.15f, Actor.MoodManager.MoodValue);
                    MathUtils.Clamp(chance, 0, 100);
                    mSuccess = RandomUtil.RandomChance(chance);
                    float time = Settings.mInterviewTime / 3;
                    mAlarm1 = AlarmManager.Global.AddAlarm(time, TimeUnit.Minutes, new AlarmTimerCallback(ShowFirstMessage), "Gamefreak130 wuz here -- Interview Message Callback", AlarmType.AlwaysPersisted, Actor);
                    mAlarm2 = AlarmManager.Global.AddAlarm(time * 2, TimeUnit.Minutes, new AlarmTimerCallback(ShowSecondMessage), "Gamefreak130 wuz here -- Interview Message Callback", AlarmType.AlwaysPersisted, Actor);
                    bool flag = DoTimedLoop(Settings.mInterviewTime);
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

            public void ShowFirstMessage()
            {
                Actor.ShowTNSIfSelectable(LocalizeString(Actor.IsFemale, mSuccess ? "FirstInterviewTNSGood" : "FirstInterviewTNSBad", new object[0]), NotificationStyle.kSimTalking);
            }

            public void ShowSecondMessage()
            {
                Actor.ShowTNSIfSelectable(LocalizeString(Actor.IsFemale, mSuccess ? "SecondInterviewTNSGood" : "SecondInterviewTNSBad", new object[0]), NotificationStyle.kSimTalking);
            }

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

                public Definition(InterviewData data)
                {
                    mData = data;
                }

                public override string[] GetPath(bool isFemale)
                {
                    return new string[]
                    {
                        Phone.LocalizeString("JobsAndOffers", new object[0]) + Localization.Ellipsis
                    };
                }

                public override string GetInteractionName(Sim actor, Phone target, InteractionObjectPair iop)
                {
                    return LocalizeString("PostponeInterview", new object[0]);
                }

                public override bool Test(Sim a, Phone target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    if (SimClock.CurrentTime() > SimClock.Subtract(mData.InterviewDate, TimeUnit.Hours, 1))
                    {
                        greyedOutTooltipCallback = CreateTooltipCallback(LocalizeString(a.IsFemale, "PostponePointOfNoReturn", new object[] { a }));
                        return false;
                    }
                    if (mData.TimesPostponed >= Settings.mMaxInterviewPostpones)
                    {
                        greyedOutTooltipCallback = CreateTooltipCallback(LocalizeString(a.IsFemale, "OutOfPostpones", new object[] { a }));
                        return false;
                    }
                    return base.Test(a, target, isAutonomous, ref greyedOutTooltipCallback) && mData.ActorId == a.SimDescription.SimDescriptionId;
                }
            }

            public override ConversationBehavior OnCallConnected()
            {
                return ConversationBehavior.TalkBriefly;
            }

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
                Actor.ShowTNSIfSelectable(LocalizeString(Actor.IsFemale, data.TimesPostponed >= Settings.mMaxInterviewPostpones ? "InterviewPostponedFinal" : "InterviewPostponed", new object[]
                    {
                        Actor,
                        data.RabbitHole.GetLocalizedName(),
                        GetTextDayOfWeek(data.InterviewDate),
                        SimClockUtils.GetText(Convert.ToInt32(data.InterviewDate.Hour), 0)
                    }), data.TimesPostponed >= Settings.mMaxInterviewPostpones ? NotificationStyle.kGameMessageNegative : NotificationStyle.kSystemMessage);
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

                public Definition(InterviewData data)
                {
                    mData = data;
                }

                public override string[] GetPath(bool isFemale)
                {
                    return new string[]
                    {
                        Phone.LocalizeString("JobsAndOffers", new object[0]) + Localization.Ellipsis
                    };
                }

                public override string GetInteractionName(Sim actor, Phone target, InteractionObjectPair iop)
                {
                    return LocalizeString("CancelInterview", new object[0]);
                }

                public override bool Test(Sim a, Phone target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    return base.Test(a, target, isAutonomous, ref greyedOutTooltipCallback) && mData.ActorId == a.SimDescription.SimDescriptionId;
                }
            }

            public override ConversationBehavior OnCallConnected()
            {
                return ConversationBehavior.TalkBriefly;
            }

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
                            greyedOutTooltipCallback = CreateTooltipCallback(LocalizeString(actor.IsFemale, "AlreadyHaveInterview", new object[0]));
                            return false;
                        }
                    }
                    if (actor.SimDescription.IsEP11Bot && actor.TraitManager != null && !actor.TraitManager.HasElement(TraitNames.ProfessionalChip))
                    {
                        greyedOutTooltipCallback = CreateTooltipCallback(Localization.LocalizeString("Ui/Tooltip/HUD/Navigation:RobotCareerDisallowed", new object[0]));
                    }
                    else if (FindJobTest(actor, false))
                    {
                        return true;
                    }
                }
                return false;
            }

            public override string[] GetPath(bool isFemale)
            {
                return new string[]
                {
                    Computer.LocalizeString("JobsAndProfessions", new object[0])
                };
            }

            public override string GetInteractionName(Sim actor, Computer target, InteractionObjectPair iop)
            {
                return LocalizeString("FindJobComputerName", new object[0]);
            }
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
                Show(new Format(LocalizeString("JobsExhausted", new object[0]), NotificationStyle.kGameMessagePositive));
                Actor.AddExitReason(ExitReason.Finished);
                return;
            }
            if (Settings.mInstantGratification || (Settings.mHoloComputerInstantGratification && Target is HoloComputer))
            {
                Helpers.UI.CareerSelectionModelEx.Singleton.ShowCareerSelection(Actor, Target.ObjectId, OccupationEntries, false);
                Actor.AddExitReason(ExitReason.Finished);
                return;
            }
            List<OccupationEntryTuple> list = new List<OccupationEntryTuple>(1);
            OccupationEntryTuple entry = OccupationEntries[0];
            list.Add(entry);
            OccupationEntries.Remove(entry);
            if (Helpers.UI.CareerSelectionModelEx.Singleton.ShowCareerSelection(Actor, Target.ObjectId, list, false))
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
                        greyedOutTooltipCallback = CreateTooltipCallback(Localization.LocalizeString("Ui/Tooltip/HUD/Navigation:RobotCareerDisallowed", new object[0]));
                    }
                    else if (FindJobTest(actor, false))
                    {
                        return true;
                    }
                }
                return false;
            }

            public override string[] GetPath(bool isFemale)
            {
                return new string[]
                {
                    Computer.LocalizeString("JobsAndProfessions", new object[0])
                };
            }

            public override string GetInteractionName(Sim a, Newspaper target, InteractionObjectPair interaction)
            {
                return LocalizeString("FindJobNewspaperName", new object[0]);
            }
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
            return true;
        }

        private void FindJobAlarmCallback()
        {
            if (OccupationEntries.Count == 0)
            {
                Show(new Format(LocalizeString("JobsExhausted", new object[0]), NotificationStyle.kGameMessagePositive));
                Target.StopUsingNewspaper(Actor, mCurrentStateMachine, mFromInventory);
                Actor.AddExitReason(ExitReason.Finished);
                return;
            }
            if (Settings.mInstantGratification)
            {
                if (!Helpers.UI.CareerSelectionModelEx.Singleton.ShowCareerSelection(Actor, Target.ObjectId, OccupationEntries, false))
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
            if (Helpers.UI.CareerSelectionModelEx.Singleton.ShowCareerSelection(Actor, Target.ObjectId, list, false))
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

        public override void PostureTransitionFailed(bool transitionExitResult)
        {
            Target.PutNewspaperAway(Actor, true);
        }
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
                        greyedOutTooltipCallback = CreateTooltipCallback(Localization.LocalizeString("Ui/Tooltip/HUD/Navigation:RobotCareerDisallowed", new object[0]));
                        return false;
                    }
                    if (FindJobTest(actor, true, ref flag))
                    {
                        return true;
                    }
                    if (!flag)
                    {
                        greyedOutTooltipCallback = CreateTooltipCallback(Phone.UploadResume.LocalizeString("NoDegreeOrEnoughFollower", new object[0]));
                    }
                }
                return false;
            }

            public override string[] GetPath(bool isFemale)
            {
                return new string[]
                {
                    Computer.LocalizeString("JobsAndProfessions", new object[0])
                };
            }
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
                Show(new Format(LocalizeString("JobsExhausted", new object[0]), NotificationStyle.kGameMessagePositive));
                Actor.AddExitReason(ExitReason.Finished);
                return;
            }
            if (Settings.mInstantGratification || (Settings.mHoloComputerInstantGratification && Target is HoloComputer))
            {
                Helpers.UI.CareerSelectionModelEx.Singleton.ShowCareerSelection(Actor, Target.ObjectId, OccupationEntries, true);
                Actor.AddExitReason(ExitReason.Finished);
                return;
            }
            List<OccupationEntryTuple> list = new List<OccupationEntryTuple>(1);
            OccupationEntryTuple entry = OccupationEntries[0];
            list.Add(entry);
            OccupationEntries.Remove(entry);
            if (Helpers.UI.CareerSelectionModelEx.Singleton.ShowCareerSelection(Actor, Target.ObjectId, list, false))
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
                        greyedOutTooltipCallback = CreateTooltipCallback(Localization.LocalizeString("Ui/Tooltip/HUD/Navigation:RobotCareerDisallowed", new object[0]));
                        return false;
                    }
                    if (FindJobTest(actor, true, ref flag))
                    {
                        return true;
                    }
                    if (!flag)
                    {
                        greyedOutTooltipCallback = CreateTooltipCallback(Phone.UploadResume.LocalizeString("NoDegreeOrEnoughFollower", new object[0]));
                    }
                }
                return false;
            }

            public override string[] GetPath(bool isFemale)
            {
                return new string[]
                {
                    Phone.LocalizeString("JobsAndOffers", new object[0]) + Localization.Ellipsis
                };
            }
        }

        private AlarmHandle EventAlarm = AlarmHandle.kInvalidHandle;

        private List<OccupationEntryTuple> OccupationEntries = new List<OccupationEntryTuple>();

        public override bool RunFromInventory()
        {
            return Run();
        }

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
                Show(new Format(LocalizeString("JobsExhausted", new object[0]), NotificationStyle.kGameMessagePositive));
                Actor.AddExitReason(ExitReason.Finished);
                return;
            }
            if (Settings.mInstantGratification || (Settings.mHoloPhoneInstantGratification && Target is PhoneFuture))
            {
                Helpers.UI.CareerSelectionModelEx.Singleton.ShowCareerSelection(Actor, Target.ObjectId, OccupationEntries, true);
                Actor.AddExitReason(ExitReason.Finished);
                return;
            }
            List<OccupationEntryTuple> list = new List<OccupationEntryTuple>(1);
            OccupationEntryTuple entry = OccupationEntries[0];
            list.Add(entry);
            OccupationEntries.Remove(entry);
            if (Helpers.UI.CareerSelectionModelEx.Singleton.ShowCareerSelection(Actor, Target.ObjectId, list, false))
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
                        if (Settings.mSelfEmployedAvailabilitySettings.TryGetValue(current.Guid.ToString(), out bool value) && value && Occupation.GetOccupationStaticData(current.Guid) is SkillBasedCareerStaticData staticData)
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

                public override string[] GetPath(bool isFemale)
                {
                    return new string[]
                    {
                        Localization.LocalizeString(isFemale, "Gameplay/Objects/RabbitHoles/CityHall:RegisterAsSelfEmployedPathName", new object[0])
                    };
                }

                public override string GetInteractionName(Sim actor, CityHall target, InteractionObjectPair iop)
                {
                    return Occupation.GetLocalizedCareerName(OccupationEntry.Occupation, actor.SimDescription);
                }

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
                TimedStage timedStage = new TimedStage(GetInteractionName(), Settings.mApplicationTime, false, false, true);
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
                string description = Localization.LocalizeString(Actor.IsFemale, skillBasedCareerStaticData.CareerDescriptionLocalizationKey, new object[0]);
                string text = string.Empty;
                foreach (string current in staticOccupation.ResponsibilitiesLocalizationKeys)
                {
                    text = text + Localization.LocalizeString(Actor.IsFemale, current, new object[0]) + "\n";
                }
                OpportunityDialog.MaptagObjectInfo mapTagObject;
                mapTagObject.mLotId = 0uL;
                mapTagObject.mMapTag = null;
                mapTagObject.mObjectGuid = ObjectGuid.InvalidObjectGuid;
                mapTagObject.mHouseholdLotId = 18446744073709551615uL;
                bool flag2 = OpportunityDialog.Show(ThumbnailKey.kInvalidThumbnailKey, Actor.ObjectId, ObjectGuid.InvalidObjectGuid, Actor.Name, OpportunityDialog.OpportunityType.SkillBasedCareer, localizedCareerName, description, string.Empty, text, mapTagObject, true, OpportunityDialog.DescriptionBackgroundType.StaticBackground, Actor.IsFemale, false);
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
                {
                    return new string[]
                    {
                        Localization.LocalizeString("Gameplay/Objects/Miscellaneous/Newspaper:JobsAndProfessions", new object[0]),
                        Localization.LocalizeString("Gameplay/Objects/Miscellaneous/Newspaper/RegisterAsSelfEmployed:PathName", new object[0])
                    };
                }

                public override string GetInteractionName(Sim a, Newspaper target, InteractionObjectPair interaction)
                {
                    return Occupation.GetLocalizedCareerName(OccupationEntry.Occupation, a.SimDescription);
                }

                public override bool Test(Sim a, Newspaper target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    if (!Settings.mNewspaperSelfEmployed || a.SimDescription.IsEnrolledInBoardingSchool() || !target.IsReadable || a.SimDescription.ChildOrBelow || GetObjects<CityHall>().Length == 0)
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

            public override bool Run()
            {
                return DoFindJob(false);
            }

            public override bool RunFromInventory()
            {
                return DoFindJob(true);
            }

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
                {
                    return new string[]
                    {
                        Phone.LocalizeString(isFemale, "JobsAndOffers", new object[0])  + Localization.Ellipsis,
                        Phone.LocalizeString(isFemale, "RegisterAsSelfEmployedPathName", new object[0])
                    };
                }

                public override string GetInteractionName(Sim actor, Phone target, InteractionObjectPair iop)
                {
                    return Occupation.GetLocalizedCareerName(OccupationEntry.Occupation, actor.SimDescription);
                }

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
                {
                    return new string[]
                    {
                        Computer.LocalizeString(isFemale, "JobsAndProfessions", new object[0]),
                        Computer.LocalizeString(isFemale, "RegisterAsSelfEmployedPathName", new object[0])
                    };
                }

                public override string GetInteractionName(Sim actor, Computer target, InteractionObjectPair iop)
                {
                    return Occupation.GetLocalizedCareerName(OccupationEntry.Occupation, actor.SimDescription);
                }

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