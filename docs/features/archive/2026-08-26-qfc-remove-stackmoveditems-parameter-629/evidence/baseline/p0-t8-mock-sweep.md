## P0-T8: MoveEmailsAsync / Mock<IQfcCollectionController> sweep of QuickFiler.Test

Command: `grep -rn "MoveEmailsAsync" QuickFiler.Test/` and `grep -rln "Mock<IQfcCollectionController>" QuickFiler.Test/`

### Direct calls to the real controller's `MoveEmailsAsync(...)` (not mocked) — `QfcCollectionControllerDefects468MoveTests.cs`

This file was NOT identified as needing more than one update by issue.md/spec.md; the sweep found four
direct-call sites, not one:

- `:165` — `Func<Task> act = () => controller.MoveEmailsAsync(null);` in
  `MoveEmailsAsync_WhenMoveIsCancelled_PropagatesOperationCanceledException` (`:144`). Argument value is
  incidental to the test's purpose (cancellation propagation). **Disposition: mechanical fix** — drop
  the argument.
- `:216` — `Func<Task> act = () => controller.MoveEmailsAsync(null);` in
  `MoveEmailsAsync_AfterFirstFailure_DoesNotReadSubjectASecondTime` (`:195`). Same shape.
  **Disposition: mechanical fix.**
- `:263` — `Func<Task> act = () => controller.MoveEmailsAsync(null);` in
  `MoveEmailsAsync_WithNullGroupFromIndexLookup_DoesNotThrow` (`:251`). Same shape.
  **Disposition: mechanical fix.**
- `:484-485` — `MoveEmailsAsync_WithNullStack_BehavesIdenticallyToAnEmptyStack` (`:471`), the one test
  issue.md/spec.md named. This test's entire premise is comparing the `null`-argument vs.
  supplied-argument behavior of the now-removed parameter. **Disposition: retire** (see
  `evidence/other/p1-t5-test-disposition.md` for the full justification) — there is no argument shape
  left to compare once the parameter is gone.

### Mock setup/verify sites — `QfcFormControllerUndoHandoffTests.cs`

- `:75` — `.Setup(g => g.MoveEmailsAsync(It.IsAny<SloStack<IMovedMailInfo>>()))`
- `:397` — `g => g.MoveEmailsAsync(It.IsAny<SloStack<IMovedMailInfo>>()),` (a `Verify` call)

Both need to change to the zero-parameter overload: `.Setup(g => g.MoveEmailsAsync())` /
`g => g.MoveEmailsAsync()`.

### Files requiring no change

The other 13 files matched by `Mock<IQfcCollectionController>` (`QfcFormControllerDeactivateTests.cs`,
`QfcFormControllerTests.cs`, `QfcFormControllerTests.Part2.cs`, `QfcHomeControllerIterationTests.cs`,
`QfcHomeControllerMetricsTests.cs`, `QfcItemController.InitializationTests.cs` and its Part2/Part3,
`QfcItemController.MailActionsTests.cs` and its Part2, `QfcItemController.NavigationTests.cs`,
`QfcItemController.SeamCoreTests.cs`, `QfcItemController.SeamDispatcherTests.cs`,
`QfcQueuePurePathsTests.cs`) construct a `Mock<IQfcCollectionController>` for unrelated members and
never call `MoveEmailsAsync` on it — confirmed by the `MoveEmailsAsync` grep above returning no hits in
any of them. No change required.

### Total edit surface confirmed by this sweep

6 test call sites across 2 files, plus the 3 production files named in spec.md's Implementation
Strategy. No file outside this set requires a change.
