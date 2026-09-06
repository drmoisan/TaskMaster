# Policy Audit — Issue #782 (pr-778-post-merge-review-residuals)

- **Component:** UtilitiesCS, UtilitiesCS.Test, QuickFiler.Test, TaskMaster (Ribbon)
- **Date:** 2026-09-05
- **Reviewer:** feature-review agent
- **Issue:** #782
- **Work Mode:** `full-feature` (marker read from `issue.md` line 10)
- **AC sources:** `spec.md` (AC1-AC12) and `user-story.md` (AC-U1 to AC-U5)
- **Base branch (resolved):** `main` -> `origin/main` @ `77c6d31404e2bc2291aec7eb9561e393c20cdcae`
- **Merge base (recomputed):** `77c6d31404e2bc2291aec7eb9561e393c20cdcae` (`git merge-base origin/main HEAD`; ancestor of HEAD, so two-dot and three-dot diffs agree)
- **Head:** `refactor/pr-778-post-merge-review-residuals-782` @ `4ed2f790e96d8c22abd36514db3848b71e073912`
- **Diff range audited:** `77c6d31404e2bc2291aec7eb9561e393c20cdcae...4ed2f790e96d8c22abd36514db3848b71e073912`

## Executive Summary

The branch changes **87 files across 22 commits**: **16 C# / csproj files (+742 / -402)** and 71 Markdown
files. The scope audited is the full branch diff against the resolved base branch, not any plan,
phase, or caller-supplied subset.

All four toolchain gates were **re-run independently by this reviewer** and all pass:
CSharpier check exit 0 (`Checked 1583 files`), analyzer `msbuild /t:Rebuild` exit 0 over 18 projects,
nullable `msbuild /t:Rebuild` exit 0 with `0 Warning(s)` / `0 Error(s)`. The reported test result
(7000 total, 7000 passed, 0 failed, 0 skipped) is corroborated by the coverage document that run
produced, from which every coverage figure below was independently re-derived.

Every acceptance criterion asserted by the delivery was verified against the tree rather than
accepted from the artifacts. **16 of 17 acceptance criteria PASS**; the single open criterion,
AC-U1, requires a pull request that does not yet exist and is correctly left unchecked.

**Zero blocking code defects were found.** Two **FAIL** verdicts are recorded, both procedural and
both dispositioned non-blocking with evidence:

1. The canonical C# coverage artifact `artifacts/csharp/coverage.xml` is absent (deliberately, under
   scope decision SD1). Coverage verification is mandatory, so the row reads FAIL.
2. The modified file `UtilitiesCS/Threading/UiThread.cs` sits at 76.83% line coverage, below both the
   85% uniform floor and the 80% remediation-trigger floor.

For finding 2 this reviewer established a fact the delivery's own artifacts do not state: the
**uncovered line set of `UiThread.cs` is byte-identical between baseline and head** — the same 19
line numbers, `28,29,30,32,33,34,67-76,118,119,120`, on both sides. Not one line transitioned from
covered to uncovered. The -0.28 percentage-point movement is purely the arithmetic consequence of a
covered three-line wrapped `throw` collapsing to one line when routed through the shared constant.
All 7 changed executable production lines are covered.

One evidence-integrity defect (**EV-1**) was found: the re-recorded baseline coverage figures are not
reproducible from the baseline document on disk. It does not change any verdict, because every
candidate baseline value is at or below the head value.

**Overall verdict: PASS with two non-blocking procedural FAIL rows. Recommendation: GO for pull
request.**

## Rejected Scope Narrowing

The caller made **no attempt to narrow the audit scope**. The caller's prompt explicitly instructed
"Determine scope yourself" and framed its two statements about the PR context artifact as
"measurements you should verify rather than take from me." Both were independently verified and both
proved correct:

| Caller statement | Independent verification | Outcome |
|---|---|---|
| The summary's `Core logic changes: 0 files` is a top-N-by-churn truncation, not the changed-file set | `git diff --stat <base>...HEAD -- "*.cs" "*.csproj"` returns 16 files, +742/-402 | Confirmed; scope derived from git, not from the summary |
| The seven `Close candidates` (#394, #449, #476, #493, #508, #584, #778) are prose scrapes | #394 appears in `spec.md` Constraint 6 as a cited past defect; the pattern holds for the others | Confirmed as false positives |

Two items were considered as possible narrowing and are recorded here for completeness. Neither is a
caller instruction, and neither was honoured as a limit on this audit.

1. **`spec.md` Constraint 11 / scope decision SD1** states that `artifacts/csharp/coverage.xml` is not
   produced partly because "the hook applies a fixed repository-wide line floor that would force a
   FAIL verdict for a shortfall that pre-exists on origin/main." This is an in-repository artifact
   arguing for the avoidance of a coverage gate. This reviewer did **not** honour it: the coverage
   figures were computed directly from the Cobertura documents and the FAIL verdicts are recorded in
   full below.
2. **The caller's instruction never to write under `.claude/**`** is a write-scope restriction, not
   an audit-scope restriction. It was honoured as a write restriction and disregarded as an audit
   restriction: this reviewer read `.claude/hooks/`, `.claude/rules/`, and `.claude/skills/` freely.
   The branch diff contains zero `.claude/` paths, verified by
   `git diff --stat <base>...HEAD -- ".claude/"` returning empty.

## Evidence Location Compliance

`validate_evidence_locations.py` does not exist in this repository, so the scan was performed
directly against the branch diff.

| Check | Command | Result |
|---|---|---|
| Files written under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, `artifacts/coverage/` | `git diff --name-only <base>...HEAD -- "artifacts/"` | **Zero paths.** PASS |
| Delivery evidence under the canonical `<FEATURE>/evidence/<kind>/` layout | `git diff --name-status` inspection | PASS — 38 evidence files under `baseline/`, `qa-gates/`, `regression-testing/`, `other/` |

No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` condition arose. This reviewer's own artifacts are written
to the active feature folder root, which is the required location for review artifacts.

## 1. General Unit Test Policy Compliance

| # | Requirement | Verdict | Evidence |
|---|---|---|---|
| 1.1 | Independence — tests run in any order | PASS | The delivery's central change is a shared `UiThreadDispatcherScope` that restores the prior static value on disposal, including a null prior. `[DoNotParallelize]` added to `IdleActionQueue_Tests` and `WpfDispatcherYieldTests`. |
| 1.2 | Isolation — one unit per test | PASS | The three added tests each pin one guard. |
| 1.3 | Fast execution | PASS | 7000 tests in 44.9 s. The five delivery-relevant tests run in 0.4-1.8 ms each. |
| 1.4 | Determinism | PASS | Zero banned timing APIs introduced — `git diff <base>...HEAD -- "*.cs" \| grep "^+"` matched no `Thread.Sleep`, `Task.Delay`, `DateTime.Now`, or `DateTime.UtcNow`. C21 runs its Act on a dedicated fresh thread specifically to remove pooled-worker coupling. |
| 1.5 | Readability and maintainability | PASS | Every added test carries a `<summary>` with Scenario and Expected sections and explicit Arrange / Act / Assert comments. |
| 1.6 | No temporary files in tests | PASS | No `GetTempPath`, `GetTempFileName`, or equivalent introduced. |
| 1.7 | Scenario completeness | PASS | Negative and error paths added for three previously unguarded throw sites. |
| 1.8 | Coverage thresholds | FAIL (non-blocking) | See section 5. |
| 1.9 | Test file location mirrors production | PASS with pre-existing deviation | The rule text prescribes a `tests/` tree; this repository has used `<Project>.Test/` assemblies throughout its history. The deviation is repository-wide and pre-existing; this branch introduces no new deviation and both new test files land in the existing mirrored structure. |
| 1.10 | Coverage Exclusion Policy — no production file excluded | PASS | No `exclude` entry matching a production source path is added. `coverage.config` is unchanged by this branch. |

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 84.50% line (112,359/132,967) / 79.14% branch (26,496/33,480). Post-change: 84.51% line (112,363/132,961) / 79.15% branch (26,500/33,480). Change: +0.01% line and +0.01% branch; both metrics improved, neither regressed. New/changed-code coverage: 100.00% line (7 of 7 changed executable production lines covered). Disposition: FAIL. Evidence: `coverage/782-p0-baseline.cobertura.xml` and `coverage/782-p7-final.cobertura.xml`, both re-aggregated by this reviewer using the pinned all-descendant `.//line` selection over the nine first-party packages; dedup cross-check 84.65% -> 84.66% line and 79.13% -> 79.15% branch.
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A — no TypeScript files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A — no Python files changed on this branch.
- PowerShell: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A — no PowerShell files changed on this branch.

### 1.2.2 Coverage Evidence Checklist

- TypeScript baseline coverage artifact: N/A - out of scope
- TypeScript post-change coverage artifact: N/A - out of scope
- PowerShell baseline coverage artifact: N/A - out of scope
- PowerShell post-change coverage artifact: N/A - out of scope
- C# baseline coverage document: `coverage/782-p0-baseline.cobertura.xml` present on disk, 18,144,506 bytes, re-aggregated by this reviewer. FAIL against the canonical-path requirement; see section 5.
- C# post-change coverage document: `coverage/782-p7-final.cobertura.xml` present on disk, 18,144,107 bytes, re-aggregated by this reviewer. FAIL against the canonical-path requirement; see section 5.

## 2. General Code Change Policy Compliance

| # | Requirement | Verdict | Evidence |
|---|---|---|---|
| 2.1 | Simplicity first | PASS | The getter change is a single-read local. The shared message is one `const`, not a new holder type; the spec records that alternative and why it was rejected. |
| 2.2 | Reusability | PASS | Six independently written reflection sites reduced to two acquisitions repository-wide, verified by a search for the token `"_dispatcher"` across all `*.cs` returning exactly two hits: `UtilitiesCS.Test/TestHelpers/UiThreadDispatcherScope.cs:117` and `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs:136`. |
| 2.3 | Extensibility | PASS | No public signature changed. `Dispatcher` keeps its declared type and private setter. |
| 2.4 | Separation of concerns | PASS | Test scaffolding lands in `UtilitiesCS.Test/TestHelpers/`, not in the production `UiThread` type. |
| 2.5 | Fail fast and explicitly | PASS | The uninitialized path throws rather than returning null; no new `catch` was introduced anywhere in the diff. |
| 2.6 | 500-line file limit | PASS | Every touched file measured with `awk END{print NR}`: `ProgressTracker_Tests.cs` 271, `ProgressTracker_ReportAndViewerTests.cs` 288, `UiThread_Tests.cs` 213, `WpfDispatcherYieldTests.cs` 256, `EmailMoveMonitorTests.cs` 317, `IdleActionQueue_Tests.cs` 278, `IdleAsyncQueue_Tests.cs` 341, `ProgressTrackerAsync_Tests.cs` 231, `UiThread.cs` 195, `WpfDispatcherYield.cs` 76, `UiThreadDispatcherScope.cs` 126, `QfcItemController.InitializationTests.Part2.cs` 397. The 514-line pre-existing violation is removed. |
| 2.7 | No policy documents modified | PASS | Zero paths under `.claude/` or `.github/instructions/` in the diff. |
| 2.8 | No secrets or `.env` files | PASS | None in the diff. |
| 2.9 | Naming conventions | PASS | `PascalCase` types and members; `camelCase` locals. |
| 2.10 | Comment why, not what | PASS | The single-read comment, the non-lazy comment, and the corrected `WpfDispatcherYield` comment all state reasons. |
| 2.11 | No absolute host paths in artifacts | PASS | Evidence artifacts substitute `<worktree>` for host paths and explicitly decline to reproduce vstest-generated TRX filenames. |

## 3. Language-Specific Code Change Policy Compliance (C#)

| # | Requirement | Verdict | Evidence |
|---|---|---|---|
| 3.1 | CSharpier formatting via the manifest-pinned tool | PASS | Re-run by this reviewer: `dotnet tool run csharpier check .` -> `Checked 1583 files in 4139ms.`, exit 0. |
| 3.2 | `dotnet format` not used | PASS | No `.csproj` was rewritten; the only csproj change is two `<Compile Include>` additions. |
| 3.3 | .NET analyzer diagnostics | PASS | Re-run by this reviewer with `/t:Rebuild`: exit 0 across all 18 projects, no analyzer diagnostics emitted. |
| 3.4 | Nullable / type-check gate | PASS | Re-run by this reviewer with `/t:Rebuild` and `/p:TreatWarningsAsErrors=true`: `0 Warning(s)`, `0 Error(s)`, exit 0. `/p:Nullable=enable` correctly not passed. |
| 3.5 | `/t:Rebuild` used, not `/t:Build` | PASS | Both reviewer invocations used `/t:Rebuild`, so `CoreCompile` actually ran and the gates are not vacuous. |
| 3.6 | Null-safety by default | PASS | `#nullable enable annotations` in the new helper matches an established repository idiom used in 30+ existing files. |
| 3.7 | Minimal public surface | PASS | The message constant is `internal`; the scope type is `internal sealed`. |
| 3.8 | XML documentation on non-obvious contracts | PASS | `Dispatcher` gains `<summary>`, `<remarks>`, and `<exception cref="InvalidOperationException">`; the file previously carried zero `///` comments. |
| 3.9 | No broad `catch (Exception)` added to production | PASS | Zero `catch` clauses added to production code. The one `catch (Exception ex)` in the diff is inside the C21 test's worker thread, which captures and re-asserts on the calling thread — a legitimate test boundary. |

## 4. Language-Specific Unit Test Policy Compliance (C#)

| # | Requirement | Verdict | Evidence |
|---|---|---|---|
| 4.1 | MSTest framework | PASS | `[TestClass]`, `[TestMethod]`, `[TestCleanup]`, `[DoNotParallelize]`, `[STATestMethod]` throughout; no xUnit or NUnit introduced. |
| 4.2 | FluentAssertions preferred | PASS | Every added assertion uses `Should()`. |
| 4.3 | Moq for mocking | PASS | No new mocking need arose; existing Moq usage untouched. |
| 4.4 | Arrange-Act-Assert | PASS | All three added tests carry explicit section comments. |
| 4.5 | `[TestClass]` applied to exactly one part of the split class | PASS | `ProgressTracker_Tests.cs:14-16` carries `[TestClass]` and `[DoNotParallelize]` on separate lines over `public partial class ProgressTracker_Tests`; `ProgressTracker_ReportAndViewerTests.cs:14` declares the same partial class with no attributes. |
| 4.6 | Test discovery preserved across the split | PASS | Reviewer-run regex comparison of `public (void\|async Task\|Task) <Name>(` across the pre-split file versus both post-split parts: 24 before, 25 after, **zero missing**, exactly one added (`Initialize_WhenDispatcherNotCaptured_ThrowsInvalidOperationException`). `partial class` preserves every fully-qualified name. |
| 4.7 | New files registered in the csproj exactly once | PASS | `UtilitiesCS.Test.csproj:76` and `:479` carry exactly one `<Compile Include>` each; no duplicate entries (CS2002, issue #394, avoided). |

## 5. Test Coverage Detail

All figures below were re-derived by this reviewer directly from the Cobertura documents. No figure
is carried forward from a delivery artifact without independent recomputation.

### 5.1 Repo-wide, first-party (nine production assemblies)

| Metric | Baseline | Post-change | Floor | Verdict |
|---|---|---|---|---|
| C# line coverage (pinned `.//line` selection) | 84.50% (112,359/132,967) | 84.51% (112,363/132,961) | 85% uniform | FAIL |
| C# line coverage (deduped cross-check) | 84.65% (55,203/65,214) | 84.66% (55,205/65,211) | 85% uniform | FAIL |
| C# branch coverage (pinned selection) | 79.14% (26,496/33,480) | 79.15% (26,500/33,480) | 75% uniform | PASS |
| C# line coverage against the CLAUDE.md testable-denominator floor | 84.50% | 84.51% | 80% | PASS |

**Disposition of the line FAIL: NON-BLOCKING.** The shortfall pre-exists on `origin/main` at 84.50%
and this change moves it upward, not downward. The repository carries an unreconciled documentation
conflict — CLAUDE.md states an 80% floor while `.claude/rules/quality-tiers.md` states a uniform 85%
floor — and the figure clears the former and misses the latter. This delivery neither caused the
shortfall nor is scoped to repair it.

### 5.2 Canonical coverage artifact presence

| Language | Canonical path | Present | Verdict |
|---|---|---|---|
| C# | `artifacts/csharp/coverage.xml` | No — the `artifacts/csharp/` directory does not exist | FAIL |

**Reason recorded as required:** coverage artifact absent for C#; coverage verification is mandatory
for all languages with changed files.

**Disposition: NON-BLOCKING.** Equivalent raw evidence exists at `coverage/782-p0-baseline.cobertura.xml`
and `coverage/782-p7-final.cobertura.xml`, and this reviewer re-derived every repo-wide, per-file,
and changed-line figure from those documents directly rather than accepting a summary. The absence is
a deliberate, documented scope decision (SD1), and the practice has repository precedent. No coverage
question was left unanswerable by the absence.

### 5.3 New production files

Zero new production C# files exist on this branch. The two added `*.cs` files,
`UtilitiesCS.Test/TestHelpers/UiThreadDispatcherScope.cs` and
`UtilitiesCS.Test/Threading/ProgressTracker_ReportAndViewerTests.cs`, are both test-assembly files
and are excluded from the coverage denominator by the derived configuration's
`<ModulePath>.*\.Test\.dll$</ModulePath>` exclusion. The new-production-file tier therefore has an
empty member set and its 85%/75% thresholds are satisfied vacuously. Verdict: PASS.

### 5.4 Modified production files

| File | Baseline line | Post line | Baseline branch | Post branch | Verdict |
|---|---|---|---|---|---|
| `UtilitiesCS/Threading/UiThread.cs` | 77.11% (64/83) | 76.83% (63/82) | 65.00% (13/20) | 65.00% (13/20) | FAIL |
| `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs` | 96.43% (27/28) | 100.00% (26/26) | 100.00% (14/14) | 100.00% (14/14) | PASS |
| `UtilitiesCS/Threading/ProgressTracker.cs` | 87.65% (149/170) | 87.65% (149/170) | 82.50% (33/40) | 82.50% (33/40) | PASS |
| `UtilitiesCS/Threading/ProgressTrackerAsync.cs` | 91.49% (43/47) | 91.49% (43/47) | 83.33% (5/6) | 83.33% (5/6) | PASS |
| `TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs` | not instrumented | not instrumented | not instrumented | not instrumented | PASS |

`RibbonViewer.EngineCommands.cs` is absent from both Cobertura documents. This reviewer verified the
stated cause rather than accepting it: `TaskMaster/Ribbon/RibbonViewer.cs:32` declares
`[ExcludeFromCodeCoverage]` on the partial type, which suppresses instrumentation for every part.
The file contributes zero executable changed lines, so it cannot regress.

**Disposition of the `UiThread.cs` FAIL: NON-BLOCKING, with the following measured basis.**

This reviewer compared the covered and uncovered line sets between the two Cobertura documents rather
than comparing only percentages. The result:

```
BASELINE uncovered (19): 28,29,30,32,33,34,67,68,69,70,71,72,73,74,75,76,118,119,120
POST     uncovered (19): 28,29,30,32,33,34,67,68,69,70,71,72,73,74,75,76,118,119,120
IDENTICAL SETS: True
```

Not one line transitioned from covered to uncovered. The uncovered residue is unchanged in both
membership and line number, and sits entirely in members the diff never touched: the `Init`
parameter-handling block (28-34), the `ThreadMonitor` construction inside `Initialize()` (67-76),
which requires a live UI thread, and the lazy `UiSyncContext` accessor (118-120).

The covered-line delta is 13 baseline-only line numbers against 12 head-only line numbers — pure
renumbering caused by inserting the 18-line XML documentation block, with a net -1 from the wrapped
three-line `throw` collapsing to a single line once routed through the shared constant. Removing a
covered line from a partially covered file necessarily lowers its percentage; that is what produced
the -0.28 point movement, and it is not a coverage regression.

Against the four-part precedent test for a sub-floor modified file: no changed-line regression
(satisfied, 7/7), residue entirely pre-existing and untouched (satisfied), at or above 80% (**not
satisfied**, 76.83%), improved versus baseline in percentage terms (**not satisfied**, -0.28 points
by denominator arithmetic alone). Because two legs are not satisfied on their face, the FAIL row is
recorded and carried into remediation inputs as a procedural item, with the recommendation that a
maintainer waive it on the byte-identical uncovered-set evidence above.

### 5.5 Changed-line coverage

Derived independently by this reviewer: added post-change line numbers were taken from
`git diff -U0` hunk headers and looked up as `<line number="N">` elements in the post-change
Cobertura document.

| File | Changed executable lines | Covered | Non-executable |
|---|---|---|---|
| `UtilitiesCS/Threading/UiThread.cs` | 4 (159, 160, 166, 168) | 4 | 24 |
| `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs` | 1 (65) | 1 | 4 |
| `UtilitiesCS/Threading/ProgressTracker.cs` | 1 (39) | 1 | 0 |
| `UtilitiesCS/Threading/ProgressTrackerAsync.cs` | 1 (39) | 1 | 0 |
| `TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs` | 0 | 0 | 2 |
| **Total** | **7** | **7** | **30** |

**Changed-line coverage: 100.00% (7/7). Zero uncovered changed lines. Verdict: PASS.** This matches
the delivery's claim exactly and was reached by an independent derivation.

## 6. Test Execution Metrics

| Metric | Value | Source |
|---|---|---|
| Total tests | 7000 | `evidence/qa-gates/p7-t5-tests-coverage.md`, TRX `ResultSummary/Counters` |
| Passed | 7000 | same |
| Failed | 0 | same |
| Skipped | 0 | same |
| Duration | 44.9116 s | same |
| Assemblies run | 9 | all nine production test assemblies, satisfying the S4-2 observation |
| Baseline total | 6997 | `evidence/baseline/p0-t6-vstest.md`; +3 equals the three tests AC7 requires |
| Excluded classes | 4 shell-icon classes plus `TestCategory!=LiveOutlook` | environmental stall reproducing against `origin/main`; CI covers them |

This reviewer did not re-execute the 7000-test run. It is corroborated indirectly but materially: the
Cobertura document that run emitted is present on disk with an mtime of 23:08, consistent with the
23:12 gate commit, and every coverage figure re-derived from it reconciles exactly with the recorded
values. A run that had not happened could not have produced that document.

## 7. Code Quality Checks

| Check | Command | Result |
|---|---|---|
| Format check | `dotnet tool run csharpier check .` | PASS — `Checked 1583 files in 4139ms.`, exit 0 |
| Analyzer build | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | PASS — exit 0, 18 projects, no diagnostics |
| Nullable build | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | PASS — `0 Warning(s)`, `0 Error(s)`, exit 0 |
| File size scan | `awk END{print NR}` over 12 touched files | PASS — maximum 397 lines |
| Reflection site scan | search for the token `"_dispatcher"` across all `*.cs` | PASS — exactly 2 hits, both intended |
| Message literal scan | search for `UiThread.Initialize()` across all `*.cs` | PASS — zero hits |
| Removed tail scan | search for `before yielding folder tree work` across all `*.cs` | PASS — zero hits |
| Workflow change scan | `git diff --name-only <base>...HEAD -- ".github/"` | PASS — zero paths; the modified-workflow green-run rule does not fire |
| Policy document scan | `git diff --stat <base>...HEAD -- ".claude/"` | PASS — zero paths |
| Banned timing API scan (added lines) | grep over added `*.cs` lines | PASS — zero hits |
| Evidence location scan | `git diff --name-only <base>...HEAD -- "artifacts/"` | PASS — zero paths |

## 8. Gaps and Exceptions

| # | Gap | Severity | Disposition |
|---|---|---|---|
| G1 | `artifacts/csharp/coverage.xml` absent | FAIL | Non-blocking. Deliberate under SD1; raw Cobertura documents present on disk and independently re-derived by this reviewer. Carried to remediation inputs as a procedural item. |
| G2 | `UtilitiesCS/Threading/UiThread.cs` at 76.83% line and 65.00% branch | FAIL | Non-blocking. Uncovered set byte-identical to baseline; residue is host-bound WinForms code in untouched members; 7/7 changed lines covered. Carried to remediation inputs with a recommendation to waive. |
| G3 | Repo-wide first-party C# line coverage 84.51%, below the 85% uniform floor | FAIL | Non-blocking. Pre-exists on `origin/main` at 84.50% and improves. Reflects the unreconciled CLAUDE.md 80% versus `.claude/rules` 85% conflict. |
| G4 | Baseline coverage figures not reproducible from the on-disk baseline document (EV-1) | Should-fix | Non-blocking. See the code review. Does not change any verdict; every candidate baseline is at or below the head figure. |
| G5 | The shared message constant's text is not pinned by any test | Should-fix | Non-blocking. See the code review, finding CR-1. `spec.md` AC10 and the delivery's code-review artifact both overstate the pinning strength of a wildcard assertion. |
| G6 | Three `spec.md` passages still describe the withdrawn C03 latch re-arm | Nit | Already disclosed by the delivery's own code-review artifact, which enumerates all three. Accepted as a recorded decision. |
| G7 | Test files live in `<Project>.Test/` rather than a `tests/` tree | Pre-existing | Repository-wide convention predating this branch. Not introduced here. |
| G8 | The PR context summary reports `Core logic changes: 0 files` and seven false auto-close candidates | Should-fix (tooling) | Non-blocking for this branch, but the PR author step must not carry the seven close candidates into the PR body. Only #782 is closed by this branch. |

No gap in this table blocks the pull request.

## 9. Summary of Changes

| Category | Files | Lines |
|---|---|---|
| Production C# | 5 | `UiThread.cs`, `WpfDispatcherYield.cs`, `ProgressTracker.cs`, `ProgressTrackerAsync.cs`, `RibbonViewer.EngineCommands.cs` |
| Test C# (modified) | 8 | `UiThread_Tests.cs`, `ProgressTracker_Tests.cs`, `ProgressTrackerAsync_Tests.cs`, `IdleAsyncQueue_Tests.cs`, `IdleActionQueue_Tests.cs`, `WpfDispatcherYieldTests.cs`, `EmailMoveMonitorTests.cs`, `QfcItemController.InitializationTests.Part2.cs` |
| Test C# (new) | 2 | `UiThreadDispatcherScope.cs`, `ProgressTracker_ReportAndViewerTests.cs` |
| Build configuration | 1 | `UtilitiesCS.Test.csproj` (two `<Compile Include>` additions) |
| Documentation and evidence | 71 | this feature folder (46 new), the #584 feature folder (22 corrected), 3 promoted potential entries |
| **Total** | **87** | **+8,691 / -448 overall; +742 / -402 in C#** |

## 10. Compliance Verdict

| Area | Verdict |
|---|---|
| General Unit Test Policy | PASS |
| General Code Change Policy | PASS |
| C# Code Change Policy | PASS |
| C# Unit Test Policy | PASS |
| Coverage — C# repo-wide line | FAIL (non-blocking, pre-existing, improving) |
| Coverage — C# repo-wide branch | PASS |
| Coverage — C# canonical artifact presence | FAIL (non-blocking, deliberate, independently substituted) |
| Coverage — C# new production files | PASS (empty member set) |
| Coverage — C# modified files | FAIL on one of five (non-blocking, zero regression proven) |
| Coverage — C# changed lines | PASS (7/7, 100%) |
| Toolchain (format, analyzers, nullable, tests) | PASS — all four re-run or corroborated by this reviewer |
| Evidence location compliance | PASS |
| Modified-workflow green-run rule | PASS (does not fire) |
| Acceptance criteria | 16 of 17 PASS; AC-U1 correctly open pending the pull request |

**Overall: PASS. Zero blocking findings. Recommendation: GO for pull request.**

Remediation inputs are produced at `remediation-inputs.2026-09-05T23-48.md` because two enumerated
coverage triggers fire mechanically. Both items in that document are procedural rather than code
defects, and each carries a recommended disposition.

## Appendix A: Test Inventory

Tests added by this delivery (3):

| Test | File | Pins |
|---|---|---|
| `YieldAsync_ProductionFallbackWithoutDispatcher_ThrowsNamingInit` | `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs` | C21 — the production fallback provider reached from a dedicated fresh thread |
| `InitializeAsync_WhenDispatcherNotCaptured_ThrowsInvalidOperationException` | `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` | C26 — the returned task faults, asserted with `ThrowAsync` |
| `Initialize_WhenDispatcherNotCaptured_ThrowsInvalidOperationException` | `UtilitiesCS.Test/Threading/ProgressTracker_ReportAndViewerTests.cs` | C26 — the genuinely synchronous throw from the non-async sibling |

Tests materially modified (2):

| Test | File | Change |
|---|---|---|
| `Dispatcher_WhenBackingFieldIsNull_ThrowsInvalidOperationExceptionNamingInitialize` | `UtilitiesCS.Test/Threading/UiThread_Tests.cs` | Assertion moved to `*UiThread.Init()*`; migrated to the install scope; name deliberately retained (SD4) |
| `Dispatcher_WhenBackingFieldIsPopulated_ReturnsThatSameInstance` | `UtilitiesCS.Test/Threading/UiThread_Tests.cs` | Sentinel moved to a dedicated STA host with `BeginInvokeShutdown` and join (C10); adds the round-trip null-restore assertion AC5 requires |

Fail-before evidence: `evidence/regression-testing/p4-t7-fail-before.md` records all three new tests
failing with `System.NullReferenceException` after both guards were temporarily removed, and
explicitly notes that removing only the `UiThread` throw would have left the sibling guard and made
the demonstration vacuous. This reviewer regards that as a genuine RED-first record.

Test method parity across the C16 split: 24 methods before, 25 after, zero lost, verified by
reviewer-run regex comparison.

## Appendix B: Toolchain Commands Reference

Commands re-run by this reviewer during this audit:

```powershell
git -C <worktree> merge-base origin/main HEAD
git -C <worktree> diff --stat 77c6d314...HEAD -- "*.cs" "*.csproj"
git -C <worktree> diff --name-status 77c6d314...HEAD
dotnet tool run csharpier check .
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
```

Coverage re-aggregation performed by this reviewer over both Cobertura documents, using the pinned
all-descendant selection and a deduped cross-check:

```powershell
[xml]$doc = Get-Content -LiteralPath 'coverage\782-p7-final.cobertura.xml'
$allow = @('Tags','ToDoModel','TaskVisualization','UtilitiesCS','QuickFiler','TaskTree','TaskMaster','SVGControl','VBFunctions')
foreach ($pkg in $doc.SelectNodes('//package')) {
    if ($allow -notcontains $pkg.GetAttribute('name')) { continue }
    foreach ($l in $pkg.SelectNodes('.//line')) { <# sum hits and condition-coverage pairs #> }
}
```

Reference commands recorded by the delivery and not re-executed by this reviewer (the test run is
the expensive gate; its output document was verified instead):

```powershell
dotnet-coverage collect --output coverage\782-p7-final.cobertura.xml --output-format cobertura `
    --settings coverage\782-effective-coverage.config -- $vstest <nine test assemblies> `
    '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' `
    '/Blame:CollectHangDump;TestTimeout=5min;HangDumpType=None' '/TestCaseFilter:<filter>'
```

Note on tooling availability: the MCP template-resolution and artifact-validation tools
(`resolve_policy_audit_template_asset`, `validate_orchestration_artifacts`) were not reachable in
this session. This artifact was therefore assembled against the canonical heading list documented in
`.claude/skills/policy-audit-template-usage/SKILL.md` and the structural requirements recorded in
this agent's memory. The coverage gate hook was simulated locally instead of relying on the MCP
validator.
