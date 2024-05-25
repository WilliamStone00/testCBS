using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanConf
{
    public class Fee
    {
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]

        public string FeeBase { get; set; }//Percentage Or Range
        public string AccountingEventCode { get; set; }
        public List<FeeRange> FeeRanges { get; set; }
    }
}
