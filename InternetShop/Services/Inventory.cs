using InternetShop.Models;

namespace InternetShop.Services;

public sealed class Inventory
{
    private readonly Dictionary<int, Product> productList = new();

    public void AddNewProduct(Product product)
    {
        if (!productList.TryAdd(product.Id, product)) throw new InvalidOperationException("Товар с таким Id уже существует.");
    }

    public void UpdateQuantity(int productId, int amount) => GetProduct(productId).ChangeStock(amount);
    public IReadOnlyCollection<Product> GetCatalog() => productList.Values.ToArray();
    public Product GetProduct(int productId) => productList.TryGetValue(productId, out var product)
        ? product
        : throw new KeyNotFoundException($"Товар {productId} не найден.");
    public IEnumerable<Product> GetUnavailableProducts() => productList.Values.Where(product => product.StockQuantity == 0);
}
