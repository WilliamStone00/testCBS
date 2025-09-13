using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CBS.FrontDesk.UI.Helper
{
    public static class LanguageHelper
    {
        public static List<KeyValuePair<string, string>> GetLanguages()
        {
            return new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("en", "English"),
            new KeyValuePair<string, string>("fr", "French")
        };
        }
    }

}