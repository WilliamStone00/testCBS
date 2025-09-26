using CBS.FrontDesk.Data.Entity.CheckManagementSystem.LossManagementSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CheckManagementSystem.LossManagementSystem
{
    public class CustomerCheckbooksDto
    {
        public string CustomerId { get; set; }
        public List<CheckbookDto> Checkbooks { get; set; }
    }
}
