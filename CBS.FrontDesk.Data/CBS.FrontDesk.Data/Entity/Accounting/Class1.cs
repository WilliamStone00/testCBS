using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class AccountingBookDetail
    {
        public string mfiChartOfAccountId { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string entryRuleCodification { get; set; }
        public bool isCommissionAccount { get; set; }
  
        public string accountType { get; set; }
        public string operationType { get; set; }
        public string commissionRecipient { get; set; }
        public string provisionPeriod { get; set; }
    }

    public class AccountingBook
    {
        public string productName { get; set; }
        public string productCode { get; set; }
        public List<AccountingBookDetail> accountingBookDetails { get; set; }
        public string productAccountBookId { get; set; }
        public string productType { get; set; }
    }

    public class AccountingEventBook
    {
        public string FeeName { get; set; }
        public string EventCode { get; set; }
 
        public string ChartOfAccountId { get; set; }
        public string ServiceType { get; set; }
    }



    public class ProductConfigurationViewModel
    {
        public List<AccountingBook> ProductConfigurations { get; set; } = new List<AccountingBook>();
        public int TotalLoanProducts => ProductConfigurations.Where(x=>x.productType.ToLower()=="loanproduct").Count();
        public int TotalSavingProducts => ProductConfigurations.Where(x => x.productType.ToLower() == "savingproduct").Count();
        public int TotalLoanNotConfiguredProducts => GetUnConfiguredLoanProduct();
        public int TotalSavingNotConfiguredProducts => GetUnConfiguredSavingProduct();
        private int GetUnConfiguredLoanProduct()
        {
            int count = 0;
            var listLoanProd = this.ProductConfigurations.Where(x => x.productType.ToLower() == "loanproduct");
            foreach (var item in listLoanProd)
            {
                if (item.accountingBookDetails == null || item.accountingBookDetails.Count == 0)
                {
                    count = count + 1;
                }
                else
                {
                    count = count + item.accountingBookDetails.Count(a => string.IsNullOrEmpty(a.entryRuleCodification));
                }
            }
            return count;
        }
        private int GetUnConfiguredSavingProduct()
        {
            int count = 0;
            var listLoanProd = this.ProductConfigurations.Where(x => x.productType.ToLower() == "savingproduct");
            foreach (var item in listLoanProd)
            {
                if (item.accountingBookDetails == null || item.accountingBookDetails.Count == 0)
                {
                    count = count + 1;
                }
                else
                {
                    count = count + item.accountingBookDetails.Count(a => string.IsNullOrEmpty(a.entryRuleCodification));
                }
            }
            return count;
        }
      
    }

    public class UnconfiguredProductsResponse
    {
        public bool success { get; set; } // Indicates if the request was processed successfully

        public bool hasUnconfiguredProducts { get; set; } // True if there are products not configured with accounts

        public int unconfiguredCount { get; set; } // Total number of unconfigured products

        // Optional: You can add a list of unconfigured products for more details if needed
        public List<AccountingBook> unconfiguredProducts { get; set; }

        public int GetUnConfiguredSavingProduct(List<AccountingBook>  UnconfiguredProducts)
        {
            int count = 0;
           
            foreach (var item in UnconfiguredProducts)
            {
                if (item.accountingBookDetails == null || item.accountingBookDetails.Count == 0)
                {
                    count = count + 1;
                }
                else
                {
                    count = count + item.accountingBookDetails.Count(a => string.IsNullOrEmpty(a.entryRuleCodification));
                }
            }
            return count;
        }
    }
}
