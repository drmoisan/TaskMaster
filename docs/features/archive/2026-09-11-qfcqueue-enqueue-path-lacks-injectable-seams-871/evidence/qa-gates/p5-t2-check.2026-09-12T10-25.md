# P5-T2 — Repository-wide CSharpier check, final QC loop

Timestamp: 2026-09-13T16-48
Command: dotnet tool run csharpier check .
EXIT_CODE: 0
LoopPass: 1

The command was run while this item held the shared cross-item build lock, which was acquired
immediately before it and released immediately after it returned.

## CMD-CHECK output, verbatim

```
Checked 1632 files in 4457ms.
CHECK-EXIT: 0
```

## Interpretation

CSharpier 1.2.6 in check mode prints one line naming each file it would reformat and exits non-zero
when it names any. This run named no file and exited 0, so no file anywhere in the tree is drifting
from the formatter's output after P5-T1.

The inspected-file count of 1632 agrees exactly with the file count P5-T1 reported as formatted, which
confirms that the check pass covered the same file set the format pass covered and that no file was
added to or removed from the tree between the two commands.

This is the read-only CI-parity verification of the write-mode command in P5-T1. It is recorded as a
separate observation because a formatter's exit code is identical on a clean run and on a repairing run,
so P5-T1's exit code alone does not establish that the tree is now clean; this command does.

Output Summary: The repository-wide CSharpier check exited 0 after inspecting 1632 files and named no
drifting file. The inspected-file count matches the 1632 files P5-T1 formatted. The tree is
CSharpier-clean. Acceptance met.
