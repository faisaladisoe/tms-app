using System.ComponentModel.DataAnnotations;

namespace TransportManagementSystem.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Material number")]
        public string Code { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Material description")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Material size (length x width)")]
        public string Size { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Dimension (m³)")]
        public float Dimension { get; set; } = 0;

        [Required]
        [Display(Name = "Box per pallet (qty)")]
        public int BoxPerPallet { get; set; } = 0;

        [Required]
        [Display(Name = "Gross weight (kg)")]
        public float GrossWeight { get; set; } = 0;
    }
}
