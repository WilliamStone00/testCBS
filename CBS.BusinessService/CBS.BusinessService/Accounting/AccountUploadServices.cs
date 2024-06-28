using CBS.API.Helper;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using CBS.FrontDesk.Service;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data;

namespace CBS.BusinessService.Accounting
{
 
    public class TrailBalanceUploudServices : BaseApiServices
    {
        private readonly ApiCallerHelper _loanConfigApiHelper;

        public string BranchId { get;  set; }
        public string BankId { get;  set; }
        public string OrganizationId { get;  set; }

        public TrailBalanceUploudServices()
        {
            _loanConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingBaseUrl"].ToString());
            this.BranchId = GetBranchID();
            this.BankId = GetBankID();
            this.OrganizationId = GetOrganizationID();
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objTrailBalanceUploud = await GetTrailBalanceUploud(id);
                if (objTrailBalanceUploud != null&&objTrailBalanceUploud.UploadStatus==true)
                {
                    string modelString = $"The upload done by {objTrailBalanceUploud.UserName} {objTrailBalanceUploud.BranchName} was 100% uploaded cannot be deleted";
                    // Handle failure scenario
                    GetExecutionMessages(objTrailBalanceUploud, false, modelString, MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, modelString);
                      return ExecutionMessage;
                }
                
                var inResponse = await _loanConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(string.Format(APICallHelper.Get_Update_Delete_TrailBalanceUploud, id), id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"The upload done by {objTrailBalanceUploud.UserName} {objTrailBalanceUploud.BranchName}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    GetExecutionMessages(inResponse, true, $"The upload done by {objTrailBalanceUploud.UserName} {objTrailBalanceUploud.BranchName}", MessagesResults.Failed,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }

        public async Task<IEnumerable<TrailBalanceUploud>> GetTrailBalanceUploud()
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<TrailBalanceUploud>>>(APICallHelper.GetAllTrailBalanceUploud);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<TrailBalanceUploud>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task< TrailBalanceUploud> GetTrailBalanceUploud(string id)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<TrailBalanceUploud>>(string.Format(APICallHelper.Get_Update_Delete_TrailBalanceUploud, id));
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
 
 

    }
}
