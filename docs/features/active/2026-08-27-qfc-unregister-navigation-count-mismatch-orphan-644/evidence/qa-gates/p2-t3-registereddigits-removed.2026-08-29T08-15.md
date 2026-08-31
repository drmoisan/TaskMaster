# QA gate — UnregisterNavigation replays the ledger; `_registeredDigits` fully removed ([P2-T3])

- Issue: #644
- Task: `[P2-T3]`
- Timestamp: 2026-08-29T08-15
- File modified: `QuickFiler/Controllers/QfcCollectionController.cs`

## The four changes, made as one indivisible edit

1. The body of `UnregisterNavigation()` replaced with a `foreach` over `RegisteredNavigationKeys`
   calling `_kbdHandler.StringActionsAsync.Remove(sourceId, key)` per recorded pair, followed by
   `RegisteredNavigationKeys.Clear()`.
2. The `var format = _registeredDigits == 2 ? "00" : "";` expression deleted with its two-line
   `// Issue #472:` comment.
3. The `_registeredDigits = digits;` assignment deleted from `RegisterNavigation()`.
4. The `private int _registeredDigits;` field declaration deleted together with its
   `// Issue #472:` comment.

**Why the three deletions are indivisible.** Deleting only the `format` expression would leave a
private field that is assigned and never read, which the C# compiler reports as **CS0414**. The
repository type-check gate runs `/p:TreatWarningsAsErrors=true` and promotes CS0414 to a build
error, and `.editorconfig`'s catch-all `dotnet_analyzer_diagnostic.severity = suggestion` covers
analyzer rule IDs rather than compiler `CSxxxx` diagnostics, so it gives no cover. The
`[P0-T10]` baseline recorded zero `CS0414` diagnostics before this edit precisely because the
field was both assigned and read; removing one side without the other would introduce the first.

**This is supersession of #472, not a revert.** #472 owns the key **format**; #644 owns the key
**cardinality**. A ledger replaces both, because it replays recorded strings verbatim. #472's
guarantee — "unregistration removes keys in the width they were registered at" — is strictly
strengthened, since verbatim replay cannot reconstruct a wrong width. #472's landed fix, commit
`9494ca35`, is present on this base and is not reopened, reverted, or re-litigated by this task.

## Post-edit text of `UnregisterNavigation()`, quoted verbatim

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

Read at lines 1188-1197 in the final state.

**Comment length revised during `[P4-T8]`.** This member's explanatory comment was originally
written as five lines. `[P4-T8]`'s acceptance and AC-14 both require the `--stat` net addition for
this file to be no greater than 10 lines, and the first draft measured +15. This comment and the
one in `RegisterNavigationAsyncAction` were each condensed from five lines to two, bringing the net
addition to **+9**. The condensation is comment-only: the statement sequence is unchanged, and the
condensed comment is still free of the token `_itemGroups`, so the acceptance clauses below are
unaffected. The Phase 4 toolchain loop was restarted from `[P4-T1]` after the change.

## Acceptance clause 1 — `_registeredDigits` gone from every `.cs` file

Command: `git grep -F -n '_registeredDigits' -- '*.cs'`
EXIT_CODE: 1 (no output)

```
(no output)
```

The pathspec scope `-- '*.cs'` is required, not a convenience. The identifier legitimately remains
in Markdown documents — this feature's `spec.md`, `issue.md`, and plan, and the #444 feature
folder — because those documents record the supersession. An unscoped search would return those
Markdown hits and could never reach zero.

## Acceptance clause 2 — the member body contains no `_itemGroups` token

Command: fixed-string count of `_itemGroups` over the member's lines 1188-1197.

```
itemGroups-token-occurrences-in-member=0
```

**Zero occurrences.** The explanatory comment was deliberately worded to say "the live item-group
count" and "the item-group collection" rather than naming the field, so that this textual gate is
decided by the member's actual content rather than by an incidental mention inside a comment.

The behavioural counterpart of this structural claim is the test
`UnregisterNavigation_AfterItemGroupsSetToNull_DoesNotThrow`, which was demonstrated red in
`[P1-T4]` with a `NullReferenceException` raised at the old `_itemGroups.Count` loop bound
(`QfcCollectionController.cs` line 1189 pre-edit) and is gated green in `[P2-T5]`.

Output Summary: `UnregisterNavigation()` now replays and clears the ledger and no longer reads the
item-group collection; the `format` expression, the `_registeredDigits` assignment, and the
`_registeredDigits` field declaration with its `// Issue #472:` comment were all deleted in the
same edit. `git grep -F -n '_registeredDigits' -- '*.cs'` produces **no output and exits 1**, and
the edited member body contains **zero** occurrences of the token `_itemGroups`. Both `[P2-T3]`
acceptance clauses hold.
