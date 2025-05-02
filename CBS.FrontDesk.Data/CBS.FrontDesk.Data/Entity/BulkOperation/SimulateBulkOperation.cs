using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.BulkOPerations
{
    public  class BulkOperationRangeBetweenAccountSimulationCommand
    {
        [Required(ErrorMessage = "Source Account Type is required.")]
        public string SourceAccountType { get; set; }

        [Required(ErrorMessage = "Start Account is required.")]
        public string StartAccount { get; set; }

        [Required(ErrorMessage = "End Account is required.")]
        public string EndAccount { get; set; }

        [Required(ErrorMessage = "Destination Account Type is required.")]
        public string DestinationAccountType { get; set; }

        [Required(ErrorMessage = "Transfer amount is required.")]
        [Range(1, double.MaxValue, ErrorMessage = "Transfer Amount must be greater than zero.")]
        public decimal TargetAmount { get; set; }

        [Required(ErrorMessage = "Expected Source Account Minimum Balance is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Expected Source Account Minimum Balance must be greater than or equal zero.")]
        public decimal SourceAccountMinAmount { get; set; }

        [Required(ErrorMessage = "Expected Source Maximum Account Balance is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Expected Source Maximum Acc Balance must be greater than or equal zero.")]
        public decimal SourceAccountMaxAmount { get; set; }

        public string Description { get; set; }

        public string SimulationType { get; set; }
    }
}
