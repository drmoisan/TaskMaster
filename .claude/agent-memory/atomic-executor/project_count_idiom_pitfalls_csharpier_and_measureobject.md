---
name: count-idiom-pitfalls-csharpier-and-measureobject
description: Two measurement idioms that silently report the wrong number in plan gates - CSharpier's "Formatted N files" is a processed count not a rewrite count, and Measure-Object -Line omits blank lines
metadata:
  type: project
---

Two counting idioms routinely used in atomic-plan acceptance gates report a different quantity than the gate assumes. Both were hit on #445 and both would have produced a wrong verdict if taken at face value.

**1. `csharpier format` prints a PROCESSED count, not a REWRITE count.**
`dotnet tool run csharpier format <5 files>` prints `Formatted 5 files in 2054ms.` even when it rewrote **zero** of them. A plan task that says "record how many of the five files the formatter rewrote" and that triggers a phase restart on a non-zero count will restart forever if it reads that line as the answer.
Measure the rewrite count directly:
```powershell
$before = @{}; foreach ($f in $files) { $before[$f] = (Get-FileHash -LiteralPath $f -Algorithm SHA256).Hash }
& $dotnet tool run csharpier format @files
foreach ($f in $files) { if ((Get-FileHash -LiteralPath $f -Algorithm SHA256).Hash -ne $before[$f]) { $rewritten++ } }
```
`csharpier check .` remains the right read-only verdict (`Checked 1517 files in ...` plus exit 0 means zero need formatting; non-conforming files are printed one per line before the summary).

**2. `Measure-Object -Line` does NOT count blank lines.**
`(Get-Content -LiteralPath $f | Measure-Object -Line).Lines` returned 86 / 84 / 84 / 17 / 143 for files whose true physical line counts are 95 / 99 / 99 / 18 / 168. Every figure is understated, by exactly the blank-line count. A file-size audit against a 500-line cap using this idiom under-reports and can pass a file that actually violates the cap.
Use `(Get-Content -LiteralPath $f).Count`, and cross-check with `wc -l`. The two agreed exactly on all five files.

Critically: baseline and final MUST use the same idiom, or the before/after comparison is incommensurable. The Uniform Count Idiom for `git grep` (`(git grep -n -F 'TOKEN' -- 'PATHSPEC' | Measure-Object -Line).Lines`) is fine because `git grep` never emits a blank line.

**Why:** Both idioms look authoritative and produce a plausible number, so neither failure announces itself. The CSharpier one caused a false restart signal; the Measure-Object one silently understated every file-size baseline.

**How to apply:** When a plan gate asks "how many files did the formatter change", hash before and after. When a plan gate asks for a file line count, use `(Get-Content).Count` and record the counting method in the baseline artifact so the final-QC task reproduces it. Related: [[project_csharpier_pipefiles_nonenforcing_gate]], [[feedback_verify_line_citations_with_numbered_output]].
