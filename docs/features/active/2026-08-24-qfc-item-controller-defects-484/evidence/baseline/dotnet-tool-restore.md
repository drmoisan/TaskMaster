# Phase 0 — dotnet tool restore

Timestamp: 2026-08-26T08-30
Task: [P0-T8]

Command: `dotnet tool restore`
EXIT_CODE: 0

```
Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier

Restore was successful.
```

## Acceptance check

Command: `dotnet tool run csharpier --version`
EXIT_CODE: 0

```
1.2.6
```

The manifest-pinned CSharpier version is `1.2.6`, matching `dotnet-tools.json` and the version CI runs.
CSharpier 1.x requires a subcommand, so all formatting invocations in this plan use
`dotnet tool run csharpier format <paths>` and `dotnet tool run csharpier check .` (decision D5).

Output Summary: `dotnet tool restore` exited 0 and CSharpier reports version `1.2.6`.
