# P9-T10 — `IItemViewer` surface delta

Timestamp: 2026-08-28T01-47
Command: git show <BASELINE_SHA>:QuickFiler/Viewers/IItemViewer.cs | (strip comment lines) | (keep declaration lines) > base; (same over the working tree) > cur; diff base cur
EXIT_CODE: 0

`BASELINE_SHA` is `cecd78130a489fcfdc2ddac7970f344256f4a75a`.

## Method

Both the baseline blob and the working-tree file were reduced to their declaration lines by dropping
every `//` and `///` comment line and keeping every remaining indented line ending in `;`. That
projection is exactly the member surface: it discards documentation and section commentary, so a
documentation-only edit produces **no** row in the resulting diff, and any signature change produces
one. The two projections were then compared with `diff`.

MemberCountBefore: **79**
MemberCountAfter: **78**

## Surface diff (verbatim)

```
21d20
< TaskScheduler UiScheduler { get; }
34c33
< void FocusSubject();
---
> bool FocusSubject();
51c50
< void SetFolderItems(string[] items);
---
> void AddFolderItems(string[] items);
```

Three hunks. Nothing else.

## Before/after member table

| # | Member at `BASELINE_SHA` | Member now | Change | Owning task |
|---|---|---|---|---|
| 1 | `TaskScheduler UiScheduler { get; }` | *(absent)* | **Removed** | P6-T2 (#489 `UiScheduler` carve-out) |
| 2 | `void FocusSubject();` | `bool FocusSubject();` | **Return type `void` to `bool`** | P8-T1 (#490 D3) |
| 3 | `void SetFolderItems(string[] items);` | `void AddFolderItems(string[] items);` | **Renamed** `SetFolderItems` to `AddFolderItems` | P8-T5 (#490 D4) |
| 4 | `void SetConversationItems(System.Collections.IList items);` | `void SetConversationItems(System.Collections.IList items);` | **XML documentation added, signature identical** | P9-T4 (#489 D3) |
| 5 | `void SortConversationByDate(SortOrder order);` | `void SortConversationByDate(SortOrder order);` | **XML documentation added, signature identical** | P9-T4 (#489 D3) |
| 6 | `void FocusSearch();` | `void FocusSearch();` | **XML documentation added, signature identical** | P9-T3 (#490 D2) |
| 7 | `bool FocusSubject();` (post-P8-T1) | `bool FocusSubject();` | **XML documentation added, no further signature change** | P9-T3 (#490 D2) |
| — | All remaining **76** members | Identical | **Unchanged** | — |

Rows 4 through 7 produce no line in the surface diff above, which is the proof that the documentation
carries no signature change: had any of the four declarations been altered, the projection would have
reported it as a fourth, fifth, sixth or seventh hunk. `FocusSubject` appears in both the changed and
the documented group because P8-T1 changed its return type and P9-T3 subsequently documented it; the
documentation added no further signature change, which is the exact condition P9-T3's acceptance
states.

## Documentation counts

| Measure | At `BASELINE_SHA` | Now |
|---|---|---|
| `<summary>` elements in `IItemViewer.cs` | 0 | **4** |
| Documented members | 0 | `FocusSubject`, `FocusSearch`, `SetConversationItems`, `SortConversationByDate` |

`git diff --numstat <BASELINE_SHA> -- QuickFiler/Viewers/IItemViewer.cs` reports `33` added and `4`
deleted. The four deletions are the `UiScheduler` declaration plus the three lines replaced in place
(`FocusSubject`, `SetFolderItems`, and the comment line naming `SetFolderItems`); the additions are
the three replacements, the 27 documentation and blank lines from P9-T3 and P9-T4, and the CSharpier
separator lines the formatter required.

## Acceptance

| P9-T10 required table entry | Recorded |
|---|---|
| `UiScheduler` removed | Row 1 |
| `FocusSubject` changed from `void` to `bool` | Row 2 |
| `SetFolderItems` renamed to `AddFolderItems` | Row 3 |
| XML documentation added to `SetConversationItems`, `SortConversationByDate`, `FocusSearch` and `FocusSubject` with no signature change | Rows 4 through 7, corroborated by their absence from the surface diff |
| Every other member unchanged | Final row: 76 members identical |

Output Summary: The `IItemViewer` surface delta is **exactly the three changes this feature makes and
nothing more**. A comment-stripped declaration-line projection of the baseline blob against the
working tree yields three hunks: `TaskScheduler UiScheduler { get; }` removed, `void FocusSubject();`
becoming `bool FocusSubject();`, and `void SetFolderItems(string[] items);` becoming
`void AddFolderItems(string[] items);`. The member count falls from 79 to 78. XML documentation was
added to four members — `SetConversationItems`, `SortConversationByDate`, `FocusSearch` and
`FocusSubject` — and produces no hunk in that projection, proving no signature changed. The remaining
76 members are identical.
