# Reflection

## What I would improve with more time

### 1. Integration / end-to-end tests
The current tests cover unit logic (normalisation, merge). With more time I'd add integration tests using `WebApplicationFactory<Program>` to hit the actual HTTP endpoint, validating 400 responses, JSON shape, and full stub-to-response flows.

### 2. Provider timeout and resilience
Real providers fail. I'd wrap each `GetStatusAsync` call in a `CancellationToken`-based timeout and treat timed-out providers as returning `null`, so one slow provider never blocks the response.

### 3. Structured logging / observability
Add `ILogger` to `FlightStatusService` to log which provider was selected and why, including the `lastUpdatedUtc` delta. Useful for debugging provider divergence in production.

### 4. Response caching
Flight status doesn't change every millisecond. A short TTL cache (e.g. 30s keyed on `flightNumber + date`) would reduce redundant stub calls and would be essential with real HTTP providers.

### 5. OpenAPI response schema
The Swagger doc currently lacks typed response examples. Adding `ProducesResponseType<FlightStatusResult>` annotations and example values would make the API self-documenting for consumers.

### 6. Frontend as a proper SPA
The plain HTML/JS approach was chosen for zero-tooling simplicity. A React or Angular version would add proper state management, loading skeletons, and easier unit testing of UI components.

### 7. Error model consistency
The `400` error currently returns `{ "error": "..." }` (an anonymous object). With more time I'd define a `ProblemDetails`-compliant response using `Results.Problem(...)` for RFC 7807 compliance.

## AI tooling reflection

Claude Code was used across all SDLC phases — not just code generation:
- **Analysis**: Parsed the brief, identified constraints, ordered the deliverables (spec.md first)
- **Design**: Modelled `ProviderResponse` vs `FlightStatusResult` separation, chose 7 scenarios to cover all merge/status branches
- **Implementation**: Generated all C# and HTML/JS with correct syntax; no runtime errors on first build
- **Tests**: Designed boundary cases (exactly 15 min, conflicting providers, arrival-only delay) rather than happy-path-only tests

The main risk of heavy AI tooling is accepting plausible-but-wrong decisions without critique. I mitigated this by reviewing the normalisation boundary logic explicitly (inclusive vs exclusive 15-min threshold) and verifying the stub scenario matrix against the spec before writing any code.
