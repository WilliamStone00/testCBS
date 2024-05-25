using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CBS.FrontDesk.UI.Models
{
 
    public class AccountUploadModel
    {
        [Required(ErrorMessage = "Please select an Excel file to upload.")]
        public HttpPostedFileBase ExcelFile { get; set; }
    }
}