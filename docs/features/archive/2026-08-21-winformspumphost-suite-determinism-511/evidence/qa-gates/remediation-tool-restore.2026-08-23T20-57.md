# Remediation QA Gate — `dotnet tool restore`

Timestamp: 2026-08-23T19-11

Command:
```
dotnet tool restore
```
(run from the worktree root `.claude/worktrees/agent-ad37a256a0fb60243`)

EXIT_CODE: 0

Output Summary:

```
Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier

Restore was successful.
```

Restored CSharpier version: **1.2.6**, matching the pin in the worktree-root manifest
`dotnet-tools.json` (`"csharpier": { "version": "1.2.6", "rollForward": false }`). The manifest sits
at the worktree root, not under `.config/`. All later formatting steps in this phase invoke
CSharpier through `dotnet tool run` so this manifest-pinned version is the one used, matching
`.github/workflows/ci.yml`.
