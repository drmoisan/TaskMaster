# Downstream handoff to sibling issue #444

Timestamp: 2026-08-26T16-26

Command: not applicable (this artifact is a written handoff record, not a command step)

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

Three findings from this feature's work bear on sibling issue #444 (duplicate keyboard-action
registration). All three are recorded below with `file:line` citations. Two of them are resolved as a
side effect of this feature; the third is a hardening opportunity this feature deliberately does not
take, per D2.

Base-commit citations are against `61edc19b`, the merge base recorded in
`evidence/baseline/p0-t10-git-baseline.2026-08-26T08-25.md`. Current-tree citations are against the
tree at the head of `bug/qfc-collection-controller-defects-468`.

---

## Fact 1 — this feature removes `WireUpKeyboardHandler` and resolves the duplicate registration as a side effect

At the base commit, `QuickFiler/Controllers/QfcCollectionController.cs:1254-1273` declared
`public void WireUpKeyboardHandler()`. Its body built a `KbdActions<Keys, KaKey, Action<Keys>>` from a
`List<KaKey>` literal containing three entries, two of which register **the same key**:

| Base-commit line | Entry |
|---|---|
| `QuickFiler/Controllers/QfcCollectionController.cs:1268` | `new KaKey("Collection", Keys.Up, (k) => SelectPreviousItem())` |
| `QuickFiler/Controllers/QfcCollectionController.cs:1269` | `new KaKey("Collection", Keys.Down, (k) => SelectNextItem())` |
| `QuickFiler/Controllers/QfcCollectionController.cs:1270` | `new KaKey("Collection", Keys.Down, (k) => _parent.ActionOkAsync())` |

Lines `:1269` and `:1270` share both the source id `"Collection"` and the key `Keys.Down`. That is
exactly the duplicate registration issue #444 describes.

`WireUpKeyboardHandler` was one of the twelve dead members removed by P1-T2 as part of issue #468. It
is absent from the current tree: a search of the owned file returns zero hits for the identifier
(`evidence/qa-gates/p1-t3-dead-identifier-sweep.2026-08-26T08-45.md`), and the reflective-caller
search over 398 build-input files plus all 42 `GetMethod(` call sites returns zero hits for it as well
(`evidence/other/p1-t1-reflective-caller-search.2026-08-26T08-25.md`).

**Consequence for #444.** The duplicate is gone, but it is gone because the *caller* was deleted, not
because the duplicate-registration hazard was fixed. The member was dead — it was reachable from no
call site — so the duplicate was dormant rather than active before removal. Issue #444 should treat
this as one instance removed, not as the class of defect closed.

`WireUpAsyncKeyboardHandler` at base `QuickFiler/Controllers/QfcCollectionController.cs:1275` is a
different member and was **not** removed; its analogous list at base `:1287-1288` registers `Keys.Up`
and `Keys.Down` once each, so it carries no duplicate.

---

## Fact 2 — the collection constructor performs no duplicate check while both add overloads do and throw

`QuickFiler/Controllers/KbdActions.cs` exposes three ways to populate the action list, and they do not
agree on duplicate handling.

| Member | `file:line` | Duplicate check | Behaviour on duplicate |
|---|---|---|---|
| `KbdActions(IEnumerable<UClass> list)` | `QuickFiler/Controllers/KbdActions.cs:26-29` | **none** — the body is `_list = new List<UClass>(list);` | duplicate is accepted silently |
| `Add(string sourceId, TKey key, VDelegate @delegate)` | `QuickFiler/Controllers/KbdActions.cs:90-104` | `_list.Any(x => x.SourceId == sourceId && StoredKeyEquals(x.Key, key))` at `:92` | logs at error level, then `throw new ArgumentException(message)` at `:97` |
| `Add(UClass instance)` | `QuickFiler/Controllers/KbdActions.cs:106-120` | `_list.Any(...)` at `:108-113` | logs at error level, then `throw new ArgumentException(message, nameof(instance))` at `:118` |

This asymmetry is the mechanism by which the base-commit duplicate at
`QuickFiler/Controllers/QfcCollectionController.cs:1269-1270` was able to exist at all. Had those two
`KaKey` instances been added through either `Add` overload, the second would have thrown. They were
passed through the collection constructor instead, which accepts them without inspection.

**Consequence for #444.** The durable fix is a duplicate check in the collection constructor that
matches the two `Add` overloads. This feature does **not** make that change: per D2, removing
`WireUpKeyboardHandler` deletes a caller and deletes zero lines in
`QuickFiler/Controllers/KbdActions.cs`. That file is outside this feature's owned file set and is
named explicitly by the scope lock as must-not-touch. The hardening is handed to #444.

One design question #444 must answer that this record cannot: whether the constructor should throw on
a duplicate (matching `Add`) or reject the whole list up front, since a mid-list throw from a
constructor leaves the caller with no object and a partially consumed enumerable.

---

## Fact 3 — the conversation-member enumerator previously never read its conversation-count parameter, and this feature makes that count live

At the base commit, `EnumerateConversationMembers` was declared at
`QuickFiler/Controllers/QfcCollectionController.cs:1875-1881` with six parameters, the fourth being
`int conversationCount` at `QuickFiler/Controllers/QfcCollectionController.cs:1879`.

The identifier `conversationCount` appears **nowhere** in that method's body. The body re-resolved the
member list itself, from `resolver.ConversationItems.SameFolder`, and drove its loop from
`insertions.Count`. The declared count was accepted and discarded.

Verification of that negative claim:

`SearchScope:` the body of `EnumerateConversationMembers` at the base commit,
`QuickFiler/Controllers/QfcCollectionController.cs:1882-1935`.
`SearchPatterns:` `conversationCount`.
`SearchResult:` none. The only hit in the range `:1875-1935` is the parameter declaration itself at
`:1879`.

In the current tree the parameter is gone from that method — its signature at
`QuickFiler/Controllers/QfcCollectionController.cs:1844-1850` now takes
`IReadOnlyList<MailItem> insertions` at `:1848` in its place, so the method no longer re-resolves and
no longer carries a value it does not use.

The count itself is now **live**, one level up. `ToggleUnGroupConv` still receives `conversationCount`
at `QuickFiler/Controllers/QfcCollectionController.cs:1668`, and passes it at
`QuickFiler/Controllers/QfcCollectionController.cs:1705` into
`ReconcileInsertionCount(...)`, declared at `QuickFiler/Controllers/QfcCollectionController.cs:1801`,
which compares it against the resolved count at
`QuickFiler/Controllers/QfcCollectionController.cs:1809`
(`if (insertionsCount != conversationCount - 1)`) and emits a single warning naming all six values at
`QuickFiler/Controllers/QfcCollectionController.cs:1813` when they disagree. Per D5 the production
behaviour is log-and-proceed with the resolved count winning; no `throw` was introduced.

**Consequence for #444.** This is context rather than an action item. It is recorded here because
#444's triage material treats the `conversationCount` parameter as evidence that the conversation
path is unverified, and that reading is now out of date: the value is read, compared, and reported
from `QuickFiler/Controllers/QfcCollectionController.cs:1801-1817`, and three tests pin the three arms
of the comparison
(`ReconcileInsertionCount_AboveReservation_ReturnsInsertionsCountAndWarnsOnce`,
`ReconcileInsertionCount_EqualToReservation_ReturnsInsertionsCountAndDoesNotWarn`,
`ReconcileInsertionCount_BelowReservation_ReturnsInsertionsCountAndWarnsOnce`,
all green in `evidence/regression-testing/p7-t12-pass-after.2026-08-26T10-39.md`).

---

## Acceptance verification

All three facts are stated, each with `file:line` citations:

| Fact | Citations |
|---|---|
| 1 — removal of `WireUpKeyboardHandler` resolves the duplicate as a side effect | base `QfcCollectionController.cs:1254-1273`, duplicate pair at `:1269` and `:1270` |
| 2 — collection constructor has no duplicate check; both `Add` overloads check and throw | `KbdActions.cs:26-29`, `:90-104` (check `:92`, throw `:97`), `:106-120` (check `:108-113`, throw `:118`) |
| 3 — the enumerator never read `conversationCount`; this feature makes it live | base `QfcCollectionController.cs:1879` declared, `:1882-1935` never read; current `:1848` replaced, count now read at `:1705`, `:1809`, `:1813` |
