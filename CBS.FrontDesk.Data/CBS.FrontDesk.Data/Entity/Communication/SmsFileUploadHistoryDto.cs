using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Communication
{
    public class SmsFileUploadHistoryDto
    {
        public string Id { get; set; }

        // Upload metadata
        public string FileUploadId { get; set; }
        public string FileCode { get; set; }

        // Row tracking
        public int RowNumber { get; set; }

        // SMS content
        public string SenderService { get; set; }
        public string Title { get; set; }
        public string CustomerName { get; set; }
        public string Recipient { get; set; }
        public string MessageBody { get; set; }
        public string MemberReference { get; set; }
        public string Purpose { get; set; }

        // Notification outcome
        public string NotificationId { get; set; }
        public string NotificationType { get; set; }
        public string NotificationStatus { get; set; }
        public DateTime NotificationTimestamp { get; set; }

        // Branch context (for reporting)
        public string BankID { get; set; }
        public string BranchID { get; set; }
        public string BranchName { get; set; }

        // Execution audit
        public bool IsSentSuccessfully { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime SentOn { get; set; }
        public string SentBy { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
    }
}
