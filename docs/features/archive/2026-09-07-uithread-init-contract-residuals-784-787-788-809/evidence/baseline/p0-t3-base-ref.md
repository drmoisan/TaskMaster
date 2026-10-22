# [P0-T3] Base ref anchor

Timestamp: 2026-09-08T00-15

Command: `git merge-base origin/main HEAD`; `git tag pre-809-base <sha>`; `git rev-parse pre-809-base`; `git rev-parse --verify pre-809-base`; `git diff --name-only pre-809-base HEAD -- <nine Write Set paths>`; `git status --porcelain --untracked-files=all`

EXIT_CODE: 0

BASE_TAG: pre-809-base
BASE_SHA: 04a54e681bd21e841e124c016df30672ee701b75

`git rev-parse --verify pre-809-base` exited 0. `git rev-parse pre-809-base` printed the 40-character sha above, which is identical to the `git merge-base origin/main HEAD` output.

INHERITED_WRITE_SET_PATHS:
```
```

The `git diff --name-only pre-809-base HEAD` span over the nine pre-existing Write Set paths returned no lines. Those nine paths are the twelve-path Write Set less the three files this delivery creates (`UtilitiesCS/Threading/IUiCaptureSource.cs`, `UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs`, `UtilitiesCS.Test/TestHelpers/UiThreadStateScope.cs`). The empty result establishes that no Write Set file changed between the base and `HEAD`, so the "exactly N changed lines" gates in [P1-T2], [P1-T3], [P2-T7] and [P6-T2] isolate this delivery's edits.

BASELINE_WORKTREE_STATUS:
```
 M docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/plan.2026-09-07T20-14.md
?? docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/evidence/baseline/p0-t2-requirements-read.md
?? docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/evidence/baseline/phase0-instructions-read.md
```

No line of that porcelain output names a path under `UtilitiesCS/`, `UtilitiesCS.Test/` or `QuickFiler.Test/`. The three lines present are this plan's own check-off marks for [P0-T1] and [P0-T2] and the two Phase 0 evidence artifacts those tasks wrote; none is a source edit. The executor therefore starts from a worktree carrying no uncommitted or untracked source change of its own, which is what makes the later per-file "exactly N changed lines" gates attributable to this delivery.

The porcelain span is a required companion to the name-listing diff above rather than a duplicate of it: an anchored `--name-only` diff enumerates tracked changes only and is blind to an untracked file.

Output Summary: `pre-809-base` created at `04a54e681bd21e841e124c016df30672ee701b75`, verified resolvable. Inherited Write Set diff is empty (0 paths). Baseline worktree carries no source-path modification.
