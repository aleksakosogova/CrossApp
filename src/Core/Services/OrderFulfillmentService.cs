using Core.Domain;

namespace Core.Services;

public class OrderFulfillmentService
{
    // Інваріант на дві сутності: замовлення не може бути виконане, якщо його кількість перевищує залишок продукту
    public static void ValidateAndFulfill(Product product, Order order)
    {
        if (product.Sku != order.ProductSku)
        {
            throw new InvalidOperationException($"Товар з SKU {order.ProductSku} не відповідає продукту на складі {product.Sku}.");
        }

        if (order.RequestedQuantity > product.Quantity)
        {
            throw new InvalidOperationException($"Неможливо виконати замовлення {order.OrderId}: замовлено {order.RequestedQuantity}, а на складі є лише {product.Quantity}.");
        }

        product.Issue(order.RequestedQuantity);
    }
}