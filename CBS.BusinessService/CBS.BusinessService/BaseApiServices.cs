using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Service
{
    public class BaseApiServices : BaseService
    {
        private readonly ApiCallerHelper _BankAPIConfigApiHelper;
        private readonly ApiCallerHelper _AccountingAPIConfigApiHelper;

        public BaseApiServices()
        {
            _BankAPIConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["BankConfigurationBaseUrl"].ToString());
            _AccountingAPIConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingBaseUrl"].ToString());
        }

        public async Task<IEnumerable<Branch>> GetAllBranchesByBankId(string Id)
        {
            try
            {
                var couApiResponse = _BankAPIConfigApiHelper.GetAsync<ResponseObject<List<Branch>>>(APICallHelper.GetAllBranch).Result;
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data.FindAll(x => x.BankId.Equals(Id));
                }
                return new List<Branch>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }

        public async Task<IEnumerable<OperationEvent>> GetAllOperationEvent()
        {
            try
            {
                var couApiResponse = _AccountingAPIConfigApiHelper.GetAsync<ResponseObject<List<OperationEvent>>>(APICallHelper.GetAllOperationEvent).Result;
                if (couApiResponse.IsSuccess)
                {
                    return couApiResponse.ApiResponseData.Data;
                }
                return new List<OperationEvent>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
    }


}
