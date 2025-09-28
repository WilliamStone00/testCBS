using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CheckManagementSystem.LossManagementSystem
{
    public class LossRequestDto
    {
        public string CustomerID { get; set; }
        public string CheckLeafID { get; set; }
        public string CheckBookID { get; set; }
        public string BranchID { get; set; }
        public string AccountID { get; set; }
        public string NationalIDNumber { get; set; }
        public string LossReporteBy { get; set; }
        public DateTime LossDate { get; set; }
            
        public string LossReason { get; set; }
        public string LossStatus { get; set; }

    }
}
