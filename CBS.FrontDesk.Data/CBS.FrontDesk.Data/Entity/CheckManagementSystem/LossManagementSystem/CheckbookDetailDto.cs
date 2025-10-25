using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CheckManagementSystem.LossManagementSystem
{
    public class CheckbookDetailDto 
    {
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public string AccountId { get; set; }
        public string AccountNumber { get; set; }
        public string BranchId { get; set; }
        public string BankId { get; set; }
        public int NumberOfLeaves { get; set; }
        public int StartSerialNumber { get; set; }
        public int EndSerialNumber { get; set; }
        public int CurrentSerialNumber { get; set; }
        public string Status { get; set; }
        public string CreatedDate { get; set; }
        public string LastUpdatedDate { get; set; }
        public string ApprovedBy { get; set; }
        public string CheckBookCategoryId { get; set; }

        // 👇 This matches "leaves": [ ... ] in your JSON
        public List<CheckLeafDto> Leaves { get; set; }
    }
}
