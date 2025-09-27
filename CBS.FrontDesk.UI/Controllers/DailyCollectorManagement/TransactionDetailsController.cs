using CBS.BusinessService.Config;
using CBS.BusinessService.DailyCollectionServices.ManualDailyCollection_Service;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Accounting;
using CBS.FrontDesk.Data.Entity.ManualDailycollection;
using CBS.FrontDesk.UI;
using CBS.FrontDesk.UI.Controllers;
using Microsoft.AspNet.SignalR.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

[CheckSessionTimeOut]
public class TransactionDetailsController : BaseController
{
    private readonly ManualDailyCollectionService _manualService;
    private readonly BranchServices _branchServices;

    public TransactionDetailsController(ManualDailyCollectionService manualService,BranchServices branchServices)
    {
        _manualService = manualService;
        _branchServices = branchServices;
    }

    // Loads the page (filters are client-side or populated via ViewBag)
    public async Task<ActionResult> Index()
    {
        await Loader();
        return View();
    }

    private async Task Loader()
    {
        var statuses = new[] { "Pending", "Approved", "Extracted", "Rejected", "Treated", "Completed" };
        ViewBag.Statuses = new SelectList(statuses);
        ViewBag.Branches = await _branchServices.GetBranches();
        ViewBag.Collectors = new List<StringValues>();
    }

    // Server-side DataTable endpoint (POST application/json)
    [HttpPost]
    public async Task<ActionResult> LoadDetailsDataTable()
    {
        GetManualEntryDailyCollectionDetailDataTableQuery query = null;

        try
        {
            // If request is JSON, read body and deserialize.
            var contentType = Request.ContentType ?? string.Empty;
            if (contentType.IndexOf("application/json", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                Request.InputStream.Position = 0;
                using (var sr = new StreamReader(Request.InputStream))
                {
                    var body = await sr.ReadToEndAsync();
                    if (!string.IsNullOrWhiteSpace(body))
                    {
                        // The client may be wrapping properties (e.g. { options: {...}, filters: {...} })
                        // Attempt direct deserialize to expected query shape first:
                        try
                        {
                            query = JsonConvert.DeserializeObject<GetManualEntryDailyCollectionDetailDataTableQuery>(body);
                        }
                        catch
                        {
                            // If the JSON shape differs (e.g. contains .options/.filters), attempt to map manually:
                            dynamic wrapper = JsonConvert.DeserializeObject(body);
                            query = new GetManualEntryDailyCollectionDetailDataTableQuery();

                            if (wrapper != null && wrapper.options != null)
                            {
                                // Try to map basic paging/sorting
                                query.Options.draw = wrapper.options.draw ?? query.Options.draw;
                                query.Options.start = wrapper.options.start ?? query.Options.start;
                                query.Options.length = wrapper.options.length ?? query.Options.length;
                                // You can map other options as needed
                            }

                            if (wrapper != null && wrapper.filters != null)
                            {
                                query.MemberBranchCode = wrapper.filters.memberBranchCode ?? query.MemberBranchCode;
                                query.MemberBranchId = wrapper.filters.memberBranchId ?? query.MemberBranchId;
                                query.MemberBranchName = wrapper.filters.memberBranchName ?? query.MemberBranchName;
                                query.Amount = wrapper.filters.amount ?? query.Amount;
                                query.MemberName = wrapper.filters.memberName ?? query.MemberName;
                                query.MemberReference = wrapper.filters.memberReference ?? query.MemberReference;
                                query.AccountNumber = wrapper.filters.accountNumber ?? query.AccountNumber;
                                query.DailyCollectorName = wrapper.filters.dailyCollectorName ?? query.DailyCollectorName;
                                query.UploadBy = wrapper.filters.uploadBy ?? query.UploadBy;
                                query.ManualEntryDailyCollectorId = wrapper.filters.manualEntryDailyCollectorId ?? query.ManualEntryDailyCollectorId;
                                query.ProcessingStatus = wrapper.filters.processingStatus ?? query.ProcessingStatus;
                                query.TreatementStatus = wrapper.filters.treatementStatus ?? query.TreatementStatus;
                                query.TreatementDate = wrapper.filters.treatementDate ?? query.TreatementDate;
                                query.AccountingDate = wrapper.filters.accountingDate ?? query.AccountingDate;
                                query.StartDate = wrapper.filters.startDate ?? query.StartDate;
                                query.EndDate = wrapper.filters.endDate ?? query.EndDate;
                            }
                        }
                    }
                }
            }

            // As a fallback, try to populate from Request.Form (if not JSON)
            if (query == null)
            {
                query = new GetManualEntryDailyCollectionDetailDataTableQuery();
                TryUpdateModel(query); // attempt model binding from form values
            }

            // Ensure query.Options exists and draw is int
            if (query.Options == null) query.Options = new ManualEntryDailyCollectionDetailDataTableOptions();

            // Call service which will call the API
            var customTable = await _manualService.GetManualEntryDetailsForDataTableAsync(query);

            if (customTable == null)
            {
                // return an empty, valid DataTables JSON structure
                return Json(new
                {
                    draw = query.Options?.draw ?? "1",
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<TransactionDetailDto>()
                }, JsonRequestBehavior.AllowGet);
            }

            // Convert returned generic data to strongly-typed list
            List<TransactionDetailDto> detailsList = new List<TransactionDetailDto>();
            try
            {
                // customTable.data may already be a typed list or could be JArray / List<object>
                var serialized = JsonConvert.SerializeObject(customTable.data ?? new List<object>());
                detailsList = JsonConvert.DeserializeObject<List<TransactionDetailDto>>(serialized) ?? new List<TransactionDetailDto>();
            }
            catch (Exception ex)
            {
                // If conversion fails, log and send empty list
                // TODO: replace with your logging mechanism
                System.Diagnostics.Trace.TraceError("LoadDetailsDataTable deserialize error: " + ex);
                detailsList = new List<TransactionDetailDto>();
            }

            return Json(new
            {
                draw = customTable.draw,
                recordsTotal = customTable.recordsTotal,
                recordsFiltered = customTable.recordsFiltered,
                data = detailsList
            }, JsonRequestBehavior.AllowGet);
        }
        catch (Exception ex)
        {
            // Log error and return 500 (DataTables will show an error in browser console)
            System.Diagnostics.Trace.TraceError("LoadDetailsDataTable error: " + ex);
            Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            return Json(new
            {
                draw = 0,
                recordsTotal = 0,
                recordsFiltered = 0,
                data = new List<TransactionDetailDto>(),
                error = "An error occurred while loading data."
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
