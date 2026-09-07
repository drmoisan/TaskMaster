# Fail-before exception dossier — issue #489 D3, the set-then-sort ordering contract

Timestamp: 2026-08-28T01-40
Command: (documentation-only change; no runnable gate produces the fail-before signal — see below)
EXIT_CODE: 0
Task: [P9-T5]

## WhyFailingRunImpossible:

The #489 D3 remedy is **documentation on an interface**, not a behavioural change. `IItemViewer`
already declares `SetConversationItems(System.Collections.IList items)` and
`SortConversationByDate(SortOrder order)` as two separate members, and the sole production caller
already invokes them back to back and in the correct order. The deliverable is the XML documentation
added by P9-T4 which states that ordering requirement on the contract itself.

XML documentation is not observable to a unit test. Nothing in the compiled assembly changes when a
`<summary>` element is added, and a test cannot assert that a human-readable contract statement is
present without reading the source file as text. A "RED" test synthesised for this item would take
one of two forms, and both are inadmissible:

1. **Assert the current, already-correct call ordering.** Such a test is green before the change and
   green after it. It can never be red, so it is an acceptance condition that cannot fail — the
   precise defect `.claude/rules/plan-acceptance-gates.md` exists to reject.
2. **Assert the presence of the documentation text by reading the source file.** This is a prose
   search against a file the same task authors, so the executor supplies both the assertion and the
   text that satisfies it. It is likewise unfalsifiable, and it is additionally wrap-fragile: any
   later reflow of the comment breaks a line-oriented search even though the contract is intact.

No third form exists. Therefore no failing run is possible for this item and this dossier stands in
its place, as the evidence conventions permit.

## Alternative proof

### 1. The two tests that must stay green

The behaviour the documentation describes is already covered by two existing tests in
`QuickFiler.Test/Controllers/QfcItemController.ConversationTests.cs`:

| Test | Declared at | Baseline outcome (P0-T13) | Role |
|---|---|---|---|
| `SetTopicThread_WhenNotInvokeRequired_SetsItemsAndSortsDescending` | `QfcItemController.ConversationTests.cs:249` | passed | Proves the pair is invoked back to back, in order, on the UI thread |
| `SetTopicThread_WhenInvokeRequired_MarshalsViaInvoke` | `QfcItemController.ConversationTests.cs:266` | passed | Proves the controller marshals the whole pair as one UI-thread turn |

Both are recorded `passed` in the `BaselineNamedPins:` block of
`FEATURE/evidence/baseline/phase0-vstest-quickfiler.2026-08-28T00-14.md`, and P9-T9 re-runs them
after the Phase 9 edits. The documentation is a statement **about** the invariant these two tests
already enforce; the tests are the executable proof and the documentation makes the requirement
discoverable to the next implementer of `IItemViewer`.

The second test is the load-bearing one for the "within the same UI-thread turn" clause: it proves
`SetTopicThread` re-enters itself through a single `_itemViewer.Invoke(...)` rather than marshalling
each member separately, so the two calls cannot be split across two turns.

### 2. The sole production caller

`git grep -n "SetConversationItems\|SortConversationByDate" -- QuickFiler/` returns exactly one
production call site, and it is the back-to-back pair inside `SetTopicThread`:

```
QuickFiler/Controllers/QfcItemController.Conversation.cs:231:            _itemViewer.SetConversationItems(conversationInfo);
QuickFiler/Controllers/QfcItemController.Conversation.cs:232:            _itemViewer.SortConversationByDate(SortOrder.Descending);
```

The only other production occurrences are the two interface declarations in
`QuickFiler/Viewers/IItemViewer.cs`, the two concrete implementations in
`QuickFiler/Viewers/ItemViewer.WebViewThread.cs:23` and `:25`, and comment text. There is no second
caller anywhere in `QuickFiler/`, and the single caller is already correct. That is the reason the
remedy is documentation rather than code.

### 3. Rejected alternative F2 — `SetConversationItemsSorted(IList, SortOrder)`

The design alternative considered and rejected was to collapse the pair into a single interface
member `SetConversationItemsSorted(IList items, SortOrder order)`, which would make the ordering
requirement unexpressible rather than merely documented.

**Reason for rejection.** The pair has **exactly one** production caller, shown above, and that
caller is already correct, so the change would remove a defect that does not occur in practice.
Against that, F2 changes `IItemViewer` — an interface consumed by sibling-owned test files on this
integration branch. Every `Mock<IItemViewer>` setup and verification naming either member would have
to be rewritten, in files this feature's scope lock forbids editing, and the churn would land in
children 468, 484, 444 and 501 rather than in #489. The cost falls on siblings and the benefit is
hypothetical, so the ordering requirement is recorded on the contract instead.

## Required-element checklist (P9-T5 acceptance)

| Required element | Present |
|---|---|
| Documentation is not observable, so no RED can be synthesised without an unfalsifiable condition under `.claude/rules/plan-acceptance-gates.md` | Yes — WhyFailingRunImpossible: above |
| `SetTopicThread_WhenNotInvokeRequired_SetsItemsAndSortsDescending` (`QfcItemController.ConversationTests.cs:249`) and `SetTopicThread_WhenInvokeRequired_MarshalsViaInvoke` (`:266`) named as the two must-stay-green tests | Yes — alternative proof section 1 |
| Sole production caller identified as the back-to-back pair inside `SetTopicThread` in `QfcItemController.Conversation.cs` | Yes — alternative proof section 2 |
| Rejected alternative F2 (`SetConversationItemsSorted(IList, SortOrder)`) recorded with its reason | Yes — alternative proof section 3 |

Output Summary: Fail-before exception dossier for #489 D3. A failing run is impossible because the
remedy is XML documentation on `IItemViewer`, which produces no observable behaviour and whose only
synthesisable "RED" forms are both unfalsifiable. The alternative proof is the two already-green
`SetTopicThread` tests at `QfcItemController.ConversationTests.cs:249` and `:266` (both `passed` at
P0-T13 baseline, re-run by P9-T9), the single production caller at
`QfcItemController.Conversation.cs:231-232`, and the recorded rejection of alternative F2. All four
elements P9-T5 requires are present.
