using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Language
{
    public class LocalizationController : Controller
    {
        public ActionResult SetLanguage(string lang)
        {
            if (!string.IsNullOrEmpty(lang))
            {
                // Set session for UI logic
                Session["SelectedLanguage"] = lang;

                // Set cookie for request culture
                HttpCookie cookie = new HttpCookie("TSC_Lang", lang)
                {
                    Expires = DateTime.Now.AddYears(1)
                };
                Response.Cookies.Add(cookie);
            }

            string returnUrl = Request.UrlReferrer?.ToString() ?? "/";
            return Redirect(returnUrl);
        }
    }

}