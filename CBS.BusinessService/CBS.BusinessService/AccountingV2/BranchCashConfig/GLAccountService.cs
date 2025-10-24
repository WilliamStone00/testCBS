using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem; // adjust namespace if needed

namespace CBS.BusinessService.CheckManagementSystem.BranchConfiguration
{
    public class GLAccountService
    {
        public async Task<IEnumerable<GLAccount>> GetGLAccounts()
        {
            try
            {
                // Simulate async API call
                await Task.Delay(200);

                // Mock GL accounts list
                var glAccounts = new List<GLAccount>
                {
                    new GLAccount { Id = "10001", AccountCode = "10001", AccountName = "Cash in Hand" },
                    new GLAccount { Id = "10002", AccountCode = "10002", AccountName = "Cash in Vault" },
                    new GLAccount { Id = "569100", AccountCode = "569100", AccountName = "Cash Shortage Expense" },
                    new GLAccount { Id = "469100", AccountCode = "469100", AccountName = "Cash Overage Income" },
                    new GLAccount { Id = "20001", AccountCode = "20001", AccountName = "Branch Suspense Account" },
                    new GLAccount { Id = "30001", AccountCode = "30001", AccountName = "Customer Deposit Account" }
                };

                // Optional: sort and format for dropdown display
                var formattedAccounts = glAccounts
                    .Select(a =>
                    {
                        a.AccountName = $"[{a.AccountCode}] {a.AccountName}";
                        return a;
                    })
                    .OrderBy(a => a.AccountCode)
                    .ToList();

                return formattedAccounts;
            }
            catch (Exception)
            {
                // Log exception (if logger available)
                throw;
            }
        }
    }

    // Mock GLAccount model (you can replace with real one from your data layer)
    public class GLAccount
    {
        public string Id { get; set; }
        public string AccountCode { get; set; }
        public string AccountName { get; set; }
    }
}
