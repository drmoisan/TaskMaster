# P0-T8 — Coverage Collector Availability

Timestamp: 2026-09-01T13-33

Command: `dotnet tool update --global dotnet-coverage` (run from the checkout root, with `PATH` and
`DOTNET_ROOT` pointed at the repository-local `.dotnet-sdk` directory)

EXIT_CODE: 0

Output Summary:

The command printed one line, recorded verbatim:

```
Tool 'dotnet-coverage' was successfully updated from version '18.5.2' to version '18.10.0'.
```

The tool was already present at 18.5.2 and was updated to 18.10.0, which is the update branch of the
task's stated behavior rather than the install branch.

After the run, `(Get-Command dotnet-coverage).Source` resolved to an executable at

```
<user-profile>\.dotnet\tools\dotnet-coverage.exe
```

The leading user-profile directory is elided. The resolved path carries the operating-system account
name, and repository artifact hygiene prohibits embedding an account or machine name in an artifact.
The remainder of the path is recorded verbatim and identifies the global .NET tools directory the
executable resolves from, which is the auditable content of the observation.

This matters because `scripts/vscode/Invoke-MSTestWithCoverage.ps1:292-294` throws when the tool is
missing; P0-T15 and P2-T7 both invoke that script.
