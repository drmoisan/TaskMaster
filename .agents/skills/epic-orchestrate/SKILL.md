---
name: epic-orchestrate
description: Execute a manually authored multi-feature epic through deterministic waves, isolated worktrees, integration-branch fan-in, and a final integration PR.
---

# Epic Orchestrate Skill

Use this manual-manifest compatibility entry only from the root session. The root session must
delegate the supplied manifest or slug to the project custom agent `epic-orchestrator`; an
ordinary `orchestrator` is prohibited from doing so. A committed planner kickoff is optional for
this entry path.

Use the epic manifest path or slug supplied in the invoking user request.

## Prerequisites

Read `AGENTS.md`, applicable language policies, `config/orchestration-routing.json`, and any
existing `artifacts/orchestration/epic-orchestrator-state.json` checkpoint before acting.

## Manifest Contract

The source of truth is `docs/features/epics/<epic-slug>/epic.md`. Its YAML frontmatter is:

```yaml
---
epic: <epic-slug>
integration_branch: epic/<epic-slug>-integration
created_at: <iso8601>
intent: # optional as a whole
  epic_type: <business | enabler>
  business_outcome_hypothesis: <non-empty measurable outcome>
  leading_indicators: [<string>, ...]
  nfrs: [<string>, ...]
features:
  - issue_num: <int>
    feature_folder: <resolvable-hint-basename>
    depends_on: [<upstream-issue_num>, ...]
---
```

`issue_num` is the stable primary key. `feature_folder` is a resolvable hint under `active/` or
`completed/`. Every dependency must resolve against the union of issue numbers and legacy folder
basenames. Reject duplicate folders, unresolved dependencies, malformed optional intent, and
cycles before kickoff.

## Wave Computation

Use longest-path layering:

```text
wave(f) = 0                                      when depends_on(f) is empty
wave(f) = 1 + max(wave(d) for d in depends_on(f)) otherwise
```

Use `scripts/dev_tools/epic_wave_computation.py` as the tested reference. Serialize features
within each wave lexicographically by folder, without changing wave membership.

## Entry Paths

- Prepared path (`epic-run`): require and validate `epic-kickoff.md`, reuse the recorded
  integration branch, and resume each child at atomic execution.
- Manual path (`epic-orchestrate`): validate the manifest and create the integration branch when
  absent. A kickoff artifact is not required.

## Integration Branch and Child Worktrees

Create a missing integration branch from current `origin/main` and push it. Before each wave,
fetch the current remote integration tip. Create every child worktree from that tip, never from
`main`, and set each feature PR base explicitly to the integration branch.

Launch all ready children in one bounded parallel wave. Before each launch, persist the
epic-child topology receipt, reviewed complexity assessment, delegation receipt, and Codex
model-routing receipt with the same `delegation_id`, then select the generated deployment agent.
C3 epic children use the elevated Sol/High profile. The child prompt contains:

> `Epic mode: true. epic_feature_folder: <epic-slug>. integration_branch: epic/<epic-slug>-integration. epic_checkpoint_path: artifacts/orchestration/epic-orchestrator-state.json. PR base branch MUST be <integration_branch>, not main; pass --base <integration_branch> to gh pr create.`

For prepared epics, also provide the committed `plan-path` and state that the child resumes at
atomic execution. For each dependency, include its concrete spec, plan, PR, merge SHA, and target
branch as upstream context.

Do not use native `spawn_agent` for worktree children. For each wave, write an immutable launch
specification under `artifacts/orchestration/epic-child-launches/<wave-id>/` and invoke
`.codex/scripts/launch-epic-child-wave.ps1` with `checkpoint_kind: "epic-orchestrator"`, the
current wave number, checkpoint path, integration branch, `max_parallel_features`, and exact
generated profile values for every child. Monitor the durable wave status and record each child
launch receipt/status path. A missing session id, nonzero exit, receipt mismatch, or incomplete
wave status blocks the wave barrier.

The JSON specification requires `schema_version: 1`, `wave_id`, `checkpoint_kind`,
`checkpoint_path`, `integration_branch`, `wave_number`, `max_parallel_features`, and `launches`.
Each launch requires `launch_id`, `delegation_id`, `feature_folder`, positive `issue_num`,
`deployment_agent`, `model`, `model_reasoning_effort`, `permissions`,
`execution_context: "epic_execution_child"`, canonical absolute `worktree_path`, `branch_name`,
and the exact prompt. Invoke it as:

```powershell
pwsh -NoProfile -File .codex/scripts/launch-epic-child-wave.ps1 -LaunchSpecPath <spec-path> -MaxParallel <max_parallel_features>
```

Parse the returned JSON `status_path`. The status file is shared by the wave; every child has its
own immutable receipt but references the same `wave.<wave_id>.status.json`.

## Wave Barrier

Do not start wave N+1 until every dependency edge is durably `merged` or `worktree_removed`.
Reconcile this state using Git worktrees, branches, and live PR state on resume. The Codex
mutation hook is a per-child deterrent; the epic-state validator is the retrospective,
authoritative backstop.

## Fan-In and Conflict Handling

Each child owns its implementation, review, PR, CI-green gate, and merge into the integration
branch. A merge conflict enters that child's existing remediation loop with a blocking
`remediation-inputs.<timestamp>.md`. After three unresolved passes, record
`blocked_conflict_loop_limit` and stop that edge. The epic agent must not resolve child conflicts
locally.

## Worktree Cleanup

After the child merge SHA is recorded and the epic checkpoint mirrors `merge_status: "merged"`,
remove its worktree. The worktree-removal hook denies removal before the matching feature is
`merged` or `worktree_removed`. Record `worktree_removed_at` after success.

## Model and Deployment Policy

Route selection and model selection are independent. The deterministic file-count or marker
route selects topology; the C1-C4 assessment selects a checked-in Codex deployment profile.

- C1: `gpt-5.6-luna`, low.
- C2: `gpt-5.6-terra`, medium.
- standalone C3 with ceiling C3: `gpt-5.6-terra`, high.
- epic C3 or C3 with a C4 sibling: `gpt-5.6-sol`, high.
- C4: `gpt-5.6-sol`, max.
- `epic-orchestrator`: `gpt-5.6-sol`, ultra.

Persist the topology and model-routing receipts before every child or `pr-author` delegation. The
deployed agent type and actual model must match its receipts and start attestation. If the
required profile is unavailable, record `model_unavailable` and stop; do not fall back silently.

## Status Projection and Checkpoint

Regenerate `docs/features/epics/<epic-slug>/epic-status.md` from the checkpoint at kickoff, each
merge-status transition, each wave transition, and each final-PR transition. Never treat the
status document as the DAG source.

Persist `artifacts/orchestration/epic-orchestrator-state.json` with `objective`, `route_id:
"epic"`, epic folder/manifest/status paths, integration branch, completed and next steps,
timestamps, bounded `max_parallel_features`, current wave, waves, features and lifecycle
timestamps, final PR, complexity/model receipts, and required agent/skill/MCP receipts. Each
launched feature records issue/folder, unique branch/worktree, delegation receipt/id,
delegation-bound model receipt, and launch receipt/status paths.

On resume, reconcile the checkpoint against `git worktree list --porcelain`, branch state, and
`gh pr view --json state,mergedAt,headRefOid` before continuing.

## Final Integration PR

After every child is merged or its worktree removed, delegate final PR authoring to the routed
`pr-author` profile, refresh PR context through the MCP surface, run the CI-green procedure,
record the current head SHA and successful conclusion, and merge the integration branch to
`main` only through the merge gate.

## Completion

Do not report completion until every feature is `merged` or `worktree_removed`, the final PR is
merged with its merge SHA recorded, `epic-status.md` reflects that state, acceptance criteria are
checked, and the MCP validator passes `epic-orchestrator-state` with `require_complete: true`.
Require both `require_codex_topology: true` and `require_codex_model_routing: true` on that final
validation call.
