using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Helper.Helper
{
    public class ExportOption : IExportOption
    {
        public object data { get; set; }
        public byte[] bytes { get; set; }
        public string path { get; set; }
        public string filename { get; set; }

    }
    public static class ExcelExportHelper
    {
        public static string ExcelContentType
        {
            get
            { return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"; }
        }
        public static DataTable ListToDataTable<T>(List<T> data)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            DataTable dataTable = new DataTable();

            for (int i = 0; i < properties.Count; i++)
            {
                PropertyDescriptor property = properties[i];
                dataTable.Columns.Add(property.Name, Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType);
            }

            object[] values = new object[properties.Count];
            foreach (T item in data)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = properties[i].GetValue(item);
                }

                dataTable.Rows.Add(values);
            }
            return dataTable;
        }
        public static byte[] ExportExcel(DataTable dataTable, string sheetName, bool showheader = true)
        {
            byte[] result = null;
            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dataTable, sheetName);
                wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                wb.Style.Font.Bold = true;
                wb.ShowRowColHeaders = showheader;
                wb.PageOptions.ShowRowAndColumnHeadings = showheader;
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    result = stream.ToArray();
                }
            }
            return result;
        }

        public static byte[] ExportExcel<T>(List<T> data, string sheetName = "Sheet-1", bool showheader = true)
        {
            var export = ExportExcel(ListToDataTable<T>(data.ToList()), sheetName, showheader);
            return export;
        }

    }

}
