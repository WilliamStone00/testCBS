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
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountOperation;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.ReportDataSetDto;

namespace CBS.BusinessService.Accounts
{
    public class SubTellerEndOfDayServices : BaseService
    {
        private readonly BranchServices _brancheServices;
        private readonly TellerProvissioningServices _tellerProvissioningServices;
        private readonly ApiCallerHelper _transactionApiHelper;
        public SubTellerEndOfDayServices(BranchServices brancheServices, TellerProvissioningServices tellerProvissioningServices)
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
                var response = await _transactionApiHelper.PostAsync<ServiceResponse<TellerProvioningHistory>>(APICallHelper.SubTellerEndOfDay, model);
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
        public TillOpenAndClossingDS MapToTillOpenAndClossingDS(Branch branch, TellerProvioningHistory tellerProvisioningHistory)
        {
            var tillDs = new TillOpenAndClossingDS
            {
                UserIdInChargeOfThisTeller = tellerProvisioningHistory.UserIdInChargeOfThisTeller,
                ProvisionedBy = tellerProvisioningHistory.ProvisionedBy,
                IsCashReplenished = tellerProvisioningHistory.IsCashReplenished,
                ReplenishedAmount = tellerProvisioningHistory.ReplenishedAmount,
                OpenedDate = tellerProvisioningHistory.OpenedDate.GetValueOrDefault(),
                ClossedDate = tellerProvisioningHistory.ClossedDate.GetValueOrDefault(),
                OpenOfDayAmount = tellerProvisioningHistory.OpenOfDayAmount,
                ReferenceId = tellerProvisioningHistory.ReferenceId,
                CloseOfReferenceId = tellerProvisioningHistory.CloseOfReferenceId,
                IsRequestedForCashReplenishment = tellerProvisioningHistory.IsRequestedForCashReplenishment,
                CashAtHand = tellerProvisioningHistory.CashAtHand,
                EndOfDayAmount = tellerProvisioningHistory.EndOfDayAmount,
                AccountBalance = tellerProvisioningHistory.AccountBalance,
                LastOPerationAmount = tellerProvisioningHistory.LastOPerationAmount,
                LastOperationType = tellerProvisioningHistory.LastOperationType,
                PreviouseBalance = tellerProvisioningHistory.PreviouseBalance,
                SubTellerComment = tellerProvisioningHistory.SubTellerComment,
                Note = tellerProvisioningHistory.Note,
                ClossedStatus = tellerProvisioningHistory.ClossedStatus,
                TillName = tellerProvisioningHistory.Teller.name, // Assuming you have a TillName property in the Branch object
                InitialPrinting = tellerProvisioningHistory.InitialPrinting,
                OpeningNote10000 = tellerProvisioningHistory.OpeningNote10000,
                OpeningNote5000 = tellerProvisioningHistory.OpeningNote5000,
                OpeningNote2000 = tellerProvisioningHistory.OpeningNote2000,
                OpeningNote1000 = tellerProvisioningHistory.OpeningNote1000,
                OpeningNote500 = tellerProvisioningHistory.OpeningNote500,
                OpeningCoin500 = tellerProvisioningHistory.OpeningCoin500,
                OpeningCoin100 = tellerProvisioningHistory.OpeningCoin100,
                OpeningCoin50 = tellerProvisioningHistory.OpeningCoin50,
                OpeningCoin25 = tellerProvisioningHistory.OpeningCoin25,
                OpeningCoin10 = tellerProvisioningHistory.OpeningCoin10,
                OpeningCoin5 = tellerProvisioningHistory.OpeningCoin5,
                OpeningCoin1 = tellerProvisioningHistory.OpeningCoin1,
                ClosingNote10000 = tellerProvisioningHistory.ClosingNote10000,
                ClosingNote5000 = tellerProvisioningHistory.ClosingNote5000,
                ClosingNote2000 = tellerProvisioningHistory.ClosingNote2000,
                ClosingNote1000 = tellerProvisioningHistory.ClosingNote1000,
                ClosingNote500 = tellerProvisioningHistory.ClosingNote500,
                ClosingCoin500 = tellerProvisioningHistory.ClosingCoin500,
                ClosingCoin100 = tellerProvisioningHistory.ClosingCoin100,
                ClosingCoin50 = tellerProvisioningHistory.ClosingCoin50,
                ClosingCoin25 = tellerProvisioningHistory.ClosingCoin25,
                ClosingCoin10 = tellerProvisioningHistory.ClosingCoin10,
                ClosingCoin5 = tellerProvisioningHistory.ClosingCoin5,
                Id = tellerProvisioningHistory.Id,
                IsPrimaryTeller = tellerProvisioningHistory.IsPrimaryTeller,
                TellerType = tellerProvisioningHistory.Teller?.isPrimary == true ? "Primary-Till" : "Sub-Till",
                ClosingCoin1 = tellerProvisioningHistory.ClosingCoin1,
                TotalOpeningAmount = tellerProvisioningHistory.TotalOpeningAmount,
                TotalClosingAmount = tellerProvisioningHistory.TotalClosingAmount,
                Logo = branch.Bank.LogoUrl,
                BranchName = branch.Name,
                BranchCode = branch.BranchCode,
                BranchAddress = branch.Address,
                BranchTelephone = branch.Telephone,
                HeadOfficeName = branch.Bank.Name,
                HeadOfficeAddress = branch.Bank.Address,
                HeadOfficeTelephone = branch.Bank.Telephone,
                HeadOfficeEmail = branch.Bank.Email,
                HeadOfficeWebSite = branch.Bank.WebSite,
                HeadOfficeInitial = branch.Bank.BankInitial,
                HeadOfficeCode = branch.Bank.BankCode
            };
            return tillDs;
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
