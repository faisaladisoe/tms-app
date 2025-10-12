using System.ComponentModel.DataAnnotations;

namespace TransportManagementSystem.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Display(Name = "Material number")]
        public required string Code { get; set; }

        [Display(Name = "Material description")]
        public required string Description { get; set; }

        [Display(Name = "Material size (length x width)")]
        public required string Size { get; set; }

        [Display(Name = "Dimension (m³)")]
        public required float Dimension { get; set; }

        [Display(Name = "Box per pallet (qty)")]
        public required int BoxPerPallet { get; set; }

        [Display(Name = "Gross weight (kg)")]
        public required float GrossWeight { get; set; }
    }
}
