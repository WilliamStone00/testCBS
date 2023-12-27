using CBS.FrontDesk.Data.Entity.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CBS.FrontDesk.Data.UserManagement
{
    public class UserList: User
    {
        public Guid id { get; set; }
        public string profilePhoto { get; set; }
        public string provider { get; set; }
        public string status { get; set; }
        public string name { get; set; }
        public string roleName { get; set; }
        public Bank Bank { get; set; }
        public Branch Branch { get; set; }
        public DateTime lastLoginDate { get; set; }
        public string strlastLoginDate { get; set; }
        public DateTime createdDate { get; set; }
        public ChangePassword ChangePassword { get; set; }=new ChangePassword();
        public List<UserClaim> userClaims { get; set; }
        public HttpPostedFileBase FileUpload { get; set; }
        public string ImageVirtualPath { get; set; }

        public UserList()
        {
            ImageVirtualPath = "~/Appfiles/Images/p.jpg";
        }
    }
}
