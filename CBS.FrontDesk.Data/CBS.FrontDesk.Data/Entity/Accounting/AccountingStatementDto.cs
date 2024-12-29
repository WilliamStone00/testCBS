using CBS.FrontDesk.Data.Entity.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class AccountingStatementDto
    {
        public List<Account> Accounts { get; set; } = new List<Account>();
        public List<Branch> Branches { get; set; } = new List<Branch>();
        public List<AccountDto> AccountDtos { get; set; } = new List<AccountDto>();
        public List<AccountingEntryDto> AccountingEntryDtos { get; set; } = new List<AccountingEntryDto>();
        public AccountingGeneralLedgerDetails AccountingGeneralLedger { get; set; } = new AccountingGeneralLedgerDetails();
        public Branch Branch { get; set; } = new  Branch();
        public Account Account  { get; set; } = new Account();
    }
}
