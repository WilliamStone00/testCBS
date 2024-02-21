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
        public string bankName { get; set; }
        [Required]
        public string firstName { get; set; }
        [Required]
        public string lastName { get; set; }
        public DateTime dateOfBirth { get; set; }
        [Required]
        public string gender { get; set; }
        [Required]
        public string email { get; set; }
        [Required]
        public string phone { get; set; }
        [Required]
        public string idNumber { get; set; }
        [Required]
        public string countryId { get; set; }
        [Required]
        public string regionId { get; set; }
        [Required]
        public string townId { get; set; }
        [Required]
        public string address { get; set; }
        public bool isUseOnLineMobileBanking { get; set; }
        public string packageId { get; set; }
        [Required]
        public string divisionId { get; set; }
        [Required]
        public string branchId { get; set; }
        [Required]
        public string economicActivitiesId { get; set; }
        [Required]
        public string bankId { get; set; }
        [Required]
        public string organizationId { get; set; }
        [Required]
        public string subDivisionId { get; set; }
        public string taxIdentificationNumber { get; set; }
        public string customerId { get; set; }
        public bool isMemberOfACompany { get; set; }
        public bool isMemberOfAGroup { get; set; }
        [Required]
        public string legalForm { get; set; }
        [Required]
        public string formalOrInformalSector { get; set; }
        public string bankingRelationship { get; set; }
        [Required]
        public string placeOfBirth { get; set; }
        [Required]
        public string occupation { get; set; }
        public string idNumberIssueDate { get; set; }
        public string idNumberIssueAt { get; set; }
        [Required]
        public string membershipApprovalStatus { get; set; }
        public string poBox { get; set; }
        public string fax { get; set; }
        public string customerPackageId { get; set; }
        [Required]
        public string language { get; set; }
        public bool active { get; set; }
        public string secretQuestion { get; set; }
        public string secretAnswer { get; set; }
        public string employerName { get; set; }
        public string employerTelephone { get; set; }
        public string employerAddress { get; set; }
        public string maritalStatus { get; set; }
        public string spouseName { get; set; }
        public string spouseAddress { get; set; }
        public int numberOfKids { get; set; }
        public string spouseOccupation { get; set; }
        public string spouseContactNumber { get; set; }
        public double income { get; set; }
        [Required]
        public string customerCategoryId { get; set; }
        public string workingStatus { get; set; }
        public string activeStatus { get; set; }
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
        public string membershipApplicantDate { get; set; }
        public string membershipApplicantProposedByReferral1 { get; set; }
        public string membershipApplicantProposedByReferral2 { get; set; }
        public string membershipApprovalBy { get; set; }
        public string membershipApprovedDate { get; set; }
        public string membershipApprovedSignatureUrl { get; set; }
        public string membershipAllocatedNumber { get; set; }

        public string photoUrl { get; set; }

        public string mobileOrOnLineBankingLoginState { get; set; }

        public string signatureUrl { get; set; }
        public List<CustomerDocument> customerDocuments { get; set; }=new List<CustomerDocument>();
        public List<MembershipNextOfKingsMember> membershipNextOfKings { get; set; }=new List<MembershipNextOfKingsMember>();
        public List<CardSignatureSpecimen> cardSignatureSpecimens { get; set; } = new List<CardSignatureSpecimen>();
        public List<CardSignatureSpecimenDetail> cardSignatureSpecimenDetails { get; set; } = new List<CardSignatureSpecimenDetail>();
        public CustomerCategory customerCategory { get; set; }=new CustomerCategory();
        public IndividualProfile()
        {
            ImageVirtualPath= "~/AppFiles/Images/p.jpg";
            ImageVirtualSignaturePath = "~/AppFiles/Images/s.jpg";
            ImageNoVirtualPath = "~/AppFiles/Images/noimage.jpg";
            ImageVirtualNoSignaturePath = "~/AppFiles/Images/no signature.png";
        }

        public class CustomerCategory
        {
            public DateTime createdDate { get; set; }
            public string createdBy { get; set; }
            public DateTime modifiedDate { get; set; }
            public string modifiedBy { get; set; }
            public DateTime deletedDate { get; set; }
            public string deletedBy { get; set; }
            public int objectState { get; set; }
            public bool isDeleted { get; set; }
            public string customerCategoryId { get; set; }
            public string categoryCode { get; set; }
            public string categoryName { get; set; }
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
