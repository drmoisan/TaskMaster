# P0-T5 — Toolchain entry-point resolution

Timestamp: 2026-09-03T08-20

## Working-directory prefix used by every command block in this plan

This executing agent's Bash tool resets its working directory to the session root between calls, and
the directory does not persist: a standalone `cd` into the item worktree followed by `pwd` in the
next call reported the session root again. Every command block in this plan is specified to run
"from the worktree root", so each recorded command line below carries a leading
`env -C <worktree-root> ` prefix, which sets the process working directory for that one command
without using `cd` and without chaining. The prefix changes no switch and no operand; it is applied
for the same class of reason the plan's own `MSYS_NO_PATHCONV=1 ` prefix is applied, and the
acceptance clauses in P0-T9 and P4-T4 are already worded as `contains` rather than `begins with` for
exactly that situation. `<worktree-root>` denotes this item's worktree root and is not spelled out
here, because it is an absolute host path.

Command:
```text
env -C <worktree-root> dotnet --version
env -C <worktree-root> PATH="/c/Program Files (x86)/Microsoft Visual Studio/Installer:$PATH" vswhere.exe -latest -products '*' -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe"
env -C <worktree-root> msbuild.exe -version
env -C <worktree-root> nuget.exe restore TaskMaster.sln
env -C <worktree-root> dotnet tool restore
env -C <worktree-root> dotnet tool run csharpier --version
env -C <worktree-root> dotnet-coverage --version
ls -la "<resolved-vstest-dir-native>\vstest.console.exe"
```

EXIT_CODE:
- `dotnet --version` — 0
- `vswhere.exe ... -find ...` — 0
- `msbuild.exe -version` — 0
- `nuget.exe restore TaskMaster.sln` — 0
- `dotnet tool restore` — 0
- `dotnet tool run csharpier --version` — 0
- `dotnet-coverage --version` — 0
- `ls -la` existence check on the resolved `vstest.console.exe` — 0

## Output Summary

SDK_BOOTSTRAP: NOT REQUIRED (first probe already reported a version beginning 8.0.2)

The first and only `dotnet --version` probe, run with the worktree root as its working directory,
printed `8.0.205` and exited 0. `.dotnet-sdk/dotnet.exe` is already present in this worktree, so none
of the four POSIX bootstrap commands was run and there is no post-bootstrap reading to record.
`scripts/vscode/Install-RepoDotNetSdk.ps1` was not invoked, in accordance with constraint 1 of
"Shell constraints measured in this worktree".

Recorded observation about the probe's working-directory sensitivity: an identical `dotnet --version`
issued without the `env -C` prefix — that is, from the session root rather than from this item's
worktree — exits 155 and reports `The repo-local .NET SDK is missing`, because `global.json`'s
`paths` entry `.dotnet-sdk` is resolved relative to whichever `global.json` the working directory
selects. That reading describes a different checkout and is not a fact about this worktree. It is
recorded here so the `env -C` prefix is understood as load-bearing rather than cosmetic.

NUGET_RESTORE: exit code 0. Restore summary line printed verbatim:
`All packages listed in packages.config are already installed.`
MSBuild auto-detection selected version `18.9.1.35102`. No `packages/build/` directory was produced.

Verbatim path line printed by `vswhere.exe`:
```text
C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe
```

RESOLVED_VSTEST_DIR_NATIVE: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform`

RESOLVED_VSTEST_DIR: `/c/Program Files/Microsoft Visual Studio/18/Community/Common7/IDE/Extensions/TestPlatform`

Both values are derived mechanically from the single printed line by stripping the trailing
`\vstest.console.exe`, and, for the POSIX spelling, by replacing the leading `C:` with `/c` and every
`\` with `/`. `vstest.console.exe` was confirmed to exist inside that directory (310088 bytes).

Tool versions reported:
- MSBuild — `MSBuild version 18.9.1+a81b43525 for .NET Framework` / `18.9.1.35102`
- CSharpier — `1.2.6`, matching the version pinned by `dotnet-tools.json`
- dotnet-coverage — `18.10.0+f4cc39224845ffa74bf246c9da2399d50e5d6342`

`dotnet tool restore` reported `Tool 'csharpier' (version '1.2.6') was restored.` followed by
`Restore was successful.` `dotnet-coverage` was already available, so no
`dotnet-coverage: UNAVAILABLE` record and no install attempt was required.

## Acceptance

All clauses satisfied: the last (and only) `dotnet --version` reading is `8.0.205`, beginning `8.0.2`,
exit 0; `nuget.exe restore TaskMaster.sln` exits 0; both resolved directory values are concrete and
non-empty and `vstest.console.exe` exists inside that directory; `msbuild.exe -version` exits 0;
`dotnet tool restore` exits 0; the reported CSharpier version is `1.2.6`.
