using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.LossManagementSystem;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace CBS.BusinessService.Config
{
    public class LossManagementService : BaseService
    {
        private readonly ApiCallerHelper _lossManagementApiHelper;

        public LossManagementService()
        {
            _lossManagementApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["CheckbookServiceBaseUrl"].ToString());
        }

        public async Task<CustomerCheckbooksDto> GetCustomerCheckbooks(string memberRef)
        {
            try
            {
                var response = await _lossManagementApiHelper.GetAsync<ResponseObject<CustomerCheckbooksDto>>(
                    string.Format(APICallHelper.GetCustomerCheckbooks, memberRef));

                return response.IsSuccess ? response.ApiResponseData.Data : null;
            }
            catch (Exception ex)
            {
                // Log exception
                throw ex;
            }
        }

        public async Task<CheckbookDetailDto> GetCheckLeavesByCheckbook(int checkBookId)
        {
            try
            {
                var response = await _lossManagementApiHelper.GetAsync<ResponseObject<CheckbookDetailDto>>(
                    string.Format(APICallHelper.GetCheckLeavesByCheckbook, checkBookId));

                return response.IsSuccess ? response.ApiResponseData.Data : null;
            }
            catch (Exception ex)
            {
                // Log exception
                throw ex;
            }
        }

        public async Task<CheckbookDetailDto> GetCheckbookDetails(int checkBookId)
        {
            try
            {
                var response = await _lossManagementApiHelper.GetAsync<ResponseObject<CheckbookDetailDto>>(
                    string.Format(APICallHelper.GetCheckbookDetails, checkBookId));

                return response.IsSuccess ? response.ApiResponseData.Data : null;
            }
            catch (Exception ex)
            {
                // Log exception
                throw ex;
            }
        }

        public async Task<LossRequestDto> RequestLossForCheckbook(int checkBookId)
        {
            try
            {
                var response = await _lossManagementApiHelper.GetAsync<ResponseObject<LossRequestDto>>(
                    string.Format(APICallHelper.RequestLossForCheckbook, checkBookId));

                return response.IsSuccess ? response.ApiResponseData.Data : null;
            }
            catch (Exception ex)
            {
                // Log exception
                throw ex;
            }
        }

        public async Task<LossRequestDto> RequestLossForCheckLeaf(int checkLeafId)
        {
            try
            {
                var response = await _lossManagementApiHelper.GetAsync<ResponseObject<LossRequestDto>>(
                    string.Format(APICallHelper.RequestLossForCheckLeaf, checkLeafId));

                return response.IsSuccess ? response.ApiResponseData.Data : null;
            }
            catch (Exception ex)
            {
                // Log exception
                throw ex;
            }
        }

        public async Task<ExecutionMessages> SubmitLossRequest(LossRequestDto request)
        {
            try
            {
                var response = await _lossManagementApiHelper.PostAsync<ServiceResponse<LossRequestDto>>(
                    APICallHelper.SubmitLossRequest, request);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response, true, "Loss Request", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                }
                else
                {
                    GetExecutionMessages(request, false, "Loss Request", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                }
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