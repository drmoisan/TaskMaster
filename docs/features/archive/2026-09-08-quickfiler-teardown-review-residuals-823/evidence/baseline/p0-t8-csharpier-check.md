# Phase 0 — CSharpier baseline (read-only check)

Timestamp: 2026-09-09T13-52

Task: [P0-T8]

Command: `dotnet tool run csharpier check .`

EXIT_CODE: 0

Verbatim printed line:

```
Checked 1622 files in 4183ms.
```

BASELINE-CSHARPIER-CHECKED-FILES: 1622

`check` is read-only and exits non-zero on drift, so exit 0 together with the single `Checked` line
is the clean-tree observation. No pre-existing drift set was recorded, because there was none: the
non-zero branch of this task was not taken, so no formatter pass repaired pre-existing drift ahead
of this baseline and the later [P6-T2] comparison is neither a blanket waiver nor unsatisfiable.

Output Summary: Read-only formatter verification passed on the unmodified tree. 1622 files checked,
exit code 0, zero drifting paths.
