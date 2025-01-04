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

namespace CBS.BusinessService.Accounting
{
    public class AccountingServices : BaseService
    {
        private readonly ApiCallerHelper _accountingApiCallerHelper;

        public AccountingServices()
        {
            _accountingApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingBaseUrl"].ToString());

        }

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
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<FileReportInfoDto>>(string.Format(APICallHelper.Get_DownloadedFile,fileId));
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
        public async Task<ExecutionMessages> Create(Account model)
        {
            try
            {
                model.AccountNumberNetwok = "xxxxx";
                model.AccountTypeId = model.AccountNumber.Equals("45100")? model.AccountCounterPartId:"YYYYYY" ;
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
        public async Task<Account> GetAccount(string id)
        {
            try
            {
                var cusResponseObject =(await GetAllAccounting()).Where(i=>i.Id.Equals(id)).FirstOrDefault();
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
                var couApiResponse = (await GetAllReportInfo()).Find(x=>x.Id.Equals(Id));
                if (couApiResponse!=null)
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
        public async Task<Account> GetAccountByAccountNumber(string id)
        {
            try
            {

                string Url = string.Format(APICallHelper.GetAccountByAccountNumberUrl, id);
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<Account>>(Url);
                if (couApiResponse.IsSuccess)
                {
                    if (couApiResponse.ApiResponseData != null)
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }

                }
                return new  Account ();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<List<Account>> GetAllLiasionAccount(string BranchId)
        {
            try
            {

                string Url = string.Format(APICallHelper.GetSystemLiaisonAccountQueryUrl, BranchId);
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<Account>>>(Url);
                if (couApiResponse.IsSuccess)
                {
                    if (couApiResponse.ApiResponseData != null)
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }

                }
                return new List<Account>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw (ex);
            }
        }
        public async Task<List<Account>> GetAllBranchAccountUsedToCreditCashFlow(string BranchId)
        {
            try
            {

                string Url = string.Format(APICallHelper.GetAllBranchAccountUsedToCreditCashFlow, BranchId);
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<Account>>>(Url);
                if (couApiResponse.IsSuccess)
                {
                    if (couApiResponse.ApiResponseData != null)
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }

                }
                return new List<Account>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw (ex);
            }
        }
        public async Task<List<Account>> GetAllAccounting()
        {
            try
            {
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<Account>>>(APICallHelper.GetAlAccounts);
                if (couApiResponse.IsSuccess)
                {
                    if (couApiResponse.ApiResponseData != null)
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }

                }
                return new List<Account>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw(ex);
            }
        }
        public async Task<List<Account>> GetAllLiaisonAccount()
        {
            try
            {
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<Account>>>(APICallHelper.GetAllLiaisonAccount);
                if (couApiResponse.IsSuccess)
                {
                    if (couApiResponse.ApiResponseData != null)
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }

                }
                return new List<Account>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw (ex);
            }
        }
        public async Task<List<Account>> GetAllAccountForABranch(string branchId)
        {
            try
            {
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<Account>>>(string.Format(APICallHelper.GetAllAccountByBranch,branchId));
                if (couApiResponse.IsSuccess)
                {
                    if (couApiResponse.ApiResponseData != null)
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }

                }
                return new List<Account>();
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

        public async Task<ExecutionMessages> Update(Account account)
        {
            try
            {
                account.AccountOwnerId = GetBranchID();
                var response = await _accountingApiCallerHelper.PutAsync<ServiceResponse<Account>>(string.Format(APICallHelper.PutAccount, account.Id), account);
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
                    // Handle failure scenario
                    return GetExecutionMessages(account, false, $"{account.AccountNumber + " " + account.AccountName}", MessagesResults.Failed,
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

                if (list.AccountModelList.Count()==0)
                {
                    // Failed creation
                    GetExecutionMessages(list, true, $"NO element was seen in the file", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, $"NO element was seen in the file");
                }


                var response = await _accountingApiCallerHelper.PostUploadAccountResultResponseAsync(APICallHelper.CreateAccounOnUploadie, list);
                if (response!=null)
                {
                    if (response.isSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{list.AccountModelList[0].AccountNumber}", MessagesResults.Success,
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
                    // Failed creation
                    GetExecutionMessages(response, true, $"{list.AccountModelList[0].AccountNumber}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, $"{list.AccountModelList[0].AccountNumber}");
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

     
        public async Task<ExecutionMessages> PostJE(JEQuery model,string url)
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
            return (await GetAllReportInfo()).Find(x=>x.Equals(fileId));
        }
    }
}
