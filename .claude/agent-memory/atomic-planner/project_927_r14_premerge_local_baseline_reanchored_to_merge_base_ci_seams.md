---
name: project-927-r14-premerge-local-baseline-reanchored-to-merge-base-ci-seams
description: Issue #927 round 14 (revision 1.17) seams - a Phase 0 local coverage baseline is voided by a mid-plan merge of main (main itself moved 0.01 on both percentages); re-anchor to main's own CI run at the merge base and compare CI against CI with the stale local pair as the negative control; vstest green-run summary shape; pwsh function output-stream trap; two coordinator-delta clauses that conflicted with plan invariants
metadata:
  type: project
---

Round 14 on #927 (2026-09-30) appended P6-T39 after execution of revision 1.16 recorded the C# coverage gate NOT MET by 0.01 on both percentages. Four seams.

**1. A pre-merge local baseline cannot gate a post-merge run.** P0-T14 measured 85.93/80.09 before the branch merged origin/main; P6-T9 ran after the merge and read 85.92/80.08 with the same denominators and test count as main's own CI mstest job at the merge base (run 36651909330). The 0.01 was main's movement (sibling items added tests), not the branch's.
**Why:** a coverage baseline describes a tree, and the R8 memory already says a mid-run merge voids BASE-SHA diffs; it voids coverage baselines the same way.
**How to apply:** when a plan directs a merge of main before Phase 6, source the coverage baseline from main's CI run at the merge base (`gh run list --branch main --commit <merge-base> --workflow CI --event push`, completed + success), read the post-change figures from the PR head's CI run, compare CI against CI, and keep the local run for reference. The negative control that shows the comparator can fail is the local figures against the stale local baseline, parsed and compared by the same functions (CONTROL-MSTEST-COMPARE=BELOW).

**2. vstest all-green summary shape (verified in the 7374-line local log).** Exactly one `Total tests: N` line, one `     Passed: N` line, no `Failed:` line, and no per-assembly `Passed! - Failed: 0, Passed: N` lines, so `Passed: [0-9]+` selects one line. Use `-cmatch`/`-clike` (case-sensitive): the plan's own artifacts carry `BASELINE-PASSED: 7343` and `BASELINE-FAILED: 0`, which a case-insensitive `Passed: [0-9]+` reads as figures. Derive failed-zero as "no Failed: line or Failed: 0, and Total equals Passed" so it can fail. Filter `gh run view --log` lines by the job-name prefix (`mstest-coverage / *`) to keep other jobs out.

**3. pwsh -Command function output trap.** A function that prints labelled lines and then `return $h` returns the strings AND the hashtable, so `$x = Get-Figures ...` captures an array. Split into a pure Get-Figures (returns only the hashtable, always, with an Ok flag) and a Show-Figures that prints; Compare-Figures reads the Ok flags and returns NOT-OBSERVED/BELOW/NOT-BELOW.

**4. Coordinator delta clauses that conflicted with invariants.** (a) "commits ... and pushes" conflicted with C9 / P6-T37 / P6-T38 / the P6-T14 NO-FORCE-PUSH record (executor never pushes); applied as commit-only and recorded as a deviation for reversal. (b) "add to C12 only if a new token" conflicted with C12's sentence "No other task stops"; named the task beside P6-T38 with the token set unchanged and recorded it. Also: the formatting-contract note still said the plan is LF while every recent count was CRLF - sweep such stale self-descriptions when the closing paragraph contradicts them.

Related: [[project-927-r13-delta-text-violates-own-check-and-quoted-phrase-residue]], [[project-927-r12-three-dot-anchor-and-control-ref-ancestry-seams]], [[project-927-r8-powershell-batch-phases-and-post-merge-reanchor-seams]].
