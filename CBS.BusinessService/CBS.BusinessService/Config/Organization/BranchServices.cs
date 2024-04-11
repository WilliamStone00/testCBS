using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity.DataTable;
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
    public class BranchServices : BaseService
    {
        private readonly ApiCallerHelper _BranchConfigApiHelper;

        public BranchServices()
        {
            _BranchConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["BankConfigurationBaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objBranch = await GetBranch(id);
                var inResponse = await _BranchConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Get_Update_Delete_Branch, id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objBranch.Name}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, null);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objBranch, false, $"{objBranch.Name}", MessagesResults.Failed,
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
            Func<Task<List<Branch>>> getDataFunc = async () => (await GetBranches()).ToList();
            var dataTable = await DatatableHelper.GenerateDataTable<Branch>(dataTableOptions, getDataFunc);
            return dataTable;
        }
        public async Task<IEnumerable<Branch>> GetBranches()
        {
            try
            {
                if (IsHeadOffice())
                {
                    var couApiResponse = await _BranchConfigApiHelper.GetAsync<ResponseObject<List<Branch>>>(APICallHelper.GetAllBranch);
                    return couApiResponse.ApiResponseData.Data;

                }
                else
                {
                    var couApiResponse = await _BranchConfigApiHelper.GetAsync<ResponseObject<List<Branch>>>(APICallHelper.GetAllBranch);
                    return couApiResponse.ApiResponseData.Data.Where(x=>x.Id==GetBranchID());

                }

            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }

        public async Task<Branch> GetBranch(string id)
        {
            try
            {
                var cusResponseObject = await _BranchConfigApiHelper.GetAsync<ResponseObject<Branch>>(string.Format(APICallHelper.Get_Update_Delete_Branch, id));
                if (cusResponseObject.ApiResponseData!=null)
                {
                    return cusResponseObject.ApiResponseData.Data;
                }
                return new Branch();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<Branch> GetBranchByBankID(string bankid)
        {
            try
            {
                var cusResponseObject = await _BranchConfigApiHelper.GetAsync<ResponseObject<Branch>>(string.Format(APICallHelper.GetBranchesByBankID, bankid));
                return cusResponseObject.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<ExecutionMessages> Create(Branch model)
        {
            try
            {

                // Make an API call to create an individual profile
                model.BankId = GetBankID();
                var response = await _BranchConfigApiHelper.PostAsync<ServiceResponse<Branch>>(APICallHelper.CreateBranch, model);
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
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(),null, response.Message);
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
        public async Task<ExecutionMessages> Update(Branch model)
        {
            try
            {

                var branch = await GetBranch(model.Id);
                if (branch != null)
                {
                    branch.BranchCode = model.BranchCode;
                    branch.Name = model.Name;
                    branch.Address = model.Address;
                    branch.ImmatriculationNumber = model.ImmatriculationNumber;
                    branch.RegistrationNumber = model.RegistrationNumber;
                    branch.Capital = model.Capital;
                    branch.DateOfCreation = model.DateOfCreation;
                    branch.Email = model.Email;
                    branch.Telephone = model.Telephone;
                    branch.Location = model.Location;
                    branch.TaxPayerNUmber = model.TaxPayerNUmber;
                    branch.PBox = model.PBox;
                    branch.WebSite = model.WebSite;
                    branch.BankInitial = model.BankInitial;
                    branch.Motto = model.Motto;
                    branch.IsHeadOffice = model.IsHeadOffice;
                    branch.ActiveStatus = model.ActiveStatus;
                    branch.HeadOfficeTelehoneNumber = model.HeadOfficeTelehoneNumber;
                    branch.HeadOfficeAddress = model.HeadOfficeAddress;
                    var response = await _BranchConfigApiHelper.PutAsync<ServiceResponse<Teller>>(string.Format(APICallHelper.Get_Update_Delete_Branch, model.Id), branch);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{branch.Name}", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, $"{branch.Name}", MessagesResults.Failed,
                            ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                    }
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
        public async Task<ExecutionMessages> UploadBranchLogo(CustomerDocumentRequest attachedToLoan)
        {
            try
            {
                var additionalParams = new Dictionary<string, string>
                {
                    { "BranchID", GetBranchID() }
                };
                var response = await _BranchConfigApiHelper.PostFilesAndParamsAsync<ServiceResponse<Bank>>(APICallHelper.UpdateBranchLogo, additionalParams, attachedToLoan.AttachedFiles);
                if (response.ApiResponseData.Success)
                {
                    GetExecutionMessages(response, true, attachedToLoan.DocumentType, MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null,
                        null);
                    return ExecutionMessage;
                }
                GetExecutionMessages(attachedToLoan, false, attachedToLoan.DocumentType, MessagesResults.Failed,
                    ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null,
                    null);
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
