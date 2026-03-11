---
name: csharp-orchestration-state-machine
description: Checkpoint schema and resume protocol for long-running C# orchestration workflows.
---

# C# Orchestration State Machine

Canonical checkpoint and resume behavior for orchestration agents running multi-step C# delivery flows.

## When to Use This Skill

Use this skill when:
- A workflow spans multiple delegations and commands.
- Execution may be interrupted and must resume deterministically.
- An orchestrator must avoid repeating completed steps.

## Canonical Checkpoint Location

- `artifacts/orchestration/csharp-orchestrator-state.json`

## Required Checkpoint Fields

- `objective`
- `change_budget_estimate`
- `path_selected` (`small` or `large`)
- `promotion-type`
- `short-name`
- `relativeFile`
- `long-name`
- `issue-num`
- `feature-folder`
- `completed_steps`
- `next_step`
- `last_updated`

## Update Protocol

- Write checkpoint after every completed orchestration sub-step.
- Treat checkpoint as source-of-truth for progress state.
- Never claim mission completion until checkpoint marks final state.

## Resume Protocol

On invocation:
1) Read checkpoint if it exists.
2) If incomplete, resume from `next_step` without re-running `completed_steps`.
3) If missing or completed, start at phase-0 intake.
4) If user explicitly requests restart, reset checkpoint and start phase-0.
