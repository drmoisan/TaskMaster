# Policy Audit: csharp-latent-hazards-uithread-ilglobals-comments (Issue #930)

- Timestamp (caller-supplied artifact stamp): 2026-09-29T00-45
- Review authored: 2026-09-29 (after the executor's terminal evidence at 2026-09-29T09-26; the artifact stamp was supplied by the caller and is retained for hook cross-artifact matching)
- Branch: `bug/csharp-latent-hazards-uithread-ilglobals-comments-930`
- Base: `origin/main`; execution self-anchor BASE-SHA `ac819907f479ee18026993054e714dc2e056142f` (evidence/baseline/tree-anchor.md)
- Work mode: `minor-audit` (issue.md marker). AC source: issue.md `## Acceptance Criteria` (AC1 to AC7).
- Reviewer: feature-review agent, reduced (small-path) audit
- Review worktree: `<repo-root>/.claude/worktrees/agent-ab7f72be619adf22f`

## Template Resolution Deviation

The MCP tools `resolve_policy_audit_template_asset` and `validate_orchestration_artifacts` are not on this agent's tool surface, and the caller prohibited the Bash tool for this review. The artifact was hand-authored preserving the twelve canonical major headings from `.claude/skills/policy-audit-template-usage/SKILL.md`. PR-context artifacts (`artifacts/pr_context.summary.txt` and `.appendix.txt`) do not exist in the review worktree; scope was taken from the caller-supplied `git diff origin/main...HEAD` and independently verified against evidence/qa-gates/footprint.md (the anchored name-only diff lists exactly six code paths) and against the six files on disk.

## Rejected Scope Narrowing

None detected. The caller's instruction to perform a "reduced (minor-audit / small-path) review" is the legitimate work mode persisted in issue.md, not a narrowing. The caller's diff paste "excluding docs" was supplemented by reading the entire feature folder (issue.md, plan, all 44 evidence files); footprint.md confirms the docs portion of the branch diff is confined to the feature folder and `.claude/agent-memory/`. The caller's severity rule ("Do not raise a Blocking finding for anything outside this diff's footprint") governs severity classification only and was applied as written.

## Evidence Location Compliance

- Branch diff outside the feature folder and `.claude/agent-memory/` lists exactly six code paths (evidence/qa-gates/footprint.md, first command); no path under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/` or `artifacts/coverage/` appears anywhere in the branch diff. Verdict: PASS.
- All evidence produced by the executor lives under `<FEATURE>/evidence/baseline/`, `<FEATURE>/evidence/regression-testing/` and `<FEATURE>/evidence/qa-gates/` (44 files, enumerated in evidence/qa-gates/reduced-audit-handoff.md and confirmed by directory listing).
- `validate_evidence_locations.py` does not exist in this repository; the manual substitute (review of the anchored name-only diff) was applied.
- No EVIDENCE_LOCATION_OVERRIDE_REJECTED event: no caller instruction supplied a non-canonical evidence path.
- This review's three artifacts are written into the feature folder in the review worktree. Copies at the identical relative path under the session checkout exist solely because the SubagentStop hook resolves advertised paths relative to its own cwd; the copies are untracked collateral to be deleted after merge and are not evidence.

## Executive Summary

**Overall verdict: PASS with one Non-blocking finding.** Blocking: 0. Non-blocking: 1. Informational: 9.

The branch delivers three independent latent-defect fixes in C#: a null guard on the dispatcher exit of `UiThread.SynchronizationContextAwaiter.IsCompleted` (#889), deletion of two public mutable static fields from `SDILReader.ILGlobals` (#863), and removal of stale numeric line counts from two XML doc comments (#862). Each fix is preceded by a regression test recorded failing against unmodified source (#889, #863) or by a token census (#862). The full C# toolchain passed in one clean iteration (CSharpier check over 1623 files, analyzer Rebuild 0 errors and 0 warnings, TreatWarningsAsErrors Rebuild 0 errors, 7322 of 7322 tests passed). First-party C# coverage moved from 85.31% lines / 79.71% branches to 85.32% lines / 79.73% branches; the single changed executable production line is covered and its condition coverage reads 4/4. The one Non-blocking finding is that two committed evidence files carry the absolute install path of `vstest.console.exe` under Program Files, which contradicts the literal text of AC7 ("no committed file contains an absolute host path") and the plan's own placeholder instruction, while disclosing neither the account name nor the host name. AC7 was unchecked in issue.md; the remedy is a placeholder substitution on two lines.

Coverage floors applied: CLAUDE.md governs C# (80% line / 75% branch repo-wide, 90% target for new code, no regression on changed lines); `.claude/rules/general-unit-test.md` and `quality-tiers.md` state 85% line / 75% branch. Both floor sets are satisfied by the post-change first-party figures, so the documented conflict between the two floor statements does not affect this verdict.

## 1. General Unit Test Policy Compliance

### 1.1 Core Principles

| Principle | Verdict | Evidence |
|---|---|---|
| Independence | PASS | The #889 test is added to `UiThreadPredicateHardening_Tests`, which already carries `[DoNotParallelize]` because it installs process-global `UiThread` statics through `UiThreadStateScope`; the scope is entered and disposed per test. The two #863 tests are reflection-only reads of `typeof(ILGlobals)`. No new attribute, no worker-count change (runsettings SHA-256 unchanged; evidence/qa-gates/concurrency-regime.md). |
| Isolation | PASS | Each new test targets one predicate exit or one structural property. |
| Fast execution | PASS | Scoped runs complete in seconds (evidence/regression-testing/889-pass-after.md, 863-pass-after.md); no sleeps, delays or retries added (`ADDED_Thread.Sleep=0`, `ADDED_Task.Delay=0`, `ADDED_Retry=0`). |
| Determinism | PASS | The #889 test creates its own STA dispatcher host and MTA execution thread and joins both (existing `SharedStaDispatcherHost` and `ApartmentThreadRunner` helpers in UtilitiesCS.Test); no wall-clock waits; no temporary files (`ADDED_GetTempFileName=0`, `ADDED_GetTempPath=0`). |
| Readability | PASS | Descriptive names; XML doc comments state scenario and expected outcome; Arrange/Act/Assert comments present. |

### 1.2 Coverage and Scenarios

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---|---|---|---|---|---|
| C# | 6 | 7322 | 7322 passed / 0 failed | 85.31% lines / 79.71% branches | 85.32% lines / 79.73% branches | 100% lines (1 of 1 changed executable line) |
| TypeScript | 0 | N/A | N/A | N/A | N/A | N/A |
| Python | 0 | N/A | N/A | N/A | N/A | N/A |
| PowerShell | 0 | N/A | N/A | N/A | N/A | N/A |

C# coverage verdict: PASS (repo-wide first-party 85.32% lines and 79.73% branches; per-file UiThread.cs 97.76%, ILGlobals.cs 94.74%; changed-line coverage 100%; no regression on changed lines).

Per-file detail (Cobertura parse transcribed in evidence/baseline/baseline-04-mstest-coverage.md and evidence/qa-gates/final-06-mstest-coverage.md, lines unioned by number with maximum hits):

- `UtilitiesCS/Threading/UiThread.cs`: baseline 130/133 lines (97.74%), post-change 131/134 (97.76%); uncovered lines 38, 39, 40 in both runs (outside the changed region). Return line 197 (baseline) condition 100% (2/2) became line 198 (post-change) condition 100% (4/4); the inserted operand line 199 has HITS=1.
- `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs`: baseline 38/40 lines (95.00%), post-change 36/38 (94.74%); uncovered count 2 in both runs (the same two statements, shifted up by the three deleted lines). The percentage decrease is the arithmetic effect of deleting two covered field-initializer lines; the diff for this file is deletions only, so there is no changed-line regression.
- `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Search.cs` and `BreadcrumbItemViewerLifecycleCoordinator.Search.cs`: comment-only edits; no executable line changed.
- Test files (`UtilitiesCS.Test/...`) are excluded from the coverage denominator by the runner's first-party filter.

Reviewer cross-check of the committed projections: the nine package counters in evidence/baseline/baseline-coverage.jacoco.xml sum to 56079 covered of 65737 lines and 13593 of 17052 branches; evidence/qa-gates/final-coverage.jacoco.xml sums to 56084 of 65736 and 13597 of 17054. Both sums equal the runner's first-party summary lines verbatim. The only package whose counters changed is UtilitiesCS (missed 4614 to 4608, covered 38810 to 38815). The changed files account for covered minus 1 and missed 0, so six lines elsewhere in UtilitiesCS flipped from missed to covered between the two runs (0.009 percentage points); this is within the known run-to-run variance of the full suite and is not attributable to the diff.

Method note on the four excluded test classes (plan Decision D3): both local coverage runs excluded `ShellUtilities_Tests`, `ShellUtilitiesStatic_Tests`, `SysImageListHelperTests` and `OSBrowser_Tests` via an identical `FullyQualifiedName!~` filter because they call the Windows shell icon API, which hangs on this workstation. The exclusion is identical on both sides of the comparison, so the delta is sound; the absolute first-party figure is an under-approximation of the CI figure, and the repository `mstest-coverage` workflow runs the four classes unfiltered on every pull request. The exclusion is a test-selection filter on the local runs, not a coverage `exclude` entry on a production path, so the Coverage Exclusion Policy blocking rule is not engaged.

### Coverage Evidence Checklist

- C# baseline coverage artifact: `evidence/baseline/baseline-coverage.jacoco.xml` (package-level JaCoCo projection) plus `evidence/baseline/baseline-04-mstest-coverage.md` (first-party summary line and per-file Cobertura parse)
- C# post-change coverage artifact: `evidence/qa-gates/final-coverage.jacoco.xml` plus `evidence/qa-gates/final-06-mstest-coverage.md`
- TypeScript baseline coverage artifact: `N/A - zero TypeScript files changed on this branch`
- TypeScript post-change coverage artifact: `N/A - zero TypeScript files changed on this branch`
- PowerShell baseline coverage artifact: `N/A - zero PowerShell files changed on this branch`
- PowerShell post-change coverage artifact: `N/A - zero PowerShell files changed on this branch`
- Python baseline coverage artifact: `N/A - zero Python files changed on this branch`
- Python post-change coverage artifact: `N/A - zero Python files changed on this branch`
- Per-language comparison summary: section 1.2.1 of this document

Note on the canonical artifact path: `artifacts/csharp/coverage.xml` does not exist in the review worktree (no `artifacts/` directory exists there). The raw Cobertura document is written by the runner under the gitignored `coverage/` directory, and CLAUDE.md's Committed Test Evidence Format prohibits committing it in any form. The committed package-level JaCoCo projection plus the transcribed per-file parse are treated as the coverage artifact for this review, consistent with prior reviews of this repository. A `artifacts/csharp/coverage.xml` present in the session checkout is a stale Cobertura document from a different branch (lines-valid 64740, timestamp 2026-09-05) and was not used as evidence.

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 85.31% lines (56079/65737) / 79.71% branches (13593/17052) -> Post-change: 85.32% lines (56084/65736) / 79.73% branches (13597/17054). Change: +0.01% lines / +0.02% branches. New/changed-code coverage: 100%. Disposition: PASS. Evidence: evidence/qa-gates/coverage-comparison.md; evidence/baseline/baseline-coverage.jacoco.xml; evidence/qa-gates/final-coverage.jacoco.xml.
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.
- PowerShell: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero PowerShell files changed on this branch.

### 1.2.2 Coverage Artifact State

The comparison above uses the executor's same-session baseline and final runs (identical runner, identical filter, identical machine), which is the comparison basis this repository's prior reviews require because cross-session repo-wide constants vary by roughly 0.015 points.

### 1.3 Scenario Completeness

| Scenario class | Verdict | Evidence |
|---|---|---|
| Positive flow | PASS | Pre-existing `IsCompleted_OnTheThreadThatOwnsTheCapturedDispatcherWithTheCapturedUiContext_ReturnsTrue` and `..._OnOwningUiThreadWithADispatcherContextCapturedInsideAnInvoke_ReturnsTrue` exercise the true outcome of the dispatcher exit (condition 4/4 post-change). |
| Negative flow | PASS | New `IsCompleted_WhenTheAwaiterContextIsAForeignDispatcherContextAndNoUiDispatcherWasCaptured_ReturnsFalse` exercises the null-dispatcher outcome; pre-existing `..._WhenTheDispatcherContextBelongsToADifferentThreadsDispatcher_ReturnsFalse` exercises the false reference outcome. |
| Edge / boundary | PASS | The null-equals-null shape is the boundary case the fix addresses. #863: `PublicStaticFields_AreExactlyTheTwoOpCodeTables` pins the surface by name. |
| Error handling | PASS | Not applicable to the changed lines (no exception path added); existing `ArgumentNullException` constructor guard unchanged. |
| Concurrency | PASS | The new #889 test runs on an explicit MTA thread with a separate STA dispatcher host; process-global state is scoped and serialized by the class-level attribute already present. |
| State transitions | PASS | Not applicable; no stateful component changed. |

### 1.4 Test Structure and Diagnostics

PASS. All three new tests use Arrange/Act/Assert with comments, descriptive names, XML doc summaries, and FluentAssertions with because-clauses (`OnlyContain(..., "a public static field that is not readonly is a process-wide mutable publication point ...")`, `BeEquivalentTo(..., "issue #863 removed the two writable fields ...")`).

### 1.5 External Dependencies and Environment

PASS. No network, database, file, process or temporary-file dependency added. The WPF `Dispatcher` used by the #889 test is created in-process on a dedicated thread by an existing test helper.

## 2. General Code Change Policy Compliance

| Section | Verdict | Evidence |
|---|---|---|
| Bugfix workflow (failing test first, minimal fix, verify) | PASS | #889: 889-fail-before.md (failed 1 of 3, `Expected observed to be False, but found True`, production source unmodified) then 889-fix-applied.md (2 added / 0 deleted) then 889-pass-after.md (128/128). #863: 863-fail-before.md (failed 2 of 15) then 863-fix-applied.md (0 added / 3 deleted) then 863-pass-after.md (15/15). #862: comment-only edit gated by token census (862-comment-edit.md). |
| Design principles (simplicity, reusability, extensibility, separation) | PASS | The fix is a single boolean operand mirroring the sibling exit; dead members removed rather than retained. |
| Classes, functions, APIs | PASS | No new type or method in production code. |
| Error handling and logging | PASS | No error-handling or logging change. |
| Module and file structure (500-line limit) | PASS | Post-format counts (evidence/qa-gates/final-03-file-size.md): UiThread.cs 308, ILGlobals.cs 225, BreadcrumbBridgeCoordinator.Search.cs 102, BreadcrumbItemViewerLifecycleCoordinator.Search.cs 45, UiThreadApartmentMeasurement_Tests.cs 206 (baseline 164), ILGlobals_Tests.cs 315 (baseline 270). All six at most 500; no file crossed the limit. |
| Naming, docs, comments | PASS | The inserted comment states why ("null must never match null"), not what. The #862 edit keeps the explanatory sentence while removing the drift-prone numeral. |
| Performance, I/O, dependencies | PASS | No dependency change; footprint.md second command printed nothing (no `.csproj`, `.sln`, `.runsettings`, `packages.config` or `*.config` changed). |
| Interaction with existing code (style match; breaking public API called out; existing tests as spec) | PASS with obligation | The removal of `ILGlobals.Cache` and `ILGlobals.modules` is a breaking public API change. In-repo callers: none (solution-wide Rebuild 0 Error(s) across 18 projects after deletion, 863-build-green.md; executor `git grep` over name, quoted-string and single-quoted forms found no `.cs` hit, 863-reference-search.md; reviewer Grep over `*.cs` for `ILGlobals\.(Cache|modules)\b` returned zero matches). The change is called out in plan Decision D4 and in reduced-audit-handoff.md; the policy's "call it out clearly in the change description" obligation is discharged only when the PR body names both removed members. The deleted `Cache_IsInitialized` test asserted only the existence of the deleted member and is replaced by two structural tests, so the existing-tests-as-spec rule is satisfied. |
| After making changes (toolchain loop) | PASS | evidence/qa-gates/toolchain-final-pass.md: `LOOP: CLEAN PASS`, one iteration, no source-rewrite commits. |
| Architecture boundaries | PASS | No new reference to VSTO, Outlook Interop, COM visibility or Ribbon callbacks. `UiThread.cs` already references `System.Windows.Threading` (WPF), unchanged. |

## 3. Language-Specific Code Change Policy Compliance

C# Code Change Policy (CLAUDE.md and `.claude/rules/csharp.md`):

| Item | Verdict | Evidence |
|---|---|---|
| CSharpier via `dotnet tool run` | PASS | final-01-csharpier-format.md: write-mode run left the anchored patch identical (`FORMAT_CHANGED_OWNED_PATCH=False`, no new paths); final-02-csharpier-check.md: `CHECK_EXIT=0`, Checked 1623 files. |
| Analyzer Rebuild (`/t:Rebuild`, `EnableNETAnalyzers`, `EnforceCodeStyleInBuild`) | PASS | final-04-analyzers.md: `MSBUILD_EXIT=0`, `0 Error(s)`, `0 Warning(s)`, `CSC_TASK_LINES=18` (non-vacuous; all 18 projects compiled). Baseline warnings 0. |
| Nullable Rebuild (`/t:Rebuild`, `TreatWarningsAsErrors=true`, no `/p:Nullable=enable`) | PASS | final-05-nullable.md: `MSBUILD_EXIT=0`, `0 Error(s)`, `CS86_ERROR_LINES=0`. `UiThread.cs` and `ILGlobals.cs` both carry `#nullable enable`; `_dispatcher is not null` on a `Dispatcher?` field. Intermediate red/green compiles also used the Rebuild target (plan Decision D6). |
| Type safety and null safety | PASS | The added guard is the null-safety fix itself. |
| Public surface minimal and intentional | PASS | Two public mutable statics removed; the two remaining public static fields are `readonly`, now pinned by test. |
| XML docs synchronized with behavior | PASS | The two #862 comments no longer state a numeral that can drift. |
| Analyzer stack wiring | PASS (pre-existing environment note) | No tracked analyzer wiring changed. The `<Analyzer Include>` items in UtilitiesCS.csproj (line 1316, Meziantou.Analyzer 3.0.235), VBFunctions.csproj and SVGControl.Test.csproj (MSTest.Analyzers 4.4.0) name versions that differ from packages.config (3.0.290, 4.4.1), which is on origin/main (reviewer-verified in the worktree: the same csproj imports `Meziantou.Analyzer.3.0.290\build\...props` at line 3 while the Analyzer item names 3.0.235). See section 8. |
| Prohibited behaviors (broad refactors, weakened assertions, sleeps/retries) | PASS | Diff confined to six files; assertions strengthened, not weakened; concurrency-regime.md shows zero added prohibited tokens. |

## 4. Language-Specific Unit Test Policy Compliance

| Item | Verdict | Evidence |
|---|---|---|
| MSTest framework, `[TestClass]`/`[TestMethod]` | PASS | All three new tests use `[TestMethod]` in existing `[TestClass]` classes. |
| Moq for mocking | PASS | No mocking needed; none introduced. |
| FluentAssertions | PASS | `Should().BeNull()`, `Should().BeFalse()`, `Should().NotBeEmpty(...)`, `Should().OnlyContain(...)`, `Should().BeEquivalentTo(...)`. |
| Test placement | PASS | Tests live in `UtilitiesCS.Test/` mirroring the production folders (`Threading/`, `NewtonsoftHelpers/SDILReader/`). |
| Coverage floors (CLAUDE.md 80/75, 90 new; rules 85/75) | PASS | See section 1.2. |
| Coverage regression on changed lines | PASS | Changed executable line hit; per-file uncovered counts unchanged. |
| Determinism infrastructure (no banned APIs) | PASS | No `Thread.Sleep`, `Task.Delay`, `DateTime.Now` or timer use added. |

## 5. Test Coverage Detail

- Production files changed: 4. Executable-line changes: 1 added operand line in UiThread.cs (covered, HITS=1); 3 deleted lines in ILGlobals.cs (two of which were covered field initializers); 2 comment-only lines in QuickFiler.
- New production code coverage target (90%): satisfied at 100% (1 of 1).
- Condition coverage of the modified return expression: baseline 2/2 on the two-operand form, post-change 4/4 on the three-operand form, so the null-dispatcher outcome added by the fix is exercised.
- Test methods: baseline 7320 suite total; post-change 7322 (one #889 test added; two #863 tests added, one deleted). The three new names and all twelve pre-existing `IsCompleted` names read `COUNT=1 OUTCOME=Passed` in the final trx; `Cache_IsInitialized` reads absent.

## 6. Test Execution Metrics

- Baseline full suite (evidence/baseline/baseline-test-summary.txt): Total 7320, executed 7320, passed 7320, failed 0; error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0.
- #889 fail-before, scoped to the hardening class (889-fail-before.md): Total 3, passed 2, failed 1 (the new test); `VSTEST_EXIT=1` as expected.
- #889 pass-after, scoped to the Threading namespace (889-pass-after.md): Total 128, passed 128, failed 0.
- #863 fail-before, scoped to `ILGlobals_Tests` (863-fail-before.md): Total 15, passed 13, failed 2 (both new structural tests); `VSTEST_EXIT=1` as expected.
- #863 pass-after (863-pass-after.md): Total 15, passed 15, failed 0.
- Final full suite with coverage (evidence/qa-gates/final-test-summary.txt): Total 7322, executed 7322, passed 7322, failed 0; error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0; `SEQUENCE_FILES=0` (no hang detected by the blame collector); plan Decision D13 single repeat not used.
- Parallel regime: Workers 0, ClassLevel (runsettings hash unchanged between baseline and final).

## 7. Code Quality Checks

- Format check: PASS (CSharpier check exit 0).
- Lint errors: 0 (analyzer Rebuild 0 errors, 0 warnings).
- Type errors: 0 (TreatWarningsAsErrors Rebuild 0 errors, 0 CS86xx).
- Architecture violations: 0 (no new boundary-crossing reference).
- File size: PASS (all six changed files at most 500 lines).
- Committed evidence format (CLAUDE.md Committed Test Evidence Format): PASS. The only tool-derived committed evidence is two package-level JaCoCo projections and two trx-derived summaries; `RAW_TOOL_DOCS=0` over the feature folder; both projections parse with 9 packages.
- Evidence hygiene: PARTIAL. Account name, host name, worktree root and `<drive>:\Users\` prefix all read 0 hits with positive controls at least 1 (evidence/qa-gates/evidence-hygiene.md). However, two committed lines carry the absolute install path `VS-INSTALL-ROOT\Common7\IDE\Extensions\TestPlatform\vstest.console.exe` (evidence/baseline/baseline-04-mstest-coverage.md line 11; evidence/qa-gates/final-06-mstest-coverage.md line 12). The sanitize gate's four patterns do not match a Program Files path, so the gate passed while the AC7 clause "no committed file contains an absolute host path" is not met literally. See section 8, finding NB-1.

## 8. Gaps and Exceptions

### NB-1 (Non-blocking): absolute host path in two committed evidence lines

- Files: `evidence/baseline/baseline-04-mstest-coverage.md` line 11 and `evidence/qa-gates/final-06-mstest-coverage.md` line 12, both reading `Runner output: Using vstest.console: VS-INSTALL-ROOT\Common7\IDE\Extensions\TestPlatform\vstest.console.exe; ...`.
- Rule: AC7 in issue.md ("no committed file contains an absolute host path, the developer account name, or the host name") and the plan's executor note ("Use the placeholders REPO-ROOT, USER-PROFILE, USER and HOST when a value outside the repository must be described").
- Impact: no identity disclosure (no account or host name; the path is a standard Visual Studio install location), so the Committed Test Evidence Format policy's purpose is met; the literal AC clause is not. AC7 has been unchecked in issue.md.
- Remedy: replace the path prefix on both lines with a placeholder (for example `PROGRAM-FILES\Microsoft Visual Studio\18\Community\...` or the token `VSTEST-CONSOLE`), commit in the exempt docs-only form, and re-check AC7. No code change is involved.

### I-1 (Informational): Phase 0 `Timestamp:` values were estimates, later replaced by file write times

- Disclosed in reduced-audit-handoff.md ("several Phase 0 artifact `Timestamp:` values were first written as estimates and were corrected in the [P1-T16] commit to the observed file write times"). The plan's convention is a CMD-TS observation at run time, so the corrected values are a post-hoc substitute rather than the prescribed observation. The git history retains both states (Phase 0 commit `df86ec9e`, correction in `699ad109`).
- Effect on evidence integrity: none on any measured figure. No acceptance criterion depends on a Timestamp value; every AC rests on exit codes, counts, test names and coverage figures that were not edited. The current Phase 0 sequence (08-51 through 09-08) is monotone and consistent with the timestamps of the later phases (09-11 onward) and with the internal mention of a first analyzer invocation at 08-54 inside baseline-02-analyzers.md.
- Residual weakness: the disclosure does not enumerate which artifacts were corrected, and a file-write time can post-date the command it labels. Recommend enumerating the corrected artifact names in the handoff if the folder is revised for NB-1.

### I-2 (Informational, pre-existing, out of footprint): analyzer HintPath skew on origin/main

- `UtilitiesCS.csproj` and `VBFunctions.csproj` reference `Meziantou.Analyzer.3.0.235` in their `<Analyzer Include>` items while packages.config and the `.props` import name 3.0.290; `SVGControl.Test.csproj` references `MSTest.Analyzers.4.4.0` while packages.config names 4.4.1. On a fresh worktree the analyzer Rebuild fails with 4 CS0006 errors until the two older versions are installed into the gitignored packages directory, which the executor did (baseline-02-analyzers.md). No tracked file changed (`git status --porcelain` over csproj/config/props/targets/packages printed nothing).
- Classification: pre-existing environment defect on origin/main, outside this diff's footprint; consistent with the known pattern of a NuGet version bump leaving stale `<Analyzer Include>` paths. Recommend a follow-up issue to align the `<Analyzer Include>` versions with packages.config; a CI analyzer run on a cold checkout would also fail on this unless it restores the older versions.

### I-3 (Informational): breaking public API change obligation

- `ILGlobals.Cache` and `ILGlobals.modules` removed from UtilitiesCS. In-repo consumers: none (three independent checks listed in section 2). The general policy requires the removal to be called out in the change description; the PR body must name both members. `quality-tiers.yml` does not exist at the repository root of this branch, so the tier-dependent "major bump required" gate for contract-breaking changes is not operable here; the call-out obligation is the applicable control.

### I-4 (Informational): unused `using System.Collections.Generic;` left in ILGlobals.cs

- After the `Dictionary<int, object> Cache` deletion, a Grep over the file for `Dictionary|IList|List<|IEnumerable|ICollection|HashSet|KeyValuePair|Queue<|Stack<|IReadOnly|Comparer<|EqualityComparer` returns zero matches, so the directive at line 3 appears unused. The analyzer Rebuild reported 0 warnings, so no enabled rule flags it. Removal is a one-line cleanup for a later touch of this file; leaving it is consistent with the minimal-fix rule.

### I-5 (Informational): six UtilitiesCS lines outside the changed files flipped missed-to-covered between runs

- Package delta (UtilitiesCS missed minus 6, covered plus 5) exceeds what the changed files explain (covered minus 1, missed 0) by six covered lines elsewhere in the assembly; 0.009 percentage points, within the known full-suite run-to-run variance. Not attributable to the diff; recorded so the package-level delta is not read as an effect of the change.

### I-6 (Informational): four shell-icon test classes excluded from both local coverage runs (plan Decision D3)

- Identical exclusion on both sides; CI runs the classes unfiltered on every pull request. The executor also reports that `scripts/vscode/Invoke-MSTestWithCoverage.ps1` hard-codes its test-case filter and exposes no filter or hang-timeout extension point, which forced a function override to apply the exclusion; candidate follow-up, no obligation created by this branch.

### I-7 (Informational): PR-context artifacts absent for this branch

- No `artifacts/` directory exists in the review worktree, and the caller prohibited Bash; scope was derived from the caller-supplied diff and verified against footprint.md and the files on disk. The session checkout's `artifacts/pr_context.summary.txt` belongs to a different branch (`documentationandmemories`, head `1c80f6e0`) and lists only `.md` files.

### I-8 (Informational): ILGlobals.cs file-level percentage decreased while its uncovered count did not

- 95.00% to 94.74% because two covered field-initializer lines were deleted; not a changed-line regression (the file's diff is deletions only). Recorded so the per-file figure is not misread.

### I-9 (Informational, pre-existing): test file hosts two classes

- `UtilitiesCS.Test/Threading/UiThreadApartmentMeasurement_Tests.cs` contains both `UiThreadPredicateHardening_Tests` (where the #889 test was added, per the plan) and `UiThreadApartmentMeasurement_Tests`. The file name matches only the second class. Pre-existing arrangement from #816; not changed by this branch.

## 9. Summary of Changes

- `UtilitiesCS/Threading/UiThread.cs` (+2/-0): one comment line and the operand `&& _dispatcher is not null` inserted into the dispatcher exit of `SynchronizationContextAwaiter.IsCompleted`, matching the guard the captured-context exit already carries.
- `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs` (+0/-3): `public static Dictionary<int, object> Cache` and `public static Module[]? modules` deleted.
- `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Search.cs` (+1/-1) and `BreadcrumbItemViewerLifecycleCoordinator.Search.cs` (+1/-1): the tokens ` (487 lines)` and ` (481 lines)` removed from XML doc comments.
- `UtilitiesCS.Test/Threading/UiThreadApartmentMeasurement_Tests.cs` (+42): one regression test and `using System.Windows.Threading;`.
- `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs` (+51/-6 net +45): `Cache_IsInitialized` replaced by `PublicStaticFields_AreAllInitOnly` and `PublicStaticFields_AreExactlyTheTwoOpCodeTables`; `using System.Linq;` added.
- Feature folder: issue.md (AC check-offs), plan check-offs, 44 evidence files. This review unchecked AC7.

## 10. Compliance Verdict

**PASS.** Blocking findings: 0. Non-blocking: 1 (NB-1, a two-line placeholder substitution in committed evidence, after which AC7 can be re-checked). Informational: 9. Remediation-inputs artifact: not produced, per the caller's instruction to write exactly three artifacts; the NB-1 remedy is fully specified in section 8 and in feature-audit.2026-09-29T00-45.md.

## Appendix A: Test Inventory

New or changed tests on this branch:

- `UtilitiesCS.Test.Threading.UiThreadPredicateHardening_Tests.IsCompleted_WhenTheAwaiterContextIsAForeignDispatcherContextAndNoUiDispatcherWasCaptured_ReturnsFalse` (added; fail-before recorded; passes after fix).
- `UtilitiesCS.Test.NewtonsoftHelpers.SDILReader.ILGlobals_Tests.PublicStaticFields_AreAllInitOnly` (added; fail-before recorded; passes after fix).
- `UtilitiesCS.Test.NewtonsoftHelpers.SDILReader.ILGlobals_Tests.PublicStaticFields_AreExactlyTheTwoOpCodeTables` (added; fail-before recorded; passes after fix).
- `UtilitiesCS.Test.NewtonsoftHelpers.SDILReader.ILGlobals_Tests.Cache_IsInitialized` (deleted with the member it asserted).

Pre-existing tests confirmed passing after the change (evidence/regression-testing/889-pass-after.md and evidence/qa-gates/final-06-mstest-coverage.md): the twelve `IsCompleted_*` tests enumerated in evidence/baseline/baseline-scoped-tests.md and the thirteen retained `ILGlobals_Tests` methods.

## Appendix B: Toolchain Commands Reference

Commands as executed by the executor (worktree root as current directory; recorded verbatim in the cited artifacts):

| Step | Command (abridged) | Artifact | Result |
|---|---|---|---|
| Format (write) | `dotnet tool run csharpier format .` with anchored patch comparison | evidence/qa-gates/final-01-csharpier-format.md | exit 0, patch unchanged |
| Format (check) | `dotnet tool run csharpier check .` | evidence/qa-gates/final-02-csharpier-check.md | exit 0, 1623 files |
| Analyzers | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | evidence/qa-gates/final-04-analyzers.md | exit 0, 0 errors, 0 warnings |
| Nullable | `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true` | evidence/qa-gates/final-05-nullable.md | exit 0, 0 errors |
| Tests with coverage | `Invoke-MSTestWithCoverageMain` from `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, dot-sourced with `Get-DotnetCoverageArgumentList` overridden (Decision D3) | evidence/qa-gates/final-06-mstest-coverage.md | completed, 7322/7322 |
| Scoped regression runs | `vstest.console.exe UtilitiesCS.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:...` | evidence/regression-testing/*.md | as recorded |

Reviewer verification method for this audit: Read, Grep and Glob over the worktree only (no Bash, per caller instruction); arithmetic re-summation of both JaCoCo projections; line-by-line reading of the six changed source files; Grep-based reference search for the removed members; Grep sweep of the feature folder for drive-letter absolute paths.
