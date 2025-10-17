using TransportManagementSystem.Models;

namespace TransportManagementSystem.Helpers
{
    internal static class ImportValidationRules
    {
        internal static readonly Dictionary<Type, (string[] Required, string[] Unique)> Rules = new()
        {
            { typeof(Expedition), (new[] { "Name" }, new[] { "Name" }) },
            { typeof(Truck), (new[] { "Type", "Tonnage", "Volume", "ExpeditionId" }, new[] { "Type" }) },
            { typeof(Models.Route), (new[] { "Name", "Abbr", "Distance" }, new[] { "Name", "Abbr" }) },
            { typeof(Operation), (new[] { "Rate", "ExpeditionId", "RouteId" }, Array.Empty<string>()) },
            { typeof(Product), (new[] { "Code", "Description", "Size", "Dimension", "BoxPerPallet", "GrossWeight" }, new[] { "Code" }) },
        };
    }
}
