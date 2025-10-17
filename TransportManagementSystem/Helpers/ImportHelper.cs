using OfficeOpenXml;
using System.Reflection;

namespace TransportManagementSystem.Helpers
{
    internal class ImportHelper
    {
        internal static List<T> ReadSheet<T>(ExcelWorksheet ws) where T : new()
        {
            var props = typeof(T).GetProperties();
            var headers = new Dictionary<int, PropertyInfo>();
            
            // Map headers
            for (int col = 1; col <= ws.Dimension.Columns; col++)
            {
                var header = ws.Cells[1, col].Text;
                var prop = props.FirstOrDefault(p => p.Name.Equals(header, StringComparison.OrdinalIgnoreCase));
                if (prop != null)
                    headers[col] = prop;
            }

            var list = new List<T>();
            for (int row = 2; row <= ws.Dimension.Rows; row++)
            {
                var obj = Activator.CreateInstance<T>()!;
                foreach (KeyValuePair<int, PropertyInfo> kvp in headers)
                {
                    var cellValue = ws.Cells[row, kvp.Key].Text;
                    if (!string.IsNullOrEmpty(cellValue))
                    {
                        var targetType = Nullable.GetUnderlyingType(kvp.Value.PropertyType) ?? kvp.Value.PropertyType;
                        var safeValue = Convert.ChangeType(cellValue, targetType);
                        kvp.Value.SetValue(obj, safeValue);
                    }
                }
                list.Add(obj);
            }
            return list;
        }
    }
}
