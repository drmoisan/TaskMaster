# Fail-before / pass-after dossier — Issue #444

Timestamp: 2026-08-27T09-45

Test: `QuickFiler.Controllers.Tests.KbdActionsRemainingBranchesTests.EnumerableConstructor_WhenSeedContainsDuplicateSourceAndStoredKey_ThrowsArgumentException`

Fix under test: the `(SourceId, StoredKey)` duplicate guard added to
`public KbdActions(IEnumerable<UClass> list)` in `QuickFiler/Controllers/KbdActions.cs` by `[P1-T4]`.

## RED — observed before the fix

- Task: `[P1-T3]` (`[expect-fail]`)
- Artifact: `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/p1-t3-444-red.2026-08-27T09-45.md`
- Command: `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~KbdActionsRemainingBranchesTests.EnumerableConstructor_WhenSeedContainsDuplicateSourceAndStoredKey_ThrowsArgumentException"`
- EXIT_CODE: 1
- **Failed count: 1**; Passed count: 0
- Failure reason: `Expected a <System.ArgumentException> to be thrown ... but no exception was thrown.`

## GREEN — observed after the fix

- Task: `[P1-T6]`
- Artifact: `docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/p1-t6-444-green.2026-08-27T09-45.md`
- Command: byte-identical to the RED command above.
- EXIT_CODE: 0
- **Failed count: 0**; Passed count: 1

## Ordering

The RED run preceded the fix and the GREEN run followed it, satisfying `CLAUDE.md`'s Bugfix Workflow
step 1. The only change between the two runs is `[P1-T4]`'s edit to
`QuickFiler/Controllers/KbdActions.cs`; the test text, the filter, and the build configuration are
identical.

Output Summary: RED failed count 1, GREEN failed count 0; both artifact paths named above; the
regression test was observed failing before and passing after the `KbdActions.cs` change.
