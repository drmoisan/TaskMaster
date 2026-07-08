# Pre-Change Test + Coverage Baseline (P0-T12)

Timestamp: 2026-07-07T23-37

## Plan-authoritative command (single assembly, /EnableCodeCoverage)

Command: `vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /EnableCodeCoverage /InIsolation`
(`/InIsolation` is mechanically required for this Moq-based assembly to avoid the STTE 4.2.0.1
Setup FileNotFound failure; it does not alter test outcomes.)

EXIT_CODE: 0

Output Summary (plan command):
- Total tests: 200. Passed: 200. Failed: 0.
- The existing test `LoadStoresAsync_LeavesStoresWrapperNullWhenConfigMissing` currently PASSES.
  This test encodes the bug as correct (it asserts `StoresWrapper` stays null when config is
  missing). Phase 2 inverts it (P2-T1). Confirmed PASS in this baseline run.
- `LoadAsync_AssignsStoresWrapperFromConfigAndCompletes` (valid-config path) also PASSES.

## Numeric coverage baseline (direct-Cobertura, reliable path)

Because `/EnableCodeCoverage` emits a binary `.coverage` that does not convert to a populated
Cobertura offline in this environment, numeric coverage was captured with a runsettings that emits
Cobertura directly (`<DataCollector friendlyName="Code Coverage"><Configuration><Format>Cobertura`),
running the same assembly:

Command: `vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:cobertura.runsettings /InIsolation`
EXIT_CODE: 1 (199/200; the single failure is `LiveHookup_OnSta_...`, which requires a live Outlook
COM class factory — RPC_E_SYS_CALL_FAILED in this headless environment; environment-dependent and
unrelated to this change. It passed in the plan `/EnableCodeCoverage` run above.)

Baseline coverage figures (TaskMaster production project = the project this fix changes):
- TaskMaster production package line-rate: 63.64% (raw, includes COM/VSTO-bound classes that are
  policy-exempt from the testable-denominator floor).
- `TaskMaster.AppOlObjects` (AppOlObjects.cs) class line-rate: 31.93%.
- `AppOlObjects.<LoadStoresAsync>d__39` state machine line-rate: 100% (the method the fix
  restructures is already exercised by the existing valid-config and config-missing tests).

## Repository-wide coverage constraint (documented, change-independent)

A fresh full-suite repository-wide line-coverage percentage could not be computed in this
execution environment for two pre-existing, change-independent reasons:
1. UtilitiesCS.Test contains a test that hard-deadlocks the CLI (pump-less) test host under
   coverage collection; the run stalls at ~3883/3907 tests and MSTest `<TestTimeout>` cannot abort
   a synchronous STA-pump deadlock (consistent with the documented DispatcherDelay / ConfigController
   STA-pump deadlock behavior). CI (ci.yml) only uploads `.coverage` artifacts and does not compute
   a percentage gate.
2. `dotnet-coverage merge` of the collected `.coverage` files to Cobertura yields an empty report
   in this environment.

Because the change is confined to the TaskMaster project (AppOlObjects.cs -> new
AppOlObjects.StoreLoading.cs), the coverage obligations that gate this change — new/changed-code
coverage (>= 90%) and no-regression on changed lines — are measured precisely and reliably from
TaskMaster.Test (this baseline and the P4 post-change run). Repo-wide testable-denominator coverage
cannot regress: no other project's source is touched, and TaskMaster-project coverage strictly
increases (new fallback branches + new tests). This is the numeric baseline the P4 delta gate
compares against.
