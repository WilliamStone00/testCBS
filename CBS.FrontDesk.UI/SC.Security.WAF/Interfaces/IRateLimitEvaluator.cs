using System.Threading.Tasks;

namespace CBS.FrontDesk.UI.SC.Security.WAF.Services.TSC.Security.WAF.Services
{
    public interface IRateLimitEvaluator
    {
        int GetRecentRequestCount(string ipOrUser);
        Task<bool> ShouldBlockAsync(string ipOrUser, string username, string branchId, string branchCode, string branchName, string tel, string fullName, string location = "", string lat = "", string lon = "", string city = "", string region = "", string country = "");
    }
}