using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CBS.FrontDesk.Data.Entity.CustomerManagement.Grouping
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Group
    {
        public string GroupId { get; set; }
        public string CustomerType { get; set; }
        public string GroupName { get; set; }
        public string GroupLeaderId { get; set; }
        public string GroupTypeId { get; set; }
        public string RegistrationNumber { get; set; }
        public string TaxPayerNumber { get; set; }
        public string DateOfEstablishment { get; set; }
        public string Email { get; set; }
        public string CountryId { get; set; }
        public string RegionId { get; set; }
        public string TownId { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string PhotoSource { get; set; }
        public string DivisionId { get; set; }
      
        public string SubDivisionId { get; set; }
        public string BankCode { get; set; }
        public string BranchCode { get; set; }
        public string EconomicActivitiesId { get; set; }
        public string BranchId { get; set; }
        public string BankId { get; set; }
        public string Occupation { get; set; }
        public string IDNumberIssueDate { get; set; }
        public string IDNumberIssueAt { get; set; }
        public string IDNumber { get; set; }
        public string MembershipApprovalStatus { get; set; }
        public double Income { get; set; }
        public string WorkingStatus { get; set; }
        public string POBox { get; set; }
        public string Fax { get; set; }
        public string FormalOrInformalSector { get; set; }
        public string BankName { get; set; }
        public string BranchName { get; set; }
        public string CustomerId { get; set; }
        public string CustomerCategoryId { get; set; }
        public string Action { get; set; }
        public string ServiceOption { get; set; }
        public string MemberStatus { get; set; }
        public string PoliticaExposedAnswer { get; set; }
        public bool IsPoliticaExposed { get; set; }

        public string PoliticaExposedQuestion { get; set; }
        public GroupType GroupType { get; set; }
        public List<GroupCustomer> GroupCustomers { get; set; }
        public List<GroupDocument> GroupDocuments { get; set; }
        public bool Active { get; set; }
        public PaginationMetadata PaginationMetadata { get; set; } = new PaginationMetadata();
        public Group()
        {
            CustomerId = "0000000";
        }
    }
    public class AddGroupCustomerCommand
    {

        public List<string> CustomerIds { get; set; }
        public string GroupId { get; set; }
        public bool commit { get; set; } = true;
    }
    public class GroupManagement
    {
        public Group Group { get; set; }
        public AddGroupCustomerCommand AddGroupCustomerCommand { get; set; }
        public GroupDocument GroupDocument { get; set; }
        public List<Group> Groups { get; set; }
        public List<GroupCustomer> GroupCustomers { get; set; }
        public List<GroupDocument> GroupDocuments { get; set; }
        public IndividualProfile Customer { get; set; }
        public string Option { get; set; }
        public string Key { get; set; }
    }
}
