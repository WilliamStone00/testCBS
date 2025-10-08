using BusinessServices;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.ChequeBookListing;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.CheckManagementSystem.Operations.ChequeBookListing
{
    public class ChequeBookMockService : BaseService
    {
        private static readonly List<ChequeBook> _mockChequeBooks = new List<ChequeBook>
        {
            new ChequeBook
            {
                Id = "CB001",
                CustomerId = "CUST001",
                CustomerName = "John Doe",
                AccountNumber = "3711000012345678",
                BranchId = "BR001",
                BranchName = "Main Branch",
                CategoryId = "CAT001",
                CategoryName = "Standard 25 Leaves",
                NumberOfLeaves = 25,
                Status = "Active",
                IssueDate = DateTime.Now.AddMonths(-1),
                ExpiryDate = DateTime.Now.AddMonths(11),
                FeeAmount = 5000,
                ChequeSeriesStart = "100001",
                ChequeSeriesEnd = "100025",
                ChequeLeaves = GenerateMockLeaves("CB001", 25, "100001")
            },
            new ChequeBook
            {
                Id = "CB002",
                CustomerId = "CUST002",
                CustomerName = "Jane Smith",
                AccountNumber = "3711000023456789",
                BranchId = "BR002",
                BranchName = "Downtown Branch",
                CategoryId = "CAT002",
                CategoryName = "Premium 50 Leaves",
                NumberOfLeaves = 50,
                Status = "Active",
                IssueDate = DateTime.Now.AddMonths(-2),
                ExpiryDate = DateTime.Now.AddMonths(10),
                FeeAmount = 8000,
                ChequeSeriesStart = "200001",
                ChequeSeriesEnd = "200050",
                ChequeLeaves = GenerateMockLeaves("CB002", 50, "200001")
            },
            new ChequeBook
            {
                Id = "CB003",
                CustomerId = "CUST003",
                CustomerName = "Mike Johnson",
                AccountNumber = "3711000034567890",
                BranchId = "BR001",
                BranchName = "Main Branch",
                CategoryId = "CAT001",
                CategoryName = "Standard 25 Leaves",
                NumberOfLeaves = 25,
                Status = "Cancelled",
                IssueDate = DateTime.Now.AddMonths(-3),
                ExpiryDate = DateTime.Now.AddMonths(9),
                FeeAmount = 5000,
                ChequeSeriesStart = "300001",
                ChequeSeriesEnd = "300025",
                ChequeLeaves = GenerateMockLeaves("CB003", 25, "300001")
            }
        };

        private static List<ChequeLeaf> GenerateMockLeaves(string chequeBookId, int count, string startSeries)
        {
            var leaves = new List<ChequeLeaf>();
            var random = new Random();

            for (int i = 1; i <= count; i++)
            {
                var leafNumber = int.Parse(startSeries) + i - 1;
                var statuses = new[] { "Available", "Used", "Available", "Available", "Blocked" };
                var status = statuses[random.Next(statuses.Length)];

                leaves.Add(new ChequeLeaf
                {
                    Id = $"{chequeBookId}-L{i}",
                    ChequeBookId = chequeBookId,
                    LeafNumber = i,
                    ChequeNumber = leafNumber.ToString("D6"),
                    Status = status,
                    UsedDate = status == "Used" ? DateTime.Now.AddDays(-random.Next(1, 30)) : (DateTime?)null,
                    Amount = status == "Used" ? random.Next(1000, 50000) : (decimal?)null,
                    Beneficiary = status == "Used" ? "Test Beneficiary" : null,
                    Remarks = status == "Blocked" ? "Reported lost" : null,
                    CreatedDate = DateTime.Now.AddMonths(-random.Next(1, 3))
                });
            }
            return leaves;
        }

        public async Task<CustomDataTable> GetChequeBooksDataTableAsync(ChequeBookQuery query)
        {
            await Task.Delay(100); // Simulate API delay

            var data = _mockChequeBooks.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(query.customerName))
            {
                data = data.Where(cb => cb.CustomerName.Contains(query.customerName));
            }

            if (!string.IsNullOrEmpty(query.branchId))
            {
                data = data.Where(cb => cb.BranchId == query.branchId);
            }

            if (!string.IsNullOrEmpty(query.status))
            {
                data = data.Where(cb => cb.Status == query.status);
            }

            if (query.fromDate.HasValue)
            {
                data = data.Where(cb => cb.IssueDate >= query.fromDate.Value);
            }

            if (query.toDate.HasValue)
            {
                data = data.Where(cb => cb.IssueDate <= query.toDate.Value);
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(query.Options.sortColumnName))
            {
                switch (query.Options.sortColumnName.ToLower())
                {
                    case "customername":
                        data = query.Options.sortDirection == "asc"
                            ? data.OrderBy(cb => cb.CustomerName)
                            : data.OrderByDescending(cb => cb.CustomerName);
                        break;
                    case "numberofleaves":
                        data = query.Options.sortDirection == "asc"
                            ? data.OrderBy(cb => cb.NumberOfLeaves)
                            : data.OrderByDescending(cb => cb.NumberOfLeaves);
                        break;
                    case "status":
                        data = query.Options.sortDirection == "asc"
                            ? data.OrderBy(cb => cb.Status)
                            : data.OrderByDescending(cb => cb.Status);
                        break;
                    case "issuedate":
                        data = query.Options.sortDirection == "asc"
                            ? data.OrderBy(cb => cb.IssueDate)
                            : data.OrderByDescending(cb => cb.IssueDate);
                        break;
                }
            }

            var totalRecords = data.Count();

            // Apply pagination
            var pagedData = data
                .Skip(query.Options.start)
                .Take(query.Options.pageSize)
                .ToList();

            return new CustomDataTable(
                draw: Convert.ToInt32(query.Options.draw),
                recordsTotal: totalRecords,
                recordsFiltered: totalRecords,
                data: pagedData.Cast<object>().ToList(),
                dataTableOptions: query.Options
            );
        }

        public async Task<ChequeBook> GetChequeBookByIdAsync(string chequeBookId)
        {
            await Task.Delay(50);
            return _mockChequeBooks.FirstOrDefault(cb => cb.Id == chequeBookId);
        }

        public async Task<ExecutionMessages> CancelChequeBookAsync(string chequeBookId, string cancellationReason)
        {
            await Task.Delay(100);

            var chequeBook = _mockChequeBooks.FirstOrDefault(cb => cb.Id == chequeBookId);
            if (chequeBook != null)
            {
                chequeBook.Status = "Cancelled";
                foreach (var leaf in chequeBook.ChequeLeaves)
                {
                    leaf.Status = "Cancelled";
                }

                GetExecutionMessages(null, true, "Cheque Book", MessagesResults.Success,
                    ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(),
                    null, $"Cheque book {chequeBookId} cancelled successfully. Reason: {cancellationReason}");
            }
            else
            {
                GetExecutionMessages(null, false, "Cheque Book", MessagesResults.Failed,
                    ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(),
                    null, "Cheque book not found");
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> MarkLeafAsUsedAsync(string leafId, string statement)
        {
            await Task.Delay(100);

            var leaf = _mockChequeBooks.SelectMany(cb => cb.ChequeLeaves).FirstOrDefault(l => l.Id == leafId);
            if (leaf != null)
            {
                leaf.Status = "Used";
                leaf.UsedDate = DateTime.Now;
                leaf.Remarks = statement;

                GetExecutionMessages(null, true, "Cheque Leaf", MessagesResults.Success,
                    ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(),
                    null, $"Leaf {leaf.ChequeNumber} marked as used. Statement: {statement}");
            }
            else
            {
                GetExecutionMessages(null, false, "Cheque Leaf", MessagesResults.Failed,
                    ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(),
                    null, "Leaf not found");
            }
            return ExecutionMessage;
        }

        public async Task<ExecutionMessages> BlockLeafAsync(string leafId, string blockReason)
        {
            await Task.Delay(100);

            var leaf = _mockChequeBooks.SelectMany(cb => cb.ChequeLeaves).FirstOrDefault(l => l.Id == leafId);
            if (leaf != null)
            {
                leaf.Status = "Blocked";
                leaf.Remarks = blockReason;

                GetExecutionMessages(null, true, "Cheque Leaf", MessagesResults.Success,
                    ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(),
                    null, $"Leaf {leaf.ChequeNumber} blocked. Reason: {blockReason}");
            }
            else
            {
                GetExecutionMessages(null, false, "Cheque Leaf", MessagesResults.Failed,
                    ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(),
                    null, "Leaf not found");
            }
            return ExecutionMessage;
        }
    }
}
