# [P5-T2] Post-Change File Line Counts — PASS

- **Issue:** #424
- **Task:** [P5-T2]
- **Limit:** 500 lines per file (`.claude/rules/general-code-change.md`, `CLAUDE.md`)

Timestamp: 2026-08-07T00-22

Command: `for f in <14 paths>; do wc -l < "$f"; done` (run from repo root, after `dotnet tool run csharpier format .`)

EXIT_CODE: 0

Output Summary: **All 14 files are `<= 500` lines.** Two of the four pre-decided fallback relocations were executed; two were not needed.

## Post-change counts

| File | Baseline | Post-change | Status |
|---|---|---|---|
| `QuickFiler/Controllers/QfcScanProgressBandMapper.cs` | (new) | **79** | OK |
| `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` | 106 | **171** | OK |
| `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` | 139 | **177** | OK |
| `QuickFiler/Controllers/QfcDatamodel.cs` | 479 | **496** | OK (4 lines headroom) |
| `QuickFiler/Controllers/QfcHomeController.cs` | 477 | **487** | OK (13 lines headroom) |
| `QuickFiler/Interfaces/IQfcDatamodel.cs` | 40 | **59** | OK |
| `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.cs` | 300 | **373** | OK |
| `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part2.cs` | (new) | **455** | OK (after relocation) |
| `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part3.cs` | (new, fallback) | **152** | OK |
| `QuickFiler.Test/Controllers/QfcDatamodelTests.cs` | 313 | **317** | OK (after relocation) |
| `QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs` | (new, fallback) | **255** | OK |
| `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncHighConfidenceTests.cs` | 254 | **473** | OK |
| `QuickFiler.Test/Controllers/QfcScanProgressBandMapperTests.cs` | (new) | **204** | OK |
| `QuickFiler.Test/Controllers/QfcHomeControllerIssue218Tests.cs` | 235 | **261** | OK |

## Fallback relocations executed (2 of 4)

**1. `QfcStreamingDequeueConfidenceGateTests.Part2.cs` — EXECUTED.** Measured 584 lines after Phase 2. The three Phase 2 progress-callback tests (`DequeueAsync_ProgressCallback_FiresOncePerScannedCandidateMonotonically`, `DequeueAsync_ProgressCallback_StopsReportingOnceTheMethodReturns`, `DequeueAsync_ThrowingProgressCallback_PropagatesAndLeavesSourceUsable`) were relocated **verbatim** into the new partial `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part3.cs` (no `[TestClass]`, per the `AllowMultiple = false` constraint). Result: Part2 584 -> **455**, Part3 **152**.

**2. `QfcDatamodelTests.cs` — EXECUTED.** Measured 529 lines after Phase 3. The entire `#region Issue #424 — Honest producer-liveness flag` (four tests plus the `WaitForState`, `CreateHighConfidenceGlobals`, `ReadLivenessFlag`, and `StartHeldOpenLoader` helpers) was relocated **verbatim** into the new `[TestClass]` `QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs`, which carries its own `CreateUninitializedDatamodel` / `SetPrivateField` helpers following the existing duplication convention in `QfcInitEmailQueueZeroBatchTests.cs:46-56`. Result: `QfcDatamodelTests.cs` 529 -> **317**, new file **255**.

## Fallback relocations not needed (2 of 4)

**3. `QuickFiler/Controllers/QfcHomeController.cs` — NOT EXECUTED.** Final count 487, within the limit. `RunAsync` was **not** relocated to `QfcHomeController.Run.cs`.

**4. `QuickFiler/Controllers/QfcDatamodel.cs` — NOT EXECUTED.** Final count 496, within the limit. `ScoreRemainingQueueMailItemAsync` was **not** relocated into `QfcDatamodel.QueueProcessing.cs`.

## csproj wiring

Both new test files were added to `QuickFiler.Test/QuickFiler.Test.csproj` (legacy non-SDK project, no globbing):

```xml
<Compile Include="Controllers\QfcDatamodelLivenessTests.cs" />
<Compile Include="Controllers\QfcStreamingDequeueConfidenceGateTests.Part3.cs" />
```

## Verification after relocation

Command: `... /InIsolation /TestCaseFilter:"FullyQualifiedName~QfcStreamingDequeueConfidenceGateTests|FullyQualifiedName~QfcDatamodelTests|FullyQualifiedName~QfcDatamodelLivenessTests"`
EXIT_CODE: 0

Output Summary:

```
Test Run Successful.
Total tests: 33
     Passed: 33
```

21 gate tests (across three partial files) + 8 datamodel tests + 4 liveness tests = 33, matching the pre-relocation totals exactly. No test was lost, renamed, or altered in the move.

One transient CS1513 (`} expected`) occurred while constructing `Part3.cs` — the extraction dropped the final method-closing brace. It was repaired and the toolchain loop restarted from formatting.

## Toolchain state

| Step | Command | EXIT_CODE |
|---|---|---|
| Format | `dotnet tool run csharpier format .` | 0 (`Formatted 1484 files`) |
| Analyzers | `msbuild TaskMaster.sln ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 (0 errors) |
| Nullable | `msbuild TaskMaster.sln ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` | 0 (0 errors) |
| Tests | scoped vstest run above | 0 (33/33) |
