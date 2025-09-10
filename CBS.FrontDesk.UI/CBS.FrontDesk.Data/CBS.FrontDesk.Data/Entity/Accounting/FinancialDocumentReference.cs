using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class FinancialDocumentReference 
    {

        public string Id { get; set; }  // Made setter private to ensure controlled access

        public string Reference { get; set; }

        public string DescriptionEn { get; set; }

        public string  DescriptionFr { get; set; }

        public string  DescriptionSp { get; set; }

        public CorrespondingAccount CorrespondingAccount { get; set; } = new CorrespondingAccount();

        public string DocumentId { get; set; }

        public string DocumentTypeId { get; set; }

    }



    public class CorrespondingAccount

    {

        public List<AccountReference> GrossAccounts { get; set; } = new List<AccountReference>();

        public List<AccountReference> GrossExceptionAccounts { get; set; } = new List<AccountReference>();

        public List<AccountReference> ProvisionAccounts { get; set; } = new List<AccountReference>();

        public List<AccountReference> ProvisionExceptionAccounts { get; set; } = new List<AccountReference>();

        public List<AccountReference> ContainsConditionAccounts { get; set; } = new List<AccountReference>();

        public bool ContainsCondition { get; set; }  // If True then ContainsConditionAccounts contains items else no items  present

        public bool ContainsException { get; set; } // If True then GrossExceptionAccounts or ProvisionExceptionAccounts contains items else no items  present

        public string Category { get; set; } // { Revenue, Asset ,Liabilities or Expense}

    }

    public class AccountReference

    {

        public string AccountNumber { get; set; }

        public string DocumentBooking { get; set; }

        public AccountReference(string accountNumber, string documentBooking)

        {

            AccountNumber = accountNumber;

            DocumentBooking = documentBooking;

        }

        public AccountReference()

        {



        }

    }
    public class FinancialDocumentReferenceDto
    {
        public string Id { get; set; }  // Made setter private to ensure controlled access
        public string Reference { get; set; }
        public string DescriptionEn { get; set; }
        public string DescriptionFr { get; set; }
        public string DescriptionSp { get; set; }
        public CorrespondingAccount CorrespondingAccount { get; set; }
        public string DocumentId { get; set; }
        public string DocumentName { get; set; }
        public string DocumentSectionName { get; set; }
        public string DocumentTypeId { get; set; }

        public string Category { get; set; } // { Revenue, Asset ,Liabilities or Expense}
    }
}
