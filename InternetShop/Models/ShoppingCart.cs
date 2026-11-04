namespace InternetShop.Models;

public sealed class ShoppingCart
{
    private readonly List<OrderItem> items = new();
    public IReadOnlyList<OrderItem> Items => items;

    public void AddItem(Product product, int quantity)
    {
        if (!product.CheckAvailability(quantity)) throw new InvalidOperationException("Товар недоступен в указанном количестве.");
        var existing = items.FirstOrDefault(item => item.Product.Id == product.Id);
        if (existing is not null)
        {
            var totalQuantity = existing.Quantity + quantity;
            if (!product.CheckAvailability(totalQuantity)) throw new InvalidOperationException("Недостаточно товара на складе.");
            items.Remove(existing);
            items.Add(new OrderItem(product, totalQuantity));
            return;
        }
        items.Add(new OrderItem(product, quantity));
    }

    public void SetQuantity(int productId, int quantity)
    {
        if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Количество должно быть больше нуля.");

        var existing = items.FirstOrDefault(item => item.Product.Id == productId)
            ?? throw new KeyNotFoundException("Товар не найден в корзине.");
        if (!existing.Product.CheckAvailability(quantity))
            throw new InvalidOperationException("Нельзя установить количество больше остатка на складе.");

        items[items.IndexOf(existing)] = new OrderItem(existing.Product, quantity, existing.PriceAtPurchase);
    }

    public void RemoveItem(int productId) => items.RemoveAll(item => item.Product.Id == productId);
    public decimal CalculateTotal() => items.Sum(item => item.GetSubtotal());
    public bool IsEmpty => items.Count == 0;
    public IReadOnlyList<OrderItem> CheckoutItems() => items.ToArray();
    public void Clear() => items.Clear();
}
