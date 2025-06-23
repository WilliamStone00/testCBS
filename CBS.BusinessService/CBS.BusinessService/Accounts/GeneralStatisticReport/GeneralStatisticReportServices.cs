using BusinessServices;
using CBS.API.Helper;
using CBS.BusinessService.Config;
using CBS.BusinessService.LoanportFolioFlattener;
using CBS.BusinessService.UserManagement;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.CashCeilingManagement;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.GeneralStatisticReport;
using CBS.FrontDesk.Data.Entity.LoanConf;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountOperation;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Data.ReportDataSetDto;
using CBS.FrontDesk.Data.ReportDataSetDto.LoanDeliquentAnalysis;
using CBS.FrontDesk.Helper;
using DocumentFormat.OpenXml;
using Microsoft.Owin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CBS.BusinessService.Accounts.GeneralStatisticReport
{
    public class GeneralStatisticReportServices : BaseService
    {
        private readonly ApiCallerHelper _transactionBaseConfigApiHelper;
        private readonly BranchServices _branchServices;
        public GeneralStatisticReportServices()
        {
            _transactionBaseConfigApiHelper = new ApiCallerHelper(ConfigurationManager.AppSettings["TransactionBaseUrl"].ToString());
            _branchServices = new BranchServices();
        }

        public async Task<GeneralStatisticsReportRPT> GetGeneralStatisticsReportAsync(GenerateGeneralStatisticsReportQuery reportCommand)
        {
            try
            {
                bool isSingleBranch = false;

                // If not head office, use user's branch
                if (!IsHeadOffice())
                {
                    isSingleBranch = true;
                    reportCommand.BranchId = GetBranchID();
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(reportCommand.BranchId))
                    {
                        reportCommand.BranchId = "All";
                    }
                }
                var couApiResponse = await _transactionBaseConfigApiHelper.PostAsync<ResponseObject<GeneralStatisticsReportRPT>>(APICallHelper.GetGeneralStatistics, reportCommand);
                if (couApiResponse.IsSuccess && couApiResponse.ApiResponseData != null)
                {
                    var report = couApiResponse.ApiResponseData.Data;
                    var branches = await _branchServices.GetBranches();

                    // If "All", fallback to default branch to fetch bank metadata
                    if (IsHeadOffice() && reportCommand.BranchId == "All")
                    {
                        reportCommand.BranchId = GetBranchID();
                    }

                    var branch = branches.FirstOrDefault(x => x.Id == reportCommand.BranchId);

                    // Enrich the report with metadata
                    var enrichedReport = GeneralStatisticsReportDtoMapper.AddBranchGetReportRPTMapper(report, branch);
                    var flatRows = GeneralStatisticsReportDtoMapper.FlattenToRows(report);
                    enrichedReport.GeneralStatisticsFlatRow=flatRows;
                    return enrichedReport;
                }

                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }



    }

}
