using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.AccountingV2.BranchCashConfig
{
    public class BranchCashConfigDto
    {
        public string Id { get; set; }
        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public string CashInHandAccountId { get; set; }
        public string CashInHandAccountName { get; set; }
        public string VaultAccountId { get; set; }
        public string VaultAccountName { get; set; }
        public string SurplusIncomeAccountId { get; set; }
        public string SurplusIncomeAccountName { get; set; }
        public string RevenueAccountId { get; set; }
        public string RevenueAccountName { get; set; }
        public string ShortageExpenseAccountId { get; set; }
        public string ShortageExpenseAccountName { get; set; }
        public bool RealTimeCashPosting { get; set; }
        public string SourceBranchAccountId { get; set; }
        public string SourceBranchAccountName { get; set; }
        public string DestinationBranchAccountId { get; set; }
        public string DestinationBranchAccountName { get; set; }
        public string HeadOfficeAccountId { get; set; }
        public string PartnerAccountId { get; set; }
        public string CamcculAccountId { get; set; }
        public string HeadOfficeLiaisonAccountId { get; set; }
        public string FormFeeIncomeAccountId { get; set; }
        public string RequestedBy { get; set; }

    }

    public class BranchCashConfigQueryDto
    {
        public DataTableOptions Options { get; set; }
        
        // Primary Identifier for direct lookup
        public string Id { get; set; }

        // Key filterable dimension
        public string BranchId { get; set; }

        // Status/Type filter
        public bool? RealTimeCashPosting { get; set; }

        // Account filters
        public string CashInHandAccountId { get; set; }
        public string VaultAccountId { get; set; }
        public string SurplusIncomeAccountId { get; set; }
        public string ShortageExpenseAccountId { get; set; }
        public string RevenueAccountId { get; set; }
        public string FormFeeIncomeAccountId { get; set; }

        public BranchCashConfigQueryDto()
        {
            Options = new DataTableOptions();
        }
    }
}