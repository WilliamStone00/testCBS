using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessServices;
using CBS.API.Helper;
using System.Configuration;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Config;
using DocumentFormat.OpenXml.EMMA;
using CBS.FrontDesk.Data.Entity.LoanConf;
using System.IO.Packaging;
using System.Security.Policy;
using System.Web.Helpers;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.Config;

namespace CBS.BusinessService.Accounting
{
    public class AccountingServices : BaseService
    {
        private readonly ApiCallerHelper _accountingApiCallerHelper;
        public BranchServices _branchService { get; }

        public AccountCategoryServices accountCartegorieService { get; }

        public AccountingServices()
        {
            _accountingApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingBaseUrl"].ToString());
            accountCartegorieService = new AccountCategoryServices();
            _branchService = new BranchServices();
            //_chartofaccountService = new ChartOfAccountManagementPositionService(); 
        }
        private const string CLASS_4 = "4"; //THIRD PARTY ACCOUNTS AND ACCRUALS(Payabels)
        private const string CLASS_4_Payabels = "THIRD PARTY ACCOUNTS AND ACCRUALS(Payabels)";
        private const string CLASS_4_Simple = "THIRD PARTY ACCOUNTS AND ACCRUALS";
        private const string CLASS_4_Recievabels = "THIRD PARTY ACCOUNTS AND ACCRUALS(Recievables)";
        public async Task<List<ReportInfo>> GetAllFileDownloadInfoPerUser()
        {
            try
            {
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<ReportInfo>>>(string.Format(APICallHelper.GetAllUserDownLoads, GetUserID()));
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<ReportInfo>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }

        public async Task<FileReportInfoDto> GetFileDownloadedByFileId(string fileId)
        {
            try
            {
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<FileReportInfoDto>>(string.Format(APICallHelper.Get_DownloadedFile, fileId));
                if (couApiResponse.IsSuccess)
                {
                    if (couApiResponse.ApiResponseData != null)
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }

                }
                return new FileReportInfoDto();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw (ex);
            }
        }
        public async Task<ExecutionMessages> Create(FrontDesk.Data.Account model)
        {
            try
            {
                model.AccountNumberNetwok = "xxxxx";
                model.AccountTypeId = model.AccountNumber.Equals("45100") ? model.AccountCounterPartId : "YYYYYY";
                model.AccountNumberManagementPosition = "0";

                // Make an API call to create an individual profile

                // model.AccountOwnerId=GetBranchID();
                var response = await _accountingApiCallerHelper.PostAsync<ApiResponse<bool>>(APICallHelper.CreateAccount, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"Account {model.AccountNumber + " " + model.AccountName} has been created successfully", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, "");
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, $"Account {model.AccountNumber + " " + model.AccountName} failed to be created ", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
            }
            return ExecutionMessage;
        }

        public async Task<AccountResponseDto> CreateAccountOnProcessing(FrontDesk.Data.Account model)
        {
            AccountResponseDto responseDto = new AccountResponseDto();
            try
            {
                model.AccountNumberNetwok = "xxxxx";
                model.AccountTypeId = model.AccountNumber.Equals("45100") ? model.AccountCounterPartId : "YYYYYY";
                model.AccountNumberManagementPosition = "0";

                // Make an API call to create an individual profile

                // model.AccountOwnerId=GetBranchID();
                var response = await _accountingApiCallerHelper.PostAccountResponseAsync<ApiResponse<AccountResponseDto>>(APICallHelper.CreateAccountMET, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    //GetExecutionMessages(response, true, $"Account {model.AccountNumber + " " + model.AccountName} has been created successfully", MessagesResults.Success,
                    //    ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, "");
                    responseDto= response.ApiResponseData;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, $"Account {model.AccountNumber + " " + model.AccountName} failed to be created ", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
            }
            return responseDto;
        }
        /// <summary>
        /// Updates account balances according to OHADA accounting rules
        /// </summary>
        public async Task<bool> CheckAccountBalance(FrontDesk.Data.Account account, decimal amount, OperationTypes operationType)
        {
            if (account==null)
            {
                return false;
            }
            // Store last balance before any cha00k0es
            account.LastBalance = account.CurrentBalance.ToString();

            // Determine account behavior based on OHADA rules
            bool isDebitNormal = account.AccountNumber.StartsWith("2") || // Fixed Assets
                                                                          //   || // Inventory
                                account.AccountNumber.StartsWith("5") || // Financial
                                account.AccountNumber.StartsWith("6");    // Expenses

            bool isCreditNormal = account.AccountNumber.StartsWith("1") || // Capital
                                account.AccountNumber.StartsWith("7") ||    // Income
                                   account.AccountNumber.StartsWith("3");
            // Handle class 4 accounts separately
            bool isClass4Receivable = account.AccountNumber.StartsWith("41") || account.AccountNumber.StartsWith("42") || account.AccountNumber.StartsWith("46");
            bool isClass4Payables = account.AccountNumber.StartsWith("40") || account.AccountNumber.StartsWith("43") || account.AccountNumber.StartsWith("48") || account.AccountNumber.StartsWith("49") ||
                account.AccountNumber.StartsWith("44") || account.AccountNumber.StartsWith("45") || account.AccountNumber.StartsWith("47");
            if (isClass4Payables || isClass4Receivable)
            {
                return true;
            }

            if (isDebitNormal || isClass4Receivable)
            {
                if (operationType == OperationTypes.DEBIT)
                {

                    // For debit-normal accounts, debit increases the balance
                    account.DebitBalance += amount;
                    return (account.DebitBalance - account.CreditBalance) > 0;
                }
                else // CREDIT
                {
                    account.CreditBalance += amount;
             
                    return (account.DebitBalance - account.CreditBalance) > 0;
                }
            }
            else
            {
                if (operationType == OperationTypes.DEBIT)
                {

                    // For debit-normal accounts, debit increases the balance
                    account.DebitBalance += amount;
                    return account.CreditBalance - account.DebitBalance > 0;
                }
                else // CREDIT
                {
                    account.CreditBalance += amount;

                    return (account.CreditBalance - account.DebitBalance) > 0;
                }
            }

   
        }

        public async Task<FrontDesk.Data.Account> GetAccountWithAccountCartegorieStatus(string id)
        {
            try
            {
                var listOfCategories = (await accountCartegorieService.GetAccountCategory()).ToList();
                    var account = (await GetAllAccounting()).Where(i => i.Id.Equals(id)).FirstOrDefault();
                var modelVal = listOfCategories.Find(x => x.Id == account.AccountCategoryId);
                if (modelVal.Name== CLASS_4_Recievabels||account.AccountNumber.StartsWith("2") || // Fixed Assets
                                                                                                //   || // Inventory
                                account.AccountNumber.StartsWith("5") || // Financial
                                account.AccountNumber.StartsWith("6") )  // Expenses)
                {
                    account.AccountCategoryId = "debit";
                }
                else
                {
                    account.AccountCategoryId = "credit";
                }
                return account;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<FrontDesk.Data.Account> GetAccount(string id)
        {
            try
            {
                var cusResponseObject = (await GetAllAccounting()).Where(i => i.Id.Equals(id)).FirstOrDefault();
                return cusResponseObject;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<ReportInfo> GetReportById(string Id)
        {
            try
            {
                var couApiResponse = (await GetAllReportInfo()).Find(x => x.Id.Equals(Id));
                if (couApiResponse != null)
                {
                    return couApiResponse;

                }
                return null;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw (ex);
            }
        }
        public async Task<List<ReportInfo>> GetAllReportInfo()
        {
            try
            {
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<ReportInfo>>>(APICallHelper.GetAllReportDownLoad);
                if (couApiResponse.IsSuccess)
                {
                    if (couApiResponse.ApiResponseData != null)
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }

                }
                return new List<ReportInfo>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw (ex);
            }
        }
        public async Task<string> GetSequenceReference()
        {
            try
            {
                var response = await _accountingApiCallerHelper.GetAsync<ApiResponse<string>>(APICallHelper.GetReferenceSequenceUrl);
                return response.ApiResponseData.ApiResponseData;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<FrontDesk.Data.Account> GetAccountByAccountNumber(string id)
        {
            try
            {

                string Url = string.Format(APICallHelper.GetAccountByAccountNumberUrl, id);
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<FrontDesk.Data.Account>>(Url);
                if (couApiResponse.IsSuccess)
                {
                    if (couApiResponse.ApiResponseData != null)
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }

                }
                return new FrontDesk.Data.Account();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<List<FrontDesk.Data.Account>> GetAllLiasionAccount(string BranchId)
        {
            try
            {

                string Url = string.Format(APICallHelper.GetSystemLiaisonAccountQueryUrl, BranchId);
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<FrontDesk.Data.Account>>>(Url);
                if (couApiResponse.IsSuccess)
                {
                    if (couApiResponse.ApiResponseData != null)
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }

                }
                return new List<FrontDesk.Data.Account>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw (ex);
            }
        }
        public async Task<List<FrontDesk.Data.Account>> GetAllBranchAccountUsedToCreditCashFlow(string BranchId)
        {
            try
            {

                string Url = string.Format(APICallHelper.GetAllBranchAccountUsedToCreditCashFlow, BranchId);
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<FrontDesk.Data.Account>>>(Url);
                if (couApiResponse.IsSuccess)
                {
                    if (couApiResponse.ApiResponseData != null)
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }

                }
                return new List<FrontDesk.Data.Account>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw (ex);
            }
        }
      
        
        public async Task<List<FrontDesk.Data.Account>> GetAllAccounting()
        {
            try
            {
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<FrontDesk.Data.Account>>>(APICallHelper.GetAlAccounts);
                if (couApiResponse.IsSuccess)
                {
                    if (couApiResponse.ApiResponseData != null)
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }

                }
                return new List<FrontDesk.Data.Account>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw (ex);
            }
        }
        public async Task<List<FrontDesk.Data.Account>> GetAllLiaisonAccount()
        {
            try
            {
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<FrontDesk.Data.Account>>>(APICallHelper.GetAllLiaisonAccount);
                if (couApiResponse.IsSuccess)
                {
                    if (couApiResponse.ApiResponseData != null)
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }

                }
                return new List<FrontDesk.Data.Account>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw (ex);
            }
        }
        public async Task<List<FrontDesk.Data.Account>> GetAllAccountForABranch(string branchId)
        {
            try
            {
                  branchId = branchId.Equals("DEFAULTID") ? (await _branchService.GetBranches()).Where(x => x.BranchCode == "001").FirstOrDefault().Id:branchId;
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<FrontDesk.Data.Account>>>(string.Format(APICallHelper.GetAllAccountByBranch, branchId));
                if (couApiResponse.IsSuccess)
                {
                    if (couApiResponse.ApiResponseData != null)
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }

                }
                return new List<FrontDesk.Data.Account>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw (ex);
            }
        }



        public async Task<IEnumerable<AccountingRole>> GetAccountingRoles()
        {
            try
            {
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<AccountingRole>>>(APICallHelper.GetAllAccountingRules);
                if (couApiResponse.IsSuccess)
                {
                    if (couApiResponse.ApiResponseData != null)
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }

                }
                return new List<AccountingRole>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<IEnumerable<StringValues>> GetEventNames(string opertionType)
        {
            try
            {
                // Make an API call to retrieve accounting event attributes
                var response = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<AccountingEventAttributs>>>(string.Format(APICallHelper.GetAllOperationServicesAccountingRuleEntryQuery, opertionType));
                if (response.IsSuccess && response.ApiResponseData != null)
                {
                    // Map the API response to StringValues objects with Text and Value properties
                    var stringValuesList = response.ApiResponseData.Data
                        .Select(item => new StringValues(item.EventCode, item.AccountingRuleEntryName))
                        .ToList();

                    return stringValuesList;
                }
                else
                {
                    // Handle unsuccessful API response
                    // You can log the error or return an empty list
                    return Enumerable.Empty<StringValues>();
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw new ApplicationException("Error occurred while fetching accounting event names.", ex);
            }
        }

        public async Task<IEnumerable<StringValues>> GetEventNamesOtherCashIn(string opertionType)
        {
            try
            {
                // Make an API call to retrieve accounting event attributes
                var response = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<AccountingEventAttributs>>>(string.Format(APICallHelper.GetAllOperationServicesAccountingRuleEntryQuery, opertionType));
                if (response.IsSuccess && response.ApiResponseData != null)
                {
                    // Map the API response to StringValues objects with Text and Value properties
                    var stringValuesList = response.ApiResponseData.Data
                        .Select(item => new StringValues($"{item.AccountingRuleEntryName}", $"{item.EventCode}"))
                        .ToList();

                    return stringValuesList;
                }
                else
                {
                    // Handle unsuccessful API response
                    // You can log the error or return an empty list
                    return Enumerable.Empty<StringValues>();
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw new ApplicationException("Error occurred while fetching accounting event names.", ex);
            }
        }

        public async Task<ExecutionMessages> Update(FrontDesk.Data.Account account)
        {
            try
            {
                account.AccountOwnerId = GetBranchID();
                var response = await _accountingApiCallerHelper.PutAsync<ServiceResponse<FrontDesk.Data.Account>>(string.Format(APICallHelper.PutAccount, account.Id), account);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"Account {account.AccountNumber + " " + account.AccountName} has been updated successfully", MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(response.ApiResponseData, false, $"{account.AccountNumber + " " + account.AccountName} has been updated", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                }

            }
            catch (Exception ex)
            {
                // Log and handle exception
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
            }
            return ExecutionMessage;
        }


        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var account = await GetAccount(id);
                var inResponse = await _accountingApiCallerHelper.DeleteAsync<ResponseObject<bool>>(string.Format(APICallHelper.Delete_Account, id));
                if (inResponse.IsSuccess)
                {


                    return GetExecutionMessages(inResponse, true, $"{account.AccountNumber + " " + account.AccountName}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);


                }
                else
                {
   
                    return GetExecutionMessages(account, false, $"{account.AccountNumber + " " + account.AccountName}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }

            }
            catch (Exception ex)
            {
    
            }
            return null;
        }
        public async Task<ExecutionMessages> Deletto(string id)
        {
            try
            {
                var account = await GetReportById(id);
                var inResponse = await _accountingApiCallerHelper.DeleteAsync<ResponseObject<bool>>(string.Format(APICallHelper.Get_Delete_ReportDownLoad, id));
                if (inResponse.IsSuccess)
                {


                    return GetExecutionMessages(inResponse, true, $"{account.ReportType + " " + account.FileName}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);


                }
                else
                {
                    // Handle failure scenario
                    return GetExecutionMessages(account, false, $"{account.ReportType + " " + account.ReportType}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }

            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return null;
        }

        public async Task<ExecutionMessages> DeleteReportDto(string id)
        {
            try
            {
                var account = await GetReportById(id);
                var inResponse = await _accountingApiCallerHelper.DeleteAsync<ResponseObject<bool>>(string.Format(APICallHelper.Get_Delete_ReportDownLoad, id));
                if (inResponse.IsSuccess)
                {


                    return GetExecutionMessages(inResponse, true, $"{account.ReportType + " " + account.FileName}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);


                }
                else
                {
                    // Handle failure scenario
                    return GetExecutionMessages(account, false, $"{account.ReportType + " " + account.ReportType}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }

            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return null;
        }

        public async Task<ExecutionMessages> Create(UploadAccount list)
        {
            try
            {

                if (list.AccountModelList.Count() == 0)
                {
                    // Failed creation
                    GetExecutionMessages(list, true, $"NO element was seen in the file", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, $"NO element was seen in the file");
                }


                var response = await _accountingApiCallerHelper.PostUploadAccountResultResponseAsync(APICallHelper.CreateAccounOnUploadie, list);
                if (response != null)
                {
                    if (response.isSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, response.message, MessagesResults.Success,
                   ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.message);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(response, true, $"{response.message}", MessagesResults.Failed,
                            ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.message);
                    }
                    // Successful creation

                }
                else
                {
                    //throw new Exception("");
                    // Failed creation
                    GetExecutionMessages(response, true, $"Posting ", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, $"Null reference exception");
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
            }
            return ExecutionMessage;
        }

        public async Task<AccountingEntriesReport> PostJournalEntries(JEQuery model, string url)
        {
            try
            {

                // Make an API call to create an individual profile


                var response = await _accountingApiCallerHelper.PostJOurnalEntriesAsync(url, model);
                if (response.StatusCode==200)
                {
                    // Successful creation
                    //GetExecutionMessages(response, true, $"Transaction was successfull", MessagesResults.Success,
                    //    ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return response.Data;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, $"Transaction was not successfull", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
            }
            return new AccountingEntriesReport();
        }
        public async Task<ExecutionMessages> PostJE(JEQuery model, string url)
        {
            try
            {

                // Make an API call to create an individual profile


                var response = await _accountingApiCallerHelper.PostAsync<ServiceResponse<ReportDto>>(url, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"Transaction was successfull", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, $"Transaction was not successfull", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> PostJE(SystemQuery model, string url)
        {
            try
            {

                // Make an API call to create an individual profile


                var response = await _accountingApiCallerHelper.PostAsync<ServiceResponse<ReportDto>>(url, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"Transaction was successfull", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, $"Transaction was not successfull", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
            }
            return ExecutionMessage;
        }
        public async Task<ReportInfo> GetFileDownloadById(string fileId)
        {
            return (await GetAllReportInfo()).Find(x => x.Equals(fileId));
        }



        public async Task<List<InfoAccount>> GetAccountInfoByEventCode(EventRequest model)
        {
            try
            {

                // Make an API call to create an individual profile

                // model.AccountOwnerId=GetBranchID();
                var response = await _accountingApiCallerHelper.PostServicesAsync<ApiResponse<List<InfoAccount>>>(string.Format(APICallHelper.GetAccountByEvenCodeUrl, model.EventCode), model);
                if (response.IsSuccess)
                {
                    return response.ApiResponseData;
                }
                else
                {
                    // Failed creation
                    return null;
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
            }
            return null;
        }

        public async Task<IExecutionMessages> CleanAccountingEntry(List<string> listOfBranchIds)
        {

            try
            {

                if (listOfBranchIds.Count() == 0)
                {
                    // Failed creation
                    GetExecutionMessages(listOfBranchIds, true, $"No branchId was selected ", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, $"No branchId was selected");
                }

                var model = new { BranchIds =Newtonsoft.Json. JsonConvert.DeserializeObject< List<string> > (listOfBranchIds[0]) };
                var response = await _accountingApiCallerHelper.PostUploadAccountResultResponseAsync(APICallHelper.CleanAccountingEntryUrl, model);
                if (response != null)
                {
                    if (response.isSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"The AccountingEntries and the account of the following branches has been cleared", MessagesResults.Success,
                   ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.message);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(response, true, $"Accounting entries cleaning has failed.", MessagesResults.Failed,
                            ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.message);
                    }
                    // Successful creation

                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(response, true, $"Accounting entries cleaning has failed.", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, $"Accounting entries cleaning has failed.");
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
            }
            return ExecutionMessage;


        }

   

        public async Task<FrontDesk.Data.Account> GetAccountItemForBranch(List<FrontDesk.Data.Account> branchAccounts, List<ChartofAccountManagementPosition> chartOfAccountMps, AccountModel item)
        {
            FrontDesk.Data.Account accountData = new FrontDesk.Data.Account();
            var account = branchAccounts.Where(x=>x.ChartOfAccountManagementPositionId.Equals(item.Id));
            if (account.Any())
            {
                accountData = account.FirstOrDefault();
            }
            else
            {
                var modell =   chartOfAccountMps.Find(x=>x.Id==item.Id);

                var response = await this.CreateAccountOnProcessing(new FrontDesk.Data.Account
                {
                    AccountNumberManagementPosition = modell.PositionNumber,
                    AccountName = modell.Description + " " + GetBranchName(),// .BranchName,
                    AccountNumber = modell.AccountNumber,

                    AccountNumberNetwok = (modell.AccountNumber.PadRight(6, '0') + modell.PositionNumber.PadRight(3, '0') + GetBranchCode() + GetBranchCode()).PadRight(6, '0'),
                    AccountNumberCU = (modell.AccountNumber.PadRight(6, '0') + modell.PositionNumber.PadRight(3, '0') + GetBranchCode()).PadRight(9, '0'),
                    AccountCategoryId = "XXX",
                    AccountTypeId = "",
                    AccountOwnerId = GetBranchID(),
                    ChartOfAccountManagementPositionId = modell.Id,
                    BranchCode = GetBranchCode(),
                    OwnerBranchCode = GetBranchCode(),
                    
                    Id="XXX",
                    LiaisonBranchCode = item.AccountNumber.Substring(item.AccountNumber.Length - 3),

                    IsNormalCreation = false,
                   
                });

                accountData = await this.GetAccount(response.Id);   

                return accountData;
            }
              //await this.CreateAccountOnProcessing(FrontDesk.Data.Account.CreateAccountModel(item))).Data: account;
            return accountData;

        }

        public OperationTypes GetOperationType(AccountModel item)
        {
            return item.BookingDirection.ToUpper() == OperationTypes.CREDIT.ToString() ? OperationTypes.CREDIT : OperationTypes.DEBIT;
        }
    }
}

