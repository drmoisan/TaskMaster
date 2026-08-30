# Code Review — issue #644, navigation key ledger

- Issue: #644
- Branch: `bug/qfc-unregister-navigation-count-mismatch-orphan-644`
- Head: `a2c69aead286ad0ec6c7087f1bd8c46d39d0d472`
- Base: `main` at `fa2ddefacf2c08abe18f3e3250d77da804534637`
- Review anchor: `e968a1a8804b7641380d4489c496662824d45767`
- Review timestamp: 2026-08-29T23-06

## Scope Reviewed

Six code paths, verified to be the complete branch-versus-base code diff:

| Path | Disposition | Net |
|---|---|---|
| `QuickFiler/Controllers/QfcCollectionController.cs` | modified | +18 / -9 |
| `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationLedgerTests.cs` | created | +361 / -0 |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs` | modified | +14 / -10 |
| `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs` | modified | +8 / -8 |
| `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` | modified | +3 / -4 |
| `QuickFiler.Test/QuickFiler.Test.csproj` | modified | +1 / -0 |

## Overall Assessment

The design is the right one for the defect. The prior implementation expressed the invariant
"unregistration removes exactly what registration added" as a coincidence between two independently
computed quantities — the loop bound at registration time and the loop bound at unregistration time
— and the defect was simply that the two can diverge. Recording the actual set and replaying it makes
the invariant structural: `_itemGroups` is no longer an input to unregistration at all, so no future
mutation path can reintroduce the divergence. The alternative of bracketing the three known
unbracketed call sites would have fixed the three reaches visible today and left the next one
exposed; the spec rejects it on exactly that ground, correctly.

The implementation is small, the ordering constraints are correct, and the test coverage is
proportionate. Five findings, all non-blocking. No blocking findings.

## Findings

| ID | Severity | Classification | Location | Summary |
|---|---|---|---|---|
| CR-1 | Minor | Non-blocking | `QfcCollectionControllerNavigationDigitsTests.cs` lines 192-195 | XML doc describes a mechanism the fix removed |
| CR-2 | Trivial | Non-blocking | `QfcCollectionControllerNavigationDigitsTests.cs` line 145 | Historical sentence names a removal set that is now wrong |
| CR-3 | Minor | Non-blocking | `QfcCollectionController.cs` `UnregisterNavigation()` | Discarded `bool` from `Remove` is now a live drift signal |
| CR-4 | Trivial | Non-blocking | `QfcCollectionController.cs` `UnregisterNavigation()` | Lazy accessor is read twice; allocates on a path that only clears |
| CR-5 | Trivial | Non-blocking | `QfcCollectionController.cs` ledger field | Ledger is a non-thread-safe `List<T>` behind a non-atomic `??=` |

A sixth candidate finding was raised and then withdrawn on verification. It is recorded under
"Withdrawn candidate finding" below rather than dropped silently, so the check is on the record.

---

### CR-1 — Stale mechanism description in a retained XML doc (Minor, Non-blocking)

**Location:** `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs`
lines 189-196, on `UnregisterNavigation_AfterRegisteringAtOneDigitAndGrowingToTen_RemovesTheOneDigitKeys`.

**Current text (lines 192-195):**

```
/// is then added without an intervening unregister, so the live <c>Digits</c> getter now
/// computes width 2. Before the fix <c>UnregisterNavigation</c> removed the never-registered
/// "01".."10" and left all nine single-digit keys orphaned. After the fix it replays the
/// recorded width 1 and, because the loop bound has grown to ten, removes every registered key.
```

**Rule violated:** `CLAUDE.md` C#6.3 and `.claude/rules/general-code-change.md` naming/documentation
section — comments must stay synchronised with behaviour. The spec's own implementation strategy
step 5 makes this obligation explicit and applies it to the `#468` defects file; the same obligation
attaches here.

**Why it is wrong now:** after this change there is no "recorded width" and no "loop bound". The
sentence describes the #472 implementation, which this commit deleted. Read literally, it tells a
future maintainer that `UnregisterNavigation` still derives a format from a stored digit count and
still iterates a count-bounded loop — the two properties this fix removed and that T5 exists to
prevent from returning.

**Why this matters more than the usual comment drift:** this specific test is the empirical proof
that #472's guarantee survived the supersession. A reader who checks the supersession claim will
land on this test, and its documentation currently describes the superseded mechanism as current.

**Verification:**

```
$ sed -n '1188,1197p' QuickFiler/Controllers/QfcCollectionController.cs
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

The method contains no `format` local, no `_registeredDigits` read, and no `_itemGroups.Count`.
`grep -rn "_registeredDigits" --include=*.cs .` returns zero occurrences repository-wide.

**Suggested correction:** replace the final sentence with a statement of the current mechanism, for
example: "After the fix the ledger replays the nine recorded keys `\"1\"..\"9\"` verbatim, so the
added tenth group is irrelevant to unregistration." The pre-fix narrative in the preceding two
sentences is accurate history and should be kept.

**Why non-blocking:** no assertion, no behaviour, and no gate is affected; the test passes and pins
the correct property. It is a documentation defect in a file this change already edited.

---

### CR-2 — Historical sentence names a removal set that is now wrong (Trivial, Non-blocking)

**Location:** same file, line 144-145.

```
/// removed the never-registered "1".."9", leaving all ten two-digit keys orphaned. After the
/// fix it replays the recorded width and removes "01".."09".
```

Under the ledger, unregistration removes `"01".."10"`, not `"01".."09"`. The sentence is accurate as
a description of what #472 alone achieved, and the immediately following paragraph (lines 147-151)
corrects it forward and explicitly records that #644 closed the residual. The correction is adjacent
and unmissable, which is why this is Trivial rather than Minor.

**Suggested correction:** if CR-1 is addressed, tighten this to "After #472 alone it replayed the
recorded width and removed `\"01\"..\"09\"`" so the tense marks it as history.

---

### CR-3 — The discarded `bool` from `Remove` is now a meaningful signal (Minor, Non-blocking)

**Location:** `QuickFiler/Controllers/QfcCollectionController.cs`, `UnregisterNavigation()`:

```csharp
foreach (var (sourceId, key) in RegisteredNavigationKeys)
{
    _kbdHandler.StringActionsAsync.Remove(sourceId, key);
}
RegisteredNavigationKeys.Clear();
```

**Observation, not a defect in this change.** The spec explicitly defers the discarded-`bool`
question to `### Downstream notes` item 5 of the #444 spec, covering all 39 production call sites,
and this review agrees that widening scope here would violate the Bugfix Workflow's minimum-scope
rule. The change is correctly scoped.

What is worth recording is that the *value* of that check has changed. Before the ledger, a `false`
return was expected noise: the loop routinely asked the registry to remove keys it had never
registered, because the format or the bound was wrong. That is precisely the defect. Under the
ledger, every key passed to `Remove` is one the controller observed the registry accept. A `false`
return is now unambiguous evidence that the registry was mutated out of band between register and
unregister — the ledger/registry drift the spec names as risk 2 and records as having no path today.
The fix has converted a noisy return value into a clean invariant violation detector, and the code
still throws it away.

**Recommendation:** promote as a follow-up scoped to this method alone (not the cross-cutting
39-call-site question), suggesting a logged warning on `false` rather than a throw, since the
keyboard boundary already catches and logs.

---

### CR-4 — Lazy accessor read twice; allocates to clear (Trivial, Non-blocking)

`UnregisterNavigation()` reads `RegisteredNavigationKeys` twice — once for the `foreach` and once for
`Clear()`. On a controller that never registered, and specifically on a reflection-built test
instance where the field is null, the first read allocates a `List<(string, string)>` solely so the
second read can clear it. The allocation is one small object on a cold path and is entirely harmless.

A local would avoid it and would also make the single-instance identity explicit:

```csharp
var ledger = RegisteredNavigationKeys;
foreach (var (sourceId, key) in ledger) { _kbdHandler.StringActionsAsync.Remove(sourceId, key); }
ledger.Clear();
```

Style preference only. The current form is correct: the second read cannot return a different
instance, because the first read assigns the field.

---

### CR-5 — Ledger is a non-thread-safe `List<T>` behind a non-atomic `??=` (Trivial, Non-blocking)

`_registeredNavigationKeys` is a plain `List<(string SourceId, string Key)>` and
`RegisteredNavigationKeys` uses `??=`, which is not atomic. Concurrent entry into
`RegisterNavigation` / `UnregisterNavigation` could tear the list or allocate two instances.

**No new exposure is introduced.** The prior code also mutated unsynchronised instance state on the
same paths (`_registeredDigits = digits;` in `RegisterNavigation`) and read `_itemGroups` without a
lock. The `??=` idiom matches the two existing seam accessors in the same file
(`_removeGroupByEntryId`, `_notifyNotReady`). `Digits` is the only member on this path carrying
`[MethodImpl(MethodImplOptions.Synchronized)]`, and it did before this change too. These are
UI-thread-driven navigation members.

One genuine, small difference worth stating: `UnregisterNavigation` previously performed no write to
controller state and now performs one (`Clear()`). That converts it from a read-only member into a
mutating one. The threading posture of the surrounding code is unchanged, so this is recorded rather
than raised.

---

### Withdrawn candidate finding — fully qualified `System.Action` (verified consistent, not a finding)

`QfcCollectionControllerNavigationLedgerTests.cs` writes `System.Action` at lines 215, 251, 286 and
321 while the file already carries `using System;` at line 1. This was initially raised as a
cosmetic inconsistency and was withdrawn after verifying the surrounding convention:

```
$ grep -rn "System.Action" QuickFiler.Test/Controllers/ | head -20
QuickFiler.Test/Controllers/EfcDataModelIssue614Tests.cs:51:            System.Action act = () =>
QuickFiler.Test/Controllers/EfcHomeControllerDependenciesProductionFactoryTests.cs:407:        private static void VerifyArgumentNull(System.Action action, string parameterName)
QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs:194:            System.Action loaded = () =>
QuickFiler.Test/Controllers/EfcHomeControllerTests.cs:18:        private Mock<System.Action> _mockParentCleanup;
... (16 further occurrences across 8 files)
```

Fully qualifying `System.Action` is the established convention throughout
`QuickFiler.Test/Controllers/`, which is unsurprising given that
`Microsoft.Office.Interop.Outlook` also defines an `Action` type and several files in this directory
import that namespace. The general code change policy directs matching the repository's existing
style where one exists. The new file matches it. No finding.

---

## What Was Verified and Found Correct

These were checked specifically because they are the load-bearing claims of the change, and each
holds.

**Record-after-`Add` ordering.** `RegisterNavigationAsyncAction` calls
`_kbdHandler.StringActionsAsync.Add(action)` and only then appends to the ledger. A duplicate-key
`ArgumentException` from `Add` therefore propagates with the ledger unpolluted, which is what keeps
`RegisterNavigation_CalledTwiceWithoutInterveningUnregister_ThrowsArgumentException` passing and what
prevents a subsequent unregistration from removing a key the registry never held. Inverting the two
statements would break that test, so the ordering is pinned by a test rather than by a comment alone.

**Recording the stored key rather than the constructed argument.** The code appends
`(action.SourceId, action.Key)` read back off the constructed `KaStringAsync` instance, not the
pre-construction string. `KaStringAsync`'s constructor and `Key` setter apply `.ToLower()`. For digit
keys that transform is the identity, so this is defensive rather than load-bearing today — but it
makes the ledger exact by construction rather than by a coincidence about digit characters, which is
the right call and is the kind of decision that stops being free later.

**`UnregisterNavigation` no longer reads `_itemGroups`.** Confirmed by reading the method: the only
identifiers it touches are `RegisteredNavigationKeys` and `_kbdHandler.StringActionsAsync`. T5 pins
this structurally by nulling `_itemGroups` and asserting no throw, so a regression that reintroduces
an `_itemGroups`-derived bound fails a test rather than passing silently.

**The frozen-file constraints held.** `QfcCollectionControllerTests.cs` went from 500 to 499 lines
and kept exactly 13 `[TestMethod]` attributes; the reduction is the expected consequence of two
`SeedCollectionKey` lines collapsing into one `RegisterNavigation()` call. `SeedCollectionKey` is
still used at line 414 and did not become dead code. The `*Key 2 SourceId Collection*` assertion is
preserved verbatim at line 422.

**The `#468` defects file changed in comments and string literals only.** The diff adds and removes
only `///` lines, `//` lines, and text inside a `because:` argument. No `Should()`, no `ThrowAsync`,
no `[TestMethod]` appears on any added or removed line. The corrected text is also *right*: under the
ledger, `UnregisterNavigation()` on a null `_itemGroups` completes instead of throwing, so the
`NullReferenceException` the test expects genuinely does originate two statements later at
`_itemGroups[selection - 1]` inside `RemoveSpecificControlGroupAsync`. The test's outcome is
unchanged and its assertion is untouched; only the attribution moved, and it moved to the correct
statement. This is the exact opposite of the common failure mode where a comment is "corrected" to
match a test that is now passing for a different reason than it claims.

**T2 carries an Act step the spec's Test Strategy table did not describe.** The spec's T2 row says
"remove one group directly from the injected `_itemGroups` list, unregister, then register again".
The implemented test additionally restores the page to five groups before the second registration.
This is a strengthening, not a drift, and it is necessary: a 5-group page shrunk to 4 orphans only
`"5"`, and re-registering the 4-group page adds `"1".."4"`, which do not collide with `"5"`, so a
bare shrink-then-re-register does not reproduce the `ArgumentException` the test is meant to pin. The
divergence is disclosed twice — in the plan at its "Correction to a non-acceptance prediction in
spec.md" paragraph, and in the test's own XML documentation, which explains why the restore step is
load-bearing. The spec's "Pre-fix result" column is descriptive text, not an acceptance criterion;
AC-3 requires only that T2 pass after the fix. Correctly handled.

**Host-free testability posture.** Every test allocates the controller through
`FormatterServices.GetUninitializedObject`, injects `_kbdHandler`, `_digits`, and `_itemGroups` by
reflection, and keeps `_digits` equal to the page width so `RegisterNavigation` never routes into
`SetVisualDigits`, which requires WinForms. `ItemViewer` is deliberately left null and the XML doc
says so and says why no test reaches a path that dereferences it. This is the same shape as the
existing `QfcCollectionControllerNavigationDigitsTests` factory, so the new file matches the
established local convention rather than inventing one.

**Self-containment of the new test file.** The file carries its own `SetControllerField`,
`MakeGroup`, `MakeGroups`, `CreateLedgerController`, and `CollectionKeys` helpers rather than
depending on `QfcCollectionController.TestSupport.cs` or on the digits test file. That duplicates
roughly 80 lines of arrangement scaffolding across three files in this directory. On the general
policy's "avoid copy-paste" principle that is a mark against it; on the concrete constraint that
`QfcCollectionControllerTests.cs` is frozen at 500 lines and 13 test methods by #468, and that
sharing helpers across the three files would require editing one of them to host the shared code, it
is the right trade. The file's own XML doc states the reasoning explicitly ("so it introduces no
cross-feature coupling"). Recorded as a deliberate, justified duplication rather than as a finding.

---

## Design Notes for Future Work

- The ExpandedFiler sibling controller's `ToggleOffNavigation` already unregisters by replaying a
  recorded catalogue rather than by a count. This change brings `QfcCollectionController` into line
  with that existing pattern rather than introducing a new one. The spec records this; it is worth
  repeating here because it is the strongest argument that the ledger is the house style for this
  problem and not a local invention.
- The `KbdActions` asymmetry remains open and is the reason this defect was dangerous rather than
  merely untidy: `Add` and `Remove` compare with `EqualityComparer<TKey>.Default`, while `Find`,
  `FindIndex`, `ContainsKey` and the indexer compare with `KeyEquals`, which for `KaStringAsync` is a
  substring test. An orphaned `"10"` therefore collides with a probe of `"1"` under `Find` even
  though `Remove("Collection", "1")` would never have removed it. This fix removes the orphan; it
  does not remove the trap that made the orphan escalate to an `InvalidOperationException`. Deferred
  by name in the spec and in #444's downstream notes, correctly.

## Verdict

**PASS. Blocking findings: 0. Non-blocking findings: 5.**

The change is well-scoped, correctly ordered, adequately tested, and its central claims verify
against the code. CR-1 is the only finding worth acting on before merge, and it is a four-word
sentence in a test comment.
