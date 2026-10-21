# Phase 0 — Local tool manifest restore

Timestamp: 2026-09-09T13-49

Task: [P0-T5]

Command: `dotnet tool restore` (from the worktree root)

EXIT_CODE: 0

Printed output:

```
Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier

Restore was successful.
```

CSHARPIER-VERSION: 1.2.6

The version is read with the Read tool from the `tools.csharpier.version` value of the
repository-root `dotnet-tools.json`, which declares `"version": "1.2.6"` with a single command name
`csharpier` and `"rollForward": false`. It is not read from a `--version` probe, because CSharpier
1's success-case output for a root-level `--version` argument has not been observed on this branch
and an assertion over unobserved output cannot distinguish a passing run from a failing one.
CSharpier 1 requires a subcommand, so every CSharpier invocation in this plan goes through
`dotnet tool run csharpier <subcommand>`.

Output Summary: `dotnet tool restore` exited 0 and restored the manifest-pinned CSharpier 1.2.6.
The manifest value read from `dotnet-tools.json` agrees with the version the restore reported.
