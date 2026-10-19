using System;
using System.Linq;

namespace taskt.Core.Automation.Attributes.PropertyAttributes
{
    /// <summary>
    /// for command property(parameter) attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class ACommandPropertyAttribute : Attribute
    {
        // nothing
    }

    /// <summary>
    /// for single command property(parameter) attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class ASingleComamndPropertyAttribute : ACommandPropertyAttribute
    {
        // nothing
    }

    /// <summary>
    /// for multi values command property(parameter) attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public abstract class AMultiCommandPropertyAttribute : ACommandPropertyAttribute
    {
        // nothing
    }

    #region enum
    /// <summary>
    /// Multi Values command property attributes behavior in VirtualProperty
    /// </summary>
    public enum MultiAttributesBehavior
    {
        Merge = 0,
        Overwrite,
    }
    #endregion

    #region Parameter Order
    //[AttributeUsage(AttributeTargets.Property)]
    /// <summary>
    /// parameter order ASC
    /// </summary>
    public sealed class PropertyParameterOrder : ASingleComamndPropertyAttribute
    {
        public int order = 3000;
        public PropertyParameterOrder()
        {
        }
        public PropertyParameterOrder(int order)
        {
            this.order = order;
        }
    }
    #endregion

    #region Replace pattern
    // TODO: make sometime
    #endregion

    #region Virtual Property
    //[AttributeUsage(AttributeTargets.Property)]
    /// <summary>
    /// virtual property like class inherit
    /// </summary>
    public sealed class PropertyVirtualProperty : ASingleComamndPropertyAttribute
    {
        public string className;
        public string propertyName;
        public PropertyVirtualProperty(string className, string propertyName)
        {
            this.className = className;
            this.propertyName = propertyName;
        }

        public bool Equals(PropertyVirtualProperty other)
        {
            return (this.className == other.className) && (this.propertyName == other.propertyName);
        }
    }
    #endregion

    #region Intermediate/Raw
    //[AttributeUsage(AttributeTargets.Property)]
    /// <summary>
    /// specify script intermediate languate convert methods
    /// </summary>
    public sealed class PropertyIntermediateConvert : ASingleComamndPropertyAttribute
    {
        public string intermediateMethod = "";
        public string rawMethod = "";

        public PropertyIntermediateConvert()
        {
        }
        public PropertyIntermediateConvert(string intermediateMethod, string rawMethod)
        {
            this.intermediateMethod = intermediateMethod;
            this.rawMethod = rawMethod;
        }
    }
    #endregion

    #region to Label, Document
    //[AttributeUsage(AttributeTargets.Property)]
    /// <summary>
    /// input specification. like Text, Number, List, etc
    /// </summary>
    public sealed class InputSpecification : ASingleComamndPropertyAttribute
    {
        public string inputSpecification;
        public bool autoGenerate = false;

        public InputSpecification()
        {
            inputSpecification = "";
            autoGenerate = false;
        }

        public InputSpecification(string desc, bool autoGenerate = false)
        {
            this.inputSpecification = desc;
            this.autoGenerate = autoGenerate;
        }
    }

    //[AttributeUsage(AttributeTargets.Property)]
    /// <summary>
    /// sample usage. like "hello", {{{vList}}}, etc.
    /// </summary>
    public sealed class SampleUsage : ASingleComamndPropertyAttribute
    {
        public string sampleUsage;
        public SampleUsage(string desc)
        {
            this.sampleUsage = desc;
        }
    }

    //[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    /// <summary>
    /// detail sample usage. specify sample usage value, valueType, description.
    /// </summary>
    public sealed class PropertyDetailSampleUsage : AMultiCommandPropertyAttribute
    {
        public string sampleUsage = "";
        public string means = "";
        public bool showInDescription = true;
        public ValueType valueType = ValueType.Manual;
        public string target = "";

        public PropertyDetailSampleUsage(string sampleUsage, string means, bool showDescription = true)
        {
            this.sampleUsage = sampleUsage;
            this.means = means;
            this.showInDescription = showDescription;
        }
        public PropertyDetailSampleUsage(string sampleUsage, ValueType type, string target = "", bool showDescription = true)
        {
            this.sampleUsage = sampleUsage;
            this.valueType = type;
            this.target = target;
            this.showInDescription = showDescription;
        }

        public enum ValueType
        {
            Manual,
            Value,
            VariableValue,
            VariableName
        }
    }

    //[AttributeUsage(AttributeTargets.Property)]
    /// <summary>
    /// detail sample usage behavior
    /// </summary>
    public sealed class PropertyDetailSampleUsageBehavior : ASingleComamndPropertyAttribute
    {
        public MultiAttributesBehavior behavior = MultiAttributesBehavior.Merge;

        public PropertyDetailSampleUsageBehavior()
        {

        }
        public PropertyDetailSampleUsageBehavior(MultiAttributesBehavior behavior)
        {
            this.behavior = behavior;
        }
    }

    //[AttributeUsage(AttributeTargets.Property)]
    /// <summary>
    /// Remarks, memo.
    /// </summary>
    public sealed class Remarks : ASingleComamndPropertyAttribute
    {
        public string remarks;
        public Remarks(string desc)
        {
            this.remarks = desc;
        }
    }

    //[AttributeUsage(AttributeTargets.Property)]
    /// <summary>
    /// Property description. This value is displayed as a Description above each parameter in the CommandEditor form.
    /// </summary>
    public sealed class PropertyDescription : ASingleComamndPropertyAttribute
    {
        public string propertyDescription;
        public PropertyDescription(string description)
        {
            this.propertyDescription = description;
        }
    }

    //[AttributeUsage(AttributeTargets.Property)]
    /// <summary>
    /// show sample usage or not, in each parameter in the CommandEditor form.
    /// </summary>
    public sealed class PropertyShowSampleUsageInDescription : ASingleComamndPropertyAttribute
    {
        public bool showSampleUsage = false;
        public PropertyShowSampleUsageInDescription()
        {

        }
        public PropertyShowSampleUsageInDescription(bool opt)
        {
            this.showSampleUsage = opt;
        }
    }
    #endregion

    #region for 2ndLabel
    //[AttributeUsage(AttributeTargets.Property)]
    /// <summary>
    /// use SecondalyLabel in the CommandEditor
    /// </summary>
    public sealed class PropertySecondaryLabel : ASingleComamndPropertyAttribute
    {
        public bool useSecondaryLabel = false;

        public PropertySecondaryLabel()
        {

        }
        public PropertySecondaryLabel(bool opt)
        {
            this.useSecondaryLabel = opt;
        }
    }

    //[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    /// <summary>
    /// Additional Parameter Description. This value apper in SecondaryLabel.
    /// </summary>
    public sealed class PropertyAddtionalParameterInfo : AMultiCommandPropertyAttribute
    {
        public string searchKey = "";
        public string description = "";
        public string sampleUsage = "";
        public string remarks = "";
        public PropertyAddtionalParameterInfo(string searchKey, string description, string sampleUsage = "", string remarks = "")
        {
            this.searchKey = searchKey;
            this.description = description;
            this.sampleUsage = sampleUsage;
            this.remarks = remarks;
        }
    }

    //[AttributeUsage(AttributeTargets.Property)]
    public sealed class PropertyAddtionalParameterInfoBehavior : ASingleComamndPropertyAttribute
    {
        public MultiAttributesBehavior behavior = MultiAttributesBehavior.Merge;

        public PropertyAddtionalParameterInfoBehavior()
        {

        }
        public PropertyAddtionalParameterInfoBehavior(MultiAttributesBehavior behavior)
        {
            this.behavior = behavior;
        }
    }
    #endregion

    #region for UIHelper/CustomUIHelper
    //[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    /// <summary>
    /// Show Insert Variable, Select File, Select Folder link
    /// </summary>
    public sealed class PropertyUIHelper : AMultiCommandPropertyAttribute
    {
        public UIAdditionalHelperType additionalHelper;
        public PropertyUIHelper(UIAdditionalHelperType helperType)
        {
            this.additionalHelper = helperType;
        }
        public enum UIAdditionalHelperType
        {
            ShowVariableHelper,
            ShowFileSelectionHelper,
            ShowFolderSelectionHelper,
        }
    }

    //[AttributeUsage(AttributeTargets.Property)]
    /// <summary>
    /// UIHelper behavior for VirtualProperty
    /// </summary>
    public sealed class PropertyUIHelperBehavior : ASingleComamndPropertyAttribute
    {
        public MultiAttributesBehavior behavior = MultiAttributesBehavior.Merge;

        public PropertyUIHelperBehavior()
        {

        }
        public PropertyUIHelperBehavior(MultiAttributesBehavior behavior)
        {
            this.behavior = behavior;
        }
    }

    //[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    /// <summary>
    /// show custom ui helper, open other form
    /// </summary>
    public sealed class PropertyCustomUIHelper : AMultiCommandPropertyAttribute
    {
        public string labelText;
        public string methodName;
        public string nameKey;
        public PropertyCustomUIHelper(string labelText, string methodName, string nameKey = "")
        {
            this.labelText = labelText;
            this.methodName = methodName;
            this.nameKey = nameKey;
        }
    }

    //[AttributeUsage(AttributeTargets.Property)]
    /// <summary>
    /// CustomUIHelper behevior for VirtualProperty
    /// </summary>
    public sealed class PropertyCustomUIHelperBehavior : ASingleComamndPropertyAttribute
    {
        public MultiAttributesBehavior behavior = MultiAttributesBehavior.Merge;

        public PropertyCustomUIHelperBehavior()
        {

        }
        public PropertyCustomUIHelperBehavior(MultiAttributesBehavior behavior)
        {
            this.behavior = behavior;
        }
    }

    //[AttributeUsage(AttributeTargets.Property)]
    /// <summary>
    /// specifiy AvailableSystemVariable behavior for VirtualProperty
    /// </summary>
    public sealed class PropertyAvailableSystemVariableBehavior : ASingleComamndPropertyAttribute
    {
        public MultiAttributesBehavior behavior = MultiAttributesBehavior.Merge;
        
        public PropertyAvailableSystemVariableBehavior()
        {

        }
        public PropertyAvailableSystemVariableBehavior(MultiAttributesBehavior behavior)
        {
            this.behavior = behavior;
        }
    }

    //[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    /// <summary>
    /// specifiy available SystemVariables
    /// </summary>
    public sealed class PropertyAvailableSystemVariable : AMultiCommandPropertyAttribute
    {
        public Engine.SystemVariables.LimitedSystemVariableNames variable = Engine.SystemVariables.LimitedSystemVariableNames.None;
        public PropertyAvailableSystemVariable()
        {

        }
        public PropertyAvailableSystemVariable(Engine.SystemVariables.LimitedSystemVariableNames variable)
        {
            this.variable = variable;
        }
    }
    #endregion

    #region for All Controls
    //[AttributeUsage(AttributeTargets.Property)]
    /// <summary>
    /// specifiy UI control type. TextBox, ComboBox, DataGridView
    /// </summary>
    public sealed class PropertyRecommendedUIControl : ASingleComamndPropertyAttribute
    {
        public RecommendeUIControlType recommendedControl = RecommendeUIControlType.TextBox;
        public PropertyRecommendedUIControl(RecommendeUIControlType ctl)
        {
            this.recommendedControl = ctl;
        }
        public enum RecommendeUIControlType
        {
            TextBox,
            ComboBox,
            DataGridView,
            MultiLineTextBox,
            CheckBox,
            RadioButton,
            TextLink,
            Label
        }
    }

    //[AttributeUsage(AttributeTargets.Property)]
    /// <summary>
    /// instance type. Excel, Word, WebBrowser, List, Dictionary, etc
    /// </summary>
    public sealed class PropertyInstanceType : ASingleComamndPropertyAttribute
    {
        public InstanceType instanceType = InstanceType.none;
        public bool autoWrapVariableMarker = false;

        public PropertyInstanceType()
        {

        }
        public PropertyInstanceType(InstanceType type, bool autoWrapVariableMarker = false)
        {
            this.instanceType = type;
            this.autoWrapVariableMarker = autoWrapVariableMarker;
        }

        public enum InstanceType
        {
            none,
            DataBase,
            Excel,
            IE,
            WebBrowser,
            StopWatch,
            Word,
            NLG,
            Dictionary,
            DataTable,
            JSON,
            List,
            Boolean,
            DateTime,
            UIElement,
            Color,
            MailKitEMail,
            MailKitEMailList,
            WebElement,
            WindowHandle,
            Numeric,
        }
    }

    //[AttributeUsage(AttributeTargets.Property)]
    /// <summary>
    /// parameter description. input, output, etc
    /// </summary>
    public sealed class PropertyParameterDirection : ASingleComamndPropertyAttribute
    {
        public ParameterDirection porpose = ParameterDirection.Unknown;
        public PropertyParameterDirection()
        {

        }
        public PropertyParameterDirection(ParameterDirection porpose)
        {
            this.porpose = porpose;
        }
        public enum ParameterDirection
        {
            /// <summary>
            /// unknown
            /// </summary>
            Unknown,
            /// <summary>
            /// value use taskt engine
            /// </summary>
            Input,
            /// <summary>
            /// for variable name to store result
            /// </summary>
            Output,
            /// <summary>
            /// input and output
            /// </summary>
            Both
        }
    }

    //[AttributeUsage(AttributeTargets.Property)]
    /// <summary>
    /// what? forgotten
    /// </summary>
    public sealed class PropertyControlIntoCommandField : ASingleComamndPropertyAttribute
    {
        public string labelName = "";
        public string bodyName = "";
        public string secondLabelName = "";
        public PropertyControlIntoCommandField(string bodyName = "", string labelName = "", string secondLabelName = "")
        {
            this.labelName = labelName;
            this.bodyName = bodyName;
            this.secondLabelName = secondLabelName;
        }
    }

    //[AttributeUsage(AttributeTargets.Property)]
    /// <summary>
    /// property value valudation rule
    /// </summary>
    public sealed class PropertyValidationRule : ASingleComamndPropertyAttribute
    {
        public string parameterName = "";
        public ValidationRuleFlags errorRule = 0;
        public ValidationRuleFlags warningRule = 0;

        public PropertyValidationRule()
        {

        }
        public PropertyValidationRule(string parameterName, ValidationRuleFlags errorRule = 0, ValidationRuleFlags warningRule = 0)
        {
            this.parameterName = parameterName;
            this.errorRule = errorRule;
            this.warningRule = warningRule;
        }

        public bool IsErrorFlag(ValidationRuleFlags err)
        {
            return ((this.errorRule & err) == err);
        }

        public bool IsWarningFlag(ValidationRuleFlags warn)
        {
            return ((this.warningRule & warn) == warn);
        }

        [Flags]
        public enum ValidationRuleFlags
        {
            None = 0,
            Empty = 1,
            LessThanZero = 2,
            GreaterThanZero = 4,
            EqualsZero = 8,
            NotEqualsZero = 16,
            NotSelectionOption = 32,
            Between = 64,
            NotBetween = 128,
        }
    }

    //[AttributeUsage(AttributeTargets.Property)]
    /// <summary>
    /// property value range. for validation rule
    /// </summary>
    public sealed class PropertyValueRange : ASingleComamndPropertyAttribute
    {
        public double min;
        public double max;
        public PropertyValueRange(double min, double max)
        {
            if (max > min)
            {
                this.min = min;
                this.max = max;
            }
            else
            {
                this.min = max;
                this.max = min;
            }
        }
    }

    //[AttributeUsage(AttributeTargets.Property)]
    /// <summary>
    /// display text in ScriptBuilder form. like "Value: 'abc'"
    /// </summary>
    public sealed class PropertyDisplayText : ASingleComamndPropertyAttribute
    {
        public bool parameterDisplay = false;
        public string parameterName = "";
        public string afterText = "";

        public PropertyDisplayText()
        {
        }
        public PropertyDisplayText(bool show, string name, string afterText = "")
        {
            this.parameterDisplay = show;
            this.parameterName = name;
            this.afterText = afterText;
        }
    }

    //[AttributeUsage(AttributeTargets.Property)]
    /// <summary>
    /// this property value allows empty or not
    /// </summary>
    public sealed class PropertyIsOptional : ASingleComamndPropertyAttribute
    {
        public bool isOptional = false;
        public string setBlankToValue = "";

        public PropertyIsOptional()
        {

        }
        public PropertyIsOptional(bool opt, string setBlankToValue = "")
        {
            this.isOptional = opt;
            this.setBlankToValue = setBlankToValue;
        }
    }
    #endregion

    #region for TextBox/ComboBox
    //[AttributeUsage(AttributeTargets.Property)]
    /// <summary>
    /// property first(initial) value. only for TextBox, ComboBox
    /// </summary>
    public sealed class PropertyFirstValue : ASingleComamndPropertyAttribute
    {
        public string firstValue = "";
        public PropertyFirstValue()
        {

        }
        public PropertyFirstValue(string firstValue)
        {
            this.firstValue = firstValue;
        }
    }
    #endregion

    #region for TextBox
    //[AttributeUsage(AttributeTargets.Property)]
    /// <summary>
    /// TextBox setting
    /// </summary>
    public sealed class PropertyTextBoxSetting : ASingleComamndPropertyAttribute
    {
        public int height = 1;
        public bool allowNewLine = true;
        public PropertyTextBoxSetting()
        {

        }
        public PropertyTextBoxSetting(int height = 1, bool allowNewLine = true)
        {
            if (height < 0)
            {
                this.height = 1;
            }
            else
            {
                this.height = height;
            }
            this.allowNewLine = allowNewLine;
        }
    }

    //[AttributeUsage(AttributeTargets.Property)]
    /// <summary>
    /// file path setting
    /// </summary>
    public sealed class PropertyFilePathSetting : ASingleComamndPropertyAttribute
    {
        /// <summary>
        /// when allowURL is true, path is URL, then supportExtension and supportFileCounter not work
        /// when allowURL is false, path is URL, then an Error Occured
        /// </summary>
        public bool allowURL = false;

        public ExtensionBehavior supportExtension = ExtensionBehavior.AllowNoExtension;
        public FileCounterBehavior supportFileCounter = FileCounterBehavior.NoSupport;
        
        /// <summary>
        /// comma separated like "txt,log"
        /// </summary>
        public string extensions = "";

        public PropertyFilePathSetting()
        {

        }

        public PropertyFilePathSetting(bool allowURL, ExtensionBehavior supportExtension, FileCounterBehavior supportFileCounter, string extensions = "")
        {
            this.supportFileCounter = supportFileCounter;
            this.supportExtension = supportExtension;
            this.allowURL = allowURL;
            this.extensions = extensions;
        }

        public System.Collections.Generic.List<string> GetExtensions()
        {
            var items = extensions.Split(',');

            var exts = new System.Collections.Generic.List<string>();
            foreach(var item in items)
            {
                var i = item.Trim();

                // remove first dot
                if (i.StartsWith("."))
                {
                    i = i.Substring(1);
                }

                // not contains white-space, dot
                if ((i.Length > 0) && (!i.Contains(' ')) && (!i.Contains('\t')) && (!i.Contains('\r')) && (!i.Contains('\n')) && (!i.Contains('.')))
                {
                    exts.Add(i);
                }
            }
            return exts;
        }

        public enum ExtensionBehavior
        {
            /// <summary>
            /// allow no extension, this behavior supports FileCounter
            /// </summary>
            AllowNoExtension,
            /// <summary>
            /// force add specified extension when not exits, this behavior supports FileCounter
            /// </summary>
            RequiredExtension,
            /// <summary>
            /// force add specified extensions when not exists, this behavior NOT supports FileCounter.<br />
            /// when file does not exists, NO Error Occured and return first extension path
            /// </summary>
            RequiredExtensionAndExists
        }

        public enum FileCounterBehavior
        {
            /// <summary>
            /// no support FileCounter
            /// </summary>
            NoSupport,
            /// <summary>
            /// search first non exists FileCounter value<br />
            /// when not found, NO Error Occuerd and return last path
            /// </summary>
            FirstNotExists,
            /// <summary>
            /// search last exists FileCounter value<br />
            /// when not found, NO Error Occuerd and return last path
            /// </summary>
            LastExists
        }
    }
    #endregion

    #region for ComboBox
    //[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    /// <summary>
    /// ComboBox items
    /// </summary>
    public sealed class PropertyUISelectionOption : AMultiCommandPropertyAttribute
    {
        public string uiOption;
        public PropertyUISelectionOption(string description)
        {
            this.uiOption = description;
        }
    }

    //[AttributeUsage(AttributeTargets.Property)]
    /// <summary>
    /// UISelectionOption behavior for VirtualProperty
    /// </summary>
    public sealed class PropertyUISelectionOptionBehavior : ASingleComamndPropertyAttribute
    {
        public MultiAttributesBehavior behavior = MultiAttributesBehavior.Merge;

        public PropertyUISelectionOptionBehavior()
        {

        }
        public PropertyUISelectionOptionBehavior(MultiAttributesBehavior behavior)
        {
            this.behavior = behavior;
        }
    }

    //[AttributeUsage(AttributeTargets.Property)]
    /// <summary>
    /// selection value sensitive, Case Sensitive etc
    /// </summary>
    public sealed class PropertySelectionValueSensitive : ASingleComamndPropertyAttribute
    {
        public bool caseSensitive = false;
        public bool whiteSpaceSensitive = true;
        
        public PropertySelectionValueSensitive()
        {
        }
        public PropertySelectionValueSensitive(bool caseSensitive, bool whiteSpaceSensitive = true)
        {
            this.caseSensitive = caseSensitive;
            this.whiteSpaceSensitive = whiteSpaceSensitive;
        }
    }

    //[AttributeUsage(AttributeTargets.Property)]
    /// <summary>
    /// specify event when ComoBox item changes
    /// </summary>
    public sealed class PropertySelectionChangeEvent : ASingleComamndPropertyAttribute
    {
        public string methodName = "";
        public PropertySelectionChangeEvent()
        {

        }
        public PropertySelectionChangeEvent(string methodName)
        {
            this.methodName = methodName;
        }
    }

    //[AttributeUsage(AttributeTargets.Property)]
    /// <summary>
    /// ComboBox items is Window Names List
    /// </summary>
    public sealed class PropertyIsWindowNamesList : ASingleComamndPropertyAttribute
    {
        public bool isWindowNamesList = false;
        public bool allowCurrentWindow = true;
        public bool allowAllWindows = false;
        public bool allowDesktop = false;

        public PropertyIsWindowNamesList()
        {

        }
        public PropertyIsWindowNamesList(bool isWindowNameList = false, bool allowCurrent = true, bool allowAll = false, bool allowDesktop = false)
        {
            this.isWindowNamesList = isWindowNameList;
            this.allowCurrentWindow = allowCurrent;
            this.allowAllWindows = allowAll;
            this.allowDesktop = allowDesktop;
        }
    }

    //[AttributeUsage(AttributeTargets.Property)]
    /// <summary>
    /// ComboBox item is Variable Name List
    /// </summary>
    public sealed class PropertyIsVariablesList : ASingleComamndPropertyAttribute
    {
        public bool isVariablesList = false;
        public PropertyIsVariablesList()
        {

        }
        public PropertyIsVariablesList(bool opt)
        {
            this.isVariablesList = opt;
        }
    }

    //[AttributeUsage(AttributeTargets.Property)]
    /// <summary>
    /// specify a method to Add items to ComboBox
    /// </summary>
    public sealed class PropertyComboBoxItemMethod : ASingleComamndPropertyAttribute
    {
        /// <summary>
        /// List&gt;string&lt; f()
        /// </summary>
        public string methodName;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="methodName">List&gt;string&lt; f()</param>
        public PropertyComboBoxItemMethod(string methodName)
        {
            this.methodName = methodName;
        }
    }
    #endregion

    #region for DataGridView
    //[AttributeUsage(AttributeTargets.Property)]
    /// <summary>
    /// DataGridView settings
    /// </summary>
    public sealed class PropertyDataGridViewSetting : ASingleComamndPropertyAttribute
    {
        public bool allowAddRow = true;
        public bool allowDeleteRow = true;
        public bool allowResizeRow = true;
        public int width = 400;
        public int height = 250;
        public bool autoGenerateColumns = true;
        public int headerRowHeight = 1;

        public PropertyDataGridViewSetting()
        {

        }
        public PropertyDataGridViewSetting(bool allowAddRow = true, bool allowDeleteRow = true, bool allowResizeRow = true, int width = 400, int height = 250, bool autoGenerateColumns = true, int headerRowHeight = 1)
        {
            this.allowAddRow = allowAddRow;
            this.allowDeleteRow = allowDeleteRow;
            this.allowResizeRow = allowResizeRow;
            this.width = width;
            this.height = height;
            this.autoGenerateColumns = autoGenerateColumns;
            this.headerRowHeight = headerRowHeight;
        }
    }

    //[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    /// <summary>
    /// DataGridView column settings
    /// </summary>
    public sealed class PropertyDataGridViewColumnSettings : AMultiCommandPropertyAttribute
    {
        public string columnName = "";
        public string headerText = "";
        public bool readOnly = false;
        public DataGridViewColumnType type = DataGridViewColumnType.TextBox;
        /// <summary>
        /// separate '\n'
        /// </summary>
        public string comboBoxItems = "";
        public string defaultValue = "";
        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="headerText"></param>
        /// <param name="readOnly"></param>
        /// <param name="type"></param>
        /// <param name="comboBoxItems">separate '\n'</param>
        /// <param name="defaultValue"></param>
        public PropertyDataGridViewColumnSettings(string columnName, string headerText, bool readOnly = false, DataGridViewColumnType type = DataGridViewColumnType.TextBox, string comboBoxItems = "", string defaultValue = null)
        {
            this.columnName = columnName;
            this.headerText = headerText;
            this.readOnly = readOnly;
            this.type = type;
            this.comboBoxItems = comboBoxItems;
            this.defaultValue = defaultValue;
        }
        public enum DataGridViewColumnType
        {
            TextBox,
            ComboBox,
            CheckBox,
            All
        }
    }

    //[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    /// <summary>
    /// specify event when DataGridView Cell is Clicked or BeginEdit
    /// </summary>
    public sealed class PropertyDataGridViewCellEditEvent : AMultiCommandPropertyAttribute
    {
        public string methodName;
        public DataGridViewCellEvent eventRaise;
        public PropertyDataGridViewCellEditEvent(string methodName, DataGridViewCellEvent eventRaise)
        {
            this.methodName = methodName;
            this.eventRaise = eventRaise;
        }
        public enum DataGridViewCellEvent
        {
            CellClick,
            CellBeginEdit
        }
    }

    //[AttributeUsage(AttributeTargets.Property)]
    /// <summary>
    /// specify method to init DataGridView
    /// </summary>
    public sealed class PropertyDataGridViewInitMethod : ASingleComamndPropertyAttribute
    {
        /// <summary>
        /// void f(DataTable)
        /// </summary>
        public string methodName;
        /// <summary>
        /// init
        /// </summary>
        /// <param name="methodName">void f(DataTable)</param>
        public PropertyDataGridViewInitMethod(string methodName)
        {
            this.methodName = methodName;
        }
    }
    #endregion
}