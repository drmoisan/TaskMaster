# D1 — Recorded Limitations Honoured in the Delivered Source ([P1-T8])

Timestamp: 2026-08-28T05-30

Command: source reading of the delivered
`QuickFiler/Viewers/ItemViewer.Breadcrumb.cs`, plus
`git diff --name-only 12465043e052fce66a1861bf1ddd037a1aa81afc -- QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs`
EXIT_CODE: 0

---

## Fact 1 — the three-argument overload contains no host disposal

The spec records that the fix covers the concrete environment-change path only: the three-argument
injected overload can also replace a host, but the outgoing host is not knowable there until inside
the post, and that overload has no production caller. Recording the limitation was preferred to
widening the fix. The delivered source honours that.

Delivered body of the three-argument overload, quoted verbatim:

```csharp
        internal void ConfigureBreadcrumbDropDown(
            IBreadcrumbDropDownHost host,
            Func<Rectangle> anchorBounds,
            Func<Rectangle> workingArea
        )
        {
            if (host == null)
            {
                throw new ArgumentNullException(nameof(host));
            }
            _ = anchorBounds ?? throw new ArgumentNullException(nameof(anchorBounds));
            _ = workingArea ?? throw new ArgumentNullException(nameof(workingArea));
            BreadcrumbItemViewerLifecycleCoordinator lifecycle = EnsureBreadcrumbLifecycle(
                BreadcrumbPopupUiOperations.CaptureCurrentOrTests()
            );
            lifecycle.ConfigureHost(host, anchorBounds, workingArea);
        }
```

The body contains **no `Dispose()` call**, **no type test for `BreadcrumbDropDownHost`**, and no
statement equivalent to the disposal `[P1-T5]` added to the two-argument overload. Its four
statements are the three argument guards, the `EnsureBreadcrumbLifecycle` call, and the
`ConfigureHost` forward — exactly as at `BASE_SHA`.

This is also what keeps
`BreadcrumbDropDownIntegrationTests.ItemViewerDisposal_OwnsHostAndDetachesBothSurfaces` green: that
test's `host.Dispose()` `Times.Once()` assertion is on a `Mock<IBreadcrumbDropDownHost>`, which is not
idempotent, and the mock host reaches the viewer through this overload. Adding an equivalent disposal
here would produce a second `Dispose()` call on that mock and break the assertion for a reason
unrelated to the defect.

This body is quoted as delivered after `[P1-T5]`. It still shows
`BreadcrumbPopupUiOperations.CaptureCurrentOrTests()`, which is correct at this point in plan order:
that identifier is retired by `[P6-T3]` and the argument is made lazy by `[P6-T7]`, both later. The
limitation recorded here — no host disposal in this overload — is unaffected by either of those
changes.

---

## Fact 2 — `BreadcrumbDropDownOpenCoordinator.cs` is unmodified

Command:

```
git diff --name-only 12465043e052fce66a1861bf1ddd037a1aa81afc -- QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs
```

**Output: no lines.**

The file is byte-identical to its state at `BASE_SHA`. This is the corroborating evidence for the
D1b residual recorded in `[P1-T7]`'s dossier and for the rejected D1 alternative: making
`BreadcrumbDropDownOpenCoordinator.Release()` synchronous would have been the cleaner fix on the
merits, and it was rejected purely on ownership, because that file belongs to sibling feature
`breadcrumb-coordinator-hub-defects-501` for issue #462.

Output Summary: Both recorded D1 limitations are honoured in the delivered source. The three-argument
`ConfigureBreadcrumbDropDown` overload contains no host disposal — its verbatim body is quoted above
and holds only the three argument guards, the lifecycle call, and the `ConfigureHost` forward — and
`git diff --name-only <BASE_SHA> -- QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs` produces
**no output lines**.
