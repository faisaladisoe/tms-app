using System.ComponentModel.DataAnnotations;

namespace TransportManagementSystem.Models
{
    public class Route
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Abbr { get; set; }

        [Display(Name = "Distance (km)")]
        public required float Distance { get; set; }

        // Navigation
        public ICollection<Operation> Operations { get; set; } = new List<Operation>();
    }
}
