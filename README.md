# Conference Room Booking API

REST API для компанії, що здає в оренду конференц-зали: пошук вільних залів,
бронювання з автоматичним розрахунком вартості (залежно від часу доби та
обраних додаткових послуг), і базове управління залами й послугами.

## Зміст

- [Features](#features)
- [Technologies](#technologies)
- [Architecture](#architecture)
- [Business Rules](#business-rules)
- [Pricing Rules](#pricing-rules)
- [Assumptions & Decisions](#assumptions--decisions)
- [API Endpoints](#api-endpoints)
- [Database](#database)
- [How to Run](#how-to-run)
- [Testing](#testing)
- [Future Improvements](#future-improvements)

## Features

- CRUD конференц-залів (назва, місткість, базова погодинна ставка) із soft delete
- CRUD додаткових послуг (проєктор, Wi-Fi, звук тощо) із soft delete
- Пошук залів, вільних на заданий інтервал часу, з достатньою місткістю
- Бронювання залу з автоматичним розрахунком вартості:
  тарифні зони доби + вартість обраних послуг
- Скасування бронювання — звільняє інтервал часу для нових бронювань
- Захист від одночасного бронювання одного інтервалу двома паралельними запитами
- Уніфікована обробка помилок у форматі `ProblemDetails` (RFC 7807)
- Повна Swagger/OpenAPI документація з прикладами запитів/відповідей
- Seed початкових даних (3 зали, 3 послуги) — API готове до тестування одразу після старту

## Technologies

| Категорія        | Технологія                                  |
|-------------------|----------------------------------------------|
| Платформа         | .NET 8, ASP.NET Core Web API                 |
| База даних        | PostgreSQL, EF Core 8 (Npgsql)               |
| Валідація         | FluentValidation                             |
| API-документація  | Swagger / OpenAPI (Swashbuckle.AspNetCore)   |
| Тестування        | xUnit, Moq, EF Core InMemory                 |

## Architecture

```
ConferenceRoomBooking.sln
src/
├── ConferenceRoomBooking.Api             — контролери, Swagger, middleware, Program.cs
├── ConferenceRoomBooking.Application     — DTO, валідатори, інтерфейси сервісів, PricingService
├── ConferenceRoomBooking.Domain          — сутності, enum'и, специфікації (без зовнішніх залежностей)
└── ConferenceRoomBooking.Infrastructure  — DbContext, EF Core конфігурації, міграції, реалізації сервісів
tests/
└── ConferenceRoomBooking.Tests           — unit-тести (Application + Infrastructure + Api)
```

Класична 4-шарова структура, залежності спрямовані всередину (Api →
Application/Infrastructure → Domain). Свідомо **без** абстракції
`IRepository<T>` / Unit of Work: `AppDbContext` в EF Core вже й так є
реалізацією обох цих патернів, а додатковий шар над ним для проєкту такого
обсягу — абстракція заради абстракції, яка ускладнює код, не додаючи
тестованості чи гнучкості (сервіси й так мокаються/тестуються напряму
через `DbContextOptionsBuilder.UseInMemoryDatabase`).

## Business Rules

- **ConferenceRoom**: `Name`, `Capacity`, `BaseHourlyRate`, `IsActive`.
  "Видалення" — це soft delete (`IsActive = false`), фізичного видалення немає.
- **Service**: `Name`, `Price`, `IsActive`. Так само soft delete замість видалення.
- **Booking**: посилається на зал, має `StartTime`/`EndTime`, розрахований
  `TotalPrice`, `Status` (`Confirmed` / `Cancelled`) і список послуг.
- **Конфлікт бронювань**: два бронювання одного залу конфліктують, якщо їхні
  інтервали перетинаються (`StartTime < чужого EndTime AND EndTime > чужого StartTime`).
  Скасовані бронювання (`Status == Cancelled`) в конфлікт не враховуються.
- **Робочі години залу**: бронювання дозволене лише в межах 06:00–23:00 і
  лише в межах одного календарного дня (не може перетинати північ).
- **Ціна послуги фіксується в момент бронювання** (`BookingService.Price`) —
  якщо пізніше ціна послуги в каталозі зміниться, вже створені бронювання
  зберігають первісну вартість.
- **Скасування бронювання** (`PATCH /api/bookings/{id}/cancel`) переводить
  його в `Status = Cancelled` — після цього воно більше не враховується як
  конфлікт для нових бронювань того самого залу на той самий інтервал.
  Повторне скасування вже скасованого бронювання повертає `409 Conflict`.

## Pricing Rules

Вартість оренди залу рахується **погодинно/по сегментах** для кожної
тарифної зони, яку перетинає інтервал бронювання — а не множенням середнього
коефіцієнта на всю тривалість. Послуги додаються один раз незалежно від
тривалості бронювання.

| Інтервал      | Назва зони | Коефіцієнт |
|----------------|------------|------------|
| 06:00–09:00    | Morning    | ×0.90 (−10%) |
| 09:00–12:00    | Standard   | ×1.00 |
| 12:00–14:00    | Peak       | ×1.15 (+15%) |
| 14:00–18:00    | Standard   | ×1.00 |
| 18:00–23:00    | Evening    | ×0.80 (−20%) |

**Приклад** (Зал А, база 2000 грн/год, бронювання 11:00–15:00, без послуг):
- 11:00–12:00 (Standard, 1 год) → 2000 × 1.00 = 2000
- 12:00–14:00 (Peak, 2 год) → 2000 × 1.15 × 2 = 4600
- 14:00–15:00 (Standard, 1 год) → 2000 × 1.00 = 2000
- **Разом: 8600 грн**

## Assumptions & Decisions

Явно зафіксовані рішення, прийняті під час реалізації:

1. **Дотичні межі інтервалів — не конфлікт.** `10:00–14:00` і `14:00–16:00`
   можна забронювати одночасно; конфлікт визначено як строге перетинання
   (`Overlapping`, `ConferenceRoomBooking.Domain.Specifications`).
2. **Пріоритет тарифних зон при перетині.** Peak (12:00–14:00, +15%) не
   "накладається" на Standard як окреме правило пріоритету — натомість
   Standard свідомо розбитий на дві зони (09:00–12:00 і 14:00–18:00), щоб
   Peak залишався єдиною зоною для свого інтервалу без перетинів.
3. **Розрахунок вартості — по сегментах, а не за середнім коефіцієнтом.**
   `PricingService` розбиває інтервал бронювання на межі тарифних зон і
   рахує кожен сегмент окремо — тому часткові години (наприклад,
   бронювання, що закінчується о 12:30) також коректно пропорційно враховані.
4. **Concurrency вирішено, але свідомо мінімально.** Перевірка конфлікту і
   вставка бронювання виконуються в одній транзакції з
   `IsolationLevel.Serializable` — якщо два одночасні запити намагаються
   забронювати той самий інтервал, PostgreSQL відкотить один з них
   (`serialization_failure`), і клієнт отримає `409 Conflict`. Транзакція
   вмикається лише для реляційного провайдера — в unit-тестах, що
   використовують EF Core InMemory, вона просто вимкнена (InMemory не
   підтримує `BeginTransactionAsync(IsolationLevel)`, а сам провайдер
   однопотоковий, тому race condition там не виникає).
5. **Один часовий пояс, `timestamp without time zone`.** Сервіс вважається
   односайтовим (одна компанія, один часовий пояс) — реальна UTC-конвертація
   не потрібна. Тому `StartTime`/`EndTime`/`CreatedAt` зберігаються як
   PostgreSQL `timestamp without time zone`, а вхідні `DateTime` явно
   приводяться до `DateTimeKind.Unspecified` (`DateTimeExtensions.AsUnspecifiedKind`)
   — Npgsql 8+ інакше кидає виняток, якщо `Kind` не відповідає типу колонки.
6. **Repository-абстракція свідомо не додана.** `AppDbContext` використовується
   напряму в сервісах Application/Infrastructure — див. розділ Architecture.
7. **Автентифікація не реалізована.** Завданням вона прямо не вимагалась;
   витрачати на неї час замість основної функціональності визнано недоцільним.
8. **Що з "було б добре" свідомо не зроблено** (через обмеження часу, не
   через забудькуватість) — див. розділ [Future Improvements](#future-improvements).

## API Endpoints

### Conference Rooms

| Метод | Маршрут | Опис |
|-------|---------|------|
| GET    | `/api/conference-rooms`  | Список усіх залів (включно з неактивними) |
| GET    | `/api/conference-rooms/available?startTime=&endTime=&capacity=` | Пошук вільних залів на інтервал |
| GET    | `/api/conference-rooms/{id}` | Зал за Id |
| POST   | `/api/conference-rooms` | Створити зал |
| PUT    | `/api/conference-rooms/{id}` | Оновити зал |
| DELETE | `/api/conference-rooms/{id}` | Деактивувати зал (soft delete) |

### Services

| Метод | Маршрут | Опис |
|-------|---------|------|
| GET    | `/api/services` | Список усіх послуг (включно з неактивними) |
| GET    | `/api/services/{id}` | Послуга за Id |
| POST   | `/api/services` | Створити послугу |
| PUT    | `/api/services/{id}` | Оновити послугу |
| DELETE | `/api/services/{id}` | Деактивувати послугу (soft delete) |

### Bookings

| Метод | Маршрут | Опис |
|-------|---------|------|
| GET   | `/api/bookings` | Список усіх бронювань (найновіші спочатку) |
| GET   | `/api/bookings/{id}` | Бронювання за Id |
| POST  | `/api/bookings` | Створити бронювання (з розрахунком вартості) |
| PATCH | `/api/bookings/{id}/cancel` | Скасувати бронювання |

Повний опис параметрів, тіл запитів/відповідей і кодів помилок — у Swagger UI
(`/swagger` при запуску в Development).

## Database

```
ConferenceRoom 1───* Booking 1───* BookingService *───1 Service
```

| Таблиця           | Ключові поля | Примітки |
|--------------------|--------------|----------|
| `ConferenceRooms`  | `Id`, `Name`, `Capacity`, `BaseHourlyRate`, `IsActive` | Індекс `(IsActive, Capacity)` — під пошук доступності |
| `Services`         | `Id`, `Name`, `Price`, `IsActive` | |
| `Bookings`         | `Id`, `ConferenceRoomId`, `StartTime`, `EndTime`, `TotalPrice`, `Status`, `CreatedAt` | Індекс `(ConferenceRoomId, StartTime, EndTime)`; `Status` зберігається як текст |
| `BookingServices`  | `BookingId` + `ServiceId` (складений PK), `Price` | `Price` — знімок ціни послуги на момент бронювання |

Міграції лежать у `src/ConferenceRoomBooking.Infrastructure/Migrations`
(`InitialCreate`). Застосовуються командою `dotnet ef database update`
(див. How to Run).

## How to Run

Потрібно: .NET 8 SDK, PostgreSQL (локально або в контейнері).

```bash
# 1. Відновити залежності
dotnet restore

# 2. Прописати рядок підключення до своєї бази у
#    src/ConferenceRoomBooking.Api/appsettings.json (ConnectionStrings:DefaultConnection)
#    Локально краще тримати реальні креденшли поза git — через
#    `dotnet user-secrets` або змінну середовища
#    ConnectionStrings__DefaultConnection.

# 3. Застосувати міграції
dotnet ef database update --project src/ConferenceRoomBooking.Infrastructure --startup-project src/ConferenceRoomBooking.Api

# 4. Запустити API
dotnet run --project src/ConferenceRoomBooking.Api
```

При першому старті в Development-режимі база автоматично наповниться
початковими даними (3 зали, 3 послуги — `DbInitializer`). Swagger UI
доступний на `/health` — простий health check, `/` перенаправляє на Swagger.

## Testing

```bash
dotnet test
```

Unit-тести (`ConferenceRoomBooking.Tests`), без залежності від реальної БД
(EF Core InMemory):

- **PricingServiceTests** — усі тарифні зони, перетин зон, часткові години,
  валідація `EndTime > StartTime`
- **BookingServiceTests** — конфлікт бронювання, неактивний/неіснуючий зал,
  неіснуюча/неактивна послуга, коректна вартість з послугами, скасування
  (переведення статусу, звільнення інтервалу для нового бронювання,
  конфлікт при повторному скасуванні)
- **ConferenceRoomServiceAvailabilityTests** — фільтрація за місткістю,
  активністю залу, конфліктом за часом
- **BookingQueryExtensionsTests** — коректність визначення "конфлікту" (в
  т.ч. дотичні межі — не конфлікт)
- **DateTimeExtensionsTests** — нормалізація `DateTimeKind`
- **GlobalExceptionHandlerTests** — коректний маппінг винятків на
  HTTP-статуси та формат `ProblemDetails`

## Future Improvements

Свідомо не реалізовано в цій ітерації — через обмеження часу, а не тому, що
про це забули:

- **Integration-тести** через `WebApplicationFactory` (наразі покриття —
  лише на рівні unit-тестів сервісів)
- **CI** (GitHub Actions: `dotnet build` + `dotnet test` на кожен push/PR)
- **Звіти й аналітика**: завантаженість залів (room-utilization), виручка за
  період, популярні послуги/зали
- **Docker** (Dockerfile + docker-compose з PostgreSQL)
- **Пагінація** для списків залів/послуг/бронювань
- **Rate limiting**

Свідомо не реалізовано взагалі (не через брак часу, а як architectural
decision — див. Assumptions & Decisions):
- Repository/Unit-of-Work абстракція над `DbContext`
- Автентифікація/авторизація
