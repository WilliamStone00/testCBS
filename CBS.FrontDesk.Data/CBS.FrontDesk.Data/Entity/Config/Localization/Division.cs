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
        [Required]
        public string Name { get; set; }
        [Required]
        public string RegionId { get; set; } // Foreign key
        public Region Region { get; set; }
        public List<SubDivision> Subdivisions { get; set; }
    }

}
