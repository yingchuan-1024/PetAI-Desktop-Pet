namespace taskt.Core
{
    /// <summary>
    /// ApplicatioSettings for Document Generation
    /// </summary>
    public class SafeDocumentGenerationApplicationSettings : SafeApplicationSettings
    {
        /// <summary>
        /// taskt client settings
        /// </summary>
        public new SafeDocumentGenerationClientSettings ClientSettings { get; private set; }

        public SafeDocumentGenerationApplicationSettings(ApplicationSettings appSettings) : base(appSettings)
        {
            ClientSettings = new SafeDocumentGenerationClientSettings(appSettings.GetClientSettings());
        }
    }
}
