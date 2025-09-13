using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CustomerManagement.Grouping
{
    public class GroupType
    {
        public string GroupTypeId { get; set; }
        public string GroupTypeName { get; set; }
        public string Description { get; set; }
        public string Action { get; set; }
        public string ServiceOption { get; set; }
    }
}
