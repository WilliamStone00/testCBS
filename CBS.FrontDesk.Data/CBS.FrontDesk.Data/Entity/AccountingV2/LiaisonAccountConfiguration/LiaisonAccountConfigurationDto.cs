using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.AccountingV2.LiaisonAccountConfiguration
{
    public class LiaisonAccountConfigurationDto
    {
        public string BranchId { get; set; } 
        
        public string BranchName { get; set; }
        public string CashInHandAccountId { get; set; }
        public string CashInHandAccountName { get; set; }

        public string VaultAccountId { get; set; }
        public string VaultAccountName { get; set; }

        public string SurplusIncomeAccountId { get; set; }
        public string SurplusIncomeAccountName { get; set; }

        public string ShortageExpenseAccountId { get; set; }
        public string ShortageExpenseAccountName { get; set; }

        public bool RealTimeCashPosting { get; set; }

     
        public string RevenueAccountId { get; set; }
        public string RevenueAccountName { get; set; } // Retained this property as it might be useful

        public string SourceBranchAccountId { get; set; }
        public string SourceBranchAccountName { get; set; }

        public string DestinationBranchAccountId { get; set; }
        public string DestinationBranchAccountName { get; set; }

        public string HeadOfficeAccountId { get; set; }
        public string PartnerAccountId { get; set; }
        public string CamcculAccountId { get; set; }
        public string HeadOfficeLiaisonAccountId { get; set; }
        public string FormFeeIncomeAccountId { get; set; }
        public bool Status { get; set; }
        public string Id { get; set; }
    }
}
