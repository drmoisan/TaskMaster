---
name: taskmaster-lacks-python-toolchain
description: TaskMaster has no extensions/ and no scripts/dev_tools/, so bugs filed against the drm-copilot MCP extension or the Python validators are not fixable in this repo
metadata:
  type: reference
---

Verified 2026-08-21 on `main @ 7a9ba612`.

TaskMaster is a C#/PowerShell repository. It has **no `extensions/` directory** and **no
`scripts/dev_tools/`**. `scripts/` contains only `dev-tools/` (hyphen), `vscode/`, and
`temp-extract-coverage.ps1`.

Consequences when triaging issues for any orchestration surface:

- A bug filed against an `mcp__drm-copilot__*` tool is a bug in the **drm-copilot** repository, not
  in TaskMaster. Observed cases: `collect_pr_context` misclassification (#513),
  `potential_to_issue` promoted-copy loss (#554), `run_poshqc_test` zero-coverage capture (#536).
  These cannot be planned or fixed here.
- The Python reference implementations the parallel and blast-radius skills cite
  (`scripts/dev_tools/compute_blast_radius.py`, `parallel_cohort_computation.py`,
  `parallel_manifest_contract.py`, `parallel_lane_assertion.py`) **do not exist in TaskMaster**. Use
  the destination-runtime ports only: `.claude/lib/blast-radius/BlastRadius.psm1` (PowerShell) and
  `.claude/lib/bash/*.sh`. Any skill step that says `poetry run python -m scripts.dev_tools...` is
  unrunnable here — including the advisory lane-assertion diagnostic the `parallel-plan` skill marks
  as REQUIRED in its completion report.
- Issue #555 documents this same gap from the hook side: `.claude/hooks/validate-orchestrator-output.ps1`
  shells out to a Python module that is absent.

**How to apply:** before accepting an issue into a TaskMaster run, confirm the code it names is
actually in this checkout. See [[drm-copilot-upstream]] for the governance upstream, and
[[bug-corpus-is-quickfiler-concentrated]] for the triage pass where this surfaced.
