# [P5-T3] Upstream-contract conformance review

Timestamp: 2026-08-27T20-07
Command: `$mb = git merge-base HEAD origin/epic/quickfiler-bug-family-integration` then `git diff "$mb..HEAD" -- QuickFiler/Controllers/KbdActions.cs QuickFiler/Controllers/QfcItemController.Navigation.cs`, compared row by row against the `### Upstream contract (exhaustive) — required by features 464 and 489` tables of `spec.md`
EXIT_CODE: 0
Output Summary: every ADDED, CHANGED, and REMOVED row of both tables verdicts **MATCH**. No member
outside those tables changed in either file.

Merge base `4f238289090e4c97ca505511a5a73e8092dce0f9`, re-derived per `[P5-T1]`.

## `QuickFiler/Controllers/KbdActions.cs`

### ADDED members: none

| Row | Verdict | Evidence |
| --- | --- | --- |
| the table declares ADDED members: **none** | **MATCH** | The diff for this file adds no member declaration. Every added line is either an XML documentation comment on the pre-existing enumerable constructor, an explanatory `//` comment, or a statement inside that constructor's existing body. No added line contains a method, property, field, indexer, or constructor declaration. |

### REMOVED members: none

| Row | Verdict | Evidence |
| --- | --- | --- |
| the table declares REMOVED members: **none** | **MATCH** | The diff for this file contains **zero** deletion lines. `git diff` reports additions only. |

### CHANGED members: one (behaviour only; signature identical)

| Row | Contract claim | Verdict | Evidence |
| --- | --- | --- | --- |
| Enumerable constructor, `public`, instance, `public KbdActions(IEnumerable<UClass> list)` | Signature identical; now throws `ArgumentException` with parameter name `list` when two or more elements share a `SourceId` and a `StoredKeyEquals`-equal `Key`; message contains `already exists`; logged via `logger.Error` before the throw; `ArgumentNullException` for a null `list` unchanged; duplicate-free sequence accepted; `KeyEquals`-overlapping but stored-key-distinct pairs remain legal | **MATCH** | The declaration line `public KbdActions(IEnumerable<UClass> list)` is not in the diff, so signature, accessibility, and static-ness are untouched. The added body is a nested `for`/`for` scan whose `if` compares `_list[i].SourceId == _list[j].SourceId && StoredKeyEquals(_list[i].Key, _list[j].Key)`, then calls `logger.Error(message)` and `throw new ArgumentException(message, nameof(list))`. The interpolated message begins `Cannot add key because it already exists.`, carrying the required literal fragment. `StoredKeyEquals` is used and `KeyEquals` does not appear in any added line. The constructor's first statement, `_list = new List<UClass>(list);`, is unchanged and still materialises the sequence, which is what preserves `ArgumentNullException` for a null argument. |

### No member outside the tables changed

**Recorded: no member outside the two tables changed in `KbdActions.cs`.** The diff is confined to
the enumerable constructor's documentation comment and body. None of the twelve members the spec
enumerates as UNCHANGED — the parameterless constructor, `StoredKeyEquals`, the indexer,
`ContainsKey`, `FilterKeys`, `Find`, `FindIndex`, both `Add` overloads, `Remove`, both
`GetEnumerator` forms, and `Keys` — appears in the diff, so none changed in signature,
accessibility, static-ness, attributes, or behaviour. `Remove` retaining its `bool` return with no
`TryRemove` added is independently gated by `[P5-T4]`.

## `QuickFiler/Controllers/QfcItemController.Navigation.cs`

### ADDED members: one

| Row | Contract claim | Verdict | Evidence |
| --- | --- | --- | --- |
| `SyncExpandedRegistrations`, `private`, instance, `private void SyncExpandedRegistrations(bool expanded)`, attributes **none**, unconditionally calls both unregister methods then both register methods when `expanded` | as stated | **MATCH** | The diff adds exactly one member declaration: `private void SyncExpandedRegistrations(bool expanded)`. It is `private` and instance, matching the row. Its declaration carries no attribute — the lines immediately preceding it are `///` documentation only, with no attribute line — so it does **not** carry `[ExcludeFromCodeCoverage]`, as the row requires. Its body is `UnregisterExpandedActions(); UnregisterExpandedAsyncActions(); if (expanded) { RegisterExpandedActions(); RegisterExpandedAsyncActions(); }`: unconditional unregistration followed by conditional registration, exactly as described. |

### REMOVED members: none

| Row | Verdict | Evidence |
| --- | --- | --- |
| the table declares REMOVED members: **none** | **MATCH** | The four deletion lines in this file's diff are four call statements inside two existing method bodies: `RegisterExpandedActions();`, `UnregisterExpandedActions();`, `RegisterExpandedAsyncActions();`, `UnregisterExpandedAsyncActions();`. No deletion line is a member declaration, so no member was removed. The four called methods themselves live in `QuickFiler/Controllers/QfcItemController.EventWiring.cs`, which `[P5-T2]` confirms is absent from the branch diff. |

### CHANGED members: two (body only)

| Row | Contract claim | Verdict | Evidence |
| --- | --- | --- | --- |
| `ToggleExpansion(Enums.ToggleState)`, `public`, instance `virtual`, `public virtual void ToggleExpansion(Enums.ToggleState desiredState)`, `[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]` retained, delegates to `SyncExpandedRegistrations(_expanded)` | as stated | **MATCH** | Body-only change. The hunk `@@ -177,13 +203,12 @@` replaces two conditional registration call sites with the single line `SyncExpandedRegistrations(_expanded);`. Neither the declaration line nor its attribute line appears in the diff; at the branch head they read `[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]` on line 199 and `public virtual void ToggleExpansion(Enums.ToggleState desiredState)` on line 200. Accessibility, `virtual`ness, parameter list, return type, and the attribute are all retained. |
| `ToggleExpansionAsync(Enums.ToggleState)`, `public`, instance `virtual` `async Task`, `public virtual async Task ToggleExpansionAsync(Enums.ToggleState desiredState)`, `[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]` retained, delegates to `SyncExpandedRegistrations(_expanded)` | as stated | **MATCH** | Body-only change. The hunk `@@ -195,13 +220,12 @@` makes the mirror substitution. At the branch head the attribute is on line 216 and the declaration `public virtual async Task ToggleExpansionAsync(Enums.ToggleState desiredState)` on line 217; neither line appears in the diff. |

### No member outside the tables changed

**Recorded: no member outside the two tables changed in `QfcItemController.Navigation.cs`.** The
diff comprises three hunks. The first, `@@ -168,6 +168,32 @@`, adds the new `private` member and its
documentation. The second and third are the body-only substitutions in the two `ToggleState`
overloads. No other member declaration or body appears. The members the spec enumerates as
unchanged — the parameterless `ToggleExpansion()` and `ToggleExpansionAsync()` routing overloads,
`ToggleExpansionOn()`, `ToggleExpansionOff()`, `JumpToFolderDropDown`, `JumpToFolderDropDownAsync`,
`JumpToSearchTextbox`, `JumpToAsync`, both `KbdExecuteAsync` overloads, `MenuDropDown`, `Reply`,
`ReplyAll`, `Forward`, and both `ToggleConversationCheckbox` overloads — are all absent from the
diff. In particular `ToggleExpansionOn()` and `ToggleExpansionOff()` retain their `_expanded` write
and their `_emailIsReadTimer` handling including the 4000 ms `Change` call, which `[P5-T6]`
independently gates by asserting the diff adds no line containing `Timer`.

## Acceptance

- The artifact records a verdict for every ADDED, CHANGED, and REMOVED row of both tables — met: for
  `KbdActions.cs` the ADDED-none row, the REMOVED-none row, and the one CHANGED row; for
  `QfcItemController.Navigation.cs` the one ADDED row, the REMOVED-none row, and the two CHANGED rows.
- Every verdict is MATCH — met; six of six.
- It records that no member outside those tables changed — met, once per file under the heading of
  that name.
