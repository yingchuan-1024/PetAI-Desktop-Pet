namespace taskt.Core
{
    /// <summary>
    /// SafeClientSettings for frmScriptBuilder
    /// </summary>
    public class SafeFrmScriptBuilderClientSettings : SafeClientSettings
    {
        public SafeFrmScriptBuilderClientSettings(ClientSettings clientSettings) : base(clientSettings) 
        {
        }

        public new bool InsertCommandsInline
        {
            get
            {
                return base.InsertCommandsInline;
            }
            set
            {
                this.clientSettings.InsertCommandsInline = value;
            }
        }

        public new bool ShowCommandSearchBar
        {
            get
            {
                return base.ShowCommandSearchBar;
            }
            set
            {
                this.clientSettings.ShowCommandSearchBar = value;
            }
        }
    }
}
