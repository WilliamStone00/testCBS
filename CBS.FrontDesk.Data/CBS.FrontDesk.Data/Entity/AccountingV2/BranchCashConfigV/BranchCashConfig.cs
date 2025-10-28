using CBS.FrontDesk.Data.Entity.AccountingV2.LiaisonMapping;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.AccountingV2.BranchCashConfigV
{
    public class BranchCashConfig
    {
        public string Id { get; set; }
       public string BranchId { get; set; }
        public string CashInHandAccountId { get; set; } // teller till
        public string VaultAccountId { get; set; }
        public string SurplusIncomeAccountId { get; set; } // overage
        public string RevenueAccountId { get; set; }
        public string ShortageExpenseAccountId { get; set; } // shortage
        public bool RealTimeCashPosting { get; set; } = true; // cash ops impacted in real time
        public string PartnerAccountId { get; set; }
        public string CamcculAccountId { get; set; }
        public string HeadOfficeLiaisonAccountId { get; set; }
        public string FormFeeIncomeAccountId { get; set; }

    }

    public sealed class BranchCashConfigRowDto
    {
        public string Id { get; set; }
        public string BranchId { get; set; }
        public string BranchName { get; set; }

        public string CashInHandAccountName { get; set; }
        public string CashInHandAccountNumber { get; set; }

        public string VaultAccountName { get; set; }
        public string VaultAccountNumber { get; set; }

        public string SurplusIncomeAccountName { get; set; }
        public string SurplusIncomeAccountNumber { get; set; }

        public string ShortageExpenseAccountName { get; set; }
        public string ShortageExpenseAccountNumber { get; set; }

        public string RevenueAccountName { get; set; }
        public string RevenueAccountNumber { get; set; }

        public string PartnerAccountName { get; set; }
        public string PartnerAccountNumber { get; set; }

        public string CamcculAccountName { get; set; }
        public string CamcculAccountNumber { get; set; }

        public string HeadOfficeLiaisonAccountName { get; set; }
        public string HeadOfficeLiaisonAccountNumber { get; set; }

        public string FormFeeIncomeAccountName { get; set; }
        public string FormFeeIncomeAccountNumber { get; set; }

        public bool RealTimeCashPosting { get; set; }
    }

    // DTO used only by the Details partial
    public sealed class BranchCashConfigDetailsVm
    {
        public string Id { get; set; }
        public string Branch { get; set; }
        public bool RealTimeCashPosting { get; set; }

        public AccountPair CashInHand { get; set; }
        public AccountPair Vault { get; set; }
        public AccountPair SurplusIncome { get; set; }
        public AccountPair ShortageExpense { get; set; }
        public AccountPair Revenue { get; set; }
        public AccountPair Partner { get; set; }
        public AccountPair Camccul { get; set; }
        public AccountPair HeadOfficeLiaison { get; set; }
        public AccountPair FormFeeIncome { get; set; }
    }

    public sealed class AccountPair
    {
        public string Name { get; set; }
        public string Number { get; set; }
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