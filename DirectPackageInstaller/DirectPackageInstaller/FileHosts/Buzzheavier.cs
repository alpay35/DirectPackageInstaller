using System;

namespace DirectPackageInstaller.FileHosts
{
    public class Buzzheavier : FileHostBase
    {
        public override string HostName => "Buzzheavier";
        public override bool Limited => false;

        public override DownloadInfo GetDownloadInfo(string URL)
        {
            if (!IsValidUrl(URL))
                throw new Exception("Invalid Buzzheavier URL");

            string directUrl = URL.Trim();
            if (!directUrl.Contains("/download", StringComparison.OrdinalIgnoreCase))
            {
                if (directUrl.EndsWith("/"))
                    directUrl += "download";
                else
                    directUrl += "/download";
            }

            return new DownloadInfo
            {
                Url = directUrl
            };
        }

        public override bool IsValidUrl(string URL)
        {
            return URL.Contains("buzzheavier.com", StringComparison.OrdinalIgnoreCase);
        }
    }
}
