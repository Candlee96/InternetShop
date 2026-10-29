namespace InternetShop.Models;

public sealed class Product
{
    public int Id { get; }
    public string Name { get; }
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }

    public Product(int id, string name, decimal price, int stockQuantity = 0)
    {
        if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Product name is required.", nameof(name));
        if (price < 0) throw new ArgumentOutOfRangeException(nameof(price));
        if (stockQuantity < 0) throw new ArgumentOutOfRangeException(nameof(stockQuantity));

        Id = id;
        Name = name.Trim();
        Price = price;
        StockQuantity = stockQuantity;
    }

    public string GetInfo() => $"{Id}: {Name} — {Price:C} (остаток: {StockQuantity})";

    public bool CheckAvailability(int amount) => amount > 0 && StockQuantity >= amount;

    public void ChangePrice(decimal price)
    {
        if (price < 0) throw new ArgumentOutOfRangeException(nameof(price));
        Price = price;
    }

    public void ChangeStock(int amount)
    {
        if (StockQuantity + amount < 0) throw new InvalidOperationException("Недостаточно товара на складе.");
        StockQuantity += amount;
    }
}
