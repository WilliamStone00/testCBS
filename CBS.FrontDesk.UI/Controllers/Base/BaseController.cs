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

namespace CBS.FrontDesk.UI.Controllers
{

    public class BaseController : Controller
    {

        private string domain = ConfigurationManager.AppSettings["domain"];
        private string timetoExpire = ConfigurationManager.AppSettings["timetoExpire"];

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                var request = filterContext.HttpContext.Request;

                // Get the controller, action, and URL
                var controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
                var actionName = filterContext.ActionDescriptor.ActionName;
                var url = request.RawUrl;

                // Log or use the controller, action, and URL as needed
                // For example:
                // Log($"Controller: {controllerName}, Action: {actionName}, URL: {url}");

                // Check if the request is not for the login page and the user is not authenticated
                if (!url.StartsWith("/Authentication/Login", StringComparison.OrdinalIgnoreCase) && Session["UserID"] == null)
                {
                    // Redirect to the login page
                    filterContext.Result = new RedirectResult("~/Authentication/Login");
                    return;
                }

                if (User.Identity.IsAuthenticated)
                {
                    if (VerifyIfSessionExist("MFA"))
                    {
                        if (!url.Contains("/MFAVerification?serviceoption=MFA"))
                        {
                            if (url== "/Authentication/Logout")
                            {

                            }
                            else if (url == "/MFAVerification/MFACodeVerification")
                            {

                            }
                            else
                            {
                                string mfaurl = Session["MFAUrl"].ToString();
                                filterContext.Result = new RedirectResult(mfaurl);
                                return;
                            }

                        }

                    }

                    if (VerifyIfSessionExist("PWD"))
                    {
                        if (!url.Contains("/UserManagement/FLoginChangePassword?serviceoption=USER"))
                        {
                            if (url == "/Authentication/Logout")
                            {

                            }
                            else if (url == "/UserManagement/FLoginChangePassword")
                            {

                            }
                            else
                            {
                                string PWDUrl = Session["CHPWDUrl"].ToString();
                                filterContext.Result = new RedirectResult(PWDUrl);
                                return;
                            }

                        }
                    }
                }

                var userIp = Request.UserHostAddress;
                if (Session["UserIP"] != null && Session["UserIP"].ToString() != userIp)
                {
                    // Possible session hijacking attempt
                    Session.Abandon();
                    Session.RemoveAll();
                    Response.Redirect("~/Authentication/Logout");
                    return;

                }
                else
                {
                    Session["UserIP"] = userIp;
                }


                // User is authenticated, proceed with getting the user session
                GetUserSession();

                // Continue with the action execution
                base.OnActionExecuting(filterContext);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                throw;
            }
        }
        protected JsonResult JsonValidationErrorResponse()
        {
            // Get all the validation errors from ModelState
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

        //public void CreateToken(UserDto reqDto, string cookieName = "CBS4U", int minutesToLive = 30, bool isMFA = false)
        //{
        //    // Set MFA session flag
        //    HttpContext.Session["MFA"] = isMFA;

        //    // Create user data object
        //    var user = new CustomMembershipUser(reqDto);
        //    string[] roles = reqDto.Roles.Select(role => role.RoleName).ToArray();
        //    CustomSerializeModel userModel = new CustomSerializeModel
        //    {
        //        Id = user.UserID,
        //        UserName = reqDto.userName,
        //        RoleName = roles,
        //        Phonenumber = user.Phonenumber,
        //        TokenRefresherID = reqDto.refreshToken,
        //        Password = reqDto.password,
        //    };

        //    BuildLocalSession(reqDto);

        //    // Serialize user data
        //    string userData = JsonConvert.SerializeObject(userModel);

        //    // Create Forms Authentication Ticket
        //    FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(
        //        1,
        //        reqDto.email,
        //        DateTime.Now,
        //        DateTime.Now.AddMinutes(minutesToLive), // Set expiration
        //        false,
        //        userData
        //    );

        //    // Encrypt ticket
        //    string encryptedTicket = FormsAuthentication.Encrypt(authTicket);

        //    // Create and add the authentication cookie
        //    HttpCookie faCookie = new HttpCookie(cookieName, encryptedTicket)
        //    {
        //        HttpOnly = true,
        //        Secure = true, // Ensure cookie is sent only over HTTPS
        //        Expires = DateTime.Now.AddMinutes(minutesToLive) // Set expiration
        //    };
        //    Response.Cookies.Add(faCookie);

        //    // Store additional cookies if needed
        //    if (isMFA)
        //    {
        //        HttpCookie mfaCookie = new HttpCookie("CBS4U_MFA", reqDto.bearerToken)
        //        {
        //            HttpOnly = true,
        //            Secure = true,
        //            Expires = DateTime.Now.AddMinutes(minutesToLive)
        //        };
        //        Response.Cookies.Add(mfaCookie);
        //    }

        //    // Store token and permissions in session
        //    HttpContext.Session["Token"] = reqDto.bearerToken;
        //    if (reqDto.Permissions == null)
        //    {
        //        HttpContext.Session["menu"] = new List<Permission>(); // Assuming Permission is your type
        //    }
        //    else
        //    {
        //        HttpContext.Session["menu"] = reqDto.Permissions.ToList();
        //    }
        //}

        private void InvalidateCookie(string cookieName)
        {
            if (Response.Cookies[cookieName] != null)
            {
                FormsAuthentication.SignOut();

                HttpCookie cookie = new HttpCookie("CBS4U");
                cookie.Expires = DateTime.Now.AddYears(-1);
                Response.Cookies.Add(cookie);
                Session.Abandon();
                Session.Clear();
            }
        }

        public void CreateToken(UserDto reqDto, int minutes_to_live = 30)
        {
            FormsAuthentication.SetAuthCookie(reqDto.userName, false);
            var encryptedToken = TokenEncryptionHelper.EncryptToken(reqDto.bearerToken);
            Session["EncryptedJWToken"] = encryptedToken;
            Session["BranchObject"] = reqDto.Branch;
            Session["AuthUser"] = reqDto;
            Session["UserIP"] = Request.UserHostAddress;
            BuildLocalSession(reqDto);
            Session.Timeout = minutes_to_live;
            if (reqDto.Permissions == null)
            {
                HttpContext.Session["menu"] = new List<Permission>(); // Assuming Permission is your type
            }
            else
            {
                HttpContext.Session["menu"] = reqDto.Permissions.ToList();
            }
        }

        //public void CreateToken(UserDto reqDto, string cookieName = "CBS4U", int minutes_to_live = 30, bool isMFA = false, bool isPWD = false)
        //{
        //    // Set authentication cookie
        //    FormsAuthentication.SetAuthCookie(reqDto.userName, false);

        //    //// Set session variables for password change and MFA
        //    //if (isPWD)
        //    //{
        //    //    HttpContext.Session["CHANGE_PWD"] = true;
        //    //}
        //    //if (isMFA)
        //    //{
        //    //    HttpContext.Session["MFA"] = true;
        //    //}

        //    //// Create custom user model
        //    //var user = new CustomMembershipUser(reqDto);
        //    //string[] roles = reqDto.Roles.Select(role => role.RoleName).ToArray();
        //    //CustomSerializeModel userModel = new CustomSerializeModel()
        //    //{
        //    //    Id = user.UserID,
        //    //    UserName = reqDto.userName,
        //    //    RoleName = roles,
        //    //    Phonenumber = user.Phonenumber,
        //    //    TokenRefresherID = reqDto.refreshToken,
        //    //    Password = reqDto.password,
        //    //};

        //    // Build local session
        //    BuildLocalSession(reqDto);

        //    //// Serialize user data into JSON
        //    //string userData = JsonConvert.SerializeObject(userModel);

        //    //// Create the authentication ticket
        //    //FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(
        //    //    1,
        //    //    reqDto.email,
        //    //    DateTime.Now,
        //    //    DateTime.Now.AddMinutes(minutes_to_live), // Token expiration time
        //    //    false,
        //    //    userData
        //    //);

        //    //// Encrypt the ticket
        //    //string encryptedTicket = FormsAuthentication.Encrypt(authTicket);

        //    //// Create and configure the authentication cookie
        //    //HttpCookie faCookie = new HttpCookie(cookieName, encryptedTicket)
        //    //{
        //    //    HttpOnly = true,
        //    //    Secure = true // Ensure the cookie is only sent over HTTPS
        //    //};

        //    //// Add the authentication cookie to the response
        //    //Response.Cookies.Add(faCookie);

        //    // Set the session timeout to match the token's expiration time
        //    Session.Timeout = minutes_to_live;
        //    if (reqDto.Permissions == null)
        //    {
        //        HttpContext.Session["menu"] = new List<Permission>(); // Assuming Permission is your type
        //    }
        //    else
        //    {
        //        HttpContext.Session["menu"] = reqDto.Permissions.ToList();
        //    }
        //}


        //public void CreateToken(UserDto reqDto, string cookieName = "CBS4U", int minutes_to_live = 30, bool isMFA = false, bool isPWD = false)
        //{


        //    // Set authentication cookie
        //    FormsAuthentication.SetAuthCookie(reqDto.userName, false);



        //    if (isPWD)
        //    {
        //        HttpContext.Session["CHANGE_PWD"] = true;
        //    }
        //    if (isMFA)
        //    {
        //        HttpContext.Session["MFA"] = true;
        //    }
        //    var user = new CustomMembershipUser(reqDto);
        //    string[] roles = reqDto.Roles.Select(role => role.RoleName).ToArray();
        //    CustomSerializeModel userModel = new CustomSerializeModel()
        //    {
        //        Id = user.UserID,
        //        UserName = reqDto.userName,
        //        RoleName = roles,
        //        Phonenumber = user.Phonenumber,
        //        TokenRefresherID = reqDto.refreshToken,
        //        Password = reqDto.password,
        //    };
        //    BuildLocalSession(reqDto);
        //    string userData = JsonConvert.SerializeObject(userModel);
        //    FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(
        //        1,
        //        reqDto.email,
        //        DateTime.Now,
        //        cookieName == "CBS4U" ? DateTime.Now.AddMinutes(Convert.ToInt32(timetoExpire)) : DateTime.Now.AddMinutes(Convert.ToInt32(minutes_to_live)),
        //        false,
        //        userData
        //    );
        //    authTicket.Expiration.AddMinutes(Convert.ToInt32(timetoExpire));
        //    authTicket.IssueDate.AddSeconds(0);
        //    string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
        //    HttpCookie faCookie = new HttpCookie(cookieName, encryptedTicket);
        //    faCookie.HttpOnly = true;
        //    faCookie.Secure = true;
        //    Response.Cookies[".AspNet.ApplicationCookie"].HttpOnly = true;
        //    Response.Cookies[".AspNet.ApplicationCookie"].Secure = true;

        //    FormsAuthentication.SetAuthCookie(reqDto.userName, false);
        //    var encryptedToken = TokenEncryptionHelper.EncryptToken(reqDto.bearerToken);
        //    Session["EncryptedJWToken"] = encryptedToken;
        //    Session["BranchObject"] = reqDto.Branch;
        //    Session["AuthUser"] = reqDto;
        //    if (reqDto.Permissions == null)
        //    {
        //        HttpContext.Session["menu"] = new List<Permission>(); // Assuming Permission is your type
        //    }
        //    else
        //    {
        //        HttpContext.Session["menu"] = reqDto.Permissions.ToList();
        //    }

        //    //Response.Cookies.Add(faCookie);
        //}
        public void RemoveSessionName(string sessionName)
        {
            HttpContext.Session.Remove(sessionName); // Removing the MFA token


        }


        public void GetUserSession()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (VerifyIfSessionExist("MFA"))
                {

                }
                if (VerifyIfSessionExist("PWD"))
                {

                }

                GetMenus();
                //ProcessAuthenticationCookie("CBS4U");
            }
        }


        private void ProcessAuthenticationCookie(string cookieName)
        {

            HttpCookie authCookie = Request.Cookies[cookieName];
            if (authCookie != null)
            {
                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                if (!authTicket.Expired)
                {
                    var user = JsonConvert.DeserializeObject<CustomSerializeModel>(authTicket.UserData);
                    bool isTokenExpired = IsTokenExpired(user.Token);
                    if (isTokenExpired)
                    {

                        //var refresher = new AuthRequest { UserName = user.UserName, Password = user.Password };
                        //RefreshToken(refresher);
                    }
                    else
                    {
                        HttpContext.Session["Token"] = user.Token;
                    }
                }
            }
        }
        //private void RefreshToken(AuthRequest authRequest)
        //{
        //    var _identityServer = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
        //    var response = _identityServer.Post<UserDto>(APICallHelper.Authentication, authRequest);
        //    if (response.IsSuccess)
        //    {
        //        response.ApiResponseData.password = authRequest.Password;
        //        if (Request.Cookies["CBS4U"] != null)
        //        {
        //            InvalidateCookie("CBS4U");
        //        }
        //        CreateToken(response.ApiResponseData, "CBS4U");
        //    }
        //}

        public List<DatabaseMenus> GetMenus()
        {
            var menus = System.Web.HttpContext.Current.Session["menu"];

            if (menus is List<DatabaseMenus> permissionMenus)
            {
                var menusMaster = permissionMenus.OrderBy(x => x.MenuMasterId).ToList();
                ViewBag.MenuItems = menusMaster;
                return menusMaster;
            }
            else
            {
                ViewBag.MenuItems = new List<DatabaseMenus>();
                return new List<DatabaseMenus>();
            }
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

        //public DataTableOptions GetDataTableOptions()
        //{
        //    DataTableOptions dataTableOptions = new DataTableOptions();
        //    // Retrieve HttpContext from the current HTTP context
        //    var context = HttpContext;
        //    // Retrieve values from query parameters
        //    dataTableOptions.draw = context.Request.QueryString["draw"];
        //    dataTableOptions.start = Convert.ToInt32(context.Request.QueryString["start"]);
        //    dataTableOptions.length = Convert.ToInt32(context.Request.QueryString["length"]);
        //    //dataTableOptions.sortColumnName = context.Request.QueryString["order[0][column]"];
        //    dataTableOptions.sortColumnName = context.Request.QueryString["columns[" + context.Request.QueryString["order[0][column]"] + "][Name]"];
        //    dataTableOptions.sortColumnDirection = context.Request.QueryString["order[0][dir]"];
        //    dataTableOptions.searchValue = context.Request.QueryString["search[value]"];

        //    // Calculate pageSize and skip
        //    dataTableOptions.pageSize = dataTableOptions.length != 0 ? dataTableOptions.length : 0;
        //    dataTableOptions.skip = dataTableOptions.start != 0 ? dataTableOptions.start : 0;

        //    // Set recordsTotal (you need to implement this logic)
        //    dataTableOptions.recordsTotal = 0;

        //    return dataTableOptions;
        //}


        //public DataTableOptions GetDataTableOptions(HttpRequest request)
        //{
        //    DataTableOptions dataTableOptions = new DataTableOptions();
        //    dataTableOptions.draw = request.Form.GetValues("draw")[0];
        //    // Skiping number of Rows count
        //    dataTableOptions.start = Convert.ToInt32(request.Form.GetValues("start")[0]);
        //    // Paging Length 10,20
        //    dataTableOptions.length = Convert.ToInt32(request.Form.GetValues("length")[0]);
        //    // Sort Column Index (changed from Name to index)
        //    dataTableOptions.sortColumnName = Convert.ToInt32(request.Form.GetValues("order[0][column]")[0]);
        //    // Sort Column Direction ( asc ,desc)
        //    dataTableOptions.sortColumnDirection = request.Form.GetValues("order[0][dir]")[0];
        //    // Search Value from (Search box)
        //    dataTableOptions.searchValue = request.Params["search[value]"];
        //    dataTableOptions.pageSize = dataTableOptions.length != 0 ? Convert.ToInt32(dataTableOptions.length) : 0;
        //    dataTableOptions.skip = dataTableOptions.start != 0 ? Convert.ToInt32(dataTableOptions.start) : 0;
        //    dataTableOptions.recordsTotal = 0;
        //    return dataTableOptions;
        //}

        public void BuildLocalSession(UserDto userSession)
        {
            var roles = userSession.Roles.Select(role => role.RoleName).ToArray();

            string fullName = userSession.firstName + " " + userSession.lastName;
            Session["FullName"] = fullName;
            Session["Msisdn"] = userSession.phoneNumber;
            Session["Email"] = userSession.email;
            Session["Initial"] = fullName.Substring(0, 1).ToUpper();
            Session["UserID"] = userSession.id;
            Session["Roles"] = roles;
            Session["RefesherToken"] = userSession.refreshToken;
            Session["Token"] = userSession.bearerToken;
            Session["UserName"] = userSession.userName;
            if (userSession.profilePhoto == null)
            {
                Session["Photo"] = "No image";
            }
            else
            {
                Session["Photo"] = userSession.profilePhoto;
            }
            if (userSession.Branch.Bank.LogoUrl == null)
            {
                Session["LogoUrl"] = userSession.Branch.ImageVirtualPath;
            }
            else
            {
                Session["LogoUrl"] = userSession.Branch.Bank.LogoUrl;
            }
            Session["BranchID"] = userSession.BranchID;
            Session["OrganizationID"] = userSession.Bank.OrganizationId;
            Session["BankID"] = userSession.Branch.BankId;
            Session["BankName"] = userSession.Branch.Bank.Name;
            Session["BranchName"] = userSession.Branch.Name;
            Session["BranchCode"] = userSession.Branch.BranchCode;
            Session["IsHavingBank"] = userSession.Branch.IsHavingBank;
            Session["BankCode"] = userSession.Branch.Bank.BankCode;

            if (userSession.Branch.IsHeadOffice)
            {
                Session["IsHeadOffice"] = true;
            }
            else
            {
                Session["IsHeadOffice"] = false;
            }
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