# Baseline — CSharpier format verification

- Timestamp: 2026-09-02T01-03
- Issue: #678
- Task: [P0-T5]

Command: `dotnet tool run csharpier check .`

EXIT_CODE: 0

## Final summary line, verbatim

```
Checked 1574 files in 4564ms.
```

This is a read-only check command, so its exit code is a real signal: CSharpier `check`
exits non-zero when any file needs formatting and 0 when none does. The summary line has the
shape of a checked-file count and an elapsed time, which is the shape a clean run prints.

## `R_BASELINE_FORMAT_DRIFT`

The run reported no path as needing formatting. `R_BASELINE_FORMAT_DRIFT` is therefore the
**empty set**, recorded explicitly rather than omitted:

```
R_BASELINE_FORMAT_DRIFT = (empty)
```

CSharpier prints one `Error ---------------------- <path>` block per non-conforming file
before the summary line. The captured output contains no such block, only the summary line,
which is consistent with the exit code of 0.

## Output Summary

`Checked 1574 files in 4564ms.` EXIT_CODE 0. No file needs formatting.
`R_BASELINE_FORMAT_DRIFT` is empty. The baseline tree is already CSharpier-clean, so any
rewrite P2-T1 performs is attributable to this cycle's own edits.
