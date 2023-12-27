


namespace CBS.FrontDesk.Service
{
    //public class CustomAuthorizeAttribute : AuthorizeAttribute
    //{
    //    IUnitOfWork _unitOfWork = new UnitOfWork();
    //    RuntimeData user = new RuntimeData(_unitOfWork);
    //    HttpContext ctx = HttpContext.Current;
    //    protected virtual CustomPrincipal CurrentUser
    //    {
    //        get { return HttpContext.Current.User as CustomPrincipal; }
    //    }

    //    protected override bool AuthorizeCore(HttpContextBase httpContext)
    //    {

    //        return ((CurrentUser != null && !CurrentUser.IsInRole(Roles)) || CurrentUser == null) ? false : true;
    //    }


    //    protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
    //    {





    //        RedirectToRouteResult routeData = null;

    //        if (CurrentUser == null)
    //        {
    //            routeData = new RedirectToRouteResult
    //                (new RouteValueDictionary
    //                (new
    //                {
    //                    controller = "Security",
    //                    action = "Login",
    //                }
    //                ));
    //        }
    //        else
    //        {
    //            routeData = new RedirectToRouteResult
    //            (new RouteValueDictionary
    //             (new
    //             {
    //                 controller = "Unauthorized",
    //                 action = "AccessDenied"
    //             }
    //             ));
    //        }


    //        filterContext.Result = routeData;
    //    }

    //}
    //[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    //public class SessionExpireFilterAttribute : ActionFilterAttribute
    //{
    //    public override void OnActionExecuting(ActionExecutingContext filterContext)
    //    {
    //        HttpContext ctx = HttpContext.Current;

    //        if (ctx.Session != null)
    //        {

    //            if (filterContext.HttpContext.Request.IsAuthenticated)
    //            {
    //                if (filterContext.HttpContext.Request.IsAjaxRequest())
    //                {
    //                    if (ctx.Session["LoginSessionID"] == null)
    //                    {

    //                        filterContext.HttpContext.Response.ClearContent();
    //                        filterContext.HttpContext.Items["AjaxPermissionDenied"] = true;

    //                    }

    //                }
    //            }
    //        }


    //        base.OnActionExecuting(filterContext);
    //    }
    //}


    //public class AuthorizationFilterAttribute : ActionFilterAttribute
    //{
    //    public override void OnActionExecuting(ActionExecutingContext filterContext)
    //    {


    //        HttpContext ctx = HttpContext.Current;
    //        string url = null;
    //        int i = 0;

    //        //if (ctx.User.Identity.IsAuthenticated)
    //        //{
    //        //    if (ctx.Session["LoginSessionID"] == null)
    //        //    {
    //        //        string redirectOnSuccess = filterContext.HttpContext.Request.Url.PathAndQuery;
    //        //        string redirectUrl = string.Format("?ReturnUrl={0}", redirectOnSuccess);
    //        //        string loginUrl = redirectUrl;
    //        //        if (ctx.Request.IsAuthenticated)
    //        //        {
    //        //            FormsAuthentication.SignOut();
    //        //        }
    //        //        RedirectResult rr = new RedirectResult(redirectOnSuccess);
    //        //        filterContext.Result = rr;

    //        //    }
    //        //}
    //        if (HttpContext.Current.Session["LoginSessionID"] == null)
    //        {
    //            filterContext.Result = new RedirectResult("~/Security/Login");
    //            return;

    //        }
    //        var controllerName = filterContext.RequestContext.RouteData.Values.ContainsKey("Controller") ? filterContext.RequestContext.RouteData.Values["Controller"].ToString() : null;
    //        var actionName = filterContext.RequestContext.RouteData.Values.ContainsKey("Action") ? filterContext.RequestContext.RouteData.Values["Action"].ToString() : null;
    //        var userName = filterContext.RequestContext.HttpContext.User.Identity.Name;
    //        url = $"{controllerName}/{actionName}";
    //        if (actionName.Equals("Index"))
    //        {
    //            url = $"{controllerName}";
    //        }

    //        MemoryCache memoryCache = MemoryCache.Default;
    //        var res = memoryCache.Get(userName);
    //        UserSessionVariable myData = res as UserSessionVariable;
    //        int roleid = myData.RoleID;

    //        int uid = myData.UserID;
    //        var roleMM = myData.RoleMenuMappings.ToList();
    //        var roleM = roleMM.Where(x => x.TRole.TRoleID.Equals(roleid)).ToList();
    //        var userM = myData.UserMenuMappings.Where(x => x.TUserID.Equals(uid)).ToList();
    //        if (roleM.Count() == 0)
    //        {
    //            filterContext.Result = new RedirectResult("~/Unauthorized/AccessDenied");
    //            return;

    //        }
    //        else
    //        {
    //            if (controllerName == "Home" || controllerName == "Momokash" || controllerName == "Microfinance")
    //            {

    //            }
    //            else
    //            {
    //                if (userM.Count().Equals(0))
    //                {

    //                    foreach (var item in roleMM)
    //                    {
    //                        foreach (var sub in item.MainMenu.SubMenus)
    //                        {
    //                            if (sub.SubMenuUrl.ToUpper().Equals(url.ToUpper()))
    //                            {
    //                                i = 1;

    //                            }

    //                        }
    //                    }
    //                }
    //                else
    //                {
    //                    foreach (var sub in userM)
    //                    {
    //                        if (sub.SubMenu.SubMenuUrl.ToUpper().Equals(url.ToUpper()))
    //                        {
    //                            i = 1;

    //                        }
    //                    }

    //                }
    //                if (i.Equals(0))
    //                {
    //                    filterContext.Result = new RedirectResult("~/Unauthorized/AccessDenied");
    //                    return;
    //                }
    //            }



    //        }




    //        base.OnActionExecuting(filterContext);
    //    }



    //}



}