using CBS.FrontDesk.Data.Entity.DataTable;
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
        public string Id { get; set; } = null;

        public string Name { get; set; } = null;

        public bool IsCentralized { get; set; } = true;

        public string BranchId { get; set; } = null;

        [Required]
        public string NotificationType { get; set; } = null;

        public string Description { get; set; } = null;

        public string Purpose { get; set; } = null;

        public string AvailablePlaceholders { get; set; } = null;

        [Required(ErrorMessage = "The template body cannot be empty.")]
        public string TemplateBody { get; set; } = null;

        public bool IsActive { get; set; } = false;

        public string TriggerEvent { get; set; } = null;

        public string ScheduleCron { get; set; } = null;

        public int? DaysBeforeExpiry { get; set; } = null;

        public string ConditionExpression { get; set; } = null;

        public string Audience { get; set; } = null;

        public string Channel { get; set; } = null;

        public bool RequiresApproval { get; set; } = false;
    }

    public class NotificationconfigQuery
    {
        public DataTableOptions Options { get; set; }
        public NotificationconfigQuery() { Options = new DataTableOptions(); }

        public string BranchId { get; set; } = null;
        public string BranchName { get; set; } = null;
        public bool? IsCentralised { get; set; }
        public bool? NotificationType { get; set; } 
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class NotificationConfigDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string NotificationType { get; set; }
        public string Channel { get; set; }
        public bool IsCentralised { get; set; }
        public string BranchId { get; set; }
        public string BranchName { get; set; }
        public string Audience { get; set; }
        public string TargetRole { get; set; }
        public string NotificationTemplate { get; set; }
        public string EmailSubject { get; set; }
        public string Language { get; set; }
        public string LocalizedTemplates { get; set; }
        public string Description { get; set; }
        public string Purpose { get; set; }
        public string TriggerEvent { get; set; }
        public string ScheduleCron { get; set; }
        public int DaysBeforeExpiry { get; set; }
        public string ConditionExpression { get; set; }
        public bool IsActive { get; set; }
        public bool IsDefault { get; set; }
        public int Priority { get; set; }
        public int DelayInMinutes { get; set; }
        public int MaxNotificationsPerDay { get; set; }
        public int CooldownPeriodInHours { get; set; }
        public bool? AllowDuplicate { get; set; }
        public bool? RequiresApproval { get; set; }
        public string ApprovedBy { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public int TotalSent { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public DateTime? LastSentDate { get; set; }
        public string AvailablePlaceholders { get; set; }
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
