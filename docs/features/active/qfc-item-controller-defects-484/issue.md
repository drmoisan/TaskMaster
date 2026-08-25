# qfc-item-controller-defects (Issue #484)

- Issue: #484
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/484
- Type: bug
- Work Mode: full-bug
- Epic: quickfiler-bug-family
- Integration Branch: epic/quickfiler-bug-family-integration
- Owner: drmoisan
- Last Updated: 2026-08-24
- Status: Active

## Summary

This feature closes five pre-existing defect issues in the `QfcItemController` partial classes.
All five were filed on 2026-08-07 during preparation research for epic #136 child F10 (issue #453)
and were deferred out of that child because its non-functional requirement prohibited behavior
change to observable QuickFiler flows. Each defect alters observable behavior on a teardown or
failure path and therefore requires its own regression test.

The five issues are grouped into one feature because they are confined to the same four partial
files of the same type and share one teardown/lifecycle contract. Splitting them would produce four
concurrent branches editing the same four files.

## Issues Closed by This Feature

| Issue | Title | Primary file | Severity |
|---|---|---|---|
| #480 | `ToggleNavigation(bool)` toggles twice, so the feature is inert | `QfcItemController.FocusAndTheme.cs` | Medium |
| #481 | No event unwiring path; 23 subscriptions are never detached | `QfcItemController.EventWiring.cs`, `.ViewerSetup.cs` | Medium |
| #483 | `MoveMailAsync` swallows every exception; missing cancellation checks | `QfcItemController.MailActions.cs` | Medium-High |
| #484 | `Cleanup()` nulls an armed `System.Threading.Timer` without disposing it | `QfcItemController.ViewerSetup.cs` | Medium |
| #485 | `WebResourceRequested` handler dereferences unguarded external inputs | `QfcItemController.ViewerSetup.cs` | Low-Medium |

## Authoritative Requirement Sources

The promoted potential documents are the authoritative requirement source. Each carries file:line,
the offending code block, root cause, suggested fix, and severity, and is richer than the GitHub
issue body:

- `docs/features/potential/promoted/2026-08-07-qfc-item-controller-togglenavigation-double-toggle.md` (#480)
- `docs/features/potential/promoted/2026-08-07-qfc-item-controller-no-event-unwiring-path.md` (#481)
- `docs/features/potential/promoted/2026-08-07-qfc-item-controller-mailactions-error-handling-defects.md` (#483)
- `docs/features/potential/promoted/2026-08-07-qfc-item-controller-cleanup-timer-and-stale-field-defects.md` (#484)
- `docs/features/potential/promoted/2026-08-07-qfc-item-controller-webview-handler-unguarded-inputs.md` (#485)

## Files This Feature Owns

Production files this feature may write:

- `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs`
- `QuickFiler/Controllers/QfcItemController.EventWiring.cs`
- `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`
- `QuickFiler/Controllers/QfcItemController.MailActions.cs`

Test files (all five already carry `Compile Include` entries in `QuickFiler.Test/QuickFiler.Test.csproj`
at lines 142, 144, 146, 150, and 153):

- `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs`
- `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs`
- `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs`
- `QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs`
- `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` — shared arrange helpers only; no test
  method may be added. This file is consumed by 16 other `QfcItemController` test files, including
  `QuickFiler.Test/Controllers/QfcItemController.NavigationTests.cs`, so this feature only appends new
  private helpers and modifies no existing member.

## Files This Feature Must Not Write

Owned by sibling epic children running concurrently on the same integration branch:

- `QuickFiler/Controllers/QfcItemController.Navigation.cs` (feature 444)
- `QuickFiler/Viewers/ItemViewer*.cs` (feature 489)
- `QuickFiler/Controllers/KbdActions.cs` (feature 444)

A fix that appears to require one of these files is recorded in `spec.md` as a downstream note and
is kept out of the plan.

## Downstream Consumers

This feature is an upstream dependency of two later epic children, both of which branch from an
integration branch that already carries this change:

- Feature 464 (EFC controllers, via #463)
- Feature 489 (ItemViewer, via #486 and #489)

Any change to the public or internal surface of the four owned partials, to event-wiring order, or
to the lifecycle contract must be stated explicitly in `spec.md` because those two features will be
authored against it.

This feature appends private arrange helpers to QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs. The change is additive: no existing helper is renamed, removed, or altered, so tests owned by other epic children that consume that file are unaffected.

## Acceptance Criteria Source

Work mode is `full-bug`. The authoritative acceptance-criteria source for this feature is
`spec.md`. `user-story.md` is intentionally absent: this is a defect-correction feature with no new
user-facing capability, and the requirements do not justify a user story.
