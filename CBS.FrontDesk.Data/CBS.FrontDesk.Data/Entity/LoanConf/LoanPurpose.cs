using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanConf
{
    public class LoanPurpose
    {
        public string id { get; set; }
        [Required]
        public string purposeName { get; set; }
    }
}
