# [P2-T5] Gate Progress Suite — Full Run

- **Issue:** #424
- **Task:** [P2-T5]
- **Scope:** both files under `QuickFiler.Test/Controllers/` forming the partial class — `QfcStreamingDequeueConfidenceGateTests.cs` + `.Part2.cs`

Timestamp: 2026-08-06T23-14

Command: `"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /Settings:"scripts\vscode\TaskMaster.cli.runsettings" /InIsolation /TestCaseFilter:"FullyQualifiedName~QfcStreamingDequeueConfidenceGateTests"`

EXIT_CODE: 0

Output Summary:

```
Test Run Successful.
Total tests: 21
     Passed: 21
 Total time: 1.2814 Seconds
```

Pass counts: **21 passed, 0 failed, 0 skipped.** Up from 18 after Phase 1 — the three Phase 2 tests added by `[P2-T3]` and `[P2-T4]` all pass, and all 18 earlier tests (8 pre-existing + 10 Phase 1) continue to pass unmodified.

## Phase 2 tests added

| Test | Task | AC 5 clause covered |
|---|---|---|
| `DequeueAsync_ProgressCallback_FiresOncePerScannedCandidateMonotonically` | P2-T3 | once per scanned candidate including rejects; `scanned` sequence `1,2,3,4,5`; `accepted` sequence `0,1,1,2,2` (monotonically non-decreasing); `quantity` reported unchanged |
| `DequeueAsync_ProgressCallback_StopsReportingOnceTheMethodReturns` | P2-T3 | no invocation after return, asserted on the deadline-expiry path (3 reports under a 3 s budget, count final after the await completes) |
| `DequeueAsync_ThrowingProgressCallback_PropagatesAndLeavesSourceUsable` | P2-T4 | a throwing callback propagates the same exception instance out of `DequeueAsync`; the un-taken remainder of the source is still takeable afterwards |

## Implementation notes verified by these tests

- The callback is `Action<int, int, int> progressCallback = null` on the widest constructor; `null` disables reporting, so every pre-existing test path is unaffected.
- It is invoked **after** the accept decision (`QfcStreamingDequeueConfidenceGate.cs`, immediately following the `score >= _cutoff` block), so the `accepted` value it reports includes the candidate just scored.
- **No try/catch surrounds the invocation.** Exceptions propagate to the caller by design (fail fast per `.claude/rules/general-code-change.md` and `csharp.md`), and this is pinned by the throwing-callback test rather than left implicit.
- `scanned` is incremented once per scored candidate only — empty polls and the deadline exit do not increment it.

## Toolchain state

| Step | Command | EXIT_CODE |
|---|---|---|
| Format | `dotnet tool run csharpier format .` | 0 (`Formatted 1480 files`) |
| Analyzers | `msbuild TaskMaster.sln ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 (0 errors) |
| Nullable | `msbuild TaskMaster.sln ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` | 0 (0 errors) |
| Tests | scoped vstest run above | 0 (21/21) |

## File-size status carried to [P5-T2]

`QfcStreamingDequeueConfidenceGateTests.Part2.cs` now measures **584 lines**, above the 500-line limit. This is the condition `[P5-T2]`'s pre-decided fallback anticipates: the three Phase 2 progress-callback tests are relocated verbatim into `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part3.cs` (partial, no `[TestClass]`), with a `<Compile Include>` added to `QuickFiler.Test/QuickFiler.Test.csproj`. That relocation is executed in Phase 5, returning `Part2.cs` to its pre-Phase-2 size of 455 lines.
