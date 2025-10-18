using System.ComponentModel.DataAnnotations;

namespace TransportManagementSystem.Models
{
    public class Order
    {
        [Required]
        [Display(Name = "Material number")]
        public string Code { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Material quantity")]
        public int Quantity { get; set; } = 0;
    }
}
