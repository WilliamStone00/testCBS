using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CheckManagementSystem.Configurations.NotificationHistory
{
    // This DTO perfectly matches the JSON object from your backend
    public class NotificationHistoryDto
    {
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public string BranchId { get; set; }
        public string NotificationType { get; set; }
        public string Content { get; set; }
        public DateTime SentDate { get; set; }
        // We can add non-database properties for display
        public string CustomerName { get; set; } // This would be populated by the service
        public string BranchName { get; set; }   // This would be populated by the service
    }
}
