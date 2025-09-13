 
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class OperationEvent 
    {
        
        public string Id { get; set; }
        [Required]
        public string OperationEventName { get; set; }
        [Required]
        public string EventCode { get; set; }

        public string OperationEventType { get; set; }
        public bool HasMultipleEntries { get; set; }
        [Required]
        public string Description { get; set; }

        public OperationEvent()
        {
            
        }
    }
}
