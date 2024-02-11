using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBS.FrontDesk.Data.Entity.DataTable
{
    public static class DatatableHelper
    {
        public static async Task<CustomDataTable> GenerateDataTable<T>(DataTableOptions dataTableOptions, Func<Task<List<T>>> getDataFunc) where T : class
        {
            List<T> data = (await getDataFunc()).ToList();
            var filteredData = FilterData(data, dataTableOptions);
            var dataTable = new CustomDataTable(Convert.ToInt32(dataTableOptions.draw), data.Count(), dataTableOptions.recordsFiltered, filteredData, dataTableOptions);
            return dataTable;
        }

        public static List<T> FilterData<T>(List<T> items, DataTableOptions dataTableOptions) where T : class
        {
            var data = items;
            string searchValue = dataTableOptions.searchValue.ToLower();

            if (!string.IsNullOrEmpty(searchValue))
            {
                data = data.Where(m => m.GetType().GetProperties().Any(prop =>
                    prop.GetValue(m).ToString().ToLower().Contains(searchValue) == true)).ToList();
            }

            dataTableOptions.recordsTotal = data.Count;

            // Sorting
            if (!string.IsNullOrEmpty(dataTableOptions.sortColumnName))
            {
                var property = typeof(T).GetProperty(dataTableOptions.sortColumnName);
                if (property != null)
                {
                    if (dataTableOptions.sortDirection == "asc")
                    {
                        data = data.OrderBy(x => property.GetValue(x, null)).ToList();
                    }
                    else
                    {
                        data = data.OrderByDescending(x => property.GetValue(x, null)).ToList();
                    }
                }
            }

            var dataList = data.Skip(dataTableOptions.skip).Take(dataTableOptions.pageSize).ToList();
            return dataList;
        }
    }


}
