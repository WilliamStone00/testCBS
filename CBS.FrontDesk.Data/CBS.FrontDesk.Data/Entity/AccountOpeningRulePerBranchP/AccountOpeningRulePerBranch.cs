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

        [Required(ErrorMessage = "Branch is required.")]
        public string BranchId { get; set; }
        [Required(ErrorMessage = "Legal form is required.")]
        public string LegalForm { get; set; }
        public string BranchName { get; set; }

        public string BranchCode { get; set; }

        [Required(ErrorMessage = "Saving Product ID is required.")]
        public string SavingProductId { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Minimum opening balance must be a non-negative number.")]
        public decimal MinimumOpeningBalance { get; set; }
        public string ProductName { get; set; }
        public decimal MinimumOpeningBalanceDailySaverShare { get; set; }
        public bool IsDailySaver { get; set; }

        public SavingProduct SavingProduct { get; set; }
    }
    public class GetAllAccountOpeningRulesQuery
    {
        public string SavingProductId { get; set; }
        public string BranchId { get; set; }
    }
}
