using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class CashDemandDataEntity
    {
        public List<UsersNotification> UsersNotifications { get; set; } = new List<UsersNotification>();
        public CashInfusion CashInfusionModel { get; set; } = new CashInfusion();
        public CashInfusionRequest CashInfusionRequest { get; set; } = new CashInfusionRequest();
        public Approval Approval { get; set; } = new Approval();
        public BankCashOut BankCashOut { get; set; } = new BankCashOut();
        public CashClearing CashClearing { get; set; } = new CashClearing();
        public BranchToBranchTransfer BranchToBranchTransfer { get; set; } = new BranchToBranchTransfer();
        public DetailsDto DetailsDto { get; set; } = new DetailsDto();
        public UploadBankReciept UploadBankReciept { get; set; } = new UploadBankReciept();
        public DepositNotificationApproval DepositApproval { get; set; } = new DepositNotificationApproval();
        public DepositNotification DepositNotification { get; set; } = new DepositNotification();
        public DepositNotificationDto DepositNotificationDto { get; set; } = new DepositNotificationDto();
        public List<DepositNotificationDto> ListDepositNotificationDto { get; set; } = new List<DepositNotificationDto>();
        public CashReplenimentRequestDto CashReplenimentRequestdto { get; set; } = new CashReplenimentRequestDto();
        public CashReplenimentRequestCompleteDto CashReplenimentRequestCompleteDto { get; set; } = new CashReplenimentRequestCompleteDto();
        public CashReplenimentRequest CashReplenimentRequest { get; set; } = new CashReplenimentRequest();
        public List<CashReplenimentRequest> ListCashReplenimentRequest { get; set; } = new List<CashReplenimentRequest>();
        public List<CashReplenimentRequestDto> ListCashReplenimentRequestDto { get; set; } = new List<CashReplenimentRequestDto>();
        public List<DetailsDto> DetailsDtos { get; set; } = new List<DetailsDto>();
        public string ServiceOption { get; set; }

        public string Action { get; set; }
    }


    public class EventCodeQuery 
    {
        /// <summary>
        /// Gets or sets the unique identifier of the AccountCategory to be retrieved.
        /// </summary>
        public string EventCode { get; set; }
        public string ToBranchCode { get; set; }
        public string ToBranchId { get; set; }
    }
    public class UploadBankRecieptDto
    {
        public string id { get; set; }
        public string bankTransactionReference { get; set; }
        public string filePath { get; set; }
        public string comment { get; set; }
        public DateTime ValueDate { get; internal set; }
    }
}
