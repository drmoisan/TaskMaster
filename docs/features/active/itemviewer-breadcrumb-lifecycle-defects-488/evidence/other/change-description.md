# Change Description — issue #488 (D1–D5) and issue #475 ([P7-T9])

Timestamp: 2026-08-28T06-18

Command: composed from the delivered source, the delivered `spec.md`, and the phase evidence artifacts.
EXIT_CODE: 0

## What this change-set delivers

Six defect units across four owned production files, each with a deterministic regression test observed
red before its fix landed.

| Unit | Change | File |
| --- | --- | --- |
| D1 | Dispose the outgoing host between the same-environment early return and the construction of its replacement | `ItemViewer.Breadcrumb.cs` |
| D2 | Retain the last theme and replay it onto the newly adopted host | `BreadcrumbItemViewerLifecycleCoordinator.cs` |
| D3 | Fail fast on a second, different `IFolderHierarchyProvider` | `ItemViewer.Breadcrumb.cs` |
| D4 | Declare and enforce UI-thread affinity via `ThrowIfOffUiBoundary` | `ItemViewer.Breadcrumb.cs` |
| D5 | Refuse breadcrumb resource creation once teardown has begun | `ItemViewer.Breadcrumb.cs` |
| #475 | Delete the ambient-probing selector; repoint both host constructor chains; make the lifecycle operations argument lazy | `BreadcrumbPopupUiOperations.cs`, `BreadcrumbDropDownHost.cs`, `ItemViewer.Breadcrumb.cs` |

---

## D3 changes no production behaviour

**D3 changes no production behaviour.** No reviewer should expect or claim a user-visible repair.

The reason is that the guard is unreachable from production code. `InitializeBreadcrumbPipeline` has
exactly one production call site, `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:150`, inside
`QfcItemController.EnsureBreadcrumbPipeline`, and it is guarded upstream:

```csharp
            if (viewer.BreadcrumbCoordinator == null)
            {
                ...
                viewer.InitializeBreadcrumbPipeline(provider);
            }
```

The call is made only when `viewer.BreadcrumbCoordinator` is null, which is exactly the condition under
which the new guard's enclosing `if (BreadcrumbCoordinator != null)` block is never entered. That file
is owned by sibling feature `qfc-item-controller-defects-484` and was read but not edited.

D3's value is that a discard which was previously **silent** is now **loud** for any future caller, and
that the wrapper is exactly as strict as the collaborator it wraps.

---

## D4's regression proxy proves the guard fires; it does not prove the race is absent

The criterion this addresses requires the spec, the change description, and the test's own
documentation to each carry this statement. All three are quoted here so that this one artifact records
all three rather than only its own.

### The delivered `spec.md` states it

> The spec, the change description, and the test's own documentation each state that this proxy
> **proves the guard fires and does not prove the race is absent**, and that a true two-thread data
> race cannot be reproduced deterministically under the repository's ban on sleeps and wall-clock
> waits. **No criterion in this document asserts that the race is eliminated.**

### The first `[P4-T1]` test's XML doc comment states it, verbatim

From `InitializeBreadcrumbPipeline_AmbientContextNull_ThrowsBoundaryDiagnostic`:

> This proxy **proves the guard fires and does not prove the race is absent.** A
> true two-thread data race cannot be reproduced deterministically under the repository ban
> on sleeps and wall-clock waits: two threads with no barrier give no way to force the
> interleaving.

### The second `[P4-T1]` test's XML doc comment states it, verbatim

From `InitializeBreadcrumbPipeline_DifferentNonNullContext_ThrowsBoundaryDiagnostic`:

> This proxy **proves the guard fires and does not prove the race is absent**: a
> true two-thread data race cannot be reproduced deterministically under the repository ban
> on sleeps and wall-clock waits. The substituted context is installed and restored in a
> `try`/`finally` on the same thread; no second thread and no timing construct.

### This change description states it

D4's regression proxy **proves the guard fires and does not prove the race is absent.** A true
two-thread data race cannot be reproduced deterministically under the repository's ban on sleeps and
wall-clock waits: two threads with no barrier offer no way to force the interleaving. What the two
tests assert is the **declared contract**, on a single thread, with no timing construct — that an
off-boundary call is refused with a diagnostic naming the operation. D4 declares and enforces UI-thread
affinity; it does not make the read-then-write atomic. Atomic initialization was rejected because it
would legitimise off-thread access to WinForms control state that is not thread-safe at all.

---

## The accepted `ObjectDisposedException` residual introduced by D1

D1's fix disposes the outgoing host **synchronously**, while the lifecycle coordinator's open
coordinator still points at that host until `ConfigureHost`'s posted lambda runs `ReleaseHostCore()`. A
`SetTheme` landing inside that window reaches the host's disposal guard and throws
`ObjectDisposedException` instead of silently theming a host that is about to be discarded.

**This residual is accepted, not fixed**, for three reasons: D4 rejects an off-boundary configure
outright, so the window is no longer reachable through `ItemViewer`'s own surface; D2's retained theme
still reaches the newly adopted host, so no theme is lost; and the window does not exist on the
production UI thread, where every post runs inline. It is recorded here rather than left to be
discovered at review. The full dossier is `evidence/other/d1-residual-dossier.md`.

**D1b** — the unobservable dispose failure inside the sibling-owned `BreadcrumbDropDownOpenCoordinator`
— is a recorded residual this feature does not fix, on ownership grounds.

---

## #475's three parts landed as one change-set

All three parts are required and were delivered together:

1. `BreadcrumbPopupUiOperations.CaptureCurrentOrTests()` is deleted; a repository-wide search of tracked
   `.cs` files returns zero hits. `CreateForCurrentThreadTests` survives on both
   `BreadcrumbPopupUiOperations` and `BreadcrumbUiDispatcher`, so every injected test seam is preserved.
2. Both `BreadcrumbDropDownHost` seven-parameter constructor chains supply `CaptureCurrent()`, with no
   constructor argument reordered.
3. `EnsureBreadcrumbLifecycle` takes a `Func<BreadcrumbPopupUiOperations>` evaluated exactly once and
   only after the already-initialized early return, with all three call sites updated.

Part 3 is **required, not opportunistic**. `EnsureBreadcrumbLifecycle` discards its operations argument
whenever the coordinator already exists, so parts 1 and 2 without part 3 would make a pure no-op call
throw on any thread without a context. `[P6-T4]` recorded exactly that failure before part 3 landed.

---

## The one publicly observable behaviour change

**The `public` seven-parameter `BreadcrumbDropDownHost` constructor now throws
`InvalidOperationException` when constructed without an ambient synchronization context**, where it
previously substituted a test dispatcher silently. No signature changed; the behaviour did. This is the
intended effect of #475 and is the only externally visible behaviour change in the change-set.

No `public` member was added, removed, or re-signed. The three added members are all private, the one
re-signed member is the private `EnsureBreadcrumbLifecycle`, and the one removed member is the
`internal static` `CaptureCurrentOrTests`.

---

## All fixes in `ItemViewer.Breadcrumb.cs` are coverage-exempt

`QuickFiler/Viewers/ItemViewer.cs:20` carries `[ExcludeFromCodeCoverage]` on the `ItemViewer` partial
**type**. A type-level attribute on one part applies to the whole partial type, so **every member of
`ItemViewer.Breadcrumb.cs` is excluded from coverage measurement**. D1, D3, D4, D5, and #475 part 3 all
land in that file and therefore **move no coverage number**.

`[P0-T15]` confirmed this empirically: the file matches zero `class` elements in the baseline Cobertura
document. A reviewer must not read flat coverage on this feature as a testing gap, and must not remove
the exemption to "fix" it — `ItemViewer.cs` is a forbidden file and its attribute is assumption D489-2.
The regression tests for those units are required by the CLAUDE.md Bugfix Workflow and by the
acceptance criteria, not by a coverage delta.

Only D2 (`BreadcrumbItemViewerLifecycleCoordinator.cs`), #475 part 1 (`BreadcrumbPopupUiOperations.cs`),
and #475 part 2 (`BreadcrumbDropDownHost.cs`) are measured.

Output Summary: Records that **D3 changes no production behaviour** with its upstream-guard reason; that
**D4's proxy proves the guard fires and does not prove the race is absent** and that a true two-thread
race cannot be reproduced deterministically under the repository ban on sleeps and wall-clock waits,
quoting the delivered `spec.md` and both `[P4-T1]` test doc comments verbatim alongside this
description's own statement; the accepted `ObjectDisposedException` residual introduced by D1; that
#475's three parts landed as one change-set; the one publicly observable behaviour change, namely the
`public` seven-parameter `BreadcrumbDropDownHost` constructor now throwing without an ambient context;
and that all fixes in `ItemViewer.Breadcrumb.cs` are coverage-exempt.
