# [P6-T3] AC7 Compile-Red Fail-Before

Timestamp: 2026-09-08T10-13
Command: `msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Rebuild /m /p:Configuration=Debug /p:Platform=AnyCPU` (the [P1-T2] command)
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary: The build fails with 6 errors and 0 warnings. Every error is a CS0246 naming `BreadcrumbPopupOwnerRegistry`, raised at the six sites in the new test file that construct the type. This is the required fail-before observation for this `[expect-fail]` task: a runtime-red run is structurally impossible because the type under test does not exist yet, so the compiler's own report is the fail-before.

FAILURE-DIAGNOSTIC: `error CS0246: The type or namespace name 'BreadcrumbPopupOwnerRegistry' could not be found (are you missing a using directive or an assembly reference?)`, raised 6 times in `QuickFiler.Test\Viewers\BreadcrumbPopupOwnerRegistryTests.cs`, at line 34 column 32, line 53 column 32, line 74 column 32, line 96 column 32, line 121 column 32 and line 157 column 32 — the six `new BreadcrumbPopupOwnerRegistry()` construction sites, one per test method. Absolute paths are not reproduced here per D14.

Verbatim error summary:

```
    0 Warning(s)
    6 Error(s)

Time Elapsed 00:00:14.58
```

The six sites were enumerated from a second run of the same command, made because the first pass of this record named two line numbers that had not been read off the build output. The two runs agree on the exit code, on the error and warning counts, and on the diagnostic. MSBuild prints each error twice, once inline and once in its error summary, so a raw match count over the output reports 12; the authoritative figure is the `6 Error(s)` line above and the six distinct sites listed.

## Why the exit code alone is not the assertion

Under the [P1-T2] command as corrected by D20 the build reaches `CoreCompile`, so the `EXIT_CODE: 1` recorded here is the compiler's own failure and the CS0246 clause is the discriminating assertion. Two observations from this run establish that directly:

- The build output contains `CoreCompile`, so compilation was entered rather than skipped.
- The build output contains no occurrence of `_CheckForInvalidConfigurationAndPlatform`. Match count: 0.

That second observation is what the D20 correction bought. Under the command as originally written the build failed inside `_CheckForInvalidConfigurationAndPlatform` because `QuickFiler.Test.csproj` declares no `Debug|Any CPU` property group, `OutputPath` was never set, and the run exited 1 having emitted no CS0246 at all. The exit-code clause of this task would have been satisfied by that failure while its diagnostic clause was unsatisfiable, so the task would have appeared to pass on a coincidence. It now fails for the reason AC7 is about.

## Registration is confirmed by the failure itself

The six diagnostics prove the new test file entered the compilation. An unregistered file compiles to nothing and would have produced a clean build here, which would have made this fail-before vacuous. [P6-T2] added the `<Compile Include="Viewers\BreadcrumbPopupOwnerRegistryTests.cs" />` item that puts it in the compilation, and this run is the behavioural evidence that the item took effect.

## Expectation

This task is tagged `[expect-fail]`. A failing build is its required outcome and `ExpectedExitCode: 1` declares that expectation, so the observed `EXIT_CODE: 1` normalizes to a pass. The compile-red window opened here is closed by [P6-T7], and the tests are run green by [P6-T8].

## D5 file-lock check

The failure is a compiler diagnostic, not a locked output. No MSB3061 or MSB3021 warning was reported, so the D5 stop condition did not fire and no process was terminated.
