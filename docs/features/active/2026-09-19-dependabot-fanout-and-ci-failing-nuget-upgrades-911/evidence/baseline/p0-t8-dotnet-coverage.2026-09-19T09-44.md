# P0-T8 — dotnet-coverage Global Tool

Timestamp: 2026-09-19T12-32

Command:
```
pwsh -NoProfile -Command 'Set-Location "C:\Users\DanMoisan\repos\TaskMaster-wt\dependabot-911";
  if (-not (Get-Command dotnet-coverage -ErrorAction SilentlyContinue)) {
      dotnet tool install --global dotnet-coverage
  }
  (Get-Command dotnet-coverage).Source; dotnet-coverage --version'
```

The guarded form is the one the plan specifies: the install runs only when the command does not
already resolve, so a re-run neither reinstalls nor fails on an already-installed tool.

EXIT_CODE: 0

## Output

```
ALREADY_PRESENT=true
RESOLVED_PATH=C:\Users\DanMoisan\.dotnet\tools\dotnet-coverage.exe
RESOLVED_VERSION=18.10.0.0
18.10.0+f4cc39224845ffa74bf246c9da2399d50e5d6342
VERSION_EXIT=0
```

| Item | Value |
|---|---|
| Resolved command path | `C:\Users\DanMoisan\.dotnet\tools\dotnet-coverage.exe` |
| File version | `18.10.0.0` |
| `dotnet-coverage --version` | `18.10.0+f4cc39224845ffa74bf246c9da2399d50e5d6342` (exit 0) |
| Install performed this run | no — the guard found the tool already present |

## Acceptance evaluation

- `Get-Command dotnet-coverage` resolves to a path:
  `C:\Users\DanMoisan\.dotnet\tools\dotnet-coverage.exe`. PASS.

**Failing-condition reachability.** The failing condition is that `Get-Command dotnet-coverage`
resolves to nothing. It is reachable on a machine where the global tool has never been installed,
and it matters because `scripts/vscode/Invoke-MSTestWithCoverage.ps1` — the script CMD-MSTEST-COVERAGE
invokes — throws before running anything when the tool is absent, so no coverage figure would ever
be recorded and every coverage-bearing gate in this plan would be unmeasurable rather than red.
The tool is a per-user global install rather than a per-worktree one, so it is shared across all
worktrees on this machine.

Output Summary: `dotnet-coverage` already present; no install was performed. `Get-Command` resolves
it to `C:\Users\DanMoisan\.dotnet\tools\dotnet-coverage.exe`, version
`18.10.0+f4cc39224845ffa74bf246c9da2399d50e5d6342`, and the tool executes at exit 0.
CMD-MSTEST-COVERAGE can therefore run.
