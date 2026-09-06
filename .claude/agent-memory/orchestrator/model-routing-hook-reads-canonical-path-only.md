---
name: model-routing-hook-reads-canonical-path-only
description: enforce-model-routing-receipt.ps1 runs from the SESSION ROOT copy; in parallel/epic mode it resolves the item checkpoint only when the DELEGATION PROMPT carries Parallel mode + issue_num, else it silently reads a sibling's file
metadata:
  type: project
---

`.claude/hooks/enforce-model-routing-receipt.ps1` reads the checkpoint through
`Get-ModelRoutingCheckpoint`, whose `$CheckpointPath` **defaults to the relative literal
`artifacts/orchestration/orchestrator-state.json`**, resolved against the hook process cwd.
Gated subagents: `atomic-planner`, `atomic-executor`, `feature-review`, `task-researcher`,
`prd-feature`, `pr-author`.

## The three traps, in the order they bite (all verified 2026-09-03, item #733)

**1. The hook that runs is the SESSION ROOT copy, not your worktree's copy.**
Diagnosing by reading `<your-worktree>/.claude/hooks/enforce-model-routing-receipt.ps1` is
misleading: that copy can lag the session root's. On #733 the worktree copy had no parallel
resolution at all while the session-root copy had the full fix, so the worktree copy said
"unfixable" and the executing hook said otherwise. **Always read
`<SESSION-ROOT>/.claude/hooks/...` when diagnosing a hook block.**

**2. Parallel/epic resolution keys off the DELEGATION PROMPT, not the checkpoint.**
The fixed hook selects the run checkpoint from a literal `Parallel mode: true` or
`Epic mode: true` marker **in the prompt text**, then locates the item in the run
checkpoint's `items` (parallel) or `features` (epic) collection by feature-folder basename
or issue number, and finally reads `<item-worktree>/artifacts/orchestration/orchestrator-state.json`.
Put these at the TOP of every delegation prompt:

```
Parallel mode: true
parallel_slug: <slug>
issue_num: <N>
Feature folder: docs/features/active/<folder>
```

Omit them and the hook falls through to the session-root default — **which is whatever
sibling item currently owns that file.** On #733 that was sibling 729, so several
`atomic-executor` delegations were admitted against *729's* receipts. The gate is
presence-only (agent-name match, no item/phase/path binding), so this fails OPEN and looks
like success. A delegation that "worked" proves nothing about your own receipt.

**3. Invalid JSON is treated as ABSENT, silently.**
`Get-ModelRoutingCheckpoint` wraps `ConvertFrom-Json` in try/catch and returns `$null` on
failure — indistinguishable from a missing file. A single missing comma from a hand-`Edit`
of the checkpoint therefore reads as "no receipt recorded" and blocks every gated
delegation with a message that points at the receipt rather than at the syntax error.

**How to apply:** after ANY hand-edit of `orchestrator-state.json`, validate it before the
next delegation:

```
pwsh -NoProfile -Command '$j = Get-Content -LiteralPath <abs> -Raw | ConvertFrom-Json; "OK"'
```

Cheap, and it converts a confusing 3-round block into one command. Do this especially after
appending to `delegation_receipts.agents[]`, which is the array most often extended by hand.

**Diagnostic ladder when you see `MODEL_ROUTING_RECEIPT_BLOCKED`:** (a) does the error name
your item worktree path? If it names no path, your prompt is missing the parallel markers.
(b) If it names your path, validate the JSON. (c) Only then check whether the receipt is
genuinely absent.

Note the hook is presence-only — it never checks the recorded `model`. The MCP validator
with `require_model_routing: true` is the correctness gate. See
[[orchestrator-state-flat-keys-and-enum]] for the receipt field set,
[[agent-worktree-hooks-resolve-to-agent-cwd]] and
[[child-orchestrator-pr-hook-reads-session-root]] for the sibling cwd-resolution problems,
and [[shared-checkpoint-read-modify-write-corrupts]] for why mirroring into the session-root
file is the wrong repair in parallel mode.
