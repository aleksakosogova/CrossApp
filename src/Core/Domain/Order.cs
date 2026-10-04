namespace Core.Domain;

public sealed class Order
{
    public string OrderId { get; }
    public string ProductSku { get; }
    public int RequestedQuantity { get; }

    public Order(string orderId, string productSku, int requestedQuantity)
    {
        OrderId = orderId;
        ProductSku = productSku;
        RequestedQuantity = requestedQuantity;
    }
}