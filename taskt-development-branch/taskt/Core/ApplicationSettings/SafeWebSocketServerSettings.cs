namespace taskt.Core
{
    /// <summary>
    /// ServerSettings for WebSocket
    /// </summary>
    public class SafeWebSocketServerSettings : SafeServerSettings
    {
        public SafeWebSocketServerSettings(ServerSettings serverSettings) : base(serverSettings) 
        { 
        }

        public new bool ConnectToServerOnStartup
        {
            get
            {
                return base.ConnectToServerOnStartup;
            }

            set
            {
                this.serverSettings.ConnectToServerOnStartup = value;
            }
        }

        public new bool ServerConnectionEnabled
        {
            get 
            {  
                return base.ServerConnectionEnabled; 
            }
            set
            {
                this.serverSettings.ServerConnectionEnabled = value;
            }
        }

        public new string ServerPublicKey
        {
            get
            {
                return base.ServerPublicKey;
            }
            set
            {
                this.serverSettings.ServerPublicKey = value;
            }
        }
    }
}
