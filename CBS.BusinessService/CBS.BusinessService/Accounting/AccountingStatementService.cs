using BusinessServices;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
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
        private readonly BranchServices _BranchServices;
        public AccountingStatementService()
        {
                _Service = new AccountingEntryServices();
            _AccountServices = new AccountingServices();
            _BranchServices= new BranchServices();
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
                            Reference = entry.ReferenceID,
                            Debit  = entry.EntryType == "DEBIT" ? entry.DrAmount.ToString() : "0",
                            Credit = entry.EntryType == "CREDIT" ? entry.CrAmount.ToString() : "0",
                            EntryDateTime = entry.EntryDate.ToString(),
                            //CreditAccountBalance = entry.DrCurrentBalance.ToString()
                        };
            return query.OrderByDescending(n => n.EntryDateTime).ToList();
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
            try
            {
                List<AccountingEntry> filteredEntries = new List<AccountingEntry>();
                var accountingEntries = await _Service.RetrieveAccountingEntries(model);
                var accounts = await _AccountServices.GetAllAccounting();
                var query = from entry in accountingEntries
                            join drAccount in accounts on entry.AccountId equals drAccount.Id                   
                            select new AccountingEntryDto
                            {
                                EntryDate = entry.EntryDate.ToString(),
                                AccountNumber = drAccount.AccountNumberCU,
                                AccountName = drAccount.AccountName,
                                Description = entry.Description,
                                 Reference = entry.ReferenceID,
                                Debit = entry.DrAmount.ToString("N") ,
                                Credit  = entry.CrAmount.ToString("N")
                                //DebitAccountBalance = entry.CrCurrentBalance.ToString(),
                                //CreditAccountBalance = entry.DrCurrentBalance.ToString()
                            };

                return query.OrderBy(n=>n.Reference).ToList();
            }
            catch (Exception ex)
            {

                throw;
            }


        }


        public async Task<List<AccountDto>> GenerateLiaisonLedgerForBranch(string BranchId)
        {
            try
            {
                List<AccountDto> filteredEntries = new List<AccountDto>();
                var letterHead = await _BranchServices.GetBranch(BranchId);
                var accounts = await _AccountServices.GetAllAccounting();
                var entries = accounts.Where(x => x.Account3 == "451" && x.AccountOwnerId == BranchId).ToList();
                var query = from entry in entries

                            select new AccountDto
                            {
     
                                AccountNumber = entry.AccountNumberCU,
                                AccountName = entry.AccountName,
                               
                                DebitBalance =entry.DebitBalance.ToString(),
                                CreditBalance =  entry.CreditBalance.ToString(),
                                CurrentBalance = entry.CurrentBalance.ToString(),
                                //CreditAccountBalance = entry.DrCurrentBalance.ToString()
                            };

                return query.ToList();
            }
            catch (Exception ex)
            {

                throw;
            }


        }

        public async Task<List<AccountLedgerDto>> GenerateAccountLedger(GLQuery model)
        {
            try
            {
                List<AccountLedgerDto> AccountLedgerDtoList = new List<AccountLedgerDto>();
                List<Account> query = new List<Account>();
                var letterHead = await _BranchServices.GetBranch(model.BranchId);
                var accounts =( await _AccountServices.GetAllAccounting()).Where(n=>n.AccountOwnerId==letterHead.Id);
                if (_AccountServices.IsHeadOffice() && model.BranchId == "XXXXXX")
                {
                     query = accounts.ToList();

                }
                else
                {

                    query = accounts.ToList();
                }

                foreach (var account in query) 
                {
                    AccountLedgerDtoList.Add(BuildLedgerAccount(account, letterHead));
                }
              var GL =  AccountLedgerDtoList.Where(br => br.BranchId == letterHead.Id);
                return GL.OrderBy(n => n.AccountNumber).ToList();
            }
            catch (Exception ex)
            {

                throw;
            }


        }

      
        public async Task<List<JournalEntryDto>> GenerateJournalEntry(JEQuery model)
        {
            try
            {
                List<JournalEntryDto> AccountLedgerDtoList = new List<JournalEntryDto>();
                List<AccountingEntryDto> query = new List<AccountingEntryDto>();
                var letterHead = await _BranchServices.GetBranch(model.BranchId);
                var accountingEntries = await _Service.RetrieveAccountingEntries(model);
             

                foreach (var account in accountingEntries)
                {
                    AccountLedgerDtoList.Add(BuildJournalEntry(account, letterHead, model));
                }

                return AccountLedgerDtoList.OrderBy(n => n.AccountNumber).ToList();
            }
            catch (Exception ex)
            {

                throw;
            }


        }

        private AccountLedgerDto BuildLedgerAccount(Account account, Branch model)
        {
            AccountLedgerDto dto = new AccountLedgerDto();
            dto.AccountNumber=account.AccountNumberCU;
            dto.AccountName=account.AccountName;
            dto.CurrentBalance = Convert.ToDecimal( account.CurrentBalance);
            dto.Address = model.Address;
            dto.BranchLocation=model.Location;
            dto.Location=model.Location;
            dto.Capital=model.Capital;
            dto.WebSite=model.WebSite;
            dto.BranchTelephone=model.Telephone;
            dto.ImmatriculationNumber=model.ImmatriculationNumber;
            dto.Name=model.Bank.Name;
            dto.BranchName=model.Name;
            dto.BranchId=model.Id;
            dto.FromDate = DateTime.Now.ToString("yyyy-MM-dd");
            dto.ToDate = DateTime.Now.ToString("yyyy-MM-dd");

            return dto;
        }
        private JournalEntryDto BuildJournalEntry(AccountingEntry account, Branch model, JEQuery query)
        {
            JournalEntryDto dto = new JournalEntryDto();
            dto.AccountNumber = account.AccountNumber.PadRight(6, '0');
            dto.Description = account.Description;
            dto.Debit  = account.DrAmount.ToString();
            dto.Credit = account.CrAmount.ToString();
            dto.Reference = account.ReferenceID.ToString();
            dto.EntryDate =account.EntryDate.ToString();
                
        dto.Address = model.Address;
            dto.BranchLocation = model.Location;
            dto.Location = model.Location;
            dto.Capital = model.Capital;
            dto.WebSite = model.WebSite;
            dto.BranchTelephone = model.Telephone;
            dto.ImmatriculationNumber = model.ImmatriculationNumber;
            dto.Name = model.Bank.Name;
            dto.BranchName = model.Name;
            dto.FromDate = query.FromDate.ToString("yyyy-MM-dd");
            dto.ToDate = query.ToDate.ToString("yyyy-MM-dd");
            return dto;
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
        public async Task<BalanceSheetData> GenerateBalanceSheet(BSQuery model)
        {
  
            return await _Service.GetBalanceSheetDataEntries(model);
        }

        public async Task<List<ModelExpenses>> GenerateIncomeStatement(SystemQuery model)
        {
            List<AccountingEntry> filteredEntries = new List<AccountingEntry>();
            return await _Service.RetrieveIncomeAndExpenseEntries(model);
        }
    }
}
