using System;
using System.ComponentModel.DataAnnotations;
namespace CBS.FrontDesk.UI.Controllers.Accounting_V2.TrialBalance { 

    public class TrialBalanceFourColumnsFlatItens
    {
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }

        public decimal OpeningDebit { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal BeginningBalance { get; set; }
        public string BeginningBookingDirection { get; set; }

        public decimal EndingBalance { get; set; }
        public string EndingBookingDirection { get; set; }


        public string Logo { get; set; } = string.Empty;
        public string Mode { get; set; } = string.Empty;

        public DateTime DayTime { get; set; }

        public string Address { get; set; }
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; } = DateTime.Now.Date;

        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime From { get; set; } = DateTime.Now.Date;


        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime To { get; set; } = DateTime.Now.Date;

        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:hh\\:mm\\:ss}", ApplyFormatInEditMode = true)]
        public TimeSpan Time { get; set; } = DateTime.Now.TimeOfDay;


        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string Username { get; set; }
        public string Phone { get; set; }
        public string Year { get; set; } = DateTime.Now.Year.ToString();


    }

}