using CBS.FrontDesk.Data.Entity.DataTable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Accounting_V2.CollectorDevice
{
    public class CollectorDeviceresponse
    {
        public string Id { get; set; }
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public string DeviceVersion { get; set; }
        public string DeviceSerialNumber { get; set; }
        public int Status { get; set; }
        public string AssignedCollectorUserId { get; set; }
        public DateTime? AssignedOn { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
   
}
