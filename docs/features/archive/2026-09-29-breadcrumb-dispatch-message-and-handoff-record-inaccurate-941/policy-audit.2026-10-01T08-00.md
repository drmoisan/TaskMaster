# Policy Audit - Issue #941 (breadcrumb DispatchValue message and #900 handoff record)

- Work Mode: minor-audit
- Review scope: full branch diff, `9b3eea58447c264eae6f95a4bfee3bfcec7fb17f..HEAD` (HEAD `c7152db65`)
- Review date: 2026-10-01
- Template provenance: the MCP policy-audit template resolver was not callable in this review session. The canonical headings were reproduced by hand from the `policy-audit-template-usage` skill (Executive Summary, sections 1 to 10, Appendix A and B).
- Method note: this review ran without a shell, per the caller constraint. The code diff was supplied by the caller and was cross-checked with the Read and Grep tools against the files at HEAD. Raw Cobertura and trx documents are git-ignored and absent from the worktree, so coverage figures are taken from the committed projections in `evidence/qa-gates/`.

## Rejected Scope Narrowing

None. The caller prompt asked for the reduced small-path audit of the full 941 branch diff and did not narrow the audit scope. The audit covers every changed file in the range.

## Evidence Location Compliance

- Files written under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/` by this branch: none found. All evidence is under `docs/features/active/2026-09-29-breadcrumb-dispatch-message-and-handoff-record-inaccurate-941/evidence/{baseline,other,regression-testing,qa-gates}/`.
- `EVIDENCE_LOCATION_OVERRIDE_REJECTED`: none; the caller supplied no non-canonical evidence path.
- Raw coverage or test-platform documents committed under the feature folder: none (glob for `*.trx`, `*.xml`, `*.cobertura`, `*.coverage` returned no files). Committed coverage evidence is the package-level projection plus the one-line summary, as `CLAUDE.md` requires.
- The validator script `validate_evidence_locations.py` was not run (no shell in this review). The glob and the path review above are the substitute evidence.

## Executive Summary

Verdict: PASS with one non-blocking coverage-floor observation.

- Production change: one string literal at `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs:183`. The line 101 `Dispatch` literal is untouched. No control flow, signature, or visibility changed.
- Test change: one `WithMessage` pattern (`BreadcrumbUiThreadDispatchTests.cs:305`) and one new deterministic MSTest regression test (`BreadcrumbPopupBoundaryCoverageTests.cs:93-111`).
- Documentation change: five corrections to the #900 handoff record, each verified against `BreadcrumbUiDispatcher.cs`.
- Toolchain: csharpier check, analyzer rebuild, nullable rebuild, and the scoped MSTest coverage run all exited 0 in loop iteration 1 (`evidence/qa-gates/p2-t9-toolchain-pass.md`).
- Coverage: the QuickFiler package line and branch rates are identical before and after. The changed dispatcher lines 180-187 hit 1 before and after.
- Blocking findings: 0. Non-blocking findings: 3 (listed in section 8).

## 1. General Unit Test Policy Compliance

### 1.1 Test review (new and modified tests)

| Criterion | Verdict | Evidence |
|---|---|---|
| Independence | PASS | The new test builds its own dispatcher through `CreateOwnerOnlyDispatcher` and its own `errors` list; no shared state. |
| Isolation | PASS | One behavior: `DispatchValue` on an owner-only dispatcher outside an executing callback faults. |
| Fast execution | PASS | Synchronous fault path; no waits. |
| Determinism | PASS | No `Thread.Sleep`, `Task.Delay`, timeout, or clock use in lines 93-111; no thread hand-off. The fault is produced synchronously (`BreadcrumbUiDispatcher.cs:180-188`). |
| Readability | PASS | Descriptive name, XML summary, explicit Arrange/Act/Assert comments. |
| No temporary files | PASS | None created. |
| Arrange-Act-Assert | PASS | Comments at test lines 96, 101, 104. |
| Failure messages | PASS | `IsFaulted` assertion carries a reason string; message assertions are specific. |

Scenario note: the new test covers the negative and error-handling flow for the owner-thread caller. The off-owner-thread caller is covered by the pre-existing, modified-pattern test at `BreadcrumbUiThreadDispatchTests.cs:298-307`.

### 1.2 Coverage

The repository policy files state two floors: `CLAUDE.md` states 80 percent line and 90 percent for new code, while `.claude/rules/general-unit-test.md` and `quality-tiers.md` state 85 percent line and 75 percent branch with no regression on changed lines. This audit reports against the stricter rules-file floors and records the disposition explicitly.

C# coverage verdict: FAIL against the 85% line floor at the QuickFiler package level (82.01% lines), dispositioned non-blocking; branch 78.27% meets the 75% floor.

C# coverage verdict, changed lines: PASS. The changed production line 183 is within lines 180-187, all at hits 1 before and after.

#### Coverage Evidence Checklist

- C# baseline coverage artifact: `evidence/baseline/p0-t10-mstest-coverage.md` and `evidence/baseline/p0-t11-dispatcher-line-hits.md`
- C# post-change coverage artifact: `evidence/qa-gates/p2-t5-mstest-coverage.md` and `evidence/qa-gates/p2-t6-dispatcher-line-hits.md`
- TypeScript baseline coverage artifact: `N/A - out of scope`
- TypeScript post-change coverage artifact: `N/A - out of scope`
- PowerShell baseline coverage artifact: `N/A - out of scope`
- PowerShell post-change coverage artifact: `N/A - out of scope`
- Python baseline coverage artifact: `N/A - out of scope`
- Python post-change coverage artifact: `N/A - out of scope`
- Per-language comparison summary: section 1.2.1 of this document

(The four non-C# languages have zero changed files on this branch, so the out-of-scope value is accurate for them.)

### 1.2.1 Per-Language Coverage Comparison

- C#: Baseline: 82.01% lines (10460/12754) and 78.27% branches (2518/3217). Post-change: 82.01% lines (10460/12754) and 78.27% branches (2518/3217). Change: 0.00% lines and 0.00% branches on the QuickFiler package, with the test total rising from 1469 to 1470. New/changed-code coverage: 100%. Disposition: FAIL. Evidence: `evidence/qa-gates/p2-t7-coverage-delta.md`.
- TypeScript: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero TypeScript files changed on this branch.
- PowerShell: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero PowerShell files changed on this branch.
- Python: Baseline: N/A. Post-change: N/A. Change: N/A. Disposition: N/A. Evidence: N/A - zero Python files changed on this branch.

### 1.2.2 Coverage Artifact State

| Language | Coverage artifact | Verdict | Disposition |
|---|---|---|---|
| C# | Committed package-level projection in `evidence/qa-gates/p2-t5-mstest-coverage.md` (scoped to `QuickFiler.Test`); raw Cobertura is git-ignored | FAIL (package line 82.01% below 85%) | Non-blocking: unchanged against baseline, changed lines hit, above the 80% remediation-trigger floor |
| TypeScript | None (no files changed) | N/A | No TypeScript files on the branch |
| PowerShell | None (no files changed) | N/A | No PowerShell files on the branch |
| Python | None (no files changed) | N/A | No Python files on the branch |

**Coverage Metrics by Language:**

| Language | Files Changed | Tests | Test Result | Baseline Coverage | Post-Change Coverage | New Code Coverage |
|---|---|---|---|---|---|---|
| C# | 3 | 1470 total, 1 added | PASS (1470 passed, 0 failed) | 82.01% lines / 78.27% branches | 82.01% lines / 78.27% branches | 100% |
| TypeScript | 0 | N/A | N/A | N/A | N/A | N/A |
| PowerShell | 0 | N/A | N/A | N/A | N/A | N/A |
| Python | 0 | N/A | N/A | N/A | N/A | N/A |

### 1.3 Coverage disposition analysis (caller question 4)

- The first recorded final measurement read QuickFiler 10459/12754 lines (0.820056) and 2517/3217 branches (0.782406), one covered unit below baseline on each. The identical second measurement read 10460/12754 and 2518/3217, equal to baseline. Both are quoted in `evidence/qa-gates/p2-t5-mstest-coverage.md`, and `p2-t7-coverage-delta.md` records the re-run clause and the unestablished cause.
- Adequacy: adequate. The diff replaces one string literal and adds one test. A literal substitution cannot remove a covered line or branch from the production assembly; the new test file lies outside the denominator (`.*\.Test\.dll$` is excluded). The changed lines 180-187 were hit 1 time in both measurements. The one-unit swing with no source change between runs matches the known run-to-run variation recorded for this repository's coverage runs. The disposition is a valid no-regression finding on the changed lines, and the record is honest because both values are stated rather than only the matching one.
- Residual: the cause of the one-unit variation is not established. This is recorded as non-blocking observation N2 in section 8.

## 2. General Code Change Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| Minimal, targeted fix (Bugfix Workflow step 2) | PASS | Production diff is one line added and one deleted at `BreadcrumbUiDispatcher.cs:183`. |
| Failing regression test first (Bugfix Workflow step 1) | PASS | `evidence/regression-testing/p1-t2-expect-fail-new-test.md`: the new test failed before the literal change with `Expected fault.Message "...cannot marshal cross-thread UI work." to contain "outside an executing Dispatch callback"`. The post-fix pass is in `p1-t5-new-test-passes-after-fix.md` and `p1-t7-three-tests-pass.md` (3 of 3 passed). |
| Behavior unchanged apart from message text | PASS | Lines 180-188 at HEAD: guard, exception construction, `Report`, and `Task.FromException<T>` are unchanged; only the literal differs. The line 101 `Dispatch` literal reads `...cannot marshal cross-thread UI work.` as before. |
| File size limit (500 lines) | PASS | `BreadcrumbUiDispatcher.cs` 285, `BreadcrumbUiThreadDispatchTests.cs` 480, `BreadcrumbPopupBoundaryCoverageTests.cs` 410 (410 confirmed by a Grep line count at HEAD); `evidence/qa-gates/p2-t8-scope-and-size.md`. |
| Match existing style | PASS | The new test sits beside the sibling owner-only test and reuses `CreateOwnerOnlyDispatcher`. |
| No scope creep | PASS | The filtered branch name list equals the four footprint paths plus the pre-existing promoted potential entry (`p2-t8-scope-and-size.md`). |
| Tone policy (prose in artifacts) | PASS | Evidence prose reviewed in the feature folder is factual and neutral; no humor, hyperbole, or metaphor found. |
| Host-path and identity hygiene | PASS | A Grep of the feature folder and the edited handoff record for drive-letter paths, `Users` paths, account names, and the user email found only two public URLs. Evidence uses `<worktree root>` placeholders. |
| Evidence format (no raw Cobertura or trx) | PASS | No raw collector or test-platform document is committed; the trx stays in the git-ignored results directory (`p1-t2` note). |

## 3. Language-Specific Code Change Policy Compliance

### 3.1 C# (`.claude/rules/csharp.md`)

| Requirement | Verdict | Evidence |
|---|---|---|
| CSharpier check | PASS | `evidence/qa-gates/p2-t1-csharpier-format.md` (REWRITTEN 0) and `p2-t2-csharpier-check.md`, exit 0. |
| Analyzer rebuild (`/t:Rebuild`) | PASS | `evidence/qa-gates/p2-t3-msbuild-analyzers.md`, exit 0. |
| Nullable rebuild (`/t:Rebuild`, `TreatWarningsAsErrors`, no `/p:Nullable=enable`) | PASS | `evidence/qa-gates/p2-t4-msbuild-nullable.md`, exit 0. |
| Tests with coverage | PASS | `p2-t5-mstest-coverage.md`: total 1470, passed 1470, failed 0, exit 0. |
| Toolchain order and single clean pass | PASS | `p2-t9-toolchain-pass.md`: all five exit codes 0 in loop iteration 1, REWRITTEN 0. |
| Naming and exception style | PASS | `PascalCase` method name; fail-fast `InvalidOperationException` retained. |
| Public surface | PASS | `DispatchValue` stays `internal`. |

Note on the test at `BreadcrumbPopupBoundaryCoverageTests.cs:106`: `result.Exception.InnerException` is dereferenced without a null guard. The file carries no `#nullable enable` directive in the viewed region, so no nullable diagnostic applies, and the preceding `IsFaulted` assertion guarantees a non-null `Exception`. Recorded as non-blocking observation N3.

### 3.2 Other languages

TypeScript, Python, and PowerShell: zero changed files on this branch. The PowerShell batch budget and PoshQC gates were not exercised, as the caller stated.

## 4. Language-Specific Unit Test Policy Compliance

| Requirement | Verdict | Evidence |
|---|---|---|
| MSTest `[TestClass]` / `[TestMethod]` | PASS | New test at `BreadcrumbPopupBoundaryCoverageTests.cs:93-94`. |
| FluentAssertions | PASS | `Should().BeTrue`, `BeOfType`, `Contain`, `Be`, `ContainSingle().Which.Should().BeSameAs` (lines 105-110). |
| Moq | PASS | Not required; a hand-built dispatcher via the existing reflection helper is the established seam. |
| No temp files or sleeps | PASS | None. |
| Assertion strength | PASS | Four independent observable effects: faulted task, exception type, message substring, action not executed, and sink received the same exception instance. |
| Modified assertion not weakened | PASS | The `BreadcrumbUiThreadDispatchTests.cs:305` pattern changed to `*outside an executing Dispatch callback*`, which still pins the `DispatchValue` null-context message and no longer matches the `Dispatch` message, so it is more specific, not weaker. |
| Assertion not satisfied by sibling | PASS | The line 86 assertion (`Contain("cannot marshal")`) targets the unchanged line 101 literal; the other two sites target the new line 183 literal. The census `git grep -n -F "cannot marshal" -- "*.cs"` returns exactly two hits (`BreadcrumbPopupBoundaryCoverageTests.cs:86`, `BreadcrumbUiDispatcher.cs:101`); a Grep at HEAD reproduces both. |

## 5. Test Coverage Detail

- Changed production lines: line 183 (the literal). Hits before: 1. Hits after: 1 (`evidence/baseline/p0-t11-dispatcher-line-hits.md`, `evidence/qa-gates/p2-t6-dispatcher-line-hits.md`; 16 matched line elements and 8 rows with hits at least 1 in both).
- C# per-file rates for `BreadcrumbUiDispatcher.cs` were not recorded in the committed evidence and the raw Cobertura is not present in the worktree. The per-file figure is therefore not independently restated here; the changed-line result and the unchanged package rates are the evidence. See N1.
- Package rates (QuickFiler): line 82.01%, branch 78.27%, both identical before and after.
- Repo-wide figure: the run was scoped to `QuickFiler.Test`, which prints "Coverage threshold assertions skipped" and a first-party figure of 24.42% lines (reference only; the denominator includes assemblies this test project does not instrument). The repo-wide gate is the PR CI run.
- Newly added production code: none; new-code coverage is stated as 100% on the single changed executable line.

## 6. Test Execution Metrics

| Run | Total | Passed | Failed | Evidence |
|---|---|---|---|---|
| Expect-fail, new test only (before fix) | 1 | 0 | 1 | `evidence/regression-testing/p1-t2-expect-fail-new-test.md` |
| New test after fix | 1 | 1 | 0 | `evidence/regression-testing/p1-t5-new-test-passes-after-fix.md` |
| Three affected tests | 3 | 3 | 0 | `evidence/regression-testing/p1-t7-three-tests-pass.md` |
| Scoped QuickFiler.Test run, baseline | 1469 | 1469 | 0 | `evidence/baseline/p0-t10-mstest-coverage.md` |
| Scoped QuickFiler.Test run, final | 1470 | 1470 | 0 | `evidence/qa-gates/p2-t5-mstest-coverage.md` |

## 7. Code Quality Checks

| Check | Command or method | Result |
|---|---|---|
| Confidentiality masking scan | Grep of feature folder and handoff record for drive letters, user paths, account names, email | PASS: only two public URLs matched |
| Suppression scan (added lines) | Reviewed the supplied diff: no `#pragma warning`, `SuppressMessage`, or `ExcludeFromCodeCoverage` added; no exclude glob added to any settings file; the new test has no `Thread.Sleep`, `Task.Delay`, `DateTime.Now`, or `Random.Shared` | PASS |
| Workflow change scan | The branch diff contains no `.github/workflows/` change | PASS |

## 8. Gaps and Exceptions

Blocking findings: 0.

Non-blocking observations:

- N1. The per-file line and branch rates for `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs` are not stated in the committed evidence. Disposition: the changed lines are covered and the package rates are unchanged, so the no-regression requirement is evidenced. A follow-up could add a per-file figure to the standard coverage projection.
- N2. The one-unit difference between the two identical measurements (10459 versus 10460 lines, 2517 versus 2518 branches) has no established cause. Disposition: adequate for this item, see section 1.3.
- N3. `result.Exception.InnerException` in the new test is read without a null guard. Disposition: safe after the `IsFaulted` assertion; no action required.

Floor note: the QuickFiler package line rate (82.01%) sits below the 85% line floor in `.claude/rules/general-unit-test.md` and above the 80% floor in `CLAUDE.md`. The shortfall is pre-existing and unchanged by this branch, and no remediation trigger fires: no changed-line regression, no new production file, no artifact absence, no toolchain failure, no acceptance criterion failure.

## 9. Summary of Changes

| File | Change |
|---|---|
| `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs` | Line 183: message literal narrowed to describe the executing-callback requirement. |
| `QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs` | Line 305: `WithMessage` pattern updated to the new wording. |
| `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs` | One new regression test (24 lines inserted after line 87). |
| `docs/features/active/breadcrumb-thread-affinity-tests-assume-taskrun-distinct-thread-900/evidence/other/p5-t14-follow-up-handoff.2026-09-17T02-39.md` | Entry 1 attribution corrected; the owner-thread comparison is cited at `:276-277`. |
| Feature folder | Plan, research, evidence, issue.md acceptance-criteria check-off. |

Handoff record accuracy check (caller question 3): `BreadcrumbUiDispatcher.cs` lines 180-188 test `_context == null` and never read `_ownerThreadId`. `_ownerThreadId` is stored at line 40, supplied at lines 54 and 64, and compared at lines 276-277 inside `IsCurrentBoundary`, whose only call site is line 78 in `Dispatch`. The corrected text ("DispatchValue ... faults for every caller outside an executing dispatcher callback, on any thread"; owner check "used by Dispatch only") matches these facts.

## 10. Compliance Verdict

PASS. All nine acceptance criteria are met. Zero blocking findings, three non-blocking observations. No remediation inputs are required.

## Appendix A: Test Inventory

| Test | File | Status |
|---|---|---|
| `DispatchValue_OwnerOnlyOnOwnerThread_FaultsOutsideExecutingCallback` | `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs:93` | New; fails before the literal change, passes after |
| `ProductionCaptureWithoutUiContext_FailsFast` | `QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs` (assertion at line 305) | Pattern modified; passes |
| `Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction` | `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs:67` (assertion at line 86) | Unmodified; passes |

## Appendix B: Toolchain Commands Reference

1. `dotnet tool run csharpier check .`
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. `scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot QuickFiler.Test` (scoped run; all four steps recorded with exit 0 under `evidence/qa-gates/`)
