using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CBS.FrontDesk.Data
{

    public class FrontEndAuditTB
    {
        [Key]
        public string UsersAuditID { get; set; }
        public string UserName { get; set; }
        public string UserRole { get; set; } // New property
        public string SessionID { get; set; }
        public string IPAddress { get; set; }
        public string PageAccessed { get; set; }
        public DateTime LoggedInAt { get; set; }
        public DateTime LoggedOutAt { get; set; }
        public string LoginStatus { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public string RequestMethod { get; set; } // New property
        public string UserAgent { get; set; } // New property
        public string Referer { get; set; } // New property
        public string SerializedRequestData { get; set; }
        public string BranchName { get; set; }
        public string AjaxPostData { get; set; }
    }


}