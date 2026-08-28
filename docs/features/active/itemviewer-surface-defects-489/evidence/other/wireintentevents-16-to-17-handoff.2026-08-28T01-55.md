# Upstream-484 handoff — `WireIntentEvents()` goes from 16 to 17 intent wires

Timestamp: 2026-08-28T01-55
Command: (count subscriptions in WireIntentEvents and detachments in UnwireIntentEvents in QuickFiler/Controllers/QfcItemController.EventWiring.cs, and read Upstream484Landed from the P0-T17 artifact)
EXIT_CODE: 0
Task: [P10-T11]

## The change

`WireIntentEvents()` in `QuickFiler/Controllers/QfcItemController.EventWiring.cs` wired **16** intent
events before this feature and wires **17** after.

The 17th is the single line P10-T5 records as the whole of the `EventWiring.cs` diff:

```
_itemViewer.PicturesChanged += this.CbxPictures_CheckedChanged;
```

It is the #486 D3 fix: an intent event that was declared on the viewer and on the interface but never
wired on the QFC controller path. The handler `CbxPictures_CheckedChanged` lands in the 489-owned
`QuickFiler/Controllers/QfcItemController.EventHandlers.cs`; only the wire statement is cross-child.

### The 17 intent wires, as measured

`WireIntentEvents()` is declared at `QuickFiler/Controllers/QfcItemController.EventWiring.cs:66` and
ends before the next member declaration at `:101`.

| # | Event | Handler | Added by this feature |
|---|---|---|---|
| 1 | `ConversationModeChanged` | `CbxConversation_CheckedChanged` | No |
| 2 | `FlagTaskClicked` | `BtnFlagTask_Click` | No |
| 3 | `PopOutClicked` | `BtnPopOut_Click` | No |
| 4 | `DeleteItemClicked` | `BtnDelItem_Click` | No |
| 5 | `ReplyClicked` | `BtnReply_Click` | No |
| 6 | `ReplyAllClicked` | `BtnReplyAll_Click` | No |
| 7 | `ForwardClicked` | `BtnForward_Click` | No |
| 8 | `BodyDoubleClick` | `TxtboxBody_DoubleClick` | No |
| 9 | `SearchTextChanged` | `TextBoxSearch_TextChanged` | No |
| 10 | `FolderKeyDown` | `_kbdHandler.CboFolders_KeyDownAsync` | No |
| 11 | `FolderSelectionChanged` | `CboFolders_SelectedIndexChanged` | No |
| 12 | `WebViewInitializationCompleted` | `WebView2Control_CoreWebView2InitializationCompleted` | No |
| 13 | `ConversationItemSelectionChanged` | `TopicThread_ItemSelectionChanged` | No |
| 14 | `SearchKeyDown` | `TextBoxSearch_KeyDown` | No |
| 15 | `EmailCopyChanged` | `CbxEmailCopy_CheckedChanged` | No |
| 16 | `AttachmentsChanged` | `CbxAttachments_CheckedChanged` | No |
| **17** | **`PicturesChanged`** | **`CbxPictures_CheckedChanged`** | **Yes** |

A raw `+=` line count over the member returns 18; one of those is the commented-out
`//_itemViewer.TxtboxSearch.KeyDown += …` line, which is not a subscription. The live count is 17,
and 16 without this feature's addition.

## The obligation this creates on 484

Upstream **484** introduces `UnwireIntentEvents()` with a documented count of **16** intent
detachments. `docs/features/active/qfc-item-controller-defects-484/spec.md:358` records
`Cleanup()` as detaching "23 additional event subscriptions before nulling: 6 control-tree and **16**
intent through `UnwireEvents()`", and `:664` specifies the test obligation as
`VerifyRemove(v => v.ConversationModeChanged -= It.IsAny<EventHandler>(), Times.Once())` "for each of
the **16** intent events".

**Adding a 17th wire obligates a 17th detachment.** That detachment is an explicit obligation on
upstream **484**, not on this feature: `UnwireIntentEvents()` lives in the 484-owned member set and
the scope lock for #489 confines the `EventWiring.cs` diff to `WireIntentEvents` alone (P10-T5).

### The gap is present and measured

`UnwireIntentEvents()` is declared at `QuickFiler/Controllers/QfcItemController.EventWiring.cs:446`.
A `-=` count over its body returns **16**: `ConversationModeChanged`, `FlagTaskClicked`,
`PopOutClicked`, `DeleteItemClicked`, `ReplyClicked`, `ReplyAllClicked`, `ForwardClicked`,
`BodyDoubleClick`, `SearchTextChanged`, `FolderKeyDown`, `FolderSelectionChanged`,
`WebViewInitializationCompleted`, `ConversationItemSelectionChanged`, `SearchKeyDown`,
`EmailCopyChanged` and `AttachmentsChanged`.

There is **no `PicturesChanged -=` line**. Wires and detachments therefore stand at 17 and 16
respectively, and the imbalance is exactly the one this record hands to 484.

The practical consequence is a leaked subscription: after `Cleanup()`, a `QfcItemController` that has
been wired still holds a live `PicturesChanged` subscription on its viewer, so a recycled viewer can
deliver a picture-toggle event to a controller that has been torn down. The remedy is one line in
`UnwireIntentEvents()`, mirroring the 16 already there, plus a matching `VerifyRemove` in 484's test
obligation, taking its documented count from 16 to 17.

## Upstream landing state

`Upstream484Landed: true`

Read from `FEATURE/evidence/baseline/phase0-upstream-landing-check.2026-08-27T23-32.md` (P0-T17),
which recorded 16 matches for the combined upstream-member grep and attributed five of 484's six named
members plus 444's `SyncExpandedRegistrations` to their expected locations. Both upstreams are on this
integration branch. `Upstream444Landed:` was likewise `true`.

Because 484 has already landed, this is a handoff to work that is **merged, not pending**: the 17th
detachment cannot be added by 484's in-flight branch and must be raised against 484's file as a
follow-up. That is the material difference from the planning-time assumption, which anticipated 484
being unlanded and able to absorb the obligation before merge.

## Acceptance

| P10-T11 condition | Result |
|---|---|
| The artifact exists | Yes — this file |
| It states the 16 to 17 change | Yes — § The change, with the full 17-row wire table and the measured before/after counts |
| It names 484 as the owner of the matching detachment | Yes — § The obligation this creates on 484 |
| It records the `Upstream484Landed:` value from P0-T17 | Yes — `Upstream484Landed: true` |
| It cites 484 spec lines 358 and 664 for the documented count of 16 | Yes |

Output Summary: `WireIntentEvents()` wired **16** intent events before this feature and wires **17**
after; the added line is `_itemViewer.PicturesChanged += this.CbxPictures_CheckedChanged;`, the #486
D3 fix, and it is the entire `EventWiring.cs` diff. Upstream 484 introduces `UnwireIntentEvents()`
with a documented count of **16** detachments (484 spec lines 358 and 664), and a measured `-=` count
over that member confirms 16 with no `PicturesChanged` detachment, so the wire/unwire balance now
stands at 17 to 16. Supplying the 17th detachment is an explicit obligation on **484**, whose file
owns that member; this feature's scope lock confines its `EventWiring.cs` diff to `WireIntentEvents`.
`Upstream484Landed: true` per P0-T17, so 484 is already merged and the obligation must be raised as a
follow-up against its file rather than absorbed before merge.

---

## Addendum — 2026-08-28: the obligation was discharged in this branch

Timestamp: 2026-08-28T03-52
Task: [P3-T1] (remediation cycle 1)

ObligationDischargedInBranch: true

The sections above are unaltered and remain the historical record of what was measured and handed off
on 2026-08-28T01-55. This addendum records what changed afterwards.

### What feature review found

Feature review of this branch raised one Blocking finding, **RC-1**, recorded in
`FEATURE/code-review.2026-08-28T03-13.md` and carried into
`FEATURE/remediation-inputs.2026-08-28T03-13.md`. It found that the imbalance this record describes
did not merely exist as a handoff note: it had **shipped on this branch**. `WireIntentEvents()`
performed 17 subscriptions and `UnwireIntentEvents()` performed 16 detachments on the branch head, so
a `QfcItemController` that had been wired and then passed through `Cleanup()` retained one live
`PicturesChanged` subscription and kept the torn-down controller reachable from its pooled viewer.

### Why the handoff had no recipient

This record itself states `Upstream484Landed: true`. 484 was **already merged** when the handoff was
written. An obligation cannot be handed to a branch that has already landed: there was no in-flight
484 work left to absorb the 17th detachment before merge. The handoff was therefore never actually
transferable, and leaving it standing would have shipped the leak while the record read as though
someone else owned the fix.

### How it was discharged

The obligation is discharged **in this branch**, in remediation cycle 1, by two changes:

1. **Production, one line.** `UnwireIntentEvents()` in
   `QuickFiler/Controllers/QfcItemController.EventWiring.cs` gains the single detachment
   `_itemViewer.PicturesChanged -= this.CbxPictures_CheckedChanged;`, placed immediately after the
   existing `AttachmentsChanged` detachment so its position mirrors the 17th wire in
   `WireIntentEvents()`. Live detachments in the member now measure **17**, matching the 17 live
   subscriptions. The member's existing `_itemViewer is null` guard covers the null-viewer teardown
   path, and a `-=` against a handler that was never attached is a no-op, so the line is safe on every
   teardown path.
2. **Regression test, one test.** `UnwireIntentEvents_DetachesPicturesChanged` in
   `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.Part2.cs` — this feature's own
   continuation file — wires, unwires, and asserts
   `VerifyRemove(v => v.PicturesChanged -= It.IsAny<EventHandler>(), Times.Once())`. It was written
   and run **before** the production change and failed with
   `Moq.MockException: Expected invocation on the mock once, but was 0 times`, with Moq's invocation
   ledger showing 17 adds against 16 removes and no `remove_PicturesChanged`; it passes after.

Evidence: `FEATURE/evidence/regression-testing/rem1-p1-t3-red-rc1.2026-08-28T03-48.md` (fail-before)
and `rem1-p2-t3-green-rc1.2026-08-28T03-51.md` (pass-after).

### No follow-up issue against 484 is required for this detachment

The paragraph above under **Upstream landing state** anticipated raising the 17th detachment "as a
follow-up against 484's file rather than absorbed before merge". That follow-up is **no longer
required**: the detachment is supplied here, in the same branch that added the 17th wire, so wires and
detachments leave this branch balanced at 17 and 17. No issue should be opened against 484 for it, and
this addendum exists so that the record above is not read as a live obligation at fan-in.

### The 484-owned test name is deliberately left unrenamed

`UnwireIntentEvents_DetachesAllSixteenIntentSubscriptions`
(`QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs:377`) is **not renamed and not
edited**. Its assertion set remains true — it does detach all sixteen of those events — and it passes
unmodified against the 17-detachment production code, because it pins sixteen individual
`VerifyRemove(..., Times.Once())` calls through a local `Off` helper and pins no total: there is no
`VerifyNoOtherCalls` anywhere in that file and no aggregate count assertion. Its "Sixteen" is now a
slightly stale name for a still-true assertion set. Renaming a merged sibling's stable test node ID
would be churn with no behavioural gain, so the staleness is recorded here in prose instead. The file
is also at 499 of the 500-line ceiling and is untouched by this remediation.

Output Summary (addendum): The machine-checkable field recorded near the top of this addendum marks
the obligation as discharged in this branch. Feature review finding RC-1 found the 17-versus-16
subscription leak shipped on this branch; because `Upstream484Landed: true`, the
obligation recorded above was never transferable to 484's in-flight work, so it has been discharged in
this branch by one detachment line in `UnwireIntentEvents()` plus the RED-first regression test
`UnwireIntentEvents_DetachesPicturesChanged`. No follow-up issue against 484 is required for this
detachment. The 484-owned test name `UnwireIntentEvents_DetachesAllSixteenIntentSubscriptions` is
deliberately left unrenamed.
