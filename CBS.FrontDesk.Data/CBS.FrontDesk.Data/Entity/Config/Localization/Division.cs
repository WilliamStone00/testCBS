using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Config
{
    public class Division
    {
        public string Id { get; set; }
        public string Name { get; set; }

        // Flattened fields
        public string RegionId { get; set; }
        public string RegionName { get; set; }

        public string CountryId { get; set; }
        public string CountryName { get; set; }
    }

}
