using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
   
    public class OperationEventDto
    {

        public string OperationEventName { get; set; }

        public string Description { get; set; }
        public string EventCode { get; set; }
        public bool HasMultipleEntries { get; set; }
    }
}
