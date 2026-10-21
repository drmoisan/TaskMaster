# P8-T10 — GREEN for issue #490: the five previously-red tests

Timestamp: 2026-08-28T01-26
Command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation "/TestCaseFilter:FullyQualifiedName~IItemViewer_DeclaresAddFolderItemsAndNotSetFolderItems|FullyQualifiedName~IItemViewer_FocusSubjectReturnsBool|FullyQualifiedName~FlagAsTask_DoesNotReadBackFlagTaskDialogResult|FullyQualifiedName~FlagAsTaskAsync_DoesNotReadBackFlagTaskDialogResult|FullyQualifiedName~Expand_WhenFocusSubjectReturnsFalse_StillEnumeratesConversation" "/Logger:trx;LogFileName=p8-t10.trx" /ResultsDirectory:<temp-results-dir>
EXIT_CODE: 0
ExpectedExitCode: 0

## Acceptance

`Test Run Successful.` — `Total tests: 5`, `Passed: 5`, 0 failed, 0 skipped, in 1.37 seconds.

```
Passed FlagAsTask_DoesNotReadBackFlagTaskDialogResult [205 ms]
Passed FlagAsTaskAsync_DoesNotReadBackFlagTaskDialogResult [37 ms]
Passed Expand_WhenFocusSubjectReturnsFalse_StillEnumeratesConversation [19 ms]
Passed IItemViewer_DeclaresAddFolderItemsAndNotSetFolderItems [23 ms]
Passed IItemViewer_FocusSubjectReturnsBool [< 1 ms]
```

TRX: `evidence/regression-testing/p8-t10.trx` (sanitised; 5 `UnitTestResult` elements, matching the
total above; the file parses under a strict XML reader after redaction).

## Red-to-green correspondence

| Test | RED artifact | Was |
|---|---|---|
| `IItemViewer_DeclaresAddFolderItemsAndNotSetFolderItems` | P7-T5 | failed — `found IItemViewer.SetFolderItems` |
| `IItemViewer_FocusSubjectReturnsBool` | P7-T5 | failed — `found System.Void` |
| `FlagAsTask_DoesNotReadBackFlagTaskDialogResult` | P7-T6 | failed — `was 1 times: v => v.FlagTaskDialogResult` |
| `FlagAsTaskAsync_DoesNotReadBackFlagTaskDialogResult` | P7-T6 | failed — same message |
| `Expand_WhenFocusSubjectReturnsFalse_StillEnumeratesConversation` | P7-T8 | did not compile — `CS1061` on `Returns(false)` |

The fifth is the one that could not be run at all before the fix: its
`Setup(v => v.FocusSubject()).Returns(false)` did not bind against the `void`-returning member and
broke the test-assembly build. It now compiles and passes, which is simultaneously the proof that
`FocusSubject` returns `bool` and that the `&Expand` action still reaches `EnumerateConversation()`
when the focus attempt reports failure.

Output Summary: **GREEN confirmed.** All five previously-red #490 tests pass — `EXIT_CODE: 0`,
`Total tests: 5`, `Passed: 5`, 0 failed, 0 skipped. Each maps to a recorded pre-fix failure in the
P7-T5, P7-T6 or P7-T8 artifact, so every one of them demonstrably failed before the change and
passes after it.
