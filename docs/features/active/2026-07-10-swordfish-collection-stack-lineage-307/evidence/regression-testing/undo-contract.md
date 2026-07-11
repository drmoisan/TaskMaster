# P5-T9 — SloStack Undo-Contract Regression (pass-after)

Timestamp: 2026-07-11T00-05
Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /InIsolation /Tests:SloStackUndoContract_Tests`
EXIT_CODE: 0

## Output Summary

The positional undo-loop contract that both `SortEmail.UndoAsync` and
`QfcFormController.UndoDialog` depend on is reproduced on the clean
`SloStack<IMovedMailInfo>` and passes. **3 passed, 0 failed.**

Tests:

- `UndoLoop_ConfirmAll_ProcessesEveryElementTopToBottom_AndDrainsStack` — Passed.
  Forward index `i`, read `stack[i]`, positional `Pop(i)` without advancing `i`;
  every element is processed top-to-bottom in order and the stack drains to 0.
- `UndoLoop_MixedConfirmAndSkip_ShiftsAndReprocessesCorrectly` — Passed.
  Skips advance `i`; confirmations `Pop(i)` and hold `i` so the next element shifts
  into index `i` and is reprocessed. Undo A and C, skip B → B is the only survivor.
- `PopAtOrdinal_ShiftsHigherIndicesDown_SoNextElementOccupiesSameIndex` — Passed.
  `Pop(1)` removes ordinal 1 and shifts higher indices down (index 2 → index 1).

## Contract Verified

`SloStack<T>.Pop(int)` removes and returns the element at the given ordinal and shifts
higher indices down, matching the legacy `ScoStack` shift-and-reprocess semantics. Top-of-stack
is index 0. This preserves the undo behavior of both undo loops with no control-flow change.
