# [P2-T3] — Analyzer build (lint gate)

- Timestamp: 2026-08-30T02-08
- Task: `[P2-T3]`
- Issue: #644
- Branch: `bug/qfc-unregister-navigation-count-mismatch-orphan-644`
- Working directory: repository root of the branch worktree, referred to below as
  `<REPO_ROOT>`. No absolute host path, account name, or machine name is written to
  this artifact; msbuild echoes full project paths in its per-project output, and each
  such path is replaced by the `<REPO_ROOT>` placeholder here.
- MSBuild used: the Visual Studio 18 full-framework MSBuild, resolved with `vswhere`
  and referred to below as `<MSBUILD>`. The resolved path lies under the fixed
  `Program Files` system location and carries no account or machine name; it is
  nonetheless recorded as a placeholder because it varies by installed Visual Studio
  edition.
- Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
- EXIT_CODE: 0

## Why `/t:Rebuild`

`/t:Rebuild` is used rather than `/t:Build`. Analyzer diagnostics are produced during
compilation, and MSBuild's incremental up-to-date check compares timestamps without
invalidating on a command-line `/p:` change, so a warm `/t:Build` returns exit 0 with
`CoreCompile` skipped on every project and runs no analyzers.

That the gate was not vacuous is confirmed directly from the build log: the log
contains 79 `CoreCompile:` target lines and 36 `csc.exe` invocations across 11730
lines, so compilation and therefore analysis genuinely ran.

## Final summary block

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:16.64
```

| Counter | Expected | Measured |
|---|---|---|
| Error(s) | 0 | **0** |
| Warning(s) | (recorded, not gated) | **5** |

## Warning detail

All five warnings are the same single diagnostic, emitted once per project that
carries a `System.Reactive` `packages.config` entry (paths stripped):

```
warning : The project contains a packages.config file, which is not supported by
System.Reactive v7.0 or later. Please migrate to PackageReference. (You can suppress
this message by setting the RxUseUnsupportedPackagesConfig property to true, but be
aware this is an unsupported scenario.)
```

The diagnostic originates in
`<REPO_ROOT>/packages/System.Reactive.7.0.0/build/System.Reactive.PackagesConfigCheck.targets`
line 31, a NuGet-supplied targets file, not in any C# source file. It is pre-existing,
is unrelated to this cycle's two comment and string-literal edits, and is out of scope.
No warning references
`QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs`.

## Acceptance

| Clause | Required | Measured | Result |
|---|---|---|---|
| `EXIT_CODE:` | 0 | **0** | PASS |
| `0 Error(s)` in the final summary block | 0 errors | **0 Error(s)** | PASS |
| Warning count recorded | recorded | **5 Warning(s)**, all one pre-existing NuGet targets diagnostic | PASS |

## Output Summary

The analyzer-enabled solution rebuild exited 0 with `0 Error(s)` and `5 Warning(s)`,
all five being the same pre-existing `System.Reactive` `packages.config` diagnostic
from a NuGet targets file. `CoreCompile` ran (79 target lines, 36 `csc.exe`
invocations), so the analyzer gate was exercised rather than skipped. The lint gate
passes.
