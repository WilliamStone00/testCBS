using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CheckManagementSystem.LossManagementSystem
{
    public class CheckLeafDto
    {
        public string Id { get; set; }
        public string CheckBookId { get; set; }
        public int SerialNumber { get; set; }
        public string Status { get; set; }
        public string CreatedDate { get; set; }
    }

}
