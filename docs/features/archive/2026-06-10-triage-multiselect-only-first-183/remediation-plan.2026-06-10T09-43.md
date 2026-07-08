# Remediation Plan: Triage_OlLogicTests file-size split (Issue #183, R1)

**Generated:** 2026-06-10T09-43
**Feature Folder:** `docs/features/active/2026-06-10-triage-multiselect-only-first-183`
**Work Mode:** `minor-audit` (remediation cycle 1)
**Cycle Inputs:** `docs/features/active/2026-06-10-triage-multiselect-only-first-183/remediation-inputs.2026-06-10T09-43.md`

## Objective

Resolve blocking remediation finding R1: the test file
`UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\Triage\Triage_OlLogicTests.cs`
is 553 lines, exceeding the repository 500-line file-size limit (General Code Change Policy
"File Size Limit"; test code is not an excepted file type). Bring the file under the limit by
splitting the existing fixture into a `partial` class across two files in the same folder,
without weakening, renaming, or removing any of the 21 existing test methods and without
changing any production code.

## Scope and Constraints

- **Test-organization change only.** No change to `Triage_OlLogic.cs` or any other production file.
- Preserve all 21 existing test methods and their assertions verbatim; no weakening, renaming, or removal.
- Convert `Triage_OlLogicTests` (namespace `UtilitiesCS.Test.EmailIntelligence`) to a `partial` class.
- Keep `[TestInitialize] Setup`, the fields `_mockGlobals`, `_triage`, `_triageOlLogic`, and the full
  `using` directive block in the original file. Replicate only the `using` directives required by the
  moved methods in the new file.
- Move the six `TrainSelectionAsync_*` test methods (the cohesive #137/#183 training group) into a new
  sibling file `Triage_OlLogicTests.TrainSelection.cs` declared as `public partial class Triage_OlLogicTests`
  in the same namespace and folder. This split yields two files each well under 500 lines (15 `[TestMethod]` methods remain
  in the original file; 6 move to the new file; total preserved at 21).
- The `UtilitiesCS.Test.csproj` project is a non-SDK project that uses explicit `<Compile Include=...>`
  items (the target file is referenced at line 129). The new file MUST be added with an explicit
  `<Compile Include=...>` entry.

## Exact File List (this remediation)

1. `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\Triage\Triage_OlLogicTests.cs` (edit: add `partial`, remove moved methods)
2. `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\Triage\Triage_OlLogicTests.TrainSelection.cs` (new)
3. `UtilitiesCS.Test\UtilitiesCS.Test.csproj` (edit: add `<Compile Include>` for the new file)

## Acceptance Criteria (this cycle)

- AC-R1.1: Every resulting test file (`Triage_OlLogicTests.cs` and `Triage_OlLogicTests.TrainSelection.cs`) is < 500 lines.
- AC-R1.2: All 21 existing test methods (including the #183 regression test) still compile and pass; method names and assertions unchanged.
- AC-R1.3: First-party C# toolchain produces a single clean pass (CSharpier, analyzer build, nullable/TWAE build, MSTest with coverage).
- AC-R1.4: No production file is modified.
- AC-R1.5: No assertion is weakened, renamed, or removed.

---

### Phase 0 — Context and Baseline Capture

- [x] [P0-T1] Read policy files in required order and record a Phase 0 evidence artifact at `docs/features/active/2026-06-10-triage-multiselect-only-first-183/evidence/remediation-baseline/phase0-instructions-read.2026-06-10T09-43.md`. The artifact MUST list, in order: `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`, and include `Timestamp:` and `Policy Order:` fields.
- [x] [P0-T2] Record the current line count of `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\Triage\Triage_OlLogicTests.cs` by running `(Get-Content 'UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\Triage\Triage_OlLogicTests.cs').Length`. Write the result to `docs/features/active/2026-06-10-triage-multiselect-only-first-183/evidence/remediation-baseline/line-counts-baseline.2026-06-10T09-43.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` (expected: 553 lines, over the 500-line limit).
- [x] [P0-T3] Confirm the `UtilitiesCS.Test.csproj` include mechanism by inspecting the file: verify it contains explicit `<Compile Include="EmailIntelligence\ClassifierGroups\Triage\Triage_OlLogicTests.cs" />` (line 129) and uses no wildcard glob. Record the finding (explicit-include vs glob) and the exact anchor line in `docs/features/active/2026-06-10-triage-multiselect-only-first-183/evidence/remediation-baseline/csproj-include-mechanism.2026-06-10T09-43.md` with `Timestamp:` and `Output Summary:`.
- [x] [P0-T4] Enumerate the existing `[TestMethod]`-decorated test methods in `Triage_OlLogicTests.cs` via `Select-String -Path 'UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\Triage\Triage_OlLogicTests.cs' -Pattern '\[TestMethod\]'` and record the full inventory to `docs/features/active/2026-06-10-triage-multiselect-only-first-183/evidence/remediation-baseline/test-method-inventory.2026-06-10T09-43.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` (expected count: 21 `[TestMethod]` methods (the `[TestInitialize] Setup()` method is excluded — it is not a test method)). This inventory is the verbatim-preservation reference for Phase 2.

### Phase 1 — Partial-Class Split

- [x] [P1-T1] In `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\Triage\Triage_OlLogicTests.cs`, change the class declaration from `public class Triage_OlLogicTests` to `public partial class Triage_OlLogicTests`. No other change in this task.
- [x] [P1-T2] Create the new file `UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\Triage\Triage_OlLogicTests.TrainSelection.cs` with namespace `UtilitiesCS.Test.EmailIntelligence`, declaration `public partial class Triage_OlLogicTests`, and the `using` directives required by the moved methods: `System`, `System.Collections`, `System.Collections.Generic`, `System.Threading`, `System.Threading.Tasks`, `FluentAssertions`, `Microsoft.Office.Interop.Outlook`, `Microsoft.VisualStudio.TestTools.UnitTesting`, `Moq`, `UtilitiesCS.EmailIntelligence`, `UtilitiesCS.EmailIntelligence.ClassifierGroups`. The class body in the new file MUST NOT redeclare `Setup`, `_mockGlobals`, `_triage`, or `_triageOlLogic` (those remain in the original file and are shared via the partial class).
- [x] [P1-T3] Move the following six test methods verbatim (signatures, bodies, comments, and assertions unchanged) from `Triage_OlLogicTests.cs` into `Triage_OlLogicTests.TrainSelection.cs`: `TrainSelectionAsync_ShouldTrainSelection`, `TrainSelectionAsync_WhenSelectionIsNull_SkipsWithoutThrowingOrTraining`, `TrainSelectionAsync_WhenSelectionContainsMailItem_TrainsClassifierWithExpectedLabel`, `TrainSelectionAsync_WhenSelectionContainsTwoMailItemsWithSameConversationId_TrainsOnlyOneItem_TotalEmailCountIncrementsOnce`, `TrainSelectionAsync_WhenSelectionContainsTwoMailItemsWithSameConversationId_TrainsOnlyOneItem_MatchEmailCountIncrementsOnce`, and `TrainSelectionAsync_WhenSelectionContainsTwoMailItemsWithSameConversationId_WritesTriageUdfToEveryItem`. Remove these method bodies from the original file. (Note: 6 `TrainSelectionAsync_*` methods move; 15 `[TestMethod]` methods remain in the original file; `Setup()` remains in the original file and is not counted as a test method (6 + 15 = 21).)
- [x] [P1-T4] Add `<Compile Include="EmailIntelligence\ClassifierGroups\Triage\Triage_OlLogicTests.TrainSelection.cs" />` to `UtilitiesCS.Test\UtilitiesCS.Test.csproj` immediately after the existing `<Compile Include="EmailIntelligence\ClassifierGroups\Triage\Triage_OlLogicTests.cs" />` entry (line 129).
- [x] [P1-T5] Verify total method preservation: run `Select-String` for `[TestMethod]` across BOTH `Triage_OlLogicTests.cs` and `Triage_OlLogicTests.TrainSelection.cs` and confirm the combined `[TestMethod]` count across both partial files is exactly 21, matching the corrected Phase 0 inventory (P0-T4). Record the combined inventory at `docs/features/active/2026-06-10-triage-multiselect-only-first-183/evidence/qa-gates/test-method-inventory-postsplit.2026-06-10T09-43.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.

### Phase 2 — Final QA Verification Loop

Run the full C# toolchain in this exact order. If any step changes files or fails, fix and restart from P2-T1.

- [x] [P2-T1] Format: run `dotnet tool run csharpier .` (or `csharpier .`). Record output to `docs/features/active/2026-06-10-triage-multiselect-only-first-183/evidence/qa-gates/remediation-csharpier.2026-06-10T09-43.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. If CSharpier reformats any file, restart from P2-T1.
- [x] [P2-T2] Lint / analyzer build: run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`. Record output to `docs/features/active/2026-06-10-triage-multiselect-only-first-183/evidence/qa-gates/remediation-analyzer-build.2026-06-10T09-43.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. On failure, fix and restart from P2-T1.
- [x] [P2-T3] Type-check / nullable build: run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`. Record output to `docs/features/active/2026-06-10-triage-multiselect-only-first-183/evidence/qa-gates/remediation-nullable-build.2026-06-10T09-43.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. On failure, fix and restart from P2-T1.
- [x] [P2-T4] Test with coverage: run `vstest.console.exe <UtilitiesCS.Test build output assembly path> /EnableCodeCoverage`. Record output to `docs/features/active/2026-06-10-triage-multiselect-only-first-183/evidence/qa-gates/remediation-tests-coverage.2026-06-10T09-43.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` including the numeric passed/failed counts and the coverage headline percent. The summary MUST confirm all 21 `Triage_OlLogicTests` test methods pass (the pre-existing unrelated `AddEntry_UseUiThreadTrue_*` failure noted in remediation-inputs remains out of scope and must be identified as the only allowed pre-existing failure). On failure of any in-scope test, fix and restart from P2-T1.
- [x] [P2-T5] Coverage no-regression check: compare the post-remediation coverage headline (P2-T4) against the baseline coverage in `docs/features/active/2026-06-10-triage-multiselect-only-first-183/evidence/baseline/tests-coverage.2026-06-10T09-13.md`. Confirm repository-wide line coverage remains >= 80% and the changed test-organization split did not reduce coverage. Record the comparison at `docs/features/active/2026-06-10-triage-multiselect-only-first-183/evidence/qa-gates/remediation-coverage-comparison.2026-06-10T09-43.md` with `Timestamp:`, baseline percent, post-change percent, and verdict.
- [x] [P2-T6] Post-split line-count verification: run `(Get-Content 'UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\Triage\Triage_OlLogicTests.cs').Length` and `(Get-Content 'UtilitiesCS.Test\EmailIntelligence\ClassifierGroups\Triage\Triage_OlLogicTests.TrainSelection.cs').Length`. Confirm each result is < 500. Record both counts at `docs/features/active/2026-06-10-triage-multiselect-only-first-183/evidence/qa-gates/line-counts-postsplit.2026-06-10T09-43.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` stating both line counts and the < 500 verdict for each file.
- [x] [P2-T7] Production-change guard: run `git diff --name-only <merge-base>..HEAD` (or `git status --porcelain`) and confirm the only modified/added files in this remediation are the three files in the Exact File List (`Triage_OlLogicTests.cs`, `Triage_OlLogicTests.TrainSelection.cs`, `UtilitiesCS.Test.csproj`). Confirm `UtilitiesCS\...\Triage_OlLogic.cs` and all other production files are unchanged. Record the file list at `docs/features/active/2026-06-10-triage-multiselect-only-first-183/evidence/qa-gates/remediation-changed-files.2026-06-10T09-43.md` with `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`.

## Evidence Artifact Index

Baseline (this cycle):
- `evidence/remediation-baseline/phase0-instructions-read.2026-06-10T09-43.md`
- `evidence/remediation-baseline/line-counts-baseline.2026-06-10T09-43.md`
- `evidence/remediation-baseline/csproj-include-mechanism.2026-06-10T09-43.md`
- `evidence/remediation-baseline/test-method-inventory.2026-06-10T09-43.md`

QA gates (this cycle):
- `evidence/qa-gates/test-method-inventory-postsplit.2026-06-10T09-43.md`
- `evidence/qa-gates/remediation-csharpier.2026-06-10T09-43.md`
- `evidence/qa-gates/remediation-analyzer-build.2026-06-10T09-43.md`
- `evidence/qa-gates/remediation-nullable-build.2026-06-10T09-43.md`
- `evidence/qa-gates/remediation-tests-coverage.2026-06-10T09-43.md`
- `evidence/qa-gates/remediation-coverage-comparison.2026-06-10T09-43.md`
- `evidence/qa-gates/line-counts-postsplit.2026-06-10T09-43.md`
- `evidence/qa-gates/remediation-changed-files.2026-06-10T09-43.md`

All evidence paths resolve to `docs/features/active/2026-06-10-triage-multiselect-only-first-183/evidence/<kind>/`, consistent with the existing flat layout and the canonical evidence-location scheme. No `artifacts/` evidence paths are used.
