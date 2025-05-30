using System.Threading.Tasks;

namespace CBS.FrontDesk.UI.SC.Security.WAF.Services
{
    public interface IWAFSuspiciousPathService
    {
        Task<bool> CheckIfSuspiciousPathAsync(string path);
        bool IsSafeStaticAsset(string path, string globalUserFullname, string globalbranchname);
    }
}