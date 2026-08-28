# Phase 3 — #461 source structure after the removal

Timestamp: 2026-08-28T00-24
Task: [P3-T5]
Command: two fixed-string searches with `grep -cF <token> QuickFiler/Controllers/EfcItemController.cs` against the delivered file, and the same two against `git show 002335989830ba9f3ad802858ef0b794f6281750:QuickFiler/Controllers/EfcItemController.cs` for the pre-change counts; plus `grep -nF '_dataModel.ConversationResolver.UpdateUI = SetTopicThread;' QuickFiler/Controllers/EfcItemController.cs`
EXIT_CODE: 0

Both searches are **fixed-string** (`-F`), so no character in either token is treated as a regular
expression metacharacter, and each token is short enough to sit on a single source line in both the
pre-change and the delivered file.

## Search 1 — the `nameof` guard literal

Token, quoted verbatim:

```
nameof(_dataModel.ConversationResolver.ConversationInfo.Expanded)
```

| File state | Command | Match count |
|---|---|---|
| `BASELINE_SHA` | `grep -cF '<token>' <baseline EfcItemController.cs>` | **1** |
| Delivered | `grep -cF '<token>' QuickFiler/Controllers/EfcItemController.cs` | **0** |

The single pre-change occurrence was the guard expression inside the deleted handler. It resolved at
compile time to the literal `"Expanded"`, a property name `ConversationResolver` never raises.

## Search 2 — the subscription

Token, quoted verbatim:

```
ConversationResolver.PropertyChanged
```

| File state | Command | Match count |
|---|---|---|
| `BASELINE_SHA` | `grep -cF '<token>' <baseline EfcItemController.cs>` | **1** |
| Delivered | `grep -cF '<token>' QuickFiler/Controllers/EfcItemController.cs` | **0** |

The single pre-change occurrence was the `+=` attachment inside `WireEventHandlers`, together with the
`if` on the resolver's nullity that guarded it. Both are gone.

## The guard literal is not retargeted

Zero matches for the `nameof` token, and no substitute literal was introduced: the delivered file contains
no `"ConversationInfo"`, `"ConversationItems"`, `"Df"` or `"UpdateUI"` string used as a `PropertyChanged`
guard, because there is no longer any `PropertyChanged` handler on the resolver to guard. Retargeting was
deliberately rejected — it would run a second `SetObjects` and `Sort` on every background conversation
load in addition to the existing `UpdateUI` dispatch, and would re-enter the lazy `ConversationInfo`
getter that the publisher documents itself as avoiding.

## The surviving live route

The delivered source line that installs the live route, at line **270**:

```csharp
            _dataModel.ConversationResolver.UpdateUI = SetTopicThread;
```

It is inside `PopulateConversation`, exactly as before the change, and is pinned by the named test
`PopulateConversation_AssignsSetTopicThreadToConversationResolverUpdateUi`, green in both `[P3-T2]` and
`[P3-T4]`.

The three sibling attachments in `WireEventHandlers` that `[P3-T3]` was required not to disturb all
survive: `CoreWebView2InitializationCompleted`, `TopicThread.ItemSelectionChanged`,
`_globals.Ol.PropertyChanged += DarkMode_Changed`, and the `Buttons.ForEach` mouse-handler block.

Output Summary: Both fixed-string searches return 0 matches in the delivered file against a pre-change
count of 1 each. The guard literal is removed rather than retargeted, and the surviving live route
`_dataModel.ConversationResolver.UpdateUI = SetTopicThread;` is present at `EfcItemController.cs:270`.
