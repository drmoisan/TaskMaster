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

The two timestamps are enforced per-entry (`delegation receipt #N missing key: started_at`), so a
retrospectively recorded preparation-run receipt still needs them — supply the artifact-derived
preparation window and say so in a `timestamp_basis` note rather than omitting them.

**Only record agents THIS run delegated.** Naming an upstream-prepared agent (`task-researcher`,
`prd-feature`, `atomic-planner` from an epic-planner preparation run) in `agents` immediately forces
`model_routing_receipts is missing a receipt for delegated agent: <name>` under
`require_model_routing` — and you would have to invent a model choice you never made. Leave
`agents: []` until your first delegation returns, and record the upstream work in `notes` and
`delegation_receipts.promotion` instead.

### Every agent you DO name needs routing + complexity entries

`--require-model-routing` builds the delegated-agent set from `delegation_receipts[].agent_name`, so
every agent named there must also carry a `model_routing_receipts[]` entry AND a
`complexity_assessments[]` entry for each phase those receipts name. That coupling is the mechanism
behind the rule above. Under `fable_policy: preferred` the C3 cell is `fable` for the four overlay
agents (`atomic-planner`, `prd-feature`, `feature-review`, `task-researcher`) and stays `opus` for
`atomic-executor` and `pr-author`.

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

### The MCP validator wants the OPPOSITE shape from the PowerShell contract (verified 2026-08-27, epic child #493)

The two validators disagree structurally. Do not try to satisfy both with one shape.

- The PS routing contract above wants `delegation_receipts` as a **LIST** of per-agent receipts
  (`agent_name`, `started_at`, …).
- `mcp__drm-copilot__validate_orchestration_artifacts --artifact-type orchestrator-state` wants
  `delegation_receipts` as an **OBJECT whose only accepted key is `promotion`**. It fails closed both
  ways: omit the key entirely and you get `Checkpoint missing required key: delegation_receipts`; add
  `atomic_planner` / `atomic_executor` / `feature_review` / anything else and each is rejected as
  `delegation_receipts object contains unsupported key: <name>`.

Workaround that passed: keep `delegation_receipts` as `{"promotion": {...}}` only, and move the
per-phase receipts to a sibling top-level key (`phase_delegations`) with a one-line note saying why.
Plain validation plus `--require-model-routing` then returns `ok:true`.

### `--require-complete` is NOT the operative gate for an epic child

It demands the full route-large matrix: agent receipts for `task-researcher`, `prd-feature`,
`atomic-planner`, `atomic-executor`, `feature-review` AND `pr-author`; six named skill receipts;
successful MCP receipts for `new_potential_entry`, the potential-promotion tool,
`new_active_feature_folder`, `collect_pr_context`, `validate_orchestration_artifacts`; `pr_gate`
carrying `pr_number` + `pr_url`; `ci_gate` carrying `verified_at`; and `local_execution_overrides` +
`delegation_bypasses` present as empty lists.

**An epic child cannot satisfy it honestly.** Two structural reasons:

1. A child whose issue already existed must NOT call the promotion tool (no idempotent path — it
   always creates a duplicate issue), so the two promotion MCP receipts can never be truthfully
   produced. Do not repurpose a follow-up promotion's receipts to fill them; label them with their
   real subject.
2. A resumed child did not itself run the research / spec / planning phases — the epic-planner
   preparation run did. Re-attributing them to the resume session would be false.

**Proof it is inoperative, not merely hard:** run the gate against an already-MERGED sibling's
archived checkpoint. Child #442 (merged as PR #649) fails `--require-complete` with a
**byte-identical** error list to #493's. No child in the epic has ever passed it.

The gates that actually govern an epic child are `enforce-pr-author-skill.ps1` (PreToolUse, on PR
creation) and `enforce-epic-merge-gate.ps1` (on PR merge) — both of which pass on the plain schema.
Record a `completion_gate_divergence` block explaining the refusal and move on; never fabricate a
receipt to clear it.
