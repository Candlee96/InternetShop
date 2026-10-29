# Журнал разработки

Дата формирования: 29.10.2026

## Командная история

Работа разделена на независимые истории, каждая история выполнялась в отдельной ветке и затем вливалась в `main` через merge-коммит.

| Ветка | Ответственный | История | Результат |
|---|---|---|---|
| `feature/constantin-auth` | Константин | Авторизация и пользователи | Выполнено |
| `feature/deepsik-inventory-payment` | Дипсик | Товары, склад, Strategy оплаты | Выполнено |
| `feature/deepsik-orders` | Дипсик | Order и State | Выполнено |
| `feature/chatgpt-cart-catalog` | Чат GPT | Product, OrderItem, ShoppingCart | Выполнено |

## Проверки

- `dotnet build .\InternetShop\InternetShop.csproj` — успешно, 0 ошибок, 0 предупреждений.
- `dotnet run --project .\InternetShop\InternetShop.csproj` — демонстрационный сценарий создания и оплаты заказа.
- История веток проверяется командами `git log --oneline --graph --all` и `git branch -a`.

## Как показать результат

1. Открыть `README.md` и структуру `InternetShop/`.
2. Показать `git log --oneline --graph --all`.
3. Показать merge-коммиты в `main`.
4. Запустить проект командами из README и показать результат в консоли.
