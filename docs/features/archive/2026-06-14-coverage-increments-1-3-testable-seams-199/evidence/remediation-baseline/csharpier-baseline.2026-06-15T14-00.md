# Baseline — CSharpier (Remediation Cycle 2026-06-15T14-00)

Timestamp: 2026-06-15T14-00
Command: dotnet tool run csharpier format .
EXIT_CODE: 0

Output Summary:
- CSharpier v1.2.6 (local tool). The plan task names `dotnet tool run csharpier .`; under CSharpier v1 the equivalent invocation is the `format` subcommand (`csharpier format .`); the legacy `csharpier .` form is no longer accepted by v1.
- "Formatted 1054 files in 1098ms." CSharpier v1 prints "Formatted N files" for all files scanned regardless of whether changes were written.
- `git status --porcelain` after the run shows no tracked `.cs` file modifications (only untracked evidence and plan files). No source files were reformatted. Baseline formatting is clean.
