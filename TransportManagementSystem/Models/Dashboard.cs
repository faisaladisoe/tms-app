namespace TransportManagementSystem.Models
{
    public class Dashboard
    {
        public List<SuggestedTruck> Trucks { get; set; } = new List<SuggestedTruck>();
        public ICollection<LoadedProduct> Products { get; set; } = new List<LoadedProduct>();

        public Route? ShipmentRoute { get; set; }
        public float TotalWeight => Products.Sum(p => (p.Product?.GrossWeight ?? 0) * (p.Quantity ?? 0));
        public float TotalVolume => Products.Sum(p => (p.Quantity ?? 0) / (float)(p.Product?.BoxPerPallet ?? 1));
        public float TotalQuantity { get; set; } = 0;
    }

    public class SuggestedTruck
    {
        public Truck? Truck { get; set; }
        public float? Rate { get; set; }
        public float? TotalRate { get; set; }
        public float TonnageFit { get; set; }
        public float VolumeFit { get; set; }
    }

    public class LoadedProduct
    {
        public Product? Product { get; set; }
        public int? Quantity { get; set; }
    }
}
