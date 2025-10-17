using Microsoft.EntityFrameworkCore;

namespace TransportManagementSystem.Helpers
{
    internal static class ImportValidator
    {
        internal static List<string> Validate<T>(IEnumerable<T> items, DbContext context) where T : class
        {
            var errors = new List<string>();
            var type = typeof(T);
            if (!ImportValidationRules.Rules.TryGetValue(type, out var rules))
                return errors;

            // Required fields
            int rowRequired = 1;
            foreach (var item in items)
            {
                foreach (var req in rules.Required)
                {
                    var val = type.GetProperty(req)?.GetValue(item);
                    if (val == null || (val is string s && string.IsNullOrWhiteSpace(s)))
                        errors.Add($"{type.Name} Row {rowRequired}: {req} is required");
                }
                rowRequired++;
            }

            // Uniqueness (single or composite)
            var dbSet = context.Set<T>();
            var dbItems = dbSet.AsNoTracking().ToList();
            var indexedItems = items.Select((item, index) => new { Item = item, Row = index + 2 }).ToList();
            foreach (var uniqSet in rules.UniqueSets)
            {
                // Check duplicates within Excel
                var duplicates = indexedItems
                    .GroupBy(x => string.Join("|", uniqSet.Select(p => type.GetProperty(p)?.GetValue(x.Item)?.ToString() ?? "")))
                    .Where(g => g.Count() > 1);

                foreach (var dup in duplicates)
                {
                    var rows = string.Join(", ", dup.Select(x => x.Row));
                    errors.Add($"{type.Name}: Duplicate combination on {string.Join(", ", uniqSet)} = {dup.Key} at rows {rows}");
                }

                // Check conflicts against DB
                foreach (var item in indexedItems)
                {
                    var keyValues = uniqSet.Select(p => type.GetProperty(p)?.GetValue(item.Item)).ToArray();

                    var exists = dbItems.Any(dbItem =>
                        uniqSet.All(p =>
                            Equals(type.GetProperty(p)?.GetValue(dbItem),
                                   type.GetProperty(p)?.GetValue(item.Item))));

                    if (exists)
                    {
                        var formatted = string.Join(", ", uniqSet.Select((p, i) => $"{p}={keyValues[i]}"));
                        errors.Add($"{type.Name} Row {item.Row}: Combination already exists in DB for {formatted}");
                    }
                }
            }

            return errors;
        }

    }
}
