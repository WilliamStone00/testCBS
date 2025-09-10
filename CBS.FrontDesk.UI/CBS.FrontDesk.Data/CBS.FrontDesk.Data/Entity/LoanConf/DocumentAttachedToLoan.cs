using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CBS.FrontDesk.Data.Entity.LoanConf
{
  
    public class DocumentAttachedToLoan
    {
        public string Id { get; set; }
        public string LoanApplicationId { get; set; }
        public string DocumentId { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public DateTime Date { get; set; }
        public string FileExtension { get; set; }
        public LoanApplication LoanApplication { get; set; }
        public Document Document { get; set; }
        public List<HttpPostedFileBase> AttachedFiles { get; set; }
    }
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

    }
}
