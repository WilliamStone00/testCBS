using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.AccountingV2.LiaisonMapping;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.AccountingV2.LiaisonAccount2
{
    public class LiaisonMappingService : BaseService
    {
        private readonly ApiCallerHelper _apiCaller;

        public LiaisonMappingService()
        {
            _apiCaller = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingBaseUrl"].ToString());
        }

        public async Task<ExecutionMessages> CreateOrUpdateLiaisonMappingAsync(CreateOrUpdateLiaisonMappingCommand command)
        {
            try
            {
                var endpoint = string.IsNullOrEmpty(command.Id) ?
                    APICallHelper.CreateLiaisonMapping :
                    APICallHelper.UpdateLiaisonMapping;

                var response = await _apiCaller.PostAsync<ServiceResponse<LiaisonMappingDto>>(endpoint, command);

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

        public async Task<CustomDataTable> GetDataTableAsync(GetLiaisonMappingsDataTableQuery request)
        {
            try
            {
                // ✅ Empêche NullReferenceException
                if (request.Options == null)
                    request.Options = new DataTableOptions();

                request.Options.sortColumnName = request.Options.sortColumnName ?? "BranchName";

                if (!IsHeadOffice())
                {
                    request.BranchId = GetBranchID();
                }

                var response = await _apiCaller.PostAsync<ResponseObject<CustomDataTable>>(
                    APICallHelper.GetLiaisonMappingsDataTable,
                    request
                );

                if (response.IsSuccess && response.ApiResponseData != null)
                {
                    return response.ApiResponseData.Data;
                }

                return new CustomDataTable(
                    draw: Convert.ToInt32(request.Options.draw ?? "1"),
                    recordsTotal: 0,
                    recordsFiltered: 0,
                    data: new List<object>(),
                    dataTableOptions: request.Options
                );
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to retrieve liaison mapping.", ex);
            }
        }

    }
}

