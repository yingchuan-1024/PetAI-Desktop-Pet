using taskt.Core.Automation.Attributes.PropertyAttributes;

namespace taskt.Core.Automation.Commands
{
    public static class ApplicationScriptControls
    {
        /// <summary>
        /// arguments
        /// </summary>
        [PropertyVirtualProperty(nameof(GeneralPropertyControls), nameof(GeneralPropertyControls.v_DisallowNewLine_OneLineTextBox))]
        [PropertyDescription("Arguments")]
        [InputSpecification("Arguments", true)]
        [PropertyDetailSampleUsage("**1**", PropertyDetailSampleUsage.ValueType.Value, "Arguments")]
        [PropertyDetailSampleUsage("**Hello**", PropertyDetailSampleUsage.ValueType.Value, "Arguments")]
        [PropertyDetailSampleUsage("**1 2 3**", PropertyDetailSampleUsage.ValueType.Value, "Arguments")]
        [PropertyDetailSampleUsage("**{{{vArgs}}}**", PropertyDetailSampleUsage.ValueType.VariableValue, "Arguments")]
        [PropertyIsOptional(true)]
        [PropertyValidationRule("Arguments", PropertyValidationRule.ValidationRuleFlags.None)]
        [PropertyDisplayText(false, "Arguments")]
        public static string v_Arguments { get; }

        /// <summary>
        /// variable name to store output
        /// </summary>
        [PropertyVirtualProperty(nameof(GeneralPropertyControls), nameof(GeneralPropertyControls.v_Result))]
        [PropertyDescription("Variable Name to Receive the Output")]
        [PropertyIsOptional(true)]
        [PropertyValidationRule("Result", PropertyValidationRule.ValidationRuleFlags.None)]
        [PropertyDisplayText(true, "Result")]
        public static string v_Result { get; }
    }
}
