# Final QC — Acceptance Criteria Verification (Issue #207, increment 2)

Timestamp: 2026-06-19T23-35

AC source: docs/features/active/2026-06-18-outlook-startup-intelconfig-deserialize-stall-207/issue.md -> ## Acceptance Criteria (AC1-AC6, increment 2; Work Mode minor-audit).

## AC1 — Hook() times each of three COM operations individually via Stopwatch; one consolidated log4net block
- PASS.
- Evidence: TaskMaster/AppGlobals/AppEvents.cs Hook() (lines 173-196). Three dedicated Stopwatches:
  - toDoItemsStopwatch around `OlToDoItems = Globals.Ol.ToDoFolder.Items`,
  - remindersStopwatch around `OlReminders = Globals.Ol.OlReminders`,
  - inboxSubscribeStopwatch around the `Globals.Ol.Inboxes.ForEach(...)` per-inbox subscription loop.
  A single `LogStartupTiming("Hook complete | startup hook", ...)` emits `toDoItemsMs`, `remindersMs`, `inboxSubscribeMs` alongside the preserved `elapsedMs` and `inboxSubscriptions`, via the existing log4net logger and the `[Startup timing]`-prefixed `LogStartupTiming` helper. Exactly one consolidated emission; no per-operation interleaved logging.
- Cited artifacts: P1-T4, P1-T5 (implementation); P2-T2 evidence/qa-gates/analyzers-2026-06-19T18-40.md (no banned API).

## AC2 — ReadConfigurationAsync records GetSerializedConfigurations() read separately from deserialize; split visible
- PASS.
- Evidence: UtilitiesCS/EmailIntelligence/IntelligenceConfig.cs ReadConfigurationAsync (lines 91-95) wraps `GetSerializedConfigurations().ToList()` in its own `readStopwatch`, capturing `readElapsedMs` and `readEntryCount`. `FormatResourceTimingBreakdown(readElapsedMs, readEntryCount, timingRows)` renders a labeled "GetSerializedConfigurations read: durationMs=...; entries=..." line preceding the per-resource deserialize table, emitted once via `logger.Info($"[IntelConfig timing]\n...")` and retained on `LastResourceTimingBreakdown`.
- Cited artifacts: P1-T1, P1-T2 (implementation); P1-T7 test ReadConfigurationAsync_RecordsReadSeparatelyFromDeserialize_SplitIsVisible (Passed, P2-T4).

## AC3 — Behavior-preserving: Hook() subscriptions and Config contents/ordering unchanged
- PASS.
- Evidence:
  - Hook(): the three assignments, the per-inbox `OlInboxes.AddLast(... ItemAdd += OlInboxItems_ItemAdd)` subscription, and their ordering are unchanged; only Stopwatch wrappers and one log line were added.
  - IntelligenceConfig: the read result is materialized with `.ToList()` over the same `GetSerializedConfigurations()` enumeration; the subsequent `.ToAsyncEnumerable()` deserialize pipeline, null-loader filtering (`.Where(kvp => kvp.Value is not null)`), converter-attachment branches, and `PropertyChanged` subscription are unchanged. The increment-1 per-resource ResourceTimingRow rows remain intact.
  - Test pin: ReadConfigurationAsync_IsBehaviorPreserving_ConfigKeysMatchNonNullFixtures asserts Config keys == {People, Derived}, Count == 2, "Missing" filtered out, and loader instances preserved (Passed, P2-T4).
- Cited artifacts: P1-T3, P1-T4, P1-T8.

## AC4 — Deterministic MSTest (Moq + FluentAssertions) over IntelligenceConfig seam, no COM/network/FS/temp; Hook() logging-only exemption recorded
- PASS.
- Evidence: UtilitiesCS.Test/EmailIntelligence/IntelligenceConfig_Tests.cs uses [TestClass]/[TestMethod], Mock<IApplicationGlobals>, FluentAssertions, and the TestableIntelligenceConfig seam (overriding GetSerializedConfigurations and DeserializeLoaderAsync) over in-memory fixtures via the LastResourceTimingBreakdown observability property. No live COM, network, filesystem, or temp files. AppEvents.Hook() logging-only COM/VSTO exemption (UT5 exception) recorded.
- Cited artifacts: P1-T7 test; P1-T9 evidence/regression-testing/appevents-hook-com-exemption-2026-06-19T18-40.md; P2-T4 (3916 passed, 0 failed).

## AC5 — No banned API; Stopwatch only
- PASS.
- Evidence: All timing uses System.Diagnostics.Stopwatch. No DateTime.Now/DateTime.UtcNow/Random.Shared/Thread.Sleep/Task.Delay introduced. No RS0030 (BannedApiAnalyzers) diagnostic in the analyzer build.
- Cited artifact: P2-T2 evidence/qa-gates/analyzers-2026-06-19T18-40.md (0 errors; no RS0030).

## AC6 — Full toolchain green in order; coverage policy met, no regression
- PASS.
- Evidence (toolchain in order, all green in the final pass):
  1. CSharpier format: P2-T1 evidence/qa-gates/csharpier-2026-06-19T18-40.md (EXIT 0; check clean).
  2. .NET analyzers: P2-T2 evidence/qa-gates/analyzers-2026-06-19T18-40.md (EXIT 0; 0 errors).
  3. Nullable/TreatWarningsAsErrors: P2-T3 evidence/qa-gates/nullable-2026-06-19T18-40.md (EXIT 0; 0 warnings, 0 errors; no in-scope diagnostics).
  4. MSTest with coverage: P2-T4 evidence/qa-gates/mstest-coverage-2026-06-19T18-40.md (EXIT 0; 3916 passed, 0 failed).
  - Coverage: P2-T5 evidence/qa-gates/coverage-delta-2026-06-19T18-40.md — repo-wide no regression (raw +0.01pp, first-party +0.01pp); IntelligenceConfig.cs new/changed lines 100% (>= 90%); AppEvents.Hook() changes COM/VSTO-exempt.

## Overall verdict
ALL SIX ACCEPTANCE CRITERIA: PASS, each with a cited artifact path.
