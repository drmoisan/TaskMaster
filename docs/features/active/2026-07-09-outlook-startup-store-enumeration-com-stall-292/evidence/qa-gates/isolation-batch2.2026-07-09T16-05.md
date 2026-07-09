# Batch 2 Toolchain Gate (issue #292, remediation cycle 1)

- Timestamp: 2026-07-09T16-05
- Task: [P2-T11]
- Batch 2 edits: `[DoNotParallelize]` added to `StoreWrapperInitProbeTests` (P2-T7), the `[TestClass]`-bearing partial part of `StoreWrapperController_Tests` in `StoreWrapperController_Tests.cs` (P2-T8), and `StoreWrapperControllerTests` (P2-T9).
- P2-T10: **N/A — census shows no scope-open path.** `StoreDisableServiceTests` subclasses `StoresWrapper` (`TestableStoresWrapper`) only to observe serialization; its tests exercise `DisableSessionOnly`/`DisableForFutureSessions`/`IsDisabled`/`GetDisabledStores`/`ReenableAsync` with a mocked or no-op `IStoreRehookService`. No `CurrentStoreContext.Begin`, `StoresWrapper.Init`/`RewireOlObjectsAsync`/`AddOrRestoreStore`, or `StoreWrapper.Init`/`Restore` executes (see `scope-open-census.2026-07-09T16-05.md` section (c)). No attribute added.
- P2-T8 partial-class safety: `[DoNotParallelize]` added on exactly one part; the other two parts (`StoreWrapperController_Tests.Launch.cs`, `StoreWrapperController_Tests.ButtonAndPopulate.cs`) carry no `[TestClass]` and were not edited — no CS0579 duplicate-attribute.
- All four steps passed in a single final pass; no restart required.

## Step 1 — Format

- Command: `dotnet tool run csharpier check .`
- EXIT_CODE: 0
- Output Summary: `Checked 1318 files in 4863ms.` Clean.

## Step 2 — Analyzer

- Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
- EXIT_CODE: 0
- Output Summary: `Build succeeded. 20 Warning(s) 0 Error(s)`. Same pre-existing CS0067 test-double warnings as Batch 1; unrelated to the attribute edits. No new diagnostics.

## Step 3 — Nullable / Type-check

- Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
- EXIT_CODE: 0
- Output Summary: `Build succeeded. 0 Warning(s) 0 Error(s)`.

## Step 4 — Test + coverage (UtilitiesCS.Test)

- Command (reliable coverage path): `dotnet-coverage collect --output <scratchpad>/utilcs.cobertura.xml --output-format cobertura --settings coverage.config -- <vstest.console.exe> UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook`
- EXIT_CODE: 0
- Output Summary: `Test Run Successful.` Total tests: 4268; Passed: 4268; **Failed: 0**; total time 41.9 s.
- Coverage headline: **UtilitiesCS package line-rate 88.18%** (no regression versus the P0-T6 pre-fix UtilitiesCS figure of 88.36% full-set / 88.16% single-assembly).
