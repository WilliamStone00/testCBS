using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class CashDemandDataEntity
    {
        //
        public CashInfusion CashInfusionModel { get; set; }
        public DetailsDto DetailsDtoModel { get; set; }
        public CashReplenimentRequestDto CashReplenimentRequestdto { get; set; }
        public CashReplenimentRequest CashReplenimentRequest { get; set; }
        public List<CashReplenimentRequest> ListCashReplenimentRequest { get; set; }
        public List<CashReplenimentRequestDto> ListCashReplenimentRequestDto { get; set; }
        public List<DetailsDto> DetailsDtos { get; set; }
        public string ServiceOption { get; set; }
        
             public string Action { get; set; }
    }

     
}
