using CBS.FrontDesk.Data.Entity.User;
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

                // Check if the request is not for the login page and the user is not authenticated
                if (!request.RawUrl.StartsWith("/Authentication/Login", StringComparison.OrdinalIgnoreCase) && Session["UserID"] == null)
                {
                    // Redirect to the login page
                    filterContext.Result = new RedirectResult("~/Authentication/Login");
                    return;
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

        //protected override void OnActionExecuting(ActionExecutingContext filterContext)
        //{
        //    // Check if the request is going to the login page to avoid redirection loops
        //    if (!filterContext.HttpContext.Request.RawUrl.StartsWith("/Authentication/Login", StringComparison.OrdinalIgnoreCase))
        //    {
        //        if (Session["UserID"] == null) // Replace "UserID" with your session key
        //        {
        //            // Redirect to login or show a session expired message
        //            filterContext.Result = new RedirectResult("~/Authentication/Login");
        //            return;
        //        }
        //    }
        //    GetUserSession();
        //    base.OnActionExecuting(filterContext);
        //}


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
        public void CreateToken(UserDto reqDto, string cookieName = "CBS4U", int minutes_to_live = 30)
        {
            var user = new CustomMembershipUser(reqDto);
            string[] roles = reqDto.Roles.Select(role => role.RoleName).ToArray();
            CustomSerializeModel userModel = new CustomSerializeModel()
            {
                Id = user.UserID,
                UserName = reqDto.userName,
                RoleName = roles,
                //FullName = user.FullName,
                //Email = user.Email,
                Phonenumber = user.Phonenumber,
                TokenRefresherID = reqDto.refreshToken,
                //Token = reqDto.bearerToken,
                Password = reqDto.password,
            };
            BuildLocalSession(reqDto);
            string userData = JsonConvert.SerializeObject(userModel);
            FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(
                1,
                reqDto.email,
                DateTime.Now,
                cookieName == "CBS4U" ? DateTime.Now.AddMinutes(Convert.ToInt32(timetoExpire)) : DateTime.Now.AddMinutes(Convert.ToInt32(minutes_to_live)),
                false,
                userData
            );
            authTicket.Expiration.AddMinutes(Convert.ToInt32(timetoExpire));
            authTicket.IssueDate.AddSeconds(0);
            string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
            HttpCookie faCookie = new HttpCookie(cookieName, encryptedTicket);
            Session.Timeout = 30;
            HttpContext.Session["Token"] = reqDto.bearerToken;
            HttpContext.Session["menu"] = reqDto.Permissions.ToList();

            Response.Cookies.Add(faCookie);
        }

        public void GetUserSession()
        {
            if (User.Identity.IsAuthenticated)
            {
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
        private void RefreshToken(AuthRequest authRequest)
        {
            var _identityServer = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());
            var response = _identityServer.Post<UserDto>(APICallHelper.Authentication, authRequest);
            if (response.IsSuccess)
            {
                response.ApiResponseData.password = authRequest.Password;
                if (Request.Cookies["CBS4U"] != null)
                {
                    InvalidateCookie("CBS4U");
                }
                CreateToken(response.ApiResponseData, "CBS4U");
            }
        }

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

            DataTableOptions dataTableOptions = new DataTableOptions();
            dataTableOptions.draw = Request.Form.GetValues("draw")[0];
            // Skiping number of Rows count
            dataTableOptions.start = Convert.ToInt32(Request.Form.GetValues("start")[0]);
            // Paging Length 10,20
            dataTableOptions.length = Convert.ToInt32(Request.Form.GetValues("length")[0]);
            // Sort Column Name
            dataTableOptions.sortColumnName = Request.Form.GetValues("order[0][column]")[0];
            // Sort Column Direction ( asc ,desc)
            dataTableOptions.sortColumnDirection = Request.Form.GetValues("order[0][dir]")[0];
            // Search Value from (Search box)
            dataTableOptions.searchValue = Request.Params["search[value]"];
            dataTableOptions.pageSize = dataTableOptions.length != 0 ? Convert.ToInt32(dataTableOptions.length) : 0;
            dataTableOptions.skip = dataTableOptions.start != 0 ? Convert.ToInt32(dataTableOptions.start) : 0;
            dataTableOptions.recordsTotal = 0;
            return dataTableOptions;
        }
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
            Session["BranchID"] = userSession.BranchID;
            Session["OrganizationID"] = userSession.Bank.OrganizationId;
            Session["BankID"] = userSession.BankID;
            Session["BankName"] = userSession.Bank.Name;
            Session["BranchName"] = userSession.Branch.Name;
            Session["BranchCode"] = userSession.Branch.BranchCode;
            Session["BankCode"] = userSession.Bank.BankCode;
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
                string body = "<b>Dear " + user.firstName.ToUpper() + "</b><br/>Your momokash account was successfully created." + "<br/>User name: <b>" + user.userName + "</b><br/>Password: <b>" + user.refreshToken + "</b><b><br/><a href=" + domain + " class='btn-gradient-primary' style='font-size: 14px; text-decoration: none'>Login to portal</a></b>";
                string subject = "MOMOKASH ACCOUNT CREATION";
                if (smstype == "Registration")
                {
                    body = "<b>Dear " + user.firstName.ToUpper() + "</b><br/>Your momokash account was successfully created." + "<br/>User name: <b>" + user.userName + "</b><br/>Password: <b>" + user.refreshToken + "</b><b><br/><a href=" + domain + " class='btn-gradient-primary' style='font-size: 14px; text-decoration: none'>Login to portal</a></b>";
                }
                else if (smstype == "Reset")
                {
                    string data = Session["FullName"].ToString();
                    subject = "MOMOKASH PASSWORD RESET";
                    body = "<b>Dear " + user.firstName.ToUpper() + "</b><br/>Your momokash account was successfully reseted by " + data.ToUpper() + "\n" + "<br/>User name: <b>" + user.userName + "</b><br/>New password: <b>" + user.refreshToken + "</b><b><br/><a href=" + domain + " class='btn-gradient-primary' style='font-size: 14px; text-decoration: none'>Login to portal using new password</a></b>";
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