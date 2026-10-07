# P5-T4 EfcDataModelFilerCleanupTests Created and Census

Timestamp: 2026-10-03T12-21
Command: Write tool created QuickFiler.Test\Controllers\EfcDataModelFilerCleanupTests.cs from Listing L-TEF (four leading spaces stripped); then CMD-CENSUS (PATHS-TEF with TOKENS-TEF)
EXIT_CODE: 0 (scoped to the CMD-CENSUS payload, its process exit code)
Output Summary: TEF was created from Listing L-TEF (192 lines; the verbatim literal `@"\\mailbox@example.com\Archive"` was confirmed on disk with its doubled backslash). Every TOKENS-TEF total equals its expectation. COMPILE-RED SPAN OPEN: the last completed task is P5-T4; QuickFiler.Test\Controllers\EfcDataModelFilerCleanupTests.cs does not compile until P5-T6 lands the `ResetFilerPromptState` seam it overrides (and it is not yet registered in QuickFiler.Test.csproj, which P5-T5 does).

```
TOKEN [TestMethod] @ TOTAL = 3
TOKEN MoveToFolderAsync_WhenFilerThrows_ResetsPromptStateAndPropagates @ TOTAL = 1
TOKEN MoveToFolderAsync_WhenFilerSucceeds_ResetsPromptStateOnce @ TOTAL = 1
TOKEN MoveToFolderAsync_WhenAGuardReturnsFalse_DoesNotResetPromptState @ TOTAL = 1
TOKEN overrideTask<bool>InvokeFilerAsync( @ TOTAL = 1
TOKEN overridevoidResetFilerPromptState() @ TOTAL = 1
TOKEN ResetCalls.Should().Be(1) @ TOTAL = 2
TOKEN ResetCalls.Should().Be(0) @ TOTAL = 1
TOKEN Task.FromException<bool>( @ TOTAL = 1
TOKEN SpecialFoldersWithoutOneDrive() @ TOTAL = 2
TOKEN DoNotParallelize @ TOTAL = 0
TOKEN Thread.Sleep @ TOTAL = 0
TOKEN Task.Delay @ TOTAL = 0
TOKEN Timeout @ TOTAL = 0
TOKEN Directory.CreateDirectory @ TOTAL = 0
TOKEN File. @ TOTAL = 0
TOKEN MemoryAppender @ TOTAL = 0
LINES QuickFiler.Test\Controllers\EfcDataModelFilerCleanupTests.cs = 192
SHA256 QuickFiler.Test\Controllers\EfcDataModelFilerCleanupTests.cs = 95B0CA83059042DD81279CE370FE0FD2D3AF9A533A2FC3A34261F5698D614B3F
```

(Each token's single per-file line carries the same value as its TOTAL line.)

## Acceptance (P5-T4)

Every TOTAL equals the TOKENS-TEF expectation (`[TestMethod]` 3, the three test names 1 each, `overrideTask<bool>InvokeFilerAsync(` 1, `overridevoidResetFilerPromptState()` 1, `ResetCalls.Should().Be(1)` 2, `ResetCalls.Should().Be(0)` 1, `Task.FromException<bool>(` 1, `SpecialFoldersWithoutOneDrive()` 2, the banned tokens 0): met.
