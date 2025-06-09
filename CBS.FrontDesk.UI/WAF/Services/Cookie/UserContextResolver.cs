using CBS.FrontDesk.Data.Entity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace CBS.FrontDesk.UI.WAF.Services.Cookie
{
    /// <summary>
    /// <b>UserContextResolver</b> is responsible for extracting authenticated user details
    /// from the request's authentication cookie (TSC). It deserializes the cookie's `UserData` payload
    /// into a strongly typed model and returns user identity and branch metadata.
    /// 
    /// Used by WAF and audit layers to understand user context for logging, policy enforcement, or IP tracking.
    /// </summary>
    public class UserContextResolver
    {
        /// <summary>
        /// Resolves the current user context from the HTTP request by inspecting the TSC authentication cookie.
        /// </summary>
        /// <param name="request">The current HTTP request</param>
        /// <returns>
        /// A tuple containing: username, branchId, branchCode, branchName, phone, fullName, and isAuthenticated flag.
        /// Returns anonymous defaults if cookie is missing, malformed, or expired.
        /// </returns>
        public (
            string username,
            string branchId,
            string branchCode,
            string branchName,
            string phone,
            string fullName,
            bool isAuthenticated
        ) Resolve(HttpRequest request)
        {
            try
            {
                // 🔐 Read the "TSC" authentication cookie from the request
                var authCookie = request.Cookies["TSC"];
                if (authCookie == null)
                    return ("anonymous", "", "", "", "", "", false);

                // 🔓 Decrypt the forms authentication ticket
                var ticket = FormsAuthentication.Decrypt(authCookie.Value);
                if (ticket == null || ticket.Expired)
                    return ("anonymous", "", "", "", "", "", false);

                // 📦 Deserialize the custom UserData payload into your expected model
                var userData = JsonConvert.DeserializeObject<CustomSerializeModel>(ticket.UserData);
                if (userData == null || string.IsNullOrWhiteSpace(userData.UserName))
                    return ("anonymous", "", "", "", "", "", false);

                // ✅ Return extracted user info
                return (
                    userData.UserName,
                    userData.BranchId ?? "",
                    userData.BranchCode ?? "",
                    userData.BranchName ?? "",
                    userData.Phonenumber ?? "",
                    userData.FullName ?? "",
                    true
                );
            }
            catch
            {
                // ❌ On any error, return safe anonymous values
                return ("anonymous", "", "", "", "", "", false);
            }
        }
    }

}