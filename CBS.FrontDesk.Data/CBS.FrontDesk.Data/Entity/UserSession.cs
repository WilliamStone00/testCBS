using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CBS.FrontDesk.Data.Entity.User;

namespace CBS.FrontDesk.Data.Entity
{
    public class UserSession
    {
        public SessionObject SessionObject { get; set; }
        public string UserID { get; set; }
        public UserDto User { get; set; }

        public DateTime SessionDate { get; set; }
        public TimeSpan SessionTime { get; set; }
        //public List<MainMenu> MainMenus { get; set; }
        //public List<SubMenu> SubMenus { get; set; }
        //public List<GroupMenu> GroupMenus { get; set; }
        //public List<SubMainMenu> AllSubMainMenus { get; set; }
        //public List<MainMenu> AllMainmenus { get; set; }
        //public List<SubMenu> AllSubmenus { get; set; }

        public UserSession()
        {
            //MainMenus = new List<MainMenu>();
            //SubMenus = new List<SubMenu>();
            //GroupMenus = new List<GroupMenu>();
            SessionObject = new SessionObject();
            User = new UserDto();
            //AllSubMainMenus = new List<SubMainMenu>();
            //AllMainmenus = new List<MainMenu>();
            //AllSubmenus = new List<SubMenu>();

        }

    }
    public class SessionObject
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string GroupID { get; set; }
        public string GroupName { get; set; }
        public string UserID { get; set; }
        public string BankID { get; set; }
        public string SessionID { get; set; }
        public string BankName { get; set; }
        public string BankCode { get; set; }


    }
}
