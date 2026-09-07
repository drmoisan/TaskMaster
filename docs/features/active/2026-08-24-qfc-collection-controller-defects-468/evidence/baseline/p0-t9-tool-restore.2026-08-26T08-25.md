# [P0-T9] CSharpier tool manifest restore

Timestamp: 2026-08-26T08-25

Command: `pwsh -NoProfile -Command "Set-Location '<WS>'; dotnet tool restore"`

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

Restore output, verbatim:

```
Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier

Restore was successful.
```

### Acceptance verification

Command: `pwsh -NoProfile -Command "Set-Location '<WS>'; dotnet tool run csharpier --version"`

```
1.2.6
```

Exit code: 0. The manifest-pinned version is `1.2.6`, matching the plan's stated expectation and
`CLAUDE.md` §C#1.1. CSharpier v1 requires a subcommand, so the mutating and verifying forms are
`dotnet tool run csharpier format <paths>` and `dotnet tool run csharpier check .`; the v0 bare-path
form `csharpier .` does not run.

Result: PASS. Both acceptance conditions are met.

### Plan-accuracy note (cosmetic, non-blocking)

P0-T9 states "The manifest is `dotnet-tools.json` at the repository root". The manifest is in fact at
`.config/dotnet-tools.json`, which is the standard .NET tool-manifest location. This does not affect
the task's command or its outcome: `dotnet tool restore` is run from the workspace root and discovers
`.config/dotnet-tools.json` by the normal upward search. Recorded here rather than silently worked
around.
