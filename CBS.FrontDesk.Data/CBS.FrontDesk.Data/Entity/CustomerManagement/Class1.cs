using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CBS.FrontDesk.Data.Entity.CustomerManagement
{
    public class CustomerDocument
    {
        public string Id { get; set; }
        public string UrlPath { get; set; }
        public string DocumentName { get; set; }
        public string Extension { get; set; }
        public string CustomerId { get; set; }
    }
    public class CustomerDocumentRequest
    {
        [Required]
        public string CustomerID { get; set; }
        [Required]
        public List<HttpPostedFileBase> AttachedFiles { get; set; }
    }
}
