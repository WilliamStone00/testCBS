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

        public string SubdivisionId { get; set; }
        public string SubdivisionName { get; set; }

        public string DivisionId { get; set; }
        public string DivisionName { get; set; }

        public string RegionId { get; set; }
        public string RegionName { get; set; }

        public string CountryId { get; set; }
        public string CountryName { get; set; }
    }

}
