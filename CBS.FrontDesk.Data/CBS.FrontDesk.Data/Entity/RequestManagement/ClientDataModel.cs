using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.RequestManagement
{
    public class ClientDataModel
    {
        public string IPAddress { get; set; }
        public string Browser { get; set; }
        public string BrowserVersion { get; set; }
        public string OS { get; set; }
        public int ScreenWidth { get; set; }
        public int ScreenHeight { get; set; }
        public string Location { get; set; }
        public string UserAgent { get; set; }
        public string DataType { get; set; }
    }
}
