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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml;
using static Sims3.UI.ObjectPicker;
using Environment = System.Environment;

namespace Gamefreak130.Common
{
    public delegate T GenericDelegate<T>();

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
        public static void InjectInteraction<TTarget>(ref InteractionDefinition singleton, InteractionDefinition newSingleton, bool requiresTuning) where TTarget : IGameObject 
            => InjectInteraction<TTarget, InteractionDefinition>(ref singleton, newSingleton, requiresTuning);

        public static void InjectInteraction<TTarget>(ref ISoloInteractionDefinition singleton, ISoloInteractionDefinition newSingleton, bool requiresTuning) where TTarget : IGameObject
        {
            if (requiresTuning)
            {
                Tunings.Inject(singleton.GetType(), typeof(TTarget), newSingleton.GetType(), typeof(TTarget), true);
            }
            singleton = newSingleton;
        }

        public static void InjectInteraction<TTarget, TDefinition>(ref TDefinition singleton, TDefinition newSingleton, bool requiresTuning) where TTarget : IGameObject where TDefinition : InteractionDefinition
        {
            if (requiresTuning)
            {
                Tunings.Inject(singleton.GetType(), typeof(TTarget), newSingleton.GetType(), typeof(TTarget), true);
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

        public static bool FindAssembly(string str, bool matchExact = false)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                string name = assembly.GetName().Name;
                if (matchExact)
                {
                    if (name == str)
                    {
                        return true;
                    }
                }
                else if (name.Contains(str))
                {
                    return true;
                }
            }
            return false;
        }

        public static List<T> CloneList<T>(IEnumerable<T> old) => old is not null ? new(old) : null;

        public static object CoinFlipSelect(object obj1, object obj2) => RandomUtil.CoinFlip() ? obj1 : obj2;

        public static int Max(params int[] values)
        {
            if (values.Length == 0)
            {
                throw new ArgumentException("No values to compare");
            }
            if (values.Length == 1)
            {
                return values[0];
            }
            int runningMax = values[0];
            for (int i = 1; i < values.Length - 1; i++)
            {
                runningMax = Math.Max(runningMax, values[i]);
            }
            return runningMax;
        }

        public static int Min(params int[] values)
        {
            if (values.Length == 0)
            {
                throw new ArgumentException("No values to compare");
            }
            if (values.Length == 1)
            {
                return values[0];
            }
            int runningMin = values[0];
            for (int i = 1; i < values.Length - 1; i++)
            {
                runningMin = Math.Min(runningMin, values[i]);
            }
            return runningMin;
        }
    }

    public static class Reflection
    {
        public static void StaticInvoke(string assemblyQualifiedTypeName, string methodName, object[] args, Type[] argTypes) => StaticInvoke(Type.GetType(assemblyQualifiedTypeName), methodName, args, argTypes);

        public static void StaticInvoke(Type type, string methodName, object[] args, Type[] argTypes)
        {
            if (type is null)
            {
                throw new ArgumentNullException("type");
            }
            if (type.GetMethod(methodName, argTypes) is not MethodInfo method)
            {
                throw new MissingMethodException("No method found in type with specified name and args");
            }
            method.Invoke(null, args);
        }

        public static T StaticInvoke<T>(string assemblyQualifiedTypeName, string methodName, object[] args, Type[] argTypes) => StaticInvoke<T>(Type.GetType(assemblyQualifiedTypeName), methodName, args, argTypes);

        public static T StaticInvoke<T>(Type type, string methodName, object[] args, Type[] argTypes)
            => type is null
            ? throw new ArgumentNullException("type")
            : type.GetMethod(methodName, argTypes) is not MethodInfo method
            ? throw new MissingMethodException("No method found in type with specified name and args")
            : (T)method.Invoke(null, args);

        public static void InstanceInvoke(string assemblyQualifiedTypeName, object[] ctorArgs, Type[] ctorArgTypes, string methodName, object[] methodArgs, Type[] methodArgTypes)
            => InstanceInvoke(Type.GetType(assemblyQualifiedTypeName), ctorArgs, ctorArgTypes, methodName, methodArgs, methodArgTypes);

        public static void InstanceInvoke(Type type, object[] ctorArgs, Type[] ctorArgTypes, string methodName, object[] methodArgs, Type[] methodArgTypes)
        {
            if (type is null)
            {
                throw new ArgumentNullException("type");
            }
            if (type.GetConstructor(ctorArgTypes) is not ConstructorInfo ctor)
            {
                throw new MissingMethodException(type.FullName, ".ctor()");
            }
            InstanceInvoke(ctor.Invoke(ctorArgs), methodName, methodArgs, methodArgTypes);
        }

        public static void InstanceInvoke(object obj, string methodName, object[] args, Type[] argTypes)
        {
            if (obj is null)
            {
                throw new ArgumentNullException("Instance object");
            }
            if (obj.GetType().GetMethod(methodName, argTypes) is not MethodInfo method)
            {
                throw new MissingMethodException("No method found in instance with specified name and args");
            }
            method.Invoke(obj, args);
        }

        public static T InstanceInvoke<T>(string assemblyQualifiedTypeName, object[] ctorArgs, Type[] ctorArgTypes, string methodName, object[] methodArgs, Type[] methodArgTypes) 
            => InstanceInvoke<T>(Type.GetType(assemblyQualifiedTypeName), ctorArgs, ctorArgTypes, methodName, methodArgs, methodArgTypes);

        public static T InstanceInvoke<T>(Type type, object[] ctorArgs, Type[] ctorArgTypes, string methodName, object[] methodArgs, Type[] methodArgTypes)
            => type is null
            ? throw new ArgumentNullException("type")
            : type.GetConstructor(ctorArgTypes) is not ConstructorInfo ctor
            ? throw new MissingMethodException(type.FullName, ".ctor()")
            : InstanceInvoke<T>(ctor.Invoke(ctorArgs), methodName, methodArgs, methodArgTypes);

        public static T InstanceInvoke<T>(object obj, string methodName, object[] args, Type[] argTypes) 
            => obj is null 
            ? throw new ArgumentNullException("Instance object")
            : obj.GetType().GetMethod(methodName, argTypes) is not MethodInfo method
            ? throw new MissingMethodException("No method found in instance with specified name and args")
            : (T)method.Invoke(obj, args);
    }

    public abstract class Logger<T>
    {
        static Logger()
        {
            Assembly assembly = typeof(Logger<T>).Assembly;
            sName = assembly.GetName().Name;
            sModVersion = (Attribute.GetCustomAttribute(assembly, typeof(AssemblyFileVersionAttribute)) as AssemblyFileVersionAttribute).Version;
            sGameVersionData = GameUtils.GetGenericString(GenericStringID.VersionData).Split('\n');
        }

        protected static readonly string sName;

        private static readonly string sModVersion;

        private static readonly string[] sGameVersionData;

        public abstract void Log(T input);

        protected void WriteLog(StringBuilder content) => WriteLog(content, $"ScriptError_{sName}_{DateTime.Now:M-d-yyyy_hh-mm-ss}__");

        protected virtual void WriteLog(StringBuilder content, string fileName)
        {
            uint fileHandle = 0;
            try
            {
                Simulator.CreateExportFile(ref fileHandle, fileName);
                if (fileHandle != 0)
                {
                    CustomXmlWriter xmlWriter = new CustomXmlWriter(fileHandle);
                    xmlWriter.WriteStartDocument();
                    xmlWriter.WriteToBuffer(GenerateXmlWrapper(content));
                    xmlWriter.FlushBufferToFile();
                }
                Notify();
            }
            finally
            {
                if (fileHandle != 0)
                {
                    Simulator.CloseScriptErrorFile(fileHandle);
                }
            }
        }

        private string GenerateXmlWrapper(StringBuilder content)
        {
            StringBuilder xmlBuilder = new();
            xmlBuilder.AppendLine($"<{sName}>");
            xmlBuilder.AppendLine($"<ModVersion value=\"{sModVersion}\"/>");
            xmlBuilder.AppendLine($"<GameVersion value=\"{sGameVersionData[0]} ({sGameVersionData[5]}) ({sGameVersionData[7]})\"/>");
            xmlBuilder.AppendLine($"<InstalledPacks value=\"{GameUtils.sProductFlags}\"/>");
            // The logger expects the content to have a new line at the end of it
            // More new lines are appended here to create exactly one line of padding before and after the XML tags
            xmlBuilder.AppendLine("<Content>" + Environment.NewLine);
            xmlBuilder.Append(content.Replace("&", "&amp;"));
            xmlBuilder.AppendLine(Environment.NewLine + "</Content>");
            xmlBuilder.AppendLine("<LoadedAssemblies>");
            xmlBuilder.Append(GenerateAssemblyList());
            xmlBuilder.AppendLine("</LoadedAssemblies>");
            xmlBuilder.Append($"</{sName}>");
            return xmlBuilder.ToString();
        }

        private StringBuilder GenerateAssemblyList()
        {
            StringBuilder result = new();
            List<string> assemblies = new List<Assembly>(AppDomain.CurrentDomain.GetAssemblies())
                                            .ConvertAll((assembly) => assembly.GetName().Name);
            assemblies.Sort();
            foreach (string assembly in assemblies)
            {
                result.AppendLine(" " + assembly);
            }
            return result;
        }

        protected virtual void Notify()
        {
        }
    }

    /// <summary>Logger for one-time events.</summary>
    /// <remarks>Any received input to log is immediately converted to a string and written to a new log file,
    /// along with timestamps and the rest of the standard log info.</remarks>
    public abstract class EventLogger<T> : Logger<T>
    {
        public override void Log(T input) => WriteLog(new(input.ToString()));

        protected override void WriteLog(StringBuilder content, string fileName)
        {
            StringBuilder log = new();
            log.AppendLine("Logged At:");
            log.AppendLine($" Sim Time: {SimClock.CurrentTime()}");
            log.AppendLine(" Real Time: " + DateTime.Now + Environment.NewLine);
            log.Append(content);
            base.WriteLog(content, fileName);
        }
    }

    public class ExceptionLogger : EventLogger<Exception>
    {
        private ExceptionLogger()
        {
        }

        internal static readonly ExceptionLogger sInstance = new();

        protected override void Notify() => StyledNotification.Show(new StyledNotification.Format($"Error occurred in {sName}\n\nAn error log has been created in your user directory. Please send it to Gamefreak130 for further review.", StyledNotification.NotificationStyle.kSystemMessage));
    }

    [Persistable]
    public abstract class Settings
    {
        public string Export()
        {
            StringBuilder text = new("<?xml version=\"1.0\" encoding=\"utf-8\"?><Settings>");
            WriteObject(text, this);
            return text.Append("</Settings>").ToString();
        }

        private static void WriteObject(StringBuilder text, object val)
        {
            if (val is null)
            {
                throw new ArgumentNullException("Object value");
            }
            Type type = val.GetType();
            if (type == typeof(string) || type == typeof(decimal) || type.IsEnum || type.IsPrimitive)
            {
                text.Append(XmlConvert.EncodeLocalName(val.ToString()));
            }
            else if ((type.IsGenericType || type.IsArray) && val is IList list)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    text.Append($"<i_{i}>");
                    WriteObject(text, list[i]);
                    text.Append($"</i_{i}>");
                }
            }
            else if (type.IsGenericType && val is IDictionary dict)
            {
                foreach (DictionaryEntry entry in dict)
                {
                    if (entry.Key is not Enum and not string and not decimal && !entry.Key.GetType().IsPrimitive)
                    {
                        throw new ArgumentException("Dictionary keys must be a primitive type, enum, or string");
                    }
                    string key = XmlConvert.EncodeLocalName(entry.Key.ToString());
                    text.Append($"<{key}>");
                    WriteObject(text, entry.Value);
                    text.Append($"</{key}>");
                }
            }
            else
            {
                foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    string name = XmlConvert.EncodeLocalName(field.Name);
                    text.Append($"<{name}>");
                    WriteObject(text, field.GetValue(val));
                    text.Append($"</{name}>");
                }
            }
        }

        public void Import(XmlDocument xml, bool replaceDictionaries = false) => ReadObject(this, xml.DocumentElement, replaceDictionaries);

        private static object ReadObject(object currentObj, XmlNode objNode, bool replaceDictionaries)
        {
            if (currentObj is null)
            {
                throw new ArgumentNullException("Object value");
            }
            Type type = currentObj.GetType();
            if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal))
            {
                return Convert.ChangeType(XmlConvert.DecodeName(objNode.InnerText), type);
            }
            else if (type.IsEnum)
            {
                try
                {
                    return Enum.Parse(currentObj.GetType(), XmlConvert.DecodeName(objNode.InnerText));
                }
                catch (ArgumentException)
                {
                }
            }
            else if (type.IsGenericType && currentObj is IDictionary currentDict)
            {
                Type[] generics = type.GetGenericArguments();
                if (!generics[0].IsEnum && generics[0] != typeof(string) && generics[0] != typeof(decimal) && !generics[0].IsPrimitive)
                {
                    throw new ArgumentException("Dictionary keys must be a primitive type, enum, or string");
                }
                IDictionary newDict = Activator.CreateInstance(type, true) as IDictionary;
                foreach (XmlNode node in objNode.ChildNodes)
                {
                    object key = null;
                    string name = XmlConvert.DecodeName(node.Name);
                    if (generics[0].IsEnum)
                    {
                        try
                        {
                            key = Enum.Parse(generics[0], name);
                        }
                        catch (ArgumentException)
                        {
                        }
                    }
                    else
                    {
                        key = Convert.ChangeType(name, generics[0]);
                    }

                    object val = generics[1] == typeof(string) ? string.Empty : Activator.CreateInstance(generics[1], true);
                    newDict[key] = ReadObject(val, node, replaceDictionaries);
                }
                if (replaceDictionaries)
                {
                    return newDict;
                }
                foreach (DictionaryEntry entry in newDict)
                {
                    if (currentDict.Contains(entry.Key))
                    {
                        currentDict[entry.Key] = newDict[entry.Key];
                    }
                }
            }
            else if ((type.IsGenericType && type == typeof(IList)) || type.IsArray)
            {
                Type elementType = type.IsGenericType ? type.GetGenericArguments()[0] : type.GetElementType();
                IList list = type.IsArray ? Array.CreateInstance(elementType, objNode.ChildNodes.Count) : Activator.CreateInstance(type, true) as IList;
                for (int i = 0; i < objNode.ChildNodes.Count; i++)
                {
                    object val = elementType == typeof(string) ? string.Empty : Activator.CreateInstance(elementType, true);
                    val = ReadObject(val, objNode[$"i_{i}"], replaceDictionaries);
                    if (list.IsFixedSize)
                    {
                        list[i] = val;
                    }
                    else
                    {
                        list.Add(val);
                    }
                }
                return list;
            }
            else
            {
                Dictionary<string, FieldInfo> fields = new();
                foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    fields.Add(field.Name, field);
                }
                foreach (XmlNode node in objNode.ChildNodes)
                {
                    if (fields.TryGetValue(XmlConvert.DecodeName(node.Name), out FieldInfo field))
                    {
                        object val = field.GetValue(currentObj);
                        if (val is null)
                        {
                            throw new ArgumentNullException($"{field.Name} in {type.FullName}");
                        }
                        field.SetValue(currentObj, ReadObject(val, node, replaceDictionaries));
                    }
                }
            }
            return currentObj;
        }
    }
}

namespace Gamefreak130.Common.UI
{
    public struct ColumnDelegateStruct
    {
        public ColumnType mColumnType;

        public GenericDelegate<ColumnInfo> mInfo;

        public ColumnDelegateStruct(ColumnType colType, GenericDelegate<ColumnInfo> infoDelegate)
        {
            mColumnType = colType;
            mInfo = infoDelegate;
        }
    }

    public struct RowTextFormat
    {
        public Color mTextColor;

        public bool mBoldTextStyle;

        public string mTooltip;

        public RowTextFormat(Color textColor, bool boldText, string tooltipText)
        {
            mTextColor = textColor;
            mBoldTextStyle = boldText;
            mTooltip = tooltipText;
        }
    }

    /// <summary>
    /// An <see cref="ObjectPickerDialog"/> which allows for the okay button to be clicked with no items selected, in which case an empty RowInfo list is returned
    /// </summary>
    public class ObjectPickerDialogEx : ObjectPickerDialog
    {
        public ObjectPickerDialogEx(bool modal, PauseMode pauseMode, string title, string buttonTrue, string buttonFalse, List<TabInfo> listObjs, List<HeaderInfo> headers, int numSelectableRows, Vector2 position, bool viewTypeToggle, List<RowInfo> preSelectedRows, bool showHeadersAndToggle, bool disableCloseButton)
            : base(modal, pauseMode, title, buttonTrue, buttonFalse, listObjs, headers, numSelectableRows, position, viewTypeToggle, preSelectedRows, showHeadersAndToggle, disableCloseButton)
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
            using (ObjectPickerDialogEx objectPickerDialog = new(modal, pauseType, title, buttonTrue, buttonFalse, listObjs, headers, numSelectableRows, position, viewTypeToggle, preSelectedRows, showHeadersAndToggle, disableCloseButton))
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
                mResult = mTable.Selected ?? new();
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

    /// <summary>
    /// Used by <see cref="MenuController"/> to construct menus using <see cref="MenuObject"/>s with arbitrary functionality
    /// </summary>
    /// <seealso cref="MenuController"/>
    public class MenuContainer
    {
        private List<RowInfo> mRowInformation;

        private readonly string[] mTabImage;

        private readonly string[] mTabText;

        private readonly GenericDelegate<List<RowInfo>> mRowPopulationDelegate;

        private readonly List<RowInfo> mHiddenRows;

        public string MenuDisplayName { get; }

        public List<HeaderInfo> Headers { get; private set; }

        public List<TabInfo> TabInformation { get; private set; }

        public Action<List<RowInfo>> OnEnd { get; }

        public MenuContainer() : this("")
        {
        }

        public MenuContainer(string title) : this(title, "")
        {
        }

        public MenuContainer(string title, string subtitle) : this(title, new[] { "" }, new[] { subtitle }, null)
        {
        }

        public MenuContainer(string title, string[] tabImage, string[] tabName, Action<List<RowInfo>> onEndDelegate) : this(title, tabImage, tabName, onEndDelegate, null)
        {
        }

        public MenuContainer(string title, string[] tabImage, string[] tabName, Action<List<RowInfo>> onEndDelegate, GenericDelegate<List<RowInfo>> rowPopulationDelegate)
        {
            mHiddenRows = new();
            MenuDisplayName = title;
            mTabImage = tabImage;
            mTabText = tabName;
            OnEnd = onEndDelegate;
            Headers = new();
            mRowInformation = new();
            TabInformation = new();
            mRowPopulationDelegate = rowPopulationDelegate;
            if (mRowPopulationDelegate is not null)
            {
                RefreshMenuObjects(0);
                if (mRowInformation.Count > 0)
                {
                    for (int i = 0; i < mRowInformation[0].ColumnInfo.Count; i++)
                    {
                        Headers.Add(new("Ui/Caption/ObjectPicker:Sim", "", 200));
                    }
                }
            }
        }

        public void RefreshMenuObjects(int tabnumber)
        {
            mRowInformation = mRowPopulationDelegate();
            TabInformation = new()
            {
                new("", mTabText[tabnumber], mRowInformation)
            };
        }

        public void SetHeaders(List<HeaderInfo> headers) => Headers = headers;

        public void SetHeader(int headerNumber, HeaderInfo headerInfos) => Headers[headerNumber] = headerInfos;

        public void ClearMenuObjects() => TabInformation.Clear();

        public void AddMenuObject(MenuObject menuItem)
        {
            if (TabInformation.Count < 1)
            {
                mRowInformation = new()
                {
                    menuItem.RowInformation
                };
                TabInformation.Add(new(mTabImage[0], mTabText[0], mRowInformation));
                Headers.Add(new("Ui/Caption/ObjectPicker:Name", "", 300));
                Headers.Add(new("Ui/Caption/ObjectPicker:Value", "", 100));
                return;
            }
            TabInformation[0].RowInfo.Add(menuItem.RowInformation);
        }

        public void AddMenuObject(List<HeaderInfo> headers, MenuObject menuItem)
        {

            if (TabInformation.Count < 1)
            {
                mRowInformation = new()
                {
                    menuItem.RowInformation
                };
                TabInformation.Add(new(mTabImage[0], mTabText[0], mRowInformation));
                Headers = headers;
                return;
            }
            TabInformation[0].RowInfo.Add(menuItem.RowInformation);
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
                if (item.Test())
                {
                    mHiddenRows.RemoveAt(i);
                    AddMenuObject(item);
                }
            }
            for (int i = TabInformation[0].RowInfo.Count - 1; i >= 0; i--)
            {
                MenuObject item = TabInformation[0].RowInfo[i].Item as MenuObject;
                if (item.Test is not null && !item.Test())
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

    /// <summary>
    /// Modal dialog utilizing <see cref="MenuContainer"/> to construct NRaas-like settings menus
    /// </summary>
    /// <seealso cref="MenuContainer"/>
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

        public MenuController(string title, string buttonTrue, string buttonFalse, List<TabInfo> listObjs, List<HeaderInfo> headers, bool showHeadersAndToggle, Action<List<RowInfo>> endResultDelegates)
            : this(true, PauseMode.PauseSimulator, title, buttonTrue, buttonFalse, listObjs, headers, showHeadersAndToggle, endResultDelegates)
        {
        }

        public MenuController(bool isModal, PauseMode pauseMode, string title, string buttonTrue, string buttonFalse, List<TabInfo> listObjs, List<HeaderInfo> headers, bool showHeadersAndToggle, Action<List<RowInfo>> endResultDelegates) 
            : base("UiObjectPicker", kWinExportID, isModal, pauseMode, null)
        {
            if (mModalDialogWindow is not null)
            {
                Text text = mModalDialogWindow.GetChildByID((uint)ControlIds.TitleText, false) as Text;
                text.Caption = title;
                mTable = mModalDialogWindow.GetChildByID((uint)ControlIds.ItemTable, false) as ObjectPicker;
                mTable.SelectionChanged += OnRowClicked;
                mTabsContainer = mTable.mTabs;
                mTable.mTable.mPopulationCompletedCallback += ResizeWindow;
                mOkayButton = mModalDialogWindow.GetChildByID((uint)ControlIds.OkayButton, false) as Button;
                mOkayButton.TooltipText = buttonTrue;
                mOkayButton.Enabled = true;
                mOkayButton.Click += OnOkayButtonClick;
                OkayID = mOkayButton.ID;
                SelectedID = mOkayButton.ID;
                mCloseButton = mModalDialogWindow.GetChildByID((uint)ControlIds.CancelButton, false) as Button;
                mCloseButton.TooltipText = buttonFalse;
                mCloseButton.Click += OnCloseButtonClick;
                CancelID = mCloseButton.ID;
                mTableOffset = mModalDialogWindow.Area.BottomRight - mModalDialogWindow.Area.TopLeft - (mTable.Area.BottomRight - mTable.Area.TopLeft);
                mTable.ShowHeaders = showHeadersAndToggle;
                mTable.ViewTypeToggle = false;
                mTable.ShowToggle = false;
                mTable.Populate(listObjs, headers, 1);
                ResizeWindow();
            }
            EndDelegates = endResultDelegates;
        }

        public void PopulateMenu(List<TabInfo> tabinfo, List<HeaderInfo> headers, int numSelectableRows) => mTable.Populate(tabinfo, headers, numSelectableRows);

        public override void Dispose() => Dispose(true);

        public void AddRow(int Tabnumber, RowInfo info)
        {
            mTable.mItems[Tabnumber].RowInfo.Clear();
            mTable.mItems[Tabnumber].RowInfo.Add(info);
            Repopulate();
        }

        public void SetTableColor(Color color) => mModalDialogWindow.GetChildByID((uint)ControlIds.TableBezel, false).ShadeColor = color;

        public void SetTitleText(string text) => (mModalDialogWindow.GetChildByID((uint)ControlIds.TitleText, false) as Text).Caption = text;

        public void SetTitleText(string text, Color textColor)
        {
            (mModalDialogWindow.GetChildByID((uint)ControlIds.TitleText, false) as Text).Caption = text;
            (mModalDialogWindow.GetChildByID((uint)ControlIds.TitleText, false) as Text).TextColor = textColor;
        }

        public void SetTitleText(string text, Color textColor, uint textStyle)
        {
            (mModalDialogWindow.GetChildByID((uint)ControlIds.TitleText, false) as Text).Caption = text;
            (mModalDialogWindow.GetChildByID((uint)ControlIds.TitleText, false) as Text).TextColor = textColor;
            (mModalDialogWindow.GetChildByID((uint)ControlIds.TitleText, false) as Text).TextStyle = textStyle;
        }

        public void SetTitleText(string text, Color textColor, bool textStyleBold)
        {
            (mModalDialogWindow.GetChildByID((uint)ControlIds.TitleText, false) as Text).Caption = text;
            (mModalDialogWindow.GetChildByID((uint)ControlIds.TitleText, false) as Text).TextColor = textColor;
            if (textStyleBold)
            {
                (mModalDialogWindow.GetChildByID((uint)ControlIds.TitleText, false) as Text).TextStyle = 2u;
            }
        }

        public void SetTitleTextColor(Color textColor) => (mModalDialogWindow.GetChildByID((uint)ControlIds.TitleText, false) as Text).TextColor = textColor;

        public void SetTitleTextStyle(uint textStyle) => (mModalDialogWindow.GetChildByID((uint)ControlIds.TitleText, false) as Text).TextStyle = textStyle;

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
            Text text = mModalDialogWindow.GetChildByID((uint)ControlIds.TitleText, false) as Text;
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

        /// <summary>Creates and shows a new submenu from the given <see cref="MenuContainer"/>, invoking <see cref="MenuObject.OnActivation()"/> when a <see cref="MenuObject"/> is selected</summary>
        /// <param name="container">The <see cref="MenuContainer"/> used to generate the menu</param>
        /// <param name="showHeaders">Whether or not to show headers at the top of the menu table. Defaults to <see langword="true"/>.</param>
        /// <returns>
        ///     <para><see langword="true"/> to terminate the entire menu tree.</para>
        ///     <para><see langword="false"/> to return control to the invoker of the function.</para>
        /// </returns>
        /// <seealso cref="MenuObject.OnActivation()"/>
        public static bool ShowMenu(MenuContainer container, bool showHeaders = true) => ShowMenu(container, 0, showHeaders);

        /// <summary>Creates and shows a new submenu from the given <see cref="MenuContainer"/>, invoking <see cref="MenuObject.OnActivation()"/> when a <see cref="MenuObject"/> is selected</summary>
        /// <param name="container">The <see cref="MenuContainer"/> used to generate the menu</param>
        /// <param name="tab">The index of the tab that the submenu will open in</param>
        /// <param name="showHeaders">Whether or not to show headers at the top of the menu table. Defaults to <see langword="true"/>.</param>
        /// <returns>
        ///     <para><see langword="true"/> to terminate the entire menu tree.</para>
        ///     <para><see langword="false"/> to return control to the invoker of the function.</para>
        /// </returns>
        /// <seealso cref="MenuObject.OnActivation()"/>
        public static bool ShowMenu(MenuContainer container, int tab, bool showHeaders = true)
        {
            try
            {
                while (true)
                {
                    container.UpdateItems();
                    MenuController controller = Show(container, tab, showHeaders);
                    if (controller.Okay)
                    {
                        if (controller.Result?[0]?.Item is MenuObject menuObject)
                        {
                            if (menuObject.OnActivation())
                            {
                                return true;
                            }
                            continue;
                        }
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                ExceptionLogger.sInstance.Log(ex);
                return true;
            }
        }

        private static MenuController Show(MenuContainer container, int tab, bool showHeaders)
        {
            Sims3.Gameplay.Gameflow.SetGameSpeed(Gameflow.GameSpeed.Pause, Sims3.Gameplay.Gameflow.SetGameSpeedContext.GameStates);
            MenuController menuController = new(container.MenuDisplayName, Localization.LocalizeString("Ui/Caption/Global:Ok"), Localization.LocalizeString("Ui/Caption/Global:Cancel"), container.TabInformation, container.Headers, showHeaders, container.OnEnd);
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

    /// <summary>
    /// Represents a single item within a <see cref="MenuController"/> dialog with arbitrary behavior upon selection
    /// </summary>
    public abstract class MenuObject : IDisposable
    {
        private List<ColumnInfo> mColumnInfoList;

        protected List<ColumnDelegateStruct> mColumnActions;

        private RowTextFormat mTextFormat;

        public GenericDelegate<bool> Test { get; protected set; }

        public RowInfo RowInformation { get; private set; }

        public MenuObject() : this(new List<ColumnDelegateStruct>(), null)
        {
        }

        public MenuObject(List<ColumnDelegateStruct> columns, GenericDelegate<bool> test)
        {
            mColumnInfoList = new();
            mColumnActions = columns;
            Test = test;
            PopulateColumnInfo();
            Fillin();
        }

        public MenuObject(string name, GenericDelegate<bool> test) : this(name, () => "", test)
        {
        }

        public MenuObject(string name, GenericDelegate<string> getValue, GenericDelegate<bool> test)
        {
            mColumnInfoList = new();
            Test = test;
            mColumnActions = new()
            {
                new(ColumnType.kText, () => new TextColumn(name)),
                new(ColumnType.kText, () => new TextColumn(getValue()))
            };
            PopulateColumnInfo();
            Fillin();
        }

        public void Fillin() => RowInformation = new(this, mColumnInfoList);

        public void Fillin(Color textColor)
        {
            mTextFormat.mTextColor = textColor;
            Fillin();
        }

        public void Fillin(Color textColor, bool boldTextStyle)
        {
            mTextFormat.mTextColor = textColor;
            Fillin(boldTextStyle);
        }

        public void Fillin(bool boldTextStyle)
        {
            mTextFormat.mBoldTextStyle = boldTextStyle;
            Fillin();
        }

        public void Fillin(string tooltipText)
        {
            mTextFormat.mTooltip = tooltipText;
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

        /// <summary>Callback method raised by <see cref="MenuController"/> when a <see cref="MenuObject"/> is selected</summary>
        /// <returns><see langword="true"/> if entire menu tree should be termined; otherwise, <see langword="false"/> to return control to the containing <see cref="MenuController"/></returns>
        /// <seealso cref="MenuController.ShowMenu(MenuContainer)"/>
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

    /// <summary>
    /// A <see cref="MenuObject"/> that performs a one-shot function before returning control to the containing <see cref="MenuController"/>
    /// </summary>
    public class GenericActionObject : MenuObject
    {
        protected readonly Function mCallback;

        public GenericActionObject(string name, GenericDelegate<bool> test, Function action) : base(name, test)
            => mCallback = action;

        public GenericActionObject(string name, GenericDelegate<string> getValue, GenericDelegate<bool> test, Function action) : base(name, getValue, test)
            => mCallback = action;

        public GenericActionObject(List<ColumnDelegateStruct> columns, GenericDelegate<bool> test, Function action) : base(columns, test) 
            => mCallback = action;

        public override bool OnActivation()
        {
            mCallback();
            return false;
        }
    }

    /// <summary>
    /// A <see cref="MenuObject"/> that creates and shows a new submenu from a given <see cref="MenuContainer"/> on activation
    /// </summary>
    public sealed class GenerateMenuObject : MenuObject
    {
        private readonly MenuContainer mToOpen;

        public GenerateMenuObject(string name, GenericDelegate<bool> test, MenuContainer toOpen) : base(name, test) 
            => mToOpen = toOpen;

        public GenerateMenuObject(List<ColumnDelegateStruct> columns, GenericDelegate<bool> test, MenuContainer toOpen) : base(columns, test) 
            => mToOpen = toOpen;

        public override bool OnActivation() => MenuController.ShowMenu(mToOpen);
    }

    /// <summary>
    /// <para>A <see cref="MenuObject"/> that performs a predicate on activation.</para> 
    /// <para>If the predicate returns <see langword="true"/>, then the entire menu tree terminates; if it returns <see langword="false"/>, then control returns to the containing <see cref="MenuController"/></para>
    /// </summary>
    public class ConditionalActionObject : MenuObject
    {
        private readonly GenericDelegate<bool> mPredicate;

        public ConditionalActionObject(string name, GenericDelegate<bool> test, GenericDelegate<bool> action) : base(name, test)
            => mPredicate = action;

        public ConditionalActionObject(string name, GenericDelegate<string> getValue, GenericDelegate<bool> test, GenericDelegate<bool> action) : base(name, getValue, test) 
            => mPredicate = action;

        public ConditionalActionObject(List<ColumnDelegateStruct> columns, GenericDelegate<bool> test, GenericDelegate<bool> action) : base(columns, test)
            => mPredicate = action;

        public override bool OnActivation() => mPredicate();
    }

    /// <summary>
    /// <para>A <see cref="MenuObject"/> that prompts the user to enter a new string value for a given <typeparamref name="T"/> (or toggles a boolean value).</para> 
    /// <para>Control is returned to the containing <see cref="MenuController"/>, regardless of the result of toggling or converting to <typeparamref name="T"/></para>
    /// </summary>
    /// <typeparam name="T">The type of the value to set</typeparam>
    public abstract class SetSimpleValueObject<T> : MenuObject where T : IConvertible
    {
        protected delegate void SetValueDelegate(T val);

        protected readonly string mMenuTitle;

        protected readonly string mDialogPrompt;

        protected GenericDelegate<T> mGetValue;

        protected SetValueDelegate mSetValue;

        public SetSimpleValueObject(string menuTitle, string dialogPrompt, GenericDelegate<bool> test) : this(menuTitle, dialogPrompt, new(), test)
        {
        }

        public SetSimpleValueObject(string menuTitle, string dialogPrompt, List<ColumnDelegateStruct> columns, GenericDelegate<bool> test) : base(columns, test)
        {
            mMenuTitle = menuTitle;
            mDialogPrompt = dialogPrompt;
        }

        protected void ConstructDefaultColumnInfo()
        {
            mColumnActions = new()
            {
                new(ColumnType.kText, () => new TextColumn(mMenuTitle)),
                new(ColumnType.kText, () => new TextColumn(mGetValue().ToString()))
            };
            PopulateColumnInfo();
            Fillin();
        }

        public override bool OnActivation()
        {
            try
            {
                Type t = typeof(T);
                T val = default;
                if (t == typeof(bool))
                {
                    // Holy boxing Batman
                    val = (T)(object)!(bool)(object)mGetValue();
                }
                else
                {
                    string str = StringInputDialog.Show(mMenuTitle, mDialogPrompt, mGetValue().ToString());
                    if (str is not null)
                    {
                        val = t.IsEnum ? (T)Enum.Parse(t, str) : (T)Convert.ChangeType(str, t);
                    }
                }

                if (val is not null)
                {
                    mSetValue(val);
                }
            }
            catch (FormatException)
            {
            }
            catch (OverflowException)
            {
            }
            catch (ArgumentException)
            {
            }
            return false;
        }
    }

    /// <summary>
    /// A <see cref="SetSimpleValueObject{T}"/> that sets the value of a readable and writable property in a given <see cref="object"/>
    /// </summary>
    /// <typeparam name="T">The type of the given property</typeparam>
    public sealed class SetSimplePropertyObject<T> : SetSimpleValueObject<T> where T : IConvertible
    {
        public SetSimplePropertyObject(string menuTitle, string propertyName, GenericDelegate<bool> test, object obj) : this(menuTitle, "", propertyName, test, obj)
        {
        }

        public SetSimplePropertyObject(string menuTitle, string dialogPrompt, string propertyName, GenericDelegate<bool> test, object obj) : base(menuTitle, dialogPrompt, test)
        {
            PropertyInfo mProperty = obj.GetType().GetProperty(propertyName, typeof(T));
            if (mProperty is null)
            {
                throw new ArgumentException("Property with given return type not found in object");
            }
            if (!mProperty.CanWrite || !mProperty.CanRead)
            {
                throw new MissingMethodException("Property must have a get and set accessor");
            }
            mGetValue = () => (T)mProperty.GetValue(obj, null);
            mSetValue = (val) => mProperty.SetValue(obj, val, null);
            ConstructDefaultColumnInfo();
        }

        public SetSimplePropertyObject(string menuTitle, string propertyName, GenericDelegate<bool> test, object obj, List<ColumnDelegateStruct> columns) : this(menuTitle, "", propertyName, test, obj, columns)
        {
        }

        public SetSimplePropertyObject(string menuTitle, string dialogPrompt, string propertyName, GenericDelegate<bool> test, object obj, List<ColumnDelegateStruct> columns) : base(menuTitle, dialogPrompt, columns, test)
        {
            PropertyInfo mProperty = obj.GetType().GetProperty(propertyName);
            if (mProperty.PropertyType != typeof(T))
            {
                throw new ArgumentException("Type mismatch between property and return value");
            }
            if (!mProperty.CanWrite || !mProperty.CanRead)
            {
                throw new MissingMethodException("Property must have a get and set accessor");
            }
            mGetValue = () => (T)mProperty.GetValue(obj, null);
            mSetValue = (val) => mProperty.SetValue(obj, val, null);
        }
    }

    /// <summary>
    /// A <see cref="SetSimpleValueObject{T}"/> that sets the value of a given <typeparamref name="TKey"/> in a given <see cref="IDictionary{TKey, TValue}"/>
    /// </summary>
    /// <typeparam name="TKey">The type of the given dictionary's keys</typeparam>
    /// <typeparam name="TValue">The type of the given dictionary's values</typeparam>
    public sealed class SetSimpleDictionaryValueObject<TKey, TValue> : SetSimpleValueObject<TValue> where TValue : IConvertible
    {
        public SetSimpleDictionaryValueObject(string menuTitle, IDictionary<TKey, TValue> dict, TKey key, GenericDelegate<bool> test) : this(menuTitle, "", dict, key, test)
        {
        }

        public SetSimpleDictionaryValueObject(string menuTitle, string dialogPrompt, IDictionary<TKey, TValue> dict, TKey key, GenericDelegate<bool> test) : base(menuTitle, dialogPrompt, test)
        {
            if (!dict.ContainsKey(key))
            {
                throw new ArgumentException("Key not in dictionary");
            }
            mGetValue = () => dict[key];
            mSetValue = (val) => dict[key] = val;
            ConstructDefaultColumnInfo();
        }

        public SetSimpleDictionaryValueObject(string menuTitle, IDictionary<TKey, TValue> dict, TKey key, List<ColumnDelegateStruct> columns, GenericDelegate<bool> test) : this(menuTitle, "", dict, key, columns, test)
        {
        }

        public SetSimpleDictionaryValueObject(string menuTitle, string dialogPrompt, IDictionary<TKey, TValue> dict, TKey key, List<ColumnDelegateStruct> columns, GenericDelegate<bool> test) : base(menuTitle, dialogPrompt, columns, test)
        {
            if (!dict.ContainsKey(key))
            {
                throw new ArgumentException("Key not in dictionary");
            }
            mGetValue = () => dict[key];
            mSetValue = (val) => dict[key] = val;
        }
    }
}