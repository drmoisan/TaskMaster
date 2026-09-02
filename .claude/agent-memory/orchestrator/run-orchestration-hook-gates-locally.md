---
name: run-orchestration-hook-gates-locally
description: The PR-creation and completion gates are importable PowerShell functions, so you can run the exact hook check locally instead of guessing at a denial; the MCP validator is a different implementation and disagrees on the bug route.
metadata:
  type: reference
---

Two functions reproduce the real gates exactly. Both take `-CheckpointPath`, so they work
against a child-scoped copy as well as the canonical one.

```
Import-Module ./.claude/lib/orchestrator-state/OrchestratorState.psm1 -Force
Invoke-OrchestratorStatePreflight -CheckpointPath 'artifacts/orchestration/orchestrator-state.json'
# -> .HasErrors / .ErrorText ; this is what enforce-pr-author-skill.ps1 runs internally

Import-Module ./.claude/lib/orchestrator-state/OrchestratorStateCompletion.psm1 -Force
Test-OrchestratorStateCompletionReadiness -CheckpointPath '<path>'
# -> a hashtable; pipe through ConvertTo-Json, since it renders as
#    "System.Collections.Hashtable" otherwise. ExitCode 0 and empty Output means pass.
```

**Why:** on 2026-08-29 the PR hook would have denied `gh pr create` with an opaque
`ORCHESTRATOR_STATE_PREFLIGHT_FAILED`. Running the function directly named the cause in one
shot — `step7_status is pending`, `step8_status is pending` — instead of costing a blocked
call per guess. `Invoke-OrchestratorStatePreflight` requires steps 5 through 8 to be
non-pending; `not-applicable` is an accepted value for a step you legitimately skipped, such as
step 7 when review returned zero blocking findings.

**How to apply:** run the preflight before every `gh pr create`, and the completion function
before declaring done. Treat the PowerShell result as authoritative: `validate-orchestrator-output.ps1`
runs `Test-OrchestratorStateCompletionReadiness` at SubagentStop, and nothing runs the MCP
validator's completion path.

**The two implementations disagree on one cell.** With `promotion-type: "bug"` the portable
gate substitutes the bug-variant potential-entry tool name into `required_mcp_tools` and the
MCP validator does not, so no single checkpoint satisfies both:

- bug-variant recorded -> PowerShell `ExitCode 0`; MCP fails with `required_mcp_tools must match
  routing matrix` plus a missing receipt for the feature-variant name.
- feature-variant recorded -> the reverse.

Conform to PowerShell and disclose the divergence. Do not fabricate a second receipt to make
the MCP call go green. Filed as issue 701.

Related: [[orchestrator-state-validator-divergence]],
[[portable-completion-gate-allows-blocked-child]],
[[completion-gate-receipt-shapes]],
[[shared-checkpoint-read-modify-write-corrupts]]
