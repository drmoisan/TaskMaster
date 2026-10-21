# P1-T7 — QfcFormControllerDeactivateTests [TestMethod] count

Timestamp: 2026-09-07T14-21
Task: [P1-T7]
Issue: #796
Channel used: A

Command:

```
pwsh -NoProfile -Command '(Select-String -Path QuickFiler.Test\Controllers\QfcFormControllerDeactivateTests.cs -SimpleMatch "[TestMethod]").Count'
```

EXIT_CODE: 0

TESTMETHOD-COUNT: 8

## Derivation

The P0-T12 baseline recorded the file at 248 physical lines declaring 7
`[TestMethod]` members. This task added exactly one test method,
`FormatDeactivationDiagnostics_IncludesEveryDiscriminatingField`, which asserts that
the pure `QfcFormController.FormatDeactivationDiagnostics` output contains the labels
`WebView2Focused=`, `ActiveFormNull=` and `Groups=` and the supplied group count.
7 + 1 = 8, and the measured count is 8.

The test is MSTest, uses FluentAssertions for every assertion, creates no window, no
external process and no temporary file, and calls a pure static method with a fixed
argument tuple, so it is deterministic and order-independent.

Output Summary: QfcFormControllerDeactivateTests declares 8 `[TestMethod]` members
after this task, matching the expected 7 + 1.
