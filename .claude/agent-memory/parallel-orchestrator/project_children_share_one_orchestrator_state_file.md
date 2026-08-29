---
name: children-share-one-orchestrator-state-file
description: Every item child on a parallel run writes the same artifacts/orchestration/orchestrator-state.json, so a finishing child's late write lands in the next child's live checkpoint
metadata:
  type: project
---

Item children on a parallel run do NOT get per-item checkpoints. They all write
`artifacts/orchestration/orchestrator-state.json` in the session root. Because the run is serial
under a conflict-dense cohort table, child N is still finishing bookkeeping when child N+1 has
already claimed that file, so child N's late write mutates child N+1's live checkpoint.

**Why:** Observed on run bugs-638-644-647 on 2026-08-29. The 638 child injected three of its own
keys into 644's checkpoint after 644 had taken it over, then over-corrected the repair by stripping
nine keys that were actually 644's own empty `pr_gate`/`ci_gate` stubs and its `head_sha`. It
restored them and archived its own final record as `orchestrator-state.638.json`. The damage was
self-reported; nothing in the parallel surface detects it, and a silently corrupted child checkpoint
can fail that child's own completion gate long after the fact.

**How to apply:**

- **Verify the claim rather than accepting it.** After any child reports touching a sibling's
  checkpoint, read the live file and confirm the identity fields belong to the CURRENT in-flight
  item: `issue-num`, `feature-folder`, `branch_name`, `plan-path`, and
  `parallel_context.item_key` / `cohort_index`. Note the identity keys are hyphenated
  (`issue-num`, `feature-folder`, `plan-path`), so a probe for `issue_number` returns `undefined`
  and proves nothing.
- **Grep by substring, not by key name.** A child reporting "zero <N> keys remaining" has checked
  key NAMES only. Walk the whole object for the digit string. Expect two legitimate hits for a
  prior item's number — `parallel_context.parallel_slug` carries every item's number in the run
  slug, and a `local_execution_overrides[].rationale` may cite the prior item's merge commit.
- **Read-only while the child is live.** Do not repair the child checkpoint yourself mid-run; you
  would be racing the child that owns it. Report instead.
- Your own `parallel-orchestrator-state.json` is a separate file and was not affected.

See [[parallel-run-execution-playbook]] and
[[do-not-repair-a-concurrent-adds-partial-item]] for the other concurrent-write hazard on this
surface.
