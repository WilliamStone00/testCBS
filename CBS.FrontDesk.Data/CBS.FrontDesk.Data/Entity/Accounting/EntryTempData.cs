using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{

    public class AddAccountingRuleCommand
    {
        public List<AccountingRule> AccountingRules { get; set; } //Loan Operation xxx
        public string EventName { get; set; }
        public double UnitAmount { get; set; }
        public bool IsDoubleValidationNeeded { get; set; }
        public string LevelOfExecution { get; set; }
        public List<string> ListOfEligibleBranchId { get; set; }
        public string EntryType { get; set; }
        public string Id { get; set; }
        public bool IsChainEntry { get; set; }
        public bool IsInterBranchTransaction { get; set; }
        public string AccountingEventRuleId { get; set; }
        public string Description { get; set; }

        public static AddAccountingRuleCommand BuildRequest(ManuallyJournalEntryDataSet model)
        {
            return new AddAccountingRuleCommand
            {
                EventName = model.AccountingRules[0].RuleName,
                UnitAmount = model.AccountingRules[0].UnitAmount,
                AccountingRules = BuildRequestItems(model.AccountingRules),
                IsDoubleValidationNeeded =model.AccountingRules[0].IsValidationNeed,
                LevelOfExecution = model.AccountingRules[0].LevelOfExecution,
                EntryType = model.AccountingRules[0].EntryType,
                ListOfEligibleBranchId = model.AccountingRules[0].ListOfEligibleBranchId,
                AccountingEventRuleId = model.AccountingRules[0].AccountingEventRuleId,
                Description = model.AccountingRules[0].Description,
                IsChainEntry = model.AccountingRules[0].IsChainEntry,
                IsInterBranchTransaction = model.AccountingRules[0].IsInterBranchTransaction,
                Id = "XXXXXX"
            };
        }

        private static List<AccountingRule> BuildRequestItems(List<AccountingRule> accountingRules)
        {
            List<AccountingRule> listItems = new List<AccountingRule>();
            foreach (var accountingRule in accountingRules)
            {
                accountingRule.MFI_ChartOfAccountId = accountingRule.MFI_ChartOfAccountId;
                accountingRule.System_Id = "";
                listItems.Add(accountingRule);
            }
            return listItems;
        }
    }

    public class AutomatedEventEntriesCommand
    {
        public List<EntryTempData> EntryTempDatas { get; set; }

        public bool IsDoubleValidationNeeded { get; set; }
        public bool IsInterBranchTransaction { get; set; }
        public bool IsSystem { get; set; }
        public string BranchId { get; set; }
        public string ExternalBranchId { get; set; }
        public List<string> ListOfBranchIds { get; set; }
        public string AccountingEventRuleId { get; set; }
    }
    public class AutomatedEventEntryCommand
    {
        public List<AutomatedEventEntry> Entries { get; set; }

        public string ReferenceId { get; set; }
        public string Description { get; set; }


    }

    public class AutomatedEventEntry
    {
        public string MFI_ChartOfAccountId { get; set; }
        public string BookingDirection { get; set; }

        public decimal Amount { get; set; }
        public string System_Id { get; set; }
    }
    public class EntryTempData
    {
        public string Id { get; set; }
        [Required]
        public DateTime ValueDate { get; set; } = DateTime.Now;
        public string Reference { get; set; }
        public string AccountBalance { get; set; }
        public string AccountId { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        [Required]
        public string BookingDirection { get; set; }
        [PositiveAmountValidator]
        public string Amount { get; set; }
        public string Credit { get; set; }
        public string Debit { get; set; }
        public string Description { get; set; }
        public string AccountingEventId { get; set; } = "MANUAL USER";
        public string BranchId { get; set; }
        public string ExternalBranchId { get; set; }
        public bool IsInterBranchTransaction { get; set; }
        public static AccountingEntryPayloadCommand ConvertToAccountingEntryPayloadCommand(List<EntryTempData> entries,string branchId)
        {
            List<EntryTempDatas> accountingEntries = new List<EntryTempDatas>();
            foreach (var Item in entries)
            {
                accountingEntries.Add(new EntryTempDatas
                {
                    Id = "012",
                    AccountId = Item.AccountId,
                    AccountName = Item.AccountName,
                    AccountNumber = Item.AccountNumber,
                    AccountingEventId = Item.AccountingEventId,
                    BookingDirection = Item.BookingDirection,
                    BranchId = branchId,
                    Amount = Convert.ToDecimal(Item.Amount),
                    AccountBalance =  Item.AccountBalance??"0",
                    Description = Item.Description,
                    ExternalBranchId = branchId,
                    Reference = Item.Reference,
                    ValueDate = Item.ValueDate
    });
            }
            return new AccountingEntryPayloadCommand
            { 
                EntryTempDatas = accountingEntries, 
                BranchId = branchId,
                ExternalBranchId = branchId,
                AccountingEventRuleId = null, 
                IsDoubleValidationNeeded = true,
                IsInterBranchTransaction = false,
                IsSystem = false,
                ListOfBranchIds = new List<string> { branchId}
            };
        }


    }
    public class EntryDescription
    {
        [Required]
        public string Description { get; set; }
        [Required]
        public string Reference { get; set; }
    }
    public class EntryTempDatas
    {
        public string Id { get; set; }
        public string AccountId { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public DateTime ValueDate { get; set; }
        public string BookingDirection { get; set; }

        public decimal Amount { get; set; }
        public string AccountBalance { get; set; }
        public string Description { get; set; }
        public string AccountingEventId { get; set; } = "MANUAL USER";
        public string Reference { get; set; }
        public string BranchId { get; set; }
        public string ExternalBranchId { get; set; }
    }
    public class AccountingEntryPayloadCommand
    {
        public List<EntryTempDatas> EntryTempDatas { get; set; }
        public bool IsDoubleValidationNeeded { get; set; }  
        public bool IsSystem { get; set; }
        public string BranchId { get; set; }
        public string AccountingEventRuleId { get; set; }
        public List<string> ListOfBranchIds { get; set; }
        public string ExternalBranchId { get; set; }
        public bool? IsInterBranchTransaction { get; set; }
    }
    public class EventEntryResponse
    {
        public string ResponseMessge { get; set; }
        public bool Status { get; set; }

    }
    public class EntryApproval
    {

        public string Id { get; set; }

        public bool HasApproved { get; set; }
        public bool ValidationIsNotRequired { get; set; }
        public string BranchId { get; internal set; }
    }
    public class EntryTempDataResult
    {

        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public decimal SumDebit { get; set; }
        public decimal SumCredit { get; set; }
        public string BookingDirection { get; set; }
        public decimal Difference { get; set; }
        public decimal Amount { get; set; }
        public string Reference { get; set; }
        public string Id { get; set; }
    }
    public class AccountingEntrie
    {
        public string AccountNumber { get; set; }
        public double Amount { get; set; }
        public string BookingDirection { get; set; }
        public string Description { get; set; }
        public string MFI_ChartOfAccountId { get; set; }
    }
    public class AccountingEventRule : BaseEntity
    {
        public string Id { get; set; }
        public List<AccountingRule> AccountingRules { get; set; } //Loan Operation xxx
        public List<string> ListOfEligibleBranchId { get; set; }
        public string EventName { get; set; }
        public double UnitAmount { get; set; }
        public string Description { get; set; }
        public bool IsDoubleValidationNeeded { get; set; }
        public string LevelOfExecution { get; set; }
        public string EntryType { get; set; }
        public bool IsChainEntry { get; set; }
        public string AccountingEventRuleId { get; set; }
        public bool IsInterBranchTransaction { get; set; }
        public class AccountingRule
        {
            public string Id { get; set; }
            public string MFI_ChartOfAccountId { get; set; }
            public string BookingDirection { get; set; }
            //public string System_Id { get; internal set; }
        }
   
        public static AccountingEventRule BuildRequest(ManuallyJournalEntryDataSet model)
        {
            return new AccountingEventRule
            {
                EventName = model.AccountingRules[0].RuleName,
                UnitAmount = model.AccountingRules[0].UnitAmount,
                AccountingRules = BuildRequestItems(model.AccountingRules),
                IsDoubleValidationNeeded = model.AccountingRules[0].IsValidationNeed,
                LevelOfExecution = model.AccountingRules[0].LevelOfExecution,
                EntryType = model.AccountingRules[0].EntryType,
                ListOfEligibleBranchId = model.AccountingRules[0].ListOfEligibleBranchId,
                AccountingEventRuleId = model.AccountingRules[0].AccountingEventRuleId,
                Description = model.AccountingRules[0].Description,
                IsChainEntry = model.AccountingRules[0].IsChainEntry,
                IsInterBranchTransaction = model.AccountingRules[0].IsInterBranchTransaction,
                Id = "XXXXXX"
            };
        }

        private static List<AccountingEventRule.AccountingRule> BuildRequestItems(List<Accounting.AccountingRule> accountingRules)
        {
            List<AccountingEventRule.AccountingRule> listItems = new List<AccountingEventRule.AccountingRule>();
            foreach (var accountingRule in accountingRules)
            {
              
                accountingRule.MFI_ChartOfAccountId = accountingRule.MFI_ChartOfAccountId;
                accountingRule.BookingDirection = accountingRule.BookingDirection;
                accountingRule.Id = "";
                listItems.Add(new AccountingEventRule.AccountingRule { BookingDirection=accountingRule.BookingDirection,MFI_ChartOfAccountId=accountingRule.MFI_ChartOfAccountId,Id=accountingRule.Id});
            }
            return listItems;
        }
    }

    public class ManualJournalEntryRequest
    {
        public string Description { get; set; }
        public List<AccountingRuleEntry> AccountingRules { get; set; }
        public string Reference { get; set; }
        public string AccountingEventId { get; set; }

        public class AccountingRuleEntry
        {
            public string AccountDescription { get; set; }
            public decimal Debit { get; set; }
            public decimal Credit { get; set; }
        }

        public static List<AccountModel> ConvertToAccountModelData(ManualJournalEntryRequest model)
        {
            List<AccountModel> accountModels = new List<AccountModel>();
            foreach (var item in model.AccountingRules)
            {
                var splitItems = item.AccountDescription.Split('-');
                var number = splitItems.Length;
                if (number > 3)
                {
                    accountModels.Add(new AccountModel
                    {
                        Id = splitItems[splitItems.Length - 1],
                        AccountName = splitItems[0] + " " + splitItems[1],
                        AccountNumber = splitItems[2],
                        BranchCode = splitItems[2].Substring(splitItems[2].Length-3),
                        Amount = item.Credit > item.Debit ? item.Credit : item.Debit,
                        BookingDirection = item.Credit > item.Debit ? "CREDIT" : "DEBIT",
                        Description = model.Description,
                        Reference = model.Reference,
                        AccountingEventId = model.AccountingEventId
                    });

                }
                else
                {
                    accountModels.Add(new AccountModel
                    {
                        Id = splitItems[2],
                        AccountName = splitItems[0],
                        AccountNumber = splitItems[1],
                        Amount = item.Credit > item.Debit ? item.Credit : item.Debit,
                        BookingDirection = item.Credit > item.Debit ? "CREDIT" : "DEBIT",
                        Description = model.Description,
                        Reference = model.Reference,
                        AccountingEventId = model.AccountingEventId
                    });
                }

            }
            return accountModels;
        }
    }
    public class AccountModel
    {
        public string Id { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public decimal Amount { get; set; }
        public string BookingDirection { get; set; }
        public string Description { get; set; }
        public string Reference { get; set; }
        public string AccountingEventId { get; set; }
        public string BranchCode { get; set; }
    }

    public class ManuallyJournalEntryDataSet
    {
        public EntryTempData EntryTempData { get; set; } = new EntryTempData();
        public Data.Account Account { get; set; } = new Account();
        public EntryDescription EntryDescription { get; set; } = new EntryDescription();
        public List<EntryTempData> EntryTempDatas { get; set; } = new List<EntryTempData>() { };
        public List<AccountingRuleDtos> AccountingRuleDtos = new List<AccountingRuleDtos>();
        public List<PostedEntry> PostedEntries { get; set; } = new List<PostedEntry>();
        public List<EntryTempDataResult> EntryTempDataResult { get; set; } = new List<EntryTempDataResult>
        {

        };
        public List<Account> Accounts { get; set; } = new List<Account>();
        public List<AccountingRule> AccountingRules { get; set; } = new List<AccountingRule>();
        public AccountingRule AccountingRule { get; set; } = new AccountingRule();
        public AccountingEventRule AccountingEventRule { get; set; } = new AccountingEventRule();
        public List<AccountingEventRule> AccountingEventRules { get; set; } = new List<AccountingEventRule>();
        public string ServiceOption { get; set; }
        public string Action { get; set; }
        public string Key { get; set; }
        public bool HasApproved { get; set; }
    }
}
