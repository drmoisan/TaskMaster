# P2-T21/P2-T22 Loop Iteration 2 — Layer 4: TaskMaster.csproj and QuickFiler.Test.csproj

Timestamp: 2026-07-20T03-05

## Context

After the P2-T22 QuickFiler.csproj fix, the P2-T23 full solution-wide rebuild surfaced a further
layer, per P2-T23's own instruction to "repeat the P2-T21/P2-T22/P2-T23 loop... until EXIT_CODE: 0
is reached" when newly-surfaced diagnostics are resolvable via the three authorized patterns.

## Baseline (this iteration)

Command: `MSBuild.exe TaskMaster.sln -t:Rebuild -m -p:Configuration=Debug "-p:Platform=Any CPU" -p:TreatWarningsAsErrors=true`

EXIT_CODE: 1 (10 Error(s), 0 Warning(s))

Per-project status: `QuickFiler.csproj` (13) now succeeds (confirms the P2-T22 fix held).
Genuinely new own-diagnostics surfaced in `TaskMaster.csproj` (15) and `QuickFiler.Test.csproj`
(14); `TaskMaster.Test.csproj` (9) and `UtilitiesCS.Test.csproj` (16) fail only as cascading
dependency failures on `TaskMaster.csproj` (grep-confirmed zero own `error` lines for each).

### TaskMaster.csproj (9 diagnostics)

- **CS8632** (4 sites) — pre-existing `?` nullable annotations used without an annotations
  context (file has no project-level `<Nullable>` and no whole-file `#nullable` pragma):
  - `TaskMaster/AppGlobals/EngineInitTimingProbe.cs(55,61)` and `(57,57)` —
    `Task<IConditionalEngine<MailItemHelper>?>` return type and `Func<...>` parameter
  - `TaskMaster/AppGlobals/ApplicationGlobals.cs(251,57)` — `DispatcherTimer? _startupHeartbeat`
  - `TaskMaster/AppGlobals/NonBlockingDelay.cs(47,18)` — `Timer? timer = null;`
- **CS8767** (1 site) — `TaskMaster/AppGlobals/StoreRehookCoordinator.cs(268,25)`:
  `StoreScopedReadinessGate.IsReady(Outlook.Store store)` (non-nullable parameter) does not match
  `IOutlookReadinessGate.IsReady(Outlook.Store? store)` (nullable parameter) it implicitly
  implements.
- **CS0618** (4 sites) — obsolete `System.Linq.Async` usage:
  `TaskMaster/AppGlobals/AppItemEngines.cs(57,34)` (`SelectAwait`),
  `TaskMaster/AppGlobals/AppEvents.cs(269,47)` (`WhereAwait`),
  `TaskMaster/AppGlobals/AppEvents.cs(301,27)` (`ForEachAwaitAsync`),
  `TaskMaster/Ribbon/RibbonController.Intelligence.cs(398,23)` (`ForEachAwaitAsync`).

### QuickFiler.Test.csproj (1 diagnostic)

- **MSTEST0032** (1 site) — `QuickFiler.Test/Controllers/QfcFormControllerTests.cs(694,13)`:
  `Assert.IsTrue(true)`, a pre-existing tautological placeholder assertion in
  `UndoConsumer_ShouldConsumeUndoQueue`.

## Remediation applied

| File | Line(s) | Code | Pattern applied | Rationale |
|---|---|---|---|---|
| `EngineInitTimingProbe.cs` | 55-58 | CS8632 x2 | `#nullable enable annotations` / `restore annotations` bracket around the method signature | Scopes the pre-existing nullable annotation without full-file nullable enable; zero IL/behavior change |
| `ApplicationGlobals.cs` | 251 | CS8632 | Same bracket around the field declaration | Same |
| `NonBlockingDelay.cs` | 47 | CS8632 | Same bracket around the local declaration | Same |
| `StoreRehookCoordinator.cs` | 268 | CS8767 | Same bracket around the method, parameter changed to `Outlook.Store?` to match the interface | Annotation-only; body passes the parameter through unchanged, no null-check added or removed |
| `AppItemEngines.cs` | 57-78 (statement) | CS0618 | Narrow pragma bracket | `SelectAwait` migration is a call-shape change; suppression preserves exact behavior |
| `AppEvents.cs` | 269-274 (statement) | CS0618 | Narrow pragma bracket | `WhereAwait` migration is a call-shape change; suppression preserves exact behavior |
| `AppEvents.cs` | 301-303 (statement) | CS0618 | Narrow pragma bracket | `ForEachAwaitAsync` -> `await foreach` is a control-flow change; suppression preserves exact behavior |
| `RibbonController.Intelligence.cs` | 398-401 (statement) | CS0618 | Narrow pragma bracket | Same as above |
| `QfcFormControllerTests.cs` | 694 | MSTEST0032 | Narrow pragma bracket | Replacing the tautological placeholder assertion with a genuine one is a test-behavior change, out of scope for this narrow build-debt remediation; suppression preserves the exact pre-existing (placeholder) test behavior |

All fixes fall within the three authorized patterns. No diagnostic required a behavior change; no
escalation was necessary.

## Verification

Command: `MSBuild.exe TaskMaster/TaskMaster.csproj -t:Rebuild -p:Configuration=Debug -p:Platform=AnyCPU -p:TreatWarningsAsErrors=true`
EXIT_CODE: 0 — Build succeeded, 0 Warning(s), 0 Error(s).

Command: `MSBuild.exe QuickFiler.Test/QuickFiler.Test.csproj -t:Rebuild -p:Configuration=Debug -p:Platform=AnyCPU -p:TreatWarningsAsErrors=true`
EXIT_CODE: 0 — Build succeeded, 0 Warning(s), 0 Error(s).

## Next step

Proceed to the next P2-T23 full solution-wide rebuild iteration.
