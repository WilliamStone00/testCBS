
using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounts
{
    public class WithdrawalNotificationServices : BaseService
    {
        private readonly ApiCallerHelper _transactionApiHelper;
        private readonly AccountServices _accountServices;

        public WithdrawalNotificationServices(AccountServices accountServices = null)
        {
            _transactionApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
            _accountServices = accountServices;
        }

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var objmodel = await GetWithdrawalNotification(id);
                var inResponse = await _transactionApiHelper.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Get_Update_Delete_WithdrawalNotification, id));
                if (inResponse.IsSuccess)
                {

                    GetExecutionMessages(inResponse, true, $"WithdrawalNotification", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null, inResponse.Message);
                    return ExecutionMessage;

                }
                else
                {
                    // Handle failure scenario
                    GetExecutionMessages(objmodel, false, $"WithdrawalNotification", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, inResponse.Message);
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
            }
            return ExecutionMessage;
        }


        public async Task<WithdrawalNotification> GetBalanceOfLoanAndSaving(string customerId, string legalForm)
        {
            try
            {
                var accounts = await _accountServices.GetCustomerAccounts(customerId);

                // Find the saving account
                var savingAccount = accounts.FirstOrDefault(x => x.accountType == "Saving");
                var savingAccountBalance = savingAccount?.balance ?? 0; // Default to 0 if saving account is null

                // Find the loan account
                var loanAccount = accounts.FirstOrDefault(x => x.accountType == "Loan");
                var loanAccountBalance = loanAccount?.balance ?? 0; // Default to 0 if loan account is null

                decimal formNotificationCharge = 0;
                if (legalForm == "Physical_Person")
                {
                    formNotificationCharge = savingAccount?.product?.WithdrawalParameters?.FirstOrDefault().PhysicalPersonWithdrawalFormFee ?? 0;
                }
                else
                {
                    formNotificationCharge = savingAccount?.product?.WithdrawalParameters?.FirstOrDefault().MoralPersonWithdrawalFormFee ?? 0;
                }

                var withdrawalNotification = new WithdrawalNotification
                {
                    LoanBalance = loanAccountBalance,
                    AccountBalance = savingAccountBalance,
                    CustomerId = customerId,
                    AccountNumber = savingAccount?.accountNumber,
                    CustomerAccount = savingAccount,
                    AccountId = savingAccount?.id,
                    FormNotificationCharge = formNotificationCharge
                };

                return withdrawalNotification;
            }
            catch (Exception ex)
            {
                throw; // Re-throw the exception for higher level handling
            }
        }
        //
        public async Task<IEnumerable<WithdrawalNotification>> GetWithdrawalNotificationsByMemberId(string customerId)
        {
            try
            {
                var couApiResponse = await _transactionApiHelper.GetAsync<ResponseObject<List<WithdrawalNotification>>>(string.Format(APICallHelper.GetAllWithdrawalNotificationByCustomerId, customerId));
                if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData != null)
                {
                    return couApiResponse.ApiResponseData.Data;
                }

                return new List<WithdrawalNotification>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<IEnumerable<WithdrawalNotification>> GetWithdrawalNotifications()
        {
            try
            {
                var couApiResponse = await _transactionApiHelper.GetAsync<ResponseObject<List<WithdrawalNotification>>>(APICallHelper.GetAllWithdrawalNotification);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<WithdrawalNotification>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<IEnumerable<WithdrawalNotification>> GetPendingWithdrawalNotifications()
        {
            try
            {
                var withdrawalNotifications = (from a in await GetWithdrawalNotifications() where a.ApprovalStatus == "Pending" select a).ToList();
                return withdrawalNotifications;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<WithdrawalNotification> GetWithdrawalNotification(string id)
        {
            try
            {
                var cusResponseObject = await _transactionApiHelper.GetAsync<ResponseObject<WithdrawalNotification>>(string.Format(APICallHelper.Get_Update_Delete_WithdrawalNotification, id));
                if (cusResponseObject.IsSuccess)
                {
                    var data = cusResponseObject.ApiResponseData.Data;

                    var intendedWithdrawalComponents = ConvertToYearMonthDay(data.DateOfIntendedWithdrawal);
                    var gracePeriodComponents = ConvertToYearMonthDay(data.GracePeriodDate);

                    data.GracePeriodDateDay = gracePeriodComponents.day;
                    data.GracePeriodDateMonth = gracePeriodComponents.month;
                    data.GracePeriodDateYear = gracePeriodComponents.year;
                    data.DateOfIntendedWithdrawalDay = intendedWithdrawalComponents.day;
                    data.DateOfIntendedWithdrawalMonth = intendedWithdrawalComponents.month;
                    data.DateOfIntendedWithdrawalYear = intendedWithdrawalComponents.year;


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
        public async Task<ExecutionMessages> Create(WithdrawalNotification model)
        {
            try
            {
                model.DateOfIntendedWithdrawal = ConvertToDate(model.DateOfIntendedWithdrawalYear, model.DateOfIntendedWithdrawalMonth, model.DateOfIntendedWithdrawalDay);
                model.GracePeriodDate = ConvertToDate(model.GracePeriodDateYear, model.GracePeriodDateMonth, model.GracePeriodDateDay);
                var response = await _transactionApiHelper.PostAsync<ServiceResponse<WithdrawalNotification>>(APICallHelper.CreateWithdrawalNotification, model);
                if (response.IsSuccess)
                {
                    // Successful creation
                    GetExecutionMessages(response, true, $"Withdrawal Notification", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, "Withdrawal Notification", MessagesResults.Failed,
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
        public async Task<ExecutionMessages> Update(WithdrawalNotification modelobj)
        {
            try
            {
                var model = await GetWithdrawalNotification(modelobj.Id);

                if (model != null)
                {
                    model.AccountNumber = modelobj.AccountNumber;
                    model.DateOfIntendedWithdrawal = modelobj.DateOfIntendedWithdrawal;
                    model.GracePeriodDate = modelobj.GracePeriodDate;
                    model.AmountRequired = modelobj.AmountRequired;
                    model.ReasonForWithdrawal = modelobj.ReasonForWithdrawal;
                    model.FormNotificationCharge = modelobj.FormNotificationCharge;
                    model.DateOfIntendedWithdrawal = ConvertToDate(modelobj.DateOfIntendedWithdrawalYear, modelobj.DateOfIntendedWithdrawalMonth, modelobj.DateOfIntendedWithdrawalDay);
                    model.GracePeriodDate = ConvertToDate(modelobj.GracePeriodDateYear, modelobj.GracePeriodDateMonth, modelobj.GracePeriodDateDay);

                    var response = await _transactionApiHelper.PutAsync<ServiceResponse<WithdrawalNotification>>(string.Format(APICallHelper.Get_Update_Delete_WithdrawalNotification, model.Id), model);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"Withdrawal Notification", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, "Withdrawal Notification", MessagesResults.Failed,
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
        public async Task<ExecutionMessages> ApproveOrRejectWithdrawalNotification(WithdrawalNotification modelobj)
        {
            try
            {
                var model = await GetWithdrawalNotification(modelobj.Id);

                if (model != null)
                {
                    model.ApprovalStatus = modelobj.ApprovalStatus;
                    model.ApprovalComment = modelobj.ApprovalComment;
                    var response = await _transactionApiHelper.PutAsync<ServiceResponse<WithdrawalNotification>>(string.Format(APICallHelper.WithdrawalNotificationValidateNotification, model.Id), model);
                    if (response.IsSuccess)
                    {
                        // Successful creation
                        GetExecutionMessages(response, true, $"Withdrawal Notification", MessagesResults.Success,
                            ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, null);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, "Withdrawal Notification", MessagesResults.Failed,
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

    }

}
