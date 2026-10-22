# Code Review: Remove `stackMovedItems` parameter from `MoveEmailsAsync` (#629)

**Review Date:** 2026-09-02
**Reviewer:** Claude (fork, orchestration session)
**Feature Folder:** `docs/features/active/2026-08-26-qfc-remove-stackmoveditems-parameter-629`
**Feature Folder Selection Rule:** Pre-existing promoted folder for issue #629; only `issue.md` had real
content at the start of this run, `spec.md`/`user-story.md`/`plan.md` were empty scaffolds authored
during this run.
**Base Branch:** `main`
**Head Branch:** `feature/qfc-remove-stackmoveditems-parameter-629`
**Review Type:** Initial review

---

## Executive Summary

The change removes an unused `stackMovedItems` parameter from `IQfcCollectionController.MoveEmailsAsync`
and its implementation, updates the single production call site, and updates four tests across two test
files. Total footprint: 5 files, no new files, no dependency changes.

**What changed:**
`IQfcCollectionController.cs` and `QfcCollectionController.cs` drop the parameter and its `<param>` doc
comment (replaced with an explanatory `<remarks>` block); `QfcCollectionController.cs` also drops the
`_ = stackMovedItems;` discard statement that issue #468 had added as an interim marker. The sole call
site in `QfcFormController.EventHandlers.cs` drops its argument. Four tests in two files are updated:
three call-shape fixes with no behavioral change, and one test rewritten to preserve early-return-branch
coverage that the old test's null-vs-supplied-stack comparison happened to also provide.

**Top 3 risks:**
1. A missed mock/setup site elsewhere in the test suite would fail to compile — mitigated by a
   full-repo grep (`evidence/baseline/p0-t8-mock-sweep.md`) that found all 6 call sites (1 more than the
   1 named in `issue.md`) before editing began.
2. Deleting rather than rewriting the null-stack test could silently drop branch coverage on the
   early-return path — mitigated by confirming via grep that this was the only test setting
   `_itemGroupsToMove` to an empty collection, and rewriting instead of deleting.
3. Cobertura's root rate attributes appear to show a coverage regression — this is a known, already-filed
   tooling defect (#529/#530), not a real regression; the true per-line sums show a marginal improvement.

**PR readiness recommendation:** **Go** — clean toolchain pass, no test-count change, no behavioral
change outside the documented scope, footprint matches the prediction in `spec.md`.

---

## Findings Table

No Blockers or Major findings.

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Info | `QuickFiler/Controllers/QfcCollectionController.cs` | doc comment | The removed `<param>` element is replaced by a `<remarks>` block explaining the undo-stack invariant rather than simply deleted. | None — this is the correct choice. | Preserves the "why" (issue #629's rationale) for future readers instead of leaving a bare signature. | Diff inspection. |
| Nit | `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs` | rewritten test | Test renamed from `MoveEmailsAsync_WithNullStack_BehavesIdenticallyToAnEmptyStack` to `MoveEmailsAsync_WithEmptyItemGroupsToMove_DoesNotThrow`. | None — rename is accurate to the new scope. | The old name described a comparison that no longer exists after the parameter removal. | `evidence/other/p1-t5-test-disposition.md` |

---

## Implementation Audit

### C# implementation audit

#### What changed well

- The scope stayed minimal: exactly the files predicted in `spec.md`'s Implementation Strategy section
  were touched, confirmed post-hoc by `evidence/other/p1-t6-footprint-check.md`.
- The doc-comment rewrite on `IQfcCollectionController.MoveEmailsAsync` explains *why* no argument is
  needed (citing where the undo stack is actually populated), following the repository's "comment why,
  not what" convention rather than just deleting the stale `<param>` block.
- The P0-T8 mock/setup sweep caught a call-site count (4 direct calls, plus 2 mock setup/verify sites)
  larger than the 1 the source issue anticipated, preventing a compile break that would only have
  surfaced later in the toolchain.

#### Type safety and API notes

- `IQfcCollectionController.MoveEmailsAsync()` remains a `Task`-returning public contract; removing a
  parameter that was never read by any implementation cannot introduce a nullable-flow change. The
  nullable build (`evidence/qa-gates/p2-t4-nullable-build.md`) confirms 0 new warnings.
- No public API outside `QuickFiler` is affected — `IQfcCollectionController` has a single production
  implementation and a single production call site, both first-party and in-repo.

#### Error handling and logging

- The only logging-adjacent line touched was a comment (`//TraceUtility.LogMethodCall(stackMovedItems);`),
  already dead code before this change; updating it to drop the removed argument keeps the comment
  internally consistent but changes no runtime behavior.

---

## Test Quality Audit

Four tests across two files were updated. Three (`MoveEmailsAsync_WhenMoveIsCancelled_...`,
`MoveEmailsAsync_AfterFirstFailure_...`, `MoveEmailsAsync_WithNullGroupFromIndexLookup_...`) required
only a call-shape fix (`MoveEmailsAsync(null)` → `MoveEmailsAsync()`); none of them asserted on the
removed argument's value, so no behavioral change resulted. The fourth was rewritten in place, and two
Moq sites in a fifth file's test class were updated to the new zero-argument signature.

### Reviewed test and QA artifacts

- `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs` — 4 call sites, 1 test
  body rewritten; verified no orphaned `SloStack`/`IMovedMailInfo` usage in the file after the change.
- `QuickFiler.Test/Controllers/QfcFormControllerUndoHandoffTests.cs` — 1 `Setup`, 1 `Verify` updated;
  confirmed `SloStack<IMovedMailInfo>` is still legitimately referenced once elsewhere in the file
  (`_mockAF.SetupGet(a => a.MovedMails)...`), so no unused-using cleanup was needed.
- `evidence/baseline/p0-t7-baseline-coverage.md` / `evidence/qa-gates/p2-t5-final-coverage.md` — full
  suite run, 6949/6949 passing both before and after.
- `evidence/qa-gates/p2-t6-coverage-delta.md` — coverage delta computed from reliable Cobertura sums,
  disposition: no regression.

### Quality assessment prompts

- **Determinism:** no randomness, timers, or wall-clock reads in any touched test.
- **Isolation:** each test still targets one behavior of `MoveEmailsAsync`; the rewrite narrowed one
  test's assertion surface rather than broadening it.
- **Speed:** no new I/O or waits introduced; full suite (6949 tests) ran in both baseline and final
  passes with no observed slowdown.
- **Diagnostics:** the rewritten test's `NotThrowAsync(because: ...)` carries an explicit rationale
  string identifying the early-return branch it pins.

---

## Security / Correctness Checks

| Check | Status | Evidence |
|---|---|---|
| No secrets in code | PASS | Diff inspection — no new literals of any kind. |
| No unsafe subprocess or command construction | N/A | No process/command code touched. |
| Input validation at boundaries | N/A | No external input boundary touched; internal signature change only. |
| Error handling remains explicit | PASS | No `catch`/`throw` logic touched; nullable build clean. |
| Configuration / path handling is safe | N/A | No configuration or path code touched. |

---

## Research Log

No external research was required. All context came from the pre-existing `issue.md` (which documents
the issue #468/#469 history and the actual undo-stack population path in `EmailFiler.cs`) and direct
repository inspection (grep sweeps, file reads).

---

## Verdict

The change is a minimal, well-scoped signature simplification with a clean toolchain pass and no
test-count regression. The one test requiring more than a mechanical fix (the null-stack comparison
test) was correctly disposed of by rewrite rather than deletion, preserving its early-return-branch
coverage. Ready for normal PR flow.
