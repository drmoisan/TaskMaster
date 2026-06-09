---
name: feedback-verify-flat-artifact-layout-after-executor
description: After an atomic-executor run, verify the feature folder kept its canonical flat artifact layout; executors have relocated committed cycle artifacts into subfolders.
metadata:
  type: feedback
---

After an `atomic-executor` execution returns, run `git status` and confirm no
previously-committed feature-folder artifacts were moved or deleted. In issue #181
cycle 4, the executor relocated already-committed cycle-1/cycle-2 reaudit artifacts
(`code-review.<ts>.md`, `feature-audit.<ts>.md`, `policy-audit.<ts>.md`,
`remediation-inputs/plan.<ts>.md`) into per-cycle subdirectories
(`<ts>-audit/`, `<ts>-remediation/`), showing as `D` (deleted) + untracked dirs —
an undirected, unreported side effect. The orchestrator reverted with
`git restore <deleted paths>` + `mv` for untracked files + `rm -rf` of the subdirs.

**Why:** The canonical convention (Remediation Loop Protocol "Required Artifacts Per
Cycle" and the schema's `audit_paths`) is FLAT files directly under the active feature
folder: `docs/features/active/<feature>/<artifact>.<ts>.md`. Subfolder grouping breaks
the documented paths and the checkpoint's recorded `audit_paths`, and scrambles the PR
diff.

**How to apply:** Add a "do not relocate/reorganize existing committed feature-folder
artifacts; keep the flat layout" guardrail to every remediation-inputs file (done for
cycle 5), and independently re-check `git status` for stray `*-audit/`/`*-remediation/`
directories before committing the cycle. Restore flat layout before the pre-push commit.
Related: [[remediation-loop-strict-handoff]].
