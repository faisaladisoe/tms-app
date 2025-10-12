using System.ComponentModel.DataAnnotations;

namespace TransportManagementSystem.Models
{
    public class Truck
    {
        public int Id { get; set; }
        public required string Type { get; set; }
        
        [Display(Name = "Tonnage (kg)")]
        public required float Tonnage { get; set; }

        [Display(Name = "Volume (pallet)")]
        public required float Volume { get; set; }

        // Foreign Key
        [Display(Name = "Expedition")]
        public required int ExpeditionId { get; set; }

        // Navigation
        public Expedition? Expedition { get; set; }
    }
}
