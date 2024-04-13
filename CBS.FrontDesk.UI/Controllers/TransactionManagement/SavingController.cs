using CBS.BusinessService.CustomerManagement;
using CBS.FrontDesk.Data.Entity.Config;
using CBS.FrontDesk.Data.Entity.CustomerManagement;
using CBS.FrontDesk.Data.Entity;
using CBS.FrontDesk.Data.Message;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using CBS.BusinessService.Accounts;
using CBS.FrontDesk.Data.Entity.SavingProducts;
using CBS.FrontDesk.Data.Entity.SavingProducts.AccountActivation;

namespace CBS.FrontDesk.UI.Controllers
{
    public class SavingController : BaseController
    {
        // GET: TransactionManagement
        private readonly AccountServices _acountServices;

        public SavingController(AccountServices acountServices)
        {
            _acountServices = acountServices;
        }
        public ActionResult Accounts()
        {
            return View();
        }
        //

        [HttpPost]
        public async Task<ActionResult> LoadData(string Path)
        {
            var dataTable = await _acountServices.GetDataTable(GetDataTableOptions(),Path);
            return Json(new { draw = dataTable.draw, recordsFiltered = dataTable.recordsTotal, recordsTotal = dataTable.recordsTotal, data = dataTable.data });

        }

        [HttpPost]
        public async Task<ActionResult> LoadDataSearch(string search)
        {
            var dataTable = await _acountServices.GetDataTableSearch(GetDataTableOptions(), search);
            return Json(new { draw = dataTable.draw, recordsFiltered = dataTable.recordsTotal, recordsTotal = dataTable.recordsTotal, data = dataTable.data });

        }


    }
}