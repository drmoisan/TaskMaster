---
name: preflight-recurring-csharp-plan-defect-classes
description: Four defect classes that recur in C# atomic plans at preflight - omitted .claude/rules/csharp.md policy read, exact-version SDK acceptance vs global.json rollForward, absolute Failed:0 over the solution suite, and a Select-String -LineNumber switch that does not exist
metadata:
  type: project
---

Four defect classes recur in TaskMaster C# atomic plans and are worth checking first at preflight.
Found together on the #476 WebView2 host/initializer plan (2026-08-24), each fixed in-place.

1. **Phase 0 omits `.claude/rules/csharp.md`.** The file exists in `.claude/rules/` and is step 4 of
   `policy-compliance-order` for any C# change set. Planners list CLAUDE.md +
   general-code-change + general-unit-test + quality-tiers + plan-acceptance-gates and stop.
   Check the read list against `ls .claude/rules/` every time, and fix the artifact's "lists all N
   paths" count when you insert it.

2. **An exact `dotnet --version` equality acceptance is unsatisfiable.** `global.json` pins
   `8.0.205` but sets `rollForward: latestFeature` and lists `$host$` in `paths`, so a correct
   environment can legitimately print a higher `8.0.x`. Gate on "an `8.0.` version resolved through
   global.json rather than the global.json `errorMessage`", not on equality.

3. **`Failed: 0` over the whole solution suite has no remediation path.** Pair it with the Phase 0
   baseline: require the Phase 0 test artifact to carry a `Failed Tests:` section, then allow only
   failures also present there and not owned by the feature. Without the baseline list the
   comparison clause is unusable. Keep `[P4-T7]`-style "EXIT_CODE 0 for each step" artifacts
   consistent with whatever carve-out you add, or they contradict each other.

4. **`Select-String` has no `-LineNumber` switch.** A plan that writes
   `Select-String -SimpleMatch ... -LineNumber` states a command that dies on parameter binding.
   The line number is the `LineNumber` property of the returned `MatchInfo`.

See also [[project_preflight_absolute_zero_gate_on_sibling_owned_assembly]] and
[[project_418_plan_rationale_clauses_are_evidence]] - class 3 is the same failure shape, and
classes 2 and 3 are both unmeasured world-state claims baked into an acceptance.
