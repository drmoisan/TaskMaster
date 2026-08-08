# Per-File Research: `QuickFiler/Controllers/QfcItemController.cs`

- Epic: #136 QuickFiler Per-File 80% Coverage — child F10 (`quickfiler-item-controller-coverage`, issue #453)
- Branch: `feature/quickfiler-item-controller-coverage`
- Worktree: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a359b62de7a79b16e`
- Production file: `QuickFiler/Controllers/QfcItemController.cs` (323 lines, verified — file ends at line 323)
- Research date: 2026-08-07

---

## 0. Measurement basis and a correction that applies to all three F10 artifacts

### 0.1 Source

No coverage run was executed for this research. The numeric baseline is read from the most recent
committed QuickFiler-wide Cobertura report,
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`
(report line 22740 for this file). Per epic.md "Measured Coverage Baseline", this is **indicative,
not authoritative** — it was captured on feature #424's branch. The child must re-measure with F1's
harness on its own branch.

### 0.2 Correction to epic.md and to the delegation brief: the `line-rate` attribute is not
reliable in this report; recompute from the `<line>` children

Epic.md "Directives for F1's Ledger and Harness" already requires the harness to (1) union
duplicate `<class>` elements sharing a `filename`, taking **max hits per line**, and (2) decide the
denominator on `<line>` child count rather than `line-rate`. This report is direct evidence that the
first directive is load-bearing and that the `line-rate` **attribute** is a pre-union artifact:

| Evidence | Report location | Consequence |
| --- | --- | --- |
| `<method name="&lt;SaveParameters&gt;b__118_0">` reports source lines 382-388 at `hits="0"` | report lines 23333-23342 | The per-method view is stale |
| The enclosing `<class>`'s own `<lines>` block reports the **same** source lines 382-388 at `hits="1"` | report lines 23477-23481 | The class-level `<lines>` block is a max-hits union |

Because of this, the class-level `line-rate` attributes disagree with the values recomputed from the
class's own `<line>` children for the two partials that contain `[ExcludeFromCodeCoverage]` members:

| File | `line-rate` attribute | Recomputed from `<line>` children | `branch-rate` attribute | Recomputed |
| --- | --- | --- | --- | --- |
| `QfcItemController.cs` | 1.000000 (100%) | 100% (no `hits="0"`) | 0.785714 | 11/14 = 0.785714 (exact match) |
| `QfcItemController.Initialization.cs` | 0.901099 (90.1%) | 123/134 = **91.8%** | 0.961538 | 25/26 = 0.961538 (exact match) |
| `QfcItemController.ViewerSetup.cs` | 0.743682 (74.4%) | 116/160 = **72.5%** | 0.560000 | 30/54 = 0.5556 (approximate) |

For **this** file both views agree exactly, so nothing here is in doubt. The discrepancy matters for
the two sibling artifacts and is recorded once, here, to avoid duplication.

**Planning rule for F10:** treat the enumerated `<line>` children as the gap list; treat the rate
attributes as approximate. Re-measure before claiming acceptance.

### 0.3 Headline for this file

| Metric | Value | Floor | Verdict |
| --- | --- | --- | --- |
| Line coverage | 100% | >= 80% (issue #136 AC1) | PASS |
| Branch coverage | 78.57% (11/14) | >= 75% (`.claude/rules/general-unit-test.md`) | PASS |
| `[ExcludeFromCodeCoverage]` members | 0 | — | Nothing to de-exempt |
| File size | 323 / 500 lines | <= 500 | PASS, 177 lines headroom |

**The delegation brief's claim of "measured 100% line" is confirmed.** This file is the only one of
the three that already satisfies both gates. It is a *hardening* target, not a remediation target.

---

## 1. Member inventory

`internal partial class QfcItemController : IQfcItemController, INotifyPropertyChanged, IItemControler`
(declared at line 25). This partial carries the type declaration, the private field set, the exposed
property surface, and the `INotifyPropertyChanged` implementation. No method with behavior beyond
property accessors exists in this file.

### 1.1 Static and instance fields

| Lines | Member | Accessibility | Notes |
| --- | --- | --- | --- |
| 30-32 | `logger` | `private static readonly log4net.ILog` | Initialized via `MethodBase.GetCurrentMethod().DeclaringType`; runs in the type initializer |
| 37 | `_isWebViewerInitialized` | private | Has initializer `= false` |
| 38 | `_suppressEvents` | private | Has initializer `= false` |
| 39 | `_webViewEnvironment` | private `CoreWebView2Environment` | Written by `ViewerSetup.InitializeWebViewAsync`, nulled by `Cleanup` |
| 40 | `_themes` | private `Dictionary<string, Theme>` | Written by the exempt `Initialization` methods |
| 41 | `_folderHandler` | private `IFolderSearchHandler` | Backing store for `TopFolderScore` |
| 42 | `_globals` | private `IApplicationGlobals` | |
| 43 | `_tableLayoutPanels` | private `IList<TableLayoutPanel>` | |
| 44 | `_parent` | private `IQfcCollectionController` | |
| 45 | `_explorerController` | private `IQfcExplorerController` | |
| 48 | `_homeController` | private `IFilerHomeController` | |
| 49 | `_kbdHandler` | private `IQfcKeyboardHandler` | Owned contract belongs to F3 (#430) |
| 50 | `_itemPositionTips` | private `IQfcTipsDetails` | |
| 51 | `_itemViewer` | private `IItemViewer` | |
| 52 | `_activeTheme` | private `string` | |
| 53 | `_emailIsReadTimer` | private `System.Threading.Timer` | **Declared here, used only in `Navigation.cs` and `ViewerSetup.Cleanup`** — see §7.1 |
| 54-57 | `_optionConversationChecked`, `_optionEmailCopy`, `_optionAttachments`, `_optionsPictures` | private bool | |
| 59 | `_tokenSource` | private `CancellationTokenSource` | |
| 60 | `_tlpStates` | private `TlpCellStates` | |
| 66-68 | `_uiDispatcher`, `_webViewInitializer`, `_mailActions` | private interface seams | Cycle-2 Phase 6 behavioral seams |
| 69-77 | `_conversationResolverFactory`, `_flagTasksFactory`, `_emailFilerFactory` | private `Func<...>` seams | Defaults applied in `SaveParameters` |
| 83-89 | `_folderPredictorFactory`, `_folderPredictorEmptyFactory` | private `Func<...>` seams | Cycle-3 P10-T7 |
| 248 | `_predeterminedFolder` | `private readonly string` | Issue #171 high-confidence path |

Field declarations without initializers emit no coverable lines. Lines 37 and 38 carry initializers
and are attributed to the constructors.

### 1.2 Properties

| Lines | Member | Accessibility | Coverage |
| --- | --- | --- | --- |
| 96-100 | `Buttons` | `public` get / `private` set | COVERED (report lines 22742-22749) |
| 103-107 | `ConvOriginID` | public get/set | COVERED |
| 110-114 | `ConversationResolver` | public get / private set | COVERED |
| 117-121 | `CounterEnter` | public get/set | COVERED |
| 124-128 | `CounterComboRight` | public get/set | COVERED |
| 130-133 | `Height` | public get (delegates to `_itemViewer.Height`) | COVERED |
| 135-139 | `ItemHelper` | public get/set | COVERED |
| 142-146 | `IsExpanded` | public get | COVERED |
| 148-152 | `IsChild` | public get/set | COVERED |
| 155-159 | `IsActiveUI` | public get/set | COVERED |
| 162-166 | `ListTipsDetails` | public get | COVERED |
| 171-175 | `ListTipsExpanded` | public get | COVERED |
| 180-185 | `Mail` | public get/set | COVERED |
| 187-190 | `Parent` | public get | COVERED |
| 192-212 | `ItemNumber` | public get/set (4 branch outcomes) | COVERED, branches 100% |
| 213-217 | `ItemIndex` | public get/set | COVERED |
| 219-235 | `ItemNumberDigits` | public get/set (2 branch outcomes) | COVERED, branches 100% |
| 237-241 | `SelectedFolder` | public get | COVERED |
| 254 | `TopFolderScore` | public get, expression-bodied | **PARTIALLY COVERED — 1 of 4 branch outcomes** |
| 256-260 | `SuppressEvents` | public get/set | COVERED |
| 262-265 | `TableLayoutPanels` | public get | COVERED |
| 267 | `Token` | public auto-property | COVERED |

### 1.3 `INotifyPropertyChanged`

| Lines | Member | Accessibility | Coverage |
| --- | --- | --- | --- |
| 273-281 | `NotifyPropertyChanged([CallerMemberName] string)` | `protected` | COVERED, branch at 277 is 2/2 |
| 283 | `event PropertyChangedEventHandler PropertyChanged` | public field-like event | COVERED (add/remove accessors) |

### 1.4 Nested types

None.

### 1.5 Dead commented code

Lines 285-319 (35 lines) are a commented-out `Handler_PropertyChanged` / `GetConversationInfoAsync`
pair. Lines 36, 47, 168-169, 177-178 are further commented-out declarations. These emit no IL and do
not affect the denominator. Recorded in §7.4.

---

## 2. What is already covered

The covering fixture is `QuickFiler.Test/Controllers/QfcItemController.PropertiesTests.cs`
(`QfcItemController_PropertiesTests`), which uses a private `PropController : QfcItemController`
subclass (PropertiesTests.cs:22-28) exposing the `protected` parameterless constructor and a
`RaiseNotify` shim for `NotifyPropertyChanged`.

| Member | Status | Covering test |
| --- | --- | --- |
| `ConvOriginID`, `CounterEnter`, `CounterComboRight`, `IsChild`, `IsActiveUI`, `Token` | COVERED | `ScalarProperties_RoundTrip` (PropertiesTests.cs:78) |
| `IsExpanded`, `SelectedFolder`, `Buttons`, `ConversationResolver`, `ListTipsDetails`, `ListTipsExpanded`, `TableLayoutPanels`, `Parent`, `ItemHelper` (getters) | COVERED | `ReadThroughProperties_ReflectBackingState` (PropertiesTests.cs:104) |
| `ItemIndex` get/set, `ItemNumber` null-viewer guard | COVERED | `ItemIndex_GetSet_IsOneLessThanItemNumber` (PropertiesTests.cs:43) |
| `ItemNumber` setter, single-digit branch | COVERED | `ItemNumber_WhenSingleDigit_WritesItemNumberTextThroughViewer` (PropertiesTests.cs:122) |
| `ItemNumberDigits` setter + `ItemNumber` two-digit branch | COVERED | `ItemNumber_WhenTwoDigit_WritesZeroPaddedItemNumberText` (PropertiesTests.cs:139) |
| `Height` | COVERED | `Height_DelegatesToViewerHeight` (PropertiesTests.cs:156) |
| `SuppressEvents` | COVERED | `SuppressEvents_RoundTrips` (PropertiesTests.cs:55) |
| `NotifyPropertyChanged` + `PropertyChanged` | COVERED | `NotifyPropertyChanged_WithName_RaisesPropertyChanged` (PropertiesTests.cs:66) |
| `TopFolderScore` null-handler path only | **PARTIALLY COVERED** | `TopFolderScore_WhenFolderHandlerNull_ReturnsZero` (PropertiesTests.cs:36) |
| `ItemHelper` setter, `Mail` get/set, `ConversationResolver` private setter | COVERED indirectly | Set from `SeamCoreTests`/`SeamFactoryTests`/`ViewerSetupTests` fixtures |

Secondary coverage of the same members arrives from
`QfcItemController.ViewerSetupTests.cs`, `QfcItemController.SeamCoreTests.cs`,
`QfcItemController.SeamFactoryTests.cs`, `QfcItemController.SeamDispatcherTests.cs` and
`QfcItemControllerBreadcrumbDropDownTests.cs`, all of which read/write these properties as fixture
setup. **Do not add duplicate round-trip tests for anything in the COVERED rows above.**

---

## 3. The gap list

Exactly one gap exists in this file.

### G1 — `TopFolderScore` (line 254): 3 of 4 branch outcomes uncovered

```csharp
public long TopFolderScore => _folderHandler?.Suggestions?.TopScore() ?? 0;
```

Cobertura (report lines 23105-23110) records line 254 as `hits="1"` with
`condition-coverage="25% (1/4)"`:

- `condition number="0"` (the `_folderHandler?.` null test): **50%** — only the null branch taken.
- `condition number="1"` (the `.Suggestions?.` null test): **0%** — never evaluated.

`TopFolderScore_WhenFolderHandlerNull_ReturnsZero` covers exactly one of these outcomes. The
remaining three are:

1. `_folderHandler` non-null, `Suggestions` null → `?? 0`.
2. `_folderHandler` non-null, `Suggestions` non-null → `TopScore()` return value flows out.
3. (Implicit) the non-null path of `condition 0`, which is entered by both cases above.

Closing G1 takes the file to **14/14 = 100% branch** and is the single cheapest branch improvement
available anywhere in the F10 family.

### Non-gaps confirmed

- No line in this file is uncovered.
- No `[ExcludeFromCodeCoverage]` attribute exists in this file (verified by reading all 323 lines).
  Nothing here participates in the epic's exemption-removal workstream.

---

## 4. Seam analysis

### G1 barrier: none

`_folderHandler` is typed `IFolderSearchHandler` (line 41) — an **interface**, already the strongest
rung of the epic's hierarchy (interface seam > injectable delegate > adapter). No Outlook Interop
type, no WinForms control, no static state, no UI thread, no `UtilitiesCS` internal, and no wall-clock
read is on this path.

The existing harness reaches the field with no production change:

```
QfcItemControllerTestSupport.SetField(controller, "_folderHandler", handler.Object);
```

(`QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs:37-47`.)

**Recommended seam: none. Zero production edits are required for this file.** This is the correct
outcome under the epic's hierarchy — introducing any new seam here would be gratuitous.

One dependency must be checked at plan time rather than assumed: the type returned by
`IFolderSearchHandler.Suggestions` and whether `TopScore()` is an instance method or an extension
method. If `Suggestions` is a concrete sealed type without an interface, the test supplies a real
instance rather than a mock; if `TopScore()` is an extension method it cannot be mocked and the test
must construct a suggestion set with a known top score. Either way the test stays deterministic and
in-memory. This is a *test-authoring* detail, not a seam requirement.

### Host-neutrality (epic Non-Goals)

`TopFolderScore` is already host-neutral: it reads a numeric score from an interface and returns a
`long`. A future WebView2/Office.js port reuses it unchanged. No action.

---

## 5. State-transition invariants

This partial holds no workflow. Its invariants are property-level and all but the last are already
pinned.

| # | Invariant | Source | Status |
| --- | --- | --- | --- |
| I1 | `ItemIndex == ItemNumber - 1` in both directions | lines 213-217 | PINNED by `ItemIndex_GetSet_IsOneLessThanItemNumber` |
| I2 | Setting `ItemNumber` renders `ItemNumberText` unpadded when `ItemNumberDigits == 1`, zero-padded to 2 otherwise | lines 196-211 | PINNED by the two `ItemNumber_When*Digit_*` tests |
| I3 | The `ItemNumber` setter is safe when `_itemViewer` is null (both digit branches guard) | lines 201, 208 | PINNED by `ItemIndex_GetSet_IsOneLessThanItemNumber` (viewer is null) |
| I4 | `NotifyPropertyChanged` is a no-op when no subscriber is attached | line 277 | PINNED (branch 2/2) |
| I5 | `TopFolderScore` returns 0 rather than throwing when either link of the chain is null | line 254 | **PARTIALLY PINNED — only the first link** |
| I6 | The `ItemNumberDigits` setter is **not** null-guarded on `_itemViewer` while the `ItemNumber` setter is | lines 228, 232 vs 201, 208 | UNPINNED — see §7.2; this is an asymmetry, not a covered invariant |

### Ordering, re-entrancy, dispose-before-setup

Explicitly enumerated as required by the brief:

- **Initialization ordering:** not applicable to this file. It declares no constructor and performs
  no ordered setup. All construction ordering lives in `QfcItemController.Initialization.cs` and is
  analysed there.
- **Re-entrancy:** the only member that can re-enter is `NotifyPropertyChanged` (line 273), if a
  subscriber's handler sets another property that notifies again. The implementation is a plain
  guarded invoke with no shared mutable state touched after the invoke, so re-entrancy is safe by
  construction and there is no state machine to corrupt. A re-entrancy test here would assert a
  property of `EventHandler` invocation, not of this code. **Recommendation: do not write one.** The
  meaningful re-entrancy surface for F10 is `EventWiring.cs`/`Navigation.cs`, not this file.
- **Dispose-before-setup:** this file declares no dispose path. `Cleanup()` lives in
  `ViewerSetup.cs:392` and nulls fields declared here. The relevant post-`Cleanup` read hazard is
  that `Height` (line 132) dereferences `_itemViewer` **without** a null guard, so
  `Height` after `Cleanup` throws `NullReferenceException`. Every other read-through property in
  this file tolerates a null backing field. This is a genuine, currently-unpinned invariant and is
  proposed as test case T3 below. It is also recorded as a latent defect in §7.3 because the
  asymmetry (guarded in `ItemNumber`, unguarded in `Height` and `ItemNumberDigits`) looks
  unintentional.

---

## 6. Determinism requirements

Audited by reading all 323 lines and by
`grep -n 'DateTime\.(Now|UtcNow|Today)|Random|Thread\.Sleep|Task\.Delay|Stopwatch|Environment\.TickCount'`
across `QuickFiler/Controllers/QfcItemController*.cs`:

- **Wall-clock reads: none in this file.** No `DateTime.Now`, `DateTime.UtcNow`, `DateTime.Today`,
  `Stopwatch`, or `Environment.TickCount`.
- **Randomness: none.** No `Random`, no `Random.Shared`, no `Guid.NewGuid`.
- **Thread pool / timers: none executed here.** `_emailIsReadTimer` (line 53) is *declared* here but
  is only constructed and disposed in `QfcItemController.Navigation.cs:211-224` with a hard-coded
  4000 ms due time. That is a determinism hazard for the `Navigation.cs` artifact, not for this
  file's tests. No test proposed here touches it.
- **Banned-API findings in production code this child will touch:** the family-wide grep returns
  exactly one hit, `QfcItemController.EventWiring.cs:135  await Task.Delay(newDelay);`. That is in
  **production** code (a debounce), where `Task.Delay` is permitted — the repository ban in
  `.claude/rules/general-unit-test.md` § Determinism Infrastructure is on **test** code. It is
  nevertheless a determinism obstacle for whoever writes the `EventWiring.cs` tests and should be
  routed through an injectable delay delegate there. **Not in scope for this file**; recorded so the
  `EventWiring.cs` artifact and the plan can pick it up.
- **Conclusion for this file:** no injected clock, no `FakeTimeProvider`, and no fake timers are
  required. The proposed tests are pure in-memory property reads.

---

## 7. Latent defects for promotion

Report only. Do not fix under this child (epic NFR: no behavior change). Promote via the MCP
promotion lifecycle per epic.md "Latent Defect Promotion".

### 7.1 `Cleanup()` nulls `_emailIsReadTimer` without disposing it — **Moderate**

- Declaration: `QfcItemController.cs:53`.
- Leak site: `QfcItemController.ViewerSetup.cs:420` — `_emailIsReadTimer = null;` with no
  `Dispose()`.
- Contrast: `QfcItemController.Navigation.cs:211-214` correctly disposes before discarding, and
  `Navigation.cs:223-224` arms the timer with a 4000 ms one-shot whose callback is
  `ApplyReadEmailFormat`.
- Impact: if `Cleanup()` runs inside the 4-second window (pooled-viewer recycling does exactly
  this), the timer stays rooted by the `TimerCallback` and fires on a thread-pool thread against a
  controller whose `_itemViewer`, `_globals`, and `ItemHelper` have all just been set to null —
  a likely `NullReferenceException` on a thread-pool thread, plus a `Timer` finalizer leak per
  recycled item. Severity Moderate rather than High only because the window is short.
- Fully described in the `file-QfcItemController.ViewerSetup.md` artifact (§7) as well, since the
  defective line lives there.

### 7.2 `ItemNumberDigits` setter is not null-guarded on `_itemViewer` — **Low**

- `QfcItemController.cs:228` and `:232` write `_itemViewer.ItemNumberText` unconditionally.
- The structurally identical `ItemNumber` setter at `:201` and `:208` guards with
  `if (_itemViewer is not null)`.
- Impact: setting `ItemNumberDigits` before `SaveParameters` has run, or after `Cleanup()`, throws
  `NullReferenceException` where the sibling setter returns quietly. The existing test comment at
  `PropertiesTests.cs:45` ("The viewer is null, so the ItemNumber setter's guarded view write is
  skipped") shows the asymmetry is known but undocumented.
- This is the "sibling methods with identical shape treated inconsistently" pattern recorded in
  prior audits of this same type.

### 7.3 `Height` dereferences `_itemViewer` without a guard — **Low**

- `QfcItemController.cs:132`: `get => _itemViewer.Height;`.
- Throws after `Cleanup()` (which nulls `_itemViewer` at `ViewerSetup.cs:403` and again at `:419`).
- Same asymmetry class as 7.2. Grouping 7.2 and 7.3 into a single "null-guard consistency on
  viewer-backed properties" issue is reasonable.

### 7.4 35 lines of commented-out dead code — **Informational**

- `QfcItemController.cs:285-319` (`Handler_PropertyChanged`, `GetConversationInfoAsync`), plus
  smaller blocks at 36, 47, 168-169, 177-178.
- No coverage impact. Removing them would free ~40 lines against the 500-line ceiling, but that is
  a behavior-neutral cleanup outside this child's mandate.

---

## 8. Proposed test case list

Three test cases. Each is individually small, independently verifiable, and becomes its own atomic
task. All three belong in the **existing** fixture
`QuickFiler.Test/Controllers/QfcItemController.PropertiesTests.cs` (no new test file, therefore no
`QuickFiler.Test.csproj` edit — see §9.2).

| ID | Target member | Scenario | Fixture needed | Purpose |
| --- | --- | --- | --- | --- |
| **T1** | `TopFolderScore` (line 254) | Positive | `PropController` + `Mock<IFolderSearchHandler>` whose `Suggestions` returns a suggestion set with a known top score; injected via `QfcItemControllerTestSupport.SetField(controller, "_folderHandler", ...)` | Closes `condition 0` non-null and `condition 1` non-null. Assert `TopFolderScore` equals the known score. |
| **T2** | `TopFolderScore` (line 254) | Edge / null-tolerance | `PropController` + `Mock<IFolderSearchHandler>` whose `Suggestions` returns `null` | Closes `condition 1` null. Assert `TopFolderScore == 0` and that no exception is thrown. |
| **T3** | `Height` (line 132) | Error-handling / dispose-before-read | `PropController` with `_itemViewer` left null (its default) | Pins the currently-undocumented behavior that `Height` throws `NullReferenceException` when no viewer is attached, i.e. after `Cleanup()`. Uses `FluentAssertions` `Invoking(...).Should().Throw<NullReferenceException>()`. Documents defect 7.3 as an executable characterisation test **without changing behavior**. |

### Naming (matching the fixture's existing convention)

- `T1` — `TopFolderScore_WhenHandlerHasSuggestions_ReturnsTopScore`
- `T2` — `TopFolderScore_WhenSuggestionsNull_ReturnsZero`
- `T3` — `Height_WhenViewerNotAttached_Throws`

### Scenario-completeness check against `.claude/rules/general-unit-test.md`

| Required scenario | Covered by |
| --- | --- |
| Positive flow, valid inputs | T1; plus the 9 pre-existing PropertiesTests |
| Negative flow, missing input | T2 (null `Suggestions`), pre-existing `TopFolderScore_WhenFolderHandlerNull_ReturnsZero` (null handler) |
| Edge / boundary | Pre-existing `ItemNumber_WhenTwoDigit_*` (digit-count boundary), T2 |
| Error handling | T3 |
| Concurrency | Not applicable — no shared mutable state or async member in this file |
| State transitions | Pre-existing `ItemIndex_GetSet_*`, `ItemNumber_When*Digit_*`; see §5 for why no ordering/re-entrancy/dispose-setup test belongs here beyond T3 |

### Projected result

Line coverage stays at 100%. Branch coverage rises from 11/14 (78.57%) to **14/14 (100%)**. T3 adds
no new branch (line 132 has none) but converts an undocumented failure mode into a pinned one.

### Explicitly NOT proposed

- No duplicate round-trip test for any property listed COVERED in §2.
- No re-entrancy test for `NotifyPropertyChanged` (§5 rationale).
- No shape-assertion test manufactured for coverage — prohibited by epic.md "A third ledger bucket".

---

## 9. File-size and creation impact

### 9.1 Production

- Current: **323 / 500 lines.** Headroom 177.
- Proposed production edits: **none.** No seam is required (§4), so the file is unchanged and the
  500-line rule is not engaged.
- No new production file is created from this file, therefore:
  - no new `<Compile Include=...>` entry in `QuickFiler/QuickFiler.csproj` is required from this
    file's work;
  - no new ledger row is required (epic.md "Mid-Wave File Creation");
  - the CRLF-preservation obligation (use the Edit tool or `perl -0777` with explicit `\r\n`, never
    git-bash `sed -i`) does not bind for this file. It does bind for the ViewerSetup work — see that
    artifact.

### 9.2 Test project — correction to the delegation brief

The brief records the `<Compile Include=...>` obligation for `QuickFiler/QuickFiler.csproj` only.
**`QuickFiler.Test/QuickFiler.Test.csproj` is also a legacy non-SDK project with no globbing**: all
17 `QfcItemController*` test files are listed explicitly at
`QuickFiler.Test/QuickFiler.Test.csproj:90` and `:132-147`. Any new test file therefore needs its
own `<Compile Include>` entry, under the same CRLF-preservation rule, and is a second fan-in
conflict surface for concurrent children.

All three test cases proposed here go into the existing
`Controllers\QfcItemController.PropertiesTests.cs` (already listed at line 139), so **this file's
work requires no csproj edit at all**.

---

## 10. Sibling boundaries — do not edit

| Sibling asset | Owner | This file's dependency | Action |
| --- | --- | --- | --- |
| `ConversationResolver` | F4 (#434) | Field `_conversationResolver` (line 109) and property `ConversationResolver` (110-114) hold the type; the default factory that constructs it **positionally** with 5 arguments lives in `Initialization.cs:382-388`, not here | Depend on the current constructor shape. No edit. Recorded as a cross-child contract note in the Initialization artifact. |
| `KeyboardHandler.cs` / `IQfcKeyboardHandler` | F3 (#430) | Field `_kbdHandler` (line 49) only; no member of this file calls it | No edit. No contract change needed. |
| `IQfcDatamodel` | F5 | Not referenced by this file | None. |
| `TlpCellStates`, `QfcThemeHelper`, `ItemViewerQueue` | F4 (`Helper Classes/`) | Field `_tlpStates` (line 60) only | No edit. |
| `IItemViewer` / `ItemViewer` | F14 | Field `_itemViewer` (line 51); `Height` (132) and the `ItemNumber`/`ItemNumberDigits` setters read `IItemViewer` members that already exist | No edit. **Do not add members to `IItemViewer`** — see the cross-fixture warning below. |

### Cross-fixture warning (applies to the whole of F10)

`QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs:340-460` contains a hand-written **full
implementation of `IQfcItemController`** used as a test double for `QfcThemeHelper`. Adding any
member to `IQfcItemController` breaks that file's compilation. `Helper Classes/QfcThemeHelperTests.cs`
is the test counterpart of `QfcThemeHelper.cs`, which belongs to **F4 (#434)** — so an
`IQfcItemController` change forces F10 to edit a file inside F4's blast radius and guarantees a
fan-in conflict.

**Constraint for F10: do not widen `IQfcItemController`.** Nothing proposed in this artifact does.
Any sibling artifact that wants a new interface member must weigh this cost explicitly.

---

## 11. Summary

| Question | Answer |
| --- | --- |
| Current coverage reality | 100% line, 78.57% branch (11/14). Both floors already met. Brief's "100% line" claim confirmed. |
| Size of the gap | One partially-covered branch: `TopFolderScore` at line 254, 1 of 4 conditions. |
| Seams required | **None.** `_folderHandler` is already an interface reachable through the existing reflection harness. |
| Proposed test cases | 3 (T1, T2, T3), all into the existing `QfcItemController.PropertiesTests.cs`. |
| File split needed | No. 323 / 500 lines, 177 headroom, no production edit proposed. |
| `[ExcludeFromCodeCoverage]` to remove | None — this file carries no attribute. |
| Latent defects found | 4 (§7): undisposed timer on `Cleanup` (Moderate), two null-guard asymmetries (Low), 35 lines of commented-out dead code (Informational). |
| Corrections to the brief | (a) `QuickFiler.Test.csproj` also has no globbing and needs `<Compile Include>` entries for new test files; (b) the report's `line-rate` attribute is a pre-union artifact and disagrees with the `<line>` children for the other two files — recompute per epic directive B. |
