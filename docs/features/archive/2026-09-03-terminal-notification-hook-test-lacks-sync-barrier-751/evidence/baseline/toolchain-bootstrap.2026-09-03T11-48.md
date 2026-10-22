# P0-T8 — Toolchain Bootstrap (Issue #751)

Timestamp: 2026-09-03T14-22

## Preconditions observed before any restore

| Probe | Value |
|---|---|
| `Test-Path 'packages'` | False |
| `Test-Path 'global.json'` | True |
| `Test-Path 'dotnet-tools.json'` (repository root) | True |
| `Test-Path '.config/dotnet-tools.json'` | False |
| `Test-Path '.dotnet-sdk'` | False |
| `nuget` on `PATH` | True |
| `dotnet` on `PATH` | True |
| `dotnet-coverage` on `PATH` | True |

The tool manifest is at the repository root as `dotnet-tools.json`, not under `.config/`, matching the note
in the P0-T8 task text. It pins csharpier to 1.2.6.

## Commands attempted, in order

### Attempt 1

Command: `dotnet tool restore`
EXIT_CODE: -2147450725

Output Summary:

```
The command could not be loaded, possibly because:
  * You intended to execute a .NET application:
      The application 'tool' does not exist or is not a managed .dll or .exe.
  * You intended to execute a .NET SDK command:
      The repo-local .NET SDK is missing. Run ./scripts/vscode/Install-RepoDotNetSdk.ps1 from the repository root, then retry dotnet format TaskMaster.sln.
```

This is exactly the repo-local SDK resolution error that `global.json` declares in its `errorMessage`
field. `global.json` pins SDK `8.0.205` with `paths` of `.dotnet-sdk` and `$host$`, and `.dotnet-sdk` did
not exist in this worktree.

### Fallback path taken — documented fallback 1 (repo-local SDK install)

Command: `scripts/vscode/Install-RepoDotNetSdk.ps1`
EXIT_CODE: 0 (the script emitted no non-zero code; `$LASTEXITCODE` was unset, and the script reported
success and produced the target directory)

Output Summary:

```
Downloading .NET SDK 8.0.205 from https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.205/dotnet-sdk-8.0.205-win-x64.zip...
Installed repo-local .NET SDK 8.0.205 to <WORKTREE>\.dotnet-sdk.
```

Sanitization: applied. Placeholder tokens used: `<WORKTREE>` for the worktree root and `<USER>` for the
account name. The installer's success line contained the worktree root and is transcribed with the root
substituted.

The script was run once, from the worktree root, as the fallback authorizes.

### Attempt 2 — retry of the first restore command

Command: `dotnet tool restore`
EXIT_CODE: 0

Output Summary:

```
Tool 'csharpier' (version '1.2.6') was restored. Available commands: csharpier

Restore was successful.
```

### Solution restore

Command: `nuget restore TaskMaster.sln`
EXIT_CODE: 0

Output Summary (final lines):

```
Installed:
    172 package(s) to packages.config projects
```

`nuget` was present on `PATH`, so the second documented fallback
(`& $msbuild TaskMaster.sln /t:Restore /m /p:RestorePackagesConfig=true`) was **not** used. The command run
matches the CI restore step at `.github/workflows/_mstest-coverage.yml:61`.

## Fallback determination (summary)

- Documented fallback 1 (`scripts/vscode/Install-RepoDotNetSdk.ps1`): **taken**, because `dotnet tool restore`
  failed with the `global.json` repo-local SDK resolution error and `.dotnet-sdk` was absent.
- Documented fallback 2 (`msbuild /t:Restore /p:RestorePackagesConfig=true`): **not taken**, because `nuget`
  resolved on `PATH` and `nuget restore TaskMaster.sln` succeeded.

## Acceptance (all three parts)

| Part | Required | Observed | Result |
|---|---|---|---|
| 1 | `EXIT_CODE: 0` for `dotnet tool restore` | 0 (attempt 2, after fallback 1) | PASS |
| 2 | `EXIT_CODE: 0` for the solution restore command used | 0 (`nuget restore TaskMaster.sln`) | PASS |
| 3 | `(Test-Path 'packages')` is `True` and `(Get-ChildItem -Path 'packages' -Directory).Count` > 0 | `True`; count = **172** | PASS |

Part 3 was checked independently of the exit codes, because both restore commands can exit 0 without having
created `packages/`. The directory did not exist before this task and contains 172 package directories after
it.
