using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CBS.FrontDesk.Data.Entity.CustomerManagement
{
    public class DocumentUploadResponse
    {
        public string Id { get; set; }
        public string UrlPath { get; set; }
        public string DocumentName { get; set; }
        public string Extension { get; set; }
        public string BaseUrl { get; set; }
        public string OperationId { get; set; }
        public string FullPath { get; set; }
        public string ServiceType { get; set; }
        public bool ResponseFromClientService { get; set; }
        public string DocumentType { get; set; }
    }
    public class CustomerDocumentRequest
    {
        [Required]
        public string CustomerID { get; set; }
        [Required]
        public List<HttpPostedFileBase> AttachedFiles { get; set; }
        [Required]
        public string DocumentType { get; set; }
        public string ServiceTypeType { get; set; }
    }
}
