---
name: epic-run
description: Execute a previously prepared epic by resolving its committed kickoff artifact and delegating it to the Codex epic-orchestrator agent.
---

# Epic Run Skill

Use this skill only from the root session. The root session must resolve the committed kickoff
artifact and delegate its invocation prompt to the project custom agent `epic-orchestrator`.
The root-invocation hook records the authorization receipt consumed when that agent starts.

Use the epic slug or path supplied in the invoking user request.

## Procedure

1. Resolve a bare slug to `docs/features/epics/<epic-slug>/`; resolve a supplied path to its
   containing epic home.
2. Require `docs/features/epics/<epic-slug>/epic-kickoff.md`.
   - If it is absent, stop before delegation. Direct the user to run `epic-plan` first or, for a
     manually authored epic, invoke `epic-orchestrate <manifest-path>`.
3. Validate the kickoff's `## Invocation Prompt`, manifest reference, integration branch,
   per-feature plan paths, preflight evidence, and current Git state. Derive optional missing
   hashes from Git; fail closed on actual drift.
   Invoke `validate_orchestration_artifacts` for `epic-planner-state` with
   `require_ready_for_execution: true` and the explicit workspace root. Do not delegate until
   that canonical repository-aware gate succeeds.
4. Delegate the invocation prompt to `epic-orchestrator` and apply `epic-orchestrate`.
5. Reuse the existing integration branch. Every child resumes at atomic execution from its
   committed plan path. Do not repeat promotion, research, feature documents, planning, or
   preflight.
6. If `artifacts/orchestration/epic-orchestrator-state.json` already tracks this epic, resume from
   its durable `next_step` instead of restarting.
7. Worktree children must be launched through `.codex/scripts/launch-epic-child-wave.ps1`; do
   not replace the launcher with native `spawn_agent`.
8. Copy the validated planner checkpoint's `max_parallel_features` into the epic-orchestrator
   checkpoint before constructing the first execution launch specification.

Wave scheduling, fan-in, worktree cleanup, `epic-status.md`, final PR creation, CI validation,
and completion are governed entirely by `epic-orchestrate`.
