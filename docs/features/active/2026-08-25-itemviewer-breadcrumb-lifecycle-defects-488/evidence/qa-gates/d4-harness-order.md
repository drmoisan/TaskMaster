# D4 — Harness Construction-Order Confirmation ([P4-T2])

Timestamp: 2026-08-28T05-44

Command: `git grep -n -F 'ItemViewer()' -- 'QuickFiler.Test/*'` to re-derive the site set
independently, then `grep -n 'SetSynchronizationContext'` over each file holding a site, plus targeted
source reads at each construction point.
EXIT_CODE: 0

The enumeration below was re-derived from the tree first and compared against constraint C6
afterwards, rather than read out of C6 and confirmed.

---

## Part 1 — the thirteen sites constraint C6 enumerates

### Ten direct-constructor sites across nine files

Every one installs its synchronization context **before** constructing the viewer.

| # | File | Context installed at | Viewer constructed at | Install precedes construct |
| --- | --- | --- | --- | --- |
| 1 | `Viewers/BreadcrumbCoordinatorLifecycleTests.cs` | 476 | 477 | **yes** |
| 2 | `Viewers/BreadcrumbCollapsedSurfaceReadinessTests.cs` | 410 | 413 | **yes** |
| 3 | `Viewers/BreadcrumbPendingOpenCloseTests.cs` | 360 | 363 | **yes** |
| 4 | `Viewers/BreadcrumbDropDownIntegrationTests.cs` | 337 | 338 | **yes** |
| 5 | `Viewers/BreadcrumbSubfolderActivationTests.cs` | 274 | 305 | **yes** |
| 6 | `Viewers/BreadcrumbSelectorOpenRetryTests.cs` | 254 | 255 | **yes** |
| 7 | `Controllers/QfcItemControllerBreadcrumbDropDownTests.cs` | 372 | 373 | **yes** |
| 8 | `Controllers/QfcItemController.EventWiringTests.cs` | 234 | 237 | **yes** |
| 9 | `Controllers/QfcItemController.EventWiringTests.cs` | 325 | 328 | **yes** |
| 10 | `Controllers/QfcItemController.ViewerSetupTests.cs` | 392 | 395 | **yes** |

Rows 8, 9, and 10 sit one line below C6's cited construct lines (C6 gives 236, 327, and 395); rows 1
through 7 match C6 exactly. The drift is expected against the pre-change citation anchor and was
resolved by locating each `SetSynchronizationContext` call and each `new QuickFiler.ItemViewer()` in
the current tree.

Rows 1, 2, and 3 are the **three harnesses the spec leaves unconfirmed** —
`BreadcrumbCoordinatorLifecycleTests.cs`, `BreadcrumbCollapsedSurfaceReadinessTests.cs`, and
`BreadcrumbPendingOpenCloseTests.cs`. All three are confirmed correct-order here, which is the
evidence the criterion `[P4-T14]` flips requires.

### Two pump-thread sites

| # | File | Line | Note |
| --- | --- | --- | --- |
| 11 | `Controllers/QfcItemController.ViewerSetupTests.cs` | 433 | `await host.InvokeAsync(() => new QuickFiler.ItemViewer())` on a `WinFormsPumpHost`. **The captured context is the pump's own.** The test's own comment says so: "the viewer must be constructed on the pump so `UiSyncContext` binds there." |
| 12 | `Controllers/QfcItemController.InitializationTests.Part2.cs` | 74 | `await host.InvokeAsync(() => new QuickFiler.ItemViewer())` inside `BuildPumpHarnessCoreAsync`. **The captured context is the pump's own.** |

C6 cites row 12 at line 84; it resolves at **74**. Same expected drift.

### One constructor-bypassing site

| # | File | Lines | Note |
| --- | --- | --- | --- |
| 13 | `Helper Classes/QfcThemeHelperTests.cs` | 247-249 | `CreateItemViewer()` produces its viewer through `CreateUninitialized<ItemViewer>()`. |

The constructor never runs there, so `_context` stays null and `UiSyncContext` is null. **The guard's
null escape makes the guard inert for that viewer**, which is why the site is harmless. It is recorded
because it is a real counterexample to the "every successfully constructed `ItemViewer` has a non-null
`UiSyncContext`" statement and must not be rediscovered as a surprise. **The file calls none of the
four guarded members**: a search of `QfcThemeHelperTests.cs` for `InitializeBreadcrumbPipeline`,
`ConfigureBreadcrumbDropDown`, and `EnsureBreadcrumbPipeline` returns zero hits, so the guard cannot
be reached from it at all.

---

## Part 2 — the six expected non-site hits, discarded

The fixed string over-matches. These six hits are not construction sites, are expected, and are
**not** discrepancies. All six are present in the tree exactly as C6 describes:

| File | Line | Why it is not a site |
| --- | --- | --- |
| `QfcViewer_Test.cs` | 29 | commented out (`//iv = new ItemViewer();`) |
| `QfcViewer_Test.cs` | 37 | commented out, and names `QfcItemViewer()` |
| `TestSupport/WinFormsPumpHost.cs` | 109 | inside an XML doc comment |
| `Controllers/QfcItemController.InitializationTests.Part3.cs` | 345 | inside an XML doc comment |
| `Helper Classes/QfcThemeHelperTests.cs` | 111 | a call to the local `CreateItemViewer()` helper |
| `Helper Classes/QfcThemeHelperTests.cs` | 141 | a call to the local `CreateItemViewer()` helper |

---

## Part 3 — DISCREPANCY: the re-derived site set does NOT equal the C6 enumeration

**The independently re-derived set is larger than C6's.** The search returns **25** hits, not the
**19** C6 states. Discarding the six non-site hits leaves **19 executable sites**, not the thirteen C6
enumerates. Six sites are present in the tree and absent from C6.

Constraint C6's own text says a site present in the tree but absent from C6 "is recorded as a
discrepancy and reported before `[P4-T4]` proceeds." That is done here and in the executor's report.

| # | File | Line | Context installed at | Install precedes construct | In C6? |
| --- | --- | --- | --- | --- | --- |
| 14 | `Controllers/BreadcrumbBridgeRouterTests.cs` | 422 | 419 | **yes** | no |
| 15 | `Controllers/EfcItemControllerTests.cs` | 201 | 196 | **yes** | no |
| 16 | `Controllers/EfcItemControllerTests.cs` | 316 | 311 | **yes** | no |
| 17 | `Controllers/QfcItemController.EventWiringTests.cs` | 433 | 430 | **yes** | no |
| 18 | `Controllers/EfcItemController.CleanupTests.cs` | 41 | **none** | n/a — see below | no |
| 19 | `Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs` | 349 | 348 | **yes** | not applicable |

### Assessment of each

- **Sites 14 through 17 satisfy the install-before-construct order.** They are ordinary omissions from
  C6's enumeration rather than violations of the property C6 exists to protect. Four of the five
  genuine additions are therefore harmless on C6's own criterion.
- **Site 18, `EfcItemController.CleanupTests.cs:41`, installs no synchronization context at all.** It
  constructs the viewer inside `CreateFiveArgumentController()` with whatever
  `SynchronizationContext.Current` happens to be on the MSTest worker thread. This is the one site
  that does not satisfy C6's install-before-construct property, and it is recorded and reported rather
  than edited, per C6's escalation rule. Two facts bound its risk: if the ambient context is null the
  viewer's `UiSyncContext` is null and the guard's null escape makes the guard inert; and a search of
  the file for `InitializeBreadcrumbPipeline`, `ConfigureBreadcrumbDropDown`, and
  `EnsureBreadcrumbPipeline` returns **zero** hits, so no guarded member is named from it. The same
  zero-hit result holds for `EfcItemControllerTests.cs`, `BreadcrumbBridgeRouterTests.cs`, and
  `QfcItemController.EventWiringTests.cs`.
- **Site 19 is this feature's own new file** and did not exist when C6 was measured, so it is not a
  discrepancy in C6. Its `ViewerScope` installs the context at 348 and constructs at 349.

**No site was edited in response.** Constraint C6 requires that a site failing the order be recorded
and reported under the escalation rule rather than corrected, and that the guard not be weakened to a
null check. Neither was done.

The static analysis above bounds the risk but does not settle it, because a guarded member could be
reached indirectly through controller code rather than named in the test file. `[P4-T8]` settles it
empirically by running the entire `QuickFiler.Test` assembly against the delivered guard and comparing
the failing-test-name set to the Phase 0 `BASELINE_FAILURE_SET`, which is empty.

---

## Part 4 — a correction to constraint C6's reachability argument

C6 argues that "every successfully constructed `ItemViewer` has a non-null `UiSyncContext`" because
the constructor "assigns `_context = SynchronizationContext.Current` and then calls
`TaskScheduler.FromCurrentSynchronizationContext()`, which throws when the ambient context is null."

**The second half of that premise does not hold against this branch's base.** The delivered
`ItemViewer` constructor reads:

```csharp
        public ItemViewer()
        {
            InitializeComponent();
            _context = SynchronizationContext.Current;
            _uiDispatcher = Dispatcher.CurrentDispatcher;
            InitControlGroups();
        }
```

There is no `TaskScheduler.FromCurrentSynchronizationContext()` call. Sibling feature
`itemviewer-surface-defects-489`, which is merged into this base, removed the `UiScheduler` property
that call backed; `UiSyncContext` and `UiDispatcher` are the survivors.
`Dispatcher.CurrentDispatcher` creates a dispatcher for the calling thread and does **not** throw on a
null ambient `SynchronizationContext`.

The consequence is that a viewer constructed with no ambient context installed — site 18 above — is
now constructible and has a null `UiSyncContext`. That widens the class of viewers for which the
guard's null escape is the operative branch, and it removes C6's argument that the escape is
unreachable except by reflection.

**This does not change any delivered design.** The D4 guard already returns without effect when
`UiSyncContext` is null, which is exactly the behaviour these viewers need. `[P6-T2]`'s reflective
null assignment is still required and still correct: that test needs a viewer whose breadcrumb
pipeline has been **seeded** through `InitializeBreadcrumbPipeline`, and seeding requires a viewer
that has run its constructor. The correction is recorded so that a reviewer does not rely on C6's
stated reachability argument, which is stale.

Output Summary: All **thirteen** sites constraint C6 enumerates are present and confirmed — the ten
direct-constructor sites all install the context before constructing, the two pump-thread sites bind
the pump's own context, and the constructor-bypassing `QfcThemeHelperTests.CreateItemViewer()` site
yields a null `UiSyncContext` that the guard's null escape renders inert, in a file that calls none of
the four guarded members. The six expected non-site hits are present and discarded. **DISCREPANCY: the
re-derived set contains six sites absent from C6** (25 hits, 19 executable sites, not 13). Four satisfy
the install-before-construct order; one, `EfcItemController.CleanupTests.cs:41`, installs no context at
all and is reported rather than edited; one is this feature's own new file. None of the four affected
files names any guarded member. `[P4-T8]` proves the enumeration empirically.
