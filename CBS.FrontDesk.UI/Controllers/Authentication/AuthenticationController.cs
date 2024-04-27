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
                                CreateToken(user, "PWD", 10);
                                string url = string.Format("~/UserManagement/FLoginChangePassword?serviceoption={0}&KEY={1}&secrete={2}&usersecreteid={3}&secrete{4}&path{5}"
                                    , "User", user.id, user.refreshToken, Guid.NewGuid(), Guid.NewGuid() + "#" + Guid.NewGuid(), "internaluserobject:" + user.firstName);
                                return RedirectToLocal(url);
                            }
                            else
                            {
                                if (Data.isMFA)
                                {
                                    CreateToken(Data, "MFA", 10);
                                    string url = string.Format("~/TwoStepaccountverification/MFACodeVerification?serviceoption={0}&KEY={1}&secrete={2}&usersecreteid={3}&secrete{4}&path{5}", "User", Data.id, Data.refreshToken, Guid.NewGuid(), Guid.NewGuid() + "#" + Guid.NewGuid(), "internaluserobject:" + Data.firstName);
                                    return Redirect(url);
                                }
                                else
                                {
                                    CreateToken(Data, "CBS4U", 10);
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
                        //if (Data.isAuthenticated)
                        //{

                        //}
                    }
                    else
                    {
                        var user = (UserDto)result.Data;
                        string url = string.Format("~/PasswordRecovery/AccountConfirmation?serviceoption={0}&KEY={1}&secrete={2}&usersecreteid={3}&secrete{4}&path{5}", "PasswordRecovery", user.id, user.refreshToken, Guid.NewGuid(), Guid.NewGuid() + "#" + Guid.NewGuid(), "forgotten_password");
                        return Redirect(url);

                    }
                }
            }

            
            ViewBag.Success = false;
            ViewBag.Message = Messaging.MessageResult(result);
            return View("Login", model);


        }
        
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

            FormsAuthentication.SignOut();
            HttpCookie cookie = new HttpCookie("CBS4U");
            cookie.Expires = DateTime.Now.AddYears(-1);
            Response.Cookies.Add(cookie);
            Session.Abandon();
            Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
