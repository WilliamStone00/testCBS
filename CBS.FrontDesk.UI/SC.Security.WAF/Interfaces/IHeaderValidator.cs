using CBS.FrontDesk.UI.SC.Security.WAF.Models;
using System.Web;

namespace CBS.FrontDesk.UI.SC.Security.WAF.Services
{
    public interface IHeaderValidator
    {
        HeaderValidationResult Validate(HttpRequest request);
    }
}