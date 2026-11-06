using System;
using System.IO;
using System.Text.Json;

namespace QuanLyKhachSan.Repositories
{
    /// <summary>Đọc cấu hình trang web tra cứu (khóa Web:BaseUrl trong appsettings.json).</summary>
    public static class CauHinhWeb
    {
        public const string MacDinh = "http://localhost:5080";

        public static string UrlGoc
        {
            get
            {
                try
                {
                    string duongDan = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                    if (!File.Exists(duongDan)) return MacDinh;
                    using (JsonDocument doc = JsonDocument.Parse(File.ReadAllText(duongDan)))
                    {
                        JsonElement web, url;
                        if (doc.RootElement.TryGetProperty("Web", out web) && web.TryGetProperty("BaseUrl", out url))
                        {
                            string s = url.GetString();
                            if (!string.IsNullOrWhiteSpace(s)) return s;
                        }
                    }
                }
                catch (JsonException) { }
                catch (IOException) { }
                return MacDinh;
            }
        }
    }
}
