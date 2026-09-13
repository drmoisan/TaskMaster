# P0-T7 — dotnet-coverage global tool availability

Timestamp: 2026-09-13T04-58
Command: if (-not (Get-Command dotnet-coverage -ErrorAction SilentlyContinue)) { dotnet tool install --global dotnet-coverage }
EXIT_CODE: 0

## CMD-COVERAGETOOL output

```
ALREADY-PRESENT
```

The guard found the tool already installed on this host, so the install branch did not run. The
wrapper reported an empty `$LASTEXITCODE` because the last statement was not an external process;
the verification span below is the authoritative check the acceptance condition names.

## Verification

Command: Get-Command dotnet-coverage

```
NAME: dotnet-coverage.exe
SOURCE: <user-profile>\.dotnet\tools\dotnet-coverage.exe
VERSION: 18.10.0.0
```

Verified: `Get-Command dotnet-coverage` resolves to a command and prints its source path. The tool
is a global tool installed under the user profile tools directory and is not supplied by the tool
manifest restore of P0-T4, which is why this task exists as a separate check.

Output Summary: dotnet-coverage version 18.10.0.0 is present and resolvable. The acceptance
condition is met: the subsequent `Get-Command dotnet-coverage` resolved to a command and printed its
source path.
