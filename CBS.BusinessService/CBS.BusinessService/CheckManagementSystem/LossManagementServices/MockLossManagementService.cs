using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessServices;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.LossManagementSystem;
using CBS.FrontDesk.Data.Message;

namespace CBS.BusinessService.CheckManagementSystem.LossManagementSystem
{
    public class MockLossManagementService : BaseService
    {
        private static readonly List<CustomerCheckbooksDto> _mockCustomerCheckbooks = new List<CustomerCheckbooksDto>
        {
            new CustomerCheckbooksDto
            {
                CustomerId = "CUST001",
                Checkbooks = new List<CheckbookDto>
                {
                    new CheckbookDto { CheckBookId = 1001, Category = "Personal", Status = "Active" },
                    new CheckbookDto { CheckBookId = 1002, Category = "Business", Status = "Active" },
                    new CheckbookDto { CheckBookId = 1003, Category = "Savings", Status = "Inactive" }
                }
            },
            new CustomerCheckbooksDto
            {
                CustomerId = "CUST002",
                Checkbooks = new List<CheckbookDto>
                {
                    new CheckbookDto { CheckBookId = 2001, Category = "Personal", Status = "Active" },
                    new CheckbookDto { CheckBookId = 2002, Category = "Premium", Status = "Active" }
                }
            },
            new CustomerCheckbooksDto
            {
                CustomerId = "CUST003",
                Checkbooks = new List<CheckbookDto>
                {
                    new CheckbookDto { CheckBookId = 3001, Category = "Business", Status = "Blocked" }
                }
            }
        };

        private static readonly List<CheckbookDetailDto> _mockCheckbookDetails = new List<CheckbookDetailDto>
        {
            new CheckbookDetailDto
            {
                CheckBookId = 1001,
                Category = "Personal",
                Status = "Active",
                CustomerId = "CUST001",
                AccountNumber = "ACC001001",
                BranchCode = "BR001",
                CheckLeaves = new List<CheckLeafDto>
                {
                    new CheckLeafDto { CheckLeafId = 100101, Status = "Available", CheckBookId = 1001 },
                    new CheckLeafDto { CheckLeafId = 100102, Status = "Available", CheckBookId = 1001 },
                    new CheckLeafDto { CheckLeafId = 100103, Status = "Used", CheckBookId = 1001 },
                    new CheckLeafDto { CheckLeafId = 100104, Status = "Available", CheckBookId = 1001 },
                    new CheckLeafDto { CheckLeafId = 100105, Status = "Lost", CheckBookId = 1001 }
                }
            },
            new CheckbookDetailDto
            {
                CheckBookId = 1002,
                Category = "Business",
                Status = "Active",
                CustomerId = "CUST001",
                AccountNumber = "ACC001002",
                BranchCode = "BR001",
                CheckLeaves = new List<CheckLeafDto>
                {
                    new CheckLeafDto { CheckLeafId = 100201, Status = "Available", CheckBookId = 1002 },
                    new CheckLeafDto { CheckLeafId = 100202, Status = "Available", CheckBookId = 1002 },
                    new CheckLeafDto { CheckLeafId = 100203, Status = "Available", CheckBookId = 1002 }
                }
            },
            new CheckbookDetailDto
            {
                CheckBookId = 2001,
                Category = "Personal",
                Status = "Active",
                CustomerId = "CUST002",
                AccountNumber = "ACC002001",
                BranchCode = "BR002",
                CheckLeaves = new List<CheckLeafDto>
                {
                    new CheckLeafDto { CheckLeafId = 200101, Status = "Used", CheckBookId = 2001 },
                    new CheckLeafDto { CheckLeafId = 200102, Status = "Available", CheckBookId = 2001 },
                    new CheckLeafDto { CheckLeafId = 200103, Status = "Cancelled", CheckBookId = 2001 }
                }
            }
        };

        private static readonly object _mockLock = new object();

        public Task<CustomerCheckbooksDto> GetCustomerCheckbooks(string memberRef)
        {
            return GetCustomerCheckbooksMock(memberRef);
        }

        public Task<CustomerCheckbooksDto> GetCustomerCheckbooksMock(string memberRef)
        {
            if (string.IsNullOrWhiteSpace(memberRef))
            {
                return Task.FromResult<CustomerCheckbooksDto>(null);
            }

            lock (_mockLock)
            {
                var customerData = _mockCustomerCheckbooks.FirstOrDefault(c =>
                    c.CustomerId.Equals(memberRef, StringComparison.OrdinalIgnoreCase));

                return Task.FromResult(customerData);
            }
        }

        public Task<CheckbookDetailDto> GetCheckLeavesByCheckbook(int checkBookId)
        {
            return GetCheckLeavesByCheckbookMock(checkBookId);
        }

        public Task<CheckbookDetailDto> GetCheckLeavesByCheckbookMock(int checkBookId)
        {
            lock (_mockLock)
            {
                var checkbookDetail = _mockCheckbookDetails.FirstOrDefault(c => c.CheckBookId == checkBookId);
                return Task.FromResult(checkbookDetail);
            }
        }

        public Task<CheckbookDetailDto> GetCheckbookDetails(int checkBookId)
        {
            return GetCheckbookDetailsMock(checkBookId);
        }

        public Task<CheckbookDetailDto> GetCheckbookDetailsMock(int checkBookId)
        {
            lock (_mockLock)
            {
                var checkbookDetail = _mockCheckbookDetails.FirstOrDefault(c => c.CheckBookId == checkBookId);
                return Task.FromResult(checkbookDetail);
            }
        }

        public Task<LossRequestDto> RequestLossForCheckbook(int checkBookId)
        {
            return RequestLossForCheckbookMock(checkBookId);
        }

        public Task<LossRequestDto> RequestLossForCheckbookMock(int checkBookId)
        {
            lock (_mockLock)
            {
                var checkbook = _mockCheckbookDetails.FirstOrDefault(c => c.CheckBookId == checkBookId);
                if (checkbook == null)
                    return Task.FromResult<LossRequestDto>(null);

                var lossRequest = new LossRequestDto
                {
                    CustomerID = checkbook.CustomerId,
                    CheckBookID = checkBookId.ToString(),
                    BranchID = checkbook.BranchCode,
                    AccountID = checkbook.AccountNumber,
                    NationalIDNumber = "NAT" + checkbook.CustomerId,
                    LossDate = DateTime.Now,
                    LossReporteBy = "System User",
                    LossReason = "Checkbook loss declaration",
                    LossStatus = "Pending"
                };

                return Task.FromResult(lossRequest);
            }
        }

        public Task<LossRequestDto> RequestLossForCheckLeaf(int checkLeafId)
        {
            return RequestLossForCheckLeafMock(checkLeafId);
        }

        public Task<LossRequestDto> RequestLossForCheckLeafMock(int checkLeafId)
        {
            lock (_mockLock)
            {
                var checkbook = _mockCheckbookDetails.FirstOrDefault(c =>
                    c.CheckLeaves.Any(cl => cl.CheckLeafId == checkLeafId));

                if (checkbook == null)
                    return Task.FromResult<LossRequestDto>(null);

                var checkLeaf = checkbook.CheckLeaves.FirstOrDefault(cl => cl.CheckLeafId == checkLeafId);

                var lossRequest = new LossRequestDto
                {
                    CustomerID = checkbook.CustomerId,
                    CheckBookID = checkbook.CheckBookId.ToString(),
                    CheckLeafID = checkLeafId.ToString(),
                    BranchID = checkbook.BranchCode,
                    AccountID = checkbook.AccountNumber,
                    NationalIDNumber = "NAT" + checkbook.CustomerId,
                    LossDate = DateTime.Now,
                    LossReporteBy = "System User",
                    LossReason = $"Check leaf {checkLeafId} loss declaration",
                    LossStatus = "Pending"
                };

                return Task.FromResult(lossRequest);
            }
        }

        public Task<ExecutionMessages> SubmitLossRequest(LossRequestDto request)
        {
            return SubmitLossRequestMock(request);
        }

        public Task<ExecutionMessages> SubmitLossRequestMock(LossRequestDto request)
        {
            if (request == null)
            {
                GetExecutionMessages(null, false, "Loss Request", MessagesResults.Failed,
                    ExecutionProcessOption.InsertObject, SystemMessageStatus.Failed.ToString(), null, "Invalid request data.");
                return Task.FromResult(ExecutionMessage);
            }

            // Simulate processing delay
            System.Threading.Thread.Sleep(1000);

            // Update check leaf Status to "Lost" if a specific leaf was reported
            if (!string.IsNullOrEmpty(request.CheckLeafID))
            {
                lock (_mockLock)
                {
                    var checkbook = _mockCheckbookDetails.FirstOrDefault(c =>
                        c.CheckBookId.ToString() == request.CheckBookID);

                    if (checkbook != null)
                    {
                        var checkLeaf = checkbook.CheckLeaves.FirstOrDefault(cl =>
                            cl.CheckLeafId.ToString() == request.CheckLeafID);

                        if (checkLeaf != null)
                        {
                            checkLeaf.Status = "Lost";
                        }
                    }
                }
            }

            GetExecutionMessages(request, true, "Loss Request", MessagesResults.Success,
                ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null,
                $"Loss request submitted successfully for {(string.IsNullOrEmpty(request.CheckLeafID) ? "checkbook" : "check leaf")}.");

            return Task.FromResult(ExecutionMessage);
        }

        // Helper method to get all mock customer IDs for testing
        public Task<List<string>> GetMockCustomerIds()
        {
            lock (_mockLock)
            {
                var customerIds = _mockCustomerCheckbooks.Select(c => c.CustomerId).ToList();
                return Task.FromResult(customerIds);
            }
        }
    }
}