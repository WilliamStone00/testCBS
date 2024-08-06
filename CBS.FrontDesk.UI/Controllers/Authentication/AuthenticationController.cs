using System;



using System.Web.Mvc;
using System.Web.Security;
using System.Web;
using CBS.FrontDesk.Service;
using CBS.API.Helper;
using System.Threading.Tasks;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.Entity;

namespace CBS.FrontDesk.UI.Controllers
{
    //[ProcessAuthenticationCookie]
    public class AuthenticationController : BaseController
    {
        private IAuthenticationServices _helper;
        //
        public AuthenticationController(IAuthenticationServices helper)
        {
            _helper = helper;

        }
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(AuthRequest model, string returnUrl = "")
        {
            var result = new ExecutionMessages();
            if (ModelState.IsValid)
            {
                result = await _helper.AuthenticateUser(model);
                var Data = new UserDto();
                if (result.Data != null)
                {
                    Data = (UserDto)result.Data;
                    if (!Data.IsBlocked)
                    {
                        try
                        {
                            if (Data.ChangePasswordOnFirstLogin)
                            {
                                var user = Data;
                                CreateToken(user, "CHANGE_PWD", 10, false, true);
                                string url = string.Format(
                                    "~/UserManagement/FLoginChangePassword?serviceoption={0}&KEY={1}&secrete={2}&usersecreteid={3}&path={4}&userName={5}",
                                    "USER", user.id, user.refreshToken, Guid.NewGuid(), user.firstName + "_" + user.lastName, user.userName
                                );
                                return RedirectToLocal(url);
                            }
                            else
                            {
                                if (Data.isMFA)
                                {
                                    CreateToken(Data, "CBS4U_MFA", 30, true);
                                    string url = string.Format("~/MFAVerification/Index?serviceoption={0}&KEY={1}&secrete={2}&usersecreteid={3}&email={4}&fullname={5}&returnUrl={6}",
                                        "MFA", Data.id, Data.refreshToken, Guid.NewGuid(), Data.email, Data.firstName, returnUrl);
                                    return Redirect(url);
                                }
                                else
                                {
                                    CreateToken(Data, "CBS4U", 30);
                                    ViewBag.Success = true;
                                    ViewBag.StartSessionWarning = true;
                                    ViewBag.Message = Messaging.MessageResult(result);
                                    return RedirectToLocal(returnUrl);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ViewBag.Success = false;
                            ViewBag.Message = $"{Messaging.MessageResult(result)}, Error: {ex.Message}";
                            return View("Login", model);
                        }
                    }
                    else
                    {
                        var user = (UserDto)result.Data;
                        string url = string.Format("~/PasswordRecovery/AccountConfirmation?serviceoption={0}&KEY={1}&secrete={2}&usersecreteid={3}&secrete{4}&path{5}",
                            "PasswordRecovery", user.id, user.refreshToken, Guid.NewGuid(), Guid.NewGuid() + "#" + Guid.NewGuid(), "forgotten_password");
                        return Redirect(url);
                    }
                }
            }

            ViewBag.Success = false;
            ViewBag.Message = Messaging.MessageResult(result);
            return View("Login", model);
        }

        //public async Task<ActionResult> Login(AuthRequest model, string returnUrl = "")
        //{
        //    var result = new ExecutionMessages();
        //    if (ModelState.IsValid)
        //    {
        //        result = await _helper.AuthenticateUser(model);
        //        var Data = new UserDto();
        //        if (result.Data != null)
        //        {
        //            Data = (UserDto)result.Data;
        //            if (!Data.IsBlocked)
        //            {
        //                try
        //                {

        //                    if (Data.ChangePasswordOnFirstLogin)
        //                    {
        //                        var user = Data;
        //                        CreateToken(user, "PWD", 10);
        //                        string url = string.Format("~/UserManagement/FLoginChangePassword?serviceoption={0}&KEY={1}&secrete={2}&usersecreteid={3}&secrete{4}&path{5}"
        //                            , "User", user.id, user.refreshToken, Guid.NewGuid(), Guid.NewGuid() + "#" + Guid.NewGuid(), "internaluserobject:" + user.firstName);
        //                        return RedirectToLocal(url);
        //                    }
        //                    else
        //                    {
        //                        if (Data.isMFA)
        //                        {
        //                            CreateToken(Data, "CBS4U_MFA", 10);
        //                            string url = string.Format("~/MFAVerification/Index?serviceoption={0}&KEY={1}&secrete={2}&usersecreteid={3}&email={4}&fullname={5}", "MFA", Data.id, Data.refreshToken, Guid.NewGuid(), Data.email, Data.firstName);
        //                            //string url = string.Format("~/MFAVerification/Index?serviceoption={0}&KEY={1}&secrete={2}&usersecreteid={3}&email{4}&fullname{5}","MFA", Data.id, Data.refreshToken, Guid.NewGuid(), Data.email, Data.firstName);
        //                            return Redirect(url);
        //                        }
        //                        else
        //                        {
        //                            CreateToken(Data, "CBS4U", 10);
        //                            ViewBag.Success = true;
        //                            ViewBag.StartSessionWarning = true;
        //                            ViewBag.Message = Messaging.MessageResult(result);
        //                            return RedirectToLocal(returnUrl);

        //                        }

        //                    }


        //                }
        //                catch (Exception ex)
        //                {
        //                    ViewBag.Success = false;
        //                    ViewBag.Message = $"{Messaging.MessageResult(result)}, Error: {ex.Message}";
        //                    return View("Login", model);
        //                }
        //                //if (Data.isAuthenticated)
        //                //{

        //                //}
        //            }
        //            else
        //            {
        //                var user = (UserDto)result.Data;
        //                string url = string.Format("~/PasswordRecovery/AccountConfirmation?serviceoption={0}&KEY={1}&secrete={2}&usersecreteid={3}&secrete{4}&path{5}", "PasswordRecovery", user.id, user.refreshToken, Guid.NewGuid(), Guid.NewGuid() + "#" + Guid.NewGuid(), "forgotten_password");
        //                return Redirect(url);

        //            }
        //        }
        //    }


        //    ViewBag.Success = false;
        //    ViewBag.Message = Messaging.MessageResult(result);
        //    return View("Login", model);


        //}

        //[HttpGet]
        //[AllowAnonymous]
        //public ActionResult Accountverification(string serviceoption = "None", string KEY = "KEY", string secrete = "none", string usersecreteid = "secrete", string path = null,string returnUrl = "")
        //{
        //    var model = new AuthenticationManagementHelper();
        //    model._object.OperationOption.KEY = KEY;
        //    model._object.OperationOption.ServiceOption = serviceoption;
        //    model._object.OperationOption.Path = "account_verification";
        //    model._object.OperationOption.ActionType = "Login";
        //    //account_verification
        //    _helper = _manager.CRUD(model);
        //    if (_helper._object.User != null)
        //    {

        //        var user = _helper._object.User;
        //        if (user.SessionID!=null)
        //        {
        //            if (CreateToken(_helper._object.User, "PWD", 10))
        //            {
        //                string url = string.Format("~/UserManagement/ChangePassword?serviceoption={0}&KEY={1}&secrete={2}&usersecreteid={3}&secrete{4}&path{5}"
        //                , "User", user.UserID, user.Password, Guid.NewGuid(), Guid.NewGuid() + "#" + Guid.NewGuid(), "internaluserobject:" + user.FullName);
        //                return RedirectToLocal(url);
        //            }
        //        }

        //    }
        //    ViewBag.Success = false;
        //    ViewBag.Message = Messaging.MessageResult(_helper.ExecutionMessage);
        //    return View("Login", model);


        //}

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
        [AllowAnonymous]
        public ActionResult Logout()
        {
            // Sign out the current user
            FormsAuthentication.SignOut();

            // List of all cookies to clear
            string[] cookieNames = { "CBS4U", "CBS4U_MFA", "MFA", "PWD", "ASP.NET_SessionId", "Token" };

            foreach (var cookieName in cookieNames)
            {
                if (Request.Cookies[cookieName] != null)
                {
                    HttpCookie cookie = new HttpCookie(cookieName)
                    {
                        Expires = DateTime.Now.AddYears(-1), // Set expiration date in the past
                        Value = string.Empty // Clear the value
                    };
                    Response.Cookies.Add(cookie);
                }
            }

            // Abandon the session
            Session.Abandon();
            Session.Clear();
            Session.RemoveAll();

            // Redirect to login page
            return RedirectToAction("Login");
        }

    }
}
