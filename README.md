# CrossApp

Наскрізний проєкт з крос-платформного програмування.

Предметна область: Замовлення. Сутності: Customer (клієнт), Product (товар), Order (замовлення), OrderLine (рядок замовлення). Призначення: оформлення замовлень і підрахунок сум.

## Запуск та збірка

- Збірка: `dotnet build`
- Запуск консольного застосування: `dotnet run --project src/Cli`
- Запуск із виводом у форматі JSON (додаткове завдання): `dotnet run --project src/Cli -- --json`
- Середовище: `.NET SDK 10.0`, Windows 11 x64

## Додаткове завдання (Розміри publish)

- Розмір каталогу `publish` для `win-x64`: `76,8 МБ`
- Розмір каталогу `publish` для `linux-x64`: `78,7 МБ`

| RID | Режим | Розмір publish | Потрібен runtime |
| :--- | :--- | :--- | :--- |
| `win-x64` | `self-contained` | `~76.8 МБ` | ні |
| `win-x64` | `framework-dependent` | `~196 КБ` | так (.NET 10) |

## Лабораторна робота 4: Доменна модель (Сутності, інваріанти, інкапсуляція)

1. Обов'язковий ключ (SKU) та реквізити: Ідентифікатор, SKU та назва товару не можуть бути порожніми або складатися лише з пробілів (`ArgumentException`).
2. Коректність кількості: Початковий залишок товару при створенні та параметри приходу/видачі не можуть бути від'ємними чи нульовими (`ArgumentOutOfRangeException`).
3. Контроль стану при видачі: Операція видачі товару неможлива, якщо запитувана кількість перевищує поточний залишок на складі (`InvalidOperationException`).

## Лабораторна робота 7: LINQ to Objects

- Порівняння синтаксису (Методний vs Query Syntax): Query Syntax (`from ... group ... into`) більш декларативний і нагадує SQL, а для складних ланцюжків із `ThenBy` і проєкціями гнучкішим та компактнішим є Method Syntax через лямбди.
- Відкладене виконання: Запити (`Where`, `Select`) виконуються під час ітерації або матеріалізації (`ToList`, `Count()`), тому зміна джерела між оголошенням і обчисленням впливає на результат.

## Лабораторна робота 8: async/await, скасування, доменні винятки, unit-тести Core

- Запуск усіх модульних тестів: `dotnet test`
- Звіт про покриття коду: `dotnet test --collect:"XPlat Code Coverage"`
- Запуск консольного застосування з асинхронним імпортом: `dotnet run --project src/Cli`
- Перевірка кооперативного скасування: під час імпорту натиснути `Ctrl + C` для переривання операції.

## Лабораторна робота 9: Шляхи, JSON, конфігурація, Secrets

- `appsettings.json` — базові налаштування (шляхи до сховища, назва файлу, логування).
- `appsettings.Development.json` — налаштування для середовища розробки.

| Ключ | Опис |
| :--- | :--- |
| `EnvironmentName` | Поточне активне середовище |
| `Storage:DataPath` | Шлях до каталогу з даними |
| `Storage:FileName` | Назва файлу каталогу товарів |
| `Logging:MinimumLevel` | Поріг виводу логів |
| `Integration:ApiKey` | Секрет інтеграції (читається з user-secrets) |

- PowerShell: `$env:CROSSAPP_DATA_PATH="C:\data"; dotnet run --project src/Cli`
- Bash: `CROSSAPP_DATA_PATH=/tmp/data dotnet run --project src/Cli`

## Лабораторна робота 10: Minimal API, CRUD, OpenAPI (Scalar)

- Запуск веб-сервісу API: `dotnet run --project src/Api --urls http://localhost:5080`
- Документація OpenAPI: `http://localhost:5080/openapi/v1.json`
- Інтерфейс Scalar UI: `http://localhost:5080/scalar/v1`

## Лабораторна робота 11: EF Core + SQLite

- Рядок підключення у файлі `appsettings.json`: `"ConnectionStrings": { "AppDb": "Data Source=data/app.db" }`
- Створити міграцію: `dotnet ef migrations add <MigrationName> --project src/Infrastructure --startup-project src/Api`
- Застосувати міграції: `dotnet ef database update --project src/Infrastructure --startup-project src/Api`

## Лабораторна робота 12: Blazor-клієнт до власного API

### Режим виконання та архітектура
- Створено проєкт `src/Web` за шаблоном Blazor Web App (`.NET 10`) у режимі **InteractiveServer** (`@rendermode InteractiveServer`)[cite: 2, 3].
- Код компонентів виконується на сервері процесу `src/Web`, оновлення інтерфейсу передаються у браузер через SignalR, а звернення до API через типізований `ProductsApiClient` (`HttpClient`) виконуються зі сторони сервера `Web` (`server-to-server`)[cite: 2, 3].
- `src/Web` **не** має посилання на `Infrastructure` і **не** містить рядка підключення до бази даних; базові адреси задаються через конфігурацію[cite: 2, 3, 5].
- Політику CORS (`Cors:WebOrigin`) додано в `src/Api` з `app.UseCors("web")` до `app.MapProducts()`: вона необхідна для браузерних клієнтів з іншим origin (наприклад, Blazor WebAssembly), тоді як при викликах з `InteractiveServer` запит до API йде із сервера `Web`[cite: 2, 3, 4].

### Ключі конфігурації
- `src/Web/appsettings.json`: `Api:BaseUrl` = `"http://localhost:5080"`[cite: 3, 4]
- `src/Api/appsettings.json`: `Cors:WebOrigin` = `"http://localhost:5090"`[cite: 3, 4]

### Запуск двох процесів
- Термінал 1 (API на порту `5080`):
  `dotnet run --project src/Api --urls http://localhost:5080`[cite: 3, 6]
- Термінал 2 (Web на порту `5090`):
  `dotnet run --project src/Web --urls http://localhost:5090`[cite: 3, 6]

### Додаткові завдання
- **Видалення з підтвердженням:** реалізовано підтвердження перед викликом `DELETE /api/products/{id}` і видалення елемента з локального списку без повного перезавантаження сторінки[cite: 10].
- **Пошук і сортування на боці API:** параметри `search`, `sortBy` та `sortDir` обробляються в `GET /api/products` на стороні сервера[cite: 10].
- **Спільний проєкт `src/Contracts`:** винесено контракти (`ProductDto`, `SaveProductRequest`, `PagedProductsResult`, `ApiResult`, `ValidationProblemResponse`) у `src/Contracts` для `Api` та `Web`[cite: 10].
  - *Плюси зв'язування:* єдине джерело правди для структури DTO і полів, менше дублювання та помилок у назвах полів.
  - *Мінуси зв'язування:* сильніша зв'язність (`coupling`) між сервером і клієнтом, ризик зламати обидва проєкти при зміні контракту, якщо API та Web розгортаються незалежно.
- **Скасування запитів:** використовується `CancellationTokenSource` при зміні параметрів запиту та в `Dispose()` компонентів при швидкому переході між сторінками[cite: 10].