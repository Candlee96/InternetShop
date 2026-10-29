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

## Git workflow

Разработка разделена на ветки:

- `feature/constantin-auth` — авторизация и иерархия пользователей;
- `feature/deepsik-inventory-payment` — склад, товары и стратегия оплаты;
- `feature/deepsik-orders` — заказы и состояния;
- `feature/chatgpt-cart-catalog` — товары, позиции и корзина.

Журнал веток и слияний находится в `docs/development-log.md`.
