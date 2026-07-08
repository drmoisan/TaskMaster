# Remediation Cycle End-State Summary (Issue #240)

- Timestamp: 2026-07-06T12-15

## Split Correctness

- All three resulting files are <= 500 lines (see `evidence/qa-gates/split-linecount-verification.md`):
  - `StoreWrapperController_Tests.cs`: 181 lines
  - `StoreWrapperController_Tests.ButtonAndPopulate.cs`: 396 lines
  - `StoreWrapperController_Tests.Launch.cs`: 234 lines
- All 39 `[TestMethod]`s are preserved with zero added/dropped (see `evidence/qa-gates/split-testmethod-verification.md`); the pre-split and post-split method-name sets are byte-identical.
- Containment held: zero diff to `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs` (see `evidence/qa-gates/split-containment-verification.md`). The tracked diff is limited to `StoreWrapperController_Tests.cs` (trim) and `UtilitiesCS.Test.csproj` (two new `<Compile Include>` entries), plus two new untracked files (`StoreWrapperController_Tests.ButtonAndPopulate.cs`, `StoreWrapperController_Tests.Launch.cs`).

## Final Toolchain Pass (P2-T1..P2-T4)

Executed in order, in a single continuous pass, with no `SKIPPED` outcomes:

1. **Format** (`dotnet tool run csharpier format .`) — EXIT_CODE 0; zero files reformatted.
2. **Analyzers** (`msbuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`) — EXIT_CODE 0; zero new warnings/errors in the three split files (20 pre-existing warnings in unrelated files).
3. **Nullable/type-check** (`msbuild ... /t:Rebuild /p:Nullable=enable /p:TreatWarningsAsErrors=true`) — EXIT_CODE 1; 84 pre-existing errors confined to the same two out-of-scope vendored projects (`SVGControl.csproj`, `UtilitiesSwordfish.NET.General.csproj`) as the P0-T6 baseline; zero new diagnostics in the three split files.
4. **Test + coverage** (`vstest.console.exe UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /EnableCodeCoverage /InIsolation`) — EXIT_CODE 0; 4170/4170 passed; `UtilitiesCS.dll` line coverage 85.88%, unchanged from the P0-T7 baseline (see `evidence/qa-gates/qa-05-coverage-delta.remediation-2026-07-06T12-15.md`).

No step required a restart of the loop: formatting made zero changes, and the only non-zero exit code (nullable/type-check) reproduced the same pre-existing, out-of-scope vendor-project error set already documented in the P0 baseline, with zero new diagnostics attributable to this cycle's changes.

## Outcome

Finding 1 (Blocking) is resolved: the file-size policy violation is eliminated, all 39 tests are preserved and passing, containment held (zero production-file diff), and the full C# toolchain passed in a single final clean pass. Findings 2, 3, and 4 remain untouched and out of scope for this cycle.
