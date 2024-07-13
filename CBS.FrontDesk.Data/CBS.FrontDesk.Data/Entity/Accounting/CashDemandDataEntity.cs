using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class CashDemandDataEntity
    {
        public CashInfusion CashInfusionModel { get; set; } = new CashInfusion();
        public CashInfusionRequest CashInfusionRequest { get; set; } = new CashInfusionRequest();
        public Approval Approval { get; set; } = new Approval();
        public BankCashOut BankCashOut { get; set; } = new BankCashOut();

        public BranchToBranchTransfer BranchToBranchTransfer { get; set; } = new BranchToBranchTransfer();
        public DetailsDto DetailsDto { get; set; } = new DetailsDto();
        public CashReplenimentRequestDto CashReplenimentRequestdto { get; set; } = new CashReplenimentRequestDto();
        public CashReplenimentRequestCompleteDto CashReplenimentRequestCompleteDto { get; set; } = new CashReplenimentRequestCompleteDto();
        public CashReplenimentRequest CashReplenimentRequest { get; set; } = new CashReplenimentRequest();
        public List<CashReplenimentRequest> ListCashReplenimentRequest { get; set; } = new List<CashReplenimentRequest>();
        public List<CashReplenimentRequestDto> ListCashReplenimentRequestDto { get; set; } = new List<CashReplenimentRequestDto>();
        public List<DetailsDto> DetailsDtos { get; set; } = new List<DetailsDto>();
        public string ServiceOption { get; set; }

        public string Action { get; set; }
    }


}
