using CBS.BusinessService.Accounting_V2.AffiliateAccounts;
using CBS.BusinessService.Accounting_V2.BranchAccountService;
using CBS.BusinessService.Accounting_V2.IPS;
using CBS.BusinessService.Accounts;
using CBS.BusinessService.CheckManagementSystem;
using CBS.BusinessService.Config;
using CBS.BusinessService.MembersAccountSettings;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Entity.Accounting_V2.IPS;
using CBS.FrontDesk.Data.Message;
using DocumentFormat.OpenXml.Wordprocessing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;


namespace CBS.FrontDesk.UI.Controllers.Accounting_V2.IPS
{
   // [CheckSessionTimeOut]
        public class IPSClaimController : BaseController
        {
        private readonly BranchAccountService _BranchAccountService;
        private readonly IPSClaimService _ipsClaimService;
            private readonly BranchServices _branchServices;
            private readonly CustomerService _customerService;
            private readonly CashDeskServices _CashDeskServices;
        private readonly AffiliateAccountService _AffiliateAccountService;


        public IPSClaimController(IPSClaimService ipsClaimService,BranchServices branchServices,        CustomerService customerService, CashDeskServices cashDeskServices,AffiliateAccountService AffiliateAccountService,BranchAccountService BranchAccountService)
            {
         
            _ipsClaimService = ipsClaimService;
                _branchServices = branchServices;
                _customerService = customerService;
            _CashDeskServices = cashDeskServices;
            _AffiliateAccountService = AffiliateAccountService;
              _BranchAccountService = BranchAccountService;
        }

        // Main page
        public async Task<ActionResult> Index()
        {
            await LoadViewData();
            return View();
        }

        public async Task<ActionResult> List()
            {
                await LoadViewData();
                return View();
            }

            // Create page
            public async Task<ActionResult> Create()
            {
                await LoadViewData();
                return View("Create", new IPSClaimCreate());
            }

            // Details page
            public async Task<ActionResult> Details(string id)
            {
                if (string.IsNullOrEmpty(id))
                    return RedirectToAction("Index");

                var claim = await _ipsClaimService.GetClaimByIdAsync(id);
                if (claim == null)
                {
                    TempData["ErrorMessage"] = "Claim not found";
                    return RedirectToAction("Index");
                }

                return View(claim);
            }

            // Load data for partial views
            public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null)
            {
                await LoadViewData();

                if (path == "list")
                {
                    var data = await _ipsClaimService.GetAllClaimsAsync();
                    return PartialView(partialView, data);
                }
                else if (path == "new")
                {
                    return PartialView(partialView, new IPSClaimCreate());
                }
                else if (path == "get")
                {
                    var data = await _ipsClaimService.GetClaimByIdAsync(KEY);
                    return PartialView(partialView, data);
                }
                else if (path == "details")
                {
                    var data = await _ipsClaimService.GetClaimByIdAsync(KEY);
                    return PartialView(partialView, data);
                }

                return PartialView("_Error");
            }

        // Get member details
        [HttpGet]
        public async Task<JsonResult> GetMemberDetails(string memberId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(memberId))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Member reference is required."
                    }, JsonRequestBehavior.AllowGet);
                }

                // Call service
                var customerData = await _ipsClaimService.GetinfoAsync(memberId);

                if (customerData == null || customerData.customer == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = customerData.statusMessage
                    }, JsonRequestBehavior.AllowGet);
                }

                // Map to claim create model
                var mappedClaimData = MapToIPSClaimCreate(customerData);

                // Calculate balances
                decimal totalAccountBalance = 0;
                decimal totalLoanBalance = 0;

                if (customerData.accounts != null)
                {
                  customerData.accountnum = customerData.accounts.Count;
                    totalAccountBalance = customerData.accounts
                        .Where(a => a.accountType != null && !a.accountType.Contains("Loan"))
                        .Sum(a => a.balance);
                }

                if (customerData.loans != null)
                {
                    totalLoanBalance = customerData.loans.Sum(l => l.balance);
                    customerData.loannum = customerData.loans.Count;
                }

                var netClaimableAmount = totalAccountBalance - totalLoanBalance;
                if (netClaimableAmount < 0) netClaimableAmount = 0;

                // Set calculated values back to DTO (optional but useful)
                customerData.totalAccountBalance = totalAccountBalance;
                customerData.totalLoanBalance = totalLoanBalance;
                customerData.netClaimableAmount = netClaimableAmount;

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        claimFormData = mappedClaimData,

                        customer = customerData.customer,

                        accounts = customerData.accounts != null
                            ? customerData.accounts.Select(a => new
                            {
                                a.id,
                                a.accountNumber,
                                a.accountName,
                                a.accountType,
                                a.balance,
                                a.blockedAmount,
                                a.status,
                                a.availableBalance
                            }).ToList<object>()
                            : new List<object>(),

                                                loans = customerData.loans != null
                            ? customerData.loans.Select(l => new
                            {
                                l.id,
                                l.loanApplicationId,
                                l.principal,
                                l.balance,
                                l.interestRate,
                                l.loanStatus,
                                l.disbursementDate,
                                l.maturityDate,
                                l.isDeliquentLoan,
                                l.loanDuration
                            }).ToList<object>()
                            : new List<object>(),
                        totalAccountBalance,
                        totalLoanBalance,
                        netClaimableAmount,

                        summary = new
                        {
                            AccountCount = customerData.accounts?.Count ?? 0,
                            LoanCount = customerData.loans?.Count ?? 0,
                            IsActive = customerData.customer.active,
                            HasActiveLoans = customerData.loans?.Any(l => l.loanStatus == "Open") ?? false
                        }
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }


        // Helper method to map customer data to IPSClaimCreate
        private IPSClaimCreate MapToIPSClaimCreate(CustomerMetaDataResponse customerData)
        {
            var customer = customerData.customer;

            if (customer == null)
                return new IPSClaimCreate();

            return new IPSClaimCreate
            {
                MemberId = customer.customerId,

                FirstName = customer.firstName,
                LastName = customer.lastName,

                DateOfBirth = customer.dateOfBirth ?? DateTime.MinValue,
                PlaceOfBirth = customer.placeOfBirth,

                LegalForm = customer.legalForm,
                CustomerType = customer.customerType,

                Gender = customer.gender,
                Phone = customer.phone,
                Address = customer.address,

                EmployerName = customer.employerName,
                EmployerTelephone = customer.employerTelephone,
                EmployerAddress = customer.employerAddress,

                UsualDutiesOfLivelihood = customer.occupation,

                BeneficiaryName = customer.fullName,
                BeneficiaryContact = customer.phone,
                BeneficiaryAddress = customer.address,

                BranchId = customer.branchId,

                // Defaults / user-entered later
                DateOfEvent = DateTime.Today,
                ClaimedAmount = customerData.netClaimableAmount
            };
        }
        private IPSClaimCreate MapToIPSClaimCreate(dynamic customerData)
        {
            if (customerData?.CustomerDto == null)
                return new IPSClaimCreate();

            var customer = customerData.CustomerDto;

            return new IPSClaimCreate
            {
                MemberId = customer.CustomerId,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                DateOfBirth = customer.DateOfBirth ?? default,
                PlaceOfBirth = customer.PlaceOfBirth,
                LegalForm = customer.LegalForm,
                CustomerType = customer.CustomerType,
                Gender = customer.Gender,
                Phone = customer.Phone,
                Address = customer.Address,
                EmployerName = customer.EmployerName,
                EmployerTelephone = customer.EmployerTelephone,
                EmployerAddress = customer.EmployerAddress,
                UsualDutiesOfLivelihood = customer.Occupation,
                BeneficiaryName = $"{customer.FirstName} {customer.LastName}".Trim(),
                BeneficiaryContact = customer.Phone,
                BeneficiaryAddress = customer.Address,
                BranchId = customer.BranchId,
                // Default values for other fields
                DateOfEvent = DateTime.Today,
                ClaimedAmount = 0
            };
        }


        // Create or Update claim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateOrUpdate(IPSClaimCreate model)
        {
            //if (!ModelState.IsValid)
            //{
            //    return Json(new
            //    {
            //        success = false,
            //        message = "Validation failed. Please check all required fields.",
            //        status = "ValidationError",
            //        errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
            //    });
            //}

            try
            {               
                // Create the claim
                var result = await _ipsClaimService.CreateClaimAsync(model);

                if (result.Result)
                {
                    var claim = result.Data as IPSClaim;

                    // Prepare success response with claim details
                    return Json(new
                    {
                        success = true,
                        message = $"Claim created successfully! Claim ID: {claim?.Id}",
                        status = result.MessageStatus,
                        claimId = claim?.Id,
                        claimType = claim?.ClaimType,
                        claimedAmount = claim?.ClaimedAmount,
                        redirectUrl = Url.Action("UploadDocuments", "IPSClaim", new { claimId = claim?.Id }),
                        detailsUrl = Url.Action("Details", "IPSClaim", new { id = claim?.Id })
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = result.MessageString,
                        status = result.MessageStatus,
                      
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"An error occurred while creating the claim: {ex.Message}",
                    status = "Error"
                });
            }
        }

        public ActionResult UploadDocuments(string claimId)
        {
            ViewBag.ClaimId = claimId;
            return View("UploadDocuments");
        }

        public async Task<ActionResult> Details2(string id)
        {
            var data = await _ipsClaimService.GetClaimByIdAsync(id);
            return View("_Details", data);
        }


        // Load datatable data
            [HttpPost]
            public async Task<JsonResult> LoadClaimsData(IPSClaimQuery query)
             {
             await LoadViewData();
                try
                {
                    var data = await _ipsClaimService.GetClaimsDataTableAsync(query);
                    var claims = JsonConvert.DeserializeObject<List<IPSClaim>>(JsonConvert.SerializeObject(data.data));

                    return Json(new
                    {
                        draw = data.draw,
                        recordsTotal = data.recordsTotal,
                        recordsFiltered = data.recordsFiltered,
                        data = claims
                    });
                }
                catch (Exception ex)
                {
                    return Json(new
                    {
                        draw = query?.Options?.draw ?? "1",
                        recordsTotal = 0,
                        recordsFiltered = 0,
                        data = new List<object>(),
                        error = ex.Message
                    });
                }
            }

            // Approve claim
            [HttpPost]
            public async Task<JsonResult> Approve(IPSClaimApprove model)
            {
                if (string.IsNullOrWhiteSpace(model.ApprovalComment))
                    return Json(new { success = false, message = "Approval comment is required." });

                var result = await _ipsClaimService.ApproveClaimAsync(model);

                return Json(new
                {
                    success = result.Result,
                    message = result.MessageString,
                    status = result.MessageStatus,
                    reload = result.Result
                });
            }

            // Reject claim
            [HttpPost]
            public async Task<JsonResult> Reject(IPSClaimReject model)
            {
                if (string.IsNullOrWhiteSpace(model.Reason))
                    return Json(new { success = false, message = "Rejection reason is required." });

                   var result = await _ipsClaimService.RejectClaimAsync(model);

                return Json(new
                {
                    success = result.Result,
                    message = result.MessageString,
                    status = result.MessageStatus,
                    reload = result.Result
                });
            }

            //// Post claim
            //[HttpPost]
            //public async Task<JsonResult> Post(PremiumClaimCashIn model)
            //{
            //    if (string.IsNullOrWhiteSpace(model.ClaimId))
            //        return Json(new { success = false, message = "Claim ID is required." });

            //    var result = await _ipsClaimService.PostClaimAsync(model);

            //    return Json(new
            //    {
            //        success = result.Result,
            //        message = result.MessageString,
            //        status = result.MessageStatus,
            //        reload = result.Result
            //    });
            //}

            // Upload document
            [HttpPost]
            public async Task<JsonResult> UploadDocument(string claimId, IPSClaimDocument document)
            {
                if (string.IsNullOrWhiteSpace(claimId))
                    return Json(new { success = false, message = "Claim ID is required." });

                var result = await _ipsClaimService.UploadDocumentAsync(claimId, document);

                return Json(new
                {
                    success = result.Result,
                    message = result.MessageString,
                    status = result.MessageStatus
                });
            }

            // Delete claim
            [HttpPost]
            public async Task<JsonResult> Delete(string id)
            {
                if (string.IsNullOrWhiteSpace(id))
                    return Json(new { success = false, message = "Claim ID is required." });

                var result = await _ipsClaimService.DeleteClaimAsync(id);

                return Json(new
                {
                    success = result.Result,
                    message = result.MessageString,
                    status = result.MessageStatus,
                    reload = result.Result
                });
            }

        private async Task LoadViewData()
        {
            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;

            ViewBag.ClaimTypes = new List<SelectListItem>
            {
                new SelectListItem { Value = "LifeSavings"  , Text = "Life Savings" },
                new SelectListItem { Value = "LoanProtection", Text = "Loan Protection" }
            };

            var affiliateAccounts = await _AffiliateAccountService.GetAllAffiliateAccounts();
            ViewBag.AffiliateAccounts = affiliateAccounts;

            ViewBag.Causes = new List<SelectListItem>
            {
                new SelectListItem { Value = "Accident", Text = "Accident" },
                new SelectListItem { Value = "Illness", Text = "Illness / Disease" },
                new SelectListItem { Value = "Natural", Text = "Natural Causes" },
                new SelectListItem { Value = "WorkRelated", Text = "Work-Related Injury" },
                new SelectListItem { Value = "OccupationalDisease", Text = "Occupational Disease" },
                new SelectListItem { Value = "MedicalProcedure", Text = "Medical / Surgical Complication" },
                new SelectListItem { Value = "PermanentDisability", Text = "Permanent Disability" },
                new SelectListItem { Value = "TemporaryDisability", Text = "Temporary Disability" },
                new SelectListItem { Value = "OldAge", Text = "Old Age" },
                new SelectListItem { Value = "Maternity", Text = "Maternity-Related" },
                new SelectListItem { Value = "CriticalIllness", Text = "Critical Illness" },
                new SelectListItem { Value = "NaturalDisaster", Text = "Natural Disaster" },
                new SelectListItem { Value = "Other", Text = "Other (Specify)" }

            };
        }

        [HttpGet]
        public async Task<ActionResult> GetMembersByBranch(string branchId)
        {
            try
            {
                var members = await _ipsClaimService.GetMemberByBranch(branchId);

                var payload = new
                {
                    success = true,
                    data = members
                };

                return Content(
                    JsonConvert.SerializeObject(payload),
                    "application/json"
                );
            }
            catch (Exception ex)
            {
                return Content(
                    JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "Error loading members: " + ex.Message
                    }),
                    "application/json"
                );
            }
        }


        //"url": "/Saving/LoadDataSearch?Search=" + search,
        [HttpPost]
        public async Task<ActionResult> UploadDocuments(IPSClaimDocument model)
        {
            var data = await _ipsClaimService.UploadFiles(model);
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
        }



        [HttpGet]
        public async Task<JsonResult> GetBranchAccounts(string branchId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(branchId))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Branch ID is required."
                    }, JsonRequestBehavior.AllowGet);
                }

                var branchAccounts = await _BranchAccountService.GetAllBranchAccountsFromDataTableAsync(branchId);

                return Json(new
                {
                    success = true,
                    data = branchAccounts.Select(a => new
                    {
                        id = a.Id,
                        code = a.Code,
                        name = a.Name,
                        
                    }).ToList()
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        // Updated Post method to handle PremiumClaimCashIn
        [HttpPost]
        public async Task<JsonResult> Post(PremiumClaimCashIn model)
        {
            if (string.IsNullOrWhiteSpace(model.ClaimId))
                return Json(new { success = false, message = "Claim ID is required." });

            if (model.TotalAmount <= 0)
                return Json(new { success = false, message = "Total amount must be greater than 0." });

         

            var result = await _ipsClaimService.PostClaimAsync(model);

            return Json(new
            {
                success = result.Result,
                message = result.MessageString,
                status = result.MessageStatus,
                reload = result.Result
            });
        }

        // Get posting data for claim
        [HttpGet]
        public async Task<JsonResult> GetPostingData(string claimId, string memberId, string branchId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(claimId) ||
                    string.IsNullOrWhiteSpace(memberId) ||
                    string.IsNullOrWhiteSpace(branchId))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Claim ID, Member ID, and Branch ID are required."
                    }, JsonRequestBehavior.AllowGet);
                }

                // Explicit types (NO var = null)
                dynamic customerData = null;
                dynamic claim = null;

                // Try get customer info
                try
                {
                    customerData = await _ipsClaimService.GetinfoAsync(memberId);
                }
                catch
                {
                    // Ignore and continue
                }

                // Try get claim info
                try
                {
                    claim = await _ipsClaimService.GetClaimByIdAsync(claimId);
                }
                catch
                {
                    // Ignore and continue
                }

                // Dropdown data
                var branchAccounts = await _BranchAccountService
                    .GetAllBranchAccountsFromDataTableAsync(branchId);

                var affiliateAccounts = await _AffiliateAccountService
                    .GetAllAffiliateAccounts();

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        claim = claim == null ? null : new
                        {
                            claim.Id,
                            claim.ClaimType,
                            claim.ClaimedAmount,
                            claim.ApprovedAmount,
                            claim.Status
                        },

                        customer = customerData?.customer == null ? null : new
                        {
                            customerData.customer.customerId,
                            customerData.customer.firstName,
                            customerData.customer.lastName,
                            customerData.customer.phone,
                            customerData.customer.address,
                            customerData.customer.email
                        },

                        accounts = customerData?.accounts != null
                            ? ((IEnumerable<dynamic>)customerData.accounts)
                                .Select(a => new
                                {
                                    a.id,
                                    a.accountNumber,
                                    a.accountName,
                                    a.accountType,
                                    a.balance,
                                    a.availableBalance,
                                    a.blockedAmount
                                })
                                .Cast<object>()
                                .ToList()
                            : new List<object>(),


                        viewBagData = new
                        {
                            affiliateAccounts = affiliateAccounts == null
                                ? new List<object>()
                                : affiliateAccounts.Select(a => new
                                {
                                    Value = a.Value?.ToString(),
                                    Text = a.Text
                                }).Cast<object>().ToList(),

                            branchAccounts = branchAccounts == null
                                ? new List<object>()
                                : branchAccounts.Select(b => new
                                {
                                    Value = b.Id?.ToString(),
                                    Text = $"{b.Code} - {b.Name}"
                                }).Cast<object>().ToList()
                        }
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error loading posting data: {ex.Message}"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        // Ensure you have these services injected in your Constructor
        // private readonly AffiliateAccountService _affiliateAccountService;
        // private readonly BranchAccountService _branchAccountService;



        [HttpPost]
        public async Task<ActionResult> ExportClaimsData(IPSClaimExportRequest request)
        {
            try
            {
                Console.WriteLine($"=== IPS CLAIMS EXPORT DEBUG START ===");
                Console.WriteLine($"Request received: {request != null}");
                Console.WriteLine($"Data count: {request?.Data?.Count ?? 0}");
                Console.WriteLine($"Export Format: {request?.ExportFormat ?? "N/A"}");
                Console.WriteLine($"Report Type: {request?.ReportType ?? "N/A"}");
                Console.WriteLine($"FileName: {request?.FileName ?? "N/A"}");

                if (request?.Data == null || !request.Data.Any())
                {
                    Console.WriteLine("No claims data to export");
                    return Json(new { success = false, message = "No claims data available for export." });
                }

                // Log data structure
                Console.WriteLine($"=== CLAIMS DATA STRUCTURE ANALYSIS ===");
                if (request.Data.Any())
                {
                    var firstItem = request.Data.First();
                    Console.WriteLine($"First item - Claim ID: {firstItem.ClaimNumber}, Member: {firstItem.MemberId}, Type: {firstItem.ClaimType}, Status: {firstItem.Status}, Amount: {firstItem.ClaimedAmount}");
                }

                // Convert to export data
                Console.WriteLine($"=== CONVERTING CLAIMS DATA ===");
                var exportData = IPSClaimExcelExportGenerator.ConvertToExportData(request.Data);
                Console.WriteLine($"Successfully converted {exportData.Count} claim records");

                if (!exportData.Any())
                {
                    Console.WriteLine("No claims data converted successfully");
                    return Json(new { success = false, message = "No valid claims data could be processed for export." });
                }

                // Apply date filtering if specified
                if (!string.IsNullOrEmpty(request.StartDate) && !string.IsNullOrEmpty(request.EndDate))
                {
                    try
                    {
                        var startDate = DateTime.Parse(request.StartDate);
                        var endDate = DateTime.Parse(request.EndDate).AddDays(1).AddSeconds(-1);

                        exportData = exportData.Where(x =>
                            (x.CreatedDate >= startDate && x.CreatedDate <= endDate) ||
                            (x.EventDate >= startDate && x.EventDate <= endDate)
                        ).ToList();

                        Console.WriteLine($"Filtered to {exportData.Count} records within date range: {request.StartDate} to {request.EndDate}");
                    }
                    catch (Exception dateEx)
                    {
                        Console.WriteLine($"Date filter error: {dateEx.Message}");
                    }
                }

                // Prepare file name and paths
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                string fileName = $"{request.FileName ?? "IPS_Claims_Report"}_{timestamp}.xlsx";
                string directoryPath = Server.MapPath("~/TempFiles");

                Console.WriteLine($"=== FILE PREPARATION ===");
                Console.WriteLine($"Directory: {directoryPath}");
                Console.WriteLine($"File Name: {fileName}");

                if (!Directory.Exists(directoryPath))
                {
                    Console.WriteLine("Creating temp directory");
                    Directory.CreateDirectory(directoryPath);
                }

                string filePath = Path.Combine(directoryPath, fileName);
                string exportedBy = Session["FullName"]?.ToString() ?? User?.Identity?.Name ?? "System";

                Console.WriteLine($"Full Path: {filePath}");
                Console.WriteLine($"Exported By: {exportedBy}");

                // Generate Excel file based on report type
                Console.WriteLine($"=== GENERATING EXCEL ===");
                var excelGenerator = new IPSClaimExcelExportGenerator();

                if (request.ReportType == "current" || request.IncludeSummary)
                {
                    // Generate with summary sheet
                    var exportOptions = new IPSClaimExportOptions
                    {
                        FileName = request.FileName,
                        StartDate = request.StartDate,
                        EndDate = request.EndDate,
                        IncludeSummary = request.IncludeSummary
                    };

                    excelGenerator.GenerateExcelWithSummary(exportData, filePath, exportedBy, exportOptions);
                }
                else
                {
                    // Generate simple export
                    excelGenerator.GenerateSimpleExcel(exportData, filePath);
                }

                // Verify file was created
                if (!System.IO.File.Exists(filePath))
                {
                    Console.WriteLine("ERROR: Excel file was not created");
                    return Json(new { success = false, message = "Failed to generate Excel file." });
                }

                Console.WriteLine($"Excel file generated successfully: {filePath}");
                Console.WriteLine($"File size: {new FileInfo(filePath).Length} bytes");

                // Read and send file to browser
                Console.WriteLine($"=== READING FILE ===");
                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                Console.WriteLine($"File bytes read: {fileBytes.Length} bytes");

                // Delete temp file after sending
                Console.WriteLine($"=== CLEANUP ===");
                System.IO.File.Delete(filePath);
                Console.WriteLine("Temp file deleted");

                Console.WriteLine($"=== EXPORT COMPLETE ===");
                Console.WriteLine($"Returning file: {fileName} ({fileBytes.Length} bytes)");

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== EXPORT ERROR ===");
                Console.WriteLine($"Error Type: {ex.GetType().Name}");
                Console.WriteLine($"Error Message: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                // Inner exception details
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Inner Stack Trace: {ex.InnerException.StackTrace}");
                }

                return Json(new
                {
                    success = false,
                    message = $"An error occurred while exporting claims data to Excel: {ex.Message}"
                });
            }
        }           

    }
}
