using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.Accounting_V2.DaillyCollectorCommission;
using CBS.FrontDesk.Data.Entity.AccountingV2.MobileMoneyV2;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.ChequeBookListing;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.CounterCheque;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace CBS.BusinessService.CheckManagementSystem.Operations.CounterCheque
{
    public class CounterChequeService : BaseService
    {
        private readonly ApiCallerHelper _apiHelper;

        public CounterChequeService()
        {
            // Ensure this key exists in your Web.config and points to the correct service
            var baseUrl = ConfigurationManager.AppSettings["CheckbookServiceBaseUrl"];
            _apiHelper = new ApiCallerHelper(baseUrl);
        }

        public async Task<List<CounterChecks>> GetChequeDetails(string CustomerId, string BranchId)
        {
            try
            {
                // Build the URL with query parameters
                var url = $"{APICallHelper.GetCustormerchequebook}?customerId={CustomerId}&branchId={BranchId}";

                // Call the API with only 1 argument
                var response = await _apiHelper.GetAsync<ResponseObject<List<CounterChecks>>>(url);

                return response.ApiResponseData?.Data ?? new List<CounterChecks>();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ExecutionMessages> IssueCounterChequeAsync(CounterChecks model)
        {
            try
            {
                // Add any necessary data from the user's session before sending
                //model.BranchId = GetBranchID();
                model.IssuedBy = GetUserFullName();

                var response = await _apiHelper.PostAsync<ServiceResponse<CounterChecks>>(APICallHelper.IssueCounterCheque, model);

                if (response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    GetExecutionMessages(response.ApiResponseData.Data, true, "Counter Cheque", MessagesResults.Success,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null,
                        response.ApiResponseData.Message ?? "Counter cheque issued successfully.");
                }
                else
                {
                    GetExecutionMessages(model, false, "Counter Cheque", MessagesResults.Failed,
                        ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null,
                        response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, "Counter Cheque", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<CounterChecks> GetCounterChequeDetailsAsync(string id)
        {
            try
            {
                var response = await _apiHelper.GetAsync<ServiceResponse<CounterChecks>>($"{APICallHelper.GetCounterChequeDetails}/{Uri.EscapeDataString(id)}");

                if (response.IsSuccess && response.ApiResponseData?.Data != null)
                {
                    return response.ApiResponseData.Data;
                }
                else
                {
                    throw new Exception(response.ApiResponseData?.Message ?? response.Message ?? "Failed to fetch counter cheque details");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching counter cheque details: {ex.Message}");
                throw new Exception($"Unable to retrieve counter cheque details: {ex.Message}", ex);
            }
        }

        public async Task<CustomDataTable> GetCounterChequesForDataTableAsync(CounterChequeQuery query)
        {
            try
            {
                var response = await _apiHelper.PostAsync<ResponseObject<CustomDataTable>>(
                    APICallHelper.GetCounterChequeDataTable, query);

                // ⚠ CRITICAL: If API call fails or returns unsuccessful, THROW exception
                if (!response.IsSuccess)
                {
                    throw new Exception($"API call failed: {response.Message}");
                }

                if (response.ApiResponseData == null)
                {
                    throw new Exception("API returned null data");
                }

                return response.ApiResponseData.Data;
            }
            catch (Exception ex)
            {
                // Log the original exception
                System.Diagnostics.Debug.WriteLine($"API Error: {ex.Message}");

                // Re-throw to trigger fallback
                throw new Exception($"Cheque book service unavailable: {ex.Message}", ex);
            }
        }

        public async Task<ExecutionMessages> TakeActionAsync(CounterChequeActionDto model)
        {
            try
            {
                string url;
                switch (model.Action?.ToLower())
                {
                    case "review": url = string.Format(APICallHelper.ReviewCounterCheque, model.CounterChequeId); break;
                    case "validate": url = string.Format(APICallHelper.ValidateCounterCheque, model.CounterChequeId); break;
                    case "reject": url = string.Format(APICallHelper.RejectCounterCheque, model.CounterChequeId); break;
                    default:
                        throw new ArgumentException("Invalid action specified for counter cheque.");
                }

                var payload = new { Motive = model.Motive, ActionBy = GetUserFullName() };

                var response = await _apiHelper.PostAsync<ServiceResponse<bool>>(url, payload);

                if (response.IsSuccess && response.ApiResponseData.Data)
                {
                    GetExecutionMessages(model, true, $"Action '{model.Action}'", MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null,
                        response.ApiResponseData.Message ?? "Action completed successfully.");
                }
                else
                {
                    GetExecutionMessages(model, false, $"Action '{model.Action}'", MessagesResults.Failed,
                       ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null,
                       response.ApiResponseData?.Message ?? response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, $"Action '{model.Action}'", MessagesResults.Error,
                   ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }
            return ExecutionMessage;
        }

        public async Task<CustomerData> GetfullChequeDetails(string chequeBookId)
        {
            if (string.IsNullOrEmpty(chequeBookId))
                throw new ArgumentException("ChequeBookId is required.");

            try
            {
                var url = string.Format(APICallHelper.GetCustomerChequeBook, chequeBookId);
                var response = await _apiHelper.GetAsync<ResponseObject<CustomerData>>(url);

                if (response?.ApiResponseData?.Data == null)
                    return null;

                // Since your DTO already represents a single checkbook, just return it
                var checkBook = response.ApiResponseData.Data;

                return checkBook;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<CustomerCheckBookStatisticsDto> GetCustomerCheckBookStatisticsAsync(string customerId)
        {
            await Task.Delay(150); // Simulate database call

            var random = new Random();
            var now = DateTime.Now;

            // Create a deterministic random based on customerId
            var seed = customerId.GetHashCode();
            var deterministicRandom = new Random(seed);

            // Mock customer details
            var customerName = deterministicRandom.Next(0, 2) == 0
                ? "AKURAWAH AWATH EUGENE"
                : "NGUIMBOU SOPHIE";

            var totalCheckBooks = deterministicRandom.Next(1, 8);
            var activeCheckBooks = deterministicRandom.Next(0, totalCheckBooks);
            var blockedCheckBooks = deterministicRandom.Next(0, 2);
            var expiredCheckBooks = totalCheckBooks - activeCheckBooks - blockedCheckBooks;

            var totalLeaves = totalCheckBooks * 100;
            var totalUsedLeaves = deterministicRandom.Next(0, totalLeaves);
            var totalRemainingLeaves = totalLeaves - totalUsedLeaves;
            var usageRate = totalLeaves > 0 ? (double)totalUsedLeaves / totalLeaves * 100 : 0;

            // ===============================
            // CLEARANCE BREAKDOWN LOGIC
            // ===============================
            var counterChecksHours = deterministicRandom.Next( 5, 100);
            var onBehalfOfHours = deterministicRandom.Next( 5, 100);
            var byOwnerHours = deterministicRandom.Next( 5, 100);

            var counterChecks24h = deterministicRandom.Next(5, 80);
            var onBehalfOf24h = deterministicRandom.Next(5, 80);
            var byOwner24h = deterministicRandom.Next(5, 80);

            // Optional: keep total consistent with breakdown
            var averageClearanceAll = (counterChecksHours + onBehalfOfHours + byOwnerHours);

            var accountsCount = deterministicRandom.Next(2, 6);

            var chequeAccounts = new List<CustomerChequeAccountDto>();

            for (int i = 0; i < accountsCount; i++)
            {
                chequeAccounts.Add(new CustomerChequeAccountDto
                {
                    AccountId = Guid.NewGuid().ToString(),
                    AccountType = deterministicRandom.Next(0, 2) == 0 ? "Savings" : "Current",
                    Chequebook = deterministicRandom.Next(0, 2) == 0 ? "Gold" : "Premuim",
                    AccountNumber = "3711-00" + deterministicRandom.Next(100000, 999999),

                    OperationType = (ChequeOperationType)deterministicRandom.Next(0, 3),

                    IsActive = deterministicRandom.Next(0, 5) != 0,
                    BalanceAmount = Math.Round(
                        deterministicRandom.Next(100_000, 5_000_000) * 1.00m, 2)
                });
            }


            // Determine risk level based on usage and bounce rate
            string riskLevel;
            var bounceRate = deterministicRandom.NextDouble();
            if (bounceRate > 0.3 || totalUsedLeaves > totalLeaves * 0.9)
                riskLevel = "High";
            else if (bounceRate > 0.1 || totalUsedLeaves > totalLeaves * 0.7)
                riskLevel = "Medium";
            else
                riskLevel = "Low";

            return new CustomerCheckBookStatisticsDto
            {
                CustomerId = customerId,
                CustomerName = customerName,
                TotalCheckBooks = totalCheckBooks,
                ActiveCheckBooks = activeCheckBooks,
                BlockedCheckBooks = blockedCheckBooks,
                ExpiredCheckBooks = expiredCheckBooks,
                ReissuedCheckBooks = deterministicRandom.Next(0, 2),
                LostCheckBooks = deterministicRandom.Next(0, 1),

                TotalLeaves = totalLeaves,
                TotalUsedLeaves = totalUsedLeaves,
                TotalRemainingLeaves = totalRemainingLeaves,
                CancelledLeaves = deterministicRandom.Next(0, 10),
                StaleLeaves = deterministicRandom.Next(0, 5),

                GlobalUsageRate = Math.Round(usageRate, 2),
                RemainingUsageRate = Math.Round(100 - usageRate, 2),

                TotalBalance = Math.Round(deterministicRandom.Next(100000, 5000000) * 1.00m, 2),
                AverageBalance = Math.Round(deterministicRandom.Next(50000, 1500000) * 1.00m, 2),

                TotalAmountIssued = Math.Round(deterministicRandom.Next(500000, 10000000) * 1.00m, 2),
                TotalAmountCleared = Math.Round(deterministicRandom.Next(300000, 8000000) * 1.00m, 2),
                TotalAmountPending = Math.Round(deterministicRandom.Next(50000, 1500000) * 1.00m, 2),
                TotalAmountRejected = Math.Round(deterministicRandom.Next(0, 1000000) * 1.00m, 2),

                HighestChequeAmount = Math.Round(deterministicRandom.Next(100000, 1000000) * 1.00m, 2),
                AverageChequeAmount = Math.Round(deterministicRandom.Next(50000, 300000) * 1.00m, 2),

                PendingClearances = deterministicRandom.Next(0, 20),
                ClearedWithin24Hours = deterministicRandom.Next(10, 100),
                ClearedWithin72Hours = deterministicRandom.Next(5, 50),
                DelayedClearances = deterministicRandom.Next(0, 15),

                //AverageClearanceTimeHours = Math.Round(deterministicRandom.NextDouble() * 96, 1), // 0-96 hours

                FirstIssuedDate = now.AddMonths(-deterministicRandom.Next(6, 36)),
                LastIssuedDate = now.AddDays(-deterministicRandom.Next(0, 90)),
                LastChequeIssuedDate = now.AddDays(-deterministicRandom.Next(0, 30)),
                LastChequeClearedDate = now.AddDays(-deterministicRandom.Next(0, 15)),

                ExpiringSoonCheckBooks = deterministicRandom.Next(0, 2),
                NeedsRenewal = deterministicRandom.Next(0, 2) == 1,

                HasBlockedCheckBooks = blockedCheckBooks > 0,
                HasBouncedCheques = deterministicRandom.Next(0, 5) >= 3,
                BouncedChequesCount = deterministicRandom.Next(0, 5),

                StopPaymentRequests = deterministicRandom.Next(0, 5),
                FraudFlaggedCheques = deterministicRandom.Next(0, 2),

                // ===============================
                // CLEARANCE BREAKDOWN
                // ===============================
                CounterChecks = counterChecksHours,
                OnBehalfOf = onBehalfOfHours,
                ByOwner = byOwnerHours,

                CounterChecksWithin24h = counterChecks24h,
                OnBehalfOfWithin24h = onBehalfOf24h,
                ByOwnerWithin24h = byOwner24h,

                // Override or align existing average if needed
                //AverageClearanceTimeHours = Math.Round(averageClearanceAll / 3, 1),
                //ClearedWithin24Hours =  counterChecks24h + onBehalfOf24h + byOwner24h,


                RiskLevel = riskLevel,

                EmergencyClearanceRequests = deterministicRandom.Next(0, 3),
                ApprovedFastTrackRequests = deterministicRandom.Next(0, 5),
                FastTrackFeesPaid = Math.Round(deterministicRandom.Next(0, 50000) * 1.00m, 2),

                GeneratedAt = now

            };
        }



        public async Task<CustomerCheckBookStatisticsDto> GetCustomerCheckBookStatistics(string customerId)
        {
            if (string.IsNullOrEmpty(customerId))
                throw new ArgumentException("customerId is required.");

			var url = string.Format(APICallHelper.GetCustomerCheckBookStatistics, customerId);
			var response = await _apiHelper.GetAsync<ResponseObject<CustomerCheckBookStatisticsDto>>(url);

			if (response?.ApiResponseData?.Data == null)
				return null;

			return response.ApiResponseData.Data;
		}

        public async Task<CustomerData> GetCustomerChequeBooks(string customerId)
        {
            if (string.IsNullOrEmpty(customerId))
                throw new ArgumentException("customerId is required.");

            try
            {
                var url = string.Format(APICallHelper.GetCustomerChequeBooks, customerId);
                var response = await _apiHelper.GetAsync<ResponseObject<CustomerData>>(url);

                if (response?.ApiResponseData?.Data == null)
                    return null;

                // Since your DTO already represents a single checkbook, just return it
                var checkBook = response.ApiResponseData.Data;

                return checkBook;
            }
            catch (Exception)
            {
                throw;
            }
        }

		public async Task<List<CheckBookLeaf>> GetChequeBookWithLeaves(string chequeBookId)
		{
			if (string.IsNullOrEmpty(chequeBookId))
				throw new ArgumentException("chequeBookId is required.");

			var url = string.Format(APICallHelper.GetChequeBookWithLeaves, chequeBookId);
			var response = await _apiHelper.GetAsync<ResponseObject<List<CheckBookLeaf>>>(url);

			return response.ApiResponseData.Data ?? new List<CheckBookLeaf>();
		}

		public async Task<CheckBookLeaf> GetChequeLeafDetails(string leafId)
		{
			if (string.IsNullOrEmpty(leafId))
				throw new ArgumentException("leafId is required");

			var url = string.Format(APICallHelper.GetChequeLeafDetails, leafId);
			var response = await _apiHelper.GetAsync<ResponseObject<CheckBookLeaf>>(url);

			return response.ApiResponseData.Data;
		}

		public async Task<ExecutionMessages> Create(CounterChecks model)
		{
			try
			{
				var response = await _apiHelper.PostAsync<ServiceResponse<bool>>(APICallHelper.CreateMobileMoneyV2, model);
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

		public async Task<ExecutionMessages> Update(CounterChecks model)
		{
			try
			{
				var response = await _apiHelper.PutAsync<ServiceResponse<CounterChecks>>(string.Format(APICallHelper.UpdateMobileMoneyV2, model.Id), model);
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
					GetExecutionMessages(null, false, "", MessagesResults.Failed,
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

	}
}
