# AC-4 Check-Off (P5-T4)

Timestamp: 2026-08-27T11-55
Task: [P5-T4]
Command: `git diff -- docs/features/active/quickfiler-test-uithread-dispatcher-493/spec.md`
EXIT_CODE: 0
Output Summary: AC-4 ("The #230 local workaround is removed, not duplicated") is verified against all
three matrix rows recorded by `P4-T4` and against the recorded `Part2.cs` migration, and is checked
off in `spec.md`. `PairsN: 4`, `PairsNMinus1: 3`, so exactly one further checkbox changed state.

PairsN: 4
PairsNMinus1: 3

`pairs(4) - pairs(3) == 1`. `pairs(3)` is the value recorded by `P5-T3` in
`<FEATURE>/evidence/other/ac-checkoff-ac3.2026-08-27T11-53.md`.

## Cited artifacts, resolved per § Conventions

| Stem | Resolved filename |
| --- | --- |
| `duplicate-swap-removal` | `<FEATURE>/evidence/qa-gates/duplicate-swap-removal.2026-08-27T11-36.md` |
| `part2-migrated` | `<FEATURE>/evidence/other/part2-migrated.2026-08-27T10-54.md` |

## Verification

`duplicate-swap-removal` records all three matrix rows holding, each with a required and an observed
count of 0:

| Row | Pattern | Target | Required | Observed |
| --- | --- | --- | --- | --- |
| 1 | `UiThreadDispatcherGate` | `QfcItemController.InitializationTests.Part2.cs` | 0 | 0 |
| 2 | `SwapUiThreadDispatcher` | `QfcItemController.InitializationTests.Part2.cs` | 0 | 0 |
| 3 | `typeof(UiThread)` | `QfcItemController.InitializationTests.Part2.cs` | 0 | 0 |
| 3 | `typeof(UiThread)` | `QfcItemController.TestSupport.cs` | 0 | 0 |
| 3 | `typeof(UiThread)` | `QfcItemController.UiThreadDispatcherFixtureTests.cs` | 0 | 0 |

Rows 1 and 2 discharge AC-4's first sentence: `Part2.cs` no longer declares its own
`SemaphoreSlim UiThreadDispatcherGate` and no longer declares its own `SwapUiThreadDispatcher`.
Row 3, combined with the single `typeof(UiThread)` match in
`QfcItemController.UiThreadDispatcherFixture.cs`, discharges "Exactly one implementation of the
reflection swap exists in the test assembly's owned files".

`part2-migrated` records the replacement mechanics AC-4's remaining sentences require:

- The two-phase `BeginTransactionAsync` then `Install` shape is preserved and was not collapsed into
  a single `SwapAsync(replacement)`, so the gate is still acquired at build start rather than at
  install time.
- The acquisition remains at build start.
- `PumpHarness.Restore()` remains idempotent via its retained `_restored` guard.
- Restore-before-release ordering is preserved and is now indivisible inside
  `UiThreadDispatcherTransaction.Dispose()`.

That artifact also records that the replacement rationale comment deliberately contains neither
`UiThreadDispatcherGate` nor `SwapUiThreadDispatcher`, so rows 1 and 2 are genuine gates rather than
ones a comment could silently satisfy.

## Result

`- [ ] **AC-4 …` changed to `- [x] **AC-4 …` in
`docs/features/active/quickfiler-test-uithread-dispatcher-493/spec.md`. Only the checkbox changed.
