

using BusinessServices;
using CBS.API.Helper;
using CBS.API.Helper.APICallHelper;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.RequestManagement;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.UserManagement;
using CBS.FrontDesk.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.RequestLoggerServicesP
{
    public class RateLimiteTrackerLoggerServices : BaseService
    {
        private static readonly Lazy<RateLimitApiHelper> _lazyApiCaller = new Lazy<RateLimitApiHelper>(() =>
        {
            var baseUrl = ConfigurationManager.AppSettings["IdentityServerBaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new InvalidOperationException("❌ IdentityServerBaseUrl is missing from AppSettings.");

            return new RateLimitApiHelper(baseUrl);
        });
        private readonly ApiCallerHelper _identityServerBaseUrl;
        public RateLimiteTrackerLoggerServices()
        {
            _identityServerBaseUrl = new ApiCallerHelper(ConfigurationManager.AppSettings["IdentityServerBaseUrl"].ToString());

        }
        private static RateLimitApiHelper ApiCaller => _lazyApiCaller.Value;

        public async Task<ExecutionMessages> Delete(string id)
        {
            try
            {
                var response = await _identityServerBaseUrl.DeleteAsync<ServiceResponse<bool>>(string.Format(APICallHelper.Get_Delete_RateLimiteTrackerLogger, id));

                if (response.IsSuccess)
                {
                    GetExecutionMessages(response, true, id, MessagesResults.Success,
                        ExecutionProcessOption.DefaultSuccessdMessages, SystemMessageStatus.Success.ToString(), null, response.Message);
                }
                else
                {
                    GetExecutionMessages(null, false, id, MessagesResults.Failed,
                        ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Failed.ToString(), null, response.Message);
                }
            }
            catch (Exception ex)
            {
                GetExecutionMessages(null, false, id, MessagesResults.Failed,
                    ExecutionProcessOption.DefaultFailedMessages, SystemMessageStatus.Error.ToString(), ex, ex.Message);
            }

            return ExecutionMessage;
        }

        public async Task<CustomDataTable> GetDataTableAsync(GetRateLimitTrackerDataTableQuery getRateLimitTrackerDataTable)
        {
            var response = await _identityServerBaseUrl.PostAsync<ResponseObject<CustomDataTable>>(
                APICallHelper.GetAllRateLimiteTrackerLogger,
                getRateLimitTrackerDataTable);

            if (response.IsSuccess && response.ApiResponseData != null)
            {
                return response.ApiResponseData.Data;
            }

            return new CustomDataTable(
                draw: Convert.ToInt32(getRateLimitTrackerDataTable.Options.draw),
                recordsTotal: 0,
                recordsFiltered: 0,
                data: new List<object>(),
                dataTableOptions: getRateLimitTrackerDataTable.Options
            );
        }

        public async Task<RateLimitDashboardDto> GetDasgboardAsync(GetRateLimitDashboardQuery getRateLimitDashboardQuery)
        {
            var response = await _identityServerBaseUrl.PostAsync<ResponseObject<RateLimitDashboardDto>>(
                APICallHelper.DashboardRateLimiteTrackerLogger,
                getRateLimitDashboardQuery);

            if (response.IsSuccess && response.ApiResponseData != null)
            {
                return response.ApiResponseData.Data;
            }

            return new RateLimitDashboardDto();
        }
        public async Task<List<RateLimiteTrackerLogger>> GetRateLimiteTrackerLoggerBylogs_by_key(GetRateLimitLogsByKeyQuery getRateLimitLogsByKey)
        {
            var response = await _identityServerBaseUrl.PostAsync<ResponseObject<List<RateLimiteTrackerLogger>>>(
                APICallHelper.GetRateLimiteTrackerLoggerBylogs_by_key,
                getRateLimitLogsByKey);

            if (response.IsSuccess && response.ApiResponseData != null)
            {
                return response.ApiResponseData.Data;
            }

            return new List<RateLimiteTrackerLogger>();
        }
        public async Task<List<RateLimiteTrackerLogger>> ExportDashboard(GetRateLimitDashboardQuery dashboardQuery)
        {
            var response = await _identityServerBaseUrl.PostAsync<ResponseObject<List<RateLimiteTrackerLogger>>>(
                APICallHelper.ExportRateLimiteTrackerLoggerBylogs,
                dashboardQuery);

            if (response.IsSuccess && response.ApiResponseData != null)
            {
                return response.ApiResponseData.Data;
            }

            return new List<RateLimiteTrackerLogger>();
        }
        //
        public async Task<RateLimiteTrackerLogger> GetRateLimiteTrackerLogger(string id)
        {
            try
            {
                var response = await _identityServerBaseUrl.GetAsync<ResponseObject<RateLimiteTrackerLogger>>(
                    string.Format(APICallHelper.Get_Delete_RateLimiteTrackerLogger, id));

                return response.ApiResponseData?.Data;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to retrieve RateLimiteTrackerLogger by ID", ex);
            }
        }

        public async Task<bool> LogRequestAsync(LogRateLimitTrackerCommand logger)
        {
            try
            {
                logger.ServiceName="BACKOFFICE";
                var result = await ApiCaller.PostAsync<ResponseObject<bool>>(APICallHelper.AddRateLimiteTrackerLogger, logger);
                return result?.Data ?? false;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to log request", ex);
            }
        }

        /// <summary>
        /// Logs a general request.
        /// </summary>
        public async Task LogRequest(string ip, string mac, string computerName, string path, int statusCode, string location, string lat, string lon, string city, string region, string country, string branchid, string branchcode, string branchname, string tel, string fullname, bool isblocked, string reason, bool isAuthenticated, string fullUrl)
        {
            var command = new LogRateLimitTrackerCommand
            {
                IpAddress = ip,
                UserName = mac,
                ComputerName = computerName,
                Path = path,
                StatusCode = statusCode,
                Location = location,
                BlockType = "n/a",
                WarningMessage = "n/a",
                BlockEndTime = DateTime.MinValue,
                FullName=fullname,
                BranchCode=branchcode,
                BranchName=branchname,
                BranchId=branchid,
                IsBlocked=isblocked,
                Reason=reason,
                City=city,
                Country=country,
                Latitude=lat,
                Longitude=lon,
                PhoneNumber=tel,
                Region=region,
                IsAuthenticated=isAuthenticated,
                FullUrl=fullUrl
            };

            await LogRequestAsync(command);
        }

        /// <summary>
        /// Logs a warning when the 80% threshold is reached.
        /// </summary>
        public async Task LogWarning(string ip, string mac, string computerName, string path, string warningMessage, string location)
        {
            var command = new LogRateLimitTrackerCommand
            {
                IpAddress = ip,
                UserName = mac,
                ComputerName = computerName,
                Path = path,
                BlockType = "n/a",
                StatusCode = 429,
                Location = location,
                WarningMessage = warningMessage,
                BlockEndTime = DateTime.MinValue
            };

            await LogRequestAsync(command);
        }

        /// <summary>
        /// Logs a blocked user request.
        /// </summary>
        public async Task LogBlockedUser(string ip, string mac, string computerName, DateTime blockEndTime, string blockType, string location, string reason)
        {
            var command = new LogRateLimitTrackerCommand
            {
                IpAddress = ip,
                UserName = mac,
                ComputerName = computerName,
                BlockType = blockType,
                BlockEndTime = blockEndTime,
                Location = location,
                Path = "n/a",
                IsBlocked=true,
                StatusCode = 429,
                Reason=reason,
                WarningMessage = "Rate limit exceeded",
            };

            await LogRequestAsync(command);
        }


    }

}
