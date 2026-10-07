# focus-and-theme-tests-leak-shared-dispatcher-setup (Issue #968)

- Date captured: 2026-10-02
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/focus-and-theme-tests-leak-shared-dispatcher-setup/ (Issue #968)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #968
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/968
- Last Updated: 2026-10-02
## Summary

Tests in `QuickFiler.Test` `QfcItemController_FocusAndThemeTests` set up the shared UI-thread dispatcher (through `UiThreadDispatcherFixture.EnsureDispatcher()`) and do not release that setup. Under the parallel test regime, another test class that releases or resets the shared dispatcher can leave a theme test running against a null dispatcher. The #950 preparation found the exposure: its transaction-test fix releases a dispatcher pin at the end of the test, and the same exposure already exists through two other tests in that file.

## Environment

- OS/version: Windows 11 (local) and windows-latest (CI)
- Python version: n/a (C# / MSTest, Workers=0, Scope=ClassLevel)
- Command/flags used: standard MSTest coverage route
- Data source or fixture: `UiThreadDispatcherFixture`

## Steps to Reproduce

1. Run `QuickFiler.Test` in parallel.
2. Have a class that resets the shared dispatcher run concurrently with `QfcItemController_FocusAndThemeTests`.
3. A theme test can observe a null dispatcher. This is intermittent.

## Expected Behavior

Each test class acquires and releases the shared dispatcher through a scoped, reference-counted pin, so no class can null it while another still depends on it.

## Actual Behavior

The theme tests depend on dispatcher state they do not own or pin.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: #950 preparation report; its plan carries the stop marker `THEME TEST NULL-DISPATCHER EXPOSURE OBSERVED`.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

## Suspected Cause / Notes

This is the same family as #950 and #882: raced static test-fixture state. It must not be fixed with `[DoNotParallelize]`, Workers=1 or retries. Find and own the shared state. Sequence it after #950 merges, because #950 introduces the pin.

## Proposed Fix / Validation Ideas

- [ ] Have the theme tests acquire and release the #950 dispatcher pin in class initialize and cleanup.
- [ ] Write a deterministic regression test that releases a competing pin mid-test, and show the theme test fails without the fix.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
