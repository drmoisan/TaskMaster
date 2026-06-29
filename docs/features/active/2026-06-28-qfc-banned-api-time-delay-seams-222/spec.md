# qfc-banned-api-time-delay-seams - Refactor Spec

- **Issue:** #222
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-06-28T18-51
- **Status:** Draft
- **Version:** 0.1

## Intent & Outcomes

Eight pre-existing banned-API usages remain in the Quick Filer controllers. These were
flagged and verified pre-existing on `main` (not introduced) during issue #218
remediation, and logged as a deferred follow-up in PR #221. Banned APIs per repo policy
are `DateTime.Now`, `DateTime.UtcNow`, `Random.Shared`, `Thread.Sleep`, and `Task.Delay`.
Their presence makes the affected code non-deterministic and not unit-testable without
wall-clock dependence.

Exact active (non-commented) sites verified on HEAD:

- `Task.Delay`:
  1. `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs:43` -> `await Task.Delay(5)`
  2. `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs:142` -> `await Task.Delay(200)`
  3. `QuickFiler/Controllers/QfcHomeController.Metrics.cs:214` -> `await Task.Delay(20)`
- `DateTime.Now`:
  4. `QuickFiler/Controllers/QfcHomeController.cs:75` -> `DateTime.Now.ToString("mm:ss.fff")` in a log string
  5. `QuickFiler/Controllers/QfcHomeController.Metrics.cs:20` -> `var now = DateTime.Now`
  6. `QuickFiler/Controllers/QfcHomeController.Metrics.cs:100` -> `curDateText = DateTime.Now.ToString("MM/dd/yyyy")`
  7. `QuickFiler/Controllers/QfcHomeController.Metrics.cs:102` -> `curTimeText = DateTime.Now.ToString("hh:mm")`
  8. `QuickFiler/Controllers/QfcHomeController.Metrics.cs:114` -> `OlEndTime = DateTime.Now`


## Acceptance Criteria

- [x] All 8 active banned-API sites in the four target files are removed and replaced with injected seams.
- [x] No new banned-API usages introduced; RS0030 not suppressed globally and policy files not weakened.
- [x] Production behavior preserved: delays remain 5/200/20 ms; timestamp formats and semantics unchanged.
- [x] Seams injected through `QfcHomeController` and `QfcDatamodel` construction paths without breaking public `IQfcDatamodel` / home-controller surfaces.
- [x] Every touched file remains <= 500 lines.
- [x] Focused MSTest+Moq+FluentAssertions tests prove time-dependent output uses the injected clock and delayed paths await the injected delay (Moq-verifiable), with no live Outlook COM and no temp files.
- [ ] New/changed code targets >= 90% coverage; coverage on changed lines not reduced; repo-wide floor (>= 80%) maintained.
- [x] C# toolchain passes in order: csharpier -> analyzer build -> nullable build (TreatWarningsAsErrors) -> vstest with coverage.

## Invariants (must not change)

The following behaviors, contracts, and external surfaces must remain identical:

- Delay durations: exactly 5 ms (`QfcDatamodel.FrameBuilding`), 200 ms (`QfcDatamodel.QueueProcessing`), and 20 ms (`QfcHomeController.Metrics`). No duration may change.
- Timestamp format strings and semantics: `"mm:ss.fff"` (LaunchAsync catch-block log), `"MM/dd/yyyy"` (`curDateText`), and `"hh:mm"` (`curTimeText`) must be preserved verbatim, and the `OlEndTime` assignment must continue to capture local now.
- Public surfaces `IQfcDatamodel` and `IQfcHomeController` must remain unchanged (no member additions, removals, or signature changes). Seams are injected via internal members and an optional factory parameter only.
- Performance characteristics to preserve (latency/throughput/memory): unchanged; the seam defaults to `TimeProvider.System`, so production timing matches today's behavior exactly.
- Compatibility guarantees (CLI flags, config schemas, versions): no changes; `LaunchAsync` gains an optional `TimeProvider timeProvider = null` parameter that is backward-compatible with existing callers.

## Scope (structural changes)

Route all eight sites through injectable time/delay seams so the source of current time
and async delay becomes injectable. Production defaults must match today's behavior
exactly (same 5/200/20 ms delays, same timestamp semantics). Either reuse/extend an
existing UtilitiesCS abstraction, add a minimal hand-rolled `IClock` + `IAsyncDelay`
seam, or add `Microsoft.Bcl.TimeProvider` (dependency approval required). Prefer the
simplest behavior-preserving design.


## Non-Goals

What is explicitly out of scope (new behavior, perf changes, UX changes, flags).

## Dependencies / Touchpoints

Upstream/downstream modules, CLIs, data paths, automation, or external consumers that rely on current structure.
- Required coordination (other teams, CI/CD, release tooling):

## Risks & Mitigations

- Solution targets .NET Framework VSTO; `System.TimeProvider` requires the
  `Microsoft.Bcl.TimeProvider` package (dependency approval required if chosen).
- Must not break `IQfcDatamodel` / home-controller public surfaces; use internal seams /
  defaults consistent with the issue #218 injection approach.
- Behavior preservation is mandatory.


## Technical Specifications

- Files/modules expected to change:
  - `QuickFiler/Controllers/QfcDatamodel.cs` (seam property)
  - `QuickFiler/Controllers/QfcDatamodel.FrameBuilding.cs` (5 ms delay site)
  - `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` (200 ms delay site)
  - `QuickFiler/Controllers/QfcHomeController.cs` (LaunchAsync optional parameter + `mm:ss.fff` site)
  - `QuickFiler/Controllers/QfcHomeController.Metrics.cs` (seam property + four time/delay sites)
  - `QuickFiler/QuickFiler.csproj` (Microsoft.Bcl.TimeProvider reference)
  - `QuickFiler/packages.config` (Microsoft.Bcl.TimeProvider package)
  - `QuickFiler.Test/QuickFiler.Test.csproj` (Microsoft.Bcl.TimeProvider + Microsoft.Extensions.TimeProvider.Testing references)
  - `QuickFiler.Test/packages.config` (Microsoft.Bcl.TimeProvider + Microsoft.Extensions.TimeProvider.Testing packages)
  - New deterministic seam test file under `QuickFiler.Test/Controllers/` (plus existing test classes for the new tests)
- Public interfaces/contracts affected (even if behavior is unchanged): none; `IQfcDatamodel` and `IQfcHomeController` are unchanged.
- Data flow or validation adjustments: current-time and async-delay reads are routed through the injectable `System.TimeProvider` seam; values and formats are unchanged.
- Logging/telemetry updates (if any): the LaunchAsync catch-block log timestamp is sourced from the seam; format and content are unchanged.
- Migration or backfill needs (if any): none.

## Test Strategy

- Regression tests to add or update:
- Invariant validation tests (ensuring outputs/behavior unchanged):
- Edge cases and negative scenarios (import/path stability, CLI flags):
- Error handling and logging verification:
- Coverage impact and targets for changed lines/modules:
- Toolchain commands to run (format → lint → type-check → test):
- Manual validation steps (if required):

## Definition of Done

- [ ] Structure matches this spec; legacy paths retired or redirected
- [ ] Invariants validated with tests or comparisons
- [ ] Imports/tooling/entry points updated
- [ ] Edge cases and error handling verified
- [ ] Tests, linting, and type checks clean
- [ ] Docs updated (initiative/README/tasks as needed)
- [ ] Toolchain pass completed (format → lint → type-check → test)

## Seeded Test Conditions (from potential)
- [ ] Injected clock controls timestamp-producing output (mm:ss.fff, MM/dd/yyyy, hh:mm).
- [ ] Delayed paths await the injected delay seam (Moq verify) instead of wall-clock `Task.Delay`.
- [ ] Production default seam yields current behavior.
- [ ] No live Outlook COM, no temp files, deterministic.
