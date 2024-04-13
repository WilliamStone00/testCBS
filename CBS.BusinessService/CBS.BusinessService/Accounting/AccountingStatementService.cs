using BusinessServices;
using CBS.FrontDesk.Data.Entity.Accounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting
{
    public class AccountingStatementService: BaseService
    {
        private readonly AccountingEntryServices _Service;
        private readonly AccountingServices _AccountServices;
        public AccountingStatementService()
        {
                _Service = new AccountingEntryServices();
            _AccountServices = new AccountingServices();
        }

        public async Task<List<AccountingEntryDto>> GenerateAccountingLedger(SystemQuery systemQuery)
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
                            AccountName = entry.EntryType == "DEBIT" ? drAccount?.AccountName : crAccount?.AccountName,
                            Description = entry.Description,
                            TransactionReference = entry.ReferenceID,
                            DebitAmount = entry.EntryType == "DEBIT" ? entry.DrAmount.ToString() : "0",
                            CreditAmount = entry.EntryType == "CREDIT" ? entry.CrAmount.ToString() : "0",
                            DebitAccountBalance = entry.CrCurrentBalance.ToString(),
                            CreditAccountBalance = entry.DrCurrentBalance.ToString()
                        };
           return query.ToList();
        }
        public async Task<List<BranchLiaisonLedgerEntry>> GenerateLiasonAccountBranchLiaison(SystemQuery model)
        {
            List<LiaisonLedgerEntry> accountingEntryDtos = new List<LiaisonLedgerEntry>();
            return (await _Service.RetrieveBranchLiaisonAccountingEntries(model));



        }
        public async Task<List<LiaisonLedgerEntry>> GenerateLiasonAccountLiaisonledger(SystemQuery model)
        {
            List<LiaisonLedgerEntry> accountingEntryDtos = new List<LiaisonLedgerEntry>();
            return ( await _Service.RetrieveLiasonAccountingEntries(model));
            
             
        
        }
        public async Task<List<AccountingEntryDto>> GenerateAccountingLedgerForAnumber(SystemQuery model)
        {
            List<AccountingEntry> filteredEntries = new List<AccountingEntry>();
            var accountingEntries = await _Service.RetrieveAccountingEntries(model);
            var accounts = await _AccountServices.GetAllAccounting();

            // Filter entries based on BranchId and AccountId
            //if ( IsHeadOffice())
            //{
            //    filteredEntries = (accountingEntries.Where(entry =>
            //    (model.BranchId == "XXXXXX" || entry.BranchId == model.BranchId) &&
            //    (model.AccountId == "XXXXXX" || entry.DrAccountId == model.AccountId || entry.CrAccountId == model.AccountId))).ToList();
                

            //}
            //else
            //{
            //    filteredEntries = (accountingEntries.Where(entry =>
            //    (model.BranchId == "XXXXXX" || entry.BranchId == GetBranchID()) &&
            //    (model.AccountId == "XXXXXX" || entry.DrAccountId == model.AccountId || entry.CrAccountId == model.AccountId))).ToList();


            //}
            // Project the filtered entries to AccountingEntryDto
            var query = from entry in accountingEntries
                        join drAccount in accounts on entry.DrAccountId equals drAccount.Id into drJoined
                        from drAccountData in drJoined.DefaultIfEmpty()
                        join crAccount in accounts on entry.CrAccountId equals crAccount.Id into crJoined
                        from crAccountData in crJoined.DefaultIfEmpty()
                        select new AccountingEntryDto
                        {
                            EntryDate = entry.EntryDate.Date.ToShortDateString(),
                            AccountNumber = entry.EntryType == "DEBIT" ? drAccountData?.AccountNumber : crAccountData?.AccountNumber,
                            AccountName = entry.EntryType == "DEBIT" ? drAccountData?.AccountName : crAccountData?.AccountName,
                            Description = entry.Description,
                            TransactionReference = entry.ReferenceID,
                            DebitAmount = entry.EntryType == "DEBIT" ? entry.DrAmount.ToString() : "0",
                            CreditAmount = entry.EntryType == "CREDIT" ? entry.CrAmount.ToString() : "0",
                            DebitAccountBalance = entry.CrCurrentBalance.ToString(),
                            CreditAccountBalance = entry.DrCurrentBalance.ToString()
                        };

            return query.ToList();


        }

        public async Task<List<TrialBalance4ColumnDto>> GenerateTrialBalance_4column(SystemQuery model)
        {
            List<AccountingEntry> filteredEntries = new List<AccountingEntry>();
            return await _Service.RetrieveTrialBalance4ColumnEntries(model);
        }
        public async Task<List<TrialBalance6ColumnDto>> GenerateTrialBalance_6column(SystemQuery model)
        {
            List<AccountingEntry> filteredEntries = new List<AccountingEntry>();
            return await _Service.RetrieveTrialBalance6ColumnEntries(model);
        }
        public async Task<List<ModelBalanceSheetAssets>> GenerateBalanceSheet(SystemQuery model)
        {
            List<AccountingEntry> filteredEntries = new List<AccountingEntry>();
            return await _Service.RetrieveBalanceSheetColumnEntries(model);
        }

        public async Task<List<ModelExpenses>> GenerateIncomeStatement(SystemQuery model)
        {
            List<AccountingEntry> filteredEntries = new List<AccountingEntry>();
            return await _Service.RetrieveIncomeAndExpenseEntries(model);
        }
    }
}
