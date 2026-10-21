# Phase 6 — Write Set boundary check

Timestamp: 2026-09-09T14-55

Task: [P6-T12]

Command: `git diff --name-status d636b0f28f548181685260d929de6d7d2940d1da...HEAD`
Command: `git status --porcelain --untracked-files=all`

The 40-character SHA is transcribed from the `BASE-SHA:` field of
`evidence/baseline/p0-t2-branch-and-base.md` per D2.

EXIT_CODE: 0

## CHANGED-PATHS

From the `git diff --name-status` span, ten paths outside this feature's own evidence folder, all
with status `M`:

- M QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs
- M QuickFiler.Test/Viewers/BreadcrumbPopupOwnerRegistryTests.cs
- M QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs
- M QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs
- M QuickFiler/Viewers/QfcFormViewer.cs
- M UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs
- M UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs
- M UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs
- M UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs
- M docs/features/active/2026-09-07-utilitiescs-archive-root-read-and-user-email-retry-801-805-812/spec.md

plus 35 paths with status `A` under
`docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/`, being the
Phase 0 through Phase 5 evidence artifacts already committed.

From the `git status --porcelain --untracked-files=all` span, twelve paths:

- ` M docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/plan.2026-09-08T23-50.md`
- `?? docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/other/p5-t4-r5-comment-only-diff.md`
- `?? docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/other/p5-t5-r2-decision-fence.md`
- `?? docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/qa-gates/p6-t1-csharpier-format.md`
- `?? docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/qa-gates/p6-t2-csharpier-check.md`
- `?? docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/qa-gates/p6-t3-msbuild-analyzers.md`
- `?? docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/qa-gates/p6-t4-msbuild-nullable.md`
- `?? docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/qa-gates/p6-t5-vstest-enablecodecoverage.md`
- `?? docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/qa-gates/p6-t6-coverage.md`
- `?? docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/qa-gates/p6-t7-loop-closure.md`
- `?? docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/qa-gates/p6-t8-file-size-audit.md`
- `?? docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/qa-gates/p6-t9-coverage-delta.md`
- `?? docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/qa-gates/p6-t10-changed-line-coverage.md`

Every path in the union lies either in the Write Set or inside
`docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/`.

## INHERITED-PATHS

- `docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/plan.2026-09-08T23-50.md` — clause A
- `docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/baseline/phase0-instructions-read.md` — clause A

Those are the two paths the [P0-T2] `INHERITED-CLAUSE-A:` block captured, both produced by
[P0-T1]. Both are also Write Set or feature-folder paths on their own terms, so neither depends on
the subtraction.

Clause B, membership of `.claude/agent-memory/`, subtracts nothing here: no path in the union lies
under `.claude/agent-memory/` or under `.claude/` at all. The executing agent made no
persistent-memory write during this run, a decision recorded in the [P6-T11] artifact.

## AC28-SCOPE-PATHS

The subset of `CHANGED-PATHS` that AC28 actually ranges over is every path from the
`git diff --name-status` span together with every porcelain row whose status code is `??`. That is
the ten modified source and document paths, the 35 added evidence paths, and the twelve untracked
evidence paths listed above: 57 paths in total. The one porcelain row with status ` M`, the plan
file, is not in this set, because AC28's porcelain clause ranges over untracked paths only.

## Dispositions

OUT-OF-WRITE-SET: NONE

Evaluated over `AC28-SCOPE-PATHS` minus `INHERITED-PATHS` minus the Write Set. Every remaining
path lies inside `docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/`, which
the Write Set admits as this feature's own folder artifacts, or is one of the nine Write Set source
files or the issue-812 `spec.md`.

MODIFIED-OUTSIDE-AC28-SCOPE: NONE

The only tracked file modified but neither committed nor untracked is
`docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/plan.2026-09-08T23-50.md`,
which is itself a Write Set path, so it is not listed here.

AC28-RESIDUAL-UNTRACKED: NONE

Every untracked addition in `AC28-SCOPE-PATHS` lies inside
`docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/`, which AC28's own text
admits on its own terms, so none is listed. No path was absorbed into a subtraction that AC28's
literal wording would not itself permit: the field ranges over both inherited clauses, and neither
clause was needed to reach this result.

Output Summary: 57 paths in AC28's scope, all inside the Write Set or this feature's own folder.
`OUT-OF-WRITE-SET: NONE`, `MODIFIED-OUTSIDE-AC28-SCOPE: NONE` and `AC28-RESIDUAL-UNTRACKED: NONE`.
The clause-A subtraction was not load-bearing and clause B subtracted nothing.
