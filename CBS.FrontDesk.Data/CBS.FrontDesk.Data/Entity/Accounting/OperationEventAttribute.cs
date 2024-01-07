using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class OperationEventAttribute
    {
        public string Id { get; set; }
        public string Name { get; set; } //Fee,Pripal/Commission
        public string OperationEventId { get; set; }

    }
}
