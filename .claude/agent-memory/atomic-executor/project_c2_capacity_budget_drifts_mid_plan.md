---
name: c2-capacity-budget-drifts-mid-plan
description: A plan's per-file line budget measured at planning time is stale by the time later phases run; expect forced test relocations and pre-emptively check remaining headroom before writing each test
metadata:
  type: project
---

An atomic plan's file-capacity table (baseline lines + planned additions per file) is a projection made
before execution. Earlier phases routinely overspend their share, so by the time you reach the later
phases the planned home for a test group is already full and the plan's own per-file projections have
become unsatisfiable.

Observed on `qfc-item-controller-defects-484`: `QfcItemController.MailActionsTests.cs` had a 184-line C2
baseline with 294 planned additions (projected 478). Phases 1-3 actually spent 275 lines, so Phase 4
started at 459 with 41 lines of headroom against a 500-line ceiling and 104 more lines of planned
content. `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` likewise hit 507 after `[P5-T10]` and
had to be compacted back to 499.

**Why:** planning-time baselines are snapshots; the ceiling is per-file and hard, while the plan's
budget is global and advisory. A well-written plan anticipates this with an explicit capacity rule
(here C2 rule 3 authorizing relocation, rule 2 mandating compaction first).

**How to apply:**
- Before writing any test in a later phase, measure `grep -c '' <file>` for **all** owned test files and
  compute a global allocation for every remaining test, not just the current one. Do this once, up
  front, rather than discovering the shortfall at the last test.
- Format with the repo formatter after each insertion and re-measure; predicted line counts are wrong
  by 20-40% because the formatter's print width decides the wrapping.
- Compaction levers that actually pay: route repeated assertion calls through a one-line local function
  so each call fits the print width (16 `VerifyRemove` calls collapsed from ~64 lines to 17); extract
  shared reflection/arrange boilerplate into the support file; cut multi-line XML doc blocks to a single
  `/// <summary>...</summary>` line; drop arrange statements the assertion does not need.
- Comments you added yourself are the first thing to compact when a **production** file overruns; never
  touch pre-existing content to make room.
- When you relocate, add a header comment naming the issue number and record the relocation plus its
  arithmetic in the file-size evidence artifact. If the spec's prose names a projected per-file figure
  the relocation invalidates, do **not** edit the spec text - record the divergence in the cited
  evidence artifact and in the AC reconciliation. See [[418-500line-gate-vs-plan-content]].
