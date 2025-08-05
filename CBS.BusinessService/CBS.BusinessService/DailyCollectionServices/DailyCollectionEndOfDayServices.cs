using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.DailyCollectionEntities;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CBS.BusinessService.DailyCollectionServices
{
    public class DailyCollectionEndOfDayServices
    {
        private readonly ApiCallerHelper _savingConfigApiHelper;

        public DailyCollectionEndOfDayServices()
        {
   
            _savingConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
        }

        public async Task<List<FileUploadCollectorDto>> GetUploadedFiles(string processStatus="Extracted")
        {
  
            string _baseUrl = string.Format(APICallHelper.GetAllDailyOperationFileUploadsByProcessingStatus, processStatus);
            var apiResponse = await _savingConfigApiHelper.GetAsync<ResponseObject<List<FileUploadCollectorDto>>>(_baseUrl);
            if (apiResponse.IsSuccess)
            {
                return apiResponse.ApiResponseData.Data;
            }
            else
            {
                return new List<FileUploadCollectorDto>();
            }
        }

        public async Task<ManualEntryDailyCollectorUploadSummaryDto> GetUploadedFile(string fileUploadId)
        {

            string _baseUrl = string.Format(APICallHelper.ViewFileByFileUploadId, fileUploadId);
            var apiResponse = await _savingConfigApiHelper.GetAsync<ResponseObject<ManualEntryDailyCollectorUploadSummaryDto>>(_baseUrl);
            if (apiResponse.IsSuccess)
            {
                return apiResponse.ApiResponseData.Data;
            }
            else
            {
                return new ManualEntryDailyCollectorUploadSummaryDto();
            }
        }

    }
}
