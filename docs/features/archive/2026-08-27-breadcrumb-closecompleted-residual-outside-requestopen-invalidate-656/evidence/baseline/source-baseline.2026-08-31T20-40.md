# Baseline — Pre-Change Source Measurements (Issue #656)

Timestamp: 2026-09-01T14-39
Task: [P0-T12]

Command:
```
(Get-Content -LiteralPath QuickFiler\Viewers\BreadcrumbDropDownOpenCoordinator.cs).Count
(Get-Content -LiteralPath QuickFiler.Test\Viewers\BreadcrumbDropDownOpenCoordinatorTests.Part3.cs).Count
(Get-Content -LiteralPath QuickFiler.Test\Viewers\BreadcrumbDropDownOpenCoordinatorTests.Part2.cs).Count
(Get-Content -LiteralPath QuickFiler.Test\Viewers\BreadcrumbDropDownOpenCoordinatorTests.cs).Count
@(Select-String -Path QuickFiler\Viewers\BreadcrumbDropDownOpenCoordinator.cs -Pattern '^\s*[^/\s].*_host\.').Count
@(Select-String -Path QuickFiler\Viewers\BreadcrumbDropDownOpenCoordinator.cs -Pattern '^\s+(internal|public)\s').Count
@(Select-String -Path QuickFiler\Viewers\BreadcrumbDropDownOpenCoordinator.cs -SimpleMatch 'lock (_sync)').Count
@(Select-String -Path QuickFiler\Viewers\BreadcrumbDropDownOpenCoordinator.cs -SimpleMatch 'if (_closeCompleted)').Count
@(Get-ChildItem QuickFiler.Test -Recurse -Filter *.cs | Select-String -SimpleMatch 'CloseCore_AfterSuccessfulCloseAndHostReopen_ReachesHostCloseAgain').Count
```

EXIT_CODE: 0

## Measured values against the plan's expected values

| Measurement | Expected | Measured | Match |
|---|---|---|---|
| `BreadcrumbDropDownOpenCoordinator.cs` line count | 378 | 378 | yes |
| `BreadcrumbDropDownOpenCoordinatorTests.Part3.cs` line count | 173 | 173 | yes |
| `BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` line count | 455 | 455 | yes |
| `BreadcrumbDropDownOpenCoordinatorTests.cs` line count | 463 | 463 | yes |
| Non-comment `_host.` lines in the coordinator | 5 | 5 | yes |
| Declared `internal`/`public` member lines in the coordinator | 12 | 12 | yes |
| `lock (_sync)` occurrences in the coordinator | 12 | 12 | yes |
| `if (_closeCompleted)` occurrences in the coordinator | 1 | 1 | yes |
| Pre-existing occurrences of the new test name in `QuickFiler.Test` | 0 | 0 | yes |

Every expected value in the plan is confirmed against the working tree. `Select-String` has no
`-Recurse` parameter, so the final measurement's file set is produced by `Get-ChildItem` and piped
in, as the plan requires.

## Enumerated baseline line numbers (used as the comparison set by P2-T5 and P2-T6)

Non-comment `_host.` lines (5):

- L112: `if (_closeInFlight && _host.IsOpen)`
- L193: `(!_host.IsOpen || !_host.Close(BreadcrumbDropDownCloseReason.Uncommitted))`
- L258: `? _host.OpenAsync(anchor, workingArea(), size)`
- L259: `: _host.OpenAsync(anchor, workingArea(), size, takeFocus: false);`
- L323: `closed = _host.Close(reason);`

`lock (_sync)` lines (12): L84, L96, L106, L134, L147, L237, L310, L327, L332, L346, L360, L368.

Declared member lines (12): L12, L51, L78, L80, L89, L104, L132, L143, L152, L171, L186, L202.

## Additional plan citations re-derived against the working tree

- `CloseCore` summary documentation occupies `:302-307`; the declaration
  `private bool CloseCore(BreadcrumbDropDownCloseReason reason)` is at `:308`; its opening brace is
  at `:309`; the `lock (_sync)` that opens the critical section is at `:310`; the completed-close
  guard `if (_closeCompleted)` is at `:316`.
- The `_closeCompleted` field documentation occupies `:38-46`, with the declaration
  `private bool _closeCompleted;` at `:46`.
- `internal void SetDroppedDown(bool droppedDown)` is at `:152`.
- In `BreadcrumbDropDownOpenCoordinatorTests.Part3.cs`, `LatchAfterRelease_IsIgnoredAndIssuesNoOpen`
  ends at `:171` and the closing brace of the partial class is at `:172`. Lines `:1-4` import
  `System.Threading.Tasks`, `FluentAssertions`,
  `Microsoft.VisualStudio.TestTools.UnitTesting`, and `QuickFiler.Viewers`, so the new test needs no
  added `using` directive.
- Harness members used by the new test, in `BreadcrumbDropDownOpenCoordinatorTests.cs`:
  `CoordinatorHarness` at `:323`, `SelectorOpen` at `:352`, `ControlledHost.IsOpen` at `:378`,
  `CloseReasons` at `:395`, `Enqueue` at `:402`, `SetOpen` at `:407`. All exist on the unmodified
  tree, so the new test compiles against unmodified production code and its Phase 1 failure is a
  runtime red rather than a compile red.

Output Summary: All nine expected baseline values matched exactly, and every plan citation into the
production and test files was re-derived and confirmed against the current working tree. No
discrepancy found in this task.
