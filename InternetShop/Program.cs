using InternetShop.Models;
using InternetShop.Services;

var inventory = new Inventory();
var users = new List<AUser>();
var orders = new List<Order>();
var store = new TextFileStore(Path.Combine(Directory.GetCurrentDirectory(), "InternetShop", "data"));
store.Load(inventory, users, orders);
var orderService = new OrderService();
if (orders.Count > 0) orderService.SetNextOrderId(orders.Max(o => o.OrderId) + 1);

Console.WriteLine("Интернет-магазин. Файлы данных: " + store.DataDirectory);
Console.WriteLine("Демо: customer@shop.local / customer123; manager@shop.local / manager123; admin@shop.local / admin123");
while (true)
{
    Console.WriteLine("\n1. Регистрация  2. Вход  3. Каталог  0. Выход");
    switch (Console.ReadLine())
    {
        case "1": Register(); break;
        case "2": Login(); break;
        case "3": ShowCatalog(); break;
        case "0": store.Save(users, inventory.GetCatalog(), orders); return;
        default: Console.WriteLine("Неизвестная команда."); break;
    }
}

void Register()
{
    var email = Ask("Email: ");
    if (users.Any(u => u.Email == email.Trim().ToLowerInvariant())) { Console.WriteLine("Пользователь уже существует."); return; }
    var name = Ask("Имя: ");
    var password = Ask("Пароль (не менее 6 символов): ");
    var userId = users.Count == 0 ? 1 : users.Max(u => u.Id) + 1;
    AUser user = email.Contains("admin", StringComparison.OrdinalIgnoreCase)
        ? new Administrator(userId, 1, email)
        : email.Contains("manager", StringComparison.OrdinalIgnoreCase)
            ? new Manager(userId, 1000 + userId, email)
            : new Customer(userId, name, email);

    if (!user.Register(password)) { Console.WriteLine("Некорректный пароль."); return; }
    users.Add(user);
    store.Save(users, inventory.GetCatalog(), orders);
    Console.WriteLine($"Регистрация выполнена. Роль: {user.GetType().Name}.");
}

void Login()
{
    var email = Ask("Email: ").Trim().ToLowerInvariant();
    var password = Ask("Пароль: ");
    var user = users.FirstOrDefault(u => u.Email == email);
    if (user is null || !user.Login(password))
    {
        Console.WriteLine("Неверные учетные данные или пользователь заблокирован.");
        return;
    }

    Console.WriteLine($"Выполнен вход: {user.Email} ({user.GetType().Name})");
    if (user is Customer customer) CustomerMenu(customer);
    else if (user is Manager manager) ManagerMenu(manager);
    else if (user is Administrator administrator) AdministratorMenu(administrator);
}

void ShowCatalog()
{
    foreach (var product in inventory.GetCatalog().OrderBy(p => p.Id)) Console.WriteLine(product.GetInfo());
}

void CustomerMenu(Customer customer)
{
    while (true)
    {
        Console.WriteLine("\n1. Каталог  2. Добавить в корзину  3. Корзина  4. Оформить заказ  0. Выход");
        switch (Console.ReadLine())
        {
            case "1": ShowCatalog(); break;
            case "2": AddToCart(customer); break;
            case "3": ShowCart(customer); break;
            case "4": Checkout(customer); break;
            case "0": return;
            default: Console.WriteLine("Неизвестная команда."); break;
        }
    }
}

void AddToCart(Customer customer)
{
    if (!int.TryParse(Ask("Id товара: "), out var id) || !int.TryParse(Ask("Количество: "), out var quantity)) { Console.WriteLine("Введите числа."); return; }
    try { customer.Cart.AddItem(inventory.GetProduct(id), quantity); Console.WriteLine("Товар добавлен."); }
    catch (Exception e) { Console.WriteLine(e.Message); }
}

void ShowCart(Customer customer)
{
    if (customer.Cart.IsEmpty) { Console.WriteLine("Корзина пуста."); return; }
    foreach (var item in customer.Cart.Items) Console.WriteLine($"{item.Product.Name}: {item.Quantity} x {item.PriceAtPurchase:C}");
    Console.WriteLine($"Итого: {customer.Cart.CalculateTotal():C}");
}

void Checkout(Customer customer)
{
    try
    {
        var order = orderService.CreateOrder(customer.Cart);
        order.CustomerId = customer.Id;
        if (orderService.Pay(order, new CardPayment())) { orders.Add(order); store.Save(users, inventory.GetCatalog(), orders); Console.WriteLine($"Заказ #{order.OrderId} оплачен."); }
    }
    catch (Exception e) { Console.WriteLine(e.Message); }
}

void ManagerMenu(Manager manager)
{
    while (true)
    {
        Console.WriteLine("\n1. Просмотреть заказы  2. Изменить статус  0. Выход");
        switch (Console.ReadLine())
        {
            case "1": ShowOrders(); break;
            case "2": ChangeOrderStatus(manager); break;
            case "0": return;
            default: Console.WriteLine("Неизвестная команда."); break;
        }
    }
}

void ShowOrders() { foreach (var o in orders) Console.WriteLine($"#{o.OrderId}, клиент {o.CustomerId}, {o.CurrentState.Name}, {o.CalculateTotal():C}"); }
void ChangeOrderStatus(Manager manager)
{
    if (!int.TryParse(Ask("Id заказа: "), out var id) || !int.TryParse(Ask("1 — отправить, 2 — доставить: "), out var action)) return;
    var order = orders.FirstOrDefault(o => o.OrderId == id);
    if (order is null) { Console.WriteLine("Заказ не найден."); return; }
    if (action == 1 && order.CurrentState is PaidState) order.UpdateStatus();
    else if (action == 2 && order.CurrentState is ShippedState) order.UpdateStatus();
    else { Console.WriteLine("Недопустимый переход статуса."); return; }
    store.Save(users, inventory.GetCatalog(), orders); Console.WriteLine("Статус изменен.");
}

void AdministratorMenu(Administrator administrator)
{
    while (true)
    {
        Console.WriteLine("\n1. Каталог  2. Добавить товар  3. Изменить остаток  4. Заблокировать пользователя  0. Выход");
        switch (Console.ReadLine())
        {
            case "1": ShowCatalog(); break;
            case "2": AddProduct(administrator); break;
            case "3": ChangeStock(administrator); break;
            case "4": BlockUser(administrator); break;
            case "0": return;
            default: Console.WriteLine("Неизвестная команда."); break;
        }
    }
}

void AddProduct(Administrator administrator)
{
    if (!int.TryParse(Ask("Id: "), out var id) || !decimal.TryParse(Ask("Цена: "), out var price) || !int.TryParse(Ask("Остаток: "), out var stock)) return;
    try { inventory.AddNewProduct(new Product(id, Ask("Название: "), price, stock)); store.Save(users, inventory.GetCatalog(), orders); Console.WriteLine("Товар добавлен."); }
    catch (Exception e) { Console.WriteLine(e.Message); }
}
void ChangeStock(Administrator administrator)
{
    if (!int.TryParse(Ask("Id товара: "), out var id) || !int.TryParse(Ask("Изменение (+/-): "), out var amount)) return;
    try { inventory.UpdateQuantity(id, amount); store.Save(users, inventory.GetCatalog(), orders); Console.WriteLine("Остаток изменен."); }
    catch (Exception e) { Console.WriteLine(e.Message); }
}
void BlockUser(Administrator administrator)
{
    var user = users.FirstOrDefault(u => u.Email == Ask("Email пользователя: ").Trim().ToLowerInvariant());
    if (user is null) { Console.WriteLine("Пользователь не найден."); return; }
    administrator.BlockUser(user); store.Save(users, inventory.GetCatalog(), orders); Console.WriteLine("Пользователь заблокирован.");
}
string Ask(string prompt) { Console.Write(prompt); return Console.ReadLine() ?? string.Empty; }
