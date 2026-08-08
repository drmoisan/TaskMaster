---
name: conditional-split-three-task-shape
description: The sanctioned decomposition for a "file may exceed 500 lines" contingency in an atomic plan is three tasks (measure / split / register), never one bundled task
metadata:
  type: project
---

A plan task that handles a conditional file split must be decomposed into exactly three tasks:

1. **Measure** — pure measurement; emits a verdict line `SPLIT REQUIRED: <file>` (one per offender) or the
   single line `SPLIT NOT REQUIRED`. States explicitly that it changes no `.cs` and no `.csproj` file.
2. **Split** — performs the split only. Carries an explicitly-authorized `NO ACTION` branch keyed off the
   measure task's verdict, recorded back into the measure task's artifact.
3. **Register** — adds the `<Compile Include>` entry to the owning project only. Same authorized `NO ACTION`
   branch. If it fires, the QC loop restarts at the first format task.

**Why:** in this repo's legacy non-SDK projects (`QuickFiler.csproj`, `QuickFiler.Test.csproj`) an
unregistered `.cs` file silently does not compile and its tests never run, so registration must be its own
auditable task. A single task that bundles measurement + creation + registration is a preflight-blocking
defect; it was raised and removed twice on the #436 plan (once at `[P2-T51]`/`[P3-T48]`/`[P6-T41]`/`[P8-T56]`,
then reintroduced at the post-format gate `[P12-T3]` and removed again).

**How to apply:** when validating any plan that measures file size against the 500-line ceiling, check that
each measurement task has a dedicated split successor and a dedicated csproj successor. Also check the
plan's own decision record enumerates every conditional split branch. A production-file companion needs
its parent class to be `partial` — if the parent is not already partial, the split task should say so
explicitly (see the `EfcDataModel.Seams.cs` contingency, which does).

Related: [[project_418_500line_gate_vs_plan_content]], [[project_preflight_fix_tasks_inherit_decomposition_rules]],
[[project_plan_task_ids_digit_only_forces_renumbering]]
