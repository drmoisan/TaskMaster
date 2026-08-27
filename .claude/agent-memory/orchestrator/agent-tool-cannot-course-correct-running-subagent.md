---
name: agent-tool-cannot-course-correct-running-subagent
description: Agent() with the same subagent_type starts a SECOND agent, it does not continue the running one; SendMessage is often absent from the orchestrator tool surface, so a mid-task correction is unrecallable
metadata:
  type: feedback
---

There is no way to course-correct a running subagent from the orchestrator unless `SendMessage` is
actually present in the tool surface. Calling `Agent()` again with the same `subagent_type` does NOT
continue the running agent — it launches a **second, context-free** one into the same worktree.

**Why:** The Agent tool's own description says to "use SendMessage with the agent's ID or name to
continue a previously spawned agent", which reads as though a continuation channel always exists. It
does not. On 2026-08-26, mid-run on epic child #442, the orchestrator found a real Phase 7 gate
hazard (untracked `.claude/state/` and `TestResults/` breaking the plan's clean-`git status`
acceptance) and tried to send the running `atomic-executor` a "MID-TASK COURSE CORRECTION". That call
started a duplicate executor against a live worktree — the exact interleaved-write failure recorded
in [[one-executor-per-worktree]] — and `SendMessage` was not in the function list, so it could not be
recalled or cancelled. There is no kill-agent tool either.

**How to apply:**

- Before delegating, front-load EVERYTHING the subagent needs. A long prompt is cheap; a duplicate
  executor is not. Anticipate the end-of-run gates (clean `git status`, ownership diffs, staging
  discipline) in the FIRST prompt rather than planning to correct later.
- Check the actual function list for `SendMessage` before assuming a correction is possible. If it is
  absent, treat every delegation as fire-and-forget.
- If a duplicate is launched anyway, the only remaining channel is a file the subagent is likely to
  read. The most reliable is `artifacts/orchestration/orchestrator-state.json`, which executors load
  for feature context. Add a prominent top-level stand-down key naming the authorized executor's
  start time and instructing any later-started duplicate to halt without touching a file. Extra
  top-level keys pass the MCP orchestrator-state validator (unlike extra keys inside
  `delegation_receipts`, which it rejects), so this costs nothing — but it is a mitigation, not a
  guarantee, because nothing forces the duplicate to read it.
- Do not try to signal through the plan file. Mutating an approved plan mid-execution risks
  confusing the legitimate executor and can break the plan validator.

See [[one-executor-per-worktree]] for why the interleaving is damaging, and
[[stale-checkpoint-is-not-a-dead-agent]] for the related trap of relaunching against a live worktree.
