---
name: batch-budget-hook-discards-out-of-root-powershell-writes
description: enforce-powershell-batch-budget.ps1 roots itself at the SESSION worktree, so Write/Edit of a .ps1 into a different execution worktree is discarded - no slot consumed, no state file written - making any plan gate that reads prodFiles/testFiles unsatisfiable
metadata:
  type: project
---

`.claude/hooks/enforce-powershell-batch-budget.ps1` computes
`$Root = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent`, and `.claude/settings.json`
registers it with the **relative** command
`pwsh -NoProfile -File .claude/hooks/enforce-powershell-batch-budget.ps1`. The relative path
resolves against the Claude Code project directory, i.e. the **session** worktree. So `$Root` is the
session worktree even when all the work happens in a different execution worktree.

`Invoke-PowerShellBatchBudgetDecision` lines 277-282 then **discard** an out-of-root candidate
rather than denying it: decision `allow`, no slot consumed, `shouldWriteState = $false`. The
containment test (lines 82-92) admits any *relative* path but requires an absolute path to equal or
be prefixed by `$Root`, and the `Write` tool always supplies an absolute path.

**Why:** a plan that splits PowerShell work into batches and then asserts batch membership by
reading the `prodFiles` / `testFiles` arrays out of
`.claude/state/powershell-batch-budget.<session-id>.json` gets empty arrays, or no state file at
all, when the files were written into a non-session worktree. The assertion then cannot fail — the
absence-shaped defect. Plan 911 revision 7 built its P2-T9 / P4-T8 / P6-T7 boundary assertions on
exactly that premise, prescribing `Write`/`Edit` over heredocs as the remedy; the remedy is
insufficient because the discard happens for the location, not the tool.

Other measured details worth keeping: the session id is `$env:CLAUDE_SESSION_ID` first, then
`<Root>/.claude/state/current-session-id`, then `worktree-<leaf>-<sha8>`. The hook stores the
absolute supplied `file_path` with `\` normalised to `/`, so membership checks must compare
suffixes. `.claude/state/powershell-batch-budget.default.json` is **tracked in git** and already at
3/3 prod, but it is a different session's file and its three temp-path entries are dropped by the
containment filter on rehydration, so it is inert. Caps default to 3 prod / 3 test.

**How to apply:** before trusting any batch-budget gate, check which worktree the hook is rooted at
and whether the target files are inside it. Confirm empirically at the first PowerShell `Write`: if
no `powershell-batch-budget.<session-id>.json` appears in either worktree's `.claude/state/`, the
discard path is confirmed and the gate is inert. See
[[planner-and-executor-observe-different-worktrees]] and
[[preflight-selfderived-gate-thresholds-are-blind]].
