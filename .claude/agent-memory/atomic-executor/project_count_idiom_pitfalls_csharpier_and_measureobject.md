---
name: count-idiom-pitfalls-csharpier-and-measureobject
description: Three measurement idioms that silently report the wrong number in plan gates - CSharpier's "Formatted N files" is a processed count not a rewrite count, Measure-Object -Line omits blank lines, and -clike reads a markdown checkbox as a wildcard character class
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

Corollary hit on #285: a plan task can instruct you to record "the `Formatted` summary line CSharpier prints" for a task whose command is `csharpier check .`. Check mode never prints a `Formatted` line — it prints `Checked N files`. Record the line the invoked subcommand actually printed and state in the artifact that check mode uses different wording; do not fabricate a `Formatted` line to satisfy the task text, and do not switch the command to `format` to make the quoted line appear (that would turn a read-only baseline into a writing one).

**2. `Measure-Object -Line` does NOT count blank lines.**
`(Get-Content -LiteralPath $f | Measure-Object -Line).Lines` returned 86 / 84 / 84 / 17 / 143 for files whose true physical line counts are 95 / 99 / 99 / 18 / 168. Every figure is understated, by exactly the blank-line count. A file-size audit against a 500-line cap using this idiom under-reports and can pass a file that actually violates the cap.
Use `(Get-Content -LiteralPath $f).Count`, and cross-check with `wc -l`. The two agreed exactly on all five files.

Critically: baseline and final MUST use the same idiom, or the before/after comparison is incommensurable. The Uniform Count Idiom for `git grep` (`(git grep -n -F 'TOKEN' -- 'PATHSPEC' | Measure-Object -Line).Lines`) is fine because `git grep` never emits a blank line.

**3. `-like`/`-clike` treats a markdown checkbox as a WILDCARD CHARACTER CLASS.**
Counting AC check-offs with `Where-Object { $_ -clike "- [x] *" }` returns **0** on a file where every line matches, because PowerShell wildcards give `[...]` character-class meaning: `- [x] *` means "dash, space, the single character `x`, space, anything". `- [ ] *` is worse — it raises `WildcardPatternException: The specified wildcard character pattern is not valid` once per pipeline item, and if the exception stream is not being read the surviving count still looks like a legitimate 0.
A zero here reads exactly like "no criteria are checked off", which is the answer a Phase 6 verification gate is looking for, so the wrong verdict is the plausible one.
Use a regex instead: `$_ -cmatch '^- \[x\] '`, or the Grep tool with `^- \[x\] ` (case-sensitive by default), which also gives line numbers to confirm the hits fall inside the AC section's line range rather than in an unrelated checkbox list elsewhere in the file (spec.md carries a severity picker at lines 24-27 that inflates a whole-file count by one).

**Why:** All three idioms look authoritative and produce a plausible number, so none of the failures announces itself. The CSharpier one caused a false restart signal; the Measure-Object one silently understated every file-size baseline.

**How to apply:** When a plan gate asks "how many files did the formatter change", hash before and after. When a plan gate asks for a file line count, use `(Get-Content).Count` and record the counting method in the baseline artifact so the final-QC task reproduces it. Related: [[project_csharpier_pipefiles_nonenforcing_gate]], [[feedback_verify_line_citations_with_numbered_output]].
