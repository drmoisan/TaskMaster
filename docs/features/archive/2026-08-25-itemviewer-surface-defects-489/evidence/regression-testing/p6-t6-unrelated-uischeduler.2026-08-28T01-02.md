# P6-T6 — The unrelated `UiScheduler` members on other types were not touched

Timestamp: 2026-08-28T01-02
Command: git diff --name-only <BASELINE_SHA> -- QuickFiler/ QuickFiler.Test/ UtilitiesCS/ ; git grep -n -E "_uiScheduler|UiScheduler" -- QuickFiler/Viewers/ItemViewerExpanded.cs
EXIT_CODE: 0
ExpectedExitCode: 0

## Acceptance clause 1 — none of the five named files appears in the diff

The complete set of files changed against `<BASELINE_SHA>` under `QuickFiler/`, `QuickFiler.Test/`
and `UtilitiesCS/` is 14 paths:

```
QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.Part2.cs
QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs
QuickFiler.Test/Controllers/QfcItemController.ThemeMarshallingTests.cs
QuickFiler.Test/QuickFiler.Test.csproj
QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs
QuickFiler.Test/Viewers/ToolStripMenuItemCbTests.cs
QuickFiler/Controllers/QfcItemController.EventHandlers.cs
QuickFiler/Controllers/QfcItemController.EventWiring.cs
QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs
QuickFiler/Viewers/IItemViewer.cs
QuickFiler/Viewers/ItemViewer.Designer.cs
QuickFiler/Viewers/ItemViewer.cs
QuickFiler/Viewers/ItemViewerExpanded.Designer.cs
QuickFiler/Viewers/ItemViewerExpanded.cs
```

A filter of that list for the five forbidden paths returns a count of **0**:

- `QuickFiler/Viewers/EfcViewer.cs` — absent
- `QuickFiler/Viewers/QfcItemViewer.cs` — absent
- `QuickFiler/Viewers/QfcFormViewer.cs` — absent
- `QuickFiler/Controllers/QfcHomeController.cs` — absent
- `QuickFiler/Interfaces/IQfcFormViewer.cs` — absent

Each of those types declares its own `UiScheduler` member independently of `IItemViewer`, and several
belong to live sibling children. `QuickFiler/Controllers/QfcItemController.Navigation.cs`, which
P10-T5 separately asserts is absent from the diff and which child 444 owns read-only, is likewise not
in the list.

## Acceptance clause 2 — `ItemViewerExpanded`'s own `UiScheduler` survives

MatchCount: 4
RequiredMinimum: 3

```
QuickFiler/Viewers/ItemViewerExpanded.cs:22:            _uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
QuickFiler/Viewers/ItemViewerExpanded.cs:59:        private TaskScheduler _uiScheduler;
QuickFiler/Viewers/ItemViewerExpanded.cs:60:        public TaskScheduler UiScheduler
QuickFiler/Viewers/ItemViewerExpanded.cs:62:            get => _uiScheduler;
```

Four matches, meeting the "at least three" condition: the constructor assignment, the backing field,
the property declaration and its getter all survive. `ItemViewerExpanded` derives from `UserControl`
and does not implement `IItemViewer`, so the P6-T2 interface deletion imposes no obligation on it and
its member is genuinely unrelated.

**No line range is asserted.** The field and property stood at `:63-67` at `BASELINE_SHA` but stand
at `:59-63` now, because P2-T1 deleted the four constructor calls at `:24-27` and P4-T3 deleted the
`L0v2h2_WebView2_ParentChanged` member. A `:63-67` range assertion would inspect the wrong lines and
could not pass; the gate is the match count and the member identity, not position.

## Scope note on the two files that do appear

`QuickFiler/Viewers/ItemViewerExpanded.cs` and `ItemViewerExpanded.Designer.cs` are in the diff, but
for the Phase 2 and Phase 4 work on issues #486 and #487 — the move-option menu handlers and the dead
`ParentChanged` handler — not for anything `UiScheduler`-related. P4-T8 records that the cumulative
change to `ItemViewerExpanded.cs` is `0` added and `27` deleted, so nothing was written into it at
any point.

Output Summary: None of the five files carrying an unrelated `UiScheduler` member — `EfcViewer.cs`,
`QfcItemViewer.cs`, `QfcFormViewer.cs`, `QfcHomeController.cs` and `IQfcFormViewer.cs` — appears in
the 14-path diff against `<BASELINE_SHA>`, and neither does the 444-owned
`QfcItemController.Navigation.cs`. `ItemViewerExpanded.cs` retains **4** `UiScheduler` matches
against a required minimum of 3, covering the field, its assignment, the property and its getter.
No line range is asserted, because P2-T1 and P4-T3 shifted the member from `:63-67` to `:59-63`.
