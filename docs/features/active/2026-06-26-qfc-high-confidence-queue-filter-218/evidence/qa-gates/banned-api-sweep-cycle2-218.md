# Banned-API Sweep (Touched Production Files) — Cycle 2, Issue #218

Timestamp: 2026-06-28T17-31

Command: `Select-String -Path 'QuickFiler/Controllers/QfcDatamodel.cs','QfcDatamodel.FrameBuilding.cs','QfcDatamodel.QueueProcessing.cs','QfcHomeController.cs','QfcHomeController.Iteration.cs','QfcHomeController.Metrics.cs','EmailSorter.cs','QfcRemainingQueueAdmission.cs' -Pattern 'DateTime\.Now','DateTime\.UtcNow','Random\.Shared','Thread\.Sleep','Task\.Delay'`

EXIT_CODE: 0

Banned set: `DateTime.Now`, `DateTime.UtcNow`, `Random.Shared`, `Thread.Sleep`, `Task.Delay`. No matches for `DateTime.UtcNow`, `Random.Shared`, or `Thread.Sleep`. No matches at all in `EmailSorter.cs`, `QfcHomeController.Iteration.cs`, or `QfcRemainingQueueAdmission.cs`.

## Active-code matches and disposition

| # | File:Line | Code | Token | Disposition |
|---|-----------|------|-------|-------------|
| 1 | QfcDatamodel.FrameBuilding.cs:43 | `await Task.Delay(5);` | Task.Delay | DEFERRED-FINDING |
| 2 | QfcDatamodel.QueueProcessing.cs:142 | `await Task.Delay(200);` | Task.Delay | DEFERRED-FINDING |
| 3 | QfcHomeController.cs:75 | `$"{DateTime.Now.ToString("mm:ss.fff")} "` | DateTime.Now | DEFERRED-FINDING |
| 4 | QfcHomeController.Metrics.cs:20 | `var now = DateTime.Now;` | DateTime.Now | DEFERRED-FINDING |
| 5 | QfcHomeController.Metrics.cs:100 | `curDateText = DateTime.Now.ToString("MM/dd/yyyy");` | DateTime.Now | DEFERRED-FINDING |
| 6 | QfcHomeController.Metrics.cs:102 | `curTimeText = DateTime.Now.ToString("hh:mm");` | DateTime.Now | DEFERRED-FINDING |
| 7 | QfcHomeController.Metrics.cs:114 | `OlEndTime = DateTime.Now;` | DateTime.Now | DEFERRED-FINDING |
| 8 | QfcHomeController.Metrics.cs:214 | `await Task.Delay(20);` | Task.Delay | DEFERRED-FINDING |

Commented-out occurrences (no runtime effect; not banned-API call sites): QfcDatamodel.cs:58,65; QfcDatamodel.FrameBuilding.cs:54,61,76,79; QfcHomeController.cs:43,262,276,281,287; QfcHomeController.Metrics.cs:21,22.

## Deferred-finding rationale (why not removed in scope)

1. **Pre-existing, not introduced.** All 8 active sites are carried verbatim from the original `QfcDatamodel.cs`/`QfcHomeController.cs` by maintainer split commit `2637e4c1`. This remediation introduces no new banned-API call site.
2. **Policy classifies migration as follow-up.** `.claude/rules/csharp.md` holds RS0030 (BannedApiAnalyzers) at `severity = suggestion` for initial rollout and states: "Legacy call-site migration is follow-up work, not a requirement of adopting this [TimeProvider] guidance." The TimeProvider time-seam guidance "does not require rewriting existing call sites."
3. **No behavior-preserving drop-in exists.** Neither `QfcHomeController` nor `QfcDatamodel` currently has an injected `TimeProvider`/`IClock` or delay seam. Replacing these sites would require adding constructor-injected production seams plus deterministic tests, which is a production behavior/architecture change beyond the mechanical, behavior-preserving completion of the test split that defines this remediation's scope (Finding 1). The cycle-2 hard constraints forbid production behavior change beyond mechanical split completion.
4. **No build impact.** RS0030 at suggestion severity does not break the analyzer or nullable builds; the gate remains green.

Follow-up recommendation: migrate these 8 `DateTime.Now`/`Task.Delay` sites to the `System.TimeProvider` seam (`Microsoft.Bcl.TimeProvider`, already present in UtilitiesCS) under a dedicated time-seam migration cycle when RS0030 is promoted from `suggestion` to `warning`.

No production file was modified in this task; therefore no rebuild was required for the sweep itself (the Phase 2 analyzer/nullable builds already confirmed exit 0 on the unchanged production tree).

Output Summary: 8 active banned-API sites found across the touched production files, all pre-existing and carried by the maintainer split. Each is recorded as a precise DEFERRED-FINDING with file/line and rationale per the plan; none is silently retained. No banned API was introduced by this remediation. No production file modified.
