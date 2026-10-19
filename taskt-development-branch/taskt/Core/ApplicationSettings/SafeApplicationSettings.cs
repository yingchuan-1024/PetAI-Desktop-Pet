namespace taskt.Core
{
    /// <summary>
    /// Safe Application Settings, Most parameters are readonly
    /// </summary>
    public class SafeApplicationSettings : IApplicationSettings
    {
        /// <summary>
        /// appSettings for protect
        /// </summary>
        protected readonly ApplicationSettings applicationSettings;

        public IClientSettings ClientSettings { get; private set; }

        public IEngineSettings EngineSettings { get; private set; }

        public IServerSettings ServerSettings { get; private set; }

        public ILocalListenerSettings ListenerSettings { get; private set; }

        public SafeApplicationSettings(ApplicationSettings appSettings) 
        { 
            this.applicationSettings = appSettings;

            this.ClientSettings = new SafeClientSettings(appSettings.GetClientSettings());
            this.EngineSettings = new SafeEngineSettings(appSettings.GetEngineSettings());
            this.ServerSettings = new SafeServerSettings(appSettings.GetServerSettings());
            this.ListenerSettings = new SafeLocalListenerSettings(appSettings.GetLocalListenerSettings());
        }

        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="appSettings"></param>
        private SafeApplicationSettings(SafeApplicationSettings appSettings) : this(appSettings.applicationSettings.Clone())
        {
            //this.applicationSettings = appSettings.applicationSettings.Clone();

            //this.ClientSettings = new SafeClientSettings(this.applicationSettings.GetClientSettings());
            //this.EngineSettings = new SafeEngineSettings(this.applicationSettings.GetEngineSettings());
            //this.ServerSettings = new SafeServerSettings(this.applicationSettings.GetServerSettings());
            //this.ListenerSettings = new SafeLocalListenerSettings(this.applicationSettings.GetLocalListenerSettings());
        }

        /// <summary>
        /// get ClientSettings As SafeClientSettings
        /// </summary>
        /// <returns></returns>
        public SafeClientSettings GetClientSettings()
        {
            return (SafeClientSettings)ClientSettings;
        }

        /// <summary>
        /// Get EngineSettings as SafeEngineSettings
        /// </summary>
        /// <returns></returns>
        public SafeEngineSettings GetEngineSettings()
        {
            return (SafeEngineSettings)EngineSettings;
        }

        /// <summary>
        /// get ServerSettings as SafeServerSettings
        /// </summary>
        /// <returns></returns>
        public SafeServerSettings GetServerSettings()
        {
            return (SafeServerSettings)ServerSettings;
        }

        /// <summary>
        /// get LocalListenerSettings as SafeLocalListenerSettings
        /// </summary>
        /// <returns></returns>
        public SafeLocalListenerSettings GetLocalListenerSettings()
        {
            return (SafeLocalListenerSettings)ListenerSettings;
        }

        /// <summary>
        /// clone instance
        /// </summary>
        /// <returns></returns>
        public SafeApplicationSettings Clone()
        {
            return new SafeApplicationSettings(this);
        }
    }
}
