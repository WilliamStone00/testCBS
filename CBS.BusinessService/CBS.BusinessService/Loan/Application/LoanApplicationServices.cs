using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.CustomerManagement;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Application
{
    public class LoanApplicationServices : BaseService
    {
        private readonly ApiCallerHelper _loanConfigApiHelper;
        private readonly IndividualProfileServices _individualProfileServices;

        public LoanApplicationServices(IndividualProfileServices individualProfileServices)
        {
            _loanConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["LoanBaseUrl"].ToString());
            _individualProfileServices=individualProfileServices;
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objLoanApplication = await GetLoanApplication(id);
                var inResponse = await _loanConfigApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(string.Format(APICallHelper.Get_Update_Delete_LoanApplication, id), id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"Loan application", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objLoanApplication, false, $"Loan application", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }

        public async Task<IEnumerable<LoanApplication>> GetLoanApplications(string param)
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<LoanApplication>>>(string.Format(APICallHelper.GetAllLoanApplicationByParameter, param));
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<LoanApplication>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<IEnumerable<LoanApplication>> GetLoanApplicationByCustomerID(string customerId)
        {
            try
            {
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<LoanApplication>>>(string.Format(APICallHelper.GetAllLoanApplicationByCustomerId, customerId));
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<LoanApplication>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<LoanApplication> GetLoanWithCustomerAndBranch(string customerId)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<LoanApplication>>(string.Format(APICallHelper.GetLoan, customerId));
                if (cusResponseObject.IsSuccess && cusResponseObject.ApiResponseData != null)
                {
                    var loan = cusResponseObject.ApiResponseData.Data;
                    var individualCustomerProfile = await _individualProfileServices.GetCustomerLight(loan.CustomerId);
                    loan.IndividualCustomer = individualCustomerProfile;
                    return loan;
                }
                return null;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }

        }
        public async Task<LoanApplication> GetLoanApplication(string id)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<LoanApplication>>(string.Format(APICallHelper.Get_Update_Delete_LoanApplication, id));
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
        public async Task<ExecutionMessages> Create(AddLoanApplicationCommand model)
        {
            try
            {

                // Make an API call to create an individual profile
                model.BranchId = GetBranchID();
               
                //model.LoanApplicationType = "Normal";
                //model.AmortizationType = "Constant_Amortization";
                var response = await _loanConfigApiHelper.PostAsync<ServiceResponse<LoanApplication>>(APICallHelper.CreateLoanApplication, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"Loan application", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, "Loan application", MessagesResults.Failed,
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
        public async Task<ExecutionMessages> GenerateOTP(AddOTPNotificationCommand model)
        {
            try
            {


                var response = await _loanConfigApiHelper.PostAsync<ServiceResponse<OTPNotification>>(APICallHelper.GenerateOTP, model);
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
        public async Task<ExecutionMessages> ValidaLoanApplication(UpdateLoanApplicationStatusCommand model)
        {
            try
            {


                var LoanApplication = await GetLoanApplication(model.Id.ToString());
                if (LoanApplication != null)
                {
                    var response = await _loanConfigApiHelper.PutAsync<ServiceResponse<AlertProfile>>(string.Format(APICallHelper.ValidateLoanApplicationStatus, model.Id), model);

                    if (response.IsSuccess)
                    {
                        GetExecutionMessages(response, true, $"Loan application", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, response.Message);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, $"Loan application", MessagesResults.Failed,
                            ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                    }
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
        public async Task<CustomDataTable> GetDataTableAsync(GetLoanApplicationsDataTableQuery loansDataTableQuery, string searchCriterial)
        {
            loansDataTableQuery.DataTableOptions.searchValue = searchCriterial;
            loansDataTableQuery.DataTableOptions.search = searchCriterial;
            loansDataTableQuery.DataTableOptions.sortColumnName = "ApplicationDate";
            if (!IsHeadOffice())
            {
                loansDataTableQuery.BranchId=GetBranchID();
            }
            // Make API call to fetch the DataTable result
            var couApiResponse = await _loanConfigApiHelper.PostAsync<ResponseObject<CustomDataTable>>(
                APICallHelper.GetLoanApplicationDatatable,
                loansDataTableQuery
            );

            // Return response if successful
            if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData != null)
            {
                return couApiResponse.ApiResponseData.Data;
            }

            // Return an empty DataTable if the request fails
            return new CustomDataTable(
                draw: Convert.ToInt32(loansDataTableQuery.DataTableOptions.draw),
                recordsTotal: 0,
                recordsFiltered: 0,
                data: new List<object>(), // No data
                dataTableOptions: loansDataTableQuery.DataTableOptions
            );
        }

    }

}
