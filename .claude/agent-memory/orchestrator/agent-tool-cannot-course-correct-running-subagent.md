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

**The same gap applies to a FINISHED agent, and that case has a better answer (2026-09-01, #662).**
After `atomic-executor` completed, orchestrator verification found a mechanical defect in its output:
the hygiene sweep had left all six committed TRX files unparseable. The natural move is to resume the
agent that owns that evidence so it also corrects its own hygiene artifacts — but `SendMessage` was
again absent, and a fresh `Agent(atomic-executor)` would arrive with no context and re-derive a
52-task plan to fix an XML escape. For a small, mechanical, fully-specified repair of a finished
agent's output, do it yourself and record the finding in an evidence artifact naming the root cause.
Reserve re-delegation for work that needs the agent's judgment, not its hands.

**Concrete slip to avoid: reaching for `SendMessage` and hitting `Agent()`.** On that same run the
attempt to resume produced a real `Agent(subagent_type: "fork")` launch, because `Agent` is what is
actually in the tool surface. It was harmless only because the prompt was a placeholder that asked
for an acknowledgement and nothing else. Before composing any continuation, confirm `SendMessage` is
in the function list; if it is not, do not start composing a "resume" call at all.

**A "correction" prompt must be self-contained, because the correction IS a new agent (#735,
2026-09-03).** Mid-review I found the diff anchor I had given `feature-review` had gone stale and
sent a correction that opened with "this supersedes the DIFF ANCHOR section of my original brief" and
closed with "everything else in my original brief stands unchanged". Both phrases are incoherent to
the recipient: it was a *fresh* `feature-review` with no knowledge of the original brief, holding only
the correction text. It nonetheless produced a complete, high-quality review — but only because the
correction happened to restate the anchor, the artifact paths and the item worktree. Had the
correction been a terse "use anchor X instead", the new agent would have had no task at all.

Two follow-on facts worth having:

- **Read-only reviewers do not necessarily collide.** Both `feature-review` agents were briefed to
  write the same three artifact paths, which looked like a guaranteed clobber. They did not collide:
  each stamped its own timestamp (`09-05` and `06-19`), so two complete sets landed side by side. Do
  not assume a race; check the filenames before panicking, and note the timestamps are each agent's
  own clock and are not a sequence.
- **Two independent passes are worth keeping.** Rather than deleting one set, commit both and say in
  the message why two exist. The second pass, briefed with the corrected anchor, independently
  recomputed the callback resolution and coverage and surfaced a latent race the first pass missed.
  Convergent PASS from two differently-briefed reviewers is stronger evidence than either alone.

Before sending any correction: assume zero shared context, restate the worktree, the artifact paths,
the requirements sources and the task, and mark plainly which earlier instruction is being replaced.

See [[one-executor-per-worktree]] for why the interleaving is damaging, and
[[stale-checkpoint-is-not-a-dead-agent]] for the related trap of relaunching against a live worktree.
Anchor staleness itself is [[three-dot-diff-degenerates-on-ancestor-base]].
