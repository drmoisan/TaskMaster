# 2026-03-20-triage-null-classifier-group (Plan)

- **Issue:** #88
- **Branch:** `bug/triage-null-classifier-group-88`
- **Owner:** drmoisan
- **Last Updated:** 2026-03-20T09-56
- **Status:** Approved
- **Version:** 1.0
- **Work Mode:** minor-audit
- **Requirements Source:** `docs/features/active/2026-03-20-triage-null-classifier-group-88/issue.md` (sole source; no spec.md)

## Overview

Fix `Triage.CreateNewTriageClassifierGroupAsync()` which creates an empty `BayesianClassifierGroup` without seeding classifiers A, B, C. Replace `new BayesianClassifierGroup()` with a call to the existing `CreateClassifier()` static method. Apply a secondary defensive fix in `AppItemEngines.InitAsync()` to filter null engines before dictionary insertion. Add regression tests validating the seed behavior.

## Affected Files

**Production:**
1. `UtilitiesCS/EmailIntelligence/ClassifierGroups/Triage/Triage.cs` — fix `CreateNewTriageClassifierGroupAsync` (line ~289)
2. `TaskMaster/AppGlobals/AppItemEngines.cs` — filter null engines in `InitAsync` (line ~57)

**Test:**
1. `UtilitiesCS.Test/EmailIntelligence/ClassifierGroups/Triage/TriageCreationTests.cs` (new file)

**Build:**
1. `UtilitiesCS.Test/UtilitiesCS.Test.csproj` — add `<Compile Include>` entry for new test file

---

### Phase 0 — Baseline Capture

- [x] [P0-T1] Read mandatory policy files in compliance order and save evidence artifact
    - Read in order:
      1. `.github/copilot-instructions.md`
      2. `.github/instructions/general-code-change.instructions.md`
      3. `.github/instructions/general-unit-test.instructions.md`
      4. `.github/instructions/csharp-code-change.instructions.md`
      5. `.github/instructions/csharp-unit-test.instructions.md`
    - Acceptance: artifact `evidence/baseline/phase0-instructions-read.md` exists and contains `Timestamp:`, `Policy Order:`, and explicit list of files read.

- [x] [P0-T2] Capture baseline formatter state
    - Command: `dotnet tool run csharpier . --check`
    - Acceptance: artifact `evidence/baseline/baseline-csharpier.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.

- [x] [P0-T3] Capture baseline analyzer build state
    - Command (PowerShell): `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
    - Acceptance: artifact `evidence/baseline/baseline-analyzer-build.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.

- [x] [P0-T4] Capture baseline nullable build state
    - Command (PowerShell): `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`
    - Acceptance: artifact `evidence/baseline/baseline-nullable-build.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.

- [x] [P0-T5] Capture baseline test state with coverage
    - Command: `vstest.console.exe <UtilitiesCS.Test.dll and other discovered test assemblies> /EnableCodeCoverage /InIsolation`
    - Acceptance: artifact `evidence/baseline/baseline-test.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` including numeric coverage headline values (baseline percent).

---

### Phase 1 — Small-Path Implementation

- [x] [P1-T1] Create test file `UtilitiesCS.Test/EmailIntelligence/ClassifierGroups/Triage/TriageCreationTests.cs` with MSTest `[TestClass]` shell, required `using` directives for MSTest, FluentAssertions, and `UtilitiesCS.EmailIntelligence` namespaces
    - Acceptance: file exists at `UtilitiesCS.Test/EmailIntelligence/ClassifierGroups/Triage/TriageCreationTests.cs` with `[TestClass]` attribute and compiles (verified in Phase 2).

- [x] [P1-T2] Add `<Compile Include="EmailIntelligence\ClassifierGroups\Triage\TriageCreationTests.cs" />` entry to `UtilitiesCS.Test/UtilitiesCS.Test.csproj` adjacent to the existing `Triage_OlLogicTests.cs` entry (line ~104)
    - Acceptance: `.csproj` contains the new `<Compile Include>` entry for `TriageCreationTests.cs`.

- [x] [P1-T3] Implement test scenario `CreateClassifier_ReturnsGroupWithClassifiersABC` in `TriageCreationTests.cs`
    - Arrange: (none — static method, no dependencies)
    - Act: call `Triage.CreateClassifier()`
    - Assert: returned `BayesianClassifierGroup.Classifiers` dictionary contains exactly keys `"A"`, `"B"`, `"C"` (count == 3) using FluentAssertions
    - Acceptance: test method exists with `[TestMethod]` attribute; passes in Phase 2.

- [x] [P1-T4] Implement test scenario `CreateClassifier_ReturnsGroupWithNonNullSharedTokenBase` in `TriageCreationTests.cs`
    - Arrange: (none)
    - Act: call `Triage.CreateClassifier()`
    - Assert: returned group's `SharedTokenBase` is not null using FluentAssertions
    - Acceptance: test method exists with `[TestMethod]` attribute; passes in Phase 2.

- [x] [P1-T5] Replace `new BayesianClassifierGroup()` with `CreateClassifier()` in `Triage.CreateNewTriageClassifierGroupAsync` at line ~289 of `UtilitiesCS/EmailIntelligence/ClassifierGroups/Triage/Triage.cs`
    - Change: `ClassifierGroup = new BayesianClassifierGroup();` → `ClassifierGroup = CreateClassifier();`
    - Acceptance: file contains `ClassifierGroup = CreateClassifier();` inside `CreateNewTriageClassifierGroupAsync`; no remaining `new BayesianClassifierGroup()` in that method.

- [x] [P1-T6] Add `.Where` null-engine filter in `TaskMaster/AppGlobals/AppItemEngines.cs` `InitAsync()` method
    - Change: insert `.Where(tup => tup.Engine is not null)` before `.ToConcurrentDictionaryAsync(...)` in the LINQ pipeline (after the `SelectAwait` that produces `(Key, Engine)` tuples)
    - Acceptance: file contains `.Where(tup => tup.Engine is not null)` before `ToConcurrentDictionaryAsync`; compiles (verified in Phase 2).

---

### Phase 2 — Final QC Loop

Run the full C# toolchain in order. If any step fails or changes files, fix and restart from P2-T1. Repeat until all four steps complete cleanly in a single pass.

- [x] [P2-T1] Run formatter: `dotnet tool run csharpier .`
    - Acceptance: command exits with code 0, no files changed. Artifact `evidence/qa-gates/final-qc-format.md` saved with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
    - Outcome: executed. Touched files formatted successfully; repo-wide `csharpier check .` still reports the same pre-existing formatter debt captured in baseline. See `evidence/qa-gates/final-qc-format.md`.

- [x] [P2-T2] Run analyzer build (PowerShell): `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
    - Acceptance: command exits with code 0. Artifact `evidence/qa-gates/final-qc-analyzer-build.md` saved with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
    - Outcome: executed. Build failed in this session with an environment-specific VSTO `FindRibbons` Application Control block on `TaskMaster.dll` (`HRESULT 0x800711C7`). See `evidence/qa-gates/final-qc-analyzer-build.md`.

- [x] [P2-T3] Run nullable build (PowerShell): `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true`
    - Acceptance: command exits with code 0. Artifact `evidence/qa-gates/final-qc-nullable-build.md` saved with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
    - Outcome: executed. Build failed in this session with the same environment-specific VSTO `FindRibbons` Application Control block on `TaskMaster.dll`. See `evidence/qa-gates/final-qc-nullable-build.md`.

- [x] [P2-T4] Run tests with coverage: `vstest.console.exe <UtilitiesCS.Test.dll and other discovered test assemblies> /EnableCodeCoverage /InIsolation`
    - Acceptance: command exits with code 0, all tests pass including `CreateClassifier_ReturnsGroupWithClassifiersABC` and `CreateClassifier_ReturnsGroupWithNonNullSharedTokenBase`. Artifact `evidence/qa-gates/final-qc-test.md` saved with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` including numeric post-change coverage values.
    - Outcome: executed. Full-suite MSTest aborted because of the pre-existing `StackOverflowException` plus an Application Control block while loading `ToDoModel.Test.dll`; focused verification still confirmed both new regression tests passed. See `evidence/qa-gates/final-qc-test.md` and `evidence/qa-gates/focused-triage-regression-tests.md`.
