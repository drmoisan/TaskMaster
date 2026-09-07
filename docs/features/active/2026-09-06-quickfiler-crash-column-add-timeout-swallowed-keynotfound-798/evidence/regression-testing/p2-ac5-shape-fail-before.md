# Phase 2 — AC5 handler shape pin (fail-before)

Timestamp: 2026-09-07T02-09
Task: [P2-T10] [expect-fail]
Issue: #798

Host-specific absolute paths, user account names and machine names are redacted to `<repo-root>`,
`<vstest>`, `<user>` and `<machine>` tokens.

## Test added

`TaskMaster.Test.Ribbon.RibbonCommandBoundaryTests.NamedQuickFilerHandlers_AreAwaitedAsyncVoidAndRouteThroughTheBoundary`

The test mirrors the `AssertAwaitedAsyncVoidShape` helper in the sibling ribbon shape-test file: for
each of `QuickFiler_Click`, `QuickFilerHighConfidence_Click` and `SortEmail_Click` it asserts the
handler returns `void` and carries the compiler-emitted `AsyncStateMachineAttribute`. It then asserts
that the `RibbonViewer` type declares a field whose type is `RibbonCommandBoundary`.

The test depends on no QuickFiler type; it reflects only over `RibbonViewer` and
`RibbonCommandBoundary`, both of which live in the TaskMaster assembly and are reachable from
TaskMaster.Test through the existing `InternalsVisibleTo` declaration.

## Build

Command: msbuild TaskMaster.Test\TaskMaster.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU
EXIT_CODE: 0
ExpectedExitCode: 0

## Test run

Command: `<vstest> TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /InIsolation /Logger:trx /ResultsDirectory:coverage\trx\p2-t10 /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None /TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName~NamedQuickFilerHandlers&FullyQualifiedName!~ShellUtilities&FullyQualifiedName!~SysImageListHelper&FullyQualifiedName!~OSBrowser"`

EXIT_CODE: 1
ExpectedExitCode: 1

The filter carries the shell-icon exclusion extension because P0-T8 recorded
`SHELL_ICON_EXCLUSION: REQUIRED`.

TRX: `coverage\trx\p2-t10\<user>_<machine>_2026-09-07_02_09_58_net481.trx`

- total: 1
- passed: 0
- failed: 1
- duration: 201 ms
- total run time: 1.7767 s

## Observed failure

Status: **Failed**, as expected, because no `RibbonCommandBoundary`-typed field exists on
`RibbonViewer` yet.

Failure message, quoted verbatim:

```
Expected boundaryFields not to be empty because RibbonViewer must hold the boundary the three handlers route through.
```

The failure is the one the plan predicts and it is not incidental. The three handler-shape
assertions run before the field assertion and all passed, so the three named handlers already have
the awaited `async void` shape today; the single unsatisfied clause is the boundary field, which
P6 introduces. The test did not hang; the four-minute blame hang timeout did not fire.

Output Summary: 1 total, 0 passed, 1 failed.
`NamedQuickFilerHandlers_AreAwaitedAsyncVoidAndRouteThroughTheBoundary` failed on its
`RibbonCommandBoundary`-typed field assertion, which is the AC5 shape fail-before condition.
EXIT_CODE 1 matches ExpectedExitCode 1.
