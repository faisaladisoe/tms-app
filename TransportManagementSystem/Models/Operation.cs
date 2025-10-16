using System.ComponentModel.DataAnnotations;

namespace TransportManagementSystem.Models
{
    public class Operation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Rate per kg (Rupiah)")]
        public float Rate { get; set; } = 0;

        // Foreign Keys
        [Required]
        [Display(Name = "Expedition")]
        public int ExpeditionId { get; set; } = 0;

        [Required]
        [Display(Name = "Route name")]
        public int RouteId { get; set; } = 0;

        // Navigation
        public Expedition? Expedition { get; set; }
        public Route? Route { get; set; }
    }
}
