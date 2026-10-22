# Baseline Toolchain Bootstrap (Issue #1005)

Timestamp: 2026-10-22T16-06
Command: dotnet tool restore
EXIT_CODE: 0
Output Summary: Tool 'csharpier' (version '1.2.6') was restored. Restore was successful.

## Step 1 - Repo SDK install (conditional)

- Command: `scripts/vscode/Install-RepoDotNetSdk.ps1`
- Not run: the pinned SDK is present (`.dotnet-sdk\dotnet.exe` exists; `dotnet --list-sdks` lists 8.0.205 under `.dotnet-sdk`, matching `global.json`). The plan runs this step only when the pinned SDK is absent.

## Step 2 - Tool restore

- Command: `dotnet tool restore`
- EXIT_CODE: 0
- Output: `Tool 'csharpier' (version '1.2.6') was restored.` / `Restore was successful.`

## Step 3 - Package restore

- Command: `pwsh -NoProfile -File <repo>\scripts\vscode\Invoke-Restore.ps1` (absolute script path)
- EXIT_CODE: 0
- Output: MSBuild `/t:Restore /p:RestorePackagesConfig=true` on `TaskMaster.sln`: `Build succeeded.` `0 Warning(s)` `0 Error(s)`.

## git status --porcelain (before restore)

```
 M docs/features/active/2026-10-22-parallel-test-failures-wall-clock-and-shared-user-config-1005/plan.2026-10-22T11-02.md
?? docs/features/active/2026-10-22-parallel-test-failures-wall-clock-and-shared-user-config-1005/evidence/baseline/
```

## git status --porcelain (after restore)

```
 M docs/features/active/2026-10-22-parallel-test-failures-wall-clock-and-shared-user-config-1005/plan.2026-10-22T11-02.md
?? docs/features/active/2026-10-22-parallel-test-failures-wall-clock-and-shared-user-config-1005/evidence/baseline/
```

Result: identical listings; no new tracked-file change from the restore (the two entries are this run's plan check-off and evidence folder).
