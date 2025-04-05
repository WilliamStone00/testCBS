using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CBS.FrontDesk.UI.Filter
{
    using System;
    using System.Web;
    using System.Web.Mvc;
    using System.Net;

    public class GlobalExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled)
                return;

            var exception = filterContext.Exception;
            var httpException = exception as HttpException;
            int statusCode = httpException?.GetHttpCode() ?? (int)HttpStatusCode.InternalServerError;

            string titleText;
            string friendlyMessage;

            switch (statusCode)
            {
                case 400:
                    titleText = "Bad Request";
                    friendlyMessage = "Your request could not be understood by the server.";
                    break;
                case 401:
                    titleText = "Unauthorized";
                    friendlyMessage = "You are not authorized to access this page.";
                    break;
                case 403:
                    titleText = "Forbidden";
                    friendlyMessage = "You do not have permission to access this resource.";
                    break;
                case 404:
                    titleText = "Page Not Found";
                    friendlyMessage = "The page you're looking for does not exist.";
                    break;
                case 408:
                    titleText = "Request Timeout";
                    friendlyMessage = "The server timed out waiting for your request.";
                    break;
                case 502:
                    titleText = "Bad Gateway";
                    friendlyMessage = "The server received an invalid response.";
                    break;
                case 503:
                    titleText = "Service Unavailable";
                    friendlyMessage = "The service is temporarily unavailable. Please try again later.";
                    break;
                case 504:
                    titleText = "Gateway Timeout";
                    friendlyMessage = "The server timed out waiting for a gateway response.";
                    break;
                case 500:
                default:
                    statusCode = 500;
                    titleText = "Server Error";
                    friendlyMessage = "An unexpected error occurred. Please try again later.";
                    break;
            }

            // Log error
            System.Diagnostics.Debug.WriteLine("[ERROR] " + exception.Message);
            System.Diagnostics.Debug.WriteLine("[STACKTRACE] " + exception.StackTrace);

            var request = filterContext.HttpContext.Request;
            var response = filterContext.HttpContext.Response;

            bool isAjax = request.IsAjaxRequest()
                          || request.AcceptTypes?.Any(t => t.Contains("json")) == true;

            filterContext.ExceptionHandled = true;



            if (isAjax)
            {
                filterContext.Result = new JsonResult
                {
                    Data = new
                    {
                        success = false,
                        error = true,
                        statusCode = statusCode,
                        message = friendlyMessage,
                        detail = exception.Message
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            else
            {            // Safely set the status code (only if headers not sent)
                try
                {
                    if (!response.HeadersWrittenSafely())
                    {
                        response.Clear();
                        response.TrySkipIisCustomErrors = true;
                        response.StatusCode = statusCode;
                    }
                }
                catch
                {
                    // In case of error when setting response, fallback silently
                }
                filterContext.Controller.ViewBag.TitleText = titleText;
                filterContext.Controller.ViewBag.StatusCode = statusCode;
                filterContext.Controller.ViewBag.Message = friendlyMessage;
                filterContext.Controller.ViewBag.StackTrace = exception.ToString();

                filterContext.Result = new ViewResult
                {
                    ViewName = "~/Views/Shared/CustomError.cshtml",
                    ViewData = new ViewDataDictionary(filterContext.Controller.ViewData)
                };
            }
        }
    }

    // ✅ Extension to detect if headers are already written
    public static class HttpResponseExtensions
    {
        public static bool HeadersWrittenSafely(this HttpResponseBase response)
        {
            try
            {
                return !response.IsClientConnected; // Best we can do in WebForms/MVC
            }
            catch
            {
                return true; // Assume headers were sent if error checking
            }
        }
    }
}