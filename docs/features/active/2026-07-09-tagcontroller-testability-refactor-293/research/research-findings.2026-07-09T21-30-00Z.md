# Research Findings — TagController Testability Refactor (#293)

- Feature: `2026-07-09-tagcontroller-testability-refactor-293` (child of epic winforms-testability-refactor #295, wave 0)
- Research date: 2026-07-09T21:30:00Z
- Scope: research-only design. No production or test code was modified.
- Authoritative pattern source: `docs/features/epics/winforms-testability-refactor/epic.md` "Shared Design Pattern" and the established QuickFiler precedent `QuickFiler/Interfaces/IQfcFormViewer.cs` (issue #227).

## 0. Repository facts established by reading (not assumed)

- `Tags/TagController.cs` is 877 lines; single public class `Tags.TagController`.
- `Tags/Tags.csproj` compiles exactly: `Helper Classes/CheckBoxController.cs`, `Helper Classes/PrefixItem.cs`, `Properties/AssemblyInfo.cs`, `Resources.Designer.cs`, `TagController.cs`, `TagLauncher.cs`, `TagViewer.cs`, `TagViewer.Designer.cs`.
- Two files are ORPHANS (present on disk, NOT in the csproj `<Compile>` set, therefore not in `Tags.dll` and irrelevant to build/coverage):
  - `Tags/CheckBoxController.cs` (root, 167 lines). Contains a null-deref constructor (`ctrlCB.Click += ...` where the `ctrlCB` getter returns a null `_ctrlCB`) and a call to a non-existent `_parent.OK_Action()`. It does not compile against the current `TagController` and is dead.
  - `Tags/AutoAssignInterface.cs` (a second `Tags.IAutoAssign` declaration lacking `AutoFindAsync`).
- The COMPILED `IAutoAssign` is `UtilitiesCS/Interfaces/IToDo/IAutoAssign.cs` — physically under the UtilitiesCS project but declared in `namespace Tags`, and it DOES include `Task<IList<string>> AutoFindAsync(object)` which `TagController` uses. `IAutoAssign` and `IPrefix` are already interfaces (Moq-friendly).
- The COMPILED `CheckBoxController` is `Helper Classes/CheckBoxController.cs` (167 lines), already annotated `[ExcludeFromCodeCoverage]`. Its constructor is empty (no NRE). It holds a `TagController _parent` and calls back into `_parent.ToggleChoice`, `.FocusCheckbox`, `.Select_Ctrl_By_Offset`, `.Select_Last_Control`, `.Select_First_Control`, `.Select_PageDown`, `.Select_PageUp`, `.ButtonOk_Action`.
- `TagLauncher.cs` (262 lines) is already `[ExcludeFromCodeCoverage]`.
- Target framework `v4.8.1`, non-SDK csproj, `packages.config`, `LangVersion=latest`. Test project `Tags.Test` already references Moq 4.20, FluentAssertions 8.9, MSTest 4.2. Precedent for headless COM mocking exists (`new Mock<MailItem>()`).
- `IForm` (`UtilitiesCS/Interfaces/IWinForm/IForm.cs`) provides `Close()`, `KeyPreview`, `ShowDialog()`, `ShowDialog(IWin32Window)` but does NOT provide `Text`, a `KeyDown` event, or `Controls`. Those must be added to `ITagViewer` explicitly if the controller needs them.
- Precedent confirmed: `IQfcFormViewer : IForm` uses a HYBRID surface — intent-named events/properties for buttons and inputs (`OkClicked`, `SkipButtonText`, `ItemsPerLoadValue`) plus a small number of raw control abstractions (`Panel`, `TableLayoutPanel`) and intent methods (`SwapItemTableLayout`, `CaptureTlpCellStates`) where a snapshot is unavoidable. A WinForms `Form`'s built-in members satisfy the `IForm` base implicitly. This is the model for `ITagViewer`.

## A. Member-level inventory: what `TagController` consumes on `_viewer` and controls

`_viewer` is typed `TagViewer`. All accesses below drive the interface/seam design.

| Member accessed | Control type | Usage | Call sites (TagController.cs line) |
|---|---|---|---|
| `SetController(this)` | TagViewer | method | ctors 28, 67 |
| `HideArchive.Checked` | CheckBox | get | 35, 391 |
| `HideArchive.CheckedChanged` | CheckBox | event subscribe | 337 |
| `ButtonAutoAssign.Visible` | Button | get/set | 127, 132, 300, 301 |
| `ButtonAutoAssign.Enabled` | Button | set | 128, 133 |
| `ButtonAutoAssign.Click` | Button | event subscribe | 331 |
| `ButtonOk.Click` | Button | event subscribe | 328 |
| `ButtonCancel.Click` | Button | event subscribe | 329 |
| `ButtonNew.Visible` | Button | get/set | 294, 295 |
| `ButtonNew.Click` | Button | event subscribe | 330 |
| `SearchText.Text` | TextBox | get (240) / set (304) | 240, 304 |
| `SearchText.Focus()` | TextBox | method | 85 |
| `SearchText.Select()` | TextBox | method | 752, 855 |
| `SearchText.SelectionStart` | TextBox | get | 581, 598 |
| `SearchText.TextChanged` | TextBox | event subscribe | 332 |
| `SearchText.KeyDown` | TextBox | event subscribe | 333 |
| `SearchText.KeyUp` | TextBox | event subscribe | 336 |
| `TemplateCheckBox` | CheckBox | passed to `ControlPosition.CreateTemplate`; removed from panel | 192, 193 |
| `L1v2L2_OptionsPanel.Controls.Add/Remove` | Panel | method | 193, 635, 688, 696 |
| `L1v2L2_OptionsPanel.KeyDown` | Panel | event subscribe | 323 |
| `L1v2L2_OptionsPanel.PreviewKeyDown` | Panel | event subscribe | 326 |
| `L1v2L2_OptionsPanel.VerticalScroll.Maximum` | Panel | get | 780, 814 |
| `L1v2L2_OptionsPanel.Height` | Panel | get | 781, 785, 815, 835 |
| `L1v2L2_OptionsPanel.AutoScrollPosition` | Panel | get/set | 801, 803, 834, 837 |
| `Text` (form caption) | Form | set | 309 |
| `Close()` | Form | method | 349, 382 |
| `KeyDown` (form) | Form | event subscribe | 340 |

Dynamic child-control accesses (on `CheckBox` instances the controller itself creates in `LoadControls`): `.Name`, `.Tag`, `.Text`, `.Checked` (property bag, no HWND); `.Focus()` (757, 860 — safe no-op when handle not created); `.Handle` + `System.Drawing.Graphics.FromHwnd(...)` inside `ControlPaint.DrawFocusRectangle` (759-762, 862-865 — FORCES an HWND, the one hard COM/UI dependency); `.Bottom`, `.Top`, `.Height`, `.ClientRectangle` (property bag, no HWND).

Static / COM / dialog call sites:
- `MessageBox.Show(...)` — `LoadSelections` (154, Yes/No add prompt), `TryGetAutoAssignment` (434, Yes/No), `LoadControls` (639, 661, 676, error popups).
- `InputBox.ShowDialog(...)` (static) — `GetUserInputCategory` (493, 505). `InputBox` already has an internal `DialogInvoker` seam BUT still constructs a real `InputBoxViewer` form before invoking it, so the seam alone does not keep the test headless.
- `ControlPaint.DrawFocusRectangle(Graphics.FromHwnd(cbx.Handle), cbx.ClientRectangle)` — `Select_Ctrl_By_Offset` (759), `Select_Ctrl_By_Position` (862).
- `ControlPosition.CreateTemplate(control)` / `ControlPosition.Set(control, ...)` — verified host-neutral: they read/write only plain `Control` layout properties (`Left/Top/Width/Height/Margin/Padding`); they do NOT touch `Handle`. No HWND dependency.
- `_autoAssigner.AddColorCategory(...)` returns `Microsoft.Office.Interop.Outlook.Category` (COM), used only for `.Name` (478). Mockable via Moq.
- `ResolveMailItem(object)` casts `object` to `Microsoft.Office.Interop.Outlook.MailItem` — pure type-check; testable with a Moq `MailItem` or any object.

## B. `ITagViewer` design (deriving from `IForm`)

Two options were evaluated.

- Option (i) Interface-typed control abstractions: wrap each control in an interface (`IButtonView`, `ITextBoxView`, `IOptionsPanelView`). Fully mockable but requires adapter wrappers for every control and a large new surface; maximum churn.
- Option (ii) Intent-named facade members that hide concrete controls behind purpose-named properties/methods/events.

Recommendation: Option (ii), hybrid — matching the ratified `IQfcFormViewer` precedent. Rationale: it minimizes the mock surface, hides WinForms types from the controller's consumed surface, preserves behavior (the viewer maps intent members 1:1 onto the existing controls), and reuses `IForm`-provided members instead of duplicating them. A raw abstraction is used only where a snapshot/collection is genuinely required.

Proposed `ITagViewer : UtilitiesCS.Interfaces.IWinForm.IForm` members:

Reused from `IForm` (do NOT redeclare): `Close()`, `KeyPreview`, `ShowDialog()`.

Added — command intent events (replace raw Button/TextBox/CheckBox event subscriptions):
- `event EventHandler OkClicked;`
- `event EventHandler CancelClicked;`
- `event EventHandler NewClicked;`
- `event EventHandler AutoAssignClicked;`
- `event EventHandler SearchTextChanged;`
- `event KeyEventHandler SearchKeyDown;`
- `event KeyEventHandler SearchKeyUp;`
- `event EventHandler HideArchiveChanged;`
- `event KeyEventHandler ViewKeyDown;` (maps to the form's `KeyDown`, which `IForm` does not expose)
- `event PreviewKeyDownEventHandler OptionsPreviewKeyDown;`
- `event KeyEventHandler OptionsKeyDown;`

Added — state intent properties:
- `bool HideArchiveChecked { get; }`
- `bool AutoAssignVisible { get; set; }` and `bool AutoAssignEnabled { get; set; }`
- `bool ButtonNewVisible { get; set; }`
- `string SearchTextValue { get; set; }`
- `int SearchSelectionStart { get; }`
- `string Caption { get; set; }` (or reuse `Text`; `IForm` lacks `Text`, so declare one member — `Caption` is clearer and avoids ambiguity)

Added — intent methods that keep the option-rendering surface small:
- `ControlPosition CaptureAndRemoveTemplate();` (viewer performs `CreateTemplate(TemplateCheckBox)` + removes template; returns the template snapshot — mirrors `IQfcFormViewer.CaptureTlpCellStates`)
- `void FocusSearch();` (wraps `SearchText.Focus()`/`.Select()`)
- The option-checkbox collection is surfaced through a minimal panel abstraction so `Add`/`Remove`/enumerate work against a fake without a live form:
  - `void AddOptionControl(CheckBox control);`
  - `void RemoveOptionControl(CheckBox control);`
  - `IReadOnlyList<CheckBox> OptionControls { get; }`
  - `int OptionsPanelHeight { get; }`
  - `int OptionsScrollMaximum { get; }`
  - `Point OptionsAutoScrollPosition { get; set; }`

`CheckBox` remains the option element type. A `CheckBox` is constructible headless and its layout/property members do not create an HWND; only `.Handle`/focus-rectangle drawing does, which is isolated by the seam in section C. Tests exercise rendering against a simple in-memory fake `ITagViewer` (or Moq with a backing `List<CheckBox>`), never a live `Form`.

`SetController` stays on the interface: `void SetController(TagController controller);` (keeps `TagLauncher` and `CheckBoxController._parent` unchanged; `TagController` remains a public concrete type).

`TagViewer` change: declare `public partial class TagViewer : Form, ITagViewer`. The `IForm` base is satisfied implicitly by `Form`'s built-in members (same mechanism as the QuickFiler viewers). The intent members are thin mappings onto existing designer controls; the `*Clicked`/`*Changed` events forward the corresponding control events.

## C. Dialog / COM seam strategy (interface seam preferred, per `.claude/rules/csharp.md`)

Preference order applied: interface seam > injectable delegate > adapter.

1. MessageBox + InputBox → ONE interface seam `IUserPrompt` (new, in `Tags`):
   - `DialogResult ShowYesNo(string message, string title);` (covers `LoadSelections` and `TryGetAutoAssignment`)
   - `void ShowMessage(string message);` (covers the three `LoadControls` error popups)
   - `string GetCategoryInput(string prompt, string title, string defaultResponse);` (covers `GetUserInputCategory`; wraps `InputBox.ShowDialog`, so tests never construct `InputBoxViewer`)
   Production default adapter `WinFormsUserPrompt : IUserPrompt` calls `MessageBox.Show` and `InputBox.ShowDialog`. Injected via a new optional constructor parameter with a safe default (`prompt ??= new WinFormsUserPrompt()`), so existing production construction paths keep current behavior. Tests inject a Moq `IUserPrompt`.
   - Consequence: `GetUserInputCategory` changes from `internal static` to an instance method (or takes `IUserPrompt`), because it must route through the injected seam. This is a low-risk signature change; its only caller is `AddColorCategory` (465).

2. Focus-rectangle HWND draw → injectable delegate seam. Extract the two-line `ControlPaint.DrawFocusRectangle(Graphics.FromHwnd(cbx.Handle), cbx.ClientRectangle)` into a private `DrawFocus(CheckBox)` routed through an injectable `Action<CheckBox> _drawFocus` defaulting to the real ControlPaint call. Tests inject a no-op. `.Focus()` itself is safe (no-op when the handle is not created) and needs no seam. This makes `Select_Ctrl_By_Offset` / `Select_Ctrl_By_Position` (index arithmetic + focus routing) testable.

3. `ControlPosition` (CreateTemplate/Set) → NO seam required. Verified host-neutral (plain layout-property reads/writes, no `Handle`). `CaptureAndRemoveTemplate` moves behind the `ITagViewer.CaptureAndRemoveTemplate()` intent method so the controller never touches `TemplateCheckBox` directly.

4. Mail item (`ResolveMailItem`, `TryGetAutoAssignment`) → no new seam. Keep `object objItem`; `ResolveMailItem` is pure type discrimination; the mail is consumed only by the mocked `IAutoAssign`. Tests supply a Moq `MailItem` (existing `NewMailItem` helper) or a plain object.

## D. File decomposition (all < 500 lines)

| File | New/Changed | Responsibility | Approx. lines |
|---|---|---|---|
| `Tags/ITagViewer.cs` | new | Viewer interface `: IForm` (section B). Interface-only, no executable lines. | ~65 |
| `Tags/IUserPrompt.cs` | new | Dialog seam interface (section C.1). | ~20 |
| `Tags/WinFormsUserPrompt.cs` | new | Production adapter over `MessageBox`/`InputBox`. Thin, host-bound. | ~35 |
| `Tags/TagSelectionModel.cs` | new | Host-neutral logic: `Search`, `ParseSearchStrings`, `FilterArchive`, `IsPrefixMissing`, `SelectionAsList`, `SelectionAsString`, `GetSelections`, `ToggleChoice/On/Off`, dictionary part of `AddOption`, `UpdateSelections`, `ResolvePrefix`, `GetDefaultPrefix`, dictionary toggle of `LoadSelections`, filtered-set computation for `FilterToSelected`/`SearchAndReload`. Owns `_dictOriginal/_dictOptions/_filteredOptions/_selections/_filteredSelections/_prefix`. Zero WinForms references. | ~210 |
| `Tags/TagController.cs` | changed | Constructors, fields, `WireEvents` (subscribes to `ITagViewer` intent events), button/keyboard event handlers, public properties (`ButtonNewActive`, `ButtonAutoAssignActive`, `ExitType`, `SetSearchText`, `SetCaption`), `ResolveMailItem`, `SetAutoAssignState`, `TryGetAutoAssignment`, `AddColorCategory`, `GetUserInputCategory`. Delegates state to `TagSelectionModel`, UI to `ITagViewer`/`IUserPrompt`. | ~240 |
| `Tags/TagController.Rendering.cs` | new (partial of same class) | `LoadControls`, `RemoveControls`, `FilterToSelected`, `Select_*` navigation, `FocusCheckbox`, the `DrawFocus` seam. Interacts with the `ITagViewer` option-panel abstraction; creates `CheckBox` rows. | ~180 |
| `Tags/TagViewer.cs` | changed | `: Form, ITagViewer`; implement intent members mapping to designer controls; keep `SetController`. | ~120 |
| `Tags/LauncherAutoAssign.cs` | new (extracted) | Move `LauncherAutoAssign` out of `TagLauncher.cs` WITHOUT `[ExcludeFromCodeCoverage]` so it is testable (see F). | ~90 |
| `Tags/TagLauncher.cs` | changed | Remaining live-form launcher + globals wiring; stays `[ExcludeFromCodeCoverage]`. | ~175 |

Keeping `TagController` as a partial class across `TagController.cs` + `TagController.Rendering.cs` preserves the single public type so `TagLauncher` and `CheckBoxController` are unaffected.

## E. Callers and required changes

Production callers of the retargeted symbols (grep repo-wide; the large file hit list is dominated by docs/coverage XML — the only production caller is `TagLauncher`):
- `Tags/TagLauncher.cs` constructs `new TagController(...)` at lines 25, 42, 47, passing a concrete `TagViewer`. After the constructor parameter type changes `TagViewer` → `ITagViewer`, these sites compile UNCHANGED because `TagViewer` implements `ITagViewer` (implicit upcast). `TagLauncher.Viewer` stays concrete `TagViewer`; `LaunchAndFindMatch` still uses `launcher.Viewer.Controls.Remove(launcher.Viewer.ButtonNew)` and `ShowDialog()` on the concrete type — no change needed. If a new `IUserPrompt` constructor parameter is added, keep an overload that defaults it, so `TagLauncher` need not pass it.
- `Tags/TagViewer.cs` `SetController(TagController)` — unchanged (still takes `TagController`).
- `Tags/Helper Classes/CheckBoxController.cs` — `_parent` stays `TagController`; the callback methods it uses stay public on `TagController`. Unchanged.

Test callers: `Tags.Test/TagControllerTests.cs` and `Tags.Test/TagControllerCoverageExpansionTests.cs` construct `new TagController(viewer, ...)` with a real `TagViewer`. These must switch to a mocked/faked `ITagViewer` and inject a Moq `IUserPrompt` and the no-op `DrawFocus` (see G/H).

Mail-item handling: keep the `object objItem` boundary; `ResolveMailItem` returns `MailItem?`; the mail flows only into the mocked `IAutoAssign.AddChoicesToDict`. No live Outlook needed.

## F. `TagLauncher` and `CheckBoxController` coverage strategy (Tags project ≥ 80%)

Coverage denominator (Tags.dll, after policy-legitimate exemptions): `TagViewer`/`TagViewer.Designer` are WinForms form-derived + designer-generated (exempt category b). `TagLauncher` and `CheckBoxController` currently carry `[ExcludeFromCodeCoverage]`. The epic NFR forbids exempting testable seams.

- `TagLauncher.cs`: The live-form launcher (`LaunchAndSelect`, `FindMatch` → `ShowDialog`) and globals/COM wiring (`GetAutoAssign()` instance overload, `GetHelper` using `MailItemHelper`/`IOutlookItem`, `CreateCategoryModule`) are irreducibly host-bound → keep `[ExcludeFromCodeCoverage]`, maintainer-ratified. BUT the inner `LauncherAutoAssign` class and the static `GetAutoAssign(filterList, delegates...)` factory are pure delegate wiring and are testable. Recommendation: extract `LauncherAutoAssign` to `Tags/LauncherAutoAssign.cs` WITHOUT the exemption and unit-test its `AddChoicesToDict`/`AddColorCategory`/`AutoFind`/`AutoFindAsync`/`FilterList` pass-through. This adds testable lines and removes an over-broad exemption.
- `CheckBoxController.cs` (Helper Classes): its behavior is WinForms-event plumbing on a real `CheckBox`, but the click state machine (`TrigByKeyChg`/`TrigByValChg` + `Tag`/`Text`/prefix → `strTemp` → `_parent.ToggleChoice`/`FocusCheckbox`) is host-neutral decision logic. Recommendation: extract that decision into a testable helper method and cover it; leave only the event subscribe/unsubscribe wiring (`CtrlCB` setter, GotFocus/LostFocus color swap) under a narrowed, maintainer-ratified `[ExcludeFromCodeCoverage]`. If extraction is judged out-of-scope for #293, the exemption must be re-ratified explicitly rather than inherited silently (per the audit precedent in `[[qfc-item-controller-227-r2-denial]]` — maintainer denied blanket exemptions; per-member barrier analysis is expected).

Irreducible lines that would need justified `[ExcludeFromCodeCoverage]` (minimized): the `DrawFocus` seam default body (`Graphics.FromHwnd`/`ControlPaint`), `WinFormsUserPrompt` adapter bodies, `TagViewer` intent-member bodies, `TagLauncher` live-form/globals members, `CheckBoxController` event-wiring members. Everything else (model, controller orchestration, rendering arithmetic against the panel abstraction, `LauncherAutoAssign`) is testable and NOT exempt.

Coverage targets: repo policy (CLAUDE.md) is repo-wide ≥ 80% and NEW modules ≥ 90%. `TagSelectionModel` is a new module → must reach ≥ 90%. The `.claude/rules` line/branch figures (85/75) are noted; the binding gate for this feature per CLAUDE.md and the epic is 80% project / 90% new module.

## G. Existing tests — reusable vs must-change

`Tags.Test/TagControllerTests.cs`:
- Reusable: the reflection-based `PrefixItem` construction via `Activator`, the `InvokeOnClick` click-raising helper pattern.
- Must change: `new TagViewer()` (live form, `[STAThread]`) and `FindOptionCheckBox`/`FindControls` panel-walking depend on real rendering. Replace with a mocked/faked `ITagViewer`; assert model state (`GetSelections`) or the fake's recorded option rows instead of walking a live control tree.

`Tags.Test/TagControllerCoverageExpansionTests.cs`:
- Reusable directly: `TestPrefix` (a clean `IPrefix` fake that implements `PrefixType`/`OlUserFieldName` — needed because the production `Tags.PrefixItem` throws `NotImplementedException` on those two members), Moq `IAutoAssign` patterns (`NewAutoAssigner`), Moq `MailItem` (`NewMailItem`), private-field reflection helpers (`GetPrivateField`/`SetPrivateField`), dictionary fixtures, `ControllerFixture`.
- Must change:
  - `CreateFixture`/`CreateAutoAssignFixture` build a live `TagViewer`; switch to a fake/mock `ITagViewer` and inject Moq `IUserPrompt` + no-op `DrawFocus`.
  - `GetVisibleOptionCheckBoxes`/`FindNamedControl` assert against live controls; replace with the fake viewer's `OptionControls`.
  - `[STAThread]` becomes unnecessary once no live control realizes an HWND.
  - `AutoAssignClick_...` uses `Task.Delay(50)` to wait on `async void ButtonAutoAssign_Click` — this is nondeterministic and `Task.Delay` is a banned API. The refactor should extract `internal async Task ButtonAutoAssign_Action()`; the `async void` handler calls it, and the test awaits the `Task`-returning method directly (no delay).

## H. Per-method test plan (no live Form/Control, no popups, no Thread.Sleep/Task.Delay, no temp files, deterministic)

| Method(s) | Approach | Positive / Negative / Edge |
|---|---|---|
| `ParseSearchStrings` | pure call on model | empty/whitespace → empty; `"a*b"` → `["a","b"]`; trims; collapses empty splits |
| `Search` | pure | no search strings → source; case-insensitive substring; no match → empty |
| `FilterArchive` | Moq `IAutoAssign.FilterList` | exclusions removed case-insensitively; null autoassigner → source unchanged |
| `IsPrefixMissing` | pure | prefix present → false; absent → true; null sample → true; short sample → true |
| `SelectionAsList`/`SelectionAsString`/`GetSelections` | pure | selected keys only; empty; ordering |
| `ToggleChoice/On/Off`, `AddOption`, `UpdateSelections` | pure state transitions | add/update/remove; duplicate; empty key; filtered-list sync |
| `ResolvePrefix`/`GetDefaultPrefix` | pure | valid key resolves; null/empty → default; unknown key → `ArgumentException` |
| `ResolveMailItem` | Moq `MailItem` / plain object / null | MailItem → returns it; non-mail → null |
| `LoadSelections` | Moq `IUserPrompt.ShowYesNo` | existing key toggles; missing key + Yes → `AddColorCategory`; missing + No → skip; prefix re-add path |
| `TryGetAutoAssignment` | Moq `IAutoAssign` + Moq `IUserPrompt` | mail + Yes → assignments added, returns true; No → false; null autoassigner/`_isMail` false → false |
| `AddColorCategory` | Moq `IUserPrompt.GetCategoryInput` + Moq `IAutoAssign` | user text added; empty input → no-op; autoassigner returns null Category → early return; `FilterToSelected` invoked when assignments > 0 |
| `GetUserInputCategory` | Moq `IUserPrompt.GetCategoryInput` (routed) | prefilled name path; empty-name retry loop until non-`" "`; never constructs `InputBoxViewer` |
| `OptionsPanel_PreviewKeyDown` | direct call with `PreviewKeyDownEventArgs` | Up/Down set `IsInputKey=true`; other keys unchanged |
| `OptionsPanel_KeyDown` | direct call, no-op `DrawFocus` | Down → offset +1; Up → offset -1; assert `intFocus` |
| `TagViewer_KeyDown` | direct call | Enter → `ButtonOk_Action` (sets `_exitType="Normal"`, calls `ITagViewer.Close`, verified via Moq) |
| `SearchText_KeyDown`/`KeyUp` | fake/Moq `ITagViewer` `SearchSelectionStart` | Right records cursor; Down → offset; KeyUp Right at same position → `FilterToSelected`; Enter → OK |
| `Select_Ctrl_By_Offset`/`_By_Position`/`_First`/`_Last`/`_PageDown`/`_PageUp` | fake viewer with option rows + no-op `DrawFocus` | index math; boundary -1 selects search; out-of-range → `ArgumentOutOfRangeException`; page scroll math |
| `LoadControls`/`RemoveControls` | fake `ITagViewer` panel abstraction | rows added with correct `Name`/`Tag`/`Text`/`Checked`; prefix stripping; remove clears collections; error path via `IUserPrompt.ShowMessage` |
| `FilterToSelected`/`SearchAndReload` | fake viewer + `SearchTextValue` | only selected rows re-rendered; reload only when filter changes |
| `SetAutoAssignState`/`ButtonAutoAssignActive`/`ButtonNewActive`/`SetCaption`/`SetSearchText` | Moq `ITagViewer` verify | property get/set forwarded to intent members |
| `ButtonAutoAssign_Action` (extracted) | Moq `IAutoAssign.AutoFindAsync` awaited | existing key → ToggleOn; new key → AddOption; empty result → no FilterToSelected; exception propagates |
| `LauncherAutoAssign` (F) | Moq delegates | each method forwards to its `Func<>`; `AutoFindAsync` runs the sync delegate |

Determinism confirmed: all setups use Moq/fakes and pure inputs; the async auto-assign path is awaited on a `Task`-returning method (no `Task.Delay`); no `InputBoxViewer`/`MessageBox`/live `Form` is constructed; no filesystem/temp files; no wall-clock/RNG.

## Defects observed (report only — do not fix as part of this refactor unless in-scope)

- `RemoveControls` (697): `_colColorbox.Remove(i)` removes the boxed `int i` object, not the element at index `i` (latent; `_colColorbox` is always empty in the current flows).
- `Tags/PrefixItem.cs`: `PrefixType` and `OlUserFieldName` throw `NotImplementedException`. Tests must use a complete `IPrefix` fake (existing `TestPrefix`) and code paths must avoid these members (current `ResolvePrefix`/`LoadSelections` use only `.Key`/`.Value`).
- Orphan files `Tags/CheckBoxController.cs` and `Tags/AutoAssignInterface.cs` are not compiled; recommend explicit deletion in a follow-up cleanup, out of scope for #293.

## Rejected alternatives (brief)

- Interface-typed control wrappers for every control (Option B-i): rejected — maximum churn and a large adapter surface for no testability gain over the hybrid facade.
- Relying solely on `InputBox.DialogInvoker`: rejected — it still constructs a live `InputBoxViewer` form; routing through `IUserPrompt.GetCategoryInput` keeps tests headless.
- Moving all rendering/navigation into `TagViewer`: rejected — larger behavioral reshuffle that risks the epic's "no behavior change" NFR; the partial-class + panel-abstraction split achieves testability with smaller, behavior-preserving changes.

## Automation Feasibility

This refactor is code-only. It requires no third-party UI, no web portal, and no human-in-the-loop step at implementation time. All changes are C# source edits (interface extraction, seam introduction, file splitting) plus MSTest/Moq/FluentAssertions test additions, and are fully automatable through the standard toolchain in order: `csharpier .` → analyzer build (`msbuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`) → nullable/type-check build (`... /p:Nullable=enable /p:TreatWarningsAsErrors=true`) → `vstest.console.exe Tags.Test.dll /EnableCodeCoverage`. No manual UI validation is required because the seams eliminate every live-form and dialog dependency from the test path.
