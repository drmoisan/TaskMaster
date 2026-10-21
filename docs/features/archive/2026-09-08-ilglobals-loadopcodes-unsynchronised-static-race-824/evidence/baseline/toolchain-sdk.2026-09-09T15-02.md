# Toolchain baseline — repository-local .NET SDK (Issue #824, task P0-T3)

Timestamp: 2026-09-09T15-02

Command (first invocation): `pwsh -NoProfile -Command 'Set-Location "<worktree-root>"; & "./scripts/vscode/Install-RepoDotNetSdk.ps1"'`

Command (second invocation): `pwsh -NoProfile -Command 'Set-Location "<worktree-root>"; Write-Output ("SDK_MARKER=" + (Test-Path -LiteralPath ".dotnet-sdk/sdk/8.0.205")); dotnet --version; dotnet --list-sdks'`

EXIT_CODE: 0

Output Summary:

First invocation, console output:

```
Downloading .NET SDK 8.0.205 from https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.205/dotnet-sdk-8.0.205-win-x64.zip...
Installed repo-local .NET SDK 8.0.205 to <worktree-root>\.dotnet-sdk.
```

The installer is a PowerShell script that completes without invoking an external process on its
success path, so `$LASTEXITCODE` was unset after it and is recorded as such rather than as a
number. The installer throws when the marker directory was not created
(`scripts/vscode/Install-RepoDotNetSdk.ps1:102-104`), so completion without a throw plus the marker
observation below is what establishes success.

Second invocation, console output:

```
SDK_MARKER=True
8.0.205
8.0.205 [<worktree-root>\.dotnet-sdk\sdk]
10.0.400 [C:\Program Files\dotnet\sdk]
```

EXIT_CODE of the second invocation: 0

- `SDK_MARKER=True`. The marker `.dotnet-sdk/sdk/8.0.205` is the exact directory the installer
  itself requires. The worktree had no `.dotnet-sdk` before this task, so the observation is
  falsifiable.
- `dotnet --version` printed `8.0.205`, which begins `8.0.` as required. The exact version string is
  `8.0.205`.
- `dotnet --list-sdks` reported two SDKs: the repo-local `8.0.205` and the host `10.0.400`. No
  acceptance condition is keyed to this output; it is recorded as context. The resolution of
  `8.0.205` by `dotnet --version` is what establishes that the pinned SDK resolves, because
  `global.json:10` makes an unresolvable SDK a hard error rather than a silent fallback.

Path-redaction note, recorded as a deliberate executor decision: the plan asks for the
`dotnet --list-sdks` output verbatim. The absolute worktree prefix is replaced by
`<worktree-root>` in this committed artifact because a committed evidence artifact must not embed
an absolute host path where a repository-relative form conveys the same information. The
discriminating content of each line — which muxer root reports which SDK version — is preserved
exactly. `C:\Program Files\dotnet\sdk` is left unmodified because it names no user profile and no
repository-relative form exists for it.
