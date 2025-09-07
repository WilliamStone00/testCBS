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
        [JsonProperty("customerId")]
        public string CustomerId { get; set; }

        [JsonProperty("fullName")]
        public string FullName { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }

        // Optional
        [JsonProperty("branchId")]
        public string BranchId { get; set; }
    }



}
