//using System;
//using System.Windows.Forms;
//using System.Xml.Serialization;
//using taskt.Core.Automation.Attributes.PropertyAttributes;

//namespace taskt.Core.Automation.Commands
//{
//    [Serializable]
//    [Attributes.ClassAttributes.Group("Numerical")]
//    [Attributes.ClassAttributes.CommandSettings("Compare Number")]
//    [Attributes.ClassAttributes.Description("This command allows you to Compare Number.")]
//    [Attributes.ClassAttributes.UsesDescription("Use this command when you want to Compare Number.")]
//    [Attributes.ClassAttributes.ImplementationDescription("")]
//    [Attributes.ClassAttributes.CommandIcon(nameof(Properties.Resources.command_function))]
//    [Attributes.ClassAttributes.EnableAutomateRender(true)]
//    [Attributes.ClassAttributes.EnableAutomateDisplayText(true)]
//    public sealed class CompareNumberCommand : ScriptCommand
//    {
//        [XmlAttribute]
//        [PropertyVirtualProperty(nameof(NumberControls), nameof(NumberControls.v_Value))]
//        public string v_TargetValue { get; set; }

//        [XmlAttribute]
//        [PropertyVirtualProperty(nameof(NumberControls), nameof(NumberControls.v_CompareMethod))]
//        [PropertySelectionChangeEvent(nameof(cmbCompareMethod_SelectionChangeCommitted))]
//        public string v_CompareMethod { get; set; }

//        [XmlAttribute]
//        [PropertyVirtualProperty(nameof(NumberControls), nameof(NumberControls.v_Value))]
//        [PropertyDescription("Numerical Value to be Compared")]
//        [PropertyDisplayText(true, "Compared")]

//        public string v_CompareValue1 { get; set; }

//        [XmlAttribute]
//        [PropertyVirtualProperty(nameof(NumberControls), nameof(NumberControls.v_Value))]
//        [PropertyDescription("Numerical Value to be Compared 2")]
//        [PropertyIsOptional(true, "")]
//        [PropertyValidationRule("Compared Value 2", PropertyValidationRule.ValidationRuleFlags.None)]
//        [PropertyDisplayText(true, "Compared")]
//        public string v_CompareValue2 { get; set; }

//        [XmlAttribute]
//        [PropertyVirtualProperty(nameof(NumberControls), nameof(NumberControls.v_OutputNumericalVariableName))]
//        public string v_Result { get; set; }

//        public CompareNumberCommand()
//        {
//        }

//        public override void RunCommand(Engine.AutomationEngineInstance engine)
//        {
//            var a = this.ExpandValueOrUserVariableAsDecimal(nameof(v_TargetValue), engine);

//            var method = this.ExpandValueOrUserVariableAsSelectionItem(nameof(v_CompareMethod), engine);

//            decimal b, c;
//            switch(method)
//            {
//                case "is between":
//                case "is not between":
//                    b = this.ExpandValueOrUserVariableAsDecimal(nameof(v_CompareValue1), engine);
//                    if (!string.IsNullOrEmpty(v_CompareValue2))
//                    {
//                        c = this.ExpandValueOrUserVariableAsDecimal(nameof(v_CompareValue2), engine);
//                    }
//                    else
//                    {
//                        throw new Exception("Compare Value 2 is not Specified.");
//                    }

//                    if (b > c)
//                    {
//                        (b, c) = (c, b);    // swap
//                    }
//                    break;

//                default:
//                    b = this.ExpandValueOrUserVariableAsDecimal(nameof(v_CompareValue1), engine);
//                    c = 0;
//                    break;
//            }

//            bool res = false;
//            switch (method)
//            {
//                case "is equal to":
//                    res = (a == b);
//                    break;
//                case "is not equal to":
//                    res = (a != b);
//                    break;
//                case "is greater than":
//                    res = (a > b);
//                    break;
//                case "is greater than or equal to":
//                    res = (a >= b);
//                    break;
//                case "is less than":
//                    res = (a < b);
//                    break;
//                case "is less than or equal to":
//                    res = (a <= b);
//                    break;
//                case "is between":
//                    res = (a >= b) && (a <= c);
//                    break;
//                case "is not between":
//                    res = (a < b) || (a > c);
//                    break;
//            }

//            res.StoreInUserVariable(engine, v_Result);
//        }

//        private void cmbCompareMethod_SelectionChangeCommitted(object sender, EventArgs e)
//        {
//            var cmb = (ComboBox)sender;
//            bool visible = false;
//            switch (cmb.SelectedItem.ToString().ToLower())
//            {
//                case "is between":
//                case "is not between":
//                    visible = true;
//                    break;
//            }
//            FormUIControls.SetVisibleParameterControlGroup(this.ControlsList, nameof(v_CompareValue2), visible);
//        }
//    }
//}
