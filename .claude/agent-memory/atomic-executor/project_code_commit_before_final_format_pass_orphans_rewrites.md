---
name: code-commit-before-final-format-pass-orphans-rewrites
description: A plan that commits the code files in Phase 2 and runs the repository-wide formatter only in Phase 3, then commits only the feature folder, leaves any formatter rewrite uncommitted and makes its own "no porcelain entry under the code trees" gates unsatisfiable
metadata:
  type: project
---

Seen at preflight of the issue #942 plan (2026-09-30). P2-T8 committed the four code files. P3-T1 then ran `dotnet tool run csharpier format .` and said "a non-zero rewritten count is not itself a restart trigger". P3-T30 committed only the feature folder. Two gates, P3-T14 and P3-T30, then asserted that no porcelain line names a path under TaskMaster/ or TaskMaster.Test/. Any rewrite of a Write Set file by the formatter, or a "repair" edit the plan authorised in P3-T2, therefore had no task that committed it, and both gates failed with no admissible remedy.

**Why:** the planner treated the format pass as a verification step. It is a write step that can change files the plan already committed.

**How to apply:** at preflight, find the last task that commits code paths and every later task that can write them: the formatter, fixing linters, and "repair and restart" branches. Require one of two shapes. Either format the Write Set before the code commit and make a later rewrite a fail-closed stop, or include the code paths in the final commit's pathspec. Related: [[project_midplan_commit_breaks_deletion_staging_and_porcelain_spans]], [[project_plan_checkoff_fixpoint_breaks_terminal_clean_tree_gate]].

Second lesson from the same review: a hunk-position gate expressed as a single lower bound ("every hunk starts at or above line N") does not prove that members declared after N were left untouched. Use bounded windows derived from single-line token positions.
