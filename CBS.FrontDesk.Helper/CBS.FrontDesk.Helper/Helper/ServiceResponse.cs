using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.CMoney;
using System;
using System.Collections.Generic;

namespace CBS.FrontDesk.Helper
{
    // Represents a service response with generic data and error information.
    public class ServiceResponse<T>
    {
        public T Data { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public List<string> Errors { get; set; } = new List<string>();

        // Constructor for a successful response with default values.
        private ServiceResponse(T data)
        {
            Data = data;
        }
        private ServiceResponse()
        {
           
        }
        // Constructor for a failed response with status code and error messages.
        private ServiceResponse(int statusCode, List<string> errors)
        {
            StatusCode = statusCode;
            Errors = errors;
        }
        // Constructor for a failed response with status code and error messages.
        /// <summary>
        /// Initializes a new instance of the ServiceResponse class with the specified status code and data payload.
        /// </summary>
        /// <param Name="statusCode">The HTTP status code of the response.</param>
        /// <param Name="data">The data payload of the response.</param>
        private ServiceResponse(int statusCode, T data)
        {
            StatusCode = statusCode;
            Data = data;
        }

        // Constructor for a failed response with status code and a single error message.
        private ServiceResponse(int statusCode, string errorMessage)
        {
            StatusCode = statusCode;
            Errors = new List<string> { errorMessage };
        }

        // Constructor for a successful response with custom message.
        private ServiceResponse(int statusCode, T data, string message)
        {
            StatusCode = statusCode;
            Data = data;
            Message = message;
        }

        // Constructor for a successful response with custom message, description, and status.
        private ServiceResponse(int statusCode, T data, string message, string description, string status)
        {
            StatusCode = statusCode;
            Data = data;
            Message = message;
            Description = description;
            Status = status;
        }

        // Constructor for a response with custom status code, message, description, and status.
        private ServiceResponse(int statusCode, string message, string description, string status)
        {
            StatusCode = statusCode;
            Message = message;
            Description = description;
            Status = status;
        }

        // Checks if the response is successful (no errors).
        public bool Success => Errors == null || Errors.Count == 0;

        // Factory method to create a response for an exception.
        public static ServiceResponse<T> ReturnException(Exception ex)
        {
            return new ServiceResponse<T>(500, $"An unexpected fault happened. Error: {ex.Message}", "Failed with Internal Server Error", "FAILED");
        }

        // Factory method to create a response for a failed operation with status code and errors.
        public static ServiceResponse<T> ReturnFailed(int statusCode, List<string> errors)
        {
            return new ServiceResponse<T>(statusCode, errors);
        }

        // Factory method to create a response for a failed operation with a single error message.
        public static ServiceResponse<T> ReturnFailed(int statusCode, string errorMessage)
        {
            return new ServiceResponse<T>(statusCode, errorMessage);
        }

        // Factory method to create a success response with default values.
        public static ServiceResponse<T> ReturnSuccess()
        {
            return new ServiceResponse<T>(default(T));
        }

        // Factory method to create a success response with a custom message.
        public static ServiceResponse<T> ReturnSuccess(string message)
        {
            return new ServiceResponse<T>(200, message, "Transaction was successful", "SUCCESS");
        }

        // Factory method to create a success response with status code and data.
        public static ServiceResponse<T> ReturnResultWith200(T data)
        {
            return new ServiceResponse<T>(200, data,string.Empty, "Transaction was successful", "SUCCESS");
        }
        public static ServiceResponse<T> ReturnResultWith200(T data,string message)
        {
            return new ServiceResponse<T>(200, data,message,"Transaction was successful","SUCCESS");
        }

        // Factory method to create a success response with status code, data, and a custom message.
        public static ServiceResponse<T> ReturnResultWith201(T data, string message)
        {
            return new ServiceResponse<T>(201, data, message, "Transaction was successful", "SUCCESS");
        }

        // Factory method to create a success response with a status code and no data.
        public static ServiceResponse<T> ReturnResultWith204()
        {
            return new ServiceResponse<T>(204, default(T));
        }

        // Factory method to create a 500 Internal Server Error response.
        public static ServiceResponse<T> Return500()
        {
            return new ServiceResponse<T>(500, "An unexpected fault happened. Try again later.");
        }

        // Factory method to create a 500 Internal Server Error response with an exception.
        public static ServiceResponse<T> Return500(Exception ex, string message=null)
        {
            return new ServiceResponse<T>(500, $"An unexpected fault happened. {message} Error: {ex.Message}", "Failed with Internal Server Error", "FAILED");
        }
        public static ServiceResponse<T> Return500(string message)
        {
            return new ServiceResponse<T>(500, $"{message}", "Failed with Internal Server Error", "FAILED");
        }

        // Factory method to create a 409 Conflict response with a custom message.
        public static ServiceResponse<T> Return409(string message)
        {
            return new ServiceResponse<T>(409, message, "Failed, Record already exists. Operation was cancelled", "FAILED");
        }

        // Factory method to create a 409 Conflict response with a default message.
        public static ServiceResponse<T> Return409()
        {
            return new ServiceResponse<T>(409, "Record already exists", "Failed, Record already exists. Operation was cancelled", "FAILED");
        }

        // Factory method to create a 422 Unprocessable Entity response with a custom message.
        public static ServiceResponse<T> Return422(string message)
        {
            return new ServiceResponse<T>(422, new List<string> { message });
        }

        // Factory method to create a 404 Not Found response with a default message.
        public static ServiceResponse<T> Return404()
        {
            return new ServiceResponse<T>(404, "Record not found", "Failed. Process terminated.", "FAILED");
        }

        // Factory method to create a 404 Not Found response with a custom message.
        public static ServiceResponse<T> Return404(string message)
        {
            return new ServiceResponse<T>(404, message, "Failed. Process terminated.", "FAILED");
        }
        // Factory method to create a Forbiden response with a custom message.
        public static ServiceResponse<T> Return403(string message)
        {
            return new ServiceResponse<T>(403, message, "Operation is forbidden. Process terminated.", "FAILED");
        }
        public static ServiceResponse<T> Return403(T data)
        {
            return new ServiceResponse<T>(403, "Operation is forbidden.", "Failed. Process terminated.", "FAILED");
        }
        // Factory method to create a Forbiden response with a custom message.
        public static ServiceResponse<T> Return403()
        {
            return new ServiceResponse<T>(403, "Operation is forbidden.", "Failed. Process terminated.", "FAILED");
        }
        public static ServiceResponse<T> Return403(T data,string message)
        {
            return new ServiceResponse<T>(403,data,message, "Operation is forbidden.", "FAILED");
        }

        public static implicit operator ServiceResponse<T>(ApiResponse<ServiceResponse<CMoneyMembersActivationAccount>> v)
        {
            throw new NotImplementedException();
        }
    }

    // Represents a generic response object with data, errors, and status information.
    //public class ResponseObject<T> where T : new()
    //{
    //    public T Data { get; set; }
    //    public List<string> Errors { get; set; }
    //    public int StatusCode { get; set; } = 200;
    //    public string StatusDescription { get; set; }
    //    public string Message { get; set; }
    //    public string Status { get; set; }

    //    // Constructor for creating a response object with optional parameters.
    //    public ResponseObject(T data = default, int statusCode = 200, string description = null, string message = null, string status = null, List<string> errors = null)
    //    {
    //        Data = data;
    //        StatusCode = statusCode;
    //        StatusDescription = description;
    //        Status = status;
    //        Message = message;
    //        Errors = errors ?? new List<string>();
    //    }
    //}

    public class ResponseObject<T> where T : new()
    {
        public T Data { get; set; }
        public List<string> Errors { get; set; }
        public int StatusCode { get; set; } = 200;
        public string StatusDescription { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }

        public ResponseObject(T data = default, int statusCode = 200, string description = null, string message = null, string status = null, List<string> errors = null)
        {
            Data = data;
            StatusCode = statusCode;
            StatusDescription = description;
            Status = status;
            Message = message;
            Errors = errors ?? new List<string>();
        }
    }


}
