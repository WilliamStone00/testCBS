using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class PostedEntry
    {

        public string Id { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string IssuedBy { get; set; }
        public string PostingSource { get; set; }
        public string BranchCode { get; set; }
        public List<EntryTempData> EntryDetail { get; set; }
        public string ApprovedDate { get; set; }
        public string ApprovedBy { get; set; }

    }

    public class PostedEntryX
    {
        public string PostingSource { get; set; }
        public string Id { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string IssuedBy { get; set; }
        public string BranchCode { get; set; }
        public string ApprovedDate { get; set; }
        public string ApprovedBy { get; set; }
        public List<EntryTempData> EntryDetail { get; set; } = new List<EntryTempData>();

        public PostedEntryX()
        {

        }
        public PostedEntry ConvertToPostedEntry(PostedEntryX model)
        {
            PostedEntry postedEntry = new PostedEntry();
            postedEntry.Id = model.Id;
            postedEntry.Status = model.Status;
            postedEntry.Description = model.Description;
            postedEntry.Amount = model.Amount;
            postedEntry.CreatedDate = model.CreatedDate;
            postedEntry.CreatedBy = model.CreatedBy;
            postedEntry.IssuedBy = model.IssuedBy;
            postedEntry.PostingSource = model.PostingSource;
            postedEntry.BranchCode = model.BranchCode;
            postedEntry.EntryDetail = model.EntryDetail;
            postedEntry.ApprovedDate = model.ApprovedDate;
            postedEntry.ApprovedBy = model.ApprovedBy;
            return postedEntry;

        }
    }
}
