namespace taskt.Core
{
    /// <summary>
    /// ApplicatioSettings for WebSocket
    /// </summary>
    public class SafeWebSocketApplicationSettings : SafeApplicationSettings
    {
        /// <summary>
        /// taskt server settings
        /// </summary>
        public new SafeWebSocketServerSettings ServerSettings { get; private set; }

        public SafeWebSocketApplicationSettings(ApplicationSettings appSettings) : base(appSettings)
        {
            ServerSettings = new SafeWebSocketServerSettings(appSettings.GetServerSettings());
        }
    }
}
