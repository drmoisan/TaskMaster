# Extraction Behavior-Preserved (P1-T4)

Timestamp: 2026-07-07T23-45

## Commands
1. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
2. `vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /InIsolation`

## EXIT_CODE
- Build: 0
- Test: 1 (sole failure is the environment-dependent live-Outlook-COM test; see below)

## Output Summary
- Build: succeeded. 0 Error(s), 37 Warning(s) (incremental; all pre-existing baseline noise, no
  new diagnostics introduced by the extraction).
- Test: Total 200. Passed 199. Failed 1.
  - The single failure is `LiveHookup_OnSta_CompletesAndDoesNotBlockStaBeyondThreshold`, which
    attempts to instantiate the live Outlook Application COM class factory
    (CLSID {0006F03A-0000-0000-C000-000000000046}) and fails with
    `COMException 0x80010100 RPC_E_SYS_CALL_FAILED` because no live Outlook process exists in this
    headless environment. This test failed identically at baseline (pre-change) in the direct-Cobertura
    run and fails deterministically 3/3 when run in isolation. It is unrelated to the store-loading
    extraction (it exercises live COM hookup, not `LoadStoresAsync`).
  - Behavior-preserving confirmation: the store-loading test set passes 7/7 when filtered
    (`LoadStores*`, `LoadAsync_AssignsStoresWrapper*`, `AwaitStoreRewire*`), and the mis-specified
    test `LoadStoresAsync_LeavesStoresWrapperNullWhenConfigMissing` STILL PASSES (extraction did not
    change `LoadStoresAsync` behavior). No test transitioned pass->fail because of the extraction; the
    only failing test is the pre-existing environment-dependent live-COM test.

## Conclusion
The Phase 1 extraction is behavior-preserving. `LoadStoresAsync` body is byte-for-byte unchanged
from baseline; `BuildFreshStoresWrapper()` is present but uncalled. No regression attributable to the
change. The effective stable baseline for this headless environment is 199/200 (LiveHookup excluded
as environment-dependent).
