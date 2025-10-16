using System.ComponentModel.DataAnnotations;

namespace TransportManagementSystem.Models
{
    public class Route
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Abbr { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Distance (km)")]
        public float Distance { get; set; } = 0;

        // Navigation
        public ICollection<Operation> Operations { get; set; } = new List<Operation>();
    }
}
