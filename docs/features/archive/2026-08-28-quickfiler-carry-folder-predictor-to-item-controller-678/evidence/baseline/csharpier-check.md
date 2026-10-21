# Phase 0 — baseline format verification (P0-T5)

Timestamp: 2026-09-01T21-30

Command: `dotnet tool run csharpier check .`
EXIT_CODE: 0

Output Summary:

The run produced exactly one non-empty output line. Reproduced verbatim:

```
Checked 1567 files in 4815ms.
```

The run reported **no** path as needing formatting. CSharpier emits one
`Error ./<path> - Was not formatted.` block per drifting file before its summary line; the captured
output contains no such block and no path of any kind.

## BASELINE_FORMAT_DRIFT

```
(empty set)
```

`BASELINE_FORMAT_DRIFT` is the empty set: zero files needed formatting at the base ref. It is
recorded here explicitly, as the plan requires, rather than omitted because it is empty.

This is a read-only check command. Its exit code is a real signal: `csharpier check` exits 1 when
any file needs formatting and 0 when none does, so the observed `EXIT_CODE: 0` distinguishes a
clean tree from a drifting one and is not the constant-0 outcome a write-mode command would give.
The file-count line is recorded alongside the exit code so that a run that checked zero files
(which would also exit 0) is distinguishable from this one, which checked 1567.
