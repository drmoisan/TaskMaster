# Batch 1 Toolchain Gate (issue #292, remediation cycle 1)

- Timestamp: 2026-07-09T16-05
- Task: [P2-T6]
- Batch 1 edits: `[DoNotParallelize]` added to `StoresWrapperTests`, `StoresWrapperRehookTests`, `StoresWrapperDisableTests`, `StoreWrapperTests`, `StoreWrapperViewerTests` (P2-T1..T5).
- All four steps passed in a single final pass; no restart was required.

## Step 1 — Format

- Command: `dotnet tool run csharpier check .`
- EXIT_CODE: 0
- Output Summary: `Checked 1318 files in 4168ms.` Clean; the added attribute lines are already csharpier-conformant (no reformat).

## Step 2 — Analyzer

- Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
- EXIT_CODE: 0
- Output Summary: `Build succeeded. 20 Warning(s) 0 Error(s)`. The 20 warnings are pre-existing CS0067 (unused-event) diagnostics in `UtilitiesCS.Test` test doubles (e.g., `SmartSerializableBase_Tests`, `StoreWrapperControllerTests.OlObjectsStubBase`, `SmartSerializable_Tests`), surfaced only because editing a `UtilitiesCS.Test` file forced that project to recompile (the P0-T4 baseline was incremental/skipped). They are unrelated to the `[DoNotParallelize]` attribute (an attribute cannot produce CS0067) and none occur in the five edited files. The analyzer gate carries no TreatWarningsAsErrors, so exit 0 = pass.

## Step 3 — Nullable / Type-check

- Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
- EXIT_CODE: 0
- Output Summary: `Build succeeded. 0 Warning(s) 0 Error(s)`. No warnings-as-errors; the added attributes introduce no nullable diagnostics.

## Step 4 — Test + coverage (UtilitiesCS.Test)

- Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /TestCaseFilter:"TestCategory!=LiveOutlook"`
- EXIT_CODE: 0
- Output Summary: `Test Run Successful.` Total tests: 4268; Passed: 4268; **Failed: 0**; total time 45.3 s. The `CurrentStoreContext` null-baseline readers now pass under single-assembly execution because every writer they could overlap is in the serial bucket.
- Coverage headline (reliable path `dotnet-coverage collect` -> Cobertura, since the `/EnableCodeCoverage` `.coverage` is not offline-convertible here): **UtilitiesCS package line-rate 88.16%** (single-assembly run; the full CI-equivalent set measured UtilitiesCS at 88.36% in P0-T6). No regression versus the pre-fix UtilitiesCS figure.
