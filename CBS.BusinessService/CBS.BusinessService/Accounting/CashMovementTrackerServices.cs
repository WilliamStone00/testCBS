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
    public class CashMovementTrackingConfigurationServices : BaseService
    {
        private readonly ApiCallerHelper _loanConfigApiHelper;

        public BranchServices branchServices { get; set; }
        public CashMovementTrackingConfigurationServices()
        {
            _loanConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingBaseUrl"].ToString());
            branchServices = new BranchServices();
        }
        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {

                var objCashMovementTrackingConfiguration = await GetCashMovementTrackingConfiguration(id);
                var inResponse = await _loanConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(string.Format(APICallHelper.Delete_ChartOfAccount, id), id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"{objCashMovementTrackingConfiguration.Name}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);


                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objCashMovementTrackingConfiguration, false, $"{objCashMovementTrackingConfiguration.Name}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }

        public async Task<IEnumerable<CashMovementTrackingConfiguration>> GetCashMovementTrackingConfiguration()
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<CashMovementTrackingConfiguration>>>(APICallHelper.GetAllCashMovementTrackerConfiguration);
                if (couApiResponse.IsSuccess)
                {

                    if (couApiResponse.ApiResponseData == null)
                    {
                        return new List<CashMovementTrackingConfiguration>();
                    }
                    else
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }
                }
                return new List<CashMovementTrackingConfiguration>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<CashMovementTrackingConfiguration> GetCashMovementTrackingConfiguration(string id)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<CashMovementTrackingConfiguration>>(string.Format(APICallHelper.Get_CashMovementTrackerConfiguration, id));
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

        public async Task<ExecutionMessages> Update(CashMovementTrackingConfiguration model)
        {
            try
            {

                var CashMovementTrackingConfiguration = await GetCashMovementTrackingConfiguration(model.Id);
                if (CashMovementTrackingConfiguration != null)
                {

                    CashMovementTrackingConfiguration.AlertTimeAfter = model.AlertTimeAfter;
                    CashMovementTrackingConfiguration.AlertTimeBefore = model.AlertTimeBefore;
                    CashMovementTrackingConfiguration.MessageAfterAlertTime = model.MessageAfterAlertTime;
                    CashMovementTrackingConfiguration.MessageBeforeAlertTime = model.MessageBeforeAlertTime;
                    CashMovementTrackingConfiguration.From = model.From;
                    CashMovementTrackingConfiguration.To = model.To;
                    CashMovementTrackingConfiguration.MovementType = model.MovementType;
                    CashMovementTrackingConfiguration.Duration = model.Duration;

                    var response = await _loanConfigApiHelper.PutAsync<ServiceResponse<CashMovementTrackingConfiguration>>(string.Format(APICallHelper.Update_CashMovementTrackerConfiguration, model.Id), CashMovementTrackingConfiguration);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"{model.Name}", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, (string)model.Name, MessagesResults.Failed,
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

        public async Task<ExecutionMessages> Create(CashMovementTrackingConfiguration model)
        {
            try
            {

                // Make an API call to create an individual profile
                var From = (await branchServices.GetBranch(model.From));
                var To = model.MovementType == "Branch-To-Branch" ? (await branchServices.GetBranch(model.From)) : (await branchServices.GetBranch(model.From));
                model.Name = $"Cash movement from {From.Name} to {To.Name}";
                var response = await _loanConfigApiHelper.PostAsync<ServiceResponse<CashMovementTrackingConfiguration>>(APICallHelper.CreateCashMovementTrackerConfiguration, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"{model.Name}", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, model.Name, MessagesResults.Failed,
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
