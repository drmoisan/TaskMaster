---
name: decomposition-must-cover-newly-inserted-tasks
description: When a preflight revision decomposes bundled measure/split/register tasks, sweep the WHOLE plan for that shape — including tasks the same revision just inserted — or the next preflight rejects it
metadata:
  type: feedback
---

When a preflight finding orders decomposition of a bundled task shape (e.g. "measure + split if over limit +
register `<Compile Include>`" in one task), apply the decomposition to **every** instance of that shape in the
plan, including any task the *same revision* introduces. Then re-read the governing Decision record and confirm
its enumerated list of instances now includes the new one.

**Why:** In #436 (`plan.2026-08-07T20-42.md`), revision 1 satisfied finding R5 by decomposing `[P2-T51]`,
`[P3-T48]`, `[P6-T41]` and `[P8-T56]` — but the *same* revision inserted a new post-format size gate
`[P12-T3]` carrying exactly the bundled shape R5 had just removed. It also contradicted the plan's own D-14
("every file-creating contingency branch carries a dedicated `<Compile Include>` task"), whose enumeration
listed the four decomposed tasks but not the new one. That cost a full extra preflight round trip.

**How to apply:**
- After writing a revision, grep the plan for the offending shape one more time; a Decision record that
  *enumerates* instances is the checklist — every enumerated list must be re-derived, not carried forward.
- A missing `<Compile Include>` in these legacy non-SDK projects (`QuickFiler.csproj`,
  `QuickFiler.Test.csproj`) fails **silently**; downstream gates that record a numeric passed count without
  asserting an expected total will not catch the dropped tests. That is why the registration task must be
  separately checkable, never folded into a measurement.
- A final-QC size gate spans **production** files too, so its registration task may target the production
  `.csproj` — unlike the Phase-2/3/6/8 test-only splits. Say so explicitly in the Decision record.
- Renumbering after a mid-phase insertion is mandatory (see [[plan-validator-task-id-sequential-constraint]]);
  sweep every cross-reference, the phase headers on both sides, and the Open Questions section.
