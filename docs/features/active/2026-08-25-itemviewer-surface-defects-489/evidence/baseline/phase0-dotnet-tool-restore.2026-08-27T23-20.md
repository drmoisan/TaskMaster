# Phase 0 — Repo-local SDK Bootstrap and Tool Manifest Restore (P0-T7)

Timestamp: 2026-08-27T23-20

Command: pwsh -File scripts/vscode/Install-RepoDotNetSdk.ps1
EXIT_CODE: SKIPPED — .dotnet-sdk already present

Command: dotnet tool restore
EXIT_CODE: 0

Output Summary:
- `.dotnet-sdk` exists on disk at the worktree root, so the SDK install step was not run and is
  recorded as `SKIPPED — .dotnet-sdk already present`. `global.json` pins
  `"paths": [".dotnet-sdk", "$host$"]` with `"version": "8.0.205"` and `"rollForward": "latestFeature"`,
  so every `dotnet` invocation would have failed with the `errorMessage` in `global.json` had the
  directory been absent.
- `dotnet tool restore` exited `0` and reported:
  `Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier` followed by
  `Restore was successful.`
- The tool manifest is `dotnet-tools.json` at the **repository root**, not `.config/dotnet-tools.json`.
  It declares `"version": 1`, `"isRoot": true`, and exactly one tool.
- Resolved CSharpier version read from `dotnet-tools.json`: **1.2.6**, with `"rollForward": false`, so
  the manifest-pinned version is the one `dotnet tool run csharpier` executes. This matches the
  version `.github/workflows/ci.yml` runs after its own `dotnet tool restore`.
