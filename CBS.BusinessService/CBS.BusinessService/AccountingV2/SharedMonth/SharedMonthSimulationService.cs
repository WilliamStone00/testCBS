using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.AccountingV2;
using CBS.FrontDesk.Data.Entity.AccountingV2.SharedMonth;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.AccountingV2.SharedMonth
{
    public class SharedMonthSimulationService : BaseService
    {

        private readonly ApiCallerHelper _apiCallerHelper;
        private readonly ApiCallerHelper _apiCallerHelperT;



        public SharedMonthSimulationService()
        {

            _apiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingV2BaseUrl"].ToString());
            _apiCallerHelperT = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());

        }


        public async Task<bool> ActivateSimulationAsync(InstrestCalculation model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            try
            {
                var apiResponse = await _apiCallerHelperT.PostAsync<ResponseObject<InstrestCalculation>>(
                    APICallHelper.ActivateShareMonthSimulation,
                    model
                );

                // Return true if API response is not null and indicates success
                return apiResponse != null && apiResponse.IsSuccess; // Assuming ApiResponse has IsSuccess property
            }
            catch (Exception ex)
            {
                // Optional: log the error
                return false; // Return false if there was an exception
            }
        }


        public async Task<List<ShareMonthPsiReportLineDto>> GetSimulationData(SharedMonthSimulation model)
        {

            if (model == null)
                throw new ArgumentNullException(nameof(model));
            model.ProductId = "0000";
            model.StartPeriodKey = "jan";
            model.EndPeriodKey = "feb";

            var apiResponse =
                await _apiCallerHelper.PostAsync<ResponseObject<List<ShareMonthPsiReportLineDto>>>(
                    APICallHelper.InterestDistributionSimulation1,
                    model
                );

            return apiResponse?.ApiResponseData?.Data ?? new List<ShareMonthPsiReportLineDto>();
        }
        public async Task<JobRequestListResponse> GetRequestDataAsync(SimulationFilterRequest model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.SimulatedByUserId = GetUserID();
            model.Name = "string";

            var apiResponse =
                await _apiCallerHelperT.PostAsync<ResponseObject<JobRequestListResponse>>(
                    APICallHelper.DataTableForRequestSimulation,
                    model
                );

            // 🔥 SAFE deserialization
            if (apiResponse?.ApiResponseData?.Data == null)
                return new JobRequestListResponse
                {
                    Items = new List<JobRequestItem>()
                };

            return apiResponse.ApiResponseData.Data;
        }

        public async Task<ApiResponse<ResponseObject<CreateSimulations>>> CreateShareMonthSimulationAsync(CreateSimulations model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            model.Date = DateTime.Now;
            model.SimulatedByUserName = GetUserFullName();
            model.SimulatedByUserId = GetUserID();

            try
            {
                var apiResponse =
                    await _apiCallerHelperT.PostAsync<ResponseObject<CreateSimulations>>(
                        APICallHelper.CreateShareMonthSimulation, // API endpoint
                        model                                      // FULL model posted
                    );

                return apiResponse;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<ApiResponse<ServiceResponse<SharedMonthFileReponse>>> SharedMonthUpload(SharedMonthFileupload model)
        {
            try
            {
                if (model.file == null)
                {
                    return new ApiResponse<ServiceResponse<SharedMonthFileReponse>>
                    {
                        IsSuccess = false,
                        ApiResponseData = null,
                        Message = "❌ No file provided."
                    };
                }

                if (string.IsNullOrWhiteSpace(model.BranchId))
                {
                    return new ApiResponse<ServiceResponse<SharedMonthFileReponse>>
                    {
                        IsSuccess = false,
                        ApiResponseData = null,
                        Message = "❌ Branch is required."
                    };
                }

                var endpoint = $"{APICallHelper.SharedMonthUpload}";

                var result = await _apiCallerHelper.UploadSharedMonthFileAsync<ServiceResponse<SharedMonthFileReponse>>(
                    model.file,
                    model.BranchId,
                    model.Month,
                    model.FileType,
                    endpoint
                );

                return result;
            }
            catch (Exception ex)
            {
                return new ApiResponse<ServiceResponse<SharedMonthFileReponse>>
                {
                    IsSuccess = false,
                    ApiResponseData = null,
                    Message = $"❌ Error while uploading shared month file: {ex.Message}"
                };
            }
        }


    }
}
