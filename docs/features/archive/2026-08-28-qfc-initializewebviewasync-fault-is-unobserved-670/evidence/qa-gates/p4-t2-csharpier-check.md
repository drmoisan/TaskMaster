# P4-T2 — Read-only formatting verification

Timestamp: 2026-09-01T20-12
Command: `dotnet tool run csharpier check .`
EXIT_CODE: 0

Output Summary:

    Checked 1567 files in 4577ms.

The command named **no** file as needing formatting. This is the read-only verification mode, so unlike the write-mode `format` invocation in P4-T1 its exit code is a real signal: `check` exits non-zero and prints each offending file, with an expected-versus-actual hunk, whenever a file's on-disk text differs from the formatter's output.

## The gate is demonstrably capable of failing

This same command exited **1** earlier in this delivery run, against the newly authored production file before it was formatted (recorded in `evidence/other/p1-t5-new-file-format.md`):

    Error .\QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs - Was not formatted.

So the exit-0 result here is a genuine pass rather than a gate that cannot fail.

## File count

The count rose from 1566 at the P0-T9 baseline to 1567 here. The difference is exactly one: `QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs`, the single production file this change adds. The count therefore corroborates the changed-file set independently of the diff gates — a second added file would have produced 1568.

CSharpier was invoked through `dotnet tool run`, so the manifest-pinned 1.2.6 was used rather than any globally installed version. A different global version produces diffs that disagree with the pinned version CI runs after `dotnet tool restore`.

Base-ref note: this task states no `git` command. The re-anchored base used throughout this delivery run is `988d35a8f8eb7436cc46a9f6424db917ed93807a`, replacing the plan-pinned `2b85134b42872e405602e6064e02dc9cda6c319b`, which is a stale ancestor rather than the current merge base.
