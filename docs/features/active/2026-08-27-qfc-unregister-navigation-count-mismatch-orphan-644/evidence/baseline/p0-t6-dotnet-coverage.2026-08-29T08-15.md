# Baseline — dotnet-coverage global tool ([P0-T6])

- Issue: #644
- Task: `[P0-T6]`
- Timestamp: 2026-08-29T08-15

## Why this task is required

`scripts/vscode/Invoke-MSTestWithCoverage.ps1` throws `dotnet-coverage not found` before it runs
anything, so the tool must be present before `[P0-T12]` and `[P4-T6]` can execute.

## Skip-branch evaluation

The task authorizes skipping the install when `Get-Command dotnet-coverage` already resolves.
That branch **did apply**:

Command: `Get-Command dotnet-coverage`
EXIT_CODE: 0

Output (host path redacted):

```
RESOLVED=<user-profile>\.dotnet\tools\dotnet-coverage.exe
```

The tool resolves from the user's global .NET tools directory, so
`dotnet tool install --global dotnet-coverage` was **not** run. This skip is explicitly
authorized by the `[P0-T6]` task text.

## Acceptance verification

Command: `dotnet-coverage --version`
EXIT_CODE: 0

Output:

```
18.5.2+6e39b75eaf98f2691cf62dbf259669cc13851fd3
```

Output Summary: `dotnet-coverage` was already installed, so the authorized skip branch was taken
and no install was performed. The acceptance probe `dotnet-coverage --version` exits 0 and prints
version string `18.5.2+6e39b75eaf98f2691cf62dbf259669cc13851fd3`. This is an authorized skip of
the *install* action only; the acceptance command itself was executed and its exit code and
output are recorded above, so this task's outcome is not `EXIT_CODE: SKIPPED`.
