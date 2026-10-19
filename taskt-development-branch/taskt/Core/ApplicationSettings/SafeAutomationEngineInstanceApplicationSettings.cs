namespace taskt.Core
{
    /// <summary>
    /// SafeApplicationSettings for AutomationEngineInstance
    /// </summary>
    public class SafeAutomationEngineInstanceApplicationSettings : SafeApplicationSettings
    {
        public new SafeAutomationEngineInstanceEngineSettings EngineSettings { get; set; }

        public SafeAutomationEngineInstanceApplicationSettings(ApplicationSettings appSettings) : base(appSettings) 
        {
            EngineSettings = new SafeAutomationEngineInstanceEngineSettings(appSettings.GetEngineSettings());
        }

        /// <summary>
        /// copy constructor
        /// </summary>
        /// <param name="appSettings"></param>
        private SafeAutomationEngineInstanceApplicationSettings(SafeAutomationEngineInstanceApplicationSettings appSettings) : this(appSettings.applicationSettings.Clone())
        {
        }

        /// <summary>
        /// clone instance
        /// </summary>
        /// <returns></returns>
        public new SafeAutomationEngineInstanceApplicationSettings Clone()
        {
            return new SafeAutomationEngineInstanceApplicationSettings(this);
        }
    }
}
