# folder-combobox-fallback-index-out-of-range (Plan)

- **Issue:** #392
- **Issue URL:** https://github.com/drmoisan/TaskMaster/issues/392
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-07-20T12-59
- **Status:** Draft
- **Version:** 1.0
- **Work Mode:** minor-audit
- **Requirements Source:** `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/issue.md` (`## Acceptance Criteria`, AC-1..AC-5)
- **Feature folder (`<FEATURE>`):** `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392`
- **Timestamp token:** every `<TS>` placeholder below MUST be substituted with the real ISO-8601
  timestamp (`yyyy-MM-ddTHH-mm`) at the moment the artifact is written, per
  `evidence-and-timestamp-conventions`.

**Fail-closed evidence rule:** Include explicit baseline artifact tasks, final-QA artifact tasks,
and coverage-comparison tasks for each in-scope language when policy requires coverage. If any
required baseline artifact, QA artifact, or coverage-comparison artifact is missing, the audit
verdict must be BLOCKED or INCOMPLETE, never PASS.

**Evidence accounting rule:** Record the expected artifact path or location in each
evidence-producing task. Do not mark evidence-backed work complete without the artifact.

## Requirements Boundary

This minor-audit plan uses only
`docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/issue.md` as the
requirements source. Acceptance criteria are limited to the checkbox items (AC-1..AC-5) under that
file's explicit `## Acceptance Criteria` section (lines 74-80). `spec.md` and `user-story.md` are
not required for minor-audit mode; confirmed absent from the feature folder at plan time (only
`issue.md` and this plan file exist there). If either appears later, that is a fail-closed
condition and must be reported, not silently ignored.

All evidence must be written under
`docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/<kind>/`.
No non-canonical path (e.g. `artifacts/baselines/`, `artifacts/qa/`, `artifacts/coverage/`,
`artifacts/evidence/`) is used for evidence anywhere in this plan. The one exception is the
repo-wide feature-review coverage-gate **input** at `artifacts/csharp/coverage.xml`, which is a
documented non-evidence tooling path (see Correction note below), not a duplicate/alternate
evidence location.

## Correction to Upstream Delegation Instructions (format only, not evidence path)

The delegation prompt instructed generating `artifacts/csharp/coverage.xml` in **Cobertura**
format. That is corrected here: `.claude/hooks/validate-feature-review-coverage.ps1` parses
`artifacts/csharp/coverage.xml` as **JaCoCo** XML (`//counter[@type="LINE"]`,
`//counter[@type="BRANCH"]`), not Cobertura. Producing Cobertura at that path would be silently
misread as zero coverage by the feature-review gate. Phase 2 therefore plans a Cobertura→JaCoCo
**conversion** (mirroring the established pattern used in `2026-07-16-quickfiler-breadcrumb-webview2-351`,
evidence `evidence/qa-gates/coverage-conversion.2026-07-18T10-55.md`), not a raw copy of the vstest
Cobertura output. `artifacts/csharp/` itself is not a forbidden evidence path — it is the gate's
tooling-input path, distinct from `<FEATURE>/evidence/<kind>/` outputs — so no
`EVIDENCE_LOCATION_OVERRIDE_REJECTED` applies to this path; only the stated file **format** is
corrected.

## Coverage Floor Used By This Plan

`issue.md` AC-5 explicitly states the applicable target for this feature: "new/changed code meets
the >= 90% coverage target" as part of the full C# toolchain pass. This plan uses that explicit
AC-5 figure for new/changed-code coverage. (Note for the record, not a plan blocker: this repo
currently carries an unresolved wider conflict between CLAUDE.md's repo-wide 80%/new-code 90%
coverage floor and `.claude/rules/general-unit-test.md`'s uniform 85%/75% line/branch floor with no
tier exception — see prior flag in atomic-planner memory. AC-5's explicit 90% new-code figure is
unambiguous for this plan's scope and is used as-is; the wider repo-floor conflict is out of scope
for this minimal-audit bugfix and is not resolved here.)

## Confirmed Facts (from source inspection, recorded for the Phase 1 diagnosis task)

- `QuickFiler/Controllers/QfcItemController.FolderHandling.cs:161-206` — `AssignFolderComboBox()`:
  when `_folderHandler.FolderArray.Length > 0` (line 170) and no predetermined-folder match is
  present/selected, the `else` branch (lines 200-203) unconditionally calls
  `_itemViewer.SetFolderSelectedIndex(1)` (line 202) regardless of `FolderArray.Length`.
- `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs:127` — `SelectRow(int index)` forwards the
  index unchanged into `BreadcrumbStateModel.SelectRow`.
- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs:233-246` — `SelectRow(int index)`
  validates `index` is `-1` or in `[0, RowCount-1]` and throws `ArgumentOutOfRangeException` for a
  single-row model (`RowCount == 1`) when `index == 1`. This validation is correct defensive
  behavior and must NOT be changed by this plan.
- `QuickFiler/Controllers/QfcItemController.FolderHandling.cs:217-230` — the retained static helper
  `PopulateAndSelectFolder(ComboBox, string[], string)`: line 228,
  `comboBox.SelectedIndex = predeterminedIndex >= 0 ? predeterminedIndex : 1;`, has the identical
  unguarded index-1 fallback; a WinForms `ComboBox` populated with exactly one item throws
  `ArgumentOutOfRangeException` when `SelectedIndex` is set to `1` (confirmed by the existing test
  `PopulateAndSelectFolder_EmptyArray_ThrowsOnIndexOneSelection` in
  `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs:72-90`, which demonstrates
  the same out-of-range throw for zero items).
- `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs` is the established test
  home for both surfaces: it already contains `[TestClass] QfcItemController_FolderHandlingTests`
  with `PopulateAndSelectFolder_*` tests (real `ComboBox`, no mocking) and
  `AssignFolderComboBox_*` tests (`Mock<IItemViewer>` via the `FolderController` test subclass, the
  `BuildFolderHandlerWithArray(params string[])` helper, and the private `SetPrivate` reflection
  helper). This file is already `<Compile Include>`-wired in
  `QuickFiler.Test/QuickFiler.Test.csproj:99`; no csproj change is required for this plan's new
  tests.
- `AssignFolderComboBox`'s `_itemViewer` is a `Mock<IItemViewer>` in existing tests, so the real
  `BreadcrumbStateModel` bounds check is not exercised through that seam; the true
  `ArgumentOutOfRangeException` repro is only directly observable through the pure, unmocked
  `PopulateAndSelectFolder` static helper. The `AssignFolderComboBox` regression test therefore
  verifies the corrected mock interaction (`SetFolderSelectedIndex(0)` instead of `(1)`) rather than
  an exception, which is sufficient to prove AC-2 at the controller-call level.

## Scope-Lock — files this plan authorizes changing

Production (modify only; no new production files):
- `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` (233 lines pre-change; stays well
  under the 500-line limit after the two small conditional edits)

Tests (modify only; file already `<Compile Include>`-wired, no csproj change required):
- `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs` (481 lines pre-change;
  verify it remains <= 500 lines after the two new test methods are added — see P1-T2/P1-T3
  acceptance criteria)

No other file may be changed by this plan. No `spec.md`/`user-story.md` edits. No
`BreadcrumbStateModel`, `BreadcrumbBridgeCoordinator`, or other UtilitiesCS file may be touched.

---

### Phase 0 — Baseline Capture

- [x] [P0-T1] Read `CLAUDE.md` in full (policy reading order position 1).
  - Acceptance: file read in this session; its Policy Compliance Order section is quoted verbatim
    in the P0-T5 evidence artifact.

- [x] [P0-T2] Read `.claude/rules/general-code-change.md` (policy reading order position 2).
  - Acceptance: file read; its Mandatory Toolchain Loop section quoted in the P0-T5 evidence
    artifact.

- [x] [P0-T3] Read `.claude/rules/general-unit-test.md` (policy reading order position 3).
  - Acceptance: file read; its Coverage Requirements section quoted in the P0-T5 evidence artifact.

- [x] [P0-T4] Read `.claude/rules/csharp.md` (policy reading order positions 4-5, C# Code Change
  Policy and C# Unit Test Policy consolidated).
  - Acceptance: file read; its Toolchain and Testing Standards sections quoted in the P0-T5
    evidence artifact.

- [x] [P0-T5] Write the Phase 0 policy-read evidence artifact to
  `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/baseline/phase0-instructions-read.md`.
  - Acceptance: file exists and contains `Timestamp:`, `Policy Order:` (the exact ordered list
    "CLAUDE.md (all sections) → General Code Change Policy → General Unit Test Policy → C# Code
    Change Policy → C# Unit Test Policy"), and an explicit list of the four files read in P0-T1
    through P0-T4, in order.

- [x] [P0-T6] Verify the minor-audit requirements boundary for issue #392.
  - Files: `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/issue.md`
    (and confirm absence of `spec.md`/`user-story.md` in the same folder).
  - Evidence: `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/baseline/minor-audit-scope.<TS>.md`.
  - Acceptance: evidence confirms `issue.md` contains `- Work Mode: minor-audit`, contains an
    explicit `## Acceptance Criteria` section listing AC-1..AC-5, treats only that section as the
    AC source, and records that `spec.md`/`user-story.md` are absent from the feature folder
    (fail-closed if either is unexpectedly present).

- [x] [P0-T7] Record baseline git state (current branch name and `HEAD` short SHA via
  `git rev-parse --abbrev-ref HEAD` and `git rev-parse --short HEAD`).
  - Evidence: `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/baseline/git-baseline-state.<TS>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`
    stating the branch name and SHA.

- [x] [P0-T8] Record candidate-defect-surface baseline notes citing the Confirmed Facts file:line
  list above (capture only, no new conclusion — the diagnosis conclusion is P1-T1's job).
  - Files: `QuickFiler/Controllers/QfcItemController.FolderHandling.cs`,
    `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs`,
    `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs`.
  - Evidence: `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/baseline/candidate-defect-surface.<TS>.md`.
  - Acceptance: artifact lists, verbatim, the file:line citations from this plan's Confirmed Facts
    section for the two unguarded index-1 fallback sites and the validating
    `BreadcrumbStateModel.SelectRow` bounds check.

- [x] [P0-T9] Run the baseline C# formatting command.
  - Command: `dotnet tool run csharpier --check .`
  - Evidence: `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/baseline/csharpier-baseline.<TS>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`
    stating whether any files are currently unformatted.

- [x] [P0-T10] Run the baseline C# analyzer build command.
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Evidence: `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/baseline/analyzer-baseline.<TS>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` with
    the warning/error count.

- [x] [P0-T11] Run the baseline C# nullable build command.
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  - Evidence: `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/baseline/nullable-baseline.<TS>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` with
    the warning/error count.

- [x] [P0-T12] Run the baseline MSTest coverage command for `QuickFiler.Test`.
  - Command: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage`
  - Evidence: `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/baseline/vstest-coverage-baseline.<TS>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` with
    total tests, pass/fail counts, and the numeric baseline line-coverage percentage for
    `QuickFiler.dll`.

---

### Phase 1 — Constrained Small-Path Implementation

Delegated to the C# small-path implementation engineer via `atomic-executor`. Follows the repo
Bugfix Workflow: (a) diagnose and document root cause, (b) author failing regression tests first,
(c) implement the minimal targeted fix in both affected call sites, (d) verify no regression, (e)
check off satisfied AC items in `issue.md`.

- [x] [P1-T1] Diagnose and document the confirmed root cause.
  - Files: `QuickFiler/Controllers/QfcItemController.FolderHandling.cs`,
    `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs`,
    `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs`.
  - Evidence: `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/other/root-cause-392.<TS>.md`.
  - Acceptance: evidence cites file:line for (a) `AssignFolderComboBox()`'s unconditional
    `SetFolderSelectedIndex(1)` at `QfcItemController.FolderHandling.cs:202`; (b) the identical
    unguarded fallback in the static `PopulateAndSelectFolder` helper at
    `QfcItemController.FolderHandling.cs:228`; (c) confirmation that
    `BreadcrumbStateModel.SelectRow` (`BreadcrumbStateModel.cs:233-246`) is correct defensive
    validation and is explicitly NOT to be modified; (d) a single confirmed primary root-cause
    statement: both call sites must clamp the fallback index to `0` when exactly one suggestion
    exists and to `1` only when two or more suggestions exist. This satisfies the diagnosis
    prerequisite for AC-1/AC-2/AC-4.

- [x] [P1-T2] [expect-fail] Author a failing regression test
  `PopulateAndSelectFolder_SingleItemNoPredeterminedMatch_SelectsIndexZeroWithoutThrowing` in
  `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs`, alongside the existing
  `PopulateAndSelectFolder_*` tests.
  - Test body: use a real `System.Windows.Forms.ComboBox` (mirroring
    `PopulateAndSelectFolder_AllMissingPredetermined_SelectsIndexOne`), with
    `folders = new[] { @"\\A\only" }` and `predeterminedFolder: null`. Assert
    `Action act = () => QfcItemController.PopulateAndSelectFolder(comboBox, folders, null);
    act.Should().NotThrow<ArgumentOutOfRangeException>();` then assert
    `comboBox.SelectedIndex.Should().Be(0)` and the returned selected string equals
    `@"\\A\only"`. No live Outlook process, no temporary files.
  - Precondition: P1-T1 complete.
  - Acceptance: new test method exists in the file; file remains <= 500 lines after the addition.

- [x] [P1-T3] [expect-fail] Author a failing regression test
  `AssignFolderComboBox_WhenSingleSuggestionNoPredeterminedMatch_SelectsIndexZero` in
  `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs`, alongside the existing
  `AssignFolderComboBox_*` tests.
  - Test body: mirror `AssignFolderComboBox_WhenNoPredeterminedFolder_SelectsTopSuggestionViaViewer`
    exactly (same `Mock<IItemViewer>` setup, same `FolderController` subclass, same
    `BuildFolderHandlerWithArray` helper) but seed the handler with exactly one folder
    (`BuildFolderHandlerWithArray(@"\\A\only")`) and no predetermined folder. Assert
    `mock.Verify(v => v.SetFolderSelectedIndex(0), Times.Once())` and
    `mock.Verify(v => v.SetFolderSelectedIndex(1), Times.Never())`, and
    `controller.SelectedFolder.Should().Be(@"\\A\only")` (with `mock.Setup(v =>
    v.GetSelectedFolder()).Returns(@"\\A\only")`).
  - Precondition: P1-T1 complete.
  - Acceptance: new test method exists in the file; file remains <= 500 lines after the addition
    (verify explicitly; if the two new tests push the file over 500 lines, this is a blocking
    condition to resolve within this same file — no new file/partial split is authorized by this
    plan's Scope-Lock).

- [x] [P1-T4] [expect-fail] Run both new regression tests via `vstest.console.exe` and confirm both
  currently fail (fail-before evidence), before any production-code change.
  - Precondition: P1-T2 and P1-T3 complete; no production code changed yet.
  - Command: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /TestCaseFilter:"FullyQualifiedName~PopulateAndSelectFolder_SingleItemNoPredeterminedMatch_SelectsIndexZeroWithoutThrowing|FullyQualifiedName~AssignFolderComboBox_WhenSingleSuggestionNoPredeterminedMatch_SelectsIndexZero"`
  - Evidence: `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/regression-testing/fail-before-392.<TS>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, a non-zero `EXIT_CODE:`, and
    `Output Summary:` stating `2 failed` (the `PopulateAndSelectFolder` test failing with an
    unhandled `ArgumentOutOfRangeException`, the `AssignFolderComboBox` test failing on the
    `SetFolderSelectedIndex(0)` verify). This satisfies AC-1's fail-before requirement.

- [x] [P1-T5] Implement the minimal fix in `AssignFolderComboBox()`'s `else` branch in
  `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` (line 202): replace the
  unconditional `_itemViewer.SetFolderSelectedIndex(1);` with a bounds-safe selection that calls
  `SetFolderSelectedIndex(0)` when `_folderHandler.FolderArray.Length == 1` and
  `SetFolderSelectedIndex(1)` when `_folderHandler.FolderArray.Length > 1` (the enclosing
  `_folderHandler?.FolderArray?.Length > 0` guard at line 170 already excludes the empty-array
  case; do not restructure that guard).
  - Precondition: P1-T4 confirmed both new tests failing.
  - Acceptance: exactly one conditional expression changed inside the `else` branch at line
    ~200-203; no other line in `AssignFolderComboBox()` changes; `git diff` for
    `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` shows only this substitution plus
    the P1-T6 change below. Satisfies AC-2.

- [x] [P1-T6] Implement the mirrored minimal fix in the static `PopulateAndSelectFolder(...)`
  helper in `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` (line 228): replace
  `comboBox.SelectedIndex = predeterminedIndex >= 0 ? predeterminedIndex : 1;` with a bounds-safe
  fallback that selects `0` when `folderArray.Length == 1` and `1` when `folderArray.Length > 1`,
  preserving the existing predetermined-folder preselection (`predeterminedIndex >= 0`) unchanged.
  - Precondition: P1-T4 confirmed both new tests failing.
  - Acceptance: exactly one line changed; `git diff` for
    `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` for this method shows only this
    substitution; the empty-array case (`folderArray.Length == 0`, no predetermined match) is
    unchanged and continues to throw via the real WinForms `ComboBox` (no behavior change is
    required or authorized for that pre-existing, separately-tested case). Satisfies AC-4.

- [x] [P1-T7] Re-run the two new regression tests from P1-T2/P1-T3 alone and confirm both now pass.
  - Precondition: P1-T5 and P1-T6 complete.
  - Command: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /TestCaseFilter:"FullyQualifiedName~PopulateAndSelectFolder_SingleItemNoPredeterminedMatch_SelectsIndexZeroWithoutThrowing|FullyQualifiedName~AssignFolderComboBox_WhenSingleSuggestionNoPredeterminedMatch_SelectsIndexZero"`
  - Evidence: `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/regression-testing/pass-after-392.<TS>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:`
    stating `2 passed, 0 failed`. Satisfies AC-1's pass-after requirement, AC-2, and AC-4.

- [x] [P1-T8] Run the targeted regression suite for the existing multi-suggestion and
  predetermined-folder-preselect tests to confirm no regression.
  - Command: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /TestCaseFilter:"FullyQualifiedName~PopulateAndSelectFolder_ExactMatchAtIndexZero|FullyQualifiedName~PopulateAndSelectFolder_AllMissingPredetermined|FullyQualifiedName~PopulateAndSelectFolder_EmptyArray|FullyQualifiedName~AssignFolderComboBox_WhenNoPredeterminedFolder|FullyQualifiedName~AssignFolderComboBox_WhenPredeterminedFolderPresent|FullyQualifiedName~AssignFolderComboBox_WhenFolderHandlerNull"`
  - Evidence: `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/regression-testing/targeted-no-regression-392.<TS>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:`
    listing pass counts per test name, confirming the pre-existing multi-suggestion (index-1),
    predetermined-preselect, empty-array-throws, and null-handler-guard tests all still pass
    unchanged. Satisfies AC-3.

- [x] [P1-T9] Check off satisfied AC items (AC-1 through AC-4) in `issue.md`'s
  `## Acceptance Criteria` section per `acceptance-criteria-tracking`, citing the Phase 1 evidence
  artifacts backing each.
  - Files: `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/issue.md`.
  - Evidence mirror: `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/issue-updates/ac-status-phase1-392.<TS>.md`.
  - Acceptance: only AC-1 through AC-4 under `## Acceptance Criteria` are changed from `- [ ]` to
    `- [x]`, each backed by the corresponding Phase 1 evidence artifact path; AC-5 remains
    unchecked pending Phase 2.

---

### Phase 2 — Final QC Loop

Unconditional full C# toolchain, run in order. If any step fails or changes files, restart this
phase from P2-T1. No `SKIPPED` outcomes; no IN_SCOPE/OUT_OF_SCOPE branches.

- [x] [P2-T1] Run the final C# formatting command.
  - Command: `dotnet tool run csharpier .`
  - Evidence: `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/qa-gates/csharpier-final-392.<TS>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`; if
    this command changes files, restart Phase 2 from P2-T1.

- [x] [P2-T2] Run the final C# analyzer build command.
  - Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Evidence: `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/qa-gates/analyzer-final-392.<TS>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:`;
    if this command fails, fix and restart Phase 2 from P2-T1.

- [x] [P2-T3] Run the final C# nullable build command.
  - Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  - Rationale for `/t:Rebuild` (not `/t:Build`): the P0-T11 baseline was captured with `/t:Rebuild`
    because MSBuild's incremental up-to-date check otherwise skips `CoreCompile` after P2-T2's
    build, which would vacuously pass both the vendored-exclusion comparison and — worse — skip
    re-verifying nullable flow on the modified first-party file. A full-recompile-to-full-recompile
    comparison is required (see
    `docs/features/potential/2026-07-07-ci-nullable-check-skipped-vendored-projects.md`).
  - Evidence: `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/qa-gates/nullable-final-392.<TS>.md`.
  - Acceptance: the command must run and the artifact must record `Timestamp:`, `Command:`,
    `EXIT_CODE:`, and `Output Summary:` including an error-set comparison against the P0-T11
    baseline artifact. Acceptance = zero NEW errors relative to the P0-T11 baseline AND zero errors
    attributable to first-party in-scope files. Pre-existing vendored `SVGControl.csproj` errors
    that are identical to the P0-T11 baseline are explicitly non-blocking, per the AC-5 scope note
    (amended 2026-07-20 by orchestrator) in `issue.md` and
    `docs/features/potential/2026-07-07-ci-nullable-check-skipped-vendored-projects.md`. If any NEW
    error appears, or any error is attributable to a first-party project, fix and restart Phase 2
    from P2-T1.

- [x] [P2-T4] Run the final full-suite MSTest coverage command for `QuickFiler.Test` via
  `vstest.console.exe` with `/EnableCodeCoverage`, producing a Cobertura report.
  - Command: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage`
  - Evidence: `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/qa-gates/vstest-coverage-final-392.<TS>.md`.
  - Acceptance: artifact contains `Timestamp:`, `Command:`, `EXIT_CODE: 0`, and `Output Summary:`
    with total tests, pass/fail counts, and the numeric post-change line-coverage percentage for
    `QuickFiler.dll`; if this command fails, fix and restart Phase 2 from P2-T1.

- [x] [P2-T5] Compare the baseline coverage (P0-T12) against the post-change coverage (P2-T4) for
  `QuickFiler/Controllers/QfcItemController.FolderHandling.cs`'s changed lines and confirm no
  regression and >= 90% coverage on the new/changed code (per AC-5's explicit target).
  - Evidence: `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/qa-gates/coverage-delta-392.<TS>.md`.
  - Acceptance: artifact contains baseline coverage %, post-change coverage %, and changed-line
    coverage % for `QfcItemController.FolderHandling.cs`, with an explicit PASS/FAIL statement on
    "no regression on changed lines" and ">= 90% coverage on new/changed code." If either check
    fails, the outcome is remediation-required, not PASS.

- [x] [P2-T6] Verify no other test regressed by comparing the baseline (P0-T12) and final (P2-T4)
  full-suite results by test name/class.
  - Evidence: `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/qa-gates/regression-check-392.<TS>.md`.
  - Acceptance: artifact confirms every test that passed at baseline still passes, and the total
    pass count did not decrease.

- [x] [P2-T7] Convert the P2-T4 Cobertura output to the JaCoCo-format canonical coverage-gate input
  at `artifacts/csharp/coverage.xml` (report/package/class `counter` elements), scoped to
  first-party assemblies only (excluding vendored/third-party and `*.Test` assemblies), mirroring
  the established conversion pattern recorded in
  `docs/features/active/2026-07-16-quickfiler-breadcrumb-webview2-351/evidence/qa-gates/coverage-conversion.2026-07-18T10-55.md`.
  Also write a canonical evidence mirror of the resulting counter totals.
  - Evidence: `artifacts/csharp/coverage.xml` (JaCoCo, tooling-input path, not an evidence
    duplicate) and
    `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/qa-gates/coverage-conversion-392.<TS>.md`.
  - Acceptance: `artifacts/csharp/coverage.xml` exists in JaCoCo format (not Cobertura) with
    `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` recorded in the evidence mirror,
    including the report-level LINE and BRANCH counter totals used to satisfy the feature-review
    coverage gate.

- [x] [P2-T8] Check off AC-5 in `issue.md`'s `## Acceptance Criteria` section and record the final
  AC closure summary.
  - Files: `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/issue.md`.
  - Evidence: `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/issue-updates/ac-status-final-392.<TS>.md`
    and `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/other/ac-closure-summary-392.<TS>.md`.
  - Acceptance: AC-5 is changed from `- [ ]` to `- [x]` when the amended AC-5 wording (baseline-scoped
    nullable gate: zero regression relative to the P0-T11 baseline, first-party projects clean;
    pre-existing vendored `SVGControl.csproj` errors do not gate this fix) is satisfied by the
    recorded P2-T1 through P2-T7 evidence; the closure summary lists AC-1 through AC-5 each mapped
    to its exact backing evidence artifact path(s) from Phases 1 and 2.

- [x] [P2-T9] Record final minor-audit readiness evidence for issue #392.
  - Evidence: `docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/qa-gates/minor-audit-readiness-392.<TS>.md`.
  - Acceptance: evidence confirms Phase 0 artifacts exist, Phase 1 diagnosis/regression-test/fix
    evidence exists, Phase 2 QC artifacts exist (including the P2-T7 JaCoCo conversion), every
    command-bearing task has an executed numeric `EXIT_CODE`, and AC-1 through AC-5 are checked off
    in `issue.md`. AC-1 through AC-5 may be recorded as all checked under the amended AC-5 wording
    (baseline-scoped nullable gate, first-party projects only), and the evidence must cite the
    vendored-nullable exclusion rationale: the P2-T3 error-set comparison against the P0-T11
    baseline, and `docs/features/potential/2026-07-07-ci-nullable-check-skipped-vendored-projects.md`.

---

## Acceptance Criteria Coverage Map (for preflight cross-check)

- AC-1 (deterministic MSTest regression test reproduces the defect and fails before the fix, passes
  after; no temp files/external dependencies) → P1-T2 (author), P1-T4 (fail-before), P1-T5/P1-T6
  (fix), P1-T7 (pass-after).
- AC-2 (`AssignFolderComboBox` no longer throws for a single-entry `FolderArray` with no
  predetermined match; selects index 0) → P1-T3 (author), P1-T4 (fail-before), P1-T5 (fix), P1-T7
  (pass-after).
- AC-3 (existing multi-suggestion index-1 behavior and predetermined-preselect behavior preserved)
  → P1-T8 (targeted no-regression run).
- AC-4 (`PopulateAndSelectFolder` applies the same bounds-safe fallback) → P1-T2 (author), P1-T4
  (fail-before), P1-T6 (fix), P1-T7 (pass-after).
- AC-5 (full C# toolchain passes in order; zero regressions; new/changed code >= 90% coverage) →
  P2-T1 through P2-T7.
