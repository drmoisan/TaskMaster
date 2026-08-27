# AC-2 Check-Off (P5-T2)

Timestamp: 2026-08-27T11-51
Task: [P5-T2]
Command: `git diff -- docs/features/active/quickfiler-test-uithread-dispatcher-493/spec.md`
EXIT_CODE: 0
Output Summary: AC-2 ("Concurrent callers cannot interleave install and restore against the shared
static") is verified against the recorded passing results for R1 and R4 and against the recorded
existence of the fixture file, and is checked off in `spec.md`. `PairsN: 2`,
`PairsNMinus1: 1`, so exactly one further checkbox changed state.

PairsN: 2
PairsNMinus1: 1

`pairs(2) - pairs(1) == 1`. `pairs(1)` is the value recorded by `P5-T1` in
`<FEATURE>/evidence/other/ac-checkoff-ac1.2026-08-27T11-49.md`, read by citation rather than
re-derived.

## Cited artifacts, resolved per § Conventions

| Stem | Resolved filename |
| --- | --- |
| `regression-tests-pass` | `<FEATURE>/evidence/regression-testing/regression-tests-pass.2026-08-27T11-01.md` |
| `fixture-created` | `<FEATURE>/evidence/other/fixture-created.2026-08-27T10-32.md` |

## Verification

AC-2 requires that every mutation of `UiThread._dispatcher` inside `QuickFiler.Test`'s owned files
goes through `UiThreadDispatcherFixture` and holds `FieldLock` for the whole read-modify-write; that
long install-to-restore transactions additionally hold `TransactionGate`; that `EnsureDispatcher`
never acquires `TransactionGate`; and that lock ordering is `TransactionGate` then `FieldLock`, never
the reverse. The spec states it is evidenced by tests R1 and R4.

The first cited artifact records both as passed:

```
| R1 | `EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt` | Passed | 71 ms |
| R4 | `Transaction_SecondCallerCannotInstallUntilTheFirstRestores` | Passed | 3 ms |
```

R1 reproduces the #230 clobber precondition with no concurrency and shows the live transaction value
survives the ensure call and the ensure scope's disposal, which is what atomic read-modify-write under
`FieldLock` buys. R4 shows a second transaction acquiring the gate observes the pre-install value and
never the first transaction's installed value, which is what `TransactionGate` plus
restore-before-release buys. R1 is documented in the test file as the primary deterministic assertion
and R4 as the supporting probabilistic one.

The second cited artifact records the fixture file's existence:

```
| File exists at the stated path | yes |
| `typeof(UiThread)` match count | 1 |
```

It also records that all five fixture field declarations carry initializers, that `Current` uses an
explicit block-bodied accessor holding `FieldLock`, that `EnsureDispatcher` obtains the parked
dispatcher before taking `FieldLock` and never touches `TransactionGate`, and that the lock ordering
is `TransactionGate` then `FieldLock`.

## Result

`- [ ] **AC-2 …` changed to `- [x] **AC-2 …` in
`docs/features/active/quickfiler-test-uithread-dispatcher-493/spec.md`. Only the checkbox changed.
