using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanConf
{
    public class FeeRange
    {
        public string Id { get; set; }
        [Required]

        public string FeeId { get; set; }
        [Required]

        public decimal AmountFrom { get; set; }
        [Required]

        public decimal AmountTo { get; set; }
        public decimal PercentageValue { get; set; }
        [Required]

        public decimal Charge { get; set; }
        public Fee Fee { get; set; }
    }
}
