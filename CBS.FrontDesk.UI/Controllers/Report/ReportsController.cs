//using CrystalDecisions.CrystalReports.Engine;
//using CrystalDecisions.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace CBS.FrontDesk.UI.Controllers
{

    public class ReportsController : Controller
    {
        // GET: Reports
        //public void ReportParameterLess()
        //{
        //    try
        //    {
        //        bool isValid = true;
        //        string strReportName = System.Web.HttpContext.Current.Session["ReportName"].ToString();
        //        var rptSource = System.Web.HttpContext.Current.Session["rptSource"];
        //        var rptpath = System.Web.HttpContext.Current.Session["rptpath"].ToString();
        //        var rpttitle = System.Web.HttpContext.Current.Session["rpttitle"].ToString();
        //        string year = "Non";
        //        string dates = "Non";
        //        string strFromDate = "Non";     // Setting FromDate 
        //        string strToDate = "Non";         // Setting ToDate    
        //        if (!string.IsNullOrEmpty(Session["Year"] as string))
        //        {
        //            year = System.Web.HttpContext.Current.Session["Year"].ToString();
        //        }
        //        if (!string.IsNullOrEmpty(Session["Dates"] as string))
        //        {
        //            dates = System.Web.HttpContext.Current.Session["Dates"].ToString();
        //        }
        //        if (!string.IsNullOrEmpty(Session["DateFrom"] as string))
        //        {
        //            strFromDate = System.Web.HttpContext.Current.Session["DateFrom"].ToString();
        //        }
        //        if (!string.IsNullOrEmpty(Session["DateTo"] as string))
        //        {
        //            strToDate = System.Web.HttpContext.Current.Session["DateTo"].ToString();
        //        }


        //        if (string.IsNullOrEmpty(strReportName))
        //        {
        //            isValid = false;
        //        }
        //        if (isValid)
        //        {
        //            ReportDocument rd = new ReportDocument();
        //            string strRptPath = Server.MapPath(rptpath);
        //            if (!rptSource.Equals("empty"))
        //            {
        //                rd.Load(strRptPath);
        //                if (rptSource != null && rptSource.GetType().ToString() != "System.String")
        //                    rd.SetDataSource(rptSource);
        //                if (year != "Non")
        //                {
        //                    rd.SetParameterValue("param", $"Header summary: {year}");

        //                }
        //                if (dates != "Non")
        //                {
        //                    if (!string.IsNullOrEmpty(strFromDate))
        //                        rd.SetParameterValue("DateFrom", strFromDate);
        //                    if (!string.IsNullOrEmpty(strToDate))
        //                        rd.SetParameterValue("DateTo", strToDate);
        //                }

        //                string SavedFileName = string.Format($"{rpttitle}-{DateTime.UtcNow.ToString("dd_mm_yyyy_hhmmss")}");
        //                rd.ExportToHttpResponse(ExportFormatType.PortableDocFormat, System.Web.HttpContext.Current.Response, false, SavedFileName);
        //                CleanReport(rd);


        //            }
        //            else
        //            {
        //                Response.Write("<p>Data source is empty</p>");
        //            }
        //        }
        //        else
        //        {
        //            Response.Write("<H2>No Report with such name found</H2>");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Response.Write(ex.ToString());
        //        if (ex.Message.Contains("Error in formula  PercentagePassed"))
        //        {
        //            Response.Write("<H2>No Data was found</H2>");
        //        }
        //        else
        //        {
        //            Response.Write(ex.ToString() + "<H2>Nothing Found; report session expired</H2>");
        //        }

        //    }
        //}
        //public void CleanReport(ReportDocument rd)
        //{
        //    rd.Close();
        //    rd.Clone();
        //    rd.Dispose();
        //    rd = null;
        //    GC.Collect();
        //    GC.WaitForPendingFinalizers();
        //}
        //public void ReportWithParameter()
        //{
        //    try
        //    {
        //        bool isValid = true;
        //        string strReportName = System.Web.HttpContext.Current.Session["ReportName"].ToString();
        //        var rptSource = System.Web.HttpContext.Current.Session["rptSource"];
        //        var rptpath = System.Web.HttpContext.Current.Session["rptpath"].ToString();
        //        string strFromDate = System.Web.HttpContext.Current.Session["DateFrom"].ToString();     // Setting FromDate 
        //        string strToDate = System.Web.HttpContext.Current.Session["DateTo"].ToString();         // Setting ToDate    
        //        string strtitle = System.Web.HttpContext.Current.Session["rpttitle"].ToString();         // Setting ToDate    

        //        if (string.IsNullOrEmpty(strReportName))
        //        {
        //            isValid = false;
        //        }
        //        if (isValid)
        //        {
        //            ReportDocument rd = new ReportDocument();
        //            string strRptPath = Server.MapPath(rptpath);
        //            rd.Load(strRptPath);
        //            if (rptSource != null && rptSource.GetType().ToString() != "System.String")
        //                rd.SetDataSource(rptSource);
        //            if (!string.IsNullOrEmpty(strFromDate))
        //                rd.SetParameterValue("DateFrom", strFromDate);
        //            if (!string.IsNullOrEmpty(strToDate))
        //                rd.SetParameterValue("DateTo", strToDate);

        //            string SavedFileName = string.Format($"{strtitle}-{DateTime.UtcNow.ToString("dd_mm_yyyy_hhmmss")}");
        //            rd.ExportToHttpResponse(ExportFormatType.PortableDocFormat, System.Web.HttpContext.Current.Response, false, SavedFileName);
        //            CleanReport(rd);
        //        }
        //        else
        //        {
        //            Response.Write("<H2>Nothing Found; No Report name found</H2>");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex.Message.Contains("Error in formula PercentagePassed"))
        //        {
        //            Response.Write("<H2>No Data was found</H2>");
        //        }
        //        else
        //        {
        //            Response.Write(ex.ToString() + "<H2>Nothing Found; report session expired</H2>");
        //        }

        //    }
        //}
        public ActionResult DownloadExcelFile()
        {
            var rptSource = System.Web.HttpContext.Current.Session["rptSource"];
            string strtitle = System.Web.HttpContext.Current.Session["rpttitle"].ToString();
            var model = rptSource;
            Export export = new Export();
            export.ToExcel(Response, model as IEnumerable<object>, strtitle);

            return new EmptyResult();
        }


    }
    public class Export
    {
        public void ToExcel(HttpResponseBase response, IEnumerable<object> object_list, string fileName)
        {
            try
            {
                var grid = new System.Web.UI.WebControls.GridView();

                // Ensure type safety
                var nonNullClientsList = object_list?.Where(item => item != null) ?? Enumerable.Empty<object>();

                grid.DataSource = nonNullClientsList;
                grid.DataBind();

                response.ClearContent();
                response.AddHeader("content-disposition", $"attachment; filename={fileName}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xls");
                response.ContentType = "application/excel";

                using (StringWriter sw = new StringWriter())
                {
                    using (HtmlTextWriter htw = new HtmlTextWriter(sw))
                    {
                        grid.RenderControl(htw);
                        response.Write(sw.ToString());
                        response.End();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                Console.WriteLine($"Error exporting to Excel: {ex.Message}");
            }
        }
    }


}