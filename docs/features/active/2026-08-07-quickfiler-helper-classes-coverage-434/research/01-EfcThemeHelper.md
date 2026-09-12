# F4 Per-File Research — `EfcThemeHelper.cs`

Timestamp: 2026-08-07T22-40

Feature: `quickfiler-helper-classes-coverage` (issue #434), child F4 of epic
`quickfiler-per-file-coverage` (issue #136), wave 1, complexity band C3.

Scope of this artifact: exactly one production file, per the #136 one-file-at-a-time mandate.

---

## 1. File facts

| Fact | Value | Evidence |
| --- | --- | --- |
| Path | `QuickFiler/Helper Classes/EfcThemeHelper.cs` | — |
| Line count | 499 (last content line is `}` at line 499) | Read of offset 490 shows lines 490-499, EOF after 499 |
| Compiled | Yes | `QuickFiler/QuickFiler.csproj:345` — `<Compile Include="Helper Classes\EfcThemeHelper.cs" />` |
| `[ExcludeFromCodeCoverage]` | **Absent** — confirmed | Repo grep for `ExcludeFromCodeCoverage` across `QuickFiler/Helper Classes/` returned **no matches** |
| Namespace / type | `QuickFiler.Helper_Classes` / `internal static class EfcThemeHelper` | `EfcThemeHelper.cs:12`, `:14` |
| Internals visible to tests | Yes | `QuickFiler/Properties/AssemblyInfo.cs:5` — `[assembly: InternalsVisibleTo("QuickFiler.Test")]` |

Numeric baseline line coverage for this file is **not** stated here. It is captured at execution
time with F1's per-file coverage harness (epic `Shared Design` §6), and recorded under
`<FEATURE>/evidence/qa-gates/`. This artifact establishes the coverage *gap* by reading production
members and the (absent) test surface.

---

## 2. Member inventory (the coverage denominator)

`EfcThemeHelper` is a static class with two public methods and no fields, properties, constructors,
or events.

| # | Member | Signature | Line span | Decision points |
| --- | --- | --- | --- | --- |
| M1 | `SetupThemes` | `public static Dictionary<string, Theme> SetupThemes(IList<Control> nav, IList<Control> tips, IList<Control> dflt2, IList<Control> selectors, IList<Control> mail, Func<bool> isAlt, IList<object> olvColumns, Action<IList<object>, Color, Color> olvSetter, Microsoft.Web.WebView2.WinForms.WebView2 webView2, Action<Enums.ToggleState> htmlConverter)` | 16–247 | **0** — straight-line construction of four `Dictionary<string, ThemeControlGroup>` literals (lines 31, 81, 131, 185) and one `Dictionary<string, Theme>` return literal (240–246). No `if`, `switch`, ternary, `??`, loop, or `catch`. |
| M2 | `SetupFormThemes` | `public static Dictionary<string, Theme> SetupFormThemes(IList<Control> tips, IList<Control> highlighted, IList<Control> default2Color, IList<Control> buttons, IList<Control> checkboxes)` | 249–497 | **0 in the method body.** One local (`darkDarkGrey`, 259–263), four `Dictionary<string, ThemeControlGroup>` literals (264, 316, 368, 420), one `Dictionary<string, Theme>` (473–479), return at 496. |
| M2-L1..L8 | Eight compiler-generated closure methods from the `isAltHover:` lambdas inside M2 | `(x) => ((Button)x).DialogResult == DialogResult.OK` at **300, 352, 404, 456**; `(x) => ((CheckBox)x).Checked` at **312, 364, 416, 468** | one line each | 1 comparison each; each `Button`/`CheckBox` cast is an implicit failure path (`InvalidCastException`). |

**Total executable surface: 2 methods + 8 lambda bodies.**

### Verified facts about the member inventory

- **`nav` and `selectors` are dead parameters of M1.** `nav` appears only inside commented-out lines
  33, 83, 133, 187. `selectors` appears nowhere after the signature at line 19. Passing `null` for
  either is safe.
- **The eight lambdas are in the file's coverage denominator.** A historical Cobertura-style report
  in this repo lists them explicitly under this file's type:
  `docs/features/archive/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/qa-gates/2026-06-12T15-54/p2-coverage.xml:60501`
  and `:60506` record `<SetupFormThemes>b__1_0(object)` and `<SetupFormThemes>b__1_1(object)` with
  `namespace="QuickFiler.Helper_Classes" type_name="EfcThemeHelper.<>c"` at
  `line_coverage="0.00"`. Any test set that ignores them leaves measured lines uncovered.
- **Neither method touches a WinForms control.** `ThemeControlGroup`'s constructors only store
  references (`UtilitiesCS/HelperClasses/ThemeHelpers/ThemeControlGroup.cs:22-129`); the
  `Theme(string, Dictionary<string, ThemeControlGroup>)` constructor only assigns two fields
  (`UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs:143-151`). All control mutation lives in
  `ThemeControlGroup.ApplyTheme*` (`ThemeControlGroup.cs:231-296`), which is UtilitiesCS code, not
  this file.
- **Input validation is delegated, and is asymmetric.** Named-argument groups resolve as follows:
  - `controls:`/`back:`/`fore:` → `ThemeControlGroup(IList<Control>, Color fore, Color back)`
    (`ThemeControlGroup.cs:42-62`), which **throws** `ArgumentNullException` on a null list and
    `ArgumentOutOfRangeException` on an empty list. In M1 this applies to `tips` and `dflt2`; in M2
    to `tips`, `highlighted`, and `default2Color`.
  - `backMain:`/`foreMain:`/`backAlt:`/`foreAlt:`/`isAlt:` → `ThemeControlGroup(..., Func<bool>)`
    (`:64-80`) — **no validation**. Applies to `mail` in M1.
  - `backMain:`/.../`hover:`/`isAltHover:` → `ThemeControlGroup(..., Func<object,bool>)` (`:82-100`)
    — **no validation**. Applies to `buttons` and `checkboxes` in M2.
  - `objects:`/`objectSetter:` → `ThemeControlGroup(IList<object>, ..., Action<...>)` (`:102-114`) —
    **no validation**. Applies to `olvColumns` in M1.
  - `webView2:` → `ThemeControlGroup(WebView2, scheme, htmlConverter, htmlDark)` (`:116-129`) —
    **no validation**; a null `WebView2` is stored and wrapped in `new List<Control> { webView2 }`
    without dereference.

---

## 3. Existing test inventory

**There is no test file for this type.**

- `SearchScope:` `QuickFiler.Test/**`, and repository-wide across `*.cs`.
- `SearchPatterns:` `EfcThemeHelper`.
- `SearchResult:` production and build references only —
  `QuickFiler/Helper Classes/EfcThemeHelper.cs:14`,
  `QuickFiler/Controllers/EfcItemController.cs:93`,
  `QuickFiler/Controllers/EfcItemController.cs:140`,
  `QuickFiler/Controllers/EfcFormController.cs:239`,
  `QuickFiler/QuickFiler.csproj:345`. **No `QuickFiler.Test` file references this type.**
- `QuickFiler.Test/QuickFiler.Test.csproj:158-165` lists the eight registered `Helper Classes\`
  test files; none is an `EfcThemeHelper` test.

---

## 4. Per-member coverage gap

| Member | Status | Missed detail |
| --- | --- | --- |
| M1 `SetupThemes` (16–247) | **uncovered** | Entire body. No caller in any test assembly. |
| M2 `SetupFormThemes` (249–497) | **uncovered** | Entire body. |
| M2-L1 `(x) => ((Button)x).DialogResult == DialogResult.OK` @300 | **uncovered** | Requires `themes["LightNormal"].ControlGroups["Buttons"].ApplyTheme()`. |
| M2-L2 `(x) => ((CheckBox)x).Checked` @312 | **uncovered** | Requires `themes["LightNormal"].ControlGroups["CheckBoxes"].ApplyTheme()`. |
| M2-L3 @352 (LightActive Buttons) | **uncovered** | Distinct closure method; a separate `ApplyTheme()` call is required. |
| M2-L4 @364 (LightActive CheckBoxes) | **uncovered** | as above |
| M2-L5 @404 (DarkNormal Buttons) | **uncovered** | as above |
| M2-L6 @416 (DarkNormal CheckBoxes) | **uncovered** | as above |
| M2-L7 @456 (DarkActive Buttons) | **uncovered** | as above |
| M2-L8 @468 (DarkActive CheckBoxes) | **uncovered** | as above |

**This file is the largest untested surface in the F4 theme/layout cluster: 100% of its executable
lines are currently unreached.**

---

## 5. Testability classification per member

| Member | Classification | WinForms/COM API touched |
| --- | --- | --- |
| M1 `SetupThemes` | **pure-testable-now** | **None.** The `WebView2` parameter is stored, never dereferenced (`ThemeControlGroup.cs:123-124`). The `IList<Control>` parameters are stored, never read. |
| M2 `SetupFormThemes` | **pure-testable-now** | **None.** |
| M2-L1..L8 (lambdas) | **pure-testable-now** | Invoked indirectly through `ThemeControlGroup.ApplyThemeTwoFieldAltHover` (`ThemeControlGroup.cs:265-284`), which touches `Control.RemoveEventHandlers("MouseEnter")`, `Control.MouseEnter +=`, `Control.MouseLeave +=`, `Control.ForeColor`, `Control.BackColor` on caller-supplied instances. The lambda bodies themselves read `Button.DialogResult` and `CheckBox.Checked`. All are property reads/writes on in-memory, never-shown controls; none creates a window handle. |

**Precedent that this works without STA and without a live form:**
`QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs:167-194`
(`SetupFormThemes_ButtonGroups_ApplyLightAndDarkHoverBranches`) already performs the identical
manoeuvre against `QfcThemeHelper.SetupFormThemes` — constructs `new Button()`, calls
`ControlGroups["Buttons"].ApplyTheme()`, asserts `BackColor`, and raises `OnMouseEnter`/
`OnMouseLeave` by reflection (`:277-285`). That test class carries no STA attribute and lives in a
plain `*Tests.cs` file.

---

## 6. Seam proposal

**Recommendation: introduce NO seam. Make no production change to this file.**

Rationale, evaluated against the epic §2 hierarchy (interface seam > injectable delegate > adapter):

The hierarchy is applied only when a seam is *required* to reach the code deterministically. It is
not required here. Every input this file needs is already a parameter, every dependency it
constructs is a pure data holder, and no member reads process, host, or COM state. The file is
already at the end state the seam hierarchy aims for: pure decision logic (a colour table) that
takes and returns plain values, with control mutation living outside it in
`ThemeControlGroup.ApplyTheme*`.

Options considered and rejected:

- **Rejected — interface seam `IEfcThemePaletteSource`.** Would replace the colour literals with an
  injectable palette. Yields no coverage the direct call does not already yield, adds indirection
  contrary to CLAUDE.md § "Simplicity first", and forces new lines into a 499-line file (see §8).
- **Rejected — injectable delegate `Func<string, ThemeControlGroup>` factory parameter.** Same
  objection; also changes the public signature consumed by three sibling-owned call sites (§7).
- **Rejected — extracting the colour table into a new host-neutral `EfcThemePalette` type.** This is
  the option most aligned with the epic's long-term "prefer host-neutral extraction a future
  WebView2/Office.js port can reuse" preference (epic Non-Goals). It is still rejected *for this
  child*: it moves ~430 lines, changes no observable behaviour, produces no coverage benefit
  (`SetupThemes`/`SetupFormThemes` would still need the same call to be covered), and materially
  increases merge-conflict surface during a 14-way parallel wave. Record it as a follow-up issue
  rather than doing it here.

**Conflict statement: requires no sibling-owned file change** (no production change at all).

---

## 7. Cross-child conflict analysis

F4 owns only the 13 files under `QuickFiler/Helper Classes/` plus `QuickFiler/Interfaces/IEmailMoveMonitor.cs`.
Every other QuickFiler file belongs to a sibling running in parallel.

### Callers of `EfcThemeHelper` outside F4 (repo-wide `*.cs` grep)

| Call site | Member called | Owning child |
| --- | --- | --- |
| `QuickFiler/Controllers/EfcItemController.cs:93` | `SetupThemes(...)` (10 args) | **F9** `quickfiler-efc-form-item-controller-coverage` |
| `QuickFiler/Controllers/EfcItemController.cs:140` | `SetupThemes(...)` (10 args) | **F9** |
| `QuickFiler/Controllers/EfcFormController.cs:239` | `SetupFormThemes(...)` (5 args) | **F9** |

No other production or test file in the repository references the type.

**Verdict: requires no sibling-owned file change.** Because the recommendation is
tests-only, all three F9-owned call sites keep compiling byte-identically.

### Shared-file risk that does apply

| Shared file | Required edit | Risk |
| --- | --- | --- |
| `QuickFiler.Test/QuickFiler.Test.csproj` | one new line `<Compile Include="Helper Classes\EfcThemeHelperTests.cs" />` inserted into the `Helper Classes\` block at lines **158-165** | **Every one of the 14 wave-1 children adds test files, so all 14 edit this file.** Mitigation: insert the new entry alphabetically *inside* the existing contiguous `Helper Classes\` block (between `:158 ConversationResolverTests.cs` and `:159 EmailMoveMonitorTests.cs`), which is a region no sibling touches — siblings append to `Controllers\`, `Viewers\`, and `Interfaces\` blocks. A 3-way merge of disjoint hunks inside one `<ItemGroup>` resolves cleanly. |
| `QuickFiler/QuickFiler.csproj` | **no edit** under the recommendation | — |

---

## 8. 500-line compliance

- Current: **499 of 500**. Headroom: **1 line**.
- Under the recommendation (tests only, no production edit) the file stays at 499 and is compliant.
  **No partial split is needed.**
- **Contingency, if any production line must be added.** Convert to a partial class and split by
  method:

  | New file | Members moved | Approx. size |
  | --- | --- | --- |
  | `QuickFiler/Helper Classes/EfcThemeHelper.cs` (retained) | `SetupThemes` (current lines 1–247) | ~250 lines |
  | `QuickFiler/Helper Classes/EfcThemeHelper.FormThemes.cs` (new) | `SetupFormThemes` (current lines 249–497) | ~260 lines |

  Both files declare `internal static partial class EfcThemeHelper`. Adding the `partial` keyword
  edits line 14 in place and adds no line. Both members are `public static` on an `internal` type,
  so no call site changes and no accessibility changes.

  **Shared-file conflict risk (flagged):** the new file requires a
  `<Compile Include="Helper Classes\EfcThemeHelper.FormThemes.cs" />` line in
  `QuickFiler/QuickFiler.csproj`, in the `Helper Classes\` block at lines **342-354**. Siblings F2
  (`QfcQueue.cs` 500-line split), F9 (`EfcFormController.cs`/`EfcItemController.cs` splits), F10 and
  F11 (`QfcCollectionController.cs` split) will each add production files to the *same*
  `<ItemGroup>`. Their additions land in the `Controllers\` block (lines ~300-341), which is
  textually separated from the `Helper Classes\` block, so the hunks are disjoint — but this is the
  single highest-probability merge conflict in F4 and should be avoided by not triggering the split.

---

## 9. Recommended test cases (enumerated individually)

Destination for all: **new file** `QuickFiler.Test/Helper Classes/EfcThemeHelperTests.cs`
(MSTest `[TestClass]`, Moq where a delegate recorder is insufficient, FluentAssertions).

Estimated file size ~430 lines with a shared `Arrange` builder; if it exceeds 500 lines, split into
`EfcThemeHelperTests.cs` (M1) and `EfcThemeHelperFormThemesTests.cs` (M2) — **two** csproj lines
instead of one.

### M1 `SetupThemes`

| # | `[TestMethod]` name | Arrange / Act / Assert | Category |
| --- | --- | --- | --- |
| 1 | `SetupThemes_ReturnsLightAndDarkNormalAndActiveThemeKeys` | Arrange in-memory `tips`/`dflt2`/`mail` lists, `olvColumns`, recording `olvSetter`, uninitialized `WebView2`, no-op `htmlConverter`; Act call `SetupThemes`; Assert keys are exactly `LightNormal`, `LightActive`, `DarkNormal`, `DarkActive`. | positive |
| 2 | `SetupThemes_EachTheme_ExposesTheFiveExpectedControlGroups` | Act as #1; Assert every returned `Theme.ControlGroups` key set equals `Tips`, `Default2Color`, `MailRelated`, `OlvColumns`, `WebView2`. | positive |
| 3 | `SetupThemes_LightNormalTipsGroup_AppliesBlackBackgroundAndWhiteText` | Act `themes["LightNormal"].ControlGroups["Tips"].ApplyTheme()`; Assert the tip `Label.BackColor == Color.Black` and `ForeColor == Color.White`. | positive |
| 4 | `SetupThemes_DarkNormalTipsGroup_AppliesLightSkyBlueBackground` | as #3 for `DarkNormal`; Assert `BackColor == Color.LightSkyBlue`, `ForeColor == SystemColors.ControlText`. | positive |
| 5 | `SetupThemes_LightActiveDefault2ColorGroup_AppliesLightCyanBackground` | Act `ApplyTheme()` on `LightActive/Default2Color`; Assert `BackColor == Color.LightCyan`. | positive |
| 6 | `SetupThemes_DarkActiveDefault2ColorGroup_AppliesSixtyFourGreyBackground` | Act on `DarkActive/Default2Color`; Assert `BackColor == Color.FromArgb(64, 64, 64)` exactly. | boundary |
| 7 | `SetupThemes_MailRelatedGroup_WhenIsAltPredicateIsTrue_AppliesAlternateAccentColors` | Arrange `isAlt = () => true`; Act `ApplyTheme()` on `DarkNormal/MailRelated`; Assert `ForeColor == Color.Goldenrod`, `BackColor == Color.Black`. | positive |
| 8 | `SetupThemes_MailRelatedGroup_WhenIsAltPredicateIsFalse_AppliesMainColors` | Arrange `isAlt = () => false`; Act on `DarkNormal/MailRelated`; Assert `ForeColor == Color.WhiteSmoke`, `BackColor == Color.Black`. | positive |
| 9 | `SetupThemes_MailRelatedGroup_WhenIsAltPredicateThrows_PropagatesTheException` | Arrange `isAlt = () => throw new InvalidOperationException()`; Act `ApplyTheme()`; Assert `Should().Throw<InvalidOperationException>()`. | error-handling |
| 10 | `SetupThemes_OlvColumnsGroup_InvokesInjectedSetterWithLightThemeColors` | Arrange recording `Action<IList<object>, Color, Color>`; Act `ApplyTheme()` on `LightNormal/OlvColumns`; Assert the recorded tuple is `(olvColumns, SystemColors.ControlText, SystemColors.Control)` and the same list instance is passed. | positive |
| 11 | `SetupThemes_OlvColumnsGroup_InvokesInjectedSetterWithDarkThemeColors` | as #10 for `DarkNormal`; Assert `(Color.WhiteSmoke, Color.Black)`. | positive |
| 12 | `SetupThemes_EveryTheme_ContainsAWebView2ControlGroup` | Act as #1 with an uninitialized `WebView2`; Assert each theme's `ControlGroups` contains key `WebView2` and the group is non-null. | positive |
| 13 | `SetupThemes_WithNullTipsList_ThrowsArgumentNullException` | Arrange `tips = null`; Act; Assert `Throw<ArgumentNullException>().WithParameterName("controls")`. | invalid-input |
| 14 | `SetupThemes_WithEmptyTipsList_ThrowsArgumentOutOfRangeException` | Arrange `tips = new List<Control>()`; Act; Assert `Throw<ArgumentOutOfRangeException>()`. | boundary |
| 15 | `SetupThemes_WithNullDefault2ColorList_ThrowsArgumentNullException` | Arrange `dflt2 = null` with a valid `tips`; Act; Assert throw. | invalid-input |
| 16 | `SetupThemes_WithEmptyDefault2ColorList_ThrowsArgumentOutOfRangeException` | Arrange `dflt2 = new List<Control>()`; Act; Assert throw. | boundary |
| 17 | `SetupThemes_WithNullNavAndSelectorLists_StillReturnsFourThemes` | Arrange `nav = null`, `selectors = null`; Act; Assert four themes returned. Documents that both parameters are unreferenced (see §2). | boundary |
| 18 | `SetupThemes_WithNullWebView2AndNullMailList_StillReturnsFourThemes` | Arrange `webView2 = null`, `mail = null`; Act; Assert four themes returned — proves the unvalidated constructor overloads are reached. | boundary |

### M2 `SetupFormThemes`

| # | `[TestMethod]` name | Arrange / Act / Assert | Category |
| --- | --- | --- | --- |
| 19 | `SetupFormThemes_ReturnsLightAndDarkNormalAndActiveThemeKeys` | Arrange five non-empty in-memory control lists; Act; Assert the four expected keys. | positive |
| 20 | `SetupFormThemes_EachTheme_ExposesTheFiveExpectedControlGroups` | Act as #19; Assert group keys are `Tips`, `highlighted`, `Default2Color`, `Buttons`, `CheckBoxes`. | positive |
| 21 | `SetupFormThemes_LightNormalButtonsGroup_AppliesControlBackgroundAndLightCyanHover` | Act `ApplyTheme()` on `LightNormal/Buttons`, then raise `OnMouseEnter`/`OnMouseLeave` by reflection; Assert `SystemColors.Control` → `Color.LightCyan` → `SystemColors.Control`. **Covers lambda M2-L1 (line 300).** | positive |
| 22 | `SetupFormThemes_LightActiveButtonsGroup_AppliesControlBackgroundAndLightCyanHover` | as #21 for `LightActive`. **Covers M2-L3 (line 352).** | positive |
| 23 | `SetupFormThemes_DarkNormalButtonsGroup_AppliesDimGrayWithDarkGrayHover` | as #21 for `DarkNormal`; Assert `Color.DimGray` → `Color.DarkGray` → `Color.DimGray`. **Covers M2-L5 (line 404).** | positive |
| 24 | `SetupFormThemes_DarkActiveButtonsGroup_AppliesDimGrayWithDarkGrayHover` | as #23 for `DarkActive`. **Covers M2-L7 (line 456).** | positive |
| 25 | `SetupFormThemes_LightNormalCheckBoxesGroup_UsesCheckedStateAsAlternatePredicate` | Arrange one `CheckBox { Checked = true }` and one unchecked; Act `ApplyTheme()` on `LightNormal/CheckBoxes`; Assert both receive `SystemColors.Control`/`SystemColors.ControlText` (main and alt are identical in Light) and hover resolves per checked state. **Covers M2-L2 (line 312).** | positive |
| 26 | `SetupFormThemes_LightActiveCheckBoxesGroup_UsesCheckedStateAsAlternatePredicate` | as #25 for `LightActive`. **Covers M2-L4 (line 364).** | positive |
| 27 | `SetupFormThemes_DarkNormalCheckBoxesGroup_UsesCheckedStateAsAlternatePredicate` | as #25 for `DarkNormal`; Assert `Color.Black`/`Color.WhiteSmoke` and `Color.DarkGray` on hover. **Covers M2-L6 (line 416).** | positive |
| 28 | `SetupFormThemes_DarkActiveCheckBoxesGroup_UsesCheckedStateAsAlternatePredicate` | as #25 for `DarkActive`. **Covers M2-L8 (line 468).** | positive |
| 29 | `SetupFormThemes_ButtonsGroup_WhenAControlIsNotAButton_ThrowsInvalidCastException` | Arrange `buttons = new List<Control> { new Panel() }`; Act `ApplyTheme()` on `LightNormal/Buttons`; Assert `Throw<InvalidCastException>()` — exercises the failure edge of the `((Button)x)` cast in M2-L1. | error-handling |
| 30 | `SetupFormThemes_CheckBoxesGroup_WhenAControlIsNotACheckBox_ThrowsInvalidCastException` | as #29 with `checkboxes = new List<Control> { new Panel() }`. | error-handling |
| 31 | `SetupFormThemes_DarkNormalHighlightedGroup_AppliesThirtyGreyBackground` | Act `ApplyTheme()` on `DarkNormal/highlighted`; Assert `BackColor == Color.FromArgb(30, 30, 30)` — pins the `darkDarkGrey` local (lines 259-263). | boundary |
| 32 | `SetupFormThemes_DarkActiveHighlightedGroup_AppliesThirtyGreyBackground` | as #31 for `DarkActive` — the same local is reused at line 434. | boundary |
| 33 | `SetupFormThemes_LightActiveDefault2ColorGroup_AppliesLightCyanBackground` | Act `ApplyTheme()` on `LightActive/Default2Color`; Assert `Color.LightCyan`. | positive |
| 34 | `SetupFormThemes_WithNullTipsList_ThrowsArgumentNullException` | Arrange `tips = null`; Act; Assert throw. | invalid-input |
| 35 | `SetupFormThemes_WithEmptyHighlightedList_ThrowsArgumentOutOfRangeException` | Arrange `highlighted = new List<Control>()`; Act; Assert throw. | boundary |
| 36 | `SetupFormThemes_WithNullDefault2ColorList_ThrowsArgumentNullException` | Arrange `default2Color = null`; Act; Assert throw. | invalid-input |
| 37 | `SetupFormThemes_WithNullButtonsAndCheckboxLists_StillReturnsFourThemes` | Arrange `buttons = null`, `checkboxes = null`; Act; Assert four themes — proves the unvalidated `TwoFieldAltHover` overload is reached. | boundary |

**Total: 37 enumerated test cases.** Category spread: 18 positive, 5 invalid-input, 11 boundary,
3 error-handling — all four categories present, as required.

---

## 10. STA determination

**STA is NOT required for any member of this file. No `*.StaTests.cs` file should be created.**

Per-member justification:

- **M1 and M2**: touch zero WinForms APIs (§5). The seam hierarchy is not even entered.
- **M2-L1..L8**: reached through `ThemeControlGroup.ApplyTheme()`, which reads/writes
  `Control.BackColor`, `Control.ForeColor`, and adds/removes `MouseEnter`/`MouseLeave` handlers on
  in-memory `Button`/`CheckBox` instances. None of these creates a window handle, opens a form, or
  shows a popup. The equivalent test already exists and runs in the default apartment:
  `QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs:167-194`.

Tests must construct **no** `Form`, show **no** dialog, and take **no** dependency on a UI thread.
The `ThemeControlGroup.ApplyTheme(bool async)` overload (`ThemeControlGroup.cs:212-229`) routes
through `UiThread.Dispatcher` and **must not** be called; use the parameterless
`ApplyTheme()` overload only.

---

## 11. Determinism

| Concern | Finding | Requirement on tests |
| --- | --- | --- |
| Wall-clock time | None. No `DateTime`, `TimeProvider`, `Stopwatch`, or timer in this file. | No clock seam needed. |
| Randomness | None. | No seeded RNG needed. |
| Ambient state — **`SystemColors`** | **Present and material.** Lines 41-43, 61-63, 110-113, 137-139, 165-166, 218-220, 270-272, 286-289, 296-298, 308-310, and many more read `SystemColors.Control`, `SystemColors.ControlText`, `SystemColors.Window`, `SystemColors.ActiveCaptionText`. These resolve against the machine's active Windows theme and differ between a developer workstation and a CI runner. | Assertions **must** compare symbolically (`.Should().Be(SystemColors.Control)`), never against a literal ARGB value. Existing precedent: `QfcThemeHelperTests.cs:64`, `:90-93`, `:178`. |
| Ambient state — `Color.FromArgb` literals | Deterministic; `Color` equality holds. Precedent `QfcThemeHelperTests.cs:67`. | Literal assertion is safe for these only. |
| `Thread.Sleep` / `Task.Delay` / real waits | None in the file; **prohibited** in the new tests (`.claude/rules/general-unit-test.md` § Determinism Infrastructure; `BannedSymbols.txt`). | — |
| Temporary files, external services | None. | Prohibited by UT4. |
| Shared mutable state across tests | The eight `isAltHover` lambdas are cached in a compiler `<>c` singleton, but they are stateless. The `IList<Control>` inputs are caller-supplied per test. | Each test must build its own control lists in `Arrange`; do not share a `[ClassInitialize]` control instance, because `ApplyTheme` mutates `BackColor` and subscribes event handlers. |

---

## 12. Projected coverage

- The file has **two methods with zero internal branches** plus **eight single-line lambdas**. Every
  executable line in M1 is reached by a single successful call; every executable line in M2 by a
  single successful call; each lambda by one `ApplyTheme()` invocation on the corresponding group.
- Test cases 1, 19, and 21-28 alone execute every executable line in the file. The remaining 27
  cases add branch/scenario completeness (invalid-input, boundary, error-handling) required by UT2.
- **Projected line coverage: ~100% of the file's executable lines.** The residual denominator is
  `using` directives (1-10), namespace/class braces, blank lines, and the commented-out blocks at
  33/83/133/187 and 481-494 — none of which is instrumented.
- **Clears the 80% floor with a very wide margin.** The argument is structural rather than
  statistical: there is no branch in this file that a test cannot take, and no line that requires a
  host, a form, a COM object, or a UI thread.
- **This file does not require an exemption.** It should be classified `testable` in F1's ledger
  (`docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`), which remains the
  authority on that classification. If F1 has instead classified it `ratified-exempt`, F1's
  classification governs and this plan is dropped — but the evidence above indicates it should not
  be exempt, since the CLAUDE.md § UT2 exemption is scoped to code that depends on
  `Application`/`MailItem`/`Store`/`MAPIFolder` without a seam, or to form-derived/Designer code, and
  this file is neither.
- Numeric before/after per-file figures are produced by **F1's harness** (Cobertura output of
  `Invoke-MSTestWithCoverage.ps1`) at execution time and committed under
  `<FEATURE>/evidence/qa-gates/`.
