# Baseline — `dotnet tool restore`

Timestamp: 2026-08-22T09-21

Command:

```
dotnet tool restore
```

Run from the worktree root
`<repo-root>\.claude\worktrees\agent-ad37a256a0fb60243`.

EXIT_CODE: 0

Output Summary:

Full command output:

```
Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier

Restore was successful.
```

- **Restored CSharpier version: `1.2.6`.** This matches the acceptance condition exactly.
- The version is confirmed against the manifest, which lives at the worktree root as
  `dotnet-tools.json` (not under `.config/`):

  ```json
  {
    "version": 1,
    "isRoot": true,
    "tools": {
      "csharpier": {
        "version": "1.2.6",
        "commands": [
          "csharpier"
        ],
        "rollForward": false
      }
    }
  }
  ```

  `"rollForward": false` means the restored version cannot drift from the pin, so the restored
  `1.2.6` is the version every subsequent `dotnet tool run csharpier` invocation in this plan will
  use. This is the same version `.github/workflows/ci.yml` runs after its own `dotnet tool restore`,
  so local formatter output agrees with CI.

This task became runnable only after P0-T8 provisioned the worktree-local SDK; before that,
`dotnet tool restore` would have failed with the `global.json` error message.
