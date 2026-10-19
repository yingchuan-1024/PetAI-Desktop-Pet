using taskt.Core.Automation.Attributes.PropertyAttributes;

namespace taskt.Core.Automation.Commands
{
    internal static class ColorControls
    {
        /// <summary>
        /// color variable name
        /// </summary>
        [PropertyVirtualProperty(nameof(GeneralPropertyControls), nameof(GeneralPropertyControls.v_InputInstanceName))]
        [PropertyDescription("Color Variable Name")]
        [InputSpecification("Color Variable Name", true)]
        [PropertyDetailSampleUsage("**vColor**", PropertyDetailSampleUsage.ValueType.VariableValue)]
        [PropertyDetailSampleUsage("**{{{vColor}}}**", PropertyDetailSampleUsage.ValueType.VariableValue)]
        [PropertyInstanceType(PropertyInstanceType.InstanceType.Color, true)]
        //[PropertyShowSampleUsageInDescription(true)]
        //[PropertyRecommendedUIControl(PropertyRecommendedUIControl.RecommendeUIControlType.ComboBox)]
        //[PropertyIsVariablesList(true)]
        //[PropertyParameterDirection(PropertyParameterDirection.ParameterDirection.Input)]
        //[PropertyValidationRule("Variable", PropertyValidationRule.ValidationRuleFlags.Empty)]
        //[PropertyDisplayText(true, "Variable")]
        //[PropertyParameterOrder(5000)]
        public static string v_InputColorVariableName { get; }

        /// <summary>
        /// color value property
        /// </summary>
        [PropertyVirtualProperty(nameof(GeneralPropertyControls), nameof(GeneralPropertyControls.v_DisallowNewLine_OneLineTextBox))]
        [PropertyDescription("Color Value")]
        [InputSpecification("Values range from 0 to 255", true)]
        [PropertyDetailSampleUsage("**0**", "Specify value **0**. **0** is min value of range")]
        [PropertyDetailSampleUsage("**255**", "Specify value **255**. **255** is max value of range")]
        [PropertyValidationRule("Value", PropertyValidationRule.ValidationRuleFlags.Empty | PropertyValidationRule.ValidationRuleFlags.NotBetween)]
        [PropertyValueRange(0, 255)]
        //[Remarks("")]
        //[PropertyShowSampleUsageInDescription(true)]
        //[PropertyTextBoxSetting(1, false)]
        //[PropertyDisplayText(true, "Value")]
        //[PropertyParameterOrder(5000)]
        public static string v_ColorValue { get; }

        ///// <summary>
        ///// Expand user variable as Color. This type is System.Drawing.Color.
        ///// </summary>
        ///// <param name="variableName"></param>
        ///// <param name="engine"></param>
        ///// <returns></returns>
        ///// <exception cref="Exception">Value is not Color</exception>
        //public static System.Drawing.Color ExpandUserVariableAsColor(this string variableName, Engine.AutomationEngineInstance engine)
        //{
        //    var v = variableName.GetRawVariable(engine);
        //    if (v.VariableValue is System.Drawing.Color color)
        //    {
        //        return color;
        //    }
        //    else
        //    {
        //        throw new Exception("Variable " + variableName + " is not Color");
        //    }
        //}

        //public static void StoreInUserVariable(this System.Drawing.Color value, Engine.AutomationEngineInstance engine, string targetVariable)
        //{
        //    ExtensionMethods.StoreInUserVariable(targetVariable, value, engine);
        //}
    }
}
