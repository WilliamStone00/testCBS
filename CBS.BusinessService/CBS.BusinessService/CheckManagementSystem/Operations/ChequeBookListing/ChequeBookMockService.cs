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
                id = "CB001",
                customerId = "CUST001",
                customerName = "John Doe",
                accountNumber = "3711000012345678",
                branchId = "BR001",
                branchName = "Main Branch",
                categoryId = "CAT001",
                categoryName = "Standard 25 Leaves",
                numberOfLeaves = 25,
                status = "Active",
                issueDate = DateTime.Now.AddMonths(-1),
                expiryDate = DateTime.Now.AddMonths(11),
                feeAmount = 5000,
                chequeSeriesStart = "100001",
                chequeSeriesEnd = "100025",
                ChequeLeaves = GenerateMockLeaves("CB001", 25, "100001")
            },
            new ChequeBook
            {
                id = "CB002",
                customerId = "CUST002",
                customerName = "Jane Smith",
                accountNumber = "3711000023456789",
                branchId = "BR002",
                branchName = "Downtown Branch",
                categoryId = "CAT002",
                categoryName = "Premium 50 Leaves",
                numberOfLeaves = 50,
                status = "Active",
                issueDate = DateTime.Now.AddMonths(-2),
                expiryDate = DateTime.Now.AddMonths(10),
                feeAmount = 8000,
                chequeSeriesStart = "200001",
                chequeSeriesEnd = "200050",
                ChequeLeaves = GenerateMockLeaves("CB002", 50, "200001")
            },
            new ChequeBook
            {
                id = "CB003",
                customerId = "CUST003",
                customerName = "Mike Johnson",
                accountNumber = "3711000034567890",
                branchId = "BR001",
                branchName = "Main Branch",
                categoryId = "CAT001",
                categoryName = "Standard 25 Leaves",
                numberOfLeaves = 25,
                status = "Cancelled",
                issueDate = DateTime.Now.AddMonths(-3),
                expiryDate = DateTime.Now.AddMonths(9),
                feeAmount = 5000,
                chequeSeriesStart = "300001",
                chequeSeriesEnd = "300025",
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
                    id = $"{chequeBookId}-L{i}",
                    chequeBookId = chequeBookId,
                    leafNumber = i,
                    chequeNumber = leafNumber.ToString("D6"),
                    status = status,
                    usedDate = status == "Used" ? DateTime.Now.AddDays(-random.Next(1, 30)) : (DateTime?)null,
                    amount = status == "Used" ? random.Next(1000, 50000) : (decimal?)null,
                    beneficiary = status == "Used" ? "Test Beneficiary" : null,
                    remarks = status == "Blocked" ? "Reported lost" : null,
                    createdDate = DateTime.Now.AddMonths(-random.Next(1, 3))
                });
            }
            return leaves;
        }

    /*    public async Task<CustomDataTable> GetChequeBooksDataTableAsync(ChequeBookQuery query)
        {
            await Task.Delay(100); // Simulate API delay

            var data = _mockChequeBooks.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(query.customerName))
            {
                data = data.Where(cb => cb.customerName.Contains(query.customerName));
            }

            if (!string.IsNullOrEmpty(query.branchId))
            {
                data = data.Where(cb => cb.branchId == query.branchId);
            }

            if (!string.IsNullOrEmpty(query.status))
            {
                data = data.Where(cb => cb.status == query.status);
            }

            if (query.fromDate.HasValue)
            {
                data = data.Where(cb => cb.issueDate >= query.fromDate.Value);
            }

            if (query.toDate.HasValue)
            {
                data = data.Where(cb => cb.issueDate <= query.toDate.Value);
            }

            // Apply sorting
            if (!string.IsNullOrEmpty(query.DataTableOptions.sortColumnName))
            {
                switch (query.DataTableOptions.sortColumnName.ToLower())
                {
                    case "customername":
                        data = query.DataTableOptions.sortDirection == "asc"
                            ? data.OrderBy(cb => cb.customerName)
                            : data.OrderByDescending(cb => cb.customerName);
                        break;
                    case "numberofleaves":
                        data = query.DataTableOptions.sortDirection == "asc"
                            ? data.OrderBy(cb => cb.numberOfLeaves)
                            : data.OrderByDescending(cb => cb.numberOfLeaves);
                        break;
                    case "status":
                        data = query.DataTableOptions.sortDirection == "asc"
                            ? data.OrderBy(cb => cb.status)
                            : data.OrderByDescending(cb => cb.status);
                        break;
                    case "issuedate":
                        data = query.DataTableOptions.sortDirection == "asc"
                            ? data.OrderBy(cb => cb.issueDate)
                            : data.OrderByDescending(cb => cb.issueDate);
                        break;
                }
            }

            var totalRecords = data.Count();

            // Apply pagination
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
*/
        public async Task<ChequeBook> GetChequeBookByIdAsync(string chequeBookId)
        {
            await Task.Delay(50);
            return _mockChequeBooks.FirstOrDefault(cb => cb.id == chequeBookId);
        }

        public async Task<ExecutionMessages> CancelChequeBookAsync(string chequeBookId, string cancellationReason)
        {
            await Task.Delay(100);

            var chequeBook = _mockChequeBooks.FirstOrDefault(cb => cb.id == chequeBookId);
            if (chequeBook != null)
            {
                chequeBook.status = "Cancelled";
                foreach (var leaf in chequeBook.ChequeLeaves)
                {
                    leaf.status = "Cancelled";
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

            var leaf = _mockChequeBooks.SelectMany(cb => cb.ChequeLeaves).FirstOrDefault(l => l.id == leafId);
            if (leaf != null)
            {
                leaf.status = "Used";
                leaf.usedDate = DateTime.Now;
                leaf.remarks = statement;

                GetExecutionMessages(null, true, "Cheque Leaf", MessagesResults.Success,
                    ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(),
                    null, $"Leaf {leaf.chequeNumber} marked as used. Statement: {statement}");
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

            var leaf = _mockChequeBooks.SelectMany(cb => cb.ChequeLeaves).FirstOrDefault(l => l.id == leafId);
            if (leaf != null)
            {
                leaf.status = "Blocked";
                leaf.remarks = blockReason;

                GetExecutionMessages(null, true, "Cheque Leaf", MessagesResults.Success,
                    ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(),
                    null, $"Leaf {leaf.chequeNumber} blocked. Reason: {blockReason}");
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
