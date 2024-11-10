
using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using DocumentFormat.OpenXml.EMMA;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounts
{
   
    public class GeneralDailyDashboardServices : BaseService
    {
        private readonly ApiCallerHelper _transactionApiHelper;

        public GeneralDailyDashboardServices()
        {
            _transactionApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());

        }

        

       
        public async Task<GeneralDailyDashboardDto> GetDailyDashboard()
        {
            try
            {
                var dailyDashboardQuery = new GetDailyDashboardQuery { BranchId = GetBranchID(), DateFrom = CurrentDate, DateTo = CurrentDate };


                var response = await _transactionApiHelper.PostAsync<ServiceResponse<GeneralDailyDashboardDto>>(APICallHelper.GetGeneralDailyDashboardByBranch, dailyDashboardQuery);
                if (response.IsSuccess)
                {
                    return response.ApiResponseData.Data;
                }
                return null;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<GeneralDailyDashboardDto> GetDailyDashboard(string datefrom, string dateto,string branchid)
        {
            try
            {
                var dailyDashboardQuery = new GetDailyDashboardQuery { BranchId = branchid, DateFrom = GetDateTime(datefrom), DateTo = GetDateTime(dateto) };
                var response = await _transactionApiHelper.PostAsync<ServiceResponse<GeneralDailyDashboardDto>>(APICallHelper.GetGeneralDailyDashboardByBranch, dailyDashboardQuery);
                if (response.IsSuccess)
                {
                    return response.ApiResponseData.Data;
                }
                return null;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }

        public async Task<List<GeneralDailyDashboardDto>> GetDailyDashboards()
        {
            try
            {
                var dailyDashboardQuery = new GetDailyDashboardQuery {DateFrom = CurrentDate, DateTo = CurrentDate };
                var response = await _transactionApiHelper.PostAsync<ServiceResponse<List<GeneralDailyDashboardDto>>>(APICallHelper.GetGeneralDailyDashboardByBranch, dailyDashboardQuery);
                if (response.IsSuccess)
                {
                    return response.ApiResponseData.Data;
                }
                return null;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<List<GeneralDailyDashboardDto>> GetDailyDashboards(string datefrom, string dateto)
        {
            try
            {
                var dailyDashboardQuery = new GetDailyDashboardQuery { DateFrom = GetDateTime(datefrom), DateTo = GetDateTime(dateto) };
                var response = await _transactionApiHelper.PostAsync<ServiceResponse<List<GeneralDailyDashboardDto>>>(APICallHelper.GetGeneralDailyDashboardByBranch, dailyDashboardQuery);
                if (response.IsSuccess)
                {
                    return response.ApiResponseData.Data;
                }
                return null;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
    }

}
