using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CBS.FrontDesk.Data.Entity.Accounting
{
    public class AddDocumentUploadedCommand
    {
        public HttpPostedFileBase FormFiles { get; set; }
        public string OperationID { get; set; }
        public string DocumentId { get; set; }
        public string DocumentType { get; set; }
        public string ServiceType { get; set; }
        public string CallBackBaseUrl { get; set; }
        public string CallBackEndPoint { get; set; }
        public string RemoteFilePath { get; set; }
           public bool IsSynchronus { get; set; }
    }
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);


    public class APICallBackRespose
    {
        public CallBackRespose data { get; set; }
        public List<object> errors { get; set; }
        public int statusCode { get; set; }
        public string statusDescription { get; set; }
        public string message { get; set; }
        public string status { get; set; }
    }


    public class CallBackRespose
    {
        public string id { get; set; }
        public string urlPath { get; set; }
        public string documentName { get; set; }
        public string extension { get; set; }
        public string baseUrl { get; set; }
        public string operationId { get; set; }
        public string documentId { get; set; }
        public string fullPath { get; set; }
        public string serviceType { get; set; }
        public string callBackBaseUrl { get; set; }
        public string callBackEndPoint { get; set; }
        public string remoteFilePath { get; set; }
        public string documentType { get; set; }
    }
}
