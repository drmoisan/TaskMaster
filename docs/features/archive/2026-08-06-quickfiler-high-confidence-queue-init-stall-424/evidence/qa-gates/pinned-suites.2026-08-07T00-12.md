# [P5-T1] Pinned Suites — PASS

- **Issue:** #424
- **Task:** [P5-T1]
- **Supersedes:** `pinned-suites.2026-08-06T23-58.md` (fail-closed record from the blocked first attempt, retained for audit)

Timestamp: 2026-08-07T00-12

## Diff verification

Command: `git status --porcelain` (against the `[P0-T3]` baseline; the pre-existing `.claude/agent-memory/` and feature-folder entries recorded there are permitted and ignored)
EXIT_CODE: 0

Output Summary — byte-identity asserted for the four listed files only:

```
byte-unmodified: QfcHomeControllerIterationTests.cs
byte-unmodified: QfcInitEmailQueueZeroBatchTests.cs
byte-unmodified: QfcHighConfidencePreFilterTests.cs
byte-unmodified: QfcFormControllerTests.cs
```

`QfcHomeControllerIssue218Tests.cs` is correctly **excluded** from the byte-unmodified list — it was reclassified as in-scope (Decisions Record item 14) and updated by `[P4-T7]`. Its diff is present and expected:

```
 M QuickFiler.Test/Controllers/QfcHomeControllerIssue218Tests.cs
```

## Suite run

Command: `"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /Settings:"scripts\vscode\TaskMaster.cli.runsettings" /InIsolation /TestCaseFilter:"FullyQualifiedName~QfcHomeControllerIterationTests|FullyQualifiedName~QfcInitEmailQueueZeroBatchTests|FullyQualifiedName~QfcHighConfidencePreFilterTests|FullyQualifiedName~QfcFormControllerTests|FullyQualifiedName~QfcHomeControllerIssue218Tests"`

EXIT_CODE: 0

Output Summary:

```
Test Run Successful.
Total tests: 64
     Passed: 64
```

Per-suite pass counts:

| Pinned suite | Tests | Result | Diff status |
|---|---|---|---|
| `QfcHomeControllerIterationTests` | 8 | 8 passed | byte-unmodified |
| `QfcInitEmailQueueZeroBatchTests` | 3 | 3 passed | byte-unmodified |
| `QfcHighConfidencePreFilterTests` | 10 | 10 passed | byte-unmodified |
| `QfcFormControllerTests` | 41 | 41 passed | byte-unmodified |
| `QfcHomeControllerIssue218Tests` | 2 | 2 passed | overload-shape hunks only ([P4-T7]) |
| **Total** | **64** | **64 passed, 0 failed** | |

## Exact-argument iteration pin

Command: `... /TestCaseFilter:"FullyQualifiedName~IterateQueueAsync_WhenDequeueReturnsFullQualifiedPage_EnqueuesAllItems|FullyQualifiedName~DequeueAsync_PropagatesCancellationBeforeTakingSourceItem|FullyQualifiedName~DequeueAsync_CancelledDuringEmptyQueueWait|FullyQualifiedName~DequeueAsync_CancelledDuringScoring"`
EXIT_CODE: 0

Output Summary:

```
Passed DequeueAsync_PropagatesCancellationBeforeTakingSourceItem [58 ms]
Passed DequeueAsync_CancelledDuringEmptyQueueWait_ThrowsOperationCanceled [8 ms]
Passed DequeueAsync_CancelledDuringScoring_ThrowsOperationCanceled [63 ms]
Passed IterateQueueAsync_WhenDequeueReturnsFullQualifiedPage_EnqueuesAllItems [272 ms]
Test Run Successful.
Total tests: 4
     Passed: 4
```

- The exact-argument pin `DequeueNextItemGroupAsync(8, 2000)` at `QfcHomeControllerIterationTests.cs:268` **passes**, confirming the post-UI iteration call site is untouched.
- The pre-existing gate cancellation test passes **without behavioral modification**, alongside the two cancellation tests added by `[P1-T8]`.

## [P4-T7] diff scope confirmation

Command: `git diff -U0 -- QuickFiler.Test/Controllers/QfcHomeControllerIssue218Tests.cs | grep "^@@"`
EXIT_CODE: 0

Output Summary — exactly four hunks, at the four sites named in `[P4-T7]`:

```
@@ -101 +101,8 @@
@@ -160 +167,7 @@
@@ -192 +205,8 @@
@@ -226 +246,7 @@
```

Every removed line is one of the four old two-argument overload calls:

```
-                .Setup(x => x.DequeueNextItemGroupAsync(It.IsAny<int>(), It.IsAny<int>()))
-                    m => m.DequeueNextItemGroupAsync(It.IsAny<int>(), It.IsAny<int>()),
-                .Setup(x => x.DequeueNextItemGroupAsync(It.IsAny<int>(), It.IsAny<int>()))
-                m => m.DequeueNextItemGroupAsync(It.IsAny<int>(), It.IsAny<int>()),
```

A filtered scan of added lines returned **zero** lines outside overload-argument syntax, confirming the `preFilterInvoked` assertion, both `LoadItemsAsync` overload-discipline assertions, and the `Times.Once` counts are unmodified. The issue #218 intent — no pre-filter, plain `MailItem` load path, first displayed page from the dequeue-layer gate — is preserved.

## Toolchain state

| Step | Command | EXIT_CODE |
|---|---|---|
| Format | `dotnet tool run csharpier format .` | 0 (`Formatted 1482 files`) |
| Analyzers | `msbuild TaskMaster.sln ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` | 0 (0 errors) |
| Nullable | `msbuild TaskMaster.sln ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` | 0 (0 errors) |
| Tests | scoped vstest runs above | 0 (64/64, 4/4) |
