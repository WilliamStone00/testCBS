using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity.DataTable;

namespace CBS.FrontDesk.Data.Entity
{
    public class IndividualCustomerProfile
    {
        public IndividualProfile CustomerList { get; set; }
        public CustomerAccount CustomerAccount { get; set; } = new CustomerAccount();
        public AddCustomerAccount AddCustomerAccount { get; set; }
        public Aggregrate Aggregrate { get; set; }
        public AccountBalance AccountBalance { get; set; }
        public MemberAccountActivation MemberAccountActivation { get; set; }
        public List<MemberAccountActivation> MemberAccountActivations { get; set; }
        public MembershipNextOfKing MembershipNextOfKingsMember { get; set; }
        public List<MembershipNextOfKing> MembershipNextOfKingsMembers { get; set; }
        public List<CardSignatureSpecimen> CardSignatureSpecimens { get; set; }
        public CardSignatureSpecimen CardSignatureSpecimen { get; set; }
        public List<CustomerAccount> CustomerAccounts { get; set; }
        public List<SavingProduct> SavingProducts { get; set; } = new List<SavingProduct>();
        public CustomerDocumentRequest CustomerDocumentRequest { get; set; } = new CustomerDocumentRequest();

        public string option { get; set; }
        public IndividualCustomerProfile()
        {

        }
        public IndividualCustomerProfile(IndividualProfile customer = null, Aggregrate aggregrate = null, AccountBalance accountBalance = null, List<CustomerAccount> customerAccounts = null, AddCustomerAccount addCustomerAccount = null, MembershipNextOfKing membershipNextOfKingsMember = null, CardSignatureSpecimen cardSignatureSpecimen = null)
        {
            CustomerList = customer;
            Aggregrate = aggregrate;
            MemberAccountActivation = new MemberAccountActivation();
            MemberAccountActivations = new List<MemberAccountActivation>();
            AccountBalance = accountBalance;
            CustomerAccounts = customerAccounts;
            AddCustomerAccount = addCustomerAccount;
            MembershipNextOfKingsMember = membershipNextOfKingsMember;
            CardSignatureSpecimen = cardSignatureSpecimen;
        }

        public IndividualCustomerProfile(MembershipNextOfKing membershipNextOfKingsMember, CardSignatureSpecimen cardSignatureSpecimen)
        {
            MembershipNextOfKingsMember = membershipNextOfKingsMember;
            CardSignatureSpecimen = cardSignatureSpecimen;
        }
    }
    public class GetCustomersForDataTableQuery
    {
        public DataTableOptions Options { get; set; }

        // Filters
        public string MembershipApprovalStatus { get; set; }
        public string Gender { get; set; }
        public string MaritalStatus { get; set; }
        public string WorkingStatus { get; set; }
        public string CustomerType { get; set; }
        public string AgeCategoryStatus { get; set; }
        public string LegalForm { get; set; }
        public string CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string BranchId { get; set; }

        // Date Ranges
        public DateTime? DateOfBirthFrom { get; set; }
        public DateTime? DateOfBirthTo { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }

        // Show all flag
        public bool ShowAll { get; set; } = false;
    }


    public abstract class ResourceParameter
    {
        public ResourceParameter(string OrderBy)
        {
            this.OrderBy = OrderBy;

        }

        const int MaxPageSize = 100;
        public int Skip { get; set; } = 0;

        private int _PageSize = 10;
        public int PageSize
        {
            get
            {
                return _PageSize;
            }
            set
            {

                _PageSize = (value > MaxPageSize) ? MaxPageSize : value;
            }
        }

        public string SearchQuery { get; set; }
        public string OrderBy { get; set; }


    }
    public class PagginationResource : ResourceParameter
    {
        public PagginationResource():base("CustomerId")
        {
        }
        public string BranchId { get; set; }
        public bool IsByBranch { get; set; }
    }
    public abstract class GroupResourceParameter
    {
       
        const int MaxPageSize = 100;
        public int Skip { get; set; } = 0;

        private int _PageSize = 10;
        public int PageSize
        {
            get
            {
                return _PageSize;
            }
            set
            {

                _PageSize = (value > MaxPageSize) ? MaxPageSize : value;
            }
        }

        public string SearchQuery { get; set; }
        public string OrderBy { get; set; }


    }
    public class GroupResource : GroupResourceParameter
    {
        public string BranchId { get; set; }
        public bool IsByBranch { get; set; }

    }
    public class LoanResource : ResourceParameter
    {
        public LoanResource() : base("CustomerId")
        {
        }
        public bool IsByBranch { get; set; }

    }
}
