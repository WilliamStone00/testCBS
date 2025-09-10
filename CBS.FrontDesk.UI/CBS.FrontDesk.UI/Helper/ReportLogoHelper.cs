using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using CrystalDecisions.CrystalReports.Engine;

namespace CBS.FrontDesk.UI.Helper
{


    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using CrystalDecisions.CrystalReports.Engine;

    public static class ReportImageHelper
    {
        public static async Task SetReportImagesAsync(ReportDocument rd, IDictionary<string, string> urlMap)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11| SecurityProtocolType.Tls;

            using (var http = new HttpClient() { Timeout = TimeSpan.FromSeconds(20) })
            {
                foreach (var kv in urlMap)
                {
                    var param = kv.Key;                // e.g. "LogoPath"
                    var url = kv.Value;              // https://...

                    if (string.IsNullOrWhiteSpace(url)) continue;

                    var uri = new Uri(Uri.EscapeUriString(url));
                    var bytes = await http.GetByteArrayAsync(uri);

                    var ext = Path.GetExtension(uri.AbsolutePath);
                    if (string.IsNullOrEmpty(ext)) ext = ".png";

                    var tempPath = Path.Combine(Path.GetTempPath(), $"{param}_{Guid.NewGuid()}{ext}");
                    // ↓ use async-friendly write that works on older frameworks
                    using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, true))
                    {
                        await fs.WriteAsync(bytes, 0, bytes.Length);
                        await fs.FlushAsync();
                    }

                    rd.SetParameterValue(param, tempPath);
                    foreach (ReportDocument sr in rd.Subreports)
                    {
                        try { sr.SetParameterValue(param, tempPath); } catch { }
                    }
                }
            }
        }
    }

}