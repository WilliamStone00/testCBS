using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CheckManagementSystem.LossManagementSystem
{
    public class CheckLeafDto
    {
        public int CheckLeafId { get; set; }
        public string Status { get; set; }
        public int CheckBookId { get; set; }
    }

}
