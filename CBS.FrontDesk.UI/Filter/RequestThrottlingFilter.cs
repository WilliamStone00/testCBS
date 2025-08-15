using System;
using System.Runtime.Caching;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Net;
using CBS.FrontDesk.Data.UserManagement;
using CBS.BusinessService.Session;
using Azure;
using CBS.FrontDesk.Service;
using System.Web;
using System.Web.Security;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity;
using System.Collections.Generic;
using System.Linq;

namespace CBS.FrontDesk.UI.Filter
{


    public class RequestThrottlingFilter : ActionFilterAttribute
    {


        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var request = filterContext.HttpContext.Request;
            string ipAddress = request.UserHostAddress;
            var actionName = filterContext.ActionDescriptor.ActionName.ToLower();
            var controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName.ToLower();
            var requestUrl = request.RawUrl.ToLower();
            // ✅ Skip access checks for blocked page to prevent redirect loop
            if (requestUrl.Contains("/error/blocked") || requestUrl.Contains("/authentication/login"))
                return; // ✅ prevent middleware from interfering
            bool isAuthenticated = filterContext.HttpContext.User.Identity.IsAuthenticated;
            if (!isAuthenticated && filterContext.HttpContext.Session["UserID"] != null)
            {
                // User is authenticated but session was missing and is now rehydrated
                isAuthenticated = true;
            }
            bool isAjaxRequest = request.IsAjaxRequest();
            // 🌐 Check Internet Connectivity (Exclude NoInternet page to prevent redirection loop)
            if (!actionName.Equals("NoInternet", StringComparison.OrdinalIgnoreCase) &&
                !controllerName.Equals("Home", StringComparison.OrdinalIgnoreCase))
            {
                if (!IsInternetAvailable())
                {
                    filterContext.Result = new RedirectResult("~/Home/NoInternet");
                    return;
                }
            }
            
            try
            {


                // ✅ INTERNET CHECK
                if (!actionName.Equals("nointernet") && !actionName.Equals("index") && !controllerName.Equals("home"))
                {
                    if (!IsInternetAvailable())
                    {
                        filterContext.Result = new RedirectResult("~/Home/NoInternet");
                        return;
                    }
                }

                // ✅ AUTHENTICATION CHECK
                if (filterContext.HttpContext.Session["UserID"] == null)
                {
                    if (isAuthenticated)
                    {
                        if (!isAjaxRequest)
                        {
                            GetUserSession(filterContext);
                            if (filterContext.HttpContext.Session["UserID"] == null)
                            {
                                filterContext.Result = new RedirectResult("~/Authentication/Login");
                                return;
                            }
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    if (!requestUrl.Contains("/authentication/resolvemultiplesessions") && !requestUrl.Contains("/authentication/login"))
                    {
                        GetUserSession(filterContext);
                    }
                    else
                    {
                        return;
                    }
               
                }

                // ✅ MFA ENFORCEMENT
                if (isAuthenticated && VerifyIfSessionExist("MFA", filterContext))
                {
                    if (!requestUrl.Contains("/mfaverification") && !requestUrl.Contains("/authentication/logout"))
                    {
                        string mfaUrl = filterContext.HttpContext.Session["MFAUrl"]?.ToString();
                        if (!string.IsNullOrEmpty(mfaUrl))
                        {
                            filterContext.Result = new RedirectResult(mfaUrl);
                            return;
                        }
                    }
                }

                // ✅ PASSWORD CHANGE ENFORCEMENT
                if (isAuthenticated && VerifyIfSessionExist("PWD", filterContext))
                {
                    if (!requestUrl.Contains("/usermanagement/floginchangepassword") && !requestUrl.Contains("/authentication/logout"))
                    {
                        string pwdUrl = filterContext.HttpContext.Session["CHPWDUrl"]?.ToString();
                        if (!string.IsNullOrEmpty(pwdUrl))
                        {
                            filterContext.Result = new RedirectResult(pwdUrl);
                            return;
                        }
                    }
                }

                // ✅ IP ADDRESS CHECK
                var userIp = ipAddress;
                if (filterContext.HttpContext.Session["UserIP"] != null && filterContext.HttpContext.Session["UserIP"].ToString() != userIp)
                {
                    filterContext.HttpContext.Session.Abandon();
                    filterContext.Result = new RedirectResult("~/Authentication/Logout");
                    return;
                }
                else
                {
                    filterContext.HttpContext.Session["UserIP"] = userIp;
                }

                // ✅ PREVENT ACCESS TO LOGIN PAGE FOR AUTHENTICATED USERS
                if (isAuthenticated && controllerName.Equals("authentication") && actionName.Equals("login"))
                {
                    filterContext.Result = new RedirectResult("~/Home/Index");
                    return;
                }
            }
            catch (Exception ex)
            {
                // ✅ Handle exception and log the error
                // LogException(ex);
                filterContext.Result = new HttpStatusCodeResult(500, "Internal Server Error");
            }

            base.OnActionExecuting(filterContext);
        }

        public bool VerifyIfSessionExist(string sessionName, ActionExecutingContext filterContext)
        {
            return filterContext.HttpContext.Session[sessionName] != null;
        }

        private bool IsInternetAvailable()
        {
            try
            {
                using (var client = new WebClient())
                {
                    using (client.OpenRead("https://www.youtube.com/"))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }
        public void GetUserSession(ActionExecutingContext filterContext)
        {
            var request = filterContext.HttpContext.Request;
            var response = filterContext.HttpContext.Response;

            string currentPath = request.Path.ToLower();
            string resolvedFlag = request.QueryString["resolved"]?.ToLower();

            System.Diagnostics.Debug.WriteLine($"🌐 Path: {currentPath}, Resolved: {resolvedFlag}");

            // ✅ 1. Bypass session checks on Login or ResolveMultipleSessions pages
            if (currentPath.Contains("/authentication/login") ||
                currentPath.Contains("/authentication/resolvemultiplesessions"))
            {
                System.Diagnostics.Debug.WriteLine($"🚫 Skipping session check on path: {currentPath}");
                return;
            }

            // ✅ 2. Avoid session logic if HttpContext or Session is null
            if (HttpContext.Current == null || HttpContext.Current.Session == null)
            {
                System.Diagnostics.Debug.WriteLine("⚠️ HttpContext or Session is null. Skipping session rehydration.");
                return;
            }

            // ✅ 3. Skip if user is not authenticated
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                System.Diagnostics.Debug.WriteLine("⛔ User not authenticated. Skipping session check.");
                return;
            }

            bool isAjaxRequest = request.IsAjaxRequest();
            var identity = filterContext.HttpContext.User as CustomPrincipal;

            // ✅ 4. Trace identity state
            if (identity == null)
            {
                System.Diagnostics.Debug.WriteLine("❌ identity is null.");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"🔍 Identity info: UserName={identity.UserName}, SessionCode={identity.SessionCode}");
            }

            // ✅ 5. Handle invalid or empty identity info
            if (identity == null || string.IsNullOrWhiteSpace(identity.SessionCode) || string.IsNullOrWhiteSpace(identity.UserName))
            {
                System.Diagnostics.Debug.WriteLine("❌ Invalid identity. Logging out...");
                if (!isAjaxRequest)
                {
                    PerformLogoutAsync();
                    filterContext.Result = new RedirectResult("~/Authentication/Login");
                }
                return;
            }

            try
            {
                var _userManagementServices = new LocalSession();
                var userSession = _userManagementServices.GetUserCurrentsession(identity.SessionCode, identity.UserName);

                // ✅ 6. Handle missing or invalid session
                if (userSession == null || string.Equals(userSession.SessionStatus, "Invalid", StringComparison.OrdinalIgnoreCase))
                {
                    System.Diagnostics.Debug.WriteLine("❌ No session or invalid. Logging out...");
                    if (!isAjaxRequest)
                    {
                        PerformLogoutAsync();
                        filterContext.Result = new RedirectResult("~/Authentication/Login");
                    }
                    return;
                }

                // ✅ 7. Handle multiple sessions and redirect
                if (userSession.SessionStatus == "Multiple_Sessions")
                {
                    string redirectUrl = $"~/Authentication/ResolveMultipleSessions" +
                                         $"?username={HttpUtility.UrlEncode(userSession.UserName)}" +
                                         $"&message={HttpUtility.UrlEncode(userSession.ErrorMessage)}" +
                                         $"&count={userSession.NumberOfSessionsOpen}" +
                                         $"&resolved=false";

                    System.Diagnostics.Debug.WriteLine($"🔁 REDIRECTING to: {redirectUrl}");

                    filterContext.Result = new RedirectResult(redirectUrl);
                    return;
                }

                // ✅ 8. Valid session - build local session
                System.Diagnostics.Debug.WriteLine("✅ Valid session. Proceeding...");
                BuildLocalSession(userSession.UserAuthDto, filterContext);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❗ Exception in GetUserSession: {ex.Message}");

                if (!isAjaxRequest)
                {
                    PerformLogoutAsync();
                    filterContext.Result = new RedirectResult("~/Authentication/Login");
                }
            }
        }
        protected Task PerformLogoutAsync()
        {
            // Sign out from Forms Authentication
            FormsAuthentication.SignOut();

            var cookieNames = new[]
            {
                "BranchObject", "AuthUser", "CBS4U", "CBS4U_MFA", "MFA", "PWD",
                "ASP.NET_SessionId", "EncryptedJWToken", "TSC"
            };

            // Expire all specified cookies
            foreach (var cookieName in cookieNames)
            {
                if (HttpContext.Current.Request.Cookies[cookieName] != null)
                {
                    var expiredCookie = new HttpCookie(cookieName)
                    {
                        Expires = DateTime.Now.AddYears(-1),
                        Value = string.Empty,
                        HttpOnly = true
                    };
                    HttpContext.Current.Response.Cookies.Add(expiredCookie);
                }
            }

            // Clear session
            HttpContext.Current.Session.Clear();
            HttpContext.Current.Session.RemoveAll();
            HttpContext.Current.Session.Abandon();

            // ✅ Explicitly clear the user in the controller context
            HttpContext.Current.User = null;

            // ✅ Also clear the user in the global context (for background services or utility methods)
            if (System.Web.HttpContext.Current != null)
            {
                System.Web.HttpContext.Current.User = null;
            }

            return Task.CompletedTask;
        }

        public void BuildLocalSession(UserDto userSession,ActionExecutingContext filterContext)
        {
            var roles = userSession.Roles.Select(role => role.RoleName).ToArray();
            var roleid = userSession.Roles.Select(role => role.RoleId)?.FirstOrDefault();

            string fullName = userSession.firstName + " " + userSession.lastName;

            HttpContext.Current.Session["FullName"] = fullName;
            HttpContext.Current.Session["Msisdn"] = userSession.phoneNumber;
            HttpContext.Current.Session["Email"] = userSession.email;
            HttpContext.Current.Session["Initial"] = userSession.firstName?.FirstOrDefault().ToString().ToUpper() ?? "U";

            HttpContext.Current.Session["UserID"] = userSession.id;
            HttpContext.Current.Session["Roles"] = roles;
            HttpContext.Current.Session["RoleId"] = roleid;
            HttpContext.Current.Session["RefesherToken"] = userSession.refreshToken;
            HttpContext.Current.Session["Token"] = userSession.bearerToken;
            HttpContext.Current.Session["UserName"] = userSession.userName;
            HttpContext.Current.Session["Photo"] = userSession.profilePhoto ?? "No image";
            HttpContext.Current.Session["SessionIP"] = userSession.SessionIP;
            HttpContext.Current.Session["SessionUserAgent"] = userSession.SessionUserAgent;

            HttpContext.Current.Session["LogoUrl"] = userSession.Branch.Bank?.LogoUrl ?? userSession.Branch.ImageVirtualPath;

            HttpContext.Current.Session["SessionCode"] = userSession.SessionCode;
            HttpContext.Current.Session["SessionId"] = userSession.SessionId;
            HttpContext.Current.Session["BranchID"] = userSession.BranchID;
            HttpContext.Current.Session["OrganizationID"] = userSession.Bank.OrganizationId;
            HttpContext.Current.Session["BankID"] = userSession.Branch.BankId;
            HttpContext.Current.Session["BankName"] = userSession.Branch.Bank.Name;
            HttpContext.Current.Session["BranchName"] = userSession.Branch.Name;
            HttpContext.Current.Session["BranchCode"] = userSession.Branch.BranchCode;
            HttpContext.Current.Session["IsHavingBank"] = userSession.Branch.IsHavingBank;
            HttpContext.Current.Session["BankCode"] = userSession.Branch.Bank.BankCode;
            HttpContext.Current.Session["IsHeadOffice"] = userSession.Branch.IsHeadOffice;

            HttpContext.Current.Session["menu"] = userSession.PermissionNodes?.ToList() ?? new List<PermissionNode>();
            HttpContext.Current.Items["MenuItems"] = HttpContext.Current.Session["menu"];

            HttpContext.Current.Session["EncryptedJWToken"] = TokenEncryptionHelper.EncryptToken(userSession.bearerToken);
            HttpContext.Current.Session["BranchObject"] = userSession.Branch;
            HttpContext.Current.Session["AuthUser"] = userSession;
            HttpContext.Current.Session["UserIP"] = userSession.SessionIP;

            // Menu Handling
            var menuJson = Newtonsoft.Json.JsonConvert.SerializeObject(userSession.PermissionNodes);
            HttpContext.Current.Session["menu"] = menuJson;

            // ✅ Assign to ViewBag via filterContext
            if (filterContext.Controller is Controller controller)
            {
                controller.ViewBag.MenuItems = string.IsNullOrEmpty(menuJson)
                                               ? new List<PermissionNode>()
                                               : Newtonsoft.Json.JsonConvert.DeserializeObject<List<PermissionNode>>(menuJson);
            }

            // 🌐 Set language in session and cookie
            var selectedLang = !string.IsNullOrWhiteSpace(userSession.UserPreferedLanguage)
                ? userSession.UserPreferedLanguage.ToLower()
                : "en";

            HttpContext.Current.Session["SelectedLanguage"] = selectedLang;

            // 🍪 Persist language selection in cookie for 1 year
            var langCookie = new HttpCookie("TSC_Lang", selectedLang)
            {
                Expires = DateTime.Now.AddYears(1)
            };
            HttpContext.Current.Response.Cookies.Add(langCookie);

            // ✅ Log session initialization
            System.Diagnostics.Debug.WriteLine($"Session initialized for UserID: {userSession.id}, IP: {userSession.SessionIP}");
        }
    }

    //using CBS.BusinessService.RequestLoggerServicesP;
    //using System;
    //using System.Threading.Tasks;
    //using System.Web.Mvc;

    //using System;
    //using System.Threading.Tasks;
    //using System.Web.Mvc;

    //public class RequestThrottlingFilter : ActionFilterAttribute
    //{
    //    private const int REQUEST_LIMIT = 50;
    //    private static readonly TimeSpan TIME_WINDOW = TimeSpan.FromSeconds(20);
    //    private readonly RequestLogger _requestLogger = new RequestLogger();

    //    public override async void OnActionExecuting(ActionExecutingContext filterContext)
    //    {
    //        var request = filterContext.HttpContext.Request;

    //        // ✅ Exclude AJAX Requests
    //        if (request.IsAjaxRequest())
    //        {
    //            base.OnActionExecuting(filterContext);
    //            return;
    //        }

    //        string ipAddress = request.UserHostAddress;
    //        string requestUrl = request.RawUrl;

    //        // ✅ Check if IP is blocked
    //        if (await _requestLogger.IsBlockedAsync(ipAddress))
    //        {
    //            filterContext.Result = new HttpStatusCodeResult(429, "Too Many Requests");
    //            return;
    //        }

    //        // ✅ Log the request
    //        await _requestLogger.LogRequestAsync(ipAddress, requestUrl);

    //        // ✅ Check the request count
    //        int requestCount = await _requestLogger.GetRequestCountAsync(ipAddress, TIME_WINDOW);
    //        if (requestCount > REQUEST_LIMIT)
    //        {
    //            await _requestLogger.BlockIpAsync(ipAddress);
    //            filterContext.Result = new HttpStatusCodeResult(429, "Too Many Requests");
    //            return;
    //        }

    //        base.OnActionExecuting(filterContext);
    //    }
    //}
}