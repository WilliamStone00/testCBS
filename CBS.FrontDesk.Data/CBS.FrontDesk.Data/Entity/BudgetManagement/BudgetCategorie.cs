using CBS.FrontDesk.Data.Entity.Accounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class BudgetCategory
    {

        public string Id { get; set; }

        public string CategoryName { get; set; }


        public string Description { get; set; } // Description or purpose of the category



        public string ChartOfAccountId { get; set; } // Owner of the category (e.g., branch, zone,head office)

        public string ChartOfAccountLabelled { get; set; }

        public ICollection<Transaction> Transactions { get; set; }

        public virtual ChartOfAccount Account { get; set; }

        public object CreateBudgetCategory()
        {
            return new {  CategoryName = CategoryName, Description = Description, ChartOfAccountId = ChartOfAccountId };


        }
    }


}
