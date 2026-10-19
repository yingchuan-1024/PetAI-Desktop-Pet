using Microsoft.CodeDom.Providers.DotNetCompilerPlatform;
using System;
using System.CodeDom.Compiler;
using System.IO;
using taskt.Core.Automation.Attributes.PropertyAttributes;

namespace taskt.Core.Automation.Commands
{
    public static class CSharpCodeCompilerControls
    {
        #region const
        private const string defaultOutFileName = "tasktOnTheFly.exe";

        /// <summary>
        /// read here
        /// https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/configure-language-version?WT.mc_id=AI-MVP-123445
        /// </summary>
        private const string defaultLangVersion = "default";
        #endregion

        #region virtual property

        /// <summary>
        /// compiled executable file name
        /// </summary>
        [PropertyVirtualProperty(nameof(GeneralPropertyControls), nameof(GeneralPropertyControls.v_DisallowNewLine_OneLineTextBox))]
        [PropertyDescription("Compiled Executable File Name")]
        [PropertyIsOptional(true, "tasktOnTheFly")]
        [PropertyFirstValue("tasktOnTheFly")]
        [PropertyValidationRule("File Name", PropertyValidationRule.ValidationRuleFlags.None)]
        [PropertyDisplayText(false, "File Name")]
        public static string v_ExecutableFileName { get;}

        /// <summary>
        /// variable name to store compiled executable file path
        /// </summary>
        [PropertyVirtualProperty(nameof(GeneralPropertyControls), nameof(GeneralPropertyControls.v_Result))]
        [PropertyDescription("Variable Name to Store Executable File Path")]
        [PropertyIsOptional(true)]
        [PropertyValidationRule("Executable File Path", PropertyValidationRule.ValidationRuleFlags.None)]
        [PropertyDisplayText(true, "Executable File Path")]
        public static string v_ExecutableFilePath { get; }

        /// <summary>
        /// C# language version
        /// </summary>
        [PropertyVirtualProperty(nameof(GeneralPropertyControls), nameof(GeneralPropertyControls.v_ComboBox))]
        [PropertyDescription("C# Language Version")]
        [PropertyUISelectionOption("default")]
        [PropertyUISelectionOption("latest")]
        [PropertyUISelectionOption("preview")]
        [PropertyUISelectionOption("14.0")]
        [PropertyUISelectionOption("13.0")]
        [PropertyUISelectionOption("12.0")]
        [PropertyUISelectionOption("11.0")]
        [PropertyUISelectionOption("10.0")]
        [PropertyUISelectionOption("9.0")]
        [PropertyUISelectionOption("8.0")]
        [PropertyUISelectionOption("7.3")]
        [PropertyUISelectionOption("7.2")]
        [PropertyUISelectionOption("7.1")]
        [PropertyUISelectionOption("7")]
        [PropertyUISelectionOption("6")]
        [PropertyUISelectionOption("5")]
        [PropertyUISelectionOption("4")]
        [PropertyUISelectionOption("3")]
        [PropertyUISelectionOption("2")]
        [PropertyUISelectionOption("1")]
        [PropertyIsOptional(true, "default")]
        [PropertyFirstValue("default")]
        [PropertyValidationRule("C# Language Version", PropertyValidationRule.ValidationRuleFlags.None)]
        [PropertyDisplayText(false, "C# Language Version")]
        [Remarks("More Information: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/configure-language-version?WT.mc_id=AI-MVP-123445")]
        public static string v_CSharpLanguageVersion { get; }

        /// <summary>
        /// delete executable file after execute
        /// </summary>
        [PropertyVirtualProperty(nameof(SelectionItemsControls), nameof(SelectionItemsControls.v_YesNoComboBox))]
        [PropertyDescription("Delete Executable File After Execute")]
        [PropertyIsOptional(true, "Yes")]
        [PropertyFirstValue("Yes")]
        [PropertyValidationRule("Delete Executable File", PropertyValidationRule.ValidationRuleFlags.None)]
        [PropertyDisplayText(false, "Delete Executable File")]
        public static string v_DeleteExecutableFile { get; }

        #endregion

        /// <summary>
        /// compile csharp code, specified code text
        /// </summary>
        /// <param name="codeInput"></param>
        /// <param name="langVersion"></param>
        /// <param name="outFileName"></param>
        /// <returns></returns>
        public static CompilerResults CompileCSCode(string codeInput, string langVersion = "", string outFileName = "")
        {
            //if (string.IsNullOrEmpty(langVersion)) 
            //{ 
            //    langVersion = defaultLangVersion;
            //}
            //if (string.IsNullOrEmpty(outFileName)) 
            //{ 
            //    outFileName = defaultOutFileName;
            //}

            //if (!outFileName.EndsWith(".exe"))
            //{
            //    outFileName += ".exe";
            //}

            //string outFilePath;
            //if (string.IsNullOrEmpty(Path.GetDirectoryName(outFileName)))
            //{
            //    outFilePath = Path.Combine(IO.Folders.GetTasktTemporaryFolderPath(), outFileName);
            //}
            //else
            //{
            //    var fn = Path.GetFileName(outFileName);
            //    outFilePath = Path.Combine(IO.Folders.GetTasktTemporaryFolderPath(), fn);
            //}

            //// create provider
            ////CodeDomProvider codeProvider = CodeDomProvider.CreateProvider("CSharp");
            //var roslyn = new Microsoft.CodeDom.Providers.DotNetCompilerPlatform.CSharpCodeProvider();

            //// create compile parameters
            //var parameters = new CompilerParameters
            //{
            //    GenerateExecutable = true,
            //    OutputAssembly = outFilePath,
            //    CompilerOptions = $"-langversion:{langVersion}",
            //};

            //// compile
            //return roslyn.CompileAssemblyFromSource(parameters, codeInput);

            return CompileProcess(new Func<CSharpCodeProvider, CompilerParameters, CompilerResults>((r, p) =>
            {
                return r.CompileAssemblyFromSource(p, codeInput);
            }), langVersion, outFileName);
        }

        /// <summary>
        /// compile csharp file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="langVersion"></param>
        /// <param name="outFileName"></param>
        /// <returns></returns>
        public static CompilerResults CompileCSFile(string filePath, string langVersion = "", string outFileName = "")
        {
            return CompileProcess(new Func<CSharpCodeProvider, CompilerParameters, CompilerResults>((r, p) =>
            {
                return r.CompileAssemblyFromFile(p, filePath);
            }), langVersion, outFileName);
        }

        /// <summary>
        /// general compile action
        /// </summary>
        /// <param name="roslynCompileFunc"></param>
        /// <param name="langVersion"></param>
        /// <param name="outFileName"></param>
        /// <returns></returns>
        private static CompilerResults CompileProcess(Func<CSharpCodeProvider, CompilerParameters, CompilerResults> roslynCompileFunc, string langVersion = "", string outFileName = "")
        {
            if (string.IsNullOrEmpty(langVersion))
            {
                langVersion = defaultLangVersion;
            }
            if (string.IsNullOrEmpty(outFileName))
            {
                outFileName = defaultOutFileName;
            }

            if (!outFileName.EndsWith(".exe"))
            {
                outFileName += ".exe";
            }

            string outFilePath;
            if (string.IsNullOrEmpty(Path.GetDirectoryName(outFileName)))
            {
                outFilePath = Path.Combine(IO.Folders.GetTasktTemporaryFolderPath(), outFileName);
            }
            else
            {
                var fn = Path.GetFileName(outFileName);
                outFilePath = Path.Combine(IO.Folders.GetTasktTemporaryFolderPath(), fn);
            }

            // create provider
            //CodeDomProvider codeProvider = CodeDomProvider.CreateProvider("CSharp");
            var roslyn = new CSharpCodeProvider();

            // create compile parameters
            var parameters = new CompilerParameters
            {
                GenerateExecutable = true,
                OutputAssembly = outFilePath,
                CompilerOptions = $"-langversion:{langVersion}",
            };

            return roslynCompileFunc(roslyn, parameters);
        }
    }
}
