# Toolchain baseline — dotnet-coverage global tool (Issue #824, task P0-T7)

Timestamp: 2026-09-09T15-05

Command (first invocation): `pwsh -NoProfile -Command 'Set-Location "<worktree-root>"; if (-not (Get-Command dotnet-coverage -ErrorAction SilentlyContinue)) { dotnet tool install --global dotnet-coverage }; dotnet-coverage --version'`

Command (second invocation): `pwsh -NoProfile -Command 'Set-Location "<worktree-root>"; Write-Output ("RESOLVED=" + [bool](Get-Command dotnet-coverage -ErrorAction SilentlyContinue)); dotnet-coverage --version'`

EXIT_CODE: 0

Output Summary:

First invocation:

```
18.10.0+f4cc39224845ffa74bf246c9da2399d50e5d6342
```

No install line was printed, so `Get-Command dotnet-coverage` already resolved and the conditional
install did not run. The tool was already present on this workstation from earlier work.

Second invocation, in a separate PowerShell session:

```
RESOLVED=True
18.10.0+f4cc39224845ffa74bf246c9da2399d50e5d6342
```

EXIT_CODE of the second invocation: 0

`RESOLVED=True` in a session that did not perform the install is the observation that matters, because
a global tool installed by a given session is not on that same session's PATH, and P0-T11 runs in a
different session again where `scripts/vscode/Invoke-MSTestWithCoverage.ps1:292-293` throws if the
tool is unresolvable. The blocked branch of this task was therefore not taken.
