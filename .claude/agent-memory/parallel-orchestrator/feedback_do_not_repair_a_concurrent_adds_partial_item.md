---
name: do-not-repair-a-concurrent-adds-partial-item
description: A stalled /parallel-add leaves an `admitted` item with no cohort, which fails checkpoint validation; leave it in place and report rather than deleting it or assigning a cohort
metadata:
  type: feedback
---

When checkpoint validation fails with `item <N> in state 'admitted' must appear in exactly one
current-generation cohort; found 0`, and `<N>` is an item you never launched, a `/parallel-add`
stalled mid-flight. Leave the record exactly as it is. Do not delete it, do not assign it a
cohort, and do not treat the validation failure as a defect in your own run.

**Why:** The add writes `items[]` at `admitted` BEFORE preparation returns a real blast radius, so
the record carries a `blast_radius_note` marking the radius a non-authoritative placeholder and
carries no `conflict_edges[]` entries. Assigning a cohort would be inventing an admission decision
from a placeholder radius; deleting the record would discard the preparation work already on disk
(research artifacts, an untracked active feature folder, a created branch). Neither is yours to
decide — admission and withdrawal are `/parallel-add` and `/parallel-remove` operations. The item
is also harmless to leave: with no cohort and no conflict edges it constrains no barrier, so
scheduling of the real items is unaffected.

**How to apply:**

- **Diagnose liveness before concluding it stalled.** Find the worktree holding the item's branch
  in `git worktree list --porcelain`. A preparation worktree with NO `locked` line and index/HEAD
  mtimes tens of minutes stale is abandoned — live claude agent worktrees carry
  `locked claude agent <id> (pid <n>)`. Cross-check `proposed_at` on the item record against the
  clock.
- **The gates do not run the schema validator.** The merge gate and both worktree-removal gates
  read only their own fields, so an invalid checkpoint still merges and still removes. A run can
  continue to completion of its own items with a foreign partial item present. Only the MCP
  `validate_orchestration_artifacts` call fails.
- **Re-read the checkpoint immediately before every write.** Your writes are whole-file
  read-modify-write, so a fresh read is what preserves the other operation's record; a checkpoint
  object cached from earlier in the session would silently drop it.
- **`parallel-add`, `parallel-remove`, and `parallel-close` skills DO exist in TaskMaster** under
  `.claude/skills/`, contrary to the "F6 not shipped" wording in the `parallel-orchestrate` skill.
  Check the directory before telling a user a mutation command is unavailable.

See [[parallel-run-execution-playbook]].
