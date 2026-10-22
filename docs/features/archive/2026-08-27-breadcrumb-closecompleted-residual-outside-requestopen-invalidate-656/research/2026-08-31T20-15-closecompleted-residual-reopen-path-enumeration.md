# Issue #656 — `_closeCompleted` residual: reopen-path enumeration and remedy analysis

- **Timestamp:** 2026-08-31T20-15
- **Issue:** #656 (`docs/features/active/2026-08-27-breadcrumb-closecompleted-residual-outside-requestopen-invalidate-656/`)
- **Scope:** research only. No source, configuration, or project file was modified.
- **Tree state:** worktree at branch `docs/parallel-session-notes-2026-08-29`, clean at session start; every line number below was re-derived by reading the files in this worktree during this session.

---

## Executive answer

**A reopen path that reaches neither `RequestOpen` nor `Invalidate` DOES NOT exist in the shipped
production code today.** The single statement in the repository that makes the drop-down host open is
`QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs:268` (`_host.OpenState = true;`), and the call
chain leading to it is closed: it is reachable only from `BreadcrumbDropDownHost.Open.cs:88`, which is
reachable only from the two `OpenAsync` overloads at `BreadcrumbDropDownHost.Open.cs:22` and `:37`,
whose only production invocations are `BreadcrumbDropDownOpenCoordinator.cs:258` and `:259` inside
`BeginOpenCore`, which is called only from `BreadcrumbDropDownOpenCoordinator.cs:218` inside
`OpenCoreAsync`, which is constructed only at `BreadcrumbDropDownOpenCoordinator.cs:115` — the
statement immediately after `_closeCompleted = false;` at `:114` in `RequestOpen`.

Issue #656 is therefore **latent-correctness hardening, not an observed user-facing failure**, exactly
as the issue's own severity note states.

---

## 1. Verified Source Facts

All line numbers re-derived from `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs`
(378 lines total) in this worktree.

| Fact | File:line | Exact source line |
| --- | --- | --- |
| Field declaration | `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs:46` | `        private bool _closeCompleted;` |
| Set site (successful-close path in `CloseCore`) | `…OpenCoordinator.cs:335` | `                    _closeCompleted = true;` |
| Early-return suppression guard in `CloseCore` | `…OpenCoordinator.cs:316-317` | `                if (_closeCompleted)` / `                    return true;` |
| Clear site 1 (`RequestOpen`) | `…OpenCoordinator.cs:114` | `                _closeCompleted = false;` |
| Clear site 2 (`Invalidate`) | `…OpenCoordinator.cs:352` | `                _closeCompleted = false;` |

Enclosing context, verified:

- `CloseCore` spans `:308-342`. Its guard block is `:310-319`:
  `if (_released) return false;` (`:312-313`) → `if (_closeInFlight) return true;` (`:314-315`) →
  `if (_closeCompleted) return true;` (`:316-317`) → `_closeInFlight = true;` (`:318`).
- The set site sits inside `if (closed) { lock (_sync) { _generation++; _closeCompleted = true; } return true; }`
  at `:330-338`. `_generation++` is `:334`; `_closeCompleted = true` is `:335`.
- `_host.Close(reason)` is invoked at `:323`, **outside** `lock (_sync)`, with `_closeInFlight` cleared
  in a `finally` at `:325-329`.
- `RequestOpen` spans `:104-118`. `_closeCompleted = false;` at `:114` is immediately followed by
  `_currentOpenTask = OpenCoreAsync(_generation);` at `:115`.
- `Invalidate(bool release)` spans `:344-356`; `_closeCompleted = false;` at `:352` sits between
  `_currentOpenTask = null;` (`:351`) and `_released = release;` (`:353`).
- `Invalidate` has exactly two callers, both in the same file: `Reset()` at `:188`
  (`Invalidate(release: false)`) and `Release()` at `:204` (`Invalidate(release: true)`).

The field is confined to one file. A repository-wide `*.cs` search for `_closeCompleted` returns six
hits, all in `BreadcrumbDropDownOpenCoordinator.cs`: `:41` (XML doc cross-reference), `:46`
(declaration), `:114`, `:316`, `:335`, `:352`. No test file references it by name and no reflective
write reaches it (see §7).

---

## 2. Reopen Path Enumeration

### 2.1 Method

The enumeration is anchored on the host's own open-state variable rather than on the name `OpenAsync`,
because that variable is what `IBreadcrumbDropDownHost.IsOpen` reports and what "the drop-down host is
open" means to the coordinator. `BreadcrumbDropDownHost.IsOpen` is a get-only expression-bodied
property, `public bool IsOpen => OpenState;` (`QuickFiler/Viewers/BreadcrumbDropDownHost.cs:191`), over
`internal bool OpenState { get; set; }` (`…Host.cs:244`). Every way for the host to become open is
therefore a write of `true` to `OpenState`, and the enumeration walks the call graph upward from there.

Three additional surfaces that could conceivably make the popup visible without that write were checked
and excluded, so the enumeration covers the whole family rather than one named method:

- **`ShowPopup`** (`QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs:98-102`), which performs the
  actual `_showPopup(DropDown, Anchor, location)` native show at `:101`. Its only invocation is
  `BreadcrumbDropDownOpenLifetime.cs:276`, inside `ShowCurrentSurface`, *after* `_host.OpenState = true`
  at `:268`. It is not an independent entry point.
- **The `_showPopup` delegate itself** (declared `…Host.cs:30`, assigned `…Host.cs:162`, defaulted to
  `BreadcrumbPopupUiOperations.ShowOwnedPopup` at `…Host.cs:74`, whose definition is
  `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs:101`). Repository-wide, `_showPopup` is invoked
  exactly once, at `…Host.Open.cs:101`.
- **Native `ToolStripDropDown` events.** The host subscribes exactly one drop-down event,
  `DropDown.Closed += OnDropDownClosed` (`…Host.cs:171`). There is no `Opened`, `VisibleChanged`, or
  equivalent handler that could observe or cause a native reopen.

### 2.2 Every write to `OpenState` (production)

| # | Site | Value written | Bearing on reopen |
| --- | --- | --- | --- |
| 1 | `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs:268` | `true` | **The only open transition in the repository.** |
| 2 | `QuickFiler/Viewers/BreadcrumbDropDownHost.cs:334` (`DisposeCoreAsync`) | `false` | Close only |
| 3 | `QuickFiler/Viewers/BreadcrumbDropDownHost.cs:402` (`CompleteClose`) | `false` | Close only |
| 4 | `QuickFiler/Viewers/BreadcrumbDropDownHost.cs:434` (`OnDropDownClosed`) | `false` | Close only |
| 5 | `QuickFiler/Viewers/BreadcrumbDropDownHost.cs:460` (`RestoreAfterOpenFailure`) | `false` | Close only |

`BreadcrumbDropDownOpenLifetime.Focus.cs:37` and `:41`, and `BreadcrumbDropDownOpenLifetime.cs:277`,
`BreadcrumbDropDownHost.cs:251`, `:256`, `:310`, `:332`, `:399`, `:401`, `:428`, `:432`, `:459`, and
`BreadcrumbDropDownHost.Open.cs:61` are **reads** of `OpenState`, not writes.

### 2.3 The enumeration table

Column meanings: a call site "reaches `RequestOpen`" if control passes through
`BreadcrumbDropDownOpenCoordinator.RequestOpen` (`:104`) before or as part of the reopen; "reaches
`Invalidate`" likewise for `BreadcrumbDropDownOpenCoordinator.Invalidate` (`:344`).

| # | Call site (file : line) | Reaches `RequestOpen`? | Reaches `Invalidate`? | Bypasses both? | Evidence |
| --- | --- | --- | --- | --- | --- |
| 1 | `BreadcrumbDropDownOpenLifetime.cs:268` — `_host.OpenState = true` | Yes (transitively) | n/a | **No** | Inside `ShowCurrentSurface` (`:258-279`), invoked at `:243` from `OpenCoreAsync`, whose only caller is the kickoff lambda at `:67-69` inside `BreadcrumbDropDownOpenLifetime.OpenAsync` (`:44-72`) |
| 2 | `BreadcrumbDropDownOpenLifetime.OpenAsync` (`:44`) | Yes (transitively) | n/a | **No** | Single invocation repository-wide: `BreadcrumbDropDownHost.Open.cs:88` |
| 3 | `BreadcrumbDropDownHost.OpenWithFocusIntentAsync` (`Open.cs:53-89`) | Yes (transitively) | n/a | **No** | Two invocations, both in the same file: `:22` and `:37` |
| 4 | `BreadcrumbDropDownHost.OpenAsync` 3-param, public (`Open.cs:18-22`) | Yes | n/a | **No** | Production invocation is `BreadcrumbDropDownOpenCoordinator.cs:258` |
| 5 | `IBreadcrumbDropDownHost.OpenAsync` 4-param, explicit impl (`Open.cs:32-37`) | Yes | n/a | **No** | Production invocation is `BreadcrumbDropDownOpenCoordinator.cs:259` |
| 6 | `BreadcrumbDropDownOpenCoordinator.BeginOpenCore` (`:232-264`), calls `_host.OpenAsync` at `:258` / `:259` | Yes | n/a | **No** | Single invocation: `:218`, inside `OpenCoreAsync` |
| 7 | `BreadcrumbDropDownOpenCoordinator.OpenCoreAsync` (`:213-230`) | Yes | n/a | **No** | Single invocation: `:115`, the statement after `_closeCompleted = false;` at `:114` |
| 8 | `BreadcrumbDropDownOpenCoordinator.RequestOpen` (`:104-118`) | **Is** `RequestOpen` | No | **No** | Clears the flag itself at `:114` |
| 9 | `BreadcrumbDropDownOpenCoordinator.SetDroppedDown(true)` (`:152-169`) | Yes | No | **No** | Posted body `:160-165`: `_openSelector()` at `:162`; if it reports no change and the selector is open, `RequestOpen()` at `:164`; if it reports a change, the selector raises `SelectorOpenStateChanged`, routed at `BreadcrumbItemViewerLifecycleCoordinator.cs:237-238` into `HandleSelectorOpenStateChanged` → `RequestOpen()` at `:180` |
| 10 | `BreadcrumbDropDownOpenCoordinator.HandleSelectorOpenStateChanged` (`:171-184`) | Yes | No | **No** | `_ = RequestOpen();` at `:180` |
| 11 | `BreadcrumbDropDownOpenCoordinator.LatchNextOpenTakesNoFocus` (`:132-140`) | No — but performs no open | No | **No** | Sets `_nextOpenTakesNoFocus` only (`:138`); the open still arrives via `SelectorOpenStateChanged` → `RequestOpen`, as its own remarks at `:123-131` document |
| 12 | `BreadcrumbItemViewerLifecycleCoordinator.PresentSearchResults` (`…Search.cs:34-43`) | Yes (transitively) | No | **No** | `_openCoordinator?.LatchNextOpenTakesNoFocus();` at `:40` then `_bridgeCoordinator?.PresentSearchResults(items)` at `:42`; the open reaches the coordinator through `SelectorOpenStateChanged` |
| 13 | `BreadcrumbItemViewerLifecycleCoordinator.SetDroppedDown` (`:192-205`) | Yes (transitively) | No | **No** | `_openCoordinator.SetDroppedDown(droppedDown);` at `:204`; the `_openCoordinator == null` branch (`:195-202`) only calls `Focus(focus)` and opens nothing |
| 14 | `BreadcrumbItemViewerLifecycleCoordinator.ConfigureHost` (`:112-168`) — **different host** branch | n/a — no open occurs | Yes | **No** | `ReleaseHostCore()` at `:133` → `coordinator.Release()` at `:318` → `Invalidate(release: true)`; a **new** coordinator is then constructed at `:134` with `_closeCompleted` at its `false` default |
| 15 | `BreadcrumbItemViewerLifecycleCoordinator.ConfigureHost` — **same host** branch | n/a — no open occurs | No | **No** | `_openCoordinator.UpdateRequestProviders(anchorBounds, workingArea);` at `:160`. `UpdateRequestProviders` (`…OpenCoordinator.cs:89-102`) only reassigns `_anchorBounds` and `_workingArea`; it issues no open, so a stale flag here cannot suppress a close of an open that never happened |
| 16 | `BreadcrumbItemViewerLifecycleCoordinator.Reset` (`:207-215`) | No | Yes | **No** | `_openCoordinator?.Reset();` at `:212` → `Invalidate(release: false)` at `…OpenCoordinator.cs:188` → clear at `:352` |
| 17 | `BreadcrumbItemViewerLifecycleCoordinator.Dispose` (`:217-235`) | No | Yes | **No** | `ReleaseHostCore()` at `:227` → `coordinator.Release()` at `:318` |
| 18 | `ItemViewer.SetBreadcrumbDropDownState` (`ItemViewer.Breadcrumb.cs:288-300`) | Yes (transitively) | No | **No** | `_breadcrumbLifecycleCoordinator.SetDroppedDown(droppedDown, FocusBreadcrumbCore);` at `:299`; the null branch (`:290-297`) only focuses |
| 19 | `ItemViewer.PresentBreadcrumbSearchResults` (`ItemViewer.Breadcrumb.cs:313-321`) | Yes (transitively) | No | **No** | `_breadcrumbLifecycleCoordinator.PresentSearchResults(items);` at `:320` |
| 20 | `ItemViewer.ResetBreadcrumb` (`ItemViewer.Breadcrumb.cs:323`) | No | Yes | **No** | `_breadcrumbLifecycleCoordinator?.Reset()` |
| 21 | `ItemViewer.ConfigureBreadcrumbDropDown(CoreWebView2Environment, IWebViewCoreInitializer)` (`ItemViewer.Breadcrumb.cs:167-221`) | n/a — no open occurs | Yes, on the replace path | **No** | Constructs the host at `:198-207`, then delegates to the 3-arg overload at `:213`; the outgoing concrete host is disposed at `:191` |
| 22 | `ItemViewer.ConfigureBreadcrumbDropDown(IBreadcrumbDropDownHost, Func<Rectangle>, Func<Rectangle>)` (`ItemViewer.Breadcrumb.cs:223-241`) | n/a — no open occurs | Depends on branch 14/15 | **No** | `lifecycle.ConfigureHost(host, anchorBounds, workingArea);` at `:240` |
| 23 | `ItemViewer.FolderSearch.cs:32` (`SetBreadcrumbDropDownState(droppedDown)`) and `:39` (`PresentBreadcrumbSearchResults(items)`) | Yes (transitively) | No | **No** | Rows 18 and 19 |
| 24 | `QfcItemController.ViewerSetup.cs:171` / `:184` (`viewer.ConfigureBreadcrumbDropDown(...)`) and `:451` (`ResetBreadcrumb()`) | n/a — no open occurs | Rows 21/22, row 20 | **No** | The controller never holds or drives an `IBreadcrumbDropDownHost` directly; a repository-wide `*.cs` search for `IBreadcrumbDropDownHost` returns six production files, all under `QuickFiler/Viewers/` plus `ItemViewer.Breadcrumb.cs` |

### 2.4 Sub-cases inside `RequestOpen` that do **not** clear the flag — checked and excluded

`RequestOpen` has two early returns ahead of the clear at `:114`:

- `:110-111` — `if (_currentOpenTask != null && !_currentOpenTask.IsCompleted) return _currentOpenTask;`
- `:112-113` — `if (_closeInFlight && _host.IsOpen) return ClosedTask;`

Neither clears `_closeCompleted`, but neither starts an open either. The `:110` branch returns the
already-running task, whose generation was invalidated by the successful close at `:334`, so
`BeginOpenCore`'s currency check at `:239-240` returns `ClosedTask` and no `_host.OpenAsync` call is
made. These are not bypassing reopen paths.

### 2.5 Plain answer

**A bypassing reopen path — one that makes the drop-down host open while reaching neither
`BreadcrumbDropDownOpenCoordinator.RequestOpen` (`QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs:104`)
nor `BreadcrumbDropDownOpenCoordinator.Invalidate` (`…:344`) — does not exist in the shipped production
code, because the repository's only open transition,
`QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs:268`, is reachable only through the closed chain
`RequestOpen (…OpenCoordinator.cs:115) → OpenCoreAsync (:218) → BeginOpenCore (:258/:259) →
BreadcrumbDropDownHost.Open.cs:22/:37 → :88 → BreadcrumbDropDownOpenLifetime.cs:67-69 → :243 → :268`,
whose first link clears `_closeCompleted` at `…OpenCoordinator.cs:114`.**

---

## 3. Numeric Derivation Evidence

Two numeric claims in this document are load-bearing for the recommendation and are derived below.

### Claim N1 — `_closeCompleted` has exactly **three** assignment sites: one set and two clears

- **Complete family:** every assignment expression whose target is the instance field
  `BreadcrumbDropDownOpenCoordinator._closeCompleted`, including reflective writes.
- **Exhaustive search scope:** all `*.cs` files in the repository (production and test), plus the
  reflective-write surface (`GetField(`) in `QuickFiler.Test`. The field is `private` on a `sealed`
  `internal` class, so the only non-source-visible write channel is reflection, which the scope covers.
- **Inclusion rules:** direct assignments (`= true`, `= false`), compound assignments, `ref`/`out`
  usage, and `FieldInfo.SetValue` calls naming the field.
- **Exclusion rules:** reads (`if (_closeCompleted)`), and XML documentation cross-references
  (`<see cref="_closeCompleted"/>`).
- **Primary search strategy / query:** identifier search `_closeCompleted` across `*.cs`, then manual
  classification of each of the six hits.
- **Primary member set:** `{ …OpenCoordinator.cs:114 (= false), …OpenCoordinator.cs:335 (= true),
  …OpenCoordinator.cs:352 (= false) }`. Excluded as non-assignments: `:41` (doc cref), `:46`
  (declaration, no initializer), `:316` (read).
- **Primary count:** 3 assignments (1 set, 2 clears).
- **Cross-check search strategy / query:** a different, structural route — full sequential read of
  `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` (all 378 lines) enumerating every mutation
  of the type's state, combined with a `GetField\(` search across `QuickFiler.Test` to enumerate every
  reflective field write in the test assembly and check whether any names this field.
- **Cross-check member set:** the sequential read yields the same three mutation statements — inside
  `RequestOpen` (`:114`), inside the `if (closed)` block of `CloseCore` (`:335`), and inside
  `Invalidate` (`:352`). The `GetField(` search returns hits in `BreadcrumbBridgeCoordinatorSupersessionTests.cs`,
  `BreadcrumbCoordinatorLifecycleTests.cs`, `BreadcrumbCollapsedSurfaceReadinessTests.cs`,
  `BreadcrumbDropDownHostTests.cs`, `BreadcrumbCoordinatorUpgradeLifetimeTests.cs`
  (`_sync`, `_current`, `_generation`), `BreadcrumbDropDownLifecycleCoverageTests.cs` (`_openLifetime`),
  and several non-breadcrumb test files; **none** names `_closeCompleted`, confirmed by the fact that
  the repository-wide `_closeCompleted` identifier search returns zero hits outside
  `BreadcrumbDropDownOpenCoordinator.cs`.
- **Cross-check count:** 3.
- **Member-set comparison:** normalized primary set `{114, 335, 352}` equals normalized cross-check set
  `{114, 335, 352}`. Reflective-write set is empty in both records. **Agreement.**

### Claim N2 — exactly **one** production statement makes the drop-down host open

- **Complete family:** every production statement that can cause `IBreadcrumbDropDownHost.IsOpen` to
  transition from `false` to `true` for the concrete `BreadcrumbDropDownHost`. Because
  `IsOpen => OpenState` (`BreadcrumbDropDownHost.cs:191`) is get-only over
  `internal bool OpenState { get; set; }` (`:244`), the family is exactly the set of writes of `true` to
  `OpenState`, plus any alternative mechanism that could show the popup without that write.
- **Exhaustive search scope:** all `*.cs` in the repository, covering both the property-write channel
  and the native-show channel (`ShowPopup`, the `_showPopup` delegate, `ShowOwnedPopup`,
  `DropDown.Show`, `DropDown.Visible`, and drop-down event subscriptions). Every member of the
  `OpenAsync` overload pair (3-param and 4-param, including the explicit interface implementation) is
  in scope; the search is not restricted to a single named method.
- **Inclusion rules:** assignments to `OpenState`; native show invocations that would make the popup
  visible.
- **Exclusion rules:** reads of `OpenState`/`IsOpen`; assignments of `false`; the unrelated
  `BreadcrumbSelectionEffects.OpenStateChanged` / `SelectorOpenStateChanged` identifier family in
  `UtilitiesCS` and `BreadcrumbBridgeCoordinator`, which concerns the selector session model and never
  touches the host property; test-assembly writes.
- **Primary search strategy / query:** identifier search `OpenState` across `*.cs`, then classify each
  hit as write-true / write-false / read / unrelated-identifier.
- **Primary member set (writes of `true`, production):** `{ BreadcrumbDropDownOpenLifetime.cs:268 }`.
  Production writes of `false`: `{ BreadcrumbDropDownHost.cs:334, :402, :434, :460 }`. Test writes:
  `{ BreadcrumbSelectorToggleUiBoundaryTests.cs:225 (compound &= on a test host),
  BreadcrumbPopupBoundaryCoverageTests.Part2.cs:343 (= false) }` — neither writes `true`.
- **Primary count:** 1.
- **Cross-check search strategy / query:** a structurally different route — regex
  `ShowOwnedPopup|_showPopup|DropDown\.(Show|Visible)` across `QuickFiler/`, plus a full read of
  `BreadcrumbDropDownHost.cs` (498 lines), `BreadcrumbDropDownHost.Open.cs` (107 lines) and
  `BreadcrumbDropDownOpenLifetime.cs` (460 lines) to enumerate every native-show and every drop-down
  event subscription.
- **Cross-check member set:** the native-show family is
  `{ BreadcrumbDropDownHost.Open.cs:101 (_showPopup invocation, inside ShowPopup at :98-102) }`, whose
  only caller is `BreadcrumbDropDownOpenLifetime.cs:276` inside `ShowCurrentSurface`, which is preceded
  in the same expression chain by `_host.OpenState = true` at `:268`. The delegate-assignment sites are
  `…Host.cs:74` and `:162`; the only definition is `BreadcrumbPopupUiOperations.cs:101`; and
  `DropDown.Visible` appears once, as a read at `…Host.cs:459`. The only drop-down event subscription is
  `DropDown.Closed += OnDropDownClosed` (`…Host.cs:171`). The cross-check therefore also yields exactly
  one open transition, located at `BreadcrumbDropDownOpenLifetime.cs:268`, with no independent
  native-show entry point.
- **Cross-check count:** 1.
- **Member-set comparison:** normalized primary set `{ BreadcrumbDropDownOpenLifetime.cs:268 }` equals
  the normalized cross-check set. **Agreement.**

---

## 4. Existing Test Contract

All three must-pass tests live in the `QuickFiler.Test.Viewers.BreadcrumbDropDownOpenCoordinatorTests`
partial class and share the private nested `CoordinatorHarness` / `ControlledHost` fixtures declared at
`QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs:323-372` and `:374` onward.

### 4.1 `PendingToggleClose_HostOwnershipSuppressesFallbackAndRepeatedClose`

- **Location:** `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs:262-280`
  (attribute `:262`, method `:263`).
- **Shape:** an open is started against a host whose open task is a pending
  `TaskCompletionSource<bool>` (`:265-269`); `SetDroppedDown(false)` is then driven **twice** (`:271`,
  `:272`) before the queue is drained.
- **Assertion encoding the contract:**
  `harness.Host.CloseReasons.Should().Equal(BreadcrumbDropDownCloseReason.Uncommitted);` at `:278` —
  a single-element sequence equality, so a second `_host.Close` call is a failure.
  The companion assertion at `:279`,
  `harness.CancelCount.Should().Be(0, "the accepting host owns pending rollback");`, would also fail:
  a second `CloseCore` reaching a host whose `CloseResult` is `true`
  (`ControlledHost.Close`, `…Tests.cs:431-439`, sets `IsOpen = false` and returns `true` on the first
  call) would take the `if (closed)` branch again rather than the `_closeCompleted` early return.
- **Why a naive remedy breaks it:** under "clear `_closeCompleted` in `CloseCore` on success" the
  second `SetDroppedDown(false)` at `:272` finds `_closeInFlight == false` (cleared in the `finally` at
  `…OpenCoordinator.cs:325-329`) and `_closeCompleted == false`, so it passes the guard block at
  `:310-319` and calls `_host.Close` a second time at `:323`. `CloseReasons` becomes
  `{ Uncommitted, Uncommitted }` and the `:278` sequence equality fails.

### 4.2 `SelectorStateTransitions_RequestOpenThenCloseOnlyWhenRequired`

- **Location:** `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs:120-140`
  (attribute `:120`, method `:121`).
- **Shape:** one successful open (`:124-130`), then `SelectorOpen = false` (`:132`) followed by **two**
  `HandleSelectorOpenStateChanged()` drives (`:133`, `:135`), each fully drained.
- **Assertions encoding the contract:** `harness.Host.Requests.Should().ContainSingle();` at `:138`
  and `harness.Host.CloseReasons.Should().Equal(BreadcrumbDropDownCloseReason.ExplicitCommit);`
  at `:139`. The second is the repeated-close guard: exactly one `ExplicitCommit` close.
- **Why a naive remedy breaks it:** the first drive at `:133` reaches
  `CloseCore(BreadcrumbDropDownCloseReason.ExplicitCommit)` (`…OpenCoordinator.cs:182`), the host
  accepts, and `_closeCompleted` would be cleared on success. The second drive at `:135` then passes the
  guard block and calls `_host.Close` again, producing
  `{ ExplicitCommit, ExplicitCommit }` and failing the `:139` sequence equality.

### 4.3 `RequestOpen_AfterSuccessfulCloseAndHostReopen_ReachesHostOpenAsync`

- **Location:** `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs:325-364`
  (doc comment `:325-331`, attribute `:332`, method `:333`).
- **Shape:** open (`:336-341`), successful close via `SetDroppedDown(false)` (`:343-346`), then the
  host is made open again through the test-only seam `harness.Host.SetOpen(true);` at `:349` — the
  comment at `:348` states the intent verbatim: "The host becomes open again by a path that bypasses
  CloseCore and RequestOpen." A second `RequestOpen()` follows at `:354`.
- **Assertions:** `harness.Host.Requests.Should().HaveCount(2, ...)` at `:358-360` and
  `reopen.Result.Should().BeTrue(...)` at `:361-363`.
- **Why a naive remedy does not break it, and why it still matters:** this test exercises the
  `RequestOpen` side, which the naive remedy leaves intact, so it would still pass. It is listed as
  must-pass because any remedy that touches the guard ordering in `RequestOpen` (`:108-114`) or the
  generation bookkeeping at `:334` risks regressing it. It is also the single existing precedent for
  driving a synthetic host reopen through `ControlledHost.SetOpen(true)` — the seam a #656 regression
  test would reuse (see §7).

### 4.4 One further standing guard, not in the delegation list but load-bearing

`CloseCore_RepeatedCloseWithoutReopen_ClosesHostExactlyOnce`
(`…Part2.cs:374-397`; doc comment `:366-373`) is explicitly documented at `:369-371` as "the standing
guard that rules out research section 6.1 option A (clearing the flag on the successful-close path)".
Its assertion at `:391-396` is a single-element `CloseReasons` sequence equality with the reason string
"the repeated close must be suppressed, so _host.Close is reached exactly once". Any remedy must keep
this green as well; it fails under the naive remedy for the same mechanism as §4.1.

---

## 5. SR-4 Rationale and the Rejected Refinement

### 5.1 The ratified decision, quoted verbatim

From `docs/features/active/breadcrumb-coordinator-hub-defects-501/spec.md:426-437`:

> - **SR-4 — DECIDED: minimal two-flag form (research §6.1 option D), without the `&& !_host.IsOpen`
>   refinement.**
>   *Rationale:* the refinement `if (_closeCompleted && !_host.IsOpen) return true;` would read
>   `_host.IsOpen` under `_sync` — the very lock-ordering hazard that #462's potential document flags
>   and that #500 exists to remove. Adding it here would create a new instance of the class of defect
>   this feature is closing.
>   **KNOWN LIMITATION (accepted, recorded, not fixed here):** if the host is reopened by a path that
>   reaches neither `RequestOpen` nor `Invalidate`, `_closeCompleted` stays `true` and a subsequent
>   close request returns `true` without closing. This residual is **strictly narrower** than HEAD's
>   behaviour, in which the single `_closePending` flag latches after *every* successful close and
>   suppresses reopen unconditionally. Closing the residual at source belongs to the host paths owned by
>   sibling feature 488 (see Cross-feature note 4).

And from the same spec's `## Implementation Notes`, `:1062-1068`:

> ### SR-4 known limitation, shipped as designed
>
> `_closeCompleted` stays `true` when the host is reopened by a path that reaches neither `RequestOpen`
> nor `Invalidate`. The two-flag form was chosen because the naive alternative (clearing the close flag on
> the successful-close path) makes `PendingToggleClose_HostOwnershipSuppressesFallbackAndRepeatedClose`
> and `SelectorStateTransitions_RequestOpenThenCloseOnlyWhenRequired` fail. The residual is filed as a
> follow-up against feature 488's host paths rather than worked around here.

The research that SR-4 adopted, `docs/features/active/breadcrumb-coordinator-hub-defects-501/research/2026-08-24T09-12-breadcrumb-ordering-invariants-research.md:733-737`:

> *Residual, worth recording in `spec.md` but not fixing here:* if the host is reopened by a path that
> never reaches `RequestOpen`, `_closeCompleted` stays `true` and a subsequent close request would
> return `true` without closing. A refinement `if (_closeCompleted && !_host.IsOpen) return true;` also
> passes every existing test and removes the residual, at the cost of reading `_host.IsOpen` under
> `_sync`. The minimal form is recommended; the refinement is the fallback if review prefers it.

### 5.2 What `_sync` guards

`private readonly object _sync = new object();` — `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs:16`.
It guards the coordinator's mutable state only: `_anchorBounds` (`:24`), `_workingArea` (`:25`),
`_currentOpenTask` (`:26`), `_generation` (`:27`), `_closeInFlight` (`:36`), `_closeCompleted` (`:46`),
`_released` (`:48`), `_nextOpenTakesNoFocus` (`:49`). Every acquisition is a short, allocation-free
critical section: `:84-85`, `:96-101`, `:106-117`, `:134-139`, `:147-148`, `:237-247`, `:310-319`,
`:327-328`, `:332-336`, `:346-355`, `:360-361`, `:368-369`.

The file's structural discipline is that **host calls happen outside the lock**. `CloseCore` is the
clearest instance: the guard block ends at `:319`, the lock is released, and only then is
`_host.Close(reason)` invoked at `:323`; the flag is restored in a `finally` at `:325-329`. The same
discipline holds in `BeginOpenCore`, which reads the providers under the lock at `:237-247` and then
calls `anchorBounds()`, `_rowCount()`, and `_host.OpenAsync` at `:249-259` with the lock released.

### 5.3 The concrete lock and reentrancy hazard

Reading `_host.IsOpen` under `_sync` would call into the host while the coordinator lock is held. What
that means concretely:

1. **The interface permits arbitrary work.** `IBreadcrumbDropDownHost.IsOpen`
   (`QuickFiler/Viewers/IBreadcrumbDropDownHost.cs:22`) is a plain `bool` getter with no documented
   purity or non-blocking contract. The coordinator is written host-neutrally against the interface —
   it is constructed with an injected `IBreadcrumbDropDownHost` (`…OpenCoordinator.cs:53`, `:64`) — so
   the analysis cannot be confined to the concrete type.
2. **The concrete host's getter is, today, safe.** `BreadcrumbDropDownHost.IsOpen => OpenState`
   (`…Host.cs:191`) over the auto-property at `:244`. It takes no lock, allocates nothing, raises no
   event, and cannot reenter the coordinator. There is no deadlock against this specific
   implementation.
3. **A second lock exists on the path that would be entered.**
   `BreadcrumbDropDownOpenLifetime` has its own `private readonly object _sync`
   (`QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs:25`), acquired in `OpenAsync` (`:54`),
   `TryCancelPendingOpen` (`:79`), `Schedule` (`:117`), `Dispose` (`:144`), the two completion
   `finally` blocks (`:173`, `:201`), `IsLifecycleCurrent` (`:390`), and `ScheduleInvalidating`
   (`:403`). `BreadcrumbDropDownHost.Close` enters that lock via `InvalidateAndSchedule` (`…Host.cs:253`
   → `…OpenLifetime.cs:135-136` → `ScheduleInvalidating` at `:399-420`). Today the coordinator holds
   `_sync` and the lifetime lock **disjointly**, never nested, because `_host.Close` is called at
   `…OpenCoordinator.cs:323` outside the lock. Adding a host read inside the lock establishes the
   ordering `coordinator._sync → host code`, which is the first half of a nesting that the current
   design categorically avoids.
4. **Reentrancy into the coordinator is a live shape elsewhere in the pipeline.** The host raises
   `PopupMessengerReady` (`…Host.cs:219`, published at `…Host.Open.cs:104-105`), and the lifecycle
   coordinator subscribes to it at `BreadcrumbItemViewerLifecycleCoordinator.cs:145`, handling it at
   `:240-256`, which reads `DropDownHost` — i.e. `_openCoordinator?.Host` (`:57`). Host events already
   re-enter the coordinator graph. A host member invoked under `_sync` is therefore not obviously
   isolated from that graph in general, even though `IsOpen` specifically is.

### 5.4 An honest qualification of the SR-4 rationale

`RequestOpen` **already reads `_host.IsOpen` under `_sync`**, at
`QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs:112`:

```csharp
if (_closeInFlight && _host.IsOpen)
    return ClosedTask;
```

inside the `lock (_sync)` opened at `:106`. So "never read `_host.IsOpen` under `_sync`" is not an
invariant the shipped file holds; SR-4's objection is that the refinement would add a **second**
instance of a pattern the sibling feature was closing, not that it would be the first. This is recorded
here so that the recommendation below is not built on an overstated premise. It does not overturn SR-4:
SR-4 is a ratified project decision, and increasing the exposure surface of a pattern under active
remediation is a legitimate reason to decline, independent of whether one instance already exists.

### 5.5 A second, independent argument against the refinement

Under production wiring the refinement would be a **no-op**, because `_host.IsOpen` is already `false`
at the moment `_closeCompleted` is consulted after a successful close. The chain:

- `BreadcrumbUiDispatcher.Dispatch` executes **inline** when already on the captured boundary
  (`QuickFiler/Viewers/BreadcrumbUiDispatcher.cs:78-95`), and `DispatchValue` likewise when
  `_executingDispatcher` is this dispatcher (`:166-178`).
- `BreadcrumbPopupUiOperations.PostAsync` is `_dispatcher.Dispatch` (`…PopupUiOperations.cs:123`) and
  `RunAsync` is `_dispatcher.DispatchValue` (`:120-121`).
- `CloseCore` is always reached from inside a `Dispatch`/`DispatchValue` callback — `SetDroppedDown`'s
  posted body (`…OpenCoordinator.cs:156-168`, close at `:167`), `HandleSelectorOpenStateChanged`'s
  posted body (`:175-183`, close at `:182`), or `FinishOpenCore` run under `RunAsync` (`:223`, close at
  `:277`). So `_executingDispatcher` is set.
- Therefore `_host.Close` → `InvalidateAndSchedule` → `ScheduleInvalidating` → `ScheduleObserved` →
  `RunOnOwnerAsync` (`…OpenLifetime.cs:425-434`) runs its `PostAsync` and `RunAsync` inline, so
  `CompleteClose` (`…Host.cs:397-411`) — and its `OpenState = false` at `:402` — executes
  **synchronously inside** `_host.Close(reason)` before it returns `true` at `…Host.cs:254`.

Consequently `!_host.IsOpen` is already true whenever the suppression is evaluated in production, and
the refinement changes no observable behavior on any path that exists today. Adding a lock-held host
call for zero behavioral delta is not a favorable trade.

---

## 6. Option Space and Recommendation

### (a) Route the bypassing reopen path(s) through `RequestOpen` or `Invalidate`

**Disposition: NOT APPLICABLE — vacuous.** §2 establishes there is no such path. There is nothing to
route. Implementing this option would require inventing a path in order to redirect it.

### (b) Clear `_closeCompleted` explicitly at the bypassing site

**Disposition: NOT APPLICABLE — vacuous, for the same reason.** There is no bypassing site at which to
place a clear.

A near-neighbour worth naming and rejecting explicitly: clearing `_closeCompleted` in
`UpdateRequestProviders` (`…OpenCoordinator.cs:89-102`), on the theory that the same-host
`ConfigureHost` branch (`BreadcrumbItemViewerLifecycleCoordinator.cs:160`) is a lifecycle re-adoption.
Rejected: `UpdateRequestProviders` performs no open (row 15 of the table), so the clear would protect
nothing, and it would silently weaken repeated-close suppression across a reconfiguration for no
demonstrated benefit.

### (c) A defensive guard at a safe point, plus a regression test driving the bypass through an internal seam

The only safe point that does not reintroduce a lock-held host call is to move the *decision* outside
`_sync` while keeping the *flag reads* inside it: read `_closeCompleted` under the lock, release, then
qualify the suppression with a lock-free `_host.IsOpen` read before re-entering the lock to latch
`_closeInFlight`. Shape:

```csharp
bool completed;
lock (_sync)
{
    if (_released) return false;
    if (_closeInFlight) return true;
    completed = _closeCompleted;
}
// Issue #656: the completed-close suppression is qualified by the host's own open state, read
// OUTSIDE _sync so no host member is invoked under the coordinator lock (spec 501 SR-4).
if (completed && !_host.IsOpen) return true;
lock (_sync)
{
    if (_released) return false;
    if (_closeInFlight) return true;
    _closeInFlight = true;
}
```

- Satisfies SR-4's stated objection literally: no host member is called under `_sync`.
- Keeps all four tests in §4 green. §4.1, §4.2 and §4.4 all evaluate the suppression while
  `ControlledHost.IsOpen` is `false` (`ControlledHost.Close` sets `IsOpen = false` when `CloseResult` is
  `true`, `…Tests.cs:436-437`; §4.1's host is never opened at all), so `completed && !IsOpen` still
  suppresses. §4.3 does not exercise `CloseCore`'s guard.
- Testable red-to-green through the existing `ControlledHost.SetOpen(true)` seam (§7).
- **Costs:** double-checked locking with a duplicated guard prologue in the file's most contested
  method; a genuine TOCTOU window between the lock release and the `_host.IsOpen` read; and roughly
  ten added lines in a method whose current shape was adversarially reviewed and ratified.

**Disposition: VIABLE, and the only option that actually removes the residual without violating SR-4.**

### (d) The rejected `&& !_host.IsOpen` refinement (read under `_sync`)

**Disposition: STAYS REJECTED.** Three independent reasons, in descending strength:

1. It is a no-op on every production path that exists today (§5.5), so it buys no behavior change.
2. It contradicts a ratified project decision (SR-4) whose stated rationale — not adding a second
   instance of a lock-held host call while a sibling feature was removing that pattern — still holds.
3. §2 shows the residual it targets is unreachable, so even the hypothetical benefit is unrealized.

What would have to change for it to be reconsidered: a bypassing reopen path would have to be
introduced (making the residual reachable), **and** the maintainer would have to re-open SR-4. Absent
both, reintroducing it is a regression against a reviewed decision.

### (e) No production change; pin the enumeration invariant with a test

Land no production edit; add one deterministic regression test that drives a full
open → successful close → synthetic host reopen → close cycle through the coordinator and asserts that
the close reaches `_host.Close`. This is red on HEAD (the close is suppressed) and would stay red
without a production change, so as a *standalone* option it is not shippable — it can only be paired
with (c). Its value is that it is the acceptance test for (c).

**Disposition: NOT SHIPPABLE ALONE; adopted as the test half of (c).**

### RECOMMENDATION

**Adopt option (c): the lock-free-qualified suppression in `CloseCore`, paired with option (e)'s
red-to-green regression test.**

Rationale, in order:

1. It is the only option that closes the residual the issue names.
2. It honors SR-4's stated objection exactly, so it does not relitigate a ratified decision — it
   satisfies the constraint SR-4 imposed rather than overriding it.
3. It leaves all four tests in §4 unedited, so no regression is traded for the fix.
4. Its footprint is minimal, which matters for the concurrent parallel run.

**Minimum production-file footprint: exactly ONE file —
`QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs`.** No other production file needs to change.
That file is 378 lines, giving 122 lines of headroom under the 500-line ceiling; the change adds
roughly ten.

This footprint is not merely convenient, it is close to forced. The two files the issue's "Suspected
Cause" section nominates as owners of the fix are at the ceiling:
`QuickFiler/Viewers/BreadcrumbDropDownHost.cs` is **498** lines and
`QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` is **497** lines. Any non-trivial edit
to either would force a partial-class split first, multiplying the change footprint and the merge
surface for no benefit — and §2 shows neither file contains a defect to fix.

**Test footprint: exactly ONE existing file —
`QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part3.cs`** (173 lines, 327 lines of
headroom). `…Part2.cs` is 455 lines and `…Tests.cs` is 463 lines, so neither has room for a new test
under the 500-line cap; Part3 is a partial of the same test class and has direct access to the shared
`CoordinatorHarness` and `ControlledHost` fixtures.

**Explicitly out of scope for this remedy:** any edit to `BreadcrumbDropDownHost.cs`,
`BreadcrumbDropDownHost.Open.cs`, `BreadcrumbDropDownOpenLifetime.cs`,
`BreadcrumbItemViewerLifecycleCoordinator.cs`, or `ItemViewer.Breadcrumb.cs`.

---

## 7. Test Seam Analysis

### 7.1 The seam exists today; nothing new is required

- **`[assembly: InternalsVisibleTo("QuickFiler.Test")]`** — `QuickFiler/Properties/AssemblyInfo.cs:5`.
  `BreadcrumbDropDownOpenCoordinator` is `internal sealed` (`…OpenCoordinator.cs:12`) and every member
  the test needs is `internal`, so no reflection is needed to construct or drive it.
- **Injectable host interface:** `IBreadcrumbDropDownHost`
  (`QuickFiler/Viewers/IBreadcrumbDropDownHost.cs:19`), supplied as the second constructor parameter
  `IBreadcrumbDropDownHost host` (`…OpenCoordinator.cs:53`, assigned `:64`).
- **The concrete test double:** `ControlledHost`, a private nested class implementing
  `IBreadcrumbDropDownHost` at `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs:374`.
  Its relevant members:
  - `internal void SetOpen(bool value)` — `…Tests.cs:407`. **This is the bypass seam.** It writes
    `IsOpen` directly, reaching neither `RequestOpen` nor `Invalidate`, which is precisely the
    hypothetical the issue describes. It is already used for exactly this purpose at
    `…Part2.cs:349`, under the comment at `:348`.
  - `internal void Enqueue(Task<bool> result)` — `…Tests.cs:402`.
  - `internal List<BreadcrumbDropDownCloseReason> CloseReasons { get; }` — `…Tests.cs:395-396`.
  - `internal bool CloseResult { get; set; }` — `…Tests.cs:397`.
  - `public bool Close(BreadcrumbDropDownCloseReason reason)` — `…Tests.cs:431-439`.
- **The deterministic pump:** `CoordinatorHarness` (`…Tests.cs:323-372`) wires the coordinator to a
  `BreadcrumbPopupUiOperations` over a `BreadcrumbUiDispatcher` bound to
  `BreadcrumbSelectorToggleUiBoundaryTests.CapturingSynchronizationContext` (aliased at
  `…Tests.cs:12`), drained explicitly by `Context.DrainOne()`, `Context.DrainAll()` and
  `Context.DrainUntil(task)`. One thread, no timers, no sleeps, no temporary files.

**No new seam has to be added.** No `[InternalsVisibleTo]` entry, no new interface, no new injection
point, no reflection. The reflective route is unnecessary and should not be used: a repository-wide
`*.cs` search confirms no test currently reads or writes `_closeCompleted` reflectively (§3, Claim N1).

### 7.2 Proposed regression test shape (design only; no test code authored here)

Placed in `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part3.cs`, MSTest
`[TestMethod]`, FluentAssertions, Arrange–Act–Assert, no Moq needed because `ControlledHost` is the
established hand-written double for this coordinator:

1. **Arrange.** `new CoordinatorHarness()`; `Host.Enqueue(Task.FromResult(true))`;
   `RequestOpen()`; `Context.DrainUntil(opening)`; assert `opening.Result` is `true`.
2. **Arrange.** `SetDroppedDown(false)`; `Context.DrainAll()`; assert `Host.CloseReasons` equals
   `{ Uncommitted }` and `Host.IsOpen` is `false` — this latches `_closeCompleted`.
3. **Act.** `harness.Host.SetOpen(true);` — the bypass, reaching neither `RequestOpen` nor
   `Invalidate`. Then `harness.SelectorOpen = true;` and a second `SetDroppedDown(false)` with
   `Context.DrainAll()`.
4. **Assert.** `Host.CloseReasons` should have two elements — the close of a genuinely open host must
   reach `_host.Close`. **Red on HEAD** (the `:316` guard suppresses it, leaving one element),
   **green after option (c)**.

This is a genuine red-to-green regression test, not a test that codifies current behavior. It satisfies
the repository bugfix workflow's "failing regression test first" requirement, the General Unit Test
Policy's determinism and no-temporary-file rules, and the C# Unit Test Policy's MSTest + FluentAssertions
requirements (Moq is available but not needed for this shape).

### 7.3 Ancillary check for the executor

`QuickFiler.Test/Viewers/BreadcrumbDropDownCoverageThresholdTests.cs` exists and pins coverage-driven
guard behavior for the popup lifecycle. Option (c) adds a branch to `CloseCore`; the executor must
confirm the new branch is exercised (the test in §7.2 covers the `completed && IsOpen` arm; §4.1/§4.2/
§4.4 cover the `completed && !IsOpen` arm) so no changed line lands uncovered.

---

## 8. Severity Framing

The issue records **Medium** and **latent**, and that framing is correct and should not be escalated.

Stated plainly: **because §2 establishes that no bypassing reopen path exists in the shipped code, this
is latent-correctness hardening, not an observed user-facing failure.** No user can currently reach the
suppressed-close state. There is no reproduction on a running Outlook host, and the issue's own
"Logs / Screenshots" section says so: "no runtime log; the residual is established by source inspection
of the flag-clearing paths"
(`docs/features/potential/promoted/2026-08-27-breadcrumb-closecompleted-residual-outside-requestopen-invalidate.md:44`).

The value of fixing it is that the coordinator's suppression currently depends on an *external*
invariant — "nothing opens the host except `RequestOpen`" — that is held by the shape of the call graph
in three other files rather than by anything the coordinator enforces or asserts. Option (c) makes the
suppression self-sufficient, so a future change to the host or lifetime that adds an open path cannot
silently reintroduce a suppressed close. That is a real but modest benefit, consistent with Medium.

The issue's own "Suspected Cause / Notes" claim that "the reopen paths that bypass `RequestOpen` and
`Invalidate` live in the ItemViewer breadcrumb lifecycle host surface"
(`…promoted/2026-08-27-…md:68-70`) is **not confirmed**: those files contain no such path. The claim
should be corrected in `spec.md` rather than carried forward, and the fix should not be sited in
`BreadcrumbItemViewerLifecycleCoordinator.cs`, `BreadcrumbDropDownHost.cs`, or
`ItemViewer.Breadcrumb.cs` as the issue anticipated.

---

## 9. Toolchain

The applicable C# toolchain, in this exact order, restarting from step 1 whenever any step fails or
auto-fixes files:

1. **Format** — `dotnet tool run csharpier format .`
   Verify read-only with `dotnet tool run csharpier check .`. Run `dotnet tool restore` once per clone
   or worktree first. Always invoke through `dotnet tool run` so the manifest-pinned CSharpier 1.2.6 is
   used; never a global install.
2. **Analyze** —
   `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. **Type-check / nullable** —
   `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. **Test** — `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

**Explicit warnings, both load-bearing:**

- **Do NOT add `/p:Nullable=enable`.** No project in this repository carries a `<Nullable>` element and
  there is no `Directory.Build.props`, so the property is a solution-wide opt-in that conscripts files
  that never adopted the pragma. CI omits it deliberately. Note that
  `BreadcrumbDropDownOpenCoordinator.cs` itself carries `#nullable enable` at line 1, so it is already
  under nullable analysis on a per-file basis and step 3 does gate it.
- **Use `/t:Rebuild`, never `/t:Build`.** MSBuild's incremental up-to-date check does not invalidate on
  a command-line `/p:` change, so a warm `/t:Build` returns exit 0 with `CoreCompile` skipped on every
  project and runs no analyzers — the gate cannot fail.

**Test assembly path for `QuickFiler.Test`:**
`QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` (repository-relative). Derived from
`QuickFiler.Test/QuickFiler.Test.csproj:17` (`<AssemblyName>QuickFiler.Test</AssemblyName>`) and `:36`
(`<OutputPath>bin\Debug\</OutputPath>` for the Debug/Any CPU configuration), and corroborated by the
`codeBase` attribute recorded in prior TRX evidence under
`docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/evidence/regression-testing/p4-t8-d4-full-suite/488-d4-full-suite.trx`.

Two local-run notes carried from prior sessions in this repository, to be confirmed by the executor
against the current runner configuration rather than assumed: local `vstest.console.exe` invocations
have needed CI's `/InIsolation` flag, and a filter excluding `\.claude\` worktree copies of the same
assembly, to avoid assembly-load failures that present as empty-message, sub-millisecond test failures.

---

## 10. Provenance read for this research

- `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` (378 lines, read in full)
- `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` (498 lines, read in full)
- `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs` (107 lines, read in full)
- `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs` (460 lines, read in full)
- `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` (497 lines, read in full)
- `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.Search.cs` (45 lines, read in full)
- `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` (449 lines, read in full)
- `QuickFiler/Viewers/IBreadcrumbDropDownHost.cs` (68 lines, read in full)
- `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs` (285 lines, read in full)
- `QuickFiler/Properties/AssemblyInfo.cs`
- `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs` (463 lines, read in relevant part
  plus both fixture classes)
- `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` (455 lines, read in full)
- `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part3.cs` (173 lines, read in full)
- `QuickFiler.Test/Viewers/BreadcrumbDropDownCoverageThresholdTests.cs` (partial)
- `QuickFiler.Test/QuickFiler.Test.csproj` (output-path and assembly-name properties)
- `docs/features/active/breadcrumb-coordinator-hub-defects-501/spec.md` (SR-4 at `:426-437`,
  cross-feature note 4 at `:183-185`, per-defect design at `:453-472`, risk at `:994-996`, follow-up at
  `:1028`, implementation note at `:1062-1068`)
- `docs/features/active/breadcrumb-coordinator-hub-defects-501/research/2026-08-24T09-12-breadcrumb-ordering-invariants-research.md`
  (`§6.1` at `:692-740`, `§6.2` at `:742-779`)
- `docs/features/active/breadcrumb-coordinator-hub-defects-501/evidence/issue-updates/followup-sr4-residual.2026-08-27T23-37.md`
- `docs/features/potential/promoted/2026-08-27-breadcrumb-closecompleted-residual-outside-requestopen-invalidate.md`
- `docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/spec.md` (`### Corrections to the
  promoted potentials (binding)` at `:228-238`)
- `docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/research/2026-08-25T10-00-itemviewer-breadcrumb-lifecycle-defects-research.md`
  (`:876-890`, the cession of the four host files by feature 501)
- `docs/features/active/2026-08-27-breadcrumb-closecompleted-residual-outside-requestopen-invalidate-656/spec.md`
