using TransportManagementSystem.Models;

namespace TransportManagementSystem.Helpers
{
    internal static class ImportValidationRules
    {
        internal static readonly Dictionary<Type, (string[] Required, string[][] UniqueSets)> Rules = new()
        {
            { typeof(Expedition), (new[] { "Name" }, new[] { new[] { "Name" } }) },
            { typeof(Truck), (new[] { "Type", "Tonnage", "Volume", "ExpeditionId" }, Array.Empty<string[]>()) },
            { typeof(Models.Route), (new[] { "Name", "Abbr", "Distance" }, new[] { new[] { "Name" }, new[] { "Abbr" }, new[] { "Name", "Abbr" } }) },
            { typeof(Operation), (new[] { "Rate", "ExpeditionId", "RouteId" }, new[] { new[] { "ExpeditionId", "RouteId" } }) },
            { typeof(Product), (new[] { "Code", "Description", "Size", "Dimension", "BoxPerPallet", "GrossWeight" }, new[] { new[] { "Code" } }) },
        };
    }
}
