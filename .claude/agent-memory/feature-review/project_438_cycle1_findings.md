---
name: 438-review-findings
description: 'Issue #438 review outcomes: cycle 1 (R1 blocking branch-floor, R2 dispositioned) and cycle 2 (R1 resolved 4/4, PASS, 0 blocking; P2-T7 constant miss adjudicated variance)'
metadata:
  type: project
---

Issue #438 (quickfiler search keystroke focus steal), work mode full-bug, base main @ 003c5715.

Cycle 1 (@ ff9d14ab, artifacts *.2026-08-08T13-25.md): all 14 gating ACs PASS; blocking R1 = new file `BreadcrumbItemViewerLifecycleCoordinator.Search.cs` branch 50% (2/4, null-conditional arms untested); R2 = `QfcItemController.EventHandlers.cs` 78.65%/61.11% pre-existing floor miss, zero changed-line regression, dispositioned non-blocking.

Cycle 2 (@ 2134fa7b, artifacts *.2026-08-08T15-34.md): **PASS, 0 blocking**. R1 resolved by two additive test methods (345->382 lines, verified test-only, zero removed lines); target file 4/4 reproducibly. P2-T7 literal repo-wide constant miss (0.858620 vs 0.858665) adjudicated measurement variance — see [[csharp-coverage-constants-nondeterministic]]. Estimated-timestamp disclosure accepted as adequate (two committed correction notes; files not renamed). Residuals: R2 maintainer disposition outstanding; PR-context classifier defect STILL has no tracking issue (gh search confirmed absent, 2026-08-08); HV-1 post-merge. Follow-ups verified via gh: #509 OPEN, #511 OPEN, #510 CLOSED dup of OPEN #394. Note: gh WAS available in this session despite the pr_context summary claiming otherwise.

**How to apply:** if #438 comes back (e.g., HV-1 negative outcome or R2 disposition), cycle-2 artifacts are the authoritative record; production code at head is bit-identical to what cycle 1 reviewed. Re-apply the pr_context summary correction if artifacts are regenerated (see [[pr-context-summary-misclassifies-cs]]).
