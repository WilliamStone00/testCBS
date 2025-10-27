using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.AccountingV2.LiaisonMapping;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace CBS.BusinessService.AccountingV2.LiaisonAccount2
{
    public class LiaisonMappingService : BaseService
    {
        private readonly ApiCallerHelper _apiCaller;

        public LiaisonMappingService()
        {
            _apiCaller = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingV2BaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> CreateOrUpdateLiaisonMappingAsync(CreateOrUpdateLiaisonMappingCommand command)
        {
            try
            {
                var endpoint = string.IsNullOrEmpty(command.Id)
                    ? APICallHelper.CreateLiaisonMapping
                    : APICallHelper.UpdateLiaisonMapping;

                var response = await _apiCaller.PostAsync<ServiceResponse<CreateOrUpdateLiaisonMappingCommand>>(endpoint, command);

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response, true, "Liaison Mapping", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                }
                else
                {
                    GetExecutionMessages(command, false, "Liaison Mapping", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                }

                return ExecutionMessage;
            }
            catch (Exception ex)
            {
                GetExecutionMessages(command, false, "Liaison Mapping", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Failed.ToString(), ex);
                return ExecutionMessage;
            }
        }

        public async Task<ExecutionMessages> DeleteLiaisonMappingAsync(string id)
        {
            try
            {
                var response = await _apiCaller.DeleteAsync<ServiceResponse<bool>>($"{APICallHelper.DeleteLiaisonMapping}/{id}");

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response, true, "Liaison Mapping Deletion", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                }
                else
                {
                    GetExecutionMessages(null, false, "Liaison Mapping Deletion", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                }

                return ExecutionMessage;
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, "Liaison Mapping Deletion", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Failed.ToString(), ex);
                return ExecutionMessage;
            }
        }

        public async Task<LiaisonMappingDto> GetLiaisonMappingAsync(string id)
        {
            try
            {
                var response = await _apiCaller.GetAsync<ResponseObject<LiaisonMappingDto>>($"{APICallHelper.GetLiaisonMapping}/{id}");
                return response.IsSuccess ? response.ApiResponseData.Data : null;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to retrieve liaison mapping.", ex);
            }
        }

        public async Task<CustomDataTable2> GetDataTableAsync(GetLiaisonMappingsDataTableQuery request)
        {
            try
            {
                if (request.Options == null)
                    request.Options = new DataTableOptions();

                request.Options.sortColumnName = "BranchName";

                // ✅ Ensure BranchId is always initialized
                if (!IsHeadOffice())
                {
                    request.BranchId = GetBranchID();
                }
                else
                {
                    // Explicitly set to empty string for Head Office instead of null
                    request.BranchId = string.Empty;
                }

                var response = await _apiCaller.PostAsync<ResponseObject<CustomDataTable2>>(
                    APICallHelper.GetLiaisonMappingsDataTable,
                    request
                );

                if (response.IsSuccess && response.ApiResponseData != null)
                    return response.ApiResponseData.Data;

                return new CustomDataTable2(
                    draw: Convert.ToInt32(request.Options.draw ?? "1"),
                    recordsTotal: 0,
                    recordsFiltered: 0,
                    data: new List<object>(),
                    dataTableOptions: request.Options
                );
            }
            catch (Exception ex)
            {
                // Log internally, but provide clean message to front-end
                throw new ApplicationException("Failed to retrieve liaison mapping. " + ex.Message, ex);
            }
        }

        // Removed unsupported methods that reference undefined endpoints and types
        // - GetLiaisonMappingsByBranchAsync
        // - CheckMappingExistsAsync  
        // - GetCounterpartyBranchesAsync
    }
}