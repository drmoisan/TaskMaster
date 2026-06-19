# AppEvents.Hook() — Logging-Only COM/VSTO Coverage Exemption (UT5 Exception Call-Out)

Timestamp: 2026-06-19T23-35

Rule:
- General Unit Test Policy UT5 (explicit-exception call-out for tests that cannot comply for a good reason).
- CLAUDE.md COM/VSTO coverage exemption: VSTO add-in lifecycle and Outlook Interop event handler classes that directly depend on `Microsoft.Office.Interop.Outlook` with no injectable seam are exempt from the 80% testable-denominator floor.

WhyNoUnitTest:
- `AppEvents.Hook()` (TaskMaster/AppGlobals/AppEvents.cs) depends directly on `Microsoft.Office.Interop.Outlook` via `Globals.Ol.ToDoFolder.Items`, `Globals.Ol.OlReminders`, and `Globals.Ol.Inboxes.ForEach(... x.Items ...)`. There is no injectable seam over these COM property accesses; exercising them requires a live Outlook process, which the External Dependency prohibition (no live COM/network/filesystem) forbids.
- The increment-2 change to `Hook()` is logging-only diagnostic instrumentation: three `System.Diagnostics.Stopwatch` measurements around the three existing COM operations and one consolidated `LogStartupTiming` emission carrying the three per-operation elapsed times. It introduces no new branch logic, no new return value, and does not alter the subscription behavior, the assignments, or their ordering.

VerificationMethod:
- Verified by inspection of the consolidated log block in `Hook()`: each of the three COM operations (`OlToDoItems = Globals.Ol.ToDoFolder.Items`, `OlReminders = Globals.Ol.OlReminders`, and the `Globals.Ol.Inboxes.ForEach(...)` subscription loop) is wrapped by its own dedicated `Stopwatch`, and a single `LogStartupTiming("Hook complete | startup hook", ...)` call emits `toDoItemsMs`, `remindersMs`, and `inboxSubscribeMs` alongside the preserved `elapsedMs` and `inboxSubscriptions`. No banned clock API (DateTime.Now/UtcNow, Random.Shared, Thread.Sleep, Task.Delay) is introduced; timing uses `Stopwatch` only.
- No live-Outlook test is introduced. The unit-testable IntelligenceConfig read-versus-deserialize seam IS covered by deterministic MSTest (UtilitiesCS.Test/EmailIntelligence/IntelligenceConfig_Tests.cs) via the `TestableIntelligenceConfig` seam and the `LastResourceTimingBreakdown` observability property.

SearchScope (negative-evidence accounting): docs/features/active/2026-06-18-outlook-startup-intelconfig-deserialize-stall-207/evidence/regression-testing/
SearchResult: this exemption dossier is the recorded UT5 exception artifact for the Hook() logging-only instrumentation.
