# CSharpier Format Check Baseline (P0-T2)

- Timestamp: 2026-07-19T08-48
- Command: `dotnet csharpier check .` (repo-local tool manifest csharpier 1.2.6 via repo `.dotnet-sdk`; csharpier v1 uses the `check` subcommand rather than the v0 `--check` flag; global csharpier 1.3.0 requires .NET 10 runtime which is not installed, so the pinned local tool is used)
- EXIT_CODE: 0
- Output Summary: PASS. `Checked 1406 files in 5081ms.` Zero unformatted files. The repository is already CSharpier-clean at baseline.
