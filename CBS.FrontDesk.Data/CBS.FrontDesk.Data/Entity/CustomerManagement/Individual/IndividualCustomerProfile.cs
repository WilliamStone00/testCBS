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
        public List<CustomerAccount> CustomerAccounts { get; set; }
        public List<SavingProduct> SavingProducts { get; set; }=new List<SavingProduct>();
        public CustomerDocumentRequest CustomerDocumentRequest { get; set; } = new CustomerDocumentRequest();
        
        public string option { get; set; }
        public IndividualCustomerProfile()
        {
            
        }
        public IndividualCustomerProfile(IndividualProfile customer, Aggregrate aggregrate, AccountBalance accountBalance, List<CustomerAccount> customerAccounts, AddCustomerAccount addCustomerAccount)
        {
            CustomerList = customer;
            Aggregrate = aggregrate;
            AccountBalance = accountBalance;
            CustomerAccounts = customerAccounts;
            AddCustomerAccount = addCustomerAccount;
        }
    }
    
}
