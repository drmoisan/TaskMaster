# Fail-before / pass-after dossier — Issue #472

Timestamp: 2026-08-27T09-45

Tests:

- `QuickFiler.Controllers.Tests.QfcCollectionControllerNavigationDigitsTests.UnregisterNavigation_AfterRegisteringAtTwoDigitsAndShrinkingToNine_RemovesTheTwoDigitKeys`
- `QuickFiler.Controllers.Tests.QfcCollectionControllerNavigationDigitsTests.UnregisterNavigation_AfterRegisteringAtOneDigitAndGrowingToTen_RemovesTheOneDigitKeys`

Fix under test: the `_registeredDigits` field and its assignment inside `RegisterNavigation`
(`[P2-T4]`), plus the `UnregisterNavigation` body rewrite that formats from that recorded width and
reads the live width property zero times (`[P2-T5]`), both in
`QuickFiler/Controllers/QfcCollectionController.cs`.

## RED — observed before the fix

- Task: `[P2-T3]` (`[expect-fail]`)
- Artifact: `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/p2-t3-472-red.2026-08-27T09-45.md`
- Command: `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~QfcCollectionControllerNavigationDigitsTests.UnregisterNavigation_AfterRegisteringAtTwoDigitsAndShrinkingToNine_RemovesTheTwoDigitKeys|FullyQualifiedName~QfcCollectionControllerNavigationDigitsTests.UnregisterNavigation_AfterRegisteringAtOneDigitAndGrowingToTen_RemovesTheOneDigitKeys"`
- EXIT_CODE: 1
- **Failed count: 2**; Passed count: 0
- Failure reasons: the shrink direction found the orphaned `"01"` still registered; the grow direction
  found the orphaned `"1"` still registered.

## GREEN — observed after the fix

- Task: `[P2-T7]`
- Artifact: `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/p2-t7-472-green.2026-08-27T09-45.md`
- Command: byte-identical to the RED command above.
- EXIT_CODE: 0
- **Failed count: 0**; Passed count: 2

## Ordering

The RED run preceded the fix and the GREEN run followed it, satisfying `CLAUDE.md`'s Bugfix Workflow
step 1. The only change between the two runs is the two edits to
`QuickFiler/Controllers/QfcCollectionController.cs`; the test text, the filter, and the build
configuration are identical.

Output Summary: RED failed count 2, GREEN failed count 0; both artifact paths named above; the #472
regression tests were observed failing before and passing after the `QfcCollectionController.cs`
change.
