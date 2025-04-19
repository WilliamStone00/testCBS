using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.UserManagement;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        public string PostingSource { get; set; }

        public string CreatedBy { get; set; }
        public string CreatedDate { get; set; }
        public string BranchId { get; set; }
 
        public string ApprovedBy { get; set; }
        public string ApprovedDate { get; set; }
        public string ValidationMessage { get; set; }
        public List<EntryTempDatas> EntryDetail { get; set; }
        public string BranchCode { get;   set; }

        public PostedEntryX ConvertToPostedEntry(PostedEntry model)
        {
            PostedEntryX postedEntry = new PostedEntryX();
            postedEntry.Id = model.Id;
            postedEntry.Status = model.Status;
            postedEntry.Description = model.Description;
            postedEntry.Amount = model.Amount;
            postedEntry.CreatedDate = DateTime.ParseExact(model.CreatedDate, "dd-MMM-yy h:mm:ss tt", CultureInfo.InvariantCulture);
            postedEntry.ValidationMessage = model.ValidationMessage;               
            postedEntry.CreatedBy = model.CreatedBy;
            postedEntry.IssuedBy = model.CreatedBy;
            postedEntry.PostingSource = model.PostingSource;
            postedEntry.BranchCode = model.BranchCode;
            postedEntry.EntryDetail = model.EntryDetail;
            
        postedEntry.ApprovedDate = DateTime.ParseExact(model.ApprovedDate, "dd-MMM-yy h:mm:ss tt", CultureInfo.InvariantCulture);

            postedEntry.EndorseBy = model.ApprovedBy;
            return postedEntry;

        }
    }
    public class ManualEntry
    {
        public string PostingSource { get; set; }
        public string Status { get; set; }
        public string Address { get; set; }
        public DateTime AccountingDate { get; set; }
        public string AccountName { get; set; }
        public string AccountNumber { get; set; }
        public decimal DebitBalance { get; set; }
        public decimal CreditBalance { get; set; }
        public string BranchName { get; set; }
        public DateTime ToDate { get; set; }
        public string BranchAddress { get; set; }
        public string Capital { get; set; }
        public string ImmatriculationNumber { get; set; }
        public string WebSite { get; set; }
        public string BranchTelephone { get; set; }
        public string HeadOfficeTelePhone { get; set; }
        public string LogoPath { get; set; }
        public string Description { get; set; }
        public string IssuedBy { get; set; }
        public string Reference { get; set; }
        public string BranchCode { get; set; }
        public string PrintersName { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime ApprovedDate { get; set; }
        public DateTime ValueDate { get; set; }
        public DateTime IssuedDate { get; set; }
        public decimal AccountBalance { get; set; }
        public string ApproveMessage { get; set; }
        public string Name { get; set; }
    }
    public class PostedEntryX
    {
        public string PostingSource { get; set; }
        public string Id { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string IssuedBy { get; set; }
        public string BranchCode { get; set; }
        public DateTime ApprovedDate { get; set; }
        public string ApprovedBy { get; set; }
        public string EndorseBy { get; set; }
        public string ValidationMessage { get; set; }
        
        public List<EntryTempDatas> EntryDetail { get; set; } = new List<EntryTempDatas>();

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
            postedEntry.CreatedDate = model.ApprovedDate.ToLongDateString();
            postedEntry.CreatedBy = model.CreatedBy;
            postedEntry.CreatedBy = model.CreatedBy;
            postedEntry.PostingSource = model.PostingSource;
            postedEntry.BranchCode = model.BranchCode;
            postedEntry.EntryDetail = model.EntryDetail;
            postedEntry.ApprovedDate = model.ApprovedDate.ToLongDateString();
            postedEntry.ApprovedBy = model.ApprovedBy;
            return postedEntry;

        }
        public static List<ManualEntry> ConvertToManualEntry(PostedEntryX model, Branch branch, User Isseur, User Approver)
        {
           List< ManualEntry> postedEntries = new List<ManualEntry>();
            foreach (var item in model.EntryDetail)
            {
               var postedEntry = new  ManualEntry ();
                postedEntry.AccountName = item.AccountName;
                postedEntry.AccountNumber = item.AccountNumber;
                postedEntry.DebitBalance = (item.BookingDirection=="DEBIT") ?Convert.ToDecimal( item.Amount):0;
                postedEntry.CreditBalance = (item.BookingDirection == "CREDIT") ? Convert.ToDecimal(item.Amount) : 0;
                postedEntry.AccountBalance = Convert.ToDecimal(item.AccountBalance);
                postedEntry.Reference = model.Id;
                postedEntry.Status = model.Status;
                postedEntry.Description = model.Description;
                postedEntry.IssuedBy = Isseur.firstName +" "+ Isseur.lastName;
                postedEntry.PostingSource = model.PostingSource;
                postedEntry.BranchCode = model.BranchCode;
                postedEntry.BranchName = branch.Name;
                postedEntry.ApprovedBy = Approver.firstName + " " + Approver.lastName;// .FullName;
                postedEntry.ApprovedDate = model.ApprovedDate;
                postedEntry.ApproveMessage = model.ValidationMessage;
                //postedEntry.CreatedBy = model.CreatedBy;
                postedEntry.ValueDate = model.CreatedDate;

                postedEntry.IssuedDate = model.CreatedDate;
                postedEntry.Capital = "";
                postedEntry.ImmatriculationNumber = branch.ImmatriculationNumber;
                postedEntry.WebSite = branch.WebSite;
                postedEntry.Name = branch.Bank.Name;
                postedEntry.BranchAddress = branch.Address;
                postedEntry.BranchTelephone = branch.Telephone;
                postedEntry.HeadOfficeTelePhone = branch.Bank.Telephone;
                postedEntry.LogoPath = branch.Bank.LogoUrl;
                postedEntries.Add(postedEntry);
            }
           
          
            return postedEntries;

        }
    }
}
