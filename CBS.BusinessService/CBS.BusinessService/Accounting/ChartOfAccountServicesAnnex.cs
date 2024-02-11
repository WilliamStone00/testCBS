using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.BusinessService.Accounting
{

    public class ChartOfAccountServicesAnnex : BaseService
    {
        private readonly ApiCallerHelper _accountingApiCallerHelper;

        public ChartOfAccountServicesAnnex()
        {
            _accountingApiCallerHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["AccountingBaseUrl"].ToString());

        }
        public async Task<IEnumerable<StringValues>> GetChartOfAccounts()
        {
            try
            {
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<ChartOfAccount>>>(APICallHelper.GetAllChartOfAccount);
                return ProcessApiResponseForChartOfAccount(couApiResponse.ApiResponseData);
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }

        public async Task<SelectList> GetEventAttributeByOperationTypeID(string productid = null)
        {
            try
            {
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<OperationEventAttributeDto>>>(string.Format(APICallHelper.GetOperationEventAttributes, productid));
                return ProcessApiResponseForOperationEventAttribute(couApiResponse.ApiResponseData);
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }

        public async Task<SelectList> GetEventAttributeByOperationTypeID()
        {
            try
            {
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<OperationEventAttributeDto>>>(string.Format(APICallHelper.GetAllOperationEventAttributes));
                return ProcessApiResponseForOperationEventAttribute(couApiResponse.ApiResponseData);
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }

        private IEnumerable<StringValues> ProcessApiResponseForChartOfAccount(ResponseObject<List<ChartOfAccount>> couApiResponse)
        {
            if (couApiResponse!=null)
            {
                if (couApiResponse.StatusCode == 200 && couApiResponse != null)
                {
                    return couApiResponse.Data.Select(a => new StringValues
                    {
                        Text = $"{a.AccountNumber}-{a.LabelEn}",
                        Value = a.Id
                    });
                }
            }
        

            return new List<StringValues>();
        }

        private SelectList ProcessApiResponseForOperationEventAttribute(ResponseObject<List<OperationEventAttributeDto>> couApiResponse)
        {
            if (couApiResponse!=null)
            {
                if (couApiResponse.StatusCode == 200 && couApiResponse != null)
                {
                    var charOfAccount = couApiResponse.Data.Select(a => new StringValues
                    {
                        Text = $"{a.Name}",
                        Value = a.Id
                    });

                    var defaultSelectedValue = "default-value";

                    return new SelectList(charOfAccount.ToList(), "Value", "Text", defaultSelectedValue);
                }
            }
           

            // Return an empty SelectList if the response is not successful or the data is null
            return new SelectList(new List<StringValues>(), "Value", "Text");
        }


    }

}
