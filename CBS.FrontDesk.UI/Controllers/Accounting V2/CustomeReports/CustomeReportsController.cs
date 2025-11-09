using System.Threading.Tasks;
using System.Web.Mvc;

namespace CBS.FrontDesk.UI.Controllers.Accounting_V2
{


  public class CustomeReportsController : BaseController
    {
        public CustomeReportsController()
        {
        }

        public async Task<ActionResult> Index()
        {
            return View();
        }


    }
}
