using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class PositiveAmountValidator : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            Decimal amount = Convert.ToDecimal(value);

            if (amount <= 0)
            {
                return new ValidationResult("Amount must be greater than 0");
            }

            return ValidationResult.Success;
        }
    }

    public class Transaction
    {
        [PositiveAmountValidator]
        public int Amount { get; set; }
    }
}

 

