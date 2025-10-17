using Microsoft.EntityFrameworkCore;

namespace TransportManagementSystem.Helpers
{
    internal static class ImportValidator
    {
        internal static List<string> Validate<T>(List<T> items, DbContext context) where T : class
        {
            var errors = new List<string>();
            var type = typeof(T);
            var props = type.GetProperties().ToDictionary(p => p.Name, p => p);
            if (!ImportValidationRules.Rules.TryGetValue(type, out var rule))
                return errors;

            // Checking for "required" rule
            for (int i = 0; i < items.Count; i++)
            {
                int row = i + 2;
                foreach (var req in rule.Required)
                {
                    if (props.TryGetValue(req, out var prop))
                    {
                        var val = prop.GetValue(items[i]);
                        if (val == null || (val is string s && string.IsNullOrEmpty(s)))
                            errors.Add($"{type.Name} Row {row}: {req} is required");
                    }
                }
            }

            // Checking for "duplicate" rule
            foreach (var uniq in rule.Unique)
            {
                var seen = new HashSet<object?>();
                for (int i = 0; i < items.Count; i++)
                {
                    var row = i + 2;
                    var val = props[uniq].GetValue(items[i]);
                    if (!seen.Add(val))
                        errors.Add($"{type.Name} Row {row}: Duplicate {uniq} in file: {val}");
                }
            }

            // Checking for "duplicate" rule in DB
            foreach (var uniq in rule.Unique)
            {
                var dbValues = context.Set<T>()
                    .Select(e => EF.Property<object>(e, uniq))
                    .Where(v => v != null)
                    .ToHashSet();

                for (int i = 0; i < items.Count; i++)
                {
                    var row = i + 2;
                    var val = props[uniq].GetValue(items[i]);
                    if (val != null && dbValues.Contains(val))
                        errors.Add($"{type.Name} Row {row}: {uniq} already exists in DB: {val}");
                }
            }

            return errors;
        }

    }
}
