# vstest Final QA (Issue #232)

Timestamp: 2026-07-03T13-35

Command (plan-specified): `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /EnableCodeCoverage`

Command (actually executed): `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /InIsolation /Settings:<scratchpad>\cobertura.runsettings /ResultsDirectory:<scratchpad>\results`

Tooling note: identical rationale to the Phase 0 baseline (`evidence/baseline/vstest-baseline.md`). The
plain `/EnableCodeCoverage` binary `.coverage` collector is not reliably convertible to numeric per-file
coverage offline in this environment, so the same Cobertura-format runsettings (identical first-party +
Swordfish module set and `[ExcludeFromCodeCoverage]`/`GeneratedCode` attribute-exclude as the ratified
#214 config) is used. `/InIsolation` is required for the Moq-based test assemblies to initialize the test
host.

EXIT_CODE: 0

Output Summary:
- Total tests: 4641
- Passed: 4641
- Failed: 0
- Total time: 52.88 seconds
- Baseline comparison: baseline was 4637 total / 4636 passed / 1 failed. The +4 tests are the four Part A
  regression tests added in Phases 1-3 (`LoadControlsAndHandlers_01_ReportedRepro_...`,
  `LoadControlsAndHandlers_01_SwapsPage_...`, `RegisterNavigation_CalledTwiceWithoutInterveningUnregister_...`,
  `SwapItemGroups_ThenSkipGuardedTrailingRegister_...`). The single baseline failure
  (`TryGetFileStreamWriter_WhenWriterReturnsMemoryStream_ReturnsStream`, UtilitiesCS.Test, unrelated to
  this change) passed on this run. Post-change failure count (0) does not exceed the Phase 0 baseline
  failure count (1). No new failures introduced.
- Repository-wide line coverage (first-party + Swordfish module set): 76.5712%
  (line-rate 0.7657115749525617; lines-covered 40353 / lines-valid 52700).
- `QfcHighConfidencePreFilter.cs` module coverage: 100%. Every Cobertura `<class>` mapped to this file
  reports line-rate="1": `QfcHighConfidencePreFilter`, `QfcPreScoredItem`, `QfcHighConfidencePreFilter.<>c`,
  `QfcHighConfidencePreFilter.<>c__DisplayClass1_0`, `QfcHighConfidencePreFilter.<FilterAsync>d__1`, and
  the nested lambda state machine. The added `logger.Debug(...)` call inside the `FilterAsync` scoring
  lambda lives in the `<FilterAsync>d__1`/DisplayClass state machines (line-rate 1), and the new static
  `logger` field initializer is exercised (class line-rate 1). The compiler-generated method index
  shifted from `d__0`/`DisplayClass0_0` (baseline) to `d__1`/`DisplayClass1_0` because the new `logger`
  field precedes `FilterAsync`; this is an expected renumbering, not a coverage change.
