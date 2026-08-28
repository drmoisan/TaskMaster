---
name: epic-plan
description: Scope and prepare a multi-feature epic end-to-end before execution through the Codex epic-planner agent.
---

# Epic Plan Skill

Use this skill only from the root session. The root session must delegate the supplied objective
to the project custom agent `epic-planner`; it must not execute this procedure locally or route it
through `orchestrator`. The root-invocation hook records the authorization receipt consumed when
`epic-planner` starts.

Use the objective or existing manifest path supplied in the invoking user request.

Planning ends after every child feature has completed promotion, research, feature documents,
atomic planning, and preflight clearance. Atomic execution, PR authoring, feature execution
review, and CI monitoring are not part of this skill.

## Prerequisites

Before proceeding, `epic-planner` must read `AGENTS.md`, the applicable language policies under
`.agents/skills/`, `config/orchestration-routing.json`, and any existing
`artifacts/orchestration/epic-planner-state.json` checkpoint.

## Epic-Worthiness Gate

The objective warrants an epic only when both conditions hold:

1. It decomposes into at least two independently mergeable child features, each with its own
   issue, active feature folder, and eventual PR.
2. At least one child exceeds, or the combined work clearly exceeds, one practical large-path
   feature budget.

When either condition fails, record `epic_worthiness.verdict: "non_epic"`, state the feature-count
and change-budget rationale, and offer a single root-session `orchestrator` delegation. Do not
create epic scaffolding unless the user explicitly overrides the verdict.

## Decomposition and Wave Design

For an epic-worthy objective:

1. Define `docs/features/epics/<epic-slug>/` and author `epic.md` using the manifest schema in
   `epic-orchestrate`.
2. Record goal, scope, non-goals, shared design, acceptance criteria, decomposition rationale,
   production-file estimates, and integration risks.
3. Add `depends_on` edges only for real upstream contracts.
4. Compute execution waves with the longest-path formula in
   `scripts/dev_tools/epic_wave_computation.py`; unresolved references and cycles block
   preparation.
5. Assess each child as C1-C4 using the central policy. Record the assessed band, rationale,
   deployment agent, model, reasoning effort, and the monotonic orchestration complexity ceiling.
6. Set `max_parallel_features` to an integer from 1 through 8. Use the routing-policy default of
   4 unless repository or user constraints require a lower value.

Child issue numbers may be placeholders only while the draft manifest is being decomposed.
Before creating child worktrees, promote every child through the worktree-aware MCP lifecycle,
backfill the final positive `issue_num` and active `feature_folder`, commit and push that resolved
manifest, and persist the promotion receipts. Launch specifications and their immutable receipts
must never use placeholders.

## Integration Branch

Create or reuse `epic/<epic-slug>-integration` from `origin/main`, push it, and commit the epic
home before preparation. All prepared child outputs must fan into this branch.

## Concurrent Preparation

Place all child preparations in one preparation batch, regardless of execution-wave
dependencies. Each child runs in an isolated worktree branched from the current
`origin/epic/<epic-slug>-integration` tip. Dependency context is supplied for planning, but it
does not serialize preparation. The batch launcher enforces `max_parallel_features`; excess
children remain queued in the same batch rather than being divided by execution wave.

Before each child starts, persist its epic-child topology receipt, complexity assessment,
delegation receipt, and Codex model-routing receipt. The model receipt must carry the same
`delegation_id` as the delegation receipt.

Do not use native `spawn_agent` for a worktree child because that API does not bind the child to
the prepared worktree. Write one immutable launch specification under
`artifacts/orchestration/epic-child-launches/<batch-id>/` and invoke
`.codex/scripts/launch-epic-child-wave.ps1`. Use `checkpoint_kind: "epic-planner"`,
`wave_number: 0`, the planner checkpoint path, integration branch, bounded maximum, and one
launch record per child. Every record supplies the unique launch/delegation ids, prepared
worktree and branch, generated `orchestrator-cN` profile, exact model/reasoning/permissions, and
prompt. C3 epic children use the elevated Sol/High profile. Monitor the returned durable status
path; a nonzero child exit or missing completion status blocks fan-in.

The launch specification schema is:

```json
{
  "schema_version": 1,
  "wave_id": "<safe-preparation-batch-id>",
  "checkpoint_kind": "epic-planner",
  "checkpoint_path": "artifacts/orchestration/epic-planner-state.json",
  "integration_branch": "epic/<epic-slug>-integration",
  "wave_number": 0,
  "max_parallel_features": 4,
  "launches": [
    {
      "launch_id": "<unique-id>",
      "delegation_id": "<unique-id>",
      "feature_folder": "<final-active-feature-folder>",
      "issue_num": 123,
      "deployment_agent": "orchestrator-c3-elevated",
      "model": "gpt-5.6-sol",
      "model_reasoning_effort": "high",
      "permissions": "orchestrator-workspace",
      "execution_context": "epic_preparation_child",
      "worktree_path": "<canonical-absolute-path>",
      "branch_name": "<child-preparation-branch>",
      "prompt": "<preparation prompt>"
    }
  ]
}
```

Write it under `artifacts/orchestration/epic-child-launches/<batch-id>/`, then run:

```powershell
pwsh -NoProfile -File .codex/scripts/launch-epic-child-wave.ps1 -LaunchSpecPath <spec-path> -MaxParallel <max_parallel_features>
```

Parse the returned JSON and persist its shared `status_path`. Every worktree must be a clean Git
worktree of this repository on the checkpoint branch. Its committed `.codex/`, `.agents/`,
`AGENTS.md`, routing configuration, and selected profile must match the trusted integration
source; the launcher rejects drift before starting Codex.

Every child prompt must contain this literal line:

> `Preparation mode: true. route_id: preparation. epic_feature_folder: <epic-slug>. integration_branch: epic/<epic-slug>-integration. Reuse and verify the completed promotion receipt, then perform research, feature documents (issue.md, spec.md, user-story.md), atomic planning, and preflight clearance only. Atomic execution, PR authoring, and CI monitoring are out of scope for this run and are executed later by epic-orchestrator. After the atomic-executor preflight returns PREFLIGHT: ALL CLEAR, commit the feature folder and plan to the current branch, set out-of-scope step statuses to not-applicable, set next_step to S5_atomic_execution, and stop, reporting research_path, plan-path, preflight_evidence_path, and the exact preflight status.`

The line intentionally omits `Epic mode: true`. For dependent children, also cite the upstream
manifest scope, specification, and planned contract.

## Preparation Route Contract

Each child selects the exact `preparation` route from the central routing configuration:

- required agents, in configured order: `task-researcher`, `prd-feature`, `atomic-planner`,
  `atomic-executor`;
- required skills: `orchestrate`, `feature-promotion-lifecycle`, `atomic-plan-contract`;
- required MCP operations: `new_potential_entry`, `potential_to_issue`,
  `new_active_feature_folder`, `validate_orchestration_artifacts`;
- mandatory completed phases: `S3_promotion`, `S4_atomic_planning`;
- terminal `next_step`: `S5_atomic_execution`;
- execution-through-CI statuses: `not-applicable`;
- `blocked_reason`: `none`;
- `requires_ci_gate`: the literal JSON Boolean `false`.

Promotion must use worktree-aware MCP calls and receipts. A child must not claim feature
completion, create a PR, execute the atomic plan, or monitor CI.

## Fan-In

As each preparation finishes:

1. Merge the child preparation branch into the integration branch.
2. Treat any conflict as a decomposition defect: abort fan-in, record blocked state, and do not
   resolve the conflict ad hoc.
3. Backfill `issue_num` and `feature_folder` in `epic.md`.
4. Record `preparation_status: "prepared"`, `research_path`, `plan_path`,
   `preflight_evidence_path`, and `preflight_status: "PREFLIGHT: ALL CLEAR"` in the planner
   checkpoint. `research_path` must be under `docs/features/<feature>/research/` inside the feature folder.
   Each child references its immutable receipt and the shared `wave.<wave_id>.status.json` path.
5. Remove the worktree only after its preparation branch is merged.

Push the integration branch after final fan-in.

There is no mid-planning approval pause. The planner completes promotion, research, feature
documents, atomic planning, and preflight before stopping at the user execution boundary.

## Execution-Readiness Gate

Do not emit a kickoff until the manifest is valid, the checkpoint has a forced `epic-planner`
topology receipt, and every child has a promoted issue, active folder, `issue.md`, research,
`spec.md`, `user-story.md`, approved atomic plan, epic-preparation topology receipt,
delegation-bound model-routing receipt, successful launcher receipt/status, `PREFLIGHT: ALL
CLEAR` evidence, and a pushed preparation commit. The durable and ignored kickoff copies must be
byte-identical; the manifest, durable kickoff, plans, and planning commits must pass the
repository-aware readiness validator without worktree drift.

Invoke `validate_orchestration_artifacts` for `epic-planner-state` with
`require_ready_for_execution: true` and the explicit workspace root. Do not write or delegate the
kickoff until this canonical gate succeeds.

## Kickoff Artifacts

Write the ignored copy to `artifacts/orchestration/epic-kickoff-<epic-slug>.md` and commit the
durable copy at `docs/features/epics/<epic-slug>/epic-kickoff.md`:

```markdown
# Epic Kickoff: <epic-slug>

Planned by epic-planner on <iso8601>. All child features are prepared. Planning state:
artifacts/orchestration/epic-planner-state.json (branch: epic/<epic-slug>-integration).

## Invocation Prompt

Run `/epic-run <epic-slug>` to execute this epic, or paste the prompt below.

Use the epic-orchestrator subagent to execute the prepared epic at
docs/features/epics/<epic-slug>/epic.md. Reuse epic/<epic-slug>-integration. Every child resumes
at atomic execution from its committed plan-path; do not repeat promotion, research, feature
documents, planning, or preflight.

## Feature Summary

| issue_num | feature_folder | wave | complexity | plan-path |
| --- | --- | --- | --- | --- |
| ... | ... | ... | ... | ... |
```

The baseline artifact shape is authoritative. An optional integrity block may record the
planning commit and plan hashes, but `epic-run` must derive absent integrity values from Git.

## Checkpoint and Completion

Persist `artifacts/orchestration/epic-planner-state.json` after every completed step with:
`objective`, epic folder and manifest, integration branch, worthiness verdict and rationale, the
bounded `max_parallel_features`, forced root `topology_receipt`, and kickoff path. Each feature
records issue/folder/dependencies/wave/complexity, `research_path`, `plan_path`, optional
`preflight_evidence_path`, preparation/preflight state, branch/worktree, topology receipt,
delegation receipt, delegation-bound model receipt, launch receipt/status paths, and planning
commit/hash when declared. Also record `completed_steps`, `next_step`, and `last_updated`.

The successful terminal result is `EPIC_EXECUTION_READY`. The final report lists the manifest,
integration branch, both kickoff paths, and one `plan-path:` and preflight-status line per child.
It must state that execution has not started and starts only after a later root invocation of
`epic-run`.
