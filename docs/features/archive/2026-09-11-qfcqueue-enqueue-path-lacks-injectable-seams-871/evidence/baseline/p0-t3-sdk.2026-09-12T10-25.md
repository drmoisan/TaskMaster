# P0-T3 — Repo-local .NET SDK provisioning

Timestamp: 2026-09-13T04-52
Command: pwsh -NoProfile -File .\scripts\vscode\Install-RepoDotNetSdk.ps1
EXIT_CODE: 0

## CMD-SDK output

```
Downloading .NET SDK 8.0.205 from https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.205/dotnet-sdk-8.0.205-win-x64.zip...
Installed repo-local .NET SDK 8.0.205 to <worktree-root>\.dotnet-sdk.
```

The installer writes no explicit exit code on success; the wrapper reported an empty
`$LASTEXITCODE`, which is the value PowerShell leaves when the last statement was not an external
process. The verification span below is the authoritative check, exactly as the task states.

## CMD-SDK-VERIFY output

Command: Test-Path .\.dotnet-sdk\sdk\8.0.205 ; dotnet --version

```
True
8.0.205
```

Verified: the first command printed `True`, the installer's own filesystem marker for the pinned
version, and the second printed `8.0.205`.

Output Summary: Repo-local SDK provisioned. CMD-SDK-VERIFY printed `True` then `8.0.205`, satisfying
both clauses of the acceptance condition. The worktree carried no SDK directory before this task,
which was confirmed by a Test-Path returning False prior to the install.
