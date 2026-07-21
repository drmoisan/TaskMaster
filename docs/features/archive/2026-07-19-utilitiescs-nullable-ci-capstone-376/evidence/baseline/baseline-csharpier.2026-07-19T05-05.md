# Baseline — csharpier check

Timestamp: 2026-07-19T05-05
Command: `csharpier check .` (global tool v1.3.0; `dotnet csharpier check .` fails locally with a
missing-repo-local-SDK error, so the globally installed `csharpier` binary was used instead, per
`.claude/rules/csharp.md`'s alternative approved command `csharpier .`)
EXIT_CODE: 0
Output Summary: `Checked 1406 files in 3162ms.` Zero unformatted files found on the unmodified
branch head; no `.cs` file is touched by this baseline task.
