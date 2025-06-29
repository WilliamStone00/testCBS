using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Config
{
    public class Region
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public string CountryId { get; set; }
        public string CountryName { get; set; } // Flattened property
    }

}
