using System;
using System.Windows.Forms;
using taskt.Core.Automation.Attributes.PropertyAttributes;
using taskt.UI.CustomControls;

namespace taskt.Core.Automation.Commands
{
    /// <summary>
    /// methods for DataTime
    /// </summary>
    internal static class DateTimeControls
    {
        /// <summary>
        /// input DateTime Variable property
        /// </summary>
        [PropertyVirtualProperty(nameof(GeneralPropertyControls), nameof(GeneralPropertyControls.v_InputInstanceName))]
        [PropertyDescription("DateTime Variable Name")]
        [InputSpecification("DateTime Variable", true)]
        [PropertyDetailSampleUsage("**{{{vDateTime}}}**", PropertyDetailSampleUsage.ValueType.VariableValue)]
        [PropertyValidationRule("DateTime Variable", PropertyValidationRule.ValidationRuleFlags.Empty)]
        [PropertyInstanceType(PropertyInstanceType.InstanceType.DateTime, true)]
        [PropertyShowSampleUsageInDescription(true)]
        //[PropertyRecommendedUIControl(PropertyRecommendedUIControl.RecommendeUIControlType.ComboBox)]
        //[PropertyParameterDirection(PropertyParameterDirection.ParameterDirection.Input)]
        //[PropertyDisplayText(true, "Variable")]
        //[Remarks("")]
        //[PropertyParameterOrder(5000)]
        public static string v_InputDateTime { get; }

        /// <summary>
        /// output DateTime Variable property
        /// </summary>
        [PropertyVirtualProperty(nameof(GeneralPropertyControls), nameof(GeneralPropertyControls.v_Result))]
        [PropertyDescription("Variable Name to Store DateTime")]
        [PropertyDetailSampleUsage("**vDateTime**", PropertyDetailSampleUsage.ValueType.VariableName)]
        [PropertyDetailSampleUsage("**{{{vDateTime}}}**", PropertyDetailSampleUsage.ValueType.VariableName)]
        [PropertyInstanceType(PropertyInstanceType.InstanceType.DateTime, true)]
        //[InputSpecification("")]
        //[Remarks("")]
        //[PropertyShowSampleUsageInDescription(true)]
        //[PropertyRecommendedUIControl(PropertyRecommendedUIControl.RecommendeUIControlType.ComboBox)]
        //[PropertyIsVariablesList(true)]
        //[PropertyParameterDirection(PropertyParameterDirection.ParameterDirection.Output)]
        //[PropertyValidationRule("DateTime Variable", PropertyValidationRule.ValidationRuleFlags.Empty)]
        //[PropertyDisplayText(true, "Variable")]
        //[PropertyParameterOrder(5000)]
        public static string v_OutputDateTime { get; }

        /// <summary>
        /// DateTime Text
        /// </summary>
        [PropertyVirtualProperty(nameof(GeneralPropertyControls), nameof(GeneralPropertyControls.v_DisallowNewLine_OneLineTextBox))]
        [PropertyDescription("DateTime Text")]
        [InputSpecification("DateTime Text", true)]
        [PropertyDetailSampleUsage("**2000-01-01**", PropertyDetailSampleUsage.ValueType.Value, "DateTime")]
        [PropertyDetailSampleUsage("**{{{vText}}**", PropertyDetailSampleUsage.ValueType.VariableValue, "DateTime")]
        [PropertyValidationRule("DateTime Text", PropertyValidationRule.ValidationRuleFlags.Empty)]
        [PropertyDisplayText(true, "DateTime Text")]
        [Remarks("Recommended to Disable the Auto Calculation")]
        //[PropertyUIHelper(PropertyUIHelper.UIAdditionalHelperType.ShowVariableHelper)]
        //[PropertyShowSampleUsageInDescription(true)]
        //[PropertyTextBoxSetting(1, false)]
        //[PropertyParameterDirection(PropertyParameterDirection.ParameterDirection.Input)]
        //[PropertyParameterOrder(5000)]
        public static string v_DateTimeText { get; }

        /// <summary>
        /// Excel Serial DateTime
        /// </summary>
        [PropertyVirtualProperty(nameof(GeneralPropertyControls), nameof(GeneralPropertyControls.v_DisallowNewLine_OneLineTextBox))]
        [PropertyDescription("Excel Serial DateTime")]
        [InputSpecification("Excel Serial DateTime", true)]
        [PropertyDetailSampleUsage("**43210**", "Specify **43210** for Excel Serial. It's means 2018-04-20.")]
        [PropertyDetailSampleUsage("**{{{vSerial}}}**", PropertyDetailSampleUsage.ValueType.VariableValue, "Excel Serial")]
        [PropertyValidationRule("Excel Serial", PropertyValidationRule.ValidationRuleFlags.Empty)]
        [PropertyDisplayText(true, "Excel Serial")]
        //[PropertyUIHelper(PropertyUIHelper.UIAdditionalHelperType.ShowVariableHelper)]
        //[Remarks("")]
        //[PropertyShowSampleUsageInDescription(true)]
        //[PropertyTextBoxSetting(1, false)]
        //[PropertyParameterDirection(PropertyParameterDirection.ParameterDirection.Input)]
        //[PropertyParameterOrder(5000)]
        public static string v_ExcelSerial { get; }

        /// <summary>
        /// Unix Time
        /// </summary>
        [PropertyVirtualProperty(nameof(GeneralPropertyControls), nameof(GeneralPropertyControls.v_DisallowNewLine_OneLineTextBox))]
        [PropertyDescription("Unix Time")]
        [InputSpecification("Unix Time", true)]
        [PropertyDetailSampleUsage("**1577836800**", "Specify **1577836800** for Unix Time. It's means 2020-01-01.")]
        [PropertyDetailSampleUsage("**{{{vTime}}}**", PropertyDetailSampleUsage.ValueType.VariableValue, "Unix Time")]
        [PropertyValidationRule("Unix Time", PropertyValidationRule.ValidationRuleFlags.Empty)]
        [PropertyDisplayText(true, "Unix Time")]
        //[PropertyUIHelper(PropertyUIHelper.UIAdditionalHelperType.ShowVariableHelper)]
        //[Remarks("")]
        //[PropertyShowSampleUsageInDescription(true)]
        //[PropertyTextBoxSetting(1, false)]
        //[PropertyParameterDirection(PropertyParameterDirection.ParameterDirection.Input)]
        //[PropertyParameterOrder(5000)]
        public static string v_UnixTime { get; }

        /// <summary>
        /// datetime format
        /// </summary>
        [PropertyVirtualProperty(nameof(GeneralPropertyControls), nameof(GeneralPropertyControls.v_DisallowNewLine_OneLineTextBox))]
        [PropertyDescription("DateTime Format")]
        [PropertyUIHelper(PropertyUIHelper.UIAdditionalHelperType.ShowVariableHelper)]
        [PropertyCustomUIHelper("Format Checker", nameof(DateTimeControls)+"+"+nameof(lnkFormatChecker_Click))]
        [InputSpecification("DateTime Format Text", true)]
        [PropertyDetailSampleUsage("**MM/dd/yyyy**", "Specify Format Month/Day/Year")]
        [PropertyDetailSampleUsage("**HH:mm:ss**", "Specify Format Hour/Minute/Second")]
        [PropertyDetailSampleUsage("{{{vFormat}}}", PropertyDetailSampleUsage.ValueType.VariableValue, "Format")]
        [Remarks("Please refer to the Microsoft DateTime.ToString() page for format details")]
        [PropertyValidationRule("Format", PropertyValidationRule.ValidationRuleFlags.Empty)]
        [PropertyDisplayText(true, "Format")]
        public static string v_Format { get; }

        ///// <summary>
        ///// Expand user variable As DateTime
        ///// </summary>
        ///// <param name="variableName"></param>
        ///// <param name="engine"></param>
        ///// <returns></returns>
        ///// <exception cref="Exception">Value is not DateTime</exception>
        //public static DateTime ExpandUserVariableAsDateTime(this string variableName, Core.Automation.Engine.AutomationEngineInstance engine)
        //{
        //    Script.ScriptVariable v = variableName.GetRawVariable(engine);
        //    if (v.VariableValue is DateTime time)
        //    {
        //        return time;
        //    }
        //    else
        //    {
        //        throw new Exception("Variable " + variableName + " is not DateTime");
        //    }
        //}

        //public static void StoreInUserVariable(this DateTime value, Core.Automation.Engine.AutomationEngineInstance engine, string targetVariable)
        //{
        //    ExtensionMethods.StoreInUserVariable(targetVariable, value, engine, false);
        //}

        /// <summary>
        /// Convert value to DateTime
        /// </summary>
        /// <param name="str"></param>
        /// <param name="parameterName"></param>
        /// <param name="sender"></param>
        /// <returns></returns>
        /// <exception cref="Exception">fail convert value to DateTime</exception>
        public static DateTime ConvertValueToDateTime(this string str, string parameterName, Core.Automation.Engine.AutomationEngineInstance engine)
        { 
            string convertedText = str.ExpandValueOrUserVariable(engine);
            if (DateTime.TryParse(convertedText, out DateTime v))
            {
                return v;
            }
            else
            {
                throw new Exception(parameterName + " '" + str + "' is not a DateTime.");
            }
        }

        /// <summary>
        /// show FormatChecker form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void lnkFormatChecker_Click(object sender, EventArgs e)
        {
            TextBox txt = (TextBox)((CommandItemControl)sender).Tag;
            UI.Forms.ScriptBuilder.CommandEditor.Supplemental.frmFormatChecker.ShowFormatCheckerFormLinkClicked(txt, "DateTime");
        }
    }
}
