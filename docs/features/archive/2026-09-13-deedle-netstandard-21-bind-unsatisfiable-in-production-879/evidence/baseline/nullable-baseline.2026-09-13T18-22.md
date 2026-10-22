# Phase 0 — Nullable Baseline

Timestamp: 2026-09-13T23-13

Build lock: ACQUIRED 879 at 2026-09-13T23:13:14, RELEASED by 879 at 2026-09-13T23:13:55.

Command:

```
msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true
```

resolved through `vswhere -latest -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\amd64\MSBuild.exe"`,
with all streams redirected to
`evidence/baseline/nullable-baseline-console.2026-09-13T18-22.txt`.
`/p:Nullable=enable` is deliberately not passed, matching
`.github/workflows/_build-nullable.yml` character for character.

EXIT_CODE: 0

ExpectedExitCode: 0

Output Summary:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
Time Elapsed 00:00:28.01
```

Baseline error count: 0. Baseline warning count: 0.

Occurrences of `Skipping target "CoreCompile"` in the console log: 0, so `/t:Rebuild`
invalidated every project and the gate was not vacuous.

Console log length after sanitisation: 11842 lines. Residual occurrences of the account name:
0. The absolute worktree path and the absolute user-profile path were replaced with
`<repo-root>` and `<user-profile>` before the log was retained, matching the convention used
by committed console logs elsewhere in this repository.

This build is the one `[P0-T10]` reads its build-output premises from.

Environment note: this baseline was taken after the analyzer-package bootstrap recorded in
`analyzer-baseline.2026-09-13T18-22.md`. Without it this command fails with the same two
`CS0006` diagnostics, because the missing `<Analyzer Include>` metadata file is a compiler
error independent of which gate properties are passed.
