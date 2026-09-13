# P0-T9 — Repo-local .NET SDK bootstrap

Timestamp: 2026-09-13T00-48

Command: `pwsh -NoProfile -Command '& ".\scripts\vscode\Install-RepoDotNetSdk.ps1"; if (Test-Path -LiteralPath ".\.dotnet-sdk\sdk\8.0.205") { "SDK_MARKER=present"; exit 0 } else { "SDK_MARKER=absent"; exit 1 }'`

Execution note: invoked with a leading `Set-Location` to the item worktree root. No absolute path is transcribed.

EXIT_CODE: 0

Output Summary:

```
Downloading .NET SDK 8.0.205 from <the pinned Microsoft build URL>...
Installed repo-local .NET SDK 8.0.205 to <the repo-local SDK directory>.
SDK_MARKER=present
```

- The repo-local SDK directory was absent before this task, as the plan's measured worktree condition 1 states; the installer downloaded and installed SDK 8.0.205, the version `global.json` pins.
- The gate is the installer's own filesystem marker, the version-named directory under the repo-local SDK directory, rather than an SDK version query. A version query prints the repository's pin error message when the directory is absent and is therefore not a usable signal.
- `SDK_MARKER=present` and exit code 0, so the gate passed.
- Outlook was not required to be closed for this task; it invokes no build.
- The two absolute paths the installer printed in its own console output are elided above, per the plan's evidence content rule. The installed location is the repo-local SDK directory named by `global.json` first in its paths list.
