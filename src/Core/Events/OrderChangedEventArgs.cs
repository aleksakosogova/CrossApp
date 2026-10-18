using System;

namespace Core.Events;

public sealed class OrderChangedEventArgs : EventArgs
{
    public string OrderId { get; }
    public OrderChangeKind Kind { get; }
    public int Delta { get; }
    public int QuantityAfter { get; }
    public DateTimeOffset At { get; } = DateTimeOffset.Now;

    public OrderChangedEventArgs(string orderId, OrderChangeKind kind, int delta, int quantityAfter)
    {
        OrderId = orderId;
        Kind = kind;
        Delta = delta;
        QuantityAfter = quantityAfter;
    }
}