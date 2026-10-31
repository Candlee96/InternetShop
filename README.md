# InternetShop — лабораторная работа №5

Прототип интернет-магазина на C# и .NET 8. Проект разработан по UML из лабораторной работы №4.

## Возможности

- иерархия пользователей `AUser`, `Customer`, `Manager`, `Administrator`;
- каталог и склад товаров;
- корзина покупателя и создание заказа;
- расчет суммы заказа и фиксация цены товара на момент покупки;
- паттерн `Strategy` для оплаты картой и бонусами;
- паттерн `State` для переходов заказа: «Создан» → «Оплачен» → «Отправлен» → «Доставлен».

## Запуск

Требуется .NET 8 SDK:

```powershell
dotnet build .\InternetShop\InternetShop.csproj
dotnet run --project .\InternetShop\InternetShop.csproj
```

## Файловое хранилище и интерактивная консоль

При запуске приложение загружает данные из `InternetShop/data`. Файлы создаются автоматически при первом запуске:

- `users.txt` — `type|id|name|email|number|passwordHash|blocked`; пароли представлены SHA-256 хешами;
- `products.txt` — `id|name|price|stock`;
- `orders.txt` — `id|createdAtUtc|status|tracking|items|customerId`, позиции заказа имеют формат `productId:quantity:price`.

Текстовые поля кодируются Base64, чтобы разделитель `|` не ломал формат. Если файлов нет, создаются демо-пользователи: `customer@shop.local / customer123`, `manager@shop.local / manager123`, `admin@shop.local / admin123`, а также два товара. Доступные действия зависят от роли вошедшего пользователя. Для проверки без ввода можно передать EOF, например `printf '3\\n0\\n' | dotnet run --project .\\InternetShop\\InternetShop.csproj`.

## Git workflow

Разработка разделена на ветки:

- `feature/constantin-auth` — авторизация и иерархия пользователей;
- `feature/deepsik-inventory-payment` — склад, товары и стратегия оплаты;
- `feature/deepsik-orders` — заказы и состояния;
- `feature/chatgpt-cart-catalog` — товары, позиции и корзина.

Журнал веток и слияний находится в `docs/development-log.md`.
