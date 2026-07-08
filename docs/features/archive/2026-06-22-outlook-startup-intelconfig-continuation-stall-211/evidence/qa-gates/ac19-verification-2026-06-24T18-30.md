# AC19 Verification (Issue #211 PostLoad/LoadInboxes attribution probe)

Timestamp: 2026-06-24T18-30

AC19 clauses mapped to evidence artifacts and code locations.

| AC19 clause | Status | Evidence / code location |
|---|---|---|
| `PerformReadinessHookup` emits START + END (Stopwatch F2 ms) for each of the three COM ops (`[readiness-hookup] step=ToDoFolder.Items\|OlReminders\|Inboxes start\|end`) | PASS | `TaskMaster/AppGlobals/AppEvents.cs` `PerformReadinessHookup` (probe constructed with sink `s => logger.Debug(s)`; EmitReadinessHookupStart/End around each of the three operations). Line shapes asserted by `StartupInboxAttributionProbeTests` FormatReadinessHookupStart/End tests. |
| `LoadInboxes` emits one `[loadinboxes]` line per enumerated store with guarded DisplayName, ShouldIncludeStore Stopwatch ms, include/exclude result, and (included only) GetDefaultFolder(olFolderInbox) Stopwatch ms | PASS | `TaskMaster/AppGlobals/AppOlObjects.cs` `EmitPerStoreInboxAttribution` (extracted) + `LoadInboxes` wiring. Tests: EmitPerStoreInboxAttribution_IncludedStore / _ExcludedStore / _DisplayNameReadThrows. |
| Behavior-preserving: included-store set, inbox-subscription behavior, COMException rethrow, phase semantics unchanged | PASS | `LoadInboxes` retains `(Folder)inbox` cast (AppOlObjects.cs) and the unchanged `catch (COMException)` transient-vs-permanent branch; excluded store returns null -> caller skips add (byte-equivalent to original `continue`). COMException propagation asserted by EmitPerStoreInboxAttribution_GetDefaultFolderThrowsComException_PropagatesUnchanged. Inbox-subscription `Globals.Ol.Inboxes.ForEach(...)` line in AppEvents unchanged. |
| Existing `Hook complete` line retained | PASS | `AppEvents.cs` `PerformReadinessHookup` `LogStartupTiming("Hook complete | startup hook", ...)` byte-identical to baseline (only START/END markers were added around the existing Stopwatch/assignment statements). |
| Pure formatting/aggregation in a coverable helper (`StartupInboxAttributionProbe`, NOT `[ExcludeFromCodeCoverage]`) | PASS | `TaskMaster/AppGlobals/StartupInboxAttributionProbe.cs` — `public sealed class`, no `[ExcludeFromCodeCoverage]`, holds Format*/Emit* pure logic. |
| Deterministic MSTest (MSTest + Moq + FluentAssertions; no live COM/timer/filesystem/network; no temp files) | PASS | `TaskMaster.Test/AppGlobals/StartupInboxAttributionProbeTests.cs` — 14 tests, `[TestClass]`/`[TestMethod]`, FluentAssertions, Moq `MAPIFolder`, injectable delegates; no live boundary. Final run: 4123/4123 passed. |
| New helper meets >= 90% new-code coverage | PASS | `StartupInboxAttributionProbe` class line-rate 100%; `EmitPerStoreInboxAttribution` method line-rate 100%. See `coverage-delta-2026-06-24T18-30.md`. |
| No repository-wide coverage regression | PASS | Whole-process 61.89% -> 61.94%; TaskMaster 53.09% -> 53.55%; UtilitiesCS 87.46% flat. See `coverage-delta-2026-06-24T18-30.md`. |
| Stopwatch only; no banned API; net48 | PASS | Markers/attribution use `System.Diagnostics.Stopwatch` only. No DateTime.Now/UtcNow/Random.Shared/Thread.Sleep/Task.Delay introduced (BannedApiAnalyzers clean; nullable/TWAE build exit 0). Target net48 unchanged. |
| All touched files <= 500 lines | PASS | AppEvents.cs=478, AppEvents.ReadinessHookup.cs=58, AppOlObjects.cs=500, StartupInboxAttributionProbe.cs=146, StartupInboxAttributionProbeTests.cs=307. All <= 500. |
| Full C# toolchain passes in order | PASS | Final pass: CSharpier check exit 0 (`final-csharpier`); analyzers 0/0 (`final-analyzers`); nullable/TWAE 0/0 (`final-nullable`); tests 4123/4123 with coverage (`final-tests-coverage`). |

Conclusion: every AC19 clause maps to a passing artifact/location. AC19 = PASS.
