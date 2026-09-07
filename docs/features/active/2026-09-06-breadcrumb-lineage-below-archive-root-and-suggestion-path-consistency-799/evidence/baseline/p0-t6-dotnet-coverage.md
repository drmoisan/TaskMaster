# [P0-T6] dotnet-coverage resolution

Timestamp: 2026-09-07T06-51

Command: $probe = Get-Command dotnet-coverage -ErrorAction SilentlyContinue ; (probe branch taken) ;
dotnet-coverage --version

EXIT_CODE: 0

BRANCH TAKEN: probe branch — `Get-Command dotnet-coverage` returned a command, so the
`dotnet tool install --global dotnet-coverage` branch and its PATH prepend were NOT executed.

DOTNET-COVERAGE-ON-PATH: true

## Printed version

```
18.10.0+f4cc39224845ffa74bf246c9da2399d50e5d6342
```

The resolved command is `dotnet-coverage.exe`.

Output Summary: The probe branch is the branch the task predicted for this host. `dotnet-coverage` was already
resolvable with no PATH amendment beyond the repository-local SDK prepend required by R11, and
`dotnet-coverage --version` exited 0 printing version 18.10.0. The tool is available for the D12 coverage form
used by [P0-T12] and by the Phase 3 coverage tasks. The probe was performed with `Get-Command` rather than by
running the tool, because an unresolvable command name raises a PowerShell CommandNotFoundException instead of
setting a non-zero exit code.
