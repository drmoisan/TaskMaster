# Coverage wrapper test-instrumentation diagnosis

- Timestamp: `2026-07-23T14-15Z`
- Command: `inspect the stalled P9-T4 process/output state, dotnet-coverage collect --help, the wrapper argument construction, prior issue-#400 raw Cobertura, and prior full-suite coverage evidence`
- EXIT_CODE: `0`
- Output Summary: `The wrapper executes all eight test assemblies but dynamically instruments them; --include-files does not constrain that dynamic profiler; prior evidence proves an instrumentation-only *.Test.dll exclusion permits all 5,849 tests to execute and pass.`

## Stalled P9-T4 run

The first final-QA coverage command ran
`scripts/vscode/Invoke-MSTestWithCoverage.ps1` for 30 minutes without producing its
requested Cobertura file. At diagnosis time:

- the workspace-owned `dotnet-coverage` and `vstest.console` processes were both idle;
- no workspace testhost process was active;
- no Cobertura output existed;
- only the two verified workspace-owned processes were terminated;
- the unrelated Visual Studio design-mode VSTest process was not touched.

This was a coverage-harness stall, not a test failure. The user independently reported
that the complete suite passed in Visual Studio.

## Root cause

`Invoke-MSTestWithCoverage.ps1` passes canonical `coverage.config` to outer
`dotnet-coverage --settings`, then passes all discovered `*.Test.dll` assemblies to
VSTest. Canonical settings do not exclude test assemblies from instrumentation.

`dotnet-coverage --include-files` is a static-instrumentation input, not a dynamic
profiler allowlist. Current issue evidence proves the distinction:

- `popup-ui-boundary-core-adapter-audit.2026-07-22T02-20.md` records
  `--include-files ...\QuickFiler.dll`;
- the associated raw Cobertura still contains both `QuickFiler` and
  `QuickFiler.Test`.

Repeating `--include-files` for production DLLs therefore cannot prevent dynamic
instrumentation of test assemblies.

The authoritative full-suite evidence records three stalls while test assemblies were
instrumented. It then temporarily added
`<ModulePath>.*\.Test\.dll$</ModulePath>` to instrumentation exclusions, still passed all
eight test DLLs to VSTest, and completed `5,849/5,849` tests. Canonical
`coverage.config` was restored afterward.

## Current assembly inventory

The wrapper currently discovers these eight Debug assemblies:

1. `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`
2. `Tags.Test\bin\Debug\Tags.Test.dll`
3. `TaskMaster.Test\bin\Debug\TaskMaster.Test.dll`
4. `TaskTree.Test\bin\Debug\TaskTree.Test.dll`
5. `TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll`
6. `ToDoModel.Test\bin\Debug\ToDoModel.Test.dll`
7. `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll`
8. `VBFunctions.Test\bin\Debug\VBFunctions.Test.dll`

All eight must remain after the `-- <vstest-path>` boundary. The correction changes
instrumentation only; it must not change test discovery or filtering.

## Authorized correction

The wrapper will:

1. read canonical `coverage.config` without writing it;
2. derive effective XML in memory;
3. retain every canonical exclusion;
4. add exactly one `.*\.Test\.dll$` instrumentation-only `ModulePath`;
5. write that effective settings file beside the requested Cobertura output;
6. pass its path to outer `dotnet-coverage --settings`;
7. execute all eight test assemblies unchanged;
8. remove the verified effective path in `finally` on success or failure.

Canonical `coverage.config` SHA-256 is
`B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943`
and is a byte-preservation gate. No post-processing, runsettings, test filter, threshold,
or canonical exclusion will change.
