# Phase 0 — baseline structural inventory of every member this plan deletes

Timestamp: 2026-08-27T23-39
Task: [P0-T16]
Command: `grep -n <identifier> <path>` per row, plus `sed -n` window reads to confirm declaration extents, and `ls -l` for the three file paths, all at `BASELINE_SHA` = `002335989830ba9f3ad802858ef0b794f6281750`
EXIT_CODE: 0

Every item the plan deletes is **present** at `BASELINE_SHA`, and every cited location is **correct as
written**. No deviation was found in this inventory.

## `QuickFiler/Controllers/EfcItemController.cs`

| Item | Plan's cited location | Observed | Status |
|---|---|---|---|
| `InitializeWebView()` | `:174` | `:174` — `internal void InitializeWebView()` | present, correct |
| `RegisterActions` | `:680` | `:680` — `internal void RegisterActions(` | present, correct |
| seven-parameter `EfcItemController` constructor | `:44-57` | `:44` — `public EfcItemController(` with parameters `globals, homeController, parent, itemViewer, dataModel, async, token`, chaining `: this(globals, homeController, parent, itemViewer, token)` and closing at `:57` | present, correct |
| field `_selectorsCtrls` | `:381` | `:381` — `private List<Control> _selectorsCtrls = null;` | present, correct |
| `_selectorsCtrls` passed to `SetupThemes` | `:97`, `:144` | `:97`, `:144` | present, correct |
| `ToggleExpansion()` | `:838` | `:838` — `public void ToggleExpansion()` | present, correct |
| `ToggleExpansion(Enums.ToggleState)` | `:862` | `:862` — `public void ToggleExpansion(Enums.ToggleState desiredState)` | present, correct |
| `ConversationResolverPropertyChanged` | `:741` | `:741` — `public async void ConversationResolverPropertyChanged(` | present, correct |
| its subscription block | `:666-669` | `:666` `if (_dataModel.ConversationResolver is not null)`, `:667` `_dataModel.ConversationResolver.PropertyChanged += new PropertyChangedEventHandler(`, `:668` `ConversationResolverPropertyChanged`, `:669` `);` | present, correct |

Members that must **survive** and were confirmed present alongside the deletion targets:
`ToggleExpansionAsync()` at `:850`, `ToggleExpansionAsync(Enums.ToggleState)` at `:907`,
`ToggleExpansionOff` at `:931`, `ToggleExpansionOn` at `:944`, and the `'E'` async registration
delegating to `KbdExecuteAsync(this.ToggleExpansionAsync)` at `:704`.

Sibling attachments in `WireEventHandlers` that `[P3-T3]` must **not** delete were confirmed present:
`CoreWebView2InitializationCompleted` at `:664-665`, `TopicThread.ItemSelectionChanged` at `:670-671`,
`_globals.Ol.PropertyChanged += DarkMode_Changed` at `:672`, and the `Buttons.ForEach` mouse-handler
block at `:673-677`.

## `QuickFiler/Viewers/EfcViewer.cs`

| Item | Plan's cited location | Observed | Status |
|---|---|---|---|
| field `_formController` | `:48` | `:48` — `private EfcFormController _formController;` | present, correct |
| `SetController` | `:50-53` | `:50` — `internal void SetController(EfcFormController controller)`, assigning at `:52` | present, correct |
| `EditFiltersMenuItem_Click` | `:157-160` | `:157` — `private void EditFiltersMenuItem_Click(object sender, EventArgs e)`, body delegating at `:159` to `_formController.EditFiltersMenuItem_Click(sender, e)` | present, correct |

The `:159` body confirms RC11-A's latent trap: the viewer-side handler dereferences `_formController`,
which is permanently null because `SetController` has no caller.

## Files deleted from the tree

| Path | Present | Size (bytes) |
|---|---|---|
| `QuickFiler/Viewers/EfcViewer3.cs` | yes | 2474 |
| `QuickFiler/Viewers/EfcViewer3.Designer.cs` | yes | 32101 |
| `QuickFiler/Viewers/EfcViewer3.resx` | yes | 5817 |

`grep -c 'EfcViewer3' QuickFiler/QuickFiler.csproj` returns **0**. The three files carry no
`Compile Include`, no `EmbeddedResource`, and no `DependentUpon` entry in that project file, so deleting
them requires no `QuickFiler.csproj` edit. This is the finding `[P2-T4]` relies on and the reason the
deletion has no contention with feature #501.

## Baseline `throw;` count in `QuickFiler/Controllers/EfcFormController.cs`

Fixed-string search for `throw;`:

```
425:                throw;
441:                throw;
457:                throw;
517:                throw;
530:                throw;
```

**Count: 5**, at lines 425, 441, 457, 517 and 530 — exactly the five lines the plan expects. This is the
baseline for the `#464` criterion "The token `throw;` does not appear inside any `async void` member of
`EfcFormController.cs`; the five occurrences previously at `:425`, `:441`, `:457`, `:517` and `:530` are
absent."

Output Summary: All fifteen inventoried items are present at BASELINE_SHA and every cited location is
correct as written; no deviation found. The three EfcViewer3 files exist and carry zero references in
QuickFiler.csproj. The baseline `throw;` count in EfcFormController.cs is 5, at lines 425, 441, 457, 517
and 530.
