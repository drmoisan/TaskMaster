# P8-T1 — Final QC toolchain step 1 of 4: formatting

Timestamp: 2026-09-07T05-39
Toolchain pass: 1

## Commands

Command: `dotnet tool run csharpier check .` (pre-format discrimination run)
EXIT_CODE: 0
Output Summary: `Checked 1593 files in 6043ms.`

Command: `dotnet tool run csharpier format .`
EXIT_CODE: 0
Output Summary: `Formatted 1593 files in 2904ms.` The count in that line is the number of files
**scanned**, not the number rewritten. csharpier 1.2.6 prints the same line shape on a clean tree and
on a repairing one, so it cannot discriminate between the two and is not used as the rewrite
evidence here.

Command: `dotnet tool run csharpier check .` (post-format verification run)
EXIT_CODE: 0
Output Summary: `Checked 1593 files in 6008ms.`

## Did the format command rewrite any file?

**No.** Before-and-after tree observation follows, with the discriminating pre-format check.

The pre-format check exited 0, which establishes that every file in the tree already matched
formatter output before `format` ran; a write-mode formatter with nothing to repair writes nothing.
This is the load-bearing observation. The porcelain comparison below corroborates it but could not
establish it alone: a file already staged or already marked `M` retains the same porcelain status
when it is rewritten, so identical porcelain output before and after is consistent with both a clean
run and a repairing one.

### git status --porcelain --untracked-files=all -- . ":(exclude).claude" — before the format pass

```
 M docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/plan.2026-09-06T22-00.md
?? docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p7-ac12-inverse-constraints.md
?? docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p7-ac13-compile-entries.md
?? docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p7-ac13-line-cap.md
?? docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p7-ac13-write-set-diff.md
?? docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p7-ac8-timeoutafter-unchanged.md
?? docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p7-ac9-fixed-arity.md
?? docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p7-csharpier-final.md
```

### The same command — after the format pass

```
 M docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/plan.2026-09-06T22-00.md
?? docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p7-ac12-inverse-constraints.md
?? docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p7-ac13-compile-entries.md
?? docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p7-ac13-line-cap.md
?? docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p7-ac13-write-set-diff.md
?? docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p7-ac8-timeoutafter-unchanged.md
?? docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p7-ac9-fixed-arity.md
?? docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p7-csharpier-final.md
```

The two observations are identical, and every path in them is an item-scoped feature-folder
artifact. No source file, no project file and no path outside this feature folder appears in either.
The sixteen write-set paths were committed at P7-T1 and none of them reappears as modified.

## Verdict

Acceptance met: `EXIT_CODE: 0` for the check command, success-case summary line
`Checked 1593 files in 6008ms.` quoted verbatim, beginning with the literal `Checked ` and ending
with the literal `ms.`; and the format command rewrote no file, recorded as a before-and-after tree
observation above. The toolchain loop proceeds to P8-T2 without a restart.
