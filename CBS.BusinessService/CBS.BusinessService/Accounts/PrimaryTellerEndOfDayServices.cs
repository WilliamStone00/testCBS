using CBS.API.Helper;
using System;
using System.Configuration;
using System.Threading.Tasks;
using BusinessServices;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System.Collections.Generic;
using CBS.FrontDesk.Data.UserManagement;
using System.Web;
using System.Linq;
using CBS.FrontDesk.Data.ReportDataSetDto;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountOperation;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.BusinessService.Config;

namespace CBS.BusinessService.Accounts
{
    public class PrimaryTellerEndOfDayServices : BaseService
    {
        private readonly BranchServices _brancheServices;
        private readonly TellerProvissioningServices _tellerProvissioningServices;
        private readonly ApiCallerHelper _transactionApiHelper;
        public PrimaryTellerEndOfDayServices(BranchServices brancheServices, TellerProvissioningServices tellerProvissioningServices)
        {
            _transactionApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
            _brancheServices = brancheServices;
            _tellerProvissioningServices = tellerProvissioningServices;
        }
        public bool IsCurrencySumValid(CurrencyNotes currencyNotes, int amount)
        {
            int totalNotesValue = currencyNotes.note10000 * 10000 +
                                  currencyNotes.note5000 * 5000 +
                                  currencyNotes.note2000 * 2000 +
                                  currencyNotes.note1000 * 1000 +
                                  currencyNotes.note500 * 500 +
                                  currencyNotes.coin500 * 500 +
                                  currencyNotes.coin100 * 100 +
                                  currencyNotes.coin50 * 50 +
                                  currencyNotes.coin25 * 25 +
                                  currencyNotes.coin10 * 10 +
                                  currencyNotes.coin5 * 5 +
                                  currencyNotes.coin1;

            return totalNotesValue == amount;
        }
        public async Task<ExecutionMessages> EndTheDay(CloseOfDayRequest model)
        {
            try
            {
                if (!ComputeDenomination(model.CurrencyNotes, Convert.ToInt32(model.Amount)))
                {
                    GetExecutionMessages(model, false, $"{model.Amount}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, "The amount you entered does not match the breakdown of the currency denominations provided. Please verify that the total cash amount you input aligns with the individual denominations listed. This ensures that the total cash in hand is accurate and properly accounted for. Review the denomination details and adjust the entered amount accordingly.");
                    return ExecutionMessage;
                }
                model.ClossedStatus = "CLOSED";
                var response = await _transactionApiHelper.PostAsync<ServiceResponse<TellerProvioningHistory>>(APICallHelper.PrimaryTellerEndOfDay, model);
                if (response.IsSuccess)
                {
                    await _tellerProvissioningServices.MapToTillOpenAndClossingDS(response.ApiResponseData.Data);

                    GetExecutionMessages(response, true, $"{model.CashAtHand}", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, $"{model.CashAtHand}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                }
                // Make an API call to create an individual profile

            }
            catch (Exception ex)
            {
                // Log and handle exception
                GetExecutionMessages(null, false, null, MessagesResults.Error, ExecutionProcessOption.TryCatch,
                    SystemMessageStatus.Failed.ToString(), ex);
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> EndTheDayAccountant(EndOfDayAccountantCommand model)
        {
            try
            {
             
                    var response = await _transactionApiHelper.PostAsync<ServiceResponse<TellerProvioningHistory>>(APICallHelper.EndOfDayAccountant, model);
                    if (response.IsSuccess)
                    {

                        GetExecutionMessages(response, true, $"{model.amountRecieved}", MessagesResults.Success,
                            ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, $"{model.amountRecieved}", MessagesResults.Failed,
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
        public async Task<ExecutionMessages> EndTheDaySubTellerVerification(EndOfDayBySubTellerIDCommand model)
        {
            try
            {
                var response = await _transactionApiHelper.PostAsync<ServiceResponse<TellerProvioningHistory>>(APICallHelper.EndOfDaySubTellerBYPrimaryTeller, model);
                if (response.IsSuccess)
                {

                    GetExecutionMessages(response, true, $"{model.primaryTellerConfirmationStatus}", MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                    return ExecutionMessage;
                }
                else
                {
                    // Failed creation
                    GetExecutionMessages(model, false, $"{model.primaryTellerConfirmationStatus}", MessagesResults.Failed,
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
        //TellerOpenningAndClossingQuery
        public async Task<IEnumerable<PrimaryTellerProvisioningDto>> GetPrimaryTellerHistories()
        {
            try
            {
                var apiUrl = HttpContext.Current.User.IsInRole("Administrator") ? APICallHelper.GetAllPrimaryTellerProvisioningHistoryQuery : string.Format(APICallHelper.GetPrimaryTellerProvisioningHistoryByUserIncharge, GetUserID());

                var couApiResponse = await _transactionApiHelper.GetAsync<ResponseObject<List<PrimaryTellerProvisioningDto>>>(apiUrl);

                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                else
                {
                    return Enumerable.Empty<PrimaryTellerProvisioningDto>();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
       
        public async Task<IEnumerable<PrimaryTellerProvisioningDto>> GetPrimaryTellerHistoriesByBranchID()
        {
            try
            {
                var apiUrl = string.Format(APICallHelper.GetPrimaryTellerProvisioningHistoryByBranchIDQuery, GetBranchID());

                var couApiResponse = await _transactionApiHelper.GetAsync<ResponseObject<List<PrimaryTellerProvisioningDto>>>(apiUrl);

                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                else
                {
                    return Enumerable.Empty<PrimaryTellerProvisioningDto>();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<EndOfTheDay> GetDailyOperationToClose(string id)
        {
            try
            {
                var apiUrl = string.Format(APICallHelper.GetSubTellerProvioningHistoryQuery, id);

                var couApiResponse = await _transactionApiHelper.GetAsync<ResponseObject<SubTellerProvisioningDto>>(apiUrl);

                if (couApiResponse.IsSuccess)
                {
                    if (couApiResponse != null)
                    {
                        var data = new EndOfTheDay
                        {
                            EndOfDayBySubTellerIDCommand = new EndOfDayBySubTellerIDCommand
                            {
                                subTellerProvioningHistoryID = couApiResponse.ApiResponseData.Data.id,
                                comment = couApiResponse.ApiResponseData.Data.primaryTellerComment,
                                primaryTellerConfirmationStatus = couApiResponse.ApiResponseData.Data.primaryTellerConfirmationStatus
                            },
                            SubTellerProvioningHistory = couApiResponse.ApiResponseData.Data,
                            TransactionHistories = couApiResponse.ApiResponseData.Data.teller.Transactions,
                        };
                        return data;
                    }
                }
                return new EndOfTheDay();

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<EndOfTheDay> GetDailyOperation(string id)
        {
            try
            {
                var apiUrl = string.Format(APICallHelper.GetPrimaryTellerProvisioningHistoryQuery, id);

                var couApiResponse = await _transactionApiHelper.GetAsync<ResponseObject<PrimaryTellerProvisioningDto>>(apiUrl);

                if (couApiResponse.IsSuccess)
                {
                    if (couApiResponse != null)
                    {
                        var data = new EndOfTheDay
                        {
                            EndOfDayPrimaryTellerCommand = new EndOfDayPrimaryTellerCommand
                            {
                                primaryTellerProvioningHistoryID = couApiResponse.ApiResponseData.Data.Id,
                            },
                            EndOfDayAccountantCommand = new EndOfDayAccountantCommand
                            {
                                primaryTellerProvioningHistoryID = couApiResponse.ApiResponseData.Data.Id,
                            },
                            PrimaryTellerProvisioningHistory = couApiResponse.ApiResponseData.Data,
                            TransactionHistories = couApiResponse.ApiResponseData.Data.teller.Transactions,
                        };
                        return data;
                    }
                }
                return new EndOfTheDay();

            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}
