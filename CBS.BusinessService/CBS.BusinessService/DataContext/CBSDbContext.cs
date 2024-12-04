
using System.Data.Entity;

namespace CBS.FrontDesk.Data.DataContext
{
    public class CBSDbContext : DbContext
    {
        public CBSDbContext() : base("Name=CBSTransactionDB")
        {
        }

        public DbSet<FrontEndAuditTB> FrontEndAuditLoggs { get; set; }

        public System.Data.Entity.DbSet<CBS.BusinessService.ThirdPartyBankAccount.ThirdPartyInstitution> ThirdPartyInstitutions { get; set; }
    }
}