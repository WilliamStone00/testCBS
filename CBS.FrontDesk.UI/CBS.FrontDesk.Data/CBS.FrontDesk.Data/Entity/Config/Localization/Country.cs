using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Config
{
    public class Country
    {
        public string Id { get; set; }
        [Required]
        public string Code { get; set; }
        [Required]
        public string Name { get; set; }
    }
    public class GlobalizationViewModel
    {
        public Country Country { get; set; } = new Country();
        public Region Region { get; set; } = new Region();
        public Division Division { get; set; } = new Division();
        public SubDivision SubDivision { get; set; } = new SubDivision();
        public Town Town { get; set; } = new Town();

        public List<Country> Countries { get; set; } = new List<Country>();
        public List<Region> Regions { get; set; } = new List<Region>();
        public List<Division> Divisions { get; set; } = new List<Division>();
        public List<SubDivision> SubDivisions { get; set; } = new List<SubDivision>();
        public List<Town> Towns { get; set; } = new List<Town>();
    }

}
