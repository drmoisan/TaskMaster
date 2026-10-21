# Phase 2 — R1 ordering and shape evidence

Timestamp: 2026-09-09T14-14

Task: [P2-T13]

Read with the Read tool against `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs`
at the retry gate and `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs` at the field
declaration, on the tree as it stands after [P2-T1] through [P2-T8].

## The three conjuncts, transcribed in source order

`UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs`:

- `:54` — `Current is not null`
- `:55` — `&& Current.UserEmailAddress is null`
- `:56` — `&& !_userEmailRetryAttemptedStores.Contains(Current)`

The membership test is the THIRD of the three. `Current is not null` is the FIRST and
`Current.UserEmailAddress is null` is the SECOND, in that order. The `if` opens at `:53` and its
condition list closes at `:57`. This is the ordering D16 and AC5 require: any dereference of
`Current` placed before the first conjunct would fail
`PopulateWithCurrent_WithNullCurrent_RendersPlaceholdersAndDoesNotThrow` with a null reference.

## The Add and the lookup

- `:59` — `_userEmailRetryAttemptedStores.Add(Current);`
- `:60` — `Current.RefreshUserEmailAddress();`

The `Add` line number, 59, is SMALLER than the `RefreshUserEmailAddress` line number, 60, so the
attempt is recorded before the lookup runs. An exception escaping the lookup therefore still
consumes that store's single attempt, which is the D16 ordering constraint and the property AC8
pins.

## The field declaration

`UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs`:

- `:113` — `private readonly HashSet<StoreWrapper> _userEmailRetryAttemptedStores =`
- `:114` — `new HashSet<StoreWrapper>();`

The declaration spans two lines because at its indentation the single-line form exceeds CSharpier's
100-column print width (D23). No task asserts the whole statement for that reason.

FIELD-IS-STATIC: NO

The declaration carries `private readonly` and no `static` keyword. `git grep -c -F "static
readonly HashSet" -- UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs` exits 1 with no
output, confirming it mechanically. A `static` set would make the latch process-wide and would
observe one read where
`PopulateWithCurrent_OnASecondControllerOverTheSameFailingStore_RetriesOnceMore` asserts
`Times.Exactly(2)` (D15).

RESET-SITES: 0

Established by two prohibition clauses, neither of which any task creates:

- `git grep -c -F "_userEmailRetryAttemptedStores.Clear()" -- UtilitiesCS` exits 1 with no output.
- `git grep -c -F "_userEmailRetryAttemptedStores.Remove(" -- UtilitiesCS` exits 1 with no output.

Their value is that they fail if a reset is introduced, which is what D16 forbids. Reassignment
needs no clause because the field is declared `readonly`.

Output Summary: The membership test is the third conjunct, following `Current is not null` then
`Current.UserEmailAddress is null`; the `Add` at `:59` precedes the `RefreshUserEmailAddress` call
at `:60`; the field is non-static and readonly; and there is no reset site anywhere in
`UtilitiesCS`.
