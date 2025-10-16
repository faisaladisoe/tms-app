using System.ComponentModel.DataAnnotations;

namespace TransportManagementSystem.Models
{
    public class Truck
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Type { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Tonnage (kg)")]
        public float Tonnage { get; set; } = 0;

        [Required]
        [Display(Name = "Volume (pallet)")]
        public float Volume { get; set; } = 0;

        // Foreign Key
        [Required]
        [Display(Name = "Expedition")]
        public int ExpeditionId { get; set; } = 0;

        // Navigation
        public Expedition? Expedition { get; set; }
    }
}
