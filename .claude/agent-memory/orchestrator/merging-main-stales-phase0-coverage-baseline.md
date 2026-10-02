---
name: merging-main-stales-phase0-coverage-baseline
description: After a mid-run merge of main, a Phase 0 local coverage baseline is stale; re-anchor no-regression checks CI-to-CI (main's push run at the merge base vs the PR run), with a negative control against the stale figure
metadata:
  type: project
---

On item 927 (2026-09-30) the local P6-T9 C# run read 85.92/80.08 against the Phase 0 baseline 85.93/80.09 and AC13 was recorded NOT MET, although the branch changed no production C#. Main's own CI push run at the new merge base printed 85.92/80.08 with identical denominators: sibling merges had moved main, not the branch. The same staleness hit the Pester baseline (320 at Phase 0 vs 342 on main CI).

**Why:** a plan that merges main after Phase 0 (required for an up-to-date head) moves the merge base, so every Phase 0 baseline, figure and merge-base equality check describes a tree the branch no longer sits on.

**How to apply:** when a plan will merge main before its final gates, have the planner source no-regression baselines from main's CI push run at MERGE-BASE-NOW (the mstest job log prints "First-party coverage:"; the Pester job prints PESTER/COVERAGE lines), compare against the PR head's CI run, add a MAIN-MOVED guard (pull_request CI tests the merge with main's tip), and keep the stale Phase 0 comparison as the negative control. Anchor diff gates at origin/main three-dot HEAD. Related: [[merging-main-invalidates-plan-base-anchor]], [[coverage-lines-covered-is-nondeterministic]].

Polling note: detect a subagent's end by the JSON field `"hookEvent":"SubagentStop"` in the transcript tail, not a bare substring, which a tool result quoting hook text also matches.
