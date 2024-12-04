using CBS.BusinessService.Accounting;
using CBS.BusinessService.Accounts;
using CBS.BusinessService;
using CBS.FrontDesk.Data.Entity.Accounting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CBS.BusinessService.Config;
using CBS.BusinessService.UserManagement;
using System.Threading;
using System.Threading.Tasks;
using CBS.FrontDesk.Data;
using System.Web.Services.Description;
using CBS.FrontDesk.Data.Message;
namespace CBS.FrontDesk.UI.Controllers.Accounting
{
    public class DocumentConfigurationController : BaseController
    {

        private readonly ChartOfAccountServices _chartOfAccountServices;

        private readonly BranchServices _branchService;
        private readonly AccountingServices _AccountServices;
        private readonly AccountClassServices _AccountClassServices;
        private readonly DocumentReportServices _documentServices;
        private readonly DocumentTypeServices _documentTypeServices;
        private readonly ChartOfAccountManagementPositionService _ChartOfAccountManagementPositionServicesServices;
        private readonly DocumentRefereceCodeServices _documentRefereceCodeServices;
        private readonly CorrespondingMappingServices _correspondingMappingServices;

        public DocumentConfigurationController()
        {
            _documentServices = new DocumentReportServices();
            _documentTypeServices = new DocumentTypeServices();
            _chartOfAccountServices = new ChartOfAccountServices();
            _documentRefereceCodeServices = new DocumentRefereceCodeServices();
            _correspondingMappingServices = new CorrespondingMappingServices();



        }
        // GET: DocumentConfiguration
        public ActionResult Index()
        {
            return View(new DocumentConfiguration());
        }

        private async Task GetList()
        {

            var DebitAccounts = await _AccountServices.GetAllAccounting();
            var CreditAccounts = BuildMenuViewBag(DebitAccounts);
            ViewBag.Accounts = CreditAccounts;
            var listAccounts = await _chartOfAccountServices.GetAllChartOfAccounts();

            ViewBag.ChartOfAccounts = BuildMenuAccountViewBag(listAccounts.ToList());

            ViewBag.BookingDirections = await this.GetBookingDirections();
            ViewBag.CreditAccounts = ViewBag.ChartOfAccounts;
            ViewBag.DebitAccounts = ViewBag.ChartOfAccounts;

        }


        private dynamic BuildMenuViewBag(IEnumerable<Data.Account> debitAccounts)
        {
            List<System.Web.WebPages.Html.SelectListItem> list = new List<System.Web.WebPages.Html.SelectListItem>();
            if (debitAccounts != null)

                foreach (var item in debitAccounts)
                {

                    list.Add(new System.Web.WebPages.Html.SelectListItem { Text = item.Id, Value = item.AccountNumber + "-" + item.AccountName });

                }

            return list;
        }

        private dynamic BuildMenuAccountViewBag(List<ChartOfAccount> ListchartOfAccounts)
        {
            List<System.Web.WebPages.Html.SelectListItem> selectListItems = new List<System.Web.WebPages.Html.SelectListItem>();

            foreach (var item in ListchartOfAccounts)
            {
                selectListItems.Add(new System.Web.WebPages.Html.SelectListItem { Text = item.Id, Value = $"{item.AccountNumber} - {item.LabelEn}" });
            }
            return selectListItems;
        }
        public async Task<ActionResult> InitializeData(string KEY = null, string partialView = null, string path = null, string serviceOption = null)
        {
            await GetList();
            var partialResult = await GetServiceAction(path, partialView, KEY, serviceOption);

            return partialResult;
        }

        private async Task<PartialViewResult> GetServiceAction(string path, string partialView, string key, string serviceOption)
        {
            if (serviceOption == "documentReferenceCode")
            {
                if (path == "list")
                {

                    var data = await _documentRefereceCodeServices.GetAllDocumentReferenceCodeModel();

                    var sysData = new DocumentConfiguration { DocumentReferenceCodes = data.ToList() };
                    return PartialView(partialView, sysData);

                }
                else if (path == "new")
                {

                    return PartialView(partialView, new DocumentConfiguration { ServiceOption = "documentReferenceCode", DocumentReferenceCode = new DocReferenceCode(), CorrespondingMapping = new CorrespondingMapping { } });
                }
                else if (path == "details")
                {
                    var data = await _documentRefereceCodeServices.GetDocumentReferenceCode(key);
                    var datast = await _correspondingMappingServices.GetAllCorrespondingMappingByDocumentReference(key);
                    var datastEx = await _correspondingMappingServices.GetAllCorrespondingMappingExceptionByDocumentReference(key);

                    return PartialView(partialView, new DocumentConfiguration { ServiceOption = "documentReferenceCode", DocumentReferenceCodeDto = data, CorrespondingMappingDataDto = datast, CorrespondingMappingExceptionDataDto = datastEx });
                }
                else
                {
                    var data = await _documentRefereceCodeServices.GetDocumentReferenceCode(key);
                    var datast = await _correspondingMappingServices.GetAllCorrespondingMappingByDocumentReference(key);
                    var datastEx = await _correspondingMappingServices.GetAllCorrespondingMappingExceptionByDocumentReference(key);

                    return PartialView(partialView, new DocumentConfiguration { ServiceOption = "documentReferenceCode", DocumentReferenceCodeDto = data, CorrespondingMappingDataDto = datast });

                }

            }
            else if (serviceOption == "correspondingMapping")
            {
                if (path == "list")
                {

                    var data = await _correspondingMappingServices.GetAllCorrespondingMappingByDocumentReference(key);

                    var sysData = new DocumentConfiguration { CorrespondingMappingDataDto = data.ToList() };
                    return PartialView(partialView, sysData);

                }
                else if (path == "new")
                {
                    return PartialView(partialView, new DocumentConfiguration { });
                }
                else
                {
                    var data = await _correspondingMappingServices.GetCorrespondingMapping(key);


                    return PartialView(partialView, new DocumentConfiguration { CorrespondingMappingDto = data });
                }

            }
            else if (serviceOption == "correspondingMappingException")
            {
                if (path == "list")
                {

                    var data = await _correspondingMappingServices.GetAllCorrespondingMappingExceptionByDocumentReference(key);

                    var sysData = new DocumentConfiguration { CorrespondingMappingExceptionDataDto = data.ToList() };
                    return PartialView(partialView, sysData);

                }
                else if (path == "new")
                {
                    return PartialView(partialView, new DocumentConfiguration { });
                }

                else
                {
                    var data = await _correspondingMappingServices.GetCorrespondingMappingException(key);


                    return PartialView(partialView, new DocumentConfiguration { CorrespondingMappingDto = data });
                }

            }
            else if (serviceOption == "document")
            {
                if (path == "list")
                {

                    var data = await _documentServices.GetAllDocument();

                    var sysData = new DocumentConfiguration { Documents = data.ToList() };
                    return PartialView(partialView, sysData);

                }
                else if (path == "new")
                {
                    return PartialView(partialView, new DocumentConfiguration { Document = new Document() });
                }

                else
                {
                    var data = await _documentServices.GetDocument(key);


                    return PartialView(partialView, new DocumentConfiguration { Document = data });
                }

            }
            else if (serviceOption == "documentType")
            {
                if (path == "list")
                {

                    var data = await _documentTypeServices.GetAllDocumentType();

                    var sysData = new DocumentConfiguration { DocumentTypes = data.ToList() };
                    return PartialView(partialView, sysData);

                }
                else if (path == "new")
                {
                    return PartialView(partialView, new DocumentConfiguration { DocumentType = new DocumentType() });
                }
                else
                {
                    var data = await _documentTypeServices.GetDocumentType(key);


                    return PartialView(partialView, new DocumentConfiguration { DocumentType = data });
                }

            }
            return null;
        }

        private async Task<Func<Task<ExecutionMessages>>> GetInsertServiceActionAsync(string serviceOption, DocumentConfiguration model)
        {
            //
            if (serviceOption == "document")
            {

                return () => _documentServices.Create(model.Document);
            }
            else if (serviceOption == "documentType")
            {
                return () => _documentTypeServices.Create(model.DocumentType);
            }
            else if (serviceOption == "documentReferenceCode")
            {
                return () => _documentRefereceCodeServices.CreateCode(model.DocumentReferenceCode);
            }
            else if (serviceOption == "correspondingMapping")
            {
                return () => _correspondingMappingServices.Create(model.CorrespondingMapping);
            }
            else if (serviceOption == "correspondingMappingException")
            {
                return () => _correspondingMappingServices.CreateException(model.CorrespondingMappingException);
            }
            else
            {
                return null;
            }
        }

        private Func<Task<ExecutionMessages>> GetUpdateServiceAction(string serviceOption, DocumentConfiguration model)
        {
            if (serviceOption == "document")
            {

                return () => _documentServices.Update(model.Document);
            }
            else if (serviceOption == "documentType")
            {
                return () => _documentTypeServices.Update(model.DocumentType);
            }
         
     
            else if (serviceOption == "documentReferenceCode")
            {

                return () => _documentRefereceCodeServices.Update(model.DocumentReferenceCodeDto);
            }
            else if (serviceOption == "correspondingMapping")
            {

                return () => _correspondingMappingServices.Update(model.CorrespondingMapping);
            }
            else if (serviceOption == "correspondingMappingException")
            {

                return () => _correspondingMappingServices.UpdateException(model.CorrespondingMappingException);
            }
        
            else
            {
                return null;
            }
        }

    }
}