---
name: net481-timeprovider-available
description: TimeProvider/FakeTimeProvider ARE available on this repo's net481 projects via Microsoft.Bcl.TimeProvider + Microsoft.Extensions.TimeProvider.Testing — do not accept "net8+ only" claims.
metadata:
  type: reference
---

`System.TimeProvider` and `Microsoft.Extensions.Time.Testing.FakeTimeProvider` are available in this
repository's **net481** projects, despite being .NET 8+ BCL types, because the polyfill packages are
already referenced.

Verified 2026-08-07 (F14 / issue #456 research):
- `Microsoft.Bcl.TimeProvider 10.0.10` — referenced by `QuickFiler/QuickFiler.csproj`,
  `QuickFiler.Test/QuickFiler.Test.csproj`, `UtilitiesCS/UtilitiesCS.csproj`,
  `UtilitiesCS.Test/UtilitiesCS.Test.csproj`, `TaskMaster/TaskMaster.csproj`
  (all `lib\net462\Microsoft.Bcl.TimeProvider.dll`).
- `Microsoft.Extensions.TimeProvider.Testing 10.8.0` — referenced by `QuickFiler.Test` and
  `UtilitiesCS.Test` (supplies `FakeTimeProvider`).
- Already in production use: `QuickFiler/Helper Classes/EmailMoveMonitor.cs`,
  `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs`, `QfcHomeController*.cs`,
  `QfcDatamodel*.cs`, `UtilitiesCS/Threading/{UiThread,ThreadMonitor,LockupStallDecider}.cs`.
- Already in test use: `QuickFiler.Test/Controllers/QfcDatamodelTests.cs`,
  `QfcHomeControllerMetricsTests.cs`, `QfcStreamingDequeueConfidenceGateTests*.cs`.

The repo states the rule in-code at `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs:316`:
"Moq cannot mock the non-virtual GetLocalNow(); FakeTimeProvider is the prescribed seam."

There is **no** `IClock`, `ISystemClock`, or `ITimerService` seam in QuickFiler; `TimeProvider` is the
only clock abstraction and is what `.claude/rules/general-unit-test.md` § Determinism Infrastructure
names for .NET.

**How to apply:** when a delegation prompt or plan asserts "net481, so TimeProvider/FakeTimeProvider is
unavailable — recommend something else", verify against the csproj `<Reference>` entries before
accepting it. That premise has been supplied and was wrong at least once. Recommend `TimeProvider` +
`FakeTimeProvider` as the deterministic-clock seam for any net481 project in this repo.
