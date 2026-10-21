# File-size and diff-bound audit (post-format)

Timestamp: 2026-09-03T15-15

Task: [P5-T10]
Issue: #731

**STATUS: COMPLETE — every acceptance condition of [P5-T10] is met.**

This artifact replaces the record written by the blocked first attempt at this task wholesale, as [P5-T10] requires. That earlier record measured the two numstat rows against the superseded bounds of 2 insertions / 1 deletion and 1 insertion / 0 deletions, and reported them as not met. Plan revision round 12 corrected both bounds to 3 insertions / 1 deletion and 2 insertions / 0 deletions, each budgeting one CSharpier-mandated blank line above an inserted comment, with the reason recorded inside the acceptance so the widening is auditable. The measurements below are taken against the corrected bounds and are met. Wholesale replacement rather than an appended second pass is required because [P6-T20] checks spec.md AC19 off against this exact path, and a retained superseded verdict would leave AC19 checked off against a document reporting its own numeric proxy as unmet.

## Command

```
(Get-Content -LiteralPath '<path>').Count          for each of the eleven paths below
git diff --numstat 35583f7c7e1f1c9b97e4f6f1e7846a3f2693c17e -- QuickFiler/Controllers/QfcCollectionController.cs QuickFiler/Controllers/QfcQueue.cs
```

The `<DIFF-BASE>` operand is the 40-character SHA recorded on the `Diff base:` line of `EVIDENCE/baseline/tree-invariants.md` by `[P0-T2]`, substituted verbatim: `35583f7c7e1f1c9b97e4f6f1e7846a3f2693c17e`. It is byte-identical to that recorded value. The literal ref `origin/main` was not used.

All line counts were taken after `[P5-T1]`, so they measure post-format content.

EXIT_CODE: 0

## Output Summary

### Line counts

Eleven paths: the seven `[P0-T3]` paths, the three test files this plan creates, and `QuickFiler/Controllers/QfcRemainingQueueAdmission.cs`.

| Path | Post-change lines | [P0-T3] baseline | Bound | Result |
|---|---|---|---|---|
| `QuickFiler/Controllers/QfcCollectionController.cs` | 2329 | 2327 | none (disclosed pre-existing debt) | recorded |
| `QuickFiler/Controllers/QfcQueue.cs` | 507 | 505 | none (disclosed pre-existing debt) | recorded |
| `QuickFiler/Controllers/QfcDatamodel.cs` | 480 | 480 | none | recorded |
| `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs` | 265 | 234 | none | recorded |
| `QuickFiler/Controllers/QfcRemainingQueueAdmission.cs` | 40 | n/a (not a [P0-T3] path) | none | recorded |
| `QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs` | 496 | 496 | must equal [P0-T3] | **PASS** |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs` | 498 | 498 | must equal [P0-T3] and be at most 500 | **PASS** |
| `QuickFiler.Test/Controllers/QfcDatamodelTests.cs` | 371 | 401 | none | recorded |
| `QuickFiler.Test/Controllers/QfcMoveMonitorTopologyTests.cs` | 187 | n/a (created) | at most 500 | **PASS** |
| `QuickFiler.Test/Controllers/QfcFormControllerCleanupTests.cs` | 399 | n/a (created) | at most 500 | **PASS** |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.Volatile.cs` | 120 | n/a (created) | at most 500 | **PASS** |

`QuickFiler/Controllers/QfcDatamodel.cs` is unchanged at 480 lines because its two added lines (comment plus formatter-required blank) are exactly offset by the two lines `[P3-T4]` removed from the construction site.

`QuickFiler.Test/Controllers/QfcDatamodelTests.cs` is 30 lines shorter than its baseline because `[P3-T5]` updated the test factory to the reduced `QfcRemainingQueueAdmission` constructor, removing the setup for the two deleted parameters. It carries no bound in this task.

### Numstat rows

```
3	1	QuickFiler/Controllers/QfcCollectionController.cs
2	0	QuickFiler/Controllers/QfcQueue.cs
```

| Path | Observed | Bound | Result |
|---|---|---|---|
| `QuickFiler/Controllers/QfcCollectionController.cs` | 3 insertions, 1 deletion | at most 3 insertions, at most 1 deletion | **PASS** — met exactly |
| `QuickFiler/Controllers/QfcQueue.cs` | 2 insertions, 0 deletions | at most 2 insertions, 0 deletions | **PASS** — met exactly |

Both rows sit exactly on their bounds. One further inserted line in either file would breach it, so the bounds remain capable of reporting a breach rather than passing unconditionally.

## Decomposition of the two numstat rows

Each bound budgets one blank line in addition to the content spec.md AC19 at line 235 bounds. CSharpier requires a blank line immediately above an inserted comment in these positions, so a standalone comment line costs two insertions rather than one.

- `QuickFiler/Controllers/QfcCollectionController.cs` — 3 insertions and 1 deletion, decomposing as: the `[P4-T5]` reentrancy-guard rewrite, one insertion plus one deletion; the `[P1-T1]` per-owner comment, one insertion; and the formatter-mandated blank line above that comment, one insertion.
- `QuickFiler/Controllers/QfcQueue.cs` — 2 insertions and 0 deletions, decomposing as: the `[P1-T3]` per-owner comment, one insertion; and the formatter-mandated blank line above it, one insertion.

The formatter requirement was established by observation rather than by inference. Removing the blank line from `QuickFiler/Controllers/QfcQueue.cs` made `dotnet tool run csharpier check` on that file exit non-zero, reporting `Was not formatted` with an `Expected: Around Line 40` block containing the blank line; rewriting the comment in `///` doc-comment form produced the identical result. Neither alternative placement costs one net insertion: a trailing comment on the declaration line, and a compensating deletion of a nearby blank line, are each one insertion plus one deletion and would breach the zero-deletion bound on `QuickFiler/Controllers/QfcQueue.cs` instead. The tree was restored to the formatter-stable form, after which the repository-wide `csharpier check .` exits 0, as `EVIDENCE/qa-gates/csharpier-check.md` records.

## Relation to spec.md AC19

The content bound spec.md AC19 states — that the diff to `QuickFiler/Controllers/QfcCollectionController.cs` is limited to one statement and one comment line — is unchanged by the corrected numeric bounds and is exactly what landed. The one additional insertion in each file is formatter-mandated whitespace rather than content.

The corrected bounds still forbid a file split or any material growth: 3 insertions with 1 deletion cannot accommodate a split of a 2329-line file or any substantive addition to it, and 2 insertions with 0 deletions forbids even a single deleted line.

`QuickFiler/Controllers/QfcCollectionController.cs` at 2329 lines and `QuickFiler/Controllers/QfcQueue.cs` at 507 lines both remain over the repository's 500-line ceiling as disclosed pre-existing debt. Splitting either is an explicit non-goal of issue #731 and is recorded as a follow-up in spec.md and at the foot of the plan.

The remaining AC19 conjuncts are evidenced in `EVIDENCE/qa-gates/scope-boundary.md`: the anchored name-status list contains zero paths whose filename ends in `Metrics.cs`, and `QuickFiler/Controllers/QfcCollectionController.cs` is not split.

## Acceptance conditions of [P5-T10]

| Condition | Observation | Result |
|---|---|---|
| Artifact records the substituted 40-character base SHA rather than the token `origin/main` | `35583f7c7e1f1c9b97e4f6f1e7846a3f2693c17e` recorded in `Command:` | **PASS** |
| That SHA is byte-identical to the `Diff base:` value in `EVIDENCE/baseline/tree-invariants.md` | Identical | **PASS** |
| `QuickFiler/Controllers/QfcCollectionController.cs` numstat at most 3 insertions and at most 1 deletion | 3 and 1 | **PASS** |
| `QuickFiler/Controllers/QfcQueue.cs` numstat at most 2 insertions and 0 deletions | 2 and 0 | **PASS** |
| `QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs` line count equals its [P0-T3] value | 496 = 496 | **PASS** |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs` line count equals its [P0-T3] value and is at most 500 | 498 = 498, and 498 <= 500 | **PASS** |
| Each of the three new test files is at most 500 lines | 187, 399, 120 | **PASS** |
| This artifact replaces the superseded record wholesale rather than appending to it | Written as a full replacement; no second-pass section retained | **PASS** |
| This artifact contains neither of the two tokens the superseded record carried | Neither appears | **PASS** |
