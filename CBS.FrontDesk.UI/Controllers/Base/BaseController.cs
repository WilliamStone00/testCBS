using CBS.FrontDesk.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.DataTable;
using System.Linq;
using CBS.FrontDesk.Helper;
using CBS.API.Helper;
using System.IdentityModel.Tokens.Jwt;
using CBS.FrontDesk.Data.Entity.Accounting;
using DocumentFormat.OpenXml.EMMA;
using System.Threading.Tasks;
using CBS.BusinessService.UserManagement;
using ClosedXML.Excel;
using CBS.BusinessService.Session;
using CBS.FrontDesk.Data.UserManagement;
using System.Globalization;

namespace CBS.FrontDesk.UI.Controllers
{

    public class BaseController : Controller
    {

        private string domain = ConfigurationManager.AppSettings["domain"];
        private string timetoExpire = ConfigurationManager.AppSettings["timetoExpire"];
        private readonly LocalSession _userManagementServices;
        //protected override void OnActionExecutingxxxx(ActionExecutingContext filterContext)
        //{
        //    try
        //    {
        //        var actionName = filterContext.ActionDescriptor.ActionName;
        //        var controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
        //        var requestUrl = filterContext.HttpContext.Request.RawUrl.ToLower();

        //        // 🌐 Check Internet Connectivity (Exclude NoInternet page to prevent redirection loop)
        //        if (!actionName.Equals("NoInternet", StringComparison.OrdinalIgnoreCase) &&
        //            !controllerName.Equals("Home", StringComparison.OrdinalIgnoreCase))
        //        {
        //            if (!IsInternetAvailable())
        //            {
        //                filterContext.Result = new RedirectResult("~/Home/NoInternet");
        //                return;
        //            }
        //        }

        //        // 🔒 Redirect to Login if user is not authenticated and not on login page
        //        if (!requestUrl.StartsWith("/authentication/login") && Session["UserID"] == null)
        //        {
        //            filterContext.Result = new RedirectResult("~/Authentication/Login");
        //            return;
        //        }

        //        // 👤 Load user session for authenticated users
        //        if (User.Identity.IsAuthenticated && !actionName.Equals("Login", StringComparison.OrdinalIgnoreCase))
        //        {
        //            GetUserSession();
        //        }

        //        // ✅ MFA enforcement
        //        if (User.Identity.IsAuthenticated && VerifyIfSessionExist("MFA"))
        //        {
        //            if (!requestUrl.Contains("/MFAVerification") &&
        //                !requestUrl.Contains("/Authentication/Logout"))
        //            {
        //                string mfaUrl = Session["MFAUrl"]?.ToString();
        //                if (!string.IsNullOrEmpty(mfaUrl))
        //                {
        //                    filterContext.Result = new RedirectResult(mfaUrl);
        //                    return;
        //                }
        //            }
        //        }

        //        // ✅ Password Change enforcement
        //        if (User.Identity.IsAuthenticated && VerifyIfSessionExist("PWD"))
        //        {
        //            if (!requestUrl.Contains("/UserManagement/FLoginChangePassword") &&
        //                !requestUrl.Contains("/Authentication/Logout"))
        //            {
        //                string pwdUrl = Session["CHPWDUrl"]?.ToString();
        //                if (!string.IsNullOrEmpty(pwdUrl))
        //                {
        //                    filterContext.Result = new RedirectResult(pwdUrl);
        //                    return;
        //                }
        //            }
        //        }

        //        // ⚠️ IP Address check for session hijacking
        //        var userIp = Request.UserHostAddress;
        //        if (Session["UserIP"] != null && Session["UserIP"].ToString() != userIp)
        //        {
        //            Session.Abandon();
        //            filterContext.Result = new RedirectResult("~/Authentication/Logout");
        //            return;
        //        }
        //        else
        //        {
        //            Session["UserIP"] = userIp;
        //        }

        //        // 🚫 Prevent authenticated users from accessing Login page again
        //        if (User.Identity.IsAuthenticated &&
        //            controllerName.Equals("Authentication", StringComparison.OrdinalIgnoreCase) &&
        //            actionName.Equals("Login", StringComparison.OrdinalIgnoreCase))
        //        {
        //            filterContext.Result = new RedirectResult("~/Home/Index");
        //            return;
        //        }

        //        // ✅ Continue with action
        //        base.OnActionExecuting(filterContext);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Optional: Handle exception and log error
        //        filterContext.Result = new RedirectResult("~/Home/Error");
        //    }
        //}

        //protected override void OnActionExecuting(ActionExecutingContext filterContext)
        //{
        //    try
        //    {
        //        // 🌐 Check Internet Connectivity
        //        if (!IsInternetAvailable())
        //        {
        //            // Handle offline scenario
        //            filterContext.Result = new RedirectResult("~/Home/NoInternet"); // Redirect to a "No Internet" page or handle it appropriately
        //            return;
        //        }

        //        var request = filterContext.HttpContext.Request;
        //        var url = request.RawUrl;
        //        var controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
        //        var actionName = filterContext.ActionDescriptor.ActionName;

        //        // 🔒 Redirect to Login if user is not authenticated and not on login page
        //        if (!url.StartsWith("/Authentication/Login", StringComparison.OrdinalIgnoreCase) &&
        //            Session["UserID"] == null)
        //        {
        //            filterContext.Result = new RedirectResult("~/Authentication/Login");
        //            return;
        //        }

        //        // ✅ MFA enforcement
        //        if (User.Identity.IsAuthenticated && VerifyIfSessionExist("MFA"))
        //        {
        //            if (!url.Contains("/MFAVerification?serviceoption=MFA") &&
        //                url != "/Authentication/Logout" &&
        //                url != "/MFAVerification/MFACodeVerification")
        //            {
        //                string mfaUrl = Session["MFAUrl"]?.ToString();
        //                if (!string.IsNullOrEmpty(mfaUrl))
        //                {
        //                    filterContext.Result = new RedirectResult(mfaUrl);
        //                    return;
        //                }
        //            }
        //        }

        //        // ✅ Password Change enforcement
        //        if (User.Identity.IsAuthenticated && VerifyIfSessionExist("PWD"))
        //        {
        //            if (!url.Contains("/UserManagement/FLoginChangePassword?serviceoption=USER") &&
        //                url != "/Authentication/Logout" &&
        //                url != "/UserManagement/FLoginChangePassword")
        //            {
        //                string pwdUrl = Session["CHPWDUrl"]?.ToString();
        //                if (!string.IsNullOrEmpty(pwdUrl))
        //                {
        //                    filterContext.Result = new RedirectResult(pwdUrl);
        //                    return;
        //                }
        //            }
        //        }

        //        // ⚠️ IP Address check for session hijacking
        //        var userIp = Request.UserHostAddress;
        //        if (Session["UserIP"] != null && Session["UserIP"].ToString() != userIp)
        //        {
        //            Session.Abandon();
        //            Session.RemoveAll();
        //            filterContext.Result = new RedirectResult("~/Authentication/Logout");
        //            return;
        //        }
        //        else
        //        {
        //            Session["UserIP"] = userIp;
        //        }

        //        // 👤 Load user session if not in Login/Logout/ResolveMultipleSessions
        //        if (!actionName.Equals("ResolveMultipleSessions", StringComparison.OrdinalIgnoreCase) &&
        //            !actionName.Equals("Login", StringComparison.OrdinalIgnoreCase) &&
        //            !actionName.Equals("Logout", StringComparison.OrdinalIgnoreCase))
        //        {
        //            GetUserSession();
        //        }

        //        // 🚫 Prevent authenticated users from accessing Login page again
        //        if (User.Identity.IsAuthenticated &&
        //            controllerName.Equals("Authentication", StringComparison.OrdinalIgnoreCase) &&
        //            (actionName.Equals("Login", StringComparison.OrdinalIgnoreCase) ||
        //             actionName.Equals("Index", StringComparison.OrdinalIgnoreCase)))
        //        {
        //            filterContext.Result = new RedirectResult("~/Home/Index");
        //            return;
        //        }

        //        // ✅ Continue with action
        //        base.OnActionExecuting(filterContext);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception (optional) and handle the error
        //    }
        //}

        //////protected override void OnActionExecuting(ActionExecutingContext filterContext)
        //////{
        //////    try
        //////    {
        //////        var actionName = filterContext.ActionDescriptor.ActionName.ToLower();
        //////        var controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName.ToLower();
        //////        var requestUrl = filterContext.HttpContext.Request.RawUrl.ToLower();
        //////        bool isAuthenticated = User.Identity.IsAuthenticated;
        //////        bool isAjaxRequest = filterContext.HttpContext.Request.IsAjaxRequest();

        //////        // ✅ 1. INTERNET CHECK (Exclude NoInternet and Home/Index to prevent looping)
        //////        if (!actionName.Equals("nointernet") && !actionName.Equals("index") && !controllerName.Equals("home"))
        //////        {
        //////            if (!IsInternetAvailable())
        //////            {
        //////                filterContext.Result = new RedirectResult("~/Home/NoInternet");
        //////                return;
        //////            }
        //////        }

        //////        // ✅ 2. AUTHENTICATION CHECK
        //////        if (Session["UserID"] == null)
        //////        {
        //////            if (isAuthenticated)
        //////            {
        //////                // If not AJAX, attempt to rebuild session
        //////                if (!isAjaxRequest)
        //////                {
        //////                    GetUserSession();
        //////                    // Recheck the session after rebuilding
        //////                    if (Session["UserID"] == null)
        //////                    {
        //////                        filterContext.Result = new RedirectResult("~/Authentication/Login");
        //////                        return;
        //////                    }
        //////                }


        //////            }
        //////            else
        //////            {
        //////                return;
        //////            }
        //////        }
        //////        else
        //////        {
        //////            // ✅ 3. SESSION VALIDATION (Rebuilding session if required)
        //////            if (isAuthenticated && !isAjaxRequest)
        //////            {
        //////                GetUserSession();
        //////            }
        //////        }

        //////        // ✅ 4. MFA ENFORCEMENT
        //////        if (isAuthenticated && VerifyIfSessionExist("MFA"))
        //////        {
        //////            if (!requestUrl.Contains("/mfaverification") && !requestUrl.Contains("/authentication/logout"))
        //////            {
        //////                string mfaUrl = Session["MFAUrl"]?.ToString();
        //////                if (!string.IsNullOrEmpty(mfaUrl))
        //////                {
        //////                    filterContext.Result = new RedirectResult(mfaUrl);
        //////                    return;
        //////                }
        //////            }
        //////        }

        //////        // ✅ 5. PASSWORD CHANGE ENFORCEMENT
        //////        if (isAuthenticated && VerifyIfSessionExist("PWD"))
        //////        {
        //////            if (!requestUrl.Contains("/usermanagement/floginchangepassword") && !requestUrl.Contains("/authentication/logout"))
        //////            {
        //////                string pwdUrl = Session["CHPWDUrl"]?.ToString();
        //////                if (!string.IsNullOrEmpty(pwdUrl))
        //////                {
        //////                    filterContext.Result = new RedirectResult(pwdUrl);
        //////                    return;
        //////                }
        //////            }
        //////        }

        //////        // ✅ 6. IP ADDRESS CHECK (Session Hijacking Prevention)
        //////        var userIp = Request.UserHostAddress;
        //////        if (Session["UserIP"] != null && Session["UserIP"].ToString() != userIp)
        //////        {
        //////            Session.Abandon();
        //////            filterContext.Result = new RedirectResult("~/Authentication/Logout");
        //////            return;
        //////        }
        //////        else
        //////        {
        //////            Session["UserIP"] = userIp;
        //////        }

        //////        // ✅ 7. PREVENT ACCESS TO LOGIN PAGE FOR AUTHENTICATED USERS
        //////        if (isAuthenticated &&
        //////            controllerName.Equals("authentication") && actionName.Equals("login"))
        //////        {
        //////            filterContext.Result = new RedirectResult("~/Home/Index");
        //////            return;
        //////        }
        //////    }
        //////    catch (Exception ex)
        //////    {
        //////        // Handle exception and log the error
        //////        // filterContext.Result = new RedirectResult("~/Home/Error");
        //////        // LogException(ex); // Implement logging as needed
        //////    }

        //////    base.OnActionExecuting(filterContext);
        //////}


        //protected override void OnActionExecuting(ActionExecutingContext filterContext)
        //{
        //    try
        //    {
        //        var request = filterContext.HttpContext.Request;
        //        var url = request.RawUrl;
        //        var controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
        //        var actionName = filterContext.ActionDescriptor.ActionName;
        //        if (!actionName.Equals("NoInternet", StringComparison.OrdinalIgnoreCase) &&
        //        !actionName.Equals("Index", StringComparison.OrdinalIgnoreCase) &&
        //        !controllerName.Equals("Home", StringComparison.OrdinalIgnoreCase))
        //                    {
        //            if (!IsInternetAvailable())
        //            {
        //                filterContext.Result = new RedirectResult("~/Home/NoInternet");
        //                return;
        //            }
        //        }


        //        // ✅ MFA enforcement
        //        if (User.Identity.IsAuthenticated && VerifyIfSessionExist("MFA"))
        //        {
        //            if (!url.Contains("/MFAVerification?serviceoption=MFA") &&
        //                url != "/Authentication/Logout" &&
        //                url != "/MFAVerification/MFACodeVerification")
        //            {
        //                string mfaUrl = Session["MFAUrl"]?.ToString();
        //                if (!string.IsNullOrEmpty(mfaUrl))
        //                {
        //                    filterContext.Result = new RedirectResult(mfaUrl);
        //                    return;
        //                }
        //            }
        //        }

        //        // ✅ Password Change enforcement
        //        if (User.Identity.IsAuthenticated && VerifyIfSessionExist("PWD"))
        //        {
        //            if (!url.Contains("/UserManagement/FLoginChangePassword?serviceoption=USER") &&
        //                url != "/Authentication/Logout" &&
        //                url != "/UserManagement/FLoginChangePassword")
        //            {
        //                string pwdUrl = Session["CHPWDUrl"]?.ToString();
        //                if (!string.IsNullOrEmpty(pwdUrl))
        //                {
        //                    filterContext.Result = new RedirectResult(pwdUrl);
        //                    return;
        //                }
        //            }
        //        }

        //        // ⚠️ IP Address check for session hijacking
        //        var userIp = Request.UserHostAddress;
        //        if (Session["UserIP"] != null && Session["UserIP"].ToString() != userIp)
        //        {
        //            Session.Abandon();
        //            Session.RemoveAll();
        //            filterContext.Result = new RedirectResult("~/Authentication/Logout");
        //            return;
        //        }
        //        else
        //        {
        //            Session["UserIP"] = userIp;
        //        }

        //        // 👤 Load user session if not in Login/Logout/ResolveMultipleSessions
        //        if (!actionName.Equals("ResolveMultipleSessions", StringComparison.OrdinalIgnoreCase) &&
        //            !actionName.Equals("Login", StringComparison.OrdinalIgnoreCase) &&
        //            !actionName.Equals("Logout", StringComparison.OrdinalIgnoreCase))
        //        {
        //            GetUserSession();
        //        }

        //        // 🚫 Prevent authenticated users from accessing Login page again
        //        if (User.Identity.IsAuthenticated &&
        //            controllerName.Equals("Authentication", StringComparison.OrdinalIgnoreCase) &&
        //            (actionName.Equals("Login", StringComparison.OrdinalIgnoreCase) ||
        //             actionName.Equals("Index", StringComparison.OrdinalIgnoreCase)))
        //        {
        //            if (!actionName.Equals("ResolveMultipleSessions", StringComparison.OrdinalIgnoreCase))
        //            {
        //                GetUserSession();
        //            }

        //            filterContext.Result = new RedirectResult("~/Home/Index");
        //            return;
        //        }

        //        // ✅ Continue with action
        //        base.OnActionExecuting(filterContext);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception (optional) and rethrow for centralized handling
        //        // throw(ex);
        //    }
        //}

        private bool IsInternetAvailable()
        {
            try
            {
                using (var client = new WebClient())
                {
                    using (client.OpenRead("http://www.google.com"))
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

        protected CultureInfo GetUserCultureInfo()
        {
            var lang = Session["SelectedLanguage"]?.ToString()?.ToLower();

            switch (lang)
            {
                case "en":
                    return new CultureInfo("en-US");
                case "fr":
                    return new CultureInfo("fr-FR");
                default:
                    return new CultureInfo("en-US"); // fallback
            }
        }


        private static readonly HashSet<string> WordList = new HashSet<string>
        {
            "LION", "TREE", "MOON", "STAR", "WOLF", "FIRE", "ROCK", "SKY", "BIRD", "CLOUD", "TSC"
        };

        public bool IsSessionCodeStructurallyValid(string sessionCode)
        {
            if (string.IsNullOrWhiteSpace(sessionCode))
                return false;

            var parts = sessionCode.ToUpper().Split('-');
            if (parts.Length != 3)
                return false;

            var prefix = parts[0];
            var digits = parts[1];
            var suffix = parts[2];

            return WordList.Contains(prefix)
                && WordList.Contains(suffix)
                && digits.Length == 3
                && digits.All(char.IsDigit);
        }
        protected JsonResult JsonValidationErrorsssssResponse()
        {
            // GetAllowAnonymous all the validation errors from ModelState
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            // Combine all the errors into a single string
            var errorMessage = string.Join("; ", errors);

            // Return the JSON response with the combined error message
            return Json(new { success = false, status = false, message = errorMessage });
        }
        protected JsonResult JsonValidationErrorResponse()
        {
            // GetAllowAnonymous all the validation errors from ModelState
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            // Combine all the errors into a single string
            var errorMessage = string.Join("; ", errors);

            // Return the JSON response with the combined error message
            return Json(new { success = false, status = false, message = errorMessage });
        }
        public bool VerifyIfSessionExist(string sessionName)
        {
            if (Session == null || string.IsNullOrEmpty(sessionName))
            {
                return false; // Session or session Name is null
            }

            var sessionValue = Session[sessionName];

            if (sessionValue != null)
            {
                return true; // Session exists
            }

            return false; // Session does not exist
        }



        public UserDto GetUserDto()
        {
            // Assuming 'UserDto' is your class
            var userAuth = HttpContext.Session["AuthUser"] as UserDto;

            if (userAuth != null)
            {
                return userAuth;
            }
            return null;

        }

        public Task<List<System.Web.WebPages.Html.SelectListItem>> GetBookingDirections()
        {
            var bookingDirections = new System.Web.WebPages.Html.SelectListItem[] {
                new System.Web.WebPages.Html.SelectListItem { Text = "DEBIT", Value = "DEBIT" },
                new System.Web.WebPages.Html.SelectListItem { Text = "CREDIT", Value = "CREDIT" },
                new System.Web.WebPages.Html.SelectListItem { Text = "NOT", Value = "DEFINE" }
            }.ToList();
            return Task.FromResult(bookingDirections);
        }
        public static bool IsTokenExpired(string token)
        {
            try
            {
                if (token == null)
                    return true;
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

                if (jsonToken == null)
                    return true;

                var expiryTime = jsonToken.ValidTo;
                if (expiryTime < DateTime.Now)
                {

                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                // Token is invalid or expired
                return true;
            }
        }



        public void CreateToken(UserDto reqDto, string cookieName = "TSC", int minutes_to_live = 60)
        {
            if (Membership.ValidateUser(reqDto.userName, ""))
            {
                var user = new CustomMembershipUser(reqDto);
                string[] roles = { reqDto.Roles.FirstOrDefault().RoleName };
                CustomSerializeModel userModel = new CustomSerializeModel()
                {
                    Id = user.UserID,
                    UserName = reqDto.userName,
                    RoleName = roles,
                    SessionIP=reqDto.SessionIP,
                    SessionUserAgent=reqDto.SessionUserAgent,
                    FullName = $"{reqDto.firstName} {reqDto.lastName}",
                    Email = user.Email,
                    BranchCode=reqDto.Branch.BranchCode,
                    BranchId=reqDto.BranchID,
                    BranchName=reqDto.Branch.Name,
                    Phonenumber = user.Phonenumber,
                    SessionID = reqDto.SessionId,
                    SessionCode = reqDto.SessionCode,
                };
                Session["SessionStartTime"] = DateTime.UtcNow;
                Session["SessionMaxLifetimeMinutes"] = minutes_to_live; // Example: 4 hours
                string userData = JsonConvert.SerializeObject(userModel);
                FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(1, reqDto.SessionId, DateTime.Now, DateTime.Now.AddHours(minutes_to_live), false, userData);
                string enTicket = FormsAuthentication.Encrypt(authTicket);
                HttpCookie faCookie = new HttpCookie(cookieName, enTicket);
                Response.Cookies.Add(faCookie);
                BuildLocalSession(reqDto);
            }

        }

        public void RemoveSessionName(string sessionName)
        {
            HttpContext.Session.Remove(sessionName); // Removing the MFA token
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
                if (Request.Cookies[cookieName] != null)
                {
                    var expiredCookie = new HttpCookie(cookieName)
                    {
                        Expires = DateTime.Now.AddYears(-1),
                        Value = string.Empty,
                        HttpOnly = true
                    };
                    Response.Cookies.Add(expiredCookie);
                }
            }

            // Clear session
            Session.Clear();
            Session.RemoveAll();
            Session.Abandon();

            // ✅ Explicitly clear the user in the controller context
            HttpContext.User = null;

            // ✅ Also clear the user in the global context (for background services or utility methods)
            if (System.Web.HttpContext.Current != null)
            {
                System.Web.HttpContext.Current.User = null;
            }

            return Task.CompletedTask;
        }

        public void GetUserSession()
        {
            if (!User.Identity.IsAuthenticated) return;

            bool isAjaxRequest = Request.IsAjaxRequest();
            var _userManagementServices = new LocalSession();
            var identity = HttpContext.User as CustomPrincipal;

            if (identity == null || string.IsNullOrWhiteSpace(identity.SessionCode) || string.IsNullOrWhiteSpace(identity.UserName))
            {
                if (!isAjaxRequest)
                {
                    Response.Redirect("~/Authentication/Login", true);
                }
                return;
            }

            var userSession = new UserSessionDto();

            if (IsInternetAvailable())
            {
                userSession = _userManagementServices.GetUserCurrentsession(identity.SessionCode, identity.UserName);
                if (userSession.SessionStatus == "Multiple_Sessions")
                {
                    string redirectUrl = $"~/Authentication/ResolveMultipleSessions?username={HttpUtility.UrlEncode(userSession.UserName)}" +
                                         $"&message={HttpUtility.UrlEncode(userSession.ErrorMessage)}" +
                                         $"&count={userSession.NumberOfSessionsOpen}";
                    Response.Redirect(redirectUrl, true);
                    return;
                }
                if (userSession == null || string.Equals(userSession.SessionStatus, "Invalid", StringComparison.OrdinalIgnoreCase))
                {
                    PerformLogoutAsync(); // 👈 Shared logout method
                    Response.Redirect("~/Authentication/Login", true);
                    return;
                }
                BuildLocalSession(userSession.UserAuthDto);

            }

            // ✅ Valid single session
            BuildLocalSession(userSession.UserAuthDto);

        }





        protected void SetMenuFromSession()
        {
            var menus = Session["menu"] as List<PermissionNode> ?? new List<PermissionNode>();
            ViewBag.MenuItems = menus.OrderBy(x => x.Menu.MenuOrder).ToList();
        }

        public DataTableOptions GetDataTableOptions()
        {
            var queryParams = HttpContext.Request.QueryString;

            // Ensure draw is not null or empty, default to "1" if missing
            var draw = !string.IsNullOrWhiteSpace(queryParams["draw"]) ? queryParams["draw"] : "1";

            // Retrieve the sort column name and provide a default (e.g., "Timestamp") if missing
            var sortColumnIndex = queryParams["order[0][column]"];
            var sortColumnName = !string.IsNullOrWhiteSpace(sortColumnIndex) && queryParams[$"columns[{sortColumnIndex}][name]"] != null
                ? queryParams[$"columns[{sortColumnIndex}][name]"]
                : "Timestamp";  // Default column if not provided

            // Default sort direction to "asc" if missing or invalid
            var sortDirection = queryParams["order[0][dir]"]?.ToLower() == "desc" ? "desc" : "asc";

            DataTableOptions dataTableOptions = new DataTableOptions
            {
                draw = draw,
                start = int.TryParse(queryParams["start"], out var start) ? start : 0,
                length = int.TryParse(queryParams["length"], out var length) ? length : 10,
                sortColumnName = sortColumnName,
                sortColumnDirection = sortDirection,
                searchValue = queryParams["search[value]"] ?? string.Empty,
                sortDirection = sortDirection
            };

            dataTableOptions.pageSize = dataTableOptions.length;
            dataTableOptions.skip = dataTableOptions.start;
            dataTableOptions.recordsTotal = 0;

            return dataTableOptions;
        }



        public DataTableOptions PostDataTableOptions()
        {
            var form = HttpContext.Request.Form;

            DataTableOptions dataTableOptions = new DataTableOptions
            {
                draw = form["draw"],
                start = int.TryParse(form["start"], out var start) ? start : 0,
                length = int.TryParse(form["length"], out var length) ? length : 10,
                sortColumnName = form[$"columns[{form["order[0][column]"]}][Name]"],
                sortColumnDirection = form["order[0][dir]"],
                searchValue = form["search[value]"] ?? string.Empty,
                sortDirection = form["order[0][dir]"],
            };

            dataTableOptions.pageSize = dataTableOptions.length;
            dataTableOptions.skip = dataTableOptions.start;
            dataTableOptions.recordsTotal = 0;

            return dataTableOptions;
        }


        public void BuildLocalSession(UserDto userSession)
        {
            var roles = userSession.Roles.Select(role => role.RoleName).ToArray();
            var roleid = userSession.Roles.Select(role => role.RoleId)?.FirstOrDefault();

            string fullName = userSession.firstName + " " + userSession.lastName;

            Session["FullName"] = fullName;
            Session["Msisdn"] = userSession.phoneNumber;
            Session["Email"] = userSession.email;
            Session["Initial"] = userSession.firstName?.FirstOrDefault().ToString().ToUpper() ?? "U";
            Session["UserID"] = userSession.id;
            Session["Roles"] = roles;
            Session["RoleId"] = roleid;
            Session["RefesherToken"] = userSession.refreshToken;
            Session["Token"] = userSession.bearerToken;
            Session["UserName"] = userSession.userName;
            Session["Photo"] = userSession.profilePhoto ?? "No image";
            Session["SessionIP"] = userSession.SessionIP;
            Session["SessionUserAgent"] = userSession.SessionUserAgent;

            Session["LogoUrl"] = userSession.Branch.Bank?.LogoUrl ?? userSession.Branch.ImageVirtualPath;

            Session["SessionCode"] = userSession.SessionCode;
            Session["SessionId"] = userSession.SessionId;
            Session["BranchID"] = userSession.BranchID;
            Session["OrganizationID"] = userSession.Bank.OrganizationId;
            Session["BankID"] = userSession.Branch.BankId;
            Session["BankName"] = userSession.Branch.Bank.Name;
            Session["BranchName"] = userSession.Branch.Name;
            Session["BranchCode"] = userSession.Branch.BranchCode;
            Session["IsHavingBank"] = userSession.Branch.IsHavingBank;
            Session["BankCode"] = userSession.Branch.Bank.BankCode;
            Session["IsHeadOffice"] = userSession.Branch.IsHeadOffice;

            Session["menu"] = userSession.PermissionNodes?.ToList() ?? new List<PermissionNode>();
            ViewBag.MenuItems = Session["menu"];

            Session["EncryptedJWToken"] = TokenEncryptionHelper.EncryptToken(userSession.bearerToken);
            Session["BranchObject"] = userSession.Branch;
            Session["AuthUser"] = userSession;
            Session["UserIP"] = userSession.SessionIP;

            // 🌐 Set language in session and cookie
            var selectedLang = !string.IsNullOrWhiteSpace(userSession.UserPreferedLanguage)
                ? userSession.UserPreferedLanguage.ToLower()
                : "en";

            Session["SelectedLanguage"] = selectedLang;

            // 🍪 Persist language selection in cookie for 1 year
            var langCookie = new HttpCookie("TSC_Lang", selectedLang)
            {
                Expires = DateTime.Now.AddYears(1)
            };
            Response.Cookies.Add(langCookie);
        }

        [NonAction]
        public void SendVerificationCode(string email, string code, string username)
        {



            var fromEmail = new MailAddress("enzyrio212@gmail.com", "CASH-HUB");
            var toEmail = new MailAddress(email);

            var fromEmailPassword = "Johnson1";
            string subject = "Verification Code!";

            string body = "<b>Hello " + username.ToUpper() + "</b><br/> Please use this code to verify your account." + "<br/>ACCOUNT VERIFICATION CODE: " + code + "";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true

            })

                smtp.Send(message);

        }

        public void SendMail(UserDto user, string smstype)
        {
            try
            {
                string body = "<b>Dear " + user.firstName.ToUpper() + "</b><br/>Your momokash account was successfully created." + "<br/>User Name: <b>" + user.userName + "</b><br/>Password: <b>" + user.refreshToken + "</b><b><br/><a href=" + domain + " class='btn-gradient-primary' style='font-size: 14px; text-decoration: none'>Login to portal</a></b>";
                string subject = "MOMOKASH ACCOUNT CREATION";
                if (smstype == "Registration")
                {
                    body = "<b>Dear " + user.firstName.ToUpper() + "</b><br/>Your momokash account was successfully created." + "<br/>User Name: <b>" + user.userName + "</b><br/>Password: <b>" + user.refreshToken + "</b><b><br/><a href=" + domain + " class='btn-gradient-primary' style='font-size: 14px; text-decoration: none'>Login to portal</a></b>";
                }
                else if (smstype == "Reset")
                {
                    string data = Session["FullName"].ToString();
                    subject = "MOMOKASH PASSWORD RESET";
                    body = "<b>Dear " + user.firstName.ToUpper() + "</b><br/>Your momokash account was successfully reseted by " + data.ToUpper() + "\n" + "<br/>User Name: <b>" + user.userName + "</b><br/>New password: <b>" + user.refreshToken + "</b><b><br/><a href=" + domain + " class='btn-gradient-primary' style='font-size: 14px; text-decoration: none'>Login to portal using new password</a></b>";
                }

                SmtpSection smtpSection = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");
                var fromEmail = new MailAddress(smtpSection.From, "MOMOKASH");
                var toEmail = new MailAddress(user.email, user.firstName);
                using (MailMessage mm = new MailMessage(fromEmail, toEmail))
                {
                    mm.Subject = subject;
                    mm.Body = body;
                    mm.IsBodyHtml = true;

                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = smtpSection.Network.Host;
                    smtp.EnableSsl = smtpSection.Network.EnableSsl;
                    NetworkCredential networkCred = new NetworkCredential(smtpSection.Network.UserName, smtpSection.Network.Password);
                    smtp.UseDefaultCredentials = smtpSection.Network.DefaultCredentials;
                    smtp.Credentials = networkCred;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Port = smtpSection.Network.Port;

                    smtp.Send(mm);
                }
            }
            catch (Exception ex)
            {

                //throw ex;
            }

        }



    }
}