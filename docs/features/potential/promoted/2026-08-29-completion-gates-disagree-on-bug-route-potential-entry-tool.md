# completion-gates-disagree-on-bug-route-potential-entry-tool (Issue #701)

- Date captured: 2026-08-29
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/completion-gates-disagree-on-bug-route-potential-entry-tool/ (Issue #701)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #701
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/701
- Last Updated: 2026-08-29
## Summary

The two orchestrator-state completion gates disagree about one entry of `required_mcp_tools` on a bug
route. The portable PowerShell gate applies a bug-route substitution and demands the bug-variant
potential-entry tool name; the MCP validator does not apply it and demands the feature-variant name.
No single checkpoint can satisfy both.

## Environment

- OS/version: Windows 11
- Python version: not applicable; the two implementations are PowerShell and the bundled MCP validator
- Command/flags used: `Test-OrchestratorStateCompletionReadiness` from `.claude/lib/orchestrator-state/OrchestratorStateCompletion.psm1`, versus the MCP orchestration-artifact validator with `require_complete` and `require_model_routing`
- Data source or fixture: any `artifacts/orchestration/orchestrator-state.json` whose `promotion-type` is `bug` and whose `route_id` is `large`

## Steps to Reproduce

1. Author a complete orchestrator-state checkpoint for a bug on route `large`, satisfying every other completion requirement.
2. Set `required_mcp_tools` to the routing-matrix list with the bug-variant substitution applied, and record a matching successful receipt.
3. Run the portable PowerShell readiness function. It returns exit code 0.
4. Run the MCP validator with `require_complete`. It fails.
5. Reverse the substitution and re-run both. The results invert.

## Expected Behavior

Both gates apply the same bug-route rule, so one checkpoint satisfies both. The PowerShell module
header describes itself as a row-by-row port of the validator surface, so the two are intended to
agree.

## Actual Behavior

With the substitution applied, the PowerShell gate returns `ExitCode 0` and the MCP validator returns:

```
Checkpoint required_mcp_tools must match routing matrix for route large.
Checkpoint missing successful MCP receipt: new_potential_entry.
```

Without the substitution, the PowerShell gate returns:

```
Checkpoint required_mcp_tools must match routing matrix for route large.
```

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: both outputs are quoted verbatim in Actual Behavior above.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium rather than High: the authoritative gate is the PowerShell one, since that is what
`.claude/hooks/validate-orchestrator-output.ps1` runs at SubagentStop. An orchestrator that conforms
to it completes correctly. The cost is that the MCP validator cannot be used as a green pre-check on
a bug route, so an agent that trusts it will either loop or fabricate a receipt to satisfy it.

## Suspected Cause / Notes

`Get-OrchestratorStateRoutingContractError` substitutes every `new_potential_entry` occurrence in the
route's `required_mcp_tools` with `new_potential_bug_entry` when `promotion-type` is exactly `bug`,
preserving matrix order, and the substituted list drives both the exact-match check and the
receipt-presence loop. The MCP validator appears to compare against the unsubstituted matrix from
`config/orchestration-routing.json`.

Only the one element differs. Every other completion requirement (`pr_gate`, `ci_gate` including its
`verified_at`, the agent receipts, `skill_receipts` needing `skill` plus `required: true` plus a
non-blank `evidence`, `mcp_call_receipts` needing `tool` plus `ok: true` plus a non-blank `evidence`,
and the empty `local_execution_overrides` and `delegation_bypasses` lists) is evaluated identically by
both.

Observed during the issue 638 child-orchestrator run on 2026-08-29.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: a parity test that runs both gates over the same bug-route fixture and asserts equal verdicts; extend it to the `feature` route so the substitution is pinned in both directions.
- [ ] Integration scenario to retest: a full bug-route orchestration completing with one checkpoint that both gates accept.
- [ ] Manual verification notes: decide which side is correct. The bug-variant name is what a bug route actually invokes, so substituting looks like the intended behavior and the MCP side looks like the defect.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch

Origin: observed during the issue 638 run. Proposed labels: bug, orchestration, tooling, follow-up.
