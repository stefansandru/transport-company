# Transport Company Management System

A full-stack application for managing passenger-transport trips, seat reservations, and employees.

---

## 1. Project Architecture

```
transport-company/
 ├─ csharpServer/         # ASP-NET Core gRPC backend
 │   ├─ grpcServer/       # gRPC service implementation
 │   ├─ model/            # Domain entities (shared with client through .proto files)
 │   └─ persistance/      # Pure ADO.NET repositories & DB helpers
 │
 └─ javaClient/           # JavaFX GUI client
     ├─ fxClient/         # UI controllers and DTO helpers
     ├─ services/         # Service and Observer interfaces
     └─ model/            # Domain entities in Java
```

Keeping the backend and frontend in their own respective Gradle and .NET projects optimizes the build process and enforces a clean separation of concerns.

---

## 2. Prerequisites

1. .NET 8 SDK (tested on 8.0.102)
2. Java 17 (or newer) and Gradle 8
3. PostgreSQL 15 (or update the connection string in `csharpServer/persistance/appsettings.json` for SQLite)

---

## 3. Running the Application

### Backend (C#)

```bash
cd csharpServer/grpcServer
dotnet run
```

The server listens on `https://localhost:5001`. Configuration can be modified in `appsettings.json`.

### Frontend (JavaFX)

```bash
cd javaClient/fxClient
./gradlew run
```

For authentication, use one of the test accounts inserted by the database seed script.

---

## 4. Database Setup

```bash
psql -U postgres
CREATE DATABASE transport_company;
\c transport_company
\i sql/schema.sql          -- Tables
\i sql/seed-data.sql       -- Demo data (employees, trips, etc.)
```

Note: You can switch to SQLite by swapping the connection string in the backend configuration.

---

## Test Accounts

| Username | Password |
|----------|----------|
| ana      | ana      |
| adi      | adi      |

Use these credentials to log in and test the application features.

