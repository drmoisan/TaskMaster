# Fail-before / pass-after dossier — Issue #482

Timestamp: 2026-08-27T09-45

Test: `QuickFiler.Controllers.Tests.QfcItemController_NavigationTests.ToggleExpansion_WhenAsyncOnThenSyncOffThenAsyncOn_DoesNotThrowAndBothRegistriesHoldOneBAndOneD`

Fix under test: the new `private void SyncExpandedRegistrations(bool expanded)` (`[P3-T4]`) plus the
rewiring of both `ToggleState` overloads to delegate to it after the `_expanded` write (`[P3-T5]`,
`[P3-T6]`), all in `QuickFiler/Controllers/QfcItemController.Navigation.cs`.

## RED — observed before the fix

- Task: `[P3-T3]` (`[expect-fail]`)
- Artifact: `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/p3-t3-482-red.2026-08-27T09-45.md`
- Command: `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~QfcItemController_NavigationTests.ToggleExpansion_WhenAsyncOnThenSyncOffThenAsyncOn_DoesNotThrowAndBothRegistriesHoldOneBAndOneD"`
- EXIT_CODE: 1
- **Failed count: 1**; Passed count: 0
- **The recorded failure is an `ArgumentException` raised on the third step.** Verbatim:
  `Did not expect System.ArgumentException, but found System.ArgumentException: Cannot add key because it already exists. Key B SourceId expansion-entry`,
  thrown from `KbdActions.Add` via `QfcItemController.RegisterExpandedAsyncActions()` inside
  `ToggleExpansionAsync`. The first two steps completed; only the third threw.

## GREEN — observed after the fix

- Task: `[P3-T8]`
- Artifact: `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/p3-t8-482-green.2026-08-27T09-45.md`
- Command: byte-identical to the RED command above.
- EXIT_CODE: 0
- **Failed count: 0**; Passed count: 1

## Ordering

The RED run preceded the fix and the GREEN run followed it, satisfying `CLAUDE.md`'s Bugfix Workflow
step 1. The only change between the two runs is the three edits to
`QuickFiler/Controllers/QfcItemController.Navigation.cs`; the test text, the filter, and the build
configuration are identical.

Output Summary: RED failed count 1 with the failure recorded as an `ArgumentException` on the third
step; GREEN failed count 0; both artifact paths named above.
