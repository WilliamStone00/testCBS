using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanConf
{
    public class LoanGuarantor
    {
        public string Id { get; set; }
        public string GuarantorType { get; set; }
        public string LoanApplicationId { get; set; }
        [Required]
        public string GuarantorName { get; set; }
        public string CustomerId { get; set; }
        [Required]
        public string IdCardNumber { get; set; }
        [Required]
        public string ExpireDate { get; set; }
        [Required]
        public string IssueDate { get; set; }
        public string Relationship { get; set; }
        public string Address { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        public bool IsCoMember { get; set; }
        [Required]
        public string AccountNumber { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public decimal GuaranteeAmount { get; set; }
        public LoanApplication LoanApplication { get; set; }
    }

}
