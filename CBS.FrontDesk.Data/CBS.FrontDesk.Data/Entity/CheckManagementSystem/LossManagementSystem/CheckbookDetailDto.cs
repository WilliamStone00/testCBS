using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CheckManagementSystem.LossManagementSystem
{
    public class CheckbookDetailDto : CheckbookDto
    {
        public string CustomerId { get; set; }
        public string AccountNumber { get; set; }
        public string BranchCode { get; set; }
        public List<CheckLeafDto> CheckLeaves { get; set; }
    }
}
