using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounts;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using Microsoft.Ajax.Utilities;
using MvcSiteMapProvider.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.TransactionManagement
{
    [CheckSessionTimeOutAttribute]

    public class TransactionConfigurationController : BaseController
    {
        // GET: TransactionConfiguration
        // GET: Tax
        private readonly SavingProductServices _savingProductServices;
        private readonly TellerServices _tellerServices;
        private readonly DepositLimitServices _depositLimitServices;
        private readonly TransferLimitServices _transferLimitServices;
        private readonly WithdrawalLimitServices _withdrawalLimitServices;
        private readonly ChartOfAccountServicesAnnex _accountingServices;
        private readonly ManagementFeeParameterServices _managementFeeParameterServices;
        private readonly ReopenFeeParameterServices _reopenFeeParameterServices;
        private readonly CloseFeeParameterServices _closeFeeParameterServices;
        private readonly EntryFeeParameterServices _entryFeeParameterServices;
        private readonly OperationFeeServices _operationFeeServices;
        private readonly SavingProductFeeServices _savingProductFeeServices;
        private readonly AccountingEntryRuleService _accountingEntryRuleService;
        private readonly ChartOfAccountManagementPositionService _accountConfiServices;
        private  bool SavingDepositProductIsPayable=false;
        private bool SavingWithdrawalProductIsPayable = false;
        private   List<ChartofAccountManagementPosition> ListOfChartOfAccount = new List<ChartofAccountManagementPosition>();
        public TransactionConfigurationController(SavingProductServices savingProductServices, ChartOfAccountServicesAnnex accountingServices, TellerServices tellerServices, DepositLimitServices depositLimitServices, TransferLimitServices transferLimitServices, WithdrawalLimitServices withdrawalLimitServices, AccountingEntryRuleService accountingEntryRuleService, ChartOfAccountManagementPositionService accountServices, ManagementFeeParameterServices managementFeeParameterServices = null, ReopenFeeParameterServices reopenFeeParameterServices = null, CloseFeeParameterServices closeFeeParameterServices = null, EntryFeeParameterServices entryFeeParameterServices = null, OperationFeeServices operationFeeServices = null, SavingProductFeeServices savingProductFeeServices = null)
        {
            _savingProductServices = savingProductServices;
            _accountingServices = accountingServices;
            _tellerServices = tellerServices;
            _depositLimitServices = depositLimitServices;
            _transferLimitServices = transferLimitServices;
            _withdrawalLimitServices = withdrawalLimitServices;
            _managementFeeParameterServices = managementFeeParameterServices;
            _reopenFeeParameterServices = reopenFeeParameterServices;
            _closeFeeParameterServices = closeFeeParameterServices;
            _entryFeeParameterServices = entryFeeParameterServices;
            _operationFeeServices = operationFeeServices;
            _savingProductFeeServices = savingProductFeeServices;
            _accountingEntryRuleService = accountingEntryRuleService;
            _accountConfiServices = accountServices;
        }

        public async Task<ActionResult> Index()
        {
            var savingProduct = await _savingProductServices.GetSavingProducts();
            return View(new SavingConfiguration { SavingProducts = savingProduct.ToList() });
        }
        public async Task<ActionResult> OrdinaryAccounts()
        {
            var savingProduct = await _savingProductServices.GetSavingProducts();
            return View(new SavingConfiguration { SavingProducts = savingProduct.ToList() });
        }
        //
        public async Task<ActionResult> PolicyandSharing(string Key, string serviceOption = null)
        {
            await GetListPolicies();
            var savingProduct = await _savingProductServices.GetSavingProduct(Key);
            var deposit = new DepositLimit { ProductId = savingProduct.Id };
            var transfer = new TransferLimit { productId = savingProduct.Id };
            var withdrawal = new WithdrawalLimit { ProductId = savingProduct.Id };
            return View(new SavingConfiguration { SavingProduct = savingProduct, DepositLimit = deposit, TransferLimit = transfer, WithdrawalLimit = withdrawal });
        }
        public async Task<ActionResult> AccountMapping(string Key, string serviceOption = null)
        {

            await GetChartOfAccounts();
            var savingProduct = await _savingProductServices.GetSavingProduct(Key);
            ViewBag.Fees = await _operationFeeServices.GetFees();
            await GetEventNames();
            ViewBag.FeeType = SavingProductFee.GetFeeTypeList();
            var savingProductFees = await _savingProductFeeServices.GetSavingProductFees(Key);
            return View(new SavingConfiguration { SavingProduct = savingProduct, SavingProductFee = new SavingProductFee { SavingProductId = savingProduct.Id }, SavingProductFees = savingProductFees.ToList() });


        }
        //public async Task<ActionResult> AccountMappingInfo(string Key)
        //{

        //    List<ChartofAccountInfo> chartofAccounts = new List<ChartofAccountInfo>();
        //    var savingProduct = await _savingProductServices.GetSavingProduct(Key);
        //    @ViewBag.ProductName = savingProduct.Name+ " Product";
        //    @ViewBag.ProductType = savingProduct.Name + " Product";
        //    var data = await _accountingEntryRuleService.GetAccountingEntryRules();
        //    var accountingRuleEntries = data.Where(x=>x.EventCode.Contains(Key)).ToList();
        //    var accountingConfigList= (await _accountConfiServices.GetChartOfAccountManagementPositions()).ToList();
        //    var rootAccount = accountingConfigList.Where(c => c.Description == "ROOT ACCOUNT").FirstOrDefault();
        //    var PhysicalTellAccount = data.Where(e=>e.EventCode.Equals("Physical_Teller")).FirstOrDefault();
        //    var accountList = accountingRuleEntries.Where(e => e.EventCode.Contains(Key));
        //    foreach (var item in accountList)
        //    {
        //        ChartofAccountInfo model = new ChartofAccountInfo();
        //        ChartofAccountInfo modelSource = new ChartofAccountInfo();
        //        var account = accountingConfigList.Find(x => x.Id.Equals(item.DeterminationAccountId));
        //        var Sourceaccount = accountingConfigList.Find(x => x.Id.Equals(PhysicalTellAccount.DeterminationAccountId));
        //        if (item.EventCode.Contains("Commission_Account"))
        //        {
        //            if (item.DeterminationAccountId==rootAccount.Id)
        //            {
        //                model = new ChartofAccountInfo {OperationPeriod="Local", OperationType = "DEPOSIT", Id = "XXXXXX", AccountName = "XXXXXXXX", AccountNumber = "XXXXXXXX", BookingDirection = "CREDIT" ,Amount="0"};
        //            }
        //            else
        //            {
        //                model = new ChartofAccountInfo { OperationPeriod = "Local", OperationType = "DEPOSIT", Id = item.DeterminationAccountId, AccountName = account.Description, AccountNumber = account.AccountNumber + "[BranchCode]" + account.PositionNumber, BookingDirection = "CREDIT", Amount = "200" };
        //            }
        //            chartofAccounts.Add(model);
        //        }
        //        else if (item.EventCode.Contains("Liasson_Account"))
        //        {
        //            if (item.DeterminationAccountId == rootAccount.Id)
        //            {
        //                model = new ChartofAccountInfo {OperationPeriod = "Local", OperationType = "DEPOSIT", Id = "XXXXXX", AccountName = "XXXXXXXX", AccountNumber = "XXXXXXXX", BookingDirection = "CREDIT", Amount = "0" };
        //            }
        //            else
        //            {
        //                model = new ChartofAccountInfo { OperationPeriod = "Local", OperationType = "DEPOSIT", Id = item.DeterminationAccountId, AccountName = account.Description, AccountNumber = account.AccountNumber + "[SourceBranchCode][DestinationBranchCode]" , BookingDirection = "CREDIT", Amount = "200" };
        //            }
        //            chartofAccounts.Add(model);
        //        }
        //        else if (item.EventCode.Contains("Principal_Saving_Account"))
        //        {
        //            if (item.DeterminationAccountId == rootAccount.Id)
        //            {
        //                model = new ChartofAccountInfo {OperationPeriod = "Local", OperationType = "DEPOSIT", Id = "XXXXXX", AccountName = "XXXXXXXX", AccountNumber = "XXXXXXXX", BookingDirection = "CREDIT", Amount = "0" };
        //            }
        //            else
        //            {
        //                model = new ChartofAccountInfo {OperationPeriod = "Local", OperationType = "DEPOSIT", Id = item.DeterminationAccountId, AccountName = account.Description, AccountNumber = account.AccountNumber + "[BranchCode]" + account.PositionNumber, BookingDirection = "CREDIT", Amount = "10000" };
        //               // modelSource = new ChartofAccountInfo { OperationType = "DEPOSIT", Id = item.Id, AccountName = accountingConfigList.Find(x => x.Id.Equals(item.Id)).Description, AccountNumber = accountingConfigList.Find(x => x.Id.Equals(item.Id)).AccountNumber + "[BranchCode]" + accountingConfigList.Find(x => x.Id.Equals(item.Id)).PositionNumber, BookingDirection = "DEBIT", Amount = "10200" };
        //                modelSource = new ChartofAccountInfo {OperationPeriod = "Local", OperationType = "DEPOSIT", Id = item.DeterminationAccountId, AccountName = Sourceaccount.Description, AccountNumber = Sourceaccount.AccountNumber + "[BranchCode]" + Sourceaccount.PositionNumber, BookingDirection = "CREDIT", Amount = "10200" };

        //                chartofAccounts.Add(modelSource);
        //            }
        //            chartofAccounts.Add(model);
        //        }
        //        else if (item.EventCode.Contains("Saving_Interest_Account"))
        //        {

        //            if (item.DeterminationAccountId == rootAccount.Id)
        //            {
        //                model = new ChartofAccountInfo { OperationPeriod = "Local", OperationType = "DEPOSIT", Id = "XXXXXX", AccountName = "XXXXXXXX", AccountNumber = "XXXXXXXX", BookingDirection = "CREDIT", Amount = "0" };
        //            }
        //            else
        //            {
        //                model = new ChartofAccountInfo {OperationPeriod = "Local", OperationType = "DEPOSIT", Id = item.Id, AccountName = account.Description, AccountNumber = account.AccountNumber + "[BranchCode]" + account.PositionNumber, BookingDirection = "CREDIT", Amount = "200" };
        //            }
        //        }
        //        else if (item.EventCode.Contains("Saving_Interest_Expense_Account"))
        //        {
        //            if (item.DeterminationAccountId == rootAccount.Id)
        //            {
        //                model = new ChartofAccountInfo { OperationPeriod = "Local", OperationType = "DEPOSIT", Id = "XXXXXX", AccountName = "XXXXXXXX", AccountNumber = "XXXXXXXX", BookingDirection = "CREDIT", Amount = "0" };
        //            }
        //            else
        //            {
        //                model = new ChartofAccountInfo {OperationPeriod = "Local", OperationType = "DEPOSIT", Id = item.Id, AccountName = account.Description, AccountNumber = account.AccountNumber + "[BranchCode]" + account.PositionNumber, BookingDirection = "CREDIT", Amount = "200" };
        //            }
        //            chartofAccounts.Add(model);
        //        }
        //        else if (item.EventCode.Contains("Withdrawal_Fee_Account"))
        //        {
        //            if (item.DeterminationAccountId == rootAccount.Id)
        //            {
        //                model = new ChartofAccountInfo { OperationPeriod = "Local", OperationType = "DEPOSIT", Id = "XXXXXX", AccountName = "XXXXXXXX", AccountNumber = "XXXXXXXX", BookingDirection = "CREDIT", Amount = "0" };
        //            }
        //            else
        //            {
        //                model = new ChartofAccountInfo {OperationPeriod = "Local", OperationType = "DEPOSIT", Id = item.Id, AccountName = account.Description, AccountNumber = account.AccountNumber + "[BranchCode]" + account.PositionNumber, BookingDirection = "CREDIT", Amount = "200" };
        //            }
        //            chartofAccounts.Add(model);
        //        }
        //        else if (item.EventCode.Contains("Transfer_Fee_Account"))
        //        {
        //            if (item.DeterminationAccountId == rootAccount.Id)
        //            {
        //                model = new ChartofAccountInfo { OperationPeriod = "Local", OperationType = "DEPOSIT", Id = "XXXXXX", AccountName = "XXXXXXXX", AccountNumber = "XXXXXXXX", BookingDirection = "CREDIT", Amount = "0" };
        //            }
        //            else
        //            {
        //                model = new ChartofAccountInfo {OperationPeriod = "Local", OperationType = "DEPOSIT", Id = item.Id, AccountName = account.Description, AccountNumber = account.AccountNumber + "[BranchCode]" + account.PositionNumber, BookingDirection = "CREDIT", Amount = "200" };
        //            }
        //            chartofAccounts.Add(model);
        //        }

        //    }


        //    return View(new SavingConfiguration { AccountingRuleEntries = accountingRuleEntries });


        //}
        public async Task<ActionResult> AccountMappingInfo(string key)
        {
               SavingDepositProductIsPayable = false;
           SavingWithdrawalProductIsPayable = false;
        List<ChartofAccountInfo> chartofAccountInfos = new List<ChartofAccountInfo>();
            var savingProduct = await _savingProductServices.GetSavingProduct(key);
                ViewBag.ProductName = $"{savingProduct.Name} Product";
             

                var accountingRuleEntries = await GetFilteredAccountingRuleEntries(key);
                var accountingConfigList = await _accountConfiServices.GetChartOfAccountManagementPositions();
            ListOfChartOfAccount= accountingConfigList.ToList();
           var rootAccount = accountingConfigList.FirstOrDefault(c => c.Description == "ROOT ACCOUNT");
                var physicalTellerAccount = await GetPhysicalTellerAccount();
            var productBook =await _accountConfiServices.GetProductAccountingBook(key);
            if (productBook.Any())
            {
                if (productBook.FirstOrDefault().ProductType.Equals("Saving_Product"))
                {
                    ViewBag.ProductType = $"{productBook.FirstOrDefault().ProductType}";
                    chartofAccountInfos = await ProcessCASHINAccountList(key, accountingRuleEntries, accountingConfigList.ToList(), rootAccount, physicalTellerAccount);
                    chartofAccountInfos.AddRange(await ProcessCASHINLiaisonAccountList(key, accountingRuleEntries, accountingConfigList.ToList(), rootAccount, physicalTellerAccount));
                    chartofAccountInfos.AddRange(await ProcessCASHOutAccountList(key, accountingRuleEntries, accountingConfigList.ToList(), rootAccount, physicalTellerAccount));
                    chartofAccountInfos.AddRange(await ProcessCASHOutLiaisonAccountList(key, accountingRuleEntries, accountingConfigList.ToList(), rootAccount, physicalTellerAccount));
                    //chartofAccountInfos.AddRange(await ProcessInterestExpenseAccountList(key, accountingRuleEntries, accountingConfigList.ToList(), rootAccount, physicalTellerAccount));

                }
                else
                {

                }
            }
             
            return View(new SavingConfiguration { chartofAccountInfos= chartofAccountInfos, AccountingRuleEntries = accountingRuleEntries ,SavingDepositProductIsChargable= SavingDepositProductIsPayable,SavingWithdrwalProductIsChargable= SavingWithdrawalProductIsPayable });
         
        }

        private async Task<List<AccountingRuleEntry>> GetFilteredAccountingRuleEntries(string key)
        {
            var data = await _accountingEntryRuleService.GetAccountingEntryRules();
            return data.Where(x => x.EventCode.Contains(key)).ToList();
        }

        private async Task<AccountingRuleEntry> GetPhysicalTellerAccount()
        {
            var data = await _accountingEntryRuleService.GetAccountingEntryRules();
            return data.FirstOrDefault(e => e.EventCode.Equals("Physical_Teller"));
        }


        private async Task<List<ChartofAccountInfo>> ProcessCASHOutAccountList(
         string key,
         List<AccountingRuleEntry> accountingRuleEntries,
         List<ChartofAccountManagementPosition> accountingConfigList,
         ChartofAccountManagementPosition rootAccount,
         AccountingRuleEntry physicalTellerAccount)
        {
            var chartOfAccounts = new List<ChartofAccountInfo>();
            var accountList = accountingRuleEntries.Where(e => e.EventCode.Contains(key));

            foreach (var item in accountList)
            {
                var account = accountingConfigList.Find(x => x.Id.Equals(item.DeterminationAccountId));
                var sourceAccount = accountingConfigList.Find(x => x.Id.Equals(physicalTellerAccount.DeterminationAccountId));
                if (item.EventCode.Contains("Principal_Saving_Account"))
                {
                    chartOfAccounts.Add(CreateChartOfAccountInfo(item, account, rootAccount, "CASHOUT", "DEBIT", CheckCommissionOnWithdrawTransaction(accountList.ToList(), rootAccount)? "10200.0" : "10000.0"));
                    chartOfAccounts.Add(CreateChartOfAccountInfo(item, sourceAccount, rootAccount, "CASHOUT", "CREDIT", CheckCommissionOnWithdrawTransaction(accountList.ToList(), rootAccount) ? "10000.0" : "10000.0"));
                }
                
                else if ( item.EventCode.Contains("Withdrawal_Fee_Account") && CheckCommissionOnWithdrawTransaction(accountList.ToList(), rootAccount) )
                {
                    SavingWithdrawalProductIsPayable = true;
                    chartOfAccounts.Add(CreateChartOfAccountInfo(item, account, rootAccount, "CASHOUT", "CREDIT", "200.0"));
                }
            }

            return chartOfAccounts;
        }
        private async Task<List<ChartofAccountInfo>> ProcessCASHOutLiaisonAccountList(string key, List<AccountingRuleEntry> accountingRuleEntries, List<ChartofAccountManagementPosition> accountingConfigList, ChartofAccountManagementPosition rootAccount, AccountingRuleEntry physicalTellerAccount)
        {
            var chartOfAccounts = new List<ChartofAccountInfo>();
            var accountList = accountingRuleEntries.Where(e => e.EventCode.Contains(key));


            foreach (var item in accountList)
            {
                var account = accountingConfigList.Find(x => x.Id.Equals(item.DeterminationAccountId));

                var sourceAccount = accountingConfigList.Find(x => x.Id.Equals(physicalTellerAccount.DeterminationAccountId));
                if (item.EventCode.Contains("Principal_Saving_Account"))
                {
                    var ruleEntry = accountingRuleEntries.Where(e => e.EventCode.Equals(key + "@Liasson_Account"));
                    var liaisonAccount = accountingConfigList.Find(x => x.Id.Equals(ruleEntry.FirstOrDefault().DeterminationAccountId));
                    chartOfAccounts.Add(CreateChartOfAccountInfo(item, sourceAccount, rootAccount, "CASHOUT_REMOTE", "CREDIT", CheckCommissionOnWithdrawTransaction(accountList.ToList(), rootAccount) ? "10000" : "10000"));
                    chartOfAccounts.Add(CreateChartOfAccountInfo(item, liaisonAccount, rootAccount, "CASHOUT_REMOTE", "DEBIT", CheckCommissionOnWithdrawTransaction(accountList.ToList(), rootAccount) ? "10200" : "10000", true));
                }
                else if (item.EventCode.Contains("Withdrawal_Fee_Account") && CheckCommissionOnWithdrawTransaction(accountList.ToList(), rootAccount))
                {
                    SavingWithdrawalProductIsPayable = true;
                    chartOfAccounts.Add(CreateChartOfAccountInfo(item, account, rootAccount, "CASHOUT_REMOTE", "CREDIT", "200"));
                }
                else if (item.EventCode.Contains("Liasson_Account"))
                {
                    var ruleEntry = accountingRuleEntries.Where(e => e.EventCode.Equals(key + "@Principal_Saving_Account"));
                    var principalAccount = accountingConfigList.Find(x => x.Id.Equals(ruleEntry.FirstOrDefault().DeterminationAccountId));
                    chartOfAccounts.Add(CreateChartOfAccountInfo(item, principalAccount, rootAccount, "CASHOUT_REMOTE", "DEBIT", CheckCommissionOnWithdrawTransaction(accountList.ToList(), rootAccount) ? "10200" : "10000", true));

                    chartOfAccounts.Add(CreateChartOfAccountInfo(item, account, rootAccount, "CASHOUT_REMOTE", "CREDIT", CheckCommissionOnWithdrawTransaction(accountList.ToList(), rootAccount) ? "10200" : "10000", true));
                }
            }
            var accountx = accountingRuleEntries.Where(e => e.EventCode.Equals(key + "@Saving_Interest_Expense_Account")).FirstOrDefault();


            var model = ListOfChartOfAccount.Find(x => x.Id == accountx.DeterminationAccountId);
            chartOfAccounts.Add(CreateChartOfAccountInfo(accountx, model, rootAccount, "INTEREST_EXPENSE", "CREDIT", "10000"));


            return chartOfAccounts;
        }

        private async Task<List<ChartofAccountInfo>> ProcessCASHINAccountList(string key,        List<AccountingRuleEntry> accountingRuleEntries,     List<ChartofAccountManagementPosition> accountingConfigList,      ChartofAccountManagementPosition rootAccount,        AccountingRuleEntry physicalTellerAccount)
        {
            var chartOfAccounts = new List<ChartofAccountInfo>();
            var accountList = accountingRuleEntries.Where(e => e.EventCode.Contains(key));

            foreach (var item in accountList)
            {
                var account = accountingConfigList.Find(x => x.Id.Equals(item.DeterminationAccountId));
                var sourceAccount = accountingConfigList.Find(x => x.Id.Equals(physicalTellerAccount.DeterminationAccountId));

                if (item.EventCode.Contains("Commission_Account")&&CheckCommissionOnDepositTransaction(accountList.ToList(),rootAccount))
                {
           
                    chartOfAccounts.Add(CreateChartOfAccountInfo(item, account, rootAccount, "DEPOSIT", "CREDIT", "200"));
                } 
                else if (item.EventCode.Contains("Principal_Saving_Account"))
                {
                    chartOfAccounts.Add(CreateChartOfAccountInfo(item, account, rootAccount, "DEPOSIT", "CREDIT", CheckCommissionOnDepositTransaction(accountList.ToList(), rootAccount) ? "10000" : "10000"));
                    chartOfAccounts.Add(CreateChartOfAccountInfo(item, sourceAccount, rootAccount, "DEPOSIT", "DEBIT", CheckCommissionOnDepositTransaction(accountList.ToList(), rootAccount) ? "10200" : "10000"));
                }
              
            }
          SavingDepositProductIsPayable = CheckCommissionOnDepositTransaction(accountList.ToList(), rootAccount);
            return chartOfAccounts;
        }

        private async Task<List<ChartofAccountInfo>> ProcessInterestExpenseAccountList(string key, List<AccountingRuleEntry> accountingRuleEntries, List<ChartofAccountManagementPosition> accountingConfigList, ChartofAccountManagementPosition rootAccount, AccountingRuleEntry physicalTellerAccount)
        {
            var chartOfAccounts = new List<ChartofAccountInfo>();
            var account = accountingRuleEntries.Where(e => e.EventCode.Equals(key+ "@Saving_Interest_Expense_Account")).FirstOrDefault();

            
                var model = ListOfChartOfAccount.Find(x=>x.Id==account.DeterminationAccountId);
                chartOfAccounts.Add(CreateChartOfAccountInfo(account, model, rootAccount, "INTEREST_EXPENSE", "CREDIT", "10000"));
            

            return chartOfAccounts;
        }


        private async Task<List<ChartofAccountInfo>> ProcessCASHINLiaisonAccountList(  string key,  List<AccountingRuleEntry> accountingRuleEntries,   List<ChartofAccountManagementPosition> accountingConfigList,   ChartofAccountManagementPosition rootAccount,  AccountingRuleEntry physicalTellerAccount)
        {
            var chartOfAccounts = new List<ChartofAccountInfo>();
            var accountList = accountingRuleEntries.Where(e => e.EventCode.Contains(key));

             

            foreach (var item in accountList)
            {
                var account = accountingConfigList.Find(x => x.Id.Equals(item.DeterminationAccountId));

                var sourceAccount = accountingConfigList.Find(x => x.Id.Equals(physicalTellerAccount.DeterminationAccountId));
                if (item.EventCode.Contains("Principal_Saving_Account"))
                {
                    var ruleEntry = accountingRuleEntries.Where(e => e.EventCode.Equals(key+"@Liasson_Account"));
                    var liaisonAccount = accountingConfigList.Find(x => x.Id.Equals(ruleEntry.FirstOrDefault().DeterminationAccountId));
                    var modelc = CreateChartOfAccountInfo(item, sourceAccount, rootAccount, "DEPOSIT_REMOTE", "DEBIT", CheckCommissionOnDepositTransaction(accountList.ToList(), rootAccount) ? "10200" : "10000");
                    chartOfAccounts.Add(modelc);
                    var modelb = CreateChartOfAccountInfo(item, liaisonAccount, rootAccount, "DEPOSIT_REMOTE", "CREDIT", CheckCommissionOnDepositTransaction(accountList.ToList(), rootAccount) ? "10200" : "10000", true);
                    chartOfAccounts.Add(modelb);
                }
                else if (item.EventCode.Contains("Commission_Account")  ||
                        item.EventCode.Contains("Transfer_Fee_Account"))
                {
          
                    chartOfAccounts.Add(CreateChartOfAccountInfo(item, account, rootAccount, "DEPOSIT_REMOTE", "CREDIT", "200"));
                }
                else if (item.EventCode.Contains("Liasson_Account"))
                {
                    var ruleEntry = accountingRuleEntries.Where(e => e.EventCode.Equals(key + "@Principal_Saving_Account"));
                    var principalAccount = accountingConfigList.Find(x => x.Id.Equals(ruleEntry.FirstOrDefault().DeterminationAccountId));
                    chartOfAccounts.Add(CreateChartOfAccountInfo(item, principalAccount, rootAccount, "DEPOSIT_REMOTE", "CREDIT", "10000"));
                 
                    chartOfAccounts.Add(CreateChartOfAccountInfo(item, account, rootAccount, "DEPOSIT_REMOTE", "DEBIT", CheckCommissionOnDepositTransaction(accountList.ToList(), rootAccount) ? "10200" : "10000",true));
                }
            }
            SavingDepositProductIsPayable = CheckCommissionOnDepositTransaction(accountList.ToList(), rootAccount);

            return chartOfAccounts;
        }

        private bool CheckCommissionOnDepositTransaction(List<AccountingRuleEntry> accountList, ChartofAccountManagementPosition rootAccount)
        {
            bool IsPresent =false;
  
            foreach (var item in accountList) 
            {
                if (item.EventCode.Contains("Commission_Account"))
                {
                    var model = ListOfChartOfAccount.Find(x => x.Id.Equals(item.DeterminationAccountId));
                    IsPresent= model.Id!=rootAccount.Id;
             
                }

            }
            return IsPresent;
        }
        private bool CheckCommissionOnWithdrawTransaction(List<AccountingRuleEntry> accountList, ChartofAccountManagementPosition rootAccount)
        {
            bool IsPresent = false;
            foreach (var item in accountList)
            {
                if (item.EventCode.Contains("Withdrawal_Fee_Account") )
                {
                    var model = ListOfChartOfAccount.Find(x => x.Id.Equals(item.DeterminationAccountId));
                    IsPresent = model.Id != rootAccount.Id; 
                }

            }
            return IsPresent;
        }
        private ChartofAccountInfo CreateChartOfAccountInfo(   AccountingRuleEntry item,     ChartofAccountManagementPosition account, ChartofAccountManagementPosition rootAccount, string operationType,   string bookingDirection,  string amount,   bool isLiassonAccount = false)
        {
            if (item.DeterminationAccountId == rootAccount.Id)
            {
                return new ChartofAccountInfo
                {
                    OperationPeriod = isLiassonAccount ? "LIAISON" : "LOCAL",
                    OperationType = operationType,
                    Id = "XXXXXX",
                    AccountName = "XXXXXXXX",
                    AccountNumber = "XXXXXXXX",
                    BookingDirection = bookingDirection,
                    Amount = "0"
                };
            }

            string accountNumber = isLiassonAccount
                ? $"{account.AccountNumber.PadRight(6,'0')}[SourceBranchCode][DestinationBranchCode]"
                : $"{account.AccountNumber.PadRight(6, '0')}[BranchCode]{account.PositionNumber}";

            return new ChartofAccountInfo
            {
                OperationPeriod = isLiassonAccount?"LIAISON":"LOCAL",
                OperationType = operationType,
                Id = item.DeterminationAccountId,
                AccountName = account.Description,
                AccountNumber = accountNumber,
                BookingDirection = bookingDirection,
                Amount = amount
            };
        }
        [HttpPost]
        public async Task<ActionResult> AddOrUpdate(SavingConfiguration model)
        {
            Func<Task<ExecutionMessages>> serviceAction = null;

            if (model.Action == "insert")
            {
                serviceAction = GetInsertServiceAction(model.ServiceOption, model);
            }
            else
            {
                serviceAction = GetUpdateServiceAction(model.ServiceOption, model);
            }

            if (serviceAction != null)
            {
                try
                {
                    var data = await serviceAction();
                    return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, status = false, message = $"An error occurred: {ex.Message}" });
                }
            }

            return Json(new { success = false, status = false, message = "Invalid option selected." });
        }

        private Func<Task<ExecutionMessages>> GetInsertServiceAction(string serviceOption, SavingConfiguration model)
        {
            if (serviceOption == "savingproduct")
            {
                return () => _savingProductServices.Create(model.SavingProduct);
            }
            else if (serviceOption == "depositlimit")
            {
                return () => _depositLimitServices.Create(model.DepositLimit);
            }
            else if (serviceOption == "transferlimit")
            {
                return () => _transferLimitServices.Create(model.TransferLimit);
            }
            else if (serviceOption == "withdrawallimit")
            {
                return () => _withdrawalLimitServices.Create(model.WithdrawalLimit);
            }
            else if (serviceOption == "CloseFeeParameter")
            {
                return () => _closeFeeParameterServices.Create(model.CloseFeeParameter);
            }
            else if (serviceOption == "EntryFeeParameter")
            {
                return () => _entryFeeParameterServices.Create(model.EntryFeeParameter);
            }
            else if (serviceOption == "ManagementFeeParameter")
            {
                return () => _managementFeeParameterServices.Create(model.ManagementFeeParameter);
            }
            else if (serviceOption == "ReopenFeeParameter")
            {
                return () => _reopenFeeParameterServices.Create(model.ReopenFeeParameter);
            }
            else if (serviceOption == "AccountMapping")
            {
                return () => _savingProductServices.UpdateProductAccountMapping(model.SavingProduct);
            }
            //accountmapping
            else if (serviceOption == "teller")
            {
                return () => _tellerServices.Create(model.Teller);
            }
            else
            {
                return null;
            }
        }

        private Func<Task<ExecutionMessages>> GetUpdateServiceAction(string serviceOption, SavingConfiguration model)
        {
            if (serviceOption == "savingproduct")
            {
                return () => _savingProductServices.Update(model.SavingProduct);
            }
            else if (serviceOption == "depositlimit")
            {
                return () => _depositLimitServices.Update(model.DepositLimit);
            }
            else if (serviceOption == "accountmapping")
            {
                return () => _savingProductServices.UpdateProductAccountMapping(model.SavingProduct);
            }
            else if (serviceOption == "mapformeventcharges")
            {
                return () => _savingProductServices.UpdateProductEventMapping(model.SavingProduct);
            }
            //
            else if (serviceOption == "transferlimit")
            {
                return () => _transferLimitServices.Update(model.TransferLimit);
            }
            else if (serviceOption == "withdrawallimit")
            {
                return () => _withdrawalLimitServices.Update(model.WithdrawalLimit);
            }
            else if (serviceOption == "CloseFeeParameter")
            {
                return () => _closeFeeParameterServices.Update(model.CloseFeeParameter);
            }
            else if (serviceOption == "EntryFeeParameter")
            {
                return () => _entryFeeParameterServices.Update(model.EntryFeeParameter);
            }
            else if (serviceOption == "ManagementFeeParameter")
            {
                return () => _managementFeeParameterServices.Update(model.ManagementFeeParameter);
            }
            else if (serviceOption == "ReopenFeeParameter")
            {
                return () => _reopenFeeParameterServices.Update(model.ReopenFeeParameter);
            }
            else if (serviceOption == "teller")
            {
                return () => _tellerServices.Update(model.Teller);
            }
            else
            {
                return null;
            }
        }


        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)

        {

            Func<Task<PartialViewResult>> serviceAction = GetServiceAction(path, partialView, KEY, serviceOption);

            if (serviceAction != null)
            {
                var partialResult = await serviceAction();

                if (partialResult != null)
                {
                    return partialResult;
                }
            }


            return HttpNotFound(); // Or return a default view for handling unknown paths
        }


        private Func<Task<PartialViewResult>> GetServiceAction(string path, string partialView, string key, string serviceOption)

        {
            if (serviceOption == "savingproduct")
            {
                if (path == "list")
                {
                    //
                    return async () =>
                    {
                        var data = await _savingProductServices.GetSavingProducts();
                        var sysData = new SavingConfiguration { SavingProducts = data.ToList() };
                        return PartialView(partialView, sysData);
                    };
                }
                else if (path == "feemapping")
                {

                    return async () =>
                    {
                        await GetEventNames();
                        var savingProductFees = await _savingProductFeeServices.GetSavingProductFees(key);
                        return PartialView(partialView, new SavingConfiguration { SavingProductFees = savingProductFees.ToList() });

                    };
                }
                else if (path == "new")
                {

                    return async () =>
                    {
                        var conf = await _savingProductServices.GetSavingConfigurationAggregates();
                        ViewBag.Frequences = conf.freeQuencies.ToList();
                        ViewBag.Currencies = conf.currencies.ToList();
                        return PartialView(partialView, new SavingConfiguration { SavingProduct = new SavingProduct() });

                    };
                }
                else
                {
                    return async () =>
                    {
                        var conf = await _savingProductServices.GetSavingConfigurationAggregates();
                        ViewBag.Frequences = conf.freeQuencies.ToList();
                        ViewBag.Currencies = conf.currencies.ToList();
                        return PartialView(partialView, new SavingConfiguration { SavingProduct = await _savingProductServices.GetSavingProduct(key) });

                    };
                }
            }
            else if (serviceOption == "depositlimit")
            {
                if (path == "list")
                {
                    if (key != null)
                    {
                        return async () =>
                        {
                            var data = await _depositLimitServices.GetDepositLimits();
                            var datas = data.Where(x => x.ProductId == key).ToList();
                            var sysData = new SavingConfiguration { DepositLimits = datas };
                            return PartialView(partialView, sysData);
                        };
                    }
                    return async () =>
                    {
                        var data = await _depositLimitServices.GetDepositLimits();
                        var sysData = new SavingConfiguration { DepositLimits = data.ToList() };
                        return PartialView(partialView, sysData);
                    };
                }
                else if (path == "new")
                {
                    return async () =>
                    {
                        await GetList();
                        var savingProduct = await _savingProductServices.GetSavingProduct(key);
                        return PartialView(partialView, new SavingConfiguration { DepositLimit = new DepositLimit { ProductId = key }, SavingProduct = savingProduct });
                    };
                }
                else
                {
                    return async () =>
                    {
                        await GetList();
                        var data = await _depositLimitServices.GetDepositLimit(key);
                        var savingProduct = await _savingProductServices.GetSavingProduct(data.ProductId);
                        return PartialView(partialView, new SavingConfiguration { DepositLimit = data, SavingProduct = savingProduct });
                    };
                }

            }
            else if (serviceOption == "transferlimit")
            {
                if (path == "list")
                {
                    if (key != null)
                    {
                        return async () =>
                        {
                            var data = await _transferLimitServices.GetTransferLimits();
                            var sysData = new SavingConfiguration { TransferLimits = data.Where(x => x.productId == key).ToList() };
                            return PartialView(partialView, sysData);
                        };
                    }
                    return async () =>
                    {
                        var data = await _transferLimitServices.GetTransferLimits();
                        var sysData = new SavingConfiguration { TransferLimits = data.ToList() };
                        return PartialView(partialView, sysData);
                    };
                }
                else if (path == "new")
                {
                    return async () =>
                    {
                        await GetList();
                        var savingProduct = await _savingProductServices.GetSavingProduct(key);
                        return PartialView(partialView, new SavingConfiguration { TransferLimit = new TransferLimit { productId = key }, SavingProduct = savingProduct });
                    };
                }
                else
                {
                    return async () =>
                    {
                        await GetList();
                        var data = await _transferLimitServices.GetTransferLimit(key);
                        var savingProduct = await _savingProductServices.GetSavingProduct(data.productId);
                        return PartialView(partialView, new SavingConfiguration { TransferLimit = data, SavingProduct = savingProduct });
                    };
                }

            }
            else if (serviceOption == "withdrawallimit")
            {
                if (path == "list")
                {
                    if (key != null)
                    {
                        return async () =>
                        {
                            var data = await _withdrawalLimitServices.GetWithdrawalLimits();
                            var withdrawals = data.Where(x => x.ProductId == key).ToList();

                            //090491483100383
                            //090491483100383
                            var sysData = new SavingConfiguration { WithdrawalLimits = withdrawals };
                            return PartialView(partialView, sysData);
                        };
                    }
                    return async () =>
                    {
                        var data = await _withdrawalLimitServices.GetWithdrawalLimits();
                        var sysData = new SavingConfiguration { WithdrawalLimits = data.ToList() };
                        return PartialView(partialView, sysData);
                    };
                }
                else if (path == "new")
                {
                    return async () =>
                    {
                        var conf = await _savingProductServices.GetSavingConfigurationAggregates();
                        ViewBag.Frequences = conf.freeQuencies.ToList();
                        ViewBag.Currencies = conf.currencies.ToList();
                        ViewBag.WithdrawalLimitTypes = conf.withdrawalTypes.ToList();
                        var savingProduct = await _savingProductServices.GetSavingProduct(key);
                        return PartialView(partialView, new SavingConfiguration { WithdrawalLimit = new WithdrawalLimit { ProductId = key }, SavingProduct = savingProduct });
                    };
                }
                else
                {
                    return async () =>
                    {
                        var conf = await _savingProductServices.GetSavingConfigurationAggregates();
                        ViewBag.Frequences = conf.freeQuencies.ToList();
                        ViewBag.Currencies = conf.currencies.ToList();
                        ViewBag.WithdrawalLimitTypes = conf.withdrawalTypes.ToList();
                        var data = await _withdrawalLimitServices.GetWithdrawalLimit(key);
                        var savingProduct = await _savingProductServices.GetSavingProduct(data.ProductId);
                        return PartialView(partialView, new SavingConfiguration { WithdrawalLimit = data, SavingProduct = savingProduct });
                    };
                }

            }

            return null;
        }

        private async Task GetEventNames()
        {
            ViewBag.EventCodes = await _accountingServices.GetEventNames("INCOME");

        }

        public async Task<bool> GetList()
        {
            var conf = await _savingProductServices.GetSavingConfigurationAggregates();
            ViewBag.Products = await _savingProductServices.GetSavingProducts();
            var chartOfAccounts = await _accountingServices.GetChartOfAccounts();
            ViewBag.DepositLimitTypes = conf.depositTypes.ToList();
            ViewBag.TransferLimitTypes = conf.transferTypes.ToList();
            ViewBag.WithdrawalLimitTypes = conf.withdrawalTypes.ToList();
            ViewBag.Frequences = conf.freeQuencies.ToList();
            ViewBag.chartOfAccounts = chartOfAccounts.ToList();
            ViewBag.Currencies = conf.currencies.ToList();
            ViewBag.OperationEventAttributes = await _accountingServices.GetEventAttributeByOperationTypeID();
            ViewBag.operationAccounts = conf.operationAccounts.ToList();
            return true;
        }
        public async Task<bool> GetChartOfAccounts()
        {
            var chartOfAccounts = await _accountingServices.GetChartOfAccounts();
            ViewBag.chartOfAccounts = chartOfAccounts.ToList();
            return true;
        }
        public async Task<bool> GetListPolicies()
        {
            var conf = await _savingProductServices.GetSavingConfigurationAggregates();
            ViewBag.DepositLimitTypes = conf.depositTypes.ToList();
            ViewBag.TransferLimitTypes = conf.transferTypes.ToList();
            ViewBag.WithdrawalLimitTypes = conf.withdrawalTypes.ToList();
            ViewBag.Frequences = conf.freeQuencies.ToList();
            return true;
        }
        public async Task<ActionResult> Ajaxloader(string Key)
        {
            var listing = await _accountingServices.GetEventAttributeByOperationTypeID(Key);
            return Json(listing, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> Delete(string Key)
        {
            var data = await _savingProductServices.Delete(Key);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) }, JsonRequestBehavior.AllowGet);
        }
    }
}