using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Language
{
    public class LanguageController : Controller
    {
        // GET: Language
        public ActionResult SetLanguage(string lang)
        {
            // Store selected language in session
            Session["SelectedLanguage"] = lang;

            // Optional: Set Thread culture
            Thread.CurrentThread.CurrentCulture = new CultureInfo(lang);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(lang);

            return new EmptyResult();
        }
    }
}