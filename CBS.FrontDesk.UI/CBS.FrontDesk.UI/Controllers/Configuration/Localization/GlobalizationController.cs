using CBS.BusinessService.CustomerManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CBS.BusinessService.Config.Localization;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Message;
using System.Threading.Tasks;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity;

namespace CBS.FrontDesk.UI.Controllers.Configuration.Localization
{
    //[CheckSessionTimeOutAttribute]

    public class GlobalizationController : BaseController
    {
        private readonly CountryServices _countryServices;
        private readonly RegionServices _regionServices;
        private readonly DivisionServices _divisionServices;
        private readonly SubDivisionServices _subDivisionServices;
        private readonly TownServices _townServices;

        public GlobalizationController(
            CountryServices countryServices,
            RegionServices regionServices,
            DivisionServices divisionServices,
            SubDivisionServices subDivisionServices,
            TownServices townServices)
        {
            _countryServices = countryServices;
            _regionServices = regionServices;
            _divisionServices = divisionServices;
            _subDivisionServices = subDivisionServices;
            _townServices = townServices;
        }

        // 🌍 Page Entry - load all data
        public async Task<ActionResult> Index()
        {
            var model = new GlobalizationViewModel
            {
                Countries = (await _countryServices.GetCountries()).ToList(),
                Regions = (await _regionServices.GetRegions()).ToList(),
                Divisions = (await _divisionServices.GetDivisions()).ToList(),
                SubDivisions = (await _subDivisionServices.GetSubDivisions()).ToList(),
                Towns = (await _townServices.GetTowns()).ToList()
            };

            return View(model);
        }


        #region 📌 Save/CRUD via Forms

        [HttpPost]
        public async Task<ActionResult> SaveCountry(GlobalizationViewModel model)
        {
            var result = await _countryServices.Create(model.Country);
            TempData["Message"] = Messaging.MessageResult(result);
            return Json(new { success = result.Result, status = result.MessageStatus, message = Messaging.MessageResult(result) });
        }

        [HttpPost]
        public async Task<ActionResult> SaveRegion(GlobalizationViewModel model)
        {
            var result = await _regionServices.Create(model.Region);
            TempData["Message"] = Messaging.MessageResult(result);
            return Json(new { success = result.Result, status = result.MessageStatus, message = Messaging.MessageResult(result) });
        }

        [HttpPost]
        public async Task<ActionResult> SaveDivision(GlobalizationViewModel model)
        {
            var result = await _divisionServices.Create(model.Division);
            TempData["Message"] = Messaging.MessageResult(result);
            return Json(new { success = result.Result, status = result.MessageStatus, message = Messaging.MessageResult(result) });
        }

        [HttpPost]
        public async Task<ActionResult> SaveSubDivision(GlobalizationViewModel model)
        {
            var result = await _subDivisionServices.Create(model.SubDivision);
            TempData["Message"] = Messaging.MessageResult(result);
            return Json(new { success = result.Result, status = result.MessageStatus, message = Messaging.MessageResult(result) });
        }

        [HttpPost]
        public async Task<ActionResult> SaveTown(GlobalizationViewModel model)
        {
            var result = await _townServices.Create(model.Town);
            TempData["Message"] = Messaging.MessageResult(result);
            return Json(new { success = result.Result, status = result.MessageStatus, message = Messaging.MessageResult(result) });
        }

        #endregion

        #region 🔽 Cascading JSON for Dropdowns

        [HttpGet]
        public async Task<JsonResult> GetRegionsByCountry(string countryId)
        {
            var data = await _regionServices.GetRegionsByCountryId(countryId);
            return Json(data.Select(x => new { x.Id, x.Name }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<JsonResult> GetDivisionsByRegion(string regionId)
        {
            var data = await _divisionServices.GetDivisionsByRegionId(regionId);
            return Json(data.Select(x => new { x.Id, x.Name }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<JsonResult> GetSubdivisionsByDivision(string divisionId)
        {
            var data = await _subDivisionServices.GetSubdivisionsByDivisionId(divisionId);
            return Json(data.Select(x => new { x.Id, x.Name }), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<JsonResult> GetTownsBySubDivision(string subDivisionId)
        {
            var data = await _townServices.GetTownsBySubDivisionId(subDivisionId);
            return Json(data.Select(x => new { x.Id, x.Name }), JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region ⚙️ Partial Loader (Modals/Edits)

        [HttpGet]
        public async Task<ActionResult> InitializeData(string key = null, string partialView = null, string path = null, string mode = null,string serviceOption=null)
        {
            ViewBag.KEY = key;
            var model = new GlobalizationViewModel();
            bool isGetById = key!=null;

            switch (path?.ToLowerInvariant())
            {
                case "list":
                    model.Countries = (await _countryServices.GetCountries()).ToList();
                    break;

                case "region":
                    if (isGetById)
                        model.Region = await _regionServices.GetRegion(key);
                    else
                        model.Regions = (await _regionServices.GetRegionsByCountryId(key)).ToList();

                    model.Countries = (await _countryServices.GetCountries()).ToList(); // for dropdown
                    break;

                case "division":
                    if (isGetById)
                        model.Division = await _divisionServices.GetDivision(key);
                    else
                        model.Divisions = (await _divisionServices.GetDivisionsByRegionId(key)).ToList();

                    model.Regions = (await _regionServices.GetRegionsByCountryId("all")).ToList(); // fallback if GetAllRegions not available
                    break;

                case "subdivision":
                    if (isGetById)
                        model.SubDivision = await _subDivisionServices.GetSubDivision(key);
                    else
                        model.SubDivisions = (await _subDivisionServices.GetSubdivisionsByDivisionId(key)).ToList();

                    model.Divisions = (await _divisionServices.GetDivisionsByRegionId("all")).ToList();
                    break;

                case "town":
                    if (isGetById)
                        model.Town = await _townServices.GetTown(key);
                    else
                        model.Towns = (await _townServices.GetTownsBySubDivisionId(key)).ToList();

                    model.SubDivisions = (await _subDivisionServices.GetSubdivisionsByDivisionId("all")).ToList();
                    break;

                default:
                    if (isGetById)
                        model.Country = await _countryServices.GetCountry(key);
                    else
                        model.Countries = (await _countryServices.GetCountries()).ToList();
                    break;
            }

            return PartialView(partialView, model);
        }

        #endregion

        [HttpPost]
        public async Task<ActionResult> Delete(string key, string type)
        {
            ExecutionMessages result = null;

            switch (type?.ToLower())
            {
                case "country":
                    result = await _countryServices.Delete(key);
                    break;

                case "region":
                    result = await _regionServices.Delete(key);
                    break;

                case "division":
                    result = await _divisionServices.Delete(key);
                    break;

                case "subdivision":
                    result = await _subDivisionServices.Delete(key);
                    break;

                case "town":
                    result = await _townServices.Delete(key);
                    break;

                default:
                    return Json(new
                    {
                        success = false,
                        status = "InvalidType",
                        message = "Unknown delete type specified."
                    });
            }

            TempData["Message"] = Messaging.MessageResult(result);

            return Json(new
            {
                success = result.Result,
                status = result.MessageStatus,
                message = Messaging.MessageResult(result)
            });
        }

    }
}