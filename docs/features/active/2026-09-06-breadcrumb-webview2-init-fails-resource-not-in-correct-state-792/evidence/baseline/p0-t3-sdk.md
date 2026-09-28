# [P0-T3] Repo-local .NET SDK bootstrap

- Issue: #792
- Timestamp: 2026-09-17T18-35
- Command: `pwsh -NoProfile -File scripts/vscode/Install-RepoDotNetSdk.ps1` (run with the item worktree as the working directory), then `dotnet --version` and `dotnet --list-sdks` from the same directory.
- EXIT_CODE: 0
- Output Summary: The install script exited 0. `dotnet --version` printed `8.0.205`, which begins `8.0.` and is a version string, not the `global.json` `errorMessage`. `dotnet --list-sdks` returned 2 rows (versions `8.0.205` and `10.0.401`); at least one row's path segment ends `.dotnet-sdk/sdk` after `.Replace('\', '/')`. The `--list-sdks` output is not transcribed because its path column carries the account name.

## Observations

- DOTNET-VERSION: 8.0.205
- SDK-UNDER-REPO-LOCAL-DIR: true
- SDK-LINE-COUNT: 2
- SDK-VERSIONS-ONLY: 8.0.205, 10.0.401
- DOTNET-SDK-DIR-GITIGNORED: true (`git check-ignore -q .dotnet-sdk` exit 0; `.gitignore` line 350), so the tree stays clean.

## Execution notes

- The initial attempt to compute `SDK-UNDER-REPO-LOCAL-DIR` inline through the Bash tool produced `false` because the doubled backslash in a regex was collapsed by the tool layer into an invalid pattern. The value above was re-derived through the plan's gitignored helper path `coverage/plan792-helper.ps1` (convention 8) using the string method `.Replace('\', '/')`, which needs no regex escape.
- `pwsh -File` resolves a relative script path against the launching shell's directory rather than `-WorkingDirectory`, so the helper was invoked by absolute path; the helper's opening branch assertion is the worktree proof.
