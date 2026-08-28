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
