namespace taskt.Core
{
    /// <summary>
    /// Safe Client Settings for Document Generation
    /// </summary>
    public class SafeDocumentGenerationClientSettings : SafeClientSettings
    {
        public SafeDocumentGenerationClientSettings(ClientSettings clientSettings) : base(clientSettings) 
        {
        }

        public new bool ShowSampleUsageInDescription
        {
            get
            {
                return base.ShowSampleUsageInDescription;
            }
            set
            {
                this.clientSettings.ShowSampleUsageInDescription = value;
            }
        }

        public new bool ShowDefaultValueInDescription
        {
            get
            {
                return base.ShowDefaultValueInDescription;
            }
            set
            {
                this.clientSettings.ShowDefaultValueInDescription = value;
            }
        }
    }
}
