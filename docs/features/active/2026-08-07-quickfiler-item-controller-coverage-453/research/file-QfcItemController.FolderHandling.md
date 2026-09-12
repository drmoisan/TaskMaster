# Per-File Research — `QuickFiler/Controllers/QfcItemController.FolderHandling.cs`

- Feature: `quickfiler-item-controller-coverage` (issue #453), epic child F10 of epic #136
- Branch: `feature/quickfiler-item-controller-coverage`
- Worktree: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a359b62de7a79b16e`
- Production file: `QuickFiler/Controllers/QfcItemController.FolderHandling.cs` (235 lines)
- Coverage report used for the baseline:
  `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`
  (indicative; F1's harness on this branch remains authoritative)

**This is the file in my assignment that fails a gate.** Branch coverage is 63.33% against a 75%
floor.

---

## 1. Corrected coverage baseline

### 1.1 The emitted numbers double-count

The `<class>` element is at report line 24222. It carries a `<methods>` block (6 method entries, 76
`<line>` children in total) **and** a class-level `<lines>` block (147 `<line>` children).

Arithmetic proof, from the report:

- Emitted `line-rate="0.896861"`. `200 / 223 = 0.896861`.
  `223 = 147 + 76`; `200 = 129` (covered class-level) `+ 71` (covered method-level).
- Emitted `branch-rate="0.686275"`. `70 / 102 = 0.686274…`.
  `102 = 60` (class-level conditions) `+ 42` (method-level conditions); `70 = 38 + 32`.

### 1.2 Corrected figures

| Metric | Emitted | **Corrected (de-duplicated)** | Divergence | Gate | Verdict |
| --- | --- | --- | --- | --- | --- |
| Line | 89.69% (200/223) | **87.76% (129/147)** | -1.93 pts | >= 80% | PASS on both |
| Branch | 68.63% (70/102) | **63.33% (38/60)** | **-5.29 pts** | >= 75% | **FAIL on both** |

**The epic's indicative table figure of 89.7% for this file is inflated.** The corrected line rate is
87.8%.

The branch divergence does not flip a gate here — the file fails on both the emitted and the corrected
figure — but the true shortfall is materially larger than the emitted number suggests (needs +7
conditions, not +7 on a 102 base). Planning against the emitted number would have understated the work.

### 1.3 Multi-class union — this file is the concrete demonstration

A grep for `filename="QuickFiler\Controllers\QfcItemController.FolderHandling.cs"` returns exactly one
`<class>` element, but that element's `<methods>` block contains **two compiler-generated lambda
methods**:

- `<method name="&lt;PopulateFolderComboBox&gt;b__150_0">` (report line 24368) — one line, `number="141" hits="0"`.
- `<method name="&lt;AssignFolderComboBox&gt;b__152_0">` (report line 24373) — one line, `number="166" hits="0"`.

The class-level `<lines>` block records `number="141" hits="1"` (report line 24519) and
`number="166" hits="0"` (report line 24537). Line 141 is therefore `max(statement=1, lambda=0) = 1`.
**The class-level `<lines>` block already applies the epic's max-hits union rule**, verified
line-by-line for both files in my assignment. Reading the class-level block is the correct
de-duplication; there is no separate merge step needed for this file.

### 1.4 Uncovered inventory (corrected, class-level `<lines>`)

Uncovered lines — 18 of 147:

| Lines | Count | Member | Source construct |
| --- | --- | --- | --- |
| 73, 74, 75, 76, 77 | 5 | `LoadFolderHandlerAsync` | `return await fp.InitAsync(ItemHelper, InitOptions.FromField);` and the lambda close inside the FromField `Task.Run` |
| 81, 82, 83, 84, 85, 86 | 6 | `LoadFolderHandlerAsync` | the FromField success `logger.Debug(...)` interpolated block |
| 95, 96, 97, 98 | 4 | `LoadFolderHandlerAsync` | the **nested** `catch (System.Exception e2)` around `_folderPredictorEmptyFactory` |
| 165, 166, 167 | 3 | `AssignFolderComboBox` | the `InvokeRequired` marshal block |

Uncovered branch conditions — 22 of 60:

| Source line | Construct | Now | Realistic max | Note |
| --- | --- | --- | --- | --- |
| 29 | `varList is null` | 2/2 | 2/2 | done |
| 36 | `logger.Debug($"…{ItemHelper?.Subject}…{ItemHelper?.EntryId}…{_folderHandler?.Suggestions?.TopScore() ?? 0}")` — 4 null-conditional conditions | **4/8** | 8/8 | all four sides reachable (see §6.3) |
| 49 | same interpolation in the `FromArrayOrString` branch | **4/8** | 8/8 | all four sides reachable |
| 60 | `varList is null` (async) | 2/2 | 2/2 | done |
| 81 | same interpolation, async FromField success | **0/8** | 5/8 | `ItemHelper` is non-null by the `ThrowIfNull()` guard at line 69, so its two conditions cap at 1/2 each; `_folderHandler` is `fp.InitAsync`'s return (`this`), never null, so it caps at 1/2 |
| 125 | same interpolation, async FromArrayOrString | **4/8** | 7/8 | `_folderHandler` again never null on this path |
| 139 | `_itemViewer.InvokeRequired` | 2/2 | 2/2 | done |
| 164 | `_itemViewer.InvokeRequired` | **1/2** | 2/2 | true side never taken |
| 170 | `_folderHandler?.FolderArray?.Length > 0` — 3 conditions | **5/6** | 6/6 | the `FolderArray` null side is untested |
| 189 | `_folderHandler.Suggestions != null` | 2/2 | 2/2 | done |
| 193 | `!IsNullOrEmpty(_predeterminedFolder) && FolderContains(...)` | 4/4 | 4/4 | done |
| 202 | `FolderArray.Length == 1 ? 0 : 1` | 2/2 | 2/2 | done |
| 227 | `IsNullOrEmpty(predeterminedFolder)` | 2/2 | 2/2 | done |
| 230 | `predeterminedIndex >= 0 ? … : (folderArray.Length == 1 ? 0 : 1)` | 4/4 | 4/4 | done |

Target to clear the 75% floor: **45 of 60**. Current 38. Deficit **+7 conditions**.

---

## 2. Member inventory

All members are on `internal partial class QfcItemController`. **This file contains no
`[ExcludeFromCodeCoverage]` attribute anywhere** — confirming the brief. No fields, no properties, no
constructors, no events, no nested types are declared here.

| # | Member | Lines | Accessibility | Exempt? |
| --- | --- | --- | --- | --- |
| 1 | `void LoadFolderHandler(object varList = null)` | 27-55 | internal | no |
| 2 | `async Task LoadFolderHandlerAsync(CancellationToken cancel, object varList = null)` | 57-131 | public | no |
| 3 | `void PopulateFolderComboBox(object varList = null)` | 133-147 | public | no |
| 4 | `async Task PopulateFolderComboBoxAsync(CancellationToken token, object varList = null)` | 149-159 | public | no |
| 5 | `void AssignFolderComboBox()` | 161-208 | public | no |
| 6 | `static string PopulateAndSelectFolder(ComboBox, string[], string)` | 210-233 | internal static | no |

Compiler-generated closures counted in the report: `<PopulateFolderComboBox>b__150_0` (the
`() => AssignFolderComboBox()` lambda at line 141) and `<AssignFolderComboBox>b__152_0` (the same
lambda shape at line 166).

### Interface surface

`QuickFiler/Interfaces/IQfcItemController.cs` declares members 3 (`:48`), 4 (`:70`), 2 (`:71`), and 5
(`:18`). Members 1 and 6 are internal and not on the interface.

---

## 3. What is already covered

Existing tests. Do not duplicate any of these.

| # | Member | Status | Covering test(s) |
| --- | --- | --- | --- |
| 1 | `LoadFolderHandler` | **PARTIALLY COVERED** — 27/27 lines but 8/16 branch conditions (lines 36 and 49 both 4/8) | `QfcItemController.FolderHandlingTests.cs:153` `…WhenVarListNull_InvokesFactoryWithItemHelperAndFromFieldOptions`; `:191` `…WhenVarListProvided_InvokesFactoryWithArrayOrStringOptions` |
| 2 | `LoadFolderHandlerAsync` | **PARTIALLY COVERED** — 15 uncovered lines (73-77, 81-86, 95-98); branch line 81 at 0/8, line 125 at 4/8 | `FolderHandlingTests.cs:230` (FromField, factory throws `InvalidOperationException` -> outer catch 100-104 -> rethrow); `:264` (FromArrayOrString, factory throws); `:298` (FromField, factory throws `ArgumentNullException` -> fallback path 87-94); `:377` (async FromArrayOrString success, via member 4) |
| 3 | `PopulateFolderComboBox` | COVERED — 10/10 lines, branch 139 at 2/2 | `FolderHandlingTests.cs:329` (else branch); `:353` (`InvokeRequired` marshal) |
| 4 | `PopulateFolderComboBoxAsync` | COVERED — 5/5 lines | `FolderHandlingTests.cs:377` `…DispatchesAssignFolderComboBoxThroughViewerDispatcher` |
| 5 | `AssignFolderComboBox` | **PARTIALLY COVERED** — 25/28 lines (165-167 uncovered); branch 164 at 1/2, 170 at 5/6 | `FolderHandlingTests.cs:416` (index-1 selection), `:440` (predetermined preselection), `:465` (null handler guard), `:481` (single-suggestion index 0); `QfcItemController.FolderSuggestionsTests.cs:66` (`SetFolderSuggestions` row model), `:111` (retained `SetFolderItems`), `:137` (predetermined + suggestions); `QfcItemControllerTests.cs:327`, `:353` |
| 6 | `PopulateAndSelectFolder` | COVERED — 9/9 lines, 6/6 branch conditions | `FolderHandlingTests.cs:28` (exact match at index 0), `:47` (missing predetermined -> index 1), `:65` (empty array -> `ArgumentOutOfRangeException`), `:84` (single item -> index 0) |

One further existing test, `FolderHandlingTests.cs:133`
`LoadFolderHandler_ProbabilityDebugLog_IncludesCallerSubjectEntryIdAndTopScore`, asserts on the
**source text** of this file rather than on behavior. It contributes no coverage and carries a
test-policy defect (see LD-4 in §10).

---

## 4. The gap list

**Line gap: 18 lines, in three clusters.**

- **A — async FromField success path (11 lines: 73-77, 81-86).** The single largest cluster. Every
  existing async FromField test drives the *failure* path: the injected factory throws. No test lets
  `fp.InitAsync(ItemHelper, InitOptions.FromField)` complete.
- **B — nested catch (4 lines: 95-98).** `_folderPredictorEmptyFactory` throwing while already
  handling an `ArgumentNullException`. Trivially reachable: both delegates are injectable fields.
- **C — `AssignFolderComboBox` marshal block (3 lines: 165-167).** The `InvokeRequired == true` path.
  The identical path *is* covered in the sibling member 3 (`FolderHandlingTests.cs:353`) but not here.

**Branch gap: 22 conditions, concentrated in four `logger.Debug` interpolations.**

Lines 36, 49, 81, and 125 are the same interpolated diagnostic string repeated four times. Each
contains four null-conditional operators — `ItemHelper?.Subject`, `ItemHelper?.EntryId`,
`_folderHandler?.`, `.Suggestions?.` — producing 4 conditions / 8 outcomes per line. **32 of this
file's 60 conditions (53%) come from these four log statements.** They account for 20 of the 22
uncovered outcomes.

This is the correction to the brief's expectation: the branch density in this file is **not** in
folder-suggestion ranking logic. Ranking lives in `UtilitiesCS/OutlookObjects/Folder/FolderScorer.cs`
and `FolderPredictor.cs` (outside the epic). The selection logic that *is* here — lines 193, 202, 227,
230 — is already at 100% branch coverage.

---

## 5. `ConversationResolver` boundary

`QfcItemController.FolderHandling.cs` contains **no** reference to `ConversationResolver`,
`IConversationResolver`, or the `_conversationResolverFactory` field. Verified by full read of all 235
lines and by a solution-wide grep. The complete `ConversationResolver` contract analysis for F10 is in
the companion artifact `file-QfcItemController.Conversation.md` §5; it applies unchanged and is not
duplicated here.

The analogous coupling in **this** file is to `UtilitiesCS.FolderPredictor` /
`UtilitiesCS.IFolderSearchHandler`, which are **UtilitiesCS types, not `QuickFiler/Helper Classes/`
types**, and therefore not F4-owned. See §9.

---

## 6. Seam analysis

### 6.1 Seams that already exist (no production change needed)

Every dependency this file reaches is already behind an injectable seam, all declared on
`QuickFiler/Controllers/QfcItemController.cs`:

| Dependency | Field | Declared | Type |
| --- | --- | --- | --- |
| Folder predictor construction | `_folderPredictorFactory` | `QfcItemController.cs:83-88` | `Func<IApplicationGlobals, object, FolderPredictor.InitOptions, FolderPredictor>` |
| Empty-predictor fallback | `_folderPredictorEmptyFactory` | `QfcItemController.cs:89` | `Func<IApplicationGlobals, FolderPredictor>` |
| The predictor itself | `_folderHandler` | `QfcItemController.cs:41` | **`IFolderSearchHandler`** — an interface, so a hand-written fake can be injected directly |
| The view | `_itemViewer` | `QfcItemController.cs:51` | `IItemViewer` — mockable |
| Globals | `_globals` | `QfcItemController.cs:42` | `IApplicationGlobals` — mockable |
| High-confidence preselection | `_predeterminedFolder` | `QfcItemController.cs:248` (`readonly`, reflection-settable via `FieldInfo.SetValue`) | `string` |

**The peer researcher's conclusion holds for this file as well: the existing harness is sufficient for
every gate-clearing test with ZERO production change.** No new interface seam, no new injectable
delegate, no adapter, and no STA-bound test file are required. `EnsureUiThreadDispatcher` /
`EnableHandlelessThemeInvoke` are not needed here.

### 6.2 Blockers, per uncovered target

| Uncovered target | Blocker | Minimum seam |
| --- | --- | --- |
| Lines 165-167 (`AssignFolderComboBox` marshal) | none — `IItemViewer.InvokeRequired` and `.Invoke` are already mocked in the sibling test at `FolderHandlingTests.cs:353` | **none** |
| Line 170 `FolderArray` null side | none — inject an `IFolderSearchHandler` fake with `FolderArray = null` directly into `_folderHandler`, exactly as `FolderSuggestionsTests.cs:84` already does with its `FakeFolderHandler` | **none** |
| Lines 36 / 49 conditions | none — `ItemHelper` is a public settable property (`QfcItemController.cs:135-139`); `_folderPredictorFactory` may return `null`; `FolderPredictor.Suggestions` has a public setter (`FolderPredictor.cs:264-268`) | **none** |
| Line 125 conditions | none — same, driven through `LoadFolderHandlerAsync(token, varList)` where `InitAsync` -> `FromArrayOrString` touches no COM (`FolderPredictor.cs:104-131`) | **none** |
| Lines 95-98 (nested catch) | none — both factory delegates are injectable and can be made to throw | **none** |
| Lines 73-77, 81-86 (async FromField success) | **Real barrier.** `fp.InitAsync(ItemHelper, FromField)` (`FolderPredictor.cs:50-69`) -> `InitializeFromEmail` (`:79`) -> `FromFolderKey(MailItemHelper)` (`:141`) -> `Suggestions.LoadFromField(mailInfo, _globals)` (`FolderScorer.cs:72`) -> `AddConversationBasedSuggestions(mailInfo.Item, …)` and `AddOlFolderKeys(mailInfo.Item, …)` (`FolderScorer.cs:86`), which reads `olMail.UserProperties.Find("FolderKey")` — a live Outlook Interop chain. | See §6.4. Optional; **not required to clear either gate**. |

### 6.3 Moq non-virtual caveat — verified relevant

`FolderPredictor.InitAsync` (`UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:50`) is a
**non-virtual method on a concrete class**, and the `_folderPredictorFactory` delegate's return type is
the concrete `FolderPredictor` (deliberately — see the design comment at `QfcItemController.cs:79-82`
and `UtilitiesCS/OutlookObjects/Folder/IFolderSearchHandler.cs:7-13`). **Moq cannot intercept it.**
Consequences for test design:

- Any test that must get past line 73 or line 109 needs a **real** `FolderPredictor` instance whose
  `InitAsync` completes, not a mock.
- The existing `BuildFolderHandlerWithArray` helper (`FolderHandlingTests.cs:100-113`) already does
  this: it invokes the single-parameter `FolderPredictor(Outlook.Application)` constructor
  (`FolderPredictor.cs:25-33`) with `null` and seeds the private `_folderList` field by reflection.
- **Correction to a stale in-code comment:** `FolderPredictor.cs:28-32` claims "`_suggestions` defaults
  via its field initializer". It does not — `FolderPredictor.cs:263` reads
  `private FolderScorer _suggestions = null!;`. A predictor built by the navigation-only constructor
  therefore has `Suggestions == null`. This is precisely why the `Suggestions != null` guard exists at
  `FolderHandling.cs:189`, and it is why line 189 already shows 2/2 branch coverage (the
  `FolderHandlingTests` fixtures produce null `Suggestions`, the `FolderSuggestionsTests` fake produces
  a non-null one). Recorded as LD-1.
- `FolderScorer.TopScore()` (`FolderScorer.cs:236-237`) returns `0` on an empty scorer, so a test may
  safely set `Suggestions = new FolderScorer()` to exercise the non-null side of the interpolation
  without populating scores.

### 6.4 The async-FromField success path (optional)

To reach lines 73-86 a test would need a real `FolderPredictor` whose
`Suggestions.LoadFromField(helper, globals)` returns `true` so the COM-heavy
`RefreshSuggestions` is skipped. That requires `helper.Item` to be a `Mock<MailItem>` whose
`UserProperties.Find("FolderKey")` returns a `Mock<UserProperty>` with an array `Value`, so
`AddOlFolderKeys` -> `AddArray` returns true (`FolderScorer.cs:86-107`). The Outlook interop types are
COM interfaces and are mockable — existing tests already do `new Mock<MailItem>()`. **However**,
`AddConversationBasedSuggestions(mailInfo.Item, appGlobals)` runs first (`FolderScorer.cs:75`) and its
tolerance for a mock mail item / null globals was **not verified in this research**. Treat this as an
open risk, not a confirmed technique.

Because §8's projection clears both gates without it, this path is proposed as an **optional stretch
task** and must not be a prerequisite for the child's acceptance.

---

## 7. State-transition invariants and determinism

### 7.1 Invariants

| ID | Invariant | Where | Pinned by |
| --- | --- | --- | --- |
| I-1 | `varList == null` selects `FromField` with `ItemHelper`; non-null selects `FromArrayOrString` with `varList`. Sync and async agree. | `:29-54`, `:60-130` | existing `:153`, `:191`, `:230`, `:264` |
| I-2 | A predictor construction failure with `ArgumentNullException` degrades to the empty-predictor fallback rather than propagating. | `:87-100` | existing `:298` |
| I-3 | Any other construction failure propagates. | `:101-105` | existing `:230`, `:264` |
| I-4 | If the fallback itself fails, the fallback's exception surfaces (original context is logged, not swallowed). | `:95-99` | **FH-8 (new)** |
| I-5 | Folder population is idempotent w.r.t. marshaling: `AssignFolderComboBox` re-checks `InvokeRequired` even when reached through `PopulateFolderComboBox`'s own check, so a direct call from a worker thread still marshals. | `:139-146` then `:164-168` | existing `:353` covers the outer check; **FH-1 (new)** covers the inner one |
| I-6 | Population is a no-op when there are no suggestions: a null handler, or a handler with a null or empty `FolderArray`, leaves the viewer untouched. | `:170` | existing `:465` (null handler); **FH-2 (new)** (null `FolderArray`) |
| I-7 | Selection precedence: a non-empty predetermined folder the viewer contains wins; otherwise index 1, or index 0 when there is exactly one entry. | `:193-205` | existing `:416`, `:440`, `:481`; and for the static seam `:28`, `:47`, `:84` |
| I-8 | `_selectedFolder` is refreshed from the viewer after every successful population, so `SelectedFolder` never lags the combo box. | `:206` | existing `:416`, `:440`, `:481` |
| I-9 | The breadcrumb pipeline is ensured **before** items are set, so ancestor chains resolve through the #351 provider. | `:176` precedes `:182` | existing `:416` (executes the ordering; `EnsureBreadcrumbPipeline` returns early for mock viewers) |
| I-10 | Diagnostic logging never throws on a partially-initialized controller: null `ItemHelper`, null `_folderHandler`, or null `Suggestions` must all be tolerated. | `:36`, `:49`, `:81`, `:125` | **FH-3..FH-7 (new)** — this is the invariant behind 20 of the 22 uncovered branch outcomes |

There is no dispose/teardown guard in this file, so there is no "act after dispose" invariant to pin
here.

### 7.2 Determinism

- **No wall-clock read.** No `DateTime.Now`, `DateTime.UtcNow`, `Stopwatch`, or `Environment.TickCount`
  anywhere in the 235 lines. Verified by full read. **No banned-API finding in this production file.**
- **No randomness.** No `Random`, `Random.Shared`, or `Guid.NewGuid`.
- **Thread-pool usage is present and must be handled correctly.** Three offloads:
  - `:64` `await Task.Run(async () => { … }, cancel).ConfigureAwait(false)` (FromField)
  - `:109` `await Task.Run(async () => { … }, cancel).ConfigureAwait(false)` (FromArrayOrString)
  - `:157` `await Task.Run(() => LoadFolderHandlerAsync(token, varList), token)` — a second `Task.Run`
    wrapping a method that already offloads (see LD-3)

  Each is **awaited**, so a test drives them deterministically by awaiting the returned `Task`. No fake
  timer is needed. `Thread.Sleep`, `Task.Delay`, polling loops, and real wall-clock waits are
  prohibited and are not required.
- **One real dispatcher is currently used by an existing test.** `:158`
  `await _itemViewer.UiDispatcher.InvokeAsync(AssignFolderComboBox)` routes through the sealed WPF
  `Dispatcher` on `IItemViewer`, not through the injected `_uiDispatcher` seam. The existing test at
  `FolderHandlingTests.cs:377` therefore spins a real running STA dispatcher via
  `QfcItemControllerTestSupport.StartRunningDispatcher()` (`TestSupport.cs:297-317`) and shuts it down
  in a `finally`. That is deterministic (completion is observed by awaiting, not polling) but is a seam
  inconsistency — recorded as LD-3. **No new test proposed here needs that dispatcher.**

---

## 8. Proposed test cases

Each is an independently verifiable atomic task. MSTest, Moq, FluentAssertions, Arrange-Act-Assert, no
temporary files, no external services, no live forms, no popups.

Shared fixture requirement: an `IFolderSearchHandler` fake with settable `FolderArray`, `Suggestions`,
and `FolderRowArray`. One exists already but is `private sealed` inside
`QfcItemController.FolderSuggestionsTests.cs:30-45`. **Recommendation:** add a shared
`internal sealed class FakeFolderSearchHandler` (~20 lines) to
`QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` (365 lines, 135 headroom) and leave the
existing private fake untouched, so no existing test is modified.

For tests needing a real predictor, reuse the established construction technique from
`FolderHandlingTests.cs:100-113` (single-parameter `FolderPredictor(Outlook.Application)` ctor invoked
with `null` via reflection, `_folderList` seeded), and set `Suggestions` through its public setter
where the non-null side is required.

| ID | Target member | Scenario | Fixture | Covers |
| --- | --- | --- | --- | --- |
| **FH-1** | 5 `AssignFolderComboBox` | positive / marshaling (I-5) | `FolderController` (local `QfcItemController` subclass exposing the protected ctor); `Mock<IItemViewer>` with `InvokeRequired == true` into `_itemViewer`. Assert `viewer.Verify(v => v.Invoke(It.IsAny<Delegate>()), Times.Once())` and `viewer.Verify(v => v.SetFolderItems(It.IsAny<string[]>()), Times.Never())`. | **lines 165, 166, 167** (+3) and **line 164 -> 2/2** (+1 condition) |
| **FH-2** | 5 | edge (I-6) | `Mock<IItemViewer>` with `InvokeRequired == false`; `FakeFolderSearchHandler { FolderArray = null }` injected into `_folderHandler`. Assert no `SetFolderItems`, no `SetFolderSelectedIndex`, no `SetFolderSuggestions`, and `SelectedFolder` unchanged. | **line 170 -> 6/6** (+1 condition) |
| **FH-3** | 1 `LoadFolderHandler` | negative / null-tolerance (I-10) | `ItemHelper` left null; `_globals` = `Mock<IApplicationGlobals>.Object`; factory returns a real `FolderPredictor` with `Suggestions = new FolderScorer()`. Act: `controller.LoadFolderHandler()`. Assert no throw and `_folderHandler` is the returned instance. | **line 36**: `ItemHelper?` null side x2, `Suggestions?` non-null side (+3 conditions) |
| **FH-4** | 1 | negative | Factory returns `null`. Act: `controller.LoadFolderHandler()`. Assert no throw and `_folderHandler` is null. | **line 36**: `_folderHandler?` null side (+1 condition) |
| **FH-5** | 1 | negative / null-tolerance, else branch | As FH-3 but `controller.LoadFolderHandler(new[] { "a", "b" })`. | **line 49**: +3 conditions |
| **FH-6** | 1 | negative, else branch | As FH-4 but with a non-null `varList`. | **line 49**: +1 condition |
| **FH-7** | 2 `LoadFolderHandlerAsync` | positive / null-tolerance, async else branch | `ItemHelper` left null; `varList = new[] { @"\\A\one" }`; factory returns a real `FolderPredictor` (`Suggestions = new FolderScorer()`) so `InitAsync` -> `FromArrayOrString` completes with no COM (`FolderPredictor.cs:104-119`). Act: `await controller.LoadFolderHandlerAsync(CancellationToken.None, varList)`. Assert `_folderHandler` is the instance. | **line 125**: +3 conditions |
| **FH-8** | 2 | error / nested failure (I-4) | Primary factory throws `ArgumentNullException`; `_folderPredictorEmptyFactory` throws `InvalidOperationException`. Act: `Func<Task> act = () => controller.LoadFolderHandlerAsync(CancellationToken.None)` with a non-null `ItemHelper`. Assert `await act.Should().ThrowAsync<InvalidOperationException>()`. | **lines 95, 96, 97, 98** (+4 lines) |
| **FH-9** *(optional stretch — see §6.4)* | 2 | positive, async FromField success | Real `FolderPredictor`; `ItemHelper` = `new MailItemHelper(mockMail, mockGlobals)` where `mockMail.UserProperties.Find("FolderKey")` yields a `Mock<UserProperty>` with a `string[] Value`, so `AddOlFolderKeys` -> `AddArray` returns true and `RefreshSuggestions` is skipped. **Open risk:** `AddConversationBasedSuggestions` tolerance not verified. | **lines 73-77, 81-86** (+11 lines) and **line 81** (+4 to +5 conditions) |

### Coverage projection

| State | Lines | Line % | Branch | Branch % | 80% line | 75% branch |
| --- | --- | --- | --- | --- | --- | --- |
| Today (corrected) | 129/147 | 87.76% | 38/60 | 63.33% | PASS | **FAIL** |
| + FH-1, FH-2 | 132/147 | 89.80% | 40/60 | 66.67% | PASS | FAIL |
| + FH-3..FH-6 | 132/147 | 89.80% | 48/60 | 80.00% | PASS | **PASS** |
| + FH-7 | 132/147 | 89.80% | 51/60 | **85.00%** | PASS | PASS |
| + FH-8 | **136/147** | **92.52%** | 51/60 | 85.00% | PASS | PASS |
| + FH-9 (optional) | 147/147 | 100.00% | 55-56/60 | 91.7-93.3% | PASS | PASS |

**FH-1 through FH-8 clear both gates with margin and require no production change.** Robustness check:
if the `Suggestions` non-null side of the interpolations behaves differently than analyzed, FH-3/FH-5
each still contribute +2 rather than +3, giving 46/60 = 76.7% — still above the 75% floor. FH-9 is not
load-bearing.

---

## 9. File-size and creation impact

### Production

`QfcItemController.FolderHandling.cs` stays at 235 lines. **No production change is proposed for this
file**, so no `QuickFiler/QuickFiler.csproj` edit and no new ledger row are needed
(`<Compile Include="Controllers\QfcItemController.FolderHandling.cs" />` already exists at
`QuickFiler.csproj:332`).

### Tests — the blocking constraint

| Test file | Current lines | Headroom to 500 | Can absorb new tests? |
| --- | --- | --- | --- |
| `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs` | **498** | **2** | **NO.** Adding a single `[TestMethod]` breaches the 500-line limit. |
| `QuickFiler.Test/Controllers/QfcItemController.FolderSuggestionsTests.cs` | 191 | 309 | Yes, but topically scoped to the #325 row model. |
| `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` | 365 | 135 | Shared helpers only; add `FakeFolderSearchHandler` here (~20 lines -> 385). |
| `QuickFiler.Test/Controllers/QfcItemControllerTests.cs` | (not measured; contains folder tests at `:327`, `:353`) | — | Not recommended; general-purpose file. |

This independently reproduces the peer finding about `QfcItemController.FocusAndThemeTests.cs` at
497/500: **`QfcItemController.FolderHandlingTests.cs` at 498/500 is a second instance of the same
problem in the same family.** Both must be treated as full.

**Recommendation: create one new test file**
`QuickFiler.Test/Controllers/QfcItemController.FolderHandlingBranchTests.cs` holding FH-1 through FH-8
(~230 lines projected, or ~260 with FH-9).

**Mandatory csproj edit.** `QuickFiler.Test/QuickFiler.Test.csproj` is a legacy non-SDK project with no
globbing; the `QfcItemController.*` entries occupy lines 90 and 132-147. Add
`<Compile Include="Controllers\QfcItemController.FolderHandlingBranchTests.cs" />` adjacent to line
134. **epic.md names only `QuickFiler/QuickFiler.csproj` and is incomplete on this point** — the brief's
correction is confirmed.

**CRLF.** Both csproj files are CRLF-terminated. Use the Edit tool or `perl -0777` with explicit
`\r\n`, never a git-bash `sed -i`.

---

## 10. Latent defects for promotion (do not fix under F10)

| ID | File:line | Description | Severity |
| --- | --- | --- | --- |
| **LD-1** | `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:28-32` vs `:263` | The navigation-only constructor's comment claims "`_suggestions` defaults via its field initializer", but the field is `private FolderScorer _suggestions = null!;` with no initializer. A predictor built by that constructor has a null `Suggestions`, which is a live trap for every caller that does not null-check. Documentation defect; the code is functionally consistent with the `FolderHandling.cs:189` guard. Outside the epic (UtilitiesCS). | Low |
| **LD-2** | `QuickFiler/Controllers/QfcItemController.FolderHandling.cs:189-192` | The guard checks `Suggestions != null` before reading `_folderHandler.FolderRowArray`, but the `FolderRowArray` getter also dereferences `_globals.AF.RecentsList` (`FolderPredictor.cs:252`). A predictor constructed with the navigation-only ctor (`_globals == null`) passes the guard and then throws. Production predictors always carry globals, so this is latent rather than active. | Low |
| **LD-3** | `QuickFiler/Controllers/QfcItemController.FolderHandling.cs:149-159` | `PopulateFolderComboBoxAsync` wraps `LoadFolderHandlerAsync` — which already offloads via `Task.Run` at `:64`/`:109` — in a **second** `Task.Run` at `:157`, and then dispatches through `_itemViewer.UiDispatcher` (`:158`) instead of the injected `_uiDispatcher` seam used by every other async member in the family. The seam inconsistency is why the existing test at `FolderHandlingTests.cs:377` must spin a real running STA WPF dispatcher. Fixing it is a behavior-adjacent seam change and is out of scope under the epic's no-behavior-change NFR. | Medium |
| **LD-4** | `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs:120-148` | `ReadControllerSource` calls `File.ReadAllText` on a path built from `AppDomain.CurrentDomain.BaseDirectory` + `..\..\..\QuickFiler\Controllers`, and `LoadFolderHandler_ProbabilityDebugLog_…` asserts on the production file's **source text**. This is a filesystem dependency inside a unit test, a brittle relative-path assumption that breaks if the output layout changes, and it asserts on source rather than behavior. Directly parallel to the `MailItemInfoTests.cs` `DateTime.Now` finding that epic.md ruled **in scope** for F4's own execution; by that precedent it is in scope for F10. | Medium (test policy) |
| **LD-5** | `QuickFiler/Controllers/QfcItemController.FolderHandling.cs:64-71` and `:109-116` | The `objItem`/`options` arguments passed to `_folderPredictorFactory` are discarded: `FolderPredictor(IApplicationGlobals, object, InitOptions)` (`FolderPredictor.cs:42-48`) ignores both parameters, and the real initialization happens in the subsequent `InitAsync` call. Dead arguments carried through the seam signature; harmless but misleading, and it is why the production default factory at `QfcItemController.Initialization.cs:395-396` looks like it initializes when it does not. | Low |

Per epic.md §"Latent Defect Promotion", these must be promoted to GitHub issues via the MCP promotion
lifecycle during F10's execution.

**Duplicate-issue check (2026-08-07, cycle 2).** Checked against the confirmed open-issue set (#230,
#426, #427, #438, #440, #441, #444, #457, #463 and the full F1-F15 child list). LD-1 through LD-5 are
not duplicates of any of those.

**Cross-reference note — open issue #427 (not a duplicate, but directly relevant to this file's
branch structure).** Issue #427 ("quickfiler-post-show-duplicate-scoring") names
`QfcItemController.LoadFolderHandlerAsync` (this file, `:57-131`) explicitly: the dequeue gate
computes folder predictions that are discarded, then `LoadFolderHandlerAsync` recomputes identical
classifications after the form displays. None of LD-1 through LD-5 describes that duplicate-scoring
symptom, so this is not a duplicate — but a #427 fix would most likely add a "prediction already
computed" short-circuit at the top of `LoadFolderHandlerAsync`, changing the line and branch set this
artifact's coverage plan (§2-§4) is built against. F10 must not pre-empt or partially implement #427;
note it in the plan as a structural-drift risk to `LoadFolderHandlerAsync`'s branch coverage, not as a
latent defect to promote here.

---

## 11. Sibling boundaries — files this child must not edit

| Dependency | Owner | Verified? | F10 action |
| --- | --- | --- | --- |
| `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs`, `FolderScorer.cs`, `IFolderSearchHandler.cs`, `FolderRow` | **UtilitiesCS — outside epic #136 entirely.** Not a `QuickFiler/Helper Classes/` file, therefore **not F4-owned**. | Yes — path confirmed | Read-only. LD-1 is recorded, not fixed. |
| `QuickFiler/Helper Classes/**` (`ConversationResolver`, `cInfoMail.cs`, `QfEnums.cs`, `QfcThemeHelper.cs`, …) | **F4 (#434)** | Yes | Not referenced by this file at all. |
| `QuickFiler/Controllers/KeyboardHandler.cs`, `Ka*.cs`, `KbdActions.cs` | **F3 (#430)** | Yes | Not referenced by this file. |
| `QuickFiler/Interfaces/IQfcDatamodel.cs`, `QfcDatamodel*.cs` | **F5** | Yes | Not referenced by this file. |
| `QuickFiler/Viewers/IItemViewer.cs` (`SetFolderItems`, `SetFolderSuggestions`, `SetFolderSelectedItem`, `SetFolderSelectedIndex`, `FolderContains`, `GetSelectedFolder`, `InvokeRequired`, `Invoke`, `UiDispatcher`) | **F14** | Yes | Read-only. No addition required — every member the new tests need already exists and is already mocked by existing tests. |
| `UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs` | **UtilitiesCS, not F4** — independently verified against the peer finding | Yes | Not used by this file. |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` (`EnsureBreadcrumbPipeline` at `:133`, itself `[ExcludeFromCodeCoverage]` at `:132`) | **F10 itself**, assigned to a concurrent peer researcher | Yes | Read-only here. Note for plan sequencing: `FolderHandling.cs:176` calls it, and it returns early when `_itemViewer` is not a concrete `ItemViewer` (`ViewerSetup.cs:135-138`), which is why mock-viewer tests are safe. If the ViewerSetup researcher recommends removing that exemption, the attributed lines move to `ViewerSetup.cs`; there is no conflict on `FolderHandling.cs`. |
| `QuickFiler/Controllers/QfcItemController.cs` (field declarations at `:41`, `:83-89`, `:248`) | **F10 itself**, concurrent peer | Yes | Read-only from this artifact; no field is added or retyped. |

No boundary crossing is proposed. No upstream change is required from any sibling for this file to
clear both gates.
