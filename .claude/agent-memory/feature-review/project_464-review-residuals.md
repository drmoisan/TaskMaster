---
name: 464-review-residuals
description: efc-controller-surface-defects-464 review PASS/0 blocking; residuals (7 promotions owed incl. RC7 EfcSelectionGuard arity, canonical C# artifact absent); shared info/exclude trap when mirroring is forbidden
metadata:
  type: project
---

2026-08-28 review of `bug/efc-controller-surface-defects-464` (epic quickfiler-bug-family, closes #459-#467): **PASS, 0 blocking, 6 non-blocking**; 74/74 spec.md ACs substantiated (72 direct, 2 via committed non-vacuous build evidence). Artifacts committed on the feature branch at `7c5303ce`.

Residuals owed at fan-in:
- **Seven follow-up promotions NOT created** (`evidence/other/followup-promotions.md`): executor lacked the promotion MCP tool and scope gates barred potential docs. Item 7 is the RC7 residual: `EfcSelectionGuard.BannerPrefix` = `"==="` (third arity variant vs producers' `"===="`) + stale comment at `EfcFormController.cs:318-320`. Orchestrator must promote all seven or they die at merge.
- **Canonical `artifacts/csharp/coverage.xml` never emitted; both raw Cobertura XMLs deleted** after verbatim root-element transcription. Accepted non-blocking because quotients recompute exactly (54667/64124=0.852520, 13001/16418=0.791875) and test-count deltas reconcile; fan-in should regenerate.
- Post-464 aggregate figures: **85.25 line / 79.19 branch**, but lines-valid swung 82070→64124 (−17,946) between baseline and final same-command runs — instrument denominator instability again; never gate on cross-run deltas here.
- New-code 90% floor FAIL non-blocking: 6/11 measured members sub-90 (all COM/dialog-bound, incl. relocated pre-existing create body at 18%); new members in `EfcItemController`/`EfcViewer` unmeasured under pre-existing class-level `[ExcludeFromCodeCoverage]`.
- `EfcFormControllerTests.cs` at 485/500 lines; `QfcItemController.ViewerSetup.cs` at 499/500.

**Operational lesson — mirror-vs-exclude trap.** When the caller forbids session-cwd mirrors ("siblings will commit them") but the SubagentStop hook Test-Paths from the session cwd: `git rev-parse --git-path info/exclude` resolves to the COMMON `.git/info/exclude`, shared by ALL worktrees — there is no per-worktree exclude. Excluding the feature folder there protects the session mirrors but also hides the review worktree's *untracked* artifacts from the orchestrator. Resolution that satisfies both: add the exclude for the mirrors AND `git add -f` + commit the artifacts on the feature branch in the review worktree (established epic pattern), making them tracked and exclusion-immune. Related: [[review-worktree-differs-from-session-cwd-mirror-artifacts]]. Cleanup owed: the mirror copies + the `docs/features/active/efc-controller-surface-defects-464/` line in `C:/Users/DanMoisan/repos/TaskMaster/.git/info/exclude` should be removed after the feature merges.
