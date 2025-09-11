using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.MemberAdjustmentConsole;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Presentation;
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


        public MemberAdjustmentModel initialiseMemberAdjustmentModel(AdjustmentType adjustmentType, IndividualCustomerProfile model)
        {
            switch (adjustmentType)
            {
                 case AdjustmentType.NameAdjustment:
                    return new MemberAdjustmentModel()
                    {
                        MemberId = model.CustomerList.CustomerId,
                        OldFirstName = model.CustomerList.FirstName,
                        OldLastName = model.CustomerList.LastName,
                        NewFirstName = model.CustomerList.FirstName,
                        NewLastName = model.CustomerList.LastName,
                        IsMemberProfileModified = true,
                        BranchId = model.CustomerList.BranchId,
                        AdjustmentType = AdjustmentType.NameAdjustment.ToString(),
                    };
                case AdjustmentType.MemberActiveStatusAdjustment:
                  return new MemberAdjustmentModel()
                  {
                      MemberId = model.CustomerList.CustomerId,
                      OldStatus = model.CustomerList.Active,
                      OldMemberStatus = model.CustomerList.ActiveStatus,
                      OldMemberShipStatus = model.CustomerList.MembershipApprovalStatus,
                      IsMemberProfileModified = true,
                      BranchId = model.CustomerList.BranchId,
                      AdjustmentType = AdjustmentType.MemberActiveStatusAdjustment.ToString(),
                  };
                case AdjustmentType.MemberMembershipStatusAdjustment:
                    return new MemberAdjustmentModel()
                    {
                        MemberId = model.CustomerList.CustomerId,
                        OldStatus = model.CustomerList.Active,
                        OldMemberStatus = model.CustomerList.ActiveStatus,
                        OldMemberShipStatus = model.CustomerList.MembershipApprovalStatus,
                        IsMemberProfileModified = true,
                        BranchId = model.CustomerList.BranchId,
                        AdjustmentType = AdjustmentType.MemberMembershipStatusAdjustment.ToString(),
                    };

                case AdjustmentType.MemberActivationAdjustment:
                    return new MemberAdjustmentModel()
                    {
                        MemberId = model.CustomerList.CustomerId,
                        OldStatus = model.CustomerList.Active,
                        OldMemberStatus = model.CustomerList.ActiveStatus,
                        OldMemberShipStatus = model.CustomerList.MembershipApprovalStatus,
                        IsMemberProfileModified = true,
                        BranchId = model.CustomerList.BranchId,
                        AdjustmentType = AdjustmentType.MemberActivationAdjustment.ToString(),
                    };

                case AdjustmentType.MemberReferenceAdjustment:
                    return new MemberAdjustmentModel()
                    {
                        MemberId = model.CustomerList.CustomerId,
                        IsMemberProfileModified = true,
                        BranchId = model.CustomerList.BranchId,
                        AdjustmentType = AdjustmentType.MemberReferenceAdjustment.ToString(),
                    };

                case AdjustmentType.MemberCategoryAdjustment:
                    return new MemberAdjustmentModel()
                    {
                        MemberId = model.CustomerList.CustomerId,
                        OldMemberCategory = model.CustomerList.CustomerType,
                        IsMemberProfileModified = true,
                        BranchId = model.CustomerList.BranchId,
                        AdjustmentType = AdjustmentType.MemberCategoryAdjustment.ToString(),
                    };
                case AdjustmentType.AccountBalanceAdjustment:
                    return new MemberAdjustmentModel()
                    {
                        MemberId = model.CustomerList.CustomerId,
                        CustomerAccounts = model.CustomerAccounts,
                        IsMemberAccountModified = true,
                        BranchId = model.CustomerList.BranchId,
                        AdjustmentType = AdjustmentType.AccountBalanceAdjustment.ToString(),
                    };
                default:
                    return null;
            }

        }

        public SubmitMemberAdjustmentRequestCommand ConvertMemberAjustmentModel(MemberAdjustmentModel model)
        {
            return new SubmitMemberAdjustmentRequestCommand
            {
                // --- Core Identifiers ---
                MemberId = model.MemberId,
                BranchId = model.BranchId,
                AccountId = model.AccountId,
                NewMemberId = model.NewMemberId,
                AdjustmentType = model.AdjustmentType,
                // --- Name Change Properties ---
                NewFirstName = model.NewFirstName,
                OldFirstName = model.OldFirstName,
                NewLastName = model.NewLastName,
                OldLastName = model.OldLastName,
                BalanceSenseDifference=model.BalanceSenseDifference,
                // --- Balance Properties ---
                NewBalance = model.NewBalance,
                OldBalance = model.OldBalance,

                // --- Member Status Properties (string-based) ---
                NewMemberStatus = model.NewMemberStatus,
                OldMemberStatus = model.OldMemberStatus,
                NewMemberShipStatus = model.NewMemberShipStatus,
                OldMemberShipStatus = model.OldMemberShipStatus,

                // --- Status Properties (boolean-based) ---
                NewStatus = model.NewStatus,
                OldStatus = model.OldStatus,
                NewAccountStatus = model.NewAccountStatus,
                AccountStatus = model.AccountStatus,

                // --- Category Properties ---
                NewMemberCategory = model.NewMemberCategory,
                OldMemberCategory = model.OldMemberCategory,

                // --- Audit and Justification ---
                Reason = model.Reason,
                RequestedBy = model.RequestedBy,

                // --- Modification Flags ---
                IsMemberProfileModified = model.IsMemberProfileModified,
                IsMemberAccountModified = model.IsMemberAccountModified
            };
        }

        public async Task<ExecutionMessages> SubmitMemberAdjustmentRequestAsync(SubmitMemberAdjustmentRequestCommand command)
        {
            try
            {
                command.RequestedBy= GetUserFullName();
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
                command.ApprovedBy= GetUserFullName();
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
                command.RejectedBy= GetUserFullName();
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

        public async Task<MemberAdjustmentRequestDetailsWithHistoryDto> GetMemberAdjustmentRequestAsync(string RequestId)
        {
            try
            {
                var url = APICallHelper.GetMemberAdjustmentRequestDetails+RequestId;
                var response = await _MemberApiCaller.GetAsync<ResponseObject<MemberAdjustmentRequestDetailsWithHistoryDto>>(url);

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
