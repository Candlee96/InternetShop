namespace InternetShop.Models;

public sealed class OrderItem
{
    public Product Product { get; }
    public int Quantity { get; }
    public decimal PriceAtPurchase { get; }

    public OrderItem(Product product, int quantity)
    {
        Product = product ?? throw new ArgumentNullException(nameof(product));
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        Quantity = quantity;
        PriceAtPurchase = product.Price;
    }

    public decimal GetSubtotal() => PriceAtPurchase * Quantity;
}
