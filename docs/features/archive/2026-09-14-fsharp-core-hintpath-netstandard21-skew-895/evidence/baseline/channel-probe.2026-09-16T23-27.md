# Phase 0 — Command-Channel Probe, Worktree Identity, Build-Lock and Outlook Gate (Issue #895)

Timestamp: 2026-09-17T01-13
Task: [P0-T2]

WORKTREE-LEAF: agent-a8bc4dc5978785885

## Rung 1 — `pwsh -NoProfile -Command`

Command: `pwsh -NoProfile -Command 'Write-Output ok'`
EXIT_CODE: 0
ExpectedExitCode: 0
Emitted: `ok`

## Rung 2 — `pwsh -NoProfile -File`

Command: `pwsh -NoProfile -File "<worktree-root>/scripts/vscode/Install-RepoDotNetSdk.ps1"`
EXIT_CODE: 0
ExpectedExitCode: 0
Emitted (two lines):

```
Downloading .NET SDK 8.0.205 from https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.205/dotnet-sdk-8.0.205-win-x64.zip...
Installed repo-local .NET SDK 8.0.205 to <worktree-root>\.dotnet-sdk.
```

This is the first-install success-case text named by the task
(`Installed repo-local .NET SDK 8.0.205 to`), not the already-installed text. The installer sets no
`$LASTEXITCODE` of its own, so its result is read from the `.dotnet-sdk/sdk/8.0.205` marker
directory recorded below.

CHANNEL: PWSH-COMMAND=OK PWSH-FILE=OK

BUILD-LOCK: SUPPLIED (leaf directory name `parallel-build-lock`; `acquire.txt`, `release.txt` and
`cancel-waiter.txt` all take `-Item`; item token for this run is `895`)

## Identity and gate payload

Command:

```
pwsh -NoProfile -Command '
Set-Location -LiteralPath "<worktree-root>"
[System.IO.Directory]::SetCurrentDirectory((Get-Location).Path)
Write-Output ("WORKTREE-LEAF=" + (Split-Path -Leaf (Get-Location).Path))
Write-Output ("TOPLEVEL-LEAF=" + (Split-Path -Leaf ((git rev-parse --show-toplevel) -replace "/", "\")))
Write-Output ("SOLUTION-PRESENT=" + (Test-Path -LiteralPath "TaskMaster.sln"))
Write-Output ("SPEC-PRESENT=" + (Test-Path -LiteralPath "docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/spec.md"))
Write-Output ("SDK-MARKER-PRESENT=" + (Test-Path -LiteralPath ".dotnet-sdk/sdk/8.0.205"))
Write-Output ("OUTLOOK-PROCESS-COUNT=" + @(Get-Process -Name OUTLOOK -ErrorAction SilentlyContinue).Count)
'
```

EXIT_CODE: 0
ExpectedExitCode: 0

## Output Summary:

```
WORKTREE-LEAF=agent-a8bc4dc5978785885
TOPLEVEL-LEAF=agent-a8bc4dc5978785885
SOLUTION-PRESENT=True
SPEC-PRESENT=True
SDK-MARKER-PRESENT=True
OUTLOOK-PROCESS-COUNT=0
```

All six acceptance clauses hold: the channel is open on both rungs, `WORKTREE-LEAF` equals
`TOPLEVEL-LEAF` (the working directory is the git top level of the assigned worktree, not another
checkout), the solution and the spec are present, the SDK marker directory exists, no Outlook
process is running, and `BUILD-LOCK:` carries the permitted value `SUPPLIED`.
