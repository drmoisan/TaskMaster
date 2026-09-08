# [P0-T8] `dotnet-coverage` global tool availability

Timestamp: 2026-09-08T00-24

Command: `Get-Command dotnet-coverage -ErrorAction SilentlyContinue`, then `dotnet-coverage --version`

EXIT_CODE: 0

DOTNET_COVERAGE_PRESENT_BEFORE: true
DOTNET_COVERAGE_INSTALLED_BY_THIS_TASK: false

Output Summary:

`Get-Command dotnet-coverage` resolved to a global tool under the user profile's `.dotnet\tools` directory, so the conditional `dotnet tool install --global dotnet-coverage` branch was not taken.

The verbatim single line printed by `dotnet-coverage --version`:

```
18.10.0+f4cc39224845ffa74bf246c9da2399d50e5d6342
```

Both entry states converge on the same end state; the version line is present, so the task is complete. This matters because `scripts/vscode/Invoke-MSTestWithCoverage.ps1:292-294` throws when the tool is absent, and every coverage task in this plan invokes it directly.
