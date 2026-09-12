# Conference Room Booking API

> 🚧 Каркас проєкту (крок 1 плану: Solution + структура). Повний README з описом фіч, бізнес-правил та Assumptions & Decisions буде написаний на кроці 11.

## Структура

```
ConferenceRoomBooking.sln
src/
├── ConferenceRoomBooking.Api             — контролери, middleware, Program.cs
├── ConferenceRoomBooking.Application     — сервіси, DTO, інтерфейси, валідатори
├── ConferenceRoomBooking.Domain          — сутності, enum'и (без залежностей)
└── ConferenceRoomBooking.Infrastructure  — DbContext, EF Core, міграції
tests/
└── ConferenceRoomBooking.Tests           — unit + integration тести
```

## Запуск (після встановлення .NET 8 SDK)

```bash
dotnet restore
dotnet build
dotnet run --project src/ConferenceRoomBooking.Api
```

Swagger буде доступний на `/swagger` у Development-режимі, health check — на `/health`.
