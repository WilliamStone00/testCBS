using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CheckManagementSystem.Operations.ChequeCancelation
{
    public class CancellationValidatiion
    {
        public string Id { get; set; }          // or int, depending on your data type
        public string Mode { get; set; }
        public string ApprovedBy { get; set; }
        public string Statement { get; set; }
    }
}
