using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity
{
   
    public class CustomerActivation
    {
        public string customerId { get; set; }
        public bool activate { get; set; }
    }
}
