CrossApp
Наскрізний проєкт з крос-платформного програмування.

Предметна область: Замовлення.
Сутності: Customer (клієнт), Product (товар), Order (замовлення), OrderLine (рядок замовлення).
Призначення: оформлення замовлень і підрахунок сум.

Запуск
dotnet build
dotnet run --project src/Cli

Запуск із виводом у форматі JSON (додаткове завдання):
dotnet run --project src/Cli -- --json

Середовище
.NET SDK 10.0, Windows 11 x64

Додаткове завдання (Розміри publish)
Розмір каталогу publish для win-x64: 76,8 МБ

Розмір каталогу publish для linux-x64: 78,7 МБ

## Порівняння режимів публікації (win-x64)
| RID | Режим | Розмір publish | Потрібен runtime |
| :--- | :--- | :--- | :--- |
| win-x64 | self-contained | ~76.8 МБ | ні |
| win-x64 | framework-dependent | ~196 КБ | так (.NET 10) |