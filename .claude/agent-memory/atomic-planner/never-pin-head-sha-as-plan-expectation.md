---
name: never-pin-head-sha-as-plan-expectation
description: Never write "expected HEAD `<sha>`" into a plan task — the plan's own commits invalidate it; record the HEAD as an observation and gate on tree invariants instead
metadata:
  type: feedback
---

A plan task must never assert an expected HEAD SHA. Have it **record** `git rev-parse HEAD` as an observation and gate on the invariants that actually matter:

- `git status --porcelain` is empty;
- `git diff --stat HEAD -- <prior plan files>` is empty, with those files stated read-only;
- `git diff --name-only <baseline-capture-sha> HEAD` contains no path ending `.cs`, `.csproj`, `packages.config`, or `app.config` — i.e. the source and build-configuration tree is identical to the tree the reused baseline evidence was captured against.

The one SHA that may appear is the **immutable commit the baseline evidence series was captured in**, used as a diff basis. That value never changes.

**Why:** In #418 cycle 2 the orchestrator gave a false clean-tree premise, then fixed it by committing the carried-in state and asking me to update the plan's "expected HEAD" to the new SHA. Committing that very edit moved HEAD again, so the expectation was stale one commit after being corrected. Chasing the SHA forward cannot converge: any commit touching the plan file invalidates the plan's own precondition. The invariant formulation is stable across any number of documentation or agent-memory commits and fails exactly when it should — when someone touches source between the baseline capture and the cycle.

**How to apply:** Applies to any plan that reuses a prior evidence series instead of re-capturing a full baseline (see [[evidence-path-normalization]] for where those artifacts live). Write the reuse argument in Design Decision form as "reuse holds for any HEAD whose source and build-configuration tree is identical to `<baseline-sha>`'s", never as "the current HEAD is `<sha>`". Also sweep downstream tasks that cite the reuse argument — a generalized Decision plus a still-SHA-pinned task reintroduces the rot. Keep clean-tree gates strict rather than enumerating a permitted-dirt set: a permitted-dirt list bakes one session's hygiene lapse into a standing condition and would direct the executor to revert another agent's files. Halt-and-report is the only correct response to unexpected dirt.
