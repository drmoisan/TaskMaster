# P0-T5 — External tool resolution

Timestamp: 2026-09-08T09-19
Task: [P0-T5]
Command: pwsh -NoProfile -File coverage/plan811-helper.ps1 (C1 preamble resolves `$vstest` and `$msbuild` through vswhere; then `Get-Command dotnet-coverage` and `dotnet-coverage --version`)
EXIT_CODE: 0

All resolved paths below are redacted: the Visual Studio installation root is written as
`<vs-install>` and the user profile root as `<user>`.

## Observations

| Tool | Resolved (redacted) | Non-empty | `Test-Path` |
|---|---|---|---|
| `vstest.console.exe` | `<vs-install>\Common7\IDE\Extensions\TestPlatform\vstest.console.exe` | `True` | `True` |
| `MSBuild.exe` | `<vs-install>\MSBuild\Current\Bin\MSBuild.exe` | `True` | `True` |
| `dotnet-coverage` | `<user>\.dotnet\tools\dotnet-coverage.exe` | `True` | resolved by `Get-Command` |

| Observation | Value |
|---|---|
| `dotnet-coverage` branch | ALREADY-PRESENT (no install was required) |
| `dotnet-coverage --version` | `18.10.0+f4cc39224845ffa74bf246c9da2399d50e5d6342` |
| `dotnet-coverage --version` exit code | `0` |

## Acceptance evaluation

- `$vstest` and `$msbuild` are both non-empty and `Test-Path` on each prints `True`. PASS
- `dotnet-coverage --version` exited 0 and printed a version string beginning with a digit
  (`18.10.0...`). PASS
- `Get-Command dotnet-coverage` succeeded, so the global-install branch did not apply; the
  ALREADY-PRESENT branch is recorded. PASS
- Every resolved path is redacted to `<vs-install>` or `<user>` in this artifact. PASS

## Output Summary

All three external tools resolved on the first attempt. vstest.console.exe and MSBuild.exe located
through vswhere against the latest installed Visual Studio; dotnet-coverage 18.10.0 already present
as a global .NET tool, so no install branch was taken.
