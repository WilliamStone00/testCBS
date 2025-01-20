using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Entity.VaultManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CashChangeManagement
{
    public class CashChangeHistory
    {
        public string Id { get; set; }
        public string Reference { get; set; }
        public DateTime ChangeDate { get; set; }
        public decimal AmountGiven { get; set; }
        public decimal AmountReceive { get; set; }
        // Denominations Given
        public int GivenNote10000 { get; set; }
        public int GivenNote5000 { get; set; }
        public int GivenNote2000 { get; set; }
        public int GivenNote1000 { get; set; }
        public int GivenNote500 { get; set; }
        public int GivenCoin500 { get; set; }
        public int GivenCoin100 { get; set; }
        public int GivenCoin50 { get; set; }
        public int GivenCoin25 { get; set; }
        public int GivenCoin10 { get; set; }
        public int GivenCoin5 { get; set; }
        public int GivenCoin1 { get; set; }

        // Denominations Received
        public int ReceivedNote10000 { get; set; }
        public int ReceivedNote5000 { get; set; }
        public int ReceivedNote2000 { get; set; }
        public int ReceivedNote1000 { get; set; }
        public int ReceivedNote500 { get; set; }
        public int ReceivedCoin500 { get; set; }
        public int ReceivedCoin100 { get; set; }
        public int ReceivedCoin50 { get; set; }
        public int ReceivedCoin25 { get; set; }
        public int ReceivedCoin10 { get; set; }
        public int ReceivedCoin5 { get; set; }
        public int ReceivedCoin1 { get; set; }
        public string ServiceOperationType { get; set; }
        public string BranchId { get; set; }
        public string BranchCode { get; set; }
        public string BranchName { get; set; }
        public string ChangedBy { get; set; }
        public string ChangeReason { get; set; }
        public string VaultId { get; set; }
        public string SubTellerId { get; set; }
        public string PrimaryTellerId { get; set; }
        public SubTellerProvissioning SubTellerProvissioning { get; set; }
        public Vault Vault { get; set; }
        public PrimaryTellerProvissioning PrimaryTellerProvissioning { get; set; }
        public CashChangeCommand CashChangeCommand { get; set; }
        public GetCashChangeHistoryQuery GetCashChangeHistoryQuery { get; set; }
        public string Action { get; set; }
        public CashChangeHistory()
        {
            SubTellerProvissioning=new SubTellerProvissioning();
            Vault=new Vault();
            PrimaryTellerProvissioning=new PrimaryTellerProvissioning();
            CashChangeCommand=new CashChangeCommand();
            GetCashChangeHistoryQuery=new GetCashChangeHistoryQuery();
        }
    }
    public class CashChangeCommand
    {
        public CurrencyNotesRequest DenominationsGiven { get; set; }
        public CurrencyNotesRequest DenominationsReceived { get; set; }
        public string ChangeReason { get; set; }
        public decimal CashReceived { get; set; }
        public decimal CashEntered { get; set; }
        public CashChangeCommand()
        {
            DenominationsGiven=new CurrencyNotesRequest();
            DenominationsReceived=new CurrencyNotesRequest();
        }
    }
    public class GetCashChangeHistoryQuery
    {
        public string VaultId { get; set; }         // Filter by Vault ID
        public string SubTellerId { get; set; }    // Filter by Sub Teller ID
        public string PrimaryTellerId { get; set; } // Filter by Primary Teller ID
        public string BranchId { get; set; }       // Filter by Branch ID
        public DateTime? StartDate { get; set; }   // Filter by start date
        public DateTime? EndDate { get; set; }     // Filter by end date
        public string UserId { get; set; }         // Filter by User ID
    }
}
