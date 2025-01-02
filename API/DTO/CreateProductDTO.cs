using System.ComponentModel.DataAnnotations;

namespace API.DTO;

public class CreateProductDTO
{
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    public string Description { get; set; } = string.Empty;
    [Required]
    public string Type { get; set; } = string.Empty;
    [Required]
    public string Brand { get; set; } = string.Empty;
    [Required]
    public string ImageUrl { get; set; } = string.Empty;
    [Range(0.01, double.MaxValue, ErrorMessage = "Price should be greater than 0.")]
    public decimal Price { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Quantity should be atleast 1")]
    public int QtyInStock { get; set; }
}
