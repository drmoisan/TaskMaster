# Baseline — CSharpier Format Check (Issue #328)

Timestamp: 2026-07-15T18-45
Command: dotnet tool run csharpier check .
EXIT_CODE: 0
Output Summary: Format-clean. Checked 1336 files in ~3.9s; zero files require reformatting.

Note: CSharpier is v1.2.6 in this repo; the v1 verify verb is `check` (the older
`--check` flag prints help). The `check` subcommand produces the authoritative
format-clean signal recorded above.
