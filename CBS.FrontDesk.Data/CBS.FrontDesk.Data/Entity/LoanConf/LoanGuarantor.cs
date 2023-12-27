using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanConf
{
    public class LoanGuarantor
    {
        public string id { get; set; }
        public string loanApplicationId { get; set; }
        public string guarantorName { get; set; }
        public string idCardNumber { get; set; }
        public string expireDate { get; set; }
        public string issueDate { get; set; }
        public string relationship { get; set; }
        public string address { get; set; }
        public string phoneNumber { get; set; }
        public bool isMemberOfMicrofinance { get; set; }
        public string bankAccountNumber { get; set; }
    }
  
}
