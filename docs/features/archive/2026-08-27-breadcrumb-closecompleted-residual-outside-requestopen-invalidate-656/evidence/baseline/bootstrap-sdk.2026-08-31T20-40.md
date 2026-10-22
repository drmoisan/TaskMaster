# Baseline — Repo-Local .NET SDK Bootstrap (Issue #656)

Timestamp: 2026-09-01T14-36
Task: [P0-T3]

Command:
```
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Install-RepoDotNetSdk.ps1
dotnet --version
```

EXIT_CODE: 0

Results:

- Pre-state: no `.dotnet-sdk` directory existed in this worktree, as the plan's environment
  preconditions predicted.
- The installer downloaded .NET SDK `8.0.205` and installed it to `<repo-root>/.dotnet-sdk`.
- `Test-Path .dotnet-sdk\dotnet.exe` is True: the executable is present.
- `dotnet --version` printed `8.0.205` and exited 0. Before this step the same command would have
  printed the `global.json` `errorMessage` instead of a version, because `global.json` pins
  `8.0.205` with `paths` `[".dotnet-sdk", "$host$"]`.
- `.dotnet-sdk/` is git-ignored, so it does not enter the change set.

Output Summary: Bootstrap succeeded. Repo-local SDK 8.0.205 installed and resolving; `dotnet
--version` exits 0 and reports the pinned version. This is a bootstrap step, not a toolchain gate.
