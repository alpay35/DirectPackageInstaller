using System;
using System.Linq;
using System.Text.Json;

namespace DirectPackageInstaller.FileHosts
{
    public class GoFile : FileHostBase
    {
        public override string HostName => "GoFile";
        public override bool Limited => false;

        public override DownloadInfo GetDownloadInfo(string URL)
        {
            if (!IsValidUrl(URL))
                throw new Exception("Invalid GoFile URL");

            string cleanUrl = URL.Trim().TrimEnd('/');
            string contentId = cleanUrl.Split('/').Last();

            try
            {
                // GoFile public contents API
                string apiUrl = $"https://api.gofile.io/contents/{contentId}?wt=4fd6sg89d7s6";
                string jsonResponse = DownloadString(apiUrl);

                using var doc = JsonDocument.Parse(jsonResponse);
                var root = doc.RootElement;
                if (root.TryGetProperty("status", out var statusProp) && statusProp.GetString() == "ok")
                {
                    if (root.TryGetProperty("data", out var dataProp))
                    {
                        if (dataProp.TryGetProperty("children", out var childrenProp))
                        {
                            foreach (var child in childrenProp.EnumerateObject())
                            {
                                if (child.Value.TryGetProperty("link", out var linkProp))
                                {
                                    return new DownloadInfo
                                    {
                                        Url = linkProp.GetString() ?? URL
                                    };
                                }
                            }
                        }
                        if (dataProp.TryGetProperty("link", out var directLink))
                        {
                            return new DownloadInfo
                            {
                                Url = directLink.GetString() ?? URL
                            };
                        }
                    }
                }
            }
            catch { }

            return new DownloadInfo { Url = URL };
        }

        public override bool IsValidUrl(string URL)
        {
            return URL.Contains("gofile.io/d/", StringComparison.OrdinalIgnoreCase);
        }
    }
}
