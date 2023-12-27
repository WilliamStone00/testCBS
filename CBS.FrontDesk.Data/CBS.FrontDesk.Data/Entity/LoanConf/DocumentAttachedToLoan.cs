using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CBS.FrontDesk.Data.Entity.LoanConf
{
  
    public class DocumentAttachedToLoan
    {
        public string Id { get; set; }
        [Required]
        public string LoanApplicationID { get; set; }
        [Required]
        public List<HttpPostedFileBase> AttachedFiles { get; set; }
    }
    public class DocumentAttachedToLoanResponse
    {
        public string Id { get; set; }
        public string LoanApplicationID { get; set; }
        public string DocumentName { get; set; }
        public string FilePath { get; set; }
    }
}
