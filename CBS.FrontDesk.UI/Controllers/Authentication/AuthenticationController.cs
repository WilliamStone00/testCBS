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
    public class AuthenticationController : BaseController
    {
        private readonly IAuthenticationServices _helper;

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
                if (result.Data is UserDto userDto)
                {
                    if (!userDto.IsBlocked)
                    {
                        try
                        {
                            if (userDto.ChangePasswordOnFirstLogin)
                            {
                                Session["PWD"] = "PWD";
                                CreateToken(userDto, userDto.expirationTime);
                                //CreateToken(userDto, "CHANGE_PWD", userDto.expirationTime, false, true);
                                var url = Url.Action("FLoginChangePassword", "UserManagement", new
                                {
                                    serviceoption = "USER",
                                    KEY = userDto.id,
                                    secrete = userDto.refreshToken,
                                    usersecreteid = Guid.NewGuid(),
                                    path = $"{userDto.firstName}_{userDto.lastName}",
                                    userName = userDto.userName
                                });
                                Session["CHPWDUrl"] = url;
                                return RedirectToLocal(url);
                            }
                            else
                            {
                                if (userDto.isMFA)
                                {
                                    Session["MFA"] = "MFA";
                                    CreateToken(userDto,userDto.expirationTime);
                                    var url = Url.Action("Index", "MFAVerification", new
                                    {
                                        serviceoption = "MFA",
                                        KEY = userDto.id,
                                        secrete = userDto.refreshToken,
                                        usersecreteid = Guid.NewGuid(),
                                        email = userDto.email,
                                        fullname = userDto.firstName,
                                        returnUrl
                                    });
                                    Session["MFAUrl"] = url;
                                    return Redirect(url);
                                }
                                else
                                {
                                    //CreateToken(userDto, "CBS4U", userDto.expirationTime, false, false);
                                    CreateToken(userDto, userDto.expirationTime);
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
                            ViewBag.Message = "An error occurred while processing your request. Please try again later.";
                            return View("Login", model);
                        }
                    }
                    else
                    {
                        ViewBag.Success = false;
                        ViewBag.Message = "Your account is blocked. Please contact support.";
                        return View("Login", model);
                    }
                }
            }

            // If we got this far, something failed, redisplay form with error message
            ViewBag.Success = false;
            ViewBag.Message = result.MessageString ?? "There was a connection issue. Please try again later.";
            return View("Login", model);
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            // Redirect to a local URL if it's valid
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

            // List of all cookies to clear
            var cookieNames = new[]{ "BranchObject", "AuthUser", "CBS4U", "CBS4U_MFA", "MFA", "PWD", "ASP.NET_SessionId", "EncryptedJWToken"};

            foreach (var cookieName in cookieNames)
            {
                if (Request.Cookies[cookieName] != null)
                {
                    var cookie = new HttpCookie(cookieName)
                    {
                        Expires = DateTime.Now.AddYears(-1), // Set expiration date in the past
                        Value = string.Empty // Clear the value
                    };
                    Response.Cookies.Add(cookie);
                }
            }
            Session.Remove("EncryptedJWToken");

            // Abandon the session
            Session.Abandon();
            Session.Clear();
            Session.RemoveAll();

            // Redirect to login page
            return RedirectToAction("Login");
        }
    }
}
