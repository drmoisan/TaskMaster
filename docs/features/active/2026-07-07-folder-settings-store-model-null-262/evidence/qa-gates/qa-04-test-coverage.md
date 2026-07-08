# QA-04 Test + Coverage (P4-T4)

Timestamp: 2026-07-08T00-04

Command (plan): `vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /EnableCodeCoverage`
Command (executed, reliable numeric-Cobertura path): `vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /Settings:cobertura.runsettings /InIsolation`
(Direct-Cobertura DataCollector is used because `/EnableCodeCoverage` emits a `.coverage` that does
not convert to a populated Cobertura offline in this environment. `/InIsolation` is required for the
Moq assembly.)

EXIT_CODE: 1 (sole failure = environment-dependent live-Outlook-COM test `LiveHookup_OnSta_...`,
RPC_E_SYS_CALL_FAILED; identical at baseline; not a regression)

Output Summary:
- Total tests: 203. Passed: 202. Failed: 1 (LiveHookup, environment-dependent, see above).

## New/changed-code coverage (store-loading logic) — AC7
AppOlObjects.StoreLoading.cs (the restructured LoadStoresAsync branches + BuildFreshStoresWrapper
seam + LoadAsync + StoresWrapper/AwaitStoreRewireAsync):
- `TaskMaster.AppOlObjects` (StoreLoading.cs synchronous members): line-rate 100%, branch-rate 100%.
- `<LoadStoresAsync>d__` state machine: line-rate 100%, branch-rate 100%.
- `<LoadAsync>d__` state machine: line-rate 100%, branch-rate 100%.
- Aggregate new/changed-code line coverage: 100% (>= 90% target). PASS.

## Repository line coverage (testable denominator) — AC7
- TaskMaster production package raw line-rate: 63.92% (post-change), up from 63.64% (baseline).
  The raw figure is below 80% only because it includes the COM/VSTO/WinForms-bound classes of
  AppOlObjects (live Outlook Application/NameSpace/Store access) that are policy-exempt from the
  testable-denominator floor per CLAUDE.md (COM/VSTO/WinForms coverage exemption).
- Repository-wide testable-denominator floor (>= 80%): this is a no-regression ("must remain") gate.
  This change is confined to the TaskMaster project and strictly INCREASES its coverage; it does not
  modify any other project's source, so it cannot lower repo-wide testable-denominator coverage.
- A fresh full-suite absolute repo-wide recomputation could not be produced in this execution
  environment because UtilitiesCS.Test contains a test that hard-deadlocks the pump-less CLI test
  host under coverage collection (stalls at ~3883/3907; MSTest TestTimeout cannot abort a synchronous
  STA-pump deadlock), and `.coverage`->Cobertura offline merge yields an empty report. This is a
  documented, change-independent constraint (CI likewise only uploads `.coverage` and computes no
  percentage gate). The no-regression property is established by construction above.
