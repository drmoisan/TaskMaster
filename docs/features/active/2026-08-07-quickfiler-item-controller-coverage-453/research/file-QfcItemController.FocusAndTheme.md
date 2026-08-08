# Per-File Research: `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs`

- Feature: F10 `quickfiler-item-controller-coverage` (issue #453), epic #136
- Branch: `feature/quickfiler-item-controller-coverage`
- Worktree: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a359b62de7a79b16e`
- Production file: `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs` — **326 lines**, no
  `[ExcludeFromCodeCoverage]` attribute anywhere in the file (verified by full read).
- Primary test file: `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` (497 lines)
- Shared harness: `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` (365 lines)

---

## 0. Headline conclusions

1. **No seam is required and the STA last-resort clause is NOT needed for this file.** Every uncovered
   line is reachable with the existing reflection-injection harness plus the mockable `IItemViewer`,
   `IQfcTipsDetails`, `IUiDispatcher` and `UtilitiesCS.Theme` doubles that already exist in
   `QfcItemController.TestSupport.cs` and `QfcItemController.FocusAndThemeTests.cs`. The plan is
   **tests only, zero production change**.
2. **The measured starting point is worse than the brief states.** Recomputed from the per-line hit
   map, the file is at **176/237 = 74.3% line** and **40/68 = 58.8% branch**, not 75.6%/57.6%. Section 2
   explains why the emitted Cobertura `line-rate` is inflated and why the epic's "373 lines" figure for
   a 326-line file is a double count.
3. **Branch coverage is the harder gate here** (58.8% against a 75% floor), and it is branch-dense in
   exactly the places the brief predicted: the `_activeTheme.Contains("Dark")` theme selector is
   written six separate times and only the `false` arm is ever taken.
4. `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` is at **497 of the 500-line
   limit**. New tests for this file **must** go into new test files, each of which needs an explicit
   `<Compile Include=...>` entry in `QuickFiler.Test/QuickFiler.Test.csproj` (that project uses explicit
   includes, not globbing — verified at `QuickFiler.Test.csproj:58-128`).

---

## 1. Member inventory

All members are instance members of `internal partial class QfcItemController`
(`QfcItemController.FocusAndTheme.cs:25`). The partial declares **no** constructors, properties,
events, fields or nested types — only methods.

| # | Member | Lines | Accessibility | Returns |
| --- | --- | --- | --- | --- |
| M1 | `ToggleFocus(Enums.ToggleState desiredState)` | 27-67 | `public` | `void` |
| M2 | `ToggleFocusAsync(Enums.ToggleState desiredState)` | 69-81 | `public` | `Task` |
| M3 | `ToggleFocus()` | 83-123 | `public` | `void` |
| M4 | `ToggleFocusAsync()` | 125-136 | `public` | `Task` |
| M5 | `ToggleFocusOnAsync()` | 138-151 | `private` | `Task` |
| M6 | `ToggleFocusOffAsync()` | 153-166 | `private` | `Task` |
| M7 | `ToggleNavigation(bool async)` | 168-179 | `public` | `void` |
| M8 | `ToggleNavigation(bool async, Enums.ToggleState desiredState)` | 181-195 | `public` | `void` |
| M9 | `ToggleNavigationAsync(Enums.ToggleState desiredState)` | 197-200 | `public` | `Task` |
| M10 | `ToggleTips(bool async, Enums.ToggleState desiredState)` | 202-217 | `public` | `void` |
| M11 | `ToggleTipsAsync(Enums.ToggleState desiredState)` | 219-246 | `public` | `Task` |
| M12 | `InvokeBeginInvoke(bool async, System.Action action)` | 248-258 | `public` | `void` |
| M13 | `ToggleSaveAttachments()` | 260-266 | `public` | `void` (body entirely commented out) |
| M14 | `ToggleSaveCopyOfMail()` | 268-273 | `public` | `void` |
| M15 | `SetThemeDark(bool async)` | 275-287 | `public` | `void` |
| M16 | `HtmlDarkConverter(Enums.ToggleState desiredState)` | 289-301 | `public` | `void` |
| M17 | `SetThemeLight(bool async)` | 303-316 | `public` | `void` |
| M18 | `ApplyReadEmailFormat(object state)` | 318-324 | `public` | `void` (`TimerCallback` shape) |

Compiler-generated closures the coverage report attributes to this file: `<ToggleFocus>b__*_0`
(the `ToggleFocus()` lambda, lines 87-121), `<ToggleNavigation>b__*_0/_1/_2` (lines 170, 173, 177),
`<ToggleSaveCopyOfMail>b__*_0` (line 271), plus async state machines for M2, M4, M5, M6, M9, M11.

Collaborators referenced (all declared in `QfcItemController.cs`, F10-owned):
`_itemViewer` (`IItemViewer`, line 51), `_uiDispatcher` (`UtilitiesCS.Threading.IUiDispatcher`, line 66),
`_themes` (`Dictionary<string, UtilitiesCS.Theme>`, line 40), `_activeTheme` (line 52),
`_activeUI` (line 160), `_expanded` (line 146), `_itemPositionTips` (`IQfcTipsDetails`, line 50),
`_listTipsDetails`/`_listTipsExpanded` (lines 162, 171), `_tableLayoutPanels` (line 43),
`_isWebViewerInitialized` (line 37), `_mailActions` (`IMailItemActions`, line 68),
`ItemHelper` (`MailItemHelper`, line 135), `ConversationResolver` (line 110), `Token` (line 267).

---

## 2. Measured coverage baseline and a correction to the epic's numbers

Source of measurement: the committed report
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`,
class element at line 26058, `filename="QuickFiler\Controllers\QfcItemController.FocusAndTheme.cs"`.
The per-line map in that element aligns exactly with the current source on this branch (spot-checked at
`ApplyReadEmailFormat` 319-324, `ToggleSaveAttachments` 261/266, `ToggleTipsAsync` 220/223/228-246), so
the file content did not change between the two branches.

**Correction to epic.md.** The epic's baseline table lists this file at "Lines 373 / 75.6%". A
326-line file cannot have 373 coverable lines. Arithmetic on the report shows the emitted
`line-rate="0.756032"` is exactly `282/373`, where `373 = 237 (class-level <lines>) + 136 (sum of the
per-method <lines>)`. The tool emits each method's lines twice — once in `<methods>/<method>/<lines>`
and once in the class-level `<lines>` union — and the class `line-rate`/`branch-rate` are computed over
the concatenation. The same holds for branches: emitted `branch-rate="0.576087"` is exactly
`53/92 = (40 + 13)/(68 + 24)`.

**Authoritative distinct-line figures for this file (recomputed from the class-level `<lines>` union):**

| Metric | Covered | Total | Rate | Gate | Shortfall |
| --- | --- | --- | --- | --- | --- |
| Line | 176 | 237 | **74.26%** | >= 80% | **+14 lines** |
| Branch (conditions) | 40 | 68 | **58.82%** | >= 75% | **+11 conditions** |

Implications the plan must carry:
- F1's harness must key on the class-level `<lines>` block (already required by epic.md's directive
  "Aggregate per file, not per class"); this file is a second, independent confirmation that a naive
  read of `line-rate` overstates coverage — here by 1.3 points.
- The child must re-measure on its own branch. These figures are indicative evidence for planning only.
- The class-level `<lines>` list includes some non-executable lines (e.g. line 45, a comment, is listed
  with `hits="1"`). Because the same convention applies to numerator and denominator, the ratio is
  self-consistent; the plan should not attempt to normalise it.

---

## 3. Coverage status per member, with the covering test named

Legend: COVERED = every line and every condition hit; PARTIAL = some lines or conditions unhit;
UNCOVERED = no lines hit.

| # | Member | Status | Covering test(s) | Uncovered lines | Uncovered conditions |
| --- | --- | --- | --- | --- | --- |
| M1 | `ToggleFocus(state)` | PARTIAL | `FocusAndThemeTests.cs:189` `ToggleFocus_StateOverload_MarshalsThroughItemViewerInvoke`; `:210` `..._Off_FromActive_DeactivatesUiAndSwitchesToNormalTheme` | 37, 38, 39, 53, 54, 55 | L36 1/2, L48 3/4, L52 1/2 |
| M2 | `ToggleFocusAsync(state)` | PARTIAL | `SeamDispatcherTests.cs:223` `ToggleFocusAsync_StateOverload_WhenTurningOn_RegistersAndRoutesThemeThroughInjectedDispatcher` | 73, 74, 75 | L72 2/4 |
| M3 | `ToggleFocus()` | PARTIAL | `FocusAndThemeTests.cs:231` `ToggleFocus_ParameterlessOverload_MarshalsThroughItemViewerInvoke`; `:251` `..._FromActive_...` | 93, 94, 95, 109, 110, 111 | L92 1/2, L108 1/2 |
| M4 | `ToggleFocusAsync()` | PARTIAL | `SeamDispatcherTests.cs:269` `ToggleFocusAsync_ParameterlessOverload_WhenActive_RoutesToOffAndThemeThroughInjectedDispatcher` | 132, 133, 134 | L127 1/2 |
| M5 | `ToggleFocusOnAsync()` | PARTIAL | `FocusAndThemeTests.cs:274` `ToggleFocusOnAsync_ActivatesUiAndSwitchesToActiveTheme` | 142, 143, 144 | L141 1/2 |
| M6 | `ToggleFocusOffAsync()` | PARTIAL | `FocusAndThemeTests.cs:290` `ToggleFocusOffAsync_DeactivatesUiAndSwitchesToNormalTheme` | 157, 158, 159 | L156 1/2 |
| M7 | `ToggleNavigation(bool)` | PARTIAL | `FocusAndThemeTests.cs:310` `ToggleNavigation_Synchronous_TogglesPositionTips` | 172, 173, 174 | L171 1/2 |
| M8 | `ToggleNavigation(bool, state)` | PARTIAL | `FocusAndThemeTests.cs:327` `ToggleNavigation_WithState_TogglesPositionTipsWithState` | 184, 185, 186, 187, 188 | L183 1/2 |
| M9 | `ToggleNavigationAsync(state)` | COVERED | `FocusAndThemeTests.cs:344` `ToggleNavigationAsync_AwaitsPositionTipsToggleAsync` | — | — |
| M10 | `ToggleTips(bool, state)` | PARTIAL | `FocusAndThemeTests.cs:363` `ToggleTips_Synchronous_DispatchesAndExecutesDelegate` | 211, 212, 213 | L210 2/4, L212 0/2 |
| M11 | `ToggleTipsAsync(state)` | PARTIAL | `FocusAndThemeTests.cs:382` `ToggleTipsAsync_WithEmptyCollections_Completes` | 229, 230, 231, 237, 238, 239, 240, 241, 245 | L228 1/2, L236 2/4, L238 0/2 |
| M12 | `InvokeBeginInvoke` | COVERED | `FocusAndThemeTests.cs:399` `InvokeBeginInvoke_WhenAsync_UsesBeginInvoke`; `:415` `..._WhenSynchronous_UsesInvoke` | — | — |
| M13 | `ToggleSaveAttachments` | COVERED | `FocusAndThemeTests.cs:433` `ToggleSaveAttachments_DoesNotThrow` | — | — |
| M14 | `ToggleSaveCopyOfMail` | COVERED | `SeamDispatcherTests.cs:143` `ToggleSaveCopyOfMail_TogglesEmailCopyThroughDispatcher` | — | — |
| M15 | `SetThemeDark(bool)` | PARTIAL | `FocusAndThemeTests.cs:448` `SetThemeDark_FromNormal_SelectsDarkNormalTheme` | 283, 284, 285, 286 | L277 2/4 |
| M16 | `HtmlDarkConverter(state)` | PARTIAL (guard only) | `FocusAndThemeTests.cs:483` `HtmlDarkConverter_WhenWebViewNotInitialized_DoesNotNavigate` | 292-300 (9 lines) | L291 1/2, L294 0/2 |
| M17 | `SetThemeLight(bool)` | PARTIAL | `FocusAndThemeTests.cs:465` `SetThemeLight_FromNormal_SelectsLightNormalTheme` | 311, 312, 313, 314 | L305 2/4 |
| M18 | `ApplyReadEmailFormat(object)` | COVERED | `SeamDispatcherTests.cs:316` `ApplyReadEmailFormat_MarksMailReadFalseAndRoutesThemeThroughInjectedDispatcherBeginInvoke` | — | — |

Total: 61 uncovered lines, 28 uncovered conditions.

### 3.1 A stale claim in the existing test file

`QfcItemController.FocusAndThemeTests.cs:20-24` states that "Members that unconditionally await the
out-of-scope `Theme.SetQfcThemeAsync()` (the two `ToggleFocusAsync` overloads) retain a per-member
bucket-(iii) exemption and are excluded here." That claim is **no longer true**: cycle-3 P10-T32/T33
added `SeamDispatcherTests.cs:223` and `:269`, which exercise both overloads through
`QfcItemControllerTestSupport.BuildDispatchableTheme`, and the coverage map confirms lines 70-81 and
126-136 are hit. The doc comment should be corrected as part of this child (documentation only, no
behavior change).

---

## 4. Shared test harness — what to reuse, not re-create

`QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` provides the following. New tests must
consume these rather than duplicating reflection boilerplate.

| Helper | Line | Purpose |
| --- | --- | --- |
| `internal sealed class HarnessController : QfcItemController` | 25-29 | Exposes the protected parameterless ctor. `QfcItemController` is `internal` and `QuickFiler` grants `InternalsVisibleTo("QuickFiler.Test")` (`QuickFiler/Properties/AssemblyInfo.cs:5`). |
| `SetField(controller, name, value)` | 37-47 | Private-field injection on `QfcItemController`. |
| `GetField(controller, name)` | 49-59 | Private-field read-back for assertions. |
| `InvokeNonPublic(controller, name, args)` | 66-80 | Calls a private instance method (used for `ToggleFocusOnAsync`/`ToggleFocusOffAsync`). |
| `EnsureSynchronizationContext()` | 87-93 | Non-null ambient `SynchronizationContext` without a WinForms context. |
| `BuildSyncDispatcher()` | 102-137 | `Mock<IUiDispatcher>` whose `Invoke`/`InvokeAsync`/`BeginInvoke` run the delegate synchronously. |
| `InjectThemes(controller, themes, activeTheme)` | 143-151 | Sets `_themes` + `_activeTheme`. |
| `BuildColorTheme(mouseOver, clicked, back)` | 166-178 | Handle-less `UtilitiesCS.Theme` carrying the three button colours, with a non-executing `_uiDispatcher` injected. |
| `BuildThemeDictionary(activeTheme, theme)` | 184-192 | Single-entry `_themes`. |
| `BuildDispatchableTheme(dispatcher)` | 201-211 | Handle-less `Theme` with an injected `IUiDispatcher` and a handle-less `Label` in `_lblSender`. Required by any test that lets `SetQfcThemeAsync`/`SetMailRead` actually run. |
| `EnsureUiThreadDispatcher()` | 238-249 | Seeds the static `UiThread._dispatcher` with a parked, never-pumped dispatcher so fire-and-forget `async: true` work is enqueued and never executes. |
| `StartRunningDispatcher()` / `ShutdownDispatcher()` | 297-326 | A real running STA dispatcher on a dedicated background thread, for members that dispatch through a real `UiDispatcher`. |

`QfcItemController.FocusAndThemeTests.cs` adds three more file-local helpers that new theme tests
should reuse by promoting them into `QfcItemController.TestSupport.cs` (see the file-size constraint in
§9) rather than copying:

| Helper | Line | Purpose |
| --- | --- | --- |
| `BuildAllThemes()` | 41-55 | Four-key `_themes` dictionary (`LightNormal`, `LightActive`, `DarkNormal`, `DarkActive`) sharing one `BuildColorTheme` instance. Already contains the Dark keys the new tests need. |
| `BuildFocusController()` | 85-97 | Controller wired with `BuildAllThemes()`, `_activeTheme = "LightNormal"`, empty tips lists, an `IQfcKeyboardHandler` stub with real `KbdActions<>` collections, and a `MailItemHelper` with an entry id. |
| `BuildExecutingViewer()` | 99-115 | `Mock<IItemViewer>` whose `Invoke`/`BeginInvoke` execute the delegate synchronously. |
| `EnableHandlelessThemeInvoke(controller)` | 136-158 | Reflection-injects 16 handle-less doubles into every `Theme` in `_themes` so the terminal `SetQfcTheme(async: false)` runs without a live window handle. |

---

## 5. Seam analysis

The seam question is settled per member. **No new seam is required.**

| # | Member | What could block a deterministic test | Verdict |
| --- | --- | --- | --- |
| M1, M3 | `ToggleFocus` overloads | `_itemViewer.Invoke` marshalling | Already seamed: `IItemViewer.Invoke(Delegate)`; `BuildExecutingViewer()` executes it inline. Terminal `Theme.SetQfcTheme(async: false)` needs handle-less doubles — `EnableHandlelessThemeInvoke` already provides them. **Nothing blocking.** |
| M2, M4 | `ToggleFocusAsync` overloads | `Theme.SetQfcThemeAsync()` | Already seamed: `Theme._uiDispatcher` is reflection-injected by `BuildDispatchableTheme`. **Nothing blocking.** |
| M5, M6 | `ToggleFocusOnAsync` / `OffAsync` | private access | `InvokeNonPublic`. **Nothing blocking.** |
| M7, M8 | `ToggleNavigation` overloads | `_itemViewer.BeginInvoke` | `BuildExecutingViewer()` already stubs `BeginInvoke`. **Nothing blocking.** |
| M10, M11 | `ToggleTips` / `ToggleTipsAsync` | `_tableLayoutPanels.ForEach(x => x.SuspendLayout())` touches real `TableLayoutPanel` objects; tips are `IQfcTipsDetails` | `_tableLayoutPanels` is `IList<TableLayoutPanel>`; existing tests inject an **empty** `List<TableLayoutPanel>`, so no control is constructed. Tips are mocked via `Mock<IQfcTipsDetails>`. **Nothing blocking** — provided the list stays empty. Do NOT construct a `TableLayoutPanel` to "cover" the `SuspendLayout` call; that would drag in the STA clause for two lines that are already hit (208, 214 both 2/2). |
| M15, M17 | `SetThemeDark` / `SetThemeLight` | `Theme.SetQfcTheme(async)` | Existing tests pass `async: true` with `EnsureUiThreadDispatcher()`, so the theme work is enqueued on the parked dispatcher and never executes. The observable effect is the `_activeTheme` switch. **Nothing blocking.** |
| M16 | `HtmlDarkConverter` | `_isWebViewerInitialized` (private bool), `_itemViewer.NavigateToString`, `ItemHelper.ToggleDark`, `ConversationResolver.Count` / `.ConversationInfo` | All reachable: `_isWebViewerInitialized` via `SetField`; `NavigateToString(string)` is on `IItemViewer` (`QuickFiler/Viewers/IItemViewer.cs:107`); `MailItemHelper.ToggleDark(Enums.ToggleState)` (`UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.Html.cs:171`) is pure regex over the `Html` string and `InitializeSafeDefaults()` sets `_html = string.Empty.ToLazy()` (`MailItemHelper.cs:193`), so a default-constructed helper never touches COM; `ConversationResolver.Count` has an `internal set` (`ConversationResolver.Loading.cs:270`) and `ConversationInfo` a public set (`:30-34`), both already used by existing tests. **Nothing blocking.** |
| M18 | `ApplyReadEmailFormat` | it is a `TimerCallback`; `_emailIsReadTimer` lives in `QfcItemController.cs:53` and is started by another partial | Already covered by calling the method directly. Tests must never start the timer. |

### 5.1 Why the STA last-resort clause does not apply

epic.md §"Shared Design" 3 permits never-shown WinForms controls on an STA thread only where no seam
can isolate the logic. In this file every UI touch is already behind one of three existing seams
(`IItemViewer`, `IUiDispatcher`, `IQfcTipsDetails`), and the one remaining concrete-type dependency
(`IList<TableLayoutPanel>`) is satisfied by an empty list. **No `*.StaTests.cs` file should be created
for this file.** If a later remediation cycle finds an STA argument for a specific member, it must
re-open this section with a per-member justification.

### 5.2 Sibling boundaries — dependencies recorded, no edits proposed

- **`UtilitiesCS.Theme` is not an F4 file.** The `_themes` dictionary is
  `Dictionary<string, UtilitiesCS.Theme>` (`QfcItemController.cs:40`), and `Theme` is declared in
  `UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs:16` and `Theme.Rendering.cs:7` — outside QuickFiler
  and outside epic #136's denominator entirely. This corrects an assumption implicit in the brief.
- **`QuickFiler/Helper Classes/QfcThemeHelper.cs` (F4 / #434) is not referenced by this file.** It is
  the factory that builds the dictionary (`QfcThemeHelper.SetupThemes`, `QfcThemeHelper.cs:36`, `:96`),
  and it is called from `QfcItemController.Initialization.cs:175`, `:209`, `:267` — an F10-owned file.
  **No edit to `QfcThemeHelper.cs` or `QfcThemeControlSet.cs` is required or proposed.**
- **`ConversationResolver` (F4 / #434)** is read by `HtmlDarkConverter` at lines 294 and 296. Tests must
  depend on its **current positional constructor** `ConversationResolver(IApplicationGlobals, MailItem)`
  (`ConversationResolver.cs:64`), exactly as `MailActionsTests.cs:101` and `SeamFactoryTests.cs:42`
  already do. Cross-child contract note: if F4 changes that constructor or the `Count` /
  `ConversationInfo` setters, F10's `HtmlDarkConverter` tests break at fan-in. No upstream change is
  needed for F10 to succeed.
- **F3 (#430)** owns `KeyboardHandler.cs` / `IMailItemActions.cs` / `MailItemActionsAdapter.cs`. This
  file touches `_mailActions` only in `ApplyReadEmailFormat` (lines 322-323), which is already covered
  through `Mock<IMailItemActions>`. No change requested of F3.
- **F5** owns `IQfcDatamodel`; this file has no dependency on it.
- **`EnableHandlelessThemeInvoke`** (`FocusAndThemeTests.cs:136-158`) reflection-injects 16 private
  fields of `UtilitiesCS.Theme`. This is an existing, fragile coupling to another assembly's internals.
  New tests must **reuse** that helper rather than adding further `typeof(Theme).GetField(...)` calls,
  so the coupling surface does not grow.

---

## 6. State-transition invariants and the tests that pin them

| ID | Invariant | Evidence | Pinning test |
| --- | --- | --- | --- |
| I1 | `_activeTheme` is always one of `{LightNormal, LightActive, DarkNormal, DarkActive}`, and a focus toggle **preserves the Light/Dark dimension** while flipping the Normal/Active dimension. Light never becomes Dark through a focus toggle. | 36-43, 52-59, 92-99, 108-115, 141-148, 156-163 | FT-01, FT-02, FT-04, FT-05, FT-08, FT-09 |
| I2 | `_activeUI == true` iff `_activeTheme` ends in `Active` after any focus toggle completes. | 35+38/42, 51+54/58, 89+93/97, 107+110/114 | FT-01, FT-02, FT-04, FT-05 |
| I3 | Ordering inside a focus toggle: state mutation (`_activeUI`, `_activeTheme`) -> `ToggleTips` -> `Register`/`UnregisterFocusAsyncActions` -> theme application. The theme application is **last and unconditional**. | 35-46 then 64; 51-62 then 64 | FT-01, FT-02 (assert `_activeTheme` before the terminal `SetQfcTheme`, and that `Invoke` fires exactly twice) |
| I4 | Idempotency: `ToggleFocus(On)` when already active, and `ToggleFocus(Off)` when already inactive, mutate no state but still re-apply the theme. | 32/48 both false -> falls through to 64 | FT-03 |
| I5 | `SetThemeDark`/`SetThemeLight` preserve the Normal/Active dimension and treat a null `_activeTheme` as Normal; neither mutates `_activeUI`. | 277-286, 305-314 | FT-18, FT-19 |
| I6 | `ToggleTips` toggles the detail tips unconditionally and the expanded tips only when `_expanded` or `ToggleState.Force` is set. `Force` is `2` in a `[Flags]` enum (`UtilitiesCS/Interfaces/Enums.cs:7-13`), so `On \| Force` is a valid combination. | 209-213, 236-241 | FT-12, FT-13, FT-15, FT-16 |
| I7 | `ToggleTips` (sync) brackets the toggles with `SuspendLayout` ... `ResumeLayout` over every panel in `_tableLayoutPanels`. `ToggleTipsAsync` does **not** do this — an intentional asymmetry to record, not to fix. | 208 vs 214; 219-246 has no suspend/resume | FT-12 (ordering assertion via an ordered mock over a stub panel list is not possible with concrete `TableLayoutPanel`; assert only that the empty-list path completes) |
| I8 | `ToggleTipsAsync` observes cancellation before touching any tip. | 223 | FT-17 |
| I9 | `HtmlDarkConverter` is a no-op unless `_isWebViewerInitialized`, and the per-conversation-item dark toggle runs only when `ConversationResolver.Count.Expanded > 0`. | 291, 294 | existing `HtmlDarkConverter_WhenWebViewNotInitialized_DoesNotNavigate`; FT-20, FT-21 |
| I10 | `InvokeBeginInvoke(async: true)` uses `BeginInvoke` and never `Invoke`, and vice versa. | 250-257 | already pinned (`InvokeBeginInvoke_WhenAsync_UsesBeginInvoke`, `..._WhenSynchronous_UsesInvoke`) |

No dispose/teardown guard exists in this file; "action-after-dispose" is not an invariant this partial
holds (disposal lives in other partials of the family).

---

## 7. Determinism requirements

Verified by full read of the file:

- **No wall-clock read.** No `DateTime.Now`, `DateTime.UtcNow`, `DateTimeOffset`, `Stopwatch`, or
  `Environment.TickCount` anywhere in the file.
- **No randomness.** No `Random`, `Random.Shared`, or `Guid.NewGuid`.
- **No thread-pool or timer work originates here.** `ApplyReadEmailFormat(object state)` has the
  `TimerCallback` shape and is the callback for `_emailIsReadTimer` (`QfcItemController.cs:53`), but the
  timer is created and scheduled by another partial. Tests call the method directly; they must never
  construct or start the timer.
- **No `Thread.Sleep` / `Task.Delay` / real waits.** The only `await`s are on `IQfcTipsDetails.ToggleAsync`
  and `Theme.SetQfcThemeAsync`, both mockable/dispatcher-routed.
- **UI-thread marshalling** occurs at lines 29, 85, 170, 173, 177, 185, 191, 204 (`IItemViewer.Invoke`/
  `BeginInvoke`) and line 270 (`IUiDispatcher.Invoke`). Both are interfaces already stubbed
  synchronously by existing helpers. No test may rely on a real message pump.
- **No banned-API finding to report** for this file. (`.claude/rules/general-unit-test.md`
  "Determinism Infrastructure" bans `Thread.Sleep`/`Task.Delay`/`Date.Now` in **tests**; the new tests
  proposed in §8 use none of them, and `QfcItemControllerTestSupport.StartRunningDispatcher`
  /`EnsureUiThreadDispatcher` provide deterministic completion without polling.)

---

## 8. Proposed test cases

Each row is one atomic task. "Lines" and "Conds" are the projected first-time-covered counts against
the §2 baseline (176/237 line, 40/68 branch). All tests are MSTest `[TestMethod]`, Arrange-Act-Assert,
Moq doubles, FluentAssertions, no temporary files, no live forms, no popups.

### Tier A — required to clear both gates (no production change)

| ID | Target | Scenario | Fixture | Lines | Conds |
| --- | --- | --- | --- | --- | --- |
| FT-01 | M1 `ToggleFocus(On)` | positive / dark theme: from `_activeUI=false`, `_activeTheme="DarkNormal"` -> `"DarkActive"`, `_activeUI=true` | `BuildFocusController` + `BuildExecutingViewer` + `EnableHandlelessThemeInvoke`; empty `_tableLayoutPanels` | 37, 38, 39 (3) | L36 (1) |
| FT-02 | M1 `ToggleFocus(Off)` | positive / dark theme: from `_activeUI=true`, `"DarkActive"` -> `"DarkNormal"`, `_activeUI=false` | same | 53, 54, 55 (3) | L52 (1) |
| FT-03 | M1 `ToggleFocus(On)` | idempotency (I4): already active -> no state change, theme still re-applied, `ToggleTips` not invoked (`Invoke` fires exactly once) | same | 0 | L48 (1) |
| FT-04 | M3 `ToggleFocus()` | positive / dark theme: from active `"DarkActive"` -> `"DarkNormal"` | same | 93, 94, 95 (3) | L92 (1) |
| FT-05 | M3 `ToggleFocus()` | positive / dark theme: from inactive `"DarkNormal"` -> `"DarkActive"` | same | 109, 110, 111 (3) | L108 (1) |
| FT-08 | M5 `ToggleFocusOnAsync` | positive / dark theme: `"DarkNormal"` -> `"DarkActive"` | `BuildFocusController` + `InvokeNonPublic` | 142, 143, 144 (3) | L141 (1) |
| FT-09 | M6 `ToggleFocusOffAsync` | positive / dark theme: `"DarkActive"` -> `"DarkNormal"` | same | 157, 158, 159 (3) | L156 (1) |
| FT-10 | M7 `ToggleNavigation(async: true)` | positive: routes through `BeginInvoke`; assert `BeginInvoke` fires **exactly twice** and `Invoke` never (documents defect D-1 without changing behavior) | `Mock<IQfcTipsDetails>` + `BuildExecutingViewer` | 172, 173, 174 (3) | L171 (1) |
| FT-11 | M8 `ToggleNavigation(async: true, On)` | positive: `BeginInvoke` with the supplied state | same | 184-188 (5) | L183 (1) |
| FT-18 | M15 `SetThemeDark(async: true)` | edge: `_activeTheme="LightActive"` (neither null nor `Normal`) -> `"DarkActive"`; `_activeUI` unchanged | `BuildAllThemes` + `EnsureUiThreadDispatcher` | 283-286 (4) | L277 (2) |
| FT-19 | M17 `SetThemeLight(async: true)` | edge: `_activeTheme="DarkActive"` -> `"LightActive"`; `_activeUI` unchanged | same | 311-314 (4) | L305 (2) |

**Tier A projection: 176 + 34 = 210/237 = 88.6% line; 40 + 13 = 53/68 = 77.9% branch.** Both gates clear
with margin, with zero production edits.

### Tier B — recommended, closes the remaining gaps (still no production change)

| ID | Target | Scenario | Fixture | Lines | Conds |
| --- | --- | --- | --- | --- | --- |
| FT-06 | M2 `ToggleFocusAsync(Off)` | positive: `_activeUI=true` + `Off` routes to `ToggleFocusOffAsync` and awaits `SetQfcThemeAsync` | `BuildDispatchableTheme` pattern from `SeamDispatcherTests.cs:223` | 73, 74, 75 (3) | L72 (2) |
| FT-07 | M4 `ToggleFocusAsync()` | positive: `_activeUI=false` routes to `ToggleFocusOnAsync` | same | 132, 133, 134 (3) | L127 (1) |
| FT-12 | M10 `ToggleTips(false, On)` | positive: `_expanded=true` + one-element `_listTipsExpanded` -> expanded tip toggled with `shareColumn:false` | `BuildExecutingViewer`, `Mock<IQfcTipsDetails>`, empty `_tableLayoutPanels` | 211, 212, 213 (3) | L210 (1), L212 (2) |
| FT-13 | M10 `ToggleTips(false, On\|Force)` | edge: `_expanded=false` but `Force` set -> expanded tips still toggled | same | 0 | L210 (1) |
| FT-14 | M11 `ToggleTipsAsync(On)` | positive: one-element `_listTipsDetails` -> `ToggleAsync(On, false)` awaited once | `Mock<IQfcTipsDetails>` returning `Task.CompletedTask` | 229, 230, 231 (3) | L228 (1) |
| FT-15 | M11 `ToggleTipsAsync(On)` | positive: `_expanded=true` + one-element `_listTipsExpanded` | same | 237-241, 245 (6) | L236 (1), L238 (2) |
| FT-16 | M11 `ToggleTipsAsync(On\|Force)` | edge: `_expanded=false` + `Force` | same | 0 | L236 (1) |
| FT-17 | M11 `ToggleTipsAsync` | error: pre-cancelled `Token` -> throws `OperationCanceledException` before any tip is toggled (I8) | `CancellationToken(canceled: true)` via `Token` setter | 0 | 0 |
| FT-20 | M16 `HtmlDarkConverter(On)` | positive: `_isWebViewerInitialized=true`, `Count.Expanded == 0` -> `NavigateToString` called once, no per-item toggle | `SetField("_isWebViewerInitialized", true)`, default `MailItemHelper`, `ConversationResolver` with `Count = new Pair<int>(0, 0)` | 292, 293, 294, 300 (4) | L291 (1), L294 (1) |
| FT-21 | M16 `HtmlDarkConverter(On)` | positive: `Count.Expanded == 1` with a one-element `ConversationInfo.Expanded` -> that item's `ToggleDark` applied | as above plus `ConversationInfo = new Pair<List<MailItemHelper>>(...)` | 295-299 (5) | L294 (1) |

**Tier A + B projection: 237/237 = 100% line; 68/68 = 100% branch** on the class-level map.

### Sequencing note for the plan

Tier A alone satisfies epic AC1 (>= 80% line) and the 75% branch floor. Tier B is what makes the file a
clean hand-off to the capstone. Recommend planning all 21 as separate atomic tasks and gating the phase
exit on the measured Tier-A threshold, so a Tier-B failure does not block the child.

---

## 9. File-size and file-creation impact

| File | Current | Limit | Change proposed | Projected |
| --- | --- | --- | --- | --- |
| `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs` | 326 | 500 | **none** | 326 |
| `QuickFiler/QuickFiler.csproj` | — | — | **no edit** (no new production file) | — |
| `QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs` | **497** | 500 | **must not grow** | 497 |
| `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` | 365 | 500 | promote `BuildAllThemes`, `BuildFocusController`, `BuildExecutingViewer`, `EnableHandlelessThemeInvoke` (~90 lines) so new files can share them | ~455 |

**Consequences the plan must encode.**

1. `QfcItemController.FocusAndThemeTests.cs` has three lines of headroom. All 21 new tests go into new
   files. Suggested split, each comfortably under 500 lines:
   - `QuickFiler.Test/Controllers/QfcItemController.FocusThemeSelectionTests.cs` — FT-01..FT-09, FT-18, FT-19
   - `QuickFiler.Test/Controllers/QfcItemController.FocusTipsTests.cs` — FT-12..FT-17
   - `QuickFiler.Test/Controllers/QfcItemController.HtmlDarkConverterTests.cs` — FT-20, FT-21
2. **Every new test file needs an explicit `<Compile Include="Controllers\....cs" />` entry in
   `QuickFiler.Test/QuickFiler.Test.csproj`.** That project uses explicit includes with no globbing
   (`QuickFiler.Test.csproj:58-128`). This is an obligation the brief did not mention — it named only
   `QuickFiler.csproj`.
3. **CRLF preservation** applies to both csproj files. Use the Edit tool or `perl -0777` with explicit
   `\r\n`; never a git-bash `sed -i`, which strips CRLF and produces a whole-file diff that is
   guaranteed to conflict at fan-in (epic.md, "Cross-Child Constraints" 1).
4. **Ledger rows.** Since no new *production* file is created for this file's work, epic.md's
   "Mid-Wave File Creation" rule does not fire here. If the promotion of shared helpers into
   `QfcItemController.TestSupport.cs` were instead done by creating a new *test* file, that still
   creates no ledger row — the ledger denominator is the `QuickFiler.csproj` compile set, not
   `QuickFiler.Test.csproj`.
5. Keep the csproj edit to a single minimal adjacent hunk near the existing `Controllers\QfcItemController*`
   entries to minimise fan-in conflict surface.

---

## 10. Latent defects for promotion

Report only; do **not** fix under this child (epic NFR: no behavior change). Promote via the MCP
promotion lifecycle per epic.md "Latent Defect Promotion".

| ID | Location | Description | Severity |
| --- | --- | --- | --- |
| D-1 | `QfcItemController.FocusAndTheme.cs:170` | `ToggleNavigation(bool async)` toggles `_itemPositionTips.Toggle(false)` **unconditionally at line 170**, then toggles it a second time in the `if/else` at 173 or 177. `IQfcTipsDetails.Toggle(bool)` is a flip (`UtilitiesCS/Interfaces/IQuickFiler/IQfcTipsDetails.cs:15`), so a single call to this overload produces **no net visibility change**. The existing test `FocusAndThemeTests.cs:310` uses `Times.AtLeastOnce()`, which masks it. | **Medium** — functional: the navigation tips never toggle through this overload. |
| D-2 | `QfcItemController.FocusAndTheme.cs:318-324` | `ApplyReadEmailFormat` writes the unread state twice: `ItemHelper.UnRead = false` (whose setter forwards to `Item.UnRead` + `Item.Save()`, per the comment at `SeamDispatcherTests.cs:335-337`) and then `_mailActions.UnRead = false; _mailActions.Save();`. Two COM writes and two saves per read-timer tick. | Low — redundant COM work. |
| D-3 | `QfcItemController.FocusAndTheme.cs:277`, `:305` | `SetThemeDark`/`SetThemeLight` index `_themes[key]` without a containment check; a `_themes` dictionary missing the requested key throws `KeyNotFoundException` at the UI boundary rather than failing with context. | Low. |
| D-4 | `QfcItemController.FocusAndTheme.cs:36-43`, `52-59`, `92-99`, `108-115`, `141-148`, `156-163` | The Light/Dark + Normal/Active theme state is encoded in a single `string` and the `Contains("Dark")` selector is duplicated **six** times. `M1`/`M3` are near-identical bodies, as are `M5`/`M6`. A two-field or enum-pair representation would remove the duplication and make I1/I2 structurally enforced instead of test-enforced. | Low — maintainability / invariant risk, not a live defect. |
| D-5 | `QfcItemController.FocusAndTheme.cs:1-21` | 21 `using` directives of which several are unused in this partial (`System.IO`, `System.Net.NetworkInformation`, `System.ComponentModel`, `TaskVisualization`, `ToDoModel`, `Microsoft.Web.WebView2.Core`), duplicated verbatim across every partial of the family. | Low — analyzer noise. |

---

## 11. Rejected alternatives

- **Construct real `TableLayoutPanel`/`Label` controls on an STA thread to cover `ToggleTips`'s
  `SuspendLayout`/`ResumeLayout` calls.** Rejected: those lines (208, 214) are already covered at 2/2
  conditions with an empty `IList<TableLayoutPanel>`, so the STA cost buys nothing, and epic.md
  §"Shared Design" 3 permits STA only as a last resort where no seam isolates the logic.
- **Introduce an `IThemeApplier` interface seam over `UtilitiesCS.Theme` so tests need not
  reflection-inject 16 private fields.** Rejected for this child: it is a cross-assembly contract change
  to `UtilitiesCS`, outside every epic child's file assignment, and it is unnecessary — `Theme` already
  exposes an injectable `_uiDispatcher` that `BuildDispatchableTheme` uses, and the existing
  `EnableHandlelessThemeInvoke` helper already makes the synchronous path work. Worth recording as a
  candidate for the separate VSTO-migration effort, not for #136.
- **Add a `Func<string, string>` seam around `ItemHelper.ToggleDark` for `HtmlDarkConverter`.**
  Rejected: `MailItemHelper.ToggleDark` is already a pure string transform on a lazily-defaulted empty
  `Html` (`MailItemHelper.cs:193`), so it is deterministic without a seam.
