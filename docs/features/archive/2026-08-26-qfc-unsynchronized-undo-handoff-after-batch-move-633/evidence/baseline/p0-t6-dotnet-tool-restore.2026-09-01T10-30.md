# dotnet tool restore and CSharpier version (P0-T6)

Timestamp: 2026-09-01T10-30
Task: [P0-T6]
Working directory: WORKTREE

## Command 1

Command: `dotnet tool restore`
EXIT_CODE: 0
Output:

```
Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier

Restore was successful.
```

## Command 2

Command: `dotnet tool run csharpier --version`
EXIT_CODE: 0
Output:

```
1.2.6
```

## Manifest cross-check

The tool manifest for this repository is `dotnet-tools.json` at the worktree root, not the conventional
`.config/dotnet-tools.json`. Its `tools.csharpier.version` field records `1.2.6`, which agrees with both
commands above.

Output Summary: `dotnet tool restore` exited 0 and restored CSharpier 1.2.6, the manifest-pinned
version. The fallback branch the plan made mandatory was not needed: contrary to the plan's stated
uncertainty about whether CSharpier v1 would accept a bare version switch, `dotnet tool run csharpier
--version` was accepted, exited 0, and printed the version string `1.2.6`. The observed version and the
manifest-pinned version agree, so the formatter this run uses is the same one `.github/workflows/ci.yml`
runs after its own `dotnet tool restore`.
