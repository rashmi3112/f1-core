namespace Core.Entities.OrderAggregate;

public enum OrderStatus
{
    Pending,
    PaymentReceived,
    PaymentFailed,
    Shipped,  //not in crse added extra
    Delivered  //not in crse added extra
}
