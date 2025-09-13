using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CBS.FrontDesk.Data.Entity.CustomerManagement.Grouping
{
    public class GroupDocument
    {
        public string GroupDocumentId { get; set; }
        public string UrlPath { get; set; }
        public string DocumentName { get; set; }
        public string Extension { get; set; }
        public string BaseUrl { get; set; }
        public string DocumentType { get; set; }
        public string GroupId { get; set; }
        public List<HttpPostedFileBase> AttachedFiles { get; set; }

        public virtual Group Group { get; set; }
    }
}
