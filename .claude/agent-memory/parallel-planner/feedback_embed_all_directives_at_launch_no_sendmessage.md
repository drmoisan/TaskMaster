---
name: embed-all-directives-at-launch-no-sendmessage
description: SendMessage can be disabled for the planner session, so a running preparation child cannot receive mid-flight directives; put phase-boundary commits and credential-rotation retry guidance in every prompt from the start
metadata:
  type: feedback
---

**Rule: every standing directive a preparation child might need must be in its launch prompt.**
Do not plan on amending a running child's instructions.

**Why:** on run `bugs-2026-09-28` (2026-09-28) the coordinator issued a quota ruling mid-wave
(raise max_concurrency to 6, commit at every phase boundary, retry through credential rotation).
The three children already running could not be told: `SendMessage` returned "disabled for this
session, in subagents as well as here". Only the children launched after the ruling carried it.

**How to apply:** include these two lines in every preparation prompt by default:

- commit working documents at every phase boundary and push each as a plain fast-forward;
- a brief auth or rate-limit error around a credential switch is retried, not a reason to abort.

If a directive arrives after launch anyway, record the undelivered set in the checkpoint and be
ready to recover uncommitted work from the affected worktrees. See
[[unchanged-ref-does-not-prove-a-dead-child]] and [[default-to-open-mode-for-parallel-runs]].
