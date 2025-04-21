using System;



using System.Web.Mvc;
using System.Web.Security;
using System.Web;
using CBS.FrontDesk.Service;
using CBS.API.Helper;
using System.Threading.Tasks;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.Entity;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using Newtonsoft.Json;
using CBS.BusinessService.Session;
using DocumentFormat.OpenXml.Wordprocessing;

namespace CBS.FrontDesk.UI.Controllers
{
    public class AuthenticationController : BaseController
    {
        private readonly IAuthenticationServices _helper;
        private readonly LocalSession _localSession;

        public AuthenticationController(IAuthenticationServices helper, LocalSession localSession)
        {
            _helper = helper;
            _localSession=localSession;
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
            var Geo = await GetGeoLocation();
            model.GeoLocationResponse = Geo;
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
                                CreateToken(userDto, "TSC", userDto.expirationTime);
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
                                    CreateToken(userDto, "TSC", userDto.expirationTime);
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
                                    CreateToken(userDto, "TSC", userDto.expirationTime);
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
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> ResolveMultipleSessions(SessionAuth model)
        {
            if (!ModelState.IsValid)
            {
                // Send back to the GET action with preserved parameters
                return RedirectToAction("ResolveMultipleSessions", new
                {
                    username = model.UserName,
                    message = "❌ Please provide a valid session recovery code.",
                    count = model.Counts
                });
            }

            try
            {
                var result = await _localSession.InvalidateAllActivetUsers(model);

                if (result.Result)
                {
                    TempData["Success"] = true;
                    TempData["Message"] = "✅ All other sessions have been terminated. Please login again.";
                    return RedirectToAction("Login", "Authentication");
                }

                // Redirect back with error message
                return RedirectToAction("ResolveMultipleSessions", new
                {
                    username = model.UserName,
                    message = result.MessageString ?? "⚠️ Failed to terminate other sessions. Please try again.",
                    count = model.Counts
                });
            }
            catch (Exception ex)
            {
                return RedirectToAction("ResolveMultipleSessions", new
                {
                    username = model.UserName,
                    message = "🚨 An error occurred. Please try again later or contact support.",
                    count = model.Counts
                });
            }
        }




        [AllowAnonymous]
        public ActionResult ResolveMultipleSessions(string username, string message, int count)
        {
            ViewBag.Username = username;
            ViewBag.Message = message;
            ViewBag.SessionCount = count;
            var sessionAuth = new SessionAuth { Counts=count, UserName=username, SessionRecoveryCode=string.Empty };
            return View(sessionAuth);
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
        public async Task<GeoLocationResponse> GetGeoLocation()
        {
            try
            {

                using (var client = new HttpClient())
                {
                    // Set the Accept header to "application/json"
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var response = await client.GetStringAsync($"https://ipinfo.io/geo");
                    var locationData = JsonConvert.DeserializeObject<GeoLocationResponse>(response);
                    return locationData;

                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
             
        }
        [AllowAnonymous]
        public async Task<ActionResult> Logout()
        {
            await _helper.Logout();
            await PerformLogoutAsync();
            return RedirectToAction("Login");
        }

        //public async Task<ActionResult> Logout()
        //{

        //    FormsAuthentication.SignOut();
        //    // List of all cookies to clear
        //    var cookieNames = new[]{ "BranchObject", "AuthUser", "CBS4U", "CBS4U_MFA", "MFA", "PWD", "ASP.NET_SessionId", "EncryptedJWToken"};
        //    await _helper.Logout();

        //    foreach (var cookieName in cookieNames)
        //    {
        //        if (Request.Cookies[cookieName] != null)
        //        {
        //            var cookie = new HttpCookie(cookieName)
        //            {
        //                Expires = DateTime.Now.AddYears(-1), // Set expiration date in the past
        //                Value = string.Empty // Clear the value
        //            };
        //            Response.Cookies.Add(cookie);
        //        }
        //    }
        //    Session.Remove("EncryptedJWToken");

        //    // Abandon the session
        //    Session.Abandon();
        //    Session.Clear();
        //    Session.RemoveAll();
        //    // Redirect to login page
        //    return RedirectToAction("Login");
        //}
    }
}
