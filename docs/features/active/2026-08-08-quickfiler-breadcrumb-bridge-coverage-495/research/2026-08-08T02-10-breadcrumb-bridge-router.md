# Research: `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` (F12 / issue #495)

- Timestamp: 2026-08-08T02-10
- Epic: `docs/features/epics/quickfiler-per-file-coverage/epic.md` (#136), child F12
- Child issue: #495
- Branch: `feature/quickfiler-breadcrumb-bridge-coverage-r2` (based on `epic/quickfiler-per-file-coverage-integration`)
- Scope: ONE production file, per the #136 one-research-artifact-per-file mandate.
- Sibling artifact (format template / quality bar):
  `docs/features/active/2026-08-08-quickfiler-breadcrumb-bridge-coverage-495/research/2026-08-08T01-15-breadcrumb-bridge-coordinator.md`

---

## 0. Type disambiguation — read this before anything else

Two similarly-named router types exist. They are **different types in different assemblies with
different owners and different test suites**. The sibling artifact recorded this conflation as a
correction to its own brief; it is restated here with both anchors so the planner cannot repeat it.

| | **In scope for this artifact** | **NOT in scope** |
| --- | --- | --- |
| Type | `BreadcrumbBridgeRouter` | `FolderBreadcrumbBridgeRouter` |
| Declared at | `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:19` | `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs:10` |
| Declaration | `public sealed class BreadcrumbBridgeRouter` | `public sealed class FolderBreadcrumbBridgeRouter` |
| Namespace | `QuickFiler.Controllers` | `UtilitiesCS.OutlookObjects.Folder` |
| Assembly | **QuickFiler** | **UtilitiesCS** |
| Epic owner | **F12 (#495)** — this file | **no child of #136**; `UtilitiesCS/**` must not be edited |
| Constructor arity | 5 (`provider, host, codec, renderer, outboundQueue`) | 1 (`provider`) |
| Cobertura `<class>` | XML line 17558, `filename="QuickFiler\Controllers\BreadcrumbBridgeRouter.cs"` | XML line 126279, `filename="UtilitiesCS\OutlookObjects\Folder\FolderBreadcrumbBridgeRouter.cs"` |
| Test coverage lives in | `QuickFiler.Test/Controllers/` | `UtilitiesCS.Test/OutlookObjects/Folder/` |

### 0.1 Every reference site, by type

**`BreadcrumbBridgeRouter` (F12, this artifact's subject) — 5 non-declaring code sites:**

| Site | Kind | Owner |
| --- | --- | --- |
| `QuickFiler/Controllers/EfcFormController.cs:141` — `private BreadcrumbBridgeRouter _router;` | production field | **F9 (#452)** |
| `QuickFiler/Controllers/EfcFormController.cs:843` — `_router = new BreadcrumbBridgeRouter(...)` | sole production construction | **F9 (#452)** |
| `QuickFiler/QuickFiler.csproj:292` — `<Compile Include="Controllers\BreadcrumbBridgeRouter.cs" />` | build | shared |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs` (4 occurrences: `:16`, `:23`, `:31`, `:48`) | test | F12 |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.cs` (9 occurrences: `:16`, `:23`, `:32`, `:67`, `:233`, `:241`, `:250`, `:265`, `:280`) | test | F12 |
| `QuickFiler.Test/Controllers/EfcHomeControllerExecuteMovesTests.cs:252`, `:259` | test | **F8 (#437)** |

Doc-comment-only mentions (no code dependency): `QuickFiler/Viewers/WebView2BreadcrumbHost.cs:26` and
`:61` (**F13-owned**).

**`FolderBreadcrumbBridgeRouter` (UtilitiesCS, NOT this artifact's subject):**

`QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs:16` (doc), `:30` (field), `:52` (construction —
this is the line the F12 brief mis-attributed); `QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs:157`,
`:159`; plus five UtilitiesCS.Test files (`FolderBreadcrumbBridgeRouterTests.cs`,
`FolderBreadcrumbBridgeRouterInFlightTests.cs`, `FolderBreadcrumbBridgeRouterEdgeTests.cs`,
`FolderBreadcrumbRouterSelectionConcurrencyTests.cs`, `BreadcrumbDuplicateIdentityTests.cs`).

> **Grep hazard.** A plain `grep BreadcrumbBridgeRouter` matches `FolderBreadcrumbBridgeRouter` as a
> substring and returns 71 hits across 14 files. Only the six sites in the first table belong to
> this file. Any inventory that reports "14 referencing files" has conflated the two types.

**Measurement consequence.** The two types' emitted rates are numerically confusable:
the UtilitiesCS class emits `branch-rate="0.922222"` and the QuickFiler class emits
`branch-rate="0.926471"`. The brief's quoted 92.2% branch figure equals the **UtilitiesCS emitted
attribute** to six digits. It nevertheless happens to be the correct recomputed figure for *this*
file as well (§2) — a coincidence, not a validation. Select the `<class>` by `filename`, never by
class name or by rate.

---

## 1. Current State — verified

### 1.1 File shape

`QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` is **450 physical lines** (namespace closes at
`:450`). Against the 500-line ceiling in `.claude/rules/general-code-change.md` § File Size Limit
that is **50 lines of headroom**. The epic manifest's `(450)` figure is confirmed exactly.

- Single type: `public sealed class BreadcrumbBridgeRouter` (`:19`). **Not `partial`.** **Not
  `IDisposable`.** No base type.
- **No `[ExcludeFromCodeCoverage]` anywhere in the file** — verified by grep, 0 occurrences (not even
  a doc-comment mention). No partial-class inheritance hazard exists, because the type is not
  `partial`.
- **No `System.Windows.Forms`, no `Microsoft.Office.Interop.Outlook`, no WebView2 type.** `using`
  set is `System`, `System.Collections.Generic`, `System.Threading`, `System.Threading.Tasks`,
  `QuickFiler.Viewers`, `UtilitiesCS`, `UtilitiesCS.OutlookObjects.Folder` (`:2`-`:8`). None of the
  three `CLAUDE.md` §UT2 exemption grounds, nor the epic's ratified fourth ground, applies to this
  file. It is unambiguously `testable` in F1's ledger.
- **Constructor surface: exactly one, `public`, 5 arguments** (`:40`-`:55`), all five null-guarded
  (`:48`-`:53`). There are **no `internal` members at all** — every member is `public` or `private`.
  `QuickFiler/Properties/AssemblyInfo.cs:5` does carry
  `[assembly: InternalsVisibleTo("QuickFiler.Test")]`, so an internal seam *would* be reachable, but
  this file exposes none, so the IVT grant is irrelevant here. Non-public reach requires reflection.
- Public surface: `SelectedFolderPath` (`:58`, private setter), events `SelectedFolderPathChanged`
  (`:61`) and `FocusSearchRequested` (`:64`), and methods `BindRowsAsync` (`:74`), `SelectFirstRow`
  (`:119`), `ApplyTheme` (`:130`), `NotifyCoreInitialized` (`:140`), `ProcessInboundAsync` (`:156`).
- Private mutable state: `_rows` (`:32`), `_selectedRowId` (`:33`), `_pendingDocument` (`:34`),
  `_darkMode` (`:35`), `_requestSequence` (`:36`).

### 1.2 Collaborator table with owning child

| Symbol | Declared at | Owner |
| --- | --- | --- |
| `IBreadcrumbWebHost` | `QuickFiler/Viewers/IBreadcrumbWebHost.cs:11` | **F13 (#455)** |
| `BreadcrumbOutboundQueue` | `QuickFiler/Controllers/BreadcrumbOutboundQueue.cs:15` | **F2 (#431)** |
| `IFolderHierarchyProvider` | `UtilitiesCS/OutlookObjects/Folder/IFolderHierarchyProvider.cs:19` | UtilitiesCS (no child) |
| `BreadcrumbMessageCodec` | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs:31` | UtilitiesCS |
| `BreadcrumbMessageException` | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessageCodec.cs:14` | UtilitiesCS |
| `BreadcrumbHtmlRenderer` | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbHtmlRenderer.cs:22` | UtilitiesCS |
| `BreadcrumbRowBuilder` | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs:13` | UtilitiesCS |
| `BreadcrumbRow` | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs:34` | UtilitiesCS |
| `BreadcrumbRowKind` (enum) | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRow.cs:10` | UtilitiesCS |
| `BreadcrumbSegment` | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbSegment.cs:16` | UtilitiesCS |
| `BreadcrumbMessageTypes` (static) | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs:10` | UtilitiesCS |
| `BreadcrumbInboundMessage` | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs:38` | UtilitiesCS |
| `BreadcrumbOutboundMessage` (abstract) | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs:72` | UtilitiesCS |
| `BreadcrumbRenderMessage` | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs:90` | UtilitiesCS |
| `BreadcrumbSubfolderResultMessage` | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs:114` | UtilitiesCS |
| `BreadcrumbFocusSearchMessage` | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbMessages.cs:146` | UtilitiesCS |
| `FolderBreadcrumbSegment` | `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbSegment.cs:15` | UtilitiesCS |
| `FolderTreeNodeKey` | `UtilitiesCS/OutlookObjects/Folder/FolderTreeNodeKey.cs:9` | UtilitiesCS |
| `FolderScore` | `UtilitiesCS/OutlookObjects/Folder/FolderScore.cs` | UtilitiesCS |

Three consequences the planner must carry:

1. **The message/serializer surface is entirely UtilitiesCS-owned and 100% public.** Every message
   type, the codec, the renderer, the row model and the row builder are `public` types with `public`
   constructors, so a QuickFiler test can build any inbound/outbound shape and any `BreadcrumbRow`
   directly, with **no reflection into UtilitiesCS** and with no dependence on the missing
   `UtilitiesCS -> QuickFiler.Test` internals grant (epic § "Cross-Child Constraints" 2).
2. **Only two collaborators are QuickFiler types, and F12 owns neither.** `IBreadcrumbWebHost` is
   F13's; `BreadcrumbOutboundQueue` is F2's. Both are already injected through the constructor, so
   F12 needs no new seam — but F12 also may not edit either (§8).
3. **The only production consumer is F9's `EfcFormController`.** `SelectedFolder` at
   `EfcFormController.cs:289`-`:294` is literally `_router?.SelectedFolderPath`, and it feeds the
   real mail-move calls at `:493` and `:772` and the folder-open calls at `:478`, `:722`, `:760`.
   The router's selection state is therefore a production filing target, not a display detail. This
   is what makes LD-2 (§7) material rather than cosmetic.

### 1.3 Concurrency and determinism inventory

Verified by direct read of all 450 lines plus a targeted grep for
`ConfigureAwait|lock |Interlocked|volatile|DateTime|Stopwatch|Timer|Task\.Delay|Thread\.Sleep|TimeProvider|SynchronizationContext|IDisposable|Dispose|CancellationToken|TaskCompletionSource`:

| Construct | Present? | Anchors |
| --- | --- | --- |
| `lock` | **No** | — |
| `Interlocked` | **No** | — |
| `volatile` | **No** | — |
| Timer of any kind | **No** | — |
| `SynchronizationContext` | **No** | — |
| `ConfigureAwait` | **No — zero occurrences** | every `await` in the file is context-capturing |
| Fire-and-forget discard (`_ =`) | **No** | — |
| `TaskCompletionSource` | **No** | — |
| `IDisposable` / `Dispose` | **No** | — |
| Disposal flag / re-entrancy guard | **No** | — |
| `CancellationToken` | Yes, 4 sites | `:77` (param), `:298`, `:309` (`CancellationToken.None`), `:336` (param) |
| `async` methods | 6 | `:74` `BindRowsAsync`, `:156` `ProcessInboundAsync`, `:187` `OnHostMessageReceived` (**`async void`**), `:200` `HandleLeafToggleAsync`, `:225` `HandleArrowKeyAsync`, `:285` `ExpandLeafAsync`, `:334` `FetchChainAsync` |
| `await` expressions | 6 | `:99`, `:176`, `:179`, `:191`, `:222`, `:239`, `:296`, `:309`, `:341`, `:350` |
| Event subscription without unsubscription | Yes | `:54` `_host.MessageReceived += OnHostMessageReceived` — never detached (LD-4) |
| Mutable un-synchronised counter | Yes | `:293` `"req-" + (++_requestSequence)` |

**Determinism finding — the brief's "injected clock and fake timers" instruction is REFUTED for this
file, exactly as the sibling artifact refuted it for `BreadcrumbBridgeCoordinator.cs`.**

A grep of this file for `DateTime|Stopwatch|Timer|Task.Delay|Thread.Sleep|TimeProvider` returns
**zero matches**. There is no time dependency of any kind to control, and no timer to fake. This
adopts F13's ratified ruling at
`docs/features/active/2026-08-07-quickfiler-breadcrumb-dropdown-webview-coverage-455/spec.md:383-390`
(§8.1: "Determinism here is **scheduler** control, not clock control. Any plan task that introduces
an injected clock or a fake-timer facility is out of scope and must be rejected — it would add a
seam with no dependency to control.").

Determinism for *this* file is weaker still than "scheduler control": **there is no scheduler
dependency either**. Every asynchronous edge is driven by an injected `Mock<IFolderHierarchyProvider>`
whose `ReturnsAsync`/`ThrowsAsync` setups produce already-completed tasks, so every `await` resumes
inline and every public entry point completes synchronously before it returns. The existing suite
relies on precisely this: `BreadcrumbBridgeRouterTests.cs:110-118` and `:122` use
`.GetAwaiter().GetResult()` with no polling, no `WaitForPost`, no context install.

`docs/features/active/2026-08-08-quickfiler-breadcrumb-bridge-coverage-495/spec.md:69-70` and `:112`,
and `issue.md:70-71` and `:95`, still carry the superseded "injected clock and fake timers" phrasing.
**It must be struck and replaced with the statement above, recorded as a documented deviation.**

Deterministic vehicles that already exist and are green for this file:

1. `Mock<IFolderHierarchyProvider>` with `ReturnsAsync` / `ThrowsAsync`
   (`BreadcrumbBridgeRouterTests.cs:85-105`, `BreadcrumbBridgeRouterQueueTests.cs:49-66`).
2. `Mock<IBreadcrumbWebHost>` with `SetupGet(h => h.IsCoreInitialized)` backed by a plain test field
   (`BreadcrumbBridgeRouterQueueTests.cs:42`) — the initialization gate is toggled by assignment,
   not by waiting.
3. `Callback<string>` capture lists `_navigated` / `_posted`
   (`BreadcrumbBridgeRouterTests.cs:41-46`) — assertions read a list, never poll.
4. `_host.Raise(h => h.MessageReceived += null, _host.Object, json)`
   (`BreadcrumbBridgeRouterQueueTests.cs:201`) — synchronous event injection at the host seam.
5. Raw-JSON string assertions (`BreadcrumbBridgeRouterTests.cs:58-61`, `:148`), because
   **`QuickFiler.Test` deliberately carries no Newtonsoft reference** — verified: no
   `Reference Include="Newtonsoft..."` exists in `QuickFiler.Test/QuickFiler.Test.csproj`. New tests
   must assert on JSON text, not on a parsed object graph.
6. Private-member reflection precedent inside the same folder:
   `EfcHomeControllerExecuteMovesTests.SetPrivateField` (`:278-285`).

---

## 2. Measured Baseline — independently recomputed, not read

Source: `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`.

**Selection.** Exactly **one** `<class>` element carries `filename="QuickFiler\Controllers\BreadcrumbBridgeRouter.cs"`
(XML line 17558). A grep for that filename across the whole report returns one hit, so **no
compiler-generated `<>c` closure class shares this filename and no cross-class union is required**.
This was checked explicitly per the epic's harness rule 1; it is not an assumption.

**Extraction.** The class block spans XML lines 17558-18262. `</methods>` closes at XML line 17818,
so the **class-level `<lines>` block is XML lines 17819-18261** and every figure below is computed
from those `<line>` elements only. `class.iter('line')` and `.//lines/line` were not used (#441);
the emitted `line-rate` / `branch-rate` attributes were not read (epic directive 5).

| Metric | Recomputed value |
| --- | --- |
| Coverable lines (class-level `<line>` count) | **282** |
| Lines with `hits="0"` | **6** |
| Line coverage | **276 / 282 = 97.87%** |
| Branching lines (`branch="True"`) | 38 |
| Branch outcomes (sum of `condition-coverage` denominators) | **90** |
| Branch outcomes taken (sum of numerators) | **83** |
| Branch coverage | **83 / 90 = 92.22%** |

**Verdict on the brief: 282 lines / 97.9% line / 92.2% branch is CONFIRMED to the stated precision.**
This is the first sibling child in the epic to confirm its brief's coverage table rather than correct
it. The corrections found in this research are elsewhere (§10).

**Both emitted attributes are wrong and must never be cited.** The `<class>` element emits
`line-rate="0.982544"` and `branch-rate="0.926471"`:

- `0.982544` is not `276/282` (0.978723). It corresponds to a denominator inflated by the
  method-level `<lines>` double-count filed as #441.
- `0.926471` = `63/68`, i.e. it reports 68 branch outcomes where the class-level block contains 90.
  The true figure (92.22%) is **lower** than the emitted one — the same "falsely passing" direction
  F10 documented for `MailActions.cs`. Here the file passes on the true figure too, but the
  distortion is real.

**Gate status.** Both floors are cleared today: 97.87% >= 80% line (issue #136 AC1) and
92.22% >= 75% branch (`.claude/rules/general-unit-test.md`). `branches-valid` is 90, not 0, so the
epic's zero-branch N/A rule does not apply. The bar for F12 on this file is therefore
**retain-or-improve on both axes**, plus closure of the residual outcomes where they are reachable
and behaviorally meaningful.

### 2.1 The six uncovered lines, exactly

| Source line | Statement | Enclosing member |
| --- | --- | --- |
| `:192` | `}` — normal (non-throwing) exit of the `try` block | `OnHostMessageReceived` (`:187`) |
| `:356` | `catch (Exception ex)` | `FetchChainAsync` (`:334`) |
| `:357` | `{` | `FetchChainAsync` |
| `:359` | `log.Error($"Breadcrumb chain fetch failed for '{folderPath}': {ex.Message}", ex);` | `FetchChainAsync` |
| `:360` | `return null;` | `FetchChainAsync` |
| `:434` | `return -1;` | `IndexOf` (`:424`) |

No other line in the file has `hits="0"`.

### 2.2 Complete branch-point census (all 38 branching lines)

**Fully covered — 34 lines, all `condition-coverage="100%"`:**
`:48`, `:49`, `:50`, `:51`, `:52` (the five constructor null-guards), `:80`, `:88`, `:103`, `:111`,
`:122`, `:142`, `:160`, `:169`, `:202`, `:204`, `:212`, `:214`, `:230`, `:232`, `:237`, `:244`,
`:265`, `:269`, `:279`, `:300`, `:345`, `:366`, `:379`, `:400`, `:413`, `:415`, `:428`,
`:439` (4/4 — both `i >= 0` and `i < _rows.Count` outcomes already observed), `:441`.

**Partial — 4 lines, 7 untaken outcomes:**

| Line | Construct | `condition-coverage` | Per-condition | Untaken side, determined from `hits` evidence |
| --- | --- | --- | --- | --- |
| `:90` | `if (text == null \|\| chains.ContainsKey(text) \|\| BreadcrumbRowBuilder.Classify(text) != BreadcrumbRowKind.Suggestion)` (spans `:90`-`:94`) | 66.67% (4/6) | c0 50%, c1 50%, **c2 100%** | c0 `text == null` **true** side; c1 `chains.ContainsKey(text)` **true** side. c2 is already both-ways because `:96` (`continue`) has `hits="1"` from the banner row in `BreadcrumbBridgeRouterTests.Bind()` (`:112`) and `:99` has `hits="1"` from the suggestion row. |
| `:288` | `if (row.Kind != BreadcrumbRowKind.Suggestion \|\| leaf?.HasSubfolders != true)` | 66.67% (4/6) | c0 50%, c1 50%, **c2 100%** | c0 `Kind != Suggestion` **true** side; c1 the `leaf?.` **null** side. c2 is both-ways because `:290` (`return`) and `:293` both have `hits="1"` — the `return` was reached via `HasSubfolders != true` in `LeafExpand_OnLeafWithoutSubfolders_IsNoOpWithoutProviderQuery` (`BreadcrumbBridgeRouterQueueTests.cs:396-425`), and `:296`-`:299` have `hits="1"`, proving `leaf` was non-null on the path taken. |
| `:372` | `SelectedFolderPath = row.Kind == TrashPseudoRow ? TrashRowText : row.LeafSegment?.FullPath ?? string.Empty;` (spans `:372`-`:375`) | 66.67% (4/6) | **c0 100%**, c1 50%, c2 50% | c1 the `row.LeafSegment?` **null** side; c2 the `??` **left-is-null** side. c0 (the ternary) is both-ways because `RowSelected_OnTrashPseudoRow_SelectsTrashPath` (`BreadcrumbBridgeRouterTests.cs:415-433`) takes the Trash arm and eight other tests take the path arm. |
| `:426` | `for (int i = 0; i < _rows.Count; i++)` in `IndexOf` | 50% (1/2) | c0 50% | the **loop-exhaustion** side (`i >= _rows.Count`). Every observed call returned early at `:430`, which is why `:434` has `hits="0"` — the two are one gap. |

**Line-number drift: none.** All six uncovered lines and all four partial branch lines re-anchor
exactly onto the constructs above in the current working-tree file at
`feature/quickfiler-breadcrumb-bridge-coverage-r2`. No re-anchoring is required.

---

## 3. Gap Inventory — six atomic test groups

7 untaken branch outcomes + 6 uncovered lines. Grouped J1-J6.

### J1 — `BindRowsAsync` input-edge guards: null row text and duplicate suggestion text (`:90` c0, c1) — 2 outcomes

**Construct.** The dedup/skip filter at `:90`-`:97` inside the `foreach` over `presentedRows`.

**Why untaken today.** Both existing binders pass short, distinct, non-null arrays:
`BreadcrumbBridgeRouterTests.Bind()` passes `{ "==== SUGGESTIONS ====", LeafPath }`
(`BreadcrumbBridgeRouterTests.cs:112`), `BindThreeRows()` passes three distinct strings (`:296`),
and `BreadcrumbBridgeRouterQueueTests.Bind()` passes `{ LeafPath }` (`:90`). `BindRowsAsync_NullPresentedRows_ThrowsArgumentNullException`
(`BreadcrumbBridgeRouterQueueTests.cs:294-306`) is the test that comes closest — it nulls the
**collection**, which trips the guard at `:80` and returns before the loop is entered. Nothing nulls
an **element**, and nothing repeats an element.

**Reachability: fully reachable via the public API. No production change.**

**J1a — null element.**
*Arrange:* the standard `BreadcrumbBridgeRouterQueueTests` harness.
*Act:* `await router.BindRowsAsync(new string[] { null, LeafPath }, Array.Empty<FolderScore>(), CancellationToken.None)`.
*Assert:* no exception; `_provider.Verify(p => p.ResolveLeafKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once)` — the provider is queried for `LeafPath` only; and the rendered document contains two rows (`row-0`, `row-1`).
*Contract pinned:* **a null presented row is skipped for chain resolution but is still rendered as a
row**, so row indices stay aligned with the presented list. This matters because `row-<index>` ids
(`BreadcrumbRowBuilder.cs:56`) are the correlation key for every inbound message; if a null row were
dropped instead of skipped, every subsequent `rowId` would be off by one. Note the safety this
guard provides: `BreadcrumbRowBuilder.Classify` throws `ArgumentNullException` on null
(`BreadcrumbRowBuilder.cs:152-155`), so the `text == null` short-circuit at `:91` is the only thing
preventing a throw at `:93`.

**J1b — duplicate suggestion text.**
*Act:* `await router.BindRowsAsync(new[] { LeafPath, LeafPath }, Array.Empty<FolderScore>(), CancellationToken.None)`.
*Assert:* `_provider.Verify(ResolveLeafKeyAsync, Times.Once)` **and**
`_provider.Verify(GetAncestorChainAsync, Times.Once)`; the document contains both `row-0` and
`row-1`, each rendering the full `Inbox / Projects / Alpha` chain.
*Contract pinned:* **the chain cache deduplicates provider round-trips across repeated suggestion
text while still binding every presented row.** This is the file's only I/O-amplification control;
without it a suggestion list with N repeats issues N provider calls per bind, and `BindFolderRows`
runs on every `SearchText.TextChanged` (`EfcFormController.cs:873-883`).

### J2 — provider failure during bind degrades one row instead of failing the bind (`:356`, `:357`, `:359`, `:360`) — 4 lines

**Construct.** The non-cancellation catch arm of `FetchChainAsync` (`:356`-`:361`).

**Why untaken today.** Two existing tests reach the neighbourhood and stop short:
- `Bind_WhenProviderCanceled_PropagatesCancellation` (`BreadcrumbBridgeRouterQueueTests.cs:351-366`)
  throws `OperationCanceledException` from `ResolveLeafKeyAsync`, which is caught one arm earlier at
  `:352`-`:355` and rethrown. It never reaches `:356`.
- `ProviderFailure_OnLeafExpand_LeavesRowStateUnchanged` (`:126-148`) throws
  `InvalidOperationException`, but from `GetImmediateSubfoldersAsync`, which is only called from
  `ExpandLeafAsync` and is caught by the *other* generic catch at `:324`-`:331` (already covered).

No test throws a non-`OperationCanceledException` from `ResolveLeafKeyAsync` or
`GetAncestorChainAsync` during a **bind**.

**Reachability: fully reachable via the public API. No production change.**

*Arrange:* two presented suggestion rows; `ResolveLeafKeyAsync` returns a valid key for both;
`GetAncestorChainAsync` is set up to **throw `InvalidOperationException("hierarchy unavailable")` for
the first path and return a normal two-segment chain for the second** (Moq supports this with a
path-discriminating `ReturnsAsync`/`ThrowsAsync` pair, exactly as `BindThreeRows` already
discriminates on `k.FolderPath` at `BreadcrumbBridgeRouterTests.cs:285-292`).
*Act:* `await router.BindRowsAsync(...)`; `_initialized = true` before the call so the document is
navigated rather than deferred.
*Assert:* `BindRowsAsync` **does not throw**; `_navigated` has exactly one entry; that document
renders the failing row as a **single leaf-only segment** (assert it contains the leaf token but not
the ancestor display name), while the healthy row renders its **full chain**.
*Contract pinned:* **a provider fault is isolated to the one suggestion that faulted; the bind
completes and every other row keeps its full breadcrumb.** That is the exact difference between this
catch arm and the rethrowing `OperationCanceledException` arm above it, and it is the behavioural
justification for the two arms existing separately. A negative assertion belongs here too:
`_posted` must contain no error payload — the degradation is silent by design, logged only.

### J3 — the host-event path routes a valid message, not only malformed ones (`:192`) — 1 line

**Construct.** `:192` is the closing brace of the `try` in `async void OnHostMessageReceived`
(`:187`-`:198`) — the sequence point reached only when `await ProcessInboundAsync(json)` completes
**without** throwing.

**Why untaken today.** `MalformedInboundJson_ViaHostEvent_IsContainedAtTheBoundary`
(`BreadcrumbBridgeRouterQueueTests.cs:193-205`) is the **only** test in the repository that raises
`IBreadcrumbWebHost.MessageReceived`, and it raises it with `"{not valid json"`. The `try` therefore
always terminates through the `catch`, which is why `:193`, `:194` and `:197` all have `hits="1"`
while `:192` has `hits="0"`. Every other inbound test calls the public `ProcessInboundAsync`
directly (`BreadcrumbBridgeRouterTests.cs:122`, `BreadcrumbBridgeRouterQueueTests.cs:100`),
bypassing the handler entirely.

**Reachability: fully reachable via the public API. No production change.** `MessageReceived` is a
plain `EventHandler<string>` on the injected `IBreadcrumbWebHost` seam
(`IBreadcrumbWebHost.cs:22`), and `Mock.Raise` is already used at
`BreadcrumbBridgeRouterQueueTests.cs:201`.

**Determinism note (important).** `OnHostMessageReceived` is `async void`, but a `rowSelected`
payload traverses `ProcessInboundAsync` with **no `await` executed** — `DeserializeInbound` (`:158`),
`FindRow` (`:159`) and `SelectRow` (`:182`) are all synchronous — so the returned `Task` is already
completed and the `await` at `:191` resumes inline. State mutation and the execution of `:192` both
occur before `_host.Raise(...)` returns. No pump, no polling, no `WaitFor` helper is needed. Prefer
`rowSelected` over `leafExpandToggle` for this test precisely because it removes even the
completed-task continuation question.

*Arrange:* initialized host, `Bind()`, subscribe to `SelectedFolderPathChanged`.
*Act:* `_host.Raise(h => h.MessageReceived += null, _host.Object, "{\"type\":\"rowSelected\",\"rowId\":\"row-0\"}")`.
*Assert:* `router.SelectedFolderPath.Should().Be(LeafPath)`; the event fired once; `_posted` contains
a `"type":"render"` payload.
*Contract pinned:* **the host event is a live routing path, not merely an exception firewall.** Today
the suite proves only that the boundary swallows bad input; it never proves that a good message
posted by the page actually reaches the router's state machine. That is a genuine gap in the spec
coverage of the bridge, not a coverage artefact.

### J4 — leaf-expand on a non-suggestion row is a documented no-op (`:288` c0) — 1 outcome

**Construct.** `row.Kind != BreadcrumbRowKind.Suggestion` at `:288`, the first disjunct of the
`ExpandLeafAsync` guard.

**Why untaken today.** Every `leafExpandToggle` in the suite targets a suggestion row: `row-1` in
`BreadcrumbBridgeRouterTests` (`:161`, `:339`, `:358`, `:361`) because `Bind()` puts the banner at
`row-0` and the suggestion at `row-1`; and `row-0` in `BreadcrumbBridgeRouterQueueTests` (`:143`,
`:167`, `:382`, `:413`) because that harness binds a single suggestion. No test ever sends
`leafExpandToggle` at a **banner** or **trash** row, so the guard's first disjunct has only ever
evaluated false.

**Reachability: fully reachable via the public API. No production change.**

*Arrange:* `await router.BindRowsAsync(new[] { "==== SUGGESTIONS ====", BreadcrumbRowBuilder.TrashRowText, LeafPath }, ...)` — this yields `row-0` Banner, `row-1` TrashPseudoRow, `row-2` Suggestion
(classification per `BreadcrumbRowBuilder.Classify`, `BreadcrumbRowBuilder.cs:150-168`). Record
`_posted.Count`.
*Act:* two inbound messages — `leafExpandToggle` at `row-0`, then at `row-1`. Use a
`[DataTestMethod]` with two `[DataRow]`s so each row kind is independently diagnosable.
*Assert:* `_posted.Count` unchanged; `_provider.Verify(GetImmediateSubfoldersAsync, Times.Never)`;
`router.SelectedFolderPath` still null.
*Contract pinned:* **banner and trash rows never issue a provider round-trip.** This is the
router-level counterpart of the model-level rule at `BreadcrumbRow.cs:260-263`
(`CanExpandLeaf`), and it is the guard that stops a stray page-side expand affordance on a section
header from hitting Outlook's folder tree. Note the interaction the test also documents: for a
TrashPseudoRow, `row.LeafSegment` is null (`BreadcrumbRow.cs:91-92`, and
`BreadcrumbRowBuilder.cs:111-117` gives trash rows `Array.Empty<BreadcrumbSegment>()`), yet `:288`
short-circuits on the first disjunct and never dereferences it — so J4 closes c0 **without** closing
c1. c1 needs J5.

### J5 — a segment-less suggestion row selects to empty string rather than throwing (`:288` c1, `:372` c1, `:372` c2) — 3 outcomes

**Construct.** The two defensive null-conditionals on `BreadcrumbRow.LeafSegment`: `leaf?.` at `:288`
and `row.LeafSegment?.FullPath ?? string.Empty` at `:375`.

**Why untaken today — and why no public arrangement can reach them.** `_rows` is assigned in exactly
two places: `:32` (`Array.Empty<BreadcrumbRow>()`) and `:109` (`_builder.BuildRows(...)`). Reading
`BreadcrumbRowBuilder.BuildRow` (`BreadcrumbRowBuilder.cs:76-142`) establishes an invariant that
makes both conditions dead through the public surface:

- Banner rows get **exactly one** segment (`:104`-`:109`) — and are rejected earlier at `:366`
  anyway, so they never reach `:372`.
- TrashPseudoRow gets **zero** segments (`:111`-`:117`) — so `LeafSegment` **is** null, but `:372`'s
  ternary takes the Trash arm at `:374` and never evaluates `LeafSegment`, and `:288` short-circuits
  on `Kind != Suggestion`.
- Suggestion rows get **at least one** segment unconditionally, because the empty-chain fallback at
  `:121`-`:129` injects a synthetic single segment.

So a `Suggestion` row with zero segments — the only shape that reaches these two conditions — is not
producible by the builder. The `BreadcrumbRow` constructor permits it (`BreadcrumbRow.cs:47-60`
accepts any `IEnumerable<BreadcrumbSegment>`), so the shape is legal in the model but unreachable
through `BindRowsAsync`.

**Reachability: reachable only by seeding the private `_rows` field via reflection.** This is the
same class of judgement call the sibling artifact made for its G3, and it reuses the reflection
precedent already committed in the same test folder
(`EfcHomeControllerExecuteMovesTests.SetPrivateField`, `:278-285`). No UtilitiesCS internals are
touched — `BreadcrumbRow` and `BreadcrumbSegment` are both `public sealed` with public constructors.

*Arrange:* build the router normally, then
`SetPrivateField(router, "_rows", new BreadcrumbRow[] { new BreadcrumbRow("row-0", BreadcrumbRowKind.Suggestion, Array.Empty<BreadcrumbSegment>(), null) })`.
*Act (two acts, one arrangement — recommend two `[TestMethod]`s sharing a private factory):*
1. `await router.ProcessInboundAsync("{\"type\":\"leafExpandToggle\",\"rowId\":\"row-0\"}")` — closes `:288` c1.
2. `router.SelectFirstRow()` — closes `:372` c1 and c2.
*Assert:* (1) no exception; `_provider.Verify(GetImmediateSubfoldersAsync, Times.Never)`; `_posted`
unchanged. (2) `router.SelectedFolderPath.Should().Be(string.Empty)` — explicitly **empty, not
null**; and `SelectedFolderPathChanged` fired with `string.Empty`.
*Contract pinned:* **a row whose breadcrumb has no segments is inert for expansion and selects to
the empty string, never to `null` and never with a `NullReferenceException`.** The empty-vs-null
distinction is behaviourally load-bearing, not cosmetic: `EfcFormController.SelectedFolder` is
`_router?.SelectedFolderPath` (`:294`), and its consumers branch on emptiness — the difference
between `""` and `null` at `EfcFormController.cs:478`, `:722`, `:760` determines whether a
folder-open is attempted. Asserting `""` pins the contract that the `?? string.Empty` fallback is
deliberate.

*If the planner rejects reflection:* these 3 outcomes stay open and branch coverage lands at
86/90 = 95.56% instead of 89/90 = 98.89%. Both are comfortably above the 75% floor, so this is a
quality choice, not a gate risk. Do **not** "solve" it by adding a production overload or by
loosening `_rows`' accessibility — that is a behaviour-surface change for zero user benefit.

### J6 — a row absent from the current row set is treated as the top row (`:426` loop-exit, `:434`) — 1 outcome + 1 line

**Construct.** `IndexOf` (`:424`-`:435`) returning `-1`, and the downstream contract in
`HandleUpArrow` (`:262`-`:274`): `FindSelectable(IndexOf(row) - 1, step: -1)` with `IndexOf == -1`
evaluates `FindSelectable(-2, -1)`, whose loop guard `i >= 0` (`:439`, already 4/4) fails
immediately, returning `null` — which routes to the focus-search hand-back at `:268`-`:270`.

**Why untaken today.** `IndexOf` is called from exactly two places, `HandleUpArrow` (`:264`) and
`MoveSelection` (`:278`), and both receive a `row` that `FindRow` (`:411`-`:422`) just returned from
`_rows`. `FindRow` returns an element **of** `_rows`, and `IndexOf` scans the same `_rows` field
with `ReferenceEquals`. Critically, **there is no suspension point between the two**: for the `Up`
and `Down` cases, `ProcessInboundAsync`'s `await HandleArrowKeyAsync(...)` at `:179` runs
synchronously through `:251`/`:254` to `IndexOf` before yielding. So `_rows` cannot be swapped in the
window, and the miss is structurally unreachable through the public API. The tests that come closest
are `ArrowKeyUp_AtTopSelectableRow_PostsFocusSearchAndRaisesEvent`
(`BreadcrumbBridgeRouterTests.cs:197-211`), which reaches the focus-search branch via a genuine
`IndexOf == 1` and a banner at index 0, and `ArrowKeyUp_OnNonTopRow_...` (`:317-329`).

**Reachability: reachable only via reflection on the private `HandleUpArrow` (or `IndexOf`) member.**

*Arrange:* bind a normal three-row set; separately construct a `BreadcrumbRow` that is **not** in
`_rows` (`new BreadcrumbRow("row-stale", BreadcrumbRowKind.Suggestion, new[] { new BreadcrumbSegment("X", "X", false) }, null)`).
Subscribe to `FocusSearchRequested`.
*Act:* reflectively invoke the private instance method `HandleUpArrow` with the foreign row.
*Assert:* `_posted` gained exactly one payload containing `"type":"focusSearch"`; the
`FocusSearchRequested` event fired once; `router.SelectedFolderPath` is unchanged.
*Contract pinned:* **an Up arrow against a row that is no longer part of the current row set degrades
to the top-of-list behaviour — hand focus back to the search box — rather than selecting an
arbitrary row or throwing.** This is the defensive contract of the `-1` sentinel, and it is exactly
the state a stale page-side DOM would produce after a re-bind. Prefer invoking `HandleUpArrow` over
invoking `IndexOf` directly: asserting `IndexOf(foreign) == -1` is a shape assertion of the kind the
epic prohibits, whereas asserting the focus-search hand-back is a behavioural assertion that happens
to traverse `:426` and `:434`.

*If the planner rejects reflection:* `:434` stays uncovered (281/282 = 99.65% line) and `:426` stays
1/2. Still far above both floors.

### 3.1 Projected result

| Scope | Line | Branch |
| --- | --- | --- |
| Baseline (measured) | 276/282 = **97.87%** | 83/90 = **92.22%** |
| After J1-J4 only (no reflection) | 281/282 = **99.65%** | 86/90 = **95.56%** |
| After J1-J6 (recommended) | 282/282 = **100.00%** | 90/90 = **100.00%** |
| Floor | >= 80% | >= 75% |

No branch outcome and no line in this file is unreachable given the reflection allowance. **Zero
documented deviations are required for reachability.** The only judgement calls are J5 and J6, both
of which are reachable but only through private-member reflection with an in-folder precedent.

---

## 4. Production Edit Verdict

**No production edit to `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` is required or
recommended. This child is tests-only for this file.**

Grounds:

- Every collaborator is already an injected seam supplied by the single public constructor
  (`:40`-`:46`). There is nothing to extract.
- The file contains no `[ExcludeFromCodeCoverage]` to remove and is not `partial`, so neither the
  partial-type propagation trap (`QfcDatamodel.cs:25`, `ItemViewer.cs`) nor the **#457 lambda-leak
  trap** applies. Recorded for completeness per the brief: had a thin-forwarder adapter been
  required, it would have to carry a **type-level** attribute on a type that is `sealed` and
  **not `partial`** (epic § "Measurement Trap", § "fourth exemption ground" condition 4). It is not
  required here.
- The **50 lines of headroom (450/500) are not consumed.** No new member, no new type, no new file.
- **No `QuickFiler/QuickFiler.csproj` edit is needed**, because no production file is created. The
  epic's "Mid-Wave File Creation" ledger-row obligation and its >= 90% new-file target therefore do
  not engage for this file. (Should a later cycle change that verdict, the obligations are: one
  `<Compile Include=...>` entry in `QuickFiler/QuickFiler.csproj`, own entries only, minimal adjacent
  hunks, **CRLF preserved via the Edit tool — never `sed -i`**, plus an appended ledger row.)
- The epic NFR is no behaviour change. All three latent defects in §7 would require behaviour
  changes to fix and are therefore promotion candidates, not edits.

Rejected alternatives, for the record:

1. **Add a `segmentIndex` range guard at `:169`** to close LD-1. Correct fix, wrong child — it is a
   behaviour change under a no-behaviour-change NFR, and it would need its own regression test per
   the Bugfix Workflow. Promote as an issue (§7).
2. **Reset `SelectedFolderPath` in `BindRowsAsync`** to close LD-2. Same objection, and it changes an
   observable production contract consumed by `EfcFormController.SelectedFolder`.
3. **Make `IndexOf`/`HandleUpArrow` `internal`** so J5/J6 avoid reflection. This would widen the type's
   surface (the file currently has zero internal members) purely for test convenience, contrary to
   `CLAUDE.md` §C#5.2 "prefer `internal` for non-public APIs" read in the other direction — the
   members are genuinely private implementation. Reflection is the lower-impact choice and has
   in-folder precedent.

---

## 5. Retain-or-Improve Risk Analysis

### 5.1 The full existing test surface

Contrary to a naive grep (which returns 14 files by matching `FolderBreadcrumbBridgeRouter` as a
substring), **exactly three test files reference this type**:

| Test file | Occurrences | Owner | Relationship to this file |
| --- | --- | --- | --- |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs` | 4 (`:16`, `:23`, `:31`, `:48`) | **F12** | primary — 16 `[TestMethod]`s, happy paths |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.cs` | 9 (`:16`, `:23`, `:32`, `:67`, `:233`, `:241`, `:250`, `:265`, `:280`) | **F12** | primary — 14 `[TestMethod]`s, negative/edge paths |
| `QuickFiler.Test/Controllers/EfcHomeControllerExecuteMovesTests.cs` | 2 (`:252`, `:259`) | **F8 (#437)** | **incidental** |

Total 30 `[TestMethod]` declarations across the two primary files (verified by grep count: 16 + 14).

### 5.2 R1 (highest) — `EfcHomeControllerExecuteMovesTests` covers this file incidentally and is F8-owned

`CreateSelectedRouter` (`EfcHomeControllerExecuteMovesTests.cs:252-276`) constructs a **real**
`BreadcrumbBridgeRouter` over `Mock<IBreadcrumbWebHost>` and `Mock<IFolderHierarchyProvider>`, drives
`BindRowsAsync` and `SelectFirstRow`, and injects the result into an uninitialized
`EfcFormController` via `SetPrivateField(formController, "_router", ...)` (`:242`).

Its real target is `EfcHomeController.ExecuteMoves`. Coverage of this file is a side effect. The
comment at `:239-241` explains why it exists: `SelectedFolder` "now derives from the breadcrumb
router's selection tracking (#349)", replacing a removed `_selectedNode` field. So the coupling is
deliberate but fragile: **the moment F8 (or F9) reinstates a simpler selection seam, this fixture is
the obvious thing to delete.**

*Which lines lose coverage if it is retired?* Not many uniquely — `SelectFirstRow` (`:119`-`:126`),
`FindSelectable` (`:437`-`:448`) and the `Mock` provider path through `BindRowsAsync` are all also
exercised by `BreadcrumbBridgeRouterTests.SelectFirstRow_SelectsTopSelectableRowAndPostsRender`
(`:245-258`). The measured impact of losing it is therefore near zero. **Severity: low impact, but
name it in the plan** as an AC that F12's own coverage must not depend on an F8-owned fixture, so a
future F8 change cannot silently regress this file.

### 5.3 R2 — `BreadcrumbBridgeRouterQueueTests.cs` is a shared file with F2 (#431)

That file's own doc comment (`:15-21`) states it covers "`BreadcrumbBridgeRouter` **and**
`BreadcrumbOutboundQueue`", and one of its 14 tests —
`OutboundQueue_NullArguments_ThrowArgumentNullException` (`:207-220`) — targets
`BreadcrumbOutboundQueue` exclusively. `QuickFiler/Controllers/BreadcrumbOutboundQueue.cs` is
assigned to **F2**, not F12 (epic § F2 file list).

Consequences:
- F2 has a legitimate reason to edit this F12-adjacent test file, creating a **fan-in conflict
  surface** beyond the two csproj files the epic already flags.
- If F2 moves the queue tests into a dedicated `BreadcrumbOutboundQueueTests.cs`, it may also move
  the shared `Setup()` fixture (`:34-74`), which is what `_router` and all 13 remaining tests depend
  on. That would be a genuine regression vector for this file.

**Mitigation for the plan:** put new F12 tests in a **new** file (§6) rather than appending to
`BreadcrumbBridgeRouterQueueTests.cs`, so F12's additions are on a surface F2 has no reason to touch.

### 5.4 R3 — open issue #440 will change the arrow-key contract this file implements

`https://github.com/drmoisan/TaskMaster/issues/440` (**open**, "Bug:
breadcrumb-left-right-arrow-parent-child-navigation") names, verbatim, the Efc surface
"`EfcViewer.FolderListBox` via `BreadcrumbBridgeRouter` and `BreadcrumbRow`". The defect is that
Left collapses the breadcrumb instead of selecting the parent node, and Right expands leaf children
instead of expanding the selected node. The fix will change `HandleArrowKeyAsync` (`:225`-`:260`)
and the `BreadcrumbRow` transitions it calls.

This is **not** a duplicate of anything in §7; it is a *constraint on F12's test design*:

- Do not add new tests that further pin the **current** Left/Right semantics. Six existing tests
  already do (`ArrowKeyRight_ThenLeft_ExpandsAndCollapses`, `ArrowKeyRight_WhenCollapsed_...`,
  `LeafExpandToggle_*`), and #440 will have to rewrite them.
- J4's non-suggestion no-op and J6's stale-row focus hand-back are **safe** under #440, because both
  assert guard behaviour that survives any parent/child re-interpretation of the arrow keys.
- Record in the plan that #440, when scheduled, will collide semantically with
  `BreadcrumbBridgeRouterTests.cs`. This is the same class of hazard the epic documents for #426
  vs F4.

### 5.5 R4 — reflective coupling into private members

- **Existing, into this type:** `EfcHomeControllerExecuteMovesTests.cs:242` sets the private field
  `"_router"` on `EfcFormController` (an F9 member, not this file's). There is currently **no**
  reflection into `BreadcrumbBridgeRouter`'s own privates.
- **Introduced by this research's recommendation:** J5 reads/writes the private field `_rows`
  (`:32`); J6 resolves the private method `HandleUpArrow` (`:262`). Both break at **runtime**, not
  compile time, if renamed. This is a new coupling and must be recorded as such — the plan should
  add an explicit note that `_rows` and `HandleUpArrow` are now test-anchored names.
- **Out of this file:** none. No test uses `typeof(BreadcrumbBridgeRouter).Assembly` as an assembly
  handle (unlike the `BreadcrumbPopupPlacementTests.cs:140` pattern the sibling artifact found).

### 5.6 R5 — F13 owns `IBreadcrumbWebHost`, F2 owns `BreadcrumbOutboundQueue`

Both are constructor parameters of this file. F13's spec commits to **no public or internal signature
changes** across its 15 files (`.../455/spec.md:49-50`), which protects the `IBreadcrumbWebHost`
contract this file compiles against. No equivalent written commitment exists from F2 regarding
`BreadcrumbOutboundQueue`'s `PostOrQueue` / `OnInitializationCompleted` / `PendingCount` surface
(`BreadcrumbOutboundQueue.cs:29`, `:37`, `:59`). **F12's plan should state the dependency explicitly**
so an F2 signature change surfaces as a known cross-child event rather than a mystery compile break.

---

## 6. Test-File Plan

### 6.1 Headroom against the 500-line test-file limit

| File | Lines | `[TestMethod]` | Headroom |
| --- | --- | --- | --- |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterTests.cs` | 435 | 16 | 65 |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.cs` | 446 | 14 | 54 |

Both counts independently verified. Neither file has room for the ~180-220 lines J1-J6 need, and
`BreadcrumbBridgeRouterQueueTests.cs` is additionally the F2-shared surface identified in R2.

### 6.2 Recommendation — one new standalone `[TestClass]`, not a `.Part2.cs`

**Create `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterEdgeTests.cs`** as a new
`[TestClass]`, **not** a partial companion.

| Task | Test method | Closes |
| --- | --- | --- |
| T1 | `BindRows_WithNullPresentedRow_SkipsChainLookupAndStillBindsEveryRow` | `:90` c0 |
| T2 | `BindRows_WithRepeatedSuggestionText_ResolvesTheChainOnlyOnce` | `:90` c1 |
| T3 | `BindRows_WhenChainFetchFaults_DegradesThatRowAndCompletesTheBind` | `:356`, `:357`, `:359`, `:360` |
| T4 | `HostMessageReceived_WithValidPayload_RoutesToSelectionWithoutThrowing` | `:192` |
| T5 | `LeafExpandToggle_OnBannerOrTrashRow_IsANoOpWithoutProviderQuery` (`[DataTestMethod]`, 2 `[DataRow]`s) | `:288` c0 |
| T6 | `LeafExpandToggle_OnSegmentlessSuggestionRow_IsANoOp` | `:288` c1 |
| T7 | `SelectFirstRow_OnSegmentlessSuggestionRow_SelectsEmptyStringNotNull` | `:372` c1, c2 |
| T8 | `UpArrow_OnRowAbsentFromTheCurrentRowSet_HandsFocusBackToSearch` | `:426`, `:434` |

**Eight declarations; nine executions counting the `[DataRow]`s.** Estimated 190-230 lines including
a compact local harness plus a `SetPrivateField` helper — comfortably inside 500. T6 and T7 share
one `SegmentlessRowRouter()` factory; T1-T5 reuse the lightweight
`Mock<IFolderHierarchyProvider>` + `Mock<IBreadcrumbWebHost>` + real codec/renderer/queue pattern
from `BreadcrumbBridgeRouterQueueTests.cs:37-74`.

Why standalone rather than `BreadcrumbBridgeRouterQueueTests.Part2.cs`:

1. `BreadcrumbBridgeRouterQueueTests` is declared `public class` at
   `BreadcrumbBridgeRouterQueueTests.cs:23` — it is **not** `partial`. A `.Part2.cs` companion would
   require editing that class declaration, which is precisely the F2-shared fan-in surface R2
   identifies. Same for `BreadcrumbBridgeRouterTests` at `:23`.
2. The repo does have `.Part2.cs` precedent —
   `QuickFiler.Test/QuickFiler.Test.csproj:82` (`Viewers\BreadcrumbDropDownOpenCoordinatorTests.Part2.cs`),
   `:85` (`Viewers\BreadcrumbPopupBoundaryCoverageTests.Part2.cs`) and
   `:123` (`Controllers\QfcStreamingDequeueConfidenceGateTests.Part2.cs`) — so the pattern is
   available if a reviewer insists. It is not needed here and costs a conflict surface.
3. A separate class keeps the two reflection-based tests (T6-T8) visibly quarantined from the
   30 reflection-free tests, which makes the R5 coupling easy to audit.

### 6.3 csproj registration

`QuickFiler.Test/QuickFiler.Test.csproj` is a non-SDK project with explicit `<Compile Include>`
entries and no globbing. Add exactly one line:

```
    <Compile Include="Controllers\BreadcrumbBridgeRouterEdgeTests.cs" />
```

**Insert immediately before line 58** (`<Compile Include="Controllers\BreadcrumbBridgeRouterQueueTests.cs" />`),
which preserves the existing alphabetical order of the breadcrumb block at `:58`-`:65` ("Edge" sorts
before "Queue"). **Preserve CRLF — use the Edit tool, never a git-bash `sed -i`** (epic
§ "Cross-Child Constraints" 1b). Four spaces of indentation, matching the surrounding entries. No
`QuickFiler/QuickFiler.csproj` edit is required.

### 6.4 Projected post-change figures for this file

| Axis | Before | After (J1-J6) | Floor |
| --- | --- | --- | --- |
| Line | 276/282 = 97.87% | 282/282 = **100.00%** | >= 80% |
| Branch | 83/90 = 92.22% | 90/90 = **100.00%** | >= 75% |

Fallback if reflection is rejected (J1-J4 only): 281/282 = 99.65% line, 86/90 = 95.56% branch. Both
retain-or-improve.

---

## 7. Determinism Contract for Every New Test

- Framework: MSTest `[TestClass]` / `[TestMethod]` / `[DataTestMethod]` + `[DataRow]`; **Moq** for
  `IFolderHierarchyProvider` and `IBreadcrumbWebHost`; **FluentAssertions** for every assertion;
  explicit `// Arrange` / `// Act` / `// Assert` section comments matching the house style at
  `BreadcrumbBridgeRouterQueueTests.cs:104-124`.
- Async edges: `await` the returned `Task` directly, or use the existing
  `.GetAwaiter().GetResult()` helper shape (`BreadcrumbBridgeRouterTests.cs:116-117`, `:122`). All
  provider setups use `ReturnsAsync` / `ThrowsAsync`, which yield completed tasks — **no polling, no
  `WaitFor*`, no manual pump, no `SynchronizationContext` install.**
- **No injected clock, no `FakeTimeProvider`, no fake timers.** There is no time dependency in this
  file (§1.3). Any plan task proposing one must be rejected.
- **No STA.** The file references no WinForms type; the existing tests are plain `[TestClass]`.
- Assertions on outbound payloads are **raw JSON substring** assertions, because `QuickFiler.Test`
  carries no Newtonsoft reference (verified against `QuickFiler.Test.csproj`). Reuse the
  `JsonEscaped` helper shape at `BreadcrumbBridgeRouterTests.cs:58-61` where HTML is embedded.
- Prohibited and must be absent from every new test: `Thread.Sleep`, `Task.Delay`, any wall-clock
  wait, real-time polling, temporary files, **any filesystem write at all**, external services or
  processes, live or shown forms, `.Show()`/`.ShowDialog()`, popups, mutable static state, and
  ordering dependencies between `[TestMethod]`s.
- Every test constructs its own router in `[TestInitialize]` or in the test body; no shared mutable
  fixture across classes.

---

## 8. Sibling Boundaries — read-only, not editable

Recorded couplings, none of which F12 may edit:

| File | Owner | Coupling |
| --- | --- | --- |
| `QuickFiler/Viewers/IBreadcrumbWebHost.cs` | **F13 (#455)** | constructor parameter `host` (`:41`), event subscription (`:54`), `NavigateToString` (`:144`, `:402`), `IsCoreInitialized` (`:400`) |
| `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` | **F13** | sole production implementation of the host seam; doc-comment references this type at `:26`, `:61` |
| `QuickFiler/Controllers/BreadcrumbOutboundQueue.cs` | **F2 (#431)** | constructor parameter `outboundQueue` (`:45`), `PostOrQueue` (`:394`), `OnInitializationCompleted` (`:148`) |
| `QuickFiler/Controllers/EfcFormController.cs` | **F9 (#452)** | sole production consumer: `:141`, `:289`-`:294`, `:411`, `:694`, `:843`-`:852`, `:893` |
| `QuickFiler.Test/Controllers/EfcHomeControllerExecuteMovesTests.cs` | **F8 (#437)** | constructs this router at `:252`-`:276` |
| `UtilitiesCS/OutlookObjects/Folder/**` | **no child of #136** | all 19 message/model/codec/renderer collaborator types (§1.2) — must not be edited |
| `QuickFiler.Test/Controllers/BreadcrumbBridgeRouterQueueTests.cs` | F12, but tests an **F2** production file | see R2 |

`ItemViewer.cs` / `ItemViewer.*.cs` (F14 #456) and the `BreadcrumbDropDown*` / `BreadcrumbPopup*` /
`BreadcrumbUiDispatcher.cs` / `WebView2*` / `IWebView*` / `BreadcrumbCollapsedSurfaceController.cs` /
`BreadcrumbWebViewSurfaceFactory.cs` set (F13 #455) have **no compile-time coupling to this file at
all** — this file's only QuickFiler dependencies are `IBreadcrumbWebHost` and
`BreadcrumbOutboundQueue`. That is a favourable finding: F12's work on this file is unusually well
isolated from F13 and F14.

---

## 9. Latent Defects — verified, assessed, NOT fixed

Cross-checked against the open-issue set retrieved from
`github.com/drmoisan/TaskMaster/issues?q=is:issue+is:open+breadcrumb` (#495, #491, #488, #476, #475,
#462, #458, #456, #455, #440, #438, #431). None of the three below duplicates any of them. **The
orchestrator promotes these via the MCP lifecycle; this agent does not.**

### LD-1 — an out-of-range `segmentIndex` escapes the `async void` host-event boundary and can crash the host process

**Severity: High. Recommend promoting to a GitHub issue.**

Verified call chain:

1. `BreadcrumbBridgeRouter.cs:169` — `if (row.CollapseAfter(message.SegmentIndex!.Value))`, with no
   range check and with the null-forgiving operator asserting the codec validated presence.
2. `BreadcrumbMessageCodec.cs:100` / `:142-158` — `OptionalInt` validates only that the token is a
   JSON **integer**. `:103-106` validates only that the field is **present** for
   `segmentDoubleClick`. **The codec performs no range validation.**
3. `BreadcrumbRow.cs:111-118` — `CollapseAfter` throws `ArgumentOutOfRangeException` when
   `segmentIndex < 0 || segmentIndex >= _segments.Count`.
4. `BreadcrumbBridgeRouter.cs:193` — `OnHostMessageReceived` catches **only**
   `BreadcrumbMessageException`. `ArgumentOutOfRangeException` propagates out of an `async void`
   method, where the runtime rethrows it on the captured `SynchronizationContext` — the Outlook UI
   thread in production, via `EfcFormController.ConfigureBreadcrumbControl` (`:834`-`:854`). On
   .NET Framework 4.8 that is an unhandled exception, i.e. a host-process crash.

The payload `{"type":"segmentDoubleClick","rowId":"row-1","segmentIndex":99}` is accepted by the
codec, reaches `:169`, and throws. The XML doc comment at `:151-154` claims malformed payloads "fail
fast with the codec's `BreadcrumbMessageException` (already logged) and leave state unchanged" —
**that claim is false for out-of-range `segmentIndex`.**

Out of scope here: fixing it means either widening the catch at `:193` or adding a guard at `:169`,
both observable behaviour changes under the epic's no-behaviour-change NFR, and both requiring a
failing regression test first per the Bugfix Workflow.

*Note for the planner:* J-group tests must **not** attempt to cover this path, because asserting the
current (crashing) behaviour would pin a defect.

### LD-2 — `BindRowsAsync` clears `_selectedRowId` but leaves `SelectedFolderPath` stale, so filing can target a folder no longer selected

**Severity: Medium-High. Recommend promoting to a GitHub issue.**

Verified call chain:

1. `BreadcrumbBridgeRouter.cs:114` — `_selectedRowId = null;` after a re-bind. `SelectedFolderPath`
   (`:58`) is **not** reset; it is written only in `SelectRow` (`:372`).
2. `BreadcrumbBridgeRouter.cs:399` — the re-rendered document is built with `_selectedRowId = null`,
   so **no row is visually highlighted**.
3. `EfcFormController.cs:289-294` — `public string SelectedFolder => _router?.SelectedFolderPath;`
   still returns the previous selection.
4. `EfcFormController.cs:873-883` — `BindFolderRows` (and thus `BindRowsAsync`) is invoked from the
   `SearchText.TextChanged` path and from the delete-path trash rebind, i.e. **on every keystroke**.
5. `EfcFormController.cs:493` and `:772` pass `SelectedFolder` into the move operation; `:478`,
   `:722` and `:760` pass it into folder-open.

Net effect: after selecting a folder and then typing one more character in the search box, the UI
shows nothing selected while the controller still reports the old folder as the filing target. A
confirm action at that moment files to a folder the user can no longer see selected. The two state
fields — `_selectedRowId` and `SelectedFolderPath` — are updated together in `SelectRow` but only one
is cleared in `BindRowsAsync`.

Not a duplicate: #462 concerns the drop-down coordinator's `closePending` flag (F13's
`BreadcrumbDropDownOpenCoordinator`), #488 concerns the ItemViewer breadcrumb pipeline lifecycle, and
#440 concerns arrow-key tree semantics. None touches this field pair.

Out of scope: resetting `SelectedFolderPath` (and deciding whether to raise
`SelectedFolderPathChanged(null)`) is an observable production contract change.

*Note for the planner:* likewise do not write a test asserting that the stale value survives a
re-bind. J1's assertions are confined to provider call counts and rendered row structure precisely to
avoid pinning this.

### LD-3 — `ExpandLeafAsync` discards the caller's `CancellationToken` and passes `CancellationToken.None`

**Severity: Low-Medium. Recommend recording; promotion optional.**

`BreadcrumbBridgeRouter.cs:296-299` and `:308-309` both call the provider with
`CancellationToken.None`, while `FetchChainAsync` (`:334`-`:362`) correctly threads the token
supplied by `BindRowsAsync` (`:77`). `ExpandLeafAsync` is reached from `ProcessInboundAsync`, which
takes no token at all (`:156`), so there is no token to thread — the asymmetry is structural rather
than a dropped parameter.

Consequence: a leaf-expansion round-trip against `OutlookFolderHierarchyProvider`
(`EfcFormController.cs:840-842`) cannot be cancelled when the form closes or the viewer is recycled;
the `catch (OperationCanceledException)` at `:318` can only ever fire from a provider that cancels on
its own initiative. `EfcFormController` does own a `Token` (used at `:893`) that could be threaded
through `ProcessInboundAsync`, but doing so is a public signature change.

Recording it is worthwhile because it is the reason `:318`-`:323` is reachable **only** by a mock
that throws (`CanceledProviderCall_OnLeafExpand_LeavesRowStateUnchanged`,
`BreadcrumbBridgeRouterQueueTests.cs:150-172`) and never by real cancellation.

### LD-4 — the router subscribes to `IBreadcrumbWebHost.MessageReceived` and never unsubscribes; the type is not `IDisposable`

**Severity: Low (informational). Does NOT warrant a new GitHub issue — see below.**

`:54` attaches `OnHostMessageReceived` in the constructor. The type declares no `Dispose`, no
detach method, and no finalizer, so the host holds a strong reference to the router for the host's
lifetime. Assessment:

- In production the router and its host are created together in a single
  `ConfigureBreadcrumbControl` call (`EfcFormController.cs:834`-`:849`, invoked once at `:393`), and
  a new `WebView2BreadcrumbHost` is constructed alongside each new router. The pair therefore becomes
  garbage together; there is no accumulating leak on the current wiring.
- The genuinely leaky half of this shape — handler retention on a **pooled** viewer — is already
  tracked as **#458** (`webview2breadcrumbhost-handler-retention-pooled-viewer`) against F13's
  `WebView2BreadcrumbHost`. Filing a second issue for the subscriber side would duplicate it.

Record the coupling in the plan so that if #458's fix introduces host reuse across viewers, the
missing detach on this side is fixed in the same change.

---

## 10. Corrections and Confirmations Against the Brief

### 10.1 Corrections — evidence-disproved claims

1. **`FolderBreadcrumbBridgeRouter` is a different type in a different assembly, owned by no child of
   #136.** The type constructed at `BreadcrumbBridgeCoordinator.cs:52` is
   `UtilitiesCS/OutlookObjects/Folder/FolderBreadcrumbBridgeRouter.cs:10`, **not** this file's
   `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs:19`. Any inventory that reports 14 referencing
   files or 71 occurrences for "BreadcrumbBridgeRouter" has substring-matched the wrong type; the
   true figure for this file is **3 test files and 1 production consumer** (§0.1, §5.1).
2. **"Use an injected clock and fake timers" is wrong for this file and must be struck.** Zero
   occurrences of `DateTime` / `Stopwatch` / `Timer` / `Task.Delay` / `Thread.Sleep` /
   `TimeProvider`. The phrasing survives at `.../495/spec.md:69-70` and `:112`, and at
   `.../495/issue.md:70-71` and `:95`; both need a documented deviation adopting F13's ruling
   (`.../455/spec.md:383-390`). Determinism here is not even scheduler control — it is
   already-completed-task control via Moq (§1.3).
3. **The brief's "roughly six uncovered lines" is exactly six, and they are not distributed as the
   phrase implies.** Four of the six (`:356`, `:357`, `:359`, `:360`) are a single contiguous catch
   arm closed by one test; the remaining two are unrelated singletons (`:192`, `:434`). The gap is
   therefore three test tasks, not six.
4. **The emitted `branch-rate="0.926471"` on this file's `<class>` element is not 92.2%.** It encodes
   63/68 against a true 83/90. The brief's 92.2% happens to be right, but it coincides digit-for-digit
   with the **UtilitiesCS** type's emitted `branch-rate="0.922222"` — so the correct figure and the
   wrong-type figure are indistinguishable by inspection. Always recompute.
5. **The epic manifest lists `Controllers/BreadcrumbOutboundQueue.cs` under F2, yet its tests live in
   an F12 test file** (`BreadcrumbBridgeRouterQueueTests.cs:207-220`). This cross-child test-file
   ownership is not recorded anywhere in the epic's "Cross-Child Constraints" and should be (R2).
6. **Open issue #440 will rewrite the arrow-key contract in this exact file** and is not listed in
   the epic's "Known Conflict Risks" (which names only #400, #424 and #426). It is a live semantic
   conflict for F12 (R3).
7. **The brief's "disposal and post-disposal invocation paths" checklist item
   (`spec.md:110` / `issue.md:93`) is not applicable to this file.** The type implements no
   `IDisposable`, holds no disposable resource, and has no disposal flag (§1.3). The item should be
   marked N/A for this file rather than left unchecked.

### 10.2 Confirmations

1. **The coverage table is confirmed exactly**: 282 coverable lines, 97.87% line (rounds to 97.9%),
   92.22% branch (rounds to 92.2%) — all recomputed from the class-level `<lines>` block, never from
   the emitted attributes.
2. **450 physical lines is confirmed**, leaving 50 lines of headroom against the 500-line ceiling.
3. **`QuickFiler/Properties/AssemblyInfo.cs:5` does grant `InternalsVisibleTo("QuickFiler.Test")`** —
   confirmed, though this file exposes no internal member for it to reach.
4. **`QuickFiler.Test/QuickFiler.Test.csproj` is non-SDK with explicit `<Compile Include>` entries and
   no globbing** — confirmed; `.Part2.cs` precedent confirmed at `:82`, `:85`, `:123`.
5. **Both floors are already cleared, so the bar is retain-or-improve** — confirmed against the epic's
   "Coverage-Target Reconciliation".
6. **Exactly one `<class>` element carries this file's `filename`** — the max-hits-per-filename union
   rule was checked and is a no-op here.
7. **Line-number drift: none.** All six uncovered lines and all four partial-branch lines re-anchor
   exactly on the current working-tree file.
8. **The sibling boundaries hold**: this file has zero compile-time coupling to any
   `BreadcrumbDropDown*`, `BreadcrumbPopup*`, `WebView2*`, or `ItemViewer*` file.
