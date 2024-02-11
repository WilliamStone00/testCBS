using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class OperationEventAttribute
    {
        public string Id { get; set; }
        [Required]
        public string Name { get; set; } //Fee,Pripal/Commission
        [Required]
        public string Description { get; set; }
     
        public string OperationEventId { get; set; }

    }
}
