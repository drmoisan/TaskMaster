# Code Review: QuickFiler banned-API time/delay seams (Issue #222)

**Review Date:** 2026-06-28
**Reviewer:** feature-reviewer agent
**Feature Folder:** `docs/features/active/2026-06-28-qfc-banned-api-time-delay-seams-222`
**Base Branch:** `main` (merge-base 86b555bf2a26f91a5f59f7dbccf6a6ac56d8e16a)
**Head Branch:** `TaskMaster-wt-2026-06-28-18-49` (d4075e02509f7340e747894dc9bff0ae1c1e1197)
**Review Type:** Re-review (cycle 2) after maintainer authority decision 222-COV-001

---

## Executive Summary

This change eliminates eight pre-existing banned-API time/delay usages in the QuickFiler controllers by routing them through an injectable `System.TimeProvider` seam, backported to the .NET Framework VSTO target via `Microsoft.Bcl.TimeProvider`. Each controller gains a single `internal TimeProvider` property defaulting to `TimeProvider.System`; `LaunchAsync` gains an optional, source-compatible `TimeProvider timeProvider = null` parameter. The public `IQfcDatamodel`/`IQfcHomeController` interfaces are untouched.

**What changed:**
Five production files (3 `Task.Delay` -> `TimeProvider.Delay`; four `DateTime.Now` reads -> `TimeProvider.GetLocalNow().LocalDateTime`; one catch-block timestamp), two test files (5 new MSTest+Moq+FluentAssertions tests using `FakeTimeProvider`), and six build/config files (package wiring for QuickFiler, QuickFiler.Test, and the TaskMaster consumer). The production delta is roughly 30 lines. Evidence reviewed: full `git diff` against merge-base, `evidence/qa-gates/*` (format/analyzer/nullable/tests/coverage-comparison/banned-api-sweep/policy-unchanged/line-counts), the two `evidence/regression-testing/*` scope dossiers, and `coverage-policy-exception.md` (222-COV-001).

**Cycle-2 delta:** The only commit since cycle 1 is `d4075e02 docs(#222): record authority coverage-policy exception (222-COV-001)`; no production or test code changed (`git diff --stat e4893265..d4075e02 -- '*.cs' '*.csproj' '*.config'` is empty). The cycle-1 Major finding (repo-wide C# coverage artifact absent) is now resolved by the maintainer authority decision and downgraded to Resolved/Info below.

**Top risks (residual, non-blocking):**
1. The five new tests depend on reflection into private members (`FormatterServices.GetUninitializedObject`, private field/method access), which couples them to current internals and is brittle under refactor. Justified by COM-boundedness.
2. The NonBlockingProducer site-8 test exercises the seam in isolation rather than through the production method, so the production call site (Metrics.cs L222) remains uncovered (documented as an unreachable defensive branch).
3. Repo-wide C# coverage is not locally measured (canonical artifact absent); verification is deferred to PR CI per ratified maintainer decision 222-COV-001.

**PR readiness recommendation:** **Go.** Implementation is sound and behavior-preserving; the cycle-1 coverage-evidence blocker is resolved by maintainer authority decision 222-COV-001. No blocker or major findings remain.

---

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Resolved (was Major) | `artifacts/csharp/coverage.xml` | n/a (absent) | Cycle-1 finding: canonical repo-wide C# coverage artifact absent; repo-wide >= 80% floor not locally demonstrable. Now resolved by maintainer authority decision 222-COV-001 (Option C): defer repo-wide verification to PR CI; accept the pre-existing below-floor figure as a ratified legacy COM/VSTO condition. | Confirm the PR CI repo-wide coverage figure during PR review per 222-COV-001. No code change required. | The decision matches resolution paths 2-3 enumerated for R1 in `remediation-inputs.2026-06-28T19-57.md` and is authorized by CLAUDE.md (maintainer-ratified testable-denominator exemption). Within-scope coverage is independently 100% testable with no regression. | `coverage-policy-exception.md` (222-COV-001); `evidence/qa-gates/coverage-comparison.md`; `final-tests.md` per-line hit counts |
| Minor | `QuickFiler.Test/Controllers/QfcDatamodelTests.cs` | L186-276 | Tests use `FormatterServices.GetUninitializedObject` and reflection to set private fields and invoke private methods (`ToggleOfflineMode`, `WaitForQueue`). Brittle; couples tests to private names/shapes. | Acceptable here given COM-boundedness; consider a narrow internal test seam (`InternalsVisibleTo` + internal method) in a future pass. | Reflection-based private access breaks silently on rename and tests implementation, not contract. | Diff inspection of QfcDatamodelTests.cs |
| Minor | `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` | `NonBlockingProducer_DelaySeam_HonorsInjectedTwentyMillisecondDelay` | Test calls `_controller.TimeProvider.Delay(...)` directly rather than invoking `NonBlockingProducer`, so it asserts seam mechanics, not that the production site uses the seam. Production L222 shows 0 hits. | Rely on the banned-API sweep + dossier for the production line; note the test is a seam-behavior check, not a call-site test. | Slightly tautological; the production call site is not exercised by this test. | `final-tests.md` L26 (L222 = 0 NOT COVERED); `nonblockingproducer-delay-branch-scope.md` |
| Info | `QuickFiler/Controllers/QfcHomeController.cs` | `LaunchAsync` signature L37-41 | Public static factory gained optional `TimeProvider timeProvider = null` parameter. Source-compatible (not an interface member), but a metadata/signature change for any precompiled external caller. | None required; all in-repo callers recompile. Note in PR description. | Optional params are source-compatible but not binary-compatible. | Diff inspection |
| Info | `TaskMaster/TaskMaster.csproj`, `TaskMaster/packages.config` | reference/package add | Scope expansion beyond spec's listed files: TaskMaster consumer-side `Microsoft.Bcl.TimeProvider` reference added. | None required; mechanically necessary because TaskMaster references QuickFiler. | Transitive package reference needed for the consuming assembly to resolve. | `p3-policy-unchanged.md` |
| Info | `QuickFiler/packages.config`, `QuickFiler.Test/packages.config` | package adds | New dependencies `Microsoft.Bcl.TimeProvider` 10.0.7 and `Microsoft.Extensions.TimeProvider.Testing` 9.0.0. spec says approval required. | Confirm maintainer approval is recorded. | First-party MS packages; canonical TimeProvider backport for .NET Framework. | spec.md L75-77; diff inspection |

No Blocker findings. No open Major findings (the cycle-1 Major is Resolved).

---

## Implementation Audit

### C# implementation audit

#### What changed well

- Minimal, behavior-preserving seam: a single `internal TimeProvider` property per controller defaulting to `TimeProvider.System`, so production timing and timestamp semantics are unchanged.
- Correct semantic mapping: `DateTime.Now` -> `TimeProvider.GetLocalNow().LocalDateTime` (local time preserved); `Task.Delay(n)` -> `TimeProvider.Delay(TimeSpan.FromMilliseconds(n)[, token])`, preserving the cancellation token in the QueueProcessing poll loop.
- The QueueProcessing 200 ms delay now forwards the existing `token` to `TimeProvider.Delay`, a small correctness improvement (cancellation now propagates to the delay) without changing the observable contract.
- `WriteMetricsAsync`/`QuickFileMetrics_WRITE` consolidate the date/time reads to a single `now` local, removing redundant repeated clock reads while preserving the exact format strings.
- Public interface surfaces are untouched; the seam is internal.

#### Type safety and API notes

- Nullable build passes with `TreatWarningsAsErrors`; the `timeProvider ?? TimeProvider.System` null-coalesce keeps the seam non-null.
- No new public API on the interfaces; only the static factory parameter, which is optional.

#### Error handling and logging

- The LaunchAsync catch-block log timestamp is now seam-sourced; format (`mm:ss.fff`) and content unchanged. The OCE path itself is unchanged.
- No broad catches introduced; no logging-pattern deviations.

---

## Test Quality Audit

The five new tests are deterministic and isolate single behaviors. They use `FakeTimeProvider` to gate delays (assert not-complete, `Advance`, then complete) and to feed fixed timestamps whose expected values are derived from the fake provider itself (time-zone independent). No temporary files, no live Outlook COM, no network. The full suite reports 186/186 passing.

Two quality caveats (both Minor, non-blocking): (1) reflection-based private access reduces refactor-resilience; (2) the site-8 test verifies the seam in isolation rather than via the production method, leaving the production call site uncovered (documented as an unreachable defensive branch).

### Reviewed test and QA artifacts

- `evidence/qa-gates/final-tests.md` — 186/186 pass; per-line hit counts for the 6 covered changed lines and the 3 uncovered (exempt) lines.
- `evidence/qa-gates/coverage-comparison.md` — baseline vs post-change deltas; explicit statement that repo-wide is not measurable from the single-assembly run.
- `evidence/qa-gates/p3-banned-api-sweep.md` — 0 active banned-API matches in the four target files (independently re-verified at head this cycle).
- `evidence/qa-gates/p3-policy-unchanged.md` — BannedSymbols.txt/.editorconfig/csharp.md unmodified (independently re-verified: `git diff 86b555bf..d4075e02` for those paths is empty).
- `evidence/regression-testing/launchasync-test-scope.md`, `nonblockingproducer-delay-branch-scope.md` — exemption dossiers for the 3 uncovered lines (ratified).
- `coverage-policy-exception.md` — maintainer authority decision 222-COV-001 resolving cycle-1 R1.

### Quality assessment prompts

- **Determinism:** Wall-clock removed from all tested paths; `FakeTimeProvider.Advance` drives delays. Deterministic.
- **Isolation:** One site per test; fresh fixtures per test.
- **Speed:** No real waits; suite EXIT_CODE 0.
- **Diagnostics:** FluentAssertions `because` messages clearly identify seam-bypass failures.

---

## Security / Correctness Checks

| Check | Status | Evidence |
|---|---|---|
| No secrets in code | PASS | Diff contains no credentials/tokens. |
| No unsafe subprocess or command construction | PASS | No process invocation introduced. |
| Input validation at boundaries | PASS | `timeProvider ?? TimeProvider.System` guards null at the factory boundary. |
| Error handling remains explicit | PASS | OCE handling unchanged; no new broad catches. |
| Configuration / path handling is safe | PASS | Package references use standard `..\packages\` HintPaths consistent with existing entries. |

---

## Research Log

No external research required. Review based on branch-diff inspection, committed QA-gate evidence, repository policy files (CLAUDE.md, `.claude/rules/*`), the maintainer authority decision `coverage-policy-exception.md`, and prior repo coverage history.

---

## Verdict

The change is a well-scoped, behavior-preserving refactor that correctly removes the eight banned-API sites and proves the new seam with deterministic tests. Implementation, banned-API integrity, file-size, public-surface preservation, and toolchain are all clean. The cycle-1 follow-up (produce/confirm repo-wide C# coverage) is resolved by maintainer authority decision 222-COV-001, which defers repo-wide verification to PR CI and ratifies the pre-existing below-floor figure under the CLAUDE.md testable-denominator framework. No blocker or open Major findings remain. Recommendation: **Go** (confirm the PR CI repo-wide coverage figure during PR review per 222-COV-001).
