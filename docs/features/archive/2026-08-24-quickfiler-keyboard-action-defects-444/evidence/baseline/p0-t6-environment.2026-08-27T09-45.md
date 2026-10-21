# [P0-T6] Execution environment

Timestamp: 2026-08-27T09-45
Command: `git rev-parse --show-toplevel`; `git rev-parse HEAD`; `git rev-parse --abbrev-ref HEAD`; `git merge-base HEAD origin/epic/quickfiler-bug-family-integration`; `git merge-base HEAD origin/main`; vswhere resolution of `MSBuild.exe` and `vstest.console.exe`
EXIT_CODE: 0

## Workspace

| Item | Value |
| --- | --- |
| `git rev-parse --show-toplevel` | `<repo-root>` |
| `git rev-parse HEAD` | `125c36b0669d9dd6095f156901bba138e2272f56` |
| `git rev-parse --abbrev-ref HEAD` | `bug/quickfiler-keyboard-action-defects-444` |

## Merge bases

| Reference | Merge base |
| --- | --- |
| `MERGE_BASE` = `git merge-base HEAD origin/epic/quickfiler-bug-family-integration` | `125c36b0669d9dd6095f156901bba138e2272f56` |
| `git merge-base HEAD origin/main` (reference only) | `a70420a766915998470a371c30020cd9d7157724` |

**The two merge bases do NOT agree.** This is expected and correct: this feature is an epic child cut
from the tip of `origin/epic/quickfiler-bug-family-integration`, so its merge base against the
integration branch is the branch point itself (`125c36b0`, identical to `HEAD` at Phase 0 start),
while its merge base against `origin/main` is the older commit at which the integration branch itself
diverged from `main` (`a70420a7`). Every diff gate in this plan uses `BASE` =
`origin/epic/quickfiler-bug-family-integration`, never `origin/main`.

`MERGE_BASE` equals `HEAD` at this point in execution, so `"$MERGE_BASE..HEAD"` two-dot ranges are
empty until the first commit of this feature lands. `[P0-T23]` is the first commit that makes those
ranges non-vacuous.

## Toolchain

| Tool | Resolved path |
| --- | --- |
| `vswhere.exe` | `<program-files-x86>\Microsoft Visual Studio\Installer\vswhere.exe` |
| `MSBuild.exe` | `<program-files>\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe` |
| `vstest.console.exe` | `<program-files>\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe` |

Output Summary: worktree, HEAD, branch, and both merge bases recorded; `MERGE_BASE` is a
40-character hexadecimal SHA; MSBuild and vstest resolve to Visual Studio 18 Community; no absolute
host path, account name, or machine name appears in this artifact.
