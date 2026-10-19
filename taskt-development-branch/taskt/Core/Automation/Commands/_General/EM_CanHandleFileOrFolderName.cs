using System;
using System.IO;

namespace taskt.Core.Automation.Commands
{
    public static class EM_CanHandleFileOrFolderName
    {
        /// <summary>
        /// file or folder?
        /// </summary>
        public enum ExpandValueType
        {
            File,
            Folder
        }

        /// <summary>
        /// expand value or user variable as File or Folder name
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameterName"></param>
        /// <param name="engine"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string ExpandValueOrUserVariableAsFileOrFolderName(this ICanHandleFileOrFolderName command, string parameterName, Engine.AutomationEngineInstance engine, ExpandValueType t)
        {
            var prop = command.ToScriptCommand().GetProperty(parameterName);
            string parameterValue = prop.GetValue(command)?.ToString() ?? "";

            var fn = parameterValue.ExpandValueOrUserVariable(engine);
            if (IsValidFileOrFolderName(fn))
            {
                return fn;
            }
            else
            {
                string typeName = Enum.GetName(typeof(ExpandValueType), t);

                throw new Exception($"{typeName} Name contains invalid chars. {typeName} Name: '{fn}'");
            }
        }

        /// <summary>
        /// check valid file/folder name
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool IsValidFileOrFolderName(string fileName)
        {
            var invs = Path.GetInvalidFileNameChars();
            return (fileName.IndexOfAny(invs) < 0);
        }
    }
}
