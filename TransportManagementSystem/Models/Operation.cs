using System.ComponentModel.DataAnnotations;

namespace TransportManagementSystem.Models
{
    public class Operation
    {
        public int Id { get; set; }

        [Display(Name = "Rate per kg (Rupiah)")]
        public required float Rate { get; set; }

        // Foreign Keys
        [Display(Name = "Expedition")]
        public required int ExpeditionId { get; set; }

        [Display(Name = "Route name")]
        public required int RouteId { get; set; }

        // Navigation
        public Expedition? Expedition { get; set; }
        public Route? Route { get; set; }
    }
}
