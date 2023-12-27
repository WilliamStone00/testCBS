using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanConf
{
    public class Period
    {
        public string id { get; set; }
        [Required]
        public string name { get; set; }
        [Required]
        public string description { get; set; }
    }
}
