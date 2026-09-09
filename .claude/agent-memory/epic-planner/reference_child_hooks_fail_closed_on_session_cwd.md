---
name: child-hooks-fail-closed-on-session-cwd
description: A non-isolated Agent(orchestrator) child inherits the session working directory, where two hooks read artifacts/orchestration/orchestrator-state.json cwd-relative and FAIL CLOSED — so every sub-delegation is denied; never launch a prep child without worktree isolation
metadata:
  type: reference
---

Measured 2026-09-09 by reading the hook sources before launching a resumed fan-out.

A subagent launched WITHOUT `isolation: "worktree"` inherits the parent's working directory. Two
hooks resolve the orchestrator checkpoint as the bare relative literal
`artifacts/orchestration/orchestrator-state.json`, and both fail closed when it is absent:

- `.claude/hooks/enforce-model-routing-receipt.ps1` — PreToolUse on `Agent`. If `subagent_type` is
  one of `atomic-planner`, `atomic-executor`, `feature-review`, `task-researcher`, `prd-feature`,
  `pr-author`, it requires a `model_routing_receipts[]` entry naming that agent. A missing file
  returns `$null`, `Test-ModelRoutingReceiptPresent` returns `$false`, and the delegation is
  DENIED. A non-isolated preparation child therefore cannot delegate at all.
- `.claude/hooks/validate-orchestrator-output.ps1` — SubagentStop on `orchestrator`, same default
  path, blocks termination.

The epic-planner session worktree holds `epic-planner-state.json`, never `orchestrator-state.json`,
so the file is genuinely absent there. Pointing N concurrent children at one shared copy is also
wrong: [[concurrent-prep-children-worktree-isolation]] records that they overwrite each other and
share one git index.

**Conclusion: always `isolation: "worktree"` for preparation children.** To resume work that lives
in a dead child's worktree, do not drop isolation to reach it — have the fresh isolated child import
that worktree by absolute path. See
[[recover-dead-prep-child-by-committing-then-relaunching]].

Two related facts measured in the same pass, both favourable:

- `enforce-orchestration-preimplementation-gate.ps1` classifies an `Agent(orchestrator)` delegation
  by STRUCTURE. `Resolve-OrchestrationDelegationMode` reading the markers `Preparation mode: true.`
  and `route_id: preparation.` returns `preparation`, which is NOT implementation, so the spawn
  needs no ready checkpoint anywhere. (Confirms [[pre-implementation-gate-blocks-orchestrator-kickoff]]
  applies to execution-mode delegations, not preparation-mode ones.)
- The same gate's `Test-ImplementationPath` returns `$false` for any path under
  `docs/features/**` via `Test-FeatureDocumentationOrEvidencePath`, and exempts
  `artifacts/orchestration/<one of seven checkpoint names>` **including absolute spellings**. A
  child writing only feature documents and its own checkpoint never arms the gate on the path leg.
