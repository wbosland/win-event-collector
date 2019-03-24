using System;
using System.Collections.Generic;
using System.Text;

namespace WindowsEventCollector.Interfaces
{
    public interface IExportService
    {
        void ExportToExcel<T>(IEnumerable<T> collection, string filePath);
    }
}
