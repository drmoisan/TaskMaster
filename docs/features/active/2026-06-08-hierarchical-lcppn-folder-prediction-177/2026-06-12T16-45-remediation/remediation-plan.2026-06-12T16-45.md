# Remediation Plan (Cycle 2): hierarchical-lcppn-folder-prediction (#177)

**Cycle:** 2
**Plan timestamp:** 2026-06-12T16-45 (UTC)
**Authored by:** atomic-planner
**Base:** `main` (merge-base `742d4f1656367ddb1d43ea66e1bdd59776f1a287`)
**Single in-scope finding:** F3 (AC20) — split over-cap test file
**Feature folder:** `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177`
**Authoritative inputs:** `remediation-inputs.2026-06-12T16-45.md`

## Scope statement

This plan remediates exactly one finding: F3 (`LcppnFolderPredictor_Tests.cs` is 554 lines,
over the 500-line cap, AC20 FAIL). The remediation splits that file by behavior into the
trimmed original plus one new sibling test file, each <= 500 lines, preserving every existing
`[TestMethod]` and the cycle-1 strict coverage of `LcppnFolderPredictor` (currently 97.71%;
must stay >= 90%).

The out-of-scope items recorded in the inputs (pre-existing over-cap production files
`BayesianClassifierGroup.cs`, `FolderScorer.cs`, `SortEmail.cs`; `FolderHierarchyNode.cs`
coverage; the pre-existing flaky `IdleAsyncQueue` test) are NOT remediated here and MUST NOT
be touched.

**Containment invariant (must hold for the whole cycle):** zero diff to
`ManagerAsyncLazy.cs`, the shared `Manager` value type, `Triage.cs`, `SpamBayes.cs`,
`CategoryClassifierGroup.cs`, and `MulticlassEngine.cs`. This cycle touches only test files
and `UtilitiesCS.Test/UtilitiesCS.Test.csproj`.

## Proposed file split

Source file: `UtilitiesCS.Test/EmailIntelligence/Bayesian/LcppnFolderPredictor_Tests.cs`
(554 lines, one `[TestClass] LcppnFolderPredictor_Tests`, 21 `[TestMethod]`s incl. two
`[DataTestMethod]`, three private helpers: `Config`, `CreateTrainedPredictor`, `MinedMail`).

The split follows the established repo convention `LcppnFolderPredictor_<Concern>_Tests.cs`
(a sibling already exists: `LcppnFolderPredictor_Serialization_Tests.cs`, a distinct
`[TestClass]` with its own duplicated helper). Each resulting file is a self-contained
`[TestClass]` in namespace `UtilitiesCS.Test.EmailIntelligence.Bayesian` with its own usings
and its own copy of only the helpers it needs (no third "helpers" production file).

### File A — trimmed original: `LcppnFolderPredictor_Tests.cs` (config / validation / training / untrain / build)

`[TestClass] LcppnFolderPredictor_Tests` (class name unchanged). Helpers retained: `Config`,
`CreateTrainedPredictor`, `MinedMail` (the `Build_*` tests in this file use `MinedMail`).

Test methods (14):
- `Config_BeamWidthBelowOne_Throws`
- `Config_Defaults_MatchSpecification`
- `Config_InvalidMinimumPathProbability_Throws` (`[DataTestMethod]`, 4 rows)
- `Config_InvalidShrinkageLambda_Throws` (`[DataTestMethod]`, 2 rows)
- `Config_NegativeMinColdStartExamples_Throws`
- `Train_Leaf_UpdatesOnlyPathClassifiers`
- `UnTrain_PriorLeaf_DecrementsOnlyPathClassifiers`
- `Train_NewLeaf_ModifiesOnlyTargetParentClassifier`
- `TrainAndUnTrain_EmptyTag_AreNoOps`
- `UnTrain_IntermediateParentMissing_SkipsMissingSegment`
- `LcppnFolderPredictor_IsAssignableToIFolderPredictor`
- `Build_NullCorpus_Throws`
- `Build_SkipsEntriesWithEmptyRelativePathAndNullTokens`
- `Build_NullConfig_Throws`

Projected length: ~300-320 lines.

### File B — new sibling: `LcppnFolderPredictor_Classify_Tests.cs` (classify / beam-descent / abstention)

`[TestClass] LcppnFolderPredictor_Classify_Tests` (new distinct class name). Helpers
duplicated into this file: `Config`, `CreateTrainedPredictor` (`MinedMail` is NOT needed here
and is not duplicated).

Test methods (9):
- `Classify_ConstructedCorpus_ReturnsLeafWithPathProductProbability`
- `Classify_ConstructedCorpus_ResultsAreOrderedDescending`
- `Classify_WiderBeam_RecoversBranchGreedyWouldDiscard`
- `Classify_BelowThreshold_ReturnsEmpty`
- `Classify_NoRootChildren_ReturnsEmpty`
- `Classify_DeepWideHierarchy_TruncatesFrontierToBeamWidth`
- `Classify_FrontierNodeWithoutClassifier_EmitsTerminalLeaf`
- `Classify_FrontierNodeWithNoChildScores_EmitsTerminalLeaf`
- `Classify_FrontierExceedsBeamWidth_TrimsToBeamWidth`

Projected length: ~280-300 lines.

Total methods after split: 14 + 9 = 23 method declarations representing all 21 original
`[TestMethod]`/`[DataTestMethod]` cases (no test dropped; the two `[DataTestMethod]`s remain
in File A). Combined coverage of `LcppnFolderPredictor` is unchanged because every test moves
intact; the only change is file partitioning and helper duplication.

---

### Phase 0 — Compliance read and baseline capture

- [x] [P0-T1] Read the policy files in the required order (`CLAUDE.md`; `.claude/rules/general-code-change.md`; `.claude/rules/general-unit-test.md`; `.claude/rules/csharp.md`) and the cycle-entry inputs `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/remediation-inputs.2026-06-12T16-45.md`. Acceptance: `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/baseline/phase0-instructions-read.2026-06-12T16-45.md` exists with `Timestamp:`, `Policy Order:`, and the explicit list of files read.
- [x] [P0-T2] Capture the baseline line count of the in-scope file. Acceptance: `evidence/baseline/linecount-baseline.2026-06-12T16-45.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` recording `LcppnFolderPredictor_Tests.cs = 554 lines` (over the 500 cap).
- [x] [P0-T3] Capture baseline CSharpier formatting state. Acceptance: `evidence/baseline/csharpier-baseline.2026-06-12T16-45.md` records `Command: dotnet tool run csharpier --check .` (or `csharpier --check .`), `EXIT_CODE:`, and `Output Summary:` (clean / list of unformatted files).
- [x] [P0-T4] Capture baseline analyzer build. Acceptance: `evidence/baseline/analyzers-baseline.2026-06-12T16-45.md` records `Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`, `EXIT_CODE:`, and `Output Summary:` (build result, warning/error counts).
- [x] [P0-T5] Capture baseline nullable/type-check build. Acceptance: `evidence/baseline/nullable-baseline.2026-06-12T16-45.md` records `Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`, `EXIT_CODE:`, and `Output Summary:`. Pre-existing unrelated CS8625 in other test files are noted as out-of-scope incremental exclusions, not failures introduced by this work.
- [x] [P0-T6] Capture baseline test run with coverage for the UtilitiesCS.Test assembly. Acceptance: `evidence/baseline/test-baseline.2026-06-12T16-45.md` records `Command: vstest.console.exe <UtilitiesCS.Test assembly path> /EnableCodeCoverage`, `EXIT_CODE:`, and `Output Summary:` including the passing/total count, repository-wide line coverage headline, and the strict `LcppnFolderPredictor` module coverage (baseline 97.71%). Canonical coverage XML written to `artifacts/csharp/coverage.xml`.

**Phase 0 gate**

- [x] [P0-T7] Confirm all Phase 0 baseline artifacts exist with required schema fields (`Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`) before implementation begins. Acceptance: every artifact named in P0-T1..P0-T6 is present and schema-complete; the baseline `LcppnFolderPredictor` strict coverage value is recorded numerically.

### Phase 1 — Split the over-cap test file

- [x] [P1-T1] Register the new test file in the non-SDK project. Add `<Compile Include="EmailIntelligence\Bayesian\LcppnFolderPredictor_Classify_Tests.cs" />` to `UtilitiesCS.Test/UtilitiesCS.Test.csproj` adjacent to the existing `LcppnFolderPredictor_Tests.cs` and `LcppnFolderPredictor_Serialization_Tests.cs` `<Compile Include>` entries. Acceptance: the csproj contains exactly one `<Compile Include="EmailIntelligence\Bayesian\LcppnFolderPredictor_Classify_Tests.cs" />` entry and the existing `LcppnFolderPredictor_Tests.cs` entry is unchanged.
- [x] [P1-T2] Create `UtilitiesCS.Test/EmailIntelligence/Bayesian/LcppnFolderPredictor_Classify_Tests.cs` containing `[TestClass] public class LcppnFolderPredictor_Classify_Tests` in namespace `UtilitiesCS.Test.EmailIntelligence.Bayesian`, with the required usings, duplicated `Config` and `CreateTrainedPredictor` private helpers, and the nine `Classify_*` test methods moved verbatim (bodies and attributes byte-identical to the originals). Acceptance: the file compiles in isolation references-wise and contains exactly the nine `Classify_*` `[TestMethod]`s listed in the split design with unchanged bodies.
- [x] [P1-T3] Trim `UtilitiesCS.Test/EmailIntelligence/Bayesian/LcppnFolderPredictor_Tests.cs` by removing the nine `Classify_*` test methods that moved to File B, retaining `[TestClass] LcppnFolderPredictor_Tests`, all usings still needed, the `Config`/`CreateTrainedPredictor`/`MinedMail` helpers, and the 14 remaining test methods unchanged. Acceptance: the file contains exactly the 14 config/validation/training/untrain/build `[TestMethod]`/`[DataTestMethod]`s from the split design and zero `Classify_*` methods.
- [x] [P1-T4] Remove any now-unused `using` directives from File A and confirm File B carries only the usings it requires. Acceptance: neither file declares an unused `using` (no IDE0005/CS unused-using diagnostic introduced) and both files retain `Microsoft.VisualStudio.TestTools.UnitTesting`, `FluentAssertions`, and `UtilitiesCS.EmailIntelligence.Bayesian`.

**Phase 1 verification (file-split correctness)**

- [x] [P1-T5] Verify the cap and test-preservation invariants. Acceptance: `evidence/qa-gates/split-verification.2026-06-12T16-45.md` records, with `Timestamp:`/`Command:`/`EXIT_CODE:`/`Output Summary:`: (a) line count of each resulting file is <= 500 (`LcppnFolderPredictor_Tests.cs` and `LcppnFolderPredictor_Classify_Tests.cs`); (b) the union of `[TestMethod]`/`[DataTestMethod]` names across both files equals the original 21-case set (no test dropped, none renamed); (c) no `Classify_*` method appears in File A and no non-`Classify_*` method appears in File B.

**Phase 1 toolchain gate (full C# loop, restart-from-CSharpier on any failure)**

- [x] [P1-T6] Run CSharpier formatting. Acceptance: `evidence/qa-gates/p1-csharpier.2026-06-12T16-45.md` records `Command: dotnet tool run csharpier .` (or `csharpier .`), `EXIT_CODE: 0`, and `Output Summary:` (no files reformatted on the final pass). If files were reformatted, restart this gate from this task.
- [x] [P1-T7] Run the analyzer build. Acceptance: `evidence/qa-gates/p1-analyzers.2026-06-12T16-45.md` records `Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`, `EXIT_CODE: 0`, and `Output Summary:` (no new analyzer errors vs. baseline). On failure, fix and restart from P1-T6.
- [x] [P1-T8] Run the nullable/type-check build (incremental gate convention). Acceptance: `evidence/qa-gates/p1-nullable.2026-06-12T16-45.md` records `Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`, `EXIT_CODE: 0` for touched files, and `Output Summary:` confirming no new nullable warnings in the two split files (pre-existing unrelated CS8625 in other files remain out of scope). On failure, fix and restart from P1-T6.
- [x] [P1-T9] Run the test suite with coverage. Acceptance: `evidence/qa-gates/p1-test.2026-06-12T16-45.md` records `Command: vstest.console.exe <UtilitiesCS.Test assembly path> /EnableCodeCoverage`, `EXIT_CODE: 0`, and `Output Summary:` with passing/total count (>= baseline count) and the strict `LcppnFolderPredictor` module coverage value. Canonical coverage XML written to `artifacts/csharp/coverage.xml`. On failure or file change, restart from P1-T6.

### Phase 2 — Final QA loop and coverage/containment verification

- [x] [P2-T1] Run the full C# toolchain in one clean final pass in order CSharpier -> analyzers msbuild -> nullable msbuild -> vstest `/EnableCodeCoverage`, restarting from CSharpier if any step changes files or fails. Acceptance: `evidence/qa-gates/final-toolchain.2026-06-12T16-45.md` records all four `Command:`/`EXIT_CODE: 0`/`Output Summary:` entries for a single uninterrupted clean pass. No `SKIPPED` outcomes.
- [x] [P2-T2] Verify the coverage no-regression and threshold criteria. Acceptance: `evidence/qa-gates/coverage-delta.2026-06-12T16-45.md` reports baseline `LcppnFolderPredictor` strict coverage (97.71%), post-change strict coverage, and confirms post-change >= 90% with no regression on changed lines; repository-wide line coverage remains >= 80%. Sourced from `artifacts/csharp/coverage.xml`.
- [x] [P2-T3] Verify containment. Acceptance: `evidence/qa-gates/containment.2026-06-12T16-45.md` records `Command: git diff --stat <merge-base>..HEAD` (or equivalent) and `Output Summary:` confirming zero diff to `ManagerAsyncLazy.cs`, the shared `Manager` value type, `Triage.cs`, `SpamBayes.cs`, `CategoryClassifierGroup.cs`, `MulticlassEngine.cs`, and that the only changed files are `UtilitiesCS.Test/EmailIntelligence/Bayesian/LcppnFolderPredictor_Tests.cs`, the new `LcppnFolderPredictor_Classify_Tests.cs`, and `UtilitiesCS.Test/UtilitiesCS.Test.csproj`.
- [x] [P2-T4] Record the cycle-2 end-state summary. Acceptance: `evidence/qa-gates/cycle2-endstate.2026-06-12T16-45.md` records that both resulting test files are <= 500 lines, all 21 test cases preserved, `LcppnFolderPredictor` strict coverage >= 90%, containment held, and the full toolchain green in a single final pass (links the P2-T1..P2-T3 artifacts).
