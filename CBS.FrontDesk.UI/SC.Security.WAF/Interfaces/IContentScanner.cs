using CBS.FrontDesk.UI.SC.Security.WAF.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CBS.FrontDesk.UI.SC.Security.WAF.Interfaces
{
    public interface IContentScanner
    {
        ContentScanResultDto Scan(HttpRequest request);
        Task<ContentScanResultDto> ScanMultipartAsync(HttpRequest request);
    }
}
