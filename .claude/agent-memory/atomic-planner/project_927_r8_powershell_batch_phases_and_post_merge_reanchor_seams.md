---
name: project-927-r8-powershell-batch-phases-and-post-merge-reanchor-seams
description: "#927 round 8: the PowerShell batch-budget hook (3 prod / 3 test per SESSION, shared across parallel-run worktrees) forces batch phases with stop-and-request tasks; write-mode PoshQC/Invoke-Formatter loops are hook-avoiding and must become observe-only + Write; a mid-run origin/main merge invalidates BASE-SHA diffs and Phase 0 baselines (re-anchor at run-time merge-base with an ancestry guard, re-capture compared baselines)"
metadata:
  type: project
---

Seams found while revising plan 927 (evidence and identity hygiene sweep) after the PowerShell batch-budget hook denied the first Phase 1 write.

**Hook mechanics that shape any plan writing PowerShell files** (from `.claude/hooks/enforce-powershell-batch-budget.ps1`, header lines 6-45, body lines 279-298, 352, 366):
- Counter is per Claude Code SESSION, persisted under `.claude/state/powershell-batch-budget.<session_id>.json`; parallel-run item worktrees nest under the session root, so the containment filter does NOT separate items — sibling items spend the same 3+3 budget.
- Only Write/Edit tools are observed. A pwsh `WriteAllText` formatter loop, `Set-Content`, a redaction helper, MCP `run_poshqc_format`/`_analyze_autofix` all rewrite `.ps1` files unobserved — the operator ruling (2026-09-29) forbids that; restructure as observe-only (compute formatted text into SCRATCH, report `REWRITE-NEEDED=`/`DIFFERS|`) and apply through Write inside a batch whose budget lists the file.
- Files already counted are always allowed; a file committed by the parent is counted again only when written again in a later batch. Out-of-root candidates (SCRATCH under `$env:TEMP`) consume no slot.
- The executor must never delete/edit `.claude/state/` or set `CLAUDE_POWERSHELL_BUDGET_*`; each batch phase opens with `STOP: POWERSHELL BATCH RESET REQUIRED (batch N)` + a resume task; the first Write of the batch is the reset observation (`STOP: POWERSHELL BATCH WRITE DENIED (batch N)` otherwise).
- Layout used: batch 1 = 3 prod guard scripts + up to 3 hygiene test files (top-ups/format), batch 2 = the one modified helper test, batch 3 = final QA loop over the 6 hygiene files; a QA-loop change to a 7th file is a terminal batch-4 stop, not a write. Make each file formatter-stable in its own batch so the final loop is expected to rewrite nothing.

**Post-merge invalidation:** when the orchestrator merges origin/main after Phase 0, every `git diff BASE-SHA HEAD` bills main's files to the item and every Phase 0 toolchain baseline (csharpier, msbuild warnings, MSTest coverage, Pester pass count/line percent, identifier/raw-document census) describes a superseded tree. Fix: compute `$mb = (git merge-base origin/main HEAD)` inside each payload (diff `$mb HEAD` = the branch's own changes even after re-merges), record `MERGE-BASE-POST-MERGE:` once and gate `git merge-base --is-ancestor <it> $mb` = 0 so a stale local origin/main cannot widen the diff; re-capture the compared baselines into `.post-merge.md` siblings (keep the spec-named stem) and point every consumer at them.

**Why:** the batch cap was the routing signal that moved the item to the large-path orchestration; the merge had changed `Invoke-MSTestWithCoverage.ps1` (entry guard 437->459, defaults 283/284->297/298, report line 388->410, new Scope part file) and the csproj analyzer references, so cited line numbers and the Pester denominator were stale.

**How to apply:** any plan that creates or edits `.ps1/.psm1/.psd1` files in a parallel run needs the C15/C16/C17 conventions (quote the hook header, count every PowerShell file including redaction-sweep candidates — derive it with a grep over `*.ps1,psm1,psd1` for the profile pattern, account, 8.3 form and legacy tokens — and put every write in a budgeted phase). Pester tests committed before the production file pin implementation details: a `Mock Get-Content` + `Should -Invoke ... -Times 1 -Exactly` fixes the default byte reader; a `PSObject.Properties.Name | Should -Be @('LineNumber')` fixes the record shape — read the committed tests before writing the production contract.
