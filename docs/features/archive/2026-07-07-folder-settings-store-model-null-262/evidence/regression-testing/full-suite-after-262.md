# Full TaskMaster.Test Suite After Fix — No Regression (P3-T4)

Timestamp: 2026-07-07T23-58

Command (plan): `vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /EnableCodeCoverage`
Command (executed, numeric-coverage reliable path): `vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:cobertura.runsettings /InIsolation`
(Direct-Cobertura runsettings is used because `/EnableCodeCoverage` emits a `.coverage` that does not
convert to a populated Cobertura offline in this environment; the DataCollector emits Cobertura
directly. `/InIsolation` is required for this Moq assembly.)

EXIT_CODE: 1 (sole failure is the environment-dependent live-Outlook-COM test; see below)

Output Summary:
- Total tests: 203 (baseline 200 + 3 new regression tests: null-deserialize, deserialize-throws,
  and the BuildFreshStoresWrapper direct-coverage test; the mis-specified test was renamed in place,
  not added). Passed: 202. Failed: 1.
- The single failure is `LiveHookup_OnSta_CompletesAndDoesNotBlockStaBeyondThreshold`
  (COMException 0x80010100 RPC_E_SYS_CALL_FAILED — requires a live Outlook COM class factory absent
  in this headless environment). It failed identically at the P0-T12 baseline; it is not a regression
  from this change. All other tests pass.
- Existing valid-config test `LoadAsync_AssignsStoresWrapperFromConfigAndCompletes` and the orthogonal
  `AppOlObjectsTests` still pass. No regression.

Repository line-coverage percentage (TaskMaster production package, the project changed):
- Baseline (P0-T12): 63.64%.
- Post-change: 63.92% (slight increase; no regression on previously-covered lines).

New/changed-code coverage (AppOlObjects.StoreLoading.cs — the restructured LoadStoresAsync plus the
new BuildFreshStoresWrapper seam):
- `TaskMaster.AppOlObjects` (StoreLoading.cs synchronous members: StoresWrapper, AwaitStoreRewireAsync,
  BuildFreshStoresWrapper): line-rate 100%.
- `<LoadStoresAsync>d__` state machine (StoreLoading.cs): line-rate 100%, branch-rate 100%.
- `<LoadAsync>d__` state machine (StoreLoading.cs): line-rate 100%.
- Aggregate AppOlObjects.StoreLoading.cs new/changed-code line coverage: 100% (>= 90% target).

Conclusion: 0 failed excluding the pre-existing environment-dependent LiveHookup test; pass count
(202) >= P0-T12 baseline plus the new tests; no regression on changed or previously-covered lines.
