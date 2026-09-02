## P1-T5: Disposition of `MoveEmailsAsync_WithNullStack_BehavesIdenticallyToAnEmptyStack`

**Disposition: rewritten, not deleted.**

The test's entire premise — that passing `null` vs. a supplied `SloStack<IMovedMailInfo>` produces
observationally identical behavior — no longer applies once the parameter is removed; there is no
second argument shape left to compare.

However, the test's *arrangement* (an uninitialized controller with an empty `_itemGroupsToMove`
collection) is the **only** test in the file exercising `MoveEmailsAsync`'s early-return branch
(`var count = _itemGroupsToMove?.Count() ?? 0; if (count <= 0) { return; }`). Deleting the test outright
would drop that branch's coverage with no replacement. Rewritten instead as
`MoveEmailsAsync_WithEmptyItemGroupsToMove_DoesNotThrow`, keeping the arrangement and the no-throw
assertion, dropping only the null-vs-supplied-stack comparison and the now-unused `stack`
local/`NoStackEffect` constant.

Also renamed and fixed the three sibling call sites in the same file
(`MoveEmailsAsync_WhenMoveIsCancelled_PropagatesOperationCanceledException`,
`MoveEmailsAsync_AfterFirstFailure_DoesNotReadSubjectASecondTime`,
`MoveEmailsAsync_WithNullGroupFromIndexLookup_DoesNotThrow`) from `controller.MoveEmailsAsync(null)` to
`controller.MoveEmailsAsync()` — these three needed no behavioral change, only a compile-shape fix,
since the argument value was never meaningful to what they were testing (see
`evidence/baseline/p0-t8-mock-sweep.md`).
