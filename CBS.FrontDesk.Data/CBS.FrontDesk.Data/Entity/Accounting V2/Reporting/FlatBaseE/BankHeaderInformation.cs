using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting_V2.Reporting.FlatBaseE
{
    public class BankHeaderInformation
    {
        //====================================== bank information ===================================================
        public string BankId { get; set; }
        public string BankBankCode { get; set; }
        public string BankName { get; set; }
        public string BankDescription { get; set; }
        public string BankTelephone { get; set; }
        public string BankEmail { get; set; }
        public string BankAddress { get; set; }
        public string BankOrganizationId { get; set; }
        public string BankCapital { get; set; }
        public string BankRegistrationNumber { get; set; }
        public string BankLogoUrl { get; set; }
        public string BankWaterMarkUrl { get; set; }
        public string BankImmatriculationNumber { get; set; }
        public string BankTaxPayerNUmber { get; set; }
        public string BankPBox { get; set; }
        public string BankWebSite { get; set; }
        public string BankDateOfCreation { get; set; }
        public string BankInitial { get; set; }
        public string BankMotto { get; set; }
        public string BankCustomerServiceContact { get; set; }
        public string BankCategoryInformation { get; set; }
        public string BankShortHeaderInfo { get; set; }
        public string BankFax { get; set; }
        public string BankRegistrationInformation { get; set; }




        //====================================== Branch information ===================================================


        public string BranchId { get; set; }

        public bool BranchIsHavingBank { get; set; } 

        public string BranchCode { get; set; }

        public string BranchName { get; set; }

        public string BranchLocation { get; set; }

        public string BranchTelephone { get; set; }

        public string BranchEmail { get; set; }

        public string BranchAddress { get; set; }

        public string BranchBankId { get; set; }

        public string BranchCapital { get; set; }

        public string BranchRegistrationNumber { get; set; }

        public string BranchLogoUrl { get; set; }

        public string BranchImmatriculationNumber { get; set; }

        public string BranchTaxPayerNUmber { get; set; }

        public bool BranchActiveStatus { get; set; }

        public bool BranchIsHeadOffice { get; set; }

        public string BranchPBox { get; set; }

        public string BranchWebSite { get; set; }

        public string BranchDateOfCreation { get; set; }


        public string BranchMotto { get; set; }

        public string BranchHeadOfficeTelehoneNumber { get; set; }

        public string BranchImageVirtualPath { get; set; }

        public string BranchHeadOfficeAddress { get; set; }


    }
}
