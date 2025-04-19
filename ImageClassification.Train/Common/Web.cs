using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Common
{
    public class Web
    {
        public static async Task<bool> DownloadAsync(string url, string destDir, string destFileName = null)
        {
            if (string.IsNullOrWhiteSpace(destFileName))
                destFileName = url.Split('/').Last();

            Directory.CreateDirectory(destDir);

            string relativeFilePath = Path.Combine(destDir, destFileName);

            if (File.Exists(relativeFilePath))
            {
                Console.WriteLine($"{relativeFilePath} already exists.");
                return false;
            }

            Console.WriteLine($"Downloading {relativeFilePath}");

            try
            {
                using (HttpClient client = new HttpClient())
                using (HttpResponseMessage response = await client.GetAsync(url))
                using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync())
                using (Stream streamToWriteTo = File.Open(relativeFilePath, FileMode.Create))
                {
                    await streamToReadFrom.CopyToAsync(streamToWriteTo);
                }

                Console.WriteLine($"\nDownloaded {relativeFilePath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Download failed: {ex.Message}");
                return false;
            }
        }
    }
}
