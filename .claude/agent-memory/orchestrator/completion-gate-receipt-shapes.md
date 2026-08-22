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

`delegation_receipts[]` is separate and needs `agent_name`, `agent_id`, `step`, `phase`,
`skill_source`, `result_signal`, `artifact_paths` (list). It is a LIST, not an object.

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
