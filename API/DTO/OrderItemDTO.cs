namespace API.DTO;

public class OrderItemDTO
{
    public int ProductId { get; set; }
    public  required string ProductName { get; set; }
    public required string ImageUrl { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}