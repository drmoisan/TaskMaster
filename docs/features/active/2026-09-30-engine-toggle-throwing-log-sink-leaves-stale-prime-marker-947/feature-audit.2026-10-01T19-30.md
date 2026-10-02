# Feature Audit — engine-toggle-throwing-log-sink-leaves-stale-prime-marker (Issue #947)

- Date: 2026-10-01 (review label `2026-10-01T19-30`; assigned, no shell clock available to this review)
- Work mode: `minor-audit` (`issue.md` line 12, the only `- Work Mode:` line). AC source: the `## Acceptance Criteria` section of `issue.md` only. `spec.md` and `user-story.md` are intentionally absent (confirmed by Glob of the feature folder).
- Branch: `bug/engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947`

## Scope and Baseline

| Item | Value | Source |
|---|---|---|
| Base commit (BASE-SHA) | `2e6ce2cabe7136bbbc8897fcb39d3e13d654ff85` — the merge of origin/main into this branch immediately before execution (reflog entry `merge origin/main`, epoch 1790890172) | caller prompt; `evidence/baseline/scope-and-anchor.md`; worktree reflog |
| Head commit | `ec312202e8fdaa6ba70a958cf3ca4a0b7503d1ce` (`docs(947): record the final QC loop, coverage comparison and acceptance check-offs`) | worktree reflog, last entry |
| Intermediate commits on the branch after BASE-SHA | `433d5c2e2` (Phase 0 evidence), `caeb82c40` (fix + tests + csproj), `ec312202e` (Phase 2 evidence and check-offs) | reflog; `reduced-audit-handoff.md` |
| Code footprint (BASE-SHA..head) | `M TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` (442 -> 476 lines), `A TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.ThrowingSink.cs` (215 lines), `M TaskMaster.Test/TaskMaster.Test.csproj` (+1 line) | `evidence/qa-gates/footprint-scope.md` (29-line name-status listing, re-read); source files re-read |
| Non-code footprint | feature folder only (plan, issue.md check-offs, 39 evidence artifacts, research record, preflight clearance) | same |
| Baseline behaviour (defect) | `CompletePrime` called `_logError` then `TryRemove` with no guard; a throwing sink skipped the clear, left the marker registered (no re-prime for the session) and faulted the discarded continuation. `HandleToggleClickAsync` called `_logError` unguarded inside its boundary catch, so a throwing sink escaped a method documented as never throwing. | `evidence/baseline/anchor-production-shape.md` (`COMPLETEPRIME-SHAPE: REPORT-THEN-CLEAR`, `catch (Exception)` 0, base lines 380/381 and 184); fail-before run |
| Baseline tests | fixture 28/28 passed; suite 7332/7332 passed | `coordinator-tests-baseline.md`; `coverage-baseline.md` |
| Baseline coverage | first-party 85.34% lines / 79.73% branches; coordinator 157/157 lines, 37/38 branches | `coverage-baseline.md`; `coverage/baseline-947.cobertura.xml` root and class node re-read |
| Post-change tests | fixture 32/32 passed; suite 7336/7336 passed | `throwing-sink-pass-after.md`; `coverage-summary.md` |
| Post-change coverage | first-party 85.34% lines / 79.73% branches; coordinator 167/167 lines, 37/38 branches; changed lines 12/12 covered | `coverage-summary.md`; `coverage/final-947.cobertura.xml` re-read |
| PR context artifacts | absent from the review worktree; not regenerable without a shell. Scope derived from the three agreeing sources above. | Glob |

## Acceptance Criteria Inventory

Source: `docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/issue.md`, section `## Acceptance Criteria` (heading at line 45; seven checkbox lines 47-53; next heading `## Logs / Screenshots` at line 55). All seven are checkbox items and all seven read `- [x]` at head. AC6 and AC7 were appended by the maintainer's scope consolidation of 2026-10-01T15:57:04Z (recorded in `## Scope Consolidation`, line 39 onward); AC1-AC5 are unchanged in text and ordinal.

| ID | Criterion (verbatim opening) | Checkbox at head |
|---|---|---|
| AC1 | When the `logError` sink throws while `CompletePrime` reports a faulted or canceled prime, the engine's prime marker is still removed, so a later `GetPressed` for the same engine starts a new prime (`EngineActiveAsync` is invoked a second time). | `[x]` |
| AC2 | The report-then-clear ordering in `CompletePrime` is preserved: the sink is invoked before the marker is removed (the clear is not reordered ahead of the report), and the existing tests in `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs` pass without modification. | `[x]` |
| AC3 | A throwing `logError` sink leaves no faulted task unobserved: the sink exception is contained inside `CompletePrime`, so the prime continuation has no remaining throw source, and a deterministic test asserts that the task returned by `GetPrimeTask` for the first prime ends in `RanToCompletion` (not `Faulted`) after the sink has thrown. | `[x]` |
| AC4 | A regression test reproducing the Steps to Reproduce fails on the pre-fix code and passes after the fix, with the failing run recorded under the feature folder's `evidence/regression-testing/`. | `[x]` |
| AC5 | New tests use MSTest, Moq, and FluentAssertions, create no temporary files, and use no `Thread.Sleep` or `Task.Delay`; the C# toolchain (CSharpier, analyzers, nullable type-check, MSTest with coverage) passes, and the changed lines in `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` are covered. | `[x]` |
| AC6 | When the `logError` sink throws while `HandleToggleClickAsync` reports a faulted toggle, `HandleToggleClickAsync` does not throw and still attempts the report: the sink exception is contained inside the click boundary by its own guarded sink call, the sink receives the toggle fault unchanged, and no control is invalidated. | `[x]` |
| AC7 | A separate regression test for the `HandleToggleClickAsync` call site, using a throwing sink on a faulted toggle, fails on the pre-fix code and passes after the fix, with the failing run recorded under the feature folder's `evidence/regression-testing/`. | `[x]` |

## Acceptance Criteria Evaluation

| ID | Status | Evidence and reasoning |
|---|---|---|
| AC1 | PASS | Code: `CompletePrime` lines 406-415 — the sink call is inside `try`/`catch (Exception)`, `_primeTasks.TryRemove(engineName, out _)` follows the block unconditionally. Tests: `GetPressed_WhenLogSinkThrowsOnFaultedPrime_LaterReadStartsNewPrime` and `GetPressed_WhenLogSinkThrowsOnCanceledPrime_LaterReadStartsNewPrime` each verify `EngineActiveAsync` `Times.Exactly(2)` and `secondPrime.Should().NotBeSameAs(firstPrime)`; both Passed in P1-T12 and P2-T7, both Failed pre-fix with `Expected invocation on the mock exactly 2 times, but was 1 times`. Both the faulted and the canceled variant named in the criterion are covered. |
| AC2 | PASS | Ordering: `_logError` at line 408 precedes `TryRemove` at line 415 (re-read; executor `SPAN-LINE` rows 408 < 415). `EngineToggleStateCoordinatorTests.PrimeFaultOrdering.cs` byte-identical to BASE-SHA (`git diff --exit-code` 0; SHA-256 `AA88DC05...F8DB` equals `BASE-HASH-PFO`; positive-control diff on the production file exits 1); its test `GetPressed_WhenPrimeFaults_PrimeHandleStaysRegisteredUntilFaultIsLogged` Passed in every run (P0-T13, P1-T6, P1-T12, P2-T7). The new `FirstPrimeCompletesAndMarkerIsCleared` test additionally observes the still-registered handle from inside a *throwing* sink (`handleSeenBySink.Should().BeSameAs(firstPrime)`). |
| AC3 | PASS | Containment: the only statement inside the new guard is the sink call; after it, `CompletePrime` has no throw source for a faulted or canceled antecedent (`GetBaseException()` and `new TaskCanceledException(completed)` do not throw). The E3 remarks document this. Test: `GetPressed_WhenLogSinkThrows_FirstPrimeCompletesAndMarkerIsCleared` asserts `firstPrime.Status.Should().Be(TaskStatus.RanToCompletion)` on the handle returned by `GetPrimeTask` captured before the trigger, after the sink has thrown; Passed in both pass-after runs; deterministic (marker completed by program order). Disclosed nuance: the `RanToCompletion` assertion also holds pre-fix because of the #944 `finally`; the discriminating assertion in the same test is the final `GetPrimeTask(...).Should().BeSameAs(Task.CompletedTask)`, which Failed pre-fix. The criterion's wording ("asserts that the task ... ends in `RanToCompletion`") is satisfied as written. |
| AC4 | PASS | `evidence/regression-testing/throwing-sink-fail-before.md` (P1-T6): `EXIT_CODE: 1` with `ExpectedExitCode: 1`, `SEQUENCE_FILES: 0`, `PROD-HASH-AT-CONTROL` equal to `BASE-HASH-PROD` (`B3C6FEB2...3086`), production file diff against BASE-SHA exit 0 at the control; the three prime-site tests Failed with the reasons quoted in the MESSAGE lines. `throwing-sink-pass-after.md`: the same three Passed in P1-T12 and P2-T7 with the production hash changed to `EFC6F0DB...05BF` and the partial's hash identical in both runs. The reproduction matches Steps to Reproduce 1-3 (throwing sink, faulted prime, second `GetPressed`). |
| AC5 | PASS | Libraries: `using Microsoft.VisualStudio.TestTools.UnitTesting;`, `using Moq;`, `using FluentAssertions;` each present once; `[TestMethod]` x4; Moq via the strict `Harness` mock; FluentAssertions throughout. Prohibited tokens 0 (`Thread.Sleep`, `Task.Delay`, `GetTempPath`, `GetTempFileName`, `File.`, `Directory.`, `DoNotParallelize`) — `determinism-tokens.md`, re-verified by reading. Toolchain: `toolchain-final-pass.md` pass 1 clean — `csharpier format` rewrote nothing, `csharpier check` exit 0, analyzer Rebuild 0/0 with `SKIP_CORECOMPILE_LINES: 0`, nullable Rebuild 0/0, coverage run exit 0 with 7336/7336 and both floors MET. Changed lines: `CHANGED-LINES-WITH-ELEMENT: 12`, `CHANGED-LINES-UNCOVERED: 0`; both `CATCH-ARM-UNCOVERED: 0`; re-verified at the Cobertura class node (lines 188-196 and 407-416 all `hits="1"`). Recorded substitution: coverage step ran by the plan's DIRECT route with four UtilitiesCS.Test shell-icon classes excluded locally (pre-existing workstation stall; CI runs them) — non-blocking, see policy audit G-1. |
| AC6 | PASS | Code: `HandleToggleClickAsync` lines 185-195 — the nested `try`/`catch (Exception)` encloses only `_logError(BuildToggleFailedMessage(engineName), ex);`; `ex` is passed unchanged and not rethrown; no `_invalidateControl` call on this path. Test: `HandleToggleClickAsync_WhenLogSinkThrowsOnToggleFault_DoesNotThrowAndAttemptsReport` asserts `NotThrowAsync`, `Errors.Should().ContainSingle()`, `Errors[0].Exception.Should().BeSameAs(failure)`, `Invalidations.Should().BeEmpty()`, `Notifications.Should().BeEmpty()`; Passed in both pass-after runs. The pre-existing `HandleToggleClickAsync_WhenToggleFaults_LogsErrorDoesNotThrowDoesNotInvalidate` also Passed. "By its own guarded sink call" is met (the guard is inside the click boundary, separate from the `CompletePrime` guard). Coverage of the new arm: 3/3 lines (191, 192, 194). |
| AC7 | PASS | Fail-before: `FAILED HandleToggleClickAsync_WhenLogSinkThrowsOnToggleFault_DoesNotThrowAndAttemptsReport` with MESSAGE `Did not expect any exception because the click boundary contains a failure of the sink itself, but found System.InvalidOperationException: sink failed`, stack naming the base production file line 184 (the unguarded call), under `PROD-HASH-AT-CONTROL` = `BASE-HASH-PROD`, `EXIT_CODE: 1` / `ExpectedExitCode: 1`. Pass-after: Passed in P1-T12 and P2-T7. The test is in the same new partial as the prime-site tests but is a separate test method, which satisfies "a separate regression test for the `HandleToggleClickAsync` call site". |

Totals: 7 PASS, 0 PARTIAL, 0 FAIL, 0 UNVERIFIED.

## Acceptance Criteria Check-off

All seven criteria were already checked `[x]` by the executor (P2-T15 to P2-T21, one flip per task, criterion text unmodified; `ac-status-summary.md` records `AC_OPEN=0 AC_DONE=7` and `git diff --numstat` of 7 insertions / 7 deletions in `issue.md`). This review evaluated every criterion PASS, so no checkbox was changed and none was unchecked. Newly checked off by this review: none.

### Acceptance Criteria Status
- Source: docs/features/active/2026-09-30-engine-toggle-throwing-log-sink-leaves-stale-prime-marker-947/issue.md
- Total AC items: 7
- Checked off (delivered): 7
- Remaining (unchecked): 0
- Items remaining: none

## Summary

**Verdict: PASS.** 7/7 acceptance criteria met on evidence; 0 Blocking findings across the three review artifacts; 6 Non-blocking findings in the code review (CR-1 to CR-6) and 6 in the policy audit (PA-1 to PA-6, the same six observations), plus 5 recorded gaps (G-1 to G-5) that are environmental or pre-existing. Remediation inputs: not produced.

Independent verification performed by this review (no command executed): both changed source files and the project-file entry read in full; the `RibbonCommandBoundary.SafeLog` precedent and the fixture `Harness` read to confirm the claims the fix and tests rely on; the retained post-processed Cobertura documents read at the root and at the coordinator `<class>` node; the worktree reflog read to confirm head, base and commit times; the feature folder swept for host identifiers.

### Follow-ups (listed here, not filed; for the orchestrator)

| ID | Follow-up | Origin |
|---|---|---|
| F-1 | Guard the `_notifyUnavailable` sink call on the refusal path of `HandleToggleClickAsync` (line 177) and settle the policy for an `_enginesAccessor()` throw (line 175); amend the "This method therefore never throws" remark to name the guarded sinks precisely. Same defect class as #947, third sink site; candidate for a shared private `SafeLog` helper that also removes the duplicated guard (CR-1). | CR-2 / PA-4 |
| F-2 | Correct the `GetPrimeTask` `<returns>` opening sentence ("The prime task" -> the registration marker). | CR-5 / PA-5; #944 FU-3 |
| F-3 | Plan a split of `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` (476/500 lines) before its next change; the main test fixture is at 470/500. | CR-4 / PA-3 |
| F-4 | #944 FU-2 — log volume after a permanently faulted configuration load — remains open and is not re-filed by this item. | research section 7 item 2 |
| F-5 | Populate the canonical C# coverage artifact path or amend the review-hook convention so that the repository's `coverage/` route and the committed projection form are what the hook reads. Pre-existing across #942/#944/#940/#947. | PA G-4 |
