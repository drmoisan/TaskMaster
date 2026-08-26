---
name: 446-review-residuals
description: "#446 (quickfiler-bug-family epic child) PASS/0 blocking 2026-08-26; AC28 whole-type-vs-AC18 spec contradiction owed a maintainer amendment; issue.md 'Also closes #427' hazard; Actions.cs carve-out truth"
metadata:
  type: project
---

Review of `bug/quickfiler-bug-family-446` (61edc19b...fd746f55) closed 2026-08-26T11-29 with 0 blocking findings; fit to merge to `epic/quickfiler-bug-family-integration`.

**Why:** the epic-close review and sibling-child reviews will re-encounter these items and should not re-adjudicate from scratch.

**How to apply / residuals owed at epic close:**
- AC28 spec self-contradiction CONFIRMED by arithmetic: whole-type >=90% on `QfcFormController`/`QfcHomeController` peaks at 71.0% even with 100% owned-file coverage ((290+213)/708 and (259+60)/449); the rest lives in five sibling-owned partials AC18 forbids touching. Maintainer spec amendment owed; blocking gate was the plan's (D-Plan-7) changed-file scope, which passed (97.39 / 47.89-carve-out / 100.00). The AC-supersession-via-plan-provision pattern ([[449-review-residuals]], [[484-review-residuals]]) applied again.
- `issue.md:5` reads `Also closes: #426, #427, #448` — superseded by D1; #427 must stay open (only 427-A delivered). P4-T17 evidence records the PR-body constraint; verify the eventual PR body carries closing keywords for #446/#448/#426 only.
- `QfcFormController.Actions.cs` carve-out truth: MessageBox is NOT the binding constraint — uncovered set is dominated by COM-bound `LoadItems*` overloads (lines 29-160) plus `ProcessUndoItemAsync`; a MessageBox seam alone reaches only ~67%. Seam-uplift follow-up was NOT routed to any promoted potential doc (CR-1) — check it got promoted.
- Dead `using System.Diagnostics;` at `Actions.cs:4` (CR-3), remove on next authorized touch.
- Post-446 same-session Cobertura roots: line 0.848402, branch 0.787469 (baseline 0.847782/0.786876). Raw repo-wide line is below the 85 floor (vendor-inflated denominator) — recorded FAIL/non-blocking per the [[csharp-repowide-coverage-below-80]] pattern.
- Hook simulation passed from both cwds after mirroring the 3 artifacts into the session cwd feature folder ([[review-worktree-differs-from-session-cwd-mirror-artifacts]] pattern worked verbatim).
