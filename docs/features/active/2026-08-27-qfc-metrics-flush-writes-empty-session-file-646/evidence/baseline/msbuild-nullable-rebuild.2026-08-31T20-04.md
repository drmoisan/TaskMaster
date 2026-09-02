# Baseline — MSBuild Nullable Rebuild (P0-T9)

Timestamp: 2026-09-01T12-24

Working directory: repository root (worktree for branch
`bug/qfc-metrics-flush-writes-empty-session-file-646`)
HEAD: `8a2054cd6c857195712c7db6cee0a34b631f3ca7`

Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
EXIT_CODE: 0

## Verbatim Printed Summary Lines

```
Build succeeded.

    5 Warning(s)
    0 Error(s)
```

## Output Summary

The nullable/type-check gate passes at baseline: `Build succeeded.`, 5 warnings, 0 errors,
exit code 0. Zero `CS86xx` nullable-flow diagnostics appear anywhere in the build log
(`grep -c "CS86"` returns `0`).

The 5 remaining warnings are the same non-compiler MSBuild warnings recorded in the P0-T8
artifact, emitted by the `_RxCheckPackagesConfig` target in
`packages/System.Reactive.7.0.0/build/System.Reactive.PackagesConfigCheck.targets(31,5)`.
`/p:TreatWarningsAsErrors=true` sets a C# compiler property and does not promote a warning
raised by an MSBuild `Warning` task, which is why these 5 survive as warnings under this
gate rather than becoming errors.

Per `CLAUDE.md` C#1.3, `/p:Nullable=enable` was deliberately **not** passed. Nullable
enforcement in this repository is per-file opt-in via `#nullable enable`; forcing the
solution-wide property would conscript files that have never adopted the pragma and does
not match `.github/workflows/ci.yml`.

## Non-Vacuity Check

`/t:Rebuild` was used, not `/t:Build`. The captured log contains 36 `csc.exe` command-line
occurrences, confirming `CoreCompile` ran on every project rather than being skipped by
MSBuild's incremental up-to-date check, which does not invalidate on a command-line `/p:`
change. The gate was capable of failing.
