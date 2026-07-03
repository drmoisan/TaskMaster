# CSharpier Baseline (Issue #232)

Timestamp: 2026-07-03T11-27

Command: `csharpier check .`

Tooling note: The repo-local .NET SDK (`.dotnet-sdk`, per `global.json` 8.0.205) is not installed
in this worktree, so `dotnet tool run csharpier` is unavailable. The globally installed CSharpier
tool (`C:\Users\DanMoisan\.dotnet\tools\csharpier`, version 1.3.0) is used instead, which CLAUDE.md
explicitly approves (`csharpier .`). CSharpier v1 uses the `check`/`format` subcommands (the older
`csharpier . --check` positional/flag form is not valid in v1).

EXIT_CODE: 0

Output Summary: `Checked 1232 files in 1782ms.` All C# files are already CSharpier-formatted at
baseline; zero files require formatting. Formatting is clean.
