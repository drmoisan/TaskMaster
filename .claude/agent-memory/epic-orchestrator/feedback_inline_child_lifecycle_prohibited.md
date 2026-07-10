---
name: inline-child-lifecycle-prohibited
description: Maintainer directive — epic-orchestrator must NOT run a child feature's lifecycle inline; on a genuine Agent(orchestrator)/Agent(pr-author) spawn failure, record delegation_failures[] verbatim and stop blocked
metadata:
  type: feedback
---

On the winforms-testability-refactor epic (#295), the maintainer issued a superseding directive: epic-orchestrator must delegate every child feature's lifecycle through `Agent(orchestrator)` and the final integration→main PR through `Agent(pr-author)`. Running a child's lifecycle inline (epic-orchestrator itself acting as the orchestrate driver) is PROHIBITED.

**Rule:** Attempt the spawn. If it genuinely fails, capture the VERBATIM error text into the epic checkpoint under a top-level `delegation_failures[]` entry and STOP in a recorded blocked state for maintainer review. Do not fall back to inline execution.

**Why:** A prior session ran children as an abbreviated inline executor-driver flow and fanned children in via direct `git merge --no-ff` (no child PR); the maintainer REJECTED both deviations and unwound the direct merges, re-landing via real child PRs. The maintainer also rejected the prior session's unsubstantiated "orchestrator not registered" claim and required a real spawn attempt with verbatim-error capture before any blocked stop.

**How to apply:** Before spawning a child orchestrator to adopt an in-flight worktree, first confirm the worktree has settled (no competing writes). Then attempt exactly one `Agent(orchestrator, model=<routing receipt model>)` spawn directed to operate in the existing worktree. On failure, record `delegation_failures[]` (attempted_at, delegate, subagent_type, purpose, verbatim_error, context) and set a `blocked` block + `next_step` blocked marker in the checkpoint; report immediately with the verbatim failure. See [[orchestrator-subagent-not-registered]] for the observed runtime failure mode.
