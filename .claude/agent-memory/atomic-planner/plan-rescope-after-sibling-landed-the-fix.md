---
name: plan-rescope-after-sibling-landed-the-fix
description: Re-scoping a cleared plan after a separate workstream landed one of its issues — split the file's contiguous TAIL so upstream citations survive, and hunt for the seam the landed work now supplies that the plan planned to invent
metadata:
  type: feedback
---

When a sibling workstream lands one of a multi-issue plan's issues and the plan must be re-scoped in place, three moves are worth more than a citation sweep.

1. **Split the over-limit file by its CONTIGUOUS TAIL, and say so in the plan.** A mandatory partial-class split normally invalidates every line citation in the rest of the plan, forcing either a re-measure pass after the split or a citation-free plan. Relocating a contiguous tail instead — the last N private members, in order, to a `Foo.Selection.cs` — leaves every member above the cut at its current line, so Phases 2..N can keep pre-split citations. Add a short "Citation Basis" section stating the cut line, and add a task that VERIFIES the surviving members still open where the plan says they do, so the assumption is gated rather than assumed.
2. **The landed work usually supplies the seam the stale plan planned to invent.** Grep the landed diff's types for the concept the research declared absent before writing any "add a new field" task. #498's plan said "add a selected-segment index field"; the landed PR had already added `ActiveSegmentIndex` / `ActivateSegment` / `ActiveSegmentKey` / `GetActiveChild`. Read the new member's GUARD too — `ActivateSegment` refuses the leaf index, so it cannot express a downward transition and the plan has to name a descent mechanism explicitly.
3. **A retired AC still needs one task, and it is not a re-implementation.** An inherited-and-verified criterion maps to no implementing task, but the plan should still run the landed regression class and assert its test file is absent from the diff. That converts "we inherited it" from a claim into evidence, and it catches the case where this feature's own change breaks the inherited fix.

**Why:** #498 re-scope, 2026-08-25. PR #605 fixed #439 independently after the plan had cleared preflight and fanned in. Six tasks would have re-fixed landed work; one of them would have re-derived `SelectedFolderPath` from a retained presented-text map that the landed `SelectRow` already derives from `row.FilingTarget`.

**How to apply:** when a directive says a sibling landed part of the scope, treat the SPEC as the authority and the research as a dated artifact, then measure the landed files yourself — an orchestrator's supplied line numbers can be off by one where a `/// <inheritdoc />` line precedes the signature. Related: [[csharp-pure-move-extraction-pattern]], [[project_501_r3_preflight_seams]], [[never-pin-head-sha-as-plan-expectation]], [[project_484_qfc_revision_seams]].
