# P7-T1 — Toolchain step 1 (CSharpier format)

Timestamp: 2026-09-08T10-05
Task: [P7-T1]
Command: dotnet tool run csharpier format .
EXIT_CODE: 0
Toolchain pass: 3

REWRITTEN_COUNT: 0

## Why the exit code is not the signal

`csharpier format` exits 0 whether or not it rewrote anything, and its summary line
`Formatted 1616 files in <n>ms.` is a processed-file count, not a rewrite count: it printed 1616
on both passes below, including the pass that rewrote five files and the pass that rewrote none.
The rewrite observation is therefore taken from the tree, as two independent measurements:

1. SHA-256 hashes of the 19 write-set `.cs` files, taken immediately before and immediately after
   the command. `REWRITTEN_COUNT` counts hash differences.
2. `git status --porcelain --untracked-files=all -- "*.cs" ":(exclude).claude"` before and after.
   Any porcelain entry that appears is added to the count. This second observation exists because
   an already-modified file stays marked `M` when reformatted, so porcelain alone cannot see a
   rewrite of a file that was already dirty; the hash comparison is what detects that case.

## Pass 1 (2026-09-08T10-05) — five files rewritten, loop restarted

`REWRITTEN_COUNT: 5`, all detected by hash difference; the porcelain count was 19 before and 19
after, so porcelain alone would have reported nothing. The five:

```
UtilitiesCS/HelperClasses/PrettyPrint.cs
UtilitiesCS.Test/Extensions/DfDeedleEtlTimeoutTests.cs
UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsEtlClockTests.cs
UtilitiesCS.Test/TestHelpers/ArmingBarrierTimeProvider.cs
UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs
```

Three of the five are new files this plan created, which is the expected case: a newly authored
file has never been through the formatter. `PrettyPrint.cs` was reformatted because the `using`
block was reordered by hand. `StackGeek_Tests.cs` was reformatted because the new
`FluentActions.Invoking(...)` chain exceeded the 100-column width and CSharpier broke it across
four lines.

Because `REWRITTEN_COUNT` was greater than 0, the toolchain loop restarted at P7-T1 as the plan
requires.

## Pass 2 (2026-09-08T10-05) — clean

```
REWRITTEN_COUNT: 0
```

No hash differed and no porcelain entry appeared. `FORMAT_EXIT: 0`.

## Pass 3 (2026-09-08T10-14) — clean, after the P7-T7 gap-closure edit

The loop restarted a second time because P7-T6 failed its clause 2 and P7-T7 added two tests. The
format pass over that edit also rewrote nothing:

```
REWRITTEN_COUNT: 0
```

`FORMAT_EXIT: 0`, `PRETTYPRINT_LINECOUNT: 680`. This is the final pass, and the artifact records
its state.

## Line-cap observation

PRETTYPRINT_LINECOUNT: 680

`UtilitiesCS/HelperClasses/PrettyPrint.cs` is at most 680 lines after the pass, as the acceptance
condition requires. It is exactly 680, unchanged from baseline, so CSharpier's own reformatting of
the file did not consume the zero-growth budget D10 established.

## Acceptance evaluation

- `EXIT_CODE: 0`. PASS
- `REWRITTEN_COUNT: 0` on the final pass, defined as hash differences plus appeared porcelain
  entries, and never read from the `Formatted N files` line. PASS
- The loop restarted at P7-T1 when `REWRITTEN_COUNT` was greater than 0, and the final artifact
  shows `REWRITTEN_COUNT: 0`. PASS
- `PrettyPrint.cs` line count is at most 680 after the pass. PASS

## Output Summary

Three format passes. The first rewrote five files (three newly created by this plan, two
reformatted after hand edits) and triggered a loop restart. The second rewrote nothing. The third,
run after P7-T7's gap-closure edit forced a further restart, also rewrote nothing, confirming the
tree is at a CSharpier fixed point. `PrettyPrint.cs` holds at exactly 680 lines throughout.
