# Feature Audit: TimeOutTask flaky-timing test fix (Issue #191)

**Audit Date:** 2026-06-12
**Feature Folder:** `docs/features/active/2026-06-12-timeout-task-flaky-timing-191`
**Base Branch:** `origin/main` (commit `aa63315b`)
**Head Branch:** `bug/timeout-task-flaky-timing` (uncommitted working-tree edits)
**Work Mode:** `minor-audit`
**Audit Type:** Initial acceptance review

---

## Scope and Baseline

- **Base branch:** `origin/main` (commit `aa63315b`)
- **Head branch/commit:** `bug/timeout-task-flaky-timing` (working-tree scope; uncommitted)
- **Merge base:** `aa63315b`
- **Evidence sources:**
  - Primary: `git diff origin/main` (full branch diff)
  - Feature evidence: `docs/features/active/2026-06-12-timeout-task-flaky-timing-191/evidence/**`
  - QA gates: `evidence/qa-gates/qa-01..05*.md`, `evidence/qa-gates/coverage-post.xml`
  - Determinism: `evidence/regression-testing/determinism-repeated-runs.md`
- **Feature folder used:** `docs/features/active/2026-06-12-timeout-task-flaky-timing-191`
- **Requirements source:** `issue.md` only
- **Work mode resolution note:** `issue.md` line 13 declares `- Work Mode: minor-audit` explicitly. Per the acceptance-criteria-tracking skill, the sole AC source is the `## Acceptance Criteria` section of `issue.md` (AC1–AC6 + Out of scope).
- **Scope note:** Validation is against the uncommitted working tree relative to `origin/main`. The branch diff is exactly two C# test files plus one agent-memory markdown note; the production file `UtilitiesCS/Threading/TimeOutTask.cs` is unchanged.

---

## Acceptance Criteria Inventory

**Authoritative AC source files for this run:**
- `docs/features/active/2026-06-12-timeout-task-flaky-timing-191/issue.md` — only source (minor-audit)

### Acceptance criteria

1. AC1: `TimeOutTask_Tests.RunWithTimeout_FuncT1TResult_ShouldReturnResult` (in `UtilitiesCS.Test/Threading/TimeOutTask_AdditionalTests.cs`) is made deterministic so it passes consistently under class-level parallelism and under code-coverage instrumentation, not only when run in isolation.
2. AC2: The fix is test-only. No change to `UtilitiesCS/Threading/TimeOutTask.cs` (production timeout semantics unchanged), and the ~775-line production file is not grown. If any other `TimeOutTask` timing test shares the same wall-clock/thread-pool sensitivity, it may be stabilized in the same test-only change.
3. AC3: The fix uses an established repository pattern for timing-sensitive tests — `[DoNotParallelize]` on the affected test class and/or a robust timing approach that does not depend on a tight wall-clock window for trivially-completing work — consistent with `ApplicationIdleTimer_Tests`, `TimerWrapper_Tests`, and the existing generous-timeout precedent in `TimeOutTask_Tests`.
4. AC4: The assertion intent is preserved (the test still verifies that `RunWithTimeout` returns the function's result for the success path). Assertions are not weakened or removed.
5. AC5: Determinism is demonstrated: the affected test(s) pass across repeated runs under class-level parallelism (capture evidence). No other test is regressed.
6. AC6: C# toolchain passes in order — CSharpier -> .NET analyzers -> nullable -> MSTest (vstest) — for the changed test assembly, with no new analyzer/nullable diagnostics and no coverage regression on changed lines.

---

## Acceptance Criteria Evaluation

| # | Criterion | Status | Evidence | Verification command(s) | Notes |
|---|-----------|--------|----------|--------------------------|-------|
| 1 | Affected test made deterministic under parallelism + coverage | PASS | 12/12 parallel runs + 1/1 coverage run PASS, zero `TimeoutException` (`evidence/regression-testing/determinism-repeated-runs.md`); affected test PASS in full parallel+coverage suite (`evidence/qa-gates/qa-04-test-coverage.md`) | `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Tests:RunWithTimeout_FuncT1TResult_ShouldReturnResult /InIsolation [/EnableCodeCoverage]` | Both mitigations applied in shipped state. |
| 2 | Test-only; no production change; file not grown | PASS | `git diff --name-only origin/main` lists only the two test files + agent-memory note; `UtilitiesCS/Threading/TimeOutTask.cs` absent from diff. A second `TimeOutTask` timing test was stabilized in the same change (`[DoNotParallelize]` on the class), permitted by AC2. | `git diff --name-only origin/main` | Production file unchanged and not grown. |
| 3 | Uses established repo pattern | PASS | `[DoNotParallelize]` added (`TimeOutTask_Tests.cs` line 10); pattern used in 5 other classes incl. `ApplicationIdleTimer_Tests`, `TimerWrapper_Tests`. Generous-timeout precedent at `TimeOutTask_Tests.cs` line 76. | `git grep -l DoNotParallelize -- '*.cs'` | Both cited patterns applied. |
| 4 | Assertion intent preserved | PASS | `result.Should().Be("result-42")` retained at `TimeOutTask_AdditionalTests.cs` line 143; `maxAttempts: 0, strict: true` retained; only `milliseconds: 200 -> 5000` changed. | `git diff origin/main -- UtilitiesCS.Test/Threading/TimeOutTask_AdditionalTests.cs` | No assertion weakened or removed. |
| 5 | Determinism demonstrated; no other test regressed | PASS | 13/13 repeated runs PASS (`evidence/regression-testing/determinism-repeated-runs.md`). The single full-suite failure (`AddEntry_UseUiThreadTrue_...` in `IdleAsyncQueue_Tests.cs`) is pre-existing (file not in this diff) and passes 3/3 in isolation (`evidence/qa-gates/qa-04-test-coverage.md`). | `git diff --name-only origin/main` (IdleAsyncQueue absent) | No regression attributable to this change. |
| 6 | C# toolchain passes in order; no new diagnostics; no changed-line coverage regression | PASS | CSharpier check EXIT 0 (`qa-01`); analyzers 0 errors, none on changed files (`qa-02`); nullable NONE_IN_CHANGED_FILES (`qa-03`); MSTest affected test PASS (`qa-04`); zero changed production lines, module 85.31% (`qa-05`). | See Appendix B of `policy-audit.2026-06-12T20-49.md` | Whole-solution nullable exit 1 is pre-existing vendored breakage, excluded per `.claude/rules/csharp.md`; in-scope files clean. |

---

## Summary

**Overall Feature Readiness:** PASS

**blocking_count: 0**

**Ready-to-merge determination:** READY FOR MERGE. All six acceptance criteria are PASS, the change is confirmed test-only with the production file unchanged and within the test-file cap (2 changed test files, both under 500 lines), and the two incidental items are non-blocking and out of scope.

**Criteria summary:**
- **PASS:** 6 criteria (AC1–AC6)
- **PARTIAL:** 0 criteria
- **UNVERIFIED:** 0 criteria
- **FAIL:** 0 criteria

**Incidental items adjudicated:**

1. (a) Pre-existing flaky `AddEntry_UseUiThreadTrue_DequeuesEntryAndSuppressesDispatcherException` (`IdleAsyncQueue_Tests.cs`): **out-of-scope, non-blocking.** The file is not in this branch diff (`git diff --name-only origin/main` does not list it), it passes 3/3 in isolation, and it appears in prior-feature evidence. It is a pre-existing intermittent failure under full-suite parallelism, NOT caused by this change.
2. (b) CSharpier v1 `format .` reformatted 8 `.csproj` files: **out-of-scope, non-blocking, resolved.** The reformatting was reverted via `git checkout`; the working-tree scan (`git status --short` filtered to `.csproj`/`.props`/`.targets`) returns NONE — no project files remain modified. The format gate is legitimately satisfied for the two in-scope `.cs` files via `csharpier check` (EXIT 0).

**Additional confirmations requested:**
- Test-only scope confirmed: `UtilitiesCS/Threading/TimeOutTask.cs` and all production files unchanged. Within the 3-test-file cap (2 test files changed).
- AC4 preservation confirmed: `result.Should().Be("result-42")` and `maxAttempts: 0, strict: true` are preserved; widening the timeout combined with `[DoNotParallelize]` is a legitimate determinism fix consistent with repo precedent, NOT a prohibited "timing hack to mask flaky behavior" (the trivial function returns immediately and the assertion is unchanged; the timeout is an upper-bound tolerance).
- Nullable/TWAE: the whole-solution build fails only due to pre-existing vendored-project breakage (per `.claude/rules/csharp.md` analyzer exclusions); the two changed files introduce zero nullable/analyzer diagnostics.

**Top gaps preventing PASS:**
1. None.

**Recommended follow-up verification steps:**
1. Optional: add `*.csproj`/`*.props`/`*.targets` to `.csharpierignore` in a separate change to prevent CSharpier v1 from reformatting project files (noted in `qa-01-csharpier.md` as out-of-scope follow-up).
2. Optional: track the pre-existing `IdleAsyncQueue_Tests` flakiness as a separate defect.

---

## Acceptance Criteria Check-off

Per the acceptance-criteria tracking rules, all six criteria are evaluated PASS and are already checked `- [x]` in `issue.md` (lines 68–73). No source-file checkbox change was required during this audit because the executor had already checked them off upon verified delivery; this audit confirms each check-off is supported by inspected evidence.

### AC Status Summary

- Source: `docs/features/active/2026-06-12-timeout-task-flaky-timing-191/issue.md`
- Total AC items: 6
- Checked off (delivered): 6
- Remaining (unchecked): 0
- Items remaining: None.

| Source File | Total AC | Checked (PASS) | Unchecked | Notes |
|-------------|----------|----------------|-----------|-------|
| `issue.md` | 6 | 6 | 0 | Checkbox-backed; all AC1–AC6 already `[x]` and confirmed by inspected evidence |
