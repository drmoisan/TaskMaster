---
name: completion-gate-receipt-shapes
description: The exact receipt shape the routing-contract completion gate wants - the missing key was always `evidence`; also the bug-promotion tool-name swap and the unresolvable MCP-vs-hook divergence it creates
metadata:
  type: project
---

**SOLVED 2026-08-22 (epic child #445).** The shape that earlier runs could not discover by guessing is
readable in `.claude/lib/orchestrator-state/OrchestratorStateRoutingContract.psm1`
(`Get-CheckpointAcknowledgedName`, ~line 225). Read it; do not guess.

A receipt counts **only when all three hold together**:

- `skill_receipts[]` — `skill` non-blank string, `required` **boolean** `true`, **`evidence` non-blank string**.
- `mcp_call_receipts[]` — `tool` non-blank string, `ok` **boolean** `true`, **`evidence` non-blank string**.

**`evidence` is the key that was always missing.** Earlier attempts tried `outcome`, `detail`,
`skills_used`, `mcp_receipts`, `mcp_tool_calls` and failed, because the harvest reads a
hard-coded `'evidence'` member regardless of the array. Truthy-but-not-`$true` flags
(`1`, `"true"`) deliberately do not count.

`delegation_receipts` is separate. Two forms are accepted (`OrchestratorStateReceipts.psm1`): a bare
LIST, or an OBJECT namespace whose only permitted keys are `agents` (a list) and `promotion` (an
object with only `potential_entry` / `issue` / `feature_folder`). The object form is what lets a run
carry BOTH the promotion receipts the orchestrator agent spec demands and the routing-contract
`agent_name` list — use it.

Each entry in the list (or in `agents`) needs **exactly these eight keys**, confirmed 2026-08-26 by
reading the MCP validator's own error output on epic child #468:
`agent_name`, `step`, `agent_id`, `skill_source`, `started_at`, `completed_at`, `result_signal`,
`artifact_paths`. There is **no `phase` key** (an earlier note here claimed one; it is wrong).

**Only record agents THIS run delegated.** Naming an upstream-prepared agent (`task-researcher`,
`prd-feature`, `atomic-planner` from an epic-planner preparation run) in `agents` immediately forces
`model_routing_receipts is missing a receipt for delegated agent: <name>` under
`require_model_routing` — and you would have to invent a model choice you never made. Leave
`agents: []` until your first delegation returns, and record the upstream work in `notes` and
`delegation_receipts.promotion` instead.

### Bug-promotion tool-name swap, and the divergence it creates

`Get-ResolvedRequiredMcpTool` swaps `new_potential_entry` for **`new_potential_bug_entry`** when the
checkpoint's promotion type is `bug`. The MCP TypeScript surface does **not** apply that swap.
Because `required_mcp_tools` is checked by EXACT LIST EQUALITY, the two demands are mutually
exclusive on a bug route and **no single list satisfies both**.

**Conform to the HOOK** (`new_potential_bug_entry`). It is the mechanism that actually blocks, and it
is semantically right for a bug. Expect the MCP `require_complete` call to keep reporting exactly two
residual errors; they are divergence artifacts, not missing work.

**Why:** this supersedes the pessimistic conclusion in
[[orchestrator-state-validator-divergence]] that `--require-complete` is simply unsatisfiable for a
resumed-at-execution epic child. It is satisfiable *at the real gate*
(`.claude/hooks/validate-orchestrator-output.ps1` returned `EXIT_CODE=0`); only the MCP surface stays
unsatisfiable, and only on the two swap-related errors.

**How to apply:** when the completion gate reports missing skill or MCP receipts, add `evidence` to
every receipt before changing anything else. Prove the result with the hook, invoking it as
`$env:CLAUDE_HOOK_INPUT = '{"output":"<summary>"}'` (top-level `.output`) and asserting `EXIT_CODE=0`
— not with the MCP tool. See [[orchestrator-state-flat-keys-and-enum]] and
[[epic-child-self-merge-step9-passed-vs-verified]].
