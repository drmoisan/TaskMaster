---
name: feedback-verify-flat-artifact-layout-after-executor
description: Honor the user's per-cycle folder layout for issue-#181 cycle artifacts; only revert artifact relocations that are UNDIRECTED agent side effects, not the user's own committed reorganization.
metadata:
  type: feedback
---

After an `atomic-executor` run, run `git status` and confirm no previously-committed
feature-folder artifacts were moved/deleted WITHOUT direction. In issue #181 cycle 4 an
executor relocated committed cycle artifacts into `<ts>-audit/`/`<ts>-remediation/`
subdirs as an undirected, unreported side effect; the orchestrator correctly reverted
that to the then-flat layout.

**UPDATE (2026-06-09, supersedes the original flat-only rule):** On the #181 branch the
USER deliberately adopted and committed (commit `a5fcb3fb`, "organized remediation cycles
into folders") a per-cycle FOLDER layout:
- each cycle's inputs + plan -> `<entry-ts>-remediation/`
- each cycle's three reaudit artifacts -> `<exit-ts>-audit/`
- the `evidence/` tree stays as-is (not foldered per cycle).
The user also committed their StackGeek WIP (`642c2851`), so it is no longer
"modified-but-unstaged" to preserve.

**Why:** The distinction is DIRECTION, not the layout itself. An agent reorganizing
committed artifacts on its own is a defect to revert. The user choosing a layout and
committing it is the convention to follow. Fighting the user's committed reorg would be
churn and would scramble their history.

**How to apply:** For #181 (and any branch where the user has committed a folder layout),
place new cycle artifacts in `<entry-ts>-remediation/` and `<exit-ts>-audit/`, set the
checkpoint `inputs_path`/`plan_path`/`audit_paths` to those foldered paths, and instruct
feature-review to write into `<exit-ts>-audit/`. Do NOT restore a flat layout or relocate
the user's existing cycle folders. Still verify (via `git status`) that no committed
artifacts were moved by an agent WITHOUT direction, and never `git add -A` when unrelated
user WIP is uncommitted. Related: [[remediation-loop-strict-handoff]].
