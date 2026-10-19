using System;
using System.Collections.Generic;
using System.Text;
using taskt.Core.Automation.Attributes.PropertyAttributes;

namespace taskt.Core.Automation.Commands
{
    /// <summary>
    /// for Text commands
    /// </summary>
    internal static class TextControls
    {
        #region Virtual Property
        /// <summary>
        /// text
        /// </summary>
        [PropertyVirtualProperty(nameof(GeneralPropertyControls), nameof(GeneralPropertyControls.v_OneLineTextBox))]
        [PropertyDescription("Text Value")]
        [InputSpecification("Text", true)]
        [PropertyDetailSampleUsage("**1**", PropertyDetailSampleUsage.ValueType.Value, "Text Value")]
        [PropertyDetailSampleUsage("**Hello**", PropertyDetailSampleUsage.ValueType.Value, "Text Value")]
        [PropertyDetailSampleUsage("**{{{vValue}}}**", PropertyDetailSampleUsage.ValueType.VariableValue, "Text Value")]
        [PropertyDisplayText(true, "Text")]
        //[Remarks("")]
        //[PropertyShowSampleUsageInDescription(true)]
        //[PropertyParameterDirection(PropertyParameterDirection.ParameterDirection.Input)]
        //[PropertyParameterOrder(5000)]
        public static string v_Text { get; }

        /// <summary>
        /// text multiline
        /// </summary>
        [PropertyVirtualProperty(nameof(GeneralPropertyControls), nameof(GeneralPropertyControls.v_MultiLinesTextBox))]
        [PropertyDescription("Text Value")]
        [InputSpecification("Text", true)]
        [PropertyDetailSampleUsage("**1**", PropertyDetailSampleUsage.ValueType.Value, "Text Value")]
        [PropertyDetailSampleUsage("**Hello**", PropertyDetailSampleUsage.ValueType.Value, "Text Value")]
        [PropertyDetailSampleUsage("**{{{vValue}}}**", PropertyDetailSampleUsage.ValueType.VariableValue, "Text Value")]
        [PropertyDisplayText(true, "Text")]
        //[Remarks("")]
        //[PropertyShowSampleUsageInDescription(true)]
        //[PropertyParameterDirection(PropertyParameterDirection.ParameterDirection.Input)]
        //[PropertyRecommendedUIControl(PropertyRecommendedUIControl.RecommendeUIControlType.MultiLineTextBox)]
        //[PropertyParameterOrder(5000)]
        public static string v_Text_MultiLine { get; }

        /// <summary>
        /// text variable name
        /// </summary>
        [PropertyVirtualProperty(nameof(GeneralPropertyControls), nameof(GeneralPropertyControls.v_Result))]
        [PropertyDescription("Variable Name to Store Text")]
        //[InputSpecification("Variable Name", true)]
        //[PropertyDetailSampleUsage("**vText**", PropertyDetailSampleUsage.ValueType.VariableName)]
        //[PropertyDetailSampleUsage("**{{{vText}}}**", PropertyDetailSampleUsage.ValueType.VariableName)]
        //[PropertyShowSampleUsageInDescription(true)]
        //[PropertyRecommendedUIControl(PropertyRecommendedUIControl.RecommendeUIControlType.ComboBox)]
        //[PropertyIsVariablesList(true)]
        //[PropertyParameterDirection(PropertyParameterDirection.ParameterDirection.Output)]
        //[Remarks("")]
        //[PropertyValidationRule("Variable", PropertyValidationRule.ValidationRuleFlags.Empty)]
        //[PropertyDisplayText(true, "Variable")]
        //[PropertyParameterOrder(5000)]
        public static string v_OutputTextVariableName { get; set; }

        ///// <summary>
        ///// for text file path
        ///// </summary>
        //[PropertyDescription("Path to the File")]
        //[InputSpecification("Path of the File", true)]
        //[Remarks("If file does not contain extensin, supplement txt automatically.\nIf file does not contain folder path, file will be opened in the same folder as script file.")]
        //[PropertyDetailSampleUsage("**C:\\temp\\myfile.txt**", PropertyDetailSampleUsage.ValueType.Value, "Path")]
        //[PropertyDetailSampleUsage("**{{{vFilePath}}}**", PropertyDetailSampleUsage.ValueType.VariableValue, "Path")]
        //[PropertyDetailSampleUsage("**http://exmample.com/mytext.txt**", PropertyDetailSampleUsage.ValueType.Value, "Path")]
        //[PropertyDetailSampleUsage("**{{{vURL}}}**", PropertyDetailSampleUsage.ValueType.VariableValue, "Path")]
        //[PropertyShowSampleUsageInDescription(true)]
        //[PropertyUIHelper(PropertyUIHelper.UIAdditionalHelperType.ShowFileSelectionHelper)]
        //[PropertyTextBoxSetting(1, false)]
        //[PropertyValidationRule("File Path", PropertyValidationRule.ValidationRuleFlags.Empty)]
        //[PropertyDisplayText(true, "Path")]
        //[PropertyParameterOrder(5000)]
        //public static string v_FilePath { get; }
        #endregion

        public static string ConvertToBase64(this string text)
        {
            return ConvertToBase64(text, Encoding.UTF8);
        }

        public static string ConvertToBase64(this string text, Encoding encoding)
        {
            byte[] textAsBytes = encoding.GetBytes(text);
            return Convert.ToBase64String(textAsBytes);
        }

        public static bool TryParseBase64(this string text, out string decodedText)
        {
            return TryParseBase64(text, Encoding.UTF8, out decodedText);
        }

        public static bool TryParseBase64(this string text, Encoding encoding, out string decodedText)
        {
            if (string.IsNullOrEmpty(text))
            {
                decodedText = text;
                return false;
            }

            try
            {
                byte[] textAsBytes = Convert.FromBase64String(text);
                decodedText = encoding.GetString(textAsBytes);
                return true;
            }
            catch (Exception)
            {
                decodedText = null;
                return false;
            }
        }

        /// <summary>
        /// Search Text Positions
        /// </summary>
        /// <param name="targetText"></param>
        /// <param name="searchText"></param>
        /// <param name="greedly"></param>
        /// <returns></returns>
        public static List<int> SearchTextPositions(string targetText, string searchText, bool greedly)
        {
            int posBias = (greedly) ? 1 : searchText.Length;

            var positions = new List<int>();
            var pos = targetText.IndexOf(searchText, 0);
            while (pos >= 0)
            {
                positions.Add(pos);
                pos = targetText.IndexOf(searchText, pos + posBias);
            }
            return positions;
        }
    }
}
