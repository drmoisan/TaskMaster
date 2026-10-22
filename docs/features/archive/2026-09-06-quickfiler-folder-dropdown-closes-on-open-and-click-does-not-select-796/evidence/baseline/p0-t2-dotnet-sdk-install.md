# P0-T2 — Command channel determination and repo-pinned .NET SDK install

Timestamp: 2026-09-07T14-05
Task: [P0-T2]
Issue: #796

COMMAND-CHANNEL: A

## Rung taken

Rung 1. The isolation guard did not refuse the pwsh invocation, so rung 2 was not
taken and no Channel B substitution applies anywhere in this plan.

Command:
`pwsh -NoProfile -File scripts/vscode/Install-RepoDotNetSdk.ps1`

EXIT_CODE: 0

Output Summary: The script downloaded the SDK archive from
`https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.205/dotnet-sdk-8.0.205-win-x64.zip`
and reported `Installed repo-local .NET SDK 8.0.205 to <WORKTREE>/.dotnet-sdk.`
The absolute destination path the script printed is redacted to `<WORKTREE>`; no
absolute host path is recorded in this artifact.

## SDK marker verification

Command:
`pwsh -NoProfile -Command 'Test-Path .dotnet-sdk/sdk/8.0.205'`

Result: `MarkerExists=True`. The directory `.dotnet-sdk/sdk/8.0.205` exists.

## dotnet version verification on the recorded channel

Command:
`pwsh -NoProfile -Command 'dotnet --version'`

EXIT_CODE: 0

Recorded stdout, verbatim:

```
8.0.205
```

The recorded stdout is a version string beginning with the two characters `8.`.
It is not the sentence `The repo-local .NET SDK is missing.`, which is what
global.json prints as its errorMessage when the pinned SDK is absent.

The `dotnet` muxer resolved from the machine PATH; the SDK itself resolved to the
repo-local `.dotnet-sdk` directory through the `paths` entry in global.json, which
is why the printed version is the pinned 8.0.205 rather than a machine-global
version.

## Working-directory prefix used on Channel A

Every `pwsh -NoProfile -Command` invocation in this plan is issued from a tool whose
current directory is not this worktree, so each invocation is prefixed inside the
single-quoted script with `Set-Location <WORKTREE>;` before the plan's command text.
This is a working-directory prefix and not a substitution of any command form: the
command text following it is the plan's text verbatim, with the plan's relative paths
preserved. It is required because `dotnet` searches upward from the current directory
for global.json, and because every relative path in this plan is relative to the
worktree root. `pwsh -NoProfile -File` invocations are issued with the script's
absolute path; `Install-RepoDotNetSdk.ps1` resolves its install directory from
`$PSScriptRoot` rather than from the current directory, so it installed into this
worktree.

## Channel B equivalents

Not applicable. The recorded channel is A. No later task in this plan substitutes a
Channel B command form.
