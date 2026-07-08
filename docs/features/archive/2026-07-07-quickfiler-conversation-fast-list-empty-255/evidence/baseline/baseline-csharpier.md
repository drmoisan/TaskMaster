# Baseline — CSharpier Formatting (Issue #255)

Timestamp: 2026-07-07T13-09

Command: dotnet tool run csharpier check .

Note: CSharpier 1.2.6 (repo-pinned in dotnet-tools.json) uses the `check` subcommand. The plan's literal `--check` flag is the CSharpier 0.x syntax and is rejected by 1.2.6; `check` is the mechanically-equivalent 1.2.6 invocation and was used to capture the same formatting-baseline signal.

EXIT_CODE: 0

Output Summary: Checked 1276 files in 4325ms. No formatting violations. Repository is CSharpier-clean at baseline.
