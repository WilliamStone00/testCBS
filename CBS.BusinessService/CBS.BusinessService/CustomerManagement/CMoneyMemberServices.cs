using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.CMoney;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.CustomerManagement
{
    public class CMoneyMemberServices : BaseService
    {
        private readonly ApiCallerHelper _loanConfigApiHelper;
        private readonly BranchServices _branchServices;
        public CMoneyMemberServices(BranchServices branchServices = null)
        {
            _loanConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["CustomerBaseUrl"].ToString());
            _branchServices=branchServices;
        }

        public async Task<IEnumerable<CMoneyMembersActivationAccount>> GetCMoneyMembersActivationAccounts(GetCMoneyMemberActivationsQuery memberActivationsQuery)
        {
            try
            {
                // Construct query parameters dynamically based on the provided memberActivationsQuery
                var queryParams = new List<string>();

                if (memberActivationsQuery.ByBranch && !string.IsNullOrEmpty(memberActivationsQuery.ParameterString))
                    queryParams.Add($"ByBranch=true&ParameterString={memberActivationsQuery.ParameterString}");
                if (memberActivationsQuery.ByUser && !string.IsNullOrEmpty(memberActivationsQuery.ParameterString))
                    queryParams.Add($"ByUser=true&ParameterString={memberActivationsQuery.ParameterString}");
                if (memberActivationsQuery.ByDate && memberActivationsQuery.StartDate.HasValue && memberActivationsQuery.EndDate.HasValue)
                    queryParams.Add($"ByDate=true&StartDate={memberActivationsQuery.StartDate.Value:yyyy-MM-dd}&EndDate={memberActivationsQuery.EndDate.Value:yyyy-MM-dd}");
                if (memberActivationsQuery.IsActive)
                    queryParams.Add("IsActive=true");
                if (memberActivationsQuery.IsDeactivated)
                    queryParams.Add("IsDeactivated=true");

                // Combine the query parameters into a single query string
                var queryString = string.Join("&", queryParams);

                // Construct the full API URL with the query string
                var apiUrl = $"{APICallHelper.CMoneyGetMembers}?{queryString}";

                // Make the API call
                var couApiResponse = await _loanConfigApiHelper.GetAsync<ResponseObject<List<CMoneyMembersActivationAccount>>>(apiUrl);

                // Check the API response and return data or an empty list
                if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData?.Data != null)
                {
                    return couApiResponse.ApiResponseData.Data;
                }

                return new List<CMoneyMembersActivationAccount>();
            }
            catch (Exception ex)
            {
                // Log and handle exception (Optional: Add logging here)
                Console.WriteLine($"Error in GetCMoneyMembersActivationAccounts: {ex.Message}");
                throw;
            }
        }
        public async Task<CustomDataTable> GetDataTable(DataTableOptions dataTableOptions, string searchCriterial, bool isByBranch = true)
        {
            if (IsHeadOffice())
            {
                isByBranch=false;
            }
            var customerParam = new PagginationResource
            {
                OrderBy = "CustomerId",
                PageSize = dataTableOptions.pageSize,
                Skip = dataTableOptions.skip,
                BranchId = GetBranchID(),
                IsByBranch = isByBranch,
                SearchQuery = searchCriterial == "" ? "all" : searchCriterial,
            };
            Func<Task<List<CMoneyMembersActivationAccount>>> getDataFunc = async () => (await GetMembers(customerParam)).ToList();
            var dataTable = await GenerateDataTable(dataTableOptions, getDataFunc);
            return dataTable;

        }
        public async Task<CustomDataTable> GenerateDataTable(DataTableOptions dataTableOptions, Func<Task<List<CMoneyMembersActivationAccount>>> getDataFunc)
        {
            List<CMoneyMembersActivationAccount> data = (await getDataFunc()).ToList();
            var paginationMetadata = data?.FirstOrDefault()?.PaginationMetadata ?? new PaginationMetadata
            {
                TotalCount = 0
            };

            // Construct CustomDataTable using pagination metadata
            var dataTable = new CustomDataTable(
                Convert.ToInt32(dataTableOptions.draw),
                paginationMetadata.TotalCount,
                dataTableOptions.recordsFiltered,
                data,
                dataTableOptions
            );

            return dataTable;
        }
        public async Task<IEnumerable<CMoneyMembersActivationAccount>> GetMembers(PagginationResource resource)
        {
            try
            {

                var queryString = ToQueryString(resource);
                var fullUrl = $"{APICallHelper.CMoneyGetMembersPaggination}?{queryString}";
                var individualProfiles = await _loanConfigApiHelper.GetAsync<ResponseObject<List<CMoneyMembersActivationAccount>>>(fullUrl);
                var branches = await _branchServices.GetBranches();
                var data = (from a in individualProfiles.ApiResponseData.Data
                            join b in branches on a.BranchId equals b.Id
                            select TransformToCustomerList(a, b)).ToList();

                return data;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        private CMoneyMembersActivationAccount TransformToCustomerList(CMoneyMembersActivationAccount c, Branch b)
        {
            var customer = new CMoneyMembersActivationAccount
            {
                CustomerId = c.CustomerId,
                SecretQuestion = c.SecretQuestion,
                SecretAnswer = c.SecretAnswer,
                PaginationMetadata = c.PaginationMetadata,
                Name=c.Name,
                BranchCode=c.BranchCode,
                LoginId=c.LoginId,
                PhoneNumber=c.PhoneNumber,
                ActivatedBy=c.ActivatedBy,
                ActivatingBranchCode=c.ActivatingBranchCode,
                ActivatingBranchId=c.ActivatingBranchId,
                ActivatingBranchName=c.ActivatingBranchName,
                ActivationDate=c.ActivationDate,
                BranchId=c.BranchId,
                DeactivationReason=c.DeactivationReason,
                Id=c.Id,
                DefaultPin=c.DefaultPin,
                IsActive=c.IsActive,
                IsSubcribed=c.IsSubcribed,
                Language=c.Language,
                LastPaymentDate=c.LastPaymentDate,
                LastSubcriptionRenewalDate=c.LastSubcriptionRenewalDate,
                FailedAttempts=c.FailedAttempts,
                HasChangeDefaultPin=c.HasChangeDefaultPin,
                LastPaymentAmount=c.LastPaymentAmount,
                BranchName=b.Name,
                LastLoginDate=c.LastLoginDate

            };

            return customer;
        }

        public async Task<CMoneyMembersActivationAccount> GetCMoneyMembersActivationAccount(string id)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<CMoneyMembersActivationAccount>>(string.Format(APICallHelper.CMoneyGetMember, id));
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
        public async Task<ExecutionMessages> Create(AddCMoneyMemberActivationCommand model)
        {
            try
            {

                // Make an API call to create an individual profile
                var response = await _loanConfigApiHelper.PostAsync<ServiceResponse<CMoneyMembersActivationAccount>>(APICallHelper.CMoneyMemberActivation, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    ExecutionMessage.SessionID=response.ApiResponseData.Data.Id;
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, null, MessagesResults.Failed,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null, response.Message);
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

        public async Task<ExecutionMessages> GenerateOTP(GenerateMemberActivationOTPCommand model)
        {
            try
            {

                // Make an API call to create an individual profile
                var response = await _loanConfigApiHelper.PostAsync<ServiceResponse<TempOTPDto>>(APICallHelper.CMonetGenerateOTP, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, null, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    ExecutionMessage.SessionID=model.PhoneNumber;
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, null, MessagesResults.Failed,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null, response.Message);
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
        public async Task<ExecutionMessages> Update(CMoneyMembersActivationAccount model)
        {
            try
            {
                ApiResponse<ServiceResponse<bool>> apiResponse;

                switch (model.Option)
                {
                    case "reset_pin":
                        apiResponse = await _loanConfigApiHelper.PutAsync<ServiceResponse<bool>>(
                           APICallHelper.CMoneyMemberResetPin, model.ResetCMoneyMemberPinCommand);
                        break;

                    case "reset_pin_with_secret":
                        apiResponse = await _loanConfigApiHelper.PutAsync<ServiceResponse<bool>>(
                            APICallHelper.CMoneyMemberResetPin, model.ResetPinWithSecurityCommand);
                        break;

                    case "update_activation_profile":
                        apiResponse = await _loanConfigApiHelper.PutAsync<ServiceResponse<bool>>(
                            APICallHelper.CMoneyMemberActivationUpdate, model.UpdateCMoneyMemberActivationCommand);
                        break;

                    case "deactivate_account":
                        apiResponse = await _loanConfigApiHelper.PutAsync<ServiceResponse<bool>>(
                            APICallHelper.CMoneyMemberDeactivation, model.DeactivateCMoneyMemberCommand);
                        break;

                    case "reactivate_account":
                        apiResponse = await _loanConfigApiHelper.PutAsync<ServiceResponse<bool>>(
                            APICallHelper.CMoneyMemberReactivation, model.ReactivateCMoneyMemberCommand);
                        break;

                    case "change_phone_number":
                        apiResponse = await _loanConfigApiHelper.PutAsync<ServiceResponse<bool>>(
                            APICallHelper.CMoneyChangePhoneNumber, model.ChangePhoneNumberCommand);
                        break;

                    case "set_security_questions":
                        apiResponse = await _loanConfigApiHelper.PutAsync<ServiceResponse<bool>>(
                            APICallHelper.CMoneySetMembersSecretQuestion, model.ManageSecretQuestionCommand);
                        break;

                    default:
                        GetExecutionMessages(null, false, null, MessagesResults.Failed, ExecutionProcessOption.InvalidOption,
                            SystemMessageStatus.Failed.ToString(), null, "Invalid option specified.");
                        return ExecutionMessage;
                }

                if (apiResponse.IsSuccess)
                {
                    GetExecutionMessages(null, true, null, MessagesResults.Success, ExecutionProcessOption.DefaultSuccessdMessages,
                        SystemMessageStatus.Success.ToString(), null, apiResponse.Message);
                }
                else
                {
                    GetExecutionMessages(null, false, null, MessagesResults.Failed, ExecutionProcessOption.DefaultFailedMessages,
                        SystemMessageStatus.Failed.ToString(), null, apiResponse.Message);
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

    }

}
