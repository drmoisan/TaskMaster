# Baseline — CSharpier Format Check (Issue #254)

Timestamp: 2026-07-07T13-03

Command: `dotnet tool run csharpier check .`

Note: Plan text specifies `dotnet tool run csharpier --check .`. The repo-resolved CSharpier is v1.2.6, whose CLI uses the `check` subcommand (`csharpier check .`); the legacy `--check` flag is rejected by this version ("'--check' was not matched. Did you mean one of the following? check"). The version-correct invocation was used to satisfy the task intent (format-check baseline). Same intent, same scope.

EXIT_CODE: 0

Output Summary: Checked 1276 files in 4387ms. No formatting differences reported. Baseline formatting state is clean.
