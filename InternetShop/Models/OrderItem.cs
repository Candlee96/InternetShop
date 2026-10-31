namespace InternetShop.Models;

public sealed class OrderItem
{
    public Product Product { get; }
    public int Quantity { get; }
    public decimal PriceAtPurchase { get; }

    public OrderItem(Product product, int quantity) : this(product, quantity, product?.Price ?? 0m) { }

    public OrderItem(Product product, int quantity, decimal priceAtPurchase)
    {
        Product = product ?? throw new ArgumentNullException(nameof(product));
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));
        if (priceAtPurchase < 0) throw new ArgumentOutOfRangeException(nameof(priceAtPurchase));
        Quantity = quantity;
        PriceAtPurchase = priceAtPurchase;
    }

    public decimal GetSubtotal() => PriceAtPurchase * Quantity;
}
