# Policy Compliance Audit — folder-combobox-fallback-index-out-of-range (Issue #392)

- Timestamp: 2026-07-20T18-00
- Reviewer: feature-review (initial audit)
- Base branch (resolved): `main`
- Merge-base SHA: `bd43572498474be89d80e1f9620dffb132ade377`
- Head SHA: `8f34f8ef45d188f02ea19caef3c6e2b610f1a4ab`
- Work Mode: `minor-audit` (AC source: explicit `## Acceptance Criteria` section of `issue.md`)
- Scope: full branch diff vs merge-base (feature-vs-base). Not narrowed to any plan, task, or phase.

## Executive Summary

The branch fixes Issue #392: `QfcItemController.AssignFolderComboBox` and the retained static helper
`PopulateAndSelectFolder` unconditionally selected folder-suggestion index 1, which throws
`ArgumentOutOfRangeException` (via `BreadcrumbStateModel.SelectRow`) when exactly one suggestion is
present. Both call sites now clamp the fallback to index 0 when `FolderArray.Length == 1`. Two new
MSTest regression tests were added; the executor's evidence documents fail-before / pass-after /
no-regression verification (541/541 tests passing) and 100% line coverage on the five reported
Cobertura sequence points spanning the new/changed lines.

Overall verdict: **PARTIAL (remediation required, coverage only)**. The C# toolchain (format,
analyzers, tests) passes; the nullable gate reproduces a pre-existing, byte-identical 34-error
condition confined to vendored `SVGControl.csproj` (dispositioned, not a regression). All five
acceptance criteria are met on their literal terms. However, mandatory C# coverage checks below the
uniform 85%/75% floor (`.claude/rules/quality-tiers.md`) are **FAIL** at three scopes: the touched
class's branch rate, the `QuickFiler` package, and the canonical repo-wide artifact. All three are
confirmed pre-existing conditions (baseline figures are materially unchanged or the change is a net
improvement) and not caused or worsened by this bug fix, but they are real, unratified sub-floor
conditions that require an explicit maintainer disposition or a follow-on coverage-uplift task before
this policy area can read PASS. See Section 5.

## 1. Scope and Baseline

- Range audited: `bd43572498474be89d80e1f9620dffb132ade377..8f34f8ef45d188f02ea19caef3c6e2b610f1a4ab`.
- Merge-base recomputed independently: `git merge-base HEAD origin/main` = `bd43572498474be89d80e1f9620dffb132ade377` (matches the supplied base).
- Changed languages with files in the branch diff: **C# only** (2 `.cs` files: 1 production, 1 test).
  No TypeScript, Python, or PowerShell files changed. 29 Markdown files (plan, issue, evidence) were
  also added; these are docs/evidence, not a changed language for coverage purposes.
- Coverage enforcement is therefore mandatory for C# and only C#.

### 1.1 PR-context summary correction (recurring C#-as-docs misclassification)

`artifacts/pr_context.summary.txt`'s "Changed files overview" reported `Core logic changes: 0 files`
and classified all 31 changed files as docs/evidence. This is the known generator misclassification
(recorded in prior feature reviews of this repo): `git diff --numstat` confirms 2 changed `.cs` files
(`QuickFiler/Controllers/QfcItemController.FolderHandling.cs` +5/-2,
`QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs` +37/-17). The summary was
corrected in place (annotated `CORRECTED BY feature-review 2026-07-20`), adding a `Core logic changes:
2 files` bullet list with the two space-free `.cs` paths in the generator's own `(+N/-N)` format so
downstream language-detection tooling recognizes C# as a changed language. C# is treated as an
in-scope changed language and its coverage is enforced below regardless of the original overview.

## 2. Toolchain Compliance (C#)

Evidence source: executor QA-gate artifacts under
`docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/`. These
are read as pre-existing evidence per the feature-review workflow (msbuild/vstest are not available
on this review host; the code under test is unchanged since these artifacts were produced, confirmed
by `git status` showing a clean working tree at HEAD).

| Stage | Command | Evidence | Exit | Verdict |
|---|---|---|---|---|
| Format | `csharpier format .` then `csharpier check .` | csharpier-final-392.2026-07-20T14-20.md | 0 (format) / 1 (check, pre-existing 32-file config-noise, unchanged from baseline; both in-scope files clean) | PASS |
| Lint / analyzers | `MSBuild.exe TaskMaster.sln /t:Build ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | analyzer-final-392.2026-07-20T14-24.md | 0 | PASS |
| Type / nullable | `MSBuild.exe TaskMaster.sln /t:Rebuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` | nullable-final-392.2026-07-20T15-10.md | 1 (34 errors, byte-identical error-set to the P0-T11 baseline, all attributed exclusively to vendored `SVGControl.csproj`; 0 new, 0 first-party) | PASS (dispositioned; see AC-5 scope note below) |
| Tests | `dotnet-coverage collect -f cobertura ... -- vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation` | vstest-coverage-final-392.2026-07-20T14-32.md | 0 (541/541 passed, 0 failed) | PASS |

Nullable disposition: the AC-5 scope note in `issue.md` (amended 2026-07-20 by orchestrator, before
feature review) explicitly scopes nullable enforcement to first-party projects per
`.claude/rules/csharp.md` (analyzers/nullable wired to first-party projects only; vendored projects
excluded). The 34 errors are confirmed byte-identical to the pre-existing baseline and are tracked
separately in `docs/features/potential/2026-07-07-ci-nullable-check-skipped-vendored-projects.md`.
Independently re-verified: `git show bd435724:QuickFiler/Controllers/QfcItemController.FolderHandling.cs`
and the test file's baseline revision contain no nullable-annotation changes, consistent with the
executor's error-set-diff claim of zero new/first-party errors.

## 3. General Code Change Policy

| Requirement | Verdict | Evidence |
|---|---|---|
| Simplicity first | PASS | The fix is a single ternary clamp (`FolderArray.Length == 1 ? 0 : 1`) at each of the two existing fallback sites; no new abstraction introduced. |
| Reusability | PASS with note | The identical `Length == 1 ? 0 : 1` clamp is duplicated across `AssignFolderComboBox` and `PopulateAndSelectFolder` rather than factored into one shared helper. Low-severity, non-blocking; see code-review Finding CR-1. |
| Separation of concerns | PASS | No I/O, COM, or UI framework logic was added; the WinForms-bound `PopulateAndSelectFolder` remains pure of `InvokeRequired` marshaling as before. |
| Error handling / fail-fast | PASS | No new try/catch or swallowed exceptions; the fix removes an unguarded invalid-index condition rather than catching the resulting exception. |
| Backward compatibility | PASS | Existing multi-suggestion (`Length > 1`) and predetermined-folder behavior is unchanged (ternary's `else` branch is the pre-existing constant `1`); verified via `targeted-no-regression-392` evidence (6 pre-existing tests, all pass). |
| Public API compatibility | PASS | No signature changes to `AssignFolderComboBox()` or `PopulateAndSelectFolder(...)`. |
| File size <= 500 lines | PASS | See Section 4. |

## 4. File-Size Compliance

Independently verified line counts at HEAD (`wc -l` / `awk 'END{print NR}'`, cross-checked):

- `QuickFiler/Controllers/QfcItemController.FolderHandling.cs`: 235 lines (baseline 232, +3). Within
  the 500-line limit.
- `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs`: **500 lines exactly**
  (baseline 480, +20). At, but not exceeding, the 500-line limit. Verdict: PASS (boundary case; a
  reviewer should note this file has no further growth headroom before the limit is breached — see
  code-review Finding CR-2).

No other production or test file was changed by this branch (all other 29 changed files are Markdown
evidence/plan/issue documents, exempt from the 500-line limit per the General Code Change Policy's
"Markdown documentation files" exception).

## 5. Coverage Verification (C#)

Mandatory for C# (changed files present). The canonical review coverage artifact path for C# is
`artifacts/csharp/coverage.xml` (JaCoCo counter form, read by
`.claude/hooks/validate-feature-review-coverage.ps1`'s `Get-JacocoRepoCoverage` /
`Get-JacocoBranchCoverage`).

### 5.1 Canonical artifact presence and scope

`artifacts/csharp/coverage.xml` is PRESENT on this review host, generated per
`evidence/qa-gates/coverage-conversion-392.2026-07-20T14-50.md` from the P2-T4
`final-coverage.cobertura.xml` (single collection: only `QuickFiler.Test.dll` was run under
`dotnet-coverage`). The artifact aggregates six first-party packages
(`QuickFiler`, `SVGControl`, `Tags`, `TaskVisualization`, `ToDoModel`, `UtilitiesCS`); only `QuickFiler`
was actually instrumented by a dedicated test project in this collection. `Tags`, `TaskVisualization`,
and `ToDoModel` read 0.00%/0.00% because no dedicated test project for those assemblies ran in this
collection (not because their production code is uncovered — they are exercised by their own suites in
PR CI). `SVGControl` (11.63%/6.68%) and `UtilitiesCS` (7.48%/7.79%) show only incidental partial
coverage from classes referenced transitively by `QuickFiler.Test`'s run, not from their own dedicated
suites.

Unlike a prior feature review in this repo (`outlook-store-exclusion-328`) where re-scoping the
aggregate to the actually-instrumented package cleared the floor, re-scoping here does **not** rescue
the number: `QuickFiler` — the one package with a genuine dedicated-suite run in this collection — is
itself measured at 73.68% line / 64.62% branch, both below the 85%/75% floor. The corrected/rescoped
figure is reported below alongside the raw six-package aggregate; the artifact itself was **not**
edited, because no legitimate re-scoping clears the floor this time (edits would misrepresent, not
correct, the measurement).

- Raw six-package aggregate (as delivered): LINE 9021/55511 = 16.25%; BRANCH 1868/13736 = 13.60%.
  Independently recomputed via `//counter[@type="LINE"]` / `//counter[@type="BRANCH"]` XPath summation
  (the hook's own parser logic) against `artifacts/csharp/coverage.xml` — reproduces 16.25%/13.60%
  exactly.
- Corrected (instrumented-only, `QuickFiler` package): LINE 5693/7727 = 73.68%; BRANCH 1012/1566 =
  64.62%. Both below the 85% line / 75% branch floor.

### 5.2 Per-class and per-method verification (feature-evidence Cobertura)

Sourced from `evidence/qa-gates/coverage-delta-392.2026-07-20T14-38.md` and
`evidence/qa-gates/vstest-coverage-final-392.2026-07-20T14-32.md` (executor's post-change Cobertura
run, not re-run here):

| Scope | Line coverage | Branch coverage | Line floor 85% | Branch floor 75% |
|---|---|---|---|---|
| `QuickFiler` package (whole assembly) | 73.68% | 64.62% | FAIL | FAIL |
| `QfcItemController.FolderHandling.cs` (class, file-scoped) | 91.89% | 73.81% | PASS | FAIL (marginal, -1.19pt) |
| `AssignFolderComboBox()` method | 89.29% | 87.5% | PASS | PASS |
| `PopulateAndSelectFolder(...)` method | 100% | 100% | PASS | PASS |

No regression on any row: every post-change figure is greater than or equal to its baseline
counterpart (baseline: package 73.67%/64.53%; class 91.55%/71.05%; `AssignFolderComboBox` 88.46%/85.71%;
`PopulateAndSelectFolder` 100%/100%, per `coverage-delta-392.2026-07-20T14-38.md`).

### 5.3 New-code and changed-line coverage

Changed lines (per `git diff`): the `else` branch of `AssignFolderComboBox()`
(`QfcItemController.FolderHandling.cs:201-205`) and the `PopulateAndSelectFolder` conditional
assignment (`:230-231`). Post-change Cobertura reports `hits="1"` on all 5 reported sequence points
(lines 201, 203, 204, 205, 231) — **100% line coverage on the new/changed code**, exceeding the >= 90%
target cited in AC-5. Both logical paths of each new ternary (`Length == 1` and the `else`/`> 1` path)
are independently exercised by named tests (2 new: `PopulateAndSelectFolder_SingleItemNoPredeterminedMatch_SelectsIndexZeroWithoutThrowing`,
`AssignFolderComboBox_WhenSingleSuggestionNoPredeterminedMatch_SelectsIndexZero`; 2 pre-existing,
re-verified: `PopulateAndSelectFolder_AllMissingPredetermined_SelectsIndexOne`,
`AssignFolderComboBox_WhenNoPredeterminedFolder_SelectsTopSuggestionViaViewer`). Verdict: PASS.
Changed-line regression: none (Section 5.2). Verdict: PASS.

### 5.4 Repo-wide C# coverage

Neither the raw six-package aggregate (16.25%/13.60%) nor the corrected instrumented-only scope
(`QuickFiler` only, 73.68%/64.62%) clears the 85% line / 75% branch floor. This differs from the
`outlook-store-exclusion-328` precedent, where the corrected scope cleared the floor; here it does
not. The true all-first-party repo-wide figure (aggregated across every first-party assembly's own
dedicated test suite, as CI would run) is not computable from this single local
`QuickFiler.Test`-only collection. **Verdict: FAIL** on every locally-measurable repo-wide scope.
Disposition: this is a pre-existing condition — the baseline `QuickFiler` package figure (73.67%/64.53%,
per `evidence/baseline/vstest-coverage-baseline.2026-07-20T13-45.md`) is virtually identical to the
post-change figure (73.68%/64.62%), so this bug fix neither introduced nor worsened the gap. The gap
predates this branch and reflects broad, pre-existing under-coverage across `QuickFiler`'s WinForms/UI
surface, unrelated to the two lines this bug fix touches. Closing it is disproportionate scope for a
minor-audit bug fix and is routed to remediation as a maintainer-disposition item (Section 5.6;
`remediation-inputs.2026-07-20T18-00.md`), not a code task for this fix.

### 5.5 Modified-file branch coverage — marginal, pre-existing, no regression

`QfcItemController.FolderHandling.cs` class-level branch coverage is 73.81%, 1.19 points below the 75%
floor. This is a marginal, pre-existing gap, not a regression: baseline was 71.05% (already below
floor before #392); the fix's own new lines are 100% line-covered and both new ternary branches are
independently test-exercised (Section 5.3). The residual sub-floor branch percentage stems from other,
unrelated pre-existing branches elsewhere in the same partial-class file that this minor-audit bug fix
did not touch. Verdict: FAIL (floor), dispositioned as pre-existing with net improvement
(71.05% -> 73.81%), no changed-line regression. Routed to remediation as an optional, narrowly-scoped
task (add 1-2 targeted tests for an existing uncovered branch in the same already-in-scope file) — see
`remediation-inputs.2026-07-20T18-00.md`.

### 5.6 C# coverage verdict (summary row)

**C# coverage: FAIL.** New-code and changed-line coverage both PASS (100% / no regression). Three
sub-floor conditions are open, all confirmed pre-existing and not caused by this change: (a) modified
class-level branch coverage 73.81% (floor 75%, marginal -1.19pt gap, closeable via a narrow test
addition); (b) `QuickFiler` package-wide line/branch coverage 73.68%/64.62% (floor 85%/75%, broad
pre-existing gap, disproportionate to fix in this cycle); (c) canonical repo-wide artifact
16.25%/13.60% (floor 85%/75%, distorted by an incomplete single-project local collection, requires
CI-level full-suite aggregation to measure accurately). None of the three is a ratified exception yet
(no `human_interaction`/`orchestrator-state` scope-change record exists for this feature, unlike the
`StoreWrapper` precedent in #328). Remediation is triggered; see
`remediation-inputs.2026-07-20T18-00.md` and `remediation-plan.2026-07-20T18-00.md`.

### 5.7 Coverage exemptions applied

None. No `[ExcludeFromCodeCoverage]` attribute and no `coverage.config`/`.csproj` coverage-exclude was
added or modified by this branch (verified: `git diff` touches only the two named `.cs` files).

## 6. Unit Test Policy

| Requirement | Verdict | Evidence |
|---|---|---|
| Framework MSTest | PASS | New tests use `[TestClass]`/`[TestMethod]` (class already `[TestClass]`). |
| Moq for mocking | PASS | `Mock<IItemViewer>` used in `AssignFolderComboBox_WhenSingleSuggestionNoPredeterminedMatch_SelectsIndexZero`. |
| FluentAssertions | PASS | `.Should()` assertions in both new tests (`act.Should().NotThrow<...>()`, `comboBox.SelectedIndex.Should().Be(0)`, `mock.Verify(...)`, `controller.SelectedFolder.Should().Be(...)`). |
| Determinism | PASS | No `Thread.Sleep`, `Task.Delay`, `DateTime.Now/UtcNow`, temp files, or `new Random()` in the changed test file (grep clean). No external dependencies; `ComboBox` is disposed via `using`. |
| Scenario completeness | PASS | New tests cover the single-suggestion positive case at both call sites; pre-existing tests (re-verified, unchanged) cover multi-suggestion, predetermined-match, empty-array (documents the pre-existing unrelated empty-array throw), and null-handler no-op paths. |
| Test file size <= 500 | PASS (at boundary) | 500 lines exactly (baseline 480, +20); see Section 4 and code-review Finding CR-2. |
| Test-file line-count regression | PASS | Grew 480 -> 500 lines; within the 500-line limit, not a regression per se (net addition of 2 new tests, doc-comment trimming, and reuse of the existing `SetPrivate` helper in place of inline reflection). |
| Independence / isolation | PASS | Each new test constructs its own `ComboBox`/`Mock<IItemViewer>`/`FolderController`; no shared mutable state between tests. |

## 7. Additional Policy Rules

### 7.1 Evidence Location Compliance

All evidence artifacts for this feature are written under the canonical
`docs/features/active/2026-07-20-folder-combobox-fallback-index-out-of-range-392/evidence/<kind>/`
tree (`evidence/baseline/`, `evidence/regression-testing/`, `evidence/qa-gates/`,
`evidence/issue-updates/`, `evidence/other/`). A scan of the branch diff
(`git diff --name-only bd435724..8f34f8ef`) for files under `artifacts/baselines/`, `artifacts/qa/`,
`artifacts/evidence/`, or `artifacts/coverage/` returned zero matches. `scripts/*/validate_evidence_locations.py`
does not exist in this repository (checked via file search); the manual `git diff` scan above is the
substitute check. Verdict: PASS. Note: `artifacts/csharp/coverage.xml` is the canonical review
coverage-artifact location (read by the coverage-validation hook), not a feature-evidence location,
and is not part of the tracked branch diff (untracked, gitignored).

### 7.2 modified-workflow-needs-green-run

`git diff --name-only bd435724..8f34f8ef` contains no files matching `.github/workflows/**`,
`scripts/benchmarks/**`, or `.github/actions/**`. The rule does not fire; no green-run evidence is
required. Verdict: PASS (rule not triggered).

## Rejected Scope Narrowing

The delegating prompt for this review explicitly stated "Scope determination is your responsibility
per your scope invariant" and did not attempt to narrow scope to any plan, task, phase, or file
subset. No caller instruction marked any language's coverage as "plan scope only," "out of scope,"
"informational only," or equivalent. The trailing `DIRECTIVE: PREFLIGHT VALIDATION ONLY` convention
referenced in shared skills is standard planner/executor handoff text and was not present in, or
applicable to, this delegation. No scope-narrowing instruction was detected or rejected in this
review cycle.

## Appendix A — Coverage Verdict Checklist

- C# — canonical artifact `artifacts/csharp/coverage.xml`: present; raw six-package aggregate FAIL
  (16.25% line / 13.60% branch); corrected instrumented-only (`QuickFiler`) scope also FAIL
  (73.68% line / 64.62% branch).
- C# — new-code / changed-line coverage: PASS (100% on all 5 reported new/changed sequence points; no
  regression).
- C# — touched non-exempt class (`QfcItemController.FolderHandling.cs`) line coverage: PASS (91.89%).
- C# — touched non-exempt class branch coverage: FAIL (73.81%, floor 75%, dispositioned pre-existing
  with net improvement, no changed-line regression).
- C# — repo-wide first-party line coverage (best-available local scope): FAIL (73.68% instrumented /
  16.25% raw aggregate).
- C# — repo-wide first-party branch coverage (best-available local scope): FAIL (64.62% instrumented /
  13.60% raw aggregate).
- TypeScript / Python / PowerShell: zero changed files on the branch; coverage enforcement not
  required for these languages.

## Appendix B — Command Reference

Commands used or cited during this review (check-only, no mutation of source):

- `git merge-base HEAD origin/main` — recomputed base SHA `bd43572498474be89d80e1f9620dffb132ade377`.
- `git diff --numstat bd435724..8f34f8ef` — enumerated the full branch diff (2 `.cs`, 29 `.md`).
- `git diff bd435724..8f34f8ef -- <path>` — read production and test diffs.
- `wc -l` / `awk 'END{print NR}' <file>` — file-size checks at HEAD and at the merge-base.
- Python `xml.etree.ElementTree` parse of `artifacts/csharp/coverage.xml` — independent re-derivation
  of report-level and package-level `LINE`/`BRANCH` counters (reproduces the executor's
  16.25%/13.60% raw figures and the 73.68%/64.62% `QuickFiler`-only rescoped figures).
- `git show bd435724:<path> | wc -l` — baseline line counts for the two touched files.
- Executor toolchain evidence (not re-run here): `csharpier-final-392`, `analyzer-final-392`,
  `nullable-final-392` (both revisions), `vstest-coverage-final-392`, `coverage-delta-392`,
  `coverage-conversion-392`, `regression-check-392`, `fail-before-392`, `pass-after-392`,
  `targeted-no-regression-392` under `evidence/qa-gates/` and `evidence/regression-testing/`.
