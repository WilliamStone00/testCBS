using CBS.BusinessService.Config;
using CBS.BusinessService.CustomerManagement;
using CBS.BusinessService.Session;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.DailySaverManagementP
{
    //[CheckSessionTimeOutAttribute]

    public class DailySaverManagementController : BaseController
    {

        private readonly DailySaverServices _dailySaverServices;
        private readonly BranchServices _branchServices;
        private readonly LocationAggregateService _locationService;
        public DailySaverManagementController(BranchServices branchServices = null, LocationAggregateService locationService = null, DailySaverServices dailySaverServices = null)
        {

            _branchServices = branchServices;
            _locationService=locationService;
            _dailySaverServices=dailySaverServices;
        }
        public async Task<ActionResult> Index()
        {

            await LoadLookupsAsync(); // Branches, Countries, EconomicActivities, etc.
            var vm = new DailySaverCreateVm
            {
                DateOfBirth = DateTime.Today.AddYears(-18),
                IDNumberIssueDate = DateTime.Today.AddYears(-5),
                IsNewCustomer = true
            };
            return View(vm);
        }
        private async Task LoadLookupsAsync()
        {
            ViewBag.Branches = await _branchServices.GetBranches();
            var agrAggregates = await _dailySaverServices.GetAggregates();
            ViewBag.Countries = agrAggregates.Countries;
            ViewBag.Regions = agrAggregates.Regions;
            ViewBag.Divisions = agrAggregates.Divisions;
            ViewBag.Subdivisions = agrAggregates.Subdivisions;
            ViewBag.Towns = agrAggregates.Towns;
            ViewBag.EconomicActivities = agrAggregates.EconomicActivities;
            ViewBag.MaritalStatuses = agrAggregates.CustomerDefaultEnum.maritalStatuses;
            ViewBag.Languages = _dailySaverServices.GetLanguages();
        }

       
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string serviceOption = null, string path = null)
        {
            ViewBag.KEY = KEY;
            //if (KEY != "null")
            //{
            //    var customer = await InitializeCustomerData(KEY);
            //    return PartialView(partialView, customer);
            //}
            //else
            //{
            //    if (serviceOption == "branch")
            //    {
            //        var data = await _individualProfileServices.GetIndividualProfileByBranch();
            //        return PartialView(partialView, data);
            //    }
            //    else if (serviceOption == "all")
            //    {
            //        //var data = await _individualProfileServices.GetMembers();
            //        return PartialView(partialView, null);
            //    }

            //}
            return PartialView(partialView, new List<IndividualProfile>());
        }

        //[HttpPost, ValidateAntiForgeryToken]
        //public async Task<ActionResult> AddOrUpdate(DailySaverCreateVm vm)
        //{
        //    if (!vm.IsNewCustomer)
        //    {
        //        if (string.IsNullOrWhiteSpace(vm.CustomerId) || !System.Text.RegularExpressions.Regex.IsMatch(vm.CustomerId, @"^\d{10}$"))
        //            ModelState.AddModelError(nameof(vm.CustomerId), "CustomerId must be exactly 10 digits for existing members.");

        //        // (Optional) verify CustomerId exists in your DB for the selected branch
        //        // var exists = await _memberService.ExistsAsync(vm.CustomerId, vm.BranchId);
        //        // if (!exists) ModelState.AddModelError(nameof(vm.CustomerId), "No member with this CustomerId in the selected branch.");
        //    }
        //    if (ModelState.IsValid)
        //    {
        //        var data = await _dailySaverServices.Create(vm);
        //        return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
        //    }
        //    else
        //    {
        //        var errorMessages = ModelState.Values
        //            .SelectMany(v => v.Errors)
        //            .Where(e => e.ErrorMessage != null)
        //            .Select(e => e.ErrorMessage)
        //            .ToList();

        //        // Convert the list of error messages to a single string with each message on a new line
        //        string errorMessage = string.Join("\n", errorMessages);

        //        // Pass the error message as the message
        //        return Json(new { success = false, status = false, message = errorMessage });
        //    }


        //}

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> AddOrUpdate(DailySaverCreateVm vm)
        {
            // Normalize common fields to avoid whitespace issues
            vm.BranchId          = vm.BranchId?.Trim();
            vm.CustomerId        = vm.CustomerId?.Trim();
            vm.DailySaverId      = vm.DailySaverId?.Trim();
            vm.FirstName         = vm.FirstName?.Trim();
            vm.LastName          = vm.LastName?.Trim();
            vm.Phone             = vm.Phone?.Trim();
            vm.Email             = vm.Email?.Trim();
            vm.IDNumber          = vm.IDNumber?.Trim();
            vm.IDNumberIssueAt   = vm.IDNumberIssueAt?.Trim();
            vm.Occupation        = vm.Occupation?.Trim();
            vm.Address           = vm.Address?.Trim();

            // 0) Branch required (selected inside the form)
            if (string.IsNullOrWhiteSpace(vm.BranchId))
                ModelState.AddModelError(nameof(vm.BranchId), "Branch is required.");

            // 1) Path-specific validation
            if (vm.IsNewCustomer)
            {
                if (string.IsNullOrWhiteSpace(vm.DailySaverId) ||
       !System.Text.RegularExpressions.Regex.IsMatch(vm.DailySaverId, @"^\d{1,7}$"))
                {
                    var len = string.IsNullOrEmpty(vm.DailySaverId) ? 0 : vm.DailySaverId.Length;
                    ModelState.AddModelError(nameof(vm.DailySaverId),
                        $"Daily saver account number must be between 1 and 7 digits. You entered {len} digit(s).");
                }

                // Optional: ensure uniqueness within the branch
                // if (await _dailySaverServices.ExistsDailySaverIdAsync(vm.BranchId, vm.DailySaverId))
                //     ModelState.AddModelError(nameof(vm.DailySaverId), "This daily saver account number already exists in the selected branch.");
            }
            else
            {
                // Existing member: CustomerId must be exactly 10 digits
                if (string.IsNullOrWhiteSpace(vm.CustomerId) ||
                    !System.Text.RegularExpressions.Regex.IsMatch(vm.CustomerId, @"^\d{10}$"))
                {
                    var len = string.IsNullOrEmpty(vm.CustomerId) ? 0 : vm.CustomerId.Length;
                    ModelState.AddModelError(nameof(vm.CustomerId),
                        $"CustomerId must be exactly 10 digits. You entered {len} digit(s).");
                }

                // Optional: verify the member exists for the selected branch
                // var exists = await _memberService.ExistsAsync(vm.CustomerId, vm.BranchId);
                // if (!exists) ModelState.AddModelError(nameof(vm.CustomerId), "No member with this CustomerId in the selected branch.");
            }

            if (!ModelState.IsValid)
            {
                // Build a field → errors map for better client-side UX
                var fieldErrors = ModelState
                    .Where(kvp => kvp.Value?.Errors != null && kvp.Value.Errors.Count > 0)
                    .ToDictionary(
                        kvp => kvp.Key,                // field name
                        kvp => kvp.Value.Errors
                              .Select(e => string.IsNullOrEmpty(e.ErrorMessage) ? "Invalid value." : e.ErrorMessage)
                              .ToArray()
                    );

                // Also keep your flat message list (backward compatible)
                var errorMessages = fieldErrors.SelectMany(kvp => kvp.Value).ToList();
                var errorMessage = string.Join("\n", errorMessages);

                return Json(new { success = false, status = false, message = errorMessage, fieldErrors });
            }

            // All good → proceed
            var data = await _dailySaverServices.Create(vm);
            return Json(new
            {
                success = data.Result,
                status = data.MessageStatus,
                message = Messaging.MessageResult(data)
            });
        }


        [HttpGet]
        public JsonResult GetRegionsByCountry(string countryId)
        {
            var regions = _locationService.GetRegionsByCountry(countryId);
            return Json(regions.Select(r => new { r.Id, r.Name }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetDivisionsByRegion(string regionId)
        {
            var divisions = _locationService.GetDivisionsByRegion(regionId);
            return Json(divisions.Select(d => new { d.Id, d.Name }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetSubDivisionsByDivision(string divisionId)
        {
            var subDivisions = _locationService.GetSubDivisionsByDivision(divisionId);
            return Json(subDivisions.Select(s => new { s.Id, s.Name }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetTownsBySubDivision(string subDivisionId)
        {
            var towns = _locationService.GetTownsBySubDivision(subDivisionId);
            return Json(towns.Select(t => new { t.Id, t.Name }), JsonRequestBehavior.AllowGet);
        }

    }
}