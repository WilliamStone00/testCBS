using BusinessServices;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.ChequeRequest;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CBS.BusinessService.CheckManagementSystem.Operations.ChequeRequestService
{
    public class ChequeRequestMockService : BaseService
    {
        // Updated mock data with proper backend format
        private static readonly List<ChequeBookRequest> _mockRequests = new List<ChequeBookRequest>
        {
            new ChequeBookRequest
            {
                Id = "REQ001",
                customerId = "CUST001",
                customerName = "John Doe",
                categoryId = "Category001",
                categoryName = "Standard - 25",
                requestDate = DateTime.Now.AddDays(-2),
                status = "Pending",
                checkBookAccount = "ACC001",
                subscriptionPaymentAccount = "ACC002",
                branchId = "BR001",
                bankId = "BK001",
                requestNote = "Initial request",
                notifyOnRejection = true,
                notifyOnAnyTransaction = false,
                notifyOnClearance = true,
                automaticRenewal = false,
                numberofCheckBooks = 1,
                numberOfPages = 25,
                feeAmount = 25.00m,
                transactionAmount = 0.00m
            },
            new ChequeBookRequest
            {
                Id = "REQ002",
                customerId = "CUST002",
                customerName = "Jane Smith",
                categoryId = "Category002",
                categoryName = "Business Gold - 100",
                requestDate = DateTime.Now.AddDays(-1),
                status = "Approved",
                approvalDate = DateTime.Now,
                checkBookAccount = "ACC003",
                subscriptionPaymentAccount = "ACC004",
                branchId = "BR001",
                bankId = "BK001",
                requestNote = "Business account",
                notifyOnRejection = true,
                notifyOnAnyTransaction = true,
                notifyOnClearance = true,
                automaticRenewal = true,
                numberofCheckBooks = 1,
                numberOfPages = 100,
                feeAmount = 100.00m,
                transactionAmount = 0.00m
            },
            new ChequeBookRequest
            {
                Id = "REQ003",
                customerId = "CUST003",
                customerName = "Peter Jones",
                categoryId = "Category001",
                categoryName = "Standard - 25",
                requestDate = DateTime.Now.AddHours(-5),
                status = "Delivered",
                checkBookAccount = "ACC005",
                subscriptionPaymentAccount = "ACC006",
                branchId = "BR002",
                bankId = "BK001",
                requestNote = "Personal account",
                notifyOnRejection = false,
                notifyOnAnyTransaction = true,
                notifyOnClearance = false,
                automaticRenewal = false,
                numberofCheckBooks = 1,
                numberOfPages = 25,
                feeAmount = 25.00m,
                transactionAmount = 0.00m
            }
        };

        public Task<ExecutionMessages> CreateRequestAsync(ChequeBookRequest model)
        {
            model.Id = "REQ" + (_mockRequests.Count + 1).ToString("D3");
            model.requestDate = DateTime.Now;
            model.status = "Pending";
            _mockRequests.Add(model);

            GetExecutionMessages(model, true, "Cheque Request", MessagesResults.Success,
                ExecutionProcessOption.InsertObject, MessagesResults.Success.ToString());

            return Task.FromResult(ExecutionMessage);
        }

        public async Task<ExecutionMessages> UpdateRequestAsync(ChequeBookRequest model)
        {
            // MOCK IMPLEMENTATION:
            var existingRequest = _mockRequests.FirstOrDefault(r => r.Id == model.Id);
            if (existingRequest != null)
            {
                // Update the properties of the existing object
                existingRequest.customerId = model.customerId;
                existingRequest.categoryId = model.categoryId;
                existingRequest.checkBookAccount = model.checkBookAccount;
                existingRequest.subscriptionPaymentAccount = model.subscriptionPaymentAccount;
                existingRequest.requestNote = model.requestNote;
                // ... update notification properties ...

                GetExecutionMessages(existingRequest, true, "Cheque Request", MessagesResults.Success,
                    ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null, "Request updated successfully.");
            }
            else
            {
                GetExecutionMessages(model, false, "Cheque Request", MessagesResults.Failed,
                    ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null, "Request not found.");
            }
            return ExecutionMessage;

            /*
            // REAL SERVICE IMPLEMENTATION would look like this:
            try
            {
                // PUT /api/v1/cheque-requests/{id}
                string url = string.Format(APICallHelper.UpdateChequeRequest, model.Id); // Assumes this constant exists
                var response = await _apiHelper.PutAsync<ServiceResponse<ChequeBookRequest>>(url, model);
                if (response.IsSuccess) { ... } else { ... }
            }
            catch (Exception ex) { ... }
            return ExecutionMessage;
            */
        }


        public Task<CustomDataTable> GetRequestsForDataTableAsync(ChequeRequestQuery query)
        {
            // C# 7.3 friendly null-check (no ??=)
            if (query == null) query = new ChequeRequestQuery();

            int start = query.Options != null ? query.Options.start : 0;
            int pageSize = (query.Options != null && query.Options.pageSize > 0) ? query.Options.pageSize : 10;

            // Start with IQueryable (avoid typing to IOrderedQueryable to prevent assignment problems)
            IQueryable<ChequeBookRequest> data = _mockRequests.AsQueryable();

            // Default sort first (so recordsTotal counts the total set in a known order)
            data = data.OrderByDescending(r => r.requestDate);

            int recordsTotal = data.Count();

            // -----------------------
            // Filters
            // -----------------------
            if (!string.IsNullOrWhiteSpace(query.CustomerName))
            {
                var nameFilter = query.CustomerName.Trim();
                data = data.Where(r => (r.customerName ?? "").IndexOf(nameFilter, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            if (!string.IsNullOrWhiteSpace(query.CategoryName))
            {
                var cat = query.CategoryName.Trim();
                data = data.Where(r => string.Equals(r.categoryName ?? "", cat, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                var st = query.Status.Trim();
                data = data.Where(r => string.Equals(r.status ?? "", st, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(query.BranchId))
            {
                var bid = query.BranchId.Trim();
                data = data.Where(r => string.Equals(r.branchId ?? "", bid, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(query.BankId))
            {
                var bk = query.BankId.Trim();
                data = data.Where(r => string.Equals(r.bankId ?? "", bk, StringComparison.OrdinalIgnoreCase));
            }

            if (query.StartDate.HasValue)
            {
                var s = query.StartDate.Value;
                data = data.Where(r => r.requestDate >= s);
            }

            if (query.EndDate.HasValue)
            {
                var e = query.EndDate.Value;
                data = data.Where(r => r.requestDate <= e);
            }

            if (query.NotifyOnRejection.HasValue)
                data = data.Where(r => r.notifyOnRejection == query.NotifyOnRejection.Value);

            if (query.AutomaticRenewal.HasValue)
                data = data.Where(r => r.automaticRenewal == query.AutomaticRenewal.Value);

            if (query.NotifyOnAnyTransaction.HasValue)
                data = data.Where(r => r.notifyOnAnyTransaction == query.NotifyOnAnyTransaction.Value);

            if (query.NotifyOnClearance.HasValue)
                data = data.Where(r => r.notifyOnClearance == query.NotifyOnClearance.Value);

            int recordsFiltered = data.Count();

            // -----------------------
            // Sorting - use IOrderedQueryable locally to avoid type issues
            // -----------------------
            IOrderedQueryable<ChequeBookRequest> ordered = null;

            if (query.Options != null && !string.IsNullOrWhiteSpace(query.Options.sortColumnName))
            {
                bool asc = string.Equals(query.Options.sortDirection, "asc", StringComparison.OrdinalIgnoreCase);
                switch (query.Options.sortColumnName.Trim().ToLower())
                {
                    case "customername":
                        ordered = asc ? data.OrderBy(r => r.customerName) : data.OrderByDescending(r => r.customerName);
                        break;
                    case "categoryname":
                        ordered = asc ? data.OrderBy(r => r.categoryName) : data.OrderByDescending(r => r.categoryName);
                        break;
                    case "requestdate":
                        ordered = asc ? data.OrderBy(r => r.requestDate) : data.OrderByDescending(r => r.requestDate);
                        break;
                    case "status":
                        ordered = asc ? data.OrderBy(r => r.status) : data.OrderByDescending(r => r.status);
                        break;
                    default:
                        ordered = data.OrderByDescending(r => r.requestDate);
                        break;
                }
            }

            // If no explicit ordered was created, apply default ordering
            var finalQuery = (ordered != null) ? (IQueryable<ChequeBookRequest>)ordered : data.OrderByDescending(r => r.requestDate);

            // -----------------------
            // Pagination
            // -----------------------
            var paged = finalQuery.Skip(start).Take(pageSize).ToList();

            // -----------------------
            // draw handling - query.Options.draw might be string or int depending on your DTO
            // Ensure we return an int draw
            // -----------------------
            int draw = 1;
            if (query?.Options != null)
            {
                // Try to parse safely whether draw is int or string
                object rawDraw = query.Options.draw as object;
                if (rawDraw != null)
                {
                    int parsed;
                    if (rawDraw is int) draw = (int)rawDraw;
                    else if (int.TryParse(rawDraw.ToString(), out parsed)) draw = parsed;
                }
            }

            var result = new CustomDataTable
            {
                draw = draw,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsFiltered,
                data = paged
            };

            return Task.FromResult(result);
        }



        public Task<List<ChequeBookRequest>> GetAllRequestsAsync()
        {
            return Task.FromResult(_mockRequests.OrderByDescending(r => r.requestDate).ToList());
        }

        public Task<ChequeBookRequest> GetRequestByIdAsync(string requestId)
        {
            var request = _mockRequests.FirstOrDefault(r => r.Id == requestId);
            return Task.FromResult(request);
        }

        public Task<ExecutionMessages> ApproveRequestAsync(string requestId, string approvalNote)
        {
            var request = _mockRequests.FirstOrDefault(r => r.Id == requestId);
            if (request != null && request.status == "Pending")
            {
                request.status = "Approved";
                request.approvalNote = approvalNote;
                request.approvalDate = DateTime.Now;
                GetExecutionMessages(null, true, "Approval", MessagesResults.Success,
                    ExecutionProcessOption.UpdateUpject, MessagesResults.Success.ToString(), null, "Request approved successfully.");
            }
            else
            {
                GetExecutionMessages(null, false, "Approval", MessagesResults.Failed,
                    ExecutionProcessOption.UpdateUpject, MessagesResults.Failed.ToString(), null, "Request could not be found or is not in a pending state.");
            }
            return Task.FromResult(ExecutionMessage);
        }

        public Task<ExecutionMessages> RejectRequestAsync(string requestId, string rejectionNote)
        {
            var request = _mockRequests.FirstOrDefault(r => r.Id == requestId);
            if (request != null && request.status == "Pending")
            {
                request.status = "Rejected";
                request.approvalNote = rejectionNote;
                GetExecutionMessages(null, true, "Rejection", MessagesResults.Success,
                    ExecutionProcessOption.UpdateUpject, MessagesResults.Success.ToString(), null, "Request rejected successfully.");
            }
            else
            {
                GetExecutionMessages(null, false, "Rejection", MessagesResults.Failed,
                    ExecutionProcessOption.UpdateUpject, MessagesResults.Failed.ToString(), null, "Request could not be found or is not in a pending state.");
            }
            return Task.FromResult(ExecutionMessage);
        }

        public Task<ExecutionMessages> ReviewRequestAsync(string requestId, string reviewNote)
        {
            var request = _mockRequests.FirstOrDefault(r => r.Id == requestId);
            if (request != null && request.status == "Pending")
            {
                request.status = "Reviewed";
                request.approvalNote = reviewNote;
                GetExecutionMessages(null, true, "Reviewed", MessagesResults.Success,
                    ExecutionProcessOption.UpdateUpject, MessagesResults.Success.ToString(), null, "Request reviewed successfully.");
            }
            else
            {
                GetExecutionMessages(null, false, "Reviewed", MessagesResults.Failed,
                    ExecutionProcessOption.UpdateUpject, MessagesResults.Failed.ToString(), null, "Request could not be found or is not in a pending state.");
            }
            return Task.FromResult(ExecutionMessage);
        }

        public Task<ExecutionMessages> MarkAsDeliveredAsync(string requestId, string deliveryNote)
        {
            var request = _mockRequests.FirstOrDefault(r => r.Id == requestId);
            if (request != null && request.status == "Approved")
            {
                request.status = "Delivered";
                request.approvalNote = deliveryNote;
                GetExecutionMessages(null, true, "Delivery", MessagesResults.Success,
                    ExecutionProcessOption.UpdateUpject, MessagesResults.Success.ToString(), null, "Cheque book marked as delivered.");
            }
            else
            {
                GetExecutionMessages(null, false, "Delivery", MessagesResults.Failed,
                    ExecutionProcessOption.UpdateUpject, MessagesResults.Failed.ToString(), null, "Request is not in an approved state.");
            }
            return Task.FromResult(ExecutionMessage);
        }
    }
}