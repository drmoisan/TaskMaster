---
name: never-pin-head-sha-as-plan-expectation
description: Never write "expected HEAD `<sha>`" or "porcelain is empty" into a plan task — record the HEAD as an observation and gate on production/build-input paths only
metadata:
  type: feedback
---

A plan task must never assert an expected HEAD SHA. Have it **record** `git rev-parse HEAD` as an observation and gate on the invariants that actually matter:

- `git status --porcelain` reports **no production or build-input path** — nothing under the touched source projects, and no `*.cs`, `*.csproj`, `packages.config`, or `app.config` anywhere. **Never assert the porcelain output is empty.** At the moment a Phase 0 task runs, the feature folder itself (`spec.md`, `user-story.md`, `research/`, and the plan being executed) is untracked, as are agent-memory edits and promoted-defect documents. An empty-porcelain gate is therefore unsatisfiable by construction and gives the executor no authorized recovery. Have the task record the full porcelain output verbatim so the exclusion stays auditable;
- `git diff --stat HEAD -- <prior plan files>` is empty, with those files stated read-only;
- `git diff --name-only <baseline-capture-sha> HEAD` contains no path ending `.cs`, `.csproj`, `packages.config`, or `app.config` — i.e. the source and build-configuration tree is identical to the tree the reused baseline evidence was captured against.

The one SHA that may appear is the **immutable commit the baseline evidence series was captured in**, used as a diff basis. That value never changes.

**Why:** In #418 cycle 2 the orchestrator gave a false clean-tree premise, then fixed it by committing the carried-in state and asking me to update the plan's "expected HEAD" to the new SHA. Committing that very edit moved HEAD again, so the expectation was stale one commit after being corrected. Chasing the SHA forward cannot converge: any commit touching the plan file invalidates the plan's own precondition. The invariant formulation is stable across any number of documentation or agent-memory commits and fails exactly when it should — when someone touches source between the baseline capture and the cycle.

**How to apply:** Applies to any plan that reuses a prior evidence series instead of re-capturing a full baseline (see [[evidence-path-normalization]] for where those artifacts live). Write the reuse argument in Design Decision form as "reuse holds for any HEAD whose source and build-configuration tree is identical to `<baseline-sha>`'s", never as "the current HEAD is `<sha>`". Also sweep downstream tasks that cite the reuse argument — a generalized Decision plus a still-SHA-pinned task reintroduces the rot. Define the gate by the **category of path it governs** (production and build inputs) rather than by enumerating specific permitted-dirty files: a named permitted-dirt list bakes one session's hygiene lapse into a standing condition and would direct the executor to revert another agent's files, while a category gate stays true in every worktree. Halt-and-report is the only correct response to a source-tree entry in the porcelain output. #454 cycle 2 required two passes on this same task: the first narrowing ("nothing outside `<FEATURE>/evidence/`") was still broader than any real tree could satisfy, because the feature folder above `evidence/` is untracked too.
