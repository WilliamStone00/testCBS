using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.AccountingV2.RoleGlResolution;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace CBS.BusinessService.AccountingV2.RoleGlResolution
{
    public class RoleGlResolutionService : BaseService
    {
        private readonly ApiCallerHelper _apiCaller;

        public RoleGlResolutionService()
        {
            _apiCaller = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingBaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> CreateOrUpdateRoleGlResolutionAsync(CreateOrUpdateRoleGlResolutionCommand command)
        {
            try
            {
                var endpoint = string.IsNullOrEmpty(command.Id) ?
                    APICallHelper.CreateRoleGlResolution :
                    APICallHelper.UpdateRoleGlResolution;

                var response = await _apiCaller.PostAsync<ServiceResponse<RoleGlResolutionDto>>(endpoint, command);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response, true, "Role GL Resolution", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                }
                else
                {
                    GetExecutionMessages(command, false, "Role GL Resolution", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                }

                return ExecutionMessage;
            }
            catch (Exception ex)
            {
                GetExecutionMessages(command, false, "Role GL Resolution", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Failed.ToString(), ex);
                return ExecutionMessage;
            }
        }

        public async Task<ExecutionMessages> DeleteRoleGlResolutionAsync(string id)
        {
            try
            {
                var response = await _apiCaller.DeleteAsync<ServiceResponse<bool>>($"{APICallHelper.DeleteRoleGlResolution}/{id}");

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response, true, "Role GL Resolution Deletion", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                }
                else
                {
                    GetExecutionMessages(null, false, "Role GL Resolution Deletion", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                }

                return ExecutionMessage;
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, "Role GL Resolution Deletion", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Failed.ToString(), ex);
                return ExecutionMessage;
            }
        }

        public async Task<RoleGlResolutionDto> GetRoleGlResolutionAsync(string id)
        {
            try
            {
                var response = await _apiCaller.GetAsync<ResponseObject<RoleGlResolutionDto>>($"{APICallHelper.GetRoleGlResolution}/{id}");
                return response.IsSuccess ? response.ApiResponseData.Data : null;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to retrieve Role GL Resolution.", ex);
            }
        }

        public async Task<CustomDataTable> GetDataTableAsync(GetRoleGlResolutionsDataTableQuery request)
        {
            request.Options.sortColumnName = request.Options.sortColumnName ?? "CreatedAt";

            if (!IsHeadOffice() && string.IsNullOrEmpty(request.BranchId))
            {
                request.BranchId = GetBranchID();
            }

            var response = await _apiCaller.PostAsync<ResponseObject<CustomDataTable>>(
                APICallHelper.GetRoleGlResolutionsDataTable,
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