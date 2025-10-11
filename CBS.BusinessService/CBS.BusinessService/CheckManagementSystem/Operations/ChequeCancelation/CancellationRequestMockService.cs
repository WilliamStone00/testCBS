using BusinessServices;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.ChequeCancelation;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.CheckManagementSystem.Operations.ChequeCancelation
{
    public class ChequeCancellationMockService : BaseService
    {
        private static readonly List<CancellationRequest> _mockRequests = new List<CancellationRequest>();
        private static int _requestCounter = 1;

        public async Task<IEnumerable<CancellationRequest>> GetCancellationRequestsAsync()
        {
            await Task.Delay(100);
            return _mockRequests;
        }

        public async Task<CancellationRequest> GetCancellationRequestByIdAsync(string id)
        {
            await Task.Delay(50);
            return _mockRequests.FirstOrDefault(r => r.Id == id);
        }

        public async Task<ExecutionMessages> CreateCancellationRequestAsync(CancellationRequest model)
        {
            await Task.Delay(100);

            try
            {
                model.Id = $"CR{_requestCounter++:D6}";
                model.RequestedDate = DateTime.UtcNow;
                model.Status = CancellationStatus.Pending;

                _mockRequests.Add(model);

                GetExecutionMessages(model, true, "Cancellation Request", MessagesResults.Success,
                    ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null,
                    "Mock cancellation request created successfully.");
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, "Cancellation Request", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }

            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> UpdateCancellationRequestAsync(CancellationRequest model)
        {
            await Task.Delay(100);

            try
            {
                var existing = _mockRequests.FirstOrDefault(r => r.Id == model.Id);
                if (existing != null)
                {
                    // Update properties
                    existing.Reason = model.Reason;
                    existing.AdditionalNotes = model.AdditionalNotes;
                    existing.ModifiedDate = DateTime.UtcNow;

                    GetExecutionMessages(existing, true, "Cancellation Request", MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null,
                        "Mock cancellation request updated successfully.");
                }
                else
                {
                    GetExecutionMessages(model, false, "Cancellation Request", MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null,
                        "Request not found");
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(model, false, "Cancellation Request", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }

            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> ReviewCancellationRequestAsync(string requestId, bool isApproved, string statement)
        {
            await Task.Delay(100);

            try
            {
                var request = _mockRequests.FirstOrDefault(r => r.Id == requestId);
                if (request != null)
                {
                    request.Status = isApproved ? CancellationStatus.Approved : CancellationStatus.Rejected;
                    request.ReviewedDate = DateTime.UtcNow;
                    request.ReviewComments = statement;
                    request.ModifiedDate = DateTime.UtcNow;

                    GetExecutionMessages(null, true, "Cancellation Request", MessagesResults.Success,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(), null,
                        $"Mock request {(isApproved ? "approved" : "rejected")} successfully.");
                }
                else
                {
                    GetExecutionMessages(null, false, "Cancellation Request", MessagesResults.Failed,
                        ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(), null,
                        "Request not found");
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, "Cancellation Request", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }

            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> DeactivateCancellationRequestAsync(string requestId)
        {
            await Task.Delay(100);

            try
            {
                var request = _mockRequests.FirstOrDefault(r => r.Id == requestId);
                if (request != null)
                {
                    request.Status = CancellationStatus.Cancelled;
                    request.ModifiedDate = DateTime.UtcNow;

                    GetExecutionMessages(null, true, $"Cancellation Request ID: {requestId}", MessagesResults.Success,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Success.ToString(), null,
                        "Mock cancellation request deactivated successfully.");
                }
                else
                {
                    GetExecutionMessages(null, false, $"Cancellation Request ID: {requestId}", MessagesResults.Failed,
                        ExecutionProcessOption.DeleteObject, SystemMessageStatus.Failed.ToString(), null,
                        "Request not found");
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, $"Cancellation Request ID: {requestId}", MessagesResults.Error,
                    ExecutionProcessOption.TryCatch, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }

            return ExecutionMessage;
        }

        public async Task<CustomDataTable> GetCancellationRequestsDataTableAsync(CancellationRequestQuery query)
        {
            await Task.Delay(100);

            var data = _mockRequests.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(query.Status))
            {
                var Status = (CancellationStatus)Enum.Parse(typeof(CancellationStatus), query.Status);
                data = data.Where(r => r.Status == Status);
            }

            if (!string.IsNullOrEmpty(query.BranchId))
            {
                data = data.Where(r => r.BranchId == query.BranchId);
            }

            if (query.FromDate.HasValue)
            {
                data = data.Where(r => r.RequestedDate >= query.FromDate.Value);
            }

            if (query.ToDate.HasValue)
            {
                data = data.Where(r => r.RequestedDate <= query.ToDate.Value);
            }

            var totalRecords = data.Count();
            var pagedData = data
                .Skip(query.DataTableOptions.start)
                .Take(query.DataTableOptions.pageSize)
                .ToList();

            return new CustomDataTable(
               draw: Convert.ToInt32(query.DataTableOptions.draw),
                recordsTotal: totalRecords,
                recordsFiltered: totalRecords,
                data: pagedData.Cast<object>().ToList(),
                dataTableOptions: query.DataTableOptions
            );
        }
    }
}
