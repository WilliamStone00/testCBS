using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CheckManagementSystem.Configurations.NotificationConfig
{
    public class NotificationConfig
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsCentralized { get; set; } = false;

        public string BranchId { get; set; }

        // This will come from the NotificationTypeDefinition's 'Value'
        [Required]
        public string NotificationType { get; set; }

        public string Description { get; set; }

        [Required(ErrorMessage = "The template body cannot be empty.")]
        public string TemplateBody { get; set; }

        public bool IsActive { get; set; } = true;
    }

    // using Newtonsoft.Json;
    public class VerifyNotificationConfigRequest
    {
        [JsonProperty("isCentralised")]
        public bool IsCentralised { get; set; }

        [JsonProperty("branchId")]
        public string BranchId { get; set; }

        [JsonProperty("notificationType")]
        public string NotificationType { get; set; }
    }

}
