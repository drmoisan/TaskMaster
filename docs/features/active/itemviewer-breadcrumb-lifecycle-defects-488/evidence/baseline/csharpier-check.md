# Phase 0 — Baseline Formatting State ([P0-T9])

Timestamp: 2026-08-28T05-12

Command: `dotnet tool run csharpier check .`, run from the worktree root. Read-only; this command
verifies and reports and does not rewrite any file.
EXIT_CODE: 0

## Complete output

```
Checked 1553 files in 4547ms.
```

## BASELINE UNFORMATTED SET

```
(empty)
```

**The baseline unformatted-file set is empty.** CSharpier checked 1553 files and reported no file as
unformatted. An empty list is an explicitly valid result for this task, and it is the strongest form
of the baseline: it means `[P8-T2]`'s acceptance reduces to the plain `EXIT_CODE: 0` branch, and the
`[P9-T6]` / `[P9-T15]` authorized-exception branch for a pre-existing unformatted file is **not**
triggered. All 54 criteria therefore remain reachable, including the one beginning
"`dotnet tool run csharpier check .` reports no formatting differences".

## How this artifact is used later

`[P8-T2]` re-runs the identical read-only command from the worktree root and compares its reported
unformatted-file set against the set recorded above. Because the set recorded here is empty, the
comparison in `[P8-T2]` succeeds only when the post-change run also reports an empty set — that is,
only when none of the seven files this feature writes was left unformatted and no other file was
disturbed. That is a strictly stronger condition than a non-empty baseline would have imposed.

A count of files processed is deliberately not treated as the result signal here. `Checked 1553 files`
is the tool's throughput line and is printed on a clean run and on a dirty run alike; the result
signal is the exit code together with the absence of any per-file unformatted report in the output.

Output Summary: EXIT_CODE 0. 1553 files checked, **zero** unformatted. The baseline unformatted set is
empty and is the comparison basis for `[P8-T2]`.
