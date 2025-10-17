namespace TransportManagementSystem.Helpers
{
    public class ImportDto
    {
    }

    public class TruckImportDto
    {
        public string Type { get; set; } = string.Empty;
        public float Tonnage { get; set; }
        public float Volume { get; set; }
        public string ExpeditionName { get; set; } = string.Empty;
    }

    public class OperationImportDto
    {
        public float Rate { get; set; }
        public string ExpeditionName { get; set; } = string.Empty;
        public string RouteAbbr { get; set; } = string.Empty;
    }
}
