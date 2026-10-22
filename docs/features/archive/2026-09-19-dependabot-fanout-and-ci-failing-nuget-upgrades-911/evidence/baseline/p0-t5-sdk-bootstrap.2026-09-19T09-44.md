# P0-T5 — Repository-Pinned .NET SDK Bootstrap

Timestamp: 2026-09-19T12-26

Command:
```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>";
  & ".\scripts\vscode\Install-RepoDotNetSdk.ps1"'
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>";
  dotnet --version; dotnet --list-sdks'
```

`Set-Location` to the execution worktree is part of the command rather than an ambient assumption:
`pwsh -File` and `pwsh -Command` both start in the session worktree, and `dotnet` searches upward
from the current directory for `global.json`, so an unset working directory would read a different
checkout's pin.

EXIT_CODE: 0

## Output

Installer:
```
Repo-local .NET SDK 8.0.205 is already installed at
<execution-worktree-root>\.dotnet-sdk.
```
The script terminated without error (`$?` = `True`). It reported the SDK already present, so this
run confirmed provisioning rather than performing a download.

`global.json` at the execution worktree root:
```json
{
  "sdk": {
    "version": "8.0.205",
    "rollForward": "latestFeature",
    "allowPrerelease": false,
    "paths": [ ".dotnet-sdk", "$host$" ],
    "errorMessage": "The repo-local .NET SDK is missing. Run ./scripts/vscode/Install-RepoDotNetSdk.ps1 from the repository root, then retry dotnet format TaskMaster.sln."
  }
}
```

`dotnet --version` (exit 0):
```
8.0.205
```

`dotnet --list-sdks` (exit 0):
```
8.0.205 [<execution-worktree-root>\.dotnet-sdk\sdk]
10.0.401 [C:\Program Files\dotnet\sdk]
```

Resolved host: `C:\Program Files\dotnet\dotnet.exe`. No `PATH` manipulation is required. The
`"paths": [".dotnet-sdk", "$host$"]` entry in `global.json` makes the ambient host select the
repo-local 8.0.205 SDK, which is why `--version` prints `8.0.205` rather than the host's 10.0.401.
A confirmatory run with `.dotnet-sdk` prepended to `PATH` resolved `dotnet` to
`…\dependabot-911\.dotnet-sdk\dotnet.exe` and printed the same `8.0.205`; the ambient form above is
the recorded measurement because it is what every later task in this plan will execute.

## Acceptance evaluation

- `dotnet --version` prints the version `global.json` pins: pinned `8.0.205`, printed `8.0.205`. PASS.
- `dotnet --list-sdks` includes a path ending `.dotnet-sdk\sdk`: the first entry is
  `<execution-worktree-root>\.dotnet-sdk\sdk`. PASS.

**Failing-condition reachability.** The failing condition is `dotnet --version` printing the
`global.json` `errorMessage` ("The repo-local .NET SDK is missing…") instead of a version. It is
reachable: `.dotnet-sdk` is a per-worktree directory that is not created by `git worktree add`, so a
fresh worktree reaches exactly that state until the installer has run. This worktree was provisioned
before Phase 0 began and therefore reports the already-installed path.

Output Summary: Repo-local .NET SDK 8.0.205 present at
`<execution-worktree-root>\.dotnet-sdk`; `dotnet --version` prints
`8.0.205`, matching the `global.json` pin; `dotnet --list-sdks` lists that SDK at a path ending
`.dotnet-sdk\sdk`. Both acceptance clauses hold, exit 0 on both commands.
