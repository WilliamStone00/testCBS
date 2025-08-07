
using BusinessServices;
using CBS.API.Helper;
using CBS.FrontDesk.Data.Entity.AccountingDayObject;
using CBS.FrontDesk.Data.Entity.CMoney;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountOperation;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using DocumentFormat.OpenXml.Office2010.Excel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CBS.FrontDesk.Data.Entity.SavingProducts.AccountOperation.PaymentReceipt;

namespace CBS.BusinessService.Accounts.MemberReceiptsP
{
   
    public class MemberReceiptServices : BaseService
    {
        private readonly ApiCallerHelper _transactionApiHelper;

        public MemberReceiptServices()
        {
            _transactionApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());

        }


        public async Task<IEnumerable<PaymentReceipt>> GetPaymentReceiptsByMemberReference(string reference)
        {
            try
            {
                var cusResponseObject = await _transactionApiHelper.GetAsync<ResponseObject<List<PaymentReceipt>>>(string.Format(APICallHelper.GetPaymentReceiptByMemberId, reference));
                if (cusResponseObject.IsSuccess)
                {
                    var data = cusResponseObject.ApiResponseData.Data;

                    return data;
                }
                return null;
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw;
            }
        }
        public async Task<IEnumerable<PaymentReceipt>> GetMemberReceiptbyMemberAndDateRanges(string memberReference, DateTime startDate, DateTime endDate)
        {
            try
            {
                // 🔁 Format the GET query string
                var url = $"{APICallHelper.GetPaymentReceiptByMemberAndDate}?memberReference={Uri.EscapeDataString(memberReference)}&startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
                var cusResponseObject = await _transactionApiHelper.GetAsync<ResponseObject<List<PaymentReceipt>>>(url);
                if (cusResponseObject.IsSuccess)
                {
                    return cusResponseObject.ApiResponseData.Data;
                }

                return null;
            }
            catch (Exception ex)
            {
                // You may log the error here
                throw;
            }
        }

        public async Task<PaymentReceipt> GetPaymentReceipt(string id)
        {
            try
            {
                var cusResponseObject = await _transactionApiHelper.GetAsync<ResponseObject<PaymentReceipt>>(string.Format(APICallHelper.GetPaymentReceiptById, id));
                if (cusResponseObject.IsSuccess)
                {
                    var data= cusResponseObject.ApiResponseData.Data;
                   
                    return data;
                }
                return new PaymentReceipt();
            }
            catch (Exception ex)
            {
                // Log and handle exception
                throw ex;
            }
        }
        public async Task<CustomDataTable> GetDataTableAsync(GetPaymentReceiptsDataTableQuery loansDataTableQuery)
        {
            loansDataTableQuery.Options.sortColumnName = "Date";
            if (!IsHeadOffice())
            {
                loansDataTableQuery.BranchId=GetBranchID();
            }
            // Make API call to fetch the DataTable result
            var couApiResponse = await _transactionApiHelper.PostAsync<ResponseObject<CustomDataTable>>(
                APICallHelper.GetPaymentReceiptDatatable,
                loansDataTableQuery
            );

            // Return response if successful
            if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData != null)
            {
                return couApiResponse.ApiResponseData.Data;
            }

            // Return an empty DataTable if the request fails
            return new CustomDataTable(
                draw: Convert.ToInt32(loansDataTableQuery.Options.draw),
                recordsTotal: 0,
                recordsFiltered: 0,
                data: new List<object>(), // No data
                dataTableOptions: loansDataTableQuery.Options
            );
        }

    }

}
