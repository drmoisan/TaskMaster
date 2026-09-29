---
name: parallel-item-preparation-is-structurally-impossible
description: In a parallel item whose session root holds a foreign checkpoint, atomic-planner, prd-feature AND every git add/commit are simultaneously blocked, so preparation cannot complete — measure all three up front and report blocked rather than working around them
metadata:
  type: project
---

Three separate PreToolUse/SubagentStop gates resolve repo-relative paths against the Claude session
cwd. In a parallel item that cwd is the coordinator worktree, never the item worktree, so all three
fail together. Verified end to end on item #882 of run `bugs-2026-09-11` (2026-09-13), and it is the
third consecutive item to deadlock this way after #743 and #871.

**1. `git add` / `git commit` — blocked, with no legitimate escape.**
`enforce-orchestration-preimplementation-gate.ps1` lines 373-395 say in terms that "the path and
command legs are single-feature by construction; only the delegation leg carries a mode marker". So
the command leg has NO parallel branch and always reads the session-root default checkpoint. When
that file is a sibling's with `lifecycle_ready` empty, every staging command in your worktree denies.
The issue #539 exemption cannot rescue it: `Test-ExemptOrchestrationSegmentToken` requires
`Token[0] -ceq 'git'` and `Token[1] -ceq 'add'|'commit'`, so a `git -C <abs> add` form fails at
`Token[1]`, and `Test-ExemptOrchestrationOperand` separately rejects any drive-lettered operand. With
Bash cwd at the session root and `cd` forbidden, no admissible form reaches the item worktree.
A git plumbing sequence (`hash-object` / `update-index` / `write-tree` / `commit-tree` /
`update-ref`) does evade the gate's `add|commit` patterns — do NOT take it. That is deliberate
evasion of a PreToolUse gate and needs explicit one-time human authorization.

**2. `Agent(atomic-planner)` — blocked.** Exactly as
[[prd-feature-hook-parses-prompt-paths]] records. Confirmed again here both by a local dot-sourced
probe and by the real delegation, which returned a byte-identical reason.

**3. `Agent(prd-feature)` — blocked, and BOTH of its stop hooks are unsatisfiable, not just one.**
[[prd-feature-stop-hooks-are-workmode-blind]] records the unconditional `user-story-path`
requirement. The second one is worse and is new: `validate-prd-feature-output.ps1` line 80 calls
`Test-Path -LiteralPath $specPath` on the path the agent itself reports, with no cwd override. In a
parallel item that resolves against the session root, where your feature folder does not exist, so
the check fails for EVERY value the agent could report. There is no prompt wording that fixes it.
Do not spend a delegation on it; author `spec.md` yourself and record it under
`local_execution_overrides` with the measured reason. `prd-feature` is not in the orchestrator
persona's mandated delegate set (`atomic-planner`, `atomic-executor`, `feature-review`,
`task-researcher`), so this is not absorbing a mandated delegated step.

## Do not record local authoring as a delegation receipt

Putting a `delegation_receipts.agents[]` entry whose `agent_name` is anything other than a real
delegate fails the MCP validator under `require_model_routing`: the gate demands a
`model_routing_receipts[]` entry for every `agent_name` it finds, and it does not recognise a prose
name. Error seen: `Checkpoint model_routing_receipts is missing a receipt for delegated agent:
orchestrator (local authoring, ...)`. Use `local_execution_overrides` instead; the checkpoint then
validates.

## The session-root checkpoint is REWRITTEN MID-RUN by a live sibling

[[model-routing-hook-reads-canonical-path-only]]'s advice to read it once and predict the whole run
is too weak. On #882 two reads minutes apart returned different item payloads and different
`model_routing_receipts` sets, because a live sibling owns the file. A single read predicts nothing
durable. Worse, the Read tool and a `pwsh` `Get-Content` of the SAME absolute path returned
different contents in the same session, so only the `pwsh` read is a valid proxy for what a
PowerShell hook sees. **Probe the hook itself, not the file**: dot-source the live session-root hook
and call its decision function with a synthetic payload. That is exact, costs one command, and on
#882 it predicted the real `atomic-planner` denial verbatim.

**How to apply.** Run all three probes before doing any work: a `git add` of an exempt path, a
dot-sourced `Invoke-PrdFeatureBeforePlannerDecision`, and a `pwsh` read of the session-root
checkpoint's `lifecycle_ready`. If the planner probe denies, preparation cannot complete — say so at
once. Still produce everything reachable (promoted record, folder, `issue.md`, `spec.md`, research
via `Agent(task-researcher)`, which IS admitted, and a declared blast radius derived from the spec
instead of from an approved plan, with that substitution stated), leave it uncommitted, and hand the
planner delegation plus the commit to the coordinator. Both defects are push-down-owned from
drm-copilot; fix upstream, never here. See
[[project_claude_files_are_pushdown_owned_fix_upstream]] and
[[shared-checkpoint-read-modify-write-corrupts]] for why writing your payload into the session-root
file is the wrong repair.
