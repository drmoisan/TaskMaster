# F4 Per-File Research — `QfcThemeHelper.cs`

Timestamp: 2026-08-07T22-40

Feature: `quickfiler-helper-classes-coverage` (issue #434), child F4 of epic
`quickfiler-per-file-coverage` (issue #136), wave 1, complexity band C3.

Scope of this artifact: exactly one production file, per the #136 one-file-at-a-time mandate.

---

## 1. File facts

| Fact | Value | Evidence |
| --- | --- | --- |
| Path | `QuickFiler/Helper Classes/QfcThemeHelper.cs` | — |
| Line count | 375 (last content line is `}` at line 375) | Read of offset 370 shows lines 370-375, EOF after 375 |
| Compiled | Yes | `QuickFiler/QuickFiler.csproj:351` — `<Compile Include="Helper Classes\QfcThemeHelper.cs" />` |
| `[ExcludeFromCodeCoverage]` | **Absent** — confirmed | Repo grep for `ExcludeFromCodeCoverage` across `QuickFiler/Helper Classes/` returned **no matches** |
| Namespace / type | `QuickFiler` / `internal static class QfcThemeHelper` | `QfcThemeHelper.cs:10`, `:12` |
| Internals visible to tests | Yes | `QuickFiler/Properties/AssemblyInfo.cs:5` |

Numeric baseline line coverage is captured at execution time with F1's per-file coverage harness
(epic `Shared Design` §6) and recorded under `<FEATURE>/evidence/qa-gates/`. This artifact
establishes the gap by reading production members against the existing test file.

---

## 2. Member inventory (the coverage denominator)

Static class; no fields, properties, constructors, or events.

| # | Member | Signature | Line span | Decision points |
| --- | --- | --- | --- | --- |
| M1 | `SetTheme` (TLP) | `public static void SetTheme(this TableLayoutPanel tlp, Color backColor)` | 14–17 | 0 |
| M2 | `SetTheme` (Label) | `public static void SetTheme(this Label lbl, Color backColor, Color forecolor)` | 19–23 | 0 |
| M3 | `SetTheme` (Button) | `public static void SetTheme(this Button btn, Color backColor)` | 25–28 | 0 |
| M4 | `SetTheme` (Control) | `public static void SetTheme(this Control control, Color backColor, Color forecolor)` | 30–34 | 0 |
| M5 | `SetupThemes` (production entry) | `public static Dictionary<string, Theme> SetupThemes(IQfcItemController controller, ItemViewer viewer, Action<Enums.ToggleState> htmlConverter, UtilitiesCS.Threading.IUiDispatcher uiDispatcher)` | 36–55 | **2** — `if (controller is null)` @43, `if (viewer is null)` @47 |
| M6 | `BuildProductionControlSet` | `internal static QfcThemeControlSet BuildProductionControlSet(IQfcItemController controller, ItemViewer viewer, Action<Enums.ToggleState> htmlConverter, UtilitiesCS.Threading.IUiDispatcher uiDispatcher)` | 57–94 | **2** — `if (controller is null)` @65, `if (viewer is null)` @69 |
| M6-L1 | closure `theme => viewer.BreadcrumbCoordinator?.SetTheme(theme)` | line **86** | 1 line | **1** — the `?.` null-conditional |
| M6-L2 | closure `() => !controller.Mail.UnRead` | line **90** | 1 line | 0 (unguarded dereference of `controller.Mail`, an Outlook `MailItem`) |
| M7 | `SetupThemes` (seam entry) | `internal static Dictionary<string, Theme> SetupThemes(QfcThemeControlSet controlSet)` | 96–238 | **1** — `if (controlSet is null)` @98 |
| M8 | `SetupFormThemes` | `public static Dictionary<string, Theme> SetupFormThemes(IList<Control> panels, IList<Control> buttons)` | 240–296 | 0 in body |
| M8-L1 | closure `(x) => false` (LightNormal Buttons) | line **264** | 1 line | 0 |
| M8-L2 | closure `(x) => false` (DarkNormal Buttons) | line **287** | 1 line | 0 |
| M9 | `CreateTheme` | `private static Theme CreateTheme(QfcThemeControlSet controlSet, string name, CoreWebView2PreferredColorScheme web2ViewScheme, Enums.ToggleState htmlDark, Color × 22)` | 298–373 | 0 — a single 45-argument `new Theme(...)` expression (327–372) |

**Total executable surface: 9 methods + 4 lambda bodies.**

### Verified facts about the member inventory

- **M1–M4 have no production callers.** Repo-wide grep for `.SetTheme(` finds only:
  `QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs:213-216` (tests), plus unrelated
  `Theme.SetTheme()` / breadcrumb-coordinator `SetTheme(string)` calls. The four extension methods
  in this file are **dead production code kept alive only by tests**. They remain in the coverage
  denominator regardless.
- **M5 delegates to M6 + M7** at lines 52-54, so its own success path is 3 lines of glue.
- **M7 and M9 are the #236 "coverage seams" refactor.** `QfcThemeControlSet` (see artifact 03)
  already exists as the injectable value object that decouples theme construction from `ItemViewer`.
  M9 is a pure data-to-`Theme` mapper; `Theme`'s primary constructor (`UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs:20-111`)
  performs field assignment only and touches no control.
- **`Theme`'s constructor defaults `uiDispatcher` to `new WpfUiDispatcher()`** (`Theme.cs:63`, `:67`)
  when null is passed. `QfcThemeControlSet` forbids a null `uiDispatcher` (`QfcThemeControlSet.cs:57`),
  so M9 always supplies a real value and the WPF fallback is never taken from this file.
- **Issue #269 regression comments** at lines 117-120 and 152-155 document the positional argument
  order `(mailReadForeColor, mailReadBackColor, mailUnreadForeColor, mailUnreadBackColor)`. That
  ordering is pinned by an existing regression test (§3).

---

## 3. Existing test inventory

**One test file: `QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs`**, registered at
`QuickFiler.Test/QuickFiler.Test.csproj:161`. 463 lines. No other file in the repository references
`QfcThemeHelper` (verified by repo-wide `*.cs` grep).

| Test method (file:line) | Production member(s) exercised |
| --- | --- |
| `SetupFormThemes_ReturnsExpectedKeysAndControlGroups` (`:28`) | M8 (full body, both dictionary literals) |
| `SetupThemes_WithControlSet_ReturnsFourExpectedThemeKeys` (`:45`) | M7 success path (103-237), M9 ×4 |
| `SetupThemes_WithControlSet_MapsRepresentativeColorsAndHtmlStates` (`:58`) | M7, M9 — asserts `NavBackColor`, `DefaultBackColor`, `DefaultForeColor`, `HtmlDark` |
| `SetupThemes_LightThemes_MailLabelColorsAreDarkTextOnLightBackground` (`:81`) | M7, M9 — issue #269 regression pinning `mailRead*`/`mailUnread*` ordering |
| `SetupThemes_WithNullController_ThrowsArgumentNullException` (`:109`) | M5 lines 43-45 (true branch of guard 1) |
| `SetupThemes_WithNullViewer_ThrowsArgumentNullException` (`:121`) | M5 lines 47-49 (true branch of guard 2) |
| `BuildProductionControlSet_MapsControllerAndViewerInputs` (`:133`) | M6 success path (73-93). Asserts `TableLayoutPanels`, `Buttons`, `TipsDetailsLabels`, `TipsExpanded`, `MenuItems`, `MenuStrip`, `Viewer`, `UiDispatcher` are the supplied instances; asserts `MailRead` is non-null (**does not invoke it**); **invokes `HtmlConverter`** at `:162` |
| `SetupFormThemes_ButtonGroups_ApplyLightAndDarkHoverBranches` (`:167`) | M8 + **M8-L1 (264)** and **M8-L2 (287)** via `ApplyTheme()` and reflected `OnMouseEnter`/`OnMouseLeave` (`:277-285`) |
| `QfcThemeControlSet_NullRequiredCollection_ThrowsArgumentNullException` (`:197`) | `QfcThemeControlSet` — see artifact 03, not this file |
| `SetTheme_Extensions_ApplyColorsToControls` (`:206`) | M1, M2, M3, M4 (all four bodies) |

Test infrastructure worth reusing (do not re-invent):

- `CreateControlSet(...)` (`:296-329`) — builds a fully populated `QfcThemeControlSet` from in-memory
  controls.
- `CreateItemViewer()` (`:247-265`) — builds an `ItemViewer` via
  `FormatterServices.GetUninitializedObject` (`:331-335`) and assigns its public control properties;
  sets `_menuItems` by reflection (`:287-294`). **This is the established headless-`ItemViewer`
  technique for this repo.**
- `FakeQfcItemController` (`:337-461`) — a hand-written `IQfcItemController` stub.
- `RaiseMouseEnter`/`RaiseMouseLeave` (`:267-285`) — reflection onto `Control.OnMouseEnter`/`OnMouseLeave`.

---

## 4. Per-member coverage gap

| Member | Status | Missed detail |
| --- | --- | --- |
| M1 `SetTheme(TableLayoutPanel)` | **covered** | `:213` |
| M2 `SetTheme(Label)` | **covered** | `:214` |
| M3 `SetTheme(Button)` | **covered** | `:215` |
| M4 `SetTheme(Control)` | **covered** | `:216` |
| M5 `SetupThemes(controller, viewer, ...)` | **partially covered** (branches missed: the success path) | Both `throw` branches covered (`:109`, `:121`); the fall-through at **lines 52-54** is **never executed** — no test calls the 4-argument overload with two non-null arguments. |
| M6 `BuildProductionControlSet` | **partially covered** (branches missed: both null guards) | Success path covered (`:133`). Lines **65-67** and **69-71** are unreached — the guards in M5 fire first in every existing test, and M6 is never called directly with a null. |
| M6-L1 closure @86 | **uncovered** | `:133` asserts nothing about `BreadcrumbThemeNotifier` and never invokes it. |
| M6-L2 closure @90 | **uncovered** | `:160` asserts only `MailRead.Should().NotBeNull()`; the delegate body is never invoked. |
| M7 `SetupThemes(controlSet)` | **partially covered** (branches missed: null guard) | Success path fully covered (`:45`, `:58`, `:81`). Lines **98-101** unreached. |
| M8 `SetupFormThemes` | **covered** | `:28`, `:167` |
| M8-L1 @264, M8-L2 @287 | **covered** | `:167-194` |
| M9 `CreateTheme` | **covered** | Called four times per `SetupThemes(controlSet)` invocation. |

**Summary of the real gap: five guard/glue regions (lines 52-54, 65-67, 69-71, 98-101) and two
lambda bodies (86, 90).** Everything else in the file is already reached. This is the smallest gap
of the four F4 theme/layout files.

---

## 5. Testability classification per member

| Member | Classification | WinForms / COM API touched |
| --- | --- | --- |
| M1 | **pure-testable-now** | `TableLayoutPanel.BackColor` (write) |
| M2 | **pure-testable-now** | `Label.BackColor`, `Label.ForeColor` (write) |
| M3 | **pure-testable-now** | `Button.BackColor` (write) |
| M4 | **pure-testable-now** | `Control.BackColor`, `Control.ForeColor` (write) |
| M5 | **pure-testable-now** | None directly; delegates to M6/M7 |
| M6 | **pure-testable-now** | Reads `ItemViewer.LblItemNumber`, `.LblSender`, `.LblSubject`, `.MenuItems`, `.MoveOptionsStrip`, `.TxtboxSearch`, `.TxtboxBody`, `.L0vhBreadcrumb_WebView2`, `.TopicThread`, `.L0v2h2_WebView2`, `.BreadcrumbCoordinator` — **all field/auto-property reads; no handle creation, no `Show()`, no `Invoke`**. Reads `IQfcItemController.TableLayoutPanels`, `.Buttons`, `.ListTipsDetails`, `.ListTipsExpanded`. Already proven testable at `QfcThemeHelperTests.cs:133-164`. |
| M6-L1 @86 | **pure-testable-now for the null branch; host-bound-irreducible for the non-null branch** | `ItemViewer.BreadcrumbCoordinator` is `internal … { get; private set; }` (`QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:25`). With the headless `FormatterServices` viewer the backing field is null, so the `?.` short-circuit branch is directly reachable. Making it **non**-null requires constructing a `BreadcrumbBridgeCoordinator` (`QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs:31,56`), which is F12-owned and instantiates a WebView2 messenger hub. **The line is covered by the null branch; only the taken-`?.` sub-branch is irreducible for F4.** |
| M6-L2 @90 | **needs-seam → already seamed** | Reads `IQfcItemController.Mail` (`Microsoft.Office.Interop.Outlook.MailItem`) and `.UnRead`. `Mail` is a settable property on the interface, and `FakeQfcItemController` (`QfcThemeHelperTests.cs:347`) already exposes `public Outlook.MailItem Mail { get; set; }`. A `Mock<Outlook.MailItem>` (Moq over the interop interface; `[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]` is present at `QuickFiler/Legacy/IAcceleratorCallbacks.cs:5`) supplies `UnRead`. No live Outlook needed. |
| M7 | **pure-testable-now** | None |
| M8, M8-L1, M8-L2 | **pure-testable-now** | Via `ThemeControlGroup.ApplyTheme()` on in-memory `Button`/`Panel` |
| M9 | **pure-testable-now** | None — `Theme`'s constructor assigns fields only (`Theme.cs:66-111`) |

---

## 6. Seam proposal

**Recommendation: introduce NO new seam. Make no production change to this file.**

The seam this file needs already exists and was ratified by the #236 refactor:

- **Interface/value-object seam (rank 1, already in place): `QfcThemeControlSet`.** The host-bound
  extraction (`ItemViewer` and `IQfcItemController` → plain control references) is isolated in M6,
  and the pure mapping (`control set + colour table → Dictionary<string, Theme>`) is isolated in M7
  and M9. M7 is `internal` and directly callable from `QuickFiler.Test`
  (`AssemblyInfo.cs:5`). This is exactly the shape the epic §2 hierarchy targets: pure decision
  logic taking and returning plain values, with host binding confined to a thin wrapper.
- **Injectable delegate seam (rank 2, already in place):** `Action<Enums.ToggleState> htmlConverter`,
  `Func<bool> mailRead`, `Action<string> breadcrumbThemeNotifier`, and
  `IUiDispatcher uiDispatcher` are all constructor-injected onto `QfcThemeControlSet` rather than
  reached statically.

Options considered and rejected:

- **Rejected — extracting the 4×24 colour table into a `QfcThemePalette` static table.** Would move
  ~140 lines out of M7. Produces **zero** additional coverage: M7 and M9 are already fully covered,
  and the extracted table would simply become a new fully-covered file. It also risks silently
  reordering the positional arguments that the issue #269 regression test
  (`QfcThemeHelperTests.cs:81-106`) exists to pin. Not worth the regression risk during a 14-way
  parallel wave.
- **Rejected — an `IItemViewerThemeSource` interface replacing the concrete `ItemViewer` parameter of
  M5/M6.** This would let M6 be driven from a Moq stub instead of a `FormatterServices`-created
  viewer. It changes the **public** signature of M5, which is called from
  `QuickFiler/Controllers/QfcItemController.Initialization.cs:175, 209, 267, 299` — **four
  sibling-owned (F10) call sites**. See §7. The `FormatterServices` technique already works
  (`QfcThemeHelperTests.cs:247-265`), so the seam buys nothing and costs a guaranteed conflict.

**Conflict statement: requires no sibling-owned file change** (no production change at all).

---

## 7. Cross-child conflict analysis

F4 owns only the 13 files under `QuickFiler/Helper Classes/` plus `QuickFiler/Interfaces/IEmailMoveMonitor.cs`.

### Every file outside F4 that calls into `QfcThemeHelper` (repo-wide `*.cs` grep)

| Call site | Member called | Owning child |
| --- | --- | --- |
| `QuickFiler/Controllers/QfcItemController.Initialization.cs:175` | `QfcThemeHelper.SetupThemes(...)` (M5) | **F10** `quickfiler-item-controller-coverage` |
| `QuickFiler/Controllers/QfcItemController.Initialization.cs:209` | `QfcThemeHelper.SetupThemes(...)` (M5) | **F10** |
| `QuickFiler/Controllers/QfcItemController.Initialization.cs:267` | `QfcThemeHelper.SetupThemes(...)` (M5) | **F10** |
| `QuickFiler/Controllers/QfcItemController.Initialization.cs:299` | `QfcThemeHelper.SetupThemes(...)` (M5) | **F10** |
| `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs:82` | `QfcThemeHelper.SetupFormThemes(_formViewer.Panels, _formViewer.Buttons)` (M8) | **F6** `quickfiler-qfc-form-explorer-controller-coverage` |

Non-executable references (comments only, no compile dependency):
`QuickFiler/Controllers/QfcItemController.Initialization.cs:166`, `:235`, `:331` — **F10**.

`QfcThemeControlSet` is constructed only inside this file (lines 73, and referenced at 57, 96, 299)
and in `QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs`. **No sibling-owned file constructs
it.**

**Verdict: requires no sibling-owned file change.** Under the tests-only recommendation, all five
F10/F6 call sites keep compiling byte-identically. Had the rejected `IItemViewerThemeSource` seam
been adopted it would have required editing
`QuickFiler/Controllers/QfcItemController.Initialization.cs:175,209,267,299` (owned by **F10**) —
the alternative that avoids that is precisely the recommendation above: keep the existing
`(IQfcItemController, ItemViewer, Action, IUiDispatcher)` signature and test through the already-
`internal` `SetupThemes(QfcThemeControlSet)` overload.

### Test-side type dependencies (no production edit)

The new tests reference `ItemViewer` (**F14**-owned), `IQfcItemController` (**F10**-owned), and
`Microsoft.Office.Interop.Outlook.MailItem`. All are consumed through their **existing** public /
internal surface, so no sibling production file changes. If F10 or F14 changes those surfaces
mid-wave, the resulting break is a compile error in F4's test file, not a merge conflict; the epic's
per-child R1–R5 remediation loop handles it.

### Shared-file risk

| Shared file | Required edit | Risk |
| --- | --- | --- |
| `QuickFiler.Test/QuickFiler.Test.csproj` | **NONE** | `QfcThemeHelperTests.cs` is already registered at line 161. Adding tests to that existing file needs no csproj change. **This is the preferred destination for all M1–M9 tests for exactly this reason.** |
| `QuickFiler/QuickFiler.csproj` | **NONE** | No production change. |

**Caveat:** `QfcThemeHelperTests.cs` is currently **463 lines** against the 500-line limit
(`.claude/rules/general-code-change.md` § File Size Limit applies to test code). The 20 new cases in
§9 will not fit. **Plan: relocate the reusable `Arrange` helpers (`CreateControlSet`,
`CreateItemViewer`, `CreateUninitialized`, `SetPrivateField`, `RaiseMouseEnter`/`RaiseMouseLeave`,
`FakeQfcItemController` — lines 226-461, ~236 lines) into a new
`QuickFiler.Test/Helper Classes/QfcThemeHelperTests.TestSupport.cs`** (a `partial class`
continuation, matching the existing repo convention at
`QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs`). That leaves `QfcThemeHelperTests.cs`
at ~230 lines with room for ~20 new cases, and costs **one** new
`<Compile Include="Helper Classes\QfcThemeHelperTests.TestSupport.cs" />` line in
`QuickFiler.Test/QuickFiler.Test.csproj`, inserted inside the contiguous `Helper Classes\` block at
lines 158-165 (a region no sibling touches).

---

## 8. 500-line compliance

- **Production file: 375 of 500. Headroom 125 lines. Compliant, and no production change is
  proposed, so it stays at 375.** No partial split required.
- **Test file: 463 of 500. Headroom 37 lines — insufficient.** See the mitigation in §7: extract the
  test-support region (lines 226-461) into a `partial class` file. This is the only 500-line action
  item for this production file.
- Cross-check of the other three F4 theme/layout files (recorded here for completeness; each has its
  own artifact): `EfcThemeHelper.cs` 499/500 — 1 line of headroom, the binding constraint of the
  cluster; `QfcThemeControlSet.cs` 110/500 — ample; `TlpCellSnapShot.cs` 223/500 — ample.

---

## 9. Recommended test cases (enumerated individually)

Destination for all: **existing file** `QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs`
(after the test-support extraction of §7). MSTest + Moq + FluentAssertions.

| # | `[TestMethod]` name | Arrange / Act / Assert | Category |
| --- | --- | --- | --- |
| 1 | `SetupThemes_WithControllerAndViewer_ReturnsFourThemesBuiltFromTheProductionControlSet` | Arrange `CreateController(...)` + `CreateItemViewer()` + `Mock<IUiDispatcher>`; Act call the 4-argument `SetupThemes`; Assert the four expected keys — **covers M5 lines 52-54, the only unreached glue in M5.** | positive |
| 2 | `SetupThemes_WithControllerAndViewer_CarriesTheSuppliedHtmlConverterOntoEveryTheme` | Arrange a recording `Action<Enums.ToggleState>`; Act as #1, then invoke `themes["DarkNormal"].HtmlConverter(Enums.ToggleState.On)`; Assert the recorder observed exactly one `On`. | positive |
| 3 | `SetupThemes_WithNullControlSet_ThrowsArgumentNullException` | Act `QfcThemeHelper.SetupThemes((QfcThemeControlSet)null)`; Assert `Throw<ArgumentNullException>().WithParameterName("controlSet")` — **covers M7 lines 98-101.** | invalid-input |
| 4 | `BuildProductionControlSet_WithNullController_ThrowsArgumentNullException` | Act call M6 directly with `controller: null`; Assert `WithParameterName("controller")` — **covers M6 lines 65-67.** | invalid-input |
| 5 | `BuildProductionControlSet_WithNullViewer_ThrowsArgumentNullException` | Act call M6 directly with `viewer: null`; Assert `WithParameterName("viewer")` — **covers M6 lines 69-71.** | invalid-input |
| 6 | `BuildProductionControlSet_BreadcrumbThemeNotifier_WithNoCoordinator_DoesNotThrow` | Arrange a headless `ItemViewer` (`BreadcrumbCoordinator` backing field null); Act `controlSet.BreadcrumbThemeNotifier("DarkNormal")`; Assert no exception — **covers the M6-L1 line at 86 via the `?.` short-circuit.** | boundary |
| 7 | `BuildProductionControlSet_MailRead_ReturnsTrueWhenTheMailItemIsRead` | Arrange `Mock<Outlook.MailItem>` with `UnRead == false` assigned to `FakeQfcItemController.Mail`; Act `controlSet.MailRead()`; Assert `BeTrue()` — **covers M6-L2 line 90.** | positive |
| 8 | `BuildProductionControlSet_MailRead_ReturnsFalseWhenTheMailItemIsUnread` | as #7 with `UnRead == true`; Assert `BeFalse()`. | positive |
| 9 | `BuildProductionControlSet_MailRead_WhenTheControllerHasNoMailItem_ThrowsNullReferenceException` | Arrange `FakeQfcItemController.Mail == null`; Act invoke `controlSet.MailRead()`; Assert `Throw<NullReferenceException>()` — pins the documented absence of a guard at line 90. | error-handling |
| 10 | `BuildProductionControlSet_MailRead_IsNotInvokedDuringConstruction` | Arrange a `FakeQfcItemController` whose `Mail` getter throws; Act call M6; Assert no exception — proves the delegate is deferred, not evaluated eagerly. | error-handling |
| 11 | `SetupThemes_WithControlSet_LightThemesCarryTheLightPreferredColorScheme` | Act M7 on `CreateControlSet()`; Assert `themes["LightNormal"].Web2ViewScheme` and `themes["LightActive"].Web2ViewScheme` are `CoreWebView2PreferredColorScheme.Light`. | positive |
| 12 | `SetupThemes_WithControlSet_DarkThemesCarryTheDarkPreferredColorSchemeAndHtmlToggleOn` | as #11; Assert `Dark` and `HtmlDark == Enums.ToggleState.On` for both dark themes. | positive |
| 13 | `SetupThemes_WithControlSet_LightThemesCarryHtmlToggleOff` | as #11; Assert `HtmlDark == Enums.ToggleState.Off` for both light themes. | boundary |
| 14 | `SetupThemes_WithControlSet_MapsTheSuppliedSenderAndSubjectLabelsOntoEveryTheme` | Act M7; then `themes["DarkNormal"].SetMailRead()`; Assert the control set's `LblSender`/`LblSubject` received `MailReadBackColor`/`MailReadForeColor` — proves M9 passed the right control references, not just the right colours. | positive |
| 15 | `SetupThemes_WithControlSet_MapsTheSuppliedUiDispatcherOntoEveryTheme` | Arrange a `Mock<IUiDispatcher>`; Act M7, then `themes["LightNormal"].SetQfcTheme(async: true)`; Assert the mock's `InvokeAsync` was called once — proves M9 line 371 wiring and guarantees no `WpfUiDispatcher` fallback (`Theme.cs:67`). | positive |
| 16 | `SetupThemes_WithControlSet_DarkNormalTipsDetailsColorsMatchTheSpecification` | Act M7; Assert `themes["DarkNormal"].TipsDetailsBackColor == Color.LightSkyBlue` and `TipsDetailsForeColor == SystemColors.ActiveCaptionText` — extends the #269-style pinning to a colour pair no existing test asserts. | positive |
| 17 | `SetupFormThemes_WithNullPanelsList_ThrowsArgumentNullException` | Act `SetupFormThemes(null, buttons)`; Assert `Throw<ArgumentNullException>()` (raised by `ThemeControlGroup.cs:24-27`). | invalid-input |
| 18 | `SetupFormThemes_WithEmptyPanelsList_ThrowsArgumentOutOfRangeException` | Act `SetupFormThemes(new List<Control>(), buttons)`; Assert `Throw<ArgumentOutOfRangeException>()` (`ThemeControlGroup.cs:48-56`). | boundary |
| 19 | `SetupFormThemes_ButtonsGroup_AltHoverPredicateIsAlwaysFalseRegardlessOfDialogResult` | Arrange `new Button { DialogResult = DialogResult.OK }`; Act `themes["LightNormal"].ControlGroups["Buttons"].ApplyTheme()`; Assert the button receives the **main** colours, not alternate — pins the `(x) => false` lambdas at 264/287 against future divergence from `EfcThemeHelper`'s `DialogResult`-sensitive predicate. | boundary |
| 20 | `SetTheme_OnTableLayoutPanelAndButton_LeavesForeColorUnchanged` | Arrange a `TableLayoutPanel` and a `Button` with a known `ForeColor`; Act M1 and M3; Assert `BackColor` changed and `ForeColor` is unchanged — distinguishes the one-colour overloads (M1, M3) from the two-colour overloads (M2, M4). | boundary |

**Total: 20 enumerated test cases.** Category spread: 9 positive, 4 invalid-input, 4 boundary,
3 error-handling — all four categories present.

---

## 10. STA determination

**STA is NOT required for any member of this file. No `*.StaTests.cs` file should be created.**

Per-member justification, working down the seam hierarchy:

- **M1–M4, M8, M9:** touch only `Control.BackColor`/`ForeColor` writes or plain field assignment. No
  handle, no form, no dispatcher. Already proven at `QfcThemeHelperTests.cs:206-224` and `:167-194`,
  neither of which is STA-scoped.
- **M5, M7:** touch nothing host-bound.
- **M6:** reads public properties on a headless `ItemViewer` created via
  `FormatterServices.GetUninitializedObject` (`QfcThemeHelperTests.cs:331-335`). This constructs no
  Designer control tree, runs no `InitializeComponent`, creates no handle, and shows nothing. The
  seam hierarchy was not exhausted here — it was **satisfied at rank 1** by `QfcThemeControlSet`
  plus the existing headless-viewer technique, so STA never becomes relevant.
- **M6-L1 non-null branch:** the only genuinely unreachable sub-branch. It is unreachable because it
  requires an F12-owned `BreadcrumbBridgeCoordinator`, **not** because it needs an STA thread.
  Creating one on an STA thread would still not make the test deterministic (it starts a WebView2
  messenger hub). Correct disposition: leave the sub-branch to F12's coordinator tests; F4 covers
  the line via the null branch (test #6).

Test constraints reaffirmed: never construct a `Form`; never call `Theme.SetTheme(bool async)` or
`ThemeControlGroup.ApplyTheme(bool async)` (both route through `UiThread.Dispatcher` —
`ThemeControlGroup.cs:212-229`); never call `Theme.SetMailRead(bool async)` or
`SetMailUnread(bool async)` (both call `Control.Invoke`/`BeginInvoke`, which requires a handle —
`Theme.cs:359-366`, `:397-404`). Use the parameterless overloads only, as test #14 does.

---

## 11. Determinism

| Concern | Finding | Requirement on tests |
| --- | --- | --- |
| Wall-clock time | None in this file. | No clock seam needed. `TimeProvider` guidance (`.claude/rules/csharp.md` § Time seam) does not apply. |
| Randomness | None. | — |
| Ambient state — **`SystemColors`** | **Present and material.** Lines 112-137, 148-172, 183-192, 213-217, 250-263, 271-289 read `SystemColors.HotTrack`, `.Control`, `.ControlText`, `.Window`, `.WindowText`, `.ControlDark`, `.ActiveCaptionText`. These resolve against the machine's active Windows theme and differ between a workstation and a CI runner. | Assert symbolically (`.Should().Be(SystemColors.Control)`), never against a literal ARGB. Existing precedent: `QfcThemeHelperTests.cs:64`, `:90-93`, `:178`, `:184`. |
| Ambient state — **`Theme`'s `WpfUiDispatcher` fallback** | `Theme.cs:67` constructs a real WPF dispatcher when `uiDispatcher` is null. `QfcThemeControlSet.cs:57` forbids null, so M9 never triggers it. | Every test must supply a `Mock<IUiDispatcher>` to `CreateControlSet`. Test #15 asserts this explicitly. |
| Ambient state — `ItemViewer.BreadcrumbCoordinator` | Null on a `FormatterServices`-created viewer; deterministic. | Test #6 relies on this; document the reliance in the test comment. |
| COM — `Outlook.MailItem` | Reached only through M6-L2, and only via `Mock<Outlook.MailItem>`. No live Outlook process, no `Application` object. | Moq over the interop interface; `DynamicProxyGenAssembly2` is already granted (`QuickFiler/Legacy/IAcceleratorCallbacks.cs:5`). |
| `Thread.Sleep` / `Task.Delay` / real waits | None in the file; **prohibited** in tests. | — |
| Temporary files, external services | None. | Prohibited by UT4. |
| Cross-test shared state | `ApplyTheme` mutates control colours and subscribes `MouseEnter`/`MouseLeave` handlers. | Build controls per test in `Arrange`; no `[ClassInitialize]` control instances. |

---

## 12. Projected coverage

- Existing tests already reach M1–M4, M7 (success), M8, M8-L1, M8-L2, and M9 in full, plus both
  `throw` branches of M5.
- The 20 cases in §9 close every remaining region: M5's success glue (52-54), M6's two guards
  (65-67, 69-71), M7's guard (98-101), and both lambda bodies (86, 90).
- After the proposed set, the only sub-branch left unexecuted in the entire file is the *taken*
  `?.` path of M6-L1 at line 86. That is **one conditional branch on an already-covered line**, so
  it costs **zero** line coverage.
- **Projected line coverage: ~100% of executable lines; projected branch coverage ≈ 97%** (one of
  roughly 34 branch outcomes unreached).
- **Clears the 80% floor with a wide margin.** The file was already close to the floor before this
  work; the value of the 20 cases is branch/scenario completeness under UT2, plus regression pinning
  of the #269 colour ordering and the `MailRead`/`HtmlConverter` deferral contracts.
- **This file does not require an exemption.** It should be classified `testable` in F1's ledger
  (`docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`), which remains the
  authority on that classification.
- Numeric before/after per-file figures are produced by **F1's harness** (Cobertura output of
  `Invoke-MSTestWithCoverage.ps1`) at execution time and committed under
  `<FEATURE>/evidence/qa-gates/`.
