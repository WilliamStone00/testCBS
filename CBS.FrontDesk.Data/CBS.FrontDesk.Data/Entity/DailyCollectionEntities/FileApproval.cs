using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CBS.FrontDesk.Data.Entity.DailyCollectionEntities
{
    public class FileApproval
    {
        [JsonProperty("fileUploadId")]
        public string FileUploadId { get; set; }

        [JsonProperty("reviewerStatement")]
        [MaxLength(500)]
        public string ReviewerStatement { get; set; }

        [JsonProperty("approvedBy")]
        [Required]
        [MaxLength(100)]
        public string ApprovedBy { get; set; }

        [JsonProperty("approvalStatement")]
        [MaxLength(500)]
        public string ApprovalStatement { get; set; }
    }
   
}
