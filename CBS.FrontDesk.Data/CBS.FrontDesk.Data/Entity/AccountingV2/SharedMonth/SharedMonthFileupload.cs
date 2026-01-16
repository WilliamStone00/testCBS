using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CBS.FrontDesk.Data.Entity.AccountingV2.SharedMonth
{
    public class SharedMonthFileupload
    {
        public string BranchId { get; set; }
        public HttpPostedFileBase file { get; set; }
        public string  FileType { get; set; }
        public string Month { get; set; }
        //public string AccountType { get; set; }
        public string ProductId { get; set; }
        public decimal InterestRate { get; set; }
        public string AccountingYearId { get; set; }
    }
    public class SharedMonthFileReponse
    {
        public bool HasPassed { get; }
        public string Message { get; }
    }
   public sealed class MemberShareMonthUpload
   {
        // -----------------------------
        // Header (top of the template)
        // -----------------------------
        public string Id { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public DateTime? Date { get; set; }          // e.g. 27/07/2025
        public string Month { get; set; }           // e.g. July
        public string UploadedBy { get; set; }
        public DateTime UploadedAt { get; set; }
        // -----------------------------
        // Lines (table body)
        // -----------------------------
        public List<CustomerInterest> Lines { get; set; } 
        public string BranchId { get; set; }
        public string FileType { get; set; }
        public decimal TotalMember { get; set; }
        public decimal TotalAmount { get; set; }

                // e.g. July
        public string BankName { get; set; }
        // -----------------------------
        // Lines (table body)
        // -----------------------------

       
        public int? AccountingYear { get; set; }
        public string AccountingYearId { get; set; }
       

    }



    public sealed class CustomerInterest
    {

        public string MemberReference { get; set; }


        public string AccountType { get; set; }

        public string MemberName { get; set; }


        public decimal Amount { get; set; }


    }

    
    

}
