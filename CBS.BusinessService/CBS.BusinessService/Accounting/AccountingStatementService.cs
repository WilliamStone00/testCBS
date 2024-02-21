using CBS.FrontDesk.Data.Entity.Accounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting
{
    public class AccountingStatementService
    {
        private readonly AccountingEntryServices _Service;
        private readonly AccountingServices _AccountServices;
        public AccountingStatementService()
        {
                _Service = new AccountingEntryServices();
            _AccountServices = new AccountingServices();
        }

        public async Task<List<AccountingEntryDto>> GenerateAccountingLedger()
        {
            List<AccountingEntryDto> accountingEntryDtos = new List<AccountingEntryDto>();
            var accountingEntries = await _Service.GetAllAccountingEntries();
            var accounts = await _AccountServices.GetAllAccounting();
            var query = from entry in accountingEntries
                        join account in accounts on entry.DrAccountId equals account.Id into drJoined
                        from drAccount in drJoined.DefaultIfEmpty()
                        join account2 in accounts on entry.CrAccountId equals account2.Id into crJoined
                        from crAccount in crJoined.DefaultIfEmpty()
                        select new AccountingEntryDto
                        {
                      
                            EntryDate = entry.EntryDate.Date.ToShortDateString(),
                           
                            AccountNumber = entry.EntryType == "DEBIT" ? drAccount?.AccountNumber : crAccount?.AccountNumber,
                            AccountName = entry.EntryType == "DEBIT" ? drAccount?.AccountHolder : crAccount?.AccountHolder,
                            Description = entry.Description,
                            TransactionReference = entry.ReferenceID,
                            DebitAmount = entry.EntryType == "DEBIT" ? entry.DrAmount.ToString() : "0",
                            CreditAmount = entry.EntryType == "CREDIT" ? entry.CrAmount.ToString() : "0",
                            DebitAccountBalance = entry.CrCurrentBalance.ToString(),
                            CreditAccountBalance = entry.DrCurrentBalance.ToString()
                        };
           return query.ToList();
        }
    }
}
