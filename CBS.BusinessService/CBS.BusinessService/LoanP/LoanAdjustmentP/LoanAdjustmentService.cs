using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.LoanAdjustmentP;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.LoanP.LoanAdjustmentP
{
    public class LoanAdjustmentService : BaseService
    {
        private readonly ApiCallerHelper _loanApiCaller;

        public LoanAdjustmentService()
        {
            _loanApiCaller = new ApiCallerHelper(ConfigurationManager.AppSettings["LoanBaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> SubmitLoanAdjustmentRequestAsync(SubmitLoanAdjustmentRequestCommand command)
        {
            try
            {
                command.RequestedBy=GetUserFullName();
                var response = await _loanApiCaller.PostAsync<ServiceResponse<bool>>(APICallHelper.SubmitLoanAdjustmentRequest, command);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response, true, "Loan Adjustment Submission", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                }
                else
                {
                    GetExecutionMessages(command, false, "Loan Adjustment Submission", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                }

                return ExecutionMessage;
            }
            catch (Exception ex)
            {
                GetExecutionMessages(command, false, "Loan Adjustment Submission", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Failed.ToString(), ex);
                return ExecutionMessage;
            }
        }

        public async Task<ExecutionMessages> ApproveLoanAdjustmentRequestAsync(ApproveLoanAdjustmentRequestCommand command)
        {
            try
            {
                command.ApprovedBy=GetUserFullName();
                var response = await _loanApiCaller.PostAsync<ServiceResponse<bool>>(APICallHelper.ApproveLoanAdjustmentRequest, command);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response, true, "Loan Adjustment Approval", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                }
                else
                {
                    GetExecutionMessages(command, false, "Loan Adjustment Approval", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                }

                return ExecutionMessage;
            }
            catch (Exception ex)
            {
                GetExecutionMessages(command, false, "Loan Adjustment Approval", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Failed.ToString(), ex);
                return ExecutionMessage;
            }
        }

        public async Task<ExecutionMessages> RejectLoanAdjustmentRequestAsync(RejectLoanAdjustmentRequestCommand command)
        {
            try
            {
                command.RejectedBy=GetUserFullName();
                var response = await _loanApiCaller.PostAsync<ServiceResponse<bool>>(APICallHelper.RejectLoanAdjustmentRequest, command);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response, true, "Loan Adjustment Rejection", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                }
                else
                {
                    GetExecutionMessages(command, false, "Loan Adjustment Rejection", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                }

                return ExecutionMessage;
            }
            catch (Exception ex)
            {
                GetExecutionMessages(command, false, "Loan Adjustment Rejection", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Failed.ToString(), ex);
                return ExecutionMessage;
            }
        }

        public async Task<LoanAdjustmentRequestDetailsDto> GetLoanAdjustmentRequestAsync(string RequestId)
        {
            try
            {
                var url = string.Format(APICallHelper.GetLoanAdjustmentRequestDetails, RequestId);
                var response = await _loanApiCaller.GetAsync<ResponseObject<LoanAdjustmentRequestDetailsDto>>(url);

                return response.IsSuccess ? response.ApiResponseData.Data : null;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to retrieve loan adjustment request.", ex);
            }
        }
        public async Task<CustomDataTable> GetDataTableAsync(GetLoanAdjustmentRequestsDataTableQuery request)
        {
            request.Options.sortColumnName = "RequestedAt";

            if (!IsHeadOffice())
            {
                request.BranchId = GetBranchID();
            }

            var response = await _loanApiCaller.PostAsync<ResponseObject<CustomDataTable>>(
                APICallHelper.LoanAdjustmentRequestDataTable,
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
