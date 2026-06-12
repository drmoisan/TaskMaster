# Regression — Fail-Before (Issue #183, AC3)

Timestamp: 2026-06-10T09-13

Command (canonical): `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /Tests:TrainSelectionAsync_WhenSelectionContainsTwoMailItemsWithSameConversationId_WritesTriageUdfToEveryItem`
Command (executed): `MSYS_NO_PATHCONV=1 "C:/Program Files/Microsoft Visual Studio/18/Community/Common7/IDE/Extensions/TestPlatform/vstest.console.exe" "UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" /InIsolation /Tests:TrainSelectionAsync_WhenSelectionContainsTwoMailItemsWithSameConversationId_WritesTriageUdfToEveryItem`

EXIT_CODE: 1 (non-zero — expected failure against the UNMODIFIED Triage_OlLogic.cs)

## Output Summary

The new regression test FAILS as expected before the fix.

- Total tests: 1; Failed: 1.
- Assertion failure (Moq):
  `Moq.MockException: Expected invocation on the mock once, but was 0 times: m => m.Save()`
- The failing mock is the SECOND MailItem (`Mock<MailItem:2>`). Its only performed invocation was `_MailItem.ConversationID` — i.e., the second item was read solely for the `.GroupBy(m => m.ConversationID).Select(g => g.First())` dedup and then dropped. The Triage UDF write path (`SetUdf -> ... -> MailItem.Save()`) is never reached for the second item.

This is the precise pre-fix defect described in issue #183: the `ConversationID` dedup (introduced for #137) suppresses the user-visible Triage UDF write for every item after the first in a conversation. `mockMailItem1.Verify(m => m.Save(), Times.Once)` (the first item) passes; `mockMailItem2.Verify(m => m.Save(), Times.Once)` (the second item) is the assertion that fails, proving the second same-conversation item is not triaged.

Seam note: `SetUdf("Triage", "A")` is an extension method and cannot be verified directly via Moq. `MailItem.Save()` is the chosen interceptable observable proxy, reached only when the per-item UDF write path completes.
