---
name: gitignore-tracking-expands-diff-scope
description: When a feature un-ignores a directory, the whole subtree appears as added files in the branch-vs-base diff and falls within audit scope.
metadata:
  type: project
---

Issue #166 changed `.gitignore` to stop ignoring `.claude/`, which causes the entire `.claude/` subtree (agents, hooks, rules, skills, settings.json) to materialize as added (status `A`) files in the branch-vs-base diff. The feature's own issue.md and plan.md asserted scope was "the single production file `.gitignore`" and "no `.claude/` files edited."

**Why:** The orchestrator pre-review `git add -A` step commits the now-tracked content, so even though the plan only edited `.gitignore`, the branch diff against the merge-base includes 70 newly-tracked files including 17 PowerShell `.ps1` hooks.

**How to apply:** For feature reviews, derive scope from the branch diff against the resolved merge-base, never from the plan's or issue's self-described scope. A `.gitignore` un-ignore is a scope-narrowing trap: the plan says "one file" but the diff carries an entire toolchain. PowerShell/Python/etc. coverage obligations attach to those newly-tracked files. See [[powershell-coverage-mandatory-when-ps1-in-diff]].
