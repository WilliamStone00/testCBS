using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.SavingProducts;
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

        public async Task<List<ProductAccountingChart>> GetProductAccountingBookByproducttype(string productType)
        {
            try
            {

                var cusResponseObject = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<ProductAccountingChart>>>(string.Format(APICallHelper.GetProductAccountingBookbyproductTypeUrl, productType));// await _savingConfigApiHelper.GetAsync<ResponseObject<SavingProduct>>(string.Format(APICallHelper.Get_Update_Delete_SavingProduct, id));
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
        public async Task<IEnumerable<StringValues>> GetEventNames(string opertionType)
        {
            try
            {
                // Make an API call to retrieve accounting event attributes
                var response = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<AccountingEventAttributs>>>(string.Format(APICallHelper.GetAllOperationServicesAccountingRuleEntryQuery, opertionType));
                if (response.IsSuccess && response.ApiResponseData != null)
                {
                    // Map the API response to StringValues objects with Text and Value properties
                    var stringValuesList = response.ApiResponseData.Data
                        .Select(item => new StringValues(item.AccountingRuleEntryName, item.EventCode))
                        .ToList();

                    return stringValuesList;
                }
                else
                {
                    // Handle unsuccessful API response
                    // You can log the error or return an empty list
                    return Enumerable.Empty<StringValues>();
                }
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw new ApplicationException("Error occurred while fetching accounting event names.", ex);
            }
        }

        public async Task<IEnumerable<StringValues>> GetChartOfAccounts()
        {
            try
            {
                var couApiResponse = await _accountingApiCallerHelper.GetAsync<ResponseObject<List<ChartOfAccountStateDto>>>(APICallHelper.GetAllChartOfAccountManagementPositionByChart);
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

        private IEnumerable<StringValues> ProcessApiResponseForChartOfAccount(ResponseObject<List<ChartOfAccountStateDto>> couApiResponse)
        {
            if (couApiResponse!=null)
            {
                if (couApiResponse.StatusCode == 200 && couApiResponse != null)
                {
                    return couApiResponse.Data.Select(a => new StringValues
                    {
                        Text = $"{a.GeneralRepresentation}",
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
