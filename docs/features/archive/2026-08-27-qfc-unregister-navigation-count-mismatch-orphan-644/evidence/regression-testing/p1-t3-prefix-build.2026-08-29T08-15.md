# Regression testing — Pre-fix build ([P1-T3])

- Issue: #644
- Task: `[P1-T3]`
- Timestamp: 2026-08-29T08-15

Purpose: prove that the new regression file compiles against **unmodified production code**. No
production file has been edited at this point; Phase 2 has not started.

Command: `msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
Working directory: repository root (`<repo-root>`)
Shell: PowerShell (`pwsh -NoProfile`)
EXIT_CODE: 0

## msbuild final summary block

```
    5 Warning(s)
    0 Error(s)

Time Elapsed 00:00:03.15
```

**0 errors**, as the acceptance requires. The 5 warnings are the same pre-existing
`System.Reactive` `packages.config` advisory recorded in `[P0-T9]` and `[P0-T10]`; the count is
unchanged from both baselines.

## The build was not vacuously incremental

`/t:Build` is the command this task names, and `/t:Build` is subject to MSBuild's incremental
up-to-date check, so a fast elapsed time invites the question of whether `QuickFiler.Test` was
actually recompiled. It was, and this was verified rather than assumed:

```
csc-total=2
ledger-in-csc=2
errors=0
```

The captured log carries `csc.exe` invocations, and
`QfcCollectionControllerNavigationLedgerTests.cs` appears on the compiler command line. The
`Compile Include` item added by `[P1-T2]` changed the project's input file set, which is what
invalidated the up-to-date check. The rebuilt assembly's timestamp confirms it:

```
QuickFiler.Test\bin\Debug\QuickFiler.Test.dll  last written 2026-08-29T13:49:17
```

Using `/t:Build` here rather than `/t:Rebuild` is what the task text specifies, and it is
appropriate: this task's gate is compilation, not analyzer or nullable enforcement. The two gates
that require analyzers and nullable-flow diagnostics to actually run — `[P4-T3]` and `[P4-T4]` —
both use `/t:Rebuild`, as do their `[P0-T9]` and `[P0-T10]` baselines.

Output Summary: The solution built with **exit code 0 and 0 errors**. `QuickFiler.Test`
recompiled with `QfcCollectionControllerNavigationLedgerTests.cs` on the `csc.exe` command line,
confirming the `[P1-T2]` project registration takes effect and that the six new tests compile
against unmodified production code. The assembly is now ready for the `[P1-T4]` expect-fail run.
