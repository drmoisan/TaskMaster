# P0-T7 — Pre-Change Exemption Census (Baseline)

Issue: #230
Task: [P0-T7]

- Timestamp: 2026-08-07T21-50
- Command: `Select-String -Pattern 'ExcludeFromCodeCoverage' -Path QuickFiler/Controllers/QfcItemController.*.cs`
- EXIT_CODE: 0 (derived from `$?` = `True` per D14; `Select-String` does not set
  `$LASTEXITCODE`)
- Output Summary: **19 `[ExcludeFromCodeCoverage]` sites** across 6 controller
  partial files, matching the plan's expected distribution exactly:
  Conversation.cs 1, EventWiring.cs 1, EventHandlers.cs 5, Initialization.cs 7,
  Navigation.cs 2, ViewerSetup.cs 3.

## Full site enumeration (file, line, member)

| # | File | Line | Member | In #230's 9-member scope? |
|---|---|---|---|---|
| 1 | `QfcItemController.Conversation.cs` | 79 | `DoLoadConversationResolverCoreAsync` | No |
| 2 | `QfcItemController.EventHandlers.cs` | 60 | `BtnPopOut_Click` | No |
| 3 | `QfcItemController.EventHandlers.cs` | 83 | `BtnReply_Click` | No |
| 4 | `QfcItemController.EventHandlers.cs` | 97 | `BtnReplyAll_Click` | No |
| 5 | `QfcItemController.EventHandlers.cs` | 111 | `BtnForward_Click` | No |
| 6 | `QfcItemController.EventHandlers.cs` | 125 | `TxtboxBody_DoubleClick` | No |
| 7 | `QfcItemController.EventWiring.cs` | 99 | `WebView2Control_CoreWebView2InitializationCompleted` | No |
| 8 | `QfcItemController.Initialization.cs` | 138 | `Initialize` (private 9-arg) | **Yes — target** |
| 9 | `QfcItemController.Initialization.cs` | 168 | `Initialize(bool async)` | **Yes — target** |
| 10 | `QfcItemController.Initialization.cs` | 200 | `InitializeAsync()` | **Yes — target** |
| 11 | `QfcItemController.Initialization.cs` | 260 | `InitializeGraphicsAsync()` | **Yes — target** |
| 12 | `QfcItemController.Initialization.cs` | 291 | `InitializeSequentialAsync()` | **Yes — target** |
| 13 | `QfcItemController.Initialization.cs` | 403 | `CreateAsync(...)` (static factory) | **Yes — target** |
| 14 | `QfcItemController.Initialization.cs` | 436 | `CreateSequentialAsync(...)` (static factory) | **Yes — target** |
| 15 | `QfcItemController.Navigation.cs` | 173 | `ToggleExpansion(Enums.ToggleState)` | No |
| 16 | `QfcItemController.Navigation.cs` | 191 | `ToggleExpansionAsync(Enums.ToggleState)` | No |
| 17 | `QfcItemController.ViewerSetup.cs` | 38 | `InitializeWebViewAsync()` | Yes (9th member) — **retained exempt**, D3 |
| 18 | `QfcItemController.ViewerSetup.cs` | 132 | `EnsureBreadcrumbPipeline()` | No — post-ratification, see below |
| 19 | `QfcItemController.ViewerSetup.cs` | 253 | `ResolveControlGroupsAsync(ItemViewer)` | **Yes — target** |

Raw `Select-String` output (file:line):

```
QfcItemController.Conversation.cs:79
QfcItemController.EventHandlers.cs:60
QfcItemController.EventHandlers.cs:83
QfcItemController.EventHandlers.cs:97
QfcItemController.EventHandlers.cs:111
QfcItemController.EventHandlers.cs:125
QfcItemController.EventWiring.cs:99
QfcItemController.Initialization.cs:138
QfcItemController.Initialization.cs:168
QfcItemController.Initialization.cs:200
QfcItemController.Initialization.cs:260
QfcItemController.Initialization.cs:291
QfcItemController.Initialization.cs:403
QfcItemController.Initialization.cs:436
QfcItemController.Navigation.cs:173
QfcItemController.Navigation.cs:191
QfcItemController.ViewerSetup.cs:38
QfcItemController.ViewerSetup.cs:132
QfcItemController.ViewerSetup.cs:253
```

All 19 attribute occurrences use the fully-qualified form
`[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]`.

## Cross-reference to the ratified boundary

The maintainer-ratified exemption boundary of record is
`docs/features/archive/2026-06-29-qfc-item-controller-testability-227/evidence/other/exemption-boundary.2026-07-02T17-00.md`
(ratified in the sibling `maintainer-decision.2026-07-02.md`). That artifact
records the boundary as 19 members reduced from 103 by issue #227, and
pre-authorizes issue #230 as the follow-up for its "category 1" bucket — the 9
members blocked by the missing WinForms message-pump test seam.

Sites 8-14, 17, and 19 in the table above are exactly that category-1 bucket
(9 members). Sites 1-7, 15-16 are other ratified categories and are untouched by
this feature.

## `EnsureBreadcrumbPipeline` — post-ratification site (out of scope)

`EnsureBreadcrumbPipeline` (`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:132`,
site 18) was added by **issue #351**, after the 2026-07-02 ratification. It is
therefore not one of the 18 controller-file members in the ratified boundary —
the ratified 19th member is `WebView2CoreInitializer`, which lives in a
non-controller file and does not appear in this grep. This grep-based census
counts 19 controller-file sites, which coincidentally equals the ratified count
but is a different set by one member.

`EnsureBreadcrumbPipeline` is **outside #230's 9-member scope** (spec.md
Non-Goal 2). It is documented here and in the post-change census (P7-T1), and is
not changed by this feature.

## Target outcome (D3)

8 of the 9 category-1 members are de-exempted by this feature; `InitializeWebViewAsync`
(site 17) is retained with an updated justification because its residual barrier is
the CoreWebView2/WebView2 runtime, an external process barred by the unit-test
policy. Expected post-change census: **19 -> 11 sites**.
