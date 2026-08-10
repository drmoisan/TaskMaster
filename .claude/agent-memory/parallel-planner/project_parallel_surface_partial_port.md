---
name: parallel-surface-partial-port
description: The parallel orchestration surface is only partially ported into TaskMaster from drm-copilot; cohort computation never landed in either repo, so parallel-plan cannot reach a ready checkpoint
metadata:
  type: project
---

As of 2026-08-10 the `parallel` orchestration surface in TaskMaster is an **incomplete port** from
the upstream [[drm-copilot-is-claude-governance-upstream]] repo. Verify current state before
relying on this — the gaps are expected to close as the port continues.

**Present in TaskMaster:** `.claude/lib/blast-radius/*.psm1` (PowerShell port, includes
`Test-BlastRadiusConflict`), `.claude/hooks/enforce-parallel-*.ps1`, the six `.claude/skills/parallel-*`
skills, both `.claude/agents/parallel-*.md`, and settings.json hook wiring. The MCP validators
`parallel-planner-state` and `parallel-kickoff` dispatch correctly (verified by probe).

**Missing in TaskMaster:** `config/blast-radius.json`, `.claude/rules/parallel-orchestration.md`,
and `route_id: parallel` in `config/orchestration-routing.json` (routes are only small, large,
remediation, preparation, epic).

**Missing in BOTH repos:** the cohort-computation library (`compute_cohorts` / Welsh-Powell
coloring). `git grep -in "compute_cohorts|welsh"` returns nothing tracked in drm-copilot. The
`feature/parallel-cohort-scheduler-445` branch and its feature folder exist, but no code landed;
the parallel epic merged F7/F8 (hooks, drift, schemas, validators) without it.

**Why:** the parallel surface was built in drm-copilot and is being distributed into consumer repos
like TaskMaster; the port is mid-flight and F2 (cohort scheduler) was never implemented upstream.

**How to apply:** `/parallel-plan` cannot produce a valid ready checkpoint in TaskMaster. Cohort
seeding and the P5 recomputation-parity check both require the absent library, and self-implementing
the coloring would make the parity check compare an implementation against itself. Halt and report
rather than fanning out preparation delegations. Note also that the skill's
`poetry run python -c "from scripts.dev_tools..."` invocation form does not apply here: TaskMaster has
no `scripts/dev_tools/`, no `pyproject.toml`, and no `poetry.lock` — the port is PowerShell.

Also note drm-copilot's `config/blast-radius.json` is **not** reusable in TaskMaster: its modules
(`scripts/dev_tools`, `packages/mcp-server`, `extensions/drm-copilot`) and shared surfaces
(`poetry.lock`, `package-lock.json`) describe drm-copilot's own layout, not a C#/VSTO repo. Copying
it would under-report contention, the opposite of the fail-closed direction the F1a corrections
(issue #452 / PR #453) established.
