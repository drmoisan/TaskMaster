# qfc-collection-conversation-index-defects (Issue #470)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/qfc-collection-conversation-index-defects/ (Issue #470)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #470
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/470
- Last Updated: 2026-08-08
## Summary

Three related index and null-guard defects in `QfcCollectionController`'s conversation-expansion
path: a `-1` index used to subscript `_itemGroups`, a slot-reservation count that can disagree with
the number of members actually inserted, and an unguarded dereference that precedes its own null
check.

## Environment

- OS/version: n/a (logic defects, reproducible wherever QuickFiler runs)
- Python version: n/a
- Command/flags used: n/a
- Data source or fixture: `QuickFiler/Controllers/QfcCollectionController.cs`

## Steps to Reproduce

**Defect 1 — `ToggleGroupConv(string)` can index `_itemGroups[-1]` (`:1733-1766`)**

1. At `:1743`, `indexOriginal` is `-1` when the original message has already been removed.
2. That routes to `PromoteFirstChild(originalId, ref childCount)` at `:1970`.
3. `PromoteFirstChild` calls `FindIndex` at `:1972`. If no group carries
   `ConvOriginID == originalId`, that also returns `-1`.
4. Line 1975 then evaluates `_itemGroups[-1].ItemViewer`, throwing `ArgumentOutOfRangeException`.
5. `ChangeConversationSilently(indexOriginal, true)` at `:1749` fails the same way for the same
   reason.

**Defect 2 — `EnumerateConversationMembers` count mismatch (`:1875-1922`)**

1. `ToggleUnGroupConv` reserves `insertCount = conversationCount - 1` slots at `:1823` and
   `:1827-1829`.
2. `EnumerateConversationMembers` iterates `insertions.Count` at `:1888-1889`, a count derived
   independently from `resolver.ConversationItems.SameFolder` at `:1883-1886`.
3. If the resolver returns **more** members than `conversationCount - 1`, then
   `_itemGroups[i + insertionIndex]` at `:1893` walks past the reserved slots and re-initializes
   groups that already hold other messages.
4. If it returns **fewer**, the empty placeholder `QfcItemGroup`s created at `:2008` are left with a
   `null` `ItemController`, which the next `RenumberGroups` at `:2068` dereferences.

**Defect 3 — `SetVisualDigits` inconsistent null handling (`:130-146`)**

1. Line 140 dereferences `grp.ItemController.ItemNumberDigits` with no guard.
2. Lines 141-142 guard the same object: `grp.ItemController?.ItemNumber... ?? 0.ToString(format)`.
3. Since `ItemController` genuinely can be `null` (see Defect 2), line 140 throws before the guard on
   line 141 is ever evaluated.

## Expected Behavior

1. A `-1` index should be handled explicitly rather than used to subscript `_itemGroups`.
2. The number of slots reserved and the number of members inserted should be derived from a single
   source, or reconciled before insertion.
3. `SetVisualDigits` should guard `ItemController` consistently across all three reads.

## Actual Behavior

1. `ArgumentOutOfRangeException` when a conversation's original message is gone and no child carries
   the matching `ConvOriginID`.
2. Either existing groups are silently overwritten, or placeholder groups with a `null`
   `ItemController` cause a `NullReferenceException` in `RenumberGroups`.
3. `NullReferenceException` at line 140 whenever `ItemController` is null.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: Confirmed directly against source at the line numbers above. Discovered during preparation
  research for issue #454 (epic #136, child F11); full analysis in
  `docs/features/active/2026-08-07-quickfiler-collection-controller-coverage-454/research/qfc-collection-controller.md`
  sections E11, E12, and E13.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

All three throw rather than corrupt, so the failure is visible. Defect 2's overwrite branch is the
most serious because it can silently re-initialize a group holding a different message.

## Suspected Cause / Notes

Defects 1 and 3 are both "guard placed after the dereference it protects", the same shape as the
`GetMoveDiagnostics` defect filed separately. Defect 2 is a genuine two-sources-of-truth problem
between the reservation count and the resolver result.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: `ToggleGroupConv` with a removed original and no matching child;
      `EnumerateConversationMembers` with resolver counts above, equal to, and below
      `conversationCount - 1`; `SetVisualDigits` with a null `ItemController`.
- [x] Integration scenario to retest: expand and collapse a conversation whose original message was
      filed in a previous step.
- [x] Manual verification notes: Defect 2 should be fixed by reconciling the counts before insertion,
      not by clamping the loop, so that a resolver disagreement is surfaced rather than hidden.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
