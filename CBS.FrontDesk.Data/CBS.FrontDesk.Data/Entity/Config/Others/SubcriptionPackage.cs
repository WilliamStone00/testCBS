using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Config
{
    public class SubcriptionPackage
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double MinimumServiceFee { get; set; }
        public double MaximumServiceFee { get; set; }
        public bool Status { get; set; }
    }

}
