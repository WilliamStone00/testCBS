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