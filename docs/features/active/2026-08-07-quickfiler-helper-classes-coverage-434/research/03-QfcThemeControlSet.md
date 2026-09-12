# F4 Per-File Research — `QfcThemeControlSet.cs`

Timestamp: 2026-08-07T22-40

Feature: `quickfiler-helper-classes-coverage` (issue #434), child F4 of epic
`quickfiler-per-file-coverage` (issue #136), wave 1, complexity band C3.

Scope of this artifact: exactly one production file, per the #136 one-file-at-a-time mandate.

---

## 1. File facts

| Fact | Value | Evidence |
| --- | --- | --- |
| Path | `QuickFiler/Helper Classes/QfcThemeControlSet.cs` | — |
| Line count | 110 (last content line is `}` at line 110) | Full Read; EOF after 110 |
| Compiled | Yes | `QuickFiler/QuickFiler.csproj:350` — `<Compile Include="Helper Classes\QfcThemeControlSet.cs" />` |
| `[ExcludeFromCodeCoverage]` | **Absent** — confirmed | Repo grep for `ExcludeFromCodeCoverage` across `QuickFiler/Helper Classes/` returned **no matches** |
| Namespace / type | `QuickFiler` / `internal sealed class QfcThemeControlSet` | `QfcThemeControlSet.cs:10`, `:12` |
| Internals visible to tests | Yes | `QuickFiler/Properties/AssemblyInfo.cs:5` |

Numeric baseline line coverage is captured at execution time with F1's per-file coverage harness
(epic `Shared Design` §6) and recorded under `<FEATURE>/evidence/qa-gates/`.

---

## 2. Member inventory (the coverage denominator)

A sealed, immutable value object: one constructor, nineteen get-only auto-properties, one private
generic helper. No fields with logic, no events, no methods beyond the helper.

### M1 — Constructor (lines 14–58)

`internal QfcThemeControlSet(Label lblItemNumber, Label lblSender, Label lblSubject, IList<TableLayoutPanel> tableLayoutPanels, IList<Button> buttons, IList<Component> menuItems, MenuStrip menuStrip, IList<IQfcTipsDetails> tipsDetailsLabels, IList<IQfcTipsDetails> tipsExpanded, TextBox textboxSearch, TextBox textboxBody, WebView2 breadcrumbWebView2, Action<string> breadcrumbThemeNotifier, FastObjectListView topicThread, WebView2 webView2, Control viewer, Func<bool> mailRead, Action<Enums.ToggleState> htmlConverter, IUiDispatcher uiDispatcher)`

**19 parameters, 19 null guards, 0 other logic.** Decision points: **14 inline `??`-throw
operators** plus **5 delegations to `RequireCollection`** (each contributing one `if`), for a
total of **19 decision points**.

| Parameter | Guard mechanism | Line |
| --- | --- | --- |
| `lblItemNumber` | `?? throw` | 36 |
| `lblSender` | `?? throw` | 37 |
| `lblSubject` | `?? throw` | 38 |
| `tableLayoutPanels` | `RequireCollection` | 39 |
| `buttons` | `RequireCollection` | 40 |
| `menuItems` | `RequireCollection` | 41 |
| `menuStrip` | `?? throw` | 42 |
| `tipsDetailsLabels` | `RequireCollection` | 43 |
| `tipsExpanded` | `RequireCollection` | 44 |
| `textboxSearch` | `?? throw` | 45 |
| `textboxBody` | `?? throw` | 46 |
| `breadcrumbWebView2` | `?? throw` (statement wrapped over 47–48) | 47–48 |
| `breadcrumbThemeNotifier` | `?? throw` (statement wrapped over 49–51) | 49–51 |
| `topicThread` | `?? throw` | 52 |
| `webView2` | `?? throw` | 53 |
| `viewer` | `?? throw` | 54 |
| `mailRead` | `?? throw` | 55 |
| `htmlConverter` | `?? throw` | 56 |
| `uiDispatcher` | `?? throw` | 57 |

### M2..M20 — Nineteen get-only auto-properties (lines 60–98)

`LblItemNumber` (60), `LblSender` (62), `LblSubject` (64), `TableLayoutPanels` (66), `Buttons` (68),
`MenuItems` (70), `MenuStrip` (72), `TipsDetailsLabels` (74), `TipsExpanded` (76), `TextboxSearch`
(78), `TextboxBody` (80), `BreadcrumbWebView2` (84), `BreadcrumbThemeNotifier` (86), `TopicThread`
(88), `WebView2` (90), `Viewer` (92), `MailRead` (94), `HtmlConverter` (96), `UiDispatcher` (98).
Each is a compiler-generated getter: 1 executable line, **0 decision points**.

### M21 — `RequireCollection` (lines 100–108)

`private static IList<T> RequireCollection<T>(IList<T> value, string parameterName)` —
**1 decision point** (`if (value is null)` @102). Note that it validates **null only**; an **empty**
collection is accepted and returned (line 107).

**Total executable surface: 1 constructor + 19 property getters + 1 generic helper = 21 members,
20 decision points.**

### Verified facts

- **The type is a pure value object.** It reads no control property, calls no method on any control,
  invokes none of its three injected delegates, and performs no I/O. Its entire behaviour is
  "validate 19 references, store 19 references".
- **Line 82-83 is an explanatory comment**, not code: it records that the folder control is the
  WebView2 breadcrumb and that `BreadcrumbThemeNotifier` posts the `themeChange` bridge message
  through the viewer's coordinator (issue #351).
- **`RequireCollection` is only reachable from M1.** It is `private static`.

---

## 3. Existing test inventory

**There is no dedicated test file for this type.** It is exercised incidentally from
`QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs` (registered at
`QuickFiler.Test/QuickFiler.Test.csproj:161`).

- `SearchScope:` `QuickFiler.Test/**` and repository-wide across `*.cs`.
- `SearchPatterns:` `QfcThemeControlSet`.
- `SearchResult:` `QuickFiler/Helper Classes/QfcThemeControlSet.cs:12,14`;
  `QuickFiler/Helper Classes/QfcThemeHelper.cs:57,73,96,299`;
  `QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs:47,60,84,145,197,296,308`;
  `QuickFiler/QuickFiler.csproj:350`. **No other file in the repository.**

| Test method (file:line) | Production member(s) exercised |
| --- | --- |
| `CreateControlSet(...)` private helper (`QfcThemeHelperTests.cs:296-329`) | M1 success path — all 19 assignment lines. Not a `[TestMethod]` itself; invoked from the four tests below. |
| `SetupThemes_WithControlSet_ReturnsFourExpectedThemeKeys` (`:45`) | M1 success path; **all 19 getters M2..M20** (read by `QfcThemeHelper.CreateTheme`, `QfcThemeHelper.cs:329-371`) |
| `SetupThemes_WithControlSet_MapsRepresentativeColorsAndHtmlStates` (`:58`) | as above |
| `SetupThemes_LightThemes_MailLabelColorsAreDarkTextOnLightBackground` (`:81`) | as above |
| `BuildProductionControlSet_MapsControllerAndViewerInputs` (`:133`) | M1 success path via `QfcThemeHelper.BuildProductionControlSet`; getters `TableLayoutPanels`, `Buttons`, `TipsDetailsLabels`, `TipsExpanded`, `MenuItems`, `MenuStrip`, `Viewer`, `UiDispatcher`, `MailRead`, `HtmlConverter` |
| `QfcThemeControlSet_NullRequiredCollection_ThrowsArgumentNullException` (`:197-203`) | **M21 `RequireCollection` null branch (102-105)** and M1 line 39; asserts `WithParameterName("tableLayoutPanels")` |

---

## 4. Per-member coverage gap

| Member | Status | Missed detail |
| --- | --- | --- |
| M1 constructor (14–58) | **partially covered** (branches missed: 18 of 19 null-throw branches) | The success path executes every assignment line, so line coverage is high. Only the `tableLayoutPanels` throw branch is taken (`:197`). The other **18** `throw` sub-expressions are never evaluated. Two of them sit on their own continuation lines — **line 48** (`breadcrumbWebView2 ?? throw ...`) and **line 51** (`?? throw new ArgumentNullException(nameof(breadcrumbThemeNotifier))`) — and will be reported as *partially covered lines*, not merely partial branches. |
| M2..M20 getters (60–98) | **covered** | All 19 are read by `QfcThemeHelper.CreateTheme` on every `SetupThemes(controlSet)` call (`QfcThemeHelper.cs:329-371`). |
| M21 `RequireCollection` (100–108) | **covered** | Null branch by `:197`; return path (107) by every successful construction. |

**Summary of the real gap: 18 unexercised null-guard branches, two of which also cost measurable
line coverage (lines 48 and 51).** No member is unreachable; no member requires a seam.

---

## 5. Testability classification per member

| Member | Classification | WinForms / COM API touched |
| --- | --- | --- |
| M1 constructor | **pure-testable-now** | **None.** It stores `Label`, `TableLayoutPanel`, `Button`, `Component`, `MenuStrip`, `TextBox`, `WebView2`, `FastObjectListView`, and `Control` references without reading a single member of any of them. No `BackColor`, no `Handle`, no `Invoke`, no `Show`. |
| M2..M20 getters | **pure-testable-now** | None — compiler-generated field reads. |
| M21 `RequireCollection` | **pure-testable-now** | None — a null check on `IList<T>`. |

**Construction requirements for the test `Arrange` (all already proven in-repo):**

- `Label`, `MenuStrip`, `TextBox`, `TableLayoutPanel`, `Button`, `Panel`, `ToolStripMenuItem`,
  `FastObjectListView` — constructed directly in memory. Precedent:
  `QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs:308-328`.
- `Microsoft.Web.WebView2.WinForms.WebView2` — constructed via
  `FormatterServices.GetUninitializedObject(typeof(WebView2))`, because its real constructor
  initialises browser infrastructure. Precedent: `QfcThemeHelperTests.cs:320`, `:323`, `:331-335`.
- `IQfcTipsDetails`, `IUiDispatcher` — `Mock<T>` (Moq). Precedent: `QfcThemeHelperTests.cs:301-302`.
- `Action<string>`, `Func<bool>`, `Action<Enums.ToggleState>` — inline lambdas. Precedent:
  `QfcThemeHelperTests.cs:321`, `:325`, `:326`.

---

## 6. Seam proposal

**Recommendation: introduce NO seam. Make no production change to this file.**

This file **is** the seam. It is the rank-1 artefact of the #236 coverage-seams refactor: the
host-neutral value object that lets `QfcThemeHelper.SetupThemes(QfcThemeControlSet)` be exercised
without an `ItemViewer`, an `IQfcItemController`, or an Outlook `MailItem`. Introducing a seam into
a seam would be circular.

Evaluated against the epic §2 hierarchy:

- **Interface seam (rank 1)** — already satisfied. The type's three delegate members
  (`BreadcrumbThemeNotifier`, `MailRead`, `HtmlConverter`) and its `IUiDispatcher` are the injection
  points; the concrete WinForms references are inert data.
- **Injectable delegate seam (rank 2)** — already satisfied by the same three delegates.
- **Adapter seam (rank 3)** — not applicable; there is no static or third-party call to wrap.

Options considered and rejected:

- **Rejected — replacing the concrete WinForms parameter types with narrow interfaces (e.g.
  `IThemedLabel`, `IThemedTextBox`).** This is the only change that would make the type "host-neutral"
  in the strict sense the epic's migration Non-Goal prefers. It is rejected for this child because
  (a) it yields **zero** additional coverage — the type is already fully constructible in a unit
  test; (b) `UtilitiesCS.Theme`'s primary constructor (`Theme.cs:20-65`) demands the concrete
  `Label`/`TextBox`/`MenuStrip`/`FastObjectListView` types, so the adapters would have to be unwrapped
  again inside `QfcThemeHelper.CreateTheme`, which is strictly worse; and (c) it would require
  changing `UtilitiesCS`, a project entirely outside epic #136's file set.
- **Rejected — replacing the 19 positional parameters with an options/builder object to shorten the
  null-guard block.** No coverage benefit; changes `QfcThemeHelper.cs:73-93` (F4-owned, so no
  sibling conflict) but adds indirection contrary to CLAUDE.md § "Simplicity first" and would
  invalidate the existing `QfcThemeControlSet_NullRequiredCollection_ThrowsArgumentNullException`
  test contract.

**Conflict statement: requires no sibling-owned file change** (no production change at all).

---

## 7. Cross-child conflict analysis

F4 owns only the 13 files under `QuickFiler/Helper Classes/` plus `QuickFiler/Interfaces/IEmailMoveMonitor.cs`.

### Every file outside F4 that references `QfcThemeControlSet` (repo-wide `*.cs` grep)

**None.**

The complete reference set is:

| Reference | Kind | Owner |
| --- | --- | --- |
| `QuickFiler/Helper Classes/QfcThemeControlSet.cs:12, 14` | declaration | **F4** |
| `QuickFiler/Helper Classes/QfcThemeHelper.cs:57` | return type of `BuildProductionControlSet` | **F4** |
| `QuickFiler/Helper Classes/QfcThemeHelper.cs:73` | `new QfcThemeControlSet(...)` — the sole construction site | **F4** |
| `QuickFiler/Helper Classes/QfcThemeHelper.cs:96` | parameter type of `SetupThemes` | **F4** |
| `QuickFiler/Helper Classes/QfcThemeHelper.cs:299` | parameter type of `CreateTheme` | **F4** |
| `QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs:47, 60, 84, 145, 197, 296, 308` | test usage | **F4** test surface |
| `QuickFiler/QuickFiler.csproj:350` | build registration | shared |

**Verdict: this is the only file in the entire F4 theme/layout cluster with zero cross-child
coupling.** Every reference is inside F4's own file set. Even a signature-changing refactor here
would touch no sibling-owned file — although §6 still recommends against one, for coverage-value
reasons rather than conflict reasons.

### Shared-file risk that does apply

| Shared file | Required edit | Risk |
| --- | --- | --- |
| `QuickFiler.Test/QuickFiler.Test.csproj` | one new line `<Compile Include="Helper Classes\QfcThemeControlSetTests.cs" />` | **All 14 wave-1 children edit this file.** This is the one unavoidable shared-file edit for `QfcThemeControlSet.cs`. Mitigation: insert alphabetically **inside** the existing contiguous `Helper Classes\` block (lines **158-165**), between `:161 QfcThemeHelperTests.cs` and `:162 TlpCellStatesTests.cs`. Siblings append to the `Controllers\`, `Viewers\`, and `Interfaces\` blocks, which are textually separated, so the hunks are disjoint and a 3-way merge resolves cleanly. Batch all of F4's csproj additions into one contiguous insertion in a single commit. |
| `QuickFiler/QuickFiler.csproj` | **no edit** | No production change. |

**Alternative considered to avoid the csproj edit entirely:** put the 25 cases in the already-
registered `QfcThemeHelperTests.cs`. **Rejected** — that file is already 463 of 500 lines
(`.claude/rules/general-code-change.md` § File Size Limit applies to test code), and artifact 02
already earmarks its remaining headroom for the 20 `QfcThemeHelper` cases. A dedicated file is
required, and it is also the correct destination under
`.claude/rules/general-unit-test.md` § Test File Location (mirror the production tree).

---

## 8. 500-line compliance

- **Production file: 110 of 500. Headroom 390 lines. Compliant; no production change proposed, so it
  stays at 110.** No partial split required, now or under any contingency in this plan.
- **New test file:** `QuickFiler.Test/Helper Classes/QfcThemeControlSetTests.cs`. Estimated ~320
  lines (a shared `Arrange` builder of ~35 lines plus 25 compact `[TestMethod]` bodies averaging
  ~11 lines). Comfortably under 500. **Contingency if it exceeds 500 during authoring:** split the
  19 null-guard cases into `QfcThemeControlSetGuardTests.cs`, which costs a **second**
  `<Compile Include>` line in `QuickFiler.Test/QuickFiler.Test.csproj` in the same block — an
  incremental, not a new, category of conflict risk.
- Cross-check of the other three F4 theme/layout files (each has its own artifact):
  `EfcThemeHelper.cs` 499/500 (1 line of headroom — the cluster's binding constraint);
  `QfcThemeHelper.cs` 375/500; `TlpCellSnapShot.cs` 223/500.

---

## 9. Recommended test cases (enumerated individually)

Destination for all: **new file** `QuickFiler.Test/Helper Classes/QfcThemeControlSetTests.cs`
(MSTest `[TestClass]`, Moq for `IQfcTipsDetails`/`IUiDispatcher`, FluentAssertions).

Shared `Arrange` helper (not a test case): a `CreateControlSet(...)` builder with one nullable
parameter per constructor argument, defaulting each to a valid in-memory instance, so each guard
test overrides exactly one argument. Model it on `QfcThemeHelperTests.cs:296-329`.

### Success path and property mapping

| # | `[TestMethod]` name | Arrange / Act / Assert | Category |
| --- | --- | --- | --- |
| 1 | `Constructor_WithAllDependenciesSupplied_ExposesEveryPropertyAsTheSuppliedInstance` | Arrange 19 distinct in-memory instances; Act construct; Assert all 19 getters `BeSameAs` their supplied argument — covers M1's 19 assignment lines and all 19 getters M2..M20 in one deterministic pass. | positive |
| 2 | `Constructor_WithEmptyCollections_IsAcceptedBecauseRequireCollectionChecksNullOnly` | Arrange all five `IList<>` arguments as empty lists; Act construct; Assert no throw and each collection getter `BeEmpty()` — pins M21's documented null-only contract (line 102 false branch, line 107). | boundary |
| 3 | `RequireCollection_ReturnsTheSameInstanceItWasGiven` | Arrange a populated `IList<Button>`; Act construct; Assert `Buttons.Should().BeSameAs(theSuppliedList)` — proves M21 returns rather than copies (line 107). | boundary |

### Null-guard cases — one per parameter (19)

Each: Arrange the builder with that one argument set to `null`; Act construct; Assert
`Should().Throw<ArgumentNullException>().WithParameterName("<camelCaseParameterName>")`.

| # | `[TestMethod]` name | Guarded line | Category |
| --- | --- | --- | --- |
| 4 | `Constructor_WithNullLblItemNumber_ThrowsArgumentNullException` | 36 | invalid-input |
| 5 | `Constructor_WithNullLblSender_ThrowsArgumentNullException` | 37 | invalid-input |
| 6 | `Constructor_WithNullLblSubject_ThrowsArgumentNullException` | 38 | invalid-input |
| 7 | `Constructor_WithNullTableLayoutPanels_ThrowsArgumentNullExceptionNamingTheParameter` | 39 via M21 | invalid-input |
| 8 | `Constructor_WithNullButtons_ThrowsArgumentNullException` | 40 via M21 | invalid-input |
| 9 | `Constructor_WithNullMenuItems_ThrowsArgumentNullException` | 41 via M21 | invalid-input |
| 10 | `Constructor_WithNullMenuStrip_ThrowsArgumentNullException` | 42 | invalid-input |
| 11 | `Constructor_WithNullTipsDetailsLabels_ThrowsArgumentNullException` | 43 via M21 | invalid-input |
| 12 | `Constructor_WithNullTipsExpanded_ThrowsArgumentNullException` | 44 via M21 | invalid-input |
| 13 | `Constructor_WithNullTextboxSearch_ThrowsArgumentNullException` | 45 | invalid-input |
| 14 | `Constructor_WithNullTextboxBody_ThrowsArgumentNullException` | 46 | invalid-input |
| 15 | `Constructor_WithNullBreadcrumbWebView2_ThrowsArgumentNullException` | **47–48** (also recovers a partially-covered line) | invalid-input |
| 16 | `Constructor_WithNullBreadcrumbThemeNotifier_ThrowsArgumentNullException` | **49–51** (also recovers a partially-covered line) | invalid-input |
| 17 | `Constructor_WithNullTopicThread_ThrowsArgumentNullException` | 52 | invalid-input |
| 18 | `Constructor_WithNullWebView2_ThrowsArgumentNullException` | 53 | invalid-input |
| 19 | `Constructor_WithNullViewer_ThrowsArgumentNullException` | 54 | invalid-input |
| 20 | `Constructor_WithNullMailRead_ThrowsArgumentNullException` | 55 | invalid-input |
| 21 | `Constructor_WithNullHtmlConverter_ThrowsArgumentNullException` | 56 | invalid-input |
| 22 | `Constructor_WithNullUiDispatcher_ThrowsArgumentNullException` | 57 | invalid-input |

### Delegate-storage contract (deferred execution)

| # | `[TestMethod]` name | Arrange / Act / Assert | Category |
| --- | --- | --- | --- |
| 23 | `Constructor_DoesNotInvokeTheMailReadDelegate` | Arrange `Func<bool> mailRead = () => throw new InvalidOperationException("must not run")`; Act construct; Assert no throw, and `MailRead.Should().NotBeNull()`. | error-handling |
| 24 | `Constructor_DoesNotInvokeTheHtmlConverterDelegate` | Arrange `Action<Enums.ToggleState> htmlConverter = _ => throw new InvalidOperationException("must not run")`; Act construct; Assert no throw. | error-handling |
| 25 | `Constructor_DoesNotInvokeTheBreadcrumbThemeNotifierDelegate` | Arrange `Action<string> notifier = _ => throw new InvalidOperationException("must not run")`; Act construct; Assert no throw — pins the issue #351 contract that the notifier posts the `themeChange` bridge message only when the theme is applied, never at construction. | error-handling |

**Total: 25 enumerated test cases.** Category spread: 1 positive, 19 invalid-input, 2 boundary,
3 error-handling — all four categories present.

---

## 10. STA determination

**STA is NOT required for any member of this file. No `*.StaTests.cs` file should be created.**

Per-member justification:

- **M1, M2..M20, M21** touch **no** WinForms control API whatsoever (§5). They store and return
  object references. The seam hierarchy is never entered because there is no boundary to isolate.
- The `Arrange` block constructs WinForms controls (`Label`, `MenuStrip`, `TextBox`,
  `TableLayoutPanel`, `Button`, `Panel`, `FastObjectListView`), but construction alone creates no
  window handle in .NET Framework WinForms — a handle is created on `CreateControl()`, `Show()`, or
  the first `Handle` access, none of which occurs. Proven by
  `QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs:296-329`, which builds the identical object
  graph in a plain `[TestClass]` with no STA attribute and no runsettings apartment scoping.
- `WebView2` is never constructed for real; `FormatterServices.GetUninitializedObject` bypasses its
  constructor entirely (`QfcThemeHelperTests.cs:331-335`), so no browser process, no COM apartment
  requirement, no `CoreWebView2` initialisation.

Tests must construct no `Form`, show no popup, and take no dependency on the UI thread. Nothing in
this file's test set comes near those boundaries.

---

## 11. Determinism

| Concern | Finding | Requirement on tests |
| --- | --- | --- |
| Wall-clock time | **None.** No `DateTime`, `DateTimeOffset`, `TimeProvider`, `Stopwatch`, or timer anywhere in the file. | No clock seam needed. |
| Randomness | **None.** | No seeded RNG needed. |
| Ambient state — `SystemColors` | **None.** Unlike `EfcThemeHelper.cs` and `QfcThemeHelper.cs`, this file contains no `Color` literal and no `SystemColors` read. It is the only file in the F4 theme/layout cluster free of Windows-theme-dependent values. | No symbolic-colour discipline needed here. |
| Ambient state — machine/process | **None.** No environment variables, no working directory, no registry, no static mutable state. |
| COM | **None.** No Outlook interop type appears in this file. `WebView2` is a WinForms wrapper, and only its reference is stored. |
| Delegate side effects | The three injected delegates are stored, never invoked (proven by tests 23–25). | Guard delegates in the `Arrange` builder must be throwing lambdas in tests 23–25 and inert lambdas elsewhere. |
| `Thread.Sleep` / `Task.Delay` / real waits | None in the file; **prohibited** in tests (`.claude/rules/general-unit-test.md` § Determinism Infrastructure; repo-root `BannedSymbols.txt`). | — |
| Temporary files, external services | None. | Prohibited by UT4. |
| Cross-test shared state | The type is immutable after construction and the tests never mutate the supplied controls. | Still build fresh instances per test in `Arrange` (UT1 Independence); do not share a `[ClassInitialize]` object graph. |

**Assessment: this is the most deterministic file in the F4 theme/layout cluster.** No seam, no
clock, no RNG, no ambient colour.

---

## 12. Projected coverage

- **Line coverage today is already high**, because the constructor's success path executes all 19
  assignment lines and `QfcThemeHelper.CreateTheme` reads all 19 getters on every
  `SetupThemes(controlSet)` call. The measurable line gap is limited to the two wrapped continuation
  lines **48** and **51**, which the current tests reach only on their non-throwing sub-expression.
- Test case 1 re-covers every assignment and getter deterministically and independently of
  `QfcThemeHelper`. Test cases 4–22 take all 19 `throw` branches, converting lines 48 and 51 from
  partially covered to fully covered. Test cases 2–3 take both branches of `RequireCollection`.
  Tests 23–25 pin the deferred-invocation contract.
- **Projected line coverage: 100% of executable lines. Projected branch coverage: 100%** — every one
  of the 20 decision points has both outcomes exercised (19 guards × {null, non-null}, plus
  `RequireCollection`'s single `if` which is shared across the five collection guards).
- **Clears the 80% floor decisively.** The argument is structural: the file has 21 members, all
  reachable from a plain constructor call, none dependent on a host, a form, a UI thread, a COM
  object, a clock, or a random source.
- **This file does not require an exemption.** It should be classified `testable` in F1's ledger
  (`docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`), which remains the
  authority on that classification. It could not qualify for the CLAUDE.md § UT2 exemption in any
  case: it is not form-derived, not Designer-generated, and depends on no
  `Application`/`MailItem`/`Store`/`MAPIFolder`.
- Numeric before/after per-file figures are produced by **F1's harness** (Cobertura output of
  `Invoke-MSTestWithCoverage.ps1`) at execution time and committed under
  `<FEATURE>/evidence/qa-gates/`.
