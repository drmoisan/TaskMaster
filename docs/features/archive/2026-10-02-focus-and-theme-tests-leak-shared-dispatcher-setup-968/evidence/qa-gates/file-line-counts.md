# File-size gate (issue #968, task P8-T8)

Timestamp: 2026-10-03T03-32
Command: pwsh -NoProfile -Command '<CMD-LINECOUNT payload>' with FILES = CS13, then pwsh -NoProfile -Command '<CMD-TOKEN-COUNT payload>' on PROJ with TOKENS `"<Compile Include="` (both the Command Reference macros executed verbatim with PREFIX expanded and WORKTREE substituted)
Canonical command: CMD-LINECOUNT on CS13; CMD-TOKEN-COUNT on QuickFiler.Test\QuickFiler.Test.csproj
EXIT_CODE: 0
Output Summary:
- WORKTREE-LEAF: agent-a291a7fbabf9d0229 (both payloads)

| File | LINES | P6-T2 value | At most 500 |
|---|---|---|---|
| QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixture.cs | 375 | 375 | yes |
| QuickFiler.Test\Controllers\QfcItemController.FocusAndThemeTests.cs | 482 | 482 | yes |
| QuickFiler.Test\Controllers\QfcItemController.TestSupport.cs | 442 | 442 | yes |
| QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs | 472 | 472 | yes |
| QuickFiler.Test\Controllers\QfcItemController.UiThreadDispatcherPinCountTests.cs | 248 | 248 | yes |
| QuickFiler.Test\Controllers\QfcDatamodelLivenessTests.cs | 352 | 352 | yes |
| QuickFiler.Test\Controllers\QfcDatamodelTeardownTests.cs | 229 | 229 | yes |
| QuickFiler.Test\Controllers\QfcInitEmailQueueZeroBatchTests.cs | 226 | 226 | yes |
| QuickFiler.Test\Controllers\QfcDatamodelTests.cs | 394 | 394 | yes |
| QuickFiler\Controllers\QfcDatamodel.cs | 367 | 367 | yes (and at most 400) |
| QuickFiler\Controllers\QfcDatamodel.QueueProcessing.cs | 413 | 413 | yes |
| QuickFiler.Test\TestSupport\SynchronousBackgroundWorker.cs | 27 | 27 | yes |
| QuickFiler.Test\TestSupport\ArmingFakeTimeProvider.cs | 49 | 49 | yes |

- Every value equals the P6-T2 value for the same file (the P8-T1 format rewrote nothing).
- QuickFiler\Controllers\QfcDatamodel.cs: 367, at most 400, beside QFCDATAMODEL-LINES-BEFORE: 495 (P4-T1) (AC28)
- Project file `<Compile Include=` count: 190 = PROJ-COMPILE-ITEMS-BASE 187 plus 3 (the project file is not a C# source file and the 500-line limit does not apply to it)
