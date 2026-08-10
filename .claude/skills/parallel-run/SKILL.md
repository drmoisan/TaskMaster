---
name: parallel-run
description: Execute a previously planned parallel run by replaying the committed parallel-kickoff artifact through the parallel-orchestrator agent. Use after /parallel-plan has prepared the run (manifest, per-item folders, research, specs, atomic plans, preflight clearance, seeded cohorts) and the user is ready to execute end-to-end.
argument-hint: "[parallel-slug]"
context: fork
agent: parallel-orchestrator
---

# Parallel Run Skill

A user invocation (`/parallel-run <parallel-slug>`) forks the `parallel-orchestrator` agent to
execute a parallel run that `parallel-planner` has already prepared. The parallel slug (or a path
under `docs/features/parallel/`) for this run is:

$ARGUMENTS

## Procedure

1. Resolve the parallel home. A bare slug resolves to `docs/features/parallel/<parallel-slug>/`; a
   path argument resolves to its containing parallel run folder.
2. Resolve the committed kickoff artifact
   `docs/features/parallel/<parallel-slug>/parallel-kickoff.md`. Discovery is a single local path
   lookup in the invoking worktree. This surface has no integration branch, so there is no
   integration ref to fetch, no ref-existence probe to run, and no ref-reading fallback to attempt:
   the durable copy of the kickoff artifact is committed by `/parallel-plan` on the planner branch
   `parallel/<parallel-slug>-plan` and is reachable from the checkout that invokes this skill.
   - STOP without delegating anything when that path does not exist. Report that the run has no
     committed kickoff artifact: the user must run `/parallel-plan` first (or, for a run whose
     manifest was authored manually, invoke `/parallel-orchestrate <manifest-path>` directly).
   - Do not synthesize a kickoff prompt, do not fall back to the planner's working copy under
     `artifacts/orchestration/`, and do not launch a partial run in place of the STOP.
3. Execute the kickoff artifact's `## Invocation Prompt` section as the run objective, applying the
   `parallel-orchestrate` skill procedure and the `## Prepared-Run Execution` section of
   `.claude/agents/parallel-orchestrator.md`: each item's child `Agent(orchestrator)` delegation
   resumes at atomic execution from that item's committed `plan-path` rather than re-running
   promotion, research, or planning. Feature-document authoring and preflight clearance are
   likewise not repeated: those outputs are already committed and preflight-clear on each item's
   own pushed feature branch.
4. Honor existing checkpoint state: if `artifacts/orchestration/parallel-orchestrator-state.json`
   already tracks this `parallel_slug`, resume per the `parallel-orchestrate` skill's resume
   procedure instead of restarting, re-deriving durable ground truth from
   `git worktree list --porcelain`, `git branch`, and `gh pr view --json state,mergedAt,headRefOid`
   before acting on any recorded value.

## Scope

This skill adds no procedure of its own beyond kickoff-artifact resolution. Cohort consumption and
ordering, the cohort barrier, `max_concurrency` slot filling, the per-item branch and worktree
lifecycle, the child kickoff parameter, model selection, per-item merge to `main` on green,
per-item merge-conflict handling, worktree cleanup, `parallel-status.md` maintenance, checkpoint
persistence, and mode-dependent completion are governed entirely by the `parallel-orchestrate`
skill (`.claude/skills/parallel-orchestrate/SKILL.md`).

There is no final integration pull request on this surface, and no fan-in step follows the items:
each item's pull request targets `main` directly and is merged individually by
`parallel-orchestrator` after its checks are durably confirmed green.
