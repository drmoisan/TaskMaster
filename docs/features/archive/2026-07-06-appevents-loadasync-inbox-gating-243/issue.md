# appevents-loadasync-inbox-gating (Issue #243)

- Date captured: 2026-07-06
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/2026-07-06-appevents-loadasync-inbox-gating-243/ (Issue #243)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #243
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/243
- Last Updated: 2026-07-06
- Work Mode: minor-audit

## Summary

`AppEvents.LoadAsync()` starts the Outlook readiness polling path through `Hook()` when `Settings.Default.EventsHooked` is true, but it immediately calls `await ProcessNewInboxItemsAsync()` before the readiness poll has populated `OlInboxes`. Because `ProcessNewInboxItemsAsync()` exits when `OlInboxes` is null or empty, startup inbox processing does not run in the hooked-events path.

## Environment

- Application: TaskMaster Outlook add-in
- Area: `TaskMaster/AppGlobals/AppEvents.cs`
- Scenario: Startup with `Settings.Default.EventsHooked == true`
- Test framework: MSTest with Moq and FluentAssertions

## Steps to Reproduce

1. Configure settings so `Settings.Default.EventsHooked` is true.
2. Start `AppEvents.LoadAsync()`.
3. Observe that `Hook()` only starts a readiness polling timer.
4. Observe that `LoadAsync()` immediately calls `ProcessNewInboxItemsAsync()` before `PerformReadinessHookup()` has populated `OlInboxes`.

## Expected Behavior

Startup inbox processing should run only after the same Outlook readiness checks that permit event hookup have passed and the inbox item subscriptions have populated `OlInboxes`.

## Actual Behavior

`ProcessNewInboxItemsAsync()` is invoked before `OlInboxes` has been populated. The null-or-empty guard prevents any inbox processing, so the method does not perform its startup processing function in this path.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: The code path shows `LoadAsync()` calling `Hook()` and then immediately awaiting `ProcessNewInboxItemsAsync()`, while `Hook()` defers inbox population to `PerformReadinessHookup()` through `HookReadinessCoordinator`.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

## Suspected Cause / Notes

The readiness-dependent subscription sequence was moved behind a polling coordinator to avoid blocking startup, but the startup inbox processing call was left in the synchronous `LoadAsync()` sequence. The fix should gate `ProcessNewInboxItemsAsync()` on the same readiness path that populates `OlInboxes`, without restoring synchronous COM hookup work to `LoadAsync()`.

## Proposed Fix / Validation Ideas

- [ ] Add a regression test proving `LoadAsync()` does not process inboxes before readiness hookup when events are hooked.
- [ ] Add or update coverage proving inbox processing is invoked after the readiness hookup path has populated `OlInboxes`.
- [ ] Preserve existing readiness polling behavior and existing event subscription behavior.

## Acceptance Criteria

- [x] `LoadAsync()` does not call `ProcessNewInboxItemsAsync()` before the Outlook readiness gate has passed when events are hooked.
- [x] The readiness-hookup path invokes startup inbox processing after `OlInboxes` has been populated by the same readiness checks that hook inbox events.
- [x] Existing deferred readiness polling and event subscription behavior remains intact.
- [x] Focused MSTest coverage proves the pre-readiness call is prevented and the post-readiness processing path runs.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch
