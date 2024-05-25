
using System.Data.Entity;

namespace CBS.FrontDesk.Data.DataContext
{
    public class CBSDbContext : DbContext
    {
        public CBSDbContext() : base("name=CBSTransactionDB")
        {
        }

        public DbSet<FrontEndAuditTB> FrontEndAuditLoggs { get; set; }
    }
}