# [P0-T4] Repo-Local .NET SDK Provisioning

Timestamp: 2026-08-26T08-36

Task: [P0-T4]
Feature: docs/features/active/quickfiler-bug-family-446

## Precondition Observed

`global.json` at the workspace root pins `sdk.version` `8.0.205` with `rollForward: latestFeature`
and `paths` of `.dotnet-sdk` and `$host$`. The directory `.dotnet-sdk` did not exist in this
worktree before this task ran (`ls -d .dotnet-sdk` reported "No such file or directory").

## Invocation 1 — before provisioning

Command: `pwsh -NoProfile -Command 'dotnet --version'`
EXIT_CODE: 1
Output Summary: the SDK host printed the `global.json` `errorMessage` instead of a version:
"The repo-local .NET SDK is missing. Run ./scripts/vscode/Install-RepoDotNetSdk.ps1 from the
repository root, then retry dotnet format TaskMaster.sln."

## Provisioning Step

Command: `pwsh -NoProfile -File scripts/vscode/Install-RepoDotNetSdk.ps1`
EXIT_CODE: 0
Output Summary: downloaded .NET SDK 8.0.205 from the Microsoft builds CDN and installed it to
`<repo-root>/.dotnet-sdk`.

## Invocation 2 — after provisioning

Command: `pwsh -NoProfile -Command 'dotnet --version; exit $LASTEXITCODE'`
EXIT_CODE: 0
Output: `8.0.205`

## Output Summary

The repo-local SDK is provisioned. The final `dotnet --version` exits 0 and prints `8.0.205`,
which begins with `8.0.` as the acceptance condition requires.
