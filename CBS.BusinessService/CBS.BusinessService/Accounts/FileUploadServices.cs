
using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Accounting_V2.Reconciliation;
using CBS.FrontDesk.Data.Entity.AccountingDayObject;
using CBS.FrontDesk.Data.Entity.CashCeilingManagement;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.SalaryManagement;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Entity.VaultManagement;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using DocumentFormat.OpenXml.Office2010.Excel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace CBS.BusinessService.Accounts
{
   
    public class FileUploadServices : BaseService
    {
        private readonly ApiCallerHelper _transactionApiHelper;
        private readonly ApiCallerHelper _LoanApiHelper;

        public FileUploadServices()
        {
            _transactionApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
            _LoanApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["LoanBaseUrl"].ToString());

        }
        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var inResponse = await _transactionApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.FileUploadGet_Or_Delete, id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(null, false, null, MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }
        public async Task<CustomDataTable> GetDataTableAsync(GetFileUploadsDataTableQuery query)
        {

            if (!IsHeadOffice())
            {
                query.BranchId=GetBranchID();
            }
            // Make API call to fetch the DataTable result
            var couApiResponse = await _transactionApiHelper.PostAsync<ResponseObject<CustomDataTable>>(
                APICallHelper.FileUploadsDatatable,
                query
            );

            // Return response if successful
            if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData != null)
            {
                return couApiResponse.ApiResponseData.Data;
            }

      

            // Return an empty DataTable if the request fails
            return new CustomDataTable(
                draw: Convert.ToInt32(query.DataTableOptions.draw),
                recordsTotal: 0,
                recordsFiltered: 0,
                data: new List<object>(), // No data
                dataTableOptions: query.DataTableOptions
            );
        }
        public async Task<CustomDataTable> LoanRepayments(LoanRepaymentsExecutiontDataTableQuery query)
        {

            if (!IsHeadOffice())
            {
                query.BranchId = GetBranchID();
            }
            // Make API call to fetch the DataTable result
            var executionResponse = await _LoanApiHelper.PostAsync<ResponseObject<CustomDataTable>>(
                APICallHelper.LoanRepayments,
                query
            );

            // Return response if successful
            if (executionResponse.IsSuccess && executionResponse.ApiResponseData != null)
            {
                return executionResponse.ApiResponseData.Data;
            }



            // Return an empty DataTable if the request fails
            return new CustomDataTable(
                draw: Convert.ToInt32(query.DataTableOptions.draw),
                recordsTotal: 0,
                recordsFiltered: 0,
                data: new List<object>(), // No data
                dataTableOptions: query.DataTableOptions
            );
        }

        public async Task<LoanRepaymentDetailsDto> GetLoanRepaymentByIdAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentException("id is required", nameof(id));

                var encodedId = Uri.EscapeDataString(id);
                string formattedUrl = string.Format(APICallHelper.LoanRepaymentsDetails, encodedId);

                var response = await _LoanApiHelper.GetAsync<ServiceResponse<LoanRepaymentDetailsDto>>(formattedUrl);

                if (response.IsSuccess)
                {
                    return response.ApiResponseData?.Data;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<FileDetailsDto> GetFileDetailsByIdAsync(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    throw new ArgumentException("id is required", nameof(id));

                var encodedId = Uri.EscapeDataString(id);
                string formattedUrl = string.Format(APICallHelper.LoanRepayFileDetails, encodedId);

                var response = await _transactionApiHelper.GetAsync<ServiceResponse<FileDetailsDto>>(formattedUrl);

                if (response.IsSuccess)
                {
                    return response.ApiResponseData?.Data;
                }
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<ExecutionMessages> SetPrivatePublicStatus(SetFileUploadPrivateViewCommand model)
        {
            try
            {
                var response = await _transactionApiHelper.PutAsync<ServiceResponse<bool>>(APICallHelper.SetPrivatePublicStatus, model);
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
        public async Task<FileUploadDto> GetFileUpload(string fileId)
        {
            try
            {
                var couApiResponse = await _transactionApiHelper.GetAsync<ResponseObject<FileUploadDto>>(string.Format(APICallHelper.FileUploadGet_Or_Delete, fileId));
                if (couApiResponse.IsSuccess)
                {
                    // FileDownloadDto should contain file data and metadata
                    return couApiResponse.ApiResponseData.Data;
                }
                return new FileUploadDto { };

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<ExecutionMessages> UploadSalaryFile(AddSalaryUploadModelCommand model)
        {
            try
            {
                var fields = new Dictionary<string, string>
                {
                    ["SalaryType"] = model.SalaryType,
                    ["BranchId"] = model.BranchId,
                    ["StandingOrderSourceChartOfAccountId"] = model.StandingOrderSourceChartOfAccountId ?? string.Empty,
                    ["PrivateView"] = model.PrivateView.ToString() // "True"/"False"
                };

                var response = await _transactionApiHelper
                    .UploadFileAsync<ServiceResponse<SalaryUploadModelSummaryDto>>(
                        file: model.File,
                        apiUrl: APICallHelper.CreateSalaryUpload,
                        fields: fields
                    );

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }

                GetExecutionMessages(model, false, null, MessagesResults.Failed,
                    ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
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
