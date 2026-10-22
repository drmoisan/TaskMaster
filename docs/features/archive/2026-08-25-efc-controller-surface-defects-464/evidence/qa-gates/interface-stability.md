# [P9-T8] Interface stability

Timestamp: 2026-08-28T01-54
Task: [P9-T8]
Command 1: `git diff --name-only <BASE> -- QuickFiler/Interfaces QuickFiler/Viewers/IItemViewer.cs`
Command 2: accessibility audit of every added and removed member declaration in the four owned
production files, via `git diff <BASE> -- <the four files>` filtered to lines beginning `+` or `-`
EXIT_CODE: 0

## Command 1 — no interface file is in the diff

| Base | Output lines |
|---|---|
| `38f097898639b054428188c9c5e266e54972c259` (evaluated) | **0** |
| `002335989830ba9f3ad802858ef0b794f6281750` (`BASELINE_SHA`, as written) | **0** |

Zero under **both** bases, so this gate is unaffected by the base drift recorded in
`changed-file-set.md` and is satisfied exactly as written. No path under `QuickFiler/Interfaces/` and not
`QuickFiler/Viewers/IItemViewer.cs` appears in the diff.

## Every added member is `internal`, `private` or `const`

Complete inventory of the members this feature added, read from the added lines of the production diff:

| Member | File | Accessibility |
|---|---|---|
| `BoundaryErrorSink` | `EfcFormController.cs` | `internal` |
| `ButtonCancelClickAsync` | `EfcFormController.cs` | `internal` |
| `ButtonOkClickAsync` | `EfcFormController.cs` | `internal` |
| `ButtonRefreshClickAsync` | `EfcFormController.cs` | `internal` |
| `ButtonCreateClickAsync` | `EfcFormController.cs` | `internal` |
| `ButtonDeleteClickAsync` | `EfcFormController.cs` | `internal` |
| `TrashRowText` | `EfcFormController.cs` | `internal const` |
| `WithTrashRow` | `EfcFormController.cs` | `internal static` |
| `ApplyDeleteGesture` | `EfcFormController.cs` | `internal` |
| `MatchesForSearchText` | `EfcFormController.cs` | `internal static` |
| `BindSourceFolderRows` | `EfcFormController.cs` | **`private`** |
| `IsBannerRow` | `EfcFormController.cs` | `internal static` |
| `IsSelectableFolder` | `EfcFormController.cs` | `internal static` |
| `IncognitoArgument` | `EfcItemController.cs` | `internal const` |
| `ThrowInitializationFailure` | `EfcItemController.cs` | `internal static` |
| `ClaimsAltChord` | `EfcViewer.cs` | `internal static` |

**No `public` member was added.** The only added lines in the four production files that contain the
token `public` are these five:

```
+        public async void ButtonCancel_Click(object sender, EventArgs e) =>
+        public async void ButtonOK_Click(object sender, EventArgs e) => await ButtonOkClickAsync();
+        public async void ButtonRefresh_Click(object sender, EventArgs e) =>
+        public async void ButtonCreate_Click(object sender, EventArgs e) =>
+        public async void ButtonDelete_Click(object sender, EventArgs e) =>
```

All five are **pre-existing** `public` members. Each is declared at `BASELINE_SHA` with the identical
signature (`EfcFormController.cs:413`, `:429`, `:445`, `:461`, `:521`). They appear as added lines only
because RC3 converted each from a braced body to an expression body delegating to its extracted
`internal async Task` counterpart. Neither the name, the parameter list, nor the accessibility of any of
the five changed. No `protected` member was added to `EfcViewer.cs`.

## No removed member was declared on an interface implemented by its declaring type

The removed member declarations in the four owned production files, and their disposition:

| Removed declaration | Declaring type | Disposition |
|---|---|---|
| `public async void ButtonCancel_Click(...)` | `EfcFormController` | **not removed** — re-declared as an expression body |
| `public async void ButtonOK_Click(...)` | `EfcFormController` | **not removed** — re-declared |
| `public async void ButtonRefresh_Click(...)` | `EfcFormController` | **not removed** — re-declared |
| `public async void ButtonCreate_Click(...)` | `EfcFormController` | **not removed** — re-declared |
| `public async void ButtonDelete_Click(...)` | `EfcFormController` | **not removed** — re-declared |
| `internal void InitializeWebView()` | `EfcItemController` | genuinely removed (RC deletion) |
| `internal void RegisterActions(...)` | `EfcItemController` | genuinely removed (RC4, #459) |
| `public async void ConversationResolverPropertyChanged(...)` | `EfcItemController` | genuinely removed |
| `public void ToggleExpansion()` | `EfcItemController` | genuinely removed (RC11, #466) |
| `public void ToggleExpansion(Enums.ToggleState)` | `EfcItemController` | genuinely removed (RC11, #466) |
| `internal void SetController(EfcFormController)` | `EfcViewer` | genuinely removed |
| `private void EditFiltersMenuItem_Click(...)` | `EfcViewer` | genuinely removed (the unreachable handler) |

The declaring types and the interfaces they implement:

- `EfcItemController` is declared `internal class EfcItemController : IItemControler`
  (`EfcItemController.cs:26`). It implements **exactly one** interface.
- `EfcViewer` is declared `public partial class EfcViewer : Form` (`EfcViewer.cs:21`). It implements
  **no** interface.

`QuickFiler/Interfaces/IItemControler.cs` declares exactly three members and nothing else:

```csharp
int CounterEnter { get; set; }
int CounterComboRight { get; set; }
public Dictionary<string, System.Action> RightKeyActions { get; }
```

None of the six genuinely removed members is one of those three, so **no removed member was declared on
any interface that its declaring type implements**.

Two name-level coincidences are recorded rather than glossed, because a reader searching by name would
otherwise find them and reach the opposite conclusion:

1. `QuickFiler/Interfaces/IQfcItemController.cs:43` declares `void ToggleExpansion();`. That interface
   is **not** implemented by `EfcItemController`, which implements only `IItemControler`. The RC11
   deletion therefore satisfies no interface obligation and breaks none. The QFC-side implementer is
   `QfcItemController`, which this feature does not touch.
2. `QuickFiler/Interfaces/IQfcFormViewer.cs:20` declares `void SetController(IFilerFormController controller);`.
   The removed `EfcViewer` member had signature `internal void SetController(EfcFormController controller)` —
   a different parameter type and `internal` rather than `public`, so it could not have been an
   implementation of that declaration in any case; and `EfcViewer` does not implement `IQfcFormViewer`.

The compiler corroborates all of this independently: the analyzer and nullable builds recorded in
`phase8-boundary-toolchain.md` both exit 0 with 0 errors. An unimplemented interface member would have
produced `CS0535` and failed both.

## Two 484 fixed points

The upstream-constraints briefing names `ToggleNavigation(bool)` and `Task MoveMailAsync()` as
declarations on `IQfcItemController` that must not lose their implementations.

- `ToggleNavigation(bool)` is present in the delivered `EfcItemController.cs:887`, and per 484's
  Downstream note 3 it was deliberately **not** aligned with 484's own fix.
- `MoveMailAsync` has **zero** occurrences in `EfcItemController.cs` at the evaluated base
  `38f09789` and zero in the delivered file. The briefing's statement that `EfcItemController`
  implements it is not correct on this base. This feature removed nothing: the member was never there.
  The discrepancy is recorded here rather than presented as a preserved invariant.

Output Summary: PASS. `git diff --name-only -- QuickFiler/Interfaces QuickFiler/Viewers/IItemViewer.cs`
returns zero lines under both bases. All sixteen members this feature added are `internal`, `private` or
`const`; no `public` member was added — the five `public` added lines are pre-existing handlers
re-declared as expression bodies with identical signatures. None of the six genuinely removed members is
declared on `IItemControler`, the only interface implemented by `EfcItemController`, and `EfcViewer`
implements no interface; the two name-level interface coincidences (`ToggleExpansion` on
`IQfcItemController`, `SetController` on `IQfcFormViewer`) are recorded and neither applies to the
declaring type. `ToggleNavigation(bool)` survives at `EfcItemController.cs:887`; `MoveMailAsync` does not
exist in that type on this base and was not removed by this feature.
