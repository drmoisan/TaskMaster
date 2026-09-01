---
name: preimplementation-gate-needs-lifecycle-ready-bool
description: PREIMPLEMENTATION_GATE_BLOCKED needs a TOP-LEVEL boolean `lifecycle_ready`, not a `lifecycle_readiness` object; the hook resolves the checkpoint against the AGENT worktree cwd
metadata:
  type: project
---

`.claude/hooks/enforce-orchestration-preimplementation-gate.ps1` `Test-OrchestrationReady` reads
exactly four values and all four must be truthy:

- `issue-num` (the HYPHEN spelling, read via `Get-StringProperty`)
- `feature-folder` (hyphen spelling; must additionally `StartsWith('docs/features/active/')`)
- `route_id`, falling back to `path_selected`
- **`lifecycle_ready` — a top-level BOOLEAN**, read as
  `if ($Payload.PSObject.Properties.Name -contains 'lifecycle_ready') { [bool]$Payload.lifecycle_ready }`

**Why:** the descriptive name for this concept is `lifecycle_readiness`, and writing a rich
`lifecycle_readiness: { promotion_complete: true, ... }` object reads like the obvious encoding and
satisfies nothing — the property name does not match, so the default `$false` stands and the gate
denies. The deny message names "lifecycle readiness" without naming the key, so the message does not
tell you which spelling it wants. Verified 2026-09-01 on issue #633: a checkpoint carrying every other
required field plus a full `lifecycle_readiness` object was still denied; adding the single line
`"lifecycle_ready": true` cleared it on the next call.

**Path resolution.** `$script:CheckpointPath` is the RELATIVE literal
`artifacts/orchestration/orchestrator-state.json`, resolved against the hook process's cwd. In an
isolated agent worktree that is the AGENT's own worktree, not the session root — so seeding your own
worktree's checkpoint is sufficient and you do not need to touch the shared session-root file. This
confirms [[agent-worktree-hooks-resolve-to-agent-cwd]] against
[[child-orchestrator-pr-hook-reads-session-root]]: the two hooks differ, so check the specific hook
rather than generalizing from the other one.

**Blast radius of the deny.** The gate fires on ordinary Write calls, including a `.ps1` written to
the system scratchpad OUTSIDE the repo. So it can block routine setup work — such as the scratch
script used to run the model-routing modules — long before any delegation. Seed the checkpoint as the
very first action on a resumed run, ahead of model-routing preflight.

**How to apply:** on any resume where the tracked checkpoint belongs to a different objective, rewrite
it for yours and include `"lifecycle_ready": true` alongside the hyphenated `issue-num` and
`feature-folder` keys. See [[orchestrator-state-flat-keys-and-enum]] and
[[orchestrator-state-json-is-tracked-in-git]].
