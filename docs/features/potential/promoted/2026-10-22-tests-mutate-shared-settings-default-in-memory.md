# tests-mutate-shared-settings-default-in-memory (Issue #1006)

- Date captured: 2026-10-22
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/tests-mutate-shared-settings-default-in-memory/ (Issue #1006)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #1006
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/1006
- Last Updated: 2026-10-22
## Summary

Several TaskMaster.Test classes assign values to the process-global `TaskMaster.Properties.Settings.Default` in memory and restore them afterwards. Under class-level parallel execution, two such tests can observe each other's values, and `ApplicationGlobalsStartupTimingTests` serializes with per-method `[DoNotParallelize]` to cope. All tests should run in parallel without shared static settings state. Found during issue #1005, which fixes the tests that call `Settings.Default.Save()`.

## Environment

- OS/version: Windows 11, Visual Studio Test Explorer or vstest with `Workers=0` / `ClassLevel`
- Python version: not applicable (C# .NET Framework 4.8, MSTest, Moq, FluentAssertions)
- Command/flags used: full MSTest run in parallel
- Data source or fixture: TaskMaster.Test AppGlobals tests

## Steps to Reproduce

1. Run TaskMaster.Test in parallel at class level.
2. Observe that tests mutating the same `Settings.Default` property can interleave (latent; not yet observed as a failure).
3. Observe `[DoNotParallelize]` on `ApplicationGlobalsStartupTimingTests` methods, which exists only because of the shared setting.

## Expected Behavior

Each test reads settings through an injected per-instance store, so no test mutates `Settings.Default` and no `[DoNotParallelize]` is needed for settings.

## Actual Behavior

Tests mutate the shared settings singleton in memory:
- `ApplicationGlobalsStartupTimingTests` and `ApplicationGlobalsTests`: `StartupTimingEnabled`
- `AppEventsTests`: `EventsHooked`
- `AppAutoFileObjectsFolderPredictorTests`: `UseLcppnPredictor`
- `AppOlObjectsTests`: `JunkPotential`, `OlJunkCertain`
- `AppToDoObjectsTests`: property values injected into `Settings.Default`

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: not applicable (latent isolation defect identified by inventory in issue #1005 research)

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

## Suspected Cause / Notes

The production readers of these settings (ApplicationGlobals startup timing, AppEvents, AppAutoFileObjects predictor selection, AppOlObjects junk folders, AppToDoObjects) read `Properties.Settings.Default` directly with no injectable seam. Issue #1005 introduces `UtilitiesCS` `ISettingsStore` with an `ApplicationSettingsStore` adapter; the same seam can be applied to these readers.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: inject `ISettingsStore` per instance into each reader; convert the listed tests to an in-memory store; remove the settings-related `[DoNotParallelize]` attributes
- [ ] Integration scenario to retest: full MSTest run with `Workers=0` / `ClassLevel`, repeated for the affected classes
- [ ] Manual verification notes: Visual Studio Test Explorer parallel run

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
