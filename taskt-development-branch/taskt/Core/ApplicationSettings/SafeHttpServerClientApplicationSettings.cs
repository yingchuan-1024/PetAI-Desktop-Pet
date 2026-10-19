namespace taskt.Core
{
    /// <summary>
    /// ApplicationSettings for HttpServerClient
    /// </summary>
    public class SafeHttpServerClientApplicationSettings : SafeApplicationSettings
    {
        /// <summary>
        /// taskt server settings
        /// </summary>
        public new SafeHttpServerClientServerSettings ServerSettings { get; private set; }

        public SafeHttpServerClientApplicationSettings(ApplicationSettings appSettings) : base(appSettings) 
        {
            ServerSettings = new SafeHttpServerClientServerSettings(appSettings.GetServerSettings());
        }
    }
}
