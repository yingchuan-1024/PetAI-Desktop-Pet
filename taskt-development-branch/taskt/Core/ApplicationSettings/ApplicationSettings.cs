using System;
using System.IO;
using System.Xml.Serialization;
using taskt.Core.IO;

namespace taskt.Core
{
    /// <summary>
    /// Defines settings for the entire application
    /// </summary>
    [Serializable]
    public sealed class ApplicationSettings : IApplicationSettings
    {
        #region xml properties

        public IServerSettings ServerSettings { get; set; } = new ServerSettings();

        public IEngineSettings EngineSettings { get; set; } = new EngineSettings();

        public IClientSettings ClientSettings { get; set; } = new ClientSettings();

        public ILocalListenerSettings ListenerSettings { get; set; } = new LocalListenerSettings();

        #endregion

        #region InnerClass

        /// <summary>
        /// ApplicationSettings for File I/O
        /// </summary>
        [Serializable]
        public sealed class FileIOApplicationSettings
        {
            /// <summary>
            /// Server Settings
            /// </summary>
            public ServerSettings ServerSettings { get; set; }

            /// <summary>
            /// Engine Settings
            /// </summary>
            public EngineSettings EngineSettings { get; set;  }

            /// <summary>
            /// Client Settings
            /// </summary>
            public ClientSettings ClientSettings { get; set;  }

            /// <summary>
            /// Listener Settings
            /// </summary>
            public LocalListenerSettings ListenerSettings { get; set; }

            public FileIOApplicationSettings() 
            { 
            }

            public FileIOApplicationSettings(ApplicationSettings settings)
            {
                ServerSettings = settings.GetServerSettings();
                EngineSettings = settings.GetEngineSettings();
                ClientSettings = settings.GetClientSettings();  
                ListenerSettings = settings.GetLocalListenerSettings();
            }

            /// <summary>
            /// Output ApplicationSettings as XML
            /// </summary>
            /// <param name="filePath"></param>
            public static void SaveAs(ApplicationSettings settings, string filePath)
            {
                string outputText;
                using (var sw = new StringWriter())
                {
                    var serializer = new XmlSerializer(typeof(FileIOApplicationSettings));
                    serializer.Serialize(sw, new FileIOApplicationSettings(settings));
                    outputText = sw.ToString();
                }

                outputText = outputText.Replace($"<{nameof(FileIOApplicationSettings)} ", $"<{nameof(ApplicationSettings)} ")
                                .Replace($"</{nameof(FileIOApplicationSettings)}>", $"</{nameof(ApplicationSettings)}>");
                using (var writer = new StreamWriter(filePath))
                {
                    writer.WriteLine(outputText);
                }
            }

            /// <summary>
            /// Open ApplicationSettings from XML
            /// </summary>
            /// <param name="filePath"></param>
            /// <returns></returns>
            public static ApplicationSettings Open(string filePath)
            {
                string xmlText;
                using (var reader = new StreamReader(filePath))
                {
                    xmlText = reader.ReadToEnd();
                }

                xmlText = xmlText.Replace($"<{nameof(ApplicationSettings)} ", $"<{nameof(FileIOApplicationSettings)} ")
                                .Replace($"</{nameof(ApplicationSettings)}>", $"</{nameof(FileIOApplicationSettings)}>");

                FileIOApplicationSettings settings = null;
                using (var sr = new StringReader(xmlText))
                {
                    var serializer = new XmlSerializer(typeof(FileIOApplicationSettings));
                    settings = (FileIOApplicationSettings)serializer.Deserialize(sr);
                }
                
                return new ApplicationSettings(settings);
            }
        }

        #endregion

        public ApplicationSettings()
        {
        }

        /// <summary>
        /// create instance by FileIOApplicationSettings
        /// </summary>
        /// <param name="settings"></param>
        private ApplicationSettings(FileIOApplicationSettings settings)
        {
            ServerSettings = settings.ServerSettings;
            EngineSettings = settings.EngineSettings;
            ClientSettings = settings.ClientSettings;
            ListenerSettings = settings.ListenerSettings;
        }

        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="appSettings"></param>
        private ApplicationSettings(ApplicationSettings appSettings)
        {
            ServerSettings = appSettings.GetServerSettings().Clone();
            EngineSettings = appSettings.GetEngineSettings().Clone();
            ClientSettings = appSettings.GetClientSettings().Clone();
            ListenerSettings = appSettings.GetLocalListenerSettings().Clone();
        }

        ///// <summary>
        ///// save taskt settigs file as XML
        ///// </summary>
        //public void Save()
        //{
        //    //create file path
        //    var filePath = Files.GetSettigsFilePath();

        //    SaveProcess(this, filePath);
        //}

        /// <summary>
        /// Save taskt settigs file as XML, the file name is specified by a argument
        /// </summary>
        /// <param name="fileName">file name of file path</param>
        public void Save(string fileName)
        {
            var filePath = (Path.IsPathRooted(fileName)) ? fileName : Files.GetSettigsFilePath(fileName);

            SaveProcess(this, filePath);
        }

        /// <summary>
        /// File Save Process
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="filePath"></param>
        private static void SaveProcess(ApplicationSettings settings, string filePath)
        {
            var settigsDir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(settigsDir))
            {
                Directory.CreateDirectory(settigsDir);
            }

            // output to xml file
            SaveAs(settings, filePath);
        }

        ///// <summary>
        ///// save taskt settigs xml file, settigs is specified
        ///// </summary>
        ///// <param name="settings"></param>
        //public static void Save(ApplicationSettings settings)
        //{
        //    //create file path
        //    var filePath = Files.GetSettigsFilePath();

        //    SaveProcess(settings, filePath);
        //}

        ///// <summary>
        ///// save taskt settigs xml file, settigs and fileName are specified
        ///// </summary>
        ///// <param name="settings"></param>
        ///// <param name="fileName">file name of file path</param>
        //public static void Save(ApplicationSettings settings, string fileName)
        //{
        //    var filePath = (Path.IsPathRooted(fileName)) ? fileName : Files.GetSettigsFilePath(fileName);

        //    SaveProcess(settings, filePath);
        //}

        /// <summary>
        /// Save settigs file as XML
        /// </summary>
        /// <param name="appSettings"></param>
        /// <param name="filePath"></param>
        private static void SaveAs(ApplicationSettings appSettings, string filePath)
        {
            //using (FileStream fileStream = File.Create(filePath))
            //{
            //    XmlSerializer serializer = new XmlSerializer(typeof(ApplicationSettings));
            //    serializer.Serialize(fileStream, appSettings);
            //    fileStream.Close();
            //}
            //new FileIOApplicationSettings(appSettings).SaveAs(filePath);
            FileIOApplicationSettings.SaveAs(appSettings, filePath);
        }

        ///// <summary>
        ///// get taskt settigs from file or create taskt settigs
        ///// </summary>
        ///// <returns></returns>
        //public static ApplicationSettings GetOrCreateApplicationSettings()
        //{
        //    //create file path
        //    var filePath = Files.GetSettigsFilePath();

        //    return GetOrCreateApplicationSettingsProcess(filePath);
        //}

        /// <summary>
        /// get taskt settigs from file or create taskt settigs, fileName is specified
        /// </summary>
        /// <param name="fileName">file name of file path</param>
        /// <returns></returns>
        public static ApplicationSettings GetOrCreateApplicationSettings(string fileName)
        {
            var filePath = (Path.IsPathRooted(fileName)) ? fileName : Files.GetSettigsFilePath(fileName);

            return GetOrCreateApplicationSettingsProcess(filePath);
        }

        /// <summary>
        /// get or create taskt settigs process
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static ApplicationSettings GetOrCreateApplicationSettingsProcess(string filePath)
        {
            ApplicationSettings appSettings;
            if (File.Exists(filePath))
            {
                try
                {
                    appSettings = Open(filePath);
                }
                catch
                {
                    appSettings = new ApplicationSettings();
                }
            }
            else
            {
                appSettings = new ApplicationSettings();
            }

            return appSettings;
        }


        /// <summary>
        /// Open taskt settigs file and convert instance
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static ApplicationSettings Open(string filePath)
        {
            //ApplicationSettings appSettings = null;
            //using (FileStream fileStream = File.Open(filePath, FileMode.Open))
            //{
            //    try
            //    {
            //        XmlSerializer serializer = new XmlSerializer(typeof(ApplicationSettings));
            //        appSettings = (ApplicationSettings)serializer.Deserialize(fileStream);
            //    }
            //    catch (Exception ex)
            //    {
            //        //appSettings = new ApplicationSettings();
            //        throw ex;
            //    }
            //    finally
            //    {
            //        fileStream.Close();
            //    }
            //}
            //return appSettings;

            return FileIOApplicationSettings.Open(filePath);
        }

        /// <summary>
        /// get ClientSettings as (typeof)ClientSettings
        /// </summary>
        /// <returns></returns>
        public ClientSettings GetClientSettings()
        {
            return (ClientSettings)ClientSettings;
        }

        /// <summary>
        /// get EngineSettings as (typeof)EngineSettings
        /// </summary>
        /// <returns></returns>
        public EngineSettings GetEngineSettings()
        {
            return (EngineSettings)EngineSettings;
        }

        /// <summary>
        /// get ServerSettings as (typeof)ServerSettings
        /// </summary>
        /// <returns></returns>
        public ServerSettings GetServerSettings()
        {
            return (ServerSettings)ServerSettings;
        }

        /// <summary>
        /// get LocalListenerSettings as (typeof)LocalListenerSettings
        /// </summary>
        /// <returns></returns>
        public LocalListenerSettings GetLocalListenerSettings()
        {
            return (LocalListenerSettings)ListenerSettings;
        }

        /// <summary>
        /// clone instance
        /// </summary>
        /// <returns></returns>
        public ApplicationSettings Clone()
        {
            return new ApplicationSettings(this);
        }
    }
}
