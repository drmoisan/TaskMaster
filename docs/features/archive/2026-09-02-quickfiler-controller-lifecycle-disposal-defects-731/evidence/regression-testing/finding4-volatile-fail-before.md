# Finding 4 — reentrancy counter Volatile.Read proxy, failing run before the fix

Timestamp: 2026-09-03T14-31

Task: [P4-T4] [expect-fail]
Issue: #731

## Command

1. Build:

```
msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"
```

MSBuild executable actually invoked: `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`, recorded in full under the Evidence path-hygiene rule's stated exception for an external build-tool executable.

2. Filtered test run:

```
<vstest> QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~ReentrancyCounterSoleReadGoesThroughVolatileRead"
```

vstest console: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe` (VSTest version 18.9.0, x64).

EXIT_CODE: 1

ExpectedExitCode: 1

The build exited 0 and the test run exited 1, which is the expected outcome for this task: the bare read has not been replaced yet.

## Output Summary

```
  Failed ReentrancyCounterSoleReadGoesThroughVolatileRead [194 ms]

Total tests: 1
     Failed: 1
Test Run Failed.
```

- Total tests: **1**
- Passed: **0**
- Failed: **1**

Observed failure message. FluentAssertions' `Contain` renders the whole subject before the expectation, and the subject here is the entire whitespace-normalised text of a 2328-line source file; that dump is elided below and replaced by the marker `[normalised source elided]`. The elision is presentational only — it removes no diagnostic content, since the operative clause is the expectation and the reason. Absolute paths in the surrounding output were rewritten to their repository-relative remainder under the Evidence path-hygiene rule.

```
Expected normalized [normalised source elided] to contain
"Volatile.Read(ref removespecificcontrolgroupcounter)" because issue #731 finding 4 requires the
sole read of the reentrancy counter to go through an explicit acquire.
```

The test failed for exactly the reason this task requires: the source still contains the bare, unsynchronised read. Before the fix the guard reads `if (removespecificcontrolgroupcounter > 1)` at `QuickFiler/Controllers/QfcCollectionController.cs:992` (baseline line 991, shifted by one by the [P1-T1] comment insertion), and no `Volatile.Read` of that counter exists anywhere in the file. The reproduction is genuine: the assertion reads the real source text and can pass only once [P4-T5] rewrites that guard.

The remaining three assertions in the test were not reached, because the first assertion in the chain threw. Their pre-fix state is nonetheless determinate and is recorded here for completeness: the two `Interlocked` writes are present at `:914` and `:1009`, and the field declaration at `:910` reads `private static int removespecificcontrolgroupcounter = 0;` and carries no `volatile` modifier.
