using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.BusinessService.Session
{
    public static class SessionHelper
    {
        private const string ReturnUrlKey = "ReturnUrl";

        public static void SaveWithExpiration<T>(string key, T value, TimeSpan expirationTime)
        {
            if (HttpContext.Current == null || value == null)
                return;

            // Store the object along with the current timestamp in the session
            var sessionData = new
            {
                Value = value,
                TimeStamp = DateTime.Now
            };

            HttpContext.Current.Session[key] = sessionData;

            // Set the expiration time for the session item
            HttpContext.Current.Session.Timeout = (int)expirationTime.TotalMinutes;
        }

        public static T Retrieve<T>(string key)
        {
            if (HttpContext.Current == null || HttpContext.Current.Session[key] == null)
                return default(T);

            var sessionData = HttpContext.Current.Session[key] as dynamic;

            if (sessionData == null || sessionData.TimeStamp == null)
                return default(T);

            // Check the expiration time and return the stored value if it's still valid
            DateTime storedTime = sessionData.TimeStamp;
            TimeSpan elapsedTime = DateTime.Now - storedTime;

            if (elapsedTime <= TimeSpan.FromMinutes(HttpContext.Current.Session.Timeout))
            {
                return sessionData.Value;
            }
            else
            {
                // If the session has expired, remove the item from the session
                HttpContext.Current.Session.Remove(key);
                return default(T);
            }
        }
        public static bool Exists(string key)
        {
            return HttpContext.Current != null && HttpContext.Current.Session[key] != null;
        }
        public static RedirectResult RedirectToLogin(string redirectUrl = "~/Authentication/Login")
        {
            HttpContext.Current.Response.Redirect(redirectUrl);
            return new RedirectResult(redirectUrl);
        }


        public static void SetReturnUrl(string returnUrl, HttpSessionStateBase session)
        {
            session[ReturnUrlKey] = returnUrl;
        }

        public static string GetReturnUrl(HttpSessionStateBase session)
        {
            return session[ReturnUrlKey] as string;
        }
    }

}
