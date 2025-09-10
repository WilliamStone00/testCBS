using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
  

    public class AccountClass
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string AccountCategoryId { get; set; }
    }
}
