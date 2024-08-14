using CBS.BusinessService;
using CBS.BusinessService.Accounting;
using CBS.BusinessService.Config;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Service;
using CBS.FrontDesk.UI.Filters;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace CBS.FrontDesk.UI
{

    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        
    
        
            GlobalHost.DependencyResolver.Register(typeof(ConnectionHub), () => new ConnectionHub());
            GlobalFilters.Filters.Add(new System.Web.Mvc.AuthorizeAttribute());
            UnityConfig.RegisterComponents();
            MvcHandler.DisableMvcResponseHeader = true;
            GlobalFilters.Filters.Add(new UserAuditFilter()); // Register UserAuditFilter

        ConnectionMonitoringService connectionService = new ConnectionMonitoringService();
        }

        protected void Application_EndRequest()
        {
            if (Context.Items["AjaxPermissionDenied"] is bool ajaxPermissionDenied && ajaxPermissionDenied)
            {
                Context.Response.StatusCode = 401;
                Context.Response.End();
            }
        }
        protected void Application_PreSendRequestHeaders()
        {
            Response.Headers.Remove("Server");
            Response.Headers.Remove("X-AspNet-Version");
            Response.Headers.Add("X-Content-Type-Options", "nosniff");
            Response.Headers.Add("X-Frame-Options", "DENY");
            Response.Headers.Add("X-XSS-Protection", "1; mode=block");
            //Response.Headers.Add("Content-Security-Policy", "default-src 'self'; script-src 'self';");
            Response.Headers.Add("Referrer-Policy", "no-referrer");

        }
        protected void Application_Error(object sender, EventArgs e)
        {
            var exception = Server.GetLastError();
            var httpException = exception as HttpException;

            Response.Clear();
            Server.ClearError();

            if (httpException != null)
            {
                int errorCode = httpException.GetHttpCode();

                // Redirect based on error code
                switch (errorCode)
                {
                    case 400:
                        Response.Redirect("~/Error/BadRequest?message=" + httpException.Message);
                        break;
                    case 401:
                        Response.Redirect("~/Error/Unauthorized?message=" + httpException.Message);
                        break;
                    case 403:
                        Response.Redirect("~/Error/Forbidden?message=" + httpException.Message);
                        break;
                    case 404:
                        Response.Redirect("~/Error/NotFound?message=" + httpException.Message);
                        break;
                    case 500:
                        Response.Redirect("~/Error/InternalServer?message=" + httpException.Message);
                        break;
                    case 503:
                        Response.Redirect("~/Error/ServiceUnavailable?message=" + httpException.Message);
                        break;
                    // Add more cases for other error codes if needed
                    default:
                        Response.Redirect("~/Error?message=" + httpException.Message);
                        break;
                }
            }
            else
            {
                // Redirect to a generic error page for other types of exceptions
                Response.Redirect("~/Error?message=" + exception.Message);
            }
        }


        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            ProcessAuthenticationCookie("CBS4U");
            ProcessAuthenticationCookie("CBS4U_MFA");
            ProcessAuthenticationCookie("PWD");
            ProcessAuthenticationCookie("CHANGE_PWD");
        }

        private void ProcessAuthenticationCookie(string cookieName)
        {
            // Retrieve the boolean value from session, default to false if null or not a boolean
            bool isMFA = HttpContext.Current.Session?["MFA"] is bool mfaValue ? mfaValue : false;
            bool isPWD = HttpContext.Current.Session?["PWD"] is bool mfaValuee ? mfaValuee : false;
            
            // Use the boolean value
            if (!isMFA || !isPWD)
            {
                HttpCookie authCookie = Request.Cookies[cookieName];
                if (authCookie != null && !string.IsNullOrEmpty(authCookie.Value))
                {
                    FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                    if (authTicket != null && !authTicket.Expired)
                    {
                        AddIdentity(authTicket);
                    }
                    else
                    {
                        InvalidateCookie(cookieName);
                    }
                }
            }
        }

        //private void ProcessAuthenticationCookie(string cookieName)
        //{
        //    // Retrieve the boolean value from session, default to false if null or not a boolean
        //    bool isMFA = HttpContext.Current.Session?["MFA"] is bool mfaValue ? mfaValue : false;

        //    // Use the boolean value
        //    if (!isMFA)
        //    {
        //        // Ensure that Request.Cookies and the specific cookie are not null
        //        HttpCookie authCookie = Request.Cookies?[cookieName];
        //        HttpCookie xauthCookie = Request.Cookies[cookieName];
        //        if (authCookie != null && !string.IsNullOrEmpty(authCookie.Value))
        //        {
        //            try
        //            {
        //                // Decrypt the cookie value
        //                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);

        //                // Validate the decrypted ticket
        //                if (authTicket != null && !authTicket.Expired)
        //                {
        //                    // Add identity based on the ticket
        //                    AddIdentity(authTicket);
        //                }
        //                else
        //                {
        //                    // Invalidate the cookie if the ticket is null or expired
        //                    InvalidateCookie(cookieName);
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                // Log or handle the exception if decryption fails
        //                // Optionally, you could invalidate the cookie if there's an issue with decryption
        //                InvalidateCookie(cookieName);
        //                // Log the error (ex.Message) for further diagnosis
        //            }
        //        }
        //        else
        //        {
        //            // Handle the case where the cookie is not present or has an empty value
        //            InvalidateCookie(cookieName);
        //        }
        //    }
        //}


        private void AddIdentity(FormsAuthenticationTicket authTicket)
        {
            var user = JsonConvert.DeserializeObject<CustomSerializeModel>(authTicket.UserData);
            CustomPrincipal principal = new CustomPrincipal(authTicket.Name)
            {
                UserId = user.Id,
                UserName = user.UserName,
                Roles = user.RoleName,
                SessionID = user.TokenRefresherID,
                Phonenumber = user.Phonenumber,
                RefresherID = user.TokenRefresherID,
                Token = user.Token,
                Password = user.Password,
            };
            HttpContext.Current.User = principal;
        }

        private void InvalidateCookie(string cookieName)
        {
            if (Response.Cookies[cookieName] != null)
            {
                Response.Cookies[cookieName].Expires = DateTime.Now.AddYears(-1);
            }
        }
    }


    //public class MvcApplication : System.Web.HttpApplication
    //{
    //    protected void Application_Start()
    //    {
    //        AreaRegistration.RegisterAllAreas();
    //        FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
    //        RouteConfig.RegisterRoutes(RouteTable.Routes);
    //        BundleConfig.RegisterBundles(BundleTable.Bundles);
    //        GlobalFilters.Filters.Add(new AuthorizeAttribute());
    //        UnityConfig.RegisterComponents();
    //    }
    //    protected void Application_EndRequest()
    //    {
    //        if (Context.Items["AjaxPermissionDenied"] is bool)
    //        {
    //            Context.Response.StatusCode = 401;
    //            Context.Response.End();
    //        }

    //    }

    //    protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
    //    {
    //        HttpCookie authCookie1 = Request.Cookies["CBS4U"];
    //        HttpCookie authCookie2 = Request.Cookies["PWD"];
    //        HttpCookie authCookie3 = Request.Cookies["MFA"];
    //        if (authCookie1 != null || authCookie2 != null || authCookie3 != null)
    //        {
    //            if (authCookie1 != null)
    //            {
    //                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie1.Value);
    //                if (!authTicket.Expired)
    //                {
    //                    AddIdentity(authCookie1);
    //                }
    //                else
    //                {
    //                    Response.Cookies["CBS4U"].Expires = DateTime.Now.AddYears(-1);

    //                }
    //            }
    //            else if (authCookie2 != null)
    //            {

    //                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie2.Value);
    //                if (!authTicket.Expired)
    //                {
    //                    AddIdentity(authCookie2);
    //                }
    //                else
    //                {
    //                    Response.Cookies["PWD"].Expires = DateTime.Now.AddYears(-1);

    //                }
    //            }
    //            else if (authCookie3 != null)
    //            {
    //                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie3.Value);
    //                if (!authTicket.Expired)
    //                {
    //                    AddIdentity(authCookie3);
    //                }
    //                else
    //                {
    //                    Response.Cookies["MFA"].Expires = DateTime.Now.AddYears(-1);

    //                }
    //            }

    //        }

    //    }

    //    public void AddIdentity(HttpCookie authCookie)
    //    {
    //        FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
    //        if (!authTicket.Expired)
    //        {
    //            var user = JsonConvert.DeserializeObject<CustomSerializeModel>(authTicket.UserData);
    //            CustomPrincipal principal = new CustomPrincipal(authTicket.Name);
    //            principal.UserId = user.Id;
    //            principal.FullName = user.FullName;
    //            principal.UserName = user.UserName;
    //            principal.Roles = user.RoleName;
    //            principal.SessionID = user.TokenRefresherID;
    //            principal.Email = user.Email;
    //            principal.Phonenumber = user.Phonenumber;
    //            HttpContext.Current.User = principal;
    //        }
    //    }
    //}

}
