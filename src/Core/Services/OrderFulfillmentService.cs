using System;
using System.Collections.Generic;
using System.Linq;
using Core.Domain;
using Core.Events;
using Core.Sinks;

namespace Core.Services;

public class OrderFulfillmentService
{
    private readonly INotificationSink? _sink; // Для додаткового завдання 1

    public OrderFulfillmentService(INotificationSink? sink = null)
    {
        _sink = sink;
    }

    // Оголошення події
    public event EventHandler<OrderChangedEventArgs>? Changed;

    // Метод підняття події
    private void OnChanged(Order order, Product product, OrderChangeKind kind, int delta)
    {
        Changed?.Invoke(this, new OrderChangedEventArgs(order.OrderId, kind, delta, product.Quantity));
        _sink?.Notify($"[Sink] {kind}: Замовлення {order.OrderId} виконано (Залишок {product.Sku}: {product.Quantity})");
    }

    // Прибрано static, щоб працювали події
    public void ValidateAndFulfill(Product product, Order order)
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

        // Піднімаємо подію ТІЛЬКИ після успішної зміни
        OnChanged(order, product, OrderChangeKind.OrderConfirmed, -order.RequestedQuantity);
    }
    
    // Крок 7: Func замість події для отримання значення
    public IReadOnlyList<Order> FindAll(IEnumerable<Order> orders, Func<Order, bool> predicate) =>
        orders.Where(predicate).ToList();
// Додаткове завдання 2: Action<string> як параметр методу імпорту
    public void ImportOrders(string[] data, Action<string> onLineSkipped)
    {
        foreach (var line in data)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                onLineSkipped("Порожній рядок"); // Викликаємо делегат для пропущеного рядка
                continue;
            }
            // Тут могла б бути логіка створення замовлення на основі рядка
        }
    }
}