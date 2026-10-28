using System;
using System.Linq;

namespace DirectPackageInstaller.FileHosts
{
    public class TorBox : FileHostBase
    {
        public override string HostName => "TorBox";
        public override bool Limited => false;

        public override DownloadInfo GetDownloadInfo(string URL)
        {
            if (!IsValidUrl(URL))
                throw new Exception("Invalid TorBox URL");

            // Direct download link from TorBox or API link
            return new DownloadInfo
            {
                Url = URL.Trim()
            };
        }

        public override bool IsValidUrl(string URL)
        {
            return URL.Contains("torbox.app", StringComparison.OrdinalIgnoreCase);
        }
    }
}
