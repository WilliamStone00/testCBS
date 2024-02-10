using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{

    public class AccountType
    {
        public string name { get; set; }
        public string Id { get; set; }
        public string operationAccountTypeId { get; set; }  
        public string operationAccountType { get; set; }

        public AccountType()
        {
        }

         public AccountTypeDto ConvertToDto() { return new AccountTypeDto
         {
             Name = name,
            Code = operationAccountTypeId,
             Description = operationAccountType
         }; }
    }

    public class AccountTypeDto
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
    }
}
