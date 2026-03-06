using System;
namespace CBS.FrontDesk.Data.Entity.Accounting_V2.Reporting.FlatBaseE
{


   public  class BalanceSheetFlatItems : BankHeaderInformation
    {
        public string GroupName { get; set; }
        public string GroupReference { get; set; }


        public string Reference { get; set; }
        public string Title { get; set; }
        public string Referernce { get; set; }
        public string Heading { get; set; }
        public string Side { get; set; }
        public string IsSubtotal { get; set; }
        public string Note { get; set; }
      
        public decimal Gross { get; set; }
        public decimal AmountProv { get; set; }
        public decimal NetN { get; set; }
        public decimal NetN1 { get; set; }
        public decimal TotalAmountGrossAsts { get; set; }
        public DateTime AccountingDate { get; set; }
        public decimal TotalAmountProvAsts { get; set; }
        public decimal TotalNetLastYearAsts { get; set; }
        public int CurrentYear { get; set; }
        public string Year { get; set; }
        public int PreviousYear { get; set; }
        public decimal TotalNetPresentAsts { get; set; }
        public decimal TotalLiabEquityNetLastYear { get; set; }
        public string PrintedBy { get; set; }
      


        //income statement 

        public decimal TotalExpensesCurrentYear { get; set; }
        public decimal TotalExpensesLastYear { get; set; }
        public decimal TotalIncomeLastYear { get; set; }
        public decimal TotalIncomeCurrentYear { get; set; }
    }
}