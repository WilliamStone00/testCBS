using System;
using System.Collections.Generic;
using System.Linq;
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
    public int StatusCode { get; set; }
    public object StatusDescription { get; set; }
    public object Message { get; set; }
    public object Status { get; set; }
}
}
