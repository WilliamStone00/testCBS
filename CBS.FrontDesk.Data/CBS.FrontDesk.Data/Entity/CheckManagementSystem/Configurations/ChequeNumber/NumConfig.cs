using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CheckManagementSystem.ChequeNumber
{
    public class NumConfig 
    {
        public string Id { get; set; } = null;
        public string BranchId { get; set; }
        public string Name { get; set; } = null; // ex: "Default Format"
        public bool IsActive { get; set; } = true;
        public List<VariablePosition> Positions { get; set; } = new List<VariablePosition>();
    }
}
