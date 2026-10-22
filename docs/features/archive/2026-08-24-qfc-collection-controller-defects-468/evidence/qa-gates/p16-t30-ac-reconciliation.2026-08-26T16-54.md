# [P16-T27 / T28 / T29 / T30] Acceptance-criteria reconciliation

Timestamp: 2026-08-26T16-54

Command:

```
grep -n '^- \[[ x]\] \*\*AC-' docs/features/active/qfc-collection-controller-defects-468/spec.md
# per pointer: test -f <path>
```

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

**Twenty-six checked boxes (AC-1 through AC-26). Three deferred rows (AC-27, AC-28, AC-29), each
unchecked and each naming the artifact the orchestrator must consume. Zero unresolved pointers.**

AC source: `docs/features/active/qfc-collection-controller-defects-468/spec.md`, the sole
acceptance-criteria source for this `full-bug` feature.

### Where the evidence pointers live, and why

Per `.claude/skills/acceptance-criteria-tracking/SKILL.md`, checking off an AC changes only `- [ ]`
to `- [x]` and must **not** modify the criterion text. Every evidence pointer is therefore recorded
in this artifact rather than inline in `spec.md`. The plan's Phase 16 preamble asks for the pointer to
be recorded "alongside" the check-off; this artifact is that record, and `spec.md` carries checkbox
flips only. `git diff --stat` on `spec.md` confirms it: 11 insertions, 11 deletions — one line
rewritten per box flipped, with no line added.

---

## Checked: AC-1 through AC-26

| AC | Task | Evidence pointer(s), relative to `<FEATURE>/evidence/` | Resolves? |
|---|---|---|---|
| AC-1 (#286) | P16-T1 | `regression-testing/p3-t2-fail-before.2026-08-26T09-42.md`, `regression-testing/p3-t3-fail-before.2026-08-26T09-48.md`, `regression-testing/p3-t5-pass-after.2026-08-26T09-53.md` | yes |
| AC-2 (#468) | P16-T2 | `qa-gates/p1-t3-dead-identifier-sweep.2026-08-26T08-45.md` | yes |
| AC-3 (#468 non-regression) | P16-T3 | `qa-gates/p1-t4-live-member-nonregression.2026-08-26T08-45.md` | yes |
| AC-4 (#469-1) | P16-T4 | `regression-testing/p6-t3-fail-before.2026-08-26T10-17.md`, `regression-testing/p6-t5-pass-after.2026-08-26T10-22.md` | yes |
| AC-5 (#469-2) | P16-T5 | `regression-testing/p6-t1-fail-before.2026-08-26T10-17.md`, `regression-testing/p6-t2-fail-before.2026-08-26T10-17.md`, `regression-testing/p6-t5-pass-after.2026-08-26T10-22.md` | yes |
| AC-6 (#469-3) | P16-T6 | `regression-testing/p4-t3-fail-before.2026-08-26T10-03.md`, `regression-testing/p4-t7-pass-after.2026-08-26T10-12.md` | yes |
| AC-7 (#469-4) | P16-T7 | `regression-testing/p12-t3-pass-after.2026-08-26T11-37.md`, `qa-gates/p12-t5-commit.2026-08-26T11-38.md` (records the P12-T1 interface doc block and the P12-T2 discard) | yes |
| AC-8 (#470-1) | P16-T8 | `regression-testing/p8-t1-fail-before.2026-08-26T10-45.md`, `regression-testing/p8-t2-fail-before.2026-08-26T10-45.md`, `regression-testing/p8-t4-pass-after.2026-08-26T10-48.md` | yes |
| AC-9 (#470-2) | P16-T9 | `regression-testing/p7-t12-pass-after.2026-08-26T10-39.md` (covers P7-T7 through P7-T12, all six tests), plus decisions D5, D6 and D7 of `plan.2026-08-24T09-39.md` | yes |
| AC-10 (#470-3) | P16-T10 | `regression-testing/p9-t1-fail-before.2026-08-26T11-00.md`, `regression-testing/p9-t3-pass-after.2026-08-26T11-02.md` | yes |
| AC-11 (#471) | P16-T11 | `regression-testing/p10-t6-fail-before.2026-08-26T11-16.md`, `regression-testing/p10-t9-pass-after.2026-08-26T11-19.md`, `regression-testing/p10-t10-neutrality.2026-08-26T11-21.md`, plus decision D10 recording the `Size`/`MinimumSize` asymmetry | yes |
| AC-12 (#473-1) | P16-T12 | `regression-testing/p11-t4-fail-before.2026-08-26T11-28.md`, `regression-testing/p11-t6-pass-after.2026-08-26T11-30.md` | yes |
| AC-13 (#473-2) | P16-T13 | `regression-testing/p5-t1-fail-before.2026-08-26T10-24.md`, `regression-testing/p5-t2-fail-before.2026-08-26T10-27.md`, `regression-testing/p5-t5-pass-after.2026-08-26T10-33.md` | yes |
| AC-14 (#474-1) | P16-T14 | `regression-testing/p2-t6-fail-before.2026-08-26T09-14.md`, `qa-gates/p2-t9-downcast-sweep.2026-08-26T09-20.md`, `regression-testing/p2-t10-pass-after.2026-08-26T09-21.md` | yes |
| AC-15 (#474-2) | P16-T15 | `regression-testing/p13-t6-pass-after.2026-08-26T16-18.md` (covers the P13-T4 and P13-T5 tests and the P13-T6 run) | yes |
| AC-16 (#468 residual risk) | P16-T16 | `other/p1-t1-reflective-caller-search.2026-08-26T08-25.md` | yes |
| AC-17 (fix order) | P16-T17 | `qa-gates/p14-t7-fix-order-audit.2026-08-26T16-34.md` | yes |
| AC-18 (bugfix workflow) | P16-T18 | `qa-gates/p14-t8-fail-before-index.2026-08-26T16-30.md` | yes |
| AC-19 (fail-before dossier) | P16-T19 | `regression-testing/fail-before-exception.2026-08-26T16-24.md` | yes |
| AC-20 (seams) | P16-T20 | `qa-gates/p14-t9-seam-audit.2026-08-26T16-35.md` | yes |
| AC-21 (owned-file discipline) | P16-T21 | `qa-gates/p14-t10-scope-lock-audit.2026-08-26T16-37.md` | yes |
| AC-22 (test-file constraints) | P16-T22 | `qa-gates/p14-t11-test-file-constraints.2026-08-26T16-38.md`, `qa-gates/p15-t7-file-size-audit.2026-08-26T16-49.md` | yes |
| AC-23 (test policy) | P16-T23 | `qa-gates/p14-t12-test-policy-audit.2026-08-26T16-39.md` | yes |
| AC-24 (toolchain) | P16-T24 | `qa-gates/p15-t1-format.2026-08-26T16-43.md`, `qa-gates/p15-t2-format-check.2026-08-26T16-44.md`, `qa-gates/p15-t3-analyzers.2026-08-26T16-45.md`, `qa-gates/p15-t4-nullable.2026-08-26T16-46.md`, `qa-gates/p15-t5-tests-coverage.2026-08-26T16-47.md`, `qa-gates/p15-t6-loop-record.2026-08-26T16-48.md` | yes |
| AC-25 (no scope creep) | P16-T25 | `qa-gates/p14-t13-scope-creep-audit.2026-08-26T16-40.md` | yes |
| AC-26 (downstream handoff) | P16-T26 | `other/downstream-handoff-444.2026-08-26T16-26.md`, plus the `## Downstream Notes for Sibling Issues` section of `spec.md` | yes |

**Twenty-six checked. Fifty distinct evidence paths named. Every one was verified to exist with
`test -f`; zero unresolved.**

## Three check-offs that carry a qualification

These are checked because the criterion is met, but each has a nuance a reviewer should see rather
than discover. None of the three is a case of flipping a box to clear a gate.

### AC-19 — the dossier records seven items, not four

AC-19 names four items with no deterministic pre-fix red state. The dossier at
`regression-testing/fail-before-exception.2026-08-26T16-24.md` records **seven**: the four AC-19 names
(#469-3 behavioural ordering, #468, #474-1, #469-4) plus three the plan's P14-T1 task added
(#470-2 above-reservation, the base-email-index guard, and #474-2 readiness). Seven is a superset of
four, so the criterion is satisfied and then some. AC-19's text was written before P13's seam existed;
the additional three reflect what the work actually required.

### AC-21 — the merge-base diff includes sibling-derived paths

AC-21 asks that the diff touch only owned files, verified by `git diff --name-only` against the merge
base. That command now returns 510 paths, because the orchestrator merged
`origin/epic/quickfiler-bug-family-integration` into this branch twice before the final QA loop so
that the loop would run against the tree that will actually be reviewed. 39 of the 49 code paths in
that diff are sibling-derived.

`qa-gates/p14-t10-scope-lock-audit.2026-08-26T16-37.md` separates the two: this feature's own
contribution is `61edc19b..48c9ad8f` plus `5f8026aa`, which is **10 code paths, every one a member of
the owned file set**, with an **empty out-of-scope set**. The 39 sibling paths are enumerated in full
so nobody mistakes them for scope creep here. The three must-not-write files —
`KbdActions.cs`, `QfcFormController.EventHandlers.cs`, `EfcFormController.cs` — appear **zero** times
even in the full 510-path diff.

The criterion is met on its substance: this feature modified no file outside its ownership.

### AC-23 — four banned-API literals occur in doc comments

AC-23 requires that the new tests contain no `Thread.Sleep`, no `Task.Delay`, and no
`UiThread.Init()`, and that the STA class call neither `Show()` nor `ShowDialog()`. The
executable-code search returns **0, 0, 0, 0**. A raw every-line search returns **1, 1, 1, 1**, and all
four hits are `///` XML doc-comment lines that state the API is *not* used — two of which are required
by other decisions in this plan (D9 mandates the STA class's explanatory comment).

`qa-gates/p14-t12-test-policy-audit.2026-08-26T16-39.md` records both measurements side by side and
explains why the comments were not deleted: removing them would satisfy a text search by destroying
documentation the plan itself requires. The criterion is met on its behavioural substance — no test
calls any banned API.

## One finding that qualifies AC-22 without falsifying it

`qa-gates/p15-t7-file-size-audit.2026-08-26T16-49.md` records that
`QuickFiler/Controllers/QfcCollectionController.cs` is **2,437 lines** post-feature against **2,349**
at the base commit: the feature **increased** it by 88 lines. The plan's P15-T7 acceptance asked for
a statement that the excess over the 500-line cap "is a pre-existing condition this feature reduces
rather than creates"; the first half is true and the second half is false, and the artifact says so
rather than asserting it.

This does **not** affect AC-22, which constrains *test* files: it requires no new test method in
`QfcCollectionControllerTests.cs` (13 before, 13 after), every new test file under 500 lines (154,
494, 497, 432, 183), and the five `Compile Include` entries between the dark-mode and datamodel
entries (lines 121-125, between 120 and 126). All three hold.

Nor does it affect AC-25, which forbids a partial-class split (none performed), removal of
`[ExcludeFromCodeCoverage]` (still at `:21`), package additions (zero), and removal of the
`stackMovedItems` parameter (retained, byte-identical).

The controller's size is claimed by **open issue #623**, whose recorded baseline of 2,349 lines is now
stale by 88.

---

## Deferred: AC-27, AC-28, AC-29 — DEFERRED-TO-ORCHESTRATOR

All three remain **unchecked** in `spec.md`. Each is satisfied only by orchestrator-owned work that
does not occur within this plan, and the acceptance-criteria-tracking rule is that evidence precedes
check-off.

| AC | Status | Artifact the orchestrator must consume | Why the executor cannot check it |
|---|---|---|---|
| **AC-27 (PR accuracy)** | **DEFERRED-TO-ORCHESTRATOR** | `<FEATURE>/evidence/other/pr-accuracy-constraints.2026-08-26T16-27.md` | The criterion is a property of the **PR body**, which the orchestrator authors. No PR body exists at the end of this plan, so there is nothing to verify. The artifact enumerates all five constraints — the two prohibited premises, the two required latency statements, and the per-defect test-name map that replaces a coverage delta. |
| **AC-28 (issue closure)** | **DEFERRED-TO-ORCHESTRATOR** | `<FEATURE>/evidence/other/issue-closure-set.2026-08-26T16-28.md` | The criterion is satisfied by the **merge** that closes the seven issues. No merge occurs in this plan. The artifact maps all seven issues (#286, #468, #469, #470, #471, #473, #474) to their acceptance criteria and gives the exact seven-line closing-keyword block for the PR body. No closing keyword appears in any commit message on this branch, deliberately. |
| **AC-29 (follow-ups filed)** | **DEFERRED-TO-ORCHESTRATOR** | `<FEATURE>/evidence/other/followup-promotion-handoff.2026-08-26T16-32.md` | The criterion requires a real issue number for every one of the nine follow-up candidates. That handoff records **`PROMOTION_DEFERRED`**: the potential-to-issue promotion tooling is not present in this executor's tool surface. Two candidates map to existing open issues (#623, #444); the other seven have new potential entries under `docs/features/potential/` awaiting promotion. Per P16-T29, a `PROMOTION_DEFERRED` disposition on any row keeps the box unchecked. |

### AC-29 in detail — the seven entries awaiting promotion

| Candidate | Potential entry |
|---|---|
| 2 — remove the `stackMovedItems` parameter | `docs/features/potential/2026-08-26-qfc-remove-stackmoveditems-parameter.md` |
| 3 — relocate the `ReadyForMove` presentation | `docs/features/potential/2026-08-26-qfc-relocate-readyformove-presentation-to-caller.md` |
| 4 — consolidate the two form-controller interfaces | `docs/features/potential/2026-08-26-consolidate-ifilerformcontroller-and-iqfcformcontroller.md` |
| 5 — remove the orphan `QuickFiler.Interfaces.IQfcFormController` | `docs/features/potential/2026-08-26-remove-orphan-quickfiler-interfaces-iqfcformcontroller.md` |
| 7 — file the unsynchronized undo handoff | `docs/features/potential/2026-08-26-qfc-unsynchronized-undo-handoff-after-batch-move.md` |
| 8 — the unsynchronized plain read of the reentrancy counter | `docs/features/potential/2026-08-26-qfc-unsynchronized-plain-read-reentrancy-counter.md` |
| 9 — settle the #468 residual risk repository-wide | `docs/features/potential/2026-08-26-issue-468-residual-reflective-caller-risk.md` |

All seven exist on disk. Candidates 1 and 6 map to open issues **#623** and **#444** respectively and
need no promotion.

---

## Acceptance Criteria Status

```
### Acceptance Criteria Status
- Source: docs/features/active/qfc-collection-controller-defects-468/spec.md
- Total AC items: 29
- Checked off (delivered): 26
- Remaining (unchecked): 3
- Items remaining:
    AC-27 (PR accuracy)     — DEFERRED-TO-ORCHESTRATOR, PR body not yet authored
    AC-28 (issue closure)   — DEFERRED-TO-ORCHESTRATOR, closing merge has not occurred
    AC-29 (follow-ups filed) — DEFERRED-TO-ORCHESTRATOR, PROMOTION_DEFERRED (tooling unavailable to the executor)
```

## Acceptance verification

| Clause | Status |
|---|---|
| the artifact records twenty-six checked boxes | met — AC-1 through AC-26, tabulated with pointers |
| three deferred rows | met — AC-27, AC-28, AC-29, each naming the artifact the orchestrator must consume |
| zero unresolved pointers | met — all fifty distinct evidence paths verified with `test -f`; none missing |
