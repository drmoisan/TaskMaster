# Baseline — CSharpier (P0-T7)

Timestamp: 2026-07-09T22-00
Command: csharpier check .
EXIT_CODE: 0
Output Summary: Checked 1318 files in 3337ms. No formatting changes required (clean baseline).

Note: The installed CSharpier is v1.3.0, which uses the `check`/`format` subcommands.
The plan's canonical command `dotnet tool run csharpier .` maps to `csharpier check .`
for verification (no local tool manifest is present; the global tool is used). Formatting
runs use `csharpier format .`.
