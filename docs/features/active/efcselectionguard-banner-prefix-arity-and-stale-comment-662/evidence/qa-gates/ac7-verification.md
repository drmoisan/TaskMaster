# AC7 Verification (P2-T18)

Timestamp: 2026-09-01T16-55

## Half 1 — the file is unmodified

Command: `git diff 43dcc800e5c75ab1d1033f0eac0e4b61ac919b59 --stat -- QuickFiler.Test/Controllers/EfcFormControllerTests.cs`

EXIT_CODE: 0

Output Summary: the command produced **empty output**.
`QuickFiler.Test/Controllers/EfcFormControllerTests.cs` shows no change against
the anchor.

### Anchor — the Execution Amendment applies to this task

This task is one of the three amended by the plan's "Execution Amendment —
corrected diff anchor (orchestrator, 2026-09-01)", which overrides the pinned
anchor `2b85134b42872e405602e6064e02dc9cda6c319b` for P2-T16, P2-T18 and P2-T23.
The pinned anchor is an ancestor of both HEAD and `origin/main`, so the two-dot
diff form would additionally report everything `origin/main` accumulated since
it, making the gate unsatisfiable as written.

The anchor was resolved at run time rather than pasted as a literal:

```
git merge-base origin/main HEAD
-> 43dcc800e5c75ab1d1033f0eac0e4b61ac919b59
```

The merge-base two-dot form is used rather than the three-dot
`origin/main...HEAD` form because this gate must report the worktree state
whether or not the work has been committed.

The P0-T7 baseline recorded `dotnet tool run csharpier check QuickFiler.Test/Controllers/EfcFormControllerTests.cs`
exiting 0, so the repository-wide format pass in P2-T1 had no pre-existing drift
to repair in this file, and the P2-T1 before-and-after tree observation confirms
the file appears in neither listing.

## Half 2 — the test still passes

Source: the TRX produced by P2-T6 at
`docs/features/active/efcselectionguard-banner-prefix-arity-and-stale-comment-662/evidence/regression-testing/p2-t6/ac7-scoped.trx`

`<Counters ... />` line:

```
<Counters total="1" executed="1" passed="1" failed="0" error="0" timeout="0" aborted="0" inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0" disconnected="0" warning="0" completed="0" inProgress="0" pending="0" />
```

`passed="1"` and `failed="0"`, which are the figures AC7 names for a scoped run
with `/Tests:IsSelectableFolder_AndIsBannerRow_ClassifyThreeAndFourEqualsRowsIdentically`.

The P2-T6 artifact records the staleness guard: the results directory was
deleted before the run and the produced TRX's `LastWriteTime` (16:01:55) is later
than P2-T1's `Timestamp:` for the current pass (15-59).

## Both halves hold

The pre-existing test
`IsSelectableFolder_AndIsBannerRow_ClassifyThreeAndFourEqualsRowsIdentically` in
`QuickFiler.Test/Controllers/EfcFormControllerTests.cs` is unmodified and still
passes.

This is the merged guard the plan's Directional Constraint protects. The
fail-before dossier records that under the prohibited widening edit, that test's
assertion at `:463` fails while its sibling at `:462` still passes — so the
assertion a reader would expect to catch a relaxation does not catch it, and only
the explicit rejection assertion does.

**AC7 checked off in `issue.md`.**
