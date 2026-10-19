using taskt.Core.Automation.Attributes.PropertyAttributes;

namespace taskt.Core.Automation.Commands
{
    /// <summary>
    /// for folder path methods
    /// </summary>
    internal static class FolderPathControls
    {
        #region Virtual Property
        /// <summary>
        /// folder path
        /// </summary>
        [PropertyVirtualProperty(nameof(GeneralPropertyControls), nameof(GeneralPropertyControls.v_DisallowNewLine_OneLineTextBox))]
        [PropertyDescription("Folder Path")]
        [InputSpecification("Folder Path", true)]
        [PropertyDetailSampleUsage("**C:\\temp**", PropertyDetailSampleUsage.ValueType.Value, "Folder Path")]
        [PropertyDetailSampleUsage("**{{{vFilePath}}}**", PropertyDetailSampleUsage.ValueType.VariableValue, "Folder Path")]
        [PropertyUIHelper(PropertyUIHelper.UIAdditionalHelperType.ShowFolderSelectionHelper)]
        [PropertyParameterDirection(PropertyParameterDirection.ParameterDirection.Input)]
        [PropertyValidationRule("Folder Path", PropertyValidationRule.ValidationRuleFlags.Empty)]
        [PropertyDisplayText(true, "Folder")]
        //[PropertyShowSampleUsageInDescription(true)]
        //[Remarks("")]
        //[PropertyUIHelper(PropertyUIHelper.UIAdditionalHelperType.ShowVariableHelper)]
        //[PropertyTextBoxSetting(1, false)]
        //[PropertyParameterOrder(5000)]
        public static string v_FolderPath { get; }

        /// <summary>
        /// for wait time
        /// </summary>
        [PropertyVirtualProperty(nameof(WaitControls), nameof(WaitControls.v_WaitTime))]
        [PropertyDescription("Wait Time for the Folder to Exist (sec)")]
        [Remarks("Specify how long to Wait before an Error will occur because the Folder is not Found.")]
        [PropertyIsOptional(true, "10")]
        [PropertyFirstValue("10")]
        //[InputSpecification("Wait Time", true)]
        //[PropertyDetailSampleUsage("**10**", PropertyDetailSampleUsage.ValueType.Value, "Wait Time")]
        //[PropertyDetailSampleUsage("**{{{vWaitTime}}}**", PropertyDetailSampleUsage.ValueType.VariableValue, "Wait Time")]
        //[PropertyShowSampleUsageInDescription(true)]
        //[PropertyTextBoxSetting(1, false)]
        //[PropertyParameterDirection(PropertyParameterDirection.ParameterDirection.Input)]
        //[PropertyDisplayText(true, "Wait", "s")]
        //[PropertyParameterOrder(5000)]
        public static string v_WaitTime { get; }

        /// <summary>
        /// folder path result
        /// </summary>
        [PropertyVirtualProperty(nameof(GeneralPropertyControls), nameof(GeneralPropertyControls.v_Result))]
        [PropertyDescription("Variable Name to Store Folder Path")]
        [InputSpecification("Variable Name", true)]
        [PropertyDetailSampleUsage("**vPath**", PropertyDetailSampleUsage.ValueType.VariableName)]
        [PropertyDetailSampleUsage("**{{{vPath}}}**", PropertyDetailSampleUsage.ValueType.VariableName)]
        [PropertyValidationRule("Folder Path Result", PropertyValidationRule.ValidationRuleFlags.None)]
        [PropertyIsOptional(true)]
        [PropertyDisplayText(false, "Variable Name to Store Folder Path")]
        //[Remarks("")]
        //[PropertyShowSampleUsageInDescription(true)]
        //[PropertyIsVariablesList(true)]
        //[PropertyRecommendedUIControl(PropertyRecommendedUIControl.RecommendeUIControlType.ComboBox)]
        //[PropertyParameterDirection(PropertyParameterDirection.ParameterDirection.Output)]
        //[PropertyParameterOrder(5000)]
        public static string v_FolderPathResult { get; }
        #endregion

        ///// <summary>
        ///// Wait For Folder
        ///// </summary>
        ///// <param name="path"></param>
        ///// <param name="waitTime"></param>
        ///// <param name="engine"></param>
        ///// <returns>file path</returns>
        ///// <exception cref="Exception"></exception>
        //public static string WaitForFolder(string path, int waitTime, Engine.AutomationEngineInstance engine)
        //{
        //    var ret = WaitControls.WaitProcess(waitTime, "Folder Path", new Func<(bool, object)>(() =>
        //    {
        //        if (Directory.Exists(path))
        //        {
        //            return (true, path);
        //        }
        //        else
        //        {
        //            return (false, null);
        //        }
        //    }), engine);

        //    if (ret is string returnPath)
        //    {
        //        return returnPath;
        //    }
        //    else
        //    {
        //        throw new Exception("Strange Value returned in WaitForFile. Type: " + ret.GetType().FullName);
        //    }
        //}

        ///// <summary>
        ///// wait for folder
        ///// </summary>
        ///// <param name="pathValue">NOT use PropertyFilePathSetting</param>
        ///// <param name="waitTimeValue"></param>
        ///// <param name="engine"></param>
        ///// <returns></returns>
        //public static string WaitForFolder(string pathValue, string waitTimeValue, Engine.AutomationEngineInstance engine)
        //{
        //    var path = pathValue.ExpandValueOrUserVariableAsFolderPath(engine);
        //    var waitTime = waitTimeValue.ExpandValueOrUserVariableAsInteger("Wait Time", engine);
        //    return WaitForFolder(path, waitTime, engine);
        //}

        ///// <summary>
        ///// wait for folder
        ///// </summary>
        ///// <param name="command"></param>
        ///// <param name="pathName">use PropertyFilePathSetting</param>
        ///// <param name="waitTimeName"></param>
        ///// <param name="engine"></param>
        ///// <returns></returns>
        //public static string WaitForFolder(ScriptCommand command, string pathName, string waitTimeName, Engine.AutomationEngineInstance engine)
        //{
        //    var path = command.ExpandValueOrUserVariableAsFolderPath(pathName, engine);
        //    var waitTime = command.ExpandValueOrUserVariableAsInteger(waitTimeName, "Wait Time", engine);
        //    return WaitForFolder(path, waitTime, engine);
        //}

        ///// <summary>
        ///// general folder action. This method search target folder before execute actionFunc, and try store Found Folder Path after execute actionFunc. 
        ///// </summary>
        ///// <param name="command"></param>
        ///// <param name="pathName"></param>
        ///// <param name="waitTimeName"></param>
        ///// <param name="engine"></param>
        ///// <param name="actionFunc"></param>
        ///// <param name="pathResultName"></param>
        ///// <param name="errorFunc"></param>
        //public static void FolderAction(ScriptCommand command, string pathName, string waitTimeName, Engine.AutomationEngineInstance engine, Action<string> actionFunc, string pathResultName = "", Action<Exception> errorFunc = null)
        //{
        //    try
        //    {
        //        var path = WaitForFolder(command, pathName, waitTimeName, engine);
        //        actionFunc(path);

        //        if (!string.IsNullOrEmpty(pathResultName))
        //        {
        //            var pathResult = command.GetRawPropertyValueAsString(pathResultName, "Folder Path Result");
        //            if (!string.IsNullOrEmpty(pathResult))
        //            {
        //                path.StoreInUserVariable(engine, pathResult);
        //            }
        //        }
        //    }
        //    catch(Exception ex)
        //    {
        //        if (errorFunc != null)
        //        {
        //            errorFunc(ex);
        //        }
        //        else
        //        {
        //            throw ex;
        //        }
        //    }
        //}

        ///// <summary>
        ///// general folder action. This method search target folder before execute actionFunc, and try store Found Folder Path after execute actionFunc. This method specifies the parameter from the value of PropertyVirtualProperty.
        ///// </summary>
        ///// <param name="command"></param>
        ///// <param name="engine"></param>
        ///// <param name="actionFunc"></param>
        ///// <param name="errorFunc"></param>
        //public static void FolderAction(ScriptCommand command, Engine.AutomationEngineInstance engine, Action<string> actionFunc,  Action<Exception> errorFunc = null)
        //{
        //    var props = command.GetParameterProperties();

        //    var folderPath = props.GetProperty(new PropertyVirtualProperty(nameof(FolderPathControls), nameof(v_FolderPath)))?.Name ?? "";
        //    var waitTime = props.GetProperty(new PropertyVirtualProperty(nameof(FolderPathControls), nameof(v_WaitTime)))?.Name ?? "";
        //    var folderResult = props.GetProperty(new PropertyVirtualProperty(nameof(FolderPathControls), nameof(v_FolderPathResult)))?.Name ?? "";

        //    FolderAction(command, folderPath, waitTime, engine, actionFunc, folderResult, errorFunc);
        //}

        ///// <summary>
        ///// Convert to FullPath specified path
        ///// </summary>
        ///// <param name="path"></param>
        ///// <param name="engine"></param>
        ///// <returns></returns>
        //private static string ConvertToFullPath(string path, Engine.AutomationEngineInstance engine)
        //{
        //    if (EM_CanHandleFileOrFolderPathExtensionMethods.IsFullPath(path))
        //    {
        //        return path;
        //    }
        //    else
        //    {
        //        return Path.Combine(Path.GetDirectoryName(engine.FileName), path);
        //    }
        //}

        ///// <summary>
        ///// expand value or User variable as Folder Path
        ///// </summary>
        ///// <param name="value"></param>
        ///// <param name="engine"></param>
        ///// <returns></returns>
        ///// <exception cref="">value is not Folder Path</exception>
        //public static string ExpandValueOrUserVariableAsFolderPath(this string value, Engine.AutomationEngineInstance engine)
        //{
        //    var p = ConvertToFullPath(value.ExpandValueOrUserVariable(engine), engine);
        //    var invs = Path.GetInvalidPathChars();
        //    if (p.IndexOfAny(invs) < 0)
        //    {
        //        return p;
        //    }
        //    else
        //    {
        //        throw new Exception("Folder Path contains Invalid chars. Path: '" + p + "'");
        //    }
        //}

        ///// <summary>
        ///// Expand value or user variable as Folder Path
        ///// </summary>
        ///// <param name="command"></param>
        ///// <param name="parameterValue"></param>
        ///// <param name="engine"></param>
        ///// <returns></returns>
        //public static string ExpandValueOrUserVariableAsFolderPath(this ScriptCommand command, string parameterValue, Engine.AutomationEngineInstance engine)
        //{
        //    return command.ExpandValueOrUserVariable(parameterValue, "Folder Path", engine).ExpandValueOrUserVariableAsFolderPath(engine);
        //}

        ///// <summary>
        ///// expand value or User variable as Folder Name
        ///// </summary>
        ///// <param name="folderName"></param>
        ///// <param name="engine"></param>
        ///// <returns></returns>
        ///// <exception cref="Exception">value is not folder name</exception>
        //public static string ExpandValueOrUserVariableAsFolderName(this string folderName, Engine.AutomationEngineInstance engine)
        //{
        //    var fn = folderName.ExpandValueOrUserVariable(engine);
        //    var invs = Path.GetInvalidFileNameChars();
        //    if (fn.IndexOfAny(invs) < 0)
        //    {
        //        return fn;
        //    }
        //    else
        //    {
        //        throw new Exception("Folder Name contains invalid chars. Folder: '" + fn + "'");
        //    }
        //}
    }
}
