using System.ComponentModel.DataAnnotations;

namespace TransportManagementSystem.Models
{
    public class Expedition
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        // Navigation
        public ICollection<Truck> Trucks { get; set; } = new List<Truck>();
        public ICollection<Operation> Operations { get; set; } = new List<Operation>();
    }
}
