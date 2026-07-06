---
name: pr-author-hook-blocks-gh-in-this-repo
description: In this repo the enforce-pr-author-skill PreToolUse hook denies every gh pr create because its required python validator module is absent; autonomous PR creation is not possible here
metadata:
  type: project
---

The registered PreToolUse hook `.claude/hooks/enforce-pr-author-skill.ps1` runs a PR-creation preflight whose default `$Invoker` is `python -m scripts.dev_tools.validate_orchestration_artifacts orchestrator-state <checkpoint> --require-pr-creation-ready`. That python package **does not exist** in this repo (`ModuleNotFoundError: No module named 'scripts.dev_tools'`), so the invoker exits 1, `Invoke-OrchestratorStatePreflight` returns `HasErrors=true`, and the hook **denies every** `gh pr create --body-file ...` with `ORCHESTRATOR_STATE_PREFLIGHT_FAILED`. Verified 2026-07-06 by simulating the hook against a well-formed command (pwsh present, hook active).

**Why:** The `.claude` governance bundle was pushed down from a reference repo that has the python `scripts/dev_tools/` tooling; this target repo (TaskMaster) does not. The authoritative validator here is the drm-copilot MCP tool `validate_orchestration_artifacts`, which the hook does not call. There is also no sanctioned autonomous workaround: `gh api` / inline `--body` bypass the mandatory pr-author skill and are prohibited.

**How to apply:** Detect this before the PR gate (run the hook's exact invoker or simulate the hook). When it blocks, resolve via the autonomous-mandate `exception` response: delegate `Agent(human-exception-runbook)` to write a `<FEATURE>/runbooks/*.runbook.md` giving the maintainer two options — (1) provision the missing module or repoint the hook `$Invoker` to the MCP validator, or (2) create the PR manually (base `main`, head the pushed branch) from the feature-folder audit artifacts. Record it in `human_interaction.requirements[]` as `{response: "exception", runbook_path}`; an exception with an existing runbook is DONE-compatible (only `halt` blocks DONE). Do not burn a `pr-author` delegation to hit the wall — the block is deterministic.

**Also note (checkpoint validator):** the MCP `validate_orchestration_artifacts` `orchestrator-state` mode is stricter/legacy (see [[orchestrator-state-validator-divergence]]); it demands `relativeFile`, `long-name`, `work-mode`, `plan-path`, `step7/8/10_status`, and step-status enum `{not-applicable, pending, delegated, verified, blocked}` (not `complete`). Conform to the canonical shape + the real SubagentStop hook rather than chasing that advisory tool.
