# QA Gate 4 — MSTest with Coverage on Touched Assemblies (Remediation Cycle 1)

- Timestamp: 2026-07-08T00-31
- Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /EnableCodeCoverage`
  (invoked via the vstest.console.exe full path found under Visual Studio 18 Community, with
  `/InIsolation` added per this repo's established Moq-assembly convention — see Deviation note;
  forward-slash relative DLL paths used, equivalent to the backslash form)
- EXIT_CODE: 1 (non-zero; caused solely by the 1 pre-existing, environment-dependent failure
  identified in the Phase 0 baseline — see Findings below; not a regression)
- Output Summary:
  - Total tests: 4410 (UtilitiesCS.Test + TaskMaster.Test assemblies only, per this task's scope)
  - Passed: 4409
  - Failed: 1 — `LiveHookup_OnSta_CompletesAndDoesNotBlockStaBeyondThreshold` in
    `TaskMaster.Test.AppGlobals.LiveOutlookHookupIntegrationTests`, the same pre-existing
    live-Outlook-COM-dependent failure identified and documented in
    `evidence/remediation-baseline/test-coverage-baseline-cycle1.md` (root cause: no live Outlook
    COM class factory available in this execution environment). No new failures. No test count
    decrease in either named assembly relative to what these assemblies contributed to the P0-T8
    baseline (7-assembly baseline was 5032 total; these 2 assemblies contribute 4410 of that
    total pre- and post-remediation, since the R1 split moved tests between two files within the
    same assembly and did not add or remove any test method).
  - All 13 tests directly affected by this remediation passed: the 6 moved `InclusionFilters_*`
    tests, the 5 moved disabled-store tests
    (`ShouldIncludeStore_ExcludesSessionDisabledStore_KeepsNonDisabled`,
    `ShouldIncludeStore_ExcludesFutureDisabledStore_KeepsNonDisabled`,
    `StoreIsIncluded_WhenIsDisabledTrue_ReturnsFalse`,
    `Init_ExcludesSessionAndFutureDisabledStores_ViaInstrumentedPath`,
    `Serialization_RoundTrip_PreservesDisabledListAndOmitsSessionSet`), and the 2 N1-fixed
    `ReenableAsync` guard tests (`Writes_ThrowArgumentException_ForSentinelIdentity`,
    `Writes_ThrowInvalidOperation_WhenModelIsNull`) — all reported `Passed` in the run output.
  - Coverage attachment produced: `TestResults\5b7bfec8-d93d-4ed1-8796-b2c6229367c3\...coverage`.

## Deviations

1. **`/InIsolation` flag added**: this repo's Moq-based test assemblies require `/InIsolation`
   under vstest to avoid a `Setup FileNotFound` error against `System.Threading.Tasks.Extensions`
   (documented repo convention). Added to the plan-literal command without changing its target
   assemblies or `/EnableCodeCoverage` intent.
2. **vstest.console.exe full path**: the bare `vstest.console.exe` command is not on this
   git-bash session's `PATH`; the full path under
   `Common7\IDE\Extensions\TestPlatform\vstest.console.exe` was used instead (same
   PATH-resolution class of deviation as QA Gates 2/3).
3. **Build-output restoration note**: immediately prior to this task, a diagnostic forced
   `/t:Rebuild` performed during P2-T3's verification (to confirm the nullable gate wasn't
   reusing a stale incremental cache) cleaned `UtilitiesCS.Test`'s own build output before
   failing on unrelated, pre-existing upstream nullable debt (documented in
   `qa-03-nullable-cycle1.md`), leaving `UtilitiesCS.Test.dll` temporarily absent. A plain
   `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"` (no
   nullable/analyzer property overrides) was run to restore the build output before this task's
   vstest invocation. This restoration step did not modify any source file; it only regenerated
   build artifacts, so it does not require restarting the toolchain loop from P2-T1.
