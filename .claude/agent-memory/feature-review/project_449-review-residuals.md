---
name: 449-review-residuals
description: '#449 QfcExplorerController review: PASS/0 blocking; residuals: untracked #584 promotion doc needs non-child commit route; unused usings in base test file; AC-12/AC-16 supersession pattern validated'
metadata:
  type: project
---

#449 (epic quickfiler-suite-determinism-foundation, wave 0) reviewed 2026-08-22: PASS, 0 blocking, all 16 spec ACs verified.

**Why:** three residuals survive the merge and matter at epic close or later reviews.

**How to apply:**
- `docs/features/potential/promoted/2026-08-22-uithread-dispatcher-null-race-progresstrackerasync.md` (Issue #584, the ProgressTrackerAsync STA flake) sits UNTRACKED in the 449 worktree — the epic forbids children committing under `docs/features/potential/**`, so verify at epic close that it reached the repo via a non-child route or that issue #584's body is accepted as the durable record.
- `QuickFiler.Test/Controllers/QfcExplorerControllerTests.cs:1-2` carries two unused usings (`System.Collections`, `System.Collections.Generic`) stranded by the 500-line-cap partial-class split; fix opportunistically on next touch.
- Validated pattern (reusable): when a plan's own split provision ([P6-Tn]) fires and supersedes an AC's literal figure ("exactly one appended line" -> two; 485 -> 486), the AC stays PASS if the supersession is recorded in a dedicated evidence artifact and carried into the check-off notes — do not raise the numeric divergence alone as a defect (cf. [[441-review-residuals-and-494-handoff]]).
- Coverage adjudication precedent: with `artifacts/csharp/coverage.xml` deliberately absent and no pr_context summary, verifying from the executor's raw `coverage/*.cobertura.xml` on disk (recompute root/package/per-file figures with a scratch parser) satisfies the evidence-verification model; a class suppressed by a class-level `[ExcludeFromCodeCoverage]` at baseline is "absent from the report", not 0% — a per-file grep for matching `<class>` elements (0 at baseline, 4 after) proves it.
