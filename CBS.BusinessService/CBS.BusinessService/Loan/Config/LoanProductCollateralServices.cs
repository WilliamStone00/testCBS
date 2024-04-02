using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.LoanConf;
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
    public class LoanProductCollateralServices : BaseService
    {
        private readonly ApiCallerHelper _loanConfigApiHelper;

        public LoanProductCollateralServices()
        {
            _loanConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["LoanBaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objLoanProductCollateral = await GetLoanProductCollateral(id);
                var inResponse = await _loanConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(string.Format(APICallHelper.Get_Update_Delete_LoanProductCollateral, id), id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objLoanProductCollateral.LoanProductCollateralTag}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objLoanProductCollateral, false, $"{objLoanProductCollateral.LoanProductCollateralTag}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }
        
        public async Task<IEnumerable<LoanProductCollateral>> GetLoanProductCollaterals()
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<LoanProductCollateral>>>(APICallHelper.GetAllLoanProductCollateral);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<LoanProductCollateral>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        //GetAllLaonApplicationCollateralByApplicationIdQuery
        public async Task<IEnumerable<LoanApplicationCollateral>> GetAllLaonApplicationCollateralByApplicationIdQuery(string loanApplicationId)
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<LoanApplicationCollateral>>>(string.Format(APICallHelper.GetAllLaonApplicationCollateralByApplicationIdQuery, loanApplicationId));
                if (couApiResponse != null && couApiResponse.IsSuccess)
                {
                    var results= couApiResponse.ApiResponseData.Data;

                    return results;
                }
                return new List<LoanApplicationCollateral>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<IEnumerable<StringValues>> GetLoanProductCollaterals(string productId)
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<LoanProductCollateral>>>(APICallHelper.GetAllLoanProductCollateral);
                if (couApiResponse!=null && couApiResponse.IsSuccess)
                {
                    var results=(from a in couApiResponse.ApiResponseData.Data where a.LoanProductId==productId select 
                                 new StringValues { Text = $"{a.LoanProductCollateralTag}", Value = a.Id }).ToList();

                    return results;
                }
                return new List<StringValues>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<LoanProductCollateral> GetLoanProductCollateral(string id)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<LoanProductCollateral>>(string.Format(APICallHelper.Get_Update_Delete_LoanProductCollateral, id));
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
        public async Task<ExecutionMessages> Create(LoanProductCollateral model)
        {
            try
            {

                // Make an API call to create an individual profile
                var response = await _loanConfigApiHelper.PostAsync<ServiceResponse<LoanProductCollateral>>(APICallHelper.CreateLoanProductCollateral, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.LoanProductCollateralTag}", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, model.LoanProductCollateralTag, MessagesResults.Failed,
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
        public async Task<ExecutionMessages> Update(LoanProductCollateral model)
        {
            try
            {

                var LoanProductCollateral = await GetLoanProductCollateral(model.Id);
                if (LoanProductCollateral != null)
                {
                    LoanProductCollateral.CollateralId = model.CollateralId;
                    LoanProductCollateral.LoanProductId = model.LoanProductId;
                    LoanProductCollateral.MinimumValueRate = model.MinimumValueRate;
                    LoanProductCollateral.MaximumValueRate = model.MaximumValueRate;
                    LoanProductCollateral.LoanProductCollateralTag = model.LoanProductCollateralTag;
                    var response = await _loanConfigApiHelper.PutAsync<ServiceResponse<LoanProductCollateral>>(string.Format(APICallHelper.Get_Update_Delete_LoanProductCollateral, model.Id), LoanProductCollateral);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{model.LoanProductCollateralTag}", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, model.LoanProductCollateralTag, MessagesResults.Failed,
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

    }

}
