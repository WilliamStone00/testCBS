using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations
{
    public class CustomerDataDto
    {
        public CustomerDto customerDto { get; set; }
        public List<AccountDto> accountDtos { get; set; }
    }

    public class CustomerDto
    {
        public string customerId { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
       
    }

    public class AccountDto
    {
        public string accountNumber { get; set; }
        public decimal balance { get; set; }
        public string status { get; set; }
        public string accountName { get; set; }
  
    }

}
