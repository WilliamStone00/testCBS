using CBS.FrontDesk.Data.Entity.SavingProducts;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounts
{
    public interface IMemberAccountJob
    {
        Task<AccountMigrationCommand> ExtractFile(MemberAccountUpload model);
        Task UploadMembersAccount(AccountMigrationCommand accountMigrationCommand);
    }
}