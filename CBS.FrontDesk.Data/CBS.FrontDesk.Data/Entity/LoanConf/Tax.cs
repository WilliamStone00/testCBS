using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.LoanConf
{
 
    public class Tax
    {
        public string Id { get; set; }
        [Required]
        public string Name { get; set; }
        public decimal TaxRate { get; set; }
        public bool AppliedWhenLoanRequestIsGreaterThanSaving { get; set; }
        public bool IsVat { get; set; }

        public bool AppliedOnInterest { get; set; }
        [Required]
        public decimal SavingControlAmount { get; set; }
        [Required]
        public string Description { get; set; }
        public List<LoanApplication> LoanApplications { get; set; }
        public Tax()
        {
            AppliedOnInterest = true;
            AppliedWhenLoanRequestIsGreaterThanSaving = true;
            SavingControlAmount = 0;
            TaxRate = 0;
            IsVat = true;
        }

    }

}
