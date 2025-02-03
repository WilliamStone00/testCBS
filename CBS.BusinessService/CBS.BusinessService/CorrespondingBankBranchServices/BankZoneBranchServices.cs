using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.CorrespondingBankManaagement;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System.Collections.Generic;
using System;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Services
{

    public class BankZoneBranchServices : BaseService
    {
        private readonly ApiCallerHelper _bankConfigApiHelper;

        public BankZoneBranchServices()
        {
            _bankConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["BankConfigurationBaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objBankZoneBranch = await GetBankZoneBranch(id);
                var inResponse = await _bankConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(string.Format(APICallHelper.Get_Update_Delete_BankZoneBranch, id), id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objBankZoneBranch.BankingZoneId}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objBankZoneBranch, false, $"{objBankZoneBranch.BankingZoneId}", MessagesResults.Failed,
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
            Func<Task<List<BankZoneBranch>>> getDataFunc = async () => (await GetBankZoneBranch()).ToList();
            var dataTable = await DatatableHelper.GenerateDataTable<BankZoneBranch>(dataTableOptions, getDataFunc);
            return dataTable;
        }
        public async Task<IEnumerable<BankZoneBranch>> GetBankZoneBranch()
        {
            try
            {
                var couApiResponse = await _bankConfigApiHelper.GetAsync<ResponseObject<List<BankZoneBranch>>>(APICallHelper.GetAllBankZoneBranch);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<BankZoneBranch>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        //
        public async Task<BankZoneBranch> GetBankZoneBranch(string id)
        {
            try
            {
                var cusResponseObject = await _bankConfigApiHelper.GetAsync<ResponseObject<BankZoneBranch>>(string.Format(APICallHelper.Get_Update_Delete_BankZoneBranch, id));
                if (cusResponseObject.IsSuccess)
                {
                    return cusResponseObject.ApiResponseData.Data;
                }
                return null;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        //public async Task<BankZoneBranchDto> GetBankZoneBranchDto(string id)
        //{
        //    try
        //    {
 
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log and handle exception
        //        throw ex;
        //    }
        //}
        public async Task<ExecutionMessages> Create(BankZoneBranchObj model)
        {
            try
            {

                // Make an API call to create an individual profile
                var response = await _bankConfigApiHelper.PostAsync<ServiceResponse<BankZoneBranchObj>>(APICallHelper.CreateBankZoneBranch, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.BankingZoneId}", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, null);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, model.BankingZoneId, MessagesResults.Failed,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null, response.Message);
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
        public async Task<List<Branch3ppBranch>> GetAllBranchBankCorrespondingBranchByZoneId(string id)
        {
            try
            {
                var cusResponseObject = await _bankConfigApiHelper.GetAsync<ResponseObject<List<Branch3ppBranch>>>(string.Format(APICallHelper.Get_BankZoneBranch_by_ZoneID, id));
                if (cusResponseObject.IsSuccess)
                {
                    return cusResponseObject.ApiResponseData.Data;
                }
                return null;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }

        public async Task<List<Branch3ppBranch>> GetAllBranchPresentInZoneByParticipant(string id,string type)
        {
            try
            {
                var cusResponseObject = await _bankConfigApiHelper.GetAsync<ResponseObject<List<Branch3ppBranch>>>(string.Format(APICallHelper.Get_BankZoneBranchbyBranchId, id,type));
                if (cusResponseObject.IsSuccess)
                {
                    return cusResponseObject.ApiResponseData.Data;
                }
                return null;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<ExecutionMessages> Update(BankZoneBranch model)
        {
            try
            {

                var BankZoneBranch = await GetBankZoneBranch(model.Id);
                if (BankZoneBranch != null)
                {
                    BankZoneBranch.BankingZoneId = model.BankingZoneId;
                    BankZoneBranch.BranchId = model.BranchId;
                    var response = await _bankConfigApiHelper.PutAsync<ServiceResponse<BankZoneBranch>>(string.Format(APICallHelper.Get_Update_Delete_BankZoneBranch, model.Id), BankZoneBranch);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{model.BranchId}", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, model.BranchId, MessagesResults.Failed,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, response.Message);
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

    }
}

 