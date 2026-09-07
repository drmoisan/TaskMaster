# P1-T13 — Seam build and scoped test run

Timestamp: 2026-09-07T01-30

Host-specific absolute paths, account names and machine names are redacted to `<repo-root>`,
`<user>` and `<host>` tokens.

## Analyzer build

Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"
/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true

Run from `<repo-root>`. MSBuild resolved through vswhere inline:
`<program-files>\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`.

EXIT_CODE: 0
ExpectedExitCode: 0

Output Summary:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:22.82
```

The summary line `0 Error(s)` is present. The warning count is 0, identical to the Phase 0
analyzer baseline recorded in the baseline evidence directory. No new diagnostic id was emitted.

`/t:Rebuild` was used, not `/t:Build`: MSBuild's up-to-date check does not invalidate on a
command-line `/p:` change, so a warm `/t:Build` returns exit 0 with `CoreCompile` skipped on every
project and runs no analyzers.

Compile-entry confirmation: the five new `.cs` files added by Phase 1 each appear in the compiler
inputs recorded in the build log, so the `<Compile Include>` entries inserted by P1-T6, P1-T7 and
P1-T10 are effective. The UtilitiesCS partial is additionally proven live by construction: the
callers of `AddQfcColumns` and `AddQfcColumnsAsync` remain in the source partial, so a missing
compile entry for the new partial would have produced CS0103 rather than a successful build.

## Nullable build (supplementary observation, not a P1-T13 acceptance clause)

Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"
/p:TreatWarningsAsErrors=true

EXIT_CODE: 0

Output Summary:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:21.29
```

Recorded because the new partial `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs` carries
`#nullable enable` and therefore opts in to nullable analysis, which this command promotes to
errors. `/p:Nullable=enable` was not supplied, matching CI and the plan's toolchain conventions.

## Scoped test run

Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll
TaskMaster.Test\bin\Debug\TaskMaster.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll
/InIsolation /Logger:trx /ResultsDirectory:coverage\trx\p1-seams
/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None
/TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser"

vstest resolved through vswhere inline:
`<program-files>\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`.

The filter carries the three shell-icon exclusions because P0-T8 recorded the verdict
`SHELL_ICON_EXCLUSION: REQUIRED`. That exclusion is environmental and is covered by CI, which runs
from a fresh checkout.

EXIT_CODE: 0
ExpectedExitCode: 0

Output Summary: `Test Run Successful.` Total tests: 6545. Passed: 6545. Failed: 0. Skipped: 0.
The console printed no `Failed:` and no `Skipped:` line, which is what a fully green vstest run
prints; the zero counts are read from the `ResultSummary/Counters` element of the produced trx,
which records `total=6545 passed=6545 failed=0 notExecuted=0` with `outcome=Completed`.

Trx: `coverage\trx\p1-seams\<user>_<host>_2026-09-07_01_24_29_net481.trx`. The `coverage`
directory is gitignored; the counts above are transcribed here because the trx itself is not
retained.

### Named tests required by P1-T11

Read from the trx by exact test name:

- `AddQfcColumnsAsync_HappyPath_CompletesWithoutThrowing` — 1 result, outcome `Passed`
- `AddQfcColumnsAsync_PreCancelledToken_CompletesGracefully` — 1 result, outcome `Passed`

Both tests now call `DfDeedle.AddQfcColumnsAsync` directly rather than through reflection, so the
widened six-parameter signature is exercised with its two default arguments applied by the
compiler.

### Issue #780 sporadic test

`DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue` was executed and recorded
outcome `Passed` on the first run.

RERUN_COUNT: 0

No rerun was forced. The rule that a run whose only failure is that test must be rerun rather than
accepted was not triggered, because the run produced no failure at all.

## Acceptance

- Analyzer build `EXIT_CODE: 0` with the summary line `0 Error(s)` — satisfied.
- Test run `EXIT_CODE: 0` with a failed count of 0 — satisfied.
- Reruns recorded — 0 reruns, recorded above.

P1-T13 acceptance satisfied.
