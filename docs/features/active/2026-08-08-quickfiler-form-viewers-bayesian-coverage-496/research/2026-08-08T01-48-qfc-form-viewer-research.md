## `QuickFiler/Viewers/QfcFormViewer.cs` (262 lines) — a `Form`, carries `[ExcludeFromCodeCoverage]`

- **Epic child:** F15 (`quickfiler-form-viewers-bayesian-coverage`, issue #496), parent epic #136.
- **Measured baseline:** not separately tabulated in the epic's below-floor/branch-floor tables because the file is currently absent from Cobertura output entirely (suppressed by its own type-level attribute) — "an absent file is not a covered file" per the epic's Scope section.
- **`[ExcludeFromCodeCoverage]` location:** type-level, `QfcFormViewer.cs:17`, on `public partial class QfcFormViewer : Form, IQfcFormViewer` (verified by reading the file directly). Independently cross-checked against the F16 capstone's Q1 census, which lists this exact site (`QfcFormViewer.cs`, line 17, type-level, `partial`, 2 files suppressed).
- **Files suppressed by this one attribute:** 2 — `QfcFormViewer.cs` itself and `QfcFormViewer.Designer.cs` (257 lines), because a type-level attribute on a partial type propagates to every partial (confirmed general rule, and confirmed specifically for this pair by the F16 capstone census).

### Frozen contract this file must not touch

`QfcFormViewer` implements `IQfcFormViewer` (`QuickFiler/Interfaces/IQfcFormViewer.cs`, 51 lines), which sibling F6's ratified spec (`docs/features/active/2026-08-07-quickfiler-qfc-form-explorer-controller-coverage-435/spec.md`) declares **frozen**: "No member may be added to, removed from, or renamed on any of the five interface files," with `IQfcFormViewer` specifically named as consumed by F3, F7, F11, plus 15 mock sites, and F6 states explicitly it needs **no edit** to `QfcFormViewer.cs` because the interface is already seam-complete for F6's purposes — but F6 also states plainly that F6 does not remove the attribute on F15's behalf and F15 "is obliged to cover that file." **This research confirms that constraint is achievable without touching `IQfcFormViewer`'s member set**: every member added below the `#region IQfcFormViewer` marker (lines 106-260) is already an implementation of an existing interface member; nothing needs to be added to the interface to make those members testable, because they are already public and already reachable through the concrete type or the interface reference.

### Current structure

`public partial class QfcFormViewer : Form, IQfcFormViewer`. Two constructors are not present — only one: `QfcFormViewer()` calls `InitializeComponent()`, captures `_context = SynchronizationContext.Current` and `_uiScheduler = TaskScheduler.FromCurrentSynchronizationContext()`. No branch in the constructor itself.

Members and branch points:

- `SetController(IFilerFormController controller)`, `SetKeyboardHandler(IQfcKeyboardHandler)` — trivial field assignment, no branch.
- `ProcessCmdKey(ref Message msg, Keys keyData)` (override) — **one real branch**: `if ((_keyboardHandler is not null) && Controllers.QfcFormKeyHandler.IsAltKeyCommand(keyData)) { ...; return true; } return base.ProcessCmdKey(...);`. This is a `protected override` member of `Form`, reachable in a unit test by calling it directly via a test-assembly-visible shim (it is `protected`, and `QfcFormViewer` is not `sealed`, so a test-only subclass in the test project can expose it, or the test can use reflection — `protected` members are not directly callable from `QuickFiler.Test` even with `InternalsVisibleTo`, since `InternalsVisibleTo` only affects `internal`/`protected internal`, not plain `protected`). A minimal test-only subclass (`private sealed class TestableQfcFormViewer : QfcFormViewer { public bool CallProcessCmdKey(ref Message m, Keys k) => ProcessCmdKey(ref m, k); }`) is the standard, seam-free way to reach a `protected override` member; this needs no production change.
- `Panels` / `LoadPanels()`, `Buttons` / `LoadButtons()` — each a `Initializer.GetOrLoad(ref field, factory)` lazy-init pattern with **no visible branch in this file** (the branch, if any, lives inside `UtilitiesCS.Initializer.GetOrLoad`, outside F15's file set). Calling the getter twice and asserting the same list instance is returned (memoization) is the correct scenario-completeness test even though there is no local branch to "cover" — it is a state-transition case per `.claude/rules/general-unit-test.md`.
- `#region IQfcFormViewer` members (lines 106-260): `Worker` (forwards to `WorkerInternal`, a Designer-declared `BackgroundWorker` field — no branch), `L1v0L2L3v_TableLayout`/`L1v_TableLayout`/`L1v0L2_PanelMain` (plain field forwards, no branch), `SwapItemTableLayout(TableLayoutPanel)` (no branch — unconditional remove/reassign/reparent/show), four `event EventHandler` add/remove pairs forwarding to Designer button `Click` events (no branch), `SkipButtonText`/`SkipButtonEnabled`/`ItemsPerLoadValue`/`ItemsPerLoadValueChanged`/`ItemsPerLoadEnabled` (plain forwards, no branch), `ItemViewerTemplateMargin` (**one branch**: `_QfcItemViewerTemplate?.Margin ?? default`), `GetKeyEventExclusionControls()` (no branch), `CaptureTlpCellStates()` (**one branch**: `if (_qfcItemViewerExpandedTemplate is null || _QfcItemViewerTemplate is null) { return null; }`, otherwise builds and returns a `TlpCellStates` from six `TlpCellSnapShot` pairs per display state).

Total real branch points: `ProcessCmdKey` (1) + `ItemViewerTemplateMargin` (1, a null-coalescing operator) + `CaptureTlpCellStates` (1, a two-operand `||` guard) = 3 branches across 262 lines — a small branch surface relative to the line count, dominated by plain property/event forwards.

### Independent verification of the already-verified finding: `_qfcItemViewerExpandedTemplate` is F14's type, not F15's

Read `QfcFormViewer.Designer.cs` directly (lines 41-42, 252, 256):

```
this._QfcItemViewerTemplate = new QuickFiler.ItemViewer();
this._qfcItemViewerExpandedTemplate = new QuickFiler.ItemViewerExpanded();
...
public ItemViewer _QfcItemViewerTemplate;
public ItemViewerExpanded _qfcItemViewerExpandedTemplate;
```

Confirmed independently: both fields are of types owned by F14 (`QuickFiler.ItemViewer`, `QuickFiler.ItemViewerExpanded` — the latter declared at `QuickFiler/Viewers/ItemViewerExpanded.cs:16`, `public partial class ItemViewerExpanded : UserControl`, **no** `[ExcludeFromCodeCoverage]` attribute on that particular file). Neither is `QuickFiler.QfcItemViewerExpanded` (F15's own, differently-named type, declared separately at `QuickFiler/Viewers/QfcItemViewerExpanded.cs:19`). The near-identical names (`ItemViewerExpanded` vs. `QfcItemViewerExpanded`) are a genuine naming trap but the two types are unrelated by inheritance or composition — `QfcFormViewer` never references `QfcItemViewerExpanded` anywhere. This means `CaptureTlpCellStates()`'s two null-guard operands are guarding against F14-owned control instances, and constructing a real (unshown) `QfcFormViewer` via `InitializeComponent()` will also construct real (unshown, off-screen) `ItemViewer` and `ItemViewerExpanded` instances as a side effect — both have parameterless constructors with no Outlook/COM dependency (verified by reading both constructors: `ItemViewer()` calls `InitializeComponent()`, captures a `SynchronizationContext`/`TaskScheduler`/`Dispatcher`, and calls `InitControlGroups()`; `ItemViewerExpanded()` is the analogous shape). This is consistent with the epic's F14 finding (recorded in prior memory) that headless `ItemViewer` construction is safe.

### What is already tested vs. the gap

**Nothing.** No test file exists for `QfcFormViewer` (`QuickFiler.Test/Viewers/QfcFormViewer*` does not exist — verified by `Glob`). The entire file is a coverage gap because the type-level `[ExcludeFromCodeCoverage]` attribute currently suppresses it from measurement altogether.

### Proposed seams

**No new interface seam** — `IQfcFormViewer` is frozen by F6's ratified contract and every member needed for testing is already implemented and already public (or `protected`, handled via a test-only subclass as noted above). The seam hierarchy resolves as follows:

1. **Interface seam: not applicable / already complete.** The interface already exposes everything F6 needs and everything this file's own tests need to assert against (the interface members ARE the members under test).
2. **Injectable delegate: not needed** for any member in this file — there is no COM call, no timer, no external I/O anywhere in `QfcFormViewer.cs`. Every branch (`ProcessCmdKey`, `ItemViewerTemplateMargin`, `CaptureTlpCellStates`) is pure, deterministic logic over already-constructed WinForms control references.
3. **Adapter: not needed.**

The only "seam" this file needs is the **test-only subclass** for the `protected override ProcessCmdKey` member, which is a test-code construct, not a production seam, and does not touch `QfcFormViewer.cs` at all.

### STA / DEC-1 last-resort clause — required

**Yes, required**, and this is the central design decision for this file. `QfcFormViewer` is `Form`-derived and its `InitializeComponent()` (in the suppressed `QfcFormViewer.Designer.cs`) builds the entire control tree, including nested F14-owned `ItemViewer`/`ItemViewerExpanded` instances. No existing test constructs it. Per epic Ruling DEC-1 (ratified, and `issue.md`'s own Constraints section states this applies "directly"), the plan should:

- Construct one **unshown** `QfcFormViewer` on a dedicated STA thread, dispose in `finally`, reusing the `RunWithViewer` harness shape from `BayesianPerformanceController.TestSupport.cs` verbatim (STA `Thread`, `SynchronizationContext` save/restore, `ExceptionDispatchInfo` exception marshalling).
- This single construction is expected to drive `QfcFormViewer.cs` to a very high line rate (following the F9/`EfcViewer` and F14/`ItemViewer` precedent already ratified for the identical shape) and simultaneously exercises the paired `QfcFormViewer.Designer.cs`'s `InitializeComponent()` almost completely, plus its one-branch `Dispose(bool)` override.
- STA-bound tests live in a dedicated `QuickFiler.Test/Viewers/QfcFormViewer.StaTests.cs` (new file) per the epic's convention, each documenting why no seam suffices (answer: `Form.InitializeComponent()` cannot be invoked outside a real construction, and `Form` construction requires an STA thread in this repository's established precedent).
- **Never** call `.Show()`/`.ShowDialog()` on the constructed instance. The three branch-bearing members (`ProcessCmdKey` via the test-only subclass, `ItemViewerTemplateMargin`, `CaptureTlpCellStates`) can all be exercised on the unshown instance after construction, inside the same `RunWithViewer`-style `action` callback.

### Disposition proposal: remove-and-cover

**Remove the `[ExcludeFromCodeCoverage]` attribute at `QfcFormViewer.cs:17` and cover the file**, rather than seek an F1-ledger exemption. None of F1's three exemption grounds (CLAUDE.md's original three, restated in the epic) nor the epic's fourth ground (prohibited-to-execute adapter, ratified for F13's WebView2 case only) apply on inspection:

- Not a VSTO add-in lifecycle class.
- **Is** Form-derived — the textual WinForms ground is available — but the epic's Shared Design §1 already closes this off explicitly for this exact file: "the attribute on `QfcFormViewer.cs:17` is itself treated as unratified, and F15 is obliged to cover that file" (F6's spec, § Anti-Pattern Guard). This is not this researcher's inference; it is F6's ratified spec quoting the epic's own ruling.
- Not an Outlook Interop event-handler class without an injectable seam — `QfcFormViewer.cs` has zero direct Outlook Interop references anywhere in the file (verified: no `Microsoft.Office.Interop.Outlook` usage).
- Not a prohibited-to-execute adapter (fourth ground) — its members are not 1:1 forwards into an external runtime requiring a filesystem or process side effect; they are ordinary WinForms property/event forwards and two pure guard branches.

Removing the attribute is achievable via the ratified DEC-1 STA construction pattern with **no behavior change**, since DEC-1 explicitly ratifies unshown Form construction for exactly this purpose and this file requires no new seam to reach every branch. The class-level exemption must not be replaced with per-member exemptions either — the epic's `#457` lambda-suppression trap warns that method-level exemptions leak nested lambdas into the denominator, and none of this file's members contain closures worth worrying about, but more fundamentally: since the file is fully coverable via DEC-1, no exemption of any granularity is warranted.

### Zero-branch caveat

Not applicable to `QfcFormViewer.cs` — it has 3 real branch points (`ProcessCmdKey`, `ItemViewerTemplateMargin`, `CaptureTlpCellStates`), so once instrumented it will report a genuine percentage. (Its paired `QfcFormViewer.Designer.cs` **is** a single-branch generated file once the attribute is removed — see the separate generated-files research artifact, which additionally notes this pairing is exactly the DEC-1/DEC-5 interaction the epic anticipated: removing the type-level attribute exposes the Designer file to instrumentation as a wanted side effect.)
