# [P1-T2] Deadline Regression Test — FAIL BEFORE (AC 11 evidence)

- **Issue:** #424
- **Task:** [P1-T2] `[expect-fail]`
- **Test:** `QuickFiler.Controllers.Tests.QfcStreamingDequeueConfidenceGateTests.DequeueAsync_LowYieldStream_StopsScanningAtDefaultFirstBatchDeadline`
- **Test file:** `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part2.cs`
- **Production state:** UNMODIFIED. `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` is at its pre-fix baseline (106 lines, no deadline parameter). The test compiles against the pre-fix constructor through the existing reflection-based `CreateGate` helper.

Timestamp: 2026-08-06T22-41

Command: `"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /Settings:"scripts\vscode\TaskMaster.cli.runsettings" /InIsolation /TestCaseFilter:"FullyQualifiedName~DequeueAsync_LowYieldStream_StopsScanningAtDefaultFirstBatchDeadline"`

EXIT_CODE: 1

Output Summary:

```
Failed DequeueAsync_LowYieldStream_StopsScanningAtDefaultFirstBatchDeadline [268 ms]
Error Message:
 Expected takeCount to be less than or equal to 13 because the 12 s first-batch deadline at
 1 s per score bounds the scan to 12 candidates plus at most one in-flight candidate, instead
 of scanning the whole source, but found 51 (difference of 38).

Test Run Failed.
Total tests: 1
     Failed: 1
 Total time: 1.3086 Seconds
```

Assertion site: `QfcStreamingDequeueConfidenceGateTests.Part2.cs:83`.

## Why this is the bug

The scenario streams 50 candidates with exactly one qualifier at position 40, `quantity = 5`, and a score loader that advances the shared `FakeTimeProvider` by 1000 ms per candidate — a low-yield folder, the condition the issue reports.

- **Observed (pre-fix): 51 `tryTakeNext` invocations.** The gate consumed all 50 candidates and then took once more to observe source exhaustion. Because only 1 of 50 qualifiers exists and `quantity = 5` is never satisfied, the loop ran to exhaustion. At the modeled 1 s per score this is a **51-second** pre-UI wait for a single displayed item, and the scan length is bounded only by folder size — exactly the unbounded behavior described in `spec.md` Root Cause Analysis and research §3 (`E[|scanned|] ~ min(N, ItemsPerIteration / p)`).
- **Required (post-fix): at most 13 invocations.** The 12-second `DefaultFirstBatchDeadline` at 1 s per score admits 12 completed scores, plus at most one in-flight candidate.

The difference of 38 is the measured gap between the unbounded scan and the bounded one.

## Determinism

No wall-clock dependence: every millisecond of the modeled budget is advanced explicitly via `FakeTimeProvider.Advance` inside the injected score loader. No `Thread.Sleep`, no `Task.Delay`, no temp files, no Outlook COM. `MailItem` candidates are Moq objects built by the existing `CreateMailItem` helper. The run used `/InIsolation`, required for these Moq-based assemblies.

## Toolchain state at capture

Preceding the run, with the new test file in place and production code untouched:

| Step | Command | EXIT_CODE |
|---|---|---|
| Format | `dotnet tool run csharpier format .` | 0 (`Formatted 1480 files`) |
| Analyzers | `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 (0 errors) |
| Nullable | `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` | 0 (0 errors) |

The failing result is therefore a genuine behavioral assertion failure, not a compile or configuration artifact.

**This is the AC 11 fail-before evidence.** The pass-after counterpart is recorded in `deadline-pass-after.<ts>.md` by `[P1-T4]`.
