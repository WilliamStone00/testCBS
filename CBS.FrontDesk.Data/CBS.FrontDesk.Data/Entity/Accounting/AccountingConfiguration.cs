using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{

    public class AccountingConfiguration
    {
        //

        public TrialBalanceFile TrialBalanceFile { get; set; }
        public List<TrialBalanceFile> TrialBalanceFiles { get; set; } = new List<TrialBalanceFile>();
   

        public AccountTreeNode AccountTreeNode { get; set; }
        public AccountType AccountType { get; set; } = new AccountType();
        public List<ChartofAccountManagementPosition> ListChartofAccountManagementPosition { get; set; } = new List<ChartofAccountManagementPosition>();
        public ChartofAccountManagementPosition ChartofAccountManagementPosition { get; set; } = new ChartofAccountManagementPosition();
        public OperationEvent OperationEvent { get; set; } = new OperationEvent();
        public OperationEventAttribute OperationEventAttribute { get; set; } = new OperationEventAttribute();
        public List<OperationEventAttributeDto> OperationEventAttributeDtos { get; set; } = new List<OperationEventAttributeDto>();
        public AccountingRuleEntry AccountingRuleEntry { get; set; } = new AccountingRuleEntry();
        public AccountingRuleEntryDto AccountingRuleEntrydto { get; set; } = new AccountingRuleEntryDto();
        public ChartOfAccount ChartOfAccount { get; set; } = new ChartOfAccount();
        public ChartOfAccountDto ChartOfAccountDto { get; set; } = new ChartOfAccountDto();
        public List<ChartOfAccountDto> ChartOfAccountDtos { get; set; } = new List<ChartOfAccountDto>();
        public List<ManagementSelectionOption> ChartofAccountManagementPositionDtos { get; set; } = new List<ManagementSelectionOption>();
        public Account Account { get; set; } = new Account();
        public List<Account> Accounts { get; set; } = new List<Account>();
        public List<ChartOfAccount> ChartOfAccounts { get; set; } = new List<ChartOfAccount>();
        public List<AccountType> AccountTypes { get; set; } = new List<AccountType>();
        public List<TrailBalanceUploud> TBuploadHistories { get; set; } = new List<TrailBalanceUploud>();
        public DocumentReferenceCode DocumentReferenceCode { get; set; } = new DocumentReferenceCode();
        public List<DocumentReferenceCodeDto> DocumentReferenceCodeDataDtos { get; set; } = new List<DocumentReferenceCodeDto>();
        public DocumentReferenceCodeDto DocumentReferenceCodeDto { get; set; } = new DocumentReferenceCodeDto();
        public CorrespondingMappingDto CorrespondingMappingDto { get; set; } = new CorrespondingMappingDto();
        public CorrespondingMapping CorrespondingMapping { get; set; } = new CorrespondingMapping();
        public IncomeStatement IncomeStatement { get; set; } = new IncomeStatement();

        public CorrespondingMappingException CorrespondingMappingException { get; set; } = new CorrespondingMappingException();
        public List<CorrespondingMappingDto> CorrespondingMappingDataDto { get; set; } = new List<CorrespondingMappingDto>();
        public List<CorrespondingMappingExceptionDto> CorrespondingMappingExceptionDataDto { get; set; } = new List<CorrespondingMappingExceptionDto>();
        public List<AccountHoDto> AccountHoDtos { get; set; } = new List<AccountHoDto>();
        public List<AccountTreeNode> AccountTreeNodes { get; set; } = new List<AccountTreeNode>();
        public List<OperationEvent> OperationEvents { get; set; } = new List<OperationEvent>();
        public List<OperationEventAttribute> OperationEventAttributes { get; set; } = new List<OperationEventAttribute>();
        public List<AccountingRuleEntry> AccountingRuleEntries { get; set; } = new List<AccountingRuleEntry>();
        public List<AccountingRule> AccountingRules { get; set; } = new List<AccountingRule>();
        public AccountingRule  AccountingRule{ get; set; } = new AccountingRule();
        public List<AccountingRuleEntryDto> AccountingRuleEntriesDTOS { get; set; } = new List<AccountingRuleEntryDto>();
        public List<AccountingRuleEntryData> AccountingRuleEntryDataDTOS { get; set; } = new List<AccountingRuleEntryData>();
        public List<Branch> Branches { get; set; } = new List<Branch>();
        public Branch Branch { get; set; }
        public List<AccountPolicy> AccountPolicies { get; set; } = new List<AccountPolicy>();
        public AccountPolicy AccountPolicy { get; set; } = new AccountPolicy();
        public List<string> BranchIds { get; set; } = new List<string>();
        public string BranchId { get; set; }
        public string ManualOperationCode { get; set; }
        public string ManualOperationName { get; set; }
        public string ServiceOption { get; set; }
        public string Action { get; set; }
        public string KEY { get; set; } = "KEY";
    }



}

