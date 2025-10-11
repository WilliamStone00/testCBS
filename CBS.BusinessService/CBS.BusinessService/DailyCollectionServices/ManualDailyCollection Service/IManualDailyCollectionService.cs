using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.ManualDailycollection;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.BusinessService.DailyCollectionServices.ManualDailyCollection_Service
{      /// <summary>
       /// Service interface for the MVC layer to call API endpoints for ManualDailyCollection.
       /// NOTE: This is a frontend service that *calls* backend API endpoints (via ApiCallerHelper).
       /// It does NOT interact directly with EF entities or repositories (those live on the API side).
       /// </summary>
    public interface IManualDailyCollectionService
    {
        /// <summary>
        /// Uploads the manual entry file to the backend API for validation/processing.
        /// </summary>
        /// <param name="file">Uploaded file from the MVC form (HttpPostedFileBase for classic ASP.NET MVC).</param>
        Task<ApiResponse<ServiceResponse<FileUploadResponse>>> UploadManualEntryFileAsync(HttpPostedFileBase file);

        /// <summary>
        /// Gets details of a single uploaded file (member rows) by fileUploadId.
        /// </summary>
        Task<FileUploadResponse> GetFileDetailsByIdAsync(string fileUploadId);

        /// <summary>
        /// Gets a list (summaries) of files filtered by processing Status (Pending/Processed/Failed).
        /// </summary>
        Task<List<FileUploadResponse>> GetFilesByProcessingStatusAsync(string processingStatus);

        Task<List<FileUploadResponse>> GetAllFilesAsync();

        Task<List<SelectListItem>> GetDailyCollectorsAsync(string customerType);

        /// <summary>
        /// Delete a file by its fileUploadId.
        /// Returns an execution message / api response similar to other services in the system.
        /// </summary>
        Task<ExecutionMessages> DeleteFileByIdAsync(string fileUploadId);
    }
}

