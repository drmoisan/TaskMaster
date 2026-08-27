# AC-1 Check-Off (P5-T1)

Timestamp: 2026-08-27T11-49
Task: [P5-T1]
Command: `git diff -- docs/features/active/quickfiler-test-uithread-dispatcher-493/spec.md`
EXIT_CODE: 0
Output Summary: AC-1 ("Restore exists and is idempotent") is verified against the recorded passing
results for R2 and R3 and is checked off in `spec.md`. `PairsN: 1`, `PairsNMinus1: 0`, so exactly one
further checkbox changed state.

PairsN: 1
PairsNMinus1: 0

`pairs(1) - pairs(0) == 1`. `pairs(0) == 0` per the § Phase 5 preamble.

## Cited artifact, resolved per § Conventions

| Stem | Resolved filename |
| --- | --- |
| `regression-tests-pass` | `<FEATURE>/evidence/regression-testing/regression-tests-pass.2026-08-27T11-01.md` |

## Verification

AC-1 requires that `EnsureUiThreadDispatcher` returns an `IDisposable` scope whose `Dispose` restores
the previous `UiThread._dispatcher` value, that a second `Dispose` neither re-writes the field nor
throws, that restores are conditional `ReferenceEquals` compare-then-write, and that a call which
performed no install returns a no-op scope. The spec states it is evidenced by tests R2 and R3.

The cited artifact records both as passed:

```
| R2 | `EnsureDispatcher_WhenTheFieldIsNull_InstallsAndRestoresOnDispose` | Passed | 1 ms |
| R3 | `EnsureDispatcher_ScopeDisposedTwice_IsIdempotent` | Passed | 3 ms |
```

R2 establishes the restore: with the field forced to a known `null` baseline, `Current` is non-null
after the ensure call and `null` after disposing the ensure scope. R3 establishes idempotency: a
second `Dispose` does not throw and `Current` is unchanged between the two disposals.

## Result

`- [ ] **AC-1 …` changed to `- [x] **AC-1 …` in
`docs/features/active/quickfiler-test-uithread-dispatcher-493/spec.md`. Only the three characters of
the checkbox changed; the criterion text is untouched. The file's CRLF line terminators and its
absence of a byte-order mark were both preserved, verified with `file` after the edit.
