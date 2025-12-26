using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanConf
{

    public class LoanTargetCatalog
    {
        public string Id { get; set; }


        public string NameEn { get; set; }

        public string NameFr { get; set; }

        public PcmfPopulation PcmfPopulation { get; set; }

        public bool IsActive { get; set; }

        public string PcmfPopulationLabel =>
       PcmfPopulation.ToString();
    }

    public class CreateLoanTargetCatalogRequest
    {

        public string NameEn { get; set; }


        public string NameFr { get; set; }

        public int PcmfPopulation { get; set; }

        public bool IsActive { get; set; }
    }

    public class UpdateLoanTargetCatalogRequest
    {

        public string Id { get; set; }


        public string NameEn { get; set; }

        public string NameFr { get; set; }

        public int PcmfPopulation { get; set; }

        public bool IsActive { get; set; }
    }

    public class DeleteLoanTargetCatalogRequest
    {
        public string Id { get; set; }
    }

    public class LoanTargetCatalogQuery
    {
        public string NameFilter { get; set; }
        public bool? IsActiveFilter { get; set; }
    }

    public enum PcmfPopulation
    {
       // None = 0,
        Societaire = 1,
        Client = 2,
        Apparente = 3,
        EMF = 4,
        EMFReseau = 5
    }

}

