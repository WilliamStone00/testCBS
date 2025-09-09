using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.CheckManagementSystem.Configurations.NotificationConfig
{
    public class NotificationType
    {
        /// <summary>
        /// The unique identifier for the type (e.g., "ChequeCashed").
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The user-friendly display name (e.g., "Cheque Cashed Notification").
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// A brief explanation of when this notification is triggered.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// A list of available placeholders for this specific notification type.
        /// </summary>
        public List<string> Placeholders { get; set; } = new List<string>();
    }
}
