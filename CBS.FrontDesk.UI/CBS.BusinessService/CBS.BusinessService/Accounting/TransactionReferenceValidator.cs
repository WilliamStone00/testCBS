using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting
{
 
    public class TransactionReferenceValidator : ValidationAttribute
    {
        protected override  ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            //string reference = Convert.ToString(value);
            //var result = await new AccountingEntryServices().CheckIfTransactionReferenceIdExist(reference);

            //if (result == false)
            //{
            //    return new ValidationResult("Transaction with reference:" + reference + " does not exist in the system. Kindly enter correct reference");
            //}

            return ValidationResult.Success;
        }
    }
}
