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
using System.Net.Sockets;
using System.Threading.Tasks;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using CBS.API.Helper;
using System.Data.Entity;
using System.Diagnostics;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using CBS.API.Helper.LoginModel.Authenthication;
using CBS.FrontDesk.Data.UserManagement;
using CBS.FrontDesk.Data.Entity.Menu;

namespace CBS.FrontDesk.UI.Controllers
{

    public class BaseController : Controller
    {

        private string domain = ConfigurationManager.AppSettings["domain"];
        private string timetoExpire = ConfigurationManager.AppSettings["timetoExpire"];

        protected override IAsyncResult BeginExecuteCore(AsyncCallback callback, object state)
        {


            try
            {
                //if (!VerifyCookies("CBS4U") || !VerifyCookies("PWD") || !VerifyCookies("MFA"))
                //{
                //    // Handle invalid/expired cookies here
                //    // Optionally, redirect to a login page or perform any other action
                //}
                //else
                //{
                GetUserSession();
                //}

                return base.BeginExecuteCore(callback, state);
            }
            catch (Exception ex)
            {

                throw ex;
            }






        }


        public static bool IsTokenExpired(string token)
        {
            try
            {
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
                cookieName == "CBS4U" ? DateTime.Now.AddHours(Convert.ToInt32(timetoExpire)) : DateTime.Now.AddMinutes(Convert.ToInt32(minutes_to_live)),
                false,
                userData
            );
            authTicket.Expiration.AddHours(Convert.ToInt32(timetoExpire));
            authTicket.IssueDate.AddSeconds(0);
            string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
            HttpCookie faCookie = new HttpCookie(cookieName, encryptedTicket);
            HttpContext.Session["Token"] = reqDto.bearerToken;
            Response.Cookies.Add(faCookie);
        }

        public void GetUserSession()
        {
            if (User.Identity.IsAuthenticated)
            {
                GetMenus();
                ProcessAuthenticationCookie("CBS4U");
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

        public List<MenuMaster> GetMenus()
        {
            var list = new List<MenuMaster> {
        new MenuMaster { Id = "1", MenuText = "Dashboards", ParentId = "0", ControllerName = "Dashboard", ActionName = "Index",MenuGroup = "Dashboards", IconClass = "tf-icons mdi mdi-view-dashboard", Description = "Dashboard view", IsVisible = true },
        new MenuMaster { Id = "3", MenuText = "Branch head offices", ParentId = "1", ControllerName = "Dashboard", ActionName = "HeadOffices", MenuGroup = "Dashboards", IconClass = "tf-icons mdi mdi-office-building", Description = "Head offices view", IsVisible = true },
        new MenuMaster { Id = "4", MenuText = "Branch offices", ParentId = "1", ControllerName = "Dashboard", ActionName = "BranchOffices", MenuGroup = "Operations", IconClass = "tf-icons mdi mdi-office-building", Description = "Branch offices view", IsVisible = true },
        new MenuMaster { Id = "5", MenuText = "User Management", ParentId = "0", ControllerName = "", ActionName = "", MenuGroup = "Operations", IconClass = "tf-icons mdi mdi-account-group", Description = "User management view", IsVisible = true },
        new MenuMaster {Id = "6", MenuText = "Create users",ParentId = "5", ControllerName = "UserManagement",ActionName = "Create",MenuGroup = "Operations",IconClass = "",Description = "Create users",IsVisible = true},
        new MenuMaster { Id = "7", MenuText = "Users", ParentId = "5", ControllerName = "UserManagement", ActionName = "Index", MenuGroup = "Operations", IconClass = "", Description = "Users", IsVisible = true },
        // Other menu items related to Operations...

        new MenuMaster { Id = "22", MenuText = "Liabilities", ParentId = "0", ControllerName = "Liabilities", ActionName = "Index", MenuGroup = "Operations", IconClass = "tf-icons mdi mdi-credit-card-multiple", Description = "Liabilities view", IsVisible = true },
        new MenuMaster { Id = "23", MenuText = "Dashboard", ParentId = "22", ControllerName = "Liabilities", ActionName = "Dashboard", MenuGroup = "Operations", IconClass = "tf-icons mdi mdi-view-dashboard", Description = "Liabilities dashboard view", IsVisible = true },
        new MenuMaster { Id = "24", MenuText = "Savings", ParentId = "22", ControllerName = "Liabilities", ActionName = "Savings", MenuGroup = "Operations", IconClass = "tf-icons mdi mdi-piggy-bank", Description = "Savings view", IsVisible = true },
        // Other menu items related to Liabilities...

        new MenuMaster { Id = "25", MenuText = "Transfer", ParentId = "0", ControllerName = "Transfer", ActionName = "Index", MenuGroup = "Operations", IconClass = "tf-icons mdi mdi-transfer", Description = "Transfer view", IsVisible = true },
        new MenuMaster { Id = "26", MenuText = "Bank to vault", ParentId = "25", ControllerName = "Transfer", ActionName = "BankToVault", MenuGroup = "Operations", IconClass = "tf-icons mdi mdi-bank", Description = "Bank to vault transfer", IsVisible = true },
        new MenuMaster { Id = "27", MenuText = "Vault to bank", ParentId = "25", ControllerName = "Transfer", ActionName = "VaultToBank", MenuGroup = "Operations", IconClass = "tf-icons mdi mdi-bank", Description = "Vault to bank transfer", IsVisible = true },
        new MenuMaster { Id = "50", MenuText = "Account to account", ParentId = "25", ControllerName = "Transfer", ActionName = "AccountToAccount", MenuGroup = "Operations", IconClass = "tf-icons mdi mdi-bank", Description = "Vault to bank transfer", IsVisible = true },
        new MenuMaster { Id = "40", MenuText = "Assets",ParentId = "0",ControllerName = "",ActionName = "",MenuGroup = "Operations",IconClass = "tf-icons mdi mdi-notebook-outline",Description = "Assets",IsVisible = true},
        new MenuMaster { Id = "41", MenuText = "Dashboard",ParentId = "40",ControllerName = "Assets",ActionName = "Dashboard",MenuGroup = "Operations",IconClass = "",Description = "Dashboard",IsVisible = true},
        new MenuMaster { Id = "42", MenuText = "Loan",ParentId = "40",ControllerName = "",ActionName = "",MenuGroup = "Operations",IconClass = "",Description = "Loan",IsVisible = true },
        new MenuMaster { Id = "43", MenuText = "Loan application",ParentId = "42",ControllerName = "Loan",ActionName = "Application",MenuGroup = "Operations",IconClass = "",Description = "Loan application",IsVisible = true},
        new MenuMaster { Id = "44", MenuText = "Loans",ParentId = "42",ControllerName = "Loan",ActionName = "Loans",MenuGroup = "Operations",IconClass = "",Description = "Loans",IsVisible = true},

        // Other menu items related to Transfer...

        new MenuMaster { Id = "28", MenuText = "Accounting", ParentId = "0", ControllerName = "Accounting", ActionName = "Index", MenuGroup = "Operations", IconClass = "tf-icons mdi mdi-calculator-variant", Description = "Accounting view", IsVisible = true },
        new MenuMaster { Id = "29", MenuText = "General ledger", ParentId = "28", ControllerName = "Accounting", ActionName = "GeneralLedger", MenuGroup = "Operations", IconClass = "tf-icons mdi mdi-book-open-variant", Description = "General ledger view", IsVisible = true },
        new MenuMaster { Id = "30", MenuText = "Chart of account", ParentId = "28", ControllerName = "Accounting", ActionName = "ChartOfAccount", MenuGroup = "Operations", IconClass = "tf-icons mdi mdi-chart-areaspline", Description = "Chart of account view", IsVisible = true },
        // Other menu items related to Customers...
        new MenuMaster { Id = "35", MenuText = "Customer management", ParentId = "0", ControllerName = "", ActionName = "", MenuGroup = "Operations", IconClass = "tf-icons mdi mdi-flip-to-front", Description = "Customer management", IsVisible = true },
        new MenuMaster { Id = "36", MenuText = "Profiles", ParentId = "35", ControllerName = "Individual", ActionName = "Create", MenuGroup = "Operations", IconClass = "", Description = "Profiles", IsVisible = true },
        new MenuMaster { Id = "37", MenuText = "Customer list", ParentId = "35", ControllerName = "Individual", ActionName = "List", MenuGroup = "Operations", IconClass = "", Description = "Customer list", IsVisible = true },
        
        // Other menu items related to Accounting...

        new MenuMaster { Id = "32", MenuText = "Support", ParentId = "0", ControllerName = "Support", ActionName = "Index", MenuGroup = "Support", IconClass = "tf-icons mdi mdi-lifebuoy", Description = "Support view", IsVisible = true },
        new MenuMaster { Id = "33", MenuText = "Support", ParentId = "32", ControllerName = "Support", ActionName = "Support", MenuGroup = "Support", IconClass = "tf-icons mdi mdi-lifebuoy", Description = "Support", IsVisible = true },
        new MenuMaster { Id = "34", MenuText = "Documentation", ParentId = "32", ControllerName = "Documentation", ActionName = "Index", MenuGroup = "Support", IconClass = "tf-icons mdi mdi-file-document-multiple-outline", Description = "Documentation view", IsVisible = true }
            };
            ViewBag.MenuItems = list;
            return list;
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