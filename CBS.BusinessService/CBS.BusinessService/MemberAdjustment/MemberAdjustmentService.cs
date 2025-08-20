using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.MemberAdjustmentConsole;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.MemberP.MemberAdjustment
{
    public class MemberAdjustmentService : BaseService
    {
        private readonly ApiCallerHelper _MemberApiCaller;

        public MemberAdjustmentService()
        {
            _MemberApiCaller = new ApiCallerHelper(ConfigurationManager.AppSettings["CustomerBaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> SubmitMemberAdjustmentRequestAsync(SubmitMemberAdjustmentRequestCommand command)
        {
            try
            {
                command.RequestedBy=GetUserFullName();
                var response = await _MemberApiCaller.PostAsync<ServiceResponse<bool>>(APICallHelper.SubmitMemberAdjustmentRequest, command);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response, true, "Member Adjustment Submission", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                }
                else
                {
                    GetExecutionMessages(command, false, "Member Adjustment Submission", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                }

                return ExecutionMessage;
            }
            catch (Exception ex)
            {
                GetExecutionMessages(command, false, "Member Adjustment Submission", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Failed.ToString(), ex);
                return ExecutionMessage;
            }
        }

        public async Task<ExecutionMessages> ApproveMemberAdjustmentRequestAsync(ApproveMemberAdjustmentRequestCommand command)
        {
            try
            {
                command.ApprovedBy=GetUserFullName();
                var response = await _MemberApiCaller.PostAsync<ServiceResponse<bool>>(APICallHelper.ApproveMemberAdjustmentRequest, command);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response, true, "Member Adjustment Approval", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                }
                else
                {
                    GetExecutionMessages(command, false, "Member Adjustment Approval", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                }

                return ExecutionMessage;
            }
            catch (Exception ex)
            {
                GetExecutionMessages(command, false, "Member Adjustment Approval", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Failed.ToString(), ex);
                return ExecutionMessage;
            }
        }

        public async Task<ExecutionMessages> RejectMemberAdjustmentRequestAsync(RejectMemberAdjustmentRequestCommand command)
        {
            try
            {
                command.RejectedBy=GetUserFullName();
                var response = await _MemberApiCaller.PostAsync<ServiceResponse<bool>>(APICallHelper.RejectMemberAdjustmentRequest, command);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response, true, "Member Adjustment Rejection", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                }
                else
                {
                    GetExecutionMessages(command, false, "Member Adjustment Rejection", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                }

                return ExecutionMessage;
            }
            catch (Exception ex)
            {
                GetExecutionMessages(command, false, "Member Adjustment Rejection", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Failed.ToString(), ex);
                return ExecutionMessage;
            }
        }

        public async Task<MemberAdjustmentRequestDetailsDto> GetMemberAdjustmentRequestAsync(string RequestId)
        {
            try
            {
                var url = string.Format(APICallHelper.GetMemberAdjustmentRequestDetails, RequestId);
                var response = await _MemberApiCaller.GetAsync<ResponseObject<MemberAdjustmentRequestDetailsDto>>(url);

                return response.IsSuccess ? response.ApiResponseData.Data : null;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to retrieve Member adjustment request.", ex);
            }
        }
        public async Task<CustomDataTable> GetDataTableAsync(GetMemberAdjustmentRequestsDataTableQuery request)
        {
            request.Options.sortColumnName = "RequestedAt";

            if (!IsHeadOffice())
            {
                request.BranchId = GetBranchID();
            }

            var response = await _MemberApiCaller.PostAsync<ResponseObject<CustomDataTable>>(
                APICallHelper.MemberAdjustmentRequestDataTable,
                request
            );

            if (response.IsSuccess && response.ApiResponseData != null)
            {
                return response.ApiResponseData.Data;
            }

            return new CustomDataTable(
                draw: Convert.ToInt32(request.Options.draw),
                recordsTotal: 0,
                recordsFiltered: 0,
                data: new List<object>(),
                dataTableOptions: request.Options
            );
        }

        

    }

}
