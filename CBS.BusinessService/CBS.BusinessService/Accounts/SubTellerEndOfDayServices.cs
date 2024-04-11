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
using System.Linq;
using System.Web;

namespace CBS.BusinessService.Accounts
{
    public class SubTellerEndOfDayServices : BaseService
    {
        private readonly ApiCallerHelper _transactionApiHelper;
        public SubTellerEndOfDayServices()
        {
            _transactionApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
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
        public async Task<ExecutionMessages> EndTheDay(EndOfDaySubTellerCommand model)
        {
            try
            {
                if (IsCurrencySumValid(model.currencyNotes, model.cashAtHand))
                {
                    var response = await _transactionApiHelper.PostAsync<ServiceResponse<SubTellerProvioningHistory>>(APICallHelper.SubTellerEndOfDay, model);
                    if (response.IsSuccess)
                    {

                        GetExecutionMessages(response, true, $"{model.cashAtHand}", MessagesResults.Success,
                            ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                        return ExecutionMessage;
                    }
                    else
                    {
                        // Failed creation
                        GetExecutionMessages(model, false, $"{model.cashAtHand}", MessagesResults.Failed,
                            ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                    }
                }
                else
                {
                    GetExecutionMessages(model, false, $"{model.cashAtHand}", MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, "Sum of notes and coins must be equal to deposit amount.");

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
        public async Task<IEnumerable<SubTellerProvisioningDto>> GetSubTellerHistories()
        {
            try
            {
                var apiUrl = HttpContext.Current.User.IsInRole("Administrator") ? APICallHelper.GetAllSubTellerProvioningHistoryQuery : string.Format(APICallHelper.GetSubTellerProvisioningHistoryByUserIncharge, GetUserID());
                var couApiResponse = await _transactionApiHelper.GetAsync<ResponseObject<List<SubTellerProvisioningDto>>>(apiUrl);
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                else
                {
                    return Enumerable.Empty<SubTellerProvisioningDto>();
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
                            EndOfDaySubTellerCommand = new EndOfDaySubTellerCommand
                            {
                                cashAtHand = ConverToInteger(couApiResponse.ApiResponseData.Data.cashAtHand.ToString()),
                                tellerProvisioningId = couApiResponse.ApiResponseData.Data.id,
                                openOfDayAmount = FormatCurrency(couApiResponse.ApiResponseData.Data.openOfDayAmount),
                                operationDate = couApiResponse.ApiResponseData.Data.openedDate.ToString(),
                                comment = couApiResponse.ApiResponseData.Data.subTellerComment
                            },
                            SubTellerProvioningHistory = couApiResponse.ApiResponseData.Data

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
        public async Task<SubTellerProvisioningDto> GetDailyOperation(string id)
        {
            try
            {
                var apiUrl = string.Format(APICallHelper.GetSubTellerProvioningHistoryQuery, id);

                var couApiResponse = await _transactionApiHelper.GetAsync<ResponseObject<SubTellerProvisioningDto>>(apiUrl);

                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                else
                {
                    return new SubTellerProvisioningDto();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
