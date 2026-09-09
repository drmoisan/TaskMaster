# Issue #821 — Teardown-guard enumeration gaps (Site A + Site B)

Timestamp: 2026-09-08T23-45
Branch: `bug/teardown-guard-enumeration-gaps-821`
Worktree: `<repo-root>/.claude/worktrees/agent-<id>` (preparation-time worktree; the execution worktree differs)
Scope: research only. No source, test, or configuration file was modified.

Every line number below was re-derived by reading the file in this worktree at the timestamp above.

---

## 0. Corrections to line numbers carried in the delegation prompt and in `issue.md` / `spec.md`

| Claim as supplied | Verified state | Verdict |
|---|---|---|
| `QfcHomeController.cs:405` reads `ParentCleanup?.Invoke();` | `QuickFiler/Controllers/QfcHomeController.cs:405` = `                ParentCleanup?.Invoke();` | Correct |
| `finally` opened at `:403` | `:403` = `            finally` | Correct |
| `Cleanup()` begins at `:371` | `:371` = `        public void Cleanup()` | Correct |
| `issue.md:9` and `spec.md:12` cite the defect as `QfcHomeController.cs:403` | `:403` is the `finally` keyword; the `Invoke` is at `:405` | **Imprecise in the issue/spec.** Cite `:405` for the invocation and `:403-407` for the block. |
| `ProgressViewer.cs` "`CancelSource` property setter spans lines 53-62" | `:53` is the **field** `private CancellationTokenSource? _cancelSource;`. The property declaration spans `:54-62`; the `set` accessor body spans `:57-61`. | **Off by one at the start.** Use `:54-62` for the property, `:57-61` for the setter. |
| `ButtonCancel.Enabled = value != null;` at `:60` | `:60` = `                ButtonCancel.Enabled = value != null;` | Correct |
| `SetCancellationTokenSource` spans `:64-68` | Correct | Correct |
| `CancelButton_Click` spans `:70-77`; `_cancelSource!.Cancel();` at `:75` | Correct | Correct |
| `#nullable enable` on `ProgressViewer.cs` | `:1` = `#nullable enable` | Correct |
| `QfcHomeControllerCleanupTests.cs:115` carries the `Times.Once` assertion | `:115` = `                parentCleanup.Verify(x => x.Invoke(), Times.Once);` | Correct |

---

## 1. Enumeration 1 — the cleanup-invoker set (Site A)

### 1.1 `ParentCleanup` — declaration and every assignment site

**Type.** `System.Action`. Declared as an auto-property, not a field:

`QuickFiler/Controllers/QfcHomeController.cs:154`
```csharp
internal System.Action ParentCleanup { get; set; }
```

Because it is an auto-property with a compiler-generated backing field, the read-into-local-then-clear idiom must be written against the property (`ParentCleanup`), not against a named field. This is a real difference from the sibling sites, which all use a plain `_parentCleanup` field. It does not block the idiom — `System.Action parentCleanup = ParentCleanup; ParentCleanup = null; parentCleanup?.Invoke();` works identically — but the plan must not assume a `_parentCleanup` field exists on this type.

| # | Assignment site | Code | Notes |
|---|---|---|---|
| A1 | `QuickFiler/Controllers/QfcHomeController.cs:32` | `ParentCleanup = parentCleanup;` | Public ctor `QfcHomeController(IApplicationGlobals, System.Action)` at `:29-33` |
| A2 | `QuickFiler/Controllers/QfcHomeController.cs:119` | `ParentCleanup = parentCleanup;` | `internal async Task InitAsync(...)` at `:108-150`, reached only from `LaunchAsync` `:60-66` |

There is no third assignment and no site that clears it. The private parameterless ctor at `:27` leaves it null.

**What the supplied callback actually does — production.** Exactly one production implementation is supplied, at three call sites, and it is the ribbon release:

`TaskMaster/Ribbon/RibbonController.cs:148-153`
```csharp
private void ReleaseQuickFiler()
{
    _quickFiler = null;
    _quickFilerLoaded = false;
    SetHighConfidenceModeForLaunch(false);
}
```

| Supply site | Route | Releases the ribbon? |
|---|---|---|
| `TaskMaster/Ribbon/RibbonController.cs:104-107` | `new QfcHomeController(Globals, ReleaseQuickFiler).Init()` (sync `LoadQuickFiler`) | Yes |
| `TaskMaster/Ribbon/RibbonController.cs:118-121` | `QfcHomeController.LaunchAsync(Globals, ReleaseQuickFiler)` (`LoadQuickFilerAsync`) | Yes |
| `TaskMaster/Ribbon/RibbonController.cs:139-142` | `QfcHomeController.LaunchAsync(Globals, ReleaseQuickFiler)` (`LoadQuickFilerHighConfidenceAsync`) | Yes |

`ReleaseQuickFiler` is idempotent in isolation (three plain assignments). The harm of a double invocation is not corruption of `_quickFiler` / `_quickFilerLoaded` but the third statement: `SetHighConfidenceModeForLaunch(false)` mutates a settings value. A second, later, unpaired invocation can therefore reset high-confidence mode after a *subsequent* launch has set it to `true` — the window is `LoadQuickFilerHighConfidenceAsync` at `:137-138` setting the flag, followed by a stale second `ParentCleanup` firing from the *previous* controller instance. That is the concrete user-visible harm, and it is why "at most once per controller" is the right invariant rather than "the callback happens to be idempotent".

**What the supplied callback does — tests.** Test suppliers are `Mock<System.Action>` objects (`QfcHomeControllerCleanupTests.cs:47,82,140`; `QfcHomeControllerPropertyTests.cs:59`; `QfcHomeControllerTests.cs:51`; `QfcHomeControllerRunAsyncTests.cs:61`; `QfcHomeControllerIterationTests.cs:51`). None releases a ribbon; they only record invocation counts.

### 1.2 Every call site of `QfcHomeController.Cleanup()`

`Cleanup` is also handed out as a **method group** — that is an indirect call site and is where the production reachability actually lives.

| # | File:line | Form | Classification |
|---|---|---|---|
| C1 | `QuickFiler/Controllers/QfcHomeController.cs:100` | method group `Cleanup` passed as the `cleanup` argument to `QfcFormControllerLoader` inside `Init()` | Indirect. Stored at `QfcFormController.cs:45` as `_parentCleanup`; invoked at `QfcFormController.SetupDisposal.cs:271`. **Reachable once only** per `QfcFormController` instance, because `:269-270` clears the field before invoking. |
| C2 | `QuickFiler/Controllers/QfcHomeController.cs:142` | method group `Cleanup` passed to `QfcFormControllerLoader` inside `InitAsync()` | Same as C1. **Reachable once only** per `QfcFormController` instance. |
| C3 | `QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs:59` (invoked at `:62` through `act`) | `controller.Cleanup()` | Test. Reachable once on that instance. |
| C4 | `QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs:98` | `controller.Cleanup()` | Test. **First of two on the same instance** (instance created at `:89`). |
| C5 | `QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs:120` | `controller.Cleanup()` | Test. **Second on the same instance as C4.** |
| C6 | `QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs:151` | `controller.Cleanup()` | Test. Reachable once on that instance. |
| C7 | `QuickFiler.Test/Controllers/QfcHomeControllerPropertyTests.cs:98` | `_controller.Cleanup()` | Test. Reachable once on that instance. |

`RibbonController` never calls `Cleanup()` directly — `SearchScope:` `TaskMaster/**/*.cs`; `SearchPatterns:` `Cleanup`; `SearchResult:` one hit, `TaskMaster/AppGlobals/AppFileSystemFolderPaths.cs:295`, which is a `//TODO:` comment. Confirmed.

**Aggregate production classification: reachable more than once — YES, but only across two `QfcFormController` instances.** `Cleanup` is captured *by two distinct construction paths* (C1 in `Init()`, C2 in `InitAsync()`). Each captured copy is independently guarded by the 810 idiom, so each can fire once. A single home controller that ran both `Init()` and `InitAsync()`, or that had `_formController` reconstructed, would produce two `QfcHomeController.Cleanup()` calls and therefore two `ParentCleanup` invocations. `RibbonController` today uses `Init()` (`:107`) or `LaunchAsync`/`InitAsync` (`:118`, `:139`) but never both on the same instance, so this remains latent.

### 1.3 Does any path call `Cleanup()` twice on the same instance today?

**Production: no.** The two indirect routes are mutually exclusive per instance under `RibbonController`, and each is single-shot by construction. State this plainly in the fix: **the Site A defect is latent in production.**

**Tests: yes.** `QfcHomeControllerCleanupTests.cs:98` and `:120` call `Cleanup()` twice on the controller created at `:89`. The second call *does* invoke `ParentCleanup` a second time on today's code — the existing assertion simply cannot see it (see F1 below).

### 1.4 Sibling cleanup / disposal sites that invoke an owner callback

`SearchScope:` the entire `QuickFiler/` tree (`QuickFiler/Controllers/`, `QuickFiler/Viewers/`, `QuickFiler/Helper Classes/`, `QuickFiler/Legacy/`, `QuickFiler/Interfaces/`).
`SearchPatterns:` (a) `\.Invoke\(\)`; (b) `(private|internal|public|protected)\s+(readonly\s+)?(System\.)?Action\??\s+_?\w+\s*[;={]`; (c) `delegate\s+\w+\s+\w+\(`; (d) `_parentCleanup|parentCleanup|ParentCleanup`; (e) `public void Cleanup\(\)`.
`SearchResult:` the table below is the complete set. No other compiled site in `QuickFiler/` invokes a parent/owner/disposal callback.

| # | File:line | Shape | Idiom applied? | Reachable > once? | In this feature's write set? |
|---|---|---|---|---|---|
| **S-A** | `QuickFiler/Controllers/QfcHomeController.cs:405` | `ParentCleanup?.Invoke();` inside `finally` | **No — unguarded** | Latent (see 1.3) | **Yes — this is Site A** |
| S-B | `QuickFiler/Controllers/QfcFormController.SetupDisposal.cs:269-271` | read-into-local, clear field, invoke local | **Yes** (comment cites Issue #810 AC4) | Yes — `ActionCancelAsync` `finally` at `EventHandlers.cs:173`, and `ButtonCancel_Click` is `async void` and re-clickable | No |
| S-C | `QuickFiler/Controllers/EfcFormController.cs:305-310` | read-into-local, clear field, `if (… is not null) Invoke()` | **Yes** | Yes | No — **out of scope** |
| **S-D** | `QuickFiler/Controllers/EfcHomeController.cs:349` | `_parentCleanup.Invoke();` — **not even null-conditional** | **No — unguarded, and NRE-prone** | Yes: `Cleanup` is handed out as a method group at **two** sites, `EfcHomeController.cs:90` and `:234`, each stored in a separate `EfcFormController._parentCleanup` | No — **out-of-scope finding, see §5** |
| S-E | `QuickFiler/Viewers/BreadcrumbMessengerHub.cs:483-485` (`BreadcrumbResourceOwner.Dispose(bool)`) | read-into-local, clear field, `dispose?.Invoke();` | **Yes** | Yes (`Component.Dispose` is re-entrant) | No |
| S-F | `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs:364-365` (`BreadcrumbNavigationSubscription.Dispose`) | `Interlocked.Exchange(ref _detach, null)` then `detach?.Invoke();` | **Yes** (atomic variant) | Yes | No — **out of scope** |
| S-G | `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs:215` (`Release()`) | `_detachPopupMessenger();` on a `readonly` field | No — but guarded by a **different, sound** mechanism: `Invalidate(release: true)` at `:361-373` sets `_released` under `lock (_sync)` and returns `false` on re-entry | Guarded | No — **out of scope** |
| S-H | `QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs:148-158` (`DetachHandlers`) | `_detachHandlers();` on a `readonly` field, wrapped in try/catch-log | No clear-after-invoke, but the delegate is an idempotent `-=` detach | Yes, harmlessly | No |
| S-I | `QuickFiler/Legacy/QuickFileController.cs:664` | `_parentCleanup.Invoke();` unguarded | No | n/a | **Not compiled** |
| S-J | `QuickFiler/Legacy/QfcLauncher.cs:62` | `_parentCleanup.Invoke();` unguarded | No | n/a | **Not compiled** |

`QuickFiler/Legacy/**` carries no `<Compile Include>` entry — `SearchScope:` `QuickFiler/QuickFiler.csproj`; `SearchPatterns:` `Legacy`, and `QuickFileController\.cs|QfcLauncher\.cs|QfcController\.cs|QfcFormLegacyViewer\.cs`; `SearchResult:` zero hits for both. S-I and S-J are therefore dead source and must not be counted as live defects.

**Answer to the enumeration question: beyond Site A there is exactly ONE additional unguarded compiled site — S-D, `EfcHomeController.cs:349`** (plus two uncompiled legacy sites). It is owned by no sibling feature named in the constraint list, but it is not in this feature's write set either, so it is reported as an out-of-scope finding in §5.

Also enumerated and found clean: `QfcItemController.ViewerSetup.cs:414-450` and `QfcCollectionController.cs:2128-2140` are `Cleanup()` methods that invoke **no** owner callback at all; `EfcItemController.cs:231` likewise. They are reachable more than once (`QfcCollectionController.cs:749,765,792,1541`) and are already field-nulling and idempotent.

### 1.5 Where item 810 applied the idiom "one level down" — verbatim

`QuickFiler/Controllers/QfcFormController.SetupDisposal.cs:262-272`:

```csharp
            finally
            {
                // Issue #810 (AC4): read the callback into a local and clear the field before
                // invoking it. Reading and clearing first is what makes exactly-once
                // unconditional. A finally that invoked before clearing would close the
                // earlier-throw hole but not the callback-throws hole, because a callback that
                // threw would leave the field set for a repeat pass to invoke a second time.
                System.Action parentCleanup = _parentCleanup;
                _parentCleanup = null;
                parentCleanup?.Invoke();
            }
```

This is the idiom the plan should copy. The only adaptation needed for `QfcHomeController` is that the target is the auto-property `ParentCleanup` rather than a `_parentCleanup` field:

```csharp
                System.Action parentCleanup = ParentCleanup;
                ParentCleanup = null;
                parentCleanup?.Invoke();
```

The rationale sentence in that comment ("a callback that threw would leave the field set") applies unchanged at Site A, because Site A's invocation is also inside a `finally`.

---

## 2. Enumeration 2 — the `CancellationTokenSource` sharer set (Site B)

### 2.1 Every source that reaches a `ProgressViewer`

`ProgressViewer` is constructed at exactly two production sites, both inside the two tracker types:

- `UtilitiesCS/Threading/ProgressTracker.cs:37-41` — `new ProgressViewer { UiDispatcher = …, CancelSource = _cancelSource }`
- `UtilitiesCS/Threading/ProgressTrackerAsync.cs:37-41` — identical shape

`SearchScope:` all `**/*.cs`; `SearchPatterns:` `new ProgressViewer`; `SearchResult:` 8 hits — the 2 production sites above and 6 in `UtilitiesCS.Test/Threading/`.

`ProgressTrackerAsync` has **no production construction site** — `SearchScope:` all `**/*.cs`; `SearchPatterns:` `new ProgressTrackerAsync\(`; `SearchResult:` 7 hits, all in `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`. It is dormant production code. The single live production `ProgressViewer` factory is `ProgressTracker.Initialize()`.

`ProgressViewer.SetCancellationTokenSource(...)` has **no production caller**. The single call at `UtilitiesCS/Threading/ProgressTrackerPane.cs:17` targets `ProgressPane`, a different type (`ProgressTrackerPane.cs:55` declares `private ProgressPane? _progressViewer;`), not `ProgressViewer`. The only caller of `ProgressViewer.SetCancellationTokenSource` in the repository is the test at `UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs:141`.

Root-tracker construction sites in production (`SearchPatterns:` `new ProgressTracker\(`; non-test hits only), with the two-argument child ctor excluded because `ProgressTracker.cs:60-69` does **not** copy `_cancelSource`:

| Source | Constructed at | Reaches `ProgressViewer` via |
|---|---|---|
| **S1** | `QuickFiler/Controllers/QfcHomeController.cs:54` — `var tokenSource = new CancellationTokenSource();` in `LaunchAsync` | `:56` `new ProgressTracker(tokenSource).Initialize()` |
| **S2** | `UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs:228` | `:230` `new ProgressTracker(tokenSource).Initialize()` (`RebuildAsync`, `[ExcludeFromCodeCoverage]` at `:221`) |
| **S3** | `UtilitiesCS/Threading/ProgressPackage.cs:25` — `_cancelSource = cancelSource ?? new CancellationTokenSource();` | `:28` `new ProgressTracker(_cancelSource, screen).Initialize()`, reached from `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailDataMiner.FolderExtraction.cs:129` `ProgressPackage.CreateAsTupleAsync()` with no arguments |

Every other production `ProgressPackage` initialization uses the **pane** overload (`ProgressPackage.cs:33-45`), which builds a `ProgressTrackerPane`/`ProgressPane` and never touches `ProgressViewer`. Verified individually at `QuickFiler/Controllers/BayesianPerformanceController.cs:58-62`, `UtilitiesCS/EmailIntelligence/ClassifierGroups/MulticlassEngine.cs:130-137` and `:148-155`, `.../Categories/CategoryClassifierGroup.cs:109-116` and `:126-133`, `.../OlFolder/OlFolderClassifierGroup.cs:280-287`, `.../Bayesian/Performance/BayesianPerformanceMeasurement.cs:215-220`, `:1351-1353`, `:1403-1405`.

`TaskMaster/AppGlobals/AppAutoFileObjects.cs:643` `CancelSource` — the long-lived add-in source — reaches `ProgressTrackerPane` (`:634`) and therefore `ProgressPane`, **not** `ProgressViewer`. It is not a `ProgressViewer` sharer.

S1 is the source the #810 lineage concerns. S2 and S3 are additional and were never enumerated by anyone; **neither is ever disposed by anyone**, which is a separate (benign) leak and is out of scope here.

### 2.2 Every holder of S1 (`QfcHomeController.cs:54`)

Ordered by the point at which each receives the reference.

| # | Holder | Stores at | Constructs | Stores | `Cancel()` | `Dispose()` | reads `.Token` | nulls own ref |
|---|---|---|---|---|---|---|---|---|
| H1 | `LaunchAsync` local `tokenSource` | `QuickFiler/Controllers/QfcHomeController.cs:54` | **Yes** `:54` | local | no | no | **Yes** `:55` | n/a (scope) |
| H2 | `ProgressTracker._cancelSource` | `UtilitiesCS/Threading/ProgressTracker.cs:22` (ctor); field decl `:80` | no | **Yes** | **no** | **no** | no | **no** |
| H3 | **`ProgressViewer._cancelSource`** | `UtilitiesCS/Threading/ProgressViewer.cs:59` (via the `CancelSource` setter, assigned from `ProgressTracker.cs:40`); field decl `:53` | no | **Yes** | **Yes `:75`** | **no** | no | **no** |
| H4 | `QfcHomeController._tokenSource` | `QuickFiler/Controllers/QfcHomeController.cs:117`; field decl `:471`; exposed `:472-475` | no | **Yes** | no | **Yes `:389`** | `:468` (other source) | **Yes `:390`** |
| H5 | `QfcDatamodel._tokenSource` | `QuickFiler/Controllers/QfcDatamodel.cs:66` (`model.TokenSource = tokenSource` in `LoadAsync`) and `:315` (ctor); field decl `:165` | no | **Yes** | **Yes `:77`** and **`QfcDatamodel.QueueProcessing.cs:50`** | no | no | **no** |
| H6 | `QfcFormController._tokenSource` | `QuickFiler/Controllers/QfcFormController.cs:39`; field decl `:187`; exposed `:188-191` | no | **Yes** | via the parent property at `QfcFormController.EventHandlers.cs:133` (`_parent?.TokenSource?.Cancel()`), not via its own field | no | `Token` property | **no** |
| H7 | `QfcCollectionController._tokenSource` | `QuickFiler/Controllers/QfcCollectionController.cs:42`; field decl `:112`; exposed `:113-117` | no | **Yes** | no | no | no | **no** |
| H8 | `QfcItemController._tokenSource` | `QuickFiler/Controllers/QfcItemController.Initialization.cs:386` (`= _homeController.TokenSource`); field decl `QfcItemController.cs:59` | no | **Yes** | no | no | no | **no** |
| H9 | `ConversationResolver._tokenSource` | `QuickFiler/Helper Classes/ConversationResolver.cs:79` (ctor) and `:97`, `:140`, `:175` (property set); field decl `:246` | no | **Yes** | no | no | no | **no** |

**Derived sharer count for S1: 9 holders.** Of those, **8 are storing holders** (H2–H9) and **4 code sites call `Cancel()`**: `QfcDatamodel.cs:77`, `QfcDatamodel.QueueProcessing.cs:50`, `QfcFormController.EventHandlers.cs:133`, `ProgressViewer.cs:75`.

### 2.3 Verdict on the "fourth sharer" claim

**CONTRADICTED as written; CONFIRMED only under a narrower reading.**

- As a count of **holders**, `ProgressViewer` is the **third** to receive the reference (H3 of 9), not the fourth. The derived holder count is **9**, not 4.
- The "fourth" figure originates in `docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/research/research.2026-09-07T22-10.md:220-227`, whose table is headed **"Every `Cancel()` on the shared instance"** and lists four rows, with `ProgressViewer.cs:75` fourth. Under that reading — *fourth site that calls `Cancel()`* — the figure is correct and my independent enumeration reproduces it exactly (same four rows, §2.2).

The plan must not restate "fourth sharer". Restate as: **`ProgressViewer` is the fourth and last site that calls `Cancel()` on the shared source, and the third of nine holders.** The substantive point the #810 research was making — that it is the only `Cancel()` driven by a user gesture rather than by teardown ordering, and that it holds its own captured reference so nulling `QfcHomeController._tokenSource` does not affect it — is independently confirmed here and is unchanged.

### 2.4 Disposal ordering, and whether `Cancel()` can follow `Dispose()`

**The only holder that disposes S1 is H4, `QfcHomeController.cs:389`**, inside the second guarded block of `Cleanup()` (`:386-402`):

```csharp
                _datamodel?.Cleanup();
                _tokenSource?.Dispose();
                _tokenSource = null;
                _datamodel = null;
```
(`:388`, `:389`, `:390`, `:391`)

`SearchScope:` `QuickFiler/**/*.cs`, `UtilitiesCS/Threading/*.cs`; `SearchPatterns:` `_tokenSource|_cancelSource|_cancellationSource` and `!\.(Cancel|Dispose)\(\)`; `SearchResult:` `QfcHomeController.cs:389` is the only `Dispose()` on this instance. Confirmed.

**Can a holder call `Cancel()` after that disposal? Yes — H3, and only H3.**

- H5 (`QfcDatamodel`) is cut off: `:388` runs its `Cleanup()` *before* the dispose, and `:391` nulls `_datamodel`, so a repeat `Cleanup()` cannot re-enter it. (This nulling is item 810's AC3 fix and is present.)
- H6 routes through `_parent?.TokenSource?.Cancel()`; `:390` nulls the field the property reads, so the `?.` short-circuits.
- H2, H7, H8, H9 never call `Cancel()`.
- **H3 holds its own captured reference (`ProgressViewer.cs:59`), never nulls it, and is driven by a user click, not by teardown ordering.** Nothing in `Cleanup()` reaches it. Its `Cancel()` at `:75` is unguarded by `try`/`catch` and by any null or disposal check.

On .NET Framework 4.8, `CancellationTokenSource.Cancel()` throws `ObjectDisposedException` after `Dispose()`. **The `ObjectDisposedException` path is the reachable half of Site B; the null path is latent** (see §2.5). The precondition is a still-open `ProgressViewer` at the moment `QfcHomeController.Cleanup()` runs. In the ordinary flow `ProgressTracker.Report` closes the viewer at 100% (`ProgressTracker.cs:163-176`), and the cancel-path catch at `QfcHomeController.cs:77-78` also reports 100 before returning, so the viewer normally closes first. The plan should state that this is an ordering *convention*, not an enforced invariant, and that the guard is what makes it structural.

### 2.5 Every path that can enable `ButtonCancel`

`ButtonCancel` is `private System.Windows.Forms.Button ButtonCancel;` (`UtilitiesCS/Threading/ProgressViewer.Designer.cs:85`). `ProgressViewer` has exactly two partial parts — `SearchScope:` all `**/*.cs`; `SearchPatterns:` `partial class ProgressViewer|class ProgressViewer`; `SearchResult:` `ProgressViewer.Designer.cs:3`, `ProgressViewer.cs:16`, plus the unrelated test class `ProgressViewer_Tests.cs:31`. So only those two files can touch it by name.

| # | Path | File:line | Sets `Enabled` to | Can it enable while `_cancelSource` is null? |
|---|---|---|---|---|
| E1 | Constructor | `ProgressViewer.cs:24` | `false` | No (disables) |
| E2 | `CancelSource` setter | `ProgressViewer.cs:60` | `value != null` | **No** — this path is correct; it is the one path that already enforces the invariant |
| E3 | `SetCancellationTokenSource` | `ProgressViewer.cs:67` | **`true`, unconditionally** | **Yes.** The parameter is declared non-nullable but is never checked, and the assignment at `:66` is unconditional. `SetCancellationTokenSource(null!)`, or a call from any nullable-oblivious caller, leaves `_cancelSource` null with the button enabled. |
| E4 | Designer `InitializeComponent` | `ProgressViewer.Designer.cs:56-65` | never assigns `Enabled`; `Button.Enabled` defaults to `true` | **Yes, transiently** — between `InitializeComponent()` at `ProgressViewer.cs:20` and the explicit `false` at `:24`. Not reachable by a user click (no handle, no message pump yet), but it means "enabled" is the *default* state, not an opted-in one. |
| E5 | External code | none | n/a | Not applicable — the field is private |

`SearchScope:` all `**/*.cs`; `SearchPatterns:` `"ButtonCancel"` (string literal, for reflection by name); `SearchResult:` the only reflective access to `ProgressViewer.ButtonCancel` is `UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs:55-70`, and it only **reads** `.Enabled` (`:75-77`) and calls `PerformClick()` (`:79`). No code anywhere writes `ProgressViewer.ButtonCancel.Enabled` from outside the type.

**Conclusion:** the issue's framing — that "the setter at line 60 is a second path to enabling the button that does not go through `SetCancellationTokenSource`" — is **backwards**. `:60` is the *correct* path; it is `SetCancellationTokenSource` at `:67` that enables unconditionally. The plan should fix `:67` (either `Enabled = tokenSource is not null;` or an `ArgumentNullException` guard), not `:60`. Because `SetCancellationTokenSource` has no production caller (§2.1), this is a latent hole rather than a live one, but it is the hole the comment at `:72-74` names and it should be closed so the comment's invariant becomes true.

### 2.6 `CancelButton_Click` wiring, `Form` derivation, and testability

**Wiring.** Designer-generated:

`UtilitiesCS/Threading/ProgressViewer.Designer.cs:65`
```csharp
            this.ButtonCancel.Click += new System.EventHandler(this.CancelButton_Click);
```

**Type.** `UtilitiesCS/Threading/ProgressViewer.cs:16` — `public partial class ProgressViewer : Form`. It is `Form`-derived and is therefore covered by the CLAUDE.md UT2 WinForms coverage exemption clause (b), but it is **not** marked `[ExcludeFromCodeCoverage]` and carries no `coverage.config` exclusion (`SearchScope:` `coverage.config`; `SearchPatterns:` `ProgressViewer|ProgressPane`; `SearchResult:` no matches). It is in the coverage denominator today.

**Testability — three facts the plan can rely on, all already proven in-repo:**

1. **The handler can be invoked without a window handle.** `UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs:126-164` constructs `new ProgressViewer()` at `:137`, **never calls `Show()`**, and invokes `CancelButton_Click` by reflection at `:144-155`. `this.Close()` on a never-shown `Form` disposes it without requiring a handle — the test's own doc comment records this at `:120-123`.
2. **The construction precondition is a non-null `SynchronizationContext.Current`**, because `ProgressViewer.cs:23` calls `TaskScheduler.FromCurrentSynchronizationContext()`. Every test installs one and restores it in a `finally` (`:43-45` / `:97`, `:129-131` / `:162`, `:317-319` / `:348`). The class is `[STATestClass]` (`:30`).
3. **A ctor-free instance is already available** via `CreateHeadlessViewer()` at `:33-34` (`FormatterServices.GetUninitializedObject`). Note its limitation: it bypasses `InitializeComponent`, so `ButtonCancel` is **null** on such an instance. Any test that must touch `ButtonCancel.Enabled` has to use the real ctor path, as `:49` and `:323` do.

**Caveat for the "does not throw" tests the issue asks for:** `MethodInfo.Invoke` wraps a handler exception in `TargetInvocationException`. A test asserting the handler does not throw must assert on the reflective call as-is (`Action act = () => cancelClick.Invoke(...); act.Should().NotThrow();` is fine because a clean call throws nothing), but a test asserting a *specific* exception type from a directly-callable member must target that member, not the reflected handler. This is one of the reasons the recommended fix in §4 splits the throwing logic into its own member.

### 2.7 State of `UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs`

**It exists** (353 lines) and is registered exactly once in the project file:

`UtilitiesCS.Test/UtilitiesCS.Test.csproj:506`
```xml
    <Compile Include="Threading\ProgressViewer_Tests.cs" />
```

| Test | Lines | Covers | Seam used |
|---|---|---|---|
| `CancelSource_WhenAssigned_EnablesButtonAndCancelsSameSourceOnClick` | `:40-99` | E2 (setter enables) and the click actually cancelling that source | Real ctor + `Show()`; reflection on the private `ButtonCancel` field; `PerformClick()` |
| `CancelPath_WhenInvoked_CancelsTokenSource` | `:125-164` | `CancelButton_Click` cancels the source | Real ctor, **no `Show()`**; `SetCancellationTokenSource`; reflection `GetMethod("CancelButton_Click", NonPublic \| Instance)` then `Invoke` |
| `Constructor_PopulatesSyncContextAndScheduler` | `:192-220` | ctor captures `UiSyncContext` / `UiScheduler` | Real ctor under an installed context |
| `UiDispatcher_SetterAndGetter_RoundTripAssignedValue` | `:239-258` | property round-trip | `CreateHeadlessViewer()` |
| `UiThreadNumber_SetterAndGetter_RoundTripAssignedValue` | `:277-294` | property round-trip | `CreateHeadlessViewer()` |
| `CancelSource_SetterAndGetter_RoundTripAssignedValue` | `:314-350` | property round-trip | Real ctor |

**Gaps relative to this issue:** nothing exercises `_cancelSource == null` at click time, nothing exercises a disposed source at click time, and nothing asserts that `SetCancellationTokenSource` refuses (or declines to enable for) a null argument.

---

## 3. Fact re-derivations

### F1 — the existing Site A test's assertion ordering — **CONFIRMED**

`QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs`, method `Cleanup_DisposesTokenSourceAndDetachesWorkerCompleted` (`:78-127`):

- `:89-92` — the single controller instance is created.
- `:98` — first `controller.Cleanup();`
- `:115` — `parentCleanup.Verify(x => x.Invoke(), Times.Once);`
- `:117-119` — comment attributing the following lines to Issue #810 (AC3).
- `:120` — second `controller.Cleanup();`
- `:121-125` — `datamodel.Verify(x => x.Cleanup(), Times.Once, …)`

The second `Cleanup()` at `:120` is **five lines after** the `Times.Once` assertion at `:115`. Moq evaluates `Verify` eagerly at the point of the call, so the assertion observes exactly one invocation and cannot fail regardless of what `:120` does. The claim in `issue.md:12-13` is exact.

Two additional facts the plan needs:

- The `Times.Once` assertion at `:115` has **no `because` string**, unlike its siblings at `:62-67`, `:105-109`, `:110-114` and `:121-125`. Adding one when the line moves is consistent with the file's own style.
- Simply relocating `:115` to after `:120` produces a test that **fails on today's code** and passes after the fix. That is exactly the "must fail before" property the issue asks for, and it requires no new test method — though a dedicated, named test (`Cleanup_CalledTwice_InvokesParentCleanupOnce`) reads better and does not entangle the AC3 datamodel assertion with the AC-new ribbon assertion. Note that `QfcFormControllerCancelTeardownTests.cs:379` already carries the sibling-level test `ActionCancelAsync_CalledTwice_InvokesParentCleanupOnce`, so a same-named home-level test matches the established naming.

### F2 — `ProgressViewer` is the fourth sharer — **CONTRADICTED**

See §2.3. Derived holder count for S1 is **9**; `ProgressViewer` is the **third** holder to receive the reference. The "fourth" figure is correct only for the narrower family "sites that call `Cancel()` on the shared source", which is what the #810 research table at `research.2026-09-07T22-10.md:220-227` actually enumerated. Do not carry the phrase "fourth sharer" into the spec.

### F3 — removing the `!` would produce a `CS86xx` promoted to an error — **CONFIRMED**

- `UtilitiesCS/Threading/ProgressViewer.cs:1` is `#nullable enable`, so the file participates in nullable flow analysis.
- `:53` declares `private CancellationTokenSource? _cancelSource;` — an explicitly nullable reference.
- Writing `_cancelSource.Cancel()` without the `!` and without a preceding null check therefore yields **CS8602, "Dereference of a possibly null reference."**
- `UtilitiesCS/UtilitiesCS.csproj` carries **no** `NoWarn`, `WarningsNotAsErrors`, or `TreatWarningsAsErrors` element (`SearchScope:` `UtilitiesCS/UtilitiesCS.csproj`; `SearchPatterns:` `NoWarn|WarningsNotAsErrors|TreatWarningsAsErrors|LangVersion|Nullable`; `SearchResult:` two hits, `:10 <LangVersion>12.0</LangVersion>` and a comment at `:1307`).
- `.editorconfig` does not downgrade any `CS86xx` diagnostic (`SearchScope:` `.editorconfig`; `SearchPatterns:` `CS86|CS8602|nullable`, case-insensitive; `SearchResult:` one hit, a prose comment at `:25`). Its catch-all `dotnet_analyzer_diagnostic.severity = suggestion` at `:27` applies to analyzer rules, not to compiler diagnostics.
- Toolchain step 3, `msbuild … /p:TreatWarningsAsErrors=true`, therefore promotes CS8602 to an error.

**Implication for the fix: the guard is not optional.** Any change that removes the `!` must supply a null check the compiler can see. The `?? throw` form recommended in §4 satisfies the flow analysis and needs no suppression.

*(The delegation prompt lists three facts under a heading that says four; there is no F4 in the supplied list. Recorded here so the omission is not read as a missing verdict.)*

---

## 4. Site B — ownership finding and design recommendation

### 4.1 Ownership: `ProgressViewer` is a **BORROWER**, not an owner

| Evidence | Location |
|---|---|
| `ProgressViewer` never constructs a `CancellationTokenSource` | `SearchScope:` `UtilitiesCS/Threading/ProgressViewer.cs` + `.Designer.cs`; `SearchPatterns:` `new CancellationTokenSource`; `SearchResult:` no matches |
| The source is always supplied from outside | `ProgressTracker.cs:40` (production), `ProgressViewer.cs:66` (test-only path) |
| `ProgressViewer.Dispose(bool)` disposes only `components` | `ProgressViewer.Designer.cs:14-21` |
| For S1, a different holder disposes it | `QfcHomeController.cs:389` |
| For S2 and S3, **no** holder disposes it | §2.1 |

**Consequence for the fix: the fix may guard, and may swallow the disposed case, but must NOT dispose and must NOT rethrow out of the handler.** A borrower that disposes a source another holder still uses would convert a benign cancel click into a repository-wide `ObjectDisposedException` generator. A borrower that rethrows out of a WinForms handler surfaces an unhandled exception dialog to an Outlook user, which is precisely what the issue's Expected Behavior forbids.

### 4.2 Does the "preserve the NRE-if-null behavior" intent still stand?

**Partly. Split it, because the comment conflates two distinct failure modes.**

- **Null `_cancelSource` while the button is enabled** is a *wiring defect* — the type was configured incorrectly by its host. Nothing the user can do is at fault, and silently doing nothing hides a real bug. The fail-fast intent stands, and per the repository requirement it must be expressed as a **thrown exception carrying a message**, not as `!`. Per CLAUDE.md §C#4.1 ("fail fast with explicit exceptions when invariants are violated") and §3 ("enforce invariants at construction/initialization time"), the right type here is `InvalidOperationException` — the object is in an invalid *state* for the operation, which is distinct from a bad argument.
- **`ObjectDisposedException` from a disposed source** is a *lifecycle race*, not a defect in this type. The owner disposed the source because the tracked operation is over. There is nothing left to cancel, and the correct response is to do nothing. Swallowing it here is not a silent-error suppression forbidden by CLAUDE.md §3, because the condition is expected and the narrow catch documents why — it is the same reasoning already accepted in-repo at `QfcFormController.SetupDisposal.cs:229-232` (`catch (ObjectDisposedException) { // A repeated Cleanup() re-enters here on the already-disposed queue. }`).

### 4.3 Recommended shape

Split the logic out of the event handler so the throw is directly observable and testable, and keep the handler as the boundary that never lets anything escape. This mirrors the established in-repo boundary pattern at `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:110-123`, whose comment records the rationale: *"This handler is `async void`, so a rethrow becomes an unhandled Outlook UI-thread exception reporting nothing actionable."* The same reasoning applies to a synchronous WinForms handler in a VSTO add-in.

Recommended replacement for `UtilitiesCS/Threading/ProgressViewer.cs:70-77`:

```csharp
        private static readonly log4net.ILog logger = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        /// <summary>
        /// Requests cancellation on the borrowed source. This viewer does not own the source and
        /// never disposes it. Throws <see cref="InvalidOperationException"/> when the Cancel button
        /// was enabled without a source, because that is a host wiring defect rather than a user
        /// error. Returns quietly when the owner has already disposed the source, because the
        /// operation this viewer was tracking has already finished and there is nothing to cancel.
        /// </summary>
        /// <exception cref="InvalidOperationException">No cancellation source has been supplied.</exception>
        internal void RequestCancel()
        {
            CancellationTokenSource source =
                _cancelSource
                ?? throw new InvalidOperationException(
                    "ProgressViewer cancellation was requested with no CancellationTokenSource. "
                        + "Assign CancelSource or call SetCancellationTokenSource before enabling ButtonCancel."
                );

            try
            {
                source.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // The source's owner disposed it; the tracked operation is already over. Distinct
                // from the null case above, which is a wiring defect, not a lifecycle race.
                logger.Debug("Cancel requested after the token source was disposed; nothing to cancel.");
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            // Boundary: this is a WinForms handler in a VSTO add-in, so an escaping exception
            // surfaces to the Outlook user. RequestCancel carries the diagnosable message; this
            // frame logs it and still closes, so a wiring defect cannot strand the dialog open.
            try
            {
                RequestCancel();
            }
            catch (System.Exception ex)
            {
                logger.Error("ProgressViewer cancel request failed.", ex);
            }
            finally
            {
                this.Close();
            }
        }
```

Notes on each choice:

- **`?? throw` rather than a separate `if`** — it gives the compiler a provably non-null local, so CS8602 is satisfied without `!` and without a suppression, which is what F3 requires.
- **`InvalidOperationException` rather than `ArgumentNullException` or `NullReferenceException`** — the fault is object state, not an argument, and re-throwing a bare NRE with a message would be worse than the current `!` (it would look like a genuine null-deref in telemetry).
- **`internal` rather than `private` on `RequestCancel`** — `UtilitiesCS` already exposes internals to `UtilitiesCS.Test` (the existing tests reach `ButtonCancel` and `CancelButton_Click` by reflection precisely because they are private). Making the new member `internal` lets the tests assert on the exception type and message directly, without the `TargetInvocationException` wrapping described in §2.6. The plan must confirm the `InternalsVisibleTo` entry exists before relying on this; `UtilitiesCS.Test/Properties/AssemblyInfo.cs` is a sibling-owned file and must not be edited (see §6), so if the attribute is absent, fall back to `private` plus reflection, matching `ProgressViewer_Tests.cs:144-155`.
- **`finally { this.Close(); }`** — today `this.Close()` at `:76` is skipped if `Cancel()` throws, leaving the dialog on screen with a live token. Closing unconditionally is a small behavior improvement that falls inside the fix's own boundary.
- **`log4net`** — already used throughout `UtilitiesCS/Threading/` (`IdleAsyncQueue.cs:28`, `AsyncMultiTasker.cs:19`, `ApplicationIdleTimer.cs:24`, `IdleActionQueue.cs:29`, `TimeOutTask.cs:15`), so no new dependency.
- **The broad `catch (System.Exception)` at the boundary** will not break the analyzer gate: `.editorconfig:27` sets `dotnet_analyzer_diagnostic.severity = suggestion` as a catch-all, so CA1031 cannot be promoted to an error.
- **Close the enabling hole too.** Change `ProgressViewer.cs:67` from `this.ButtonCancel.Enabled = true;` to `this.ButtonCancel.Enabled = tokenSource is not null;`, making E3 agree with E2 and making the comment's stated invariant true. This does not make the `InvalidOperationException` dead code — a test can still reach it by assigning `CancelSource = null` and calling `RequestCancel()` directly, since `RequestCancel` deliberately does not consult `ButtonCancel.Enabled`.
- **File size.** `ProgressViewer.cs` is currently 93 lines; the change lands it near 125. Well under the 500-line ceiling.

### 4.4 Recommended tests (no test code written here)

| Test | Asserts | Seam |
|---|---|---|
| `RequestCancel_WhenSourceIsNull_ThrowsInvalidOperationExceptionWithMessage` | `InvalidOperationException`, message mentions `CancelSource` / `SetCancellationTokenSource` | Real ctor under an installed `SynchronizationContext`; `viewer.CancelSource = null;` then `RequestCancel()` |
| `RequestCancel_WhenSourceIsDisposed_DoesNotThrow` | no throw | Real ctor; assign a `CancellationTokenSource`, dispose it, call `RequestCancel()` |
| `CancelButton_Click_WhenSourceIsNull_DoesNotThrowOutOfTheHandler` | no throw, and the form is closed/disposed | Reflection on `CancelButton_Click`, matching `ProgressViewer_Tests.cs:144-155` |
| `CancelButton_Click_WhenSourceIsDisposed_DoesNotThrowOutOfTheHandler` | no throw, form closed | As above |
| `SetCancellationTokenSource_WithNull_DoesNotEnableButton` | `ButtonCancel.Enabled == false` | Real ctor; reflection on the private `ButtonCancel` field, matching `ProgressViewer_Tests.cs:55-70` |

All are deterministic, create no temporary files, sleep nowhere, and require no Outlook process. The existing class-level `[STATestClass]` and the install/restore `SynchronizationContext` pattern (`:43-45` / `:97`) carry over unchanged.

For Site A, one test, placed **inside** `QfcHomeControllerCleanupTests.cs`:

| Test | Asserts |
|---|---|
| `Cleanup_CalledTwice_InvokesParentCleanupOnce` | both `controller.Cleanup()` calls precede a single `parentCleanup.Verify(x => x.Invoke(), Times.Once, "<because>")`; must fail on pre-fix code |

The existing `:115` assertion should either move below `:120` or be left in place with the new dedicated test added; the plan should pick one and say why. Leaving `:115` where it is and adding a dedicated test keeps the AC3 datamodel assertion at `:121-125` uncontaminated, which is the cleaner option.

---

## 5. Out-of-scope findings (do not change under this issue; file follow-ups)

| # | Finding | Location | Why out of scope |
|---|---|---|---|
| O-1 | `EfcHomeController.Cleanup()` invokes `_parentCleanup.Invoke();` fully unguarded — no null-conditional and no read-into-local-then-clear. `Cleanup` is handed to **two** distinct `EfcFormController` instances (`:90`, `:234`), each of which will invoke it once, so a double ribbon release is reachable, and a null field produces an NRE that aborts teardown. | `QuickFiler/Controllers/EfcHomeController.cs:342-350` | Not in this feature's write set. This is the exact same defect class as Site A and should be filed as a follow-up issue with this enumeration attached. |
| O-2 | `ProgressPane.CancelButton_Click` carries the identical `_tokenSource!.Cancel();` with the identical comment, and `SetCancellationTokenSource` at `:46-50` enables unconditionally. `ProgressPane` is the **live** production progress surface (it is what `AppAutoFileObjects.LoadProgressPane` builds), so this one is more reachable than Site B. | `UtilitiesCS/Threading/ProgressPane.cs:46-59` (`!.Cancel()` at `:57`) | Not in this feature's write set. Should be filed as a follow-up and fixed with the same shape as §4.3. |
| O-3 | S2 (`SubjectMapSco.Orchestration.cs:228`) and S3 (`ProgressPackage.cs:25`) construct a `CancellationTokenSource` that no holder ever disposes. | as cited | Resource leak, not this issue's defect class. |
| O-4 | `QfcHomeController.CreateCancellationToken()` has no production caller (`SearchScope:` all `**/*.cs`; `SearchPatterns:` `CreateCancellationToken`; `SearchResult:` 6 hits — `QfcHomeController.cs:465` decl, `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs:124` call, and four `EfcHomeController` entries). Consequently the synchronous `Init()` path at `:86-106` passes a **null** `_tokenSource` to `QfcFormControllerLoader` at `:102`, and `QfcFormController.Actions.cs:38,75,131` then early-return on `_tokenSource is null`, so `LoadItems`/`LoadItemsAsync` silently do nothing on that path. `RibbonController.LoadQuickFiler()` (`:97-110`) is that path. | `QuickFiler/Controllers/QfcHomeController.cs:102`, `:465-469` | Distinct defect, distinct blast radius. File a follow-up. |
| O-5 | `ProgressTrackerAsync` is dormant production code with no construction site outside its own tests. | `UtilitiesCS/Threading/ProgressTrackerAsync.cs` | Dead-code cleanup, not this issue. |

Files named in the delegation prompt's off-limits list that this enumeration **did not** land on: `QfcItemController.FolderHandling.cs`, `StoreWrapperController*.cs`, `BreadcrumbPopupOwnerRegistry.cs`, `SDIL Reader/**`, `OlTableExtensions.*`, `TimeOutTask.cs`, `DfDeedle.cs`, `FolderPredictorTests.cs`, `.editorconfig`, `BannedSymbols.txt`, and the `Console.SetOut` restores. Two were touched by the enumeration in read-only fashion and are recorded as out-of-scope: `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs` (S-G's sibling; no change proposed) and `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` (S-F; already correct, no change proposed). `UtilitiesCS.Test/Properties/AssemblyInfo.cs` is relevant only as a read-only precondition check for the `internal RequestCancel` option in §4.3 — it must not be edited.

---

## 6. Constraints recorded

**Project-file registration.** Both test files this feature owns already carry a `Compile Include` entry, so **no new entry is required** for either:

| File | Project | Exact existing line |
|---|---|---|
| `QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs` | `QuickFiler.Test/QuickFiler.Test.csproj:175` | `    <Compile Include="Controllers\QfcHomeControllerCleanupTests.cs" />` |
| `UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs` | `UtilitiesCS.Test/UtilitiesCS.Test.csproj:506` | `    <Compile Include="Threading\ProgressViewer_Tests.cs" />` |

Each appears exactly once — no duplicate-entry hazard. Production entries, also already present: `QuickFiler/QuickFiler.csproj:330` `    <Compile Include="Controllers\QfcHomeController.cs" />`; `UtilitiesCS/UtilitiesCS.csproj:973-975` (a three-line element carrying `<SubType>Form</SubType>`) and `:976-978` for `ProgressViewer.Designer.cs` with `<DependentUpon>ProgressViewer.cs</DependentUpon>`.

**Framework.** net48. No `init` accessor, positional record, or record struct is proposed anywhere in §4. The recommended change introduces one `internal` method, one `static readonly` logger field, and one modified assignment — all net48-legal.

**Test framework.** MSTest with `[STATestClass]` on the ProgressViewer suite, Moq for the `System.Action` parent-cleanup mocks, FluentAssertions throughout. All recommended tests stay within these.

**Determinism.** No temporary files, no sleeps, no live Outlook. The disposed-source test disposes a `CancellationTokenSource` synchronously and asserts on the next statement; no timing is involved.

**Toolchain.** The nullable gate (step 3) is the binding constraint for Site B — see F3. The analyzer gate (step 2) is unaffected by the boundary catch because of `.editorconfig:27`.

---

## 7. Numeric Derivation Evidence

### N-1 — Holders of source S1: **9**

- **Complete Family:** every object or scope in the compiled repository that captures a reference to the `CancellationTokenSource` constructed at `QuickFiler/Controllers/QfcHomeController.cs:54`, whether in a field, a property-backed field, or a method-local, via any assignment, constructor parameter, property setter, or method argument.
- **Exhaustive Search Scope:** `QuickFiler/**/*.cs`, `UtilitiesCS/Threading/*.cs`, `TaskMaster/**/*.cs`. Excludes `QuickFiler/Legacy/**` (no `<Compile Include>` entry, verified) and all `*.Test` projects.
- **Inclusion Rules:** the holder must retain the reference beyond the statement that receives it (a field, a property-backed field, or a local whose scope spans further use). All storing types on the path from `:54` are included regardless of whether they call `Cancel()`.
- **Exclusion Rules:** excluded — parameters that are forwarded without being stored (e.g. `DfDeedle.GetEmailDataInViewAsync` at `QfcDatamodel.FrameBuilding.cs:86`); `CancellationToken` struct copies, which are values and not references to the source; child `ProgressTracker` instances from `SpawnChild`, because the child ctor at `ProgressTracker.cs:60-69` copies `_jobName` and `_progressViewer` but **not** `_cancelSource`; the second, unrelated source at `QfcHomeController.cs:467` (`CreateCancellationToken()`, no production caller).
- **Primary Search Strategy:** declaration-side. Regex over the scope for `CancellationTokenSource`-typed private fields: `_tokenSource|_cancelSource|_cancellationSource`, then read each hit's declaring type to confirm it lies on the S1 path.
- **Primary Member Set:** { `LaunchAsync` local `tokenSource` (`QfcHomeController.cs:54`), `ProgressTracker._cancelSource` (`ProgressTracker.cs:80`), `ProgressViewer._cancelSource` (`ProgressViewer.cs:53`), `QfcHomeController._tokenSource` (`:471`), `QfcDatamodel._tokenSource` (`:165`), `QfcFormController._tokenSource` (`:187`), `QfcCollectionController._tokenSource` (`:112`), `QfcItemController._tokenSource` (`QfcItemController.cs:59`), `ConversationResolver._tokenSource` (`:246`) }
- **Primary Count:** 9
- **Cross-check Search Strategy:** flow-side forward trace, a different axis. Start at `QfcHomeController.cs:54` and follow every argument-passing site out of `LaunchAsync`/`InitAsync`/`Run*`/`LoadItems*`/`Initialize`, reading each callee's parameter-storage statements. Query expressions used per receiving type: `tokenSource|TokenSource|CancellationTokenSource` scoped to that type's files. Trace edges walked: `:54` → `:56` `new ProgressTracker(tokenSource)` → `ProgressTracker.cs:22` → `ProgressTracker.cs:40` `CancelSource =` → `ProgressViewer.cs:59`; `:54` → `:60-66` `InitAsync` → `:117`; `:117` → `:122-127` `QfcAsyncDataModelLoader` → `QfcDatamodel.cs:66`; `:117` → `:137-146` `QfcFormControllerLoader` → `QfcFormController.cs:39`; `QfcFormController.Actions.cs:56,90,146` `tokenSource: TokenSource` → `QfcCollectionController.cs:42`; `QfcItemController.Initialization.cs:386` `= _homeController.TokenSource`; `QfcItemController.Initialization.cs:393-399` `new ConversationResolver(…, _tokenSource, …)` → `ConversationResolver.cs:79`.
- **Cross-check Member Set:** { `LaunchAsync` local, `ProgressTracker`, `ProgressViewer`, `QfcHomeController`, `QfcDatamodel`, `QfcFormController`, `QfcCollectionController`, `QfcItemController`, `ConversationResolver` }
- **Cross-check Count:** 9
- **Member-set Comparison:** normalizing both to declaring type (the `LaunchAsync` local normalizes to "`QfcHomeController.LaunchAsync` local scope"), the two sets are **identical, 9 members, no residue on either side**. The primary set is declaration-anchored and would have caught a holder that stores the source but is never reached from `:54`; the cross-check is flow-anchored and would have caught a holder that stores it under a field name outside the three-alternative regex. Neither found such a member.

### N-2 — Sites that call `Cancel()` on source S1: **4**

- **Complete Family:** every syntactic call to `CancellationTokenSource.Cancel()` in compiled production code whose receiver can be the S1 instance, across all invocation forms: `x.Cancel()`, `x?.Cancel()`, `x!.Cancel()`, and calls through a property chain.
- **Exhaustive Search Scope:** `QuickFiler/**/*.cs`, `UtilitiesCS/**/*.cs`, `TaskMaster/**/*.cs`, excluding `QuickFiler/Legacy/**` and all `*.Test` projects.
- **Inclusion Rules:** the receiver must be one of the nine N-1 holders, or a property chain resolving to one.
- **Exclusion Rules:** `Cancel()` on a different source (`QfcQueue.cs:50,101` timeout sources; `EfcHomeController.cs:399`; `BreadcrumbCoordinatorUpgradeLifetime.cs:54`); `Cancel(bool)` overloads (none present); `CancelAsync` (`QfcDatamodel.cs:78` is `_worker?.CancelAsync()` on a `BackgroundWorker`, a different type).
- **Primary Search Strategy or Query Expression:** holder-anchored. For each of the nine N-1 holders, grep that holder's own files for its field identifier and inspect every use — patterns `_tokenSource|_cancelSource` and `TokenSource\?\.Cancel|TokenSource\.Cancel` scoped per file.
- **Primary Member Set:** { `QfcDatamodel.cs:77` `_tokenSource?.Cancel();`, `QfcDatamodel.QueueProcessing.cs:50` `_tokenSource?.Cancel();`, `QfcFormController.EventHandlers.cs:133` `_parent?.TokenSource?.Cancel()`, `ProgressViewer.cs:75` `_cancelSource!.Cancel();` }
- **Primary Count:** 4
- **Cross-check Search Strategy or Query Expression:** operator-anchored and overload-exhaustive, a different axis. Three independent regexes covering all three dereference operators, run repo-wide rather than per holder: (a) `!\.(Cancel|Dispose)\(\)` — 7 hits repo-wide, of which `ProgressViewer.cs:75` and `ProgressPane.cs:57` are `Cancel`, and `ProgressPane.cs:57` is excluded because its receiver is the AF/pane source, not S1; (b) `\?\.Cancel\(\)` scoped to the holder files; (c) `\.Cancel\(\)` without a preceding `?` or `!`, scoped to the holder files — zero additional hits. The `!\.` regex is what makes this exhaustive rather than a single-pattern search: a `?.`-only or `.`-only grep would have missed `ProgressViewer.cs:75` entirely, which is the whole point of the issue.
- **Cross-check Member Set:** { `QfcDatamodel.cs:77`, `QfcDatamodel.QueueProcessing.cs:50`, `QfcFormController.EventHandlers.cs:133`, `ProgressViewer.cs:75` }
- **Cross-check Count:** 4
- **Member-set Comparison:** the two sets are **identical, 4 members**. This is also identical to the four-row table at `docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/research/research.2026-09-07T22-10.md:220-227`, which is the origin of the "fourth" figure — establishing that the #810 figure was correct for *this* family and was simply mislabelled "sharer" when carried into `issue.md:18`.

### N-3 — Additional unguarded cleanup-callback invocation sites beyond Site A, in compiled `QuickFiler`: **1**

- **Complete Family:** every invocation, in compiled `QuickFiler` production code, of a delegate-typed field or property that carries a parent-cleanup, owner-release, or disposal callback — across all invocation forms (`X.Invoke()`, `X?.Invoke()`, `X()`, `X?.Invoke()` on a local) and all delegate types (`System.Action`, `Action`, and repository-declared `delegate` types).
- **Exhaustive Search Scope:** `QuickFiler/**/*.cs` in its entirety — `Controllers/`, `Viewers/`, `Helper Classes/`, `Interfaces/`, and `Legacy/`. `Legacy/` is searched but its hits are then excluded as uncompiled.
- **Inclusion Rules:** the invoked delegate must be stored in a field or property of the invoking type (so a repeat call could re-invoke it) **and** its semantic role must be release/teardown of a parent, owner, or resource. "Unguarded" means the site does **not** clear the stored delegate before invoking it, by any mechanism (plain assignment to null, `Interlocked.Exchange`, or a once-only flag/lock).
- **Exclusion Rules:** excluded — sites in `QuickFiler/Legacy/**` (no `<Compile Include>` entry: `SearchScope:` `QuickFiler/QuickFiler.csproj`; `SearchPatterns:` `Legacy` and `QuickFileController\.cs|QfcLauncher\.cs|QfcController\.cs|QfcFormLegacyViewer\.cs`; `SearchResult:` zero hits for both); delegates invoked for non-teardown purposes (`QfcStreamingDequeueConfidenceGate.cs:246` `_sourceActive?.Invoke()` is a predicate probe); `readonly` delegate fields whose invocation is guarded by a separate once-only mechanism (`BreadcrumbDropDownOpenCoordinator.cs:215`, guarded by `_released` under `lock` at `:361-373`); focus/UI-gesture delegates that are not teardown (`BreadcrumbDropDownHost.cs:297,306`); idempotent `-=` detachers with no repeat-invocation harm (`BreadcrumbWebViewSurfaceFactory.cs:152`).
- **Primary Search Strategy or Query Expression:** invocation-side. Regex `\.Invoke\(\)` over `QuickFiler/**` — 9 hits: `Legacy/QuickFileController.cs:664`, `Legacy/QfcLauncher.cs:62`, `Controllers/EfcHomeController.cs:349`, `Controllers/EfcFormController.cs:309`, `Controllers/QfcFormController.SetupDisposal.cs:271`, `Controllers/QfcHomeController.cs:405`, `Viewers/BreadcrumbMessengerHub.cs:485`, `Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs:365`, `Controllers/QfcStreamingDequeueConfidenceGate.cs:246`. Applying the rules: 2 excluded as uncompiled, 1 excluded as a predicate, 3 excluded as already applying the idiom (`EfcFormController.cs:305-310`, `QfcFormController.SetupDisposal.cs:269-271`, `BreadcrumbMessengerHub.cs:483-485`), 1 excluded as applying the `Interlocked` variant (`BreadcrumbItemViewerLifecycleCoordinator.cs:364-365`), 1 is Site A itself.
- **Primary Member Set:** { `QuickFiler/Controllers/EfcHomeController.cs:349` } — i.e. Site A plus exactly one other.
- **Primary Count:** 1 (excluding Site A)
- **Cross-check Search Strategy or Query Expression:** declaration-side, a different axis, and deliberately overload-exhaustive across delegate *types* rather than invocation *syntax* — this catches a site invoked as `X()` rather than `X.Invoke()`, which the primary regex cannot see. Two regexes: (a) delegate-typed field/property declarations, `(private|internal|public|protected)\s+(readonly\s+)?(System\.)?Action\??\s+_?\w+\s*[;={]` over `QuickFiler/**` — 13 hits; (b) repository-declared delegate types, `delegate\s+\w+\s+\w+\(` over `QuickFiler/**` — 11 hits, of which the two teardown-role ones (`Legacy/QuickFileController.cs:86 ParentCleanupMethod`, `Legacy/QfcLauncher.cs:13 ParentCleanupFunction`) are both uncompiled. Each of the 13 Action declarations was then individually traced to its invocation site(s) by grepping its exact identifier (`_cancelSelector|_detachPopupMessenger|_focusPending|_focusAnchor|_cancelSelection|_detachHandlers|_toggleControl` as one query, plus `_parentCleanup|ParentCleanup`, `_dispose`, `_detach`). `KaStringAsync.cs:154 _toggleControl` has a getter/setter but **no invocation site** and no teardown role.
- **Cross-check Member Set:** { `QuickFiler/Controllers/EfcHomeController.cs:349` (`_parentCleanup` declared `:285`, invoked unguarded) } — with `QuickFiler/Controllers/QfcHomeController.cs:154`→`:405` identified as Site A.
- **Cross-check Count:** 1 (excluding Site A)
- **Member-set Comparison:** the two sets are **identical, 1 member: `EfcHomeController.cs:349`**. The declaration-side cross-check additionally confirmed that no delegate field in `QuickFiler` is invoked with bare-call syntax `X()` in a teardown role that the `\.Invoke\(\)` regex would have missed — the only bare-call invocations found (`BreadcrumbDropDownOpenCoordinator.cs:203,204,215,279,298,357`; `BreadcrumbDropDownHost.cs:297,306`; `BreadcrumbDropDownHost.Open.cs:149`; `BreadcrumbWebViewSurfaceFactory.cs:152`) are all on `readonly` fields that cannot be cleared and were individually classified under the exclusion rules above. This is the specific gap that a single-pattern grep would have left open, and it is closed.

---

## 8. Open questions the plan must resolve

1. **Does `UtilitiesCS` grant `InternalsVisibleTo("UtilitiesCS.Test")`?** The `internal RequestCancel` recommendation in §4.3 depends on it. `UtilitiesCS.Test/Properties/AssemblyInfo.cs` is a sibling-owned, off-limits file, so if the attribute is absent the plan must fall back to `private RequestCancel` plus reflection (the pattern already used at `ProgressViewer_Tests.cs:144-155`) rather than adding the attribute. This is a one-line read the plan must perform; I did not resolve it because doing so cleanly means reading a file whose ownership is contested and I did not want the finding to read as an edit proposal against it.
2. **Site A test placement: move `:115` below `:120`, or add a dedicated test and leave `:115` alone?** §4.4 recommends the latter, but the issue's Manual Verification note (`issue.md:135-137`) explicitly asks to *move* it, and the two produce different evidence. The plan must choose and record the rationale, because "the test must fail before the fix" is satisfied by either and the choice affects whether the AC3 datamodel assertion at `:121-125` stays isolated.
3. **Should `SetCancellationTokenSource` throw `ArgumentNullException` instead of merely declining to enable?** §4.3 recommends `Enabled = tokenSource is not null;` as the minimal change. A throw would be more fail-fast and is arguably more consistent with CLAUDE.md §C#4.3, but the member has no production caller, so a throw changes only test-visible behavior. The plan should pick one; this research does not have evidence favouring either strongly.
4. **Scope decision on O-2 (`ProgressPane.cs:57`).** It is the *live* production progress surface — more reachable than Site B — and carries a character-for-character identical defect. Fixing only `ProgressViewer` reproduces exactly the enumeration-gap failure mode this issue exists to close. The plan must either widen the write set to include `UtilitiesCS/Threading/ProgressPane.cs` (with the orchestrator's agreement, since it is not in the supplied owned-files list) or file the follow-up issue **before** this fix merges, so the gap is recorded rather than re-created.
5. **Scope decision on O-1 (`EfcHomeController.cs:349`).** Same reasoning as O-2, for Site A's defect class. It is the single additional unguarded site N-3 found, and leaving it unrecorded would repeat the 810 pattern.
