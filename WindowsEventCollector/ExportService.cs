using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using WindowsEventCollector.Interfaces;

namespace WindowsEventCollector
{
    public class ExportService : IExportService
    {
        public void ExportToExcel<T>(IEnumerable<T> collection, string filePath)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Event Logs");
                worksheet.FirstCell().InsertTable(CreateDataTable(collection), false);
                worksheet.Rows(1, 1).Style.Font.SetBold();
                worksheet.Columns().AdjustToContents();
                worksheet.Columns().Style.Alignment.SetWrapText(false);
                worksheet.Columns().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Left);
                workbook.SaveAs(filePath);
            }
        }

        private static DataTable CreateDataTable<T>(IEnumerable<T> list)
        {
            Type type = typeof(T);
            var properties = type.GetProperties();

            DataTable dataTable = new DataTable();
            foreach (PropertyInfo info in properties)
            {
                dataTable.Columns.Add(new DataColumn(info.Name, Nullable.GetUnderlyingType(info.PropertyType) ?? info.PropertyType));
            }

            foreach (T entity in list)
            {
                object[] values = new object[properties.Length];
                for (int i = 0; i < properties.Length; i++)
                {
                    values[i] = properties[i].GetValue(entity);
                }

                dataTable.Rows.Add(values);
            }

            return dataTable;
        }
    }
}
