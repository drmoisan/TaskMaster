# itemviewer-breadcrumb-lifecycle-defects (Spec)

- **Issue:** #488 (primary); also closes #475
- **Parent (optional):** epic `quickfiler-bug-family`; integration branch `epic/quickfiler-bug-family-integration`; wave 3 (last child)
- **Owner:** drmoisan
- **Last Updated:** 2026-08-25
- **Status:** Approved
- **Version:** 1.0

Work mode is `full-bug`. Per `.claude/skills/acceptance-criteria-tracking/SKILL.md`, this document is
the **sole** authoritative acceptance-criteria source for the feature. `user-story.md` is intentionally
absent and must not be created; a `user-story.md` in this folder splits the AC surface and is an
integrity failure.

## Authoritative inputs and precedence

1. `research/2026-08-25T10-00-itemviewer-breadcrumb-lifecycle-defects-research.md` — the primary
   technical input. Authoritative on root cause, minimal-fix design, regression-test feasibility, blast
   radius, and file-size headroom.
2. `research/2026-08-25T10-20-orchestrator-comment-crosscheck.md` — closes the research's single
   unverified input (§0.1, the #488 correction comment). **Authoritative where it addresses the two
   adjacent defects the research left unevaluated** (`SetBridgeCoordinator` replace-without-dispose;
   `Reset()` surface-detach synchrony).
3. The promoted potentials
   `docs/features/potential/promoted/2026-08-07-itemviewer-breadcrumb-pipeline-lifecycle.md` (#488) and
   `docs/features/potential/promoted/2026-08-07-breadcrumb-capturecurrentortests-silently-degrades-in-production.md`
   (#475) — read for provenance only. **Three of their reachability claims and one of their mechanism
   claims are wrong.** The corrections are tabulated under Root Cause Analysis and are binding. A review
   that "restores" a potential's wording is reintroducing a known error.

Where this document and the research disagree, **this document governs**: the research is a
point-in-time input captured before the design was finalised. Four known divergences are recorded under
"Divergences from the research" below.

## Line-citation anchor

Every `file:line` citation is anchored to the **pre-change** source at worktree HEAD `988e819b`. All
citations in this document were re-verified by direct file read on 2026-08-25; the research's own
citation-drift table (§1.1) was re-checked rather than trusted. `BreadcrumbBridgeRouter.cs`,
`EfcFormController.cs`, and several `UtilitiesCS/OutlookObjects/Folder/` breadcrumb files changed on
`main` via PR #605, so any citation into those files is deliberately absent from this document. This
feature's own edits shift the lines that follow them: **resolve every citation by the member name it
accompanies, not by the line number.**

---

## Context

- **Summary.** This feature closes two pre-existing defect issues in the QuickFiler breadcrumb pipeline.
  #488 records five lifecycle defects (D1 through D5) in `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs`
  and the collaborator it wraps; #475 records a fail-fast guard inverted into a silent degradation in
  `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs`. Six defect units in total.

- **Why the two issues are one feature.** #475's fix edits two call sites
  (`ItemViewer.Breadcrumb.cs:156` and `:192`) inside the same file that four of #488's five defects live
  in. Splitting them would produce two concurrent branches editing the same file.

- **Observed environment.** VSTO add-in hosted in Microsoft Outlook on Windows 11; `QuickFiler`
  assembly, .NET Framework 4.8.1 WinForms with Microsoft WebView2. All six defects are present in the
  current source of the four owned files.

- **Customer impact and severity.** Severity must be read together with reachability, because the
  research established that most of these defects are **latent** on today's production paths:

  | Unit | Filed severity | Reachability today | Consequence if reached |
  | --- | --- | --- | --- |
  | D1 | High | **Latent** — the replacement path runs on every pooled reuse, but on the captured UI boundary, where the ordering is currently correct | An outgoing open host's close callbacks cancel the **new** host's live selector session and steal focus back to the anchor |
  | D2 | Medium | **Latent** — configure and theme are issued back to back on the UI thread | Split theme: collapsed breadcrumb dark, popup light (same family as #254 / #269) |
  | D3 | Medium | **Not reachable** — an upstream guard prevents the second call entirely | None in production; a laxer wrapper than the collaborator it wraps |
  | D4 | Medium | **Not reachable** — both production callers are UI-thread-bound; nothing declares the constraint | Leaked `BreadcrumbMessengerHub` and an undetached `MessageReceived` subscription per lost race |
  | D5 | Medium | **Plausibly live** — an in-flight `InitializeWebViewAsync` resuming against a disposed pooled viewer | A `Container` created after `Dispose(bool)` ran is never disposed; the hub, lifecycle coordinator and bridge coordinator all leak |
  | #475 | High | **Latent on production paths**; **live as a design defect** | Popup silently never opens: no exception, no user-visible error, one log line |

- **First observed.** All six were identified on 2026-08-07 during preparation research for epic #136
  (children F13 and F14) and were deferred out of that epic because its children carry a hard
  no-behaviour-change non-functional requirement. Each of the six alters observable behaviour on a
  construction, replacement, or teardown path, so each requires its own regression test.

---

## Repro & Evidence

Reproduction is by source inspection and by unit-level exercise of the affected members. No live Outlook
session and no live WebView2 runtime is required for any of the six: every fix lands on a host-neutral
seam that an MSTest fixture can drive.

### D1 — host replacement on WebView2 environment change

- **Steps.** Configure the drop-down with environment A, then configure again with a different
  environment reference. Observe the outgoing host.
- **Expected.** The outgoing host is disposed before its replacement exists over the same anchor.
- **Actual.** The replacement is constructed **synchronously** at `ItemViewer.Breadcrumb.cs:159`, before
  any release is even scheduled. The disposal of the outgoing host is reached only later, through
  `BreadcrumbItemViewerLifecycleCoordinator.ConfigureHost`'s posted lambda (`:120`) →
  `ReleaseHostCore()` (`:129`, body at `:292-304`) → `coordinator.Release()` (`:302`) →
  a second `PostAsync` whose task is discarded (`BreadcrumbDropDownOpenCoordinator.cs:187`).
- **Determinism.** Deterministic given a drainable synchronization context. On the production UI thread
  every `PostAsync` runs inline (`BreadcrumbUiDispatcher.cs:255-278`), so the ordering currently holds.

### D2 — `SetBreadcrumbTheme` lost when the host post is deferred

- **Steps.** Queue `ConfigureHost` without draining, then call `SetTheme("dark")`, then drain.
- **Expected.** The adopted host carries the theme.
- **Actual.** `BreadcrumbItemViewerLifecycleCoordinator.SetTheme` (`:155-160`) reads
  `DropDownHost` → `_openCoordinator?.Host` (`:53`). `_openCoordinator` is assigned only inside the
  `ConfigureHost` post (`:130`), so `DropDownHost` is null at `:159`, the null-conditional swallows the
  call, and nothing is surfaced. The coordinator retains no theme, so no replay occurs.
- **Determinism.** Fully deterministic with `QueuedCreatorThreadSynchronizationContext`.

### D3 — a second, different `IFolderHierarchyProvider` is silently discarded

- **Steps.** Call `InitializeBreadcrumbPipeline(providerA)`, then `InitializeBreadcrumbPipeline(providerB)`.
- **Expected.** The second call either fails fast or re-initializes explicitly.
- **Actual.** `ItemViewer.Breadcrumb.cs:45-48` returns as soon as `BreadcrumbCoordinator != null`,
  without comparing providers. `BreadcrumbItemViewerLifecycleCoordinator.SetBridgeCoordinator`
  (`:62-77`, compare at `:66-69`) *does* compare by reference, so the wrapper is laxer than the
  collaborator it wraps.
- **Determinism.** Deterministic, and reachable only from a test: see the reachability correction below.

### D4 — non-atomic read-then-write on pipeline initialization

- **Steps.** Enter `InitializeBreadcrumbPipeline` from two threads with no barrier.
- **Expected.** One pipeline is built, or the off-thread caller is rejected.
- **Actual.** Four unsynchronized read-then-write pairs, all verified:

  | Pair | Read | Write |
  | --- | --- | --- |
  | Bridge coordinator | `ItemViewer.Breadcrumb.cs:45` | `:59` |
  | Drop-down host | `:147-148` | `:159` construction; coordinator `:130` adoption |
  | Lifecycle coordinator | `:278` | `:289` |
  | Resource owner | `:302` | `:307-309` |

  Both threads construct a `BreadcrumbItemViewerLifecycleCoordinator` (with its own
  `BreadcrumbMessengerHub`, `:284`) and a `BreadcrumbBridgeCoordinator` (`:53-57`); one pair is
  overwritten and never disposed.
- **Determinism.** **Not deterministically reproducible.** See Test Strategy, D4.

### D5 — a `Container` created during teardown is never disposed

- **Steps.** Dispose the viewer, then reach `EnsureBreadcrumbResourceOwnership` (`:300-310`).
- **Expected.** Creation during teardown is refused, or the created `Container` is disposed.
- **Actual.** `components ??= new Container();` at `:307` and `components.Add(...)` at `:309` run
  unguarded. `ItemViewer.Designer.cs:16-23` disposes `components` only when it is non-null **at the
  moment `Dispose(bool)` runs**, and `Control.Dispose` does not run twice, so a `Container` created
  afterwards is never disposed: `BreadcrumbResourceOwner.Dispose` never fires,
  `DisposeBreadcrumbResources` (`:312-317`) never runs, and the hub, the lifecycle coordinator and the
  bridge coordinator all leak.
- **Determinism.** Deterministic: dispose the viewer, then call `InitializeBreadcrumbPipeline`.

### #475 — `CaptureCurrentOrTests()` inverts the fail-fast guard

- **Steps.** Construct the popup operations from a thread where `SynchronizationContext.Current` is
  null, then request a drop-down open.
- **Expected.** Construction off the owning UI synchronization context is a programming error and fails
  fast with the `InvalidOperationException` that `BreadcrumbUiDispatcher.CaptureCurrent()` (`:44-56`)
  already defines.
- **Actual.** `BreadcrumbPopupUiOperations.cs:86-89` selects `CreateForCurrentThreadTests()` — a
  dispatcher whose documented contract is to *report* cross-thread work rather than schedule it.
- **Refined failure mechanism (this is what a test drives).** A null-context dispatcher still runs work
  **inline on its owner thread**; it fails only when work must cross threads.
  `BreadcrumbDropDownOpenCoordinator.OpenCoreAsync` (`:194-211`) awaits with `.ConfigureAwait(false)`
  three times, so the continuation resumes on a thread-pool thread; the dispatcher then returns a
  faulted task with the "owner-thread-only test dispatcher cannot marshal cross-thread UI work"
  message, `OpenCoreAsync`'s `catch` converts it to `RollbackAsync`, and the open resolves `false`. Net
  observable: no popup, no exception, one log line.

---

## Scope & Non-Goals

### In scope

Exactly two issues and six defect units: #488 D1-D5 and #475. Every fix lands in one of four owned
production files plus test files.

### Out of scope / non-goals — recorded with reasons, not absorbed

Three follow-up candidates, all recorded in the cross-check. Each is real and each is deliberately not
planned here.

1. **D1c — `ConfigureHost`'s generation guard drops the *incoming* host without disposing it**
   (`BreadcrumbItemViewerLifecycleCoordinator.cs:122-125`, research §3.6). Out of scope because it leaks
   the **incoming** host, which is a different defect from the one #488 D1 filed, and because adding a
   `Dispose()` to a branch no current test exercises would be an unpinned behaviour change inside a
   bugfix change-set. Promote to a new issue.
2. **`SetBridgeCoordinator` replaces without disposing while `Dispose():216` disposes**
   (`BreadcrumbItemViewerLifecycleCoordinator.cs:62-77`; `UnsubscribeBridge()` at `:306-317` detaches
   four handlers and disposes nothing). Out of scope because it stays **dormant under the fail-fast D3
   design** — see the explicit coupling recorded under D3 below. Promote to a new issue.
3. **`Reset()` detaches the collapsed surface synchronously but the popup surface via a posted lambda**
   (`BreadcrumbItemViewerLifecycleCoordinator.Reset()` at `:191-199` calls `DetachCollapsedMessenger()`
   synchronously at `:197`; the popup half reaches `_detachPopupMessenger()` inside
   `BreadcrumbDropDownOpenCoordinator.cs`'s posted lambda). Out of scope because the asynchronous half
   lives in a file owned by sibling feature `breadcrumb-coordinator-hub-defects-501` (#462); changing
   only the collapsed half does not fix a synchrony mismatch. Promote to a new issue naming both files.

Also out of scope: any widening of D1's fix to the 3-arg injected `ConfigureBreadcrumbDropDown`
overload (see the recorded limitation under D1); any re-initialization branch for D3; any change to
`IBreadcrumbDropDownHost`, `IItemViewer`, or `IQfcItemController`; and any opportunistic refactor of the
breadcrumb open/close path.

### Explicitly excluded files

These must not be written by this feature:

- `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs`,
  `QuickFiler/Viewers/BreadcrumbMessengerHub.cs`,
  `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs`,
  `QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs` — sibling feature 501 (#462, #500, #501,
  #502).
- `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` and the other `QfcItemController` partials —
  sibling feature 484 (#480, #481, #483, #484, #485).
- `QuickFiler/Viewers/ItemViewer.cs`, `ItemViewer.WebViewThread.cs`, `ItemViewer.FolderSearch.cs`,
  `ItemViewer.DisplayState.cs`, `ItemViewer.Commands.cs` — sibling feature 489 (#486, #487, #489, #490).
- `QuickFiler/Viewers/ItemViewer.Designer.cs` — **6224 lines**, designer-generated, already twelve times
  over the 500-line ceiling, and in sibling 489's diff. D5's design is chosen specifically to avoid it.
- `QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs` — **exactly 500 lines**. It is at the
  ceiling and cannot grow by one line.
- `QuickFiler/QuickFiler.csproj` — this feature adds no production file, so no edit is required.

`docs/features/active/breadcrumb-coordinator-hub-defects-501/spec.md:155-164` explicitly cedes this
feature's four owned production files to feature 488. Verified by direct read.

---

## Root Cause Analysis

### Corrections to the promoted potentials (binding)

The four rows below replace the corresponding claims in the promoted potentials. The corrected version
is what this feature implements and what a reviewer must check against.

| Unit | Potential claims | Corrected truth (verified at HEAD) |
| --- | --- | --- |
| **D1** | "The first host is never disposed by this file"; `BreadcrumbItemViewerLifecycleCoordinator.cs:127-142` "does not call `IBreadcrumbDropDownHost.Dispose()`" | **FALSE.** `BreadcrumbDropDownOpenCoordinator.Release()` (`:183-192`) calls `_host.Dispose()` at `:190`. The residual is **ordering, not omission**: the replacement host is constructed synchronously at `ItemViewer.Breadcrumb.cs:159` before any release is scheduled, and both hosts capture the *same* `() => BreadcrumbCoordinator?.CancelSelector()` closure (`:166`) and the same `FocusBreadcrumbCore` callback (`:165`), which resolve the **current** bridge coordinator at invocation time. A late dispose of an open outgoing host takes `DisposeCoreAsync`'s `if (OpenState && !_resetPending)` branch (`BreadcrumbDropDownHost.cs:303`) → `CompleteClose` → `FinishClose`, cancelling the **new** host's live selector session. Do not carry the potential's or the comment's three-item residual list; carry the ordering framing. |
| **D3** | "Pooled viewer reuse reaches this path" | **NOT REACHABLE IN PRODUCTION.** `QfcItemController.ViewerSetup.cs:143` guards `viewer.InitializeBreadcrumbPipeline(provider)` on `viewer.BreadcrumbCoordinator == null`. `BreadcrumbCoordinator` is nulled only by `DisposeBreadcrumbResources` (`ItemViewer.Breadcrumb.cs:316`), which runs only on component disposal; `ResetBreadcrumb()` does not clear it. So on a pooled reuse the second controller **never calls `InitializeBreadcrumbPipeline` at all**. The stale-provider *symptom* is real but originates upstream at `ViewerSetup.cs:143`, in a 484-owned file. **Fixing `:45` changes no production behaviour.** The second caller (`QfcItemController.FolderHandling.cs:176`) is behind the same guard and does not change this. |
| **D5** | Reachable "via the deferred `ConfigureHost` post racing `Control.Dispose`" | **WRONG MECHANISM.** `ConfigureHost`'s posted lambda (`:120-152`) never calls back into `EnsureBreadcrumbResourceOwnership`; it constructs an open coordinator and attaches a messenger. `EnsureBreadcrumbResourceOwnership` is reached only synchronously, from `EnsureBreadcrumbLifecycle` (`:283`), itself reached from `InitializeBreadcrumbPipeline` (`:50`) and both `ConfigureBreadcrumbDropDown` overloads (`:155`, `:191`). **The correct path:** `QfcItemController.InitializeWebViewAsync` is an `async Task` that awaits four times before reaching `EnsureBreadcrumbPipeline()` and `ConfigureBreadcrumbDropDown`; if the pooled viewer is disposed while that initialization is in flight, the continuation runs against a disposed `ItemViewer`, creates a fresh `Container`, and leaks everything hung off it. This path is more plausibly live than the filed one. |
| **#475** | Open question: "Confirm whether any production path legitimately runs without a synchronization context" | **ANSWERED: NO.** Sites 3 and 4 (`BreadcrumbDropDownHost.cs:98` in the `public` 7-param `LegacySurfaceFactory` ctor, `:118` in the `internal` 7-param `ReadySurfaceFactory` ctor) have **no in-repo production caller**; production constructs the host through the 8-param internal ctor, which receives `lifecycle.Operations` explicitly (`ItemViewer.Breadcrumb.cs:167`). Sites 1 and 2 (`ItemViewer.Breadcrumb.cs:156`, `:192`) are reached only via `QfcItemController.InitializeWebViewAsync`, which has **no `ConfigureAwait(false)` anywhere on the path**, so `SynchronizationContext.Current` is non-null at both sites in every production invocation. #475 is therefore **latent on production paths and live as a design defect** — a test-only affordance selected at runtime by probing ambient state, reachable from a `public` constructor. Restoring fail-fast carries no known production regression. |

### Confirmed mechanisms not corrected

D2 and D4 are confirmed exactly as filed. D2's split-theme symptom arises because
`BreadcrumbBridgeCoordinator.SetTheme` posts through its own dispatcher and therefore still lands on the
collapsed surface, while the popup surface is skipped.

### Affected components

`QuickFiler/Viewers/ItemViewer.Breadcrumb.cs`, `BreadcrumbItemViewerLifecycleCoordinator.cs`,
`BreadcrumbPopupUiOperations.cs`, `BreadcrumbDropDownHost.cs`.

---

## Proposed Fix

### Design summary (what changes where)

| Unit | Decision | Owned file | Approximate delta |
| --- | --- | --- | --- |
| D1 | Dispose the outgoing host in `ItemViewer.ConfigureBreadcrumbDropDown(env, initializer)` before the replacement is constructed | `ItemViewer.Breadcrumb.cs` | +4 |
| D2 | Retain the last theme on the lifecycle coordinator and replay it when a host is adopted | `BreadcrumbItemViewerLifecycleCoordinator.cs` | +6 |
| D3 | Fail fast on a different provider, matching `SetBridgeCoordinator`'s reference comparison | `ItemViewer.Breadcrumb.cs` | +8 |
| D4 | Declare and enforce UI-thread affinity against `ItemViewer.UiSyncContext` | `ItemViewer.Breadcrumb.cs` | +14 |
| D5 | Refuse creation during teardown: `if (IsDisposed \|\| Disposing) throw` | `ItemViewer.Breadcrumb.cs` | +4 |
| #475 | Delete `CaptureCurrentOrTests()`; point `BreadcrumbDropDownHost.cs:98`/`:118` at `CaptureCurrent()`; make `EnsureBreadcrumbLifecycle`'s operations argument lazy | `BreadcrumbPopupUiOperations.cs` (−4), `BreadcrumbDropDownHost.cs` (0), `ItemViewer.Breadcrumb.cs` (+3) | net −1 |

### D1 — dispose the outgoing host before constructing its replacement

**Decision.** In `ItemViewer.ConfigureBreadcrumbDropDown(CoreWebView2Environment, IWebViewCoreInitializer)`,
between the same-environment early return (`:147-153`) and the `EnsureBreadcrumbLifecycle` call at
`:155`, dispose the outgoing host when it is a concrete `BreadcrumbDropDownHost`.

**Rationale.**

- **Ordering is guaranteed by statement order**, on every thread, with no dispatcher reasoning required.
- **Exactly one *effective* disposal.** `Release()`'s later posted `_host.Dispose()` hits the
  `if (_disposed) return;` early-return at `BreadcrumbDropDownHost.cs:260-261` and is a no-op.
- **`BreadcrumbDropDownIntegrationTests.cs:308`'s `Times.Once()` stays green.** That assertion is on a
  `Mock<IBreadcrumbDropDownHost>`, which is not idempotent. The fix must therefore type-test for the
  **concrete** `BreadcrumbDropDownHost`, not for `IBreadcrumbDropDownHost`: a mock host installed by an
  earlier 3-arg `ConfigureBreadcrumbDropDown` call fails the type test and is not disposed here.
- **A fresh pattern variable is required.** The `is BreadcrumbDropDownHost existing` variable bound in
  the `:147-153` guard is definitely assigned only on the branch that returns, so a new pattern variable
  (`outgoing`) must be introduced.
- The same-environment early return still fires first, so
  `ConfigureBreadcrumbDropDown_RepeatedSameEnvironmentReusesPopupHost`
  (`QfcItemControllerBreadcrumbDropDownTests.cs:91-122`) is untouched.

**Rejected alternative, on ownership rather than merit.** Making
`BreadcrumbDropDownOpenCoordinator.Release()` synchronous is the cleaner fix — it has exactly one caller
(`BreadcrumbItemViewerLifecycleCoordinator.cs:302`), which already calls `DetachPopupMessenger()`
synchronously at `:301`, and `BreadcrumbDropDownHost.Dispose()` (`:258-265`) only sets `_disposed` and
hands teardown to `_openLifetime.DisposeAndSchedule`, so it is thread-safe to call from anywhere.
**It is out of bounds**: `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` is owned by sibling
feature `breadcrumb-coordinator-hub-defects-501` for #462. Two concurrent branches editing that file is
exactly the conflict the epic's file assignment exists to prevent.

**Second rejected alternative.** Disposing inside `ReleaseHostCore()` is in an owned file but produces
**two** `Dispose()` calls on the same host, breaking `BreadcrumbDropDownIntegrationTests.cs:308` for a
reason unrelated to the defect.

**Recorded limitations.**

- The fix covers the concrete environment-change path only. The 3-arg injected overload (`:179-195`) can
  also replace a host, but the outgoing host is not knowable there until inside the post, and that
  overload has no production caller. Recording the limitation is preferred to widening the fix.
- **D1b (unobservable dispose failure) is not fixed.** `_ = _operations.PostAsync(...)` discards the
  task and `BreadcrumbUiDispatcher.Dispatch` routes any exception to the error sink, so a host that
  fails to dispose leaves a WebView2-backed `ToolStripDropDown` alive with only a log line. That code
  lives in 501's file. Recorded as a known residual.

### D2 — retained-theme replay on the lifecycle coordinator

**Decision.** Add a `private string? _theme` field to `BreadcrumbItemViewerLifecycleCoordinator`, assign
it in `SetTheme` (`:155-160`) before the two forwarding calls, and replay it onto the newly adopted host
inside `ConfigureHost`'s post, in the **newly-adopted branch only** (after `:141`), guarded by
`!string.IsNullOrWhiteSpace(_theme)`.

**Rationale.**

- The guard is required: `BreadcrumbDropDownHost.SetTheme` throws `ArgumentException` on null or
  whitespace (`:243-244`).
- The `else` branch (same host, `UpdateRequestProviders` at `:145`) deliberately does **not** replay: the
  host already holds the theme, and replaying there would add a redundant `SetTheme` call that
  `Mock<IBreadcrumbDropDownHost>`-based tests could observe.

**The tempting one-line alternative is incorrect, not merely weaker.** "Post `SetTheme` on the same
queue and rely on FIFO" appears to order the `ConfigureHost` post and the `SetTheme` post on the same
`SynchronizationContext`. It only orders the case where both calls are issued from the same side of the
boundary. **If `ConfigureBreadcrumbDropDown` is issued off-boundary (posted) and `SetBreadcrumbTheme` is
issued *on* the UI thread, `SetTheme` runs inline and still precedes the configure post.** The defect
survives. Reject it on correctness.

**Reordering the caller is also unavailable.** `QfcItemController.ViewerSetup.cs:169-170` already calls
configure-then-theme; the problem is that host installation is posted while `SetTheme` is not. There is
nothing to reorder, and `ViewerSetup.cs` is 484-owned in any case.

### D3 — fail fast on a different provider

**Decision.** Add a `private IFolderHierarchyProvider _breadcrumbProvider` field to
`ItemViewer.Breadcrumb.cs`; replace the `:45-48` guard so that a non-null `BreadcrumbCoordinator` with a
reference-unequal provider throws `InvalidOperationException`, and returns otherwise; assign
`_breadcrumbProvider` alongside `:59`; and null it in `DisposeBreadcrumbResources` (`:312-317`) so a
re-created pipeline after disposal is not blocked by a stale reference.

**Rationale.**

- A new field is required because `BreadcrumbBridgeCoordinator` does not expose its provider — the
  constructor passes it straight into the router, and there is no `Provider` member.
- Reference equality is sufficient to make the discard **non-silent**, which is what the filed criterion
  asks for ("either fails fast or re-initializes explicitly"), and it makes the wrapper exactly as strict
  as the collaborator it wraps.
- Explicit re-initialization is **not** built: no production caller reaches the guard with a second
  provider, so a re-initialization branch would be unreachable code carrying real teardown risk.
- **This changes no production behaviour** (see the D3 correction row). State that plainly; do not expect
  or claim a user-visible repair.

**Load-bearing coupling — do not lose this.** The choice of **fail-fast** is what keeps the
`SetBridgeCoordinator` replace-without-dispose defect (Out of Scope item 2) dormant and therefore out of
scope. Under fail-fast, `InitializeBreadcrumbPipeline` never constructs a second
`BreadcrumbBridgeCoordinator`, so nothing new ever reaches `SetBridgeCoordinator`'s replacement branch.
**If this spec were amended to adopt explicit re-initialization instead, that defect becomes live and
MUST be pulled into scope in the same change-set.** The scope decision in
`research/2026-08-25T10-20-orchestrator-comment-crosscheck.md` § "Claim 2" is explicitly contingent on
this.

### D4 — declare and enforce UI-thread affinity

**Decision.** Add a private `ThrowIfOffUiBoundary(string operation)` helper to `ItemViewer.Breadcrumb.cs`
that throws `InvalidOperationException` when `UiSyncContext` is non-null and
`SynchronizationContext.Current` is not reference-equal to it. Call it as the first statement of
`InitializeBreadcrumbPipeline(provider, operations)` (`:40-60`), both `ConfigureBreadcrumbDropDown`
overloads (`:142-177`, `:179-195`), and `EnsureBreadcrumbResourceOwnership` (`:300-310`).

**Rationale.**

- **Reference equality against `UiSyncContext`, not managed thread identity.** `UiSyncContext`
  (`ItemViewer.cs:60-63`, backed by `_context` assigned at `:26`) is the context captured in the
  `ItemViewer` constructor, and the comparison matches `BreadcrumbUiDispatcher`'s own boundary rule. A
  `ConfigureAwait(false)` continuation can land on a recycled pool thread whose id matches, so bare
  thread identity is not a boundary proof.
- The `UiSyncContext != null` escape keeps a viewer constructed without an ambient context (a test shape)
  from throwing.
- Atomic initialization is rejected: it would need `Interlocked.CompareExchange` or a lock on three
  fields plus a disposal path for the loser of each race, and **it does not solve the underlying
  problem** — `ItemViewer` is a `UserControl`, `components` is WinForms state, and the breadcrumb anchor
  is a `Control`. Making the fields atomic would legitimise off-thread access to control state that is
  not thread-safe at all.
- **Explicit limitation.** This **declares and enforces** the contract; it does not make the
  read-then-write atomic. A caller that violates the contract now receives a diagnostic instead of a
  silent leak. The filed criterion permits exactly this ("Pipeline initialization is atomic, **or**
  UI-thread affinity is declared and enforced").

### D5 — refuse creation during teardown

**Decision.** Make `if (IsDisposed || Disposing) { throw new ObjectDisposedException(nameof(ItemViewer)); }`
the first statement of `EnsureBreadcrumbResourceOwnership` (`:300`).

**Rationale.**

- `Control.IsDisposed` and `Control.Disposing` are both public WinForms properties, so no new state is
  needed. `Disposing` covers the window *during* `Dispose(bool)`, which `IsDisposed` alone does not.
- The alternative — disposing a late-created `Container` — would require editing
  `ItemViewer.Designer.cs` (6224 lines, designer-generated, in sibling 489's diff) or adding a second
  disposal path with its own re-entrancy problem.
- Fail-fast is the repository default (`CLAUDE.md` § "Error Handling"; `.claude/rules/general-code-change.md`
  § "Error Handling and Logging"). A silent early return would leave `BreadcrumbCoordinator` null and
  degrade `AttachBreadcrumbWebViewAsync` to a `false` return with no diagnostic — the same class of
  silent degradation #475 exists to remove.

**Open item that must be discharged before the throw is adopted (research §3.5).** The throw propagates
out of `QfcItemController.EnsureBreadcrumbPipeline` (`ViewerSetup.cs:136-161`, itself
`[ExcludeFromCodeCoverage]` at `:135`) and faults `InitializeWebViewAsync`'s task. **Confirm that a
faulted `InitializeWebViewAsync` task is observed by its caller** and does not become an unobserved
`TaskException`. If it is not observed, the correct response is a new issue against `ViewerSetup.cs`
(484-owned), **not** a weakening of this guard. This is a blocking acceptance criterion below.

### #475 — delete the ambient-probing selector, in three parts

All three parts are **required** and must land as one change-set.

1. **Delete `BreadcrumbPopupUiOperations.CaptureCurrentOrTests()` (`:86-89`).** Keep
   `BreadcrumbPopupUiOperations.CreateForCurrentThreadTests()` (`:83-84`) and
   `BreadcrumbUiDispatcher.CreateForCurrentThreadTests()` (`:62-65`): seven tests call them directly, and
   injecting a test dispatcher explicitly is precisely the discipline #475 asks for. Only the
   *ambient-probing selector* is removed.
2. **Point `BreadcrumbDropDownHost.cs:98` and `:118` at `CaptureCurrent()`.** No production caller
   reaches either. **Do not reorder the constructor arguments:** at `:91-93` the
   `surfaceFactory ?? throw` inside `NormalizeFactory` is evaluated **before** the operations argument at
   `:98`, which is why `BreadcrumbDropDownIntegrationTests.cs:21-39`
   (`Constructor_NullLegacySurfaceFactory_ThrowsForSurfaceFactory`) passes today without an ambient
   context.
3. **Make `EnsureBreadcrumbLifecycle`'s operations argument lazy — a `Func<BreadcrumbPopupUiOperations>`,
   three call sites.** This is **required, not opportunistic.** `ItemViewer.Breadcrumb.cs:156` and `:192`
   currently pass the operations object as an **eagerly evaluated argument** to `EnsureBreadcrumbLifecycle`
   (`:274-298`), which **discards it whenever `_breadcrumbLifecycleCoordinator` is already non-null**
   (`:278-281`). A bare swap to `CaptureCurrent()` would therefore make a pure no-op call throw on any
   thread without a context — for example `BreadcrumbSelectorOpenRetryTests.cs:264`, which calls the
   3-arg `ConfigureBreadcrumbDropDown` on a viewer whose lifecycle was already seeded with injected
   operations. Laziness is what makes #475's "no test loses its seam" claim actually true. The three call
   sites become `EnsureBreadcrumbLifecycle(() => operations)` at `:50` and
   `EnsureBreadcrumbLifecycle(BreadcrumbPopupUiOperations.CaptureCurrent)` (method group) at `:155` and
   `:191`; the parameter is evaluated once, after the early return.

The injectable constructor `BreadcrumbPopupUiOperations.cs:62-78` is confirmed present and unchanged and
remains the seam used by eight existing test files.

### Boundaries and invariants to preserve

1. **No public API change.** `IBreadcrumbDropDownHost`, `IItemViewer`, and `IQfcItemController` are
   unmodified. `BreadcrumbDropDownHost`'s `public` 7-param constructor keeps its signature; only the
   operations argument it forwards changes.
2. **`BreadcrumbDropDownIntegrationTests.cs:308`'s `Times.Once()` on `host.Dispose()` remains true**, and
   the file remains at exactly 500 lines, unmodified.
3. **The same-environment early return at `:147-153` still fires before any new statement.**
4. **`ResetBreadcrumb()` semantics are unchanged.** `Reset()` (`:191-199`) is not edited.
5. **`ConfigureHost`'s `else` branch is not made to replay the theme** (see D2).
6. **No new `[ExcludeFromCodeCoverage]` attribute is introduced anywhere.**

### Interaction between the fixes (must be understood before review)

Two cross-defect interactions are load-bearing:

- **D4 closes the window D1 and D2 leave open.** Once `ThrowIfOffUiBoundary` guards both
  `ConfigureBreadcrumbDropDown` overloads, an off-boundary configure is rejected at the entry point, so
  the deferred-post window that makes D1a and D2 harmful is no longer reachable **through `ItemViewer`'s
  own surface**. D1 and D2 remain necessary because the window is still reachable by driving
  `BreadcrumbItemViewerLifecycleCoordinator` directly (which the regression tests do) and by a viewer
  constructed with a null `UiSyncContext`.
- **D1 introduces a narrow, accepted `ObjectDisposedException` residual.** After D1's fix the outgoing
  host is disposed while `_openCoordinator` still points at it, until `ConfigureHost`'s post runs
  `ReleaseHostCore()`. A `SetTheme` landing inside that window reaches
  `BreadcrumbDropDownHost.SetTheme` → `ThrowIfDisposed()` (`:245`) and throws instead of silently
  theming the wrong host. This is accepted, not fixed: it is a diagnostic on a contract violation that
  D4 now rejects outright, D2's retained theme still reaches the newly adopted host, and the window does
  not exist on the production UI thread where every post runs inline. It must be recorded in the change
  description rather than discovered at review.

### Files/modules to change

**Production (four owned files, all within the 500-line ceiling after the change):**

| File | Pre-change lines | Headroom | Expected delta | Post-change estimate |
| --- | --- | --- | --- | --- |
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | 319 | 181 | +33 (D1 +4, D3 +8, D4 +14, D5 +4, #475 +3) | ~352 |
| `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` | 481 | **19** | +6 (D2) | ~487 |
| `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` | 494 | **6** | −4 (#475 deletion) | ~490 |
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` | 463 | 37 | 0 (two identifier swaps) | 463 |

Line counts verified by direct measurement on 2026-08-25. `BreadcrumbItemViewerLifecycleCoordinator.cs`
and `BreadcrumbPopupUiOperations.cs` are the two constrained files: **no edit other than the one named
above may target either.**

**Test:**

| File | Pre-change lines | Disposition |
| --- | --- | --- |
| `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs` | 480 | Delete `CaptureCurrentOrTests_NullAndControlledContexts_SelectExpectedBoundaries` (`:170-195`, 26 lines) and replace it with the fail-fast test. Net change must not push the file over 500. |
| `QuickFiler.Test/Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs` | 382 | Home for the D2 coordinator-level regression test. `RecordingHost.SetTheme` (`:344`) and `Dispose` (`:348`) are empty and are the natural recorders to extend. |
| `QuickFiler.Test/Viewers/ItemViewerBreadcrumbLifecycleRegressionTests.cs` | **new** | Home for the viewer-level regression tests (D1, D3, D4, D5, and #475's seam-preservation guard). |
| `QuickFiler.Test/QuickFiler.Test.csproj` | — | **Exactly one** added `<Compile Include="Viewers\ItemViewerBreadcrumbLifecycleRegressionTests.cs" />` line, and no other change. |

### Functions/classes impacted

`ItemViewer.InitializeBreadcrumbPipeline` (both overloads), `ItemViewer.ConfigureBreadcrumbDropDown`
(both overloads), `ItemViewer.EnsureBreadcrumbLifecycle`, `ItemViewer.EnsureBreadcrumbResourceOwnership`,
`ItemViewer.DisposeBreadcrumbResources`, a new private `ItemViewer.ThrowIfOffUiBoundary`;
`BreadcrumbItemViewerLifecycleCoordinator.SetTheme` and `.ConfigureHost`;
`BreadcrumbPopupUiOperations.CaptureCurrentOrTests` (deleted); two `BreadcrumbDropDownHost` constructor
chains.

### Error handling and logging updates

Three new failure modes, all fail-fast and all diagnostic rather than logged:

- `InvalidOperationException` — a second, reference-unequal `IFolderHierarchyProvider` (D3).
- `InvalidOperationException` — a guarded breadcrumb entry point called off the owning UI
  synchronization context (D4). The message must name the operation.
- `ObjectDisposedException` — breadcrumb resource ownership requested during or after teardown (D5).

`InvalidOperationException` from `BreadcrumbUiDispatcher.CaptureCurrent()` is **restored**, not added:
#475's deletion stops suppressing a throw the production contract already defined. No new logging
statement is introduced; no existing log statement is removed.

### Rollback / feature-flag considerations

None. No configuration key, no feature flag, no migration. Rollback is `git revert` of the change-set.

### Backward-compatibility expectations

No public member is added, removed, or re-signed. The one publicly observable behaviour change is that
`BreadcrumbDropDownHost`'s `public` 7-param constructor now throws `InvalidOperationException` when
constructed without an ambient `SynchronizationContext`, which is the fail-fast contract
`BreadcrumbUiDispatcher.CaptureCurrent()` already documents and which no in-repo production caller
reaches.

---

## Dependencies on 489

Sibling feature `itemviewer-surface-defects-489` (issues #486, #487, #489, #490) is being prepared
**concurrently** and is **not present on this branch**; its spec is not final.

**None of the six items below is a contract this feature consumes.** Every recommended change is confined
to `ItemViewer.Breadcrumb.cs`, `BreadcrumbItemViewerLifecycleCoordinator.cs`,
`BreadcrumbPopupUiOperations.cs`, and `BreadcrumbDropDownHost.cs`, none of which is on 489's surface.
Each row is an assumption of the form "489 does not remove or rename something this feature reads", and
each is one targeted re-check once 489's spec is final.

| # | File | Member / contract | What this feature assumes |
| --- | --- | --- | --- |
| D489-1 | `QuickFiler/Viewers/ItemViewer.cs` | `public SynchronizationContext UiSyncContext { get; }` (`:60-63`), backed by `_context` assigned in the constructor at `:26` | That it still exists and still returns the context captured at construction. D4's `ThrowIfOffUiBoundary` uses it as the boundary proof. 489's #489 Defect 4 proposes consolidating `UiSyncContext` / `UiScheduler` (`:66-69`) / `UiDispatcher` onto **one** seam; if `UiSyncContext` is the survivor there is no impact, otherwise the guard must be re-pointed at the survivor. |
| D489-2 | `QuickFiler/Viewers/ItemViewer.cs` | `[ExcludeFromCodeCoverage]` on the `ItemViewer` partial type declaration at `:20` | That it is not removed. It exempts every member of `ItemViewer.Breadcrumb.cs` from coverage measurement. Removing it would put roughly 350 lines into the coverage denominator and change this feature's coverage target. |
| D489-3 | `QuickFiler/Viewers/ItemViewer.Designer.cs` | `protected override void Dispose(bool disposing)` at `:16-23` — disposes `components` only when non-null | That its shape is unchanged. It is the entire basis of D5. 489's #487 Defect 1 proposes deleting the `L0v2h2_WebView2_ParentChanged` handler and its designer wiring in this same file; that edit does not touch `Dispose(bool)`, but it does put the file in 489's diff. This feature deliberately does **not** edit it. |
| **D489-4** | `QuickFiler/Viewers/ItemViewer.*.cs` (all partials of the `ItemViewer` type) | Member-name uniqueness across the partial type | That 489 introduces no member named `ThrowIfOffUiBoundary` or `_breadcrumbProvider` at type scope. 489 owns `ItemViewer.cs`, `ItemViewer.WebViewThread.cs`, `ItemViewer.FolderSearch.cs`, `ItemViewer.DisplayState.cs`, `ItemViewer.Commands.cs`; this feature owns `ItemViewer.Breadcrumb.cs`. **A name collision across partials is a compile error at integration, not a merge conflict.** Git will merge both files cleanly and the build will then fail. It surfaces only at integration, so it must be checked deliberately rather than relied on to appear at fan-in. |
| D489-5 | `QuickFiler.Test/QuickFiler.Test.csproj` | `<Compile Include=...>` item-group content | That 489 adds no entry with the same file name as this feature's `Viewers\ItemViewerBreadcrumbLifecycleRegressionTests.cs`. Adjacent entries are expected and produce at worst an ordinary textual merge conflict; a same-name entry is a duplicate-compile-item build error. |
| D489-6 | `QuickFiler/Viewers/IItemViewer.cs` | No change to the interface | 489's #490 Defect 1 proposes renaming or re-specifying `SetFolderItems`. This feature does not call `SetFolderItems`, but `BreadcrumbDropDownIntegrationTests` and `BreadcrumbSubfolderActivationTests` — which this feature must keep green — do. If 489 changes that member's semantics, those tests change under 489, not here. |

There is no case in which this feature must wait on 489's spec to be finalised.

---

## Assumptions, Constraints, Dependencies

### Assumptions

- Worktree HEAD is `988e819b` or a descendant that does not modify the four owned files. Every citation
  in this document was verified against that tree.
- `docs/features/active/breadcrumb-coordinator-hub-defects-501/spec.md:155-164` continues to cede this
  feature's four production files. Verified on 2026-08-25.
- `QuickFiler.Test` builds and runs without a live Outlook process and without a live WebView2 runtime
  for every test this feature adds.

### Constraints

1. **500-line ceiling** on every production and test file this feature writes
   (`.claude/rules/general-code-change.md` § "File Size Limit"). Two owned files have single-digit or
   sub-twenty-line headroom; see the table above.
2. **`BreadcrumbDropDownIntegrationTests.cs` is at exactly 500 lines and cannot grow.** New viewer-level
   tests require a new file plus one `<Compile Include>` line.
3. **The `QuickFiler.Test.csproj` `Compile Include` item group is NOT alphabetically ordered.** It is
   ordered by area and insertion history — for example `Viewers\ItemViewerBreadcrumbDropDownContractTests.cs`
   sits between `Viewers\BreadcrumbDropDownLifecycleTests.cs` and
   `Viewers\BreadcrumbDropDownOpenCoordinatorTests.cs`. Insert the new entry adjacent to that existing
   `Viewers\ItemViewer*` entry. Do not attempt to "restore" alphabetical order, and do not edit either
   `.csproj` outside this one added line.
4. **MSTest + Moq + FluentAssertions only.** No xUnit, no NUnit.
5. **No temporary files in tests. No `Thread.Sleep`, no `Task.Delay`, no wall-clock wait**
   (`.claude/rules/general-unit-test.md` § "Determinism Infrastructure").
6. **No edit to any file listed under "Explicitly excluded files".**
7. Neutral, factual tone in all authored content (`.claude/rules/tonality.md`).

### External dependencies

None. No new NuGet package, no new project reference, no service.

---

## Data / API / Config Impact

- **User-facing or API changes:** none. No public member added, removed, or re-signed.
- **Data or migration considerations:** none.
- **Logging/telemetry updates:** none. Three new fail-fast exceptions replace three silent paths; no log
  statement is added or removed.
- **Compatibility notes:** no CLI flag, config schema, or version change.

---

## Test Strategy

### Governing constraints

Per the CLAUDE.md Bugfix Workflow, **a failing regression test comes FIRST for each defect unit**, then
the minimal targeted fix. Evidence must record each regression test failing against the unfixed code
before the corresponding production change lands. All evidence artifacts are written to
`docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/evidence/<kind>/`.

### Reusable harnesses already in the tree — use these, do not invent new ones

| Harness | Location | What it gives | Reusable from a new file? |
| --- | --- | --- | --- |
| `QueuedCreatorThreadSynchronizationContext` | `BreadcrumbItemViewerLifecycleCoordinatorTests.cs:354-380` | A queue that runs only on explicit `DrainOnCreatorThread()`. Makes "posted but not yet run" a first-class deterministic state. | `private` — same file only |
| `LifecycleFixture` | same file, `:259-292` | A fully wired `BreadcrumbItemViewerLifecycleCoordinator` over that queue | `private` — same file only |
| `RecordingHost` | same file, `:294-352` | A hand-written `IBreadcrumbDropDownHost`; `SetTheme` (`:344`) and `Dispose` (`:348`) are empty | `private` — same file only |
| `InvokeAmbientNull<T>` | `BreadcrumbSelectorToggleUiBoundaryTests.cs:325-337` | Runs a delegate with `SynchronizationContext.Current` nulled, **on the same thread, no second thread, no timing** | **`internal static` on a `public sealed` class — reusable** |
| `ViewerScope` | `QfcItemControllerBreadcrumbDropDownTests.cs:365-383` | A real `ItemViewer` under a plain ambient context, disposed deterministically | `private` — the new file must define its own equivalent |
| `Host(viewer)` reflection helper | same file, `:335-345` | Reads the non-public `BreadcrumbDropDownHost` property | `private` — the new file must define its own equivalent |
| Uninitialized environment | `FormatterServices.GetUninitializedObject(typeof(CoreWebView2Environment))` | A `CoreWebView2Environment` identity token with no SDK call | Pattern, freely reusable |

The new test file must define its own viewer scope and its own reflection accessor rather than widening
the accessibility of another feature's test helpers.

### Regression tests to add, by defect unit

- **D1 — deterministic.** New file. Two `FormatterServices`-produced environments, a strict
  `Mock<IWebViewCoreInitializer>` with no setups, and a **drainable** synchronization context so that
  "posted but not drained" is observable (under an inline context the outgoing host is disposed anyway
  and the test would pass before the fix). Configure with `env1`, capture the host through the
  reflection accessor, configure with `env2`, then assert the first host is disposed. **Observation
  point:** `host1.DropDown.IsDisposed` — `DropDown` is a public property
  (`BreadcrumbDropDownHost.cs:182`) and `DisposeCoreAsync` calls `DropDown.Dispose` at `:321`. Secondary
  assertion: `host1.Close(reason)` returns `false` (`:230-231`).
- **D2 — deterministic, the cleanest of the six.** `BreadcrumbItemViewerLifecycleCoordinatorTests.cs`.
  Extend `RecordingHost` with a `List<string> ThemesApplied` recorder in `SetTheme`. Arrange:
  `Coordinator.ConfigureHost(host, ...)` with the post **queued and not drained**. Act:
  `Coordinator.SetTheme("dark")`, then `Queue.DrainOnCreatorThread()`. Assert:
  `host.ThemesApplied.Should().Equal("dark")`. Red today, because `DropDownHost` is null when `SetTheme`
  runs. No threads, no timing.
- **D3 — deterministic.** New file. Two distinct `Mock<IFolderHierarchyProvider>(MockBehavior.Strict)`.
  `InitializeBreadcrumbPipeline(p1)`, then
  `Action act = () => viewer.InitializeBreadcrumbPipeline(p2)` → `act.Should().Throw<InvalidOperationException>()`.
  Companion positive case: re-calling with `p1` `.Should().NotThrow()` and `viewer.BreadcrumbCoordinator`
  unchanged (`BeSameAs`).
- **D4 — a true data race CANNOT be reproduced deterministically.** Two threads with no barrier, and the
  repository bans `Thread.Sleep`, `Task.Delay`, and wall-clock waits, so there is no way to force the
  interleaving. **The deterministic proxy asserts the declared contract, not the race:** construct the
  viewer under context A, then invoke `InitializeBreadcrumbPipeline` inside
  `BreadcrumbSelectorToggleUiBoundaryTests.InvokeAmbientNull` — **same thread, ambient context nulled, no
  timing** — and assert `InvalidOperationException`. A second case using a *different non-null* context
  proves the comparison is reference equality rather than a null check. **This proves the guard fires. It
  does not prove the race is gone,** and no criterion below claims otherwise.
- **D5 — deterministic.** New file. `viewer.Dispose()`, then
  `Action act = () => viewer.InitializeBreadcrumbPipeline(provider)` →
  `act.Should().Throw<ObjectDisposedException>()`, plus `viewer.BreadcrumbCoordinator.Should().BeNull()`
  to pin that no pipeline is built against a dead viewer. Pre-fix the call succeeds.
- **#475 — two tests.** (1) In `BreadcrumbPopupBoundaryCoverageTests.Part2.cs`, replace the deleted test
  with a fail-fast assertion using that file's existing `WithContext` helper:
  `WithContext(null, BreadcrumbPopupUiOperations.CaptureCurrent)` →
  `.Should().Throw<InvalidOperationException>()`, retaining the controlled-context half (currently
  `:184-194`) against `CaptureCurrent`. (2) In the new file, a seam-preservation guard: on a viewer whose
  lifecycle was already seeded via `InitializeBreadcrumbPipeline(provider, operations)`, call the 3-arg
  `ConfigureBreadcrumbDropDown(host, ...)` inside `InvokeAmbientNull` and assert `.Should().NotThrow()`.
  Test (2) is red before the laziness change and green after — but only if all three #475 parts land as
  one change-set.

### Existing tests that must change

Exactly one, and it is the only mandatory test edit in the whole change-set:

- `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs:170-195` —
  `CaptureCurrentOrTests_NullAndControlledContexts_SelectExpectedBoundaries`. It calls the deleted method
  at `:178` and `:186` and asserts the silent fallback. **Delete and replace** as described above. No
  other test in the repository references `CaptureCurrentOrTests`.

### Existing tests that constrain the fix and must be re-run and re-reasoned (no edit expected)

| Test | Location | Constraint it imposes |
| --- | --- | --- |
| `ItemViewerDisposal_OwnsHostAndDetachesBothSurfaces` | `BreadcrumbDropDownIntegrationTests.cs:296-312` | `host.Dispose()` `Times.Once()` on viewer disposal. This assertion is what rules out both rejected D1 alternatives. |
| `ConfigureAndAttachBreadcrumbAsync_CachesCurrentThemeAndCreatesOneCandidatePerSession` | `QfcItemControllerBreadcrumbDropDownTests.cs:187-262`, assertion at `:257-259` | "Assert no stale pooled theme is replayed." **The highest-risk interaction with D2's retained theme.** The retained value is overwritten by `SetBreadcrumbTheme("light")` before the re-attach, so the replay should carry `"light"` and the test should stay green — but this must be re-run explicitly and the reasoning recorded in evidence. |
| `ConfigureBreadcrumbDropDown_RepeatedSameEnvironmentReusesPopupHost` | `QfcItemControllerBreadcrumbDropDownTests.cs:91-122` | Same-environment configure must reuse the host and dispose nothing. Pins the `:147-153` early return. |
| `ConfigureBreadcrumbDropDown_PassesExistingEnvironmentAndDarkThemeLazily` / `..._LightThemeUsesSameControllerSetupSeam` | `QfcItemControllerBreadcrumbDropDownTests.cs:24-58`, `:60-89` | `Theme == "dark"` / `"light"` and `ControlHost == null` immediately after configure+theme. D2's replay must remain additive. |
| `HostReplacement_SubscriptionsAndMessengerReplacementPreserveOrder` | `BreadcrumbItemViewerLifecycleCoordinatorTests.cs:17-49` | Subscription order `add`/`remove` across a host swap. Stays green because `RecordingHost.Dispose` is empty; it is also the natural place for a coordinator-level D1 assertion. |
| `ResetAndPooledReuse_DetachPopupAndDoNotDuplicateCallbacks` | `BreadcrumbDropDownIntegrationTests.cs:226-261` | Reset-then-reconfigure with the **same** host must take the `UpdateRequestProviders` branch. |
| `SetBridgeCoordinator_SameReference_DoesNotDuplicateSubscriptions` | `BreadcrumbItemViewerLifecycleCoordinatorTests.cs:134-151` | The reference-comparison precedent D3 mirrors. |
| `DisposedCoordinator_SetBridgeCoordinatorThrows` | same file, `:203-216` | The `ObjectDisposedException` precedent D5 mirrors. |
| `Constructor_NullLegacySurfaceFactory_ThrowsForSurfaceFactory` | `BreadcrumbDropDownIntegrationTests.cs:21-39` | Passes today without an ambient context because the `surfaceFactory ?? throw` is evaluated before the operations argument. #475 part 2 must not reorder those arguments. |

### Tests that must be re-verified against D4's affinity guard

Every test that constructs an `ItemViewer` and then calls a guarded member. The guard compares
`SynchronizationContext.Current` against the context captured in the `ItemViewer` **constructor**, so a
test that constructs the viewer *before* installing its context would break.

| Harness | Context installed at | Viewer constructed at | Status |
| --- | --- | --- | --- |
| `ItemViewerDropDownHarness` | `BreadcrumbDropDownIntegrationTests.cs:337` | `:338` | Verified — correct order |
| `ViewerScope` | `QfcItemControllerBreadcrumbDropDownTests.cs:372` | `:373` | Verified — correct order |
| `SelectorOpenHarness` | `BreadcrumbSelectorOpenRetryTests.cs:254` | `:255` | Verified — correct order |
| `SubfolderActivationHarness` | `BreadcrumbSubfolderActivationTests.cs:274-276` | `:305` | Verified — correct order |
| `ViewerScope` (readiness) | `BreadcrumbCollapsedSurfaceReadinessTests.cs` near `:404-415` | — | **Must be confirmed during implementation** |
| pending open/close scope | `BreadcrumbPendingOpenCloseTests.cs`, used at `:160-189` | — | **Must be confirmed during implementation** |
| lifecycle scope | `BreadcrumbCoordinatorLifecycleTests.cs:26-34`, used at `:122` | — | **Must be confirmed during implementation** |

### Coverage impact and targets — read this before treating flat coverage as a testing gap

`QuickFiler/Viewers/ItemViewer.cs:20` carries `[ExcludeFromCodeCoverage]` on the `ItemViewer` partial
**type** declaration. A type-level attribute on one part applies to the whole partial type, so **every
member of `ItemViewer.Breadcrumb.cs` is already excluded from coverage measurement.** Four of the six
defect fixes (D1, D3, D4, D5) and one third of #475's fix land in that file.

**Consequence:** the regression tests for those units are required by the Bugfix Workflow and by the
acceptance criteria below, but they **move no coverage number**. A reviewer must not read flat coverage
on this feature as evidence of a testing gap, and must not "fix" it by removing the exemption — that is
D489-2's assumption and a 489-owned file.

Fixes placed in `BreadcrumbItemViewerLifecycleCoordinator.cs` (D2), `BreadcrumbPopupUiOperations.cs`
(#475 part 1) and `BreadcrumbDropDownHost.cs` (#475 part 2) **are** measured.

Targets: repository-wide line coverage `>= 80%` against the testable denominator defined in CLAUDE.md
§ UT2 (COM/VSTO/WinForms/Outlook-Interop exemptions); `>= 90%` for each new measured production member;
and no reduction in coverage for the changed lines relative to the Phase 0 baseline. **No baseline has
been captured for this feature yet**, so the repository-wide figure is a record-and-report obligation
inside the criterion below: the raw uninstrumented figure must be recorded alongside the
testable-denominator figure, and the blocking condition is that this change does not lower it.

### Toolchain commands to run (format → lint → type-check → test)

Bootstrap, required once in this worktree:

1. `nuget restore TaskMaster.sln` — mandatory. The `.csproj` files import `..\packages\...\*.props`
   conditionally; without restore, the analyzer, MSTest adapter, and coverage props silently do not
   import and the build produces a weaker diagnostic set.
2. `dotnet tool restore` — mandatory before the first `dotnet tool run csharpier` invocation.

Then the four-stage loop, restarting from stage 1 on any failure or auto-fix:

1. `dotnet tool run csharpier format .` (verify with `dotnet tool run csharpier check .`)
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation`

Notes on stage 3: do **not** add `/p:Nullable=enable`. That property is absent from
`.github/workflows/ci.yml` deliberately; adding it conscripts every file that has never adopted the
`#nullable enable` pragma. Notes on stage 4: `/InIsolation` matches CI and is load-bearing; a run started
from the repository root must exclude `\.claude\worktrees\` from assembly discovery, or it will pick up
sibling agent worktrees.

### Baseline requirement

Because this feature changes behaviour, the pre-change pass/fail counts for `QuickFiler.Test` and the
pre-change coverage figures must be recorded **before any production edit**, so that every tightened or
accommodated assertion is attributable. Baseline evidence goes to
`docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/evidence/baseline/`.

### Manual validation steps

None required. All six defect units carry an automated deterministic regression test, with D4's proxy
scoped as stated above.

---

## Acceptance Criteria

Fifty-four criteria, distributed as: Process 2, D1 6, D2 5, D3 6, D4 6, D5 4, #475 7, scope and
ownership 6, file-size/toolchain/coverage/document-integrity 12. Every one is verifiable and able to
fail. Where a criterion touches a file this feature does not own, it is expressed as a comparison
against the Phase 0 baseline rather than as an absolute count.

### Process (applies to every defect unit)

- [ ] Phase 0 evidence records the pre-change `QuickFiler.Test` pass/fail counts and the pre-change
      coverage figures, captured before any production file is edited, under
      `docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/evidence/baseline/`.
- [ ] For each of the six defect units (D1, D2, D3, D4, D5, #475), evidence records the unit's regression
      test **failing against the unfixed code** before the corresponding production change lands, per the
      CLAUDE.md Bugfix Workflow.

### D1 — host replacement on WebView2 environment change

- [x] `ItemViewer.ConfigureBreadcrumbDropDown(CoreWebView2Environment, IWebViewCoreInitializer)` disposes
      the outgoing host **between** the same-environment early return and the construction of the
      replacement, so that the ordering is guaranteed by statement order and not by dispatcher behaviour.
- [x] The disposal is guarded by a type test for the **concrete** `BreadcrumbDropDownHost` (not for
      `IBreadcrumbDropDownHost`), using a pattern variable distinct from the one bound in the
      same-environment guard.
- [x] A regression test configures with environment A, captures the host, configures with environment B
      under a **drainable** (non-inline) synchronization context, and asserts the first host is disposed —
      observing `host1.DropDown.IsDisposed` and, as a secondary assertion, that
      `host1.Close(reason)` returns `false`.
- [x] `BreadcrumbDropDownIntegrationTests.ItemViewerDisposal_OwnsHostAndDetachesBothSurfaces` passes
      unmodified, with its `host.Dispose()` `Times.Once()` assertion intact, and
      `QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs` is byte-identical to its pre-change
      state.
- [x] `QfcItemControllerBreadcrumbDropDownTests.ConfigureBreadcrumbDropDown_RepeatedSameEnvironmentReusesPopupHost`
      passes unmodified.
- [x] The spec's recorded D1 limitations are honoured in the delivered source: the 3-arg injected
      `ConfigureBreadcrumbDropDown` overload is **not** given an equivalent disposal, and
      `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` is unmodified.

### D2 — `SetBreadcrumbTheme` lost when the host post is deferred

- [x] `BreadcrumbItemViewerLifecycleCoordinator` carries a private retained-theme field that `SetTheme`
      assigns before forwarding to the bridge coordinator and the drop-down host.
- [x] `ConfigureHost`'s posted lambda replays the retained theme onto the host **in the newly-adopted
      branch only**, guarded against null or whitespace. The `UpdateRequestProviders` branch performs no
      `SetTheme` call.
- [x] A regression test in `BreadcrumbItemViewerLifecycleCoordinatorTests.cs` queues `ConfigureHost`
      without draining, calls `SetTheme("dark")`, drains, and asserts the recording host received exactly
      `"dark"`. The test contains no second thread, no `Thread.Sleep`, no `Task.Delay`, and no wall-clock
      wait.
- [x] `QfcItemControllerBreadcrumbDropDownTests.ConfigureAndAttachBreadcrumbAsync_CachesCurrentThemeAndCreatesOneCandidatePerSession`
      passes unmodified, and evidence records the explicit reasoning for why its "no stale pooled theme is
      replayed" assertion survives the retained-theme replay.
- [x] `QfcItemControllerBreadcrumbDropDownTests.ConfigureBreadcrumbDropDown_PassesExistingEnvironmentAndDarkThemeLazily`
      and `..._LightThemeUsesSameControllerSetupSeam` both pass unmodified.

### D3 — a second, different `IFolderHierarchyProvider`

- [x] `InitializeBreadcrumbPipeline` throws `InvalidOperationException` when `BreadcrumbCoordinator` is
      non-null and the supplied provider is not reference-equal to the retained one, and returns without
      effect when it is reference-equal — matching `BreadcrumbItemViewerLifecycleCoordinator.SetBridgeCoordinator`'s
      reference comparison.
- [x] The retained provider is stored in a private `ItemViewer` field assigned on the successful
      initialization path, and is nulled by `DisposeBreadcrumbResources` so a pipeline re-created after
      disposal is not blocked by a stale reference.
- [x] Two regression tests exist: a negative case asserting `InvalidOperationException` for a second,
      distinct `Mock<IFolderHierarchyProvider>(MockBehavior.Strict)`, and a positive case asserting
      `NotThrow` and an unchanged `BreadcrumbCoordinator` (`BeSameAs`) for a repeat call with the same
      provider.
- [x] No re-initialization branch is added. `BreadcrumbBridgeCoordinator`,
      `BreadcrumbItemViewerLifecycleCoordinator.SetBridgeCoordinator`, and `UnsubscribeBridge` are
      unmodified.
- [ ] The change description and this spec both state that D3 **changes no production behaviour**, with
      the reason (`QfcItemController.ViewerSetup.cs:143` guards the only two production callers on
      `viewer.BreadcrumbCoordinator == null`), so no reviewer expects a user-visible repair.
- [x] The fail-fast/`SetBridgeCoordinator` coupling is recorded in the delivered spec: the
      `SetBridgeCoordinator` replace-without-dispose defect is out of scope **because** D3 fails fast, and
      adopting explicit re-initialization instead would require pulling that defect into scope.

### D4 — UI-thread affinity

- [ ] A private `ItemViewer` helper enforces UI-thread affinity by comparing
      `SynchronizationContext.Current` for **reference equality** against `UiSyncContext`, throwing
      `InvalidOperationException` whose message names the operation, and returning without effect when
      `UiSyncContext` is null.
- [ ] The helper is invoked as the first statement of `InitializeBreadcrumbPipeline(provider, operations)`,
      of both `ConfigureBreadcrumbDropDown` overloads, and of `EnsureBreadcrumbResourceOwnership`.
- [ ] A regression test invokes `InitializeBreadcrumbPipeline` through the existing
      `BreadcrumbSelectorToggleUiBoundaryTests.InvokeAmbientNull` helper — **same thread, ambient context
      nulled, no second thread and no timing construct** — and asserts `InvalidOperationException`. A
      second case installs a *different non-null* context and asserts the same, proving the comparison is
      reference equality rather than a null check.
- [ ] The spec, the change description, and the test's own documentation each state that this proxy
      **proves the guard fires and does not prove the race is absent**, and that a true two-thread data
      race cannot be reproduced deterministically under the repository's ban on sleeps and wall-clock
      waits. **No criterion in this document asserts that the race is eliminated.**
- [ ] No `Interlocked`, `lock`, `Monitor`, `Volatile`, or `Mutex` construct is introduced by this feature.
- [ ] The three unconfirmed harnesses (`BreadcrumbCollapsedSurfaceReadinessTests.cs`,
      `BreadcrumbPendingOpenCloseTests.cs`, `BreadcrumbCoordinatorLifecycleTests.cs`) are each confirmed
      to install their synchronization context **before** constructing the `ItemViewer`, and the
      confirmation is recorded in evidence.

### D5 — `Container` created during teardown

- [ ] `EnsureBreadcrumbResourceOwnership` throws `ObjectDisposedException` as its first action when the
      viewer `IsDisposed` **or** `Disposing`, so no `Container` is created and no `BreadcrumbResourceOwner`
      is added after teardown has begun.
- [ ] A regression test disposes a real `ItemViewer`, calls `InitializeBreadcrumbPipeline`, asserts
      `ObjectDisposedException`, and additionally asserts `viewer.BreadcrumbCoordinator` is null.
- [ ] `QuickFiler/Viewers/ItemViewer.Designer.cs` is byte-identical to its pre-change state.
- [ ] The research §3.5 open item is discharged with recorded evidence: it is confirmed whether a faulted
      `QfcItemController.InitializeWebViewAsync` task is observed by its caller. If it is not observed, a
      new issue is opened against `QfcItemController.ViewerSetup.cs` (484-owned) and referenced here —
      **the guard is not weakened in response.**

### #475 — `CaptureCurrentOrTests()`

- [ ] `BreadcrumbPopupUiOperations.CaptureCurrentOrTests()` no longer exists, and a repository-wide search
      for the identifier returns no hit in any `.cs` file.
- [ ] `BreadcrumbPopupUiOperations.CreateForCurrentThreadTests()` and
      `BreadcrumbUiDispatcher.CreateForCurrentThreadTests()` both still exist and are unmodified; only the
      ambient-probing selector is removed.
- [ ] Both `BreadcrumbDropDownHost` constructor chains that previously supplied
      `CaptureCurrentOrTests()` now supply `CaptureCurrent()`, and the constructor argument order is
      unchanged, so `BreadcrumbDropDownIntegrationTests.Constructor_NullLegacySurfaceFactory_ThrowsForSurfaceFactory`
      still passes without an ambient context.
- [ ] `ItemViewer.EnsureBreadcrumbLifecycle` takes a `Func<BreadcrumbPopupUiOperations>` and evaluates it
      **only after** the already-initialized early return, and all three of its call sites are updated
      accordingly.
- [ ] `CaptureCurrentOrTests_NullAndControlledContexts_SelectExpectedBoundaries` is deleted and replaced
      in `BreadcrumbPopupBoundaryCoverageTests.Part2.cs` with a test asserting that
      `CaptureCurrent` under a null ambient context throws `InvalidOperationException`, retaining the
      controlled-context half against `CaptureCurrent`.
- [ ] A seam-preservation regression test calls the 3-arg `ConfigureBreadcrumbDropDown` inside
      `InvokeAmbientNull` on a viewer whose lifecycle was already seeded with injected operations, and
      asserts `NotThrow`. Evidence records this test red before the laziness change and green after.
- [ ] No test file other than `BreadcrumbPopupBoundaryCoverageTests.Part2.cs` and the new regression file
      is modified in service of #475; specifically, no existing test's injected
      `BreadcrumbPopupUiOperations` seam is removed or replaced.

### Scope, ownership, and the 489 dependency

- [ ] The set of files changed by this feature is a subset of: the four owned production files
      (`ItemViewer.Breadcrumb.cs`, `BreadcrumbItemViewerLifecycleCoordinator.cs`,
      `BreadcrumbPopupUiOperations.cs`, `BreadcrumbDropDownHost.cs`), the two owned test files
      (`BreadcrumbItemViewerLifecycleCoordinatorTests.cs`, `BreadcrumbPopupBoundaryCoverageTests.Part2.cs`),
      the one new test file, and `QuickFiler.Test/QuickFiler.Test.csproj`.
- [ ] `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs`,
      `QuickFiler/Viewers/BreadcrumbMessengerHub.cs`, `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs`,
      `QuickFiler/Viewers/ItemViewer.cs`, `QuickFiler/Viewers/ItemViewer.Designer.cs`,
      `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`, and `QuickFiler/QuickFiler.csproj` are all
      byte-identical to their pre-change state.
- [ ] No public member is added, removed, or re-signed in any changed file, and
      `QuickFiler/Viewers/IBreadcrumbDropDownHost.cs` and `QuickFiler/Viewers/IItemViewer.cs` are
      unmodified.
- [ ] The `QuickFiler.Test.csproj` diff is exactly one added `<Compile Include>` line for the new test
      file, positioned adjacent to the existing `Viewers\ItemViewerBreadcrumbDropDownContractTests.cs`
      entry, with no reordering of any other entry.
- [ ] The three out-of-scope follow-up candidates (D1c; `SetBridgeCoordinator` replace-without-dispose;
      `Reset()` surface-detach synchrony) are each recorded as a potential entry or GitHub issue, with the
      mechanism and triggers carried forward so the follow-up does not have to re-derive them. No fix for
      any of the three appears in this feature's diff.
- [ ] The `## Dependencies on 489` section of this spec is re-checked against 489's finalised spec before
      integration, and each of D489-1 through D489-6 is recorded as confirmed or as requiring a named
      adjustment. **D489-4 is checked explicitly**, because a member-name collision across `ItemViewer`
      partials is a compile error at integration rather than a merge conflict and will not surface at
      fan-in.

### File size, toolchain, coverage, and document integrity

- [ ] Every production and test file touched by this feature is at most **500 lines** after the change.
      The post-change line counts of all four owned production files, both owned test files, and the new
      test file are recorded in evidence. In particular
      `BreadcrumbItemViewerLifecycleCoordinator.cs` (pre-change 481) and
      `BreadcrumbPopupUiOperations.cs` (pre-change 494) are each verified at or under 500.
- [ ] `QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs` remains at exactly 500 lines and is
      unmodified.
- [ ] Every new test uses MSTest, Moq, and FluentAssertions, and no new test contains `Thread.Sleep`,
      `Task.Delay`, a wall-clock wait, or a temporary file.
- [ ] `dotnet tool run csharpier check .` reports no formatting differences.
- [ ] `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
      completes with zero errors, and the analyzer warning count for the four owned production files is
      no greater than the Phase 0 baseline count for those same files.
- [ ] `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
      completes with zero errors. `/p:Nullable=enable` is **not** added to this command.
- [ ] `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /InIsolation`
      reports zero failures, and the pass count is greater than or equal to the Phase 0 baseline pass
      count plus the number of tests added, minus the one deleted test.
- [ ] All four toolchain stages pass in a single consecutive pass with no intervening file modification.
- [ ] Coverage for the changed lines is not reduced relative to the Phase 0 baseline, and each new or
      changed **measured** production member — that is, members in
      `BreadcrumbItemViewerLifecycleCoordinator.cs`, `BreadcrumbPopupUiOperations.cs`, and
      `BreadcrumbDropDownHost.cs` — reaches `>= 90%` line coverage.
- [ ] Repository-wide line coverage is `>= 80%` against the testable denominator defined in CLAUDE.md
      § UT2. Both the testable-denominator figure and the raw uninstrumented figure are recorded in
      evidence together with the Phase 0 baseline values, and the delivered change does not lower either.
- [ ] Evidence records that all fixes in `ItemViewer.Breadcrumb.cs` (D1, D3, D4, D5, and #475 part 3) are
      **coverage-exempt** because `ItemViewer.cs:20` carries `[ExcludeFromCodeCoverage]` on the partial
      type, so their regression tests move no coverage number. **No new `[ExcludeFromCodeCoverage]`
      attribute is introduced anywhere by this feature, and none is removed.**
- [ ] `docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/user-story.md` **does not exist**.
      This is a `full-bug` feature; `spec.md` is the sole acceptance-criteria source, and a second
      checkbox-bearing document in this folder is an integrity failure.

---

## Divergences from the research

Recorded so a reviewer does not treat the research as a correction of this document.

1. **`QuickFiler.Test.csproj` item-group ordering.** Research §6 instructs inserting the new
   `<Compile Include>` entry "at its alphabetical position among the existing `Viewers\ItemViewer*`
   entries" and calls that "this feature's alphabetical region". **The item group is not alphabetically
   ordered**; it is ordered by area and insertion history, and there is exactly one existing
   `Viewers\ItemViewer*` entry (`Viewers\ItemViewerBreadcrumbDropDownContractTests.cs`) sitting between
   two `Breadcrumb*` entries. The delivered instruction is Constraint 3 above: insert adjacent to that
   existing entry and reorder nothing.
2. **Reusability of the referenced test harnesses.** Research §4 lists `ViewerScope` and the `Host(viewer)`
   reflection accessor from `QfcItemControllerBreadcrumbDropDownTests.cs`, and `LifecycleFixture` /
   `RecordingHost` / `QueuedCreatorThreadSynchronizationContext` from
   `BreadcrumbItemViewerLifecycleCoordinatorTests.cs`, as harnesses to reuse. **All five are `private`
   nested types or `private static` members** and are not reachable from a new file. Only
   `BreadcrumbSelectorToggleUiBoundaryTests.InvokeAmbientNull<T>` (`internal static` on a `public sealed`
   class) is genuinely reusable. The new test file defines its own viewer scope and reflection accessor;
   no other test file's member accessibility is widened.
3. **D1's pattern variable.** Research §3.1 states that the `is BreadcrumbDropDownHost existing` variable
   declared at `:148` "is already in scope for the whole method body". It is in scope but **not
   definitely assigned** on the fall-through path, so a fresh pattern variable is required. The delivered
   design says so explicitly.
4. **The D1/D2 `ObjectDisposedException` interaction.** The research does not evaluate what happens when a
   `SetTheme` lands between D1's synchronous dispose and `ConfigureHost`'s post. This document records it
   under "Interaction between the fixes" as an accepted, documented residual rather than leaving it to be
   discovered at review.

---

## Risks & Mitigations

### R1 — `BreadcrumbItemViewerLifecycleCoordinator.cs` file-size ceiling (highest-likelihood scope risk)

The file is **481 of 500 lines (19 spare)** and must absorb D2's retained-theme field, the assignment in
`SetTheme`, and the guarded replay in `ConfigureHost` — about six lines. **Mitigation:** no edit other
than D2 may target this file; if the delivered diff exceeds nineteen lines, the excess must be removed
rather than the ceiling waived, and any genuinely required overflow is a scope escalation, not a
formatting adjustment. Note that CSharpier reflows argument lists, so a hand-count before formatting is
not authoritative — measure after stage 1 of the toolchain.

### R2 — `BreadcrumbPopupUiOperations.cs` headroom

The file is **494 of 500 (6 spare)**, but #475's only edit there is a four-line deletion, so it improves
to roughly 490. **Mitigation:** no addition to this file is permitted; if the fail-fast test helper needs
a home, it belongs in the test project.

### R3 — D2's retained theme perturbs the pooled-reuse theme assertion

`ConfigureAndAttachBreadcrumbAsync_CachesCurrentThemeAndCreatesOneCandidatePerSession`
(`QfcItemControllerBreadcrumbDropDownTests.cs:257-259`) asserts that no stale pooled theme is replayed
after `ResetBreadcrumb()` and a dark→light flip. The retained value is overwritten by
`SetBreadcrumbTheme("light")` before the re-attach, so the assertion should hold. **Mitigation:** this is
the single assertion most likely to be perturbed; re-run it explicitly and record the reasoning in
evidence rather than assuming it.

### R4 — D4's affinity guard breaks a test that constructs its viewer before its context

Three harnesses were not confirmed during research. **Mitigation:** confirm all three before the guard
lands (an explicit acceptance criterion). If a harness constructs the viewer first, the correct response
is to reorder that harness's own setup, not to weaken the guard to a null check.

### R5 — D5's throw becomes an unobserved task exception

The `ObjectDisposedException` propagates out of `QfcItemController.EnsureBreadcrumbPipeline` and faults
`InitializeWebViewAsync`'s task. **Mitigation:** the open item is a blocking acceptance criterion; if the
fault is not observed, a new issue is opened against the 484-owned file and the guard stands.

### R6 — D4's regression test proves the guard, not the absence of the race

A reviewer may read the D4 criterion as a claim that the data race is eliminated. **Mitigation:** the
limitation is stated in three places — Test Strategy, the D4 design rationale, and the D4 acceptance
criteria — and no criterion asserts race elimination.

### R7 — Concurrent sibling features on the same integration branch

Features 484, 489, 501, and 476 are prepared or in flight against
`epic/quickfiler-bug-family-integration`. **Mitigation:** the owned-file list is disjoint from all four
(verified against 501's spec `:155-164` and 484's upstream-contract table at `:329-394`), and the one
shared artifact is `QuickFiler.Test.csproj`, where this feature adds exactly one line. The residual is
D489-4, which is a compile error rather than a merge conflict and therefore requires an explicit check.

### R8 — Citation drift

The promoted potentials' citations had already drifted by up to three lines when the research re-derived
them, and PR #605 recently moved several adjacent breadcrumb files on `main`. **Mitigation:** every
citation in this document was re-verified by direct file read on 2026-08-25; resolve any citation by the
member name it accompanies, not by the line number.

---

## Rollout & Follow-up

### Release/rollout steps

1. Branch from `epic/quickfiler-bug-family-integration`.
2. Capture the Phase 0 baseline (test counts, coverage, per-file line counts, analyzer warning counts for
   the four owned production files).
3. Deliver each defect unit as failing-test-then-fix, in an order that keeps the toolchain green at each
   step. #475's three parts must land together as one change-set.
4. Run the four-stage toolchain to a single clean consecutive pass.
5. Check off the acceptance criteria in this file as each is verified, per
   `.claude/skills/acceptance-criteria-tracking/SKILL.md`.
6. Open the pull request against the integration branch; close #488 and #475.

### Post-fix monitoring or clean-up tasks

- Promote the three out-of-scope follow-up candidates through the potential→issue lifecycle.
- Re-check `## Dependencies on 489` against 489's finalised spec before integration, with D489-4 checked
  explicitly.
- Re-check the D2 assumption that no additional caller has begun issuing `SetBreadcrumbTheme` before
  `ConfigureBreadcrumbDropDown` once 484's `Cleanup()` changes land.

### Links

- Issue #488 — https://github.com/drmoisan/TaskMaster/issues/488 (primary)
- Issue #475 — https://github.com/drmoisan/TaskMaster/issues/475
- `docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/issue.md`
- `docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/research/2026-08-25T10-00-itemviewer-breadcrumb-lifecycle-defects-research.md`
- `docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/research/2026-08-25T10-20-orchestrator-comment-crosscheck.md`
- `docs/features/active/qfc-item-controller-defects-484/spec.md` — sibling exemplar; its
  `### Upstream contract (exhaustive) — required by features 464 and 489` (`:329-394`) is the
  authoritative `QfcItemController` surface enumeration and is **cited, not re-derived**, by this feature.
- `docs/features/active/breadcrumb-coordinator-hub-defects-501/spec.md:155-164` — the file cession that
  makes this feature's ownership claim valid.
