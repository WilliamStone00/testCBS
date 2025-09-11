using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CBS.FrontDesk.UI.Helper
{
    public static class JsonMVC
    {
        public static string Serialize(object data)
        {
            return JsonConvert.SerializeObject(data);
        }
    }
}