using InternetShop.Models;
using InternetShop.Services;

var inventory = new Inventory();
var users = new List<AUser>();
var orders = new List<Order>();
var store = new TextFileStore(Path.Combine(Directory.GetCurrentDirectory(), "InternetShop", "data"));
store.Load(inventory, users, orders);
var orderService = new OrderService();
if (orders.Count > 0) orderService.SetNextOrderId(orders.Max(o => o.OrderId) + 1);
var expiredOrders = orders.Where(order => orderService.CancelIfPaymentExpired(order)).ToList();
if (expiredOrders.Count > 0) store.Save(users, inventory.GetCatalog(), orders);

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
        Console.WriteLine("\n1. Поиск товара  2. Каталог  3. Добавить в корзину  4. Корзина  5. Создать заказ  6. Мои заказы  7. Получить заказ  8. Удалить товар из корзины  9. Оплатить заказ  10. Уведомить о наличии  0. Выход");
        switch (Console.ReadLine())
        {
            case "1": SearchProduct(customer); break;
            case "2": ShowCatalog(); break;
            case "3": AddToCart(customer); break;
            case "4": ShowCart(customer); break;
            case "5": CreateCustomerOrder(customer); break;
            case "6": ShowCustomerOrders(customer); break;
            case "7": ConfirmReceipt(customer); break;
            case "8": RemoveFromCart(customer); break;
            case "9": PayCustomerOrder(customer); break;
            case "10": RequestAvailabilityNotification(customer); break;
            case "0": return;
            default: Console.WriteLine("Неизвестная команда."); break;
        }
    }
}

void RequestAvailabilityNotification(Customer customer)
{
    if (!int.TryParse(Ask("Id товара: "), out var productId))
    {
        Console.WriteLine("Введите число.");
        return;
    }

    try
    {
        inventory.RequestAvailabilityNotification(productId, customer.Id);
        Console.WriteLine("Вы подписались на уведомление о поступлении товара.");
    }
    catch (Exception e) { Console.WriteLine(e.Message); }
}

void ConfirmReceipt(Customer customer)
{
    if (!int.TryParse(Ask("Id заказа: "), out var id)) { Console.WriteLine("Введите число."); return; }
    var order = orders.FirstOrDefault(o => o.OrderId == id && o.CustomerId == customer.Id);
    if (order is null) { Console.WriteLine("Ваш заказ не найден."); return; }
    if (order.CurrentState is not ShippedState)
    {
        Console.WriteLine("Получить можно только отправленный заказ.");
        return;
    }

    order.UpdateStatus();
    store.Save(users, inventory.GetCatalog(), orders);
    Console.WriteLine($"Заказ #{order.OrderId} отмечен как доставленный.");
}

void RemoveFromCart(Customer customer)
{
    if (customer.Cart.IsEmpty) { Console.WriteLine("Корзина пуста."); return; }
    if (!int.TryParse(Ask("Id товара для удаления: "), out var id)) { Console.WriteLine("Введите число."); return; }
    var item = customer.Cart.Items.FirstOrDefault(i => i.Product.Id == id);
    if (item is null) { Console.WriteLine("Такого товара в корзине нет."); return; }
    customer.Cart.RemoveItem(id);
    Console.WriteLine($"{item.Product.Name} удалён из корзины.");
}

void ShowCustomerOrders(Customer customer)
{
    var customerOrders = orders.Where(order => order.CustomerId == customer.Id).OrderByDescending(order => order.CreatedAt).ToList();
    if (customerOrders.Count == 0)
    {
        Console.WriteLine("У вас пока нет заказов.");
        return;
    }

    foreach (var order in customerOrders)
    {
        var tracking = string.IsNullOrWhiteSpace(order.TrackingNumber) ? "трек-номер еще не назначен" : $", трек-номер: {order.TrackingNumber}";
        Console.WriteLine($"Заказ #{order.OrderId}: {order.CurrentState.Name}, {order.CalculateTotal():C}{tracking}");
        foreach (var item in order.OrderItems)
            Console.WriteLine($"  {item.Product.Name}: {item.Quantity} шт.");
    }
}

void SearchProduct(Customer customer)
{
    var query = Ask("Название товара для поиска: ").Trim();
    if (string.IsNullOrWhiteSpace(query)) { Console.WriteLine("Введите название товара."); return; }
    var products = inventory.GetCatalog().Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
    if (products.Count == 0) { Console.WriteLine("Товар недоступен: ничего не найдено."); return; }

    foreach (var product in products) Console.WriteLine(product.GetInfo());
    if (products.All(product => product.StockQuantity == 0))
        Console.WriteLine("Товар найден, но сейчас его нет в наличии. Повторите поиск позже.");
    else
        Console.WriteLine("Товар в наличии. Для добавления в корзину используйте пункт 3.");
}

void CreateCustomerOrder(Customer customer)
{
    try
    {
        var order = orderService.CreateOrder(customer.Cart, customer.Id);
        orders.Add(order);
        store.Save(users, inventory.GetCatalog(), orders);
        Console.WriteLine($"Заказ #{order.OrderId} создан. Оплатите его в течение 15 минут через пункт 9.");
    }
    catch (Exception e) { Console.WriteLine(e.Message); }
}

void PayCustomerOrder(Customer customer)
{
    if (!int.TryParse(Ask("Id заказа: "), out var id)) { Console.WriteLine("Введите число."); return; }
    var order = orders.FirstOrDefault(o => o.OrderId == id && o.CustomerId == customer.Id);
    if (order is null) { Console.WriteLine("Ваш заказ не найден."); return; }
    if (orderService.Pay(order, new CardPayment()))
    {
        store.Save(users, inventory.GetCatalog(), orders);
        Console.WriteLine($"Заказ #{order.OrderId} оплачен.");
    }
    else if (orderService.CancelIfPaymentExpired(order))
    {
        store.Save(users, inventory.GetCatalog(), orders);
        Console.WriteLine("Срок оплаты 15 минут истек. Заказ отменен, товар возвращен на склад.");
    }
    else Console.WriteLine("Заказ уже оплачен, отменен или оплатить его не удалось.");
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

    while (true)
    {
        Console.WriteLine("\nКорзина:");
        foreach (var item in customer.Cart.Items)
            Console.WriteLine($"ID {item.Product.Id}: {item.Product.Name}: {item.Quantity} x {item.PriceAtPurchase:C} (на складе: {item.Product.StockQuantity})");
        Console.WriteLine($"Итого: {customer.Cart.CalculateTotal():C}");

        var input = Ask("Введите ID товара, чтобы изменить количество (0 — назад): " );
        if (input == "0") return;
        if (!int.TryParse(input, out var productId))
        {
            Console.WriteLine("Введите корректный ID товара.");
            continue;
        }

        if (!customer.Cart.Items.Any(item => item.Product.Id == productId))
        {
            Console.WriteLine("Такого товара нет в корзине.");
            continue;
        }

        if (!int.TryParse(Ask("Новое количество (положительное число): "), out var quantity))
        {
            Console.WriteLine("Введите целое число больше нуля.");
            continue;
        }

        try
        {
            customer.Cart.SetQuantity(productId, quantity);
            Console.WriteLine("Количество обновлено.");
        }
        catch (Exception e) { Console.WriteLine(e.Message); }
    }
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
    if (action == 1 && order.CurrentState is PaidState)
    {
        order.AssignTrackingNumber($"TRK-{order.OrderId:000000}");
        order.UpdateStatus();
    }
    else if (action == 2 && order.CurrentState is ShippedState)
    {
        order.UpdateStatus();
    }
    else { Console.WriteLine("Недопустимый переход статуса."); return; }
    store.Save(users, inventory.GetCatalog(), orders); Console.WriteLine($"Статус изменен: {order.CurrentState.Name}.");
}

void AdministratorMenu(Administrator administrator)
{
    while (true)
    {
        Console.WriteLine("\n1. Каталог  2. Добавить товар  3. Изменить остаток  4. Удалить товар  5. Пользователи  0. Выход");
        switch (Console.ReadLine())
        {
            case "1": ShowCatalog(); break;
            case "2": AddProduct(administrator); break;
            case "3": ChangeStock(administrator); break;
            case "4": RemoveProduct(); break;
            case "5": ManageUsers(administrator); break;
            case "0": return;
            default: Console.WriteLine("Неизвестная команда."); break;
        }
    }
}

void AddProduct(Administrator administrator)
{
    var idInput = Ask("Id (или U для автоматического выбора): ").Trim();
    int id;
    if (string.Equals(idInput, "u", StringComparison.OrdinalIgnoreCase))
    {
        var usedIds = inventory.GetCatalog().Select(product => product.Id).ToHashSet();
        id = 1;
        while (usedIds.Contains(id)) id++;
        Console.WriteLine($"Выбран свободный ID: {id}");
    }
    else if (!int.TryParse(idInput, out id) || id <= 0)
    {
        Console.WriteLine("Введите положительный ID или U для автоматического выбора.");
        return;
    }

    if (!decimal.TryParse(Ask("Цена (неотрицательное число): "), out var price) || price < 0)
    {
        Console.WriteLine("Цена должна быть неотрицательным числом.");
        return;
    }
    if (!int.TryParse(Ask("Остаток (0 или больше): "), out var stock) || stock < 0)
    {
        Console.WriteLine("Остаток должен быть целым числом от 0 и выше.");
        return;
    }

    var name = Ask("Название: ").Trim();
    if (string.IsNullOrWhiteSpace(name))
    {
        Console.WriteLine("Название не может быть пустым.");
        return;
    }

    try
    {
        inventory.AddNewProduct(new Product(id, name, price, stock));
        store.Save(users, inventory.GetCatalog(), orders);
        Console.WriteLine($"Товар '{name}' добавлен с ID {id}.");
    }
    catch (Exception e) { Console.WriteLine($"Товар не добавлен: {e.Message}"); }
}
void ChangeStock(Administrator administrator)
{
    if (!int.TryParse(Ask("Id товара: "), out var id) || !int.TryParse(Ask("Изменение (+/-): "), out var amount)) return;
    try { inventory.UpdateQuantity(id, amount); store.Save(users, inventory.GetCatalog(), orders); Console.WriteLine("Остаток изменен."); }
    catch (Exception e) { Console.WriteLine(e.Message); }
}

void RemoveProduct()
{
    if (!int.TryParse(Ask("Id товара для удаления: "), out var id)) { Console.WriteLine("Введите число."); return; }
    if (orders.Any(order => order.OrderItems.Any(item => item.Product.Id == id)))
    {
        Console.WriteLine("Нельзя удалить товар, который уже входит в существующий заказ.");
        return;
    }

    try
    {
        var product = inventory.GetProduct(id);
        inventory.RemoveProduct(id);
        store.Save(users, inventory.GetCatalog(), orders);
        Console.WriteLine($"Товар '{product.Name}' удалён.");
    }
    catch (Exception e) { Console.WriteLine($"Товар не удалён: {e.Message}"); }
}

void ManageUsers(Administrator administrator)
{
    while (true)
    {
        Console.WriteLine("\nСписок пользователей:");
        foreach (var user in users.OrderBy(user => user.Id))
        {
            var name = user is Customer customer ? customer.Name : user is Manager ? "Менеджер" : "Администратор";
            var role = user is Customer ? "Customer" : user is Manager ? "Manager" : "Administrator";
            Console.WriteLine($"ID {user.Id} | {name} | {user.Email} | {role} | {(user.IsBlocked ? "Заблокирован" : "Активен")}");
        }

        if (!int.TryParse(Ask("Введите ID для блокировки/разблокировки (0 — назад): "), out var id) || id == 0) return;
        var selectedUser = users.FirstOrDefault(user => user.Id == id);
        if (selectedUser is null)
        {
            Console.WriteLine("Пользователь с таким ID не найден.");
            continue;
        }

        if (selectedUser.IsBlocked)
        {
            administrator.UnblockUser(selectedUser);
            Console.WriteLine("Пользователь разблокирован.");
        }
        else
        {
            administrator.BlockUser(selectedUser);
            Console.WriteLine("Пользователь заблокирован.");
        }

        store.Save(users, inventory.GetCatalog(), orders);
    }
}
string Ask(string prompt) { Console.Write(prompt); return Console.ReadLine() ?? string.Empty; }
