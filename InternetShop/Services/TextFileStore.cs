using System.Globalization;
using InternetShop.Models;

namespace InternetShop.Services;

/// <summary>Simple pipe-delimited UTF-8 persistence for shop data.</summary>
public sealed class TextFileStore
{
    private readonly string dataDirectory;
    public TextFileStore(string? dataDirectory = null) => this.dataDirectory = dataDirectory ?? Path.Combine(AppContext.BaseDirectory, "data");
    public string DataDirectory => dataDirectory;

    public void Load(Inventory inventory, IList<AUser> users, IList<Order> orders)
    {
        Directory.CreateDirectory(dataDirectory);
        var usersPath = Path.Combine(dataDirectory, "users.txt");
        var productsPath = Path.Combine(dataDirectory, "products.txt");
        var ordersPath = Path.Combine(dataDirectory, "orders.txt");
        if (!File.Exists(usersPath) || !File.Exists(productsPath) || !File.Exists(ordersPath))
        {
            Seed(users, inventory, orders);
            Save(users, inventory.GetCatalog(), orders);
            return;
        }

        foreach (var line in File.ReadLines(usersPath).Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var f = line.Split('|');
            if ((f.Length != 7 && f.Length != 8) || !int.TryParse(f[1], out var id)) continue;

            // В новом формате роль находится сразу после email:
            // type|id|name|email|role|number|passwordHash|blocked
            var role = f.Length == 8 ? f[4].Trim().ToLowerInvariant() : f[0].ToLowerInvariant();
            var numberIndex = f.Length == 8 ? 5 : 4;
            var passwordIndex = f.Length == 8 ? 6 : 5;
            var blockedIndex = f.Length == 8 ? 7 : 6;
            AUser user = role switch
            {
                "admin" or "administrator" => new Administrator(id, int.Parse(f[numberIndex], CultureInfo.InvariantCulture), Decode(f[3])),
                "manager" => new Manager(id, int.Parse(f[numberIndex], CultureInfo.InvariantCulture), Decode(f[3])),
                "customer" => new Customer(id, Decode(f[2]), Decode(f[3])),
                _ => null!
            };
            if (user is null || !user.SetPasswordHash(f[passwordIndex])) continue;
            if (bool.TryParse(f[blockedIndex], out var blocked) && blocked) user.Block();
            users.Add(user);
        }
        foreach (var line in File.ReadLines(productsPath).Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var f = line.Split('|');
            if (f.Length != 4) continue;
            inventory.AddNewProduct(new Product(int.Parse(f[0], CultureInfo.InvariantCulture), Decode(f[1]), decimal.Parse(f[2], CultureInfo.InvariantCulture), int.Parse(f[3], CultureInfo.InvariantCulture)));
        }
        foreach (var line in File.ReadLines(ordersPath).Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var f = line.Split('|');
            if (f.Length != 6) continue;
            var order = new Order(int.Parse(f[0], CultureInfo.InvariantCulture), Array.Empty<OrderItem>(), DateTime.Parse(f[1], CultureInfo.InvariantCulture));
            foreach (var item in f[4].Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var fields = item.Split(':');
                if (fields.Length != 3 || !int.TryParse(fields[0], out var productId) || !int.TryParse(fields[1], out var quantity) || !decimal.TryParse(fields[2], CultureInfo.InvariantCulture, out var price)) continue;
                order.AddPersistedItem(new OrderItem(inventory.GetProduct(productId), quantity, price));
            }
            if (order.OrderItems.Count == 0) continue;
            order.SetPersistedStatus(f[2], Decode(f[3]));
            order.CustomerId = int.Parse(f[5], CultureInfo.InvariantCulture);
            orders.Add(order);
        }
    }

    public void Save(IEnumerable<AUser> users, IEnumerable<Product> products, IEnumerable<Order> orders)
    {
        Directory.CreateDirectory(dataDirectory);
        Write("users.txt", "type|id|name|email|role|number|passwordHash|blocked", users.Select(u =>
            $"{(u is Customer ? "Customer" : u is Manager ? "Manager" : "Administrator")}|{u.Id}|{Encode(u is Customer c ? c.Name : "")}|{Encode(u.Email)}|{(u is Administrator ? "admin" : u is Manager ? "manager" : "customer")}|{(u is Manager m ? m.EmployeeId : u is Administrator a ? a.AdminLevel : 0)}|{u.PasswordHash}|{u.IsBlocked}"));
        Write("products.txt", "id|name|price|stock", products.Select(p => $"{p.Id}|{Encode(p.Name)}|{p.Price.ToString(CultureInfo.InvariantCulture)}|{p.StockQuantity}"));
        Write("orders.txt", "id|createdAtUtc|status|tracking|items|customerId", orders.Select(o =>
            $"{o.OrderId}|{o.CreatedAt.ToString("O", CultureInfo.InvariantCulture)}|{o.CurrentState.Name}|{Encode(o.TrackingNumber ?? "")}|{string.Join(',', o.OrderItems.Select(i => $"{i.Product.Id}:{i.Quantity}:{i.PriceAtPurchase.ToString(CultureInfo.InvariantCulture)}"))}|{o.CustomerId}"));
    }

    private void Write(string name, string header, IEnumerable<string> lines)
    {
        var path = Path.Combine(dataDirectory, name);
        var temp = path + ".tmp";
        File.WriteAllLines(temp, new[] { header }.Concat(lines));
        File.Move(temp, path, true);
    }
    private static string Encode(string value) => Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(value));
    private static string Decode(string value)
    {
        try { return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(value)); }
        catch (FormatException) { return value; }
    }

    private static void Seed(IList<AUser> users, Inventory inventory, IList<Order> orders)
    {
        var admin = new Administrator(1, 1, "admin@shop.local"); admin.Register("admin123"); users.Add(admin);
        var manager = new Manager(2, 1001, "manager@shop.local"); manager.Register("manager123"); users.Add(manager);
        var customer = new Customer(3, "Иван", "customer@shop.local"); customer.Register("customer123"); users.Add(customer);
        inventory.AddNewProduct(new Product(1, "Ноутбук", 85000m, 4));
        inventory.AddNewProduct(new Product(2, "Мышь", 2500m, 10));
    }
}
