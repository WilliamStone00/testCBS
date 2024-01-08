using CBS.FrontDesk.Data.Entity.User;
using CBS.FrontDesk.Service;
using CBS.FrontDesk.UI.Controllers.ErrorHandler;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
            GlobalFilters.Filters.Add(new AuthorizeAttribute());
            UnityConfig.RegisterComponents();
        }

        protected void Application_EndRequest()
        {
            if (Context.Items["AjaxPermissionDenied"] is bool ajaxPermissionDenied && ajaxPermissionDenied)
            {
                Context.Response.StatusCode = 401;
                Context.Response.End();
            }
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
                        Response.Redirect("~/Error/BadRequest");
                        break;
                    case 401:
                        Response.Redirect("~/Error/Unauthorized");
                        break;
                    case 403:
                        Response.Redirect("~/Error/Forbidden");
                        break;
                    case 404:
                        Response.Redirect("~/Error/NotFound");
                        break;
                    case 500:
                        Response.Redirect("~/Error/InternalServer");
                        break;
                    case 503:
                        Response.Redirect("~/Error/ServiceUnavailable");
                        break;
                    // Add more cases for other error codes if needed
                    default:
                        Response.Redirect("~/Error");
                        break;
                }
            }
            else
            {
                // Redirect to a generic error page for other types of exceptions
                Response.Redirect("~/Error");
            }
        }



        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            ProcessAuthenticationCookie("CBS4U");
            ProcessAuthenticationCookie("PWD");
            ProcessAuthenticationCookie("MFA");
        }

        private void ProcessAuthenticationCookie(string cookieName)
        {



            HttpCookie authCookie = Request.Cookies[cookieName];
            if (authCookie != null)
            {
                FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                if (!authTicket.Expired)
                {
                    AddIdentity(authTicket);
                }
                else
                {
                    InvalidateCookie(cookieName);
                }
            }
        }

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
