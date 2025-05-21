using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.ErrorHandler
{
    [AllowAnonymous]
    public class ErrorController : BaseController
    {
        public ActionResult Index()
        {
            ViewBag.TitleText = "Unexpected Error";
            ViewBag.Message = "An unexpected error occurred.";
            ViewBag.StatusCode = 500;
            return View("~/Views/Shared/Error.cshtml");
        }

        public ActionResult Show(int id)
        {
            string message;
            string suggestion;
            string userIp = Request.UserHostAddress ?? "Unknown IP";

            switch (id)
            {
                case 400:
                    message = "Bad Request. The server could not understand the request due to invalid syntax.";
                    suggestion = "Please verify the URL and the data you are sending. Contact support if the issue persists.";
                    break;
                case 401:
                    message = "Unauthorized Access. You do not have the necessary credentials to access this resource.";
                    suggestion = "Please log in with the appropriate credentials. If the problem continues, contact your system administrator.";
                    break;
                case 403:
                    message = "Forbidden. Access to this resource is denied.";
                    suggestion = "You do not have permission to view this page. Please contact your administrator if you believe this is an error.";
                    break;
                case 404:
                    message = "Page Not Found. The requested resource could not be located.";
                    suggestion = "Check the URL for typos or use the site search to find the page you are looking for.";
                    break;
                case 408:
                    message = "Request Timeout. The server took too long to respond.";
                    suggestion = "Please try refreshing the page or check your internet connection.";
                    break;
                case 429:
                    message = "Too Many Requests. You have exceeded the allowed rate limit.";
                    suggestion = $"Your IP ({userIp}) has been temporarily blacklisted for 30 minutes due to excessive requests. Please wait and try again later.";
                    break;
                case 500:
                    message = "Internal Server Error. The server encountered an unexpected condition.";
                    suggestion = "Try again later or contact technical support if the issue persists.";
                    break;
                case 502:
                    message = "Bad Gateway. The server received an invalid response from the upstream server.";
                    suggestion = "Please try again later. The issue may resolve on its own.";
                    break;
                case 503:
                    message = "Service Unavailable. The server is temporarily unable to handle the request.";
                    suggestion = "The server may be undergoing maintenance. Please try again after some time.";
                    break;
                case 504:
                    message = "Gateway Timeout. The server took too long to respond to your request.";
                    suggestion = "The server may be busy or temporarily down. Try refreshing the page or come back later.";
                    break;
                default:
                    message = "An unexpected error occurred. Something went wrong while processing your request.";
                    suggestion = "Please try again later or contact technical support.";
                    break;
            }

            ViewBag.TitleText = $"Error {id}";
            ViewBag.Message = message;
            ViewBag.Suggestion = suggestion;
            ViewBag.StatusCode = id;
            ViewBag.UserIP = userIp;

            return View("~/Views/Shared/Error.cshtml");
        }
    }



}