---
name: preflight-cited-tree-gate-and-span-enumeration
description: Round-2 preflight lessons from #959. A cited-tree diff gate that includes a file main later changes (e.g. .gitignore) stops a branch that merged main; stop-string conventions need a detecting task; compile-red span fixes must enumerate every span.
metadata:
  type: project
---

Three round-1 fixes in #959 (plan.2026-10-02T05-07.md, rev 1.1) were applied but left gaps that only a sibling re-check found:

1. A `CITED-TREE-EXIT` diff (base SHA vs MERGE-BASE over a PATHS-CITED list) included `.gitignore`. origin/main changed `.gitignore` after the branch point (#961 merged `*.rptproj.bak`/`*.csproj.bak` into `*.bak`), so a branch brought up to date with main stops at P0-T3. That contradicts the same revision's rule that a branch already containing main remains valid.
   **Why:** files cited only by content, and whose effect other gates already observe (porcelain/footprint), do not need line-identity protection.
   **How to apply:** in preflight, run `git diff --name-only <branch-point> origin/main` and intersect the result with every cited-tree pathspec list.

2. A new `ANCHOR MOVED` stop string had no task that detected it. A mid-run merge of main would surface only as an unrelated footprint failure.
   **How to apply:** for every stop string a delta adds, find the task whose observation produces it.

3. The compile-red span fix declared the P4 and P5 spans but missed P2 (the test file is registered at P2-T2 and calls a seam that P2-T3 lands). For every test listing that calls a member which does not exist yet, find the task that registers the file and the task that first builds the project green.

Related: [[project_preflight_recurring_csharp_plan_defect_classes]], [[project_preflight_moving_base_two_dot_diff_inertness_test]]
