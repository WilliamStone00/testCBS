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

namespace CBS.FrontDesk.UI.Controllers
{

    public class BaseController : Controller
    {

        private string domain = ConfigurationManager.AppSettings["domain"];
        private string timetoExpire = ConfigurationManager.AppSettings["timetoExpire"];
        private readonly LocalSession _userManagementServices;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                var request = filterContext.HttpContext.Request;
                var url = request.RawUrl;
                var controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
                var actionName = filterContext.ActionDescriptor.ActionName;

                // 🔒 Redirect to Login if user is not authenticated and not on login page
                //if (!url.StartsWith("/Authentication/Login", StringComparison.OrdinalIgnoreCase) &&
                //    Session["UserID"] == null)
                //{
                //    filterContext.Result = new RedirectResult("~/Authentication/Login");
                //    return;
                //}

                // ✅ MFA enforcement
                if (User.Identity.IsAuthenticated && VerifyIfSessionExist("MFA"))
                {
                    if (!url.Contains("/MFAVerification?serviceoption=MFA") &&
                        url != "/Authentication/Logout" &&
                        url != "/MFAVerification/MFACodeVerification")
                    {
                        string mfaUrl = Session["MFAUrl"]?.ToString();
                        if (!string.IsNullOrEmpty(mfaUrl))
                        {
                            filterContext.Result = new RedirectResult(mfaUrl);
                            return;
                        }
                    }
                }

                // ✅ Password Change enforcement
                if (User.Identity.IsAuthenticated && VerifyIfSessionExist("PWD"))
                {
                    if (!url.Contains("/UserManagement/FLoginChangePassword?serviceoption=USER") &&
                        url != "/Authentication/Logout" &&
                        url != "/UserManagement/FLoginChangePassword")
                    {
                        string pwdUrl = Session["CHPWDUrl"]?.ToString();
                        if (!string.IsNullOrEmpty(pwdUrl))
                        {
                            filterContext.Result = new RedirectResult(pwdUrl);
                            return;
                        }
                    }
                }

                // ⚠️ IP Address check for session hijacking
                var userIp = Request.UserHostAddress;
                if (Session["UserIP"] != null && Session["UserIP"].ToString() != userIp)
                {
                    Session.Abandon();
                    Session.RemoveAll();
                    filterContext.Result = new RedirectResult("~/Authentication/Logout");
                    return;
                }
                else
                {
                    Session["UserIP"] = userIp;
                }

                // 👤 Load user session if not in Login/Logout/ResolveMultipleSessions
                if (!actionName.Equals("ResolveMultipleSessions", StringComparison.OrdinalIgnoreCase) &&
                    !actionName.Equals("Login", StringComparison.OrdinalIgnoreCase) &&
                    !actionName.Equals("Logout", StringComparison.OrdinalIgnoreCase))
                {
                    GetUserSession();
                }

                // 🚫 Prevent authenticated users from accessing Login page again
                if (User.Identity.IsAuthenticated &&
                    controllerName.Equals("Authentication", StringComparison.OrdinalIgnoreCase) &&
                    (actionName.Equals("Login", StringComparison.OrdinalIgnoreCase) ||
                     actionName.Equals("Index", StringComparison.OrdinalIgnoreCase)))
                {
                    if (!actionName.Equals("ResolveMultipleSessions", StringComparison.OrdinalIgnoreCase))
                    {
                        GetUserSession();
                    }

                    filterContext.Result = new RedirectResult("~/Home/Index");
                    return;
                }

                // ✅ Continue with action
                base.OnActionExecuting(filterContext);
            }
            catch (Exception ex)
            {
                // Log the exception (optional) and rethrow for centralized handling
                throw;
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
                    FullName = user.FullName,
                    Email = user.Email,
                    Phonenumber = user.Phonenumber,
                    SessionID = reqDto.SessionId,
                    SessionCode=reqDto.SessionCode,
                };
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
            FormsAuthentication.SignOut();

            // List of cookies to clear
            var cookieNames = new[]
            {
                "BranchObject", "AuthUser", "CBS4U", "CBS4U_MFA", "MFA", "PWD",
                "ASP.NET_SessionId", "EncryptedJWToken","TSC"
            };


            foreach (var cookieName in cookieNames)
            {
                if (Request.Cookies[cookieName] != null)
                {
                    var cookie = new HttpCookie(cookieName)
                    {
                        Expires = DateTime.Now.AddYears(-1),
                        Value = string.Empty
                    };
                    Response.Cookies.Add(cookie);
                }
            }

            Session.Clear();
            Session.Abandon();
            Session.RemoveAll();
            Session.Remove("EncryptedJWToken");
            return Task.CompletedTask;
        }

        public void GetUserSession()
        {
            if (!User.Identity.IsAuthenticated) return;

            if (VerifyIfSessionExist("MFA")) { }
            if (VerifyIfSessionExist("PWD")) { }

            var _userManagementServices = new LocalSession();
            var identity = HttpContext.User as CustomPrincipal;
            if (identity == null || string.IsNullOrWhiteSpace(identity.SessionCode) || string.IsNullOrWhiteSpace(identity.UserName))
            {
                Response.Redirect("~/Authentication/Login", true);
                return;
            }
            var userSession = _userManagementServices.GetUserCurrentsession(identity.SessionCode, identity.UserName);

            if (userSession == null || string.Equals(userSession.SessionStatus, "Invalid", StringComparison.OrdinalIgnoreCase))
            {
                PerformLogoutAsync(); // 👈 Shared logout method
                Response.Redirect("~/Authentication/Login", true);
                return;
            }

            if (userSession.SessionStatus == "Multiple_Sessions")
            {
                string redirectUrl = $"~/Authentication/ResolveMultipleSessions?username={HttpUtility.UrlEncode(userSession.UserName)}" +
                                     $"&message={HttpUtility.UrlEncode(userSession.ErrorMessage)}" +
                                     $"&count={userSession.NumberOfSessionsOpen}";
                Response.Redirect(redirectUrl, true);
                return;
            }
            BuildLocalSession(userSession.UserAuthDto);
            // ✅ Valid single session
            SetMenuFromSession();
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

            Session["FullName"] = userSession.firstName + " " + userSession.lastName;
            Session["Msisdn"] = userSession.phoneNumber;
            Session["Email"] = userSession.email;
            Session["Initial"] = userSession.firstName?.FirstOrDefault().ToString().ToUpper() ?? "U";
            Session["UserID"] = userSession.id;
            Session["Roles"] = userSession.Roles.Select(r => r.RoleName).ToArray();
            Session["RoleId"] = userSession.Roles.Select(r => r.RoleId).FirstOrDefault();
            Session["RefesherToken"] = userSession.refreshToken;
            Session["Token"] = userSession.bearerToken;
            Session["UserName"] = userSession.userName;
            Session["Photo"] = userSession.profilePhoto ?? "No image";

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

            if (userSession.PermissionNodes == null)
            {
                HttpContext.Session["menu"] = new List<PermissionNode>(); // Assuming Permission is your type
            }
            else
            {
                HttpContext.Session["menu"] = userSession.PermissionNodes.ToList();
            }

            Session["EncryptedJWToken"] = TokenEncryptionHelper.EncryptToken(userSession.bearerToken);
            Session["BranchObject"] = userSession.Branch;
            Session["AuthUser"] = userSession;
            Session["UserIP"] = Request.UserHostAddress;
            ViewBag.MenuItems = Session["menu"];



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