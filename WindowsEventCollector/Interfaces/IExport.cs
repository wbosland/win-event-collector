using System.Collections.Generic;

namespace WindowsEventCollector.Interfaces
{
    public interface IExport
    {
        void Export<T>(IEnumerable<T> collection, string filePath);
    }
}