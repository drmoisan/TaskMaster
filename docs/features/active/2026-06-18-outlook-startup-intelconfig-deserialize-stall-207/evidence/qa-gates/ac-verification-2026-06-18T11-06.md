# Phase 2 — Acceptance Criteria Verification (Issue #207)

Timestamp: 2026-06-18T11-06

AC source: docs/features/active/2026-06-18-outlook-startup-intelconfig-deserialize-stall-207/issue.md -> `## Acceptance Criteria` (AC1-AC6).

Scope: exactly two files changed -
- UtilitiesCS/EmailIntelligence/IntelligenceConfig.cs (production instrumentation)
- UtilitiesCS.Test/EmailIntelligence/IntelligenceConfig_Tests.cs (tests)

---

## AC1 — Per-resource key + payload size + Stopwatch elapsed recorded for each entry

Verdict: PASS

Evidence:
- IntelligenceConfig.cs `ReadConfigurationAsync`: for every enumerated resource entry, the code
  computes the UTF-8 payload size (`Encoding.UTF8.GetByteCount(kvp.Value)`), starts a
  `System.Diagnostics.Stopwatch` immediately before the `await DeserializeLoaderAsync(kvp.Value)`
  call, stops it immediately after, and records a `ResourceTimingRow(kvp.Key, sizeBytes,
  stopwatch.Elapsed.TotalMilliseconds)`. The row is added inside the per-entry lambda, so every
  enumerated entry contributes exactly one row (including the null-loader entry, which still gets a
  row before the null-handling branch).
- Test `ReadConfigurationAsync_WithFixtureResources_ProducesBreakdownRowPerEntry` asserts a row for
  each of three fixture keys (People, Derived, Missing), including the null-loader entry.
- Test `ReadConfigurationAsync_RecordsUtf8PayloadSizePerEntry` asserts the size column carries the
  UTF-8 byte count of the serialized payload.
- Coverage: instrumentation lines 100% covered (evidence/qa-gates/mstest-coverage-2026-06-18T11-06.md).

## AC2 — Single consolidated log4net block, [Startup timing]-consistent style

Verdict: PASS

Evidence:
- IntelligenceConfig.cs emits the breakdown exactly once after the per-resource loop completes via
  `logger.Info($"[IntelConfig timing]\n{LastResourceTimingBreakdown}")`. There is one emission
  call; no per-iteration table logging.
- The table is rendered by `FormatResourceTimingBreakdown` using
  `UtilitiesCS.PrettyPrinters.ToFormattedText(string[][], headers, justifications)` - the same
  column-alignment helper that `StartupTimingRecorder.FormatTable` uses for the existing
  `[Startup timing]` table - producing the same bordered `===`/header layout.
- The emission uses the same `log4net.ILog` logger and the same `logger.Info(...)` shape as
  `StartupTimingRecorder.EmitTable` (`logger.Info($"[Startup timing]\n{FormatTable()}")`), so it is
  captured on the same console/Debug output path during startup.
- Test `ReadConfigurationAsync_WithFixtureResources_ProducesBreakdownRowPerEntry` asserts the
  rendered text contains the `Duration`, `SizeBytes`, and `ResourceKey` headers, confirming the
  consolidated table style.

## AC3 — Behavior-preserving: Config contents / ordering unchanged

Verdict: PASS

Evidence:
- The only additions to `ReadConfigurationAsync` are the stopwatch measurement, the timing-row
  accumulation, and the single post-loop log emission. The enumeration source
  (`GetSerializedConfigurations().ToAsyncEnumerable()`), the null-loader filtering
  (`logger.Error` + null KVP, then `.Where(kvp => kvp.Value is not null)`), the converter-attachment
  branches (PeopleScoConverter / ScoDictionaryConverter), the `PropertyChanged` subscription, and
  the final `.ToConcurrentDictionaryAsync()` are all unchanged.
- Pinning test `ReadConfigurationAsync_IsBehaviorPreserving_ConfigKeysMatchNonNullFixtures` asserts
  the returned dictionary keys equal exactly the non-null fixture keys (People, Derived), excludes
  the null-loader key (Missing), and returns the same loader instances.
- The pre-existing test `InitAsync_WhenResourcesDeserializeLoaders_AddsConvertersAndWritesCurrentConfiguration`
  continues to pass unchanged, confirming converter attachment and write semantics are intact.

## AC4 — Deterministic MSTest (Moq + FluentAssertions) over existing seams, no COM/network/FS/temp

Verdict: PASS

Evidence:
- Three new MSTest methods use `[TestMethod]`, `Mock<IApplicationGlobals>` (Moq), and
  FluentAssertions. They drive the existing `TestableIntelligenceConfig` seam, which overrides the
  internal-virtual seams `GetSerializedConfigurations` and `DeserializeLoaderAsync` (the issue text
  references `protected internal virtual`; the actual modifier in the codebase is `internal virtual`,
  overridden via InternalsVisibleTo("UtilitiesCS.Test")).
- Fixtures are in-memory dictionaries; no live Outlook/COM, no network, no filesystem, no temporary
  files. Determinism: assertions check structural content (keys, headers, byte counts), not timing
  magnitudes, so they do not depend on wall-clock duration.
- All three tests pass: see evidence/qa-gates/trx/final.trx and final-full.trx (3915/3915 passed).

## AC5 — No banned API; Stopwatch only

Verdict: PASS

Evidence:
- Timing uses `System.Diagnostics.Stopwatch.StartNew()` / `.Stop()` / `.Elapsed.TotalMilliseconds`.
- No `DateTime.Now`, `DateTime.UtcNow`, `Random.Shared`, `Thread.Sleep`, or `Task.Delay` introduced.
- Analyzer build (evidence/qa-gates/analyzers-2026-06-18T11-06.md): no RS0030 (BannedApiAnalyzers)
  match anywhere in the build log; 0 errors.

## AC6 — Full toolchain green in order; coverage policy met; no regression

Verdict: PASS

Evidence (toolchain run in order CSharpier -> analyzers -> nullable -> MSTest-with-coverage):
- P2-T1 CSharpier: evidence/qa-gates/csharpier-2026-06-18T11-06.md (EXIT_CODE 0, check clean).
- P2-T2 Analyzers: evidence/qa-gates/analyzers-2026-06-18T11-06.md (EXIT_CODE 0, 0 errors, no RS0030).
- P2-T3 Nullable: evidence/qa-gates/nullable-2026-06-18T11-06.md (EXIT_CODE 0, 0 warnings/errors).
- P2-T4 MSTest+coverage: evidence/qa-gates/mstest-coverage-2026-06-18T11-06.md (3915/3915 passed).
- P2-T5 Coverage delta: evidence/qa-gates/coverage-delta-2026-06-18T11-06.md (PASS; no repo-wide
  regression; new/changed lines effectively 100%).
- Loop note: the analyzer step initially failed (CS0518 from the positional record struct); the fix
  (constructor-initialized readonly struct) restarted the loop from CSharpier, and the recorded
  results above are the clean final pass.

---

## Summary: all AC1-AC6 PASS. Verdict: PASS.
