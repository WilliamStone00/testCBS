
using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.SalaryManagement;
using CBS.FrontDesk.Data.LoanAdjustmentP;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using DocumentFormat.OpenXml.EMMA;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounts
{
   
    public class SalaryProcessedServices : BaseService
    {
        private readonly ApiCallerHelper _transactionApiHelper;

        public SalaryProcessedServices()
        {
            _transactionApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());

        }

        public async Task<ExecutionMessages> Delete(RevokeTempPayCodesByIdList revokeTemp)
        {
            try
            {
                var response = await _transactionApiHelper.PostAsync<ServiceResponse<bool>>(APICallHelper.RevolkTempCodeBulk, revokeTemp);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(revokeTemp, false, null, MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }
   
        public async Task<ExecutionMessages> GenerateForKnown(GenerateTempPayCodeForKnownBeneficiary model)
        {
            try
            {
                var response = await _transactionApiHelper.PostAsync<ServiceResponse<bool>>(APICallHelper.GenerateProcessedSalaryTempCodeKnown, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, null, MessagesResults.Failed,
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

        public async Task<ExecutionMessages> Register(RegisterNonMemberAndGenerateTempCode model)
        {
            try
            {
                var response = await _transactionApiHelper.PostAsync<ServiceResponse<bool>>(APICallHelper.GenerateProcessedSalaryTempCodeNoneMember, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, null, MessagesResults.Failed,
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
    
        public async Task<SalaryExtract> GetSalary(string id)
        {
            try
            {
                var couApiResponse = await _transactionApiHelper.GetAsync<ResponseObject<SalaryExtract>>(string.Format(APICallHelper.GetProcessedSalaryById, id));
                if (couApiResponse.IsSuccess)
                {
                    // FileDownloadDto should contain file data and metadata
                    return couApiResponse.ApiResponseData.Data;
                }
                return new SalaryExtract();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
  

        public async Task<CustomDataTable> GetDataTableAsync(GetProcessedSalaryDataTableQuery request)
        {
            request.DataTableOptions.sortColumnName = "CreatedDate";

            if (!IsHeadOffice())
            {
                request.BranchId = GetBranchID();
            }

            var response = await _transactionApiHelper.PostAsync<ResponseObject<CustomDataTable>>(
                APICallHelper.GetProcessedSalaryDataTable,
                request
            );

            if (response.IsSuccess && response.ApiResponseData != null)
            {
                return response.ApiResponseData.Data;
            }

            return new CustomDataTable(
                draw: Convert.ToInt32(request.DataTableOptions.draw),
                recordsTotal: 0,
                recordsFiltered: 0,
                data: new List<object>(),
                dataTableOptions: request.DataTableOptions
            );
        }


    }

}
