---
name: parallel-child-hook-exposure-measured-not-assumed
description: Measured on issue #872 - a non-isolated parallel child committed docs/features/active paths and delegated atomic-executor WITHOUT ever syncing the shared session-root checkpoint; the prd-feature hook gates only atomic-planner
metadata:
  type: project
---

A parallel item's brief usually warns that hooks resolve `artifacts/orchestration/orchestrator-state.json`
relative to the agent cwd, which for a non-isolated child is the COORDINATOR session worktree, not the
item worktree — so the child is evaluated against a sibling's state. That is true of the path resolution
and it is the right thing to be warned about. It does not follow that you must write into the shared
file. Measure first; on #872 the answer was that no sync was needed at all.

**What actually fired, measured 2026-09-13 from a non-isolated child whose checkpoint existed ONLY in
its own item worktree:**

- `git add` and `git commit` of a `docs/features/active/**` path: **allowed**. The pre-implementation
  gate's `Test-ImplementationPath` returns `$false` for anything matching `(^|/)docs/features/active/`
  before it ever considers the file extension, so a feature-folder commit is not an implementation
  operation and never reaches the readiness check. No exempt-form contortion and no shared-file sync
  was required.
- `Agent(atomic-executor)`: **allowed**, despite `enforce-model-routing-receipt.ps1` defaulting to the
  canonical relative checkpoint path. Do not conclude the receipt is unnecessary — record it anyway —
  but do not pre-emptively write into the shared file to satisfy it.
- `enforce-prd-feature-before-planner.ps1`: gates **only** `atomic-planner`. Line 362 returns allow for
  every other `subagent_type`. So the `PRD_FEATURE_BLOCKED` hazard that makes remediation unreachable on
  a parallel surface does NOT touch `atomic-executor` or `feature-review`; a blocking review finding is
  the only thing that walks you into it.

**The gate's actual bar is low.** `Test-OrchestrationReady` wants exactly four things: a non-empty
`issue-num`, a `feature-folder` that `StartsWith('docs/features/active/')`, a `route_id` **or**
`path_selected`, and a truthy `lifecycle_ready`. It does not cross-check the issue number against the
prompt or the branch. That is why a sibling's well-formed checkpoint sitting at the shared path does not
actually block you, and why a race on that file would not block a sibling either.

**How to apply.** Seed your checkpoint in your own worktree, then just TRY the gated command. Treat the
one byte-for-byte sync into the session root as a last resort you have evidence you need, not as setup.
Every avoided write to that shared file is one less chance of the failure mode where a child writes into
the coordinator worktree — which on this run destroyed another item's uncommitted work. Note also that
the MCP validator is stricter than the hook: it demanded `relativeFile`, `long-name`, `work-mode` and
`plan-path` (all hyphenated, not underscored) and rejected `step7_status: in-progress` and an unsupported
key under `delegation_receipts.promotion`. See [[orchestrator-state-flat-keys-and-enum]] and
[[preimplementation-gate-reads-sibling-checkpoint]].
