# Toolchain baseline — local dotnet tool manifest (Issue #824, task P0-T6)

Timestamp: 2026-09-09T15-04

Command: `pwsh -NoProfile -Command 'Set-Location "<worktree-root>"; dotnet tool restore'`

EXIT_CODE: 0

Output Summary:

```
Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier

Restore was successful.
```

csharpier 1.2.6 was restored, matching the pin at `dotnet-tools.json:5-11`. CSharpier v1 requires a
subcommand, so every invocation in this plan uses `dotnet tool run csharpier format .` or
`dotnet tool run csharpier check .`, never the bare-path form and never a globally installed copy.
