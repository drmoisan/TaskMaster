# [P2-T4] — Nullable / warnings-as-errors build (type-check gate)

- Timestamp: 2026-08-30T02-08
- Task: `[P2-T4]`
- Issue: #644
- Branch: `bug/qfc-unregister-navigation-count-mismatch-orphan-644`
- Working directory: repository root of the branch worktree, referred to below as
  `<REPO_ROOT>`. No absolute host path, account name, or machine name is written to
  this artifact; every msbuild-echoed project path is replaced by that placeholder.
- MSBuild used: the Visual Studio 18 full-framework MSBuild, resolved with `vswhere`
  and recorded as a placeholder because it varies by installed Visual Studio edition.
- Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
- EXIT_CODE: 0

## Command fidelity

The command is character-for-character the one in `.github/workflows/ci.yml`
(step "Build with nullable warnings treated as errors"), as `CLAUDE.md` requires.
Two properties are load-bearing and were preserved:

- `/p:Nullable=enable` was **not** added. No project in this repository carries a
  `<Nullable>` element and there is no `Directory.Build.props`, so the property would
  be a solution-wide opt-in conscripting every file that has never adopted the pragma.
  CI omits it deliberately.
- `/t:Build` was **not** substituted. MSBuild's up-to-date check does not invalidate on
  a command-line `/p:` change, so a warm `/t:Build` would return exit 0 having skipped
  `CoreCompile` on every project and the gate could not fail.

That the gate was not vacuous is confirmed from the build log: 64 `CoreCompile:`
target lines and 36 `csc.exe` invocations across 11807 lines.

## Final summary block

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:15.49
```

| Counter | Expected | Measured |
|---|---|---|
| Error(s) | 0 | **0** |
| Warning(s) | (recorded, not gated) | **5** |

## Nullable diagnostics

A search of the build log for the `CS86xx` nullable-flow diagnostic family returns
**0** occurrences. Nullable enforcement in this repository is per-file opt-in: a file
participates when it carries a `#nullable enable` directive, and
`/p:TreatWarningsAsErrors=true` then promotes its `CS86xx` diagnostics to build errors.
No such diagnostic was produced, and `0 Error(s)` confirms none was promoted.

## Warning detail

All five warnings are the same single pre-existing diagnostic recorded in `[P2-T3]`,
emitted once per project carrying a `System.Reactive` `packages.config` entry:

```
warning : The project contains a packages.config file, which is not supported by
System.Reactive v7.0 or later. Please migrate to PackageReference. (You can suppress
this message by setting the RxUseUnsupportedPackagesConfig property to true, but be
aware this is an unsupported scenario.)
```

It originates in
`<REPO_ROOT>/packages/System.Reactive.7.0.0/build/System.Reactive.PackagesConfigCheck.targets`
line 31, a NuGet-supplied targets file, not in any C# source file. It is not promoted
to an error by `/p:TreatWarningsAsErrors=true` because it is emitted by a targets file
rather than by the compiler. It is pre-existing and out of scope for this cycle. No
warning references
`QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs`.

## Acceptance

| Clause | Required | Measured | Result |
|---|---|---|---|
| `EXIT_CODE:` | 0 | **0** | PASS |
| `0 Error(s)` in the final summary block | 0 errors | **0 Error(s)** | PASS |
| Warning count recorded | recorded | **5 Warning(s)**, all one pre-existing NuGet targets diagnostic | PASS |

## Output Summary

The warnings-as-errors solution rebuild exited 0 with `0 Error(s)` and `5 Warning(s)`,
all five being the same pre-existing `System.Reactive` `packages.config` diagnostic.
Zero `CS86xx` nullable diagnostics were produced. `CoreCompile` ran (64 target lines,
36 `csc.exe` invocations), so the type-check gate was exercised rather than skipped.
The type-check gate passes.
