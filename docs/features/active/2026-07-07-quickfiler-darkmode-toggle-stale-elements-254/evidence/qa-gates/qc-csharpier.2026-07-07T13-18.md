# QC — CSharpier Format (Issue #254)

Timestamp: 2026-07-07T13-18

Command: `dotnet tool run csharpier .` (executed as CSharpier v1 `dotnet tool run csharpier format .`, then verified with `dotnet tool run csharpier check .`)

Note: The repo-resolved CSharpier is v1.2.6, whose CLI uses `format`/`check` subcommands. The plan's bare `csharpier .` maps to the v1 `format` action; a follow-up `check` confirms no residual diffs.

EXIT_CODE: 0

Output Summary: Formatted 1277 files in 4987ms (no changes to already-formatted files). `csharpier check .` -> Checked 1277 files, exit 0, no formatting differences. Only the two intended files (`Theme.Rendering.cs`, `Theme.MailLabelThemingTests.cs`) are modified/new and both are CSharpier-clean. No loop restart required.
