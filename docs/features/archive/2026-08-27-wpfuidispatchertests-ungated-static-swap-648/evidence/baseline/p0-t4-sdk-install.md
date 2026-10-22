# P0-T4 — Install the Repo-Pinned .NET SDK

Timestamp: 2026-09-01T13-22

Command: `pwsh -NoProfile -File scripts/vscode/Install-RepoDotNetSdk.ps1` (run from the checkout root)

EXIT_CODE: 0

Output Summary:

Before the run, `ls -d .dotnet-sdk` reported `No such file or directory`, so the directory did not
exist in this checkout. `global.json` pins `sdk.version` to `8.0.205` with `paths` of `.dotnet-sdk`
and `$host$`, so no `dotnet` command could satisfy the pin until this task completed.

The script printed two lines:

```
Downloading .NET SDK 8.0.205 from https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.205/dotnet-sdk-8.0.205-win-x64.zip...
Installed repo-local .NET SDK 8.0.205 to <checkout-root>\.dotnet-sdk.
```

The absolute path the second line printed named this checkout's `.dotnet-sdk` directory; it is
elided here so that no machine-specific absolute path is written into an artifact.

After the run, `ls -d .dotnet-sdk` resolved and the directory's top-level entries include
`dotnet.exe`, `host/`, `LICENSE.txt`, `packs/`, and `sdk/`. The acceptance condition — exit code 0
and `.dotnet-sdk` present after the run where it was absent before — is satisfied.
