---
name: 444-review-residuals
description: "#444/#472/#482 epic-child review 2026-08-27: PASS/0 blocking; 3 ACs deferred-pending-PR-body (472-10, 482-11, 482-12); residuals OB-1 merge-up vs #493 fan-in, OB-3 NavigationTests at 498 lines, OB-5 p5-t25 deferral wording"
metadata:
  type: project
---

Review of `bug/quickfiler-keyboard-action-defects-444` vs `epic/quickfiler-bug-family-integration`
(2026-08-27, artifacts at timestamp 2026-08-27T20-34): 0 Blocking, 54/57 AC PASS, 3 DEFERRED
pending the integration PR body (472-10 record #644 in PR body; 482-11 widening statement; 482-12
trigger/severity correction). Issue #644 verified OPEN. Coverage independently re-parsed from
gitignored `coverage/coverage.cobertura.{baseline,final}.xml` still present in the executor
worktree — figures matched committed evidence exactly (85.13/79.21 vs 85.04/79.12).

**Why:** residuals to re-check at epic close.
**How to apply:**
- OB-1: branch was 10 commits behind the epic tip at review (the #493 fan-in, merge `13a22ade`);
  a final merge-up is owed before the integration PR merge. Recompute merge-base yourself — the
  caller's "0 behind" was stale (see [[stale-caller-merge-base]], [[epic-child-twodot-diff-divergence-noise]]).
- OB-3: `QuickFiler.Test/Controllers/QfcItemController.NavigationTests.cs` is at 498/500 lines;
  the next feature touching it must split it.
- OB-5 pattern: [P5-T25]-style deferral artifacts can carry a DEFERRED line whose wording
  contradicts the artifact's own summary (said promotion was out of scope when it was completed
  on-branch). Read the whole artifact, not the marker line.
- Executor worktrees under `TaskMaster/.claude/worktrees/` keep the raw Cobertura XMLs (gitignored)
  even after the deleted-raw-XML narrative; parse them for independent verification instead of
  trusting extracted figures ([[verify-zero-own-effect-coverage-noise-491]]).
- Hook vs caller conflict: caller said "fresh timestamp per artifact" but the hook requires all
  three artifacts to share one timestamp; write fast, then rename+sed all three to one final fresh
  value. Simulated PASS from both cwds.
