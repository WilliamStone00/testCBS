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
    //public class ChequeBookMockService : BaseService
    //{
    //    private static readonly List<ChequeBook> _mockChequeBooks = new List<ChequeBook>
    //    {
     

    //    };

    //    private static List<ChequeLeaf> GenerateMockLeaves(string chequeBookId, int count, string startSeries)
    //    {
    //        var leaves = new List<ChequeLeaf>();
    //        var random = new Random();

    //        for (int i = 1; i <= count; i++)
    //        {
    //            var leafNumber = int.Parse(startSeries) + i - 1;
    //            var Statuses = new[] { "Available", "Used", "Available", "Available", "Blocked" };
    //            var Status = Statuses[random.Next(Statuses.Length)];

    //            leaves.Add(new ChequeLeaf
    //            {

    //                Id = $"{chequeBookId}-L{i}",
    //                ChequeBookId = chequeBookId,
    //                LeafNumber = i,
    //                ChequeNumber = leafNumber.ToString("D6"),
    //                Status = status,
    //                UsedDate = status == "Used" ? DateTime.Now.AddDays(-random.Next(1, 30)) : (DateTime?)null,
    //                Amount = status == "Used" ? random.Next(1000, 50000) : (decimal?)null,
    //                Beneficiary = status == "Used" ? "Test Beneficiary" : null,
    //                Remarks = status == "Blocked" ? "Reported lost" : null,
    //                CreatedDate = DateTime.Now.AddMonths(-random.Next(1, 3))

    //                id = $"{chequeBookId}-L{i}",
    //                chequeBookId = chequeBookId,
    //                leafNumber = i,
    //                chequeNumber = leafNumber.ToString("D6"),
    //                status = Status,
    //                usedDate = Status == "Used" ? DateTime.Now.AddDays(-random.Next(1, 30)) : (DateTime?)null,
    //                amount = Status == "Used" ? random.Next(1000, 50000) : (decimal?)null,
    //                beneficiary = Status == "Used" ? "Test Beneficiary" : null,
    //                remarks = Status == "Blocked" ? "Reported lost" : null,
    //                createdDate = DateTime.Now.AddMonths(-random.Next(1, 3))

    //            });
    //        }
    //        return leaves;
    //    }


          

    //    public async Task<ChequeBook> GetChequeBookByIdAsync(string chequeBookId)
    //    {
    //        await Task.Delay(50);
    //        return _mockChequeBooks.FirstOrDefault(cb => cb.Id == chequeBookId);
    //    }

    //    public async Task<ExecutionMessages> CancelChequeBookAsync(string chequeBookId, string cancellationReason)
    //    {
    //        await Task.Delay(100);

    //        var chequeBook = _mockChequeBooks.FirstOrDefault(cb => cb.Id == chequeBookId);
    //        if (chequeBook != null)
    //        {
    //            chequeBook.Status = "Cancelled";
    //            foreach (var leaf in chequeBook.Leaves)
    //            {
    //                leaf.Status = "Cancelled";
    //            }

    //            GetExecutionMessages(null, true, "Cheque Book", MessagesResults.Success,
    //                ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(),
    //                null, $"Cheque book {chequeBookId} cancelled successfully. Reason: {cancellationReason}");
    //        }
    //        else
    //        {
    //            GetExecutionMessages(null, false, "Cheque Book", MessagesResults.Failed,
    //                ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(),
    //                null, "Cheque book not found");
    //        }
    //        return ExecutionMessage;
    //    }

    //    public async Task<ExecutionMessages> MarkLeafAsUsedAsync(string leafId, string statement)
    //    {
    //        await Task.Delay(100);

    //        var leaf = _mockChequeBooks.SelectMany(cb => cb.Leaves).FirstOrDefault(l => l.Id == leafId);
    //        if (leaf != null)
    //        {
    //            leaf.Status = "Used";
    //            leaf.UsedDate = DateTime.Now;
    //            leaf.Remarks = statement;

    //            GetExecutionMessages(null, true, "Cheque Leaf", MessagesResults.Success,
    //                ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(),
    //                null, $"Leaf {leaf.ChequeNumber} marked as used. Statement: {statement}");
    //        }
    //        else
    //        {
    //            GetExecutionMessages(null, false, "Cheque Leaf", MessagesResults.Failed,
    //                ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(),
    //                null, "Leaf not found");
    //        }
    //        return ExecutionMessage;
    //    }

    //    public async Task<ExecutionMessages> BlockLeafAsync(string leafId, string blockReason)
    //    {
    //        await Task.Delay(100);

    //        var leaf = _mockChequeBooks.SelectMany(cb => cb.Leaves).FirstOrDefault(l => l.Id == leafId);
    //        if (leaf != null)
    //        {
    //            leaf.Status = "Blocked";
    //            leaf.Remarks = blockReason;

    //            GetExecutionMessages(null, true, "Cheque Leaf", MessagesResults.Success,
    //                ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Success.ToString(),
    //                null, $"Leaf {leaf.ChequeNumber} blocked. Reason: {blockReason}");
    //        }
    //        else
    //        {
    //            GetExecutionMessages(null, false, "Cheque Leaf", MessagesResults.Failed,
    //                ExecutionProcessOption.UpdateUpject, SystemMessageStatus.Failed.ToString(),
    //                null, "Leaf not found");
    //        }
    //        return ExecutionMessage;
    //    }
    //}
}
