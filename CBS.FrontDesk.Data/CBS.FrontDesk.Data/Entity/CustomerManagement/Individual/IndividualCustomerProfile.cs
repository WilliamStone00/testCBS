using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CBS.FrontDesk.Data.Entity.CustomerManagement;

namespace CBS.FrontDesk.Data.Entity
{
    public class IndividualCustomerProfile
    {
        public IndividualProfile CustomerList { get; set; }
        public AddCustomerAccount AddCustomerAccount { get; set; }
        public Aggregrate Aggregrate { get; set; }
        public AccountBalance AccountBalance { get; set; }
        public MemberAccountActivation MemberAccountActivation { get; set; }
        public List<MemberAccountActivation> MemberAccountActivations { get; set; }
        public MembershipNextOfKingsMember MembershipNextOfKingsMember { get; set; }
        public List<MembershipNextOfKingsMember> MembershipNextOfKingsMembers { get; set; }
        public List<CardSignatureSpecimen> CardSignatureSpecimens { get; set; }
        public CardSignatureSpecimen CardSignatureSpecimen { get; set; }
        public List<CustomerAccount> CustomerAccounts { get; set; }
        public List<SavingProduct> SavingProducts { get; set; }=new List<SavingProduct>();
        public CustomerDocumentRequest CustomerDocumentRequest { get; set; } = new CustomerDocumentRequest();
        
        public string option { get; set; }
        public IndividualCustomerProfile()
        {
            
        }
        public IndividualCustomerProfile(IndividualProfile customer=null, Aggregrate aggregrate=null, AccountBalance accountBalance=null, List<CustomerAccount> customerAccounts=null, AddCustomerAccount addCustomerAccount=null, MembershipNextOfKingsMember membershipNextOfKingsMember=null, CardSignatureSpecimen cardSignatureSpecimen=null)
        {
            CustomerList = customer;
            Aggregrate = aggregrate;
            MemberAccountActivation=new MemberAccountActivation();
            MemberAccountActivations=new List<MemberAccountActivation>();
            AccountBalance = accountBalance;
            CustomerAccounts = customerAccounts;
            AddCustomerAccount = addCustomerAccount;
            MembershipNextOfKingsMember = membershipNextOfKingsMember;
            CardSignatureSpecimen = cardSignatureSpecimen;
        }

        public IndividualCustomerProfile(MembershipNextOfKingsMember membershipNextOfKingsMember, CardSignatureSpecimen cardSignatureSpecimen)
        {
            MembershipNextOfKingsMember = membershipNextOfKingsMember;
            CardSignatureSpecimen = cardSignatureSpecimen;
        }
    }
    
}
