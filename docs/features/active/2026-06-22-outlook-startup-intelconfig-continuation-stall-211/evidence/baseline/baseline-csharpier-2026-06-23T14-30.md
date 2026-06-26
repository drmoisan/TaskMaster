# Baseline — CSharpier (#211 Phase 3)

Timestamp: 2026-06-23T14-30
Command: `dotnet tool run csharpier check .`
EXIT_CODE: 0

Note on command form: this repo's csharpier is v1.x, which uses subcommand syntax (`csharpier check <path>` / `csharpier format <path>`). The plan's literal `csharpier . --check` is the v0 form and is not accepted by the installed v1 CLI. The v1 equivalent check command was used and recorded here.

Output Summary:
- `Checked 1089 files in 3432ms.`
- No files reported as needing formatting. Formatter baseline is clean (EXIT_CODE 0).
