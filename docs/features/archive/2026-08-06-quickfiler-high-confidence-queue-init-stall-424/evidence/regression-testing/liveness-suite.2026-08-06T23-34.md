# [P3-T8] QfcDatamodelTests Suite — Full Run + [P3-T5] Diff Verification

- **Issue:** #424
- **Task:** [P3-T8]
- **Scope:** `QuickFiler.Test/Controllers/QfcDatamodelTests.cs`

Timestamp: 2026-08-06T23-34

Command: `"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /Settings:"scripts\vscode\TaskMaster.cli.runsettings" /InIsolation /TestCaseFilter:"FullyQualifiedName~QfcDatamodelTests"`

EXIT_CODE: 0

Output Summary:

```
Test Run Successful.
Total tests: 12
     Passed: 12
```

| Test | Status | Category |
|---|---|---|
| `TryQueueRemainingMailItemAsync_HighConfidenceEnabled_AddsAndHooksWithoutScoring` | Passed | admission (#233) — **unmodified** |
| `TryQueueRemainingMailItemAsync_HighConfidenceEnabled_IgnoresThresholdAtAdmission` | Passed | admission (#233) — **unmodified** |
| `DequeueNextItemGroupAsync_HighConfidenceMode_WaitsWhileSourceWorkerActive` | Passed | pinned polling test — retargeted by [P3-T5] |
| `TryQueueRemainingMailItemAsync_HighConfidenceEnabled_AddsBelowThresholdCandidate` | Passed | admission (#233) — **unmodified** |
| `TryQueueRemainingMailItemAsync_HighConfidenceDisabled_AddsAndHooksWithoutScoring` | Passed | admission (#233) — **unmodified** |
| `TryQueueRemainingMailItemAsync_NullMailItem_DoesNotScoreAddOrHook` | Passed | admission (#218) — **unmodified** |
| `ToggleOfflineMode_WhenOnline_AwaitsInjectedFiveMillisecondDelay` | Passed | TimeProvider seam (#222) — **unmodified** |
| `WaitForQueue_WhenWorkerBusyAndQueueShort_AwaitsInjectedTwoHundredMsDelay` | Passed | TimeProvider seam (#222) — retargeted by [P3-T6] |
| `DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle` | Passed | new, [P3-T1] fail-before/pass-after |
| `RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces` | Passed | new, [P3-T7](a) |
| `RemainingLoadActive_AfterLoaderCompletes_BecomesFalse` | Passed | new, [P3-T7](b) |
| `RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally` | Passed | new, [P3-T7](c) |

## [P3-T5] diff verification — admission tests untouched

Command: `git diff -U0 -- QuickFiler.Test/Controllers/QfcDatamodelTests.cs | grep "^@@"`
EXIT_CODE: 0

Output Summary — every hunk header, with its authorization:

```
@@ -116 +115,0 @@     ] pinned polling test (baseline 103-136)
@@ -119,0 +119,3 @@    ]   authorized by [P3-T5]
@@ -131 +133 @@        ]
@@ -290 +291,0 @@      ] WaitForQueue test (baseline 281-309)
@@ -292,0 +294,3 @@    ]   authorized by [P3-T6]
@@ -303 +307 @@        ]
@@ -305 +309 @@        ]
@@ -311,0 +316,212 @@   ] new "#424 Honest producer-liveness flag" region appended
                        ]   authorized by [P3-T1] and [P3-T7]
```

**No hunk intersects the admission-never-scores regions.** Baseline lines 49-100 and 139-217 are untouched; the nearest modification begins at baseline line 116.

Byte-identity confirmed directly against `HEAD`:

Command: `git show HEAD:QuickFiler.Test/Controllers/QfcDatamodelTests.cs | sed -n '48,100p'` vs `sed -n '48,100p'` on the worktree file; likewise baseline `138,217` vs worktree `140,219`
EXIT_CODE: 0

Output Summary:
```
region 49-100:   BYTE-IDENTICAL
region 139-217:  BYTE-IDENTICAL
```

The issue #233 admission-never-scores contract is therefore preserved exactly, satisfying the spec's "Admission tests (49-100, 139-217) must NOT change" requirement.

## Changes made to this file

1. **[P3-T5]** `DequeueNextItemGroupAsync_HighConfidenceMode_WaitsWhileSourceWorkerActive` — the `SetPrivateField(worker, "isRunning", ...)` reflection pin is replaced with `SetPrivateField(model, "_remainingLoadActive", ...)`. Assertions and structure are otherwise unchanged.
2. **[P3-T6]** `WaitForQueue_WhenWorkerBusyAndQueueShort_AwaitsInjectedTwoHundredMsDelay` — same substitution, required because `[P3-T3]` rewired `WaitForQueue` (`QfcDatamodel.QueueProcessing.cs:132`) to consume the flag, which defaults `false`. The `FakeTimeProvider` 200 ms delay assertion is intact; no other assertion changed. Without this update the test would fail, since the loop would exit immediately.
3. **[P3-T1] / [P3-T7]** A new `#region Issue #424 — Honest producer-liveness flag` appended with four tests and three helpers (`WaitForState`, `CreateHighConfidenceGlobals`, `ReadLivenessFlag`, `StartHeldOpenLoader`).

One CS0136 compile error was encountered and fixed during this phase: a lambda parameter named `release` shadowed the enclosing `out TaskCompletionSource<bool> release`; the lambda parameter was renamed to `signal` and the toolchain loop restarted from formatting.

## Toolchain state

| Step | Command | EXIT_CODE |
|---|---|---|
| Format | `dotnet tool run csharpier format .` | 0 (`Formatted 1480 files`) |
| Analyzers | `msbuild TaskMaster.sln ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 (0 errors) |
| Nullable | `msbuild TaskMaster.sln ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` | 0 (0 errors) |
| Tests | scoped vstest run above | 0 (12/12) |

## File-size status carried to [P5-T2]

`QfcDatamodelTests.cs` measures **529 lines** after formatting, above the 500-line limit. This is the condition `[P5-T2]`'s pre-decided fallback anticipates: the Phase 3 liveness tests are relocated verbatim into a new `[TestClass]` `QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs` carrying its own `CreateUninitializedDatamodel` / `SetPrivateField` helpers, with a `<Compile Include>` added to `QuickFiler.Test/QuickFiler.Test.csproj`.
