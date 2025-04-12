using CBS.FrontDesk.Data.Entity.SavingProducts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.AccountOpeningRulePerBranchP
{
    using System.ComponentModel.DataAnnotations;

    public class AccountOpeningRulePerBranch
    {
       
        public string Id { get; set; }

        [Required(ErrorMessage = "Branch ID is required.")]
        public string BranchId { get; set; }

        public string BranchName { get; set; }

        public string BranchCode { get; set; }

        [Required(ErrorMessage = "Saving Product ID is required.")]
        public string SavingProductId { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Minimum opening balance must be a non-negative number.")]
        public decimal MinimumOpeningBalance { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Maximum opening balance must be a non-negative number.")]
        public decimal MaximumOpeningBalance { get; set; }

        [Required]
        public bool RequiredOpeningBalance { get; set; }

        [Required]
        public bool CanPayInInstallments { get; set; }

        // Navigation property (optional in validation context)
        public SavingProduct SavingProduct { get; set; }
    }
    public class GetAllAccountOpeningRulesQuery
    {
        public string SavingProductId { get; set; }
        public string BranchId { get; set; }
    }
}
