using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Config
{
    public class BankServices : BaseService
    {
        private readonly ApiCallerHelper _bankConfigApiHelper;
        private readonly ApiCallerHelper _identityServerBaseUrl;

        public BankServices()
        {
            _bankConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["BankConfigurationBaseUrl"].ToString());
            _identityServerBaseUrl = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objBank = await GetBank(id);
                var inResponse = await _bankConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Get_Update_Delete_Bank, id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objBank.Name}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, null);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objBank, false, $"{objBank.Name}", MessagesResults.Failed,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(), null, null);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }
        public async Task<CustomDataTable> GetDataTable(DataTableOptions dataTableOptions)
        {
            Func<Task<List<Bank>>> getDataFunc = async () => (await GetBanks()).ToList();
            var dataTable = await DatatableHelper.GenerateDataTable<Bank>(dataTableOptions, getDataFunc);
            return dataTable;
        }
       
        public async Task<IEnumerable<Bank>> GetBanks()
        {
            try
            {
                if (IsHeadOffice())
                {
                    var couApiResponse = await _bankConfigApiHelper.GetAsync<ResponseObject<List<Bank>>>(APICallHelper.GetAllBank);
                    return couApiResponse.ApiResponseData.Data;

                }
                else
                {
                    var couApiResponse = await _bankConfigApiHelper.GetAsync<ResponseObject<List<Bank>>>(APICallHelper.GetAllBank);
                    return couApiResponse.ApiResponseData.Data.Where(x => x.Id == GetBankID());

                }

            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<Bank> GetBank(string id)
        {
            try
            {
                var cusResponseObject = await _bankConfigApiHelper.GetAsync<ResponseObject<Bank>>(string.Format(APICallHelper.Get_Update_Delete_Bank, id));
                return cusResponseObject.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<ExecutionMessages> Create(Bank model)
        {
            try
            {

                // Make an API call to create an individual profile
                var response = await _bankConfigApiHelper.PostAsync<ServiceResponse<Bank>>(APICallHelper.CreateBank, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.Name}", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, null);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, model.Name, MessagesResults.Failed,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null, null);
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
        public async Task<ExecutionMessages> Update(Bank model)
        {
            try
            {
                var bank = await GetBank(model.Id);
                if (bank == null)
                {
                    GetExecutionMessages(model, false, "Bank not found", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString());
                    return ExecutionMessage;
                }

                MapBankUpdates(bank, model);

                // ✅ Use the correct response type
                var url = string.Format(APICallHelper.Get_Update_Delete_Bank, model.Id);
                var response = await _bankConfigApiHelper
                    .PutAsync<ServiceResponse<Bank>>(url, bank);

                if (response?.IsSuccess == true)
                {
                    GetExecutionMessages(response, true, $"{bank.Name}", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }

                GetExecutionMessages(model, false, $"{bank.Name}", MessagesResults.Failed,
                    ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response?.Message);
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
            }

            return ExecutionMessage;
        }
        private static void MapBankUpdates(Bank bank, Bank src)
        {
            string T(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

            // Core identity
            bank.BankCode  = T(src.BankCode);
            bank.Name      = T(src.Name);
            bank.BankInitial = T(src.BankInitial);
            bank.Description = string.IsNullOrWhiteSpace(src.Description) ? "N/A" : src.Description.Trim();

            // Contact
            bank.Telephone = T(src.Telephone);
            bank.Fax       = T(src.Fax);
            bank.Email     = T(src.Email);
            bank.Address   = T(src.Address);
            bank.PBox      = T(src.PBox);
            bank.CustomerServiceContact = T(src.CustomerServiceContact);

            // Registration & Tax
            bank.RegistrationNumber     = T(src.RegistrationNumber);
            bank.ImmatriculationNumber  = T(src.ImmatriculationNumber);
            bank.TaxPayerNUmber         = T(src.TaxPayerNUmber);
            bank.RegistrationInformation = T(src.RegistrationInformation);

            // Organization / category
            bank.OrganizationId       = "1";
            bank.CategoryInformation  = T(src.CategoryInformation);
            bank.ShortHeaderInfo      = T(src.ShortHeaderInfo);

            // Branding & web
            bank.WebSite  = T(src.WebSite);
            bank.Motto    = T(src.Motto);
            if (!string.IsNullOrWhiteSpace(src.LogoUrl))
                bank.LogoUrl = src.LogoUrl.Trim(); // keep existing if none provided

            // Financials / dates
            bank.Capital = string.IsNullOrWhiteSpace(src.Capital) ? "0" : src.Capital.Trim();

            // If your entity uses DateTime/DateTime? instead of string, parse safely:
            // if (DateTime.TryParse(src.DateOfCreation, out var dc)) bank.DateOfCreation = dc; else keep current
            bank.DateOfCreation = src.DateOfCreation; // keep as-is if your type is string
        }

        public async Task<ExecutionMessages> UploadBankLogo(CustomerDocumentRequest documentRequest)
        {
            try
            {
               
                var additionalParams = new Dictionary<string, string>
                {
                    { "OperationID", documentRequest.CustomerID },
                    { "DocumentId", "N/A" },
                    { "DocumentType", "Bank Logo" },
                    { "ServiceType", "BankMicroservice" },
                    { "CallBackBaseUrl",ConfigurationManager.AppSettings["BankConfigurationBaseUrl"].ToString()},
                    { "CallBackEndPoint", APICallHelper.AttachedDocumentsRemotelyBank},
                    { "RemoteFilePath", $"BankLogo" },
                };
                var response = await _identityServerBaseUrl.PostFilesAndParamsAsync<DocumentAttachedToLoan>(APICallHelper.AttachedDocuments, additionalParams, documentRequest.AttachedFiles);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null,
                        response.Message);
                    return ExecutionMessage;
                }
                GetExecutionMessages(documentRequest, false, null, MessagesResults.Failed,
                    ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
            }
            return ExecutionMessage;
        }
        public async Task<ExecutionMessages> UploadBankWaterM(CustomerDocumentRequest documentRequest)
        {
            try
            {

                var additionalParams = new Dictionary<string, string>
                {
                    { "OperationID", documentRequest.CustomerID },
                    { "DocumentId", "N/A" },
                    { "DocumentType", "Bank Water Mark" },
                    { "ServiceType", "BankMicroservice" },
                    { "CallBackBaseUrl",ConfigurationManager.AppSettings["BankConfigurationBaseUrl"].ToString()},
                    { "CallBackEndPoint", APICallHelper.AttachedDocumentsRemotelyWaterMark},
                    { "RemoteFilePath", $"BankWaterMark" },
                };
                var response = await _identityServerBaseUrl.PostFilesAndParamsAsync<DocumentAttachedToLoan>(APICallHelper.AttachedDocuments, additionalParams, documentRequest.AttachedFiles);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null,
                        response.Message);
                    return ExecutionMessage;
                }
                GetExecutionMessages(documentRequest, false, null, MessagesResults.Failed,
                    ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
            }
            return ExecutionMessage;
        }

    }
}
