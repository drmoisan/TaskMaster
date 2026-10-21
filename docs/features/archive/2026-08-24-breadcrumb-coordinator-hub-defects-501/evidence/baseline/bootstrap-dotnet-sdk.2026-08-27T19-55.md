# Bootstrap — Repo-Local .NET SDK (P0-T6)

Timestamp: 2026-08-27T19-55

`global.json` pins `sdk.version` to `8.0.205` with `rollForward: latestFeature` and
`paths: [".dotnet-sdk", "$host$"]`. Before this task the worktree had no `.dotnet-sdk` directory, so
every `dotnet` invocation printed the `global.json` `errorMessage` instead of a version.

## Step 1 — provision

Command: `pwsh -NoProfile -File ./scripts/vscode/Install-RepoDotNetSdk.ps1`
EXIT_CODE: 0
Output Summary:

```
Downloading .NET SDK 8.0.205 from https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.205/dotnet-sdk-8.0.205-win-x64.zip...
Installed repo-local .NET SDK 8.0.205 to WS\.dotnet-sdk.
```

(The tool printed the absolute workspace path; it is recorded here as the literal token `WS`.)

## Step 2 — version

Command: `dotnet --version`
EXIT_CODE: 0
Output Summary: `8.0.205`

## Step 3 — installed SDKs

Command: `dotnet --list-sdks`
EXIT_CODE: 0
Output Summary (workspace path rendered as `WS`):

```
8.0.205 [WS\.dotnet-sdk\sdk]
10.0.400 [C:\Program Files\dotnet\sdk]
```

The first entry is a path ending `.dotnet-sdk\sdk`, so the repo-local SDK is the one `global.json`
resolves. No `PATH` modification is required: the host `dotnet` muxer honours the `global.json`
`paths` entry from the workspace directory.

Acceptance: `dotnet --version` printed `8.0.205` and `dotnet --list-sdks` contains a path ending
`.dotnet-sdk\sdk`. PASS.
