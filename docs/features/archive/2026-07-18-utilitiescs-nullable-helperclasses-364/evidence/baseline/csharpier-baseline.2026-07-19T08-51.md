# CSharpier Format Check — Baseline (Issue #364)

- Timestamp: 2026-07-19T08-51
- Task: [P0-T2]
- Command: `csharpier check .` (working equivalent of the plan-literal `dotnet tool run csharpier --check .`)
- EXIT_CODE: 0

## Invocation Note

The plan-literal command is `dotnet tool run csharpier --check .`. Two mechanically-necessary substitutions were applied and recorded:
1. The repo-local .NET SDK is not installed in this worktree (`dotnet tool run` reports "The repo-local .NET SDK is missing"), so the global csharpier tool on PATH is used via pwsh.
2. The installed csharpier is v1.3.0, which uses subcommand syntax (`csharpier check .` / `csharpier format .`); the v0-style `--check` flag is not accepted. `csharpier check .` is the exact v1 equivalent of `--check`.

## Output Summary

- Result: PASS (clean).
- Checked 1406 files in ~2501 ms.
- Unformatted files: 0.
