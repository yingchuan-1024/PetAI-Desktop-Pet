using System;
using System.IO;
using System.Threading;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using taskt.Core;
using taskt.Core.Automation.Engine;

namespace taskt
{
    internal static class PetaiBridge
    {
        internal static void Configure(string root)
        {
            Directory.CreateDirectory(root);
            var settings = new ApplicationSettings();
            settings.GetClientSettings().RootFolder = root;
            settings.GetClientSettings().AttendedTasksFolder = Path.Combine(root, "My Scripts");
            settings.GetClientSettings().CheckForUpdateAtStartup = false;
            settings.GetClientSettings().AntiIdleWhileOpen = false;
            settings.GetServerSettings().ServerConnectionEnabled = false;
            settings.GetServerSettings().ConnectToServerOnStartup = false;
            settings.GetLocalListenerSettings().LocalListeningEnabled = false;
            settings.GetLocalListenerSettings().StartListenerOnStartup = false;
            settings.GetEngineSettings().TrackExecutionMetrics = false;
            var file = Path.Combine(root, "petai-settings.xml");
            settings.Save(file);
            Program.LoadPetaiSettings(file);
            foreach (var name in new[] { "Logs", "My Scripts", "BeforeConverted", "Temporary", "RunWithoutSaving", "AutoSave" })
                Directory.CreateDirectory(Path.Combine(root, name));
        }

        private static void Write(string file, object value)
        {
            var temporary = file + "." + Guid.NewGuid().ToString("N") + ".tmp";
            File.WriteAllText(temporary, JsonConvert.SerializeObject(value));
            if (File.Exists(file)) File.Replace(temporary, file, null);
            else File.Move(temporary, file);
        }

        internal static void Run(string requestFile, bool validate)
        {
            string response = null;
            string id = null;
            try
            {
                requestFile = Path.GetFullPath(requestFile);
                var request = JObject.Parse(File.ReadAllText(requestFile));
                id = (string)request["id"];
                var script = (string)request["scriptPath"];
                var requestedResponse = (string)request["responseFile"];
                var directory = Path.GetDirectoryName(requestFile);
                if (string.IsNullOrWhiteSpace(id) || !Path.IsPathRooted(script) ||
                    !Path.IsPathRooted(requestedResponse) || Path.GetDirectoryName(Path.GetFullPath(requestedResponse)) != directory ||
                    !string.Equals(Path.GetExtension(script), ".xml", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Invalid bridge request");
                response = Path.GetFullPath(requestedResponse);
                using (var reader = XmlReader.Create(script, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null }))
                {
                    var parsed = (Core.Script.Script)Core.Script.Script.CreateSerializer().Deserialize(reader);
                    if (parsed == null || parsed.Commands == null) throw new InvalidDataException("Invalid Taskt script");
                }
                if (validate)
                {
                    Write(response, new { id, status = "validated", ok = true });
                    return;
                }
                App.UpdateLocationAndVersionInfo();
                Configure(Path.Combine(directory, "engine"));
                var engine = new AutomationEngineInstance();
                var gate = new object();
                bool finished = false;
                bool cancelRequested = false;
                using (var done = new ManualResetEvent(false))
                {
                    engine.ScriptFinishedEvent += (sender, result) =>
                    {
                        lock (gate)
                        {
                            finished = true;
                            var status = result.Result == ScriptFinishedEventArgs.ScriptFinishedResult.Successful ? "succeeded" :
                                result.Result == ScriptFinishedEventArgs.ScriptFinishedResult.Cancelled ? "cancelled" : "failed";
                            Write(response, new { id, status, ok = status == "succeeded", engineResult = result.Result.ToString(),
                                error = result.Error, executionTimeMs = result.ExecutionTime.TotalMilliseconds,
                                cancelRequested, cancelConfirmed = status == "cancelled", externalProcessesMayBeActive = true });
                            done.Set();
                        }
                    };
                    Write(response, new { id, status = "running", ok = false });
                    engine.ExecuteScriptAsync(script);
                    while (!done.WaitOne(100))
                    {
                        lock (gate)
                        {
                            if (!finished && !cancelRequested && File.Exists(Path.Combine(directory, "cancel.json")))
                            {
                                cancelRequested = true;
                                engine.CancelScript();
                                engine.ResumeScript();
                                Write(response, new { id, status = "cancel_requested", ok = false, cancelRequested = true,
                                    cancelConfirmed = false, externalProcessesMayBeActive = true });
                            }
                        }
                    }
                }
            }
            catch (Exception error)
            {
                Environment.ExitCode = 1;
                if (response != null) Write(response, new { id, status = "bridge_error", ok = false, error = error.Message,
                    cancelConfirmed = false, externalProcessesMayBeActive = true });
            }
        }
    }
}
