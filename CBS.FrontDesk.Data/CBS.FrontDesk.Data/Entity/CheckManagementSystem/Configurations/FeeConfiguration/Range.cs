using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CheckManagementSystem.Configurations.FeeConfiguration
{
    public class Range
    {
        public string id { get; set; }
        public string name { get; set; } 
        public decimal amountFrom { get; set; }
        public decimal amountTo { get; set; }
        public decimal value { get; set; }
    }
}
