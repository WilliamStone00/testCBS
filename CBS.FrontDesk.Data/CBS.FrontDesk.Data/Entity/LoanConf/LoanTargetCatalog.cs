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

           public int PcmfPopulation { get; set; }

            public bool IsActive { get; set; }

            public DateTime? CreatedDate { get; set; }
            public string CreatedBy { get; set; }
            public DateTime? ModifiedDate { get; set; }
            public string ModifiedBy { get; set; }
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
    }

