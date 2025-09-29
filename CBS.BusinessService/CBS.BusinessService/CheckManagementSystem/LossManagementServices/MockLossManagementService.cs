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
                    new CheckbookDto { CheckBookId = "1001", Category = "Personal", Status = "Active" },
                    new CheckbookDto { CheckBookId = "1002", Category = "Business", Status = "Active" },
                    new CheckbookDto { CheckBookId = "1003", Category = "Savings", Status = "Inactive" }
                }
            },
            new CustomerCheckbooksDto
            {
                CustomerId = "CUST002",
                Checkbooks = new List<CheckbookDto>
                {
                    new CheckbookDto { CheckBookId = "2001", Category = "Personal", Status = "Active" },
                    new CheckbookDto { CheckBookId = "2002", Category = "Premium", Status = "Active" }
                }
            },
            new CustomerCheckbooksDto
            {
                CustomerId = "CUST003",
                Checkbooks = new List<CheckbookDto>
                {
                    new CheckbookDto { CheckBookId = "3001", Category = "Business", Status = "Blocked" }
                }
            }
        };

        private static readonly List<CheckbookDetailDto> _mockCheckbookDetails = new List<CheckbookDetailDto>
        {
            new CheckbookDetailDto
            {
                CheckBookId = "1001",
                Category = "Personal",
                Status = "Active",
                CustomerId = "CUST001",
                AccountNumber = "ACC001001",
                BranchCode = "BR001",
                CheckLeaves = new List<CheckLeafDto>
                {
                    new CheckLeafDto { CheckLeafId = "100101", Status = "Available", CheckBookId = "1001" },
                    new CheckLeafDto { CheckLeafId = "100102", Status = "Available", CheckBookId = "1001" },
                    new CheckLeafDto { CheckLeafId = "100103", Status = "Used", CheckBookId = "1001" },
                    new CheckLeafDto { CheckLeafId = "100104", Status = "Available", CheckBookId = "1001" },
                    new CheckLeafDto { CheckLeafId = "100105", Status = "Lost", CheckBookId = "1001" }
                }
            },
            new CheckbookDetailDto
            {
                CheckBookId = "1002",
                Category = "Business",
                Status = "Active",
                CustomerId = "CUST001",
                AccountNumber = "ACC001002",
                BranchCode = "BR001",
                CheckLeaves = new List<CheckLeafDto>
                {
                    new CheckLeafDto { CheckLeafId = "100201", Status = "Available", CheckBookId = "1002" },
                    new CheckLeafDto { CheckLeafId = "100202", Status = "Available", CheckBookId = "1002" },
                    new CheckLeafDto { CheckLeafId = "100203", Status = "Available", CheckBookId = "1002" }
                }
            },
            new CheckbookDetailDto
            {
                CheckBookId = "2001",
                Category = "Personal",
                Status = "Active",
                CustomerId = "CUST002",
                AccountNumber = "ACC002001",
                BranchCode = "BR002",
                CheckLeaves = new List<CheckLeafDto>
                {
                    new CheckLeafDto { CheckLeafId = "200101", Status = "Used", CheckBookId = "2001" },
                    new CheckLeafDto { CheckLeafId = "200102", Status = "Available", CheckBookId = "2001" },
                    new CheckLeafDto { CheckLeafId = "200103", Status = "Cancelled", CheckBookId = "2001" }
                }
            }
        };

        // Mock branch data for dropdown
        private static readonly List<BranchDto> _mockBranches = new List<BranchDto>
        {
            new BranchDto { BranchId = "BR001", BranchName = "Main Branch" },
            new BranchDto { BranchId = "BR002", BranchName = "Downtown Branch" },
            new BranchDto { BranchId = "BR003", BranchName = "Westside Branch" },
            new BranchDto { BranchId = "BR004", BranchName = "Eastside Branch" },
            new BranchDto { BranchId = "BR005", BranchName = "North Branch" },
            new BranchDto { BranchId = "BR006", BranchName = "South Branch" }
        };

        public async Task<CheckLeafDetailDto> GetCheckLeafDetails(string checkLeafId)
        {
            // Find the check leaf in mock data
            var checkbook = _mockCheckbookDetails.FirstOrDefault(cb =>
                cb.CheckLeaves.Any(cl => cl.CheckLeafId == checkLeafId));

            if (checkbook != null)
            {
                var checkLeaf = checkbook.CheckLeaves.FirstOrDefault(cl => cl.CheckLeafId == checkLeafId);
                if (checkLeaf != null)
                {
                    return new CheckLeafDetailDto
                    {
                        CheckLeafId = checkLeafId,
                        CheckBookId = checkbook.CheckBookId,
                        Status = checkLeaf.Status,
                        CustomerId = checkbook.CustomerId,
                        AccountNumber = checkbook.AccountNumber,
                        BranchCode = checkbook.BranchCode,
                        LeafNumber = $"L{checkLeafId}",
                        IssueDate = DateTime.Now.AddMonths(-6),
                        ExpiryDate = DateTime.Now.AddMonths(6)
                    };
                }
            }

            // Fallback mock data if not found
            return new CheckLeafDetailDto
            {
                CheckLeafId = checkLeafId,
                CheckBookId = "1001",
                Status = "Available",
                CustomerId = "CUST001",
                AccountNumber = "ACC001001",
                BranchCode = "BR001",
                LeafNumber = $"L{checkLeafId}",
                IssueDate = DateTime.Now.AddMonths(-6),
                ExpiryDate = DateTime.Now.AddMonths(6)
            };
        }

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

        public Task<CheckbookDetailDto> GetCheckLeavesByCheckbook(string checkBookId)
        {
            return GetCheckLeavesByCheckbookMock(checkBookId);
        }

        public Task<CheckbookDetailDto> GetCheckLeavesByCheckbookMock(string checkBookId)
        {
            lock (_mockLock)
            {
                var checkbookDetail = _mockCheckbookDetails.FirstOrDefault(c => c.CheckBookId == checkBookId);
                return Task.FromResult(checkbookDetail);
            }
        }

        public Task<CheckbookDetailDto> GetCheckbookDetails(string checkBookId)
        {
            return GetCheckbookDetailsMock(checkBookId);
        }

        public Task<CheckbookDetailDto> GetCheckbookDetailsMock(string checkBookId)
        {
            lock (_mockLock)
            {
                var checkbookDetail = _mockCheckbookDetails.FirstOrDefault(c => c.CheckBookId == checkBookId);
                return Task.FromResult(checkbookDetail);
            }
        }

        public Task<LossRequestDto> RequestLossForCheckbook(string checkBookId)
        {
            return RequestLossForCheckbookMock(checkBookId);
        }

        public Task<LossRequestDto> RequestLossForCheckbookMock(string checkBookId)
        {
            lock (_mockLock)
            {
                var checkbook = _mockCheckbookDetails.FirstOrDefault(c => c.CheckBookId == checkBookId);
                if (checkbook == null)
                    return Task.FromResult<LossRequestDto>(null);

                var lossRequest = new LossRequestDto
                {
                    CustomerID = checkbook.CustomerId,
                    CheckBookID = checkBookId,
                    CheckLeafID = "", // Empty for checkbook level request
                    BranchID = checkbook.BranchCode,
                    AccountID = checkbook.AccountNumber,
                    NationalIDNumber = "NAT" + checkbook.CustomerId,
                    LossDate = DateTime.Now,
                    LossReportedBy = "Owner", // Default to Owner
                    LossReason = "Misplaced or Lost", // Default reason
                    Status = "Pending",
                    LossLocation = "Unknown", // Default location
                    CreatedDate = DateTime.Now,
                    Description = $"Loss declaration for entire checkbook {checkBookId}",
                    ClientSuggestion = "Block", // Default suggestion
                    // Third party fields will be null/empty by default
                    ThirdPartyName = "",
                    ThirdPartyNationalID = "",
                    ThirdPartyIDExpiry = null,
                    ThirdPartyDeliveryDate = null,
                    ThirdPartyIssueLocation = ""
                };

                return Task.FromResult(lossRequest);
            }
        }

        public Task<LossRequestDto> RequestLossForCheckLeaf(string checkLeafId)
        {
            return RequestLossForCheckLeafMock(checkLeafId);
        }

        public Task<LossRequestDto> RequestLossForCheckLeafMock(string checkLeafId)
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
                    CheckBookID = checkbook.CheckBookId,
                    CheckLeafID = checkLeafId,
                    BranchID = checkbook.BranchCode,
                    AccountID = checkbook.AccountNumber,
                    NationalIDNumber = "NAT" + checkbook.CustomerId,
                    LossDate = DateTime.Now,
                    LossReportedBy = "Owner", // Default to Owner
                    LossReason = "Misplaced or Lost", // Default reason
                    Status = "Pending",
                    LossLocation = "Unknown", // Default location
                    CreatedDate = DateTime.Now,
                    Description = $"Loss declaration for check leaf {checkLeafId} from checkbook {checkbook.CheckBookId}",
                    ClientSuggestion = "Cancel", // Default suggestion for leaf
                    // Third party fields will be null/empty by default
                    ThirdPartyName = "",
                    ThirdPartyNationalID = "",
                    ThirdPartyIDExpiry = null,
                    ThirdPartyDeliveryDate = null,
                    ThirdPartyIssueLocation = ""
                };

                return Task.FromResult(lossRequest);
            }
        }

        public Task<List<BranchDto>> GetBranches()
        {
            return Task.FromResult(_mockBranches);
        }

        public Task<List<string>> GetLossReasons()
        {
            var reasons = new List<string>
            {
                "Misplaced or Lost",
                "Stolen",
                "Damaged/Destroyed",
                "Others"
            };
            return Task.FromResult(reasons);
        }

        public Task<List<string>> GetClientSuggestions()
        {
            var suggestions = new List<string>
            {
                "Cancel",
                "Block",
                "Reprint"
            };
            return Task.FromResult(suggestions);
        }

        public Task<List<string>> GetReportedByOptions()
        {
            var options = new List<string>
            {
                "Owner",
                "ThirdParty"
            };
            return Task.FromResult(options);
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

            // Update check leaf status to "Lost" if a specific leaf was reported
            if (!string.IsNullOrEmpty(request.CheckLeafID))
            {
                lock (_mockLock)
                {
                    var checkbook = _mockCheckbookDetails.FirstOrDefault(c =>
                        c.CheckBookId == request.CheckBookID);

                    if (checkbook != null)
                    {
                        var checkLeaf = checkbook.CheckLeaves.FirstOrDefault(cl =>
                            cl.CheckLeafId == request.CheckLeafID);

                        if (checkLeaf != null)
                        {
                            checkLeaf.Status = "Lost";
                        }
                    }
                }
            }
            // Update checkbook status if entire checkbook was reported
            else if (!string.IsNullOrEmpty(request.CheckBookID))
            {
                lock (_mockLock)
                {
                    var checkbook = _mockCheckbookDetails.FirstOrDefault(c =>
                        c.CheckBookId == request.CheckBookID);

                    if (checkbook != null)
                    {
                        checkbook.Status = "Blocked";
                        // Mark all leaves as lost
                        foreach (var leaf in checkbook.CheckLeaves.Where(cl => cl.Status == "Available"))
                        {
                            leaf.Status = "Lost";
                        }
                    }
                }
            }

            GetExecutionMessages(request, true, "Loss Request", MessagesResults.Success,
                ExecutionProcessOption.InsertObject, SystemMessageStatus.Success.ToString(), null,
                $"Loss request submitted successfully for {(string.IsNullOrEmpty(request.CheckLeafID) ? "checkbook" : "check leaf")}. " +
                $"Client suggestion: {request.ClientSuggestion}. Status: {request.Status}");

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

    // Add this DTO for branch data
    public class BranchDto
    {
        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public string DisplayName => $"{BranchName} - {BranchId}";
    }
}