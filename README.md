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

|RID|Режим|Розмір publish|Потрібен runtime|
|-|-|-|-|
|win-x64|self-contained|\~76.8 МБ|ні|
|win-x64|framework-dependent|\~196 КБ|так (.NET 10)|

Лабораторна робота 4: Доменна модель (Сутності, інваріанти, інкапсуляція)



Перелік реалізованих інваріантів:

1\. Обов'язковий ключ (SKU) та реквізити: Ідентифікатор, SKU та назва товару не можуть бути порожніми або складатися лише з пробілів (`ArgumentException`)\[cite: 2, 4, 5].

2\. Коректність кількості: Початковий залишок товару при створенні та параметри приходу/видачі не можуть бути від'ємними чи нульовими (`ArgumentOutOfRangeException`)\[cite: 2, 4, 5].

3\. Контроль стану при видачі: Операція видачі товару неможлива, якщо запитувана кількість перевищує поточний залишок на складові (`InvalidOperationException`)\[cite: 2, 4, 5].

# Лабораторна робота 7: LINQ to Objects

## Порівняння синтаксису (Методний vs Query Syntax)
Query Syntax (`from ... group ... into`) є більш декларативним і нагадує SQL, проте для складних ланцюжків із `ThenBy` та кількома проєкціями методний синтаксис (Method Syntax через лямбди) є більш гнучким і компактним у C#.

## Відкладене виконання
Запити (Where, Select) виконуються не в момент оголошення, а під час ітерації чи матеріалізації (ToList, Count()). Зміна джерела між оголошенням і викликом Count() впливає на результат.

## Лабораторна робота 8: async/await, скасування, доменні винятки, unit-тести Core



### Як запустити тести та перевірити роботу:

1. Запуск усіх модульних тестів:

   ```bash

   dotnet test

   ```

2. Звіт про покриття коду:

   ```bash

   dotnet test --collect:"XPlat Code Coverage"

   ```

3. Запуск консольного застосування з асинхронним імпортом:

   ```bash

   dotnet run --project src/Cli

   ```

4. Перевірка кооперативного скасування:

   Під час виконання імпорту натисніть **`Ctrl + C`** для переривання операції. Консоль виведе: `Імпорт скасовано.`

## Лабораторна робота 9: Шляхи, JSON, конфігурація, Secrets

### Профілі конфігурації
- **appsettings.json** — базові налаштування (шляхи до сховища, назва файлу, логування)[cite: 1, 2].
- **appsettings.Development.json** — налаштування для середовища розробки[cite: 1, 2].

### Таблиця ключів конфігурації
| Ключ | Опис |
| :--- | :--- |
| `EnvironmentName` | Поточне активне середовище[cite: 4] |
| `Storage:DataPath` | Шлях до каталогу з даними[cite: 2, 4] |
| `Storage:FileName` | Назва файлу каталогу товарів[cite: 2, 4] |
| `Logging:MinimumLevel` | Поріг виводу логів[cite: 2] |
| `Integration:ApiKey` | Секрет інтеграції (читається з user-secrets)[cite: 6] |

### Приклади запуску (зміна шляху через змінні середовища)
- **PowerShell:**
  ```powershell
  $env:CROSSAPP_DATA_PATH="C:\data"; dotnet run --project src/Cli
Bash:

Bash
CROSSAPP_DATA_PATH=/tmp/data dotnet run --project src/Cli

## Лабораторна робота 10: Minimal API, CRUD, OpenAPI (Scalar)

Запуск веб-сервісу API:
```bash
dotnet run --project src/Api --urls http://localhost:5080
Документація OpenAPI: http://localhost:5080/openapi/v1.json

Інтерфейс Scalar UI: http://localhost:5080/scalar/v1[cite: 1, 4]

Приклад запиту через curl:
Bash
curl -i -X POST http://localhost:5080/api/products \
  -H "Content-Type: application/json" \
  -d '{"sku":"A-100","name":"Цемент М400","unit":"мішок","quantity":40}'

---

### Звіт до лабораторної роботи №10

1. **Таблиця маршрутів:**

| Метод | Шлях | Тіло запиту (Request Body) | Коди відповідей (Response Status Codes) |
| :--- | :--- | :--- | :--- |
| **GET** | `/api/products` | — | `200 OK` |
| **GET** | `/api/products/{id}` | — | `200 OK`, `404 Not Found` |
| **GET** | `/api/products/low-stock?threshold=5` | — | `200 OK` |
| **POST** | `/api/products` | `SaveProductRequest` (JSON) | `201 Created`, `400 Bad Request`[cite: 2] |
| **PUT** | `/api/products/{id}` | `SaveProductRequest` (JSON) | `200 OK`, `404 Not Found`, `400 Bad Request` |
| **DELETE** | `/api/products/{id}` | — | `204 No Content`, `404 Not Found`[cite: 2] |

2. **Чому DTO окремо від доменної сутності:**
   DTO (Data Transfer Object) повністю відокремлює зовнішній контракт HTTP-апі від внутрішньої доменної моделі. Це дозволяє захистити інваріанти сутностей, приховати приватні поля чи внутрішні зміни бази/структури даних і забезпечити стабільний JSON-контракт для клієнтів (наприклад, майбутнього Blazor-додатка)[cite: 1, 2].

3. **Час життя сервісів у DI (Dependency Injection):**
   - **Singleton:** Створюється один екземпляр сервісу на весь час роботи застосунку[cite: 2]. Ми обрали його для `InMemoryCatalogStore`, щоб збережений стан пам'яті (список товарів) не втрачався між різними HTTP-запитами.
   - **Scoped:** Створюється один екземпляр на кожен окремий HTTP-запит[cite: 2].
   - **Transient:** Новий екземпляр створюється щоразу при зверненні до контейнера[cite: 2].
