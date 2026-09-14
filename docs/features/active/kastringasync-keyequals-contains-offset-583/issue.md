# KaStringAsync.KeyEquals branch 1 computes a prefix-only Substring offset under a Contains guard

- Issue: #583
- Type: bug
- Work Mode: full-bug
- Severity: Low (latent)
- Owner: drmoisan
- Last Updated: 2026-09-12T10-25
- Source: https://github.com/drmoisan/TaskMaster/issues/583

## Summary

Branch 1 of `KaStringAsync.KeyEquals` guards on a substring test but computes its `Update`
argument with prefix arithmetic. The two are only consistent when `other` is a prefix of `Key`.

In `QuickFiler/Controllers/KaStringAsync.cs`:

- The branch-1 guard is `Key.Contains(other)` - a substring test.
- Its body computes `Update(Key.Substring(other.Length - 1, 1))`, an offset that is only
  meaningful when `other` is a prefix of `Key`.

For `Key = "abc"` and `other = "b"`, `Contains` is `true` and the expression yields `"a"`, which
is neither the matched character nor the character following it.

## Reachability

Reachable in principle whenever the registered digit width is 2. `GenerateStringKbdAction` in
`QuickFiler/Controllers/QfcCollectionController.cs` registers keys `"01"` through `"12"` at that
width; typing `"1"` matches `"01"` at index 1, as a substring rather than as a prefix.

It has no observable effect today, because `Update` is `null` on every `KaStringAsync` instance
production creates:

- `QfcCollectionController.cs` passes `null` for both `update` and `toggleControl`.
- `KbdActions.Add(string, TKey, VDelegate)` builds its element with the parameterless
  constructor, which assigns neither callback.

So every `Update is not null` guard in `KeyEquals` evaluates `false` on every production
evaluation. This is a latent defect, not a live one.

## Maintainer Decision (recorded 2026-09-11)

Resolution: Option 2 - keep the `Contains` guard and correct the offset arithmetic.

Branch 1 of `KaStringAsync.KeyEquals` keeps its substring test. The `Update` argument is computed
from the match position (`Key.IndexOf(other)`) so that `Update` receives the last matched
character for every matching row, not only for prefix matches. For the two-digit registration
`"01"` through `"12"`, typing `1` continues to match `01`, `10`, `11`, and `12`; the `01` row now
yields `1` instead of `0`.

Rationale: this is the only option with no user-visible change to keyboard filtering, and
`Update` is null on every production instance today, so the correction carries no runtime risk.

Binding constraints on the implementation:

- Do not substitute `StartsWith` for `Contains`.
- Do not change the branch-1 guard.
- The pinned test FilterKeys_WhenDistinctStoredKeysCoexist_PreservesKeyboardMatchingSemantics in
  the QuickFiler.Test KbdActionsTests file must continue to pass unchanged, and that file must
  not be edited.

## Regression Case

`Key` equal to `"01"`, `other` equal to `"1"`, with a non-null `Update`, must yield `"1"`.
Today it yields `"0"`.

## Acceptance Criteria

- [ ] AC1: The recorded decision (corrected offset arithmetic under the retained `Contains`
  guard) is reflected in the implementation; the branch-1 guard text is unchanged and no
  `StartsWith` call is introduced in `QuickFiler/Controllers/KaStringAsync.cs`.
- [ ] AC2: `KaStringAsync.KeyEquals` branch 1 derives its `Update` argument from the match
  position via `Key.IndexOf(other)`, so the character passed to `Update` is the last character
  of the matched span for a non-prefix match as well as for a prefix match.
- [ ] AC3: A regression test in `QuickFiler.Test/Controllers/KaStringAsyncTests.cs` covers the
  two-digit-width non-prefix case (`Key` equal to `"01"`, `other` equal to `"1"`) with a non-null
  `Update` and asserts the received argument is `"1"`.
- [ ] AC4: The pre-existing prefix-case behaviour is preserved: for `Key` equal to `"abc"` and
  `other` equal to `"ab"`, `Update` still receives `"b"`.
- [ ] AC5: The pinned keyboard-matching test in the QuickFiler.Test KbdActionsTests file passes
  unchanged, and that file is not modified by this change.
- [ ] AC6: The full C# toolchain passes in order: CSharpier check, the analyzer rebuild, the
  nullable rebuild, and the MSTest run.
