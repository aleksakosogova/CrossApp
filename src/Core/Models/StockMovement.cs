namespace Core.Domain;

public record StockMovement(string Sku, int ChangedQuantity, string Type);