# [P2-T4] — Nullable / Type-Check Build Gate

Timestamp: 2026-08-29T23-23
Run performed: 2026-08-30T01-17
Task: [P2-T4]
Working directory: `<repo-root>` (the repository root of this worktree)
Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
EXIT_CODE: 0

Redaction note: no absolute host path, account name, or machine name appears in this artifact.
The repository root is written as `<repo-root>` and per-project paths are written
repository-relative, following the convention already used by
`evidence/qa-gates/p4-t5-vstest-final.2026-08-29T08-15.md`. This is load-bearing here because
the worktree root is itself an absolute path under the user profile and msbuild echoes full
project paths in its per-project output.

## Command fidelity

The command is character-for-character the one the plan mandates, which is in turn the command
in `.github/workflows/ci.yml`. Two properties are load-bearing and were preserved:

- `/p:Nullable=enable` was **not** added. No project in this repository carries a `<Nullable>`
  element and there is no `Directory.Build.props`, so that property would be a solution-wide
  opt-in conscripting every file that has never adopted the `#nullable enable` pragma. CI omits
  it deliberately. Nullable enforcement here is per-file opt-in, and
  `/p:TreatWarningsAsErrors=true` promotes the `CS86xx` diagnostics of opted-in files to build
  errors.
- `/t:Build` was **not** substituted for `/t:Rebuild`. MSBuild's up-to-date check does not
  invalidate on a command-line `/p:` change, so a warm `/t:Build` would return exit 0 having
  skipped `CoreCompile` on every project and the gate could not fail. The log records 55
  `CoreCompile` invocations, confirming the compilation actually ran.

## Result

Final summary block:

```
Build succeeded.
    5 Warning(s)
    0 Error(s)
```

EXIT_CODE: 0

### Warning and error counts

- Errors: `0` — matching the `0 Error(s)` the acceptance requires.
- Warnings: `5`.

All five warnings are the same pre-existing advisory emitted once per affected project by a
package-supplied targets file, identical to the set `[P2-T3]` recorded:

```
packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5): warning :
The project contains a packages.config file, which is not supported by System.Reactive v7.0 or
later. Please migrate to PackageReference.
```

Affected projects: `QuickFiler.csproj`, `TaskMaster.csproj`, `ToDoModel.csproj`,
`UtilitiesCS.csproj`, `UtilitiesCS.Test.csproj`. These are warnings from a targets file, not
compiler diagnostics, which is why `/p:TreatWarningsAsErrors=true` did not promote them to
errors.

### CS0414 statement

No `CS0414` diagnostic appears. A literal search of the build log for `CS0414` returns `0`
matches.

### Nullable diagnostics

A search of the build log for the `CS86xx` diagnostic family returns `0` matches, so no
nullable-flow diagnostic was raised anywhere in the solution under warnings-as-errors.

### The edited file

`QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs` appears twice in
the build log. Both occurrences are `csc.exe` command-line and response-file lines that
enumerate the file as a compile input, not diagnostics. The file therefore carries zero
diagnostics in this gate, which is the expected result for an edit confined to XML
documentation comment text and one string literal's contents.

## Acceptance

| Clause | Required | Measured | Result |
|---|---|---|---|
| `EXIT_CODE` | `0` | `0` | PASS |
| msbuild final-summary error count | `0 Error(s)` | `0 Error(s)` | PASS |
| msbuild final-summary warning count | recorded | `5 Warning(s)` | RECORDED |
| Explicit statement that no `CS0414` diagnostic appears | stated | stated; log search returns `0` matches | PASS |

## Output Summary

`msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"
/p:TreatWarningsAsErrors=true` exited 0 with `Build succeeded`, `0 Error(s)` and
`5 Warning(s)`. No `CS0414` diagnostic appears and no `CS86xx` nullable diagnostic appears
anywhere in the log. All five warnings are the pre-existing System.Reactive packages.config
advisory, unrelated to this change. 55 `CoreCompile` invocations confirm the type check
actually ran rather than being skipped by incrementality. Type-check gate PASS.
