using CBS.FrontDesk.Data.Entity.CustomerManagement.Grouping;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CustomerManagement
{
    public class SubcriptionPackage
    {
        public string FeeTyeName { get; set; }
        public string Amount { get; set; }
    }
    public class MemberActivationRequest
    {
        public string ReferenceNumber { get; set; }
        public int AlphaNumber { get; set; }
        public string MemberType { get; set; } = "New";
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string BranchId { get; set; }
        public string EventName { get; set; }
        public string ActivationType { get; set; }// Re-Subcription, New-Subcription
        public decimal TotalSubcriptionAmount { get; set; }
        public List<SubcriptionPackage> SubcriptionPackage { get; set; }
    }

    public class MemberAccountHistory
    {
        public string Id { get; set; }
        public string ReferenceNumber { get; set; }
        public string FeeTyeName { get; set; }
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string BranchId { get; set; }
        public string TransactionRferenceId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date  { get; set; }
        public decimal TotalSubcriptionAmount { get; set; }
        public string EventName { get; set; }
    }

    public class IndividualProfile
    {
        public string CustomerId { get; set; }
        public string CustomerType { get; set; }
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
        public bool IsDailyCollector { get; set; } = false;
        public string MobileLoginId { get; set; }
        public bool NoneMemberAccount { get; set; } = false;
        public string Address { get; set; }
        public string IDNumber { get; set; }
        public string IDNumberIssueDate { get; set; }
        public string IDNumberIssueAt { get; set; }
        public string BankName { get; set; }
        public string Matricule { get; set; }
        public string AccountConfirmationNumber { get; set; }

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
        public string MName { get; set; }
        public string MAddress { get; set; }
        public string MOccupation { get; set; }
        public string MPhone { get; set; }
        public string FName { get; set; }
        public string FAddress { get; set; }
        public string FOccupation { get; set; }
        public string FPhone { get; set; }
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
        public PaginationMetadata PaginationMetadata { get; set; } = new PaginationMetadata();
        public IndividualProfile()
        {
            CustomerId = "0000000";
            CustomerCode = "0000000";
            ImageVirtualPath = "~/AppFiles/Images/p.jpg";
            ImageVirtualSignaturePath = "~/AppFiles/Images/s.jpg";
            ImageNoVirtualPath = "~/AppFiles/Images/noimage.jpg";
            ImageVirtualNoSignaturePath = "~/AppFiles/Images/no signature.png";
        }
    }
    public class PaginationMetadata
    {
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int Skip { get; set; }
        public int TotalPages { get; set; }
    }
    public class CustomerListingDto
    {

        public string Id { get; set; }
        public string Name { get; set; }
        public string BankingRelationship { get; set; } //This is an enum//  1.  CB-Customer  2. NCB-Non-customer
        public DateTime RegistrationDate { get; set; }
        public string IDNumber { get; set; }
        public string IDNumberIssueDate { get; set; }
        public string MembershipApprovalStatus { get; set; }
        public string Address { get; set; }
        public string BranchId { get; set; }
        public string PhoneNumber { get; set; }
        public string Matricule { get; set; }
        public string AccountConfirmationNumber { get; set; }

    }
    public class ReportQuerTemplate : IValidatableObject
    {
        public bool ByBranch { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date From")]
        public string DateFrom { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date To")]
        public string DateTo { get; set; }

        [Required(ErrorMessage = "Branch is required.")]
        public string BranchId { get; set; }

        [Required(ErrorMessage = "Members Status Type is required.")]
        public string MembersStatusType { get; set; } // None Members, Members, Both

        [Required(ErrorMessage = "Legal Form Status is required.")]
        public string LegalFormStatus { get; set; } // Moral_Person, Physical_Person, Both

        [Required(ErrorMessage = "Query Parameter is required.")]
        public string QueryParameter { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var validationResults = new List<ValidationResult>();

            // Validate DateFrom and DateTo only if QueryParameter is ByDate
            if (QueryParameter == "ByDate")
            {
                if (string.IsNullOrWhiteSpace(DateFrom))
                {
                    validationResults.Add(new ValidationResult(
                        "Date From is required when Query Parameter is ByDate.",
                        new[] { nameof(DateFrom) }));
                }

                if (string.IsNullOrWhiteSpace(DateTo))
                {
                    validationResults.Add(new ValidationResult(
                        "Date To is required when Query Parameter is ByDate.",
                        new[] { nameof(DateTo) }));
                }
                else
                {
                    // Validate DateFrom and DateTo if both are provided
                    DateTime fromDate;
                    DateTime toDate;

                    if (DateTime.TryParse(DateFrom, out fromDate) && DateTime.TryParse(DateTo, out toDate))
                    {
                        if (fromDate > toDate)
                        {
                            validationResults.Add(new ValidationResult(
                                "The DateFrom must be less than or equal to DateTo.",
                                new[] { nameof(DateFrom), nameof(DateTo) }));
                        }
                    }
                    else
                    {
                        validationResults.Add(new ValidationResult(
                            "Invalid date format for DateFrom or DateTo.",
                            new[] { nameof(DateFrom), nameof(DateTo) }));
                    }
                }
            }

            // BranchId must be present at all times
            if (string.IsNullOrEmpty(BranchId))
            {
                validationResults.Add(new ValidationResult(
                    "Branch is required.",
                    new[] { nameof(BranchId) }));
            }

            return validationResults;
        }
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
