# parallel-test-failures-wall-clock-and-shared-user-config (Issue #1005)

- Date captured: 2026-10-22
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/parallel-test-failures-wall-clock-and-shared-user-config/ (Issue #1005)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #1005
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/1005
- Last Updated: 2026-10-22
## Summary

Two MSTest tests fail intermittently when the suite runs in parallel in Visual Studio Test Explorer. `DfDeedle_COM_Tests.AddQfcColumnsAsync_HappyPath_CompletesWithoutThrowing` depends on wall-clock deadlines, and `TaskTreeControllerMoveLogicTests.MoveObjectsToChildren_DesyncedRoot_FiresMessageSeam` writes the shared on-disk `user.config` through `Properties.Settings.Default.Save()`. Both violate the unit-test isolation rules; every test must pass when run in parallel.

## Environment

- OS/version: Windows 11, Visual Studio 18 Test Explorer (parallel execution)
- Python version: not applicable (C# .NET Framework 4.8, MSTest, Moq, FluentAssertions)
- Command/flags used: Visual Studio Test Explorer run of the full solution
- Data source or fixture: UtilitiesCS.Test `DfDeedle_COM_Tests`; TaskTree.Test `TaskTreeControllerMoveLogicTests`

## Steps to Reproduce

1. Run the full test suite in parallel in Visual Studio Test Explorer.
2. Observe `AddQfcColumnsAsync_HappyPath_CompletesWithoutThrowing` fail after 9.5 s with `TimeoutException: The column add step timed out after 9000 ms for folder '(unknown folder)'` thrown from `DfDeedle.AddQfcColumnsAsync`.
3. Observe `MoveObjectsToChildren_DesyncedRoot_FiresMessageSeam` fail with `ConfigurationErrorsException: Failed to save settings: The configuration file has been changed by another program` from `IDList.GetNextToDoID` -> `ApplicationSettingsBase.Save()`, reached through `TreeOfToDoItems.AddChild` and `TaskTreeController.MoveObjectsToChildren`.

## Expected Behavior

Both tests pass deterministically in any order and under parallel execution, with no dependence on wall-clock timing and no write to shared on-disk configuration.

## Actual Behavior

1. The test calls `DfDeedle.AddQfcColumnsAsync(table, folder, token, 0)` without the `timeProvider` (or `columnAdder`) seam, so the production retry loop waits on three real 3000 ms deadlines for a `Task.Run` over Moq-backed COM mocks; under parallel load the work misses the budget and the method throws.
2. `IDList.GetNextToDoID` persists `MaxLengthOfID` through `Properties.Settings.Default.Save()`, so concurrent tests race on one shared `user.config` file and one save fails.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: `System.TimeoutException: The column add step timed out after 9000 ms for folder '(unknown folder)'` and `System.Configuration.ConfigurationErrorsException: Failed to save settings: The configuration file has been changed by another program. (...\TestSourceHost__...\user.config)`

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

## Suspected Cause / Notes

- `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs` `AddQfcColumnsAsync` already accepts optional `columnAdder` and `timeProvider` seams; the happy-path test does not use them, so it is bound to the wall clock.
- `ToDoModel/Data Model/ID/IDList.cs` `GetNextToDoID` writes `Properties.Settings.Default` and calls `Save()` with no injectable persistence seam, so any test that allocates an ID writes shared machine state.
- Other tests in the repository may share either pattern; the fix should search for them.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: make the DfDeedle happy-path test deterministic through the existing seams (a synchronous or pre-completed column adder and a fake time provider); add an injectable settings-persistence seam to `IDList` so tests never write `user.config`, with a production default that preserves current behavior
- [ ] Integration scenario to retest: full MSTest run in parallel (CI shape and Test Explorer shape), plus repeated runs of the two affected classes
- [ ] Manual verification notes: re-run the suite in Visual Studio Test Explorer with parallel execution enabled

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
