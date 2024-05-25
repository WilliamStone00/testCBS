using CBS.FrontDesk.Data.Entity.CustomerManagement.Grouping;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CustomerManagement
{

    public class IndividualProfile
    {
        public string CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsMemberOfACompany { get; set; }
        public bool IsMemberOfAGroup { get; set; }
        public string LegalForm { get; set; }
        public string FormalOrInformalSector { get; set; }
        public string BankingRelationship { get; set; } //This is an enum//  1.  CB-Customer  2. NCB-Non-customer
        public DateTime DateOfBirth { get; set; }
        public string PlaceOfBirth { get; set; }
        public string Occupation { get; set; }
        public string VillageOfOrigin { get; set; }
        public string Address { get; set; }
        public string IDNumber { get; set; }
        public string IDNumberIssueDate { get; set; }
        public string IDNumberIssueAt { get; set; }
        public string BankName { get; set; }
        public string MembershipApplicantDate { get; set; }
        public string MembershipApplicantProposedByReferral1 { get; set; }
        public string MembershipApplicantProposedByReferral2 { get; set; }
        public string MembershipApprovalBy { get; set; }
        public string MembershipApprovalStatus { get; set; }
        public string MembershipApprovedDate { get; set; }
        public string MembershipApprovedSignatureUrl { get; set; }
        public string MembershipAllocatedNumber { get; set; }
        public string POBox { get; set; }
        public string Fax { get; set; }
        public string Gender { get; set; } = "Male";
        public string Email { get; set; }
        public string Phone { get; set; }
        public string CountryId { get; set; }
        public string RegionId { get; set; }
        public string CustomerCode { get; set; }
        public string ProfileType { get; set; }
        public string TownId { get; set; }
        public int MobileOrOnLineBankingLoginFailedAttempts { get; set; }
        public int NumberOfAttemptsOfMobileOrOnLineBankingLogin { get; set; }
        public bool IsUseOnLineMobileBanking { get; set; }
        public string CustomerPackageId { get; set; }
        public string DivisionId { get; set; }
        public string BranchId { get; set; }
        public string EconomicActivitiesId { get; set; }
        public string BankId { get; set; }
        public string OrganizationId { get; set; }
        public string Language { get; set; }
        public string SubDivisionId { get; set; }
        public string TaxIdentificationNumber { get; set; }
        public bool IsBelongToGroup { get; set; }
        public bool Active { get; set; }
        public string SecretQuestion { get; set; }
        public string SecretAnswer { get; set; }
        public string EmployerName { get; set; }
        public string EmployerTelephone { get; set; }
        public string EmployerAddress { get; set; }
        public string MaritalStatus { get; set; }
        public string SpouseName { get; set; }
        public string SpouseAddress { get; set; }
        public int NumberOfKids { get; set; }
        public string SpouseOccupation { get; set; }
        public string SpouseContactNumber { get; set; }
        public double Income { get; set; }
        public string CustomerCategoryId { get; set; }
        public string WorkingStatus { get; set; }
        public string ActiveStatus { get; set; }
        public string RegistrationNumber { get; set; }
        public DateTime CompanyCreationDate { get; set; }
        public string PlaceOfCreation { get; set; }
        public string ConditionForWithdrawal { get; set; }
        public List<CustomerDocument> CustomerDocuments { get; set; } = new List<CustomerDocument>();
        public List<MembershipNextOfKing> MembershipNextOfKings { get; set; } = new List<MembershipNextOfKing>();
        public List<CardSignatureSpecimen> CardSignatureSpecimens { get; set; } = new List<CardSignatureSpecimen>();
        public CustomerCategory CustomerCategory { get; set; } = new CustomerCategory();
        public List<GroupCustomer> GroupCustomers { get; set; } = new List<GroupCustomer>();
        public string ImageVirtualPath { get; set; }
        public string ImageNoVirtualPath { get; set; }
        public string ImageVirtualSignaturePath { get; set; }
        public string ImageVirtualNoSignaturePath { get; set; }
        public string branch { get; set; }
        public object pin { get; set; }
        public string name { get; set; }
        public string town { get; set; }
        public string branchCode { get; set; }
        public string bankCode { get; set; }
        public string photoUrl { get; set; }
        public string mobileOrOnLineBankingLoginState { get; set; }
        public string signatureUrl { get; set; }
        public IndividualProfile()
        {
            ImageVirtualPath= "~/AppFiles/Images/p.jpg";
            ImageVirtualSignaturePath = "~/AppFiles/Images/s.jpg";
            ImageNoVirtualPath = "~/AppFiles/Images/noimage.jpg";
            ImageVirtualNoSignaturePath = "~/AppFiles/Images/no signature.png";
        }

       

        public class CustomerDocument
        {
            public string documentId { get; set; }
            public string personId { get; set; }
            public string urlPath { get; set; }
            public string documentName { get; set; }
            public string extension { get; set; }
            public string baseUrl { get; set; }
            public string documentType { get; set; }

          
        }


      

    }

}
