using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.DailyCollectionData 
{
    public class Zone
    {
        public string Id { get; set; }
        public string Name { get; set; }
          public string Description { get; set; }
        public string BranchId { get; set; }
        public string RegionId { get; set; }
        public string DivisionId { get; set; }
        public string SubDivisionId { get; set; }
        public string TownId { get; set; }

    }

    public class ZoneDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
 
        public string BranchId { get; set; }
        public string RegionId { get; set; }
        public string Region { get; set; }
        public string DivisionId { get; set; }
        public string Division { get; set; }
        public string SubDivisionId { get; set; }
        public string SubDivision  { get; set; }
        public string TownId { get; set; }
        public string Town  { get; set; }
    }
}
