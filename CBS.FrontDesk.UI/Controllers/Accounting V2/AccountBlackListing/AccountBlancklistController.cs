using CBS.BusinessService.Accounting_V2.Affiliate;
using CBS.BusinessService.Accounting_V2.AffiliateAccounts;
using CBS.BusinessService.Accounting_V2.BranchAccountService;
using CBS.BusinessService.Config;
using CBS.FrontDesk.Data.Entity.Accounting_V2.AccountBlackList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Accounting_V2.AccountBlackListing
{
    public class AccountBlancklistController : Controller
    {
        private readonly AffiliateService _AffiliateServices;
        private readonly BranchServices _branchServices;
        private readonly BranchAccountService _branchAccountService;

        /// <summary>
        /// Injects the required AffiliateController via dependency injection.
        /// </summary>
        /// <param name="CategoryConfigService">The service for cheque admin operations.</param>
        public AccountBlancklistController(BranchAccountService branchAccountService, AffiliateService affiliateService, BranchServices branchServices)
        {
            _AffiliateServices = affiliateService;
            _branchServices = branchServices;
            _branchAccountService = branchAccountService;
        }
        // GET: AccountBlancklist
        public async Task<ActionResult> Index()
        {
            await loader();
            return View(new AccountBlacklist());
        }

        public async Task<bool> loader()
        {
            var branches = await _branchServices.GetBranches();
            ViewBag.Branches = branches;

            var branchAccountId = await _branchAccountService.GetBranchFromEndpointAsync();
            ViewBag.branchAccountId = branchAccountId;
            

            return true;
        }
    }
}