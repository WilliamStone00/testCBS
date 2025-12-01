using System;
namespace CBS.FrontDesk.Data.Entity.Accounting_V2.Reporting.FlatBaseE
{


   public  class BalanceSheetFlatItems : BankHeaderInformation
    {
        public string GroupName { get; set; }
        public string GroupReference { get; set; }


        public string Reference { get; set; }
        public string Referernce { get; set; }
        public string Heading { get; set; }
        public decimal Gross { get; set; }
        public decimal AmountProv { get; set; }
        public decimal NetN { get; set; }
        public decimal NetN1 { get; set; }
        public decimal TotalAssetsNetCurrentYear { get; set; }
        public decimal TotalAssetsNetLastYear { get; set; }
        public decimal TotalLiabEquityNetCurrentYear { get; set; }
        public decimal TotalLiabEquityNetLastYear { get; set; }
        public string PrintedBy { get; set; }
    }
}