# Phase 0 — .NET SDK Resolution ([P0-T5])

Timestamp: 2026-08-28T05-10

Command: `dotnet --version` and `dotnet --list-sdks`, run from the worktree root.
EXIT_CODE: 0

## `dotnet --version`

```
8.0.205
```

The command exited 0 and printed a version rather than the `global.json` error message. The
`Install-RepoDotNetSdk.ps1` remediation branch stated by this task was therefore not required at the
time of this probe: the repo-local SDK was already installed into `.dotnet-sdk` during worktree
bootstrap before plan execution began, and the probe confirms it resolves.

## Version-component check

`global.json` pins `"version": "8.0.205"` under `"rollForward": "latestFeature"`, so the acceptance is
a component comparison rather than a string equality:

| Component | Required | Observed | Result |
| --- | --- | --- | --- |
| major | `8` | `8` | pass |
| minor | `0` | `0` | pass |
| remaining (feature/patch) | at or above `205` | `205` | pass |

## `dotnet --list-sdks`

```
8.0.205 [<worktree-root>\.dotnet-sdk\sdk]
10.0.400 [C:\Program Files\dotnet\sdk]
```

The repo-local `8.0.205` under this worktree's `.dotnet-sdk` is what `global.json` selects, through
the `"paths": [".dotnet-sdk", "$host$"]` entry which searches the repo-local location first. The
machine-wide `10.0.400` is present but is not selected: it fails the `8.0` major/minor pin, and
`rollForward: latestFeature` does not cross a major version. Absolute paths are elided above to keep
the artifact free of a host account name; the first path is the `.dotnet-sdk\sdk` directory of this
worktree.

Output Summary: `dotnet --version` exits 0 and prints `8.0.205`, satisfying the major `8`, minor `0`,
remaining-at-or-above-`205` condition. The repo-local SDK under this worktree's `.dotnet-sdk` is the
one selected by `global.json`.
