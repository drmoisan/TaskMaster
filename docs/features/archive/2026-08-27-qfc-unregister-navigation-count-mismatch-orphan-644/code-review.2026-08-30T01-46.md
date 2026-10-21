# Code Review — Issue #644 (cycle-exit reaudit)

- **Timestamp:** 2026-08-30T01-46
- **Branch:** `bug/qfc-unregister-navigation-count-mismatch-orphan-644`
- **Head:** `85a1939f92f64ebada4e71d19cc095dc2e8e8a26`
- **Base:** `main` at `fa2ddefacf2c08abe18f3e3250d77da804534637` (merge base, verified)
- **Blocking findings:** **0**

## Scope reviewed

The whole branch diff against the resolved base branch, re-read in this session — not the two items
the remediation cycle addressed. Six code paths:

| Path | Change | Lines |
|---|---|---|
| `QuickFiler/Controllers/QfcCollectionController.cs` | modified | +18 / -9 |
| `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationLedgerTests.cs` | added | +361 |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs` | modified | +14 / -10 |
| `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs` | modified | +12 / -12 |
| `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` | modified | +3 / -4 |
| `QuickFiler.Test/QuickFiler.Test.csproj` | modified | +1 |

## Overall assessment

The fix is well-designed and correctly scoped. It replaces a derived, count-bounded unregister loop
with a ledger of the exact `(SourceId, Key)` pairs registration added, which makes the
register/unregister invariant structural rather than a coincidence between two independently
computed quantities. That is the right shape of fix for this defect class: it removes the *category*
of failure rather than the specific path that exposed it.

The remediation cycle's two corrections are sound and correctly constrained to documentation text.
One further instance of the same defect class survives, raised below as CR-6.

## The fix

```csharp
// Issue #644: the exact (SourceId, Key) pairs the last RegisterNavigation added, so an
// _itemGroups mutation between register and unregister cannot orphan a registration.
private List<(string SourceId, string Key)> _registeredNavigationKeys;

private List<(string SourceId, string Key)> RegisteredNavigationKeys =>
    _registeredNavigationKeys ??= new List<(string SourceId, string Key)>();
```

```csharp
public void UnregisterNavigation()
{
    // Issue #644: replay the recorded registration set verbatim and drain it. A count-bound
    // loop orphaned every key past the live count when a group was removed unbracketed.
    foreach (var (sourceId, key) in RegisteredNavigationKeys)
    {
        _kbdHandler.StringActionsAsync.Remove(sourceId, key);
    }
    RegisteredNavigationKeys.Clear();
}
```

Three properties are worth calling out as correct:

1. **`UnregisterNavigation` no longer reads `_itemGroups` at all.** The dependency that made
   unregistration sensitive to an intervening mutation is gone, not merely guarded. The test
   `UnregisterNavigation_AfterItemGroupsSetToNull_DoesNotThrow` pins this by setting the field to
   null, which is the strongest available statement of the property.
2. **The ledger is written strictly after a successful `Add`, reading the key back off the
   constructed instance.**

   ```csharp
   var action = GenerateStringKbdAction(itemIndex, digits);
   _kbdHandler.StringActionsAsync.Add(action);
   RegisteredNavigationKeys.Add((action.SourceId, action.Key));
   ```

   This ordering matters and the inline comment explains why: a duplicate-key `ArgumentException`
   from `Add` propagates before the ledger is touched, so a failed registration cannot leave a
   phantom entry that a later unregister would try to remove. Reading `SourceId` and `Key` off the
   constructed action rather than recomputing them also removes any possibility of the ledger and
   the registry disagreeing about the key format — which is precisely the #472 defect class.
3. **`_registeredDigits` is deleted rather than left dormant.** Verified:
   `grep -rn "_registeredDigits"` returns zero occurrences repository-wide. The field, its
   assignment, and the format expression derived from it were removed together, which CS0414 makes
   indivisible.

## Remediation cycle verification

Verified by reading the file and the diff, not by accepting the executor's report.

**CR-1, corrected at lines 189-196.** The block now reads "After the fix the ledger replays / the
nine recorded keys `"1".."9"` verbatim, so the added tenth group is irrelevant to / unregistration."
This matches the delivered mechanism exactly: a ledger of recorded key strings replayed verbatim,
no recorded width, no loop bound. Correct and complete for that block.

**Third instance, corrected at line 222.** The `.BeEmpty(...)` because-message now reads "the ledger
replays each key verbatim, so every key is removed regardless of group count". Correct.

**Neither correction touched executable code.** The complete remediation diff for this file is eight
lines — four removed, four added — of which three added lines are XML documentation comment lines
and one is the contents of a string literal. No assertion expression, no test name, no attribute,
and no statement changed. The file held at 226 lines and 3 `[TestMethod]` attributes, both
re-measured this session. All three tests in the file are recorded `Passed` in the TRX.

## Findings

All findings are Non-blocking. Total blocking: **0**.

### CR-6 — a fourth instance of the stale-mechanism class survives (Minor, Non-blocking) — NEW

**Location:** `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs` line 179.

```csharp
remaining
    .Where(k => k.StartsWith("0", StringComparison.Ordinal))
    .Should()
    .BeEmpty(
        "the recorded registration width is replayed, so the '0'-prefixed keys go"
    );
```

"the recorded registration width" names `_registeredDigits`, which this branch deleted. There is no
recorded width in the delivered code; the `'0'`-prefixed keys go because the ledger replays each
recorded key string verbatim.

This is structurally identical to the line-222 message the cycle corrected — same file, same defect,
same kind of string literal, differing only in which test it sits on. It appears in no predecessor
artifact: a search across every prior document for `prefixed keys go`, `recorded registration width`,
and `line 179` returns no match. The cycle's acceptance clauses were anchored on specific literal
fragments and its two sweep tasks were scoped to host identity, so no task ever swept this file for
the class as a class.

**Non-blocking** because the text is a diagnostic message on an assertion that is itself correct and
passing, and because the identical class was classified Non-blocking as CR-1 and CR-2 at cycle entry.

**Suggested correction:** `"the ledger replays each recorded key verbatim, so no '0'-prefixed key survives"`.

### CR-2 — historical sentence names a removal set that is now wrong (Trivial, Non-blocking) — STANDS

**Location:** same file, lines 144-145.

```
/// removed the never-registered "1".."9", leaving all ten two-digit keys orphaned. After the
/// fix it replays the recorded width and removes "01".."09".
```

Two problems, both surviving: the mechanism ("the recorded width") is deleted, and the outcome
("removes `"01".."09"`", nine of ten) is contradicted by the test's own assertion 36 lines below,
which is `remaining.Should().BeEmpty(...)` — all ten removed.

The cycle-entry recommendation was explicitly conditional: "if CR-1 is addressed, tighten this ... so
the tense marks it as history". CR-1 was addressed, which made the recommendation live, and it was
not acted on because the remediation inputs asserted that this block "is correct and must not be
disturbed". That assertion is accurate about the block's *second* paragraph, which correctly records
the residual as closed by #644, but not about its first.

**Suggested correction, marking tense:** "After #472 alone it replayed the recorded width and
removed `"01".."09"`; since #644 the ledger replays every recorded key, so all ten go."

CR-2 and CR-6 are the same defect and sit 34 lines apart in the same file. Correct them together.

### CR-3 — the discarded `bool` from `Remove` is now a meaningful signal (Minor, Non-blocking) — STANDS

`UnregisterNavigation` discards the `bool` returned by `StringActionsAsync.Remove`. Before this fix
that return was uninformative, because the loop could legitimately ask to remove a key it had never
registered. After this fix the ledger contains exactly the keys registration added, so a `false`
return now means a genuine desynchronization between the ledger and the registry — the one condition
the fix exists to prevent. Discarding it discards the only cheap self-check the new design makes
available.

Unchanged by this cycle, which touched no production file. Remains a **promotion candidate** rather
than in-scope work, correctly: the repository already tracks the cross-cutting question of all 42
production call sites discarding this return, and widening a bugfix to cover it would violate the
`CLAUDE.md` Bugfix Workflow rule.

### CR-4 — lazy accessor read twice; allocates in order to clear (Trivial, Non-blocking) — STANDS

`UnregisterNavigation` reads `RegisteredNavigationKeys` twice, once for the `foreach` and once for
`.Clear()`. On the no-prior-registration path the `??=` allocates a `List<T>` solely so that zero
items can be enumerated and the empty list cleared. A single local read would avoid both the second
property evaluation and the allocation:

```csharp
var ledger = _registeredNavigationKeys;
if (ledger == null) { return; }
foreach (var (sourceId, key) in ledger) { _kbdHandler.StringActionsAsync.Remove(sourceId, key); }
ledger.Clear();
```

Trivial: the allocation is one empty `List<T>` on a keyboard-setup path, not a hot loop.

### CR-5 — ledger is a non-thread-safe `List<T>` behind a non-atomic `??=` (Trivial, Non-blocking) — STANDS

`_registeredNavigationKeys ??= new List<(string SourceId, string Key)>()` compiles to a
read-test-write sequence that is not atomic, and `List<T>` is not thread-safe for concurrent
mutation. `RegisterNavigation` and `UnregisterNavigation` carry no
`[MethodImpl(MethodImplOptions.Synchronized)]`, unlike the neighbouring `Digits` getter.

Trivial in practice: both members are invoked from the UI thread on the QuickFiler surface, and the
pre-existing code they replace was equally unsynchronized. Recorded so a future change that moves
registration off the UI thread has the hazard already written down.

## What was verified and found correct

- **Regression tests genuinely regress.** T1
  (`UnregisterNavigation_AfterGroupRemovedThroughRemoveGroupByEntryIdSeam_RemovesEveryRegisteredKey`)
  was recorded failing against unmodified production code at `[P1-T4]` and passing at `[P2-T5]` and
  in this cycle's TRX. Red-before-green is established by artifact, not asserted.
- **All six new tests execute and pass.** Read from the TRX by test name, not from console text: T1
  through T6 all carry `outcome="Passed"`.
- **The amended characterisation tests are arrangement-only.** The diff for
  `QfcCollectionControllerTests.cs` is three `SeedCollectionKey(kbd, "1");` / `SeedCollectionKey(kbd, "2");`
  lines replaced by `controller.RegisterNavigation();`. No assertion changed. This is the correct
  amendment for a ledger design: the tests must now register through the production path so the
  ledger is populated, rather than seeding the registry behind it. The `SeedCollectionKey` helper is
  still used at line 414, so no dead code was left behind.
- **The `#468` defects-file edit is comment and string-literal only.** Verified line by line: the
  XML doc, one `because:` string, and one inline comment. The relocation it documents is real — with
  `UnregisterNavigation` no longer reading `_itemGroups`, the `NullReferenceException` now originates
  one statement later at `_itemGroups[selection - 1]` inside `RemoveSpecificControlGroupAsync`. The
  comment correctly re-attributes it, and the assertion and expected outcome are unchanged. This is
  exactly the kind of comment-versus-behavior synchronization CR-6 and CR-2 are about, done
  correctly.
- **Test design quality.** The new file is self-contained: it carries its own reflection field
  setter and item-group builder rather than depending on another test file, so it introduces no
  cross-feature coupling. It constructs no form-derived type, uses `FormatterServices.GetUninitializedObject`
  to sidestep the WinForms constructor, and documents why. Scenario coverage spans positive, negative
  (empty ledger), edge (width crossing), error (null field), and state transition (repeated cycles).
- **No banned test constructs.** No `Thread.Sleep`, `Task.Delay`, `DateTime.Now`/`UtcNow`, temporary
  file API, or unseeded `Random` in the new file.
- **Framework compliance.** MSTest, Moq, FluentAssertions throughout.
- **File sizes.** New test file 361 lines. The frozen characterisation file went 500 -> 499 lines and
  held at exactly 13 `[TestMethod]` attributes, respecting issue #468's freeze.
- **Formatter.** `dotnet tool run csharpier check .` re-run in this session: exit 0,
  `Checked 1562 files in 4658ms`, no unformatted file. This matters more than usual here, because a
  string-literal length change can force CSharpier to reflow the enclosing call; it did not.

## Design notes for future work

- The ledger records `SourceId` alongside `Key` even though every entry is `"Collection"`. That is
  the right call: it makes the record self-describing and lets `UnregisterNavigation` pass the
  recorded source rather than a hard-coded literal, so a future second source needs no change to the
  unregister path.
- CR-3 and CR-5 are the two findings worth promoting to issues. CR-3 has the higher value: it turns a
  silent desynchronization into a detectable one, and the fix's own rationale is that silent
  desynchronization is the defect.

## Verdict

**0 blocking findings.** Five non-blocking findings stand (CR-2, CR-3, CR-4, CR-5, CR-6), of which
CR-6 is newly raised by this reaudit. CR-1 is closed and verified.

Recommend merge. Correct CR-2 and CR-6 together in a follow-up — they are one defect in two places,
34 lines apart, and both are documentation text on passing assertions.
