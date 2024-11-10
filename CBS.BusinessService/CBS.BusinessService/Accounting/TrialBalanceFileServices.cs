using CBS.API.Helper;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using CBS.FrontDesk.Service;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting
{
 

    public class TrialBalanceFileServices : BaseApiServices
    {
        private readonly ApiCallerHelper _loanConfigApiHelper;

 

        public TrialBalanceFileServices()
        {
            _loanConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingBaseUrl"].ToString());
           
        }

 
        public async Task<FrontDesk.Data.Entity.Accounting.TrialBalanceFile> GetTrialBalanceFile(string id)
        {
            try
            {
                var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<FrontDesk.Data.Entity.Accounting.TrialBalanceFile>>(string.Format(APICallHelper.Get_Update_Delete_TrialBalanceFile, id));
                if (cusResponseObject.IsSuccess)
                {
                    return cusResponseObject.ApiResponseData.Data;
                }
                return null;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }

 
 
        public async Task<List<FrontDesk.Data.Entity.Accounting.TrialBalanceFile>> GetAllTrialBalanceFile()
        {
            try
            {
          
                    var cusResponseObject = await _loanConfigApiHelper.GetAsync<ResponseObject<List<FrontDesk.Data.Entity.Accounting.TrialBalanceFile>>>(APICallHelper.Get_TrialBalanceFile);
                    if (cusResponseObject.IsSuccess)
                    {
                        return cusResponseObject.ApiResponseData.Data;
                    }
                


            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
            return null;
        }
    }
}
