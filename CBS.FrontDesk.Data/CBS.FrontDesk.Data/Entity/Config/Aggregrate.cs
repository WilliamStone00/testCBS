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
        public List<EconomicActivity> EconomicActivities { get; set; }= new List<EconomicActivity>();
        public List<Organization> Organizations { get; set; } = new List<Organization>();
        public List<Bank> Banks { get; set; }=new List<Bank>();
        public List<Branch> Branches { get; set; }=new List<Branch>();
        public List<Country> Countries { get; set; }=new List<Country>();
        public List<Region> Regions { get; set; }=new List<Region> ();
        public List<SubDivision> Subdivisions { get; set; }=new List<SubDivision>();
        public List<Division> Divisions { get; set; }=new List<Division> ();
        public List<Town> Towns { get; set; }=new List<Town>();
        public List<SavingProduct> Savings { get; set; }=new List<SavingProduct>();
        public List<SubcriptionPackage> SubcriptionPackages { get; set; }=new List<SubcriptionPackage>();
        public CustomerDefaultEnum CustomerDefaultEnum { get; set; } = new CustomerDefaultEnum();
    }

    public class CustomerDefaultEnum
    {
        public List<StringValues> legalForms { get; set; }
        public List<StringValues> workingStatuses { get; set; }
        public List<StringValues> activeStatuses { get; set; }
        public List<StringValues> maritalStatuses { get; set; }
        public List<StringValues> bankingRelationships { get; set; }
        public List<StringValues> formalOrInformalSectors { get; set; }
        public List<StringValues> genders { get; set; }
        public List<StringValues> membershipApprovalStatuses { get; set; }
        public List<StringValues> languages { get; set; }
        public List<StringValues> customerCategories { get; set; }
        public List<StringValues> relationships { get; set; }
        //
        //
    }



}
