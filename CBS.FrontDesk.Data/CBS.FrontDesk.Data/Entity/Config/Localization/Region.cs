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
        [Required]
        public string Name { get; set; }
        [Required]
        public string CountryId { get; set; }
        public Country Country { get; set; }
        public List<Division> Divisions { get; set; }
    }

}
