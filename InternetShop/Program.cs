using InternetShop.Models;
using InternetShop.Services;

var inventory = new Inventory();
inventory.AddNewProduct(new Product(1, "Ноутбук", 85000m, 4));
inventory.AddNewProduct(new Product(2, "Мышь", 2500m, 10));

var customer = new Customer(1, "Иван", "ivan@example.com");
customer.Register("secret1");
customer.Cart.AddItem(inventory.GetProduct(1), 1);
customer.Cart.AddItem(inventory.GetProduct(2), 2);

var orderService = new OrderService();
var order = orderService.CreateOrder(customer.Cart);
orderService.Pay(order, new CardPayment());
order.UpdateStatus();
order.AssignTrackingNumber("TRK-0001");

Console.WriteLine($"Заказ #{order.OrderId}: {order.CurrentState.Name}, сумма {order.CalculateTotal():C}");
Console.WriteLine($"Трекинг: {order.TrackingNumber}");
Console.WriteLine("Каталог:");
foreach (var product in inventory.GetCatalog()) Console.WriteLine(product.GetInfo());
