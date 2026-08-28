# Bootstrap — Manifest Tool Restore ([P0-T7])

Timestamp: 2026-08-27T19-59

Command:
```
dotnet tool restore
```
(run from the workspace root)

EXIT_CODE: 0

## Output Summary

```
Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier

Restore was successful.
```

- The manifest is `dotnet-tools.json` at the repository root (not under `.config/`) and pins
  CSharpier `1.2.6`, which is the version `CLAUDE.md` §C#1.1 and `.claude/rules/csharp.md` item 1
  require. Every formatter invocation in this plan goes through `dotnet tool run csharpier` so the
  manifest-pinned version is used, never a global install.
- CSharpier 1.x requires a subcommand, so the approved forms are
  `dotnet tool run csharpier format <paths>` and `dotnet tool run csharpier check .`.
