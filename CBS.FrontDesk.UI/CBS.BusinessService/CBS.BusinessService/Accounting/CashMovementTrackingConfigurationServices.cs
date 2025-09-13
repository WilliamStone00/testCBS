using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.CashMovementTracker;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting
{
    public class CashMovementTrackerServices : BaseService
    {
        private readonly ApiCallerHelper _loanConfigApiHelper;
 
        public BranchServices branchServices { get;  set; }
        public CashMovementTrackerServices()
        {
            _loanConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingBaseUrl"].ToString());

        }
        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
             
                var objCashMovementTracker = await GetCashMovementTracker(id);
                var inResponse = await _loanConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(string.Format(APICallHelper.Delete_CashMovementTracker, id), id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objCashMovementTracker.ReferenceId}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);


                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objCashMovementTracker, false, $"{objCashMovementTracker.ReferenceId}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }

        public async Task<IEnumerable<CashMovementTracker>> GetCashMovementTracker()
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<CashMovementTracker>>>(APICallHelper.GetAllCashMovementTracker);
                if (couApiResponse.IsSuccess)
                {

                    if (couApiResponse.ApiResponseData == null)
                    {
                        return new List<CashMovementTracker>();
                    }
                    else
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }
                }
                return new List<CashMovementTracker>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<CashMovementTracker> GetCashMovementTracker(string id)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<CashMovementTracker>>(string.Format(APICallHelper.Get_CashMovementTracker, id));
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

        public async Task<ExecutionMessages> Update(CashMovementTracker model)
        {
            try
            {

                var CashMovementTracker = await GetCashMovementTracker(model.Id);
                if (CashMovementTracker != null)
                {
                    CashMovementTracker.OperationType = model.OperationType;

                    CashMovementTracker.Id = model.Id;
                    CashMovementTracker.ReferenceId = model.ReferenceId;
                    CashMovementTracker.Constraint = model.Constraint;
                    CashMovementTracker.DoneBy = UserID;
                    CashMovementTracker.Status = model.Status;
                    CashMovementTracker.StartTime = model.StartTime;
                    CashMovementTracker.ExpectedEndTime = model.ExpectedEndTime;
                    CashMovementTracker.EndTime = model.EndTime;
                    var response = await _loanConfigApiHelper.PutAsync<ServiceResponse<CashMovementTracker>>(string.Format(APICallHelper.Update_CashMovementTracker, model.Id), CashMovementTracker);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{model.ReferenceId}", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, (string)model.ReferenceId, MessagesResults.Failed,
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

        public async Task<ExecutionMessages> Create(CashMovementTracker model)
        {
            try
            {

                // Make an API call to create an individual profile

                //model.BankId = this.BankId;
                //model.BranchId= this .BranchId;
                //model.OrganizationId= this .OrganizationId;
                var response = await _loanConfigApiHelper.PostAsync<ServiceResponse< CashMovementTracker>>(APICallHelper.CreateCashMovementTracker, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.ReferenceId}", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, model.ReferenceId, MessagesResults.Failed,
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


    }
}
