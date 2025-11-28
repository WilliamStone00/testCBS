using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.BusinessService.Accounting_V2.ExportReports
{
    public class TrialBalance6ColumnsExport
    {
        public void ExportTb6(dynamic tb)
        {
           var data = tb;

            var BranchName = data[0].BranchName;
            var BranchCode = data[0].BranchCode;
            //var BranchCode = data[0].BranchCode;

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Trial Balance");
                ws.Cell(1, 1).Value = BranchName;

               
            }

            



        }
    }
}
