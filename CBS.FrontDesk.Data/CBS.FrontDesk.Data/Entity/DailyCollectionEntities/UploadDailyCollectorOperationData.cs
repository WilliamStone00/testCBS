using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace CBS.FrontDesk.Data.Entity.DailyCollectionEntities
{
    public class UploadDailyCollectorOperationData
    {
        [Required(ErrorMessage = "Please select an Excel file to upload.")]
        public HttpPostedFileBase ExcelFile { get; set; }

        public string CollectorId { get; set; }

    }
}
