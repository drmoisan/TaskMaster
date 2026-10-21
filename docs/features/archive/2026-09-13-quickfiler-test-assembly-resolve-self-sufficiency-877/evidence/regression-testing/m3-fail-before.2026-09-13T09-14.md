# M3 fail-before: QfcInitEmailQueueZeroBatchTests alone, no runsettings

Timestamp: 2026-09-13T09-14
Command: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /TestCaseFilter:"FullyQualifiedName~QfcInitEmailQueueZeroBatchTests" /InIsolation /ResultsDirectory:<scratch>/m3-pre /Logger:"trx;LogFileName=m3-pre.trx"`
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary: Test Run Failed. Total tests: 3, Failed: 3, Passed: 0. Total time 1.1470 seconds. VSTest version 18.10.0 (x64). All three failures are the same assembly-load failure at Deedle static-initializer time. Head: c9590a8b7, before any fix.

## Why this run is the primary guard

This run selects one class and passes no runsettings file, so no other test class in the assembly has an
opportunity to install a process-global `AssemblyResolve` handler before the Deedle bind is attempted.
A full-suite run cannot prove self-sufficiency, because any earlier class can rescue the bind invisibly.

## Failure text

```
Failed InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing [40 ms]
Failed InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker [< 1 ms]
Failed InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop [2 ms]

System.TypeInitializationException: The type initializer for 'Deedle.Reflection' threw an exception.
 ---> System.TypeInitializationException: The type initializer for '<StartupCode$Deedle>.$FrameUtils' threw an exception.
 ---> System.IO.FileNotFoundException: Could not load file or assembly
      'netstandard, Version=2.1.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
      or one of its dependencies. The system cannot find the file specified.
   at <StartupCode$Deedle>.$FrameUtils..cctor()
   at Deedle.Reflection..cctor()
   at Deedle.Reflection.convertRecordSequence[T](IEnumerable`1 data)
   at QuickFiler.Controllers.Tests.QfcInitEmailQueueZeroBatchTests.CreateTwoRowEmailFrame()
      in QuickFiler\Test\Controllers\QfcInitEmailQueueZeroBatchTests.cs:line 86
```

Host paths in the original console output are redacted to repository-relative form. The raw `.trx` was
written to the session scratch directory and is deliberately not committed, per the issue #671 evidence
decision that only projections are committed.
