using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.Communication
{
    public class SmsFileUploadDto
    {
        public string Id { get; set; } // Unique Identifier for the file
        public string FileName { get; set; } // Original file name
        public string FilePath { get; set; } // Path where the file is stored
        public string FileHash { get; set; } // Unique hash to prevent duplicate uploads
        public string BranchId { get; set; } // Branch Id associated with the upload
        public string BranchName { get; set; } // Branch name associated with the upload
        public string BranchCode { get; set; } // Branch code associated with the upload
        public string UploadedBy { get; set; } // User who uploaded the file
        public DateTime UploadedOn { get; set; } // Timestamp of when the file was uploaded
        public string FileUploadId { get; set; } // Reference id for file tracking
        public string FileCategory { get; set; }
        public string Status { get; set; }
        public string FileCode { get; set; }
        public string UserId { get; set; }
        public string FileType { get; set; }
        public string MessageTemplate { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string Purpose { get; set; }
        public string SenderService { get; set; }
        public string Title { get; set; }
    }
}
