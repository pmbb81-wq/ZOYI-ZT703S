using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZOYI
{
    public static class UpdateChecker
    {
        private const string GITHUB_API = "https://api.github.com/repos/pmbb81-wq/ZOYI-ZT703S/releases/latest";
        private const string CURRENT_VERSION = "1.8";

        public class ReleaseInfo
        {
            public string TagName { get; set; } = "";
            public string Name { get; set; } = "";
            public string Body { get; set; } = "";
            public string HtmlUrl { get; set; } = "";
            public string PublishedAt { get; set; } = "";
            public Asset[] Assets { get; set; } = Array.Empty<Asset>();
        }

        public class Asset
        {
            public string Name { get; set; } = "";
            public string BrowserDownloadUrl { get; set; } = "";
            public long Size { get; set; }
        }

        public static async Task<ReleaseInfo?> CheckForUpdate()
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "ZOYI-UpdateChecker");
                client.Timeout = TimeSpan.FromSeconds(10);

                var json = await client.GetStringAsync(GITHUB_API);
                var release = JsonSerializer.Deserialize<ReleaseInfo>(json);

                if (release == null || string.IsNullOrEmpty(release.TagName))
                    return null;

                string remoteVersion = release.TagName.TrimStart('v');
                if (CompareVersions(remoteVersion, CURRENT_VERSION) > 0)
                    return release;

                return null;
            }
            catch
            {
                return null;
            }
        }

        private static int CompareVersions(string a, string b)
        {
            var partsA = a.Split('.');
            var partsB = b.Split('.');
            int len = Math.Max(partsA.Length, partsB.Length);

            for (int i = 0; i < len; i++)
            {
                int va = i < partsA.Length ? ParseInt(partsA[i]) : 0;
                int vb = i < partsB.Length ? ParseInt(partsB[i]) : 0;
                if (va > vb) return 1;
                if (va < vb) return -1;
            }
            return 0;
        }

        private static int ParseInt(string s)
        {
            int.TryParse(s, out int result);
            return result;
        }

        public static async Task<bool> DownloadAndRun(Asset asset, string downloadPath)
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "ZOYI-UpdateChecker");

                var data = await client.GetByteArrayAsync(asset.BrowserDownloadUrl);
                System.IO.File.WriteAllBytes(downloadPath, data);

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(downloadPath)
                {
                    UseShellExecute = true
                });

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
