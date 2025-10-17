using OfficeOpenXml;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
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
                TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
                var header = ws.Cells[1, col].Text.Trim();
                var capitalizedHeader = textInfo.ToTitleCase(header);
                var normalizedHeader = string.Join("", capitalizedHeader.Split());

                foreach (var prop in props)
                {
                    var displayAttr = prop.GetCustomAttribute<DisplayAttribute>();
                    var displayName = displayAttr?.Name ?? prop.Name;

                    if (string.Equals(normalizedHeader, displayName, StringComparison.OrdinalIgnoreCase) || 
                        string.Equals(normalizedHeader, prop.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        headers[col] = prop;
                        break;
                    }
                }
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
