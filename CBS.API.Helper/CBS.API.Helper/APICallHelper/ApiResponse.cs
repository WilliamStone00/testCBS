using CBS.FrontDesk.Data.Entity.DailyCollectionEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CBS.API.Helper
{
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public T ApiResponseData { get; set; }

       
    }

    public class ServiceResponseXX<T>
    {
        public object Data { get; set; }
        public List<string> Errors { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string StatusDescription { get; set; }
        public string Message { get; set; }
        public bool Status { get; set; }
    }

    public class ServiceResponseDailySaverUploadResult
    {
        public DailySaverUploadResult Data { get; set; }
        public List<string> Errors { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string StatusDescription { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }

        public ServiceResponseDailySaverUploadResult Failed(string v)
        {
            this.StatusDescription = v;
            this.Status = "Failed";
            this.Message= v;
            return this;
        }
    }
}
