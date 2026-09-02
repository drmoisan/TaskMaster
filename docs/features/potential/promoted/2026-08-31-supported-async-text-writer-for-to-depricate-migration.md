# supported-async-text-writer-for-to-depricate-migration (Issue #708)

- Date captured: 2026-08-31
- Author: Dan Moisan

- Status: Promoted -> docs/features/active/supported-async-text-writer-for-to-depricate-migration/ (Issue #708)

- Issue: #708
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/708
- Last Updated: 2026-08-31
## Problem / Why

`FileIO2` lives in a folder named `To Depricate`, and `FileIO2.WriteTextFileAsync` is the only asynchronous text-append primitive the repository has. Issue #647 corrected its failure reporting but deliberately kept it in place, because there is no supported replacement to migrate callers to. The folder name records an intent that cannot be acted on while callers have nowhere to go.

## Proposed Behavior

Introduce a supported, testable async text-writing abstraction outside `To Depricate`, with an injectable writer seam and an injectable delay so callers can be tested without touching the filesystem. Migrate the three `WriteTextFileAsync` call sites to it, then delete `FileIO2.WriteTextFileAsync`.

## Acceptance Criteria (early draft)

- [ ] A supported async text-writer type exists outside `UtilitiesCS/To Depricate/`, with an interface that permits a test double.
- [ ] The type reports write failure through its return value or a typed result rather than by throwing from an async void boundary.
- [ ] The three current callers of `FileIO2.WriteTextFileAsync` are migrated: `QuickFiler/Controllers/QfcHomeController.Metrics.cs`, `TaskMaster/AppGlobals/AppOlObjects.cs`, and the `UtilitiesCS.Test` suite.
- [ ] `FileIO2.WriteTextFileAsync` is deleted and no call to it remains.
- [ ] The new type reaches at least 90 percent line coverage as new code.

## Constraints & Risks

- The `TaskMaster/AppGlobals/AppOlObjects.cs` call site is an async void lambda on a `System.Timers.Timer` elapsed callback. An exception escaping it is re-raised on the thread pool and terminates the Outlook host process, so the replacement must not reintroduce a throwing failure path there.
- The synchronous `FileIO2.WriteTextFile` overload and its callers are a separate surface and are not in scope for this item.
- `AppOlObjects.cs` is at 494 of the 500-line file limit, so the migration must not add net lines there without an extraction.
- This is a new capability rather than a defect fix; it was recorded as an explicit non-goal of issue #647.

## Test Conditions to Consider

- [ ] Unit coverage areas: transient-failure retry, terminal mid-write failure, cancellation before opening, cancellation during the retry window, and the success path with content assertions.
- [ ] Integration scenarios: the QuickFiler metrics flush and the timed disk writer, each asserting that a false result is logged rather than discarded.
- [ ] CLI/API examples: not applicable; this is an in-process library type.

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create `docs/features/active/supported-async-text-writer-for-to-depricate-migration/` folder from the template
