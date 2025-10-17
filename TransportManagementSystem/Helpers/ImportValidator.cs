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
