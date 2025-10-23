using Microsoft.EntityFrameworkCore;

namespace TransportManagementSystem.Helpers
{
    internal class FileValidation
    {
        public string Error { get; set; } = string.Empty;
        public bool IsValid => Error.Length == 0;
    }

    internal static class ImportValidator
    {
        internal static FileValidation ValidateFile(IFormFile file)
        {
            const int MAX_FILE_SIZE = 1024 * 1024 * 5;      // 5 MB
            FileValidation error = new();

            // Ensure there's always an excel file.
            if (file == null || file.Length == 0)
            {
                error.Error = "Please upload a valid Excel file.";
                return error;
            }

            // Ensure upload a valid excel format.
            string[] allowedExtensions = { ".xlsx", ".xls" };
            if (file != null)
            {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!Array.Exists(allowedExtensions, item => item == extension))
                {
                    error.Error = "Only Excel files (.xlsx, .xls) are supported.";
                    return error;
                }
            }

            // Ensure a reasonable excel file size.
            if (file != null && file.Length > MAX_FILE_SIZE)
            {
                error.Error = "The excel file size exceeds the maximum allowed size (5 MB)";
                return error;
            }

            return error;
        }

        internal static List<string> Validate<T>(IEnumerable<T> items, DbContext context) where T : class
        {
            var errors = new List<string>();
            var type = typeof(T);
            if (!ImportValidationRules.Rules.TryGetValue(type, out var rules))
                return errors;

            // Row starts from number two.
            var indexedItems = items.Select((item, index) => new { Item = item, Row = index + 2 }).ToList();

            // Required fields
            foreach (var x in indexedItems)
            {
                foreach (var req in rules.Required)
                {
                    var val = type.GetProperty(req)?.GetValue(x.Item);
                    if (val == null || (val is string s && string.IsNullOrWhiteSpace(s)))
                        errors.Add($"{type.Name} Row {x.Row}: {req} is required");
                }
            }

            // Build hash sets for each unique set
            var dbItems = context.Set<T>().AsNoTracking().ToList();
            var dbKeySets = rules.UniqueSets.ToDictionary(
                uniqSet => uniqSet,
                uniqSet => new HashSet<string>(
                    dbItems.Select(dbItem =>
                        string.Join("|", uniqSet.Select(p =>
                            type.GetProperty(p)?.GetValue(dbItem)?.ToString() ?? "")))
                )
            );

            // Uniqueness (single or composite)
            foreach (var uniqSet in rules.UniqueSets)
            {
                // Check duplicates within Excel
                var duplicates = indexedItems
                    .GroupBy(x => string.Join("|", uniqSet.Select(p =>
                        type.GetProperty(p)?.GetValue(x.Item)?.ToString() ?? "")))
                    .Where(g => g.Count() > 1);

                foreach (var dup in duplicates)
                {
                    var rows = string.Join(", ", dup.Select(x => x.Row));
                    errors.Add($"{type.Name}: Duplicate combination on {string.Join(", ", uniqSet)} = {dup.Key} at rows {rows}");
                }

                // Check conflicts against DB
                foreach (var x in indexedItems)
                {
                    var key = string.Join("|", uniqSet.Select(p =>
                        type.GetProperty(p)?.GetValue(x.Item)?.ToString() ?? ""));
                    if (dbKeySets[uniqSet].Contains(key))
                        errors.Add($"{type.Name} Row {x.Row}: Combination already exists in DB for {string.Join(", ", uniqSet)} = {key}");
                }
            }

            return errors;
        }

    }
}
