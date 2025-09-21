using BusinessServices;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.ChequeRequest;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.CheckManagementSystem.Operations.ChequeRequestService
{


    public class ChequeRequestMockService : BaseService
    {
        // Our in-memory "database" table for cheque requests. Static to persist across requests.
        private static readonly List<ChequeBookRequest> _mockRequests = new List<ChequeBookRequest>
    {
        new ChequeBookRequest { Id = "REQ001", customerName = "John Doe", categoryName = "Standard - 25", requestDate = DateTime.Now.AddDays(-2), status = "Pending" },
        new ChequeBookRequest { Id = "REQ002", customerName = "Jane Smith", categoryName = "Business Gold - 100", requestDate = DateTime.Now.AddDays(-1), status = "Approved", approvalDate = DateTime.Now },
        new ChequeBookRequest { Id = "REQ003", customerName = "Peter Jones", categoryName = "Standard - 25", requestDate = DateTime.Now.AddHours(-5), status = "Delivered" },
        new ChequeBookRequest { Id = "REQ004", customerName = "Mary Williams", categoryName = "Standard - 25", requestDate = DateTime.Now.AddHours(-2), status = "Rejected" },
        new ChequeBookRequest { Id = "REQ005", customerName = "David Miller", categoryName = "Standard - 25", requestDate = DateTime.Now.AddDays(-3), status = "Pending" }

    };

        public Task<ExecutionMessages> CreateRequestAsync(ChequeBookRequest model)
        {
            model.Id = "REQ" + new Random().Next(100, 999);
            model.requestDate = DateTime.Now;
            model.status = "Pending";
            _mockRequests.Add(model);

            GetExecutionMessages(model, true, "Cheque Request", MessagesResults.Success,
                ExecutionProcessOption.InsertObject, MessagesResults.Success.ToString());

            return Task.FromResult(ExecutionMessage);
        }

        public Task<List<ChequeBookRequest>> GetAllRequestsAsync()
        {
            // Return a copy of the list, ordered by most recent
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
                request.approvalNote = rejectionNote; // Using same field for simplicity
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

        // New mock method for "Delivered"
        public Task<ExecutionMessages> MarkAsDeliveredAsync(string requestId, string deliveryNote)
        {
            var request = _mockRequests.FirstOrDefault(r => r.Id == requestId);
            if (request != null && request.status == "Approved")
            {
                request.status = "Delivered";
                GetExecutionMessages(null, true, "Delivery", MessagesResults.Success, ExecutionProcessOption.UpdateUpject, MessagesResults.Success.ToString(), null, "Cheque book marked as delivered.");
            }
            else
            {
                GetExecutionMessages(null, false, "Delivery", MessagesResults.Failed, ExecutionProcessOption.UpdateUpject, MessagesResults.Failed.ToString(), null, "Request is not in an approved state.");
            }
            return Task.FromResult(ExecutionMessage);
        }

        public Task<CustomDataTable> GetRequestsForDataTableAsync(ChequeRequestQuery query)
        {
            // Start with the full, unfiltered list of mock data.
            IEnumerable<ChequeBookRequest> filteredData = _mockRequests;

            int recordsTotal = _mockRequests.Count;

            // --- 1. SIMULATE FILTERING ---
            // Apply each filter from the query object if it has a value.

            // Filter by Customer Name (case-insensitive search)
            if (!string.IsNullOrWhiteSpace(query.CustomerFilter))
            {
                filteredData = filteredData.Where(r =>
                    r.customerName.IndexOf(query.CustomerFilter, StringComparison.OrdinalIgnoreCase) >= 0
                );
            }

            // Filter by Category
            if (!string.IsNullOrWhiteSpace(query.CategoryFilter))
            {
                filteredData = filteredData.Where(r => r.categoryName == query.CategoryFilter);
            }

            // Filter by Status
            if (!string.IsNullOrWhiteSpace(query.StatusFilter))
            {
                filteredData = filteredData.Where(r =>
                    r.status.Equals(query.StatusFilter, StringComparison.OrdinalIgnoreCase)
                );
            }

            int recordsFiltered = filteredData.Count();

            // --- 2. SIMULATE SORTING ---
            // Apply sorting based on the column name and direction from the DataTable.
            if (query.Options != null && !string.IsNullOrEmpty(query.Options.sortColumnName))
            {
                bool isAscending = query.Options.sortDirection?.ToLower() == "asc";

                switch (query.Options.sortColumnName)
                {
                    case "CustomerName":
                        filteredData = isAscending ? filteredData.OrderBy(r => r.customerName) : filteredData.OrderByDescending(r => r.customerName);
                        break;
                    case "CategoryName":
                        filteredData = isAscending ? filteredData.OrderBy(r => r.categoryName) : filteredData.OrderByDescending(r => r.categoryName);
                        break;
                    case "RequestDate":
                        filteredData = isAscending ? filteredData.OrderBy(r => r.requestDate) : filteredData.OrderByDescending(r => r.requestDate);
                        break;
                        // Add other sortable columns here if needed
                }
            }

            // --- 3. SIMULATE PAGINATION ---
            // Apply Skip() and Take() to get only the data for the current page.
            if (query.Options != null)
            {
                filteredData = filteredData.Skip(query.Options.start).Take(query.Options.pageSize);
            }

            // --- 4. ASSEMBLE THE FINAL RESPONSE ---
            // Create the CustomDataTable object that the controller expects.
            var dataTable = new CustomDataTable
            {
               // draw = query.Options?.draw ?? "0",
                draw = Convert.ToInt32(query.Options.draw),
                recordsTotal = recordsTotal,
                recordsFiltered = recordsFiltered,
                data = filteredData.ToList() // The final, paged, and sorted data
            };

            return Task.FromResult(dataTable);
        }

    }
}
