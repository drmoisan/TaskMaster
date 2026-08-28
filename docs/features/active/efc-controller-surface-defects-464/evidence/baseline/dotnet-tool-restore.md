# Phase 0 — dotnet tool restore

Timestamp: 2026-08-27T23-20
Task: [P0-T8]
Command: `dotnet tool restore` from the worktree root, then `dotnet tool run csharpier --version`, both under `pwsh -NoProfile`
EXIT_CODE: 0

## Result

- `dotnet tool restore` — EXIT_CODE 0. Output: `Tool 'csharpier' (version '1.2.6') was restored. Available
  commands: csharpier` followed by `Restore was successful.`
- `dotnet tool run csharpier --version` — EXIT_CODE 0, printed `1.2.6`, which is the version pinned by
  `dotnet-tools.json` and the version `.github/workflows/ci.yml` runs.

No `TOOLCHAIN_ABSENT` condition arose. The `[P0-T5]` installer branch had already provided the SDK, so no
second attempt was required.

Every csharpier invocation later in this plan goes through `dotnet tool run` so that this
manifest-pinned 1.2.6 is used, never a globally installed version.

Output Summary: dotnet tool restore exited 0 and csharpier reports version 1.2.6, matching the
dotnet-tools.json pin.
