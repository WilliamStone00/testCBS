using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CBS.FrontDesk.Data.Entity.CustomerManagement.Individual;

namespace CBS.FrontDesk.Data.Entity
{
    public class IndividualCustomerProfile
    {
        public CustomerList CustomerList { get; set; }
        public Aggregrate Aggregrate { get; set; }
        public AccountBalance AccountBalance { get; set; }
        public List<CustomerAccount> CustomerAccounts { get; set; }
        public string option { get; set; }
        public IndividualCustomerProfile()
        {
            
        }
        public IndividualCustomerProfile(CustomerList customer, Aggregrate aggregrate, AccountBalance accountBalance, List<CustomerAccount> customerAccounts)
        {
            CustomerList = customer;
            Aggregrate = aggregrate;
            AccountBalance = accountBalance;
            CustomerAccounts = customerAccounts;
        }
    }
}
