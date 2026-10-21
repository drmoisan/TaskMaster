# dotnet tool restore (P0-T4)

Timestamp: 2026-09-01T15-41

Command: `dotnet tool restore`

EXIT_CODE: 0

Output Summary:

```
Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier

Restore was successful.
```

Version confirmation, run from the worktree root:

- `dotnet tool run csharpier --version` printed `1.2.6`.

That figure matches the version pinned by the repo-root `dotnet-tools.json`.
CSharpier 1.2.6's CLI requires a subcommand, so `format` and `check` are the
only two invocation forms this plan uses.
