# Baseline — CSharpier Format Check (Issue #185)

Timestamp: 2026-06-12T10-37

Command: dotnet tool run csharpier check .

Note: csharpier 1.2.6 (v1) uses the subcommand `check`. The plan referenced the legacy
`--check` flag form; the v1-equivalent `check` subcommand was run to perform the same
format-verification check.

EXIT_CODE: 0

Output Summary: Pass. Checked 1060 files in 2176ms. Zero unformatted files reported. Baseline formatting is clean.
