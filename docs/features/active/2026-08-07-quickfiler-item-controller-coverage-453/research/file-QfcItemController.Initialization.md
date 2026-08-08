# Per-File Research: `QuickFiler/Controllers/QfcItemController.Initialization.cs`

- Epic: #136 QuickFiler Per-File 80% Coverage — child F10 (`quickfiler-item-controller-coverage`, issue #453)
- Branch: `feature/quickfiler-item-controller-coverage`
- Worktree: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a359b62de7a79b16e`
- Production file: `QuickFiler/Controllers/QfcItemController.Initialization.cs` (466 lines, verified)
- Research date: 2026-08-07

> Measurement basis, the union-vs-`line-rate` correction, and the shared cross-fixture constraint on
> `IQfcItemController` are established once in `file-QfcItemController.md` §0 and §10 and are not
> repeated here. Read that artifact first.

---

## 0. Headline

| Metric | Value today | Floor | Verdict today |
| --- | --- | --- | --- |
| Line coverage (`line-rate` attribute) | 90.11% | >= 80% | PASS |
| Line coverage (recomputed from `<line>` children) | 123/134 = **91.8%** | >= 80% | PASS |
| Branch coverage | 25/26 = **96.15%** | >= 75% | PASS |
| `[ExcludeFromCodeCoverage]` members | **7** (lines 138, 168, 200, 260, 291, 403, 436) | 0 preferred | FAIL — epic AC2 |
| File size | 466 / 500 | <= 500 | PASS, **only 34 lines headroom** |

**This file already passes both coverage gates and will *fail* them the moment the epic's real
objective — exemption removal — is pursued.** That inversion is the single most important planning
fact in this artifact and is quantified in §3.2.

The brief's figures are confirmed: 466 lines, 7 method-level exemptions at exactly lines 138, 168,
200, 260, 291, 403, 436, measured ~90.1% line.

---

## 1. Member inventory

`internal partial class QfcItemController` (line 25). Every member below is declared in this file.
No nested types, no properties, no events, no fields.

| Lines | Member | Accessibility | Exempt? | Live callers |
| --- | --- | --- | --- | --- |
| 27 | `QfcItemController()` | `protected` | No | Test harnesses (`HarnessController`, `PropController`, `TestableQfcItemController`, `KeyboardRegistrationQfcItemController`) and the two static factories at 418 / 451 |
| 29-75 | `QfcItemController(IApplicationGlobals, IFilerHomeController, IQfcCollectionController, IItemViewer, int, int, MailItem, TlpCellStates, [8 optional seams])` — the primary constructor | `public` | No | `QfcCollectionController.cs:681, 778, 803, 844, 1853`; `QfcQueue.cs:405` |
| 86-109 | `QfcItemController(..., string predeterminedFolder)` — issue #171 high-confidence overload | `public` | No | `QfcCollectionController.cs:620` |
| 111-133 | `QfcItemController(..., bool async)` | `public` | No | **None found** in `QuickFiler/`; see §7.2 — the `async` parameter is also never read |
| 138-163 | `void Initialize(IApplicationGlobals, IFilerHomeController, IQfcCollectionController, IItemViewer, int, int, MailItem, TlpCellStates, bool)` | `private` | **YES (138)** | **None. Dead code.** |
| 168-195 | `void Initialize(bool async)` | `public` (on `IQfcItemController:25`) | **YES (168)** | `QfcCollectionController.cs:813, 1897, 1945` |
| 200-256 | `Task InitializeAsync()` | `public` (on `IQfcItemController:23`) | **YES (200)** | `QfcCollectionController.cs:790, 854`; `QfcQueue.cs:415`; and line 429 below |
| 260-287 | `Task InitializeGraphicsAsync()` | `public` (on `IQfcItemController:98`) | **YES (260)** | `QfcCollectionController.cs:384, 479` |
| 291-322 | `Task InitializeSequentialAsync()` | `public` (on `IQfcItemController:24`) | **YES (291)** | `QfcCollectionController.cs:692`; and line 462 below |
| 346-398 | `void SaveParameters(IApplicationGlobals, IFilerHomeController, IQfcCollectionController, IItemViewer, int, int, MailItem, TlpCellStates)` | `internal` | No | All three constructors and both static factories |
| 403-431 | `static Task<QfcItemController> CreateAsync(...)` | `public static` | **YES (403)** | **None. Dead code.** |
| 436-464 | `static Task<QfcItemController> CreateSequentialAsync(...)` | `public static` | **YES (436)** | **None. Dead code.** |

Verified by
`grep -n '\.Initialize\(|\.InitializeAsync\(\)|\.InitializeGraphicsAsync\(\)|\.InitializeSequentialAsync\(\)|QfcItemController\.CreateAsync|QfcItemController\.CreateSequentialAsync|new QfcItemController'`
over `QuickFiler/`, which returns the call sites tabulated above and no others.

Commented-out dead code occupies lines 324-344 (a duplicate `InitializeSequentialAsync`) and
228-252 (a commented parallel-task block inside `InitializeAsync`). These emit no IL.

### 1.1 Correction to the exemption comments in this file

Every one of the 7 exemption comments (lines 135-137, 165-167, 197-199, 258-259, 289-290, 400-402,
433-435) asserts the same barrier: *"Not unit-reachable without a live ItemViewer"* / *"requires a
live ItemViewer"*. **That justification is stale.** The current test project already constructs a
real, headless `QuickFiler.ItemViewer` twice, in ordinary `[TestClass]`/`[TestMethod]` bodies with no
STA attribute and no message pump:

- `QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs:379-405` —
  `ResolveControlGroups_WithHeadlessItemViewer_PopulatesConcreteControlCollections`, which
  constructs `new QuickFiler.ItemViewer()` at line 386 inside a `SynchronizationContext`
  save/install/restore try-finally and executes `ResolveControlGroups` against its real
  Designer-built control tree.
- `QuickFiler.Test/Controllers/QfcItemControllerBreadcrumbDropDownTests.cs:365-383` — a reusable
  `ViewerScope : IDisposable` implementing the same pattern, used by six tests.

`ResolveControlGroups` itself was de-exempted on this basis; its in-file comment says so
(`ViewerSetup.cs:204`: *"De-exempted cycle-5 (R1): covered by a headless real-ItemViewer test"*).
The Initialization comments were not updated to match. Barrier analysis in §4 therefore starts from
"a headless `ItemViewer` is available" and asks what *else* blocks each member.

---

## 2. What is already covered

Covering fixture: `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs`
(`QfcItemController_InitializationTests`, 193 lines, 4 tests). It builds a `Mock<IFilerHomeController>`
wired with `KeyboardHandler`, `ExplorerController`, `TokenSource` and `Token`
(InitializationTests.cs:24-39) and uses `Mock<IItemViewer>` throughout — no live viewer.

| Member | Status | Covering test |
| --- | --- | --- |
| `QfcItemController()` (27) | COVERED | Every `HarnessController` instantiation across the family; `SaveParameters_AssignsAllFieldsAndResolvesCollaborators` (InitializationTests.cs:142) |
| Primary ctor (29-75) | COVERED | `PrimaryConstructor_AssignsFieldsAndSetsControllerBackReference` (InitializationTests.cs:42) |
| Predetermined-folder ctor (86-109) | COVERED | `PredeterminedFolderConstructor_StoresPredeterminedFolder` (InitializationTests.cs:75) |
| `bool async` ctor (111-133) | COVERED | `AsyncFlagConstructor_AssignsFieldsViaSaveParameters` (InitializationTests.cs:110) |
| `SaveParameters` field assignment + collaborator resolution (356-375) | COVERED | `SaveParameters_AssignsAllFieldsAndResolvesCollaborators` (InitializationTests.cs:142) asserts `_kbdHandler`, `_explorerController`, `_globals`, `_tokenSource`, `Token`, `Parent`, `ItemNumber`, `ItemNumberDigits`, and `_itemViewer.Controller = this` |
| `SaveParameters` seam-default `??=` chain (380-397) | **PARTIALLY COVERED** — see §3.1 | Same test, indirectly (it does not assert any seam default) |
| `Initialize(9 args)` (138-163) | UNCOVERED | none |
| `Initialize(bool)` (168-195) | UNCOVERED | none |
| `InitializeAsync()` (200-256) | UNCOVERED | none |
| `InitializeGraphicsAsync()` (260-287) | UNCOVERED | none |
| `InitializeSequentialAsync()` (291-322) | UNCOVERED | none |
| `CreateAsync` (403-431) | UNCOVERED | none |
| `CreateSequentialAsync` (436-464) | UNCOVERED | none |

Verified by grepping the whole of `QuickFiler.Test/` for each member name: the only hits are
`QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs:365-369, 460`, which are
`throw new NotImplementedException()` stubs in a hand-written `IQfcItemController` test double — not
coverage of this file.

**Do not duplicate the four existing InitializationTests.** Every proposal in §8 is additive.

---

## 3. The gap list

### 3.1 Gap today (with the 7 exemptions in place): 11 uncovered lines, 1 uncovered branch

All 11 are **compiler-generated lambda bodies**, which is itself a finding: `[ExcludeFromCodeCoverage]`
applied to a method does **not** propagate to lambdas declared inside it, so an exempt method still
leaks its closure bodies into the file's denominator at zero hits.

| Uncovered line(s) | Source construct | Enclosing member | Enclosing member exempt? |
| --- | --- | --- | --- |
| 253 | `() => WireEvents()` passed to `Task.Run` | `InitializeAsync` | Yes (200) |
| 264 | `() => ResolveControlGroups((ItemViewer)_itemViewer)` | `InitializeGraphicsAsync` | Yes (260) |
| 267-272 | `() => QfcThemeHelper.SetupThemes(this, (ItemViewer)_itemViewer, HtmlDarkConverter, _uiDispatcher)` | `InitializeGraphicsAsync` | Yes (260) |
| 297 | `() => ResolveControlGroups((ItemViewer)_itemViewer)` | `InitializeSequentialAsync` | Yes (291) |
| 390 | `new FlagTasks(globals, itemList, blFile, hWndCaller)` — body of the default `_flagTasksFactory` | `SaveParameters` | No |
| 396 | `new FolderPredictor(globals, objItem, options)` — body of the default `_folderPredictorFactory` | `SaveParameters` | No |

The single uncovered **branch** is at line **392**, `condition-coverage="75% (3/4)"` (report lines
23495-23500):

```csharp
_mailActions ??= mailItem is null ? null : new MailItemActionsAdapter(mailItem);
```

- `condition 0` (the `??=` null test) — 100%.
- `condition 1` (`mailItem is null`) — **50%**. All four existing InitializationTests pass
  `mailItem: null`, so only the null arm is taken. The `new MailItemActionsAdapter(mailItem)` arm is
  never executed.

Note that lines **382-388** (`<SaveParameters>b__118_0`, the default `_conversationResolverFactory`)
are reported `hits="0"` by the per-method entry but `hits="1"` by the class-level union — the union
is authoritative (see `file-QfcItemController.md` §0.2), so they are COVERED.

### 3.2 Gap after de-exemption — the inversion

The measured 91.8% **excludes the 7 exempt method bodies from the denominator entirely**. Removing an
attribute adds that body at zero hits before any new test exists. Estimated additional coverable
lines (statement sequence points, excluding the lambda bodies already counted in §3.1):

| Member | Attribute line | Est. new coverable lines |
| --- | --- | --- |
| `Initialize(9 args)` | 138 | ~4 |
| `Initialize(bool)` | 168 | ~9 |
| `InitializeAsync()` | 200 | ~15 |
| `InitializeGraphicsAsync()` | 260 | ~10 |
| `InitializeSequentialAsync()` | 291 | ~11 |
| `CreateAsync` | 403 | ~7 |
| `CreateSequentialAsync` | 436 | ~7 |
| **Total** | | **~63** |

Projected effect of removing all seven with no new tests:

- Denominator 134 → **~197**
- Covered 123 → 123
- Line coverage **91.8% → ~62.4%** — a 17.6-point drop below the 80% floor.
- To restore 80% of 197 (= 158 covered) the child must newly cover **~35 lines**.

These estimates must be confirmed by measurement, not trusted. The direction and rough magnitude are
what matter for sequencing: **remove attributes and add the covering tests in the same atomic
task, never in separate tasks**, or the file will sit below floor between them.

---

## 4. Seam analysis

### 4.1 Barrier taxonomy (verified against source, not against the exemption comments)

| Barrier | Verified evidence | Real today? |
| --- | --- | --- |
| **A — concrete `(ItemViewer)` cast** at 172, 175, 207, 209, 264, 267, 297, 299 | `new QuickFiler.ItemViewer()` already runs headless in two existing test files (§1.1) | **DEFEATED** |
| **B — `await _itemViewer.UiSyncContext` with no WinForms message pump** | `ItemViewer.cs:23-30` — the constructor runs `InitializeComponent()` at line 25 and *then* captures `_context = SynchronizationContext.Current` at line 26. WinForms control creation installs a `WindowsFormsSynchronizationContext`, so `UiSyncContext` is a WinForms context whose `Post` requires a running message loop. Reached only via `ResolveControlGroupsAsync` (`ViewerSetup.cs:265`) and `InitializeWebViewAsync` (`ViewerSetup.cs:55`) | **REAL** |
| **C — WebView2 core initialization** | `ViewerSetup.cs:76` dereferences `.CoreWebView2` directly, outside the injected `IWebViewCoreInitializer` seam | **REAL** (analysed in the ViewerSetup artifact) |
| **D — `QfcThemeHelper.SetupThemes(this, (ItemViewer), ...)`** at 175, 209, 267, 299 | The pure overload `SetupThemes(QfcThemeControlSet)` is already unit-tested at `QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs:45-107` with a synthetic control set. The 4-arg overload adapts a concrete viewer into that control set | **SOFT** — expected to work headless; confirm at plan time |
| **E — `_itemViewer.UiDispatcher.InvokeAsync(...)` fire-and-forget** at 193 | `ItemViewer.cs:28` captures `Dispatcher.CurrentDispatcher`. With no `Dispatcher.Run()`, the operation queues and never executes — a deterministic no-op, exactly the documented "parked dispatcher" rationale at `QfcItemController.TestSupport.cs:224-249`. The call is discarded (`_ =`), so nothing blocks | **NOT A BARRIER**, with the isolation caveat in §6.2 |
| **F — thread-pool `Task.Run`** at 253, 264, 297 | Awaited by the caller, so completion is observed deterministically | **NOT A BARRIER** |

### 4.2 Per-member seam recommendation

Applying the epic hierarchy (interface seam > injectable delegate > adapter; STA-constructed
never-shown controls only as a last resort in `*.StaTests.cs`):

| Member | Barriers actually present | Minimum seam required | Disposition |
| --- | --- | --- | --- |
| `Initialize(9 args)` (138) | A, D | — | **DELETE** (dead, §7.1) |
| `Initialize(bool)` (168) | A, D, E | **None.** Existing `Mock<IItemViewer>` + headless `ItemViewer` + existing `IUiDispatcher` seam suffice | **DE-EXEMPT + cover** |
| `InitializeGraphicsAsync()` (260) | A, D, F, plus a fire-and-forget `InitializeWebViewAsync` at 286 | **None**, provided `_webViewInitializer` is injected (already an interface field) so the discarded task completes cleanly | **DE-EXEMPT + cover** |
| `InitializeSequentialAsync()` (291) | A, D, F, fire-and-forget at 321 | **None**, same condition. Uses the *synchronous* `ResolveControlGroups` (297), not the `UiSyncContext`-bound async one | **DE-EXEMPT + cover** |
| `InitializeAsync()` (200) | A, D, F **and B** (via `ResolveControlGroupsAsync` at 207) **and C** (via `await InitializeWebViewAsync()` at 255, which is *awaited*, not discarded) | Would require repointing `await _itemViewer.UiSyncContext` onto the injected `IUiDispatcher`. That changes thread affinity from the WinForms context to a WPF dispatcher — an observable behavior change, prohibited by the epic NFR | **RETAIN exemption, with a rewritten rationale** (§4.3) |
| `CreateAsync` (403) | Inherits B and C from `InitializeAsync` | — | **DELETE** (dead, §7.1) |
| `CreateSequentialAsync` (436) | Inherits from `InitializeSequentialAsync` | — | **DELETE** (dead, §7.1) |

### 4.3 Resulting exemption boundary: 7 → 1

- 3 removed by **deletion of dead code** (138, 403, 436).
- 3 removed by **de-exemption plus coverage** (168, 260, 291).
- 1 **retained**: `InitializeAsync()` (200). Its current comment must be replaced, because the
  reason it gives is false. Accurate rationale:

  > Residual: awaits `ResolveControlGroupsAsync`, which awaits `_itemViewer.UiSyncContext` — a
  > `WindowsFormsSynchronizationContext` captured after `InitializeComponent()` (`ItemViewer.cs:25-26`)
  > whose continuation requires a running WinForms message loop that a unit test must not start. It
  > also awaits `InitializeWebViewAsync`, which dereferences `.CoreWebView2` outside the injected
  > `IWebViewCoreInitializer` seam. Repointing `UiSyncContext` onto the injected `IUiDispatcher`
  > would change thread affinity and is prohibited by the epic's no-behavior-change NFR. Follow-up:
  > issue tracked in §7.5.

  This boundary is stronger than a category claim and matches the maintainer precedent that a
  per-member barrier analysis — not a blanket per-partial exemption — is required.

### 4.4 Host-neutrality

None of the recommended work adds a WinForms or WPF dependency. `Initialize(bool)`'s orchestration is
already sequencing calls that are individually host-neutral or already seamed. The de-exemption path
adds **zero** new production types, which is the outcome the epic's Non-Goals prefer.

---

## 5. State-transition invariants

This file is the ordering authority for the whole `QfcItemController` type. Eight invariants, each
with the test that would pin it (test IDs refer to §8).

| # | Invariant | Evidence in source | Pinned today? | Pin with |
| --- | --- | --- | --- | --- |
| **INIT-1** | `ResolveControlGroups` must run **before** `SetupThemes` | Explicit comment at 174: *"Note: need control groups established prior to this"*; enforced by call order at 172→175, 207→209, 264→266, 297→299 | No | **B2** |
| **INIT-2** | `SetupThemes` must run **before** `SetThemeDark`/`SetThemeLight` (which index `_themes["DarkNormal"]` / `["LightNormal"]` — `FocusAndTheme.cs:279, 307`) | 209→215, 266→275, 299→305 | No | **B6**, **B7** |
| **INIT-3** | `PopulateControls` must run **before** `ToggleTips`/`ToggleNavigation` | 182→186→187; 224→225→226; 314→316→318 | No | **B1** |
| **INIT-4** | `WireEvents` must run **last**, after all state is populated | 190 (last but for the fire-and-forget); 253; 285; 319 | No | **B1**, **B4** |
| **INIT-5** | `SaveParameters` must run before any `Initialize*` | Structurally enforced: all three ctors call it (65, 98, 123) and both factories call it (419, 452) before initializing | Partially (the three ctor tests) | **A4** |
| **INIT-6** | Seam defaults are **write-once**: an injected seam is never overwritten (`??=` at 380-397), and the injected values are captured *before* `SaveParameters` runs (57-64) | 57-64 then 380-397 | No | **A4** |
| **INIT-7** | **Re-entrancy:** `Initialize(bool)` is *not* idempotent — a second call re-runs `WireEvents()` (190), which subscribes again without unsubscribing | `EventWiring.cs:28-32` → `WireControlTreeEvents()` + `WireIntentEvents()`; `WireIntentEvents` performs bare `+=` (proved by `SeamFactoryTests.cs:239-282`, which asserts `VerifyAdd(..., Times.Once())` after a single call) | No | **B3** (characterisation) |
| **INIT-8** | **Dispose-before-setup:** after `Cleanup()` (`ViewerSetup.cs:392-421`), a subsequent `SaveParameters` restores every plain field (they are assigned unconditionally at 360-375) but **cannot** restore the seam fields, because `??=` at 380-397 sees them non-null. `_mailActions` in particular stays bound to the *previous* `MailItem` (created at 392-394 from the constructor's `mailItem`, and never nulled by `Cleanup`) | 380-397 vs `ViewerSetup.cs:402-420` | No | **A6** (characterisation) — and see defect §7.4 |

### Explicit coverage of the three categories the brief requires

- **Ordering** — INIT-1 through INIT-5, pinned by B1, B2, B4, B6, B7, A4.
- **Re-entrancy** — INIT-7, pinned by B3. This is a real, currently-unpinned hazard: three production
  call sites invoke `Initialize(false)` (`QfcCollectionController.cs:813, 1897, 1945`), and nothing
  in the type prevents two of them reaching the same instance.
- **Dispose-before-setup** — INIT-8, pinned by A6. `Cleanup()` publishes a reuse contract (it nulls
  `_globals`, `_itemViewer`, `_parent`, `_homeController`, `ItemHelper`) that `SaveParameters`'
  `??=` seam defaults do not honour.

---

## 6. Determinism requirements

### 6.1 Audit result

- **Wall-clock:** none. No `DateTime.Now`, `DateTime.UtcNow`, `DateTime.Today`, `Stopwatch`, or
  `Environment.TickCount` anywhere in this file (verified by grep over
  `QuickFiler/Controllers/QfcItemController*.cs`, which returns exactly one hit family-wide, in
  `EventWiring.cs:135`).
- **Randomness:** none.
- **Banned-API finding in production code this child will touch:** **none in this file.** The one
  family-wide hit, `QfcItemController.EventWiring.cs:135  await Task.Delay(newDelay);`, is in
  production code (where `Task.Delay` is permitted; the repository ban is on test code) and belongs
  to the `EventWiring.cs` artifact, not this one.
- **Thread pool:** `Task.Run` at 253, 264, 266, 297. All are `await`ed by their caller, so a test that
  awaits the method observes completion deterministically. No polling, no sleeping.

### 6.2 Two determinism hazards the new tests must design around

1. **Fire-and-forget WebView initialization.** Lines 193 (`_ = _itemViewer.UiDispatcher.InvokeAsync(InitializeWebViewAsync)`),
   286 and 321 (`_ = InitializeWebViewAsync()`) discard a `Task`. Line 193 queues onto an unpumped
   dispatcher and is inert. Lines 286 and 321 start immediately. Every test of
   `InitializeGraphicsAsync` / `InitializeSequentialAsync` **must** inject a
   `Mock<IWebViewCoreInitializer>` returning completed tasks so the discarded task finishes without
   faulting, and must **not** assert anything about it. Do not use `Task.Delay`, `Thread.Sleep`, or
   any wall-clock wait to "let it finish" — that is a policy violation.
2. **Shared-dispatcher leakage.** `ItemViewer` captures `Dispatcher.CurrentDispatcher` at
   construction (`ItemViewer.cs:28`). If a headless viewer is built on the MSTest thread, anything
   posted to its dispatcher stays queued on the *shared* thread dispatcher and could be executed by
   an unrelated later test that pumps it. `QfcItemController.TestSupport.cs:224-249` documents this
   exact hazard and its mitigation (a dedicated parked dispatcher). New tests should follow the
   `ViewerScope` pattern and, where they post to the viewer's dispatcher, use
   `StartRunningDispatcher()` / `ShutdownDispatcher()` (TestSupport.cs:297-326) on a dedicated STA
   thread rather than the test thread.

No injected clock and no `FakeTimeProvider` are required for this file.

---

## 7. Latent defects for promotion

Report only; do not fix under this child. Promote via the MCP promotion lifecycle per epic.md.

### 7.1 Three unreferenced members carrying coverage exemptions — **Moderate (maintainability)**

- `private void Initialize(9 args)` — `Initialization.cs:138-163`.
- `public static Task<QfcItemController> CreateAsync(...)` — `Initialization.cs:403-431`.
- `public static Task<QfcItemController> CreateSequentialAsync(...)` — `Initialization.cs:436-464`.

No call site exists anywhere in `QuickFiler/` (verified by grep, §1). Together they are ~64 physical
lines and ~18 of the ~63 lines that de-exemption would add to the denominator.

`[ExcludeFromCodeCoverage]` on dead code is not an "irreducible remainder" and does not survive the
epic's ratification standard, so the policy-consistent disposition is **deletion**, which is
observably behavior-neutral (the type is `internal`; the two statics are `public` only within the
assembly). Deletion also takes the file from 466 to ~402 lines — see §9. If the plan owner declines
deletion, the fallback is to keep them exempt with a `dead-code` rationale explicitly recorded in
F1's ledger, and to accept that F16 will re-raise it.

### 7.2 `QfcItemController(..., bool async)` ignores its `async` parameter — **Low**

`Initialization.cs:111-133`. The parameter is neither stored nor read; the body is byte-identical to
the primary constructor's `SaveParameters` call. The overload is therefore indistinguishable from the
primary constructor at runtime and has no production call site. Callers reading the signature would
reasonably expect it to select an async initialization path. Candidate for removal alongside §7.1.

### 7.3 The high-confidence constructor cannot be seam-injected — **Low (testability)**

`Initialization.cs:86-109` accepts no seam parameters, so items arriving on the issue-#171
high-confidence path (`QfcCollectionController.cs:620`) always get production `WpfUiDispatcher`,
`WebView2CoreInitializer`, `MailItemActionsAdapter`, and the four production factories. The
primary constructor's 8 optional seams are unavailable on that path. Not a runtime defect, but it
means the high-confidence path is structurally less testable than the standard path.

### 7.4 `Cleanup()` + `SaveParameters` re-initialization leaves `_mailActions` bound to the previous mail item — **Moderate (latent)**

- `ViewerSetup.cs:392-421` nulls `_mailItem` (line 405) but **not** `_mailActions`.
- `Initialization.cs:392-394` uses `??=`, so on a second `SaveParameters` the stale adapter — built
  from the *old* `MailItem` — is preserved.
- Consequence if a controller instance were ever reused after `Cleanup()`: `Reply`, `ReplyAll`,
  `Forward`, `Display`, and `EntryID` (all routed through `_mailActions`, see
  `SeamCoreTests.cs:47-99`) would act on the previous message.
- Severity is Moderate rather than High because production creates a **new** `QfcItemController` per
  item (six `new QfcItemController(...)` sites; no site calls `SaveParameters` twice on one
  instance). It is a trap, not a live bug: `Cleanup()` advertises a reuse contract the seam defaults
  do not satisfy. The asymmetry is visible in-file — `_conversationResolverFactory` (382-388) reads
  `_globals`/`_tokenSource`/`Token` at *invoke* time and so survives re-initialization correctly,
  whereas `_mailActions` captures at *creation* time.

### 7.5 `InitializeAsync` thread-affinity follow-up — **Informational**

The one retained exemption exists because `await _itemViewer.UiSyncContext`
(`ViewerSetup.cs:265`, reached from `Initialization.cs:207`) is a WinForms-context await with no
seam. Migrating the family off `UiSyncContext` onto the already-injected `IUiDispatcher` would
eliminate the last exemption in this file, but it is a thread-affinity change and therefore out of
scope under the epic's no-behavior-change NFR. Promote as a standalone testability issue.

---

## 8. Proposed test case list

13 test cases in two groups plus 3 non-test atomic tasks. Each is individually small and
independently verifiable.

### Group A — non-exempt gap closure (no production change, no attribute removal)

| ID | Target | Scenario | Fixture | Closes |
| --- | --- | --- | --- | --- |
| **A1** | `SaveParameters` line 392 | Positive | `HarnessController` + `Mock<IFilerHomeController>` (reuse `BuildHomeController`) + `Mock<IItemViewer>` + **non-null** `Mock<MailItem>`; assert `GetField(controller, "_mailActions")` is a `MailItemActionsAdapter` | The last uncovered branch (392 condition 1). Takes the file to 26/26 = 100% branch |
| **A2** | `SaveParameters` line 389-390 (default `_flagTasksFactory`) | Positive | Construct without injecting `flagTasksFactory`; read the field, invoke the captured delegate with a `Mock<IApplicationGlobals>`, a single-item `List<MailItem>` of `Mock<MailItem>`, `false`, `IntPtr.Zero`; assert a `FlagTasks` is returned | Line 390. **Conditional:** confirm at plan time that the `TaskVisualization.FlagTasks` constructor does not display a dialog or touch live COM. `SeamFactoryTests.cs:24-25` injects a factory expressly so "no modal dialog is launched", so this must be verified before the task is scheduled. If it cannot be satisfied, accept line 390 as an uncovered residual and record why |
| **A3** | `SaveParameters` line 395-396 (default `_folderPredictorFactory`) | Positive | Same shape; invoke the captured delegate with `Mock<IApplicationGlobals>`, an object, and a `FolderPredictor.InitOptions`; assert a `FolderPredictor` is returned. The seam exists because `InitAsync` (not the constructor) is expensive | Line 396. Same plan-time verification caveat as A2 |
| **A4** | Primary ctor (57-64) + `SaveParameters` (380-397) | Ordering / invariant INIT-5, INIT-6 | Construct with **all eight** optional seams supplied as distinct mocks/delegates; assert each private field is reference-equal to what was passed, i.e. `??=` never overwrote an injected seam | Pins INIT-6; no new lines but removes a silent-regression risk on the seam contract |
| **A5** | `SaveParameters` line 372-375 | Negative / error | `HarnessController` with a `Mock<IFilerHomeController>` returning `null` for `KeyboardHandler`; assert `_kbdHandler` is null and no throw (the code does not guard) | Characterises the unguarded collaborator pull |
| **A6** | `SaveParameters` after `Cleanup()` | Dispose-before-setup / invariant INIT-8 | Construct with a non-null `Mock<MailItem>` A; capture `_mailActions`; call `Cleanup()`; call `SaveParameters` again with a **different** `Mock<MailItem>` B; assert `_mailActions` is still the *same* instance (bound to A) while `_globals`/`_itemViewer`/`Parent` have been restored | Characterises defect §7.4 **without changing behavior**. Explicitly the dispose-before-setup case the brief requires |

### Group B — de-exemption (each task removes exactly one attribute *and* adds its covering test)

| ID | Target | Attribute removed | Scenario | Fixture |
| --- | --- | --- | --- | --- |
| **B0** | `InitializeSequentialAsync()` line 294 | 291 | Negative / cancellation | `HarnessController` + `Mock<IItemViewer>`; set `Token` to an already-cancelled token; assert `OperationCanceledException`. **Reaches the guard before any viewer access — needs no headless viewer at all.** Cheapest de-exemption win in the file; schedule first |
| **B1** | `Initialize(false)` | 168 | Positive + ordering (INIT-3, INIT-4) | `ViewerScope`-style headless `ItemViewer`; `Mock<IApplicationGlobals>` with `Ol.DarkMode`, `QfSettings`; `Mock<IFilerHomeController>`; `Mock<IWebViewCoreInitializer>`; `BuildSyncDispatcher()`. Assert `TableLayoutPanels` and `Buttons` are non-empty (ResolveControlGroups ran), `_themes` has the four expected keys (SetupThemes ran), viewer intent members were written (PopulateControls ran) |
| **B2** | `Initialize(false)` | (same, 168) | Ordering (INIT-1) | Same fixture; assert `_itemPositionTips` — set only by `ResolveControlGroups` (`ViewerSetup.cs:223`) — is non-null whenever `_themes` is non-null, i.e. the documented precondition at line 174 held |
| **B3** | `Initialize(false)` | (same, 168) | **Re-entrancy** (INIT-7) | Same fixture but with a `Mock<IItemViewer>` for the event assertions; call `Initialize(false)` twice; assert `VerifyAdd(v => v.FlagTaskClicked += ..., Times.Exactly(2))` — characterising the double subscription — and that `Buttons.Count` is unchanged (collections are replaced, not appended). Do **not** change behavior; record the double-subscribe as a defect finding at execution time |
| **B4** | `InitializeSequentialAsync()` | 291 | Positive + ordering (INIT-4) | Headless viewer + `Mock<IWebViewCoreInitializer>` + a dedicated `StartRunningDispatcher()`; `await` the method; assert control groups, themes, and populated controls as in B1 |
| **B5** | `InitializeGraphicsAsync()` | 260 | Positive, dark branch (line 275 true, INIT-2) | Headless viewer; `Mock<IOlObjects>.DarkMode = true`; assert `_activeTheme` reflects the dark theme after the call |
| **B6** | `InitializeGraphicsAsync()` | (same, 260) | Branch, light (line 275 false, INIT-2) | Same with `DarkMode = false`; assert the light theme was selected |
| **B7** | `InitializeGraphicsAsync()` | (same, 260) | Negative / error | `Mock<IApplicationGlobals>` whose `Ol` throws, or a viewer whose control tree is absent; assert the failure surfaces rather than being swallowed. Confirm the actual production behavior first and characterise it — do not assume |

### Group C — non-test atomic tasks

| ID | Action | Rationale |
| --- | --- | --- |
| **C1** | Delete `private void Initialize(9 args)`, `Initialization.cs:135-163` (comment + attribute + member) | §7.1 |
| **C2** | Delete `CreateAsync`, `Initialization.cs:400-431` | §7.1 |
| **C3** | Delete `CreateSequentialAsync`, `Initialization.cs:433-464` | §7.1 |
| **C4** | Replace the exemption comment on `InitializeAsync` (`Initialization.cs:197-199`) with the accurate rationale in §4.3; append the F1 ledger row for the single retained exemption | §4.3 |

### Scenario-completeness check

| Required scenario | Covered by |
| --- | --- |
| Positive, valid inputs | A1-A4, B1, B4, B5, B6 + the 4 pre-existing tests |
| Negative / missing input | A5, B0 |
| Edge / boundary | B6 (theme-selection boundary), A6 |
| Error handling | B7, A6 |
| Concurrency | Not applicable — no shared mutable state across threads; the `Task.Run` uses are awaited |
| Ordering | B1, B2, B4, A4 (INIT-1..INIT-6) |
| Re-entrancy | B3 (INIT-7) |
| Dispose-before-setup | A6 (INIT-8) |

### Projected result

With C1-C3 executed and B0-B7 covering `Initialize(bool)`, `InitializeGraphicsAsync`, and
`InitializeSequentialAsync` (which also covers the currently-uncovered lambdas at 264, 267-272, 297),
plus A1-A3 closing lines 390, 392, 396:

- Denominator ≈ 134 + 30 = **~164** (the ~18 lines of dead code never enter it).
- Uncovered ≈ line 253 only (the lambda inside the retained-exempt `InitializeAsync`), plus lines 390
  and/or 396 if A2/A3 prove infeasible.
- Projected line coverage **>= 98%**, branch **100%**, exemption count **7 → 1**.

Sequencing rule: **each Group-B task removes its attribute and lands its test together.** Splitting
them leaves the file below the 80% floor between tasks.

---

## 9. File-size and creation impact

- Current: **466 / 500 lines — only 34 lines of headroom.** This is the tightest file of the three
  and the constraint binds.
- The recommended plan **adds no production lines** (no new seam is required, §4.4) and **removes
  ~64** via C1-C3, landing the file at **~402 lines** with ~98 lines of headroom. C4 is comment-only.
- **If C1-C3 are declined**, any future seam addition here breaches 500. The pre-planned split in
  that case is a new partial `QuickFiler/Controllers/QfcItemController.InitializationSeams.cs`
  carrying the seam-default block currently at lines 377-397 (~21 lines). That would require:
  - a new `<Compile Include="Controllers\QfcItemController.InitializationSeams.cs" />` entry in
    `QuickFiler/QuickFiler.csproj` (legacy non-SDK, **no globbing**), added with the Edit tool or
    `perl -0777` using explicit `\r\n` — **never** git-bash `sed -i`, which strips CRLF and produces
    a whole-file diff guaranteed to conflict at fan-in;
  - an F1 ledger row for the new file, appended in the same change as the csproj entry, defaulting to
    bucket `testable` at **>= 90% line coverage** (epic.md "Mid-Wave File Creation", rule 4).
  This split is **not** recommended; deletion is the cheaper and more honest route.
- **Test project:** all Group-A and Group-B tests fit in the existing
  `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.cs` (currently 193 lines).
  Adding 13 tests will take it past 500 lines. Plan for a second fixture file
  `QfcItemController.InitializationSeamTests.cs` (Group A) or
  `QfcItemController.InitializationLifecycleTests.cs` (Group B), which **requires** its own
  `<Compile Include>` entry in `QuickFiler.Test/QuickFiler.Test.csproj` — that project also has no
  globbing (see `file-QfcItemController.md` §9.2). Test files are subject to the same 500-line limit.
- **No STA file is required.** Both existing headless-`ItemViewer` tests run in plain
  `[TestClass]`/`[TestMethod]` with no STA attribute, so the epic's `*.StaTests.cs` last-resort clause
  is not engaged by anything proposed here.

---

## 10. Sibling boundaries — do not edit

| Sibling asset | Owner | Dependency in this file | Action |
| --- | --- | --- | --- |
| `ConversationResolver` | **F4 (#434)** | `Initialization.cs:382-388` constructs it **positionally** with five arguments: `(_globals, mail, _tokenSource, Token, SetTopicThread)` | **Depend on the current constructor shape.** If F4 reorders, inserts, or removes a parameter, this call site breaks silently at compile time in F10's branch and at fan-in. **Cross-child contract note:** F4 must not change `ConversationResolver`'s 5-argument constructor signature without notifying F10. No edit by F10. |
| `FolderPredictor`, `FolderPredictor.InitOptions` | UtilitiesCS (outside the epic) | `Initialization.cs:395-397` — two default factories | No edit. A3 constructs one in a test only. |
| `FlagTasks` | `TaskVisualization` (outside the epic) | `Initialization.cs:389-390` | No edit. A2 is conditional on its constructor being side-effect free. |
| `EmailFiler`, `EmailFilerConfig` | UtilitiesCS | `Initialization.cs:391` | No edit; already covered via injected factories in `SeamFactoryTests`. |
| `IQfcKeyboardHandler` / `KeyboardHandler.cs` | **F3 (#430)** | `Initialization.cs:372` — `_kbdHandler = _homeController.KeyboardHandler` | Read-only dependency on the existing property. No edit, no contract change needed. |
| `IQfcDatamodel` | **F5** | Not referenced in this file | None. |
| `QfcThemeHelper.SetupThemes`, `TlpCellStates` | **F4 (#434)** | `Initialization.cs:175, 209, 266, 299` (SetupThemes); parameter `tlpStates` | Called, never edited. B1/B4/B5/B6 exercise the existing 4-argument overload. **Cross-child contract note:** F4 must preserve `SetupThemes(IQfcItemController, ItemViewer, Action<Enums.ToggleState>, IUiDispatcher)`. |
| `ItemViewer` / `IItemViewer` | **F14** | `Initialization.cs:172, 175, 207, 209, 264, 267, 297, 299` cast to concrete `ItemViewer`; `:193` reads `UiDispatcher`; `:371` writes `Controller` | No edit. **Do not widen `IQfcItemController`** — see `file-QfcItemController.md` §10 for the `QfcThemeHelperTests.cs` test-double constraint that makes any interface widening a cross-child conflict. |
| `IQfcCollectionController` / `QfcCollectionController.cs` | **F11** | Consumes this file's constructors and `Initialize*` at 6 + 6 call sites | **Read-only.** Every de-exemption in §4 is attribute-and-test only, so F11's call sites are untouched. C1-C3 delete only members F11 does not call — verified. |

---

## 11. Summary

| Question | Answer |
| --- | --- |
| Current coverage reality | 91.8% line (recomputed; attribute says 90.1%), 96.15% branch. **Both floors already met** — but only because 7 exempt method bodies are outside the denominator. |
| Size of the gap | 11 uncovered lines and 1 partial branch **today**; **~63 additional uncovered lines** the moment the 7 exemptions come off, which would drop the file to ~62%. |
| Seams required | **None.** Barrier A ("requires a live ItemViewer") is already defeated by headless `ItemViewer` construction proven in two existing test files; every other barrier is either already seamed (`IUiDispatcher`, `IWebViewCoreInitializer`) or confined to the one member that stays exempt. Zero new production types. |
| Proposed test cases | **13** (A1-A6, B0-B7) plus **4** non-test atomic tasks (C1-C4). |
| File split needed | **No** — provided the three dead members are deleted (466 → ~402). If deletion is declined, a pre-planned `QfcItemController.InitializationSeams.cs` split is documented in §9, with its csproj + ledger obligations. The **test** fixture will need a second file, which does require a `QuickFiler.Test.csproj` entry. |
| Exemption boundary | **7 → 1.** Three deleted with their dead members, three de-exempted and covered, one (`InitializeAsync`, line 200) retained with a rewritten, member-specific rationale. |
| Latent defects found | 5 (§7): three dead exempt members (Moderate), ignored `async` ctor parameter (Low), un-seamable high-confidence ctor (Low), stale `_mailActions` across `Cleanup`/`SaveParameters` (Moderate, latent), plus the `InitializeAsync` thread-affinity follow-up (Informational). |
| Corrections to the brief / epic | (a) All seven exemption comments in this file cite a barrier that no longer exists; (b) `[ExcludeFromCodeCoverage]` does **not** cover lambdas declared inside the exempt method — all 11 currently-uncovered lines are such lambdas; (c) three of the seven exempt members are unreachable dead code, so their exemption is not an "irreducible remainder" under the epic's own standard. |
