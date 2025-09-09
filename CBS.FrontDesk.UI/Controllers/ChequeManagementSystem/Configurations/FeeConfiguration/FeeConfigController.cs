using CBS.BusinessService.CheckManagementSystem.Configurations.FeeConfiguration;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.CheckManagementSystem.Configurations.FeeConfiguration;
using CBS.FrontDesk.Data.Message;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.ChequeManagementSystem.Configurations.FeeConfiguration
{
 //   [CheckSessionTimeOut]
    public class FeeConfigController : BaseController
    {
        private readonly FeeConfigService _feeConfigService;
        private readonly BranchServices _branchServices;

        public FeeConfigController(FeeConfigService feeConfigService, BranchServices branchServices)
        {
            _feeConfigService = feeConfigService;
            _branchServices = branchServices;
        }

        public async Task<ActionResult> Index()
        {
            await Loader();
            return View(new FeeConfig());
        }

        private async Task Loader()
        {
            ViewBag.Branches = await _branchServices.GetBranches();
            ViewBag.FeeTypes = await _feeConfigService.GetFeeTypesMockAsync(); // Using Mock
        }

        // THIS IS THE ONLY INITIALIZE ACTION WE NEED
        public async Task<ActionResult> InitializeData(string partialView, bool? isCentralized, string feeType, string branchId = null)
        {
            bool isCentralizedValue = isCentralized ?? false;
            await Loader();
            FeeConfig model = null;

            if (!string.IsNullOrWhiteSpace(feeType))
            {
                var configs = await _feeConfigService.GetConfigsMockAsync(feeType, isCentralizedValue ? null : branchId, isCentralizedValue);
                model = configs.FirstOrDefault();
            }

            if (model == null)
            {
                model = new FeeConfig
                {
                    IsCentralized = isCentralizedValue,
                    BranchId = isCentralizedValue ? null : branchId,
                    FeeType = feeType,
                    IsActive = true,
                    AcceptPercentage = true
                };
            }
            return PartialView(partialView, model);
        }

        [HttpPost]
        public async Task<ActionResult> CreateOrUpdate(FeeConfig model)
        {

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, status = "Failed", message = "Please fill all required fields." });
            }

            ExecutionMessages data;
            if (string.IsNullOrWhiteSpace(model.Id))
            {
                data = await _feeConfigService.CreateMockAsync(model);
            }
            else
            {
                data = await _feeConfigService.UpdateMockAsync(model);
            }
            return Json(new { success = data.Result, status = data.MessageStatus, message = Messaging.MessageResult(data) });
        }

        [HttpGet]
        public async Task<ActionResult> Delete(string KEY)
        {
            if (string.IsNullOrWhiteSpace(KEY))
            {
                return Json(new { success = false, status = "Failed", message = "Invalid ID provided." });
            }
            var result = await _feeConfigService.DeleteMockAsync(KEY);
            return Json(new { success = result.Result, status = result.MessageStatus, message = Messaging.MessageResult(result) });
        }
    }
}