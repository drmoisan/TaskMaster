# R2 and R3 — Red run (expect-fail)

- Timestamp: 2026-09-02T01-23
- Issue: #678
- Task: [P1-T7] `[expect-fail]`

Command (Derivation D7; this is the invocation the `EXIT_CODE:` below reports):

```
vstest.console.exe QuickFiler.Test/bin/Debug/QuickFiler.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:TestCategory!=LiveOutlook&(FullyQualifiedName~ProjectPredeterminedFolder_BoundaryCases_MatchFolderPredictorProjection|FullyQualifiedName~AssignFolderComboBox_WhenEmptyArchiveRootAndLeadingSeparator_PreselectsProjectedFolder|FullyQualifiedName~LoadFolderHandlerAsync_WhenCarriedHandlerAndCancelledToken_ObservesCancellation)" /Logger:trx "/ResultsDirectory:TestResults\p1-t7"
```

EXIT_CODE: 1
ExpectedExitCode: 1

`TestResults\p1-t7` was deleted before the run, so exactly one TRX exists in it.

## Clause 1 — the pre-run build exits 0

`msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"` → exit **0**.

Recorded inside `Output Summary:` rather than as this artifact's own `Command:` and
`EXIT_CODE:`, because `ExpectedExitCode:` is a per-file field and a build recorded as the
artifact's command would be normalised against the declared expectation of 1.

## Clause 2 — exactly 3 tests discovered and executed, all three named individually

```
A total of 1 test files matched the specified pattern.
Total tests: 3
```

```
  Failed ProjectPredeterminedFolder_BoundaryCases_MatchFolderPredictorProjection [149 ms]
  Failed AssignFolderComboBox_WhenEmptyArchiveRootAndLeadingSeparator_PreselectsProjectedFolder [171 ms]
  Failed LoadFolderHandlerAsync_WhenCarriedHandlerAndCancelledToken_ObservesCancellation [59 ms]
```

## Clause 3 — all 3 reported as failed

```
Total tests: 3
     Failed: 3
Test Run Failed.
```

## Clause 4 — each failure is an assertion failure, none is a build or assembly-load error

### Failure 1 — `ProjectPredeterminedFolder_BoundaryCases_MatchFolderPredictorProjection`

FluentAssertions `StringAssertions.Be`, thrown through `StringEqualityStrategy` and
`AssertionChain.FailWith`:

```
Expected QfcItemController.ProjectPredeterminedFolder(@"\\Archive\Projects\Active",
string.Empty) to be a match with the expectation because a non-null globals with an EMPTY
archive root gives FolderPredictor an archivePrefix of one separator, which it strips, but it
differs at index 1:
  "\\Archive\Projects\Active"    (actual)
  "\Archive\Projects\Active"     (expected)
```

Frame, with the absolute host path replaced by the repository-relative path:
`QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs:219`.

### Failure 2 — `AssignFolderComboBox_WhenEmptyArchiveRootAndLeadingSeparator_PreselectsProjectedFolder`

`Moq.MockException` from a `Verify` with a `Times.Once()` argument:

```
Expected invocation on the mock once, but was 0 times:
  v => v.SetFolderSelectedItem("Projects\Active")
Performed invocations:
   Mock<IItemViewer:1> (v):
      IItemViewer.InvokeRequired
      IItemViewer.AddFolderItems(["\\A\header", "\\A\top", "Projects\Active"])
      IItemViewer.FolderContains("\Projects\Active")
      IItemViewer.SetFolderSelectedIndex(1)
      IItemViewer.GetSelectedFolder()
```

Frame: `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs:288`.

The recorded invocation list states the R2 defect directly and at the boundary R2 names:
`FolderContains` was probed with the **raw** `\Projects\Active` rather than the projected
`Projects\Active`, the probe therefore missed, and the selection fell back to
`SetFolderSelectedIndex(1)` — which is exactly the AC12 mismatch the change set out to close,
reopening in the (non-null globals, empty archive root, leading-separator path) state.

### Failure 3 — `LoadFolderHandlerAsync_WhenCarriedHandlerAndCancelledToken_ObservesCancellation`

FluentAssertions `ThrowAsync<T>`:

```
Expected a <System.OperationCanceledException> to be thrown because the pre-change
Task.Run(..., cancel) route threw for an already-cancelled token, and the adoption path must
reproduce that outcome, but no exception was thrown.
```

## Clause 5 — the R3 message states that no exception was thrown

The recorded message ends with the literal `but no exception was thrown.` — not "the wrong
exception type was thrown". This is the distinction the clause requires: the adoption branch
returned **normally** for an already-cancelled token, silently adopting the carried handler
for work the caller had already cancelled. Had the message reported a wrong exception type,
the branch would have been observing cancellation in some other form and R3 would be a
different defect.

## Clause 6 — none of the three is a build or assembly-load error

The pre-run build exited 0 and the P1-T6 analyzer build exited 0, so all three tests
compiled. The runner reported `A total of 1 test files matched the specified pattern`,
discovered three tests and executed each with a measurable duration (149 ms, 171 ms, 59 ms)
rather than a sub-millisecond failure with an empty message, which is the assembly-load
signature. Every one of the three failures carries a stack frame inside the test method
itself, in an assertion API — `StringAssertions.Be`, `Moq.Mock.Verify`, and
`AsyncFunctionAssertions.ThrowAsync` respectively.

## Output Summary

Pre-run build exit 0. Scoped run discovered and executed exactly 3 tests, named all three
individually, and reported all 3 as failed; run exit code 1, equal to the declared
`ExpectedExitCode`. Failure 1 is a FluentAssertions string-equality failure at line 219;
failure 2 is a `Moq.MockException` at line 288 whose invocation list shows the raw rather
than projected `FolderContains` probe and the index-1 fallback; failure 3 is a
FluentAssertions `ThrowAsync` failure stating that **no** exception was thrown. None is a
build error or an assembly-load error.
