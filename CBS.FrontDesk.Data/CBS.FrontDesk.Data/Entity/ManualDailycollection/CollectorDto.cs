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
        [JsonProperty("collectorId")]
        public string CollectorId { get; set; }

        [JsonProperty("collectorName")]
        public string CollectorName { get; set; }

        [JsonProperty("user")]
        public UserDto User { get; set; }
    }

    public class UserDto
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; }
    }
}
