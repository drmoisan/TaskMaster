# Finding 1 — Rebuild After the CustomUI Edit (P1-T6)

Timestamp: 2026-09-03T01-42
Task: [P1-T6]
Command: `& $msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0

MSBuild version 18.9.1+a81b43525 for .NET Framework, resolved through vswhere.

## Trailing counts

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:05.40
```

Five warnings, matching the P0-T6 and P0-T7 baselines exactly; they are the System.Reactive
`packages.config` advisory, one per consuming project. Zero errors.

## Why this build is load-bearing rather than routine

The two Finding 1 tests read the CustomUI document through `GetManifestResourceStream` on the
TaskMaster assembly, not from the source tree. Without this build the edited document would not be
re-embedded into `TaskMaster.dll` and would not be copied to the test output directory, so P1-T7
would answer from the pre-fix resource and would report a false failure — or, worse, a false pass
against stale content. `/t:Build` is correct here because this phase changed source files, so
`CoreCompile` is not up to date and does run.

Output Summary: Build succeeded with EXIT_CODE 0, 5 warnings (all the pre-existing System.Reactive
advisory) and 0 errors. The edited CustomUI document is re-embedded and copied to the test output
directory.
