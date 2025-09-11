using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CustomerManagement.Grouping
{
    public class GroupCustomer
    {
        public string GroupCustomerId { get; set; }
        public bool IsGroupLeader { get; set; }
        public DateTime DateOfJoining { get; set; }
        public string GroupId { get; set; }
        public string CustomerId { get; set; }
        public IndividualProfile Customer { get; set; }
        public Group Group { get; set; }
    }
}
