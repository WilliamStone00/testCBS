using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.ErrorHandler
{
    // ErrorController.cs
    using System.Web.Mvc;

    public class ErrorController : Controller
    {
        public ActionResult BadRequest(string message)
        {
            return BuildView("Bad Request", 400, message);
        }

        public ActionResult Unauthorized(string message)
        {
            return BuildView("Unauthorized", 401, message);
        }

        public ActionResult Forbidden(string message)
        {
            return BuildView("Forbidden", 403, message);
        }

        public ActionResult NotFound(string message)
        {
            return BuildView("Page Not Found", 404, message);
        }

        public ActionResult RequestTimeout(string message)
        {
            return BuildView("Request Timeout", 408, message);
        }

        public ActionResult Conflict(string message)
        {
            return BuildView("Conflict", 409, message);
        }

        public ActionResult Gone(string message)
        {
            return BuildView("Resource Gone", 410, message);
        }

        public ActionResult UnsupportedMediaType(string message)
        {
            return BuildView("Unsupported Media Type", 415, message);
        }

        public ActionResult TooManyRequests(string message)
        {
            return BuildView("Too Many Requests", 429, message);
        }

        public ActionResult InternalServer(string message)
        {
            return BuildView("Internal Server Error", 500, message);
        }

        public ActionResult NotImplemented(string message)
        {
            return BuildView("Not Implemented", 501, message);
        }

        public ActionResult BadGateway(string message)
        {
            return BuildView("Bad Gateway", 502, message);
        }

        public ActionResult ServiceUnavailable(string message)
        {
            return BuildView("Service Unavailable", 503, message);
        }

        public ActionResult GatewayTimeout(string message)
        {
            return BuildView("Gateway Timeout", 504, message);
        }

        public ActionResult CustomError(string message)
        {
            return BuildView("Application Error", 520, message);
        }

        private ActionResult BuildView(string title, int code, string message)
        {
            ViewBag.TitleText = title;
            ViewBag.StatusCode = code;
            ViewBag.Message = message ?? "An unexpected error has occurred.";
            return View("~/Views/Shared/CustomError.cshtml");
        }
    }



}