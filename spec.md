# Flight Status Tracker — Specification

## Data Models

### ProviderResponse (internal)
Returned by each stub provider before normalisation.

| Field               | Type      | AeroTrack | QuickFlight | Notes                          |
|---------------------|-----------|-----------|-------------|--------------------------------|
| FlightNumber        | string    | ✓         | ✓           |                                |
| Date                | DateOnly  | ✓         | ✓           |                                |
| RawStatus           | string    | ✓         | ✓           | Provider-specific vocabulary   |
| ScheduledDeparture  | DateTime? | ✓         | ✓           |                                |
| ActualDeparture     | DateTime? | ✓         | ✗           |                                |
| ScheduledArrival    | DateTime? | ✓         | ✓           |                                |
| ActualArrival       | DateTime? | ✓         | ✗           |                                |
| Terminal            | string?   | ✓         | ✗           | AeroTrack only                 |
| Gate                | string?   | ✓         | ✗           | AeroTrack only                 |
| DelayReason         | string?   | ✓         | ✗           | AeroTrack only                 |
| LastUpdatedUtc      | DateTime  | ✓         | ✓           | Used for merge precedence      |
| ProviderName        | string    | ✓         | ✓           |                                |

### FlightStatusResult (API response)

```json
{
  "flightNumber":       "AA200",
  "date":               "2024-06-01",
  "status":             "Delayed",
  "scheduledDeparture": "2024-06-01T09:00:00Z",
  "actualDeparture":    "2024-06-01T09:50:00Z",
  "scheduledArrival":   "2024-06-01T12:30:00Z",
  "actualArrival":      "2024-06-01T13:20:00Z",
  "terminal":           "A",
  "gate":               "A7",
  "delayReason":        "Air traffic control hold",
  "message":            null,
  "lastUpdatedUtc":     "2024-06-01T09:00:00Z",
  "source":             "QuickFlight"
}
```

### Unified Status Enum

| Value     | Rule                                                     |
|-----------|----------------------------------------------------------|
| OnTime    | Departure or arrival within 15 minutes of schedule       |
| Delayed   | Departure or arrival pushed beyond 15 minutes            |
| Cancelled | Flight will not operate                                  |
| Diverted  | Flight landed at a different airport                     |
| Unknown   | No usable status returned by either provider             |

## Interface Contracts

### IFlightStatusProvider

```csharp
public interface IFlightStatusProvider
{
    string Name { get; }
    Task<ProviderResponse?> GetStatusAsync(string flightNumber, DateOnly date);
}
```

Returns `null` when the provider has no data for the given flight/date.

### Status Normalisation Rules

| Raw value (case-insensitive, ignoring `-_`) | Normalised    |
|---------------------------------------------|---------------|
| CANCELLED, CANCELED                         | Cancelled     |
| DIVERTED, REROUTED                          | Diverted      |
| DELAYED, LATE                               | Delayed       |
| ONTIME, SCHEDULED + times within 15 min    | OnTime        |
| ONTIME, SCHEDULED (no times)               | OnTime        |
| SCHEDULED + times beyond 15 min            | Delayed       |
| Anything else                               | Unknown       |

When actual departure or arrival times are present, the 15-minute threshold is calculated as:
`max(|actualDep - scheduledDep|, |actualArr - scheduledArr|) <= 15 minutes`

### Merge Rules

1. Query both providers concurrently
2. Filter out null responses
3. If none respond → return Unknown with message "No data available from any provider."
4. Pick the response with the latest `LastUpdatedUtc`
5. Normalise its `RawStatus` into the unified enum
6. Return `FlightStatusResult` with `Source` set to the winning provider's name

## API Endpoints

### GET /flights/status

| Parameter    | Type   | Required | Format     |
|--------------|--------|----------|------------|
| flightNumber | string | Yes      | e.g. AA200 |
| date         | string | Yes      | yyyy-MM-dd |

**Responses:**
- `200 OK` — FlightStatusResult JSON
- `400 Bad Request` — `{ "error": "..." }` if params missing or date malformed

## Stub Scenarios

| Flight | AeroTrack Result            | AeroTrack Updated | QuickFlight Result       | QuickFlight Updated | Expected Winner   | Unified Status |
|--------|-----------------------------|-------------------|--------------------------|---------------------|-------------------|----------------|
| AA100  | ON_TIME, dep +2min          | 11:00 UTC         | on-time                  | 10:00 UTC           | AeroTrack (later) | OnTime         |
| AA200  | DELAYED, dep +50min         | 09:00 UTC         | late                     | 11:00 UTC           | QuickFlight (later)| Delayed       |
| AA300  | CANCELLED                   | 10:00 UTC         | (no data)                | —                   | AeroTrack (only)  | Cancelled      |
| AA400  | (no data)                   | —                 | diverted                 | 10:00 UTC           | QuickFlight (only)| Diverted       |
| AA500  | (no data)                   | —                 | (no data)                | —                   | None              | Unknown        |
| AA600  | ON_TIME, dep +5min          | 10:00 UTC         | late                     | 11:00 UTC           | QuickFlight (later)| Delayed       |
| BA100  | DIVERTED                    | 10:00 UTC         | on-time                  | 09:00 UTC           | AeroTrack (later) | Diverted       |
