# PowerGrid Device Monitor

A full-stack web application for monitoring simulated electrical protection devices and relay telemetry.

The project demonstrates full-stack software engineering using a **React + TypeScript frontend**, an **ASP.NET Core C# REST API**, **Entity Framework Core**, **SQLite persistence**, and **NUnit automated testing**.

> **Note:** This project uses simulated protection-device telemetry for software engineering and educational purposes. It is not connected to real electrical protection hardware or a production power system.

---

## Overview

PowerGrid Device Monitor provides an operations-style dashboard for viewing simulated protection relays.

Each device reports electrical and operational information including:

- Voltage
- Current
- Temperature
- Operating status
- Alarm acknowledgement state

Operators can view monitored devices, identify active alarms, acknowledge alarms, and register additional simulated relays.

The application separates the user interface, API, business logic, and data-access layers to demonstrate a maintainable full-stack architecture.

---

## Features

- Monitor multiple simulated protection relays
- Display voltage, current, and temperature measurements
- Display `NORMAL` and `ALARM` operating states
- Highlight high-temperature measurements
- Calculate and display the number of active alarms
- Acknowledge active device alarms
- Prevent acknowledgement of devices without an active alarm
- Add new protection relays through the web interface
- Validate device information through the backend API
- Persist device information using SQLite
- Retrieve device information through REST API endpoints
- Seed initial device data when the database is empty
- Automated backend testing with NUnit
- Responsive operations-style monitoring dashboard

---

## Technology Stack

### Frontend

- React
- TypeScript
- Vite
- HTML
- CSS
- Fetch API

### Backend

- C#
- ASP.NET Core
- Minimal REST API
- Dependency Injection
- Asynchronous programming with `async` / `await`

### Data

- Entity Framework Core
- SQLite
- EF Core Migrations

### Testing

- NUnit
- Microsoft.NET.Test.Sdk
- SQLite in-memory databases

### Development Tools

- Git
- GitHub
- .NET 10
- Node.js
- npm
- Visual Studio Code

---

## Architecture

```text
┌──────────────────────────────┐
│     React + TypeScript       │
│          Frontend            │
└──────────────┬───────────────┘
               │
               │ HTTP / JSON
               ▼
┌──────────────────────────────┐
│       ASP.NET Core API       │
│          Program.cs          │
└──────────────┬───────────────┘
               │
               │ Dependency Injection
               ▼
┌──────────────────────────────┐
│        DeviceService         │
│       Business Logic         │
└──────────────┬───────────────┘
               │
               │ Entity Framework Core
               ▼
┌──────────────────────────────┐
│     PowerGridDbContext       │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│       SQLite Database        │
└──────────────────────────────┘
```

The React frontend communicates with the ASP.NET Core backend through HTTP requests using JSON.

The API uses dependency injection to provide `DeviceService`.

`DeviceService` contains device operations and communicates with `PowerGridDbContext`.

Entity Framework Core maps C# objects to the SQLite database and handles persistent storage.

---

## Device Model

Protection devices are represented using the C# `Device` model.

```csharp
public class Device
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public double Voltage { get; set; }

    public double Current { get; set; }

    public double Temperature { get; set; }

    public string Status { get; set; } = "NORMAL";

    public bool AlarmAcknowledged { get; set; }
}
```

An example device returned by the API:

```json
{
  "id": 3,
  "name": "Relay-103",
  "voltage": 126.2,
  "current": 29.4,
  "temperature": 71.0,
  "status": "ALARM",
  "alarmAcknowledged": false
}
```

---

## REST API

The ASP.NET Core backend provides REST endpoints for interacting with protection devices.

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/devices` | Retrieve all monitored devices |
| `GET` | `/api/devices/{id}` | Retrieve a specific device |
| `POST` | `/api/devices` | Register a new protection device |
| `POST` | `/api/devices/{id}/acknowledge` | Acknowledge an active device alarm |

---

## Example API Request

Retrieve all devices:

```http
GET /api/devices
```

Example response:

```json
[
  {
    "id": 1,
    "name": "Relay-101",
    "voltage": 120.4,
    "current": 12.1,
    "temperature": 41.0,
    "status": "NORMAL",
    "alarmAcknowledged": false
  },
  {
    "id": 3,
    "name": "Relay-103",
    "voltage": 126.2,
    "current": 29.4,
    "temperature": 71.0,
    "status": "ALARM",
    "alarmAcknowledged": false
  }
]
```

---

## Alarm Acknowledgement

The application contains a business rule for alarm acknowledgement.

Only devices with:

```text
Status = ALARM
```

can be acknowledged.

When an active alarm is acknowledged:

```text
AlarmAcknowledged = true
```

A device with:

```text
Status = NORMAL
```

cannot be acknowledged as an active alarm.

This behavior is also verified through automated NUnit testing.

---

## Adding Protection Relays

The dashboard allows users to register additional simulated protection devices.

The form accepts:

- Device name
- Voltage
- Current
- Temperature
- Status

The frontend sends the information to:

```http
POST /api/devices
```

The backend validates the request before saving the device.

Examples of backend validation include:

- Device name cannot be empty
- Electrical measurements cannot be negative
- Status must be `NORMAL` or `ALARM`

Entity Framework Core then stores valid devices in SQLite.

---

## Database

The application uses **SQLite** for local persistent storage and **Entity Framework Core** as the object-relational mapper.

Database access is handled through:

```text
PowerGridDbContext
```

The database schema is maintained through EF Core migrations.

The initial migration is stored in:

```text
PowerGrid.Api/Migrations/
```

### Create the Database

From the backend directory:

```bash
cd PowerGrid.Api
dotnet ef database update
```

This creates the local SQLite database from the committed migration.

The generated:

```text
powergrid.db
```

file is intentionally excluded from Git.

This keeps machine-specific runtime data out of source control while allowing another developer to recreate the database from the migration.

---

## Initial Device Data

When the API starts, it checks whether the database contains any devices.

If the database is empty, the application creates initial simulated protection relays such as:

```text
Relay-101
Relay-102
Relay-103
```

This provides sample telemetry when the application is started for the first time.

---

## Automated Testing

Backend behavior is tested using **NUnit**.

The project currently contains three automated service tests.

### Tests

```text
AddDeviceAsync_SavesDeviceToDatabase
AcknowledgeAlarmAsync_UpdatesAlarmInDatabase
AcknowledgeAlarmAsync_NormalDevice_ReturnsFalse
```

These verify that:

1. A new protection device is successfully persisted.
2. An active alarm can be acknowledged and the database is updated.
3. A `NORMAL` device cannot be acknowledged as an active alarm.

### Run Tests

From the project root:

```bash
dotnet test PowerGrid.Api.Tests
```

Or:

```bash
cd PowerGrid.Api.Tests
dotnet test
```

A successful test run should report:

```text
Total tests: 3
Failed: 0
Succeeded: 3
```

### Test Database Isolation

Tests use temporary SQLite in-memory databases:

```text
Data Source=:memory:
```

This allows the service and Entity Framework Core database behavior to be tested without modifying the application's development database.

---

## Running the Project

### Prerequisites

Install:

- .NET 10 SDK
- Node.js
- npm
- Entity Framework Core CLI tools

Verify .NET:

```bash
dotnet --version
```

Verify Node.js:

```bash
node --version
```

Verify npm:

```bash
npm --version
```

---

## Run the Backend

Clone the repository and enter the project:

```bash
git clone <repository-url>
cd powergrid-device-monitor
```

Restore the backend dependencies:

```bash
cd PowerGrid.Api
dotnet restore
```

Create/update the SQLite database:

```bash
dotnet ef database update
```

Start the API:

```bash
dotnet run
```

The development API runs at:

```text
http://localhost:5122
```

The device endpoint can be accessed at:

```text
http://localhost:5122/api/devices
```

---

## Run the Frontend

Keep the backend running and open another terminal.

From the project root:

```bash
cd powergrid-client
npm install
npm run dev
```

The frontend normally runs at:

```text
http://localhost:5173
```

Open that address in a browser to access the PowerGrid Device Monitor dashboard.

---

## Project Structure

```text
powergrid-device-monitor/
│
├── PowerGrid.Api/
│   │
│   ├── Data/
│   │   └── PowerGridDbContext.cs
│   │
│   ├── Migrations/
│   │   ├── InitialCreate.cs
│   │   └── PowerGridDbContextModelSnapshot.cs
│   │
│   ├── Models/
│   │   └── Device.cs
│   │
│   ├── Services/
│   │   └── DeviceService.cs
│   │
│   ├── Program.cs
│   ├── PowerGrid.Api.csproj
│   └── appsettings.json
│
├── PowerGrid.Api.Tests/
│   ├── DeviceServiceTests.cs
│   └── PowerGrid.Api.Tests.csproj
│
├── powergrid-client/
│   │
│   ├── src/
│   │   ├── App.tsx
│   │   ├── App.css
│   │   ├── index.css
│   │   └── main.tsx
│   │
│   ├── package.json
│   └── vite.config.ts
│
├── .gitignore
└── README.md
```

---

## Engineering Concepts Demonstrated

### Object-Oriented Programming

The backend models protection devices as C# objects using the `Device` class.

### Service Layer

`DeviceService` separates device operations from HTTP endpoint definitions.

### Dependency Injection

ASP.NET Core dependency injection provides the database context and device service to the application.

### REST API Design

The backend exposes HTTP endpoints that exchange JSON data with the frontend.

### Asynchronous Programming

Database operations use C# `async` and `await` to avoid blocking application execution.

### Frontend State Management

React state tracks:

- Device data
- Loading state
- API errors
- Add-device form state
- New-device information

### Type Safety

TypeScript defines the structure of protection-device information using a `Device` interface.

### Data Persistence

Entity Framework Core maps C# objects to SQLite records.

### Database Migrations

EF Core migrations provide a repeatable way to create and evolve the database schema.

### Automated Testing

NUnit verifies service behavior using isolated SQLite in-memory databases.

### Version Control

Git tracks application source code, database migrations, tests, frontend code, and documentation while excluding generated dependencies, build artifacts, and local database files.

---

## Security Considerations

This repository is currently a local software engineering demonstration and does **not** implement user authentication or authorization.

A production monitoring application would require additional security controls such as:

- HTTPS
- User authentication
- Role-based authorization
- Least-privilege access
- Audit logging
- Secure secret and configuration management
- Stronger request validation
- Restricted CORS configuration
- Dependency vulnerability monitoring
- Secure database configuration

Security requirements would be especially important before connecting software like this to real operational or power-system infrastructure.

---

## Current Limitations

The project intentionally focuses on full-stack application architecture rather than real protection-system integration.

Current limitations include:

- Telemetry is simulated
- No physical relay communication
- No real-time telemetry stream
- No historical telemetry database
- No authentication or authorization
- No production deployment configuration

---

## Future Improvements

Potential future extensions include:

- Real-time telemetry with SignalR or WebSockets
- Historical voltage, current, and temperature charts
- Alarm and event history
- Authentication
- Role-based operator permissions
- Audit logging
- Additional API integration tests
- React frontend testing
- PostgreSQL or SQL Server support
- Containerization with Docker
- Production deployment pipeline
- Hardware communication adapters
- Real protection-device integration through an appropriate controlled interface

---

## Purpose

PowerGrid Device Monitor was built to apply full-stack software engineering concepts in a power-systems context.

The project combines:

```text
C#
ASP.NET Core
Object-Oriented Programming
REST APIs
React
TypeScript
Entity Framework Core
SQLite
NUnit
Git
```

into a single working application with a clear separation between the user interface, backend API, business logic, and persistent data storage.