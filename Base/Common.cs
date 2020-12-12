using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using static Sims3.UI.ObjectPicker;

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
                if (interactionTuning is null)
                {
                    interactionTuning = AutonomyTuning.GetTuning(oldType, oldType.FullName, oldTarget);
                    if (interactionTuning is null)
                    {
                        return null;
                    }
                    if (clone)
                    {
                        interactionTuning = CloneTuning(interactionTuning);
                    }
                    AutonomyTuning.AddTuning(newType.FullName, newTarget.FullName, interactionTuning);
                }
                InteractionObjectPair.sTuningCache.Remove(new(newType, newTarget));
            }
            catch (Exception)
            {
            }
            result = interactionTuning;
            return result;
        }

        private static InteractionTuning CloneTuning(InteractionTuning oldTuning) => new()
        {
            mFlags = oldTuning.mFlags,
            ActionTopic = oldTuning.ActionTopic,
            AlwaysChooseBest = oldTuning.AlwaysChooseBest,
            Availability = CloneAvailability(oldTuning.Availability),
            CodeVersion = oldTuning.CodeVersion,
            FullInteractionName = oldTuning.FullInteractionName,
            FullObjectName = oldTuning.FullObjectName,
            mChecks = Helpers.CloneList(oldTuning.mChecks),
            mTradeoff = CloneTradeoff(oldTuning.mTradeoff),
            PosturePreconditions = oldTuning.PosturePreconditions,
            ScoringFunction = oldTuning.ScoringFunction,
            ScoringFunctionOnlyAppliesToSpecificCommodity = oldTuning.ScoringFunctionOnlyAppliesToSpecificCommodity,
            ScoringFunctionString = oldTuning.ScoringFunctionString,
            ShortInteractionName = oldTuning.ShortInteractionName,
            ShortObjectName = oldTuning.ShortObjectName
        };

        private static Tradeoff CloneTradeoff(Tradeoff old) => new()
        {
            mFlags = old.mFlags,
            mInputs = Helpers.CloneList(old.mInputs),
            mName = old.mName,
            mNumParameters = old.mNumParameters,
            mOutputs = Helpers.CloneList(old.mOutputs),
            mVariableRestrictions = old.mVariableRestrictions,
            TimeEstimate = old.TimeEstimate
        };

        private static Availability CloneAvailability(Availability old) => new()
        {
            mFlags = old.mFlags,
            AgeSpeciesAvailabilityFlags = old.AgeSpeciesAvailabilityFlags,
            CareerThresholdType = old.CareerThresholdType,
            CareerThresholdValue = old.CareerThresholdValue,
            ExcludingBuffs = Helpers.CloneList(old.ExcludingBuffs),
            ExcludingTraits = Helpers.CloneList(old.ExcludingTraits),
            MoodThresholdType = old.MoodThresholdType,
            MoodThresholdValue = old.MoodThresholdValue,
            MotiveThresholdType = old.MotiveThresholdType,
            MotiveThresholdValue = old.MotiveThresholdValue,
            RequiredBuffs = Helpers.CloneList(old.RequiredBuffs),
            RequiredTraits = Helpers.CloneList(old.RequiredTraits),
            SkillThresholdType = old.SkillThresholdType,
            SkillThresholdValue = old.SkillThresholdValue,
            WorldRestrictionType = old.WorldRestrictionType,
            OccultRestrictions = old.OccultRestrictions,
            OccultRestrictionType = old.OccultRestrictionType,
            SnowLevelValue = old.SnowLevelValue,
            WorldRestrictionWorldNames = Helpers.CloneList(old.WorldRestrictionWorldNames),
            WorldRestrictionWorldTypes = Helpers.CloneList(old.WorldRestrictionWorldTypes)
        };
    }

    public class BuffBooter
    {
        public string mXmlResource;

        public BuffBooter(string xmlResource) => mXmlResource = xmlResource;

        public void LoadBuffData()
        {
            AddBuffs(null);
            UIManager.NewHotInstallStoreBuffData += AddBuffs;
        }

        public void AddBuffs(ResourceKey[] resourceKeys)
        {
            if (XmlDbData.ReadData(mXmlResource) is XmlDbData xmlDbData)
            {
                BuffManager.ParseBuffData(xmlDbData, true);
            }
        }
    }

    public static class Helpers
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
            if (gameObject.ItemComp?.InteractionsInventory is not null)
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

        public static List<T> CloneList<T>(IEnumerable<T> old) => old is not null ? new(old) : null;

        public static object CoinFlipSelect(object obj1, object obj2) => RandomUtil.CoinFlip() ? obj1 : obj2;
    }

    public static class Reflection
    {
        public static void StaticInvoke(string assemblyQualifiedTypeName, string methodName, object[] args, Type[] argTypes) => StaticInvoke(Type.GetType(assemblyQualifiedTypeName), methodName, args, argTypes);

        public static void StaticInvoke(Type type, string methodName, object[] args, Type[] argTypes)
        {
            if (type is null)
            {
                throw new ArgumentNullException("Null type");
            }
            if (type.GetMethod(methodName, argTypes) is not MethodInfo method)
            {
                throw new ArgumentException("No method found in type with specified name and args");
            }
            method.Invoke(null, args);
        }

        public static T StaticInvoke<T>(string assemblyQualifiedTypeName, string methodName, object[] args, Type[] argTypes) => StaticInvoke<T>(Type.GetType(assemblyQualifiedTypeName), methodName, args, argTypes);

        public static T StaticInvoke<T>(Type type, string methodName, object[] args, Type[] argTypes)
            => type is null
            ? throw new ArgumentNullException("Null type")
            : type.GetMethod(methodName, argTypes) is not MethodInfo method
            ? throw new ArgumentException("No method found in type with specified name and args")
            : (T)method.Invoke(null, args);

        public static void InstanceInvoke(string assemblyQualifiedTypeName, object[] ctorArgs, Type[] ctorArgTypes, string methodName, object[] methodArgs, Type[] methodArgTypes)
            => InstanceInvoke(Type.GetType(assemblyQualifiedTypeName), ctorArgs, ctorArgTypes, methodName, methodArgs, methodArgTypes);

        public static void InstanceInvoke(Type type, object[] ctorArgs, Type[] ctorArgTypes, string methodName, object[] methodArgs, Type[] methodArgTypes)
        {
            if (type is null)
            {
                throw new ArgumentNullException("Null type");
            }
            if (type.GetConstructor(ctorArgTypes) is not ConstructorInfo ctor)
            {
                throw new ArgumentException("No type constructor found with specified args");
            }
            InstanceInvoke(ctor.Invoke(ctorArgs), methodName, methodArgs, methodArgTypes);
        }

        public static void InstanceInvoke(object obj, string methodName, object[] args, Type[] argTypes)
        {
            if (obj is null)
            {
                throw new ArgumentNullException("Instance object is null");
            }
            if (obj.GetType().GetMethod(methodName, argTypes) is not MethodInfo method)
            {
                throw new ArgumentException("No method found in instance with specified name and args");
            }
            method.Invoke(obj, args);
        }

        public static T InstanceInvoke<T>(string assemblyQualifiedTypeName, object[] ctorArgs, Type[] ctorArgTypes, string methodName, object[] methodArgs, Type[] methodArgTypes) 
            => InstanceInvoke<T>(Type.GetType(assemblyQualifiedTypeName), ctorArgs, ctorArgTypes, methodName, methodArgs, methodArgTypes);

        public static T InstanceInvoke<T>(Type type, object[] ctorArgs, Type[] ctorArgTypes, string methodName, object[] methodArgs, Type[] methodArgTypes)
            => type is null
            ? throw new ArgumentNullException("Null type")
            : type.GetConstructor(ctorArgTypes) is not ConstructorInfo ctor
            ? throw new ArgumentException("No type constructor found with specified args")
            : InstanceInvoke<T>(ctor.Invoke(ctorArgs), methodName, methodArgs, methodArgTypes);

        public static T InstanceInvoke<T>(object obj, string methodName, object[] args, Type[] argTypes) 
            => obj is null 
            ? throw new ArgumentNullException("Instance object is null")
            : obj.GetType().GetMethod(methodName, argTypes) is not MethodInfo method
            ? throw new ArgumentException("No method found in instance with specified name and args")
            : (T)method.Invoke(obj, args);
    }

    public delegate T GenericDelegate<T>();

    public interface IPersistedSettings
    {
    }
}

namespace Gamefreak130.Common.UI
{
    public struct PreviousMenuInfo
    {
        public object mMenuID;

        public int mMenuTab;

        public PreviousMenuInfo(object menuID, int tabNumber)
        {
            mMenuID = menuID;
            mMenuTab = tabNumber;
        }
    }

    public struct ColumnDelegateStruct
    {
        public ColumnType mColumnType;

        public GenericDelegate<ColumnInfo> mInfo;

        public ColumnDelegateStruct(ColumnType ColType, GenericDelegate<ColumnInfo> infoDelegate)
        {
            mColumnType = ColType;
            mInfo = infoDelegate;
        }
    }

    public struct RowTextFormat
    {
        public Color mTextColor;

        public bool mBoldTextStyle;

        public string mTooltip;

        public RowTextFormat(Color TextColor, bool BoldText, string TooltipText)
        {
            mTextColor = TextColor;
            mBoldTextStyle = BoldText;
            mTooltip = TooltipText;
        }
    }

    public class MenuContainer
    {
        private List<RowInfo> mRowInformation;

        private readonly string[] mTabImage;

        private readonly string[] mTabText;

        private readonly GenericDelegate<List<RowInfo>> mRowPopulationDelegate;

        private readonly List<RowInfo> mHiddenRows;

        public string MenuDisplayName { get; }

        public int Selectable { get; } = 1;

        public List<HeaderInfo> Headers { get; private set; }

        public List<TabInfo> TabInformation { get; private set; }

        public Action<List<RowInfo>> OnEnd { get; }

        public MenuContainer()
        {
        }

        public MenuContainer(string Title)
        {
            mHiddenRows = new();
            MenuDisplayName = Title;
            mTabImage = new[] { "" };
            mTabText = new[] { "" };
            OnEnd = null;
            Headers = new();
            mRowInformation = new();
            TabInformation = new();
        }

        public MenuContainer(string Title, string Subtitle)
        {
            mHiddenRows = new();
            MenuDisplayName = Title;
            mTabImage = new[] { "" };
            mTabText = new[] { Subtitle };
            OnEnd = null;
            Headers = new();
            mRowInformation = new();
            TabInformation = new();
        }

        public MenuContainer(string Title, string[] TabImage, string[] TabName, int numSelectable, Action<List<RowInfo>> OnEndDelegate)
        {
            mHiddenRows = new();
            MenuDisplayName = Title;
            mTabImage = TabImage;
            mTabText = TabName;
            Selectable = numSelectable;
            OnEnd = OnEndDelegate;
            Headers = new();
            mRowInformation = new();
            TabInformation = new();
        }

        public MenuContainer(string Title, string[] TabImage, string[] TabName, int numSelectable, Action<List<RowInfo>> OnEndDelegate, GenericDelegate<List<RowInfo>> RowPopulationDelegate)
        {
            mHiddenRows = new();
            MenuDisplayName = Title;
            mTabImage = TabImage;
            mTabText = TabName;
            Selectable = numSelectable;
            OnEnd = OnEndDelegate;
            mRowPopulationDelegate = RowPopulationDelegate;
            Headers = new();
            mRowInformation = new();
            TabInformation = new();
            RefreshMenuObjects(0);
            if (mRowInformation.Count > 0)
            {
                for (int i = 0; i < mRowInformation[0].ColumnInfo.Count; i++)
                {
                    Headers.Add(new("Ui/Caption/ObjectPicker:Sim", "", 200));
                }
            }
        }

        public void RefreshMenuObjects(int Tabnumber)
        {
            mRowInformation = mRowPopulationDelegate();
            TabInformation = new()
            {
                new("", mTabText[Tabnumber], mRowInformation)
            };
        }

        public void SetHeaders(List<HeaderInfo> headers) => Headers = headers;

        public void SetHeader(int HeaderNumber, HeaderInfo HeaderInfos) => Headers[HeaderNumber] = HeaderInfos;

        public void ClearMenuObjects() => TabInformation.Clear();

        public void AddMenuObject(MenuObject MenuItem)
        {
            if (TabInformation.Count < 1)
            {
                mRowInformation = new()
                {
                    MenuItem.RowInformation
                };
                TabInformation.Add(new(mTabImage[0], mTabText[0], mRowInformation));
                Headers.Add(new("Ui/Caption/ObjectPicker:Name", "", 300));
                Headers.Add(new("Ui/Caption/ObjectPicker:Value", "", 100));
                return;
            }
            TabInformation[0].RowInfo.Add(MenuItem.RowInformation);
        }

        public void AddMenuObject(List<HeaderInfo> headers, MenuObject MenuItem)
        {

            if (TabInformation.Count < 1)
            {
                mRowInformation = new()
                {
                    MenuItem.RowInformation
                };
                TabInformation.Add(new(mTabImage[0], mTabText[0], mRowInformation));
                Headers = headers;
                return;
            }
            TabInformation[0].RowInfo.Add(MenuItem.RowInformation);
            Headers = headers;
        }

        public void AddMenuObject(List<HeaderInfo> headers, RowInfo item)
        {
            if (TabInformation.Count < 1)
            {
                mRowInformation = new()
                {
                    item
                };
                TabInformation.Add(new(mTabImage[0], mTabText[0], mRowInformation));
                Headers = headers;
                return;
            }
            TabInformation[0].RowInfo.Add(item);
            Headers = headers;
        }

        public void UpdateRows()
        {
            for (int i = mHiddenRows.Count - 1; i >= 0; i--)
            {
                MenuObject item = mHiddenRows[i].Item as MenuObject;
                if (item.Predicate())
                {
                    mHiddenRows.RemoveAt(i);
                    AddMenuObject(item);
                }
            }
            for (int i = TabInformation[0].RowInfo.Count - 1; i >= 0; i--)
            {
                MenuObject item = TabInformation[0].RowInfo[i].Item as MenuObject;
                if (item.Predicate is not null && !item.Predicate())
                {
                    mHiddenRows.Add(TabInformation[0].RowInfo[i]);
                    TabInformation[0].RowInfo.RemoveAt(i);
                }
            }
        }

        public void UpdateItems()
        {
            UpdateRows();
            foreach (TabInfo current in TabInformation)
            {
                foreach (RowInfo current2 in current.RowInfo)
                {
                    (current2.Item as MenuObject)?.UpdateMenuObject();
                }
            }
        }
    }

    public class MenuController : ModalDialog
    {
        private enum ControlIds : uint
        {
            ItemTable = 99576784u,
            OkayButton,
            CancelButton,
            TitleText,
            TableBackground,
            TableBezel
        }

        private const int kWinExportID = 1;

        private Vector2 mTableOffset;

        private ObjectPicker mTable;

        private readonly Button mOkayButton;

        private readonly Button mCloseButton;

        private readonly TabContainer mTabsContainer;

        public bool Okay { get; private set; }

        public List<RowInfo> Result { get; private set; }

        public Action<List<RowInfo>> EndDelegates { get; private set; }

        public void ShowModal()
        {
            mModalDialogWindow.Moveable = true;
            StartModal();
        }

        public void Stop() => StopModal();

        public MenuController(string title, string buttonTrue, string buttonFalse, List<TabInfo> listObjs, List<HeaderInfo> headers, int numSelectableRows, bool showHeadersAndToggle, Action<List<RowInfo>> EndResultDelegate) 
            : this(true, PauseMode.PauseSimulator, title, buttonTrue, buttonFalse, listObjs, headers, numSelectableRows, showHeadersAndToggle)
            => EndDelegates = EndResultDelegate;

        public MenuController(bool _, PauseMode __, string title, string buttonTrue, string buttonFalse, List<TabInfo> listObjs, List<HeaderInfo> headers, int numSelectableRows, bool showHeadersAndToggle) : base("UiObjectPicker", kWinExportID, true, PauseMode.PauseSimulator, null)
        {
            if (mModalDialogWindow is not null)
            {
                Text text = mModalDialogWindow.GetChildByID(99576787u, false) as Text;
                text.Caption = title;
                mTable = mModalDialogWindow.GetChildByID(99576784u, false) as ObjectPicker;
                mTable.SelectionChanged += OnRowClicked;
                mTabsContainer = mTable.mTabs;
                mTable.mTable.mPopulationCompletedCallback += ResizeWindow;
                mOkayButton = mModalDialogWindow.GetChildByID(99576785u, false) as Button;
                mOkayButton.TooltipText = buttonTrue;
                mOkayButton.Enabled = true;
                mOkayButton.Click += OnOkayButtonClick;
                OkayID = mOkayButton.ID;
                SelectedID = mOkayButton.ID;
                mCloseButton = mModalDialogWindow.GetChildByID(99576786u, false) as Button;
                mCloseButton.TooltipText = buttonFalse;
                mCloseButton.Click += OnCloseButtonClick;
                CancelID = mCloseButton.ID;
                mTableOffset = mModalDialogWindow.Area.BottomRight - mModalDialogWindow.Area.TopLeft - (mTable.Area.BottomRight - mTable.Area.TopLeft);
                mTable.ShowHeaders = showHeadersAndToggle;
                mTable.ViewTypeToggle = false;
                mTable.ShowToggle = false;
                mTable.Populate(listObjs, headers, numSelectableRows);
                ResizeWindow();
            }
        }

        public void PopulateMenu(List<TabInfo> tabinfo, List<HeaderInfo> headers, int numSelectableRows) => mTable.Populate(tabinfo, headers, numSelectableRows);

        public override void Dispose() => Dispose(true);

        public void AddRow(int Tabnumber, RowInfo rinfo)
        {
            mTable.mItems[Tabnumber].RowInfo.Clear();
            mTable.mItems[Tabnumber].RowInfo.Add(rinfo);
            Repopulate();
        }

        /*public void UpdateItems()
        {
            if (mTable is null)
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
            mTable.mTable.ScrollRowToTop(15);//TODO Fix
        }*/

        public void SetTableColor(Color color) => mModalDialogWindow.GetChildByID(99576789u, false).ShadeColor = color;

        public void SetTitleText(string Text) => (mModalDialogWindow.GetChildByID(99576787u, false) as Text).Caption = Text;

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
            if (TextStyleBold)
            {
                (mModalDialogWindow.GetChildByID(99576787u, false) as Text).TextStyle = 2u;
            }
        }

        public void SetTitleTextColor(Color TextColor) => (mModalDialogWindow.GetChildByID(99576787u, false) as Text).TextColor = TextColor;

        public void SetTitleTextStyle(uint TextStyle) => (mModalDialogWindow.GetChildByID(99576787u, false) as Text).TextStyle = TextStyle;

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
            mModalDialogWindow.Area = new(mModalDialogWindow.Area.TopLeft, mModalDialogWindow.Area.TopLeft + mTable.TableArea.BottomRight + mTableOffset);
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
                EndDelegates?.Invoke(mTable.Selected);
                Result = mTable.Selected;
                Okay = true;
            }
            else
            {
                Result = null;
                Okay = false;
            }
            mTable.Populate(null, null, 0);
            EndDelegates = null;
            return true;
        }

        public static bool ShowMenu(MenuContainer container)
        {
            while (true)
            {
                container.UpdateItems();
                MenuController controller = Show(container);
                if (controller.Okay)
                {
                    if (controller.Result is not null)
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
                container.UpdateItems();
                MenuController controller = Show(container, tab);
                if (controller.Okay)
                {
                    if (controller.Result is not null)
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
            MenuController menuController = new(container.MenuDisplayName, Localization.LocalizeString("Ui/Caption/Global:Ok"), Localization.LocalizeString("Ui/Caption/Global:Cancel"), container.TabInformation, container.Headers, container.Selectable, true, container.OnEnd);
            menuController.SetTitleTextStyle(2u);
            menuController.ShowModal();
            return menuController;
        }

        private static MenuController Show(MenuContainer container, int tab)
        {
            Sims3.Gameplay.Gameflow.SetGameSpeed(Gameflow.GameSpeed.Pause, Sims3.Gameplay.Gameflow.SetGameSpeedContext.GameStates);
            MenuController menuController = new(container.MenuDisplayName, Localization.LocalizeString("Ui/Caption/Global:Ok"), Localization.LocalizeString("Ui/Caption/Global:Cancel"), container.TabInformation, container.Headers, container.Selectable, true, container.OnEnd);
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
        private List<ColumnInfo> mColumnInfoList;

        protected List<ColumnDelegateStruct> mColumnActions;

        private RowTextFormat mTextFormat;

        public GenericDelegate<bool> Predicate { get; protected set; }

        public RowInfo RowInformation { get; private set; }

        public MenuObject()
        {
            mColumnInfoList = new();
            mColumnActions = new();
        }

        public void Fillin() => RowInformation = new(this, mColumnInfoList);

        public void Fillin(Color TextColor)
        {
            mTextFormat.mTextColor = TextColor;
            Fillin();
        }

        public void Fillin(Color TextColor, bool BoldTextStyle)
        {
            mTextFormat.mTextColor = TextColor;
            mTextFormat.mBoldTextStyle = BoldTextStyle;
            Fillin();
        }

        public void Fillin(bool BoldTextStyle)
        {
            mTextFormat.mBoldTextStyle = BoldTextStyle;
            Fillin();
        }

        public void Fillin(string TooltipText)
        {
            mTextFormat.mTooltip = TooltipText;
            Fillin();
        }

        public void Dispose()
        {
            RowInformation = null;
            mColumnInfoList.Clear();
            mColumnInfoList = null;
        }

        public virtual void PopulateColumnInfo()
        {
            foreach (ColumnDelegateStruct column in mColumnActions)
            {
                mColumnInfoList.Add(column.mInfo());
            }
        }

        public virtual void AdaptToMenu(TabInfo tabInfo)
        {
        }

        public virtual bool OnActivation() => true;

        public void UpdateMenuObject()
        {
            for (int i = 0; i < mColumnInfoList.Count; i++)
            {
                mColumnInfoList[i] = mColumnActions[i].mInfo();
            }
            Fillin();
        }
    }

    public class GenericActionObject : MenuObject
    {
        protected readonly Function mCallback;

        public GenericActionObject(string name, GenericDelegate<string> getValue, GenericDelegate<bool> predicate, Function action)
        {
            mCallback = action;
            Predicate = predicate;
            mColumnActions = new()
            {
                new(ColumnType.kText, () => new TextColumn(name)),
                new(ColumnType.kText, () => new TextColumn(getValue()))
            };
            PopulateColumnInfo();
            Fillin();
        }

        public GenericActionObject(List<ColumnDelegateStruct> columns, GenericDelegate<bool> predicate, Function action)
        {
            mCallback = action;
            Predicate = predicate;
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
            mColumnActions = new()
            {
                new(ColumnType.kText, () => new TextColumn(name)),
                new(ColumnType.kText, () => new TextColumn(""))
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

    public class OneShotActionObject : MenuObject
    {
        private readonly GenericDelegate<bool> mCallback;

        public OneShotActionObject(string name, GenericDelegate<string> getValue, GenericDelegate<bool> test, GenericDelegate<bool> action)
        {
            mCallback = action;
            Predicate = test;
            mColumnActions = new()
            {
                new(ColumnType.kText, () => new TextColumn(name)), 
                new(ColumnType.kText, () => new TextColumn(getValue()))
            };
            PopulateColumnInfo();
            Fillin();
        }

        public OneShotActionObject(List<ColumnDelegateStruct> columns, GenericDelegate<bool> test, GenericDelegate<bool> action)
        {
            mCallback = action;
            Predicate = test;
            mColumnActions = columns;
            PopulateColumnInfo();
            Fillin();
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

    public sealed class SetValuePromptObject<T> : MenuObject where T : struct
    {
        private readonly IPersistedSettings mSettings;

        private readonly PropertyInfo mSetting;

        private readonly string mMenuTitle;

        private readonly string mDialogPrompt;

        public SetValuePromptObject(string menuTitle, string dialogPrompt, string settingName, GenericDelegate<bool> test, IPersistedSettings settings)
        {
            mSettings = settings;
            mSetting = mSettings.GetType().GetProperty(settingName);
            mMenuTitle = menuTitle;
            mDialogPrompt = dialogPrompt;
            Predicate = test;
            mColumnActions = new()
            {
                new(ColumnType.kText, () => new TextColumn(mMenuTitle)),
                new(ColumnType.kText, () => new TextColumn(mSetting.GetValue(mSettings, null).ToString()))
            };
            PopulateColumnInfo();
            Fillin();
        }

        public SetValuePromptObject(string menuTitle, string dialogPrompt, string settingName, GenericDelegate<bool> test, IPersistedSettings settings, List<ColumnDelegateStruct> columns)
        {
            mSettings = settings;
            mSetting = mSettings.GetType().GetProperty(settingName);
            mMenuTitle = menuTitle;
            mDialogPrompt = dialogPrompt;
            Predicate = test;
            mColumnActions = columns;
            PopulateColumnInfo();
            Fillin();
        }

        public override bool OnActivation()
        {
            try
            {
                string str = StringInputDialog.Show(mMenuTitle, mDialogPrompt, mSetting.GetValue(mSettings, null).ToString());
                object[] args = { str, null };
                if (Reflection.StaticInvoke<bool>(typeof(T), "TryParse", args, new[] { typeof(string), typeof(T).MakeByRefType() }))
                {
                    mSetting.SetValue(mSettings, args[1], null);
                }
            }
            catch
            {
            }
            return false;
        }
    }
}