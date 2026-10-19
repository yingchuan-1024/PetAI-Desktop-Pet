using System;

namespace taskt.Core
{
    /// <summary>
    /// ServerSettings for HttpServerClient
    /// </summary>
    public class SafeHttpServerClientServerSettings : SafeServerSettings
    {
        public SafeHttpServerClientServerSettings(ServerSettings serverSettings) : base(serverSettings)
        {
        }

        public new Guid HTTPGuid
        {
            get
            {
                return base.HTTPGuid;
            }
            set
            {
                this.serverSettings.HTTPGuid = value;
            }
        }

        public new string HTTPServerURL
        {
            get
            {
                return base.HTTPServerURL;
            }
            set
            {
                this.serverSettings.HTTPServerURL = value;
            }
        }
    }
}
