---
name: midrun-main-merge-reanchor-scope-phrasing
description: Preflight of an execution-time revision that merges origin/main mid-plan and re-anchors some gates - check the scoping words of the substitution and every restart-basis restatement elsewhere in the plan
metadata:
  type: project
---

When an orchestrator revises a plan mid-execution (a coordinator-approved restart plus a merge of origin/main), two defect shapes recur (#944 revision round 3, 2026-09-30):

1. The substitution paragraph says "In pass 2, P3-Tx and P3-Ty substitute MAIN-MERGE-SHA" although those tasks run once after the restarted loop and are not part of pass 2. Read literally, the substitution never applies, and the un-substituted anchored diff reports the merged sibling's files (FOOTPRINT OUTSIDE, raw .xml projections). The pathspec justification for the tasks that keep the old anchor also tends to omit pathspecs, such as the runsettings files in a protected-files diff.
2. The restart-basis extension is added to the task's re-run rule only, while the Execution conventions restart rule still names the original basis ("for the issue 780 sporadic failure"). This is the same class as [[supersede-residual]].

**Why:** revisions made during execution are written against the task text and skip the conventions block and the task-order semantics.

**How to apply:** verify the pathspec claim with `git diff --stat OLD-ANCHOR MAIN-MERGE -- . ":(exclude)docs" ":(exclude).claude"` rather than the revision's own subset. Grep the plan for every restatement of the restart basis, and check that the scoping words ("in pass 2", "after the loop") match when each named task actually runs.
