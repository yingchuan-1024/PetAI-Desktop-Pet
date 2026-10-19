using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using RestSharp;
using static taskt.Core.Automation.Commands.TextControls;

namespace taskt.Core.Server
{
    /// <summary>
    /// Exposes a local API which enables other (taskt) clients to send automation
    /// </summary>
    public static class LocalTCPListener
    {
        public static Automation.Engine.AutomationEngineInstance automationInstance;
        
        public static Automation.Engine.AutomationEngineInstance ExecuteCommandEngine;
        
        /// <summary>
        /// LocalListener Logger
        /// </summary>
        private static Serilog.Core.Logger automationLogger;
        
        /// <summary>
        /// TcpListener for LocalListener
        /// </summary>
        public static TcpListener automationListener;

        private static ILocalListenerSettings listenerSettings;
        
        public static UI.Forms.ScriptBuilder.frmScriptBuilder associatedBuilder;

        public static int Port;

        /// <summary>
        /// LocalListener is Listening state
        /// </summary>
        public static bool IsListening { get; set; }

        public static string TasktResult { get; set; }
        public static event EventHandler ListeningStarted;
        public static event EventHandler ListeningStopped;

        /// <summary>
        /// parameter names
        /// </summary>
        private enum ParameterName
        {
            None,
            ScriptData,
            ScriptLocation,
            CommandData,
        }

        /// <summary>
        /// Rest API order
        /// </summary>
        private enum RestOrder
        {
            None,
            ExecuteScript,
            AwaitScript,
            EngineStatus,
            RestartTaskt,
        }

        static LocalTCPListener()
        {
            automationLogger = new Logging().CreateLogger("Automation Client", Serilog.RollingInterval.Day);
            ExecuteCommandEngine = new Automation.Engine.AutomationEngineInstance();
        }

        public static void Initialize(UI.Forms.ScriptBuilder.frmScriptBuilder builder)
        {
            associatedBuilder = builder;
            automationLogger.Information("Automation Listener Initializing");

            automationInstance = new Automation.Engine.AutomationEngineInstance();

            var appSettings = App.Taskt_Settings;

            listenerSettings = appSettings.ListenerSettings;

            if (listenerSettings.LocalListeningEnabled)
            {
                automationLogger.Information("Local Listening is Enabled");
            }
            else
            {
                automationLogger.Information("Local Listening is Disabled");
            }

            if ((listenerSettings.StartListenerOnStartup) && (listenerSettings.LocalListeningEnabled))
            {
                automationLogger.Information("Automatically Starting Listening Service");
                StartListening(listenerSettings.ListeningPort);
            }
            else if ((listenerSettings.StartListenerOnStartup) && (!listenerSettings.LocalListeningEnabled))
            {
                automationLogger.Information("Listening Service is not Enabled! Unable to Automatically Start Listening!");
            }

            automationLogger.Information("Automation Listener Finished Initializing");
        }

        /// <summary>
        /// start LocalListener thread
        /// </summary>
        /// <param name="port"></param>
        public static void StartListening(int port)
        {
            if (!listenerSettings.LocalListeningEnabled)
            {
                automationLogger.Information("Listening Service is not Enabled! Unable to Start Listening!");
                return;
            }

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;   // back ground thread
                StartAutomationListener(port);
            }).Start();
        }

        /// <summary>
        /// LocalListener start and wait/recieve data
        /// </summary>
        /// <param name="port"></param>
        public static void StartAutomationListener(int port)
        {
            automationLogger.Information("Local HTTP listener is disabled in the PetAI distribution");
            return;
#pragma warning disable CS0162
            try
            {
                // TcpListener server = new TcpListener(port);
                automationListener = new TcpListener(IPAddress.Any, port);
                Port = port;    // "Port" is static variable, what?

                // Start listening for client requests.
                automationListener.Start();
                automationLogger.Information($"Automation Listener Endpoint started at {automationListener.LocalEndpoint}");

                // Enter the listening loop.
                while (true)
                {
                    IsListening = true;

                    ListeningStarted?.Invoke(null, null);
                    automationLogger.Information($"Automation Listener Waiting for Request");

                    TcpClient client = automationListener.AcceptTcpClient();
                    client.NoDelay = false;

                    automationLogger.Information($"Client '{client.Client.RemoteEndPoint}' Connected to Automation Listener");

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();
                    stream.ReadTimeout = 1000;
                    stream.WriteTimeout = 1000;

                    // Buffer for reading data
                    byte[] bytes = new byte[65535]; // many
                    string data = null;

                    try
                    {
                        using (var ms = new MemoryStream())
                        {
                            int resSize = 0;
                            do
                            {
                                try
                                {
                                    resSize = stream.Read(bytes, 0, bytes.Length);
                                    if ((resSize > 0))
                                    {
                                        ms.Write(bytes, 0, resSize);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                catch
                                {
                                    break;
                                }
                            } while (resSize > 0);
                            data = System.Text.Encoding.ASCII.GetString(ms.GetBuffer(), 0, (int)ms.Length);
                        }

                        automationLogger.Information($"Client Message Content: {data}");

                        //break out request content, split NewLine
                        var messageContent = data.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

                        if (listenerSettings.EnableWhitelist)
                        {
                            // check WhiteList
                            automationLogger.Information($"Listener requires IP Verification (Whitelist)");

                            // verify that client is allowed to connect
                            var clientAddress = (((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString());

                            // get list of WhiteList-IP
                            var enabledIPs = listenerSettings.IPWhiteList.Split(',');

                            if (enabledIPs.Any(s => s.Trim().Contains(clientAddress)))
                            {
                                automationLogger.Information($"Client '{clientAddress}' verified from WhiteList '{listenerSettings.IPWhiteList}'");
                            }
                            else
                            {
                                automationLogger.Information($"Closing Client Connection due to IP verification failure");
                                SendResponse(ResponseCode.Unauthorized, $"Unauthorized", stream);
                                return;
                            }
                        }
                        else
                        {
                            automationLogger.Information($"Listener does not require IP Verification");
                        }

                        //if (listenerSettings.RequireListenerAuthenticationKey)
                        //{
                        //    // check AuthenticationKey

                        //    // extract AuthKey
                        //    string authKey = "";
                        //    foreach (var item in messageContent)
                        //    {
                        //        if (item.StartsWith("AuthKey: "))
                        //        {
                        //            authKey = item.Replace("AuthKey: ", "");
                        //            break;
                        //        }
                        //    }

                        //    // auth key check
                        //    if (string.IsNullOrEmpty(authKey))
                        //    {
                        //        // auth key not provided
                        //        automationLogger.Information($"Closing Client Connection due to Null/Empty Auth Key");
                        //        SendResponse(ResponseCode.Unauthorized, $"Invalid Auth Key", stream);
                        //        break;
                        //    }
                        //    else if (authKey != listenerSettings.AuthKey)
                        //    {
                        //        // auth key invalid   
                        //        automationLogger.Information($"Closing Client Connection due to Invalid Auth Key");
                        //        SendResponse(ResponseCode.Unauthorized, $"Invalid Auth Key", stream);
                        //        break;
                        //    }
                        //    else if (authKey == listenerSettings.AuthKey)
                        //    {
                        //        // auth key valid
                        //        automationLogger.Information($"Auth Key Verified");
                        //        ProcessRequest(data, messageContent, stream);
                        //    }
                        //}
                        //else
                        //{
                        //    // verification not required
                        //    automationLogger.Information($"Auth Key Verification Not Required");
                        //    ProcessRequest(data, messageContent, stream);
                        //}
                        ProcessRequest(data, messageContent, stream);
                    }
                    catch (Exception ex)
                    {
                        automationLogger.Information($"Error Occured Reading Stream: {ex}");
                    }

                    // Shutdown and end connection
                    client.Close();
                    automationLogger.Information($"Client Connection Closed");
                }
            }
            catch (SocketException ex)
            {
                automationLogger.Information("SocketException: {0}", ex);
            }
            finally
            {
                // Stop listening for new clients.
                automationListener.Stop();
                IsListening = false;
                ListeningStopped?.Invoke(null, null);
            }
        }

        /// <summary>
        /// process request by LocalListener messages
        /// </summary>
        /// <param name="data"></param>
        /// <param name="messageContent"></param>
        /// <param name="stream"></param>
        private static void ProcessRequest(string data, string[] messageContent, NetworkStream stream)
        {
            // extract order
            RestOrder order = RestOrder.None;
            var lowData = data.ToLower();
            if (lowData.StartsWith("post /executescript") || lowData.StartsWith("post /execute_script"))
            {
                order = RestOrder.ExecuteScript;
            }
            else if (lowData.StartsWith("post /awaitscript") || lowData.StartsWith("post /await_script"))
            {
                order = RestOrder.AwaitScript;
            }
            else if (lowData.StartsWith("post /enginestatus") || lowData.StartsWith("post /engine_status"))
            {
                order = RestOrder.EngineStatus;
            }
            else if (lowData.StartsWith("post /restarttaskt") || lowData.StartsWith("post /restart_taskt"))
            {
                order = RestOrder.RestartTaskt;
            }
            else 
            {
                // invalid request
                automationLogger.Information($"Invalid Client Request: {messageContent[0]}");
                SendResponse(ResponseCode.InternalServerError, "Invalid Client Request", stream);
                return;
            }

            // extract body, header
            string headerParameterValue = "";
            ParameterName parameterName = ParameterName.None;
            string body = "";
            int idx;
            for (idx = 0; idx < messageContent.Length; idx++)
            {
                if (string.IsNullOrEmpty(messageContent[idx]))
                {
                    break;
                }
                else
                {
                    // extract parameter from header
                    var v = messageContent[idx].ToLower();
                    if (v.StartsWith("scriptdata: "))
                    {
                        parameterName = ParameterName.ScriptData;
                        headerParameterValue = messageContent[idx].Substring(12);
                    }
                    else if (v.StartsWith("scriptlocation: "))
                    {
                        parameterName = ParameterName.ScriptLocation;
                        headerParameterValue = messageContent[idx].Substring(16);
                    }
                    else if (v.StartsWith("commanddata: "))
                    {
                        parameterName = ParameterName.CommandData;
                        headerParameterValue = messageContent[idx].Substring(13);
                    }
                }
            }
            for (; idx < messageContent.Length; idx++)
            {
                body += $"{messageContent[idx]}\r\n";
            }
            body = body.Trim();
            JObject jsonBody = null;
            try
            {
                jsonBody = JObject.Parse(body);
            }
            catch
            {
                // nothing;
            }

            // Auth Key
            if (App.Taskt_Settings.ListenerSettings.RequireListenerAuthenticationKey)
            {
                string authKey = "";
                foreach (var item in messageContent)
                {
                    if (item.ToLower().StartsWith("authkey: "))
                    {
                        authKey = item.Substring(9);
                        break;
                    }
                }
                if (string.IsNullOrEmpty(authKey))
                {
                    var a = jsonBody.GetValue("AuthKey", StringComparison.CurrentCultureIgnoreCase);
                    if (a != null)
                    {
                        authKey = a.ToString();
                    }
                }

                // auth key check
                if (string.IsNullOrEmpty(authKey))
                {
                    // auth key not provided
                    automationLogger.Information($"Closing Client Connection due to Null/Empty Auth Key");
                    SendResponse(ResponseCode.Unauthorized, $"Invalid Auth Key", stream);
                    return;
                }
                else if (authKey != listenerSettings.AuthKey)
                {
                    // auth key invalid   
                    automationLogger.Information($"Closing Client Connection due to Invalid Auth Key");
                    SendResponse(ResponseCode.Unauthorized, $"Invalid Auth Key", stream);
                    return;
                }
                else if (authKey == listenerSettings.AuthKey)
                {
                    // auth key valid
                    automationLogger.Information($"Auth Key Verified");
                }
            }

            switch (order)
            {
                case RestOrder.AwaitScript:
                case RestOrder.ExecuteScript:
                    automationLogger.Information($"Client Requests Script Execution");

                    string dataParameter64 = "";  // data (base64)
                    switch (parameterName)
                    {
                        case ParameterName.ScriptData:
                            dataParameter64 = headerParameterValue;
                            break;
                        case ParameterName.ScriptLocation:
                            dataParameter64 = headerParameterValue;
                            break;
                        case ParameterName.CommandData:
                            dataParameter64 = headerParameterValue;
                            break;
                        default:
                            // no parameter in header
                            if (jsonBody != null)
                            {
                                var scriptData = jsonBody.GetValue("ScriptData", StringComparison.CurrentCultureIgnoreCase);
                                if (scriptData != null)
                                {
                                    dataParameter64 = scriptData.ToString();
                                    parameterName = ParameterName.ScriptData;
                                }
                                else
                                {
                                    var scriptLocation = jsonBody.GetValue("ScriptLocation", StringComparison.CurrentCultureIgnoreCase);
                                    if (scriptLocation != null)
                                    {
                                        dataParameter64 = scriptLocation.ToString();
                                        parameterName = ParameterName.ScriptLocation;
                                    }
                                    else
                                    {
                                        var commandData = jsonBody.GetValue("CommandData", StringComparison.CurrentCultureIgnoreCase);
                                        parameterName = ParameterName.CommandData;
                                    }
                                }
                            }
                            break;
                    }

                    // check to see if nothing was provided
                    if (string.IsNullOrEmpty(dataParameter64))
                    {
                        automationLogger.Information($"Data Not Found");
                        SendResponse(ResponseCode.InternalServerError, "Data Not Found", stream);
                        return;
                    }

                    // decode from Base64
                    if (dataParameter64.TryParseBase64(out var rawString))
                    {
                        automationLogger.Information($"Client Passed Base64 String: {rawString}");
                    }
                    else
                    {
                        automationLogger.Information($"Client Did Not Pass Base64 String");
                        SendResponse(ResponseCode.InternalServerError, "Invalid Data. Base64?", stream);
                        return;
                    }

                    // check if data parameter references file location
                    switch(parameterName)
                    {
                        case ParameterName.ScriptLocation:
                            // specified file path
                            string filePath;
                            if (File.Exists(rawString))
                            {
                                filePath = rawString;
                            }
                            else if (File.Exists(Path.Combine(IO.Folders.GetScriptsFolderPath(), rawString)))
                            {
                                // file was found at fallback to scripts folder, specified only file name
                                filePath = Path.Combine(IO.Folders.GetScriptsFolderPath(), rawString);
                            }
                            else
                            {
                                // file not found
                                automationLogger.Information($"Client Script Location Not Found: {rawString}");
                                SendResponse(ResponseCode.InternalServerError, $"Client Script Location Not Found.", stream);
                                return;
                            }

                            automationLogger.Information($"Executing Script: {filePath}");
                            associatedBuilder.Invoke(new MethodInvoker(delegate ()
                            {
                                var newEngine = new UI.Forms.ScriptEngine.frmScriptEngine(filePath, associatedBuilder);
                                newEngine.callBackForm = null;
                                newEngine.Show();
                            }));
                            break;

                        case ParameterName.ScriptData:
                            // specified script file data
                            // log execution
                            automationLogger.Information($"Executing Script: {rawString}");

                            try
                            {
                                // check script file
                                Script.Script.DeserializeScript(rawString);
                            }
                            catch
                            {
                                automationLogger.Information($"Invalid Script File Data");
                                SendResponse(ResponseCode.InternalServerError, "Invalid Script File Data", stream);
                                break;
                            }

                            // save script file
                            var tempFilePath = Script.Script.GetRunWithoutSavingScriptFilePath();
                            using (var writer = new StreamWriter(tempFilePath))
                            {
                                writer.Write(rawString);
                            }

                            // invoke builder and pass it script data to execute
                            associatedBuilder.Invoke(new MethodInvoker(delegate ()
                            {
                                var newEngine = new UI.Forms.ScriptEngine.frmScriptEngine(tempFilePath, associatedBuilder);
                                newEngine.callBackForm = null;
                                newEngine.Show();
                            }));
                            break;
                    }

                    if (order == RestOrder.AwaitScript)
                    {
                        //reset result value
                        TasktResult = "";

                        //add reference to script finished event
                        automationInstance.ScriptFinishedEvent += AutomationInstance_ScriptFinishedEvent;

                        //wait for script to finish before returning
                        do
                        {
                            Thread.Sleep(1000);
                        } while (TasktResult == string.Empty);

                        //send response back to client
                        SendResponse(ResponseCode.OK, automationInstance.TasktResult, stream);
                    }
                    else
                    {   // POST /ExecuteScript
                        // return success immediately
                        SendResponse(ResponseCode.OK, "Script Launched Successfully", stream);
                    }
                    break;
                //else if (data.StartsWith("POST /ExecuteCommand"))   // I think this place will not be executed.
                //{
                //    automationLogger.Information($"Client Requests Command Execution");

                //    // locate the body content
                //    string dataParameter = "";

                //    // find the script parameter (CommandData)
                //    foreach (var item in messageContent)
                //    {
                //        if (item.StartsWith("CommandData: "))
                //        {
                //            dataParameter = item.Replace("CommandData: ", "");
                //            break;
                //        }
                //    }

                //    // check to see if nothing was provided
                //    if (string.IsNullOrEmpty(dataParameter))
                //    {
                //        automationLogger.Information($"Client Command Data Not Found");
                //        return;
                //    }

                //    if (dataParameter.TryParseBase64(out var base64SourceString))
                //    {
                //        automationLogger.Information($"Client Passed Base64 String: {base64SourceString}");
                //        dataParameter = base64SourceString;
                //    }
                //    else
                //    {
                //        automationLogger.Information($"Client Did Not Pass Base64 String");
                //    }

                //    try
                //    {
                //        // deserialize command (JSON?)
                //        var command = JsonConvert.DeserializeObject(dataParameter, new Newtonsoft.Json.JsonSerializerSettings() { TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All });

                //        // log execution
                //        automationLogger.Information($"Executing Command: {dataParameter}");

                //        // define script action
                //        var scriptAction = new Script.ScriptAction() { ScriptCommand = (Automation.Commands.ScriptCommand)command };

                //        // execute command
                //        ExecuteCommandEngine.ExecuteCommand(scriptAction);

                //        // send back response
                //        SendResponse(ResponseCode.OK, "Command Executed Successfully", stream);
                //    }
                //    catch (Exception ex)
                //    {
                //        SendResponse(ResponseCode.InternalServerError, $"An error occured: {ex.ToString()}", stream);
                //    }
                //}
                case RestOrder.EngineStatus:
                    automationLogger.Information($"Returning Engine Status: {Client.ClientStatus}");
                    SendResponse(ResponseCode.OK, Client.ClientStatus, stream);
                    break;

                case RestOrder.RestartTaskt:
                    automationLogger.Information($"Restarting taskt");
                    SendResponse(ResponseCode.OK, "taskt is being Restarted", stream);
                    Application.Restart();
                    break;

                default:
                    // i don't know this message :-(
                    automationLogger.Information($"Invalid Client Request");
                    SendResponse(ResponseCode.InternalServerError, "Invalid Client Request", stream);
                    break;
            }
        }

        private static void AutomationInstance_ScriptFinishedEvent(object sender, Automation.Engine.ScriptFinishedEventArgs e)
        {
            //set result once script completes
            TasktResult = automationInstance.TasktResult;
        }

        /// <summary>
        /// send LocalListener Response(Result) to Client
        /// </summary>
        /// <param name="ResponseCode"></param>
        /// <param name="content"></param>
        /// <param name="networkStream"></param>
        /// <exception cref="NotImplementedException"></exception>
        public static void SendResponse(ResponseCode ResponseCode, string content, Stream networkStream)
        {
            using (var writer = new StreamWriter(networkStream))
            {
                // i want to change switch()
                string responseHeader;
                switch (ResponseCode)
                {
                    case ResponseCode.OK:
                        responseHeader = "HTTP/1.1 200 OK";
                        break;
                    case ResponseCode.InternalServerError:
                        responseHeader = "HTTP/1.1 500 INTERNAL SERVER ERROR";
                        break;
                    case ResponseCode.Unauthorized:
                        responseHeader = "HTTP/1.1 401 UNAUTHORIZED";
                        break;
                    default:
                        throw new NotImplementedException();
                }

                // send result message
                writer.Write(responseHeader);
                writer.Write(Environment.NewLine);
                writer.Write("Content-Type: text/plain; charset=utf-8");
                writer.Write(Environment.NewLine);
                writer.Write("Content-Length: " + content.Length);
                writer.Write(Environment.NewLine);
                writer.Write(Environment.NewLine);
                writer.Write(content);
                writer.Flush();
            }
        }

        /// <summary>
        /// stop listener
        /// </summary>
        public static void StopAutomationListener()
        {
            automationListener.Stop();
        }

        ///// <summary>
        ///// send task to LocalListener(server) from Client
        ///// </summary>
        ///// <param name="endpoint"></param>
        ///// <param name="parameterType"></param>
        ///// <param name="timeout"></param>
        ///// <param name="scriptData"></param>
        ///// <param name="awaitPreference"></param>
        ///// <returns></returns>
        ///// <exception cref="FileNotFoundException"></exception>
        //public static string SendAutomationTask(string endpoint, string parameterType, string timeout, string scriptData = "", string awaitPreference = "")
        //{
        //    var request = new RestRequest();
        //    //request.Method = RestSharp.Method.POST;
        //    request.Method = Method.Post;
        //    request.AddHeader("Content-Type", "text/plain");

        //    // authKey get from appSettings, it's ok? really?
        //    if (listenerSettings.RequireListenerAuthenticationKey)
        //    {
        //        request.AddHeader("AuthKey", listenerSettings.AuthKey);
        //    }

        //    // MEMO: Resource part of the remote endpoint URL. For example, when using the client-level base URL https://localhost:5000/api and Resource set to weather, the request will be sent to https://localhost:5000/api/weather

        //    // check type of execution needed
        //    if (parameterType == "run raw script data")
        //    {
        //        // handle await preference
        //        if (awaitPreference == "await for result")
        //        {
        //            request.Resource = "/AwaitScript";
        //        }
        //        else
        //        {
        //            request.Resource = "/ExecuteScript";
        //        }

        //        // add raw script data (convert to Base64)
        //        request.AddParameter("ScriptData", scriptData.ConvertToBase64(), RestSharp.ParameterType.HttpHeader);
        //    }
        //    else if (parameterType == "run local file")
        //    {
        //        // handle await preference
        //        if (awaitPreference == "await for result")
        //        {
        //            request.Resource = "/AwaitScript";
        //        }
        //        else
        //        {
        //            request.Resource = "/ExecuteScript";
        //        }

        //        if (File.Exists(scriptData))
        //        {
        //            //file was found at path provided
        //            scriptData = File.ReadAllText(scriptData);
        //        }
        //        else if (File.Exists(Path.Combine(IO.Folders.GetScriptsFolderPath(), scriptData)))
        //        {
        //            // file was found at fallback to scripts folder
        //            scriptData = Path.Combine(IO.Folders.GetScriptsFolderPath(), scriptData);
        //            scriptData = File.ReadAllText(scriptData);
        //        }
        //        else
        //        {
        //            throw new FileNotFoundException(scriptData);
        //        }

        //        // add script data (convert to Base64)
        //        request.AddParameter("ScriptData", scriptData.ConvertToBase64(), RestSharp.ParameterType.HttpHeader);
        //    }
        //    else if (parameterType == "run remote file")
        //    {
        //        // handle await preference
        //        if (awaitPreference == "await for result")
        //        {
        //            request.Resource = "/AwaitScript";
        //        }
        //        else
        //        {
        //            request.Resource = "/ExecuteScript";
        //        }

        //        // add script parameter (convert to Base64)
        //        request.AddParameter("ScriptLocation", scriptData.ConvertToBase64(), RestSharp.ParameterType.HttpHeader);
        //    }
        //    else if (parameterType == "run command json")
        //    {
        //        request.Resource = "/ExecuteCommand";

        //        // add script data
        //        request.AddParameter("CommandData", scriptData.ConvertToBase64(), RestSharp.ParameterType.HttpHeader);
        //    }
        //    else if (parameterType == "get engine status")
        //    {
        //        request.Resource = "/EngineStatus";
        //    }
        //    else if (parameterType == "restart taskt")
        //    {
        //        request.Resource = "/RestartTaskt";
        //    }

        //    request.Timeout = new TimeSpan(int.Parse(timeout));

        //    if (!endpoint.StartsWith("http://"))
        //    {
        //        endpoint = $"http://{endpoint}";
        //    }
        //    var client = new RestClient(endpoint);

        //    var resp = client.Execute<RestResponse>(request);

        //    //var resp = await client.ExecuteAsync<RestResponse>(request);


        //    //var stopDateTime = DateTime.Now.AddMilliseconds(int.Parse(timeout));
        //    //while (true)
        //    //{
        //    //    if (resp.ErrorMessage != null)
        //    //    {
        //    //        break;
        //    //    }
        //    //    else if (DateTime.Now > stopDateTime)
        //    //    {
        //    //        break;
        //    //    }
        //    //    Thread.Sleep(500);  // wait 500 ms
        //    //}

        //    string content;
        //    if (resp.ErrorMessage is null)
        //    {
        //        content = resp.Content;
        //    }
        //    else
        //    {
        //        content = resp.ErrorMessage;
        //    }

        //    return content;
        //}

        public static string SendAutomationTask(string endpoint, string parameterType, string timeout, string scriptData = "", string awaitPreference = "", string authKey = "")
        {
            var request = new RestRequest();
                    //.AddHeader("Content-Type", "text/json");

            // authKey get from appSettings, it's ok? really?
            //if (listenerSettings.RequireListenerAuthenticationKey)
            //{
            //    request.AddHeader("AuthKey", listenerSettings.AuthKey);
            //}
            var sendBodyObject = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(authKey))
            {
                sendBodyObject.Add("AuthKey", authKey);
            }

            // MEMO: Resource part of the remote endpoint URL. For example, when using the client-level base URL https://localhost:5000/api and Resource set to weather, the request will be sent to https://localhost:5000/api/weather

            // decide request.Resource
            switch (parameterType)
            {
                case "run raw script data":
                case "run local file":
                case "run remote file":
                    request.Resource = (awaitPreference == "await for result") ? "/AwaitScript" : "/ExecuteScript";
                    break;
                case "run command json":
                    request.Resource = "/ExecuteCommand";
                    break;
                case "get engine status":
                    request.Resource = "/EngineStatus";
                    break;
                case "restart taskt":
                    request.Resource = "/RestartTaskt";
                    break;
            }

            // decide reqest.parameter
            switch (parameterType)
            {
                case "run raw script data":
                    sendBodyObject.Add("ScriptData", scriptData.ConvertToBase64());
                    request.AddJsonBody(sendBodyObject);
                    break;
                case "run local file":
                    if (File.Exists(scriptData))
                    {
                        //file was found at path provided
                        scriptData = File.ReadAllText(scriptData);
                    }
                    else if (File.Exists(Path.Combine(IO.Folders.GetScriptsFolderPath(), scriptData)))
                    {
                        // file was found at fallback to scripts folder
                        scriptData = Path.Combine(IO.Folders.GetScriptsFolderPath(), scriptData);
                        scriptData = File.ReadAllText(scriptData);
                    }
                    else
                    {
                        throw new FileNotFoundException(scriptData);
                    }

                    sendBodyObject.Add("ScriptData", scriptData.ConvertToBase64());
                    request.AddJsonBody(sendBodyObject);
                    break;
                case "run remote file":
                    sendBodyObject.Add("ScriptLocation", scriptData.ConvertToBase64());
                    request.AddJsonBody(sendBodyObject);
                    break;
                case "run command json":
                    sendBodyObject.Add("CommandData", scriptData.ConvertToBase64());
                    request.AddJsonBody(sendBodyObject);
                    break;
            }

            request.Timeout = TimeSpan.FromSeconds(double.Parse(timeout));

            if (!endpoint.StartsWith("http://"))
            {
                endpoint = $"http://{endpoint}";
            }

            //var client = new RestClient(endpoint);
            var client = new RestClient(new RestClientOptions(endpoint)
            {
                Timeout = TimeSpan.FromSeconds(double.Parse(timeout))
            });

            string content;
            try
            {
                var resp = client.Execute(request, Method.Post);
                if (resp.ErrorMessage is null)
                {
                    content = resp.Content;
                }
                else
                {
                    content = resp.ErrorMessage;
                }
            }
            catch
            {
                content = "Exception";
            }
            return content;
        }

        /// <summary>
        /// get LocalListener IP and Port
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string GetListeningAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return $"{ip.ToString()}:{Port.ToString()}";
                }
            }

            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }

    public enum ResponseCode
    {
        OK = 200,
        InternalServerError = 500,
        Unauthorized = 401
    }
}
