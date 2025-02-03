using API.DTO;
using Core.Entities.OrderAggregate;

namespace API.Extensions;

public static class OrderMappingExtensions
{
    public static OrderDTO ToDto(this Order order)
    {
        return new OrderDTO
        {
            Id = order.Id,
            BuyerEmail = order.BuyerEmail,
            OrderDate = order.OrderDate,
            ShippingAddress = order.ShippingAddress,
            PaymentSummary = order.PaymentSummary,
            DeliveryMethod = order.DeliveryMethod.Description,
            ShippingPrice = order.DeliveryMethod.Price,
            OrderItems = order.OrderItems.Select(x => x.ToDto()).ToList(),
            Subtotal = order.Subtotal,
            Discount = order.Discount,
            Total = order.GetTotal(),
            Status = order.Status.ToString(),
            PaymentIntentId = order.PaymentIntentId 
        };
    }

    public static OrderItemDTO ToDto(this OrderItem orderItem)
    {
        return new OrderItemDTO
        {
            ProductId = orderItem.ItemOrdered.ProductId,
            ProductName = orderItem.ItemOrdered.ProductName,
            ImageUrl = orderItem.ItemOrdered.ImageUrl,
            Price = orderItem.Price,
            Quantity = orderItem.Quantity
        };
    }
}
