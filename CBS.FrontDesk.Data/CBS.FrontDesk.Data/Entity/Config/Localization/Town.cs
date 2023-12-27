using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Config
{
    public class Town
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string SubdivisionId { get; set; } // Foreign key
        public SubDivision Subdivision { get; set; }
    }

}
