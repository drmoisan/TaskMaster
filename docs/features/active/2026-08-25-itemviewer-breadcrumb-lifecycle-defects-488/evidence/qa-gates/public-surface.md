# Public-Surface Stability ([P7-T4])

Timestamp: 2026-08-28T06-13

Command: `git diff 12465043e052fce66a1861bf1ddd037a1aa81afc` over the four owned production files,
filtered to added and removed lines carrying an accessibility modifier, plus a targeted read of both
`public` seven-parameter `BreadcrumbDropDownHost` constructor signatures.
EXIT_CODE: 0

## Every member added, removed, or re-signed

The complete set, with declared accessibility. Nothing else in the four files changed a member
declaration.

### Added members — three, all private

| Member | Accessibility | File | Unit |
| --- | --- | --- | --- |
| `ThrowIfOffUiBoundary(string operation)` | **private** | `ItemViewer.Breadcrumb.cs` | D4 — the UI-affinity helper |
| `_breadcrumbProvider` | **private** | `ItemViewer.Breadcrumb.cs` | D3 — the retained-provider field |
| `_retainedTheme` | **private** | `BreadcrumbItemViewerLifecycleCoordinator.cs` | D2 — the retained-theme field |

Verbatim from the diff:

```
+        private void ThrowIfOffUiBoundary(string operation)
+        private IFolderHierarchyProvider _breadcrumbProvider;
+        private string? _retainedTheme;
```

### Re-signed member — one, private

| Member | Accessibility | Change |
| --- | --- | --- |
| `EnsureBreadcrumbLifecycle` | **private**, before and after | parameter changed from `BreadcrumbPopupUiOperations operations` to `Func<BreadcrumbPopupUiOperations> operationsFactory` |

Verbatim from the diff:

```
-            BreadcrumbPopupUiOperations operations
+            Func<BreadcrumbPopupUiOperations> operationsFactory
+            BreadcrumbPopupUiOperations operations = operationsFactory();
```

The member was `private` before the change and is `private` after, so #475 part 3 re-signs no
externally visible member. This is what the criterion `[P6-T14]` flips requires when it says
`EnsureBreadcrumbLifecycle` "remains private so no public member is re-signed".

### Removed member — one, internal static

| Member | Accessibility | File |
| --- | --- | --- |
| `CaptureCurrentOrTests()` | **internal static** | `BreadcrumbPopupUiOperations.cs` |

Verbatim from the diff:

```
-        internal static BreadcrumbPopupUiOperations CaptureCurrentOrTests() =>
```

`internal`, not `public`. Its removal is visible only inside the `QuickFiler` assembly and to
`QuickFiler.Test` through `InternalsVisibleTo`, and every reference to it was retired in the same
change-set.

## No `public` member was added, removed, or re-signed

Filtering the diff over all four owned production files for added or removed lines beginning with a
`public` modifier returns a count of **0**.

## Both seven-parameter `BreadcrumbDropDownHost` constructors keep their signatures

Read from the delivered source:

```csharp
        public BreadcrumbDropDownHost(
            Control anchor,
            CoreWebView2Environment environment,
            IWebViewCoreInitializer initializer,
            string html,
            Action focusPending,
            Action focusAnchor,
            Action cancelSelection
        )
```

```csharp
        public BreadcrumbDropDownHost(
            Control anchor,
            CoreWebView2Environment environment,
            LegacySurfaceFactory surfaceFactory,
            Action focusPending,
            Action focusAnchor,
            Action cancelSelection,
            Action<ToolStripDropDown, Control, Point> showPopup
        )
```

Both parameter lists are unchanged in arity, type, order, and name. #475 part 2 changed only the
**argument** each chain forwards to the private constructor — `CaptureCurrentOrTests()` became
`CaptureCurrent()` — which is not part of either signature. The `git diff --numstat` for that file is
`2 2`, one changed line per chain.

Preserving the order is load-bearing rather than incidental: in the second constructor the
`surfaceFactory ?? throw new ArgumentNullException(nameof(surfaceFactory))` is evaluated before the
operations argument, which is why
`Constructor_NullLegacySurfaceFactory_ThrowsForSurfaceFactory` still passes without an ambient context.

## Note on observable behaviour

No signature changed, but the second `public` constructor's **behaviour** did: it now throws
`InvalidOperationException` when constructed without an ambient synchronization context, where it
previously substituted a test dispatcher silently. That is the one publicly observable behaviour change
in the change-set, it is the intended effect of #475, and it is recorded in the change description
`[P7-T9]` writes. It is a behaviour change, not a surface change, so it does not bear on this task's
acceptance.

Output Summary: Exactly **three** members were added, all **private**: `ThrowIfOffUiBoundary`,
`_breadcrumbProvider`, and the lifecycle coordinator's retained-theme field. Exactly **one** member was
re-signed, the **private** `EnsureBreadcrumbLifecycle`. Exactly **one** member was removed, the
**internal static** `CaptureCurrentOrTests`. **No `public` member was added, removed, or re-signed**
(filter count 0), and both seven-parameter `BreadcrumbDropDownHost` constructors keep their signatures
unchanged.
