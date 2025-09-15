
using CBS.BusinessService.Config;
using CBS.BusinessService.DailyCollectionServices.ManualDailyCollection_Service;
using CBS.FrontDesk.Data.Entity.DataTable;
using CBS.FrontDesk.Data.Entity.ManualDailycollection;
using CBS.FrontDesk.Data.Message;
using CBS.FrontDesk.Helper;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Math;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Services.Description;
using System.Web.UI.WebControls;
using ZXing.QrCode.Internal;

namespace CBS.FrontDesk.UI.Controllers.DailyCollectorManagement
{
    // [CheckSessionTimeOut]
    public class FileValidationController : BaseController
    {
        private readonly ManualDailyCollectionService _manualService;
        private readonly BranchServices _branchServices;

        public FileValidationController(ManualDailyCollectionService manualService, BranchServices branchServices)
        {
            _manualService = manualService;
            _branchServices = branchServices;
        }


        public async Task<ActionResult> Index()
        {
            ViewBag.Statuses = new SelectList(new[] { "Pending", "Approved", "Extracted", "Rejected", "Treated", "Completed"});
            ViewBag.Branches = await _branchServices.GetBranches();
            return View();
        }

        //[HttpPost]
        //public async Task<ActionResult> LoadFiles(GetFilesForDataTableQuery query)
        // {
        //    try
        //    {
        //        var dataTable = await _manualService.GetextractedFilesForDataTableAsync(query);
        //        var fileList = JsonConvert.DeserializeObject<List<FileUploadSummary>>(JsonConvert.SerializeObject(dataTable.data));

        //        var resultData = fileList.Select(f => new
        //        {
        //            fileUploadId = f.FileUploadId,

        //            collectorName = f.CollectorName, 
        //            branchName = f.BranchName,
        //            id = f.Id,
        //            uploadedBy = f.UploadedBy,
        //            uploadedOn = f.UploadedOn.ToString("yyyy-MM-dd HH:mm"),
        //            Status = f.status
        //        }).ToList();

        //        return Json(new
        //        {
        //            draw = query.Options?.draw ?? "1",
        //            recordsTotal = dataTable.recordsTotal,
        //            recordsFiltered = dataTable.recordsFiltered,
        //            data = resultData
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error loading file data.");
        //    }
        //}

        // LEAD DATA TABLE AND SORT IT 
        [HttpPost]
        public async Task<ActionResult> LoadFiles(FileValidationRequestDto request)
        {
            try
            {
                // -------------------------
                // Defensive: ensure nested options exist
                // -------------------------
                var options = request?.DataTableOptions ?? new DataTableOptions();

                // parse draw/start/length carefully (could be strings)
                int draw = 0;
                int start = 0;
                int length = 10;

                int.TryParse(options?.draw ?? "0", out draw);
                int.TryParse(options?.start.ToString() ?? "0", out start);
                // prefer options.length, fallback to pageSize
                if (!int.TryParse(options?.length.ToString() ?? "", out length))
                {
                    length = options?.pageSize ?? 10;
                }

                // -------------------------
                // Pull basic filters from top-level DTO if present
                // -------------------------
                string statusFilter = request?.StatusFilter;
                DateTime? startDate = null;
                DateTime? endDate = null;
                string branchIdFilter = null;
                string collectorIdFilter = null;
                string uploadedByFilter = null;
                string fileNameFilter = null;
                string globalSearch = null;
                bool showAll = false;

                // -------------------------
                // Defensive: read request body JSON for nested 'filters' object
                // This covers client code that sends { options: {...}, filters: {...} }
                // -------------------------
                try
                {
                    Request.InputStream.Seek(0, System.IO.SeekOrigin.Begin);
                    using (var reader = new System.IO.StreamReader(Request.InputStream))
                    {
                        var body = reader.ReadToEnd();
                        if (!string.IsNullOrWhiteSpace(body))
                        {
                            // parse safely
                            var j = Newtonsoft.Json.Linq.JObject.Parse(body);

                            // try to extract filters.* if not already provided
                            var filters = j["filters"];
                            if (filters != null)
                            {
                                statusFilter = statusFilter ?? (string)filters["status"];
                                var df = (string)filters["dateFrom"];
                                var dt = (string)filters["dateTo"];
                                if (DateTime.TryParse(df, out var dfrom)) startDate = dfrom;
                                if (DateTime.TryParse(dt, out var dto)) endDate = dto;

                                fileNameFilter = fileNameFilter ?? (string)filters["fileName"];
                                branchIdFilter = branchIdFilter ?? (string)filters["branchId"];
                                collectorIdFilter = collectorIdFilter ?? ((string)filters["collectorId"] ?? (string)filters["collectorName"]);
                                uploadedByFilter = uploadedByFilter ?? (string)filters["uploadedBy"];
                                globalSearch = globalSearch ?? (string)filters["globalSearch"];
                                var showAllVal = filters["showAll"] ?? filters["showall"];
                                if (showAllVal != null) bool.TryParse(showAllVal.ToString(), out showAll);
                            }

                            // Also try to pull options.* (sort column/direction) if your client embedded them
                            var opts = j["options"];
                            if (opts != null)
                            {
                                // If options.draw/start/length present in JSON but not mapped, override ours
                                if (opts["draw"] != null) int.TryParse(opts["draw"].ToString(), out draw);
                                if (opts["start"] != null) int.TryParse(opts["start"].ToString(), out start);
                                if (opts["pageSize"] != null && int.TryParse(opts["pageSize"].ToString(), out var ps)) length = ps;
                                // map to our DataTableOptions fields if needed (we'll read sort from options below)
                                if (opts["sortColumnName"] != null) options.sortColumnName = opts["sortColumnName"].ToString();
                                if (opts["sortColumnDirection"] != null) options.sortColumnDirection = opts["sortColumnDirection"].ToString();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // don't fail the whole action because of request body parsing problems
                    System.Diagnostics.Trace.WriteLine("LoadFiles: request body parse failed: " + ex.Message);
                }

                // -------------------------
                // Build a service query object (keeps compatibility with existing service)
                // -------------------------
                var svcQuery = new GetFilesForDataTableQuery
                {
                    Options = new DataTableOptions
                    {
                        draw = options.draw,
                        start = options.start,
                        length = options.length,
                        pageSize = options.pageSize,
                        sortColumnName = options.sortColumnName,
                        sortColumnDirection = options.sortColumnDirection,
                        search = options.search,
                        sortDirection = options.sortDirection
                    },
                    StatusFilter = statusFilter,
                    StartDate = startDate,
                    EndDate = endDate,
                    BranchId = branchIdFilter,
                    CollectorId = collectorIdFilter,
                    UploadedByUserId = uploadedByFilter,
                    FileName = fileNameFilter,
                    GlobalSearch = globalSearch,
                    ShowAll = showAll
                };

                // -------------------------
                // Call the existing service to get upstream data (may already be paged)
                // We will treat upstream.data as the source and apply local filtering/sorting/pagination
                // -------------------------
                var upstream = await _manualService.GetextractedFilesForDataTableAsync(svcQuery);

                // Convert upstream.data into strongly typed list
                List<FileUploadSummary> fileList;
                try
                {
                    fileList = JsonConvert.DeserializeObject<List<FileUploadSummary>>(
                        JsonConvert.SerializeObject(upstream?.data ?? new List<object>())
                    ) ?? new List<FileUploadSummary>();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine("LoadFiles: mapping upstream.data failed: " + ex.Message);
                    fileList = new List<FileUploadSummary>();
                }

                // total records before filtering
                var recordsTotal = fileList.Count;

                // -------------------------
                // APPLY FILTERS (case-insensitive)
                // -------------------------
                if (!showAll)
                {
                    if (!string.IsNullOrWhiteSpace(statusFilter))
                    {
                        var s = statusFilter.Trim();
                        fileList = fileList.Where(f => string.Equals(
                            (f.status ?? f.Status ?? string.Empty), s, StringComparison.OrdinalIgnoreCase)).ToList();
                    }

                    if (!string.IsNullOrWhiteSpace(branchIdFilter))
                    {
                        var b = branchIdFilter.Trim();
                        fileList = fileList.Where(f =>
                            (f.BranchId ?? f.branchId ?? "").Equals(b, StringComparison.OrdinalIgnoreCase)
                            || (f.BranchName ?? f.branchName ?? "").IndexOf(b, StringComparison.OrdinalIgnoreCase) >= 0
                        ).ToList();
                    }

                    if (!string.IsNullOrWhiteSpace(collectorIdFilter))
                    {
                        var c = collectorIdFilter.Trim();
                        fileList = fileList.Where(f =>
                            (!string.IsNullOrWhiteSpace(f.collectorId) && f.collectorId.Equals(c, StringComparison.OrdinalIgnoreCase))
                            || (!string.IsNullOrWhiteSpace(f.CollectorName) && f.CollectorName.IndexOf(c, StringComparison.OrdinalIgnoreCase) >= 0)
                            || (!string.IsNullOrWhiteSpace(f.collectorName) && f.collectorName.IndexOf(c, StringComparison.OrdinalIgnoreCase) >= 0)
                        ).ToList();
                    }

                    if (!string.IsNullOrWhiteSpace(uploadedByFilter))
                    {
                        var ub = uploadedByFilter.Trim();
                        fileList = fileList.Where(f => (f.UploadedBy ?? f.uploadedBy ?? "").IndexOf(ub, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                    }

                    if (!string.IsNullOrWhiteSpace(fileNameFilter))
                    {
                        var fn = fileNameFilter.Trim();
                        fileList = fileList.Where(f =>
                            ((f.FileUploadId ?? f.fileUploadId ?? f.FileName ?? "")).IndexOf(fn, StringComparison.OrdinalIgnoreCase) >= 0
                        ).ToList();
                    }

                    if (!string.IsNullOrWhiteSpace(globalSearch))
                    {
                        var gs = globalSearch.Trim();
                        fileList = fileList.Where(f =>
                            (f.CollectorName ?? f.collectorName ?? "").IndexOf(gs, StringComparison.OrdinalIgnoreCase) >= 0
                            || (f.BranchName ?? f.branchName ?? "").IndexOf(gs, StringComparison.OrdinalIgnoreCase) >= 0
                            || (f.UploadedBy ?? f.uploadedBy ?? "").IndexOf(gs, StringComparison.OrdinalIgnoreCase) >= 0
                            || ((f.FileUploadId ?? f.fileUploadId ?? f.FileName ?? "")).IndexOf(gs, StringComparison.OrdinalIgnoreCase) >= 0
                        ).ToList();
                    }

                    // Date range: try CreatedDate, ModifiedDate or UploadedOn fields (the FileUploadSummary contains DateTimes)
                    if (startDate.HasValue || endDate.HasValue)
                    {
                        DateTime sDate = startDate ?? DateTime.MinValue;
                        DateTime eDate = endDate ?? DateTime.MaxValue;

                        fileList = fileList.Where(f =>
                        {
                            DateTime dt = DateTime.MinValue;
                            if (f.CreatedDate != default(DateTime) && f.CreatedDate != DateTime.MinValue) dt = f.CreatedDate;
                            else if (f.CreatedDate != null) // defensive - if some serialization makes it nullable
                            {
                                DateTime.TryParse(f.CreatedDate.ToString(), out dt);
                            }

                            // fallback to UploadedOn
                            if (dt == DateTime.MinValue && f.UploadedOn != default(DateTime))
                            {
                                dt = f.UploadedOn;
                            }

                            // final fallback to CreatedDate property named differently in upstream (try "createdDate" if the dynamic mapping had it)
                            // If dt still MinValue, we exclude the row from date filtered results unless you want to include them.
                            if (dt == DateTime.MinValue)
                                return false;

                            return dt >= sDate && dt <= eDate;
                        }).ToList();
                    }
                } // end if !showAll

                // recordsFiltered after filters
                var recordsFiltered = fileList.Count;

                // -------------------------
                // SORTING: determine requested sort column and direction
                // options.sortColumnName and options.sortColumnDirection used (case-insensitive)
                // -------------------------
                string sortColumn = options?.sortColumnName ?? options?.sortDirection; // defensive fallback
                string sortDirectionRaw = options?.sortColumnDirection ?? options?.sortColumnDirection ?? options?.sortDirection;
                bool sortDesc = string.Equals(sortDirectionRaw, "desc", StringComparison.OrdinalIgnoreCase);

                // Map sort column names to actual selectors
                Func<FileUploadSummary, object> selector = f => f.CollectorName ?? f.collectorName ?? "";

                if (!string.IsNullOrWhiteSpace(sortColumn))
                {
                    var col = sortColumn.Trim().ToLowerInvariant();
                    switch (col)
                    {
                        case "collectorname":
                        case "collector":
                            selector = f => f.CollectorName ?? f.collectorName ?? "";
                            break;
                        case "branchname":
                        case "branch":
                            selector = f => f.BranchName ?? f.branchName ?? "";
                            break;
                        case "status":
                            selector = f => f.status ?? f.Status ?? "";
                            break;
                        case "uploadedon":
                        case "createddate":
                        case "modifieddate":
                        case "uploaddatetime":
                            selector = f =>
                            {
                                // prefer CreatedDate then UploadedOn then CreatedDate (just in case)
                                if (f.CreatedDate != default(DateTime)) return f.CreatedDate;
                                if (f.UploadedOn != default(DateTime)) return f.UploadedOn;
                                return DateTime.MinValue;
                            };
                            break;
                        case "uploadedby":
                        case "uploadedbyname":
                            selector = f => f.UploadedBy ?? f.uploadedBy ?? "";
                            break;
                        case "fileuploadid":
                        case "fileid":
                        case "filename":
                        case "id":
                            selector = f => (f.FileUploadId ?? f.fileUploadId ?? f.FileName ?? "");
                            break;
                        case "totalamount":
                            selector = f => (object)(f.TotalAmount);
                            break;
                        case "totalmember":
                            selector = f => (object)(f.TotalMember);
                            break;
                        default:
                            selector = f => f.CollectorName ?? f.collectorName ?? "";
                            break;
                    }
                }

                // Apply ordering
                IOrderedEnumerable<FileUploadSummary> ordered;
                try
                {
                    ordered = sortDesc ? fileList.OrderByDescending(selector) : fileList.OrderBy(selector);
                }
                catch
                {
                    ordered = fileList.OrderBy(f => f.CollectorName ?? f.collectorName ?? "");
                }

                // -------------------------
                // PAGINATION: slice the ordered set
                // length <= 0 or length == -1 => return all
                // -------------------------
                IEnumerable<FileUploadSummary> paged;
                if (length <= 0 || length == -1)
                {
                    paged = ordered;
                }
                else
                {
                    paged = ordered.Skip(start).Take(length);
                }

                // -------------------------
                // Map to response shape expected by the client
                // -------------------------
                var resultData = paged.Select(f => new
                {
                    fileUploadId = f.FileUploadId ?? f.fileUploadId,
                    collectorName = f.CollectorName ?? f.collectorName,
                    branchName = f.BranchName ?? f.branchName,
                    id = f.Id ?? f.Id,
                    uploadedBy = f.UploadedBy ?? f.uploadedBy,
                    uploadedOn = (f.CreatedDate != default(DateTime) ? f.CreatedDate.ToString("yyyy-MM-dd HH:mm")
                                 : (f.UploadedOn != default(DateTime) ? f.UploadedOn.ToString("yyyy-MM-dd HH:mm")
                                     : (f.CreatedDate != default(DateTime) ? f.CreatedDate.ToString("yyyy-MM-dd HH:mm") : ""))),
                    Status = f.status ?? f.Status
                }).ToList();

                // -------------------------
                // Return DataTables friendly payload
                // -------------------------
                return Json(new
                {
                    draw = draw,
                    recordsTotal = recordsTotal,
                    recordsFiltered = recordsFiltered,
                    data = resultData
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("LoadFiles: exception: " + ex.ToString());
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Error loading file data.");
            }
        }



        // Returns the partial view with file summary and DataTable placeholder
        [HttpGet]
        public async Task<ActionResult> GetextractedFileDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Missing id");

            var model = await _manualService.GetExtractedDetailsAsync(id);

            if (model == null)
                return HttpNotFound();

            return PartialView("_FileDetails", model);
        }

        // Returns JSON for DataTable
        [HttpGet]
        public async Task<ActionResult> GetTransactionDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Missing id");

            var fileDetails = await _manualService.GetExtractedDetailsAsync(id);

            if (fileDetails?.Details == null || !fileDetails.Details.Any())
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);

            var jsonData = fileDetails.Details.Select(d => new
            {
                memberName = d.MemberName,
                accountNumber = d.AccountNumber,
                memberBranchName = d.MemberBranchName,
                amount = d.Amount
            });

            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }


        // ACTION 3: Gets the partial view for the validation/review/approve modal
        [HttpGet]
        public ActionResult GetActionForm(string fileUploadId, string mode)
        {
            var model = new ValidationDto
            {
                ManualEntryDailyCollectorId = fileUploadId,
                ApprovedBy = Session["FullName"]?.ToString(),
                Mode = mode // "approve", "review", or "reject"
            };
            return PartialView("_ActionForm", model);
        }

        [HttpPost]
        public async Task<ActionResult> SubmitAction(ValidationDto model)
        {
            if (!ModelState.IsValid)
            {
                // --- THIS IS THE CRITICAL CHANGE ---
                // We need to return the ModelState errors in a format the client can parse.
                var errors = new Dictionary<string, string[]>();
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    if (state.Errors.Any())
                    {
                        errors[key] = state.Errors.Select(e => e.ErrorMessage).ToArray();
                    }
                }
                return Json(new { success = false, message = "Please correct the validation errors.", errors = errors });
            }

            var result = await _manualService.SubmitFileActionAsync(model);
            // Ensure Messaging.MessageResult returns a string
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }


        [HttpPost]
        public async Task<ActionResult> RejectFile(string KEY)
        {
            if (string.IsNullOrWhiteSpace(KEY))
            {
                return Json(new { success = false, message = "Invalid ID provided for rejection." });
            }

            var result = await _manualService.RejectFileAsync(KEY);
            // Ensure Messaging.MessageResult returns a string
            return Json(new { success = result.Result, message = Messaging.MessageResult(result) });
        }

        [HttpPost]
        public async Task<ActionResult> LoadDetailsDataTable(GetManualEntryDailyCollectionDetailDataTableQuery query)
        {
            var result = await _manualService.GetManualEntryDetailsForDataTableAsync(query);
            return Json(result);
        }     


    }
}