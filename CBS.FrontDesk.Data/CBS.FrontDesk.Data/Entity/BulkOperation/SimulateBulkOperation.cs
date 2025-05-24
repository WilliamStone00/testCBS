using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.BulkOperation
{
    public  class SimulateBulkOperation
    {
        //[Required(ErrorMessage = "Source Account Type is required.")]
        public string SourceAccountType { get; set; }

       // [Required(ErrorMessage = "Start Account is required.")]
        public string StartAccount { get; set; }

       //[Required(ErrorMessage = "End Account is required.")]
        public string EndAccount { get; set; }

       // [Required(ErrorMessage = "Destination Account Type is required.")]
        public string DestinationAccountType { get; set; }

        public string DestinationAccountId { get; set; }

        /*
                [Required(ErrorMessage = "Transfer amount is required.")]
                [Range(1, double.MaxValue, ErrorMessage = "Transfer Amount must be greater than zero.")]*/
        public decimal TargetAmount { get; set; }
        public decimal ContributionAmount { get; set; }

     /*   [Required(ErrorMessage = "Expected Source Account Minimum Balance is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Expected Source Account Minimum Balance must be greater than or equal zero.")]*/
        public decimal SourceAccountMinAmount { get; set; }

/*        [Required(ErrorMessage = "Expected Source Maximum Account Balance is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Expected Source Maximum Acc Balance must be greater than or equal zero.")]*/
        public decimal SourceAccountMaxAmount { get; set; }

        public string Description { get; set; }

        public string IsContribution { get; set; }

        public string SimulationType { get; set; }
        public string BranchId { get; set; }

      /*  public string BankCode { get; set; }
        public string BranchCode { get; set; }
        public string BranchId { get; set; }
        public string BankId { get; set; }
        public string BankName { get; set; }
        public string BranchName { get; set; }*/
        public List<SavingProduct> SavingProducts { get; set; } = new List<SavingProduct>();
        public List<Branch> Branches { get; set; } = new List<Branch>();
        public BulkOperationSelectionModel BulkOperationSelectionModel { get; set; }


    }

    public class BulkOperationSelectionModel
    {
        public string SelectedOperationType { get; set; } = "specific"; // Default to first option
    }
}
