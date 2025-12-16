using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations
{
  

    public class CustomerDataDto
    {
        public CustomerDto CustomerDto { get; set; }
        public List<AccountDto> AccountDtos { get; set; } = new List<AccountDto>();
        public List<SelectListItem> AccountSelectList { get; set; } = new List<SelectListItem>();
		public string BranchId { get; set; }
	}

    public class CustomerDto
    {
        public string CustomerId { get; set; }
        public string CustomerTempId { get; set; }
        public DateTime? DateOfCreationOfCustomerTempId { get; set; }
        public string CustomerTempCallbackStatus { get; set; }
        public string DailySaverId { get; set; }
        public string ExistingCustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsDailySaver { get; set; }
        public bool IsDailySaverAndMember { get; set; }
        public bool IsMemberOfACompany { get; set; }
        public bool IsMemberOfAGroup { get; set; }
        public string LegalForm { get; set; }
        public string CustomerType { get; set; }
        public string FormalOrInformalSector { get; set; }
        public string BankingRelationship { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string PlaceOfBirth { get; set; }
        public string Occupation { get; set; }
        public string Matricule { get; set; }
        public string AccountConfirmationNumber { get; set; }
        public string VillageOfOrigin { get; set; }
        public string Address { get; set; }
        public string IdNumber { get; set; }
        public DateTime? IdNumberIssueDate { get; set; }
        public string IdNumberIssueAt { get; set; }
        public string BankName { get; set; }
        public DateTime? MembershipApplicantDate { get; set; }
        public string MembershipApplicantProposedByReferral1 { get; set; }
        public string MembershipApplicantProposedByReferral2 { get; set; }
        public string MembershipApprovalBy { get; set; }
        public string MembershipApprovalStatus { get; set; }
        public DateTime? MembershipApprovedDate { get; set; }
        public string MembershipApprovedSignatureUrl { get; set; }
        public string MembershipAllocatedNumber { get; set; }
        public string PoBox { get; set; }
        public string Fax { get; set; }
        public string MobileLoginId { get; set; }
        public string Gender { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string CountryId { get; set; }
        public string RegionId { get; set; }
        public string CustomerCode { get; set; }
        public string ProfileType { get; set; }
        public string TownId { get; set; }
        public string PhotoUrl { get; set; }
        public string Pin { get; set; }
        public int MobileOrOnLineBankingLoginFailedAttempts { get; set; }
        public int NumberOfAttemptsOfMobileOrOnLineBankingLogin { get; set; }
        public bool IsUseOnLineMobileBanking { get; set; }
        public string MobileOrOnLineBankingLoginState { get; set; }
        public string CustomerPackageId { get; set; }
        public string DivisionId { get; set; }
        public string BranchId { get; set; }
        public string EconomicActivitiesId { get; set; }
        public string SignatureUrl { get; set; }
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
        public decimal Income { get; set; }
        public string CustomerCategoryId { get; set; }
        public string AgeCategoryStatus { get; set; }
        public string WorkingStatus { get; set; }
        public string ActiveStatus { get; set; }
        public bool IsDailyCollector { get; set; }
        public string RegistrationNumber { get; set; }
        public DateTime? CompanyCreationDate { get; set; }
        public string PlaceOfCreation { get; set; }
        public string ConditionForWithdrawal { get; set; }
        public string MName { get; set; }
        public string MAddress { get; set; }
        public string MOccupation { get; set; }
        public string MPhone { get; set; }
        public string FName { get; set; }
        public string FAddress { get; set; }
        public string FOccupation { get; set; }
        public string FPhone { get; set; }
        public bool TagAsPISCollectionProfile { get; set; }
        public string UserId { get; set; }
        public bool IsLinkToUser { get; set; }
       
    }

    public class AccountDto
    {
        public string Id { get; set; }
        public string ProductId { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string AccountId { get; set; }
        public bool IsRemoveAccount { get; set; }
        public string AccountNumber { get; set; }
        public decimal Balance { get; set; }
        public decimal PreviousBalance { get; set; }
        public string Status { get; set; }
        public string TellerId { get; set; }
        public string BranchCode { get; set; }
        public string EncryptedBalance { get; set; }
        public decimal InterestGenerated { get; set; }
        public decimal LastInterestPosted { get; set; }
        public decimal BlockedAmount { get; set; }
        public string BlockedId { get; set; }
        public string ProfileType { get; set; }
        public DateTime? DateBlocked { get; set; }
        public DateTime? DateReleased { get; set; }
        public string ReasonOfBlocked { get; set; }
        public DateTime? DateOfLastOperation { get; set; }
        public DateTime? LastInterestCalculatedDate { get; set; }
        public string AccountName { get; set; }
        public string LastOperation { get; set; }
        public string AccountType { get; set; }
        public bool IsTellerAccount { get; set; }
        public string OpenningOfDayStatus { get; set; }
        public DateTime? OpenningOfDayDate { get; set; }
        public string OpenningOfDayReference { get; set; }
        public decimal OpeningBalance { get; set; }
        public DateTime? DateOfOpeningBalance { get; set; }
        public decimal LastOperationAmount { get; set; }
        public string BankId { get; set; }
        public string BranchId { get; set; }
    }


}
