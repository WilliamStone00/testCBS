using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace CBS.FrontDesk.Data.Entity.DailyCollectionEntities
{
    public class  UploadDailyCollectorData
    {
        [Required(ErrorMessage = "Please select an Excel file to upload.")]
        public HttpPostedFileBase FormFile { get; set; }
        public string BranchId { get; set; }
        public string CollectorId { get; set; }
        public string AccountId { get; set; }
        public string CollectorName { get; set; }

    }
}
