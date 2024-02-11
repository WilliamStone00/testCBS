using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.ReportDataSetDto
{
    public class TransactionReportDS
    {
        public decimal Amount { get; set; }
        public decimal OriginalDepositAmount { get; set; }
        public string CustomerName { get; set; }
        public string AccountNumber { get; set; }
        public string CustomerNumber { get; set; }
        public string TransactionType { get; set; }
        public string OperationType { get; set; }
        public string TransactionRef { get; set; }
        public string AccountType { get; set; }
        public decimal Tax { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal PreviousBalance { get; set; }
        public decimal CurrentBalance { get; set; }
        public string Note { get; set; }
        public decimal Fee { get; set; }
        public string FeeType { get; set; }
        public int Coin1 { get; set; }
        public int Coin5 { get; set; }
        public int Coin10 { get; set; }
        public int Coin25 { get; set; }
        public int Coin50 { get; set; }
        public int Coin100 { get; set; }
        public int Coin500 { get; set; }
        public int Note500 { get; set; }
        public int Note1000 { get; set; }
        public int Note2000 { get; set; }
        public int Note5000 { get; set; }
        public int Note10000 { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        public string BName { get; set; }
        public string BLocation { get; set; }
        public string BTelephone { get; set; }
        public string BEmail { get; set; }
        public string BAddress { get; set; }
        public string BCapital { get; set; }
        public string BRegistrationNumber { get; set; }
        public string BLogoUrl { get; set; }
        public string BImmatriculationNumber { get; set; }
        public string BTaxPayerNUmber { get; set; }
        public string BPBox { get; set; }
        public string BWebSite { get; set; }
        public string BBankInitial { get; set; }
        public string BMotto { get; set; }
        public string BHeadOfficeTelehoneNumber { get; set; }
        public string BHeadOfficeAddress { get; set; }
        public string TellerName { get; set; }
        public string CashierName { get; set; }
    }
}
