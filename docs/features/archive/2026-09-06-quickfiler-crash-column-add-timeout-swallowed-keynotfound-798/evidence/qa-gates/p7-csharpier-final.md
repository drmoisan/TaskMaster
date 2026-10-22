# P7-T8 — Final CSharpier format-then-check pass

Timestamp: 2026-09-07T05-37

## Commands

Command: `dotnet tool run csharpier check .` (pre-format discrimination run)
EXIT_CODE: 0
Output Summary: `Checked 1593 files in 6694ms.`

Command: `dotnet tool run csharpier format .`
EXIT_CODE: 0
Output Summary: `Formatted 1593 files in 3035ms.` The number in that line is the count of files
**scanned**, not the count rewritten; csharpier 1.2.6 prints the same shape on a clean tree and on a
repairing one, so this line is recorded for completeness and is not the evidence that the pass wrote
nothing. No task in this plan asserts the literal `Formatted 0 files in`.

Command: `dotnet tool run csharpier check .` (post-format verification run)
EXIT_CODE: 0
Output Summary: `Checked 1593 files in 6872ms.`

## Did the format pass rewrite any file?

**No.**

Two independent observations support that conclusion, and the discriminating one is the pre-format
check:

1. **Pre-format check exited 0.** Every file in the tree already matched formatter output *before*
   `format` ran, so the format pass had nothing to write. This is the discriminating observation. An
   identical before-and-after porcelain comparison alone would not be, because a file already marked
   `M` in the index stays marked `M` when it is rewritten, so porcelain cannot distinguish "not
   rewritten" from "rewritten to a different modified content".

2. **Before-and-after porcelain observations are identical**, and neither contains any tracked C#
   source path.

### git status --porcelain --untracked-files=all -- . ":(exclude).claude" — before the format pass

```
 M docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/plan.2026-09-06T22-00.md
?? docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p7-ac12-inverse-constraints.md
?? docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p7-ac13-compile-entries.md
?? docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p7-ac13-line-cap.md
?? docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p7-ac13-write-set-diff.md
?? docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p7-ac8-timeoutafter-unchanged.md
?? docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/qa-gates/p7-ac9-fixed-arity.md
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
```

Every path in both observations is an item-scoped feature-folder artifact. All sixteen write-set
paths were committed at P7-T1 and none reappears as modified, which is the tree-level confirmation
that the format pass wrote to no source file.

## Consequence for P7-T4

P7-T8's acceptance requires P7-T4 to be re-run only if the format command rewrote a file. It did
not, so the P7-T4 line-cap counts remain valid as recorded. P8-T9 re-verifies them after the final
Phase 8 formatting pass regardless.

## Verdict

Acceptance met: the check command recorded `EXIT_CODE: 0` and its success-case summary line
`Checked 1593 files in 6872ms.` begins with the literal `Checked ` and ends with the literal `ms.`.
