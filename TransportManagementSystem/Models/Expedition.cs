namespace TransportManagementSystem.Models
{
    public class Expedition
    {
        public int Id { get; set; }
        public required string Name { get; set; }

        // Navigation
        public ICollection<Truck> Trucks { get; set; } = new List<Truck>();
        public ICollection<Operation> Operations { get; set; } = new List<Operation>();
    }
}
