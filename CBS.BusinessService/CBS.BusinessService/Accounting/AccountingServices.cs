using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessServices;
using CBS.API.Helper;
using System.Configuration;

namespace CBS.BusinessService.Accounting
{
    public class AccountingServices:BaseService
    {
        private readonly ApiCallerHelper _accountingApiCallerHelper;

        public AccountingServices()
        {
            _accountingApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingBaseUrl"].ToString());

        }
        public async Task<IEnumerable<AccountingRole>> GetAccountingRoles()
        {
            try
            {
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<AccountingRole>>>(APICallHelper.GetAllAccountingRules);
                if (couApiResponse.IsSuccess)
                {
                    if (couApiResponse.ApiResponseData!=null)
                    {
                        return couApiResponse.ApiResponseData.Data;
                    }
                    
                }
                return new List<AccountingRole>();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }



    }
}
