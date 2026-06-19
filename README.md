# Flight Status Tracker

SkyRoute platform feature — lookup flight status via two stub providers (AeroTrack and QuickFlight), normalised into a unified model.

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- A modern browser (for the UI)

No credentials, API keys, or internet connection required. Runs fully offline.

## Quick Start

### 1. Clone and restore

```bash
git clone <repo-url>
cd flight-status
dotnet restore
```

### 2. Run the API

```bash
cd FlightStatus.Api
dotnet run
```

API starts at **http://localhost:5050**  
Swagger UI: **http://localhost:5050/swagger**

### 3. Open the UI

Open `skyroute-ui/index.html` directly in a browser — no server needed.

## Run Tests

```bash
dotnet test
```

Expected: all tests pass.

## API

```
GET /flights/status?flightNumber={code}&date={yyyy-MM-dd}
```

Returns a unified `FlightStatusResult`. Returns `400` if parameters are missing or date is malformed.

## Test Flights

| Flight | Expected Status | Notes                                    |
|--------|-----------------|------------------------------------------|
| AA100  | OnTime          | AeroTrack wins (updated later)           |
| AA200  | Delayed         | QuickFlight wins (updated later)         |
| AA300  | Cancelled       | AeroTrack only                           |
| AA400  | Diverted        | QuickFlight only                         |
| AA500  | Unknown         | Neither provider has data                |
| AA600  | Delayed         | QuickFlight wins, overrides AeroTrack    |
| BA100  | Diverted        | AeroTrack wins, overrides QuickFlight    |

## Project Structure

```
flight-status/
├── README.md
├── spec.md                    # Data models and contracts (committed first)
├── FlightStatus.Api/          # .NET 9 Minimal API
│   ├── Models/                # FlightStatus enum, ProviderResponse, FlightStatusResult
│   ├── Providers/             # IFlightStatusProvider, AeroTrackProvider, QuickFlightProvider
│   └── Services/              # StatusNormaliser, FlightStatusService
├── FlightStatus.Tests/        # xUnit tests — normalisation and merge rules
├── skyroute-ui/               # Plain HTML/JS frontend
│   └── index.html
├── prompts.md
└── reflection.md
```

## Assumptions

- Date parameter is used to scope stub data; stubs return the same result for any date
- "Within 15 minutes" is inclusive (≤ 15 min = OnTime, > 15 min = Delayed)
- When actual times are present they override the raw status string (except Cancelled/Diverted/Delayed — those are always honoured)
- CORS is open (AllowAnyOrigin) for local development
