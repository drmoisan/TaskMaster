# P5-T173 — UI-dispatch seam dependency and blast-radius inventory (read-only)

Timestamp: 2026-07-22T15-07Z

Command: `cd "C:/Users/DanMoisan/repos/TaskMaster-wt/2026-07-21T10-25" && grep -rn "\.Dispatch(\|IsCurrentBoundary\|_ownerThreadId\|CaptureCurrent()\|CreateForCurrentThreadTests()\|CaptureCurrentOrTests()\|new BreadcrumbUiDispatcher(\|new BreadcrumbBridgeCoordinator(" --include=*.cs QuickFiler QuickFiler.Test`

EXIT_CODE: 0

## Determination context

P5-T172 recorded `DETERMINATION: B`. The deciding member is the private
`QuickFiler/Viewers/BreadcrumbUiDispatcher.cs` method `IsCurrentBoundary()` (lines 255-263), and specifically its third
disjunct at lines 259-262 (bare owner-thread-identity match). Its only caller is `Dispatch` at line 78. `DispatchValue<T>`
does not call it (it gates on `ReferenceEquals(_executingDispatcher, this)` at line 166), so `DispatchValue` call sites
are outside the deciding seam.

## Liveness precondition of the deciding branch

The deciding disjunct is reachable only when `_ownerThreadId.HasValue`. That is set by exactly two factories:

| Factory | `_context` | `_ownerThreadId` | Deciding disjunct |
|---|---|---|---|
| `BreadcrumbUiDispatcher.CaptureCurrent()` (lines 44-56) | non-null | set | **live** — thread identity can substitute for the captured context |
| `BreadcrumbUiDispatcher.CreateForCurrentThreadTests()` (lines 62-65) | null | set | live and **load-bearing** — the only available inline proof |
| `internal BreadcrumbUiDispatcher(SynchronizationContext, Action<Exception>)` (lines 25-30) | non-null | `null` | not reachable |

## First-party production call sites of the deciding member (via `Dispatch`)

| # | File:line | Member | Dispatcher origin | Classification |
|---:|---|---|---|---|
| 1 | `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs:219` | `SetTheme` | ctor-injected or `CaptureCurrent()` (line 452) | preserved-behavior |
| 2 | `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs:228` | `ApplyTransition` | same | preserved-behavior |
| 3 | `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs:247` | `PostRenderAndSelectorAsync` | same | **requires-review — deciding path of the P5-T172 failure** |
| 4 | `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs:327` | `DispatchInboundMessageAsync` (selector) | same | requires-review — same post-await resumption shape |
| 5 | `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs:336` | `DispatchAsync` (synthetic arrow) | same | requires-review — same post-await resumption shape |
| 6 | `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs:340` | `DispatchAsync` (publish outputs) | same | requires-review — same post-await resumption shape |
| 7 | `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs:120` | `PostAsync` | `CaptureCurrent()` (line 73), `CreateForCurrentThreadTests()` (line 76), `CaptureCurrentOrTests()` (lines 78-81), or ctor-injected | preserved-behavior |
| 8 | `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs:429` | readiness handler detach | same | preserved-behavior |
| 9 | `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs:453` | `NavigationStarted` | same | preserved-behavior |
| 10 | `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs:455` | navigation completion | same | preserved-behavior |
| 11 | `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs:463` | readiness cancel | same | preserved-behavior |
| 12 | `QuickFiler/Viewers/WebView2Messenger.cs:40` | messenger dispatch | `CaptureCurrent()` (line 144) or injected | preserved-behavior |
| 13 | `QuickFiler/Viewers/WebView2Messenger.cs:62` | messenger dispatch | same | preserved-behavior |
| 14 | `QuickFiler/Viewers/WebView2Messenger.cs:80` | messenger dispatch | same | preserved-behavior |
| 15 | `QuickFiler/Viewers/WebView2Messenger.cs:104` | messenger dispatch | same | preserved-behavior |

Additional first-party production sites that construct a deciding-branch-live dispatcher (no `Dispatch` call of their
own): `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:58`, `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:91`,
`QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs:170`, `QuickFiler/Viewers/BreadcrumbDropDownHost.cs:55`.

"requires-review" entries 4-6 are the same latent post-await resumption defect on other inbound paths; a correction that
removes bare thread identity as boundary proof when a context was captured makes all four deterministic in one place.
They require no separate edit.

## First-party tests whose current behavior depends on the deciding branch

### Group 1 — dispatcher from `CaptureCurrent()` (deciding disjunct live, context also present)

| File:line | Test / harness | Classification |
|---|---|---|
| `QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs:39` | `SetSuggestionsAsync_WorkerProviderCompletion_SchedulesPostOnOwningContext` | **requires-review** — the failing case; must become deterministic |
| `QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs:78` | `InboundWorkerMessage_SchedulesEveryPostAndCallbackOnOwningContext` | requires-review — latently exposed to the same disjunct through `DispatchAsync`; expected to become deterministic |
| `QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs:287` | `ProductionCaptureWithoutUiContext_FailsFast` (capture guard only) | preserved-behavior — asserts the `CaptureCurrent()` null-context throw; no `Dispatch` call |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs:46` | `WorkerProviderAndSelectorToggle_MarshalPostsAndCallbackEntryToOwningBoundary` | preserved-behavior — observes with a blocking `context.WaitForPost()` (line 64), which keeps the owner thread occupied so the deciding disjunct cannot fire; drains install the captured context |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownReadinessTests.cs:132` | `CaptureCurrent_ControlledContext_CreatesOperationsWithoutInvokingWebView` | preserved-behavior — construction only, no `Dispatch` |
| `QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorTests.cs:101,429` | harness / `SelectRow_WhileSuggestionsUpgradeInFlight_...` | preserved-behavior — these already terminate at the `CaptureCurrent()` guard (`InvalidOperationException: Breadcrumb UI components must be constructed on an owning UI synchronization context.`) because no ambient context is installed; the guard is not being changed |
| `QuickFiler.Test/Viewers/BreadcrumbCoordinatorLifecycleTests.cs:33,62,121,156` | known unpumped lifecycle harness (P6 scope, fixed rule / correction-map item 13) | preserved-behavior — same pre-existing `CaptureCurrent()` guard failure, observed unchanged; explicitly excluded from the P5 gate |

### Group 2 — dispatcher from `CreateForCurrentThreadTests()` (context-less; thread identity is the only inline proof and must be preserved verbatim)

| File:line | Classification |
|---|---|
| `QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs:299` | preserved-behavior |
| `QuickFiler.Test/Viewers/BreadcrumbBridgeCoordinatorProbabilityTests.cs:150` | preserved-behavior |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownReadinessTests.cs:314` | preserved-behavior |
| `QuickFiler.Test/Viewers/BreadcrumbDuplicateIdentityIntegrationTests.cs:151` | preserved-behavior |
| `QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs:212` | preserved-behavior |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorCoordinatorTests.cs:386` | preserved-behavior |
| `QuickFiler.Test/Viewers/FolderBreadcrumbAssetContractTests.cs:184` | preserved-behavior |

### Group 3 — dispatcher from the two-argument internal constructor (`_ownerThreadId == null`, deciding disjunct unreachable)

Every remaining `new BreadcrumbUiDispatcher(context, sink)` test site, including
`BreadcrumbUiThreadDispatchTests.cs:166,192,223,261,318`, `BreadcrumbSelectorToggleUiBoundaryTests.cs:99,185,257`,
`BreadcrumbPopupBoundaryCoverageTests.cs`, `BreadcrumbDropDownLifecycleCoverageTests.cs`,
`BreadcrumbDropDownOpenCoordinatorTests.cs`, `BreadcrumbSelectorOpenRetryTests.cs`,
`BreadcrumbCollapsedSurfaceReadinessTests.cs`, and `BreadcrumbDropDownHostTests.cs`. All are classified
preserved-behavior: the deciding disjunct is not reachable for them, so their inline/post semantics are unaffected.
This explicitly includes `BreadcrumbUiThreadDispatchTests.DispatchValue_NestedSynchronousDispatch_ExecutesInlineWithoutAnotherPost`
(line 218, dispatcher constructed at line 223), whose `context.PostCount.Should().Be(0)` assertion at line 250 depends on
the ambient-context disjunct at line 258, not on the deciding thread-identity disjunct.

## Scope conclusion

- No first-party production caller outside the enumerated set above is in scope.
- The correction set is **one** production C# source, `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs`, and **zero** test
  C# sources. This is within the P5-T177 bound of at most two production sources and at most one test source, so no
  atomic replanning is triggered.

Output Summary: Deciding member `BreadcrumbUiDispatcher.IsCurrentBoundary()` lines 259-262, reached only through
`Dispatch` (line 78) and only when `_ownerThreadId.HasValue`. Fifteen first-party production `Dispatch` call sites were
enumerated: one is the deciding path (`BreadcrumbBridgeCoordinator.cs:247`), three more share the same post-await
resumption shape and are fixed by the same single change (`:327`, `:336`, `:340`), and eleven are preserved-behavior.
Tests were partitioned into three groups: two requires-review cases (both in `BreadcrumbUiThreadDispatchTests.cs`, lines
39 and 78), seven `CreateForCurrentThreadTests()` sites whose context-less thread-identity inline path must be preserved
verbatim, and all remaining sites where the deciding disjunct is unreachable because `_ownerThreadId` is null. The
`BreadcrumbBridgeCoordinatorTests` / `BreadcrumbCoordinatorLifecycleTests` sites were observed to fail today at the
unchanged `CaptureCurrent()` ambient-context guard and are unaffected. Planned edit set: 1 production file, 0 test files
— within the at-most-two-production / at-most-one-test bound; no replanning required. EXIT_CODE: 0.
