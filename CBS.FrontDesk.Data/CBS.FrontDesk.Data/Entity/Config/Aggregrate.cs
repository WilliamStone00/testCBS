using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CBS.FrontDesk.Data.Entity.SavingProducts;

namespace CBS.FrontDesk.Data.Entity.Config
{



  







    public class Aggregrate
    {
        public List<EconomicActivity> EconomicActivities { get; set; }
        public List<Organization> Organizations { get; set; }
        public List<Bank> Banks { get; set; }
        public List<Branch> Branches { get; set; }
        public List<Country> Countries { get; set; }
        public List<Region> Regions { get; set; }
        public List<SubDivision> Subdivisions { get; set; }
        public List<Division> Divisions { get; set; }
        public List<Town> Towns { get; set; }
        public List<Saving> Savings { get; set; }
        public List<SubcriptionPackage> SubcriptionPackages { get; set; }
    }





}
