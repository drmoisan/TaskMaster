# Code Review — Issue #644 (remediation cycle 2, exit reaudit)

- **Timestamp:** 2026-08-30T13-10
- **Branch:** `bug/qfc-unregister-navigation-count-mismatch-orphan-644`
- **Head:** `4572fef5`
- **Base:** `main` at `fa2ddefacf2c08abe18f3e3250d77da804534637`
- **Span citations anchored at:** `e968a1a8` (recorded `diff_anchor_substitution`)

## Scope reviewed

The full branch diff against the resolved base, not the remediation cycle's two lines:

| Path | Disposition | numstat |
|---|---|---|
| `QuickFiler/Controllers/QfcCollectionController.cs` | modified | +18/-9 |
| `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationLedgerTests.cs` | added | +361/-0 |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs` | modified | +14/-10 |
| `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs` | modified | +14/-14 |
| `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` | modified | +3/-4 |
| `QuickFiler.Test/QuickFiler.Test.csproj` | modified | +1/-0 |

Plus the feature folder and, at the branch tip, 15 `.claude/agent-memory/**` documents. Neither is
code; both are covered in the policy audit at this timestamp.

## Overall assessment

The fix is well shaped. It replaces a bug class rather than three bug instances: a count-bounded
removal loop whose bound is recomputed from mutable state is swapped for a ledger of the exact
`(SourceId, Key)` pairs registration added, replayed verbatim. After the change `_itemGroups` is not
an input to unregistration at all, which is why the invariant is structural rather than a coincidence
between two independently-computed counts.

Two remediation cycles have been spent on documentation-comment accuracy in the test tree, and the
class is now closed and swept. No production code changed in either cycle.

**0 blocking findings.** Four non-blocking findings stand (CR-3, CR-4, CR-5, and CR-7 newly raised
here). CR-1, CR-2 and CR-6 are closed and independently verified.

## The production change

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
    foreach (var (sourceId, key) in RegisteredNavigationKeys)
    {
        _kbdHandler.StringActionsAsync.Remove(sourceId, key);
    }
    RegisteredNavigationKeys.Clear();
}
```

```csharp
internal void RegisterNavigationAsyncAction(int itemIndex, int digits)
{
    var action = GenerateStringKbdAction(itemIndex, digits);
    _kbdHandler.StringActionsAsync.Add(action);

    // Issue #644: record strictly after a successful Add, reading the key back off the
    // constructed instance, so a duplicate-key ArgumentException leaves the ledger clean.
    RegisteredNavigationKeys.Add((action.SourceId, action.Key));
}
```

What is right about it:

- **Record-after-Add is load-bearing and correctly ordered.** A duplicate-key `ArgumentException`
  from `Add` propagates before the ledger entry is appended, so a partially-completed registration
  leaves the ledger claiming only keys the registry actually holds. The retained test
  `RegisterNavigation_CalledTwiceWithoutInterveningUnregister_ThrowsArgumentException` fails if this
  ordering is inverted, and it passes.
- **Reading the key back off the constructed instance** rather than off the pre-construction string
  makes the ledger exact by definition. `KaStringAsync`'s constructor and `Key` setter both lower-case
  their input; for digit keys that is the identity, but the design does not depend on that.
- **The ledger is complete.** Verified rather than assumed: `StringActionsAsync.Add` appears exactly
  once in the whole of `QuickFiler` and `QuickFiler.Test`, inside `RegisterNavigationAsyncAction`,
  which in turn is called only from `RegisterNavigation`'s loop at line 1184. There is no second path
  that registers a `"Collection"` key and could bypass the ledger.
- **Recording `SourceId` alongside `Key`** even though every entry is currently `"Collection"` is the
  right call: the record is self-describing and `UnregisterNavigation` passes the recorded source
  rather than a hard-coded literal, so a future second source needs no change to the unregister path.
- **`_registeredDigits` and its `format` expression are deleted in the same commit** as the ledger.
  That is mandatory rather than optional under the repository type-check gate: retaining an assigned-
  never-read private field raises CS0414, which `/p:TreatWarningsAsErrors=true` promotes to an error.
  The supersession of #472 is recorded in the spec and in the test documentation, and #472's own
  regression test survives unchanged and passing, which settles empirically that this strengthens
  rather than reverts it.

## The test change

- **The new file is genuinely self-contained.** It carries its own reflection field setter,
  item-group builder, and controller factory rather than depending on another test file, so it adds
  no cross-feature coupling. Six tests cover positive flow, the empty-ledger negative case, the
  width-crossing edge, the null-field error path, and repeated state transitions.
- **Host-free by construction.** `FormatterServices.GetUninitializedObject` sidesteps the WinForms
  constructor, and the factory keeps `_digits` equal to the width the page needs so `RegisterNavigation`
  never routes into `SetVisualDigits`. The reason is documented in the factory's own XML comment,
  which is the right place for it.
- **The characterisation-test amendment is arrangement-only.** Three `SeedCollectionKey(...)` lines
  become `controller.RegisterNavigation();`. No assertion changed. This is the correct amendment under
  a ledger design: the tests must register through the production path so the ledger is populated,
  rather than seeding the registry behind it. `SeedCollectionKey` is still used at line 414, so no
  dead helper was left behind.
- **The #468 defects-file edit is comment and string-literal only**, and the relocation it documents
  is real: with `UnregisterNavigation` no longer reading `_itemGroups`, the `NullReferenceException`
  now originates one statement later at `_itemGroups[selection - 1]` inside
  `RemoveSpecificControlGroupAsync`. The assertion and the expected outcome are unchanged.

## Remediation cycle 2 verification

Both items were verified against the tree, not accepted on report. The full evidence is in the policy
audit at this timestamp; the code-level summary is:

- **CR-6 (line 179) closed.** The `because:` message now describes the ledger. It no longer names
  `_registeredDigits`, a field with zero occurrences repository-wide.
- **CR-2 (line 145) closed.** The historical sentence is now past tense and attributed to #472, so the
  paragraph beneath it reads as the update it is.
- **Nothing else moved.** `git diff --numstat d7faef54^ d7faef54` on that file is `2 2`. The three
  test names, the three `[TestMethod]` attributes, both `.Should()` chain structures, the 226-line
  length, and the two cycle-1 corrections at line 222 and lines 189-196 are all unchanged.
- **No formatter reflow.** The replacement literal is nine characters longer and the enclosing
  `.BeEmpty(` call kept its single-argument single-line form; `csharpier check` re-run in this session
  reports the tree at fixed point.

The class-scoped sweep now returns three hits and all three are legitimate past-tense history. The
defect class is closed.

## Findings

| ID | Severity | Blocking | Status | Summary |
|---|---|---|---|---|
| CR-3 | Minor | No | STANDS | The discarded `bool` from `Remove` is now a meaningful desynchronization signal |
| CR-4 | Trivial | No | STANDS | Lazy accessor read twice; allocates in order to clear |
| CR-5 | Trivial | No | STANDS | Ledger is a non-thread-safe `List<T>` behind a non-atomic `??=` |
| CR-7 | Trivial | No | NEW | Call-site count drift between the spec and the cycle-1 code review |

### CR-3 — the discarded `bool` from `Remove` is now a meaningful signal (Minor, Non-blocking) — STANDS

`UnregisterNavigation` discards the `bool` returned by `StringActionsAsync.Remove`. Before this fix
that return was uninformative, because the loop could legitimately ask to remove a key it had never
registered. After this fix the ledger contains exactly the keys registration added, so a `false`
return means a genuine divergence between ledger and registry — the one condition the fix exists to
prevent. Discarding it discards the cheapest self-check the new design makes available.

Unchanged by this cycle, which touched no production file. It remains a promotion candidate rather
than in-scope work: the repository already tracks the cross-cutting question of every production call
site discarding this return, and widening a bugfix to cover it would violate the `CLAUDE.md` Bugfix
Workflow rule. Of the standing findings this is the one worth promoting first, because the fix's own
rationale is that silent divergence is the defect.

### CR-4 — lazy accessor read twice; allocates in order to clear (Trivial, Non-blocking) — STANDS

`UnregisterNavigation` reads `RegisteredNavigationKeys` twice, once for the `foreach` and once for
`.Clear()`. On the no-prior-registration path the `??=` allocates a `List<T>` solely so that zero
items can be enumerated and an empty list cleared. A single local read avoids both:

```csharp
var ledger = _registeredNavigationKeys;
if (ledger == null) { return; }
foreach (var (sourceId, key) in ledger) { _kbdHandler.StringActionsAsync.Remove(sourceId, key); }
ledger.Clear();
```

Trivial: one empty `List<T>` on a keyboard-setup path, not a hot loop.

### CR-5 — ledger is a non-thread-safe `List<T>` behind a non-atomic `??=` (Trivial, Non-blocking) — STANDS

`_registeredNavigationKeys ??= new List<(string SourceId, string Key)>()` compiles to a
read-test-write sequence that is not atomic, and `List<T>` is not safe for concurrent mutation.
Neither `RegisterNavigation` nor `UnregisterNavigation` carries
`[MethodImpl(MethodImplOptions.Synchronized)]`, unlike the neighbouring `Digits` getter.

Trivial in practice: both members are invoked from the UI thread on the QuickFiler surface, and the
code they replace was equally unsynchronized. Recorded so that a future change moving registration off
the UI thread finds the hazard already written down.

### CR-7 — call-site count drift between the spec and the cycle-1 code review (Trivial, Non-blocking) — NEW

`code-review.2026-08-30T01-46.md` describes CR-3 as covering "all 42 production call sites discarding
this return". The spec's measured figure is different and is the one to trust: a content search
returned 41 raw occurrences across four production files, two of which are commented out in
`QuickFiler/Controllers/QfcItemController.EventWiring.cs`, giving **39 live production call sites**.
42 is the figure from the original issue text, which the spec explicitly corrects to 39 under
"Correction to the issue text (call-site count)".

The predecessor artifact is a cycle-entry record and is not rewritten; the figure is corrected forward
here. This has no effect on any verdict — CR-3's substance does not depend on the count — but a
promotion issue drafted from the cycle-1 wording would carry the wrong number into a new issue body,
which is why it is worth one line.

## What was verified and found correct

- **Red-before-green is established by artifact, not asserted.** T1 was recorded failing against
  unmodified production code at `[P1-T4]` and passing at `[P2-T5]`.
- **All six new tests execute and pass**, re-run in this session by name:
  `UnregisterNavigation_AfterGroupRemovedThroughRemoveGroupByEntryIdSeam_RemovesEveryRegisteredKey`,
  `UnregisterNavigation_AfterUnbracketedItemGroupsRemoval_ThenReRegister_DoesNotThrow`,
  `RegisterAndUnregisterNavigation_RepeatedCycles_LeaveRegistryEmpty`,
  `UnregisterNavigation_WithNoPriorRegistration_DoesNotThrowAndLeavesRegistryUnchanged`,
  `UnregisterNavigation_AfterItemGroupsSetToNull_DoesNotThrow`,
  `UnregisterNavigation_AfterTwoDigitRegistrationAndShrinkToNine_LeavesNoCollectionKeys`.
- **The four amended or supersession-critical existing tests pass**, also re-run by name:
  `LoadControlsAndHandlers_01_ReportedRepro_SwapToOverlappingCachedPage_ThrowsBeforeFix`,
  `LoadControlsAndHandlers_01_SwapsPage_RemovesOutgoingKeysAndAddsIncomingKeys`,
  `SwapItemGroups_ThenSkipGuardedTrailingRegister_LeavesExactlyOneEntryPerIncomingKey`,
  `RegisterNavigation_CalledTwiceWithoutInterveningUnregister_ThrowsArgumentException`, plus
  `RemoveSpecificControlGroupAsync_ThrowAtFirstStatement_RestoresReentrancyCounter`.
- **Whole-assembly run green:** 1254 total, 1254 passed, 0 failed.
- **No banned test constructs** in the added file: no `Thread.Sleep`, `Task.Delay`,
  `DateTime.Now`/`UtcNow`, temporary-file API, or unseeded `Random`.
- **Framework compliance:** MSTest, Moq, FluentAssertions throughout.
- **File sizes:** added file 361 lines; the frozen characterisation file went 500 to 499 lines and
  held at exactly 13 `[TestMethod]` attributes, respecting issue #468's freeze; every changed test
  file is under the 500-line limit.
- **Formatter at fixed point:** `dotnet tool run csharpier check .` re-run, `Checked 1562 files`, no
  file reported. This matters here because a literal-length change can force CSharpier to reflow the
  enclosing call; it did not.

## Design notes for future work

- The two findings worth promoting are CR-3 and CR-5, in that order. CR-3 converts a silent divergence
  into a detectable one; CR-5 is a documented hazard rather than a present defect.
- The remaining structural debt in this file is its size (2446 lines, pre-existing) and its reliance
  on `[ExcludeFromCodeCoverage]`. The spec already routes both to a separate refactor, correctly. If
  that refactor happens, the rejected Option B — an extracted `NavigationKeyLedger` type — becomes the
  natural design, and the ledger introduced here is already shaped to be lifted out unchanged.

## Verdict

**0 blocking findings.** Recommend merge. CR-3, CR-4, CR-5 and CR-7 are non-blocking and should be
carried into promotion rather than into a third remediation cycle.
