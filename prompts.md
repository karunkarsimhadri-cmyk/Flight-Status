# AI Prompts & Decisions Log

## Tool Used
Claude Code (Anthropic) — IDE-integrated, used throughout the SDLC.

---

## 1. Analysis — Reading the brief

**Prompt:** _(PDF of challenge brief uploaded)_  
> "Please go through the document and create application and build solution"

**What AI did:**
- Parsed all functional requirements, tech stack constraints, and evaluation criteria
- Identified the key design decision: normalisation must be decoupled from merge logic
- Flagged that `spec.md` must be committed before any implementation files

**Judgement call:** Used plain HTML/JS for the frontend (one of the allowed options) because it has zero build tooling, making "run from a clean clone" trivially satisfied.

---

## 2. Design — spec.md

**Prompt:** Drafted spec.md covering data models, interface contracts, normalisation rules, and stub scenarios.

**Judgement calls:**
- Named the enum `FlightStatus` (not `UnifiedStatus`) — clear intent, no namespace clash since root namespace is `FlightStatus.Api`
- `ProviderResponse` is an internal record; consumers never see raw status strings
- Merge logic lives in `FlightStatusService`, normalisation in `StatusNormaliser` — single responsibility

---

## 3. Stub scenario design

**Prompt:** Designed 7 deterministic stub scenarios to cover all merge and status branches.

**Scenarios chosen to exercise:**
- AA100: Both respond, AeroTrack is later → AeroTrack wins
- AA200: Both respond, QuickFlight is later → QuickFlight wins
- AA300: AeroTrack only → single-provider path
- AA400: QuickFlight only → single-provider path
- AA500: Neither responds → Unknown path
- AA600: Both respond, QuickFlight is later AND disagrees → QuickFlight wins (conflict case)
- BA100: Both respond, AeroTrack is later AND disagrees → AeroTrack wins (conflict case)

AA600 and BA100 deliberately show conflicting statuses so the merge rule is tested under disagreement.

---

## 4. Normalisation design

**Prompt:** Design `StatusNormaliser` to handle multiple raw-status vocabularies.

**Approach:** Normalise the raw string to uppercase with `-` and `_` stripped, then switch:
- Explicit terminal states (CANCELLED, DIVERTED, DELAYED) → always honoured
- ONTIME / SCHEDULED → compute from actual times if present, else trust the string
- Anything else → compute from times or Unknown

**Boundary:** 15 minutes is inclusive (≤ 15 = OnTime, per spec "within 15 minutes").

---

## 5. Test design

**Prompt:** Write meaningful xUnit tests for normalisation and merge rules.

**Tests written:**
- 10 theory cases for raw-status string mapping
- 3 boundary tests for the 15-minute threshold (14 min, exactly 15, 16 min)
- 1 test for arrival delay overriding on-time departure
- 1 test confirming CANCELLED ignores actual times
- 4 merge tests: both providers (later wins), only A, only B, neither
- 1 merge test for conflicting statuses (QuickFlight later → Delayed overrides AeroTrack OnTime)

---

## 6. Frontend

**Prompt:** Build a plain HTML/JS UI with colour-coded status card.

**Colour choices:**
- Green (#22c55e) — OnTime
- Amber (#f59e0b) — Delayed
- Red (#ef4444) — Cancelled, Diverted
- Grey (#64748b) — Unknown

**AeroTrack-only fields** (terminal, gate, delay reason) rendered only when non-null — `detail()` helper returns empty string for missing values.
