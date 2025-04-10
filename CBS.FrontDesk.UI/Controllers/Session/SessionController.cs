using CBS.API.Helper;
using CBS.BusinessService.Session;
using CBS.FrontDesk.Data.UserManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Session
{
    // Controller: SessionController.cs
    public class SessionController : BaseController
    {
        private readonly LocalSession _localSession;
        private static int _cachedTimeout = 5; // Default in case DB call fails

        public SessionController()
        {
            _localSession = new LocalSession();
        }

        [HttpGet]
        public async Task<JsonResult> GetIdleTimeout()
        {
            
            var idleTime = await _localSession.GetCurrentIdletimeByBranch();

            int timeout = 5; // fallback timeout in minutes
            if (idleTime != null && idleTime.IdleDuration.TotalMinutes > 0)
            {
                timeout = (int)idleTime.IdleDuration.TotalMinutes;
                _cachedTimeout = timeout; // store for reuse
            }

            return Json(new { timeout }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult ExtendSessionTimeout()
        {
            Session.Timeout = _cachedTimeout;
            return Json(new { success = true, timeout = Session.Timeout }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult RegisterIdleEvent()
        {
            // Register idle timeout event to DB (for tracking or analytics)
            return new HttpStatusCodeResult(200);
        }


        public ActionResult Locked()
        {
            Session["SessionUnlocked"] = false; // Ensure it's locked
            ViewBag.Username = Session["UserName"]?.ToString();
            return View(new SessionAuth { UserName = ViewBag.Username, SessionRecoveryCode = string.Empty });
        }

        [HttpPost]
        public JsonResult ValidateRecoveryCode(string recoveryCode)
        {
            string userId = Session["UserID"]?.ToString();
            string username = Session["Username"]?.ToString();
            string sessionCode = Session["SessionCode"]?.ToString();

            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(recoveryCode) ||
                string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(sessionCode))
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid recovery session parameters. Logging out...",
                    redirect = true // 👈 this flag tells the frontend to redirect
                });
            }

            var sessionResult = _localSession.GetUserCurrentsession(sessionCode, username);

            if (sessionResult == null)
            {
                Session.Clear();
                Session.Abandon();
                return Json(new
                {
                    success = false,
                    message = "Session expired. Logging out...",
                    redirect = true // 👈 same for expired
                });
            }

            Session["SessionUnlocked"] = true;
            return Json(new { success = true });
        }

    }
}