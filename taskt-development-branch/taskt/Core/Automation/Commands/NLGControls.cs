using taskt.Core.Automation.Attributes.PropertyAttributes;

namespace taskt.Core.Automation.Commands
{
    internal static class NLGControls
    {
        /// <summary>
        /// NLG Instance Name
        /// </summary>
        [PropertyVirtualProperty(nameof(GeneralPropertyControls), nameof(GeneralPropertyControls.v_InputInstanceName))]
        [PropertyDescription("NLG Instance Name")]
        [InputSpecification("NLG Instance Name", true)]
        [PropertyDetailSampleUsage("**nlgInstance**", PropertyDetailSampleUsage.ValueType.Value, "Instance")]
        [PropertyDetailSampleUsage("**{{{vInstance}}}**", PropertyDetailSampleUsage.ValueType.VariableValue, "Instance")]
        [PropertyValidationRule("NLG Instance", PropertyValidationRule.ValidationRuleFlags.Empty)]
        [PropertyDisplayText(true, "Instance")]
        [PropertyFirstValue("%kwd_default_nlg_instance%")]
        [Remarks("Failure to enter the correct instance name or failure to first call **Create NLG Instance** command will cause an error")]
        [PropertyInstanceType(PropertyInstanceType.InstanceType.NLG)]
        //[PropertyShowSampleUsageInDescription(true)]
        //[PropertyParameterDirection(PropertyParameterDirection.ParameterDirection.Input)]
        //[PropertyRecommendedUIControl(PropertyRecommendedUIControl.RecommendeUIControlType.ComboBox)]
        //[PropertyParameterOrder(5000)]
        public static string v_InstanceName { get; }
    }
}
