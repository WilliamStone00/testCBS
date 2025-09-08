using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.ManualDailycollection
{
    public class CollectorDto
    {       
        public string CustomerId { get; set; }       
        public string FullName { get; set; }     
        public string UserId { get; set; }
        public string BranchId { get; set; }
    }



}
