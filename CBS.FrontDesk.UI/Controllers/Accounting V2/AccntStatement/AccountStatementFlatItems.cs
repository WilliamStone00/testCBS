using System;
using System.ComponentModel.DataAnnotations;

namespace CBS.FrontDesk.UI.Controllers.Accounting_V2.AccntStatement {
    public class AccountStatementFlatItems
    {
        // ---------- Parent (Header) Fields ----------
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal ClosingBalance { get; set; }
        public string BranchTel { get; set; }
        public string BranchEmail { get; set; }
        public string LogoUrl { get; set; }
        public string PBox { get; set; }
        public string DisplayName { get; set; }
        public string ImmatriculationNumber { get; set; }
        public string RegistrationNumber { get; set; }
        public string BankInitial { get; set; }
        public string Motto { get; set; }
        public string MottoDisplayName { get; set; }
        public string HeadOfficeAddress { get; set; }
        public string Towns { get; set; }
        public string HeadOfficePhone { get; set; }
        public string InitByName { get; set; }
     

        // ---------- Movement Fields ----------
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; } = DateTime.Now.Date;
        public DateTime DayTime { get; set; } = DateTime.Now.Date;

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]

        public DateTime From { get; set; }


        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime To { get; set; } = DateTime.Now.Date;

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]

        public DateTime DateFrom { get; set; }


        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime DateTo { get; set; } = DateTime.Now.Date;

        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:hh\\:mm\\:ss}", ApplyFormatInEditMode = true)]

        public TimeSpan Time { get; set; } = DateTime.Now.TimeOfDay;

        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:hh\\:mm\\:ss}", ApplyFormatInEditMode = true)]

        public TimeSpan Entime { get; set; } = DateTime.Now.TimeOfDay;
        public string ReferenceNumber { get; set; }
        public string BranchId { get; set; }

        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public string Description { get; set; }
        public string DrCr { get; set; }
        public string AccountingDate { get; set; }
        public decimal Amount { get; set; }

        public decimal Balance { get; set; }
        public int Seq { get; set; }
        public string AuxiliaryRef { get; set; }
        public DateTime EntryDate { get; set; }
        public string UserName { get; set; }
        public string PrintedBy { get; set; }
        public string Currency { get; set; }
        
        public string Year { get; set; }
       

        public bool InterbranchStatus { get; set; }
        public string CounterpartyBranchId { get; set; }
        public string TimeOfOperation { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string Username { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public TimeSpan time{ get; set; }
    }


}
