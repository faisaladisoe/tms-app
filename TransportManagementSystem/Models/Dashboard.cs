namespace TransportManagementSystem.Models
{
    public class Dashboard
    {
        public Truck? Truck { get; set; }
        public float RemainingTruckTonnage { get; set; } = 0;
        public float RemainingTruckVolume { get; set; } = 0;
        public ICollection<LoadedProduct> Products { get; set; } = new List<LoadedProduct>();
    }

    public class LoadedProduct
    {
        public Product? Product { get; set; }
        public int? Quantity { get; set; }
    }
}
