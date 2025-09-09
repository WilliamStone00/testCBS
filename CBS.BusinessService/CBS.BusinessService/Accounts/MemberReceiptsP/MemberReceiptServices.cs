
using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.AccountingDayObject;
using CBS.FrontDesk.Data.Entity.CMoney;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountOperation;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNet.SignalR.Hosting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using static CBS.FrontDesk.Data.Entity.SavingProducts.AccountOperation.PaymentReceipt;

namespace CBS.BusinessService.Accounts.MemberReceiptsP
{
   
    public class MemberReceiptServices : BaseService
    {
        private readonly ApiCallerHelper _transactionApiHelper;
        private readonly BranchServices _branchServices;
        public MemberReceiptServices(BranchServices branchServices)
        {
            _transactionApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
            _branchServices=branchServices;
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
                var cusResponseObject = await _transactionApiHelper
                    .GetAsync<ResponseObject<PaymentReceipt>>(
                        string.Format(APICallHelper.GetPaymentReceiptById, id));

                if (cusResponseObject.IsSuccess)
                {
                    var data = cusResponseObject.ApiResponseData.Data;
                    var transaction = cusResponseObject.ApiResponseData.Data;

                    Branch branch = await _branchServices.GetBranch(data.BranchId);
                    var rptSource = PaymentReceiptMapping.MapPaymentReceipt(transaction, branch);

                    // ✅ Null-safe check for OperationTypeGrouping
                    var grouping = transaction?.OperationTypeGrouping ?? string.Empty;
                    if (grouping.IndexOf("loan", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        HttpContext.Current.Session["path"] = "loan";
                    }
                    else
                    {
                        HttpContext.Current.Session["path"] = "cashin_cashout";
                    }
                    HttpContext.Current.Session["AccountingDate"] = data.AccountingDay;
                    HttpContext.Current.Session["TransactionDate"] = data.Date;
                    HttpContext.Current.Session["rptSource"] = rptSource;
                    return data;
                }

                return new PaymentReceipt();
            }
            catch (Exception ex)
            {
                // Always rethrow preserving stack trace
                throw;
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
